namespace GameBoy.CPU;

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                           ALU8 OPCODES                                           //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes opcodes used for the 8-bit Arithmetic Logic Unit. </summary>

    /// <summary> INC r,(HL) </summary>
    public void ExecuteIncRHl(ushort opcode)
    {
        // Bits 5-3 of this opcode select the target register index:
        // 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int target = (opcode >> 3) & 0x07;

        byte old = ReadReg8(target); // Read current 8-bit value from the selected source.
        byte result = (byte)(old + 1); // Perform 8-bit increment with wraparound (0xFF -> 0x00).

        WriteReg8(target, result); // Write back to the same register

        bool carry = _state.FlagC; // INC affects Z, N, H and leaves C unchanged. Preserve carry.
        SetFlagsZNHC(
            z: result == 0, // Z: Set if result is 0.
            n: false, // N: Reset for increment.
            h: (old & 0x0F) == 0x0F, // H: Set when low nibble overflowed
            c: carry); // C: Unchanged

        // Timing:  (4/12 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - INC r,r8:     0 cycles
        //  - INC r,HL:     8 cycles.
        if (target == 6) _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }

    /// <summary> DEC r,(HL) </summary>
    public void ExecuteDecRHl(ushort opcode)
    {
        // Bits 5-3 of this opcode select the target register index:
        // 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int target = (opcode >> 3) & 0x07;

        byte old = ReadReg8(target); // Read current 8-bit value from the selected source.
        byte result = (byte)(old - 1); // Perform 8-bit increment with wraparound (0x00 -> 0xFF).

        WriteReg8(target, result); // Write back to the same register

        bool carry = _state.FlagC; // INC affects Z, N, H and leaves C unchanged. Preserve carry.
        SetFlagsZNHC(
            z: result == 0, // Z: Set if result is 0.
            n: true, // N: Set for decrement.
            h: (old & 0x0F) == 0X00, // H: Set when low nibble overflowed
            c: carry); // C: Unchanged

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (target == 6) _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }

    /// <summary> ADD A,r </summary>
    public void ExecuteAddAr(ushort opcode)
    {
        byte a = _state.A;

        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src); // Read source operand (reg or memory at HL)
        int sum = a + val; // Perform addition with 8-bit wraparound.

        _state.A = (byte)sum; // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0, // set if carry is 0.
            n: false, // reset.
            h: ((a & 0x0F) + (val & 0x0F)) > 0x0F, // set if carry from bit 3 to bit 4.
            c: sum > 0xFF); // set if carry out of bit 7.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> ADC A,r </summary>
    public void ExecuteAdcAr(ushort opcode)
    {
        byte a = _state.A;

        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src); // Read source operand (reg or memory at HL)
        int carryIn = _state.FlagC ? 1 : 0; // Carry-in is the current C flag.
        int sum = a + val + carryIn; // Perform the addition.

        _state.A = (byte)sum; // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0, // set if carry is 0.
            n: false, // reset.
            h: ((a & 0x0F) + (val & 0x0F) + carryIn) > 0x0F, // set if carry from bit 3.
            c: sum > 0xFF); // set if carry from bit 7.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> ADD/ADC A,d8 </summary>
    public void ExecuteAddAdcAd8(ushort opcode)
    {
        byte a = _state.A;

        byte val = ReadNextByte(); // Read immediate 8-bit operand from instruction stream.
        int carryIn =
            (opcode == 0xCE && _state.FlagC) ? 1 : 0; // ADC includes carry-in from the current C flag; ADD does not.
        int sum = a + val + carryIn; // Perform addition with 8-bit wraparound.

        _state.A = (byte)sum; // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: false, // reset.
            h: ((a & 0x0F) + (val & 0x0F) + carryIn) > 0x0F, // set on half-carry.
            c: sum > 0xFF); // set on full carry.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - ADD/ADC A,d8: 4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> SUB A,r </summary>
    public void ExecuteSubAr(ushort opcode)
    {
        byte a = _state.A;

        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src); // Read source operand (reg or memory at HL)
        int diff = a - val; // Perform subtraction with 8-bit wraparound.

        _state.A = (byte)diff; // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: true, // set.
            h: (a & 0x0F) < (val & 0x0F), // set on half-borrow (borrow from bit 4)
            c: a < val); // set on full borrow (A < val)

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> SBC A,r </summary>
    public void ExecuteSbcAr(ushort opcode)
    {
        byte a = _state.A;

        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src); // Read source operand (reg or memory at HL)
        int carryIn = _state.FlagC ? 1 : 0; // Carry-in is the current C flag.
        int diff = a - val - carryIn; // Perform the subtraction.

        _state.A = (byte)diff; // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: true, // set.
            h: (a & 0x0F) < ((val & 0x0F) + carryIn), // set on half-borrow (bit 4 borrow).
            c: a < (val + carryIn)); // set on full borrow.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> SUB/SBC A,d8 </summary>
    public void ExecuteSubSbcAd8(ushort opcode)
    {
        byte a = _state.A;

        byte val = ReadNextByte(); // Read immediate 8-bit operand from instruction stream.
        int carryIn =
            (opcode == 0xDE && _state.FlagC) ? 1 : 0; // ADC includes carry-in from the current C flag; ADD does not.
        int diff = a - val - carryIn; // Perform addition with 8-bit wraparound.

        _state.A = (byte)diff; // Store the result back in A.
        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: true, // reset.
            h: (a & 0x0F) < ((val & 0x0F) + carryIn), // set on half-carry.
            c: a < (val + carryIn)); // set on full carry.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - SUB/SBC A,d8: 4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> AND A,r </summary>
    public void ExecuteAndAr(ushort opcode)
    {
        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src); // Read source operand (register or memory at HL).
        _state.A = (byte)(_state.A & val); // Perform bitwise AND into A.

        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: false, // reset.
            h: true, // set.
            c: false // reset.
        );

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> AND A,d8 </summary>
    public void ExecuteAndAd8(ushort opcode)
    {
        // Perform a bitwise AND operation on reg A and the next byte. 
        _state.A = (byte)(_state.A & ReadNextByte());
        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: false, // reset.
            h: true, // set.
            c: false // reset.
        );

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - AND A,d8:     4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> XOR A,r </summary>
    public void ExecuteXorAr(ushort opcode)
    {
        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src); // Read source operand (register or memory at HL).
        _state.A = (byte)(_state.A ^ val); // Perform bitwise XOR into A.

        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: false, // reset.
            h: false, // reset.
            c: false); // reset.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> XOR A,d8 </summary>
    public void ExecuteXorAd8(ushort opcode)
    {
        // Perform a bitwise XOR operation on reg A and the next byte. 
        _state.A = (byte)(_state.A ^ ReadNextByte());
        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: false, // reset.
            h: false, // reset.
            c: false); // reset.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - XOR A,d8:     4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> OR A,r </summary>
    public void ExecuteOrAr(ushort opcode)
    {
        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;

        byte val = ReadReg8(src); // Read source operand (register or memory at HL).
        _state.A = (byte)(_state.A | val); // Perform bitwise OR into A.

        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: false, // reset.
            h: false, // reset.
            c: false // reset.
        );

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> OR A,d8 </summary>
    public void ExecuteOrAd8(ushort opcode)
    {
        // Perform a bitwise OR operation on reg A and the next byte. 
        _state.A = (byte)(_state.A | ReadNextByte());
        SetFlagsZNHC(
            z: _state.A == 0, // set if result is 0.
            n: false, // reset.
            h: false, // reset.
            c: false); // reset.

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - OR A,d8:      4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> CP A,r </summary>
    public void ExecuteCpAr(ushort opcode)
    {
        byte a = _state.A;
                
        // Bits 2-0 encode source register: 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int src = opcode & 0x07;
                
        byte val = ReadReg8(src);           // Read source operand (register or memory at HL).
        byte res = (byte)(a - val);         // Compare is a subtraction for flags only (A unchanged).
                
        SetFlagsZNHC(
            z: res == 0,                    // set if A == val (result zero)
            n: true,                        // set.
            h: (a & 0x0F) < (val & 0x0F),   // set on half-borrow.
            c: a < val);                    // set on full borrow.
                
        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (src == 6) _state.AddClockCycles(MachineCycle);
        CompleteInstruction();
    }

    /// <summary> CP A,d8 </summary>
    public void ExecuteCpAd8(ushort opcode)
    {
        byte a   = _state.A;        // Value in register A.
        byte val = ReadNextByte();  // Read next byte in stream.
                
        // Compare is a subtraction for flags only (A unchanged).
        byte res = (byte)(a - val);
        SetFlagsZNHC(
            z: res == 0,                    // set if A == d8
            n: true,                        // set.
            h: (a & 0x0F) < (val & 0x0F),   // set on half-borrow.
            c: a < val);                    // set on full borrow.
                
        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - CP A,d8:      4 cycles.
        _state.AddClockCycles(MachineCycle);
        CompleteInstruction(); 
    }
    
    /// <summary> DAA (Decimal Adjust After) </summary>
    public void ExecuteDaa(ushort opcode)
    {
        // DAA adjusts A to a valid BCD result after ADD/ADC or SUB/SBC,
        // using the current N/H/C flags to determine which corrections to apply.
        byte a = _state.A; byte correction = 0;
        bool n = _state.FlagN; bool h = _state.FlagH; bool c = _state.FlagC;

        if (!n) // Addition correction
        {
            if (h || (a & 0x0F) > 0x09) // If half-carry set or low nibble > 9
                correction |= 0x06;     // add 0x06.
                    
            if (c || a > 0x99)          //  If carry set or A > 0x99
            {
                correction |= 0x60;     // add 0x60 and set carry.
                c = true;
            }

            a = (byte)(a + correction);
        }
        else    // Subtraction correction
        {
            if (h) correction |= 0x06;  // If half-carry set, subtract 0x06.
            if (c) correction |= 0x60;  // If carry set, subtract 0x60.
                    
            a = (byte)(a - correction);
        }

        _state.A = a;
        SetFlagsZNHC(
            z: _state.A == 0,   // set if adjusted A is 0.
            n: n,               // unchanged.
            h: false,           // reset.
            c: c);              // unchanged.
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> CPL (Complement Accumulator) </summary>
    public void ExecuteCpl(ushort opcode)
    {
        _state.A = (byte)~_state.A;  // Invert all bits in A (1's complement).
                
        SetFlagsZNHC(
            z: _state.FlagZ,    // unchanged.
            n: true,            // set.
            h: true,            // set.
            c: _state.FlagC);   // unchanged.
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> SCF (Set Carry Flag) </summary>
    public void ExecuteScf(ushort opcode)
    {
        SetFlagsZNHC(
            z: _state.FlagZ,    // unchanged.
            n: false,           // reset.
            h: false,           // reset.
            c: true);           // set.
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> CCF (Compliment Carry Flag) </summary>
    public void ExecuteCcf(ushort opcode)
    {
        SetFlagsZNHC(
            z: _state.FlagZ,    // unchanged.
            n: false,           // reset.
            h: false,           // reset.
            c: !_state.FlagC);  // complement carry.
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }
}