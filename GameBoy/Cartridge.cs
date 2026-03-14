using System.Text;

namespace GameBoy;

/// <summary>
/// Represents a loaded Game Boy cartridge image and its parsed header data.
/// </summary>
public class Cartridge
{
    private const int MinROMSize = 0x150;
    
    public CartridgeHeader Header { get; }
    public byte[]          ROMData { get; }
    public string          SourcePath { get; }

    /// <summary> Force the use of the factory method. </summary>
    private Cartridge(CartridgeHeader header, byte[] romData, string sourcePath)
    {
        Header     = header;
        ROMData    = romData;
        SourcePath = sourcePath;
    }
    
    /// <summary>
    /// Construct a Cartridge using raw ROM data
    /// </summary>
    public static Cartridge FromROM(byte[] romData, string sourcePath)
    {
        if (romData.Length < MinROMSize)
            throw new ArgumentException($"Invalid ROM size {romData.Length}", nameof(romData));

        return new Cartridge(
            header:     ParseHeader(romData),
            romData:    romData,
            sourcePath: sourcePath
        );
    }

    /// <summary>
    /// Reads the fixed header region and builds a CartridgeHeader
    /// </summary>
    private static CartridgeHeader ParseHeader(byte[] romData)
    {
        // First get the title field and construct a string title.
        ReadOnlySpan<byte> titleBytes = romData.AsSpan(start: 0x0134, length: 16);

        int length = titleBytes.IndexOf((byte)0x00);
        if (length < 0)
            length = titleBytes.Length;
        
        string title = Encoding.ASCII.GetString(titleBytes[..length]).TrimEnd();
        
        // build and return a header with the info from the ROM file.
        return new CartridgeHeader
        {
            Title          = title,
            TypeCode  = romData[0x0147],
            ROMSizeCode    = romData[0x0148],
            RAMSizeCode    = romData[0x0149],
            HeaderChecksum = romData[0x014D]
        };
    }

    /// <summary>
    /// Does all the validation checks on a Cartridge and returns whether it passes. 
    /// </summary>
    public bool IsValid() =>
        HasValidHeaderChecksum() && HasValidROMSize();

    /// <summary>
    /// Validates the standard Game Boy header checksum over bytes 0x0134 through 0x014C.
    /// </summary>
    public bool HasValidHeaderChecksum()
    {
        // run over the memory addresses to compute the checksum
        var checksum = 0;
        for (var address = 0x0134; address <= 0x014C; address++)
            checksum = checksum - ROMData[address] - 1;

        // compare the result to the header file
        return (byte)checksum == Header.HeaderChecksum;
    }
    
    /// <summary>
    /// Checks whether the actual ROM length matches the size declared by the ROM header.
    /// </summary>
    public bool HasValidROMSize()
    {
        // Getting the ROMSize from the Header can throw an exception, 
        // we only want this to return true or false for validation.
        try
        {
            return ROMData.Length == Header.ExpectedROMSize;
        }
        catch { return false; }
    }
}