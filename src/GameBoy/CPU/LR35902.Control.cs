namespace GameBoy.CPU;

// ReSharper disable InconsistentNaming; Using opcode names for function names, doesn't follow C# convention.

public sealed partial class LR35902
{
    //--------------------------------------------------------------------------------------------------//
    //                                          CONTROL OPCODES                                         //
    //--------------------------------------------------------------------------------------------------//
    /// <summary> Executes CPU control opcodes for idle states and interrupt enable state. </summary>

    /// <summary> Execute no operation. </summary>
    private void NOP()
    {
        CompleteInstruction();
    }

    /// <summary> Enter the stopped CPU state. </summary>
    private void STOP()
    {
        _state.Stop();  // Enter stopped state until external wake event.
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - STOP is a 1-byte instruction in this simplified model.
        CompleteInstruction();
    }

    /// <summary> Enter the halted CPU state, including HALT bug handling when interrupts are pending. </summary>
    private void HALT()
    {
        bool interruptPending = GetPendingInterrupts() != 0;
        _state.Halt(_state.InterruptMasterEnabled, interruptPending);
                
        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        //  - HALT is a 1-byte instruction.
        CompleteInstruction();
    }

    /// <summary> Disable interrupt servicing immediately. </summary>
    private void DI()
    {
        _state.DisableInterrupts();   // DI clears IME immediately and cancels any pending EI enable.

        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }

    /// <summary> Schedule interrupt servicing to be enabled after the following instruction. </summary>
    private void EI()
    {
        _state.ScheduleInterruptEnable(); // EI does not enable IME immediately on LR35902.

        // Timing:  (4 total cycles)
        //  - opcode fetch: 4 cycles.
        CompleteInstruction();
    }
    
}
