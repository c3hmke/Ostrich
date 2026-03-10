using Emulation;
using GameBoy.Video;

namespace GameBoy;

public class Emulator : IEmulator
{
    private readonly GBVideoSource _screen = new();
    private readonly GBInputSink   _input  = new();
    
    public IVideoSource Screen      => _screen;
    public IInputSink   Input       => _input;
    public IInputState  InputState  => _input;
    
    public byte[]? ROM     { get; private set; }
    public string? ROMPath { get; private set; }

    /// <summary>
    /// Load a ROM into memory and reset the core.
    /// </summary>
    public void LoadROM(byte[] rom, string path)
    {
        if (rom == null || rom.Length < 0x150)
            throw new ArgumentException($"Invalid ROM size {rom?.Length ?? 0}");
        
        ROM = rom; ROMPath = path;
        Reset();
    }
    public bool IsROMLoaded => ROM is not null;

    /// <summary> Set the screen to black. </summary>
    public void Reset() => _screen.Clear(0xFFFFFF);

    public void StepFrame()
    {
        if (!IsROMLoaded) return;
        
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