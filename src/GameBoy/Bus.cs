namespace GameBoy;

/// <summary>
/// Central Address Bus for the emulator.
/// 
/// All CPU memory reads and writes should eventually flow through here.
/// </summary>
public sealed class Bus (Cartridge cartridge)
{
    private readonly byte[] _workRam = new byte[8 * 1024];  // Internal work RAM mapped at 0xC000-0xDFFF. DMG has 8KB in this region.
    private readonly byte[] _highRam = new byte[127];       // Small scratch region at top of memory mapped at 0xFF80-0xFFFE.
    
    private byte _interruptEnable = 0x00;                   // IE at 0xFFFF: which interrupt sources are enabled.
    private byte _interruptFlags  = 0x00;                   // IF at 0xFF0F: which interrupt sources are currently pending.

    /// <summary>
    /// Reads a single byte from the Game Boy address space
    /// </summary>
    public byte ReadByte(ushort address)
    {
        switch (address)
        {
            case <= 0x7FFF:                                 // 0x0000-0x07FF is cartridge controlled ROM space.
                return cartridge.ReadROM(address);
            
            case >= 0xC000 and <= 0xDFFF:                   // 0xC000-0xDFFF is fixed internal work RAM.
                return _workRam[address - 0xC000];
            
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
}