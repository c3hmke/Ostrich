using Xunit;

namespace Unit;

public sealed class CpuCoreOpcodeTests
{
    [Fact] // Verifies NOP advances PC by one byte and consumes one machine cycle.
    public void Nop_AdvancesPcAndCycleCount()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x00);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0101, cpu.State.PC);
        Assert.Equal((ulong)4, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies unimplemented opcodes fail fast with NotSupportedException.
    public void UnsupportedOpcode_Throws()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xD3);

        Assert.Throws<NotSupportedException>(() => cpu.StepInstruction());
    }
    
    [Fact] // Verifies LD A,d8 loads immediate data into A and updates timing/PC.
    public void LdA_d8_LoadsImmediateValueIntoA()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x3E, 0x42);

        cpu.StepInstruction();

        Assert.Equal((byte)0x42, cpu.State.A);
        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies LD SP,d16 writes a 16-bit immediate into SP.
    public void LdSp_d16_LoadsImmediateIntoSp()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x31, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.SP);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD BC,d16 writes a 16-bit immediate into BC.
    public void LdBc_d16_LoadsImmediateIntoBc()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x01, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.B);
        Assert.Equal((byte)0x34, cpu.State.C);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD DE,d16 writes a 16-bit immediate into DE.
    public void LdDe_d16_LoadsImmediateIntoDe()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x11, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.D);
        Assert.Equal((byte)0x34, cpu.State.E);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD HL,d16 writes a 16-bit immediate into HL.
    public void LdHl_d16_LoadsImmediateIntoHl()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x21, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.H);
        Assert.Equal((byte)0x34, cpu.State.L);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies LD (a16),A stores accumulator contents at an absolute address.
    public void LdA16_A_WritesAccumulatorToAbsoluteAddress()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x3E, 0x77, 0xEA, 0x00, 0xC0);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x77, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies LD A,(a16) reads a byte from absolute address into A.
    public void LdA_a16_ReadsAccumulatorFromAbsoluteAddress()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0xFA, 0x00, 0xC0);
        bus.WriteByte(0xC000, 0x5A);

        cpu.StepInstruction();

        Assert.Equal((byte)0x5A, cpu.State.A);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount);
    }
}
