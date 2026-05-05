// ReSharper disable UseUtf8StringLiteral; Use hex for byte codes.

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

    [Fact] // Verifies STOP places CPU into stopped execution state.
    public void Stop_SetsStoppedState()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x10);

        cpu.StepInstruction();

        Assert.True(cpu.State.Stopped);
        Assert.False(cpu.State.Halted);
        Assert.Equal((ushort)0x0101, cpu.State.PC);
        Assert.Equal((ulong)4, cpu.State.CycleCount);
    }

    [Fact] // Verifies CPU does not execute following instructions once STOP has been entered.
    public void Stop_PreventsFurtherInstructionExecution()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x10, // STOP
            0x00  // NOP (must not execute once stopped)
        );

        cpu.StepInstruction();

        ushort pcAfterStop = cpu.State.PC;
        ulong cyclesAfterStop = cpu.State.CycleCount;

        cpu.StepInstruction();

        Assert.True(cpu.State.Stopped);
        Assert.False(cpu.State.Halted);
        Assert.Equal(pcAfterStop, cpu.State.PC);
        Assert.Equal(cyclesAfterStop, cpu.State.CycleCount);
    }

    [Fact] // Verifies HALT places CPU into halted execution state.
    public void Halt_SetsHaltedState()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(0x76);

        cpu.StepInstruction();

        Assert.True(cpu.State.Halted);
        Assert.False(cpu.State.Stopped);
        Assert.Equal((ushort)0x0101, cpu.State.PC);
        Assert.Equal((ulong)4, cpu.State.CycleCount);
    }

    [Fact] // Verifies CPU does not execute following instructions once HALT has been entered.
    public void Halt_PreventsFurtherInstructionExecution()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x76, // HALT
            0x00  // NOP (must not execute once halted)
        );

        cpu.StepInstruction();

        ushort pcAfterHalt = cpu.State.PC;
        ulong cyclesAfterHalt = cpu.State.CycleCount;

        cpu.StepInstruction();

        Assert.True(cpu.State.Halted);
        Assert.False(cpu.State.Stopped);
        Assert.Equal(pcAfterHalt, cpu.State.PC);
        Assert.Equal(cyclesAfterHalt, cpu.State.CycleCount);
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

    [Fact] // Verifies DAA add-path adjusts 0x0A to BCD 0x10 after INC A.
    public void Daa_AddPath_AdjustsLowNibbleOverflow()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> clears carry
            0x3E, 0x09, // LD A,0x09
            0x3C,       // INC A -> 0x0A, N=0, H=0
            0x27        // DAA
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x10, cpu.State.A);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.False(cpu.State.FlagZ);
        Assert.Equal((ushort)0x0107, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 8 + 4 + 8 + 4 + 4
    }

    [Fact] // Verifies DAA add-path uses incoming carry and can roll to 0x00.
    public void Daa_AddPath_WithCarry_AdjustsAndKeepsCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> C=1
            0x3E, 0x9A, // LD A,0x9A (flags unchanged)
            0x27        // DAA
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.True(cpu.State.FlagZ);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 8 + 4 + 8 + 4
    }

    [Fact] // Verifies DAA sub-path with H set subtracts 0x06 correction.
    public void Daa_SubPath_WithHalfBorrow_AdjustsDown()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> clears carry
            0x3E, 0x10, // LD A,0x10
            0x3D,       // DEC A -> 0x0F, N=1, H=1
            0x27        // DAA
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x09, cpu.State.A);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.False(cpu.State.FlagZ);
        Assert.Equal((ushort)0x0107, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 8 + 4 + 8 + 4 + 4
    }

    [Fact] // Verifies DAA sub-path uses carry and half-borrow corrections together.
    public void Daa_SubPath_WithCarryAndHalfBorrow_AdjustsToBcd()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> C=1
            0x3E, 0x00, // LD A,0x00 (flags unchanged)
            0x3D,       // DEC A -> 0xFF, N=1, H=1, C stays 1
            0x27        // DAA -> 0x99
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x99, cpu.State.A);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.False(cpu.State.FlagZ);
        Assert.Equal((ushort)0x0107, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 8 + 4 + 8 + 4 + 4
    }

    [Fact] // Verifies CPL flips all bits in accumulator.
    public void Cpl_ComplementsAccumulatorBits()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x3C, // LD A,0x3C
            0x2F        // CPL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xC3, cpu.State.A);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }

    [Fact] // Verifies CPL preserves Z/C and sets N/H.
    public void Cpl_PreservesZAndC_SetsNAndH()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> C=0 and Z=0 per RLCA behavior
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> C=1
            0x3E, 0x00, // LD A,0x00
            0x3C,       // INC A -> A=0x01, Z=0, C unchanged (=1)
            0x3D,       // DEC A -> A=0x00, Z=1, C unchanged (=1)
            0x2F        // CPL
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xFF, cpu.State.A);
        Assert.True(cpu.State.FlagZ);  // preserved
        Assert.True(cpu.State.FlagC);  // preserved
        Assert.True(cpu.State.FlagN);  // set
        Assert.True(cpu.State.FlagH);  // set
        Assert.Equal((ushort)0x010B, cpu.State.PC);
        Assert.Equal((ulong)44, cpu.State.CycleCount); // 8+4+8+4+8+4+4+4+4
    }
    
    [Fact] // Verifies SCF sets carry and clears N/H.
    public void Scf_SetsCarryAndClearsNAndH()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x10, // LD A,0x10
            0x3D,       // DEC A -> N=1, H=1, Z=0
            0x37        // SCF
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.True(cpu.State.FlagC);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagZ); // unchanged from DEC result (A=0x0F)
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 4 + 4
    }

    [Fact] // Verifies SCF preserves Z while forcing carry set.
    public void Scf_PreservesZFlag()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x01, // LD A,0x01
            0x3D,       // DEC A -> A=0x00, Z=1, N=1
            0x37        // SCF
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.True(cpu.State.FlagZ);  // unchanged
        Assert.True(cpu.State.FlagC);  // set
        Assert.False(cpu.State.FlagN); // reset
        Assert.False(cpu.State.FlagH); // reset
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 4 + 4
    }
    
    [Fact] // Verifies CCF toggles carry from 0 to 1 and clears N/H.
    public void Ccf_TogglesCarryFromZeroToOne()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> C=0
            0x3F        // CCF
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.True(cpu.State.FlagC);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagZ); // unchanged from RLCA (Z always 0)
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)16, cpu.State.CycleCount); // 8 + 4 + 4
    }

    [Fact] // Verifies CCF toggles carry from 1 to 0 while preserving Z.
    public void Ccf_TogglesCarryFromOneToZero_AndPreservesZ()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x01, // LD A,0x01
            0x3D,       // DEC A -> A=0x00, Z=1
            0x3E, 0x80, // LD A,0x80 (Z unchanged)
            0x07,       // RLCA -> C=1, Z forced 0
            0x3E, 0x01, // LD A,0x01
            0x3D,       // DEC A -> A=0x00, Z=1 (C unchanged = 1)
            0x3F        // CCF -> C=0, Z preserved
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.False(cpu.State.FlagC);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.True(cpu.State.FlagZ);  // unchanged by CCF
        Assert.Equal((ushort)0x010A, cpu.State.PC);
        Assert.Equal((ulong)40, cpu.State.CycleCount); // 8+4+8+4+8+4+4
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

    [Fact] // Verifies LD B,C copies source register C into destination register B.
    public void LdB_C_CopiesRegisterToRegister()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x0E, 0x42, // LD C,0x42
            0x41        // LD B,C
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x42, cpu.State.B);
        Assert.Equal((byte)0x42, cpu.State.C);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }

    [Fact] // Verifies LD A,(HL) reads from memory via LD r,r' matrix timing/path.
    public void LdA_Hl_ReadsFromMemoryViaLdMatrix()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x7E              // LD A,(HL)
        );
        bus.WriteByte(0xC000, 0x5A);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x5A, cpu.State.A);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Fact] // Verifies LD (HL),A writes to memory via LD r,r' matrix timing/path.
    public void LdHl_A_WritesToMemoryViaLdMatrix()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x3E, 0x77,       // LD A,0x77
            0x77              // LD (HL),A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x77, bus.ReadByte(0xC000));
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 8 + 8
    }

    [Fact] // Verifies LD D,(HL) reads memory into non-A register through matrix decode.
    public void LdD_Hl_ReadsIntoRegisterViaLdMatrix()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x56              // LD D,(HL)
        );
        bus.WriteByte(0xC000, 0xAB);

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xAB, cpu.State.D);
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 12 + 8
    }

    [Theory] // Verifies representative LD r,r' matrix opcodes across reg-reg and HL forms.
    [InlineData(0x78, 0xAA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 60UL)] // LD A,B
    [InlineData(0x5D, 0x00, 0xCC, 0x00, 0x00, 0x00, 0xCC, 0x00, 0x00, 0x00, 60UL)] // LD E,L
    [InlineData(0x6F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x77, 0x00, 0x00, 60UL)] // LD L,A
    [InlineData(0x46, 0x00, 0x00, 0x00, 0x00, 0xC0, 0x00, 0x00, 0x33, 0x33, 64UL)] // LD B,(HL)
    [InlineData(0x70, 0x99, 0x00, 0x00, 0x00, 0xC0, 0x00, 0x00, 0x00, 0x99, 64UL)] // LD (HL),B
    public void LdMatrix_RepresentativeOpcodes_WorkAsExpected(
        byte opcode, byte bInit, byte cInit, byte dInit, byte eInit, byte hInit,
        byte lInit, byte aInit, byte hlMemInit, byte expectedHlMem, ulong expectedCycles)
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x06, bInit,       // LD B,d8
            0x0E, cInit,       // LD C,d8
            0x16, dInit,       // LD D,d8
            0x1E, eInit,       // LD E,d8
            0x26, hInit,       // LD H,d8
            0x2E, lInit,       // LD L,d8
            0x3E, aInit,       // LD A,d8
            opcode             // LD r,r'
        );

        ushort hlAddr = (ushort)((hInit << 8) | lInit);
        bus.WriteByte(hlAddr, hlMemInit);

        for (int i = 0; i < 8; i++)
            cpu.StepInstruction();

        Assert.Equal(((opcode == 0x46) ? hlMemInit : bInit), cpu.State.B);
        Assert.Equal(cInit, cpu.State.C);
        Assert.Equal(dInit, cpu.State.D);
        Assert.Equal(((opcode == 0x5D) ? lInit : eInit), cpu.State.E);
        Assert.Equal(hInit, cpu.State.H);
        Assert.Equal(((opcode == 0x6F) ? aInit : lInit), cpu.State.L);
        Assert.Equal(((opcode == 0x78) ? bInit : aInit), cpu.State.A);

        Assert.Equal(expectedHlMem, bus.ReadByte(hlAddr));
        Assert.Equal((ushort)0x010F, cpu.State.PC);
        Assert.Equal(expectedCycles, cpu.State.CycleCount);
    }

    [Fact] // Verifies ADD A,B sums register operand into A.
    public void AddA_B_AddsRegisterOperand()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x12, // LD A,0x12
            0x06, 0x22, // LD B,0x22
            0x80        // ADD A,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x34, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies ADD A,E sets half-carry when low nibble overflows.
    public void AddA_E_HalfCarryEdge_SetsHalfCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x0F, // LD A,0x0F
            0x1E, 0x01, // LD E,0x01
            0x83        // ADD A,E
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x10, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies ADD A,H sets carry and zero on 8-bit overflow.
    public void AddA_H_CarryAndZeroEdge_SetsCarryAndZero()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0xFF, // LD A,0xFF
            0x26, 0x01, // LD H,0x01
            0x84        // ADD A,H
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies ADD A,(HL) reads memory operand and uses 8-cycle ALU timing.
    public void AddA_Hl_ReadsMemoryOperandAndAdds()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x3E, 0x10,       // LD A,0x10
            0x86              // ADD A,(HL)
        );
        bus.WriteByte(0xC000, 0x22);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x32, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 8 + 8
    }

    [Fact] // Verifies ADD A,A doubles accumulator using same ALU path.
    public void AddA_A_DoublesAccumulator()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x20, // LD A,0x20
            0x87        // ADD A,A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x40, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }

    [Fact] // Verifies ADC A,B adds register operand when carry-in is clear.
    public void AdcA_B_WithoutCarryIn_AddsRegisterOperand()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> C=0
            0x3E, 0x12, // LD A,0x12
            0x06, 0x22, // LD B,0x22
            0x88        // ADC A,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x34, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 8 + 8 + 4
    }

    [Fact] // Verifies ADC A,B consumes carry-in when carry flag is set.
    public void AdcA_B_WithCarryIn_AddsCarryIn()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> C=1
            0x3E, 0x12, // LD A,0x12
            0x06, 0x22, // LD B,0x22
            0x88        // ADC A,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x35, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 8 + 8 + 4
    }

    [Fact] // Verifies ADC A,E sets half-carry on nibble overflow with carry-in.
    public void AdcA_E_HalfCarryEdge_SetsHalfCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> C=1
            0x3E, 0x0F, // LD A,0x0F
            0x1E, 0x00, // LD E,0x00
            0x8B        // ADC A,E
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x10, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 8 + 8 + 4
    }

    [Fact] // Verifies ADC A,H sets carry and zero on 8-bit overflow with carry-in.
    public void AdcA_H_CarryAndZeroEdge_SetsCarryAndZero()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> C=1
            0x3E, 0xFF, // LD A,0xFF
            0x26, 0x00, // LD H,0x00
            0x8C        // ADC A,H
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 8 + 8 + 4
    }

    [Fact] // Verifies ADC A,(HL) reads memory operand and uses 8-cycle ALU timing.
    public void AdcA_Hl_ReadsMemoryOperandAndUsesEightCycleTiming()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x3E, 0x80,       // LD A,0x80
            0x07,             // RLCA -> C=1
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x3E, 0x10,       // LD A,0x10
            0x8E              // ADC A,(HL)
        );
        bus.WriteByte(0xC000, 0x22);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x33, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0109, cpu.State.PC);
        Assert.Equal((ulong)40, cpu.State.CycleCount); // 8 + 4 + 12 + 8 + 8
    }

    [Fact] // Verifies SUB A,B subtracts register operand from A.
    public void SubA_B_SubtractsRegisterOperand()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x34, // LD A,0x34
            0x06, 0x12, // LD B,0x12
            0x90        // SUB A,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x22, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies SUB A,E sets half-borrow when low nibble borrows.
    public void SubA_E_HalfBorrowEdge_SetsHalfCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x10, // LD A,0x10
            0x1E, 0x01, // LD E,0x01
            0x93        // SUB A,E
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x0F, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies SUB A,H sets carry when full borrow occurs.
    public void SubA_H_BorrowEdge_SetsCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x26, 0x01, // LD H,0x01
            0x94        // SUB A,H
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xFF, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies SUB A,(HL) reads memory operand and uses 8-cycle ALU timing.
    public void SubA_Hl_ReadsMemoryOperandAndSubtracts()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x3E, 0x34,       // LD A,0x34
            0x96              // SUB A,(HL)
        );
        bus.WriteByte(0xC000, 0x12);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x22, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 8 + 8
    }

    [Fact] // Verifies SUB A,A produces zero with N set and no borrow flags.
    public void SubA_A_ProducesZero()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x20, // LD A,0x20
            0x97        // SUB A,A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }

    [Fact] // Verifies SBC A,B subtracts register operand when carry-in is clear.
    public void SbcA_B_WithoutCarryIn_SubtractsRegisterOperand()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> C=0
            0x3E, 0x34, // LD A,0x34
            0x06, 0x12, // LD B,0x12
            0x98        // SBC A,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x22, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 8 + 8 + 4
    }

    [Fact] // Verifies SBC A,B consumes carry-in during subtraction.
    public void SbcA_B_WithCarryIn_SubtractsCarryIn()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> C=1
            0x3E, 0x34, // LD A,0x34
            0x06, 0x12, // LD B,0x12
            0x98        // SBC A,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x21, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 8 + 8 + 4
    }

    [Fact] // Verifies SBC A,E sets half-borrow on nibble borrow with carry-in.
    public void SbcA_E_HalfBorrowEdge_SetsHalfCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> C=1
            0x3E, 0x10, // LD A,0x10
            0x1E, 0x00, // LD E,0x00
            0x9B        // SBC A,E
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x0F, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 8 + 8 + 4
    }

    [Fact] // Verifies SBC A,H sets carry on full borrow with carry-in.
    public void SbcA_H_BorrowEdge_SetsCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x80, // LD A,0x80
            0x07,       // RLCA -> C=1
            0x3E, 0x00, // LD A,0x00
            0x26, 0x00, // LD H,0x00
            0x9C        // SBC A,H
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xFF, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0108, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 8 + 4 + 8 + 8 + 4
    }

    [Fact] // Verifies SBC A,(HL) reads memory operand and uses 8-cycle ALU timing.
    public void SbcA_Hl_ReadsMemoryOperandAndSubtractsWithCarry()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x3E, 0x80,       // LD A,0x80
            0x07,             // RLCA -> C=1
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x3E, 0x34,       // LD A,0x34
            0x9E              // SBC A,(HL)
        );
        bus.WriteByte(0xC000, 0x12);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x21, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0109, cpu.State.PC);
        Assert.Equal((ulong)40, cpu.State.CycleCount); // 8 + 4 + 12 + 8 + 8
    }

    [Fact] // Verifies SBC A,A produces zero when carry-in is clear.
    public void SbcA_A_ProducesZeroWhenCarryClear()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> C=0
            0x3E, 0x20, // LD A,0x20
            0x9F        // SBC A,A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)24, cpu.State.CycleCount); // 8 + 4 + 8 + 4
    }
    
    [Fact] // Verifies AND A,B computes bitwise AND and updates flags.
    public void AndA_B_ComputesBitwiseAnd()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0xF0, // LD A,0xF0
            0x06, 0x3C, // LD B,0x3C
            0xA0        // AND A,B
        );
    
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
    
        Assert.Equal((byte)0x30, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }
    
    [Fact] // Verifies AND A,E can produce zero and sets Z while forcing H=1.
    public void AndA_E_ZeroEdge_SetsZeroFlag()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x0F, // LD A,0x0F
            0x1E, 0xF0, // LD E,0xF0
            0xA3        // AND A,E
        );
    
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
    
        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }
    
    [Fact] // Verifies AND A,(HL) reads memory operand and uses 8-cycle ALU timing.
    public void AndA_Hl_ReadsMemoryOperandAndComputesAnd()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x3E, 0xAA,       // LD A,0xAA
            0xA6              // AND A,(HL)
        );
        bus.WriteByte(0xC000, 0x0F);
    
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
    
        Assert.Equal((byte)0x0A, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 8 + 8
    }
    
    [Fact] // Verifies AND A,A leaves A unchanged apart from flag updates.
    public void AndA_A_LeavesAccumulatorValue()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x5A, // LD A,0x5A
            0xA7        // AND A,A
        );
    
        cpu.StepInstruction();
        cpu.StepInstruction();
    
        Assert.Equal((byte)0x5A, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }

    [Fact] // Verifies XOR A,B computes bitwise XOR and updates flags.
    public void XorA_B_ComputesBitwiseXor()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0xF0, // LD A,0xF0
            0x06, 0x3C, // LD B,0x3C
            0xA8        // XOR A,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xCC, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies XOR A,E can produce zero and sets Z flag.
    public void XorA_E_ZeroEdge_SetsZeroFlag()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x5A, // LD A,0x5A
            0x1E, 0x5A, // LD E,0x5A
            0xAB        // XOR A,E
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies XOR A,(HL) reads memory operand and uses 8-cycle ALU timing.
    public void XorA_Hl_ReadsMemoryOperandAndComputesXor()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x3E, 0xAA,       // LD A,0xAA
            0xAE              // XOR A,(HL)
        );
        bus.WriteByte(0xC000, 0x0F);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xA5, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 8 + 8
    }

    [Fact] // Verifies XOR A,A produces zero and clears N/H/C.
    public void XorA_A_ProducesZero()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x5A, // LD A,0x5A
            0xAF        // XOR A,A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }
    
    [Fact] // Verifies OR A,B computes bitwise OR and updates flags.
    public void OrA_B_ComputesBitwiseOr()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0xF0, // LD A,0xF0
            0x06, 0x0F, // LD B,0x0F
            0xB0        // OR A,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xFF, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies OR A,E can produce zero and sets Z flag.
    public void OrA_E_ZeroEdge_SetsZeroFlag()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x1E, 0x00, // LD E,0x00
            0xB3        // OR A,E
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies OR A,(HL) reads memory operand and uses 8-cycle ALU timing.
    public void OrA_Hl_ReadsMemoryOperandAndComputesOr()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x3E, 0xA0,       // LD A,0xA0
            0xB6              // OR A,(HL)
        );
        bus.WriteByte(0xC000, 0x0F);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xAF, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 8 + 8
    }

    [Fact] // Verifies OR A,A leaves A unchanged apart from flag updates.
    public void OrA_A_LeavesAccumulatorValue()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x5A, // LD A,0x5A
            0xB7        // OR A,A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x5A, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.False(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
    }

    [Fact] // Verifies CP A,B sets Z on equality and leaves A unchanged.
    public void CpA_B_Equal_SetsZeroAndKeepsA()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x34, // LD A,0x34
            0x06, 0x34, // LD B,0x34
            0xB8        // CP A,B
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x34, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies CP A,E sets half-borrow on nibble borrow and leaves A unchanged.
    public void CpA_E_HalfBorrowEdge_SetsHalfCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x10, // LD A,0x10
            0x1E, 0x01, // LD E,0x01
            0xBB        // CP A,E
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x10, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies CP A,H sets carry on full borrow and leaves A unchanged.
    public void CpA_H_BorrowEdge_SetsCarry()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x26, 0x01, // LD H,0x01
            0xBC        // CP A,H
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x00, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.True(cpu.State.FlagH);
        Assert.True(cpu.State.FlagC);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 8 + 4
    }

    [Fact] // Verifies CP A,(HL) reads memory operand and uses 8-cycle ALU timing.
    public void CpA_Hl_ReadsMemoryOperandAndUsesEightCycleTiming()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x21, 0x00, 0xC0, // LD HL,0xC000
            0x3E, 0x34,       // LD A,0x34
            0xBE              // CP A,(HL)
        );
        bus.WriteByte(0xC000, 0x12);

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x34, cpu.State.A);
        Assert.False(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0106, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 8 + 8
    }

    [Fact] // Verifies CP A,A sets Z and leaves A unchanged.
    public void CpA_A_SetsZeroAndKeepsA()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x5A, // LD A,0x5A
            0xBF        // CP A,A
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x5A, cpu.State.A);
        Assert.True(cpu.State.FlagZ);
        Assert.True(cpu.State.FlagN);
        Assert.False(cpu.State.FlagH);
        Assert.False(cpu.State.FlagC);
        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ulong)12, cpu.State.CycleCount); // 8 + 4
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
