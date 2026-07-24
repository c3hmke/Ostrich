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
    
    [Fact]
    public void Oam_ReadsBackWrittenValues()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFE00, 0x12);
        bus.WriteByte(0xFE9F, 0x34);

        Assert.Equal((byte)0x12, bus.ReadByte(0xFE00));
        Assert.Equal((byte)0x34, bus.ReadByte(0xFE9F));
    }
    
    [Fact]
    public void UnusableMemory_ReadsAsFF()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        Assert.Equal((byte)0xFF, bus.ReadByte(0xFEA0));
        Assert.Equal((byte)0xFF, bus.ReadByte(0xFEFF));
    }
    
    [Fact]
    public void UnusableMemory_IgnoresWrites()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFEA0, 0x12);
        bus.WriteByte(0xFEFF, 0x34);

        Assert.Equal((byte)0xFF, bus.ReadByte(0xFEA0));
        Assert.Equal((byte)0xFF, bus.ReadByte(0xFEFF));
    }
    
    [Fact]
    public void EchoRam_ReadsFromWorkRamMirror()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xC000, 0x12);
        bus.WriteByte(0xDDFF, 0x34);

        Assert.Equal((byte)0x12, bus.ReadByte(0xE000));
        Assert.Equal((byte)0x34, bus.ReadByte(0xFDFF));
    }
    [Fact]
    public void EchoRam_WritesToWorkRamMirror()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xE000, 0x56);
        bus.WriteByte(0xFDFF, 0x78);

        Assert.Equal((byte)0x56, bus.ReadByte(0xC000));
        Assert.Equal((byte)0x78, bus.ReadByte(0xDDFF));
    }
}