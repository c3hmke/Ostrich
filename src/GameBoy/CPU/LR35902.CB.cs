namespace GameBoy.CPU;

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                        CB-PREFIXED OPCODES                                       //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes a CB-prefixed opcode read after the 0xCB prefix byte. </summary>
    private void ExecuteCBOpcode(byte opcode, bool applyPendingInterruptEnableAfterInstruction)
    {
        int target = opcode & 0x07;           // 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A
        int group  = (opcode >> 3) & 0x1F;    // Encodes operation family and bit index where relevant.

        switch (opcode)
        {
            //---------- ROTATE / SHIFT ----------//
            //--- RLC r
            case >= 0x00 and <= 0x07:
            {
                byte old    = ReadReg8(target);                         // Read the current 8-bit operand.
                bool carry  = (old & 0x80) != 0;                        // Old bit 7 becomes carry and new bit 0.
                byte result = (byte)((old << 1) | (carry ? 1 : 0));     // Rotate left circular.

                WriteReg8(target, result);                              // Write rotated result back to the same target.
                SetFlagsZNHC(
                    z: result == 0,                                     // Set if result is zero.
                    n: false,                                           // Reset.
                    h: false,                                           // Reset.
                    c: carry);                                          // Set from old bit 7.

                CompleteCBInstruction(target, applyPendingInterruptEnableAfterInstruction);
                return;
            }

            //--- RRC r
            case >= 0x08 and <= 0x0F:
            {
                byte old    = ReadReg8(target);                         // Read the current 8-bit operand.
                bool carry  = (old & 0x01) != 0;                        // Old bit 0 becomes carry and new bit 7.
                byte result = (byte)((old >> 1) | (carry ? 0x80 : 0));  // Rotate right circular.

                WriteReg8(target, result);                              // Write rotated result back to the same target.
                SetFlagsZNHC(
                    z: result == 0,                                     // Set if result is zero.
                    n: false,                                           // Reset.
                    h: false,                                           // Reset.
                    c: carry);                                          // Set from old bit 0.

                CompleteCBInstruction(target, applyPendingInterruptEnableAfterInstruction);
                return;
            }
            
            default:
                throw new NotSupportedException($"CB opcode {opcode:X2} not supported");
        }
        
    }
    
    private void CompleteCBInstruction(int targetReg, bool applyPendingInterruptEnable)
    {
        // Timing:  (8/16 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - CB prefix:    4 cycles.
        //  - (HL) form:    8 extra cycles.
        if (targetReg == 6) _state.AddClockCycles(MachineCycle * 3);
        else                _state.AddClockCycles(MachineCycle);

        CompleteInstruction(applyPendingInterruptEnable);
    }
}