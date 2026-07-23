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
}