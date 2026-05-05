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

    [Fact] // Verifies PUSH BC stores BC on stack and decrements SP.
    public void PushBc_PushesWordFromBc()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0x01, 0x34, 0x12, // LD BC,0x1234
            0xC5              // PUSH BC
        );

        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0xFFFC, cpu.State.SP);
        Assert.Equal((byte)0x34, bus.ReadByte(0xFFFC)); // low byte
        Assert.Equal((byte)0x12, bus.ReadByte(0xFFFD)); // high byte
        Assert.Equal((ushort)0x0104, cpu.State.PC);
        Assert.Equal((ulong)28, cpu.State.CycleCount); // 12 + 16
    }

    [Fact] // Verifies POP DE restores a pushed word and increments SP.
    public void PopDe_PopsWordIntoDe()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x01, 0x34, 0x12, // LD BC,0x1234
            0xC5,             // PUSH BC
            0xD1              // POP DE
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.D);
        Assert.Equal((byte)0x34, cpu.State.E);
        Assert.Equal((ushort)0xFFFE, cpu.State.SP);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)40, cpu.State.CycleCount); // 12 + 16 + 12
    }

    [Fact] // Verifies POP AF masks low nibble of F through AF setter.
    public void PopAf_MasksLowNibbleOfF()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x01, 0x3F, 0x12, // LD BC,0x123F
            0xC5,             // PUSH BC
            0xF1              // POP AF
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0x12, cpu.State.A);
        Assert.Equal((byte)0x30, cpu.State.F); // low nibble masked to zero
        Assert.Equal((ushort)0xFFFE, cpu.State.SP);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)40, cpu.State.CycleCount); // 12 + 16 + 12
    }

    [Fact] // Verifies PUSH AF writes A/F bytes in little-endian stack order.
    public void PushAf_PushesAfWordToStack()
    {
        var (cpu, bus) = TestCpuFactory.CreateCpuAndBusWithProgram(
            0xAF,             // XOR A,A -> A=0, Z=1 N/H/C=0
            0x37,             // SCF -> Z preserved, C=1 => F=0x90
            0x3E, 0x12,       // LD A,0x12 (flags unchanged)
            0xF5              // PUSH AF
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((ushort)0xFFFC, cpu.State.SP);
        Assert.Equal((byte)0x90, bus.ReadByte(0xFFFC)); // F byte
        Assert.Equal((byte)0x12, bus.ReadByte(0xFFFD)); // A byte
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)32, cpu.State.CycleCount); // 4 + 4 + 8 + 16
    }

    [Fact] // Verifies PUSH HL then POP BC round-trips value through stack.
    public void PushHlThenPopBc_RoundTripsWord()
    {
        var cpu = TestCpuFactory.CreateCpuWithProgram(
            0x21, 0xCD, 0xAB, // LD HL,0xABCD
            0xE5,             // PUSH HL
            0xC1              // POP BC
        );

        cpu.StepInstruction();
        cpu.StepInstruction();
        cpu.StepInstruction();

        Assert.Equal((byte)0xAB, cpu.State.B);
        Assert.Equal((byte)0xCD, cpu.State.C);
        Assert.Equal((ushort)0xFFFE, cpu.State.SP);
        Assert.Equal((ushort)0x0105, cpu.State.PC);
        Assert.Equal((ulong)40, cpu.State.CycleCount); // 12 + 16 + 12
    }
}
