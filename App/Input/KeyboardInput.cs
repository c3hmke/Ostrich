using Emulation;
using Silk.NET.Input;

namespace App.Input;

/// <summary>
/// Event driven keyboard input capture to pass down actions to the emulator.
/// </summary>
public sealed class KeyboardInput (IInputSink sink, InputBindings bindings)
{
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
        if (bindings.KeyToButton.TryGetValue(key, out var btn))
            sink.SetButton(button: btn, pressed: true);
    }

    private void OnKeyUp(IKeyboard keyboard, Key key, int _)
    {
        if (bindings.KeyToButton.TryGetValue(key, out var btn))
            sink.SetButton(button: btn, pressed: false);
    }
}