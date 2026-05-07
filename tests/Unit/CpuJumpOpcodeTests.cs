using Xunit;

namespace Unit;

public sealed class CpuJumpOpcodeTests
{
    [Fact] // Verifies JR adds a positive signed offset relative to the next PC.
    public void Jr_r8_AppliesSignedRelativeOffset()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x18, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies JR handles negative signed offsets (backward jump).
    public void Jr_r8_AppliesNegativeSignedRelativeOffset()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x18, 0xFE);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0100, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies JR NZ does not branch when zero flag is set.
    public void JrNz_e8_DoesNotJumpWhenZIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x20, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies JR Z branches when zero flag is set.
    public void JrZ_e8_JumpsWhenZIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x28, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies JR NC does not branch when carry flag is set.
    public void JrNc_e8_DoesNotJumpWhenCIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x30, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies JR C branches when carry flag is set.
    public void JrC_e8_JumpsWhenCIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x38, 0x02);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }
    
    [Fact] // Verifies JP loads PC with a 16-bit absolute target.
    public void Jp_a16_JumpsToAbsoluteAddress()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xC3, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount);
    }

    [Fact] // Verifies JP NZ branches when zero flag is clear.
    public void JpNz_a16_JumpsWhenZIsClear()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0xAF,             // XOR A,A -> Z=1
            0x3E, 0x01,       // LD A,0x01
            0xB7,             // OR A,A -> Z=0
            0xC2, 0x34, 0x12  // JP NZ,0x1234
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 4 + 8 + 4 + 16
    }

    [Fact] // Verifies JP NZ falls through when zero flag is set.
    public void JpNz_a16_DoesNotJumpWhenZIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xC2, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact] // Verifies JP C branches when carry flag is set.
    public void JpC_a16_JumpsWhenCIsSet()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xDA, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount);
    }

    [Fact] // Verifies JP C falls through when carry flag is clear.
    public void JpC_a16_DoesNotJumpWhenCIsClear()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00,       // LD A,0x00
            0x07,             // RLCA -> C=0
            0xDA, 0x34, 0x12  // JP C,0x1234
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 8 + 4 + 12
    }
}
