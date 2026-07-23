namespace GameBoy.CPU;

// ReSharper disable InconsistentNaming; Using opcode names for function names, doesn't follow C# convention.
// ReSharper disable InvalidXmlDocComment; local functions used to prevent class from calling them inapropriately.

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                        CB-PREFIXED OPCODES                                       //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes CB-prefixed rotate, shift, and bit manipulation opcodes. </summary>
    
    /// <summary>
    /// Executes the second byte of a CB-prefixed instruction.
    /// CB opcodes use bits 2-0 to select the target register or (HL), and higher bits to select
    /// rotate/shift, bit-test, reset-bit, or set-bit operations. Each case performs the operation,
    /// updates flags according to the LR35902 rules, then delegates shared timing and completion.
    /// </summary>
    private void ExecuteCBOpcode(byte opcode)
    {
        int target = opcode & 0x07; // 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A

        switch (opcode)
        {
            //---------- ROTATE / SHIFT ----------//
            case <= 0x07: RLC_r();  return;                 //--- RLC r
            case <= 0x0F: RRC_r();  return;                 //--- RRC r
            case <= 0x17: RL_r();   return;                 //--- RL r
            case <= 0x1F: RR_r();   return;                 //--- RR r
            case <= 0x27: SLA_r();  return;                 //--- SLA r
            case <= 0x2F: SRA_r();  return;                 //--- SRA r
            case <= 0x37: SWAP_r(); return;                 //--- SWAP r
            case <= 0x3F: SRL_r();  return;                 //--- SRL r
            
            //--------- BIT MANIPULATION --------//
            case <= 0x7F: BIT_b_r(); return;                //--- BIT b,r
            case <= 0xBF: RES_b_r(); return;                //--- RES b,r
            case <= 0xFF: SET_b_r(); return;                //--- SET b,r
        }
        
        /// <summary> Rotate the target left circular, moving old bit 7 into carry and bit 0. </summary>
        void RLC_r()
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

            CompleteCBInstruction(opcode, target);
        }

        /// <summary> Rotate the target right circular, moving old bit 0 into carry and bit 7. </summary>
        void RRC_r()
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

            CompleteCBInstruction(opcode, target);
        }

        /// <summary> Rotate the target left through the carry flag. </summary>
        void RL_r()
        {
            byte old      = ReadReg8(target);                       // Read the current 8-bit operand.
            bool oldCarry = _state.FlagC;                           // Old carry is shifted into bit 0.
            bool carryOut = (old & 0x80) != 0;                      // Old bit 7 becomes the new carry.
            byte result   = (byte)((old << 1) | (oldCarry ? 1 : 0));// Rotate left through carry.

            WriteReg8(target, result);                              // Write rotated result back to the same target.
            SetFlagsZNHC(
                z: result == 0,                                     // Set if result is zero.
                n: false,                                           // Reset.
                h: false,                                           // Reset.
                c: carryOut);                                       // Set from old bit 7.

            CompleteCBInstruction(opcode, target);
        }

        /// <summary> Rotate the target right through the carry flag. </summary>
        void RR_r()
        {
            byte old      = ReadReg8(target);                       // Read the current 8-bit operand.
            bool oldCarry = _state.FlagC;                           // Old carry is shifted into bit 7.
            bool carryOut = (old & 0x01) != 0;                      // Old bit 0 becomes the new carry.
            byte result   = (byte)((old >> 1) | (oldCarry ? 0x80 : 0)); // Rotate right through carry.

            WriteReg8(target, result);                              // Write rotated result back to the same target.
            SetFlagsZNHC(
                z: result == 0,                                     // Set if result is zero.
                n: false,                                           // Reset.
                h: false,                                           // Reset.
                c: carryOut);                                       // Set from old bit 0.

            CompleteCBInstruction(opcode, target);
        }
        
        /// <summary> Shift the target left and move old bit 7 into carry. </summary>
        void SLA_r()
        {
            byte old    = ReadReg8(target);                         // Read the current 8-bit operand.
            bool carry  = (old & 0x80) != 0;                        // Old bit 7 becomes the new carry.
            byte result = (byte)(old << 1);                         // Shift left arithmetic; bit 0 becomes 0.

            WriteReg8(target, result);                              // Write shifted result back to the same target.
            SetFlagsZNHC(
                z: result == 0,                                     // Set if result is zero.
                n: false,                                           // Reset.
                h: false,                                           // Reset.
                c: carry);                                          // Set from old bit 7.

            CompleteCBInstruction(opcode, target);
        }

        /// <summary> Shift the target right, preserving old bit 7 and moving old bit 0 into carry. </summary>
        void SRA_r()
        {
            byte old    = ReadReg8(target);                         // Read the current 8-bit operand.
            bool carry  = (old & 0x01) != 0;                        // Old bit 0 becomes the new carry.
            byte result = (byte)((old >> 1) | (old & 0x80));        // Shift right arithmetic; preserve old bit 7.

            WriteReg8(target, result);                              // Write shifted result back to the same target.
            SetFlagsZNHC(
                z: result == 0,                                     // Set if result is zero.
                n: false,                                           // Reset.
                h: false,                                           // Reset.
                c: carry);                                          // Set from old bit 0.

            CompleteCBInstruction(opcode, target);
        }

        /// <summary> Swap the upper and lower nibbles of the target. </summary>
        void SWAP_r()
        {
            byte old    = ReadReg8(target);                         // Read the current 8-bit operand.
            byte result = (byte)((old >> 4) | (old << 4));          // Exchange upper and lower nibbles.

            WriteReg8(target, result);                              // Write swapped result back to the same target.
            SetFlagsZNHC(
                z: result == 0,                                     // Set if result is zero.
                n: false,                                           // Reset.
                h: false,                                           // Reset.
                c: false);                                          // Reset.

            CompleteCBInstruction(opcode, target);
        }

        /// <summary> Shift the target right and move old bit 0 into carry. </summary>
        void SRL_r()
        {
            byte old    = ReadReg8(target);                         // Read the current 8-bit operand.
            bool carry  = (old & 0x01) != 0;                        // Old bit 0 becomes the new carry.
            byte result = (byte)(old >> 1);                         // Shift right logical; new bit 7 becomes 0.

            WriteReg8(target, result);                              // Write shifted result back to the same target.
            SetFlagsZNHC(
                z: result == 0,                                     // Set if result is zero.
                n: false,                                           // Reset.
                h: false,                                           // Reset.
                c: carry);                                          // Set from old bit 0.

            CompleteCBInstruction(opcode, target);
        }

        /// <summary> Test one bit in the target and update flags without changing the target. </summary>
        void BIT_b_r()
        {
            int  bit = (opcode >> 3) & 0x07;                        // Bits 5-3 encode which bit to test: 0..7.
            byte val = ReadReg8(target);                            // Read the current 8-bit operand.
            bool set = (val & (1 << bit)) != 0;                     // Test whether the selected bit is set.

            SetFlagsZNHC(
                z: !set,                                            // Set if the tested bit is 0.
                n: false,                                           // Reset.
                h: true,                                            // Set.
                c: _state.FlagC);                                   // Unchanged.

            CompleteCBInstruction(opcode, target);
        }

        /// <summary> Clear one bit in the target. </summary>
        void RES_b_r()
        {
            int  bit    = (opcode >> 3) & 0x07;                     // Bits 5-3 encode which bit to clear: 0..7.
            byte old    = ReadReg8(target);                         // Read the current 8-bit operand.
            byte result = (byte)(old & ~(1 << bit));                // Clear the selected bit.

            WriteReg8(target, result);                              // Write modified result back to the same target.
            // RES does not affect flags on LR35902.

            CompleteCBInstruction(opcode, target);
        }

        /// <summary> Set one bit in the target. </summary>
        void SET_b_r()
        {
            int  bit    = (opcode >> 3) & 0x07;                     // Bits 5-3 encode which bit to set: 0..7.
            byte old    = ReadReg8(target);                         // Read the current 8-bit operand.
            byte result = (byte)(old | (1 << bit));                 // Set the selected bit.

            WriteReg8(target, result);                              // Write modified result back to the same target.
            // SET does not affect flags on LR35902.

            CompleteCBInstruction(opcode, target);
        }
        
    }
    
    private void CompleteCBInstruction(byte opcode, int targetReg)
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

        CompleteInstruction();
    }
}
