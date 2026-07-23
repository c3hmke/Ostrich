namespace GameBoy.CPU;

// ReSharper disable InconsistentNaming; Using opcode names for function names, doesn't follow C# convention.

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                           ALU8 OPCODES                                           //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes opcodes that perform 8-bit arithmetic, logic, compare, and flag operations. </summary>

    /// <summary> Increment one 8-bit register or the byte at HL. </summary>
    private void INC_r_HL(ushort opcode)
    {
        // Bits 5-3 of this opcode select the target register index:
        // 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int target = (opcode >> 3) & 0x07;

        byte old = ReadReg8(target);                // Read current 8-bit value from the selected source.
        byte result = (byte)(old + 1);              // Perform 8-bit increment with wraparound (0xFF -> 0x00).

        WriteReg8(target, result);                  // Write back to the same register

        bool carry = _state.FlagC;                  // INC affects Z, N, H and leaves C unchanged. Preserve carry.
        SetFlagsZNHC(
            z: result == 0,                         // Z: Set if result is 0.
            n: false,                               // N: Reset for increment.
            h: (old & 0x0F) == 0x0F,                // H: Set when low nibble overflowed
            c: carry);                              // C: Unchanged

        // Timing:  (4/12 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - INC r,r8:     0 cycles
        //  - INC r,HL:     8 cycles.
        if (target == 6) _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }

    /// <summary> Decrement one 8-bit register or the byte at HL. </summary>
    private void DEC_r_HL(ushort opcode)
    {
        // Bits 5-3 of this opcode select the target register index:
        // 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int target = (opcode >> 3) & 0x07;

        byte old = ReadReg8(target);                    // Read current 8-bit value from the selected source.
        byte result = (byte)(old - 1);                  // Perform 8-bit increment with wraparound (0x00 -> 0xFF).

        WriteReg8(target, result);                      // Write back to the same register

        bool carry = _state.FlagC;                      // INC affects Z, N, H. Leaves C unchanged, preserve carry.
        SetFlagsZNHC(
            z: result == 0,                             // Z: Set if result is 0.
            n: true,                                    // N: Set for decrement.
            h: (old & 0x0F) == 0X00,                    // H: Set when low nibble overflowed
            c: carry);                                  // C: Unchanged

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (target == 6) _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }

    /// <summary> Add one 8-bit register or the byte at HL to A. </summary>
    private void ADD_A_r(ushort opcode)
    {
        byte a = _state.A;

        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src);                       // Read source operand (reg or memory at HL)
        int sum = a + val;                              // Perform addition with 8-bit wraparound.
        
        _state.A = (byte)sum;                           // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0,                           // set if carry is 0.
            n: false,                                   // reset.
            h: ((a & 0x0F) + (val & 0x0F)) > 0x0F,      // set if carry from bit 3 to bit 4.
            c: sum > 0xFF);                             // set if carry out of bit 7.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }
    
    /// <summary> Add one 8-bit register or the byte at HL plus carry to A. </summary>
    private void ADC_A_r(ushort opcode)
    {
        byte a = _state.A;

        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src);                       // Read source operand (reg or memory at HL)
        int carryIn = _state.FlagC ? 1 : 0;             // Carry-in is the current C flag.
        int sum = a + val + carryIn;                    // Perform the addition.

        _state.A = (byte)sum;                           // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0,                                // set if carry is 0.
            n: false,                                        // reset.
            h: ((a & 0x0F) + (val & 0x0F) + carryIn) > 0x0F, // set if carry from bit 3.
            c: sum > 0xFF);                                  // set if carry from bit 7.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Add an immediate 8-bit value to A, optionally including carry. </summary>
    private void ADD_ADC_A_d8(ushort opcode)
    {
        byte a = _state.A;

        byte val = ReadNextByte();                      // Read immediate 8-bit operand from instruction stream.
        int carryIn =
            (opcode == 0xCE && _state.FlagC) ? 1 : 0;   // ADC includes carry-in from current C flag; ADD doesn't.
        int sum = a + val + carryIn;                    // Perform addition with 8-bit wraparound.

        _state.A = (byte)sum;                           // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0,                                // set if result is 0.
            n: false,                                        // reset.
            h: ((a & 0x0F) + (val & 0x0F) + carryIn) > 0x0F, // set on half-carry.
            c: sum > 0xFF);                                  // set on full carry.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - ADD/ADC A,d8: 4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Subtract one 8-bit register or the byte at HL from A. </summary>
    private void SUB_A_r(ushort opcode)
    {
        byte a = _state.A;

        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src);                       // Read source operand (reg or memory at HL)
        int diff = a - val;                             // Perform subtraction with 8-bit wraparound.

        _state.A = (byte)diff;                          // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0,                           // set if result is 0.
            n: true,                                    // set.
            h: (a & 0x0F) < (val & 0x0F),               // set on half-borrow (borrow from bit 4)
            c: a < val);                                // set on full borrow (A < val)

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Subtract one 8-bit register or the byte at HL plus carry from A. </summary>
    private void SBC_A_r(ushort opcode)
    {
        byte a = _state.A;

        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src);                       // Read source operand (reg or memory at HL)
        int carryIn = _state.FlagC ? 1 : 0;             // Carry-in is the current C flag.
        int diff = a - val - carryIn;                   // Perform the subtraction.

        _state.A = (byte)diff;                          // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0,                           // set if result is 0.
            n: true,                                    // set.
            h: (a & 0x0F) < ((val & 0x0F) + carryIn),   // set on half-borrow (bit 4 borrow).
            c: a < (val + carryIn));                    // set on full borrow.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Subtract an immediate 8-bit value from A, optionally including carry. </summary>
    private void SUB_SBC_A_d8(ushort opcode)
    {
        byte a = _state.A;

        byte val = ReadNextByte();                      // Read immediate 8-bit operand from instruction stream.
        int carryIn =
            (opcode == 0xDE && _state.FlagC) ? 1 : 0;   // ADC includes carry-in from the current C flag; ADD does not.
        int diff = a - val - carryIn;                   // Perform addition with 8-bit wraparound.

        _state.A = (byte)diff;                          // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0,                           // set if result is 0.
            n: true,                                    // reset.
            h: (a & 0x0F) < ((val & 0x0F) + carryIn),   // set on half-carry.
            c: a < (val + carryIn));                    // set on full carry.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - SUB/SBC A,d8: 4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Bitwise AND one 8-bit register or the byte at HL into A. </summary>
    private void AND_A_r(ushort opcode)
    {
        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src);                       // Read source operand (register or memory at HL).
        _state.A = (byte)(_state.A & val);              // Perform bitwise AND into A.

        SetFlagsZNHC(
            z: _state.A == 0,                           // set if result is 0.
            n: false,                                   // reset.
            h: true,                                    // set.
            c: false                                    // reset.
        );

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Bitwise AND an immediate 8-bit value into A. </summary>
    private void AND_A_d8()
    {
        // Perform a bitwise AND operation on reg A and the next byte. 
        _state.A = (byte)(_state.A & ReadNextByte());
        SetFlagsZNHC(
            z: _state.A == 0,                           // set if result is 0.
            n: false,                                   // reset.
            h: true,                                    // set.
            c: false                                    // reset.
        );

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - AND A,d8:     4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Bitwise XOR one 8-bit register or the byte at HL into A. </summary>
    private void XOR_A_r(ushort opcode)
    {
        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src);                       // Read source operand (register or memory at HL).
        _state.A = (byte)(_state.A ^ val);              // Perform bitwise XOR into A.

        SetFlagsZNHC(
            z: _state.A == 0,                           // set if result is 0.
            n: false,                                   // reset.
            h: false,                                   // reset.
            c: false);                                  // reset.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Bitwise XOR an immediate 8-bit value into A. </summary>
    private void XOR_A_d8()
    {
        // Perform a bitwise XOR operation on reg A and the next byte. 
        _state.A = (byte)(_state.A ^ ReadNextByte());
        SetFlagsZNHC(
            z: _state.A == 0,                           // set if result is 0.
            n: false,                                   // reset.
            h: false,                                   // reset.
            c: false);                                  // reset.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - XOR A,d8:     4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Bitwise OR one 8-bit register or the byte at HL into A. </summary>
    private void OR_A_r(ushort opcode)
    {
        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src);                       // Read source operand (register or memory at HL).
        _state.A = (byte)(_state.A | val);              // Perform bitwise OR into A.

        SetFlagsZNHC(
            z: _state.A == 0,                           // set if result is 0.
            n: false,                                   // reset.
            h: false,                                   // reset.
            c: false                                    // reset.
        );

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Bitwise OR an immediate 8-bit value into A. </summary>
    private void OR_A_d8()
    {
        // Perform a bitwise OR operation on reg A and the next byte. 
        _state.A = (byte)(_state.A | ReadNextByte());
        SetFlagsZNHC(
            z: _state.A == 0,                           // set if result is 0.
            n: false,                                   // reset.
            h: false,                                   // reset.
            c: false);                                  // reset.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - OR A,d8:      4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Compare A with one 8-bit register or the byte at HL. </summary>
    private void CP_A_r(ushort opcode)
    {
        byte a = _state.A;
                
        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;
                
        byte val = ReadReg8(src);                       // Read source operand (register or memory at HL).
        byte res = (byte)(a - val);                     // Compare is a subtraction for flags only (A unchanged).
                
        SetFlagsZNHC(
            z: res == 0,                                // set if A == val (result zero)
            n: true,                                    // set.
            h: (a & 0x0F) < (val & 0x0F),               // set on half-borrow.
            c: a < val);                                // set on full borrow.
                
        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> Compare A with an immediate 8-bit value. </summary>
    private void CP_A_d8()
    {
        byte a   = _state.A;                            // Value in register A.
        byte val = ReadNextByte();                      // Read next byte in stream.
                
        // Compare is a subtraction for flags only (A unchanged).
        byte res = (byte)(a - val);
        SetFlagsZNHC(
            z: res == 0,                                // set if A == d8
            n: true,                                    // set.
            h: (a & 0x0F) < (val & 0x0F),               // set on half-borrow.
            c: a < val);                                // set on full borrow.
                
        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - CP A,d8:      4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction(); 
    }
    
    /// <summary> Decimal Adjust After, Adjust A to a valid BCD value after an arithmetic operation. </summary>
    private void DAA()
    {
        // DAA (Decimal Adjust After) adjusts A to a valid BCD result after ADD/ADC or ,
        // SUB/SBC using the current N/H/C flags to determine which corrections to apply.
        byte a = _state.A; byte correction = 0;
        bool n = _state.FlagN; bool h = _state.FlagH; bool c = _state.FlagC;

        if (!n)                                         // Addition correction: --
        {
            if (h || (a & 0x0F) > 0x09)                 // If half-carry set or low nibble > 9
                correction |= 0x06;                     // add 0x06.
                    
            if (c || a > 0x99)                          //  If carry set or A > 0x99
            {
                correction |= 0x60;                     // add 0x60 and set carry.
                c = true;
            }

            a = (byte)(a + correction);
        }
        else                                            // Subtraction correction: --
        {
            if (h) correction |= 0x06;                  // If half-carry set, subtract 0x06.
            if (c) correction |= 0x60;                  // If carry set, subtract 0x60.
                    
            a = (byte)(a - correction);
        }

        _state.A = a;
        SetFlagsZNHC(
            z: _state.A == 0,                           // set if adjusted A is 0.
            n: n,                                       // unchanged.
            h: false,                                   // reset.
            c: c);                                      // unchanged.
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> Complement all bits in A. </summary>
    private void CPL()
    {
        _state.A = (byte)~_state.A;                     // Invert all bits in A (1's complement).
                
        SetFlagsZNHC(
            z: _state.FlagZ,                            // unchanged.
            n: true,                                    // set.
            h: true,                                    // set.
            c: _state.FlagC);                           // unchanged.
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> Set the carry flag. </summary>
    private void SCF()
    {
        SetFlagsZNHC(
            z: _state.FlagZ,                            // unchanged.
            n: false,                                   // reset.
            h: false,                                   // reset.
            c: true);                                   // set.
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> Complement the carry flag. </summary>
    private void CCF()
    {
        SetFlagsZNHC(
            z: _state.FlagZ,                            // unchanged.
            n: false,                                   // reset.
            h: false,                                   // reset.
            c: !_state.FlagC);                          // complement carry.
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }
}
