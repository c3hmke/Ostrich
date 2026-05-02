using Xunit;

namespace Unit;

public sealed class CpuJumpOpcodeTests
{
    [Fact]
    public void Jr_r8_AppliesSignedRelativeOffset()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x18, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact]
    public void Jr_r8_AppliesNegativeSignedRelativeOffset()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x18, 0xFE);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0100, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact]
    public void JrNz_e8_DoesNotJumpWhenZIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x20, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact]
    public void JrZ_e8_JumpsWhenZIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x28, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact]
    public void JrNc_e8_DoesNotJumpWhenCIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x30, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact]
    public void JrC_e8_JumpsWhenCIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x38, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact]
    public void Jp_a16_JumpsToAbsoluteAddress()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xC3, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount);
    }
}
