using GameBoy;
using GameBoy.CPU;

namespace Unit;

internal static class TestCpuFactory
{
    internal static LR35902 CreateCpuWithProgram(params byte[] programAtEntry)
        => CreateCpuWithProgram(programAtEntry, romPatches: null);

    internal static LR35902 CreateCpuWithProgram(byte[] programAtEntry, Dictionary<int, byte>? romPatches)
    {
        byte[] rom = BuildValidRom(programAtEntry, romPatches);
        var cart = Cartridge.FromROM(rom, "unit.gb");
        var bus = new Bus(cart);

        var cpu = new LR35902();
        cpu.AttachBus(bus);

        return cpu;
    }

    internal static (LR35902 cpu, Bus bus) CreateCpuAndBusWithProgram(params byte[] programAtEntry)
    {
        byte[] rom = BuildValidRom(programAtEntry, romPatches: null);
        var cart = Cartridge.FromROM(rom, "unit.gb");
        var bus = new Bus(cart);

        var cpu = new LR35902();
        cpu.AttachBus(bus);

        return (cpu, bus);
    }

    private static byte[] BuildValidRom(byte[] programAtEntry, Dictionary<int, byte>? romPatches)
    {
        var rom = new byte[32 * 1024];
        Array.Copy(programAtEntry, 0, rom, 0x0100, programAtEntry.Length);

        if (romPatches is not null)
        {
            foreach ((int address, byte value) in romPatches)
                rom[address] = value;
        }

        rom[0x0147] = 0x00;
        rom[0x0148] = 0x00;
        rom[0x0149] = 0x00;

        int checksum = 0;
        for (int address = 0x0134; address <= 0x014C; address++)
            checksum = checksum - rom[address] - 1;

        rom[0x014D] = (byte)checksum;
        return rom;
    }
}
