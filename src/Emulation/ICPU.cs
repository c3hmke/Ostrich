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
    bool   Halted     { get; }
    bool   Stopped    { get; }
    
    //--------------------------------------------------------------------------------------------------//
    //                                          Registers                                               //
    //--------------------------------------------------------------------------------------------------//
    byte A { get; }
    byte B { get; }
    byte C { get; }
    byte D { get; }
    byte E { get; }
    byte F { get; }
    byte H { get; }
    byte L { get; }
    
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
    void Halt();                                    // Enter halted state until interrupt-related wake event.
    void Stop();                                    // Enter stopped state until external wake event.
    void Resume();                                  // Resumes CPU execution.
    void Reset();                                   // Reset all members on the CPUState.
    void SetFlags(bool z, bool n, bool h, bool c);  // Set flags on the CPUState.
}

/// <summary> Represents a CPU. </summary>
public interface ICPU
{
    ICPUState State { get; }
}