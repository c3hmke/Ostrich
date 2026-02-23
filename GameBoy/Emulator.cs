using Emulation;
using GameBoy.Video;

namespace GameBoy;

public class Emulator : IEmulator
{
    public IVideoSource Screen { get; } = new GBVideoSource();
    public IInputSink   Input  { get; } = new GBInputSink();
    
    // Let the core read the current button state later
    internal GBInputSink JoyPad => (GBInputSink)Input;
    
    /// <summary>
    /// Minimal button state store.
    /// This does NOT implement the JoyPad register semantics yet.
    /// It only records what is currently pressed.
    /// </summary>
    internal sealed class GBInputSink : IInputSink
    {
        // Store pressed state (true = pressed).
        // You can swap this to a bitfield later when wiring JoyPad.
        private readonly bool[] _pressed = new bool[8];

        public void SetButton(GameButton button, bool pressed)
        {
            _pressed[(int)button] = pressed;

            // TODO:
            // - if pressed transitioned false->true, request JoyPad interrupt
            // - map to JoyPad register active-low bits based on select lines
        }

        public bool IsPressed(GameButton button) => _pressed[(int)button];
    }
}