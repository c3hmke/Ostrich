namespace GameBoy.CPU;

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                           ALU16 OPCODES                                          //
    //--------------------------------------------------------------------------------------------------//
    /// Executes opcodes used for the 16-bit Arithmetic Logic Unit.

    /// <summary> Increment one 16-bit register pair: BC, DE, HL, or SP. </summary>
    private void INC_rr(ushort opcode)
    {
        // Bits 5-4 encode the 16-bit register target:
        // 00=BC, 01=DE, 10=HL, 11=SP.
        switch ((opcode >> 4) & 0x03)
        {
            case 0x00: _state.BC++; break;
            case 0x01: _state.DE++; break;
            case 0x02: _state.HL++; break;
            case 0x03: _state.SP++; break;
        }
        // INC rr does not affect flags on LR35902.
                
        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - INC rr:       4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Decrement one 16-bit register pair: BC, DE, HL, or SP. </summary>
    private void DEC_rr(ushort opcode)
    {
        // Bits 5-4 encode the 16-bit register target:
        // 00=BC, 01=DE, 10=HL, 11=SP.
        switch ((opcode >> 4) & 0x03)
        {
            case 0x00: _state.BC--; break;
            case 0x01: _state.DE--; break;
            case 0x02: _state.HL--; break;
            case 0x03: _state.SP--; break;
        }
        // DEC rr does not affect flags on LR35902.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - DEC rr:       4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Add a 16-bit register pair to HL. </summary>
    private void ADD_HL_rr(ushort opcode)
    {
        // Bits 5-4 encode the 16-bit source register pair:
        // 00=BC, 01=DE, 10=HL, 11=SP.
        ushort rr = ((opcode >> 4) & 0x03) switch
        {
            0x00 => _state.BC,
            0x01 => _state.DE,
            0x02 => _state.HL,
            _    => _state.SP
        };
                
        ushort hl  = _state.HL;     // Keep old HL for half-carry/carry checks.
        int    sum = hl + rr;       // Then add RR to HL.
        _state.HL  = (ushort)sum;   // Write back the result.
                
        SetFlagsZNHC(
            z: _state.FlagZ,                                // unchanged.
            n: false,                                       // 0.
            h: ((hl & 0x0FFF) + (rr & 0x0FFF)) > 0x0FFF,    // carry from bit 11
            c: sum > 0xFFFF);                               // carry from bit 15
                
        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - ADD HL,rr:    4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Add a signed 8-bit immediate value to SP. </summary>
    private void ADD_SP_e8()
    {
        byte e8   = ReadNextByte();     // Read signed 8-bit immediate operand.
        _state.SP = AddSignedToSP(e8);  // Add signed immediate to SP.
                
        // Helper applies LR35902 flag behavior for this instruction:
        // Z=0, N=0, H/C from low-byte carry behavior.
                
        // Timing:  (16 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - ADD SP,e8:    12 cycles.
        _state.AddClockCycles(MachineCycle * 3);
        CompleteInstruction();
    }

    /// <summary> Store SP plus a signed 8-bit immediate value in HL. </summary>
    private void LD_HL_SPe8()
    {
        byte e8   = ReadNextByte();     // Read signed 8-bit immediate operand.
        _state.HL = AddSignedToSP(e8);  // Compute SP + signed immediate and store into HL.
                
        // Uses same flag behavior as ADD SP,e8 via shared helper.

        // Timing:  (12 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - LD HL,SP+e8:  8 cycles.
        _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }
}
