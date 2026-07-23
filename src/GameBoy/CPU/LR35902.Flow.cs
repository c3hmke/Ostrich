namespace GameBoy.CPU;

// ReSharper disable InconsistentNaming; Using opcode names for function names, doesn't follow C# convention.

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                           FLOW OPCODES                                           //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes opcodes that change control flow through jumps, calls, returns, and resets. </summary>

    /// <summary> Jump relative by a signed 8-bit offset. </summary>
    private void JR_r8(ushort opcode)
    {
        sbyte offset = unchecked((sbyte)ReadNextByte());
        _state.PC    = (ushort)(_state.PC + offset);
                
        // Timing:  (8 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - JR r8:        4 cycles.
        _state.AddClockCycles(MachineCycle * 2);
        CompleteInstruction();
    }

    /// <summary> Jump relative by a signed 8-bit offset when the encoded condition is met. </summary>
    private void JR_N_e8(ushort opcode)
    {
        sbyte offset       = unchecked((sbyte)ReadNextByte());
        ConditionCode cond = (ConditionCode)((opcode >> 3) & 0x03);

        if (CheckCond(cond))
        {
            _state.PC = (ushort)(_state.PC + offset);
                    
            // Timing:  (12 total cycles)
            //  - opcode fetch: 4 cycles.
            //  - JR N',e8:     8 cycles.
            _state.AddClockCycles(MachineCycle * 2);
        }
        else
        {
            // Timing:  (12 total cycles)
            //  - opcode fetch:     4 cycles.
            //  - JR N',e8 no pop:  4 cycles.
            _state.AddClockCycles(MachineCycle);
        }
                
        CompleteInstruction();
    }

    /// <summary> Jump to an immediate 16-bit address. </summary>
    private void JP_a16()
    {
        _state.PC = ReadNextWord(); // Jump to the next address in bitstream.
                
        // Timing:  (16 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - JP a16:       12 cycles.
        _state.AddClockCycles(MachineCycle * 3);
        CompleteInstruction();
    }

    /// <summary> Jump to an immediate 16-bit address when the encoded condition is met. </summary>
    private void JP_cc_a16(ushort opcode)
    {
        ushort target = ReadNextWord();
        var    cc     = (ConditionCode)((opcode >> 3) & 0x03);

        if (CheckCond(cc))
        {
            _state.PC = target;

            // Timing:  (16 total cycles)
            //  - opcode fetch: 4 cycles.
            //  - JP cc,a16:   12 cycles.
            _state.AddClockCycles(MachineCycle * 3);
        }
        else
        {
            // Timing:  (12 total cycles)
            //  - opcode fetch:    4 cycles.
            //  - JP cc,a16 miss:  8 cycles.
            _state.AddClockCycles(MachineCycle * 2);
        }

        CompleteInstruction();
    }

    /// <summary> Jump to the address stored in HL. </summary>
    private void JP_HL()
    {
        _state.PC = _state.HL;  // Jump to address currently in HL
                
        // Timing:  (4 total cycles)
        //  - opcode fetch:    4 cycles.
        CompleteInstruction();
    }

    /// <summary> Return from a subroutine when the encoded condition is met. </summary>
    private void RET_cc(ushort opcode)
    {
        var cond = (ConditionCode)((opcode >> 3) & 0x03);

        if (CheckCond(cond))
        {
            // Condition met: pop return address into PC.
            _state.PC = PopWord();

            // Timing:  (20 total cycles)
            //  - opcode fetch: 4 cycles.
            //  - RET cc:       16 cycles
            _state.AddClockCycles(MachineCycle * 4);
        }
        else
        {
            // Timing:  (8 total cycles)
            //  - opcode fetch:  4 cycles.
            //  - RET cc no pop: 4 cycles
            _state.AddClockCycles(MachineCycle);
        }

        CompleteInstruction();
    }
    
    /// <summary> Return from a subroutine. </summary>
    private void RET()
    {
        _state.PC = PopWord();
                
        // Timing:  (16 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - RET:          12 cycles
        _state.AddClockCycles(MachineCycle * 3); // total 16
        CompleteInstruction();
    }

    /// <summary> Return from an interrupt handler and re-enable interrupts. </summary>
    private void RETI()
    {
        _state.PC = PopWord();      // Return from interrupt: pop return address into PC.
        _state.EnableInterrupts();  // RETI re-enables IME immediately.

        // Timing:  (16 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - RETI:         12 cycles.
        _state.AddClockCycles(MachineCycle * 3);
                
        CompleteInstruction();
    }

    /// <summary> Call the fixed reset vector encoded by the opcode. </summary>
    private void RST_vec(ushort opcode)
    {
        ushort vec = (ushort)(opcode & 0x38);   // Compute vector from opcode.
                
        PushWord(_state.PC);                    // Store the return address.
        _state.PC = vec;                        // Then jump to vector.
                
        // Timing:  (16 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - RST vec:      12 cycles.
        _state.AddClockCycles(MachineCycle * 3);
        CompleteInstruction();
    }

    /// <summary> Call an immediate 16-bit address when the encoded condition is met. </summary>
    private void CALL_cc_a16(ushort opcode)
    {
        ushort target = ReadNextWord();
        var    cc     = (ConditionCode)((opcode >> 3) & 0x03);

        if (CheckCond(cc))
        {
            PushWord(_state.PC);    // Push return address.
            _state.PC = target;     // Jump to target.

            // Timing: 24 total cycles.
            // - opcode fetch: 4 cycles.
            // - CALL cc,a16:  20 cycles.
            _state.AddClockCycles(MachineCycle * 5);
        }
        else
        {
            // Condition not met: no push, no jump.
            // Timing: 12 total cycles.
            // - opcode fetch:       4 cycles.
            // - CALL cc,a16 miss:   8 cycles.
            _state.AddClockCycles(MachineCycle * 2);
        }

        CompleteInstruction();
    }

    /// <summary> Call an immediate 16-bit address. </summary>
    private void CALL_a16()
    {
        ushort target = ReadNextWord();
                
        PushWord(_state.PC);    // Push return address.
        _state.PC = target;     // Jump to target.
                
        // Timing: 24 total cycles.
        // - opcode fetch: 4 cycles.
        // - CALL a16:     20 cycles.
        _state.AddClockCycles(MachineCycle * 5);
        CompleteInstruction();
    }
    
    //--------------------------------------------------------------------------------------------------//
    //                                            HELPERS                                               //
    //--------------------------------------------------------------------------------------------------//
    
    /// <summary> Evaluate NZ/Z/NC/C condition code used by JR/JP/CALL/RET conditional forms. </summary>
    private bool CheckCond(ConditionCode cond)
    {
        return cond switch
        {
            ConditionCode.NZ => !_state.FlagZ,
            ConditionCode.Z  =>  _state.FlagZ,
            ConditionCode.NC => !_state.FlagC,
            ConditionCode.C  =>  _state.FlagC,
            _ => throw new ArgumentOutOfRangeException(nameof(cond), cond, null)
        };
    }
}
