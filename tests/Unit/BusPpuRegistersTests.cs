using Xunit;

namespace Unit;

public class BusPpuRegistersTests
{
    [Fact]
    public void PpuRegisters_LyDefaultsToZero()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        Assert.Equal((byte)0x00, bus.ReadByte(0xFF44));
    }

    [Fact]
    public void PpuRegisters_LyIncrementsEvery456Cycles()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.Tick(455);

        Assert.Equal((byte)0x00, bus.ReadByte(0xFF44));

        bus.Tick(1);

        Assert.Equal((byte)0x01, bus.ReadByte(0xFF44));
    }

    [Fact]
    public void PpuRegisters_LyKeepsLeftoverCycles()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.Tick(500);
        bus.Tick(412);

        Assert.Equal((byte)0x02, bus.ReadByte(0xFF44));
    }
    
    [Fact]
    public void PpuRegisters_ReachingLine144RequestsVBlankInterrupt()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.Tick(456 * 143);

        Assert.Equal((byte)143, bus.ReadByte(0xFF44));
        Assert.Equal((byte)0x00, bus.ReadByte(0xFF0F));

        bus.Tick(456);

        Assert.Equal((byte)144, bus.ReadByte(0xFF44));
        Assert.Equal((byte)0x01, bus.ReadByte(0xFF0F));
    }
    
    [Fact]
    public void PpuRegisters_LyWrapsAfterLine153()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.Tick(456 * 153);

        Assert.Equal((byte)153, bus.ReadByte(0xFF44));

        bus.Tick(456);

        Assert.Equal((byte)0, bus.ReadByte(0xFF44));
    }
    
    [Fact]
    public void PpuRegisters_ReadsBackWrittenLcdRegisters()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFF40, 0x91); // LCDC
        bus.WriteByte(0xFF42, 0x12); // SCY
        bus.WriteByte(0xFF43, 0x34); // SCX
        bus.WriteByte(0xFF45, 0x56); // LYC
        bus.WriteByte(0xFF47, 0xE4); // BGP
        bus.WriteByte(0xFF4A, 0x78); // WY
        bus.WriteByte(0xFF4B, 0x9A); // WX

        Assert.Equal((byte)0x91, bus.ReadByte(0xFF40));
        Assert.Equal((byte)0x12, bus.ReadByte(0xFF42));
        Assert.Equal((byte)0x34, bus.ReadByte(0xFF43));
        Assert.Equal((byte)0x56, bus.ReadByte(0xFF45));
        Assert.Equal((byte)0xE4, bus.ReadByte(0xFF47));
        Assert.Equal((byte)0x78, bus.ReadByte(0xFF4A));
        Assert.Equal((byte)0x9A, bus.ReadByte(0xFF4B));
    }
}