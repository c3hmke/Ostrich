namespace Emulation;

/// <summary> Represents the state on a CPU. </summary>
public interface ICPUState
{
    ushort PC         { get; }
    ushort SP         { get; }
    ulong  CycleCount { get; }
    bool   Halted     { get; }
    
    // Registers
    byte A { get; }
    byte B { get; }
    byte C { get; }
    byte D { get; }
    byte E { get; }
    byte F { get; }
    byte H { get; }
    byte L { get; }
    
    // Flags
    bool FlagZ { get; }
    bool FlagN { get; }
    bool FlagH { get; }
    bool FlagC { get; }
}

public interface ICPU
{
    ICPUState State { get; }
}