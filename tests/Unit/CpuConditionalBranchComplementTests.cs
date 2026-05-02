using Xunit;

namespace Unit;

public sealed class CpuConditionalBranchComplementTests
{
    [Fact] // Verifies JR NZ branches when zero flag is clear.
    public void JrNz_e8_JumpsWhenZIsClear()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xE8, 0x00, 0x20, 0x02);

        cpu.StepInstruction(); // ADD SP,+0 (clears Z and C)
        cpu.StepInstruction(); // JR NZ,+2

        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies JR Z does not branch when zero flag is clear.
    public void JrZ_e8_DoesNotJumpWhenZIsClear()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xE8, 0x00, 0x28, 0x02);

        cpu.StepInstruction(); // ADD SP,+0 (clears Z and C)
        cpu.StepInstruction(); // JR Z,+2

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies JR NC branches when carry flag is clear.
    public void JrNc_e8_JumpsWhenCIsClear()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xE8, 0x00, 0x30, 0x02);

        cpu.StepInstruction(); // ADD SP,+0 (clears Z and C)
        cpu.StepInstruction(); // JR NC,+2

        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies JR C does not branch when carry flag is clear.
    public void JrC_e8_DoesNotJumpWhenCIsClear()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xE8, 0x00, 0x38, 0x02);

        cpu.StepInstruction(); // ADD SP,+0 (clears Z and C)
        cpu.StepInstruction(); // JR C,+2

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount);
    }
}
