namespace GameBoy;

/// <summary> High-level cartridge hardware categories </summary>
public enum CartridgeType
{
    Unknown = 0, ROMOnly, MBC1, MBC1RAM, MBC1RAMBattery
}

/// <summary>
/// Parsed metadata from the Game Boy ROM header.
/// Contains descriptive info about the cart image.
/// </summary>
public class CartridgeHeader
{
    /// <summary> Game title read from the ROM. </summary>
    public required string Title { get; init; }
    
    /// <summary> Raw cartridge type from address 0x0148. </summary>
    public byte TypeCode { get; init; }
    
    /// <summary> Raw ROM size from address 0x0149. </summary>
    public byte ROMSizeCode { get; init; }
    
    /// <summary> Raw RAM size code from address 0x0149. </summary>
    public byte RAMSizeCode { get; init; }
    
    /// <summary> Checksum byte stored in the ROM. </summary>
    public byte HeaderChecksum { get; init; }

    /// <summary> Get the Cartridge Type as defined by the header byte. </summary>
    public CartridgeType CartridgeType => TypeCode switch
    {
        0x00 => CartridgeType.ROMOnly,
        0x01 => CartridgeType.MBC1,
        0x02 => CartridgeType.MBC1RAM,
        0x03 => CartridgeType.MBC1RAMBattery,
        _    => CartridgeType.Unknown
    };
    
    /// <summary> Get the ROM size as defined by the header byte. </summary>
    public int ExpectedROMSize => ROMSizeCode switch
    {
        0x00 => 32 * 1024,
        0x01 => 64 * 1024,
        0x02 => 128 * 1024,
        0x03 => 256 * 1024,
        0x04 => 512 * 1024,
        0x05 => 1024 * 1024,
        0x06 => 2 * 1024 * 1024,
        0x07 => 4 * 1024 * 1024,
        0x08 => 8 * 1024 * 1024,
        
        _ => throw new NotSupportedException($"Unsupported ROM size code: 0x{ROMSizeCode:X2}")
    };
    
    /// <summary> Get the RAM size as defined by the header byte. </summary>
    public int ExpectedRAMSize => RAMSizeCode switch
    {
        0x00 => 0,
        0x02 => 8 * 1024,
        0x03 => 32 * 1024,
        0x04 => 128 * 1024,
        0x05 => 64 * 1024,
        
        _ => throw new NotSupportedException($"Unsupported RAM size code: 0x{RAMSizeCode:X2}")
    };
}