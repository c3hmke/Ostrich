namespace GameBoy.CPU;

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                           LOAD8 OPCODES                                          //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes opcodes that load 8-bit values between registers and memory. </summary>

    /// <summary> Load an immediate 8-bit value into a register or the byte at HL. </summary>
    private void LD_r_d8(ushort opcode)
    {
        // Read the next 8-bits from bus; advances PC past that byte.
        byte val = ReadNextByte();
                
        // Bits 5-3 encode the destination register index:
        // 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int dest = (opcode >> 3) & 0x07;
                
        WriteReg8(dest, val);
                
                
        // Timing:  (8/12 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - LD r,d8:      4 cycles.
        //  - LD (HL),d8:   8 cycles.
        _state.AddClockCycles((dest == 6) ? MachineCycle * 2 : MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Copy an 8-bit register or the byte at HL into another 8-bit target. </summary>
    private void LD_r_r(ushort opcode)
    {
        // Encoding:
        int dst = (opcode >> 3) & 0x07; // bits 5-3 = destination register (B,C,D,E,H,L,(HL),A)
        int src = opcode & 0x07;        // bits 2-0 = source register      (B,C,D,E,H,L,(HL),A)
                
        // Read from source register/memory and write to destination register/memory.
        WriteReg8(dst, ReadReg8(src));
                
        // Timing:  (8/12 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - r->r form:    0 cycles.
        //  - HL forms:     4 cycles.
        if (src == 6 || dst == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Store A into memory addressed by BC or DE. </summary>
    private void LD_BCDE_A(ushort opcode)
    {
        // Select the destination address from BC/DE based on opcode.
        ushort dest = (opcode == 0x02 ? _state.BC : _state.DE);
                
        // Store accumulator into memory at the destination address.
        Bus.WriteByte(dest, _state.A);
                
        // Timing:  (8 total)
        //  - opcode fetch: 4 cycles.
        //  - LD (BC/DE),A: 4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Load A from memory addressed by BC or DE. </summary>
    private void LD_A_BCDE(ushort opcode)
    {
        // Select the source address from BC/DE based on opcode.
        ushort src = (opcode == 0x0A ? _state.BC : _state.DE);
                
        // Load accumulator from memory at the source address.
        _state.A = Bus.ReadByte(src);
                
        // Timing:  (8 total)
        //  - opcode fetch: 4 cycles.
        //  - LD A,(BC/DE): 4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Store A into memory at HL, then increment or decrement HL. </summary>
    private void LD_HL_A(ushort opcode)
    {
        // Write to HL then increment or decrement based on opcode.
        WriteAtHL(_state.A, 
            opcode == 0x22 ? HLStep.Increment : HLStep.Decrement);

        // Timing:  (8 total)
        //  - opcode fetch: 4 cycles.
        //  - LD (HL+/-),A: 4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Load A from memory at HL, then increment or decrement HL. </summary>
    private void LD_A_HL(ushort opcode)
    {
        // Read from HL then increment or decrement based on opcode.
        _state.A = ReadAtHL(
            opcode == 0x2A ? HLStep.Increment : HLStep.Decrement);

        // Timing:  (8 total)
        //  - opcode fetch: 4 cycles.
        //  - LD (HL+/-),A: 4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Store A into high memory at address FF00 plus an immediate 8-bit offset. </summary>
    private void LDH_a8_A()
    {
        byte offset = ReadNextByte();               // Read 8-bit offset from instruction stream.
        ushort addr = (ushort)(0xFF00 + offset);    // LDH uses high-memory I/O space at 0xFF00 + a8.
                
        Bus.WriteByte(addr, _state.A);              // Store accumulator into high-memory I/O/register space.
                
        // Timing:  (12 total)
        //  - opcode fetch: 4 cycles.
        //  - LDH (a8),A:   8 cycles.
        _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }

    /// <summary> Load A from high memory at address FF00 plus an immediate 8-bit offset. </summary>
    private void LDH_A_a8()
    {
        byte offset = ReadNextByte();               // Read 8-bit offset from instruction stream.
        ushort addr = (ushort)(0xFF00 + offset);    // LDH uses high-memory I/O space at 0xFF00 + a8.
        
        _state.A = Bus.ReadByte(addr);              // Load accumulator from high-memory I/O/register space.
                
        // Timing:  (12 total)
        //  - opcode fetch: 4 cycles.
        //  - LDH A,(a8):   8 cycles.
        _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }

    /// <summary> Store A into high memory at address FF00 plus C. </summary>
    private void LD_C_A()
    {
        ushort addr = (ushort)(0xFF00 + _state.C);  // Uses high-memory I/O space addressed by register C.
        Bus.WriteByte(addr, _state.A);              // Store accumulator into address 0xFF00 + C.

        // Timing:  (8 total)
        //  - opcode fetch: 4 cycles.
        //  - LD (C),A:     4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Load A from high memory at address FF00 plus C. </summary>
    private void LD_A_C()
    {
        ushort addr = (ushort)(0xFF00 + _state.C);   // Uses high-memory I/O space addressed by register C.
        _state.A = Bus.ReadByte(addr);               // Load accumulator from address 0xFF00 + C.

        // Timing:  (8 total)
        //  - opcode fetch: 4 cycles.
        //  - LD A,(C):     4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Store A into memory at an immediate 16-bit address. </summary>
    private void LD_a16_A()
    {
        ushort addr = ReadNextWord();               // Read full 16-bit address from instruction stream.
        Bus.WriteByte(addr, _state.A);              // Store accumulator at absolute address.

        // Timing:  (16 total)
        //  - opcode fetch: 4 cycles.
        //  - LD (a16),A:   12 cycles.
        _state.AddClockCycles(MachineCycle * 3);
        CompleteInstruction();
    }

    /// <summary> Load A from memory at an immediate 16-bit address. </summary>
    private void LD_A_a16()
    {
        ushort addr = ReadNextWord();               // Read full 16-bit address from instruction stream.
        _state.A    = Bus.ReadByte(addr);           // Load accumulator from absolute address.

        // Timing:  (16 total)
        //  - opcode fetch: 4 cycles.
        //  - LD A,(a16):   12 cycles.
        _state.AddClockCycles(MachineCycle * 3);
        CompleteInstruction();
    }
}
