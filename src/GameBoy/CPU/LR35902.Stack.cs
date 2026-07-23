namespace GameBoy.CPU;

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                           STACK OPCODES                                          //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes opcodes that push and pop 16-bit register pairs on the stack. </summary>

    /// <summary> Push BC, DE, HL, or AF onto the stack. </summary>
    private void PUSH_rr(ushort opcode)
    {
        // Bits 5-4 encode source pair: 00=BC, 01=DE, 10=HL, 11=AF.
        ushort val = ((opcode >> 4) & 0x03) switch
        {
            0x00 => _state.BC,
            0x01 => _state.DE,
            0x02 => _state.HL,
            _    => _state.AF,
        };
                
        PushWord(val);  // Push 16-bit (little-endian) value onto stack, SP-=2
                
        // Timing:  (16 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - PUSH rr:      12 cycles
        _state.AddClockCycles(MachineCycle * 3);
        CompleteInstruction();
    }

    /// <summary> Pop a 16-bit value from the stack into BC, DE, HL, or AF. </summary>
    private void POP_rr(ushort opcode)
    {
        ushort val = PopWord(); // pop 16-bit (little-endian) value from stack, SP += 2

        // Bits 5-4 encode destination pair: 00=BC, 01=DE, 10=HL, 11=AF.
        switch ((opcode >> 4) & 0x03)
        {
            case 0x00: _state.BC = val; break;
            case 0x01: _state.DE = val; break;
            case 0x02: _state.HL = val; break;
            case 0x03: _state.AF = val; break; // AF setter masks low nibble of F
        }
                
        // Timing:  (12 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - POP rr:       8 cycles
        _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }
    
    //--------------------------------------------------------------------------------------------------//
    //                                            HELPERS                                               //
    //--------------------------------------------------------------------------------------------------//
    
    /// <summary> Push a word onto the stack. </summary>
    private void PushWord(ushort value)
    {
        if (_bus is null) return;

        _state.SP--; _bus.WriteByte(_state.SP, (byte)(value >> 8));   // high byte
        _state.SP--; _bus.WriteByte(_state.SP, (byte)(value & 0xFF)); // low byte
    }
    
    /// <summary> Pop a word from the stack. </summary>
    private ushort PopWord()
    {
        if (_bus is null) return 0;
        
        byte lo = _bus.ReadByte(_state.SP); _state.SP++;
        byte hi = _bus.ReadByte(_state.SP); _state.SP++;
        
        return (ushort)((hi << 8) | lo);
    }
}
