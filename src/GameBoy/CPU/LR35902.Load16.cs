namespace GameBoy.CPU;

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                           LOAD16 OPCODES                                         //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes opcodes that load 16-bit values between registers and memory. </summary>

    /// <summary> Load an immediate 16-bit value into BC, DE, HL, or SP. </summary>
    private void LD_rr_d16(ushort opcode)
    {
        // Read the next 16-bits from operand; advances PC past both bytes.
        ushort val = ReadNextWord();

        // bits 5-4 of these opcodes encode which 16-bit register pair to load:
        // 00=BC, 01=DE, 10=HL, 11=SP
        switch ((opcode >> 4) & 0x03)
        {
            case 0x00: _state.BC = val; break;
            case 0x01: _state.DE = val; break;
            case 0x02: _state.HL = val; break;
            case 0x03: _state.SP = val; break;
        }
                
        // Timing:  (12 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - LD rr,d16:    8 cycles.
        _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }
    
    /// <summary> Copy HL into SP. </summary>
    private void LD_SP_HL()
    {
        _state.SP = _state.HL;   // Copy the 16-bit value in HL into SP.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - LD SP,HL:     4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }
    
    /// <summary> Store SP into memory at an immediate 16-bit address. </summary>
    private void LD_a16_SP()
    {
        // Read the destination address from the immediate operand.
        ushort dest = ReadNextWord();

        // Store SP as little-endian at [a16] and [a16+1].
        Bus.WriteByte(dest, (byte)(_state.SP & 0x00FF));
        Bus.WriteByte((ushort)(dest + 1), (byte)(_state.SP >> 8));

        // Timing:  (20 total)
        //  - opcode fetch:  4 cycles.
        //  - LD (a16),SP:   16 cycles.
        _state.AddClockCycles(MachineCycle * 4);
        CompleteInstruction();
    }
}
