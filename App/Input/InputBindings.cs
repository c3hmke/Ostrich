using Emulation;

namespace App.Input;
using Silk.NET.Input;

/// <summary>
/// Bindings used by the Application to communicate button presses to the emulator
/// </summary>
public class InputBindings
{
    public readonly Dictionary<Key, GameButton> KeyToButton = new()
    {
        // D-Pad
        [Key.Up]    = GameButton.Up,
        [Key.Down]  = GameButton.Down,
        [Key.Left]  = GameButton.Left,
        [Key.Right] = GameButton.Right,
        
        // Face Buttons
        [Key.Z] = GameButton.A,
        [Key.X] = GameButton.B,
        
        // Func Buttons
        [Key.Enter]     = GameButton.Start,
        [Key.Backspace] = GameButton.Select,
    };
}