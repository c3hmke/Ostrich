using Xunit;

namespace Unit;

public sealed class BusIoRegisterTests
{
    [Fact]
    public void JoypadRegister_DefaultsToNoButtonsPressed()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        Assert.Equal((byte)0xCF, bus.ReadByte(0xFF00));
    }

    [Fact]
    public void JoypadRegister_PreservesSelectionBitsAndReportsNoButtonsPressed()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFF00, 0x10);

        Assert.Equal((byte)0xDF, bus.ReadByte(0xFF00));

        bus.WriteByte(0xFF00, 0x20);

        Assert.Equal((byte)0xEF, bus.ReadByte(0xFF00));
    }
    
    [Fact]
    public void SerialRegisters_ReadBackWrittenValues()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFF01, 0x42);
        bus.WriteByte(0xFF02, 0x81);

        Assert.Equal((byte)0x42, bus.ReadByte(0xFF01));
        Assert.Equal((byte)0x81, bus.ReadByte(0xFF02));
    }
    
    [Fact]
    public void SerialRegisters_DefaultReadValues()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        Assert.Equal((byte)0x00, bus.ReadByte(0xFF01));
        Assert.Equal((byte)0x7E, bus.ReadByte(0xFF02));
    }
}