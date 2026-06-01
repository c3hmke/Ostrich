using Xunit;

namespace Unit;

public sealed class CpuInterruptTests
{
    [Fact] // Verifies a pending enabled interrupt wakes HALT and services the highest-priority vector when IME is set.
    public void PendingInterrupt_WakesHaltAndServicesInterruptWhenImeEnabled()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0xFB, // EI
            0x00, // NOP -> delayed EI takes effect after this instruction
            0x76  // HALT
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        bus.WriteByte(0xFFFF, 0x01); // IE: VBlank enabled
        bus.WriteByte(0xFF0F, 0x01); // IF: VBlank pending

        cpu.StepInstruction();

        Assert.False(cpu.State.Halted);
        Assert.False(cpu.State.InterruptMasterEnabled);
        Assert.Equal((ushort)0x0040, cpu.State.PC);
        Assert.Equal((ushort)0xFFFC, cpu.State.SP);
        Assert.Equal((byte)0x03, bus.ReadByte(0xFFFC));
        Assert.Equal((byte)0x01, bus.ReadByte(0xFFFD));
        Assert.Equal((byte)0x00, bus.ReadByte(0xFF0F));
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 4 + 4 + 4 + 20
    }

    [Fact] // Verifies a pending enabled interrupt wakes HALT without servicing when IME is clear.
    public void PendingInterrupt_WakesHaltWithoutServicingWhenImeDisabled()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x76, // HALT
            0x00  // NOP
        );

        cpu.StepInstruction();

        bus.WriteByte(0xFFFF, 0x01); // IE: VBlank enabled
        bus.WriteByte(0xFF0F, 0x01); // IF: VBlank pending

        cpu.StepInstruction();

        Assert.False(cpu.State.Halted);
        Assert.False(cpu.State.InterruptMasterEnabled);
        Assert.Equal((ushort)0x0102, cpu.State.PC); // NOP executed after HALT wake.
        Assert.Equal((ushort)0xFFFE, cpu.State.SP); // No interrupt stack push occurred.
        Assert.Equal((byte)0x01, bus.ReadByte(0xFF0F));
        Assert.Equal((ulong)8, cpu.State.CycleCount); // 4 + 4
    }

    [Fact] // Verifies interrupt servicing pushes PC, clears only the chosen IF bit, and uses priority order.
    public void PendingInterrupts_ServiceHighestPriorityAndPreserveLowerPriorityRequests()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0xFB, // EI
            0x00, // NOP -> delayed EI takes effect after this instruction
            0x00  // Placeholder instruction that must not execute when interrupt is serviced
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        bus.WriteByte(0xFFFF, 0x05); // IE: VBlank + Timer enabled
        bus.WriteByte(0xFF0F, 0x05); // IF: VBlank + Timer pending

        cpu.StepInstruction();

        Assert.False(cpu.State.InterruptMasterEnabled);
        Assert.Equal((ushort)0x0040, cpu.State.PC); // VBlank has highest priority.
        Assert.Equal((ushort)0xFFFC, cpu.State.SP);
        Assert.Equal((byte)0x02, bus.ReadByte(0xFFFC));
        Assert.Equal((byte)0x01, bus.ReadByte(0xFFFD));
        Assert.Equal((byte)0x04, bus.ReadByte(0xFF0F)); // Timer request remains pending.
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 4 + 4 + 20
    }
}
