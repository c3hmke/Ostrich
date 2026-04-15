namespace GameBoy;


/// <summary> Holds the state of the CPU. </summary>
public sealed class CPUState
{
    public byte   A, B, C, D, E, F, H, L;       // Registers 
    public ushort PC, SP;                       // Special
    public ulong  CycleCount;                   // Number of cycles ran
    public bool   Halted;                       // Whether currently halted

    // Flags on the CPU
    public bool FlagZ => (F & 0x80) != 0;
    public bool FlagN => (F & 0x40) != 0;
    public bool FlagH => (F & 0x20) != 0;
    public bool FlagC => (F & 0x10) != 0;
}

/// <summary>
/// Emulates the LR35902 CPU, using the CPUState to execute commands
/// </summary>
public class CPU
{
    
}