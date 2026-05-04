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

    [Fact] // Verifies STOP places CPU into halted/stopped execution state.
    public void Stop_SetsHaltedState()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x10);

        cpu.StepInstruction();

        Assert.True(cpu.State.Halted);
        Assert.Equal((ushort)0x0101, cpu.State.PC);
        Assert.Equal((ulong)4, cpu.State.CycleCount);
    }

    [Fact] // Verifies CPU does not execute following instructions once STOP has been entered.
    public void Stop_PreventsFurtherInstructionExecution()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x10, // STOP
            0x00  // NOP (must not execute once stopped/ halted)
        );

        cpu.StepInstruction();

        ushort pcAfterStop = cpu.State.PC;
        ulong cyclesAfterStop = cpu.State.CycleCount;

        cpu.StepInstruction();

        Assert.True(cpu.State.Halted);
        Assert.Equal(pcAfterStop, cpu.State.PC);
        Assert.Equal(cyclesAfterStop, cpu.State.CycleCount);
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

    [Fact] // Verifies LD SP,d16 writes a 16-bit immediate into SP.
    public void LdSp_d16_LoadsImmediateIntoSp()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x31, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.SP);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount);
    }

    [Fact] // Verifies INC BC increments the BC register pair by one.
    public void IncBc_IncrementsBc()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x01, 0x34, 0x12, // LD BC,0x1234
            0x03              // INC BC
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.B);
        Assert.Equal((byte)0x35, cpu.State.C);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies INC DE increments the DE register pair by one.
    public void IncDe_IncrementsDe()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x11, 0x34, 0x12, // LD DE,0x1234
            0x13              // INC DE
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.D);
        Assert.Equal((byte)0x35, cpu.State.E);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies INC HL increments the HL register pair by one.
    public void IncHl_IncrementsHl()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x21, 0x34, 0x12, // LD HL,0x1234
            0x23              // INC HL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.H);
        Assert.Equal((byte)0x35, cpu.State.L);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies INC SP increments the stack pointer by one.
    public void IncSp_IncrementsSp()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x33);

        cpu.StepInstruction();

        Assert.Equal((ushort)0xFFFF, cpu.State.SP);
        Assert.Equal((ushort)0x0101, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact] // Verifies DEC BC decrements the BC register pair by one.
    public void DecBc_DecrementsBc()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x01, 0x34, 0x12, // LD BC,0x1234
            0x0B              // DEC BC
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.B);
        Assert.Equal((byte)0x33, cpu.State.C);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies DEC DE decrements the DE register pair by one.
    public void DecDe_DecrementsDe()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x11, 0x34, 0x12, // LD DE,0x1234
            0x1B              // DEC DE
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.D);
        Assert.Equal((byte)0x33, cpu.State.E);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies DEC HL decrements the HL register pair by one.
    public void DecHl_DecrementsHl()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x21, 0x34, 0x12, // LD HL,0x1234
            0x2B              // DEC HL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.H);
        Assert.Equal((byte)0x33, cpu.State.L);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies DEC SP decrements the stack pointer by one.
    public void DecSp_DecrementsSp()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x3B);

        cpu.StepInstruction();

        Assert.Equal((ushort)0xFFFD, cpu.State.SP);
        Assert.Equal((ushort)0x0101, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact] // Verifies ADD HL,BC adds BC into HL and updates timing.
    public void AddHl_Bc_AddsBcIntoHl()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x21, 0x34, 0x12, // LD HL,0x1234
            0x01, 0x02, 0x01, // LD BC,0x0102
            0x09              // ADD HL,BC
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x13, cpu.State.H);
        Assert.Equal((byte)0x36, cpu.State.L);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0107, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 12 + 12 + 8
    }

    [Fact] // Verifies ADD HL,DE adds DE into HL and updates timing.
    public void AddHl_De_AddsDeIntoHl()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x21, 0x00, 0x10, // LD HL,0x1000
            0x11, 0x11, 0x01, // LD DE,0x0111
            0x19              // ADD HL,DE
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x11, cpu.State.H);
        Assert.Equal((byte)0x11, cpu.State.L);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0107, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 12 + 12 + 8
    }

    [Fact] // Verifies ADD HL,HL doubles HL and sets half-carry on bit-11 carry.
    public void AddHl_Hl_HalfCarryEdge_SetsHalfCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x21, 0xFF, 0x0F, // LD HL,0x0FFF
            0x29              // ADD HL,HL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x1F, cpu.State.H);
        Assert.Equal((byte)0xFE, cpu.State.L);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies ADD HL,SP sets carry on 16-bit overflow.
    public void AddHl_Sp_CarryEdge_SetsCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x21, 0xFF, 0xFF, // LD HL,0xFFFF
            0x31, 0x01, 0x00, // LD SP,0x0001
            0x39              // ADD HL,SP
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.H);
        Assert.Equal((byte)0x00, cpu.State.L);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0107, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 12 + 12 + 8
    }

    [Fact] // Verifies INC C sets half-carry when low nibble overflows (0x0F -> 0x10).
    public void IncC_HalfCarryEdge_SetsHalfCarryFlag()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x01, 0x0F, 0x12, // LD BC,0x120F
            0x0C              // INC C
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x10, cpu.State.C);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 12 + 4
    }

    [Fact] // Verifies INC C wraps to 0x00 and sets Z/H when incrementing 0xFF.
    public void IncC_OverflowToZero_SetsZeroAndHalfCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x01, 0xFF, 0x12, // LD BC,0x12FF
            0x0C              // INC C
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.C);
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 12 + 4
    }

    [Fact] // Verifies DEC C sets half-carry on half-borrow edge (0x10 -> 0x0F).
    public void DecC_HalfBorrowEdge_SetsHalfCarryFlag()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x01, 0x10, 0x12, // LD BC,0x1210
            0x0D              // DEC C
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x0F, cpu.State.C);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 12 + 4
    }

    [Fact] // Verifies DEC C reaches zero and does not set H when decrementing 0x01.
    public void DecC_ToZero_SetsZeroAndKeepsHalfCarryClear()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x01, 0x01, 0x12, // LD BC,0x1201
            0x0D              // DEC C
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.C);
        Assert.True(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 12 + 4
    }

    [Fact] // Verifies INC (HL) updates memory and sets H on nibble overflow.
    public void IncAtHl_HalfCarryEdge_UpdatesMemoryAndFlags()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x34              // INC (HL)
        );
        bus.WriteByte(0xC000, 0x0F);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x10, bus.ReadByte(0xC000));
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 12 + 12
    }

    [Fact] // Verifies DEC (HL) updates memory and sets H on half-borrow edge.
    public void DecAtHl_HalfBorrowEdge_UpdatesMemoryAndFlags()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x35              // DEC (HL)
        );
        bus.WriteByte(0xC000, 0x10);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x0F, bus.ReadByte(0xC000));
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 12 + 12
    }

    [Fact] // Verifies LD B,d8 loads immediate data into B and updates timing/PC.
    public void LdB_d8_LoadsImmediateValueIntoB()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x06, 0x42);

        cpu.StepInstruction();

        Assert.Equal((byte)0x42, cpu.State.B);
        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD C,d8 loads immediate data into C and updates timing/PC.
    public void LdC_d8_LoadsImmediateValueIntoC()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x0E, 0x42);

        cpu.StepInstruction();

        Assert.Equal((byte)0x42, cpu.State.C);
        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD D,d8 loads immediate data into D and updates timing/PC.
    public void LdD_d8_LoadsImmediateValueIntoD()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x16, 0x42);

        cpu.StepInstruction();

        Assert.Equal((byte)0x42, cpu.State.D);
        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD E,d8 loads immediate data into E and updates timing/PC.
    public void LdE_d8_LoadsImmediateValueIntoE()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x1E, 0x42);

        cpu.StepInstruction();

        Assert.Equal((byte)0x42, cpu.State.E);
        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD H,d8 loads immediate data into H and updates timing/PC.
    public void LdH_d8_LoadsImmediateValueIntoH()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x26, 0x42);

        cpu.StepInstruction();

        Assert.Equal((byte)0x42, cpu.State.H);
        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD L,d8 loads immediate data into L and updates timing/PC.
    public void LdL_d8_LoadsImmediateValueIntoL()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x2E, 0x42);

        cpu.StepInstruction();

        Assert.Equal((byte)0x42, cpu.State.L);
        Assert.Equal((ushort)0x0102, cpu.State.PC);
        Assert.Equal((ulong)8, cpu.State.CycleCount);
    }

    [Fact] // Verifies LD (HL),d8 stores immediate data into memory at HL.
    public void LdAtHl_d8_WritesImmediateValueAtHl()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x36, 0x42        // LD (HL),0x42
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x42, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 12 + 12
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

    [Fact] // Verifies LD (BC),A stores accumulator at address held in BC.
    public void LdBc_A_WritesAccumulatorToAddressInBc()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x01, 0x00, 0xC0, // LD BC,0xC000
            0x3E, 0x7B,       // LD A,0x7B
            0x02              // LD (BC),A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x7B, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 8 + 8
    }

    [Fact] // Verifies LD (DE),A stores accumulator at address held in DE.
    public void LdDe_A_WritesAccumulatorToAddressInDe()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x11, 0x00, 0xC0, // LD DE,0xC000
            0x3E, 0x4D,       // LD A,0x4D
            0x12              // LD (DE),A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x4D, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 8 + 8
    }

    [Fact] // Verifies LD A,(BC) loads accumulator from address held in BC.
    public void LdA_Bc_ReadsAccumulatorFromAddressInBc()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x01, 0x00, 0xC0, // LD BC,0xC000
            0x0A              // LD A,(BC)
        );
        bus.WriteByte(0xC000, 0x6E);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x6E, cpu.State.A);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies LD A,(DE) loads accumulator from address held in DE.
    public void LdA_De_ReadsAccumulatorFromAddressInDe()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x11, 0x00, 0xC0, // LD DE,0xC000
            0x1A              // LD A,(DE)
        );
        bus.WriteByte(0xC000, 0x91);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x91, cpu.State.A);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies LD (a16),SP writes SP as little-endian bytes to absolute address.
    public void LdA16_Sp_WritesSpToAbsoluteAddressLittleEndian()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x31, 0x34, 0x12, // LD SP,0x1234
            0x08, 0x00, 0xC0  // LD (0xC000),SP
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x34, bus.ReadByte(0xC000)); // low byte
        Assert.Equal((byte)0x12, bus.ReadByte(0xC001)); // high byte
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 12 + 20
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
    
    [Fact] // Verifies RLCA rotates bit 7 into carry and bit 0.
    public void Rlca_RotatesBit7IntoCarryAndBit0()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x85, // LD A,0x85
            0x07        // RLCA
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x0B, cpu.State.A); // 1000_0101 -> 0000_1011
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }

    [Fact] // Verifies RLCA on zero keeps A at zero and clears carry.
    public void Rlca_WithZeroInput_KeepsZeroAndClearsCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07        // RLCA
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.False(cpu.State.FlagZ); // RLCA always clears Z on GB CPU
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }

    [Fact] // Verifies RRCA rotates bit 0 into carry and bit 7.
    public void Rrca_RotatesBit0IntoCarryAndBit7()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x01, // LD A,0x01
            0x0F        // RRCA
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x80, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }

    [Fact] // Verifies RLA rotates left through carry.
    public void Rla_RotatesLeftThroughCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> clears carry
            0x3E, 0x80, // LD A,0x80
            0x17        // RLA
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.False(cpu.State.FlagZ); // RLA always clears Z on GB CPU
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);  // old bit7
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 8 + 4 + 8 + 4
    }

    [Fact] // Verifies RRA rotates right through carry.
    public void Rra_RotatesRightThroughCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> clears carry
            0x3E, 0x01, // LD A,0x01
            0x1F        // RRA
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.False(cpu.State.FlagZ); // RRA always clears Z on GB CPU
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);  // old bit0
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 8 + 4 + 8 + 4
    }

    [Fact] // Verifies RRA consumes carry as input into bit 7.
    public void Rra_UsesCarryInForBit7()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> sets carry=1 and A=0x01
            0x3E, 0x00, // LD A,0x00 (carry remains set)
            0x1F        // RRA -> carry-in should set bit 7
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x80, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC); // old bit0 of A=0x00
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 8 + 4 + 8 + 4
    }

    [Fact] // Verifies unimplemented opcodes fail fast with NotSupportedException.
    public void UnsupportedOpcode_Throws()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0xD3);

        Assert.Throws<NotSupportedException>(() => cpu.StepInstruction());
    }
}
