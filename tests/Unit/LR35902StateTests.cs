using GameBoy.CPU;
using Xunit;

namespace Unit;

public sealed class LR35902StateTests
{
    [Fact] // Verifies pair setters split high/low bytes into component registers.
    public void RegisterPairSetters_UpdateComponentRegisters()
    {
        var state = new LR35902State
        {
            AF = 0x12F7,
            BC = 0x3456,
            DE = 0x789A,
            HL = 0xBCDE
        };

        Assert.Equal((byte)0x12, state.A);
        Assert.Equal((byte)0xF0, state.F);
        Assert.Equal((byte)0x34, state.B);
        Assert.Equal((byte)0x56, state.C);
        Assert.Equal((byte)0x78, state.D);
        Assert.Equal((byte)0x9A, state.E);
        Assert.Equal((byte)0xBC, state.H);
        Assert.Equal((byte)0xDE, state.L);
    }
    
    [Fact] // Verifies pair getters recombine component registers into 16-bit values.
    public void RegisterPairGetters_CombineRegisters()
    {
        var state = new LR35902State
        {
            AF = 0x1234,
            BC = 0x5678,
            DE = 0x9ABC,
            HL = 0xDEF0
        };

        Assert.Equal((ushort)0x1230, state.AF);
        Assert.Equal((ushort)0x5678, state.BC);
        Assert.Equal((ushort)0x9ABC, state.DE);
        Assert.Equal((ushort)0xDEF0, state.HL);
    }
    
    [Fact] // Verifies SetFlags maps Z/N/H/C exactly onto the high nibble of F.
    public void SetFlags_SetsExactHighNibble()
    {
        var state = new LR35902State();

        state.SetFlags(z: true, n: false, h: true, c: false);

        Assert.Equal((byte)0xA0, state.F);
        Assert.True(state.FlagZ);
        Assert.False(state.FlagN);
        Assert.True(state.FlagH);
        Assert.False(state.FlagC);
    }
    
    [Fact] // Verifies individual flag updates never pollute the low nibble of F.
    public void SetFlagOperations_PreserveLowNibbleAsZero()
    {
        var state = new LR35902State
        {
            AF = 0x00FF
        };

        state.SetFlagZ(false);
        state.SetFlagN(true);
        state.SetFlagH(false);
        state.SetFlagC(true);

        Assert.Equal((byte)0x50, state.F);
        Assert.Equal((byte)0x00, (byte)(state.F & 0x0F));
    }
    
    [Fact] // Verifies Reset restores canonical power-on state for counters/registers.
    public void Reset_RestoresDefaultCpuState()
    {
        var state = new LR35902State
        {
            AF = 0x1230,
            BC = 0x5678,
            DE = 0x9ABC,
            HL = 0xDEF0
        };

        state.SetFlags(z: false, n: false, h: false, c: false);
        state.AddClockCycles(1234);

        state.Reset();

        Assert.Equal((ushort)0x0100, state.PC);
        Assert.Equal((ushort)0xFFFE, state.SP);
        Assert.Equal((ulong)0, state.CycleCount);
        Assert.False(state.Halted);
        Assert.False(state.Stopped);
        Assert.False(state.InterruptMasterEnabled);
        Assert.False(state.InterruptEnabledPending);

        Assert.Equal((byte)0x00, state.A);
        Assert.Equal((byte)0x00, state.B);
        Assert.Equal((byte)0x00, state.C);
        Assert.Equal((byte)0x00, state.D);
        Assert.Equal((byte)0x00, state.E);
        Assert.Equal((byte)0xB0, state.F);
        Assert.Equal((byte)0x00, state.H);
        Assert.Equal((byte)0x00, state.L);
    }

    [Fact] // Verifies Halt enters halted state and clears stopped state.
    public void Halt_SetsHaltedAndClearsStopped()
    {
        var state = new LR35902State();

        state.Stop();
        state.Halt();

        Assert.True(state.Halted);
        Assert.False(state.Stopped);
    }

    [Fact] // Verifies Stop enters stopped state and clears halted state.
    public void Stop_SetsStoppedAndClearsHalted()
    {
        var state = new LR35902State();

        state.Halt();
        state.Stop();

        Assert.False(state.Halted);
        Assert.True(state.Stopped);
    }

    [Fact] // Verifies Resume exits both halted and stopped states.
    public void Resume_ClearsHaltedAndStopped()
    {
        var state = new LR35902State();

        state.Halt();
        state.Resume();
        Assert.False(state.Halted);
        Assert.False(state.Stopped);

        state.Stop();
        state.Resume();
        Assert.False(state.Halted);
        Assert.False(state.Stopped);
    }

    [Fact] // Verifies DisableInterrupts clears both IME and any pending delayed enable.
    public void DisableInterrupts_ClearsImeAndPending()
    {
        var state = new LR35902State();

        state.ScheduleInterruptEnable();
        state.EnableInterrupts();
        state.ScheduleInterruptEnable();
        state.DisableInterrupts();

        Assert.False(state.InterruptMasterEnabled);
        Assert.False(state.InterruptEnabledPending);
    }

    [Fact] // Verifies EnableInterrupts sets IME immediately and clears pending enable state.
    public void EnableInterrupts_SetsImeAndClearsPending()
    {
        var state = new LR35902State();

        state.ScheduleInterruptEnable();
        state.EnableInterrupts();

        Assert.True(state.InterruptMasterEnabled);
        Assert.False(state.InterruptEnabledPending);
    }

    [Fact] // Verifies ScheduleInterruptEnable defers IME by setting only the pending latch.
    public void ScheduleInterruptEnable_SetsPendingOnly()
    {
        var state = new LR35902State();

        state.ScheduleInterruptEnable();

        Assert.False(state.InterruptMasterEnabled);
        Assert.True(state.InterruptEnabledPending);
    }

    [Fact] // Verifies ApplyPendingInterruptEnable promotes pending enable into active IME.
    public void ApplyPendingInterruptEnable_PromotesPendingToIme()
    {
        var state = new LR35902State();

        state.ScheduleInterruptEnable();
        state.ApplyPendingInterruptEnable();

        Assert.True(state.InterruptMasterEnabled);
        Assert.False(state.InterruptEnabledPending);
    }
}
