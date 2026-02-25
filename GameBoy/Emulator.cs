using Emulation;
using GameBoy.Video;

namespace GameBoy;

public class Emulator : IEmulator
{
    public IVideoSource Screen { get; } = new GBVideoSource();
    
    private readonly GBInputSink _input = new();
    
    public IInputSink  Input      => _input;
    public IInputState InputState => _input;
    
    public byte[]? ROM     { get; private set; }
    public string? ROMPath { get; private set; }

    public void LoadROM(byte[] rom, string path)
    {
        if (rom == null || rom.Length < 0x150)
            throw new ArgumentException($"Invalid ROM size {rom?.Length ?? 0}");
        
        ROM = rom; ROMPath = path;
        
        // TODO: Parse header, create Cart(MBC), reset CPU/PPU/MMU state etc.
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