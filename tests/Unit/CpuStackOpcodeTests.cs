using Xunit;

namespace Unit;

public sealed class CpuStackOpcodeTests
{
    [Fact]
    public void Call_a16_PushesReturnAddressAndJumps()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(0xCD, 0x34, 0x12);

        cpu.StepInstruction();

        Assert.Equal((ushort)0x1234, cpu.State.PC);
        Assert.Equal((ushort)0xFFFC, cpu.State.SP);
        Assert.Equal((byte)0x03, bus.ReadByte(0xFFFC));
        Assert.Equal((byte)0x01, bus.ReadByte(0xFFFD));
        Assert.Equal((ulong)24, cpu.State.CycleCount);
    }

    [Fact]
    public void Ret_PopsAddressFromStack()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            programAtEntry: new byte[] { 0xCD, 0x34, 0x12 },
            romPatches: new Dictionary<int, byte>
            {
                [0x1234] = 0xC9
            });

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ushort)0xFFFE, cpu.State.SP);
        Assert.Equal((ulong)40, cpu.State.CycleCount);
    }
}
