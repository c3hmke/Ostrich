namespace GameBoy;

/// <summary>
/// Central Address Bus for the emulator.
/// 
/// All CPU memory reads and writes should eventually flow through here.
/// </summary>
public sealed class Bus (Cartridge cartridge)
{
    /// <summary> Internal work RAM mapped at 0xC000-0xDFFF. DMG has 8KB in this region. </summary>
    private readonly byte[] _workRam = new byte[8 * 1024];
    
    /// <summary> Small scratch region at top of memory mapped at 0xFF80-0xFFFE. </summary>
    private readonly byte[] _highRam = new byte[127];

    /// <summary>
    /// Reads a single byte from the Game Boy address space
    /// </summary>
    public byte ReadByte(ushort address)
    {
        switch (address)
        {
            case <= 0x7FFF:                             // 0x0000-0x07FF is cartridge controlled ROM space.
                return cartridge.ReadROM(address);
            
            case >= 0xC000 and <= 0xDFFF:               // 0xC000-0xDFFF is fixed internal work RAM.
                return _workRam[address - 0xC000];
            
            case >= 0xFF80 and <= 0xFFFE:               // 0xFF80-0xFFFE is high RAM
                return _highRam[address - 0xFF80];
            
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
            
            case >= 0xFF80 and <= 0xFFFE:               // Write into the high RAM.
                _highRam[address - 0xFF80] = value;
                return;
            
            default: throw new NotImplementedException($"Write not implemented for 0x{address:X4}");
        }
    }
}