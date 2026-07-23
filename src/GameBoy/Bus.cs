namespace GameBoy;

/// <summary>
/// Central Address Bus for the emulator.
/// All CPU memory reads and writes should eventually flow through here.
/// </summary>
public sealed class Bus
{
    private readonly Cartridge    _cartridge;                     
    private readonly byte[]       _workRam = new byte[8 * 1024]; // Internal work RAM mapped at 0xC000-0xDFFF. DMG has 8KB in this region.
    private readonly byte[]       _highRam = new byte[127];      // Small scratch region at top of memory mapped at 0xFF80-0xFFFE.
    
    private readonly GameBoyTimer _timer;                            
    private byte                  _interruptEnable = 0x00;       // IE at 0xFFFF: which interrupt sources are enabled.
    private byte                  _interruptFlags  = 0x00;       // IF at 0xFF0F: which interrupt sources are currently pending.
    
    public Bus(Cartridge cartridge)
    {
        _cartridge = cartridge;
        _timer = new GameBoyTimer(RequestInterrupt);
    }
    
    public void Tick(uint cycles)
    {
        _timer.Tick(cycles);
    }
    
    /// <summary>
    /// Reads a single byte from the Game Boy address space
    /// </summary>
    public byte ReadByte(ushort address)
    {
        switch (address)
        {
            case <= 0x7FFF:                                 // 0x0000-0x07FF is cartridge controlled ROM space.
                return _cartridge.ReadROM(address);
            
            case >= 0xC000 and <= 0xDFFF:                   // 0xC000-0xDFFF is fixed internal work RAM.
                return _workRam[address - 0xC000];
            
            case >= 0xFF04 and <= 0xFF07:                   // Timer Registers
                return _timer.ReadByte(address);
            
            case 0xFF0F:                                    // Interrupt Flag (IF)
                return _interruptFlags;
            
            case >= 0xFF80 and <= 0xFFFE:                   // 0xFF80-0xFFFE is high RAM
                return _highRam[address - 0xFF80];
            
            case 0xFFFF:                                    // Interrupt Enable (IE)
                return _interruptEnable;
            
            default: throw new NotImplementedException($"Read not implemented for 0x{address:X4}");
        }
    }
    
    /// <summary>
    /// Writes a single byte into the Game Boy address space.
    /// </summary>
    public void WriteByte(ushort address, byte value)
    {
        switch (address)
        {
            case <= 0x7FFF:                             // This space is only writeable for MBC carts.   
                return;
            
            case >= 0xC000 and <= 0xDFFF:               // Write to the internal work RAM.
                _workRam[address - 0xC000] = value;
                return;
            
            case >= 0xFF04 and <= 0xFF07:               // Write a value to the Timer.
                _timer.WriteByte(address, value);
                return;
            
            case 0xFF0F:                                // Interrupt Flag (IF)
                _interruptFlags = (byte)(value & 0x1F); // Only low 5 interrupt bits are used.
                return;
            
            case >= 0xFF80 and <= 0xFFFE:               // Write into the high RAM.
                _highRam[address - 0xFF80] = value;
                return;
            
            case 0xFFFF:                                 // Interrupt Enable (IE)
                _interruptEnable = (byte)(value & 0x1F); // Only low 5 interrupt bits are used.
                return;
            
            default: throw new NotImplementedException($"Write not implemented for 0x{address:X4}");
        }
    }
    
    private void RequestInterrupt(byte interruptMask)
    {
        _interruptFlags = (byte)((_interruptFlags | interruptMask) & 0x1F);
    }
}


/// <summary>
/// The Timer Register used by the Bus for event timings:
///     FF04 DIV  - increments every 256 CPU cycles.
///     FF05 TIMA - increments based on TAC frequency.
///     FF06 TMA  - modulo reload value.
///     FF07 TAC  - bit 2 enables timer, bits 0-1 select frequency.
///
/// Frequency periods in CPU T-cycles:
///     0 : 1024
///     1 : 16
///     2 : 64
///     3 : 256
/// </summary>
internal sealed class GameBoyTimer(Action<byte> requestInterrupt)
{
    private const byte TimerInterrupt = 0x04;

    private byte _divider;
    private byte _timerCounter;
    private byte _timerModulo;
    private byte _timerControl;

    private uint _dividerCycles;
    private uint _timerCycles;

    public void Tick(uint cycles)
    {
        // Always advance DIV.
        // Only advance TIMA if TAC bit 2 is enabled.
    }

    public byte ReadByte(ushort address)
    {
        return address switch
        {
            0xFF04 => _divider,
            0xFF05 => _timerCounter,
            0xFF06 => _timerModulo,
            0xFF07 => (byte)(0xF8 | _timerControl),
            _ => throw new ArgumentOutOfRangeException(nameof(address), address, "Not a timer register.")
        };
    }

    public void WriteByte(ushort address, byte value)
    {
        switch (address)
        {
            case 0xFF04:
                _divider = 0;
                _dividerCycles = 0;
                return;

            case 0xFF05:
                _timerCounter = value;
                return;

            case 0xFF06:
                _timerModulo = value;
                return;

            case 0xFF07:
                _timerControl = (byte)(value & 0x07);
                _timerCycles = 0;
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(address), address, "Not a timer register.");
        }
    }
}