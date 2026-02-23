namespace Emulation;


/// <summary>
/// Receives input events from a frontend. The emulator owns how these
/// affect its internal controller state.
/// </summary>
public interface IInputSink
{
    void SetButton(GameButton button, bool pressed);
}

/// <summary>
/// Buttons common on emulated systems
/// </summary>
public enum GameButton
{
    Up, Down, Left, Right,      // D-Pad
    A, B,                       // Face Buttons
    Start, Select               // Func Buttons
}