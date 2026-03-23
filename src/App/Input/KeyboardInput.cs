using Emulation;
using Silk.NET.Input;

namespace App.Input;

/// <summary>
/// Event driven keyboard input capture to pass down actions to the emulator.
/// </summary>
public sealed class KeyboardInput (IInputSink sink, InputBindings bindings)
{
    /// <summary>
    /// The last key pressed, regardless of mapping. Used to remap inputs. 
    /// </summary>
    public Key? LastKeyDown { get; private set; }
    public void ClearLastKeyDown() => LastKeyDown = null;

    /// <summary>
    /// Attach an external input device to the App inputs.
    /// </summary>
    public void Attach(IInputContext inputContext)
    {
        // Attach to all connected keyboards
        foreach (var keyboard in inputContext.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
            keyboard.KeyUp   += OnKeyUp;
        }
    }
    
    private void OnKeyDown(IKeyboard keyboard, Key key, int _)
    {
        LastKeyDown = key;
        
        if (bindings.TryGetButtonForKey(key, out var btn))
            sink.SetButton(button: btn, pressed: true);
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int _)
    {
        if (bindings.TryGetButtonForKey(key, out var btn))
            sink.SetButton(button: btn, pressed: false);
    }
}