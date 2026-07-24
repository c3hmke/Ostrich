namespace GameBoy;

/// <summary>
/// Central Address Bus for the emulator.
/// All CPU memory reads and writes should eventually flow through here so address routing stays in one place instead of leaking into the CPU.
/// </summary>
public sealed class Bus
{
    private readonly Cartridge    _cartridge;                    // Cartridge-controlled ROM space.
    private readonly byte[]       _highRam = new byte[127];      // Small scratch region at top of memory mapped at 0xFF80-0xFFFE.
    private readonly byte[]       _vRam    = new byte[8 * 1024]; // Internal video RAM mapped at 0x8000-0x9FFF. (8KB)
    private readonly byte[]       _workRam = new byte[8 * 1024]; // Internal work RAM mapped at 0xC000-0xDFFF. (8KB)
    
    private readonly GameBoyTimer _timer;                        // Memory-mapped timer hardware: DIV, TIMA, TMA, TAC.
    private readonly PPU          _ppu;                          // Minimal PPU timing hardware: currently exposes LY at 0xFF44.
    private byte                  _interruptEnable = 0x00;       // IE at 0xFFFF: which interrupt sources are enabled.
    private byte                  _interruptFlags  = 0x00;       // IF at 0xFF0F: which interrupt sources are currently pending.
    
    /// <summary>
    /// Creates a bus for a loaded cartridge and attaches memory-mapped hardware devices.
    /// </summary>
    public Bus(Cartridge cartridge)
    {
        _cartridge = cartridge;

        // Hardware components request interrupts through the bus because IF lives at 0xFF0F here.
        _timer = new GameBoyTimer(RequestInterrupt);
        _ppu   = new PPU(RequestInterrupt);
    }
    
    /// <summary>
    /// Advances memory-mapped hardware by the number of CPU cycles consumed by the last CPU step.
    /// </summary>
    public void Tick(uint cycles)
    {
        _timer.Tick(cycles);
        _ppu.Tick(cycles);
    }
    
    /// <summary>
    /// Reads a single byte from the Game Boy address space.
    /// </summary>
    public byte ReadByte(ushort address)
    {
        switch (address)
        {
            case <= 0x7FFF:                                 // 0x0000-0x7FFF is cartridge-controlled ROM space.
                return _cartridge.ReadROM(address);
            
            case >= 0x8000 and <= 0x9FFF:                   // 0x800-0x9FFF is fixed internal video RAM.
                return _vRam[address - 0x8000];
            
            case >= 0xC000 and <= 0xDFFF:                   // 0xC000-0xDFFF is fixed internal work RAM.
                return _workRam[address - 0xC000];
            
            case >= 0xFF04 and <= 0xFF07:                   // Timer registers are memory-mapped IO.
                return _timer.ReadByte(address);
            
            case 0xFF44:                                    // Read PPU LY: current LCD scanline.
                return _ppu.ReadByte(address);              
            
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
            case <= 0x7FFF:                             // Ignored for plain ROM carts; later MBC carts will handle bank switching here.
                return;
            
            case >= 0x8000 and <= 0x9FFF:               // Write to the internal video RAM.
                _vRam[address - 0x8000] = value;
                return;
            
            case >= 0xC000 and <= 0xDFFF:               // Write to the internal work RAM.
                _workRam[address - 0xC000] = value;
                return;
            
            case >= 0xFF04 and <= 0xFF07:               // Timer writes can update counters or reset DIV.
                _timer.WriteByte(address, value);
                return;
            
            case 0xFF44:                                // Write PPU LY: first-pass PPU behavior treats writes as timing reset.
                _ppu.WriteByte(address, value);
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
    
    /// <summary>
    /// Sets an interrupt request bit in IF at 0xFF0F.
    /// </summary>
    private void RequestInterrupt(byte interruptMask)
    {
        // Only the low five IF bits exist: VBlank, STAT, Timer, Serial, Joypad.
        _interruptFlags = (byte)((_interruptFlags | interruptMask) & 0x1F);
    }
}


/// <summary>
/// Emulates the Game Boy timer registers mapped at 0xFF04-0xFF07.
/// </summary>
/// <param name="requestInterrupt">Callback used to request the timer interrupt when TIMA overflows.</param>
internal sealed class GameBoyTimer(Action<byte> requestInterrupt)
{
    private const byte TimerInterrupt = 0x04; // IF bit 2.

    private byte _divider;      // FF04 DIV: increments continuously at 16384Hz, exposed as every 256 CPU cycles.
    private byte _timerCounter; // FF05 TIMA: programmable counter.
    private byte _timerModulo;  // FF06 TMA: reload value used when TIMA overflows.
    private byte _timerControl; // FF07 TAC: enable bit plus input clock selection.

    private uint _dividerCycles; // Accumulates leftover cycles until DIV has enough to increment.
    private uint _timerCycles;   // Accumulates leftover cycles until TIMA has enough to increment.

    /// <summary>
    /// Advances DIV and, when enabled, TIMA by the supplied CPU cycle count.
    /// </summary>
    public void Tick(uint cycles)
    {
        // DIV always advances, even when the programmable TIMA timer is disabled.
        _dividerCycles += cycles;
        while (_dividerCycles >= 256)
        {
            _dividerCycles -= 256;
            _divider++;
        }

        // TAC bit 2 is the TIMA enable flag. Frequency bits still read/write when disabled,
        // but the TIMA counter itself should not advance.
        if ((_timerControl & 0x04) == 0)
            return;

        // TIMA advances at the frequency selected by TAC bits 0-1.
        // Keep leftover cycles so split CPU steps still add up correctly.
        _timerCycles += cycles;
        uint timerPeriod = GetTimerPeriodCycles();  
        
        while (_timerCycles >= timerPeriod)
        {
            _timerCycles -= timerPeriod;
            IncrementTimerCounter();
        }
    }

    /// <summary>
    /// Reads one of the timer's memory-mapped registers.
    /// </summary>
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

    /// <summary>
    /// Writes one of the timer's memory-mapped registers.
    /// </summary>
    public void WriteByte(ushort address, byte value)
    {
        switch (address)
        {
            // Any write to DIV resets it, regardless of the written value.
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

            // TAC only uses the low three bits. Resetting the accumulator keeps this
            // first-pass timer behavior easy to reason about when frequency changes.
            case 0xFF07:
                _timerControl = (byte)(value & 0x07);
                _timerCycles = 0;
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(address), address, "Not a timer register.");
        }
    }
    
    /// <summary>
    /// Converts TAC frequency select bits into a TIMA period measured in CPU clock cycles.
    /// </summary>
    private uint GetTimerPeriodCycles()
    {
        // TAC bits 0-1 select the TIMA input clock, expressed here in CPU T-cycles.
        return (_timerControl & 0x03) switch
        {
            0 => 1024,
            1 => 16,
            2 => 64,
            3 => 256,
            _ => throw new InvalidOperationException("Unreachable timer frequency.")
        };
    }
    
    /// <summary>
    /// Increments TIMA, reloading TMA and requesting the timer interrupt when TIMA overflows.
    /// </summary>
    private void IncrementTimerCounter()
    {
        // Simplified overflow behavior: reload immediately and request the timer interrupt.
        // Real hardware has delayed reload edge cases that can be modeled later.
        if (_timerCounter == 0xFF)
        {
            _timerCounter = _timerModulo;
            requestInterrupt(TimerInterrupt);
            return;
        }

        _timerCounter++;
    }
}
