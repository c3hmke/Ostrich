using Emulation;
using GameBoy.Video;

namespace GameBoy;

public class Emulator : IEmulator
{
    private readonly GBVideoSource _screen = new();
    private readonly GBInputSink   _input  = new();

    public Cartridge?   LoadedCartridge { get; private set; }
    private Bus?        _bus;
    
    public IVideoSource Screen      => _screen;
    public IInputSink   Input       => _input;
    public IInputState  InputState  => _input;
    
    
    /// <summary> Indicates if a cartridge is successfully loaded. </summary>
    public bool IsROMLoaded => LoadedCartridge is not null;


    /// <summary>
    /// Load a ROM into memory and reset the core.
    /// </summary>
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
        if (LoadedCartridge is null)
        {
            _bus = null;
            _screen.Clear(0xFFFFFF);
            
            return;
        }
        
        _bus = new Bus(LoadedCartridge);
        _screen.Clear(0xFFFFFF); // Placeholder visual state until frame execution is wired to CPU/PPU.
    }

    public void StepFrame()
    {
        if (LoadedCartridge is null || _bus is null) 
            return;
        
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