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
/// Read-Only view of the current button state.
/// </summary>
public interface IInputState
{
    bool IsPressed(GameButton button);
}

/// <summary>
/// Buttons common on emulated systems.
/// </summary>
public enum GameButton
{
    Up, Down, Left, Right,      // D-Pad
    A, B,                       // Face Buttons
    Start, Select               // Func Buttons
}