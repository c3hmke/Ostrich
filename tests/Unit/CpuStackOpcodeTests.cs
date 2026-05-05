using Xunit;

namespace Unit;

public sealed class CpuStackOpcodeTests
{
    [Fact] // Verifies CALL pushes return address to stack and jumps to target.
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
    
    [Fact] // Verifies RET pops the return address from stack back into PC.
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

    [Fact] // Verifies RET NZ returns when Z flag is clear.
    public void RetNz_Taken_PopsAddressFromStack()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            programAtEntry: new byte[] { 0xCD, 0x34, 0x12 },
            romPatches: new Dictionary<int, byte>
            {
                [0x1234] = 0x07, // RLCA -> clears Z
                [0x1235] = 0xC0  // RET NZ
            });

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ushort)0xFFFE, cpu.State.SP);
        Assert.Equal((ulong)48, cpu.State.CycleCount); // 24 + 4 + 20
    }

    [Fact] // Verifies RET NZ falls through when Z flag is set.
    public void RetNz_NotTaken_DoesNotPopAddress()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x01, // LD A,0x01
            0x3D,       // DEC A -> Z=1
            0xC0        // RET NZ
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ushort)0xFFFE, cpu.State.SP);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 4 + 8
    }

    [Fact] // Verifies RET C returns when carry flag is set.
    public void RetC_Taken_PopsAddressFromStack()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            programAtEntry: new byte[] { 0xCD, 0x34, 0x12 },
            romPatches: new Dictionary<int, byte>
            {
                [0x1234] = 0x3E, // LD A,0x80
                [0x1235] = 0x80,
                [0x1236] = 0x07, // RLCA -> C=1
                [0x1237] = 0xD8  // RET C
            });

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x0103, cpu.State.PC);
        Assert.Equal((ushort)0xFFFE, cpu.State.SP);
        Assert.Equal((ulong)56, cpu.State.CycleCount); // 24 + 8 + 4 + 20
    }

    [Fact] // Verifies RET C falls through when carry flag is clear.
    public void RetC_NotTaken_DoesNotPopAddress()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x3E, 0x00, // LD A,0x00
            0x07,       // RLCA -> C=0
            0xD8        // RET C
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ushort)0xFFFE, cpu.State.SP);
        Assert.Equal((ulong)20, cpu.State.CycleCount); // 8 + 4 + 8
    }
}
