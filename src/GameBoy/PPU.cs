namespace GameBoy;

// ReSharper disable InconsistentNaming; Naming scheme follows Game Boy internals over C# standards.

/// <summary>
/// Minimal PPU timing model.
/// Tracks scanline progress and exposes LY timing, but does not render pixels yet.
/// </summary>
/// <param name="requestInterrupt">Callback used to request VBlank when the PPU reaches scanline 144.</param>
internal sealed class PPU(Action<byte> requestInterrupt)
{
    private const byte VBlankInterrupt = 0x01; // IF bit 0.

    private const uint CyclesPerScanline = 456;
    private const byte VisibleScanlines  = 144;
    private const byte TotalScanlines    = 154;

    private uint _scanlineCycles; // Accumulates cycles until a full 456-cycle scanline has elapsed.
    private byte _LY;             // FF44 LY: current LCD scanline.
    
    /// <summary>
    /// Advances the PPU by the supplied CPU cycle count.
    /// </summary>
    public void Tick(uint cycles)
    {
        // The PPU advances in lockstep with CPU time. Each full scanline takes 456 CPU cycles.
        _scanlineCycles += cycles;

        // Use a loop so a large tick can advance through multiple scanlines while preserving leftovers.
        while (_scanlineCycles >= CyclesPerScanline)
        {
            _scanlineCycles -= CyclesPerScanline; _LY++;
            
            // LY 144 is the first VBlank line, so request the VBlank interrupt exactly when it starts.
            if (_LY == VisibleScanlines) requestInterrupt(VBlankInterrupt);
            
            // A full frame is 154 scanlines: 0-143 visible, 144-153 VBlank, then back to 0.
            if (_LY >= TotalScanlines) _LY = 0;
        }
    }
    
    /// <summary>
    /// Reads one of the PPU's memory-mapped registers.
    /// </summary>
    public byte ReadByte(ushort address)
    {
        return address switch
        {
            0xFF44 => _LY,
            _ => throw new ArgumentOutOfRangeException(nameof(address), address, "Not a PPU register.")
        };
    }
    
    /// <summary>
    /// Writes one of the PPU's memory-mapped registers.
    /// </summary>
    public void WriteByte(ushort address, byte value)
    {
        switch (address)
        {
            // LY is read-only-ish from the CPU's point of view in real hardware.
            // For this first pass, treat writes as a reset so tests/debugging stay simple.
            case 0xFF44:
                _LY = 0;
                _scanlineCycles = 0;
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(address), address, "Not a PPU register.");
        }
    }
}
