namespace GameBoy.CPU;

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                           ALU8 OPCODES                                           //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes opcodes used for the 8-bit Arithmetic Logic Unit. </summary>

    public void ExecuteIncRHl(ushort opcode)
    {
        // Bits 5-3 of this opcode select the target register index:
        // 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int target = (opcode >> 3) & 0x07;
                
        byte old    = ReadReg8(target); // Read current 8-bit value from the selected source.
        byte result = (byte)(old + 1);  // Perform 8-bit increment with wraparound (0xFF -> 0x00).
                
        WriteReg8(target, result);      // Write back to the same register
                
        bool carry  = _state.FlagC;     // INC affects Z, N, H and leaves C unchanged. Preserve carry.
        SetFlagsZNHC(
            z: result == 0,             // Z: Set if result is 0.
            n: false,                   // N: Reset for increment.
            h: (old & 0x0F) == 0x0F,    // H: Set when low nibble overflowed
            c: carry);                  // C: Unchanged
                
        // Timing:  (4/12 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - INC r,r8:     0 cycles
        //  - INC r,HL:     8 cycles.
        if (target == 6) _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }

    public void ExecuteDecRHl(ushort opcode)
    {
        // Bits 5-3 of this opcode select the target register index:
        // 0=B, 1=C, 2=D, 3=E, 4=H, 5=L, 6=(HL), 7=A.
        int target = (opcode >> 3) & 0x07;
                
        byte old    = ReadReg8(target); // Read current 8-bit value from the selected source.
        byte result = (byte)(old - 1);  // Perform 8-bit increment with wraparound (0x00 -> 0xFF).
                
        WriteReg8(target, result);      // Write back to the same register
                
        bool carry  = _state.FlagC;     // INC affects Z, N, H and leaves C unchanged. Preserve carry.
        SetFlagsZNHC(
            z: result == 0,             // Z: Set if result is 0.
            n: true,                    // N: Set for decrement.
            h: (old & 0x0F) == 0X00,    // H: Set when low nibble overflowed
            c: carry);                  // C: Unchanged

        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HL form only: 8 cycles.
        if (target == 6) _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }
}