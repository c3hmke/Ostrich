using Xunit;

namespace Unit;

public sealed class CpuSpMathOpcodeTests
{
    [Fact] // Verifies ADD SP,e8 applies signed addition and sets H/C flags correctly.
    public void AddSp_e8_UpdatesSpAndFlags()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x31, 0xF8, 0xFF, 0xE8, 0x08);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x0000, cpu.State.SP);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies LD HL,SP+e8 computes HL, preserves SP, and updates flags.
    public void LdHlSpPlusE8_SetsHlAndKeepsSp()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x31, 0xF8, 0xFF, 0xF8, 0x08);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0xFFF8, cpu.State.SP);
        Assert.Equal((ushort)0x0000, (ushort)((cpu.State.H << 8) | cpu.State.L));
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD SP,HL copies HL into SP and leaves HL unchanged.
    public void LdSpHl_CopiesHlIntoSp()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x31, 0xF8, 0xFF, // LD SP,0xFFF8
            0xF8, 0x08,       // LD HL,SP+0x08 -> HL=0x0000
            0xF9              // LD SP,HL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x0000, cpu.State.SP);
        Assert.Equal((ushort)0x0000, (ushort)((cpu.State.H << 8) | cpu.State.L));
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 12 + 12 + 8
    }

    [Fact] // Verifies LD SP,HL does not modify CPU flags.
    public void LdSpHl_PreservesFlags()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x01,       // LD A,0x01
            0x97,             // SUB A,A -> Z=1, N=1, H=0, C=0
            0x21, 0x34, 0x12, // LD HL,0x1234
            0xF9              // LD SP,HL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.SP);
        Assert.True(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0107, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 12 + 8
    }
}
