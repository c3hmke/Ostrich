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

    private static LR35902 CreateCpuWithProgram(params byte[] programAtEntry)
    {
        byte[] rom = BuildValidRom(programAtEntry);
        var cart = Cartridge.FromROM(rom, "unit.gb");
        var bus = new Bus(cart);

        var cpu = new LR35902();
        cpu.AttachBus(bus);

        return cpu;
    }

    private static byte[] BuildValidRom(byte[] programAtEntry)
    {
        var rom = new byte[32 * 1024];
        Array.Copy(programAtEntry, 0, rom, 0x0100, programAtEntry.Length);

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
