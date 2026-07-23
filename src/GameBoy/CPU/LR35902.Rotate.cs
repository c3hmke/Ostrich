namespace GameBoy.CPU;

// ReSharper disable InconsistentNaming; Using opcode names for function names, doesn't follow C# convention.

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                           ROTATE OPCODES                                         //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes accumulator rotate opcodes that shift A through or around the carry flag. </summary>

    /// <summary> Rotate A left circular, moving old bit 7 into carry and bit 0. </summary>
    private void RCLA()
    {
        bool carry = (_state.A & 0x80) != 0;                         // Capture bit 7, this becomes carry & new bit 0.
        _state.A   = (byte)((_state.A << 1) | (carry ? 1 : 0));      // Rotate A left circular.
                
        // RLCA flags on LR35902: Z=0, N=0, H=0, C=old bit7.
        SetFlagsZNHC(z: false, n: false, h: false, c: carry);
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> Rotate A right circular, moving old bit 0 into carry and bit 7. </summary>
    private void RRCA()
    {
        bool carry = (_state.A & 0x01) != 0;                          // Capture bit 0, this becomes carry & new bit 7.
        _state.A   = (byte)((_state.A >> 1) | (carry ? 0x80 : 0x00)); // Rotate A right circular.
                
        // RRCA flags on LR35902: Z=0, N=0, H=0, C=old bit7.
        SetFlagsZNHC(z: false, n: false, h: false, c: carry);

        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> Rotate A left through the carry flag. </summary>
    private void RLA()
    {
        bool oldCarry = _state.FlagC;                            // RLA Rotates left through carry:
        bool carryOut = (_state.A & 0x80) != 0;                  // old carry -> bit 0, old bit 7 -> new carry.
                
        _state.A = (byte)((_state.A << 1) | (oldCarry ? 1 : 0));
                
        // RLA flags on LR35902: Z=0, N=0, H=0, C=old bit7.
        SetFlagsZNHC(z: false, n: false, h: false, c: carryOut);
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> Rotate A right through the carry flag. </summary>
    private void RRA()
    {
        bool oldCarry = _state.FlagC;                            // RLA Rotates right through carry:
        bool carryOut = (_state.A & 0x01) != 0;                  // old carry -> bit 0, old bit 7 -> new carry.
                
        _state.A = (byte)((_state.A >> 1) | (oldCarry ? 0x80 : 0x00));
                
        // RLA flags on LR35902: Z=0, N=0, H=0, C=old bit7.
        SetFlagsZNHC(z: false, n: false, h: false, c: carryOut);
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }
}
