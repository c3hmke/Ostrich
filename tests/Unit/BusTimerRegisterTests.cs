using Xunit;

namespace Unit;

public sealed class BusTimerRegisterTests
{
    [Fact]
    public void TimerRegisters_DefaultReadValues()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        Assert.Equal((byte)0x00, bus.ReadByte(0xFF04)); // DIV
        Assert.Equal((byte)0x00, bus.ReadByte(0xFF05)); // TIMA
        Assert.Equal((byte)0x00, bus.ReadByte(0xFF06)); // TMA
        Assert.Equal((byte)0xF8, bus.ReadByte(0xFF07)); // TAC upper bits read as 1
    }

    [Fact]
    public void TimerRegisters_WriteAndReadTimaTmaTac()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFF05, 0x12); // TIMA
        bus.WriteByte(0xFF06, 0x34); // TMA
        bus.WriteByte(0xFF07, 0xFF); // TAC only keeps low 3 bits

        Assert.Equal((byte)0x12, bus.ReadByte(0xFF05));
        Assert.Equal((byte)0x34, bus.ReadByte(0xFF06));
        Assert.Equal((byte)0xFF, bus.ReadByte(0xFF07)); // 0xF8 | 0x07
    }

    [Fact]
    public void TimerRegisters_WriteToDivResetsDivider()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFF04, 0xAB);

        Assert.Equal((byte)0x00, bus.ReadByte(0xFF04));
    }
}