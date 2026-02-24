using Emulation;

namespace App.Input;
using Silk.NET.Input;

/// <summary>
/// Bindings used by the Application to communicate button presses to the emulator
/// </summary>
public class InputBindings (Dictionary<GameButton, Key>? buttonMap)
{
    /// <summary>
    /// Map of keys and the buttons they're bound to.
    /// </summary>
    private readonly Dictionary<GameButton, Key> _buttonMap 
        = buttonMap ?? new Dictionary<GameButton, Key>
    {
        [GameButton.Up]     = Key.Up,
        [GameButton.Down]   = Key.Down,
        [GameButton.Left]   = Key.Left,
        [GameButton.Right]  = Key.Right,
        [GameButton.A]      = Key.Z,
        [GameButton.B]      = Key.X,
        [GameButton.Start]  = Key.Enter,
        [GameButton.Select] = Key.Backspace,
    };
    
    public IReadOnlyDictionary<GameButton, Key> All => _buttonMap;
    
    public Key GetKey(GameButton button) => _buttonMap[button];
    public void Rebind(GameButton button, Key newKey) => _buttonMap[button] = newKey;
    
    public bool TryGetButtonForKey(Key key, out GameButton button)
    {
        foreach (var kv in _buttonMap)
        {
            if (kv.Value == key)
            {
                button = kv.Key;
                return true;
            }
        }

        button = default;
        return false;
    }
}