using Emulation;
using GameBoy.CPU;
using GameBoy.Video;

namespace GameBoy;

public class Emulator : IEmulator
{
    private readonly GBVideoSource _screen = new();
    private readonly GBInputSink   _input  = new();
    private readonly LR35902       _cpu    = new();
    
    public  Cartridge?  LoadedCartridge { get; private set; }
    private Bus?        _bus;
    
    public IVideoSource Screen      => _screen;
    public IInputSink   Input       => _input;
    public IInputState  InputState  => _input;
    public ICPU         CPU         => _cpu;
    
    /// <summary> Indicates if a cartridge is successfully loaded. </summary>
    public bool IsROMLoaded => LoadedCartridge is not null;


    /// <summary> Load a ROM into memory and reset the core. </summary>
    public void LoadROM(byte[] rom, string path)
    {
        if (rom == null) 
            throw new ArgumentNullException(nameof(rom));
        
        var loadedCart = Cartridge.FromROM(rom, path);
        
        if (!loadedCart.IsValid)
            throw new InvalidOperationException("ROM failed cartridge validation.");
        
        if (loadedCart.Header.CartridgeType == CartridgeType.Unknown)
            throw new NotSupportedException($"Unsupported cartridge type 0x{loadedCart.Header.TypeCode:X2}.");
        
        LoadedCartridge = loadedCart;
        Reset();
    }

    /// <summary> Set the screen to black. </summary>
    public void Reset()
    {
        _cpu.Reset();
        
        if (LoadedCartridge is null)
        {
            _bus = null;
            _screen.Clear(0xFFFFFF);
            
            return;
        }
        
        _bus = new Bus(LoadedCartridge);
        _cpu.AttachBus(_bus);
        
        _screen.Clear(0xFFFFFF); // Placeholder visual state until frame execution is wired to CPU/PPU.
    }

    public void StepFrame()
    {
        if (LoadedCartridge is null || _bus is null) 
            return;
        
        const ulong cyclesPerFrame  = 70224;
        uint        cyclesThisFrame = 0;
        
        while (cyclesThisFrame < cyclesPerFrame)
        {
            uint elapsedCycles = _cpu.StepInstruction();

            // STOP or no bus can currently produce 0 cycles.
            // Keep this until STOP wake/input behavior is modeled.
            if (elapsedCycles == 0)
                break;
            
            cyclesThisFrame += elapsedCycles;
            
            // TODO:
            // _timer.Tick(elapsedCycles);
            // _ppu.Tick(elapsedCycles);
            // _apu.Tick(elapsedCycles);
        }
        
        _screen.Clear(0xFFAAFFAA); // Placeholder until PPU exists.
    }
    
    /// <summary> Minimal button state store. </summary>
    internal sealed class GBInputSink : IInputSink, IInputState
    {
        private readonly bool[] _pressed = new bool[8];

        public void SetButton(GameButton button, bool pressed)
            => _pressed[(int)button] = pressed;

        public bool IsPressed(GameButton button)
            => _pressed[(int)button];
    }
}