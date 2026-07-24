using Xunit;

namespace Unit;

public sealed class BusMemoryMapTests
{
    [Fact]
    public void Vram_ReadsBackWrittenValues()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0x8000, 0x12);
        bus.WriteByte(0x9FFF, 0x34);

        Assert.Equal((byte)0x12, bus.ReadByte(0x8000));
        Assert.Equal((byte)0x34, bus.ReadByte(0x9FFF));
    }
}