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
    
    private readonly ulong _cyclesPerFrame = 70224;

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
        
        ulong frameEndCycle = _cpu.State.CycleCount + _cyclesPerFrame;
        while (_cpu.State.CycleCount < frameEndCycle)
        {
            ulong cyclesBefore = _cpu.State.CycleCount;
            
            try
            {
                _cpu.StepInstruction();
            }
            catch (NotSupportedException)
            {
                // Temporary bring-up behavior: stop stepping this frame
                // once we hit an opcode we haven't implemented yet.
                break;
            }
            
            // Temporary guard: current HALT/STOP paths may consume no cycles,
            // which would otherwise make this loop infinite.
            if (_cpu.State.CycleCount == cyclesBefore)
                break;
        }
        
        _screen.Clear(0xFFAAFFAA); // Placeholder until CPU/PPU exist.
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