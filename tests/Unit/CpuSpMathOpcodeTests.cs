using Xunit;

namespace Unit;

public sealed class CpuSpMathOpcodeTests
{
    [Fact]
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

    [Fact]
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
}
