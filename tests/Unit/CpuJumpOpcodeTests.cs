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

    [Fact] // Verifies JP HL loads PC directly from the HL register pair.
    public void JpHl_JumpsToAddressInHl()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x21, 0x23, 0xC1, // LD HL,0xC123
            0xE9              // JP HL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0xC123, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 12 + 4
    }

    [Fact] // Verifies JP HL does not modify any CPU flags.
    public void JpHl_DoesNotModifyFlags()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x01,       // LD A,0x01
            0x3D,             // DEC A -> Z=1, N=1
            0x37,             // SCF   -> Z=1, N=0, H=0, C=1
            0x21, 0x34, 0x12, // LD HL,0x1234
            0xE9              // JP HL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x1234, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 4 + 12 + 4
    }

    [Fact] // Verifies JP HL uses the latest HL value at execution time.
    public void JpHl_UsesCurrentHlValue()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x21, 0x34, 0x12, // LD HL,0x1234
            0x23,             // INC HL -> 0x1235
            0xE9              // JP HL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x1235, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 12 + 8 + 4
    }
}
