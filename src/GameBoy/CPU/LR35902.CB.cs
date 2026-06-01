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

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
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

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }
            
            //--- RL r
            case >= 0x10 and <= 0x17:
            {
                byte old      = ReadReg8(target);                     // Read the current 8-bit operand.
                bool oldCarry = _state.FlagC;                         // Old carry is shifted into bit 0.
                bool carryOut = (old & 0x80) != 0;                    // Old bit 7 becomes the new carry.
                byte result   = (byte)((old << 1) | (oldCarry ? 1 : 0)); // Rotate left through carry.

                WriteReg8(target, result);                            // Write rotated result back to the same target.
                SetFlagsZNHC(
                    z: result == 0,                                   // Set if result is zero.
                    n: false,                                         // Reset.
                    h: false,                                         // Reset.
                    c: carryOut);                                     // Set from old bit 7.

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }

            //--- RR r
            case >= 0x18 and <= 0x1F:
            {
                byte old      = ReadReg8(target);                          // Read the current 8-bit operand.
                bool oldCarry = _state.FlagC;                              // Old carry is shifted into bit 7.
                bool carryOut = (old & 0x01) != 0;                         // Old bit 0 becomes the new carry.
                byte result   = (byte)((old >> 1) | (oldCarry ? 0x80 : 0));// Rotate right through carry.

                WriteReg8(target, result);                                 // Write rotated result back to the same target.
                SetFlagsZNHC(
                    z: result == 0,                                        // Set if result is zero.
                    n: false,                                              // Reset.
                    h: false,                                              // Reset.
                    c: carryOut);                                          // Set from old bit 0.

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }
            
            //--- SLA r
            case >= 0x20 and <= 0x27:
            {
                byte old    = ReadReg8(target);                     // Read the current 8-bit operand.
                bool carry  = (old & 0x80) != 0;                    // Old bit 7 becomes the new carry.
                byte result = (byte)(old << 1);                     // Shift left arithmetic; bit 0 becomes 0.

                WriteReg8(target, result);                          // Write shifted result back to the same target.
                SetFlagsZNHC(
                    z: result == 0,                                 // Set if result is zero.
                    n: false,                                       // Reset.
                    h: false,                                       // Reset.
                    c: carry);                                      // Set from old bit 7.

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }

            //--- SRA r
            case >= 0x28 and <= 0x2F:
            {
                byte old    = ReadReg8(target);                      // Read the current 8-bit operand.
                bool carry  = (old & 0x01) != 0;                     // Old bit 0 becomes the new carry.
                byte result = (byte)((old >> 1) | (old & 0x80));     // Shift right arithmetic; preserve old bit 7.

                WriteReg8(target, result);                           // Write shifted result back to the same target.
                SetFlagsZNHC(
                    z: result == 0,                                  // Set if result is zero.
                    n: false,                                        // Reset.
                    h: false,                                        // Reset.
                    c: carry);                                       // Set from old bit 0.

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }

            //--- SWAP r
            case >= 0x30 and <= 0x37:
            {
                byte old    = ReadReg8(target);                     // Read the current 8-bit operand.
                byte result = (byte)((old >> 4) | (old << 4));      // Exchange upper and lower nibbles.

                WriteReg8(target, result);                          // Write swapped result back to the same target.
                SetFlagsZNHC(
                    z: result == 0,                                 // Set if result is zero.
                    n: false,                                       // Reset.
                    h: false,                                       // Reset.
                    c: false);                                      // Reset.

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }
            
            //--- SRL r
            case >= 0x38 and <= 0x3F:
            {
                byte old    = ReadReg8(target);                     // Read the current 8-bit operand.
                bool carry  = (old & 0x01) != 0;                    // Old bit 0 becomes the new carry.
                byte result = (byte)(old >> 1);                     // Shift right logical; new bit 7 becomes 0.

                WriteReg8(target, result);                          // Write shifted result back to the same target.
                SetFlagsZNHC(
                    z: result == 0,                                 // Set if result is zero.
                    n: false,                                       // Reset.
                    h: false,                                       // Reset.
                    c: carry);                                      // Set from old bit 0.

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }
            
            //---------- BIT ----------//
            //--- BIT b,r
            case >= 0x40 and <= 0x7F:
            {
                int  bit = (opcode >> 3) & 0x07;                    // Bits 5-3 encode which bit to test: 0..7.
                byte val = ReadReg8(target);                        // Read the current 8-bit operand.
                bool set = (val & (1 << bit)) != 0;                 // Test whether the selected bit is set.

                SetFlagsZNHC(
                    z: !set,                                        // Set if the tested bit is 0.
                    n: false,                                       // Reset.
                    h: true,                                        // Set.
                    c: _state.FlagC);                               // Unchanged.

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }

            //---------- RES ----------//
            //--- RES b,r
            case >= 0x80 and <= 0xBF:
            {
                int  bit    = (opcode >> 3) & 0x07;                 // Bits 5-3 encode which bit to clear: 0..7.
                byte old    = ReadReg8(target);                     // Read the current 8-bit operand.
                byte result = (byte)(old & ~(1 << bit));            // Clear the selected bit.

                WriteReg8(target, result);                          // Write modified result back to the same target.
                                                                    // RES does not affect flags on LR35902.

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }

            //---------- SET ----------//
            //--- SET b,r
            case >= 0xC0 and <= 0xFF:
            {
                int  bit    = (opcode >> 3) & 0x07;                 // Bits 5-3 encode which bit to set: 0..7.
                byte old    = ReadReg8(target);                     // Read the current 8-bit operand.
                byte result = (byte)(old | (1 << bit));             // Set the selected bit.

                WriteReg8(target, result);                          // Write modified result back to the same target.
                                                                    // SET does not affect flags on LR35902.

                CompleteCBInstruction(opcode, target, applyPendingInterruptEnableAfterInstruction);
                return;
            }
        }
        
    }
    
    private void CompleteCBInstruction(byte opcode, int targetReg, bool applyPendingInterruptEnable)
    {
        // Timing:
        //  - register CB ops:   8 total cycles.
        //  - BIT b,(HL):       12 total cycles.
        //  - other (HL) forms: 16 total cycles.
        if (targetReg == 6)
        {
            if (opcode is >= 0x40 and <= 0x7F) _state.AddClockCycles(MachineCycle * 2); // total 12
            else                               _state.AddClockCycles(MachineCycle * 3); // total 16
        }
        else
        {
            _state.AddClockCycles(MachineCycle); // total 8
        }

        CompleteInstruction(applyPendingInterruptEnable);
    }
}
