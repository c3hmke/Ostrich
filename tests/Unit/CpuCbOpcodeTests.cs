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

    [Fact] // Verifies CB RL B rotates left through carry.
    public void CbRl_B_RotatesLeftThroughCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x37,       // SCF -> carry in = 1
            0x06, 0x80, // LD B,0x80
            0xCB, 0x10  // RL B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x01, cpu.State.B);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 4 + 8 + 8
    }

    [Fact] // Verifies CB RR (HL) rotates right through carry and uses the HL timing.
    public void CbRr_HL_RotatesRightThroughCarry()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x37,             // SCF -> carry in = 1
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0xCB, 0x1E        // RR (HL)
        );
        bus.WriteByte(0xC000, 0x02);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x81, bus.ReadByte(0xC000));
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 4 + 12 + 16
    }

    [Fact] // Verifies CB SLA B shifts left, clears bit 0, and moves bit 7 into carry.
    public void CbSla_B_ShiftsLeftAndSetsCarryFromBitSeven()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x06, 0x81, // LD B,0x81
            0xCB, 0x20  // SLA B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x02, cpu.State.B);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 8
    }

    [Fact] // Verifies CB SRA B preserves bit 7 while shifting right.
    public void CbSra_B_PreservesSignBit()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x06, 0x81, // LD B,0x81
            0xCB, 0x28  // SRA B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xC0, cpu.State.B);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 8
    }

    [Fact] // Verifies CB SWAP (HL) exchanges upper and lower nibbles and uses the HL timing.
    public void CbSwap_HL_SwapsNibblesInMemory()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0xCB, 0x36        // SWAP (HL)
        );
        bus.WriteByte(0xC000, 0xF0);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x0F, bus.ReadByte(0xC000));
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 16
    }

    [Fact] // Verifies CB SRL B shifts right logically and clears bit 7.
    public void CbSrl_B_ShiftsRightLogically()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x06, 0x81, // LD B,0x81
            0xCB, 0x38  // SRL B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x40, cpu.State.B);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 8
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

    [Fact] // Verifies CB BIT 0,(HL) reads from memory and uses the HL timing.
    public void CbBit0_HL_TestsBitZeroInMemory()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x37,             // SCF -> carry should be preserved by BIT
            0xCB, 0x46        // BIT 0,(HL)
        );
        bus.WriteByte(0xC000, 0x00);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, bus.ReadByte(0xC000));
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 12 + 4 + 16
    }

    [Fact] // Verifies CB RES 7,B clears bit 7 selected from opcode bits 5-3.
    public void CbRes7_B_ClearsBitSeven()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x06, 0xFF, // LD B,0xFF
            0xCB, 0xB8  // RES 7,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x7F, cpu.State.B);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 8
    }

    [Fact] // Verifies CB RES 7,(HL) clears bit 7 in memory and uses the HL timing.
    public void CbRes7_HL_ClearsBitSevenInMemory()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0xCB, 0xBE        // RES 7,(HL)
        );
        bus.WriteByte(0xC000, 0xFF);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x7F, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 16
    }

    [Fact] // Verifies CB SET 7,B sets bit 7 selected from opcode bits 5-3.
    public void CbSet7_B_SetsBitSeven()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x06, 0x00, // LD B,0x00
            0xCB, 0xF8  // SET 7,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x80, cpu.State.B);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 8
    }

    [Fact] // Verifies CB SET 7,(HL) sets bit 7 in memory and uses the HL timing.
    public void CbSet7_HL_SetsBitSevenInMemory()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0xCB, 0xFE        // SET 7,(HL)
        );
        bus.WriteByte(0xC000, 0x00);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x80, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 16
    }
}
