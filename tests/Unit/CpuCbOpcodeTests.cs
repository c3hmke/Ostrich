using Xunit;

namespace Unit;

public sealed class CpuCbOpcodeTests
{
    [Fact] // Verifies CB RLC B rotates bit 7 into carry and bit 0.
    public void CbRlc_B_RotatesBit7IntoCarryAndBit0()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x06, 0x85, // LD B,0x85
            0xCB, 0x00  // RLC B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x0B, cpu.State.B); // 1000_0101 -> 0000_1011
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 8
    }

    [Fact] // Verifies CB RLC B on zero keeps zero and sets Z unlike RLCA.
    public void CbRlc_B_WithZeroInput_SetsZeroAndClearsCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x06, 0x00, // LD B,0x00
            0xCB, 0x00  // RLC B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.B);
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 8
    }

    [Fact] // Verifies CB RLC (HL) rotates the byte in memory and uses the HL timing.
    public void CbRlc_HL_RotatesMemoryAtAddressInHl()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0xCB, 0x06        // RLC (HL)
        );
        bus.WriteByte(0xC000, 0x80);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x01, bus.ReadByte(0xC000));
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 16
    }

    [Fact] // Verifies CB RRC A rotates bit 0 into carry and bit 7.
    public void CbRrc_A_RotatesBit0IntoCarryAndBit7()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x01, // LD A,0x01
            0xCB, 0x0F  // RRC A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x80, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 8
    }

    [Fact] // Verifies CB RRC (HL) on zero keeps zero, clears carry, and sets Z.
    public void CbRrc_HL_WithZeroInput_SetsZeroAndClearsCarry()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0xCB, 0x0E        // RRC (HL)
        );
        bus.WriteByte(0xC000, 0x00);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, bus.ReadByte(0xC000));
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 16
    }

    [Fact] // Verifies CB BIT 7,B decodes the tested bit index from opcode bits 5-3.
    public void CbBit7_B_TestsBitSevenRatherThanLowerBits()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x06, 0x80, // LD B,0x80
            0x37,       // SCF -> carry should be preserved by BIT
            0xCB, 0x78  // BIT 7,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x80, cpu.State.B); // BIT does not modify the operand.
        Assert.False(cpu.State.FlagZ);         // Bit 7 is set, so Z must clear.
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);          // BIT preserves carry.
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 4 + 8
    }
}
