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
    
    [Fact]
    public void TimerRegisters_DivIncrementsEvery256Cycles()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.Tick(255);

        Assert.Equal((byte)0x00, bus.ReadByte(0xFF04));

        bus.Tick(1);

        Assert.Equal((byte)0x01, bus.ReadByte(0xFF04));
    }
    
    [Fact]
    public void TimerRegisters_DivKeepsLeftoverCycles()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.Tick(300);  // 300 + 212 is 512. First tick should leave 44 cycles
        bus.Tick(212);  // and the second tick should use those leftovers.

        Assert.Equal((byte)0x02, bus.ReadByte(0xFF04));
    }
    
    [Fact]
    public void TimerRegisters_TimaDoesNotIncrementWhenTimerDisabled()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFF07, 0x01); // frequency selected, but enable bit is not set
        bus.Tick(16);

        Assert.Equal((byte)0x00, bus.ReadByte(0xFF05));
    }
    
    [Fact]
    public void TimerRegisters_TimaIncrementsWhenTimerEnabled()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFF07, 0x05); // enable bit set, frequency 01 = 16 cycles
        bus.Tick(15);

        Assert.Equal((byte)0x00, bus.ReadByte(0xFF05));

        bus.Tick(1);

        Assert.Equal((byte)0x01, bus.ReadByte(0xFF05));
    }
    
    [Fact]
    public void TimerRegisters_TimaOverflowReloadsTmaAndRequestsInterrupt()
    {
        var (_, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0x00);

        bus.WriteByte(0xFF05, 0xFF); // TIMA
        bus.WriteByte(0xFF06, 0x42); // TMA
        bus.WriteByte(0xFF07, 0x05); // enable bit set, frequency 01 = 16 cycles

        bus.Tick(16);

        Assert.Equal((byte)0x42, bus.ReadByte(0xFF05));
        Assert.Equal((byte)0x04, bus.ReadByte(0xFF0F));
    }
}