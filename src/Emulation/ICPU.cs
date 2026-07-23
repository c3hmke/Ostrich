namespace Emulation;

/// <summary> Represents the state on a CPU. </summary>
public interface ICPUState
{
    //--------------------------------------------------------------------------------------------------//
    //                                          Counters                                                //
    //--------------------------------------------------------------------------------------------------//
    ushort PC         { get; }
    ushort SP         { get; }
    ulong  CycleCount { get; }
    
    //--------------------------------------------------------------------------------------------------//
    //                                          Execution                                               //
    //--------------------------------------------------------------------------------------------------//
    bool   Halted                  { get; }
    bool   Stopped                 { get; }
    bool   InterruptMasterEnabled  { get; }
    bool   InterruptEnabledPending { get; }
    
    //--------------------------------------------------------------------------------------------------//
    //                                          Registers                                               //
    //--------------------------------------------------------------------------------------------------//
    byte A { get; } byte B { get; }
    byte C { get; } byte D { get; }
    byte E { get; } byte F { get; }
    byte H { get; } byte L { get; }
    
    //--------------------------------------------------------------------------------------------------//
    //                                            FLAGS                                                 //
    //--------------------------------------------------------------------------------------------------//
    bool FlagZ { get; }
    bool FlagN { get; }
    bool FlagH { get; }
    bool FlagC { get; }
    
    
    //--------------------------------------------------------------------------------------------------//
    //                                         FUNCTIONS                                                //
    //--------------------------------------------------------------------------------------------------//
    void Halt(bool interruptMasterEnabled, bool interruptPending); // Enter HALT or trigger HALT bug based on interrupt state.
    void Stop();                                                   // Enter stopped state until external wake event.
    void Resume();                                                 // Resumes CPU execution.
    void Reset();                                                  // Reset all members on the CPUState.
    void DisableInterrupts();                                      // Clears IME immediately.
    void EnableInterrupts();                                       // Sets IME immediately.
    void ScheduleInterruptEnable();                                // Schedules IME to be enabled after the following instruction.
    void ApplyPendingInterruptEnable();                            // Applies the delayed EI effect when appropriate.
    void SetFlags(bool z, bool n, bool h, bool c);                 // Set flags on the CPUState.
}

/// <summary> Represents a CPU. </summary>
public interface ICPU
{
    ICPUState State { get; }

    /// <summary>
    /// Advances the CPU by one instruction or interrupt/idle step,
    /// returning the number of clock cycles consumed.
    /// </summary>
    uint StepInstruction();
}
