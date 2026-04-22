using GameBoy;
using GameBoy.CPU;
using Xunit;

namespace Unit;

public sealed class CpuStepInstructionTests
{
    [Fact]
    public void Nop_AdvancesPcAndCycleCount()
    {
        var cpu = CreateCpuWithProgram(0x00);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0101, cpu.State.PC);
        Assert.Equal((ulong)4, cpu.State.CycleCount);
    }

    [Fact]
    public void UnsupportedOpcode_Throws()
    {
        var cpu = CreateCpuWithProgram(0xD3);

        Assert.Throws<NotSupportedException>(() => cpu.StepInstruction());
    }

    [Fact]
    public void LdA_d8_LoadsImmediateValueIntoA()
    {
        var cpu = CreateCpuWithProgram(0x3E, 0x42);

        cpu.StepInstruction();

        Assert.Equal((byte)0x42, cpu.State.A);
        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact]
    public void Jr_r8_AppliesSignedRelativeOffset()
    {
        var cpu = CreateCpuWithProgram(0x18, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact]
    public void Jr_r8_AppliesNegativeSignedRelativeOffset()
    {
        var cpu = CreateCpuWithProgram(0x18, 0xFE);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0100, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact]
    public void Jp_a16_JumpsToAbsoluteAddress()
    {
        var cpu = CreateCpuWithProgram(0xC3, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount);
    }

    [Fact]
    public void LdSp_d16_LoadsImmediateIntoSp()
    {
        var cpu = CreateCpuWithProgram(0x31, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.SP);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact]
    public void LdA16_A_WritesAccumulatorToAbsoluteAddress()
    {
        var (cpu, bus) = CreateCpuAndBusWithProgram(0x3E, 0x77, 0xEA, 0x00, 0xC0);

        cpu.StepInstruction(); // LD A,0x77
        cpu.StepInstruction(); // LD (0xC000),A

        Assert.Equal((byte)0x77, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount);
    }

    [Fact]
    public void LdA_a16_ReadsAccumulatorFromAbsoluteAddress()
    {
        var (cpu, bus) = CreateCpuAndBusWithProgram(0xFA, 0x00, 0xC0);
        bus.WriteByte(0xC000, 0x5A);

        cpu.StepInstruction();

        Assert.Equal((byte)0x5A, cpu.State.A);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount);
    }

    [Fact]
    public void Call_a16_PushesReturnAddressAndJumps()
    {
        var (cpu, bus) = CreateCpuAndBusWithProgram(0xCD, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.PC);
        Assert.Equal((ushort)0xFFFC, cpu.State.SP);
        Assert.Equal((byte)0x03, bus.ReadByte(0xFFFC));
        Assert.Equal((byte)0x01, bus.ReadByte(0xFFFD));
        Assert.Equal((ulong)24, cpu.State.CycleCount);
    }

    [Fact]
    public void Ret_PopsAddressFromStack()
    {
        var cpu = CreateCpuWithProgram(
            programAtEntry: new byte[] { 0xCD, 0x34, 0x12 },
            romPatches: new Dictionary<int, byte>
            {
                [0x1234] = 0xC9
            });

        cpu.StepInstruction(); // CALL 0x1234
        cpu.StepInstruction(); // RET

        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ushort)0xFFFE, cpu.State.SP);
        Assert.Equal((ulong)40, cpu.State.CycleCount);
    }

    private static LR35902 CreateCpuWithProgram(params byte[] programAtEntry)
        => CreateCpuWithProgram(programAtEntry, romPatches: null);

    private static LR35902 CreateCpuWithProgram(byte[] programAtEntry, Dictionary<int, byte>? romPatches)
    {
        byte[] rom = BuildValidRom(programAtEntry, romPatches);
        var cart = Cartridge.FromROM(rom, "unit.gb");
        var bus = new Bus(cart);

        var cpu = new LR35902();
        cpu.AttachBus(bus);

        return cpu;
    }

    private static (LR35902 cpu, Bus bus) CreateCpuAndBusWithProgram(params byte[] programAtEntry)
    {
        byte[] rom = BuildValidRom(programAtEntry, romPatches: null);
        var cart = Cartridge.FromROM(rom, "unit.gb");
        var bus = new Bus(cart);

        var cpu = new LR35902();
        cpu.AttachBus(bus);

        return (cpu, bus);
    }

    private static byte[] BuildValidRom(byte[] programAtEntry, Dictionary<int, byte>? romPatches)
    {
        var rom = new byte[32 * 1024];
        Array.Copy(programAtEntry, 0, rom, 0x0100, programAtEntry.Length);

        if (romPatches is not null)
        {
            foreach ((int address, byte value) in romPatches)
                rom[address] = value;
        }

        rom[0x0147] = 0x00;
        rom[0x0148] = 0x00;
        rom[0x0149] = 0x00;

        int checksum = 0;
        for (int address = 0x0134; address <= 0x014C; address++)
            checksum = checksum - rom[address] - 1;

        rom[0x014D] = (byte)checksum;
        return rom;
    }
}
