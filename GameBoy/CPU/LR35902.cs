using Emulation;

namespace GameBoy.CPU;

public sealed class LR35902State : ICPUState
{
    // public byte   A, B, C, D, E, F, H, L;
    // public ushort PC, SP;
    // public ulong  CycleCount;
    // public bool   Halted;

    public ushort PC { get; }
    public ushort SP { get; }
    public ulong CycleCount { get; }
    public bool Halted { get; }
    public byte A { get; }
    public byte B { get; }
    public byte C { get; }
    public byte D { get; }
    public byte E { get; }
    public byte F { get; }
    public byte H { get; }
    public byte L { get; }
    public bool FlagZ => (F & 0x80) != 0;
    public bool FlagN => (F & 0x40) != 0;
    public bool FlagH => (F & 0x20) != 0;
    public bool FlagC => (F & 0x10) != 0;
}

/// <summary>
/// The LR35902 is the CPU found in the DMG Game Boy. This class emulates
/// the function of that specific CPU.
/// </summary>
public sealed class LR35902 : ICPU
{
    private readonly LR35902State _state = new();
    
    public ICPUState State => _state;
}