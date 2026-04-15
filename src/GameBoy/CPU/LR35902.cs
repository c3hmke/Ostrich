using Emulation;

namespace GameBoy.CPU;

public sealed class LR35902State : ICPUState
{
    public ushort PC         { get; } = 0x0100; // Entry point (0x0000 for reset vector)
    public ushort SP         { get; } = 0xFFFE; // Top of HRAM
    public ulong  CycleCount { get; } = 0;
    public bool   Halted     { get; } = false;

    public byte   A          { get; } = 0;
    public byte   B          { get; } = 0;
    public byte   C          { get; } = 0;
    public byte   D          { get; } = 0;
    public byte   E          { get; } = 0;
    public byte   F          { get; } = 0xB0;   // Convention (bits 5,7 set)
    public byte   H          { get; } = 0;
    public byte   L          { get; } = 0;
    
    public bool FlagZ => (F & 0x80) != 0;
    public bool FlagN => (F & 0x40) != 0;
    public bool FlagH => (F & 0x20) != 0;
    public bool FlagC => (F & 0x10) != 0;
}

public sealed class LR35902 : ICPU
{
    private readonly LR35902State _state = new();
    
    public ICPUState State => _state;
}