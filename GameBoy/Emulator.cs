using Emulation;
using GameBoy.Video;

namespace GameBoy;

public class Emulator : IEmulator
{
    public IVideoSource Screen { get; } = new GBVideoSource();
    
    private readonly GBInputSink _input = new();
    
    public IInputSink  Input      => _input;
    public IInputState InputState => _input;
    
    
    /// <summary>
    /// Minimal button state store.
    /// </summary>
    internal sealed class GBInputSink : IInputSink, IInputState
    {
        private readonly bool[] _pressed = new bool[8];

        public void SetButton(GameButton button, bool pressed)
            => _pressed[(int)button] = pressed;

        public bool IsPressed(GameButton button)
            => _pressed[(int)button];
    }
}