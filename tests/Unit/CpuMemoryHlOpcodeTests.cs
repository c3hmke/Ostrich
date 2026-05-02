using Xunit;

namespace Unit;

public sealed class CpuMemoryHlOpcodeTests
{
    [Fact] // Verifies LD (HL+),A writes to HL then increments HL.
    public void LdHlInc_A_WritesAtHlThenIncrementsHl()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x31, 0x00, 0xC0, 0xF8, 0x00, 0x3E, 0x5A, 0x22);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x5A, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0xC001, (ushort)((cpu.State.H << 8) | cpu.State.L));
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)40, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies LD A,(HL+) reads from HL then increments HL.
    public void LdA_HlInc_ReadsAtHlThenIncrementsHl()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x31, 0x00, 0xC0, 0xF8, 0x00, 0x2A);
        bus.WriteByte(0xC000, 0xA5);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xA5, cpu.State.A);
        Assert.Equal((ushort)0xC001, (ushort)((cpu.State.H << 8) | cpu.State.L));
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies LD (HL-),A writes to HL then decrements HL.
    public void LdHlDec_A_WritesAtHlThenDecrementsHl()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x31, 0x00, 0xC0, 0xF8, 0x00, 0x3E, 0x9B, 0x32);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x9B, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0xBFFF, (ushort)((cpu.State.H << 8) | cpu.State.L));
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)40, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies LD A,(HL-) reads from HL then decrements HL.
    public void LdA_HlDec_ReadsAtHlThenDecrementsHl()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x31, 0x00, 0xC0, 0xF8, 0x00, 0x3A);
        bus.WriteByte(0xC000, 0x33);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x33, cpu.State.A);
        Assert.Equal((ushort)0xBFFF, (ushort)((cpu.State.H << 8) | cpu.State.L));
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies LD (HL),A writes memory without modifying HL.
    public void LdHl_A_WritesWithoutChangingHl()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x31, 0x00, 0xC0, 0xF8, 0x00, 0x3E, 0x11, 0x77);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x11, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0xC000, (ushort)((cpu.State.H << 8) | cpu.State.L));
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)40, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies LD A,(HL) reads memory without modifying HL.
    public void LdA_Hl_ReadsWithoutChangingHl()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x31, 0x00, 0xC0, 0xF8, 0x00, 0x7E);
        bus.WriteByte(0xC000, 0x44);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x44, cpu.State.A);
        Assert.Equal((ushort)0xC000, (ushort)((cpu.State.H << 8) | cpu.State.L));
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount);
    }
}
