using Emulation;

namespace App.Config;

/// <summary>
/// Used to persist user configuration.
/// </summary>
public class AppConfig
{
    public WindowConfigDTO  Window { get; set; } = new();
    public InputBindingsDTO Input  { get; set; } = new();
}

public sealed class WindowConfigDTO
{
    public int  Scale        { get; set; } = 3;
    public bool VSyncEnabled { get; set; } = true;
}

public sealed class InputBindingsDTO
{
    // Store these as strings for stability across versions / platforms
    public Dictionary<GameButton, string> ButtonToKey { get; set; } = new()
    {
        [GameButton.Up]     = "Up",
        [GameButton.Down]   = "Down",
        [GameButton.Left]   = "Left",
        [GameButton.Right]  = "Right",
        [GameButton.A]      = "Z",
        [GameButton.B]      = "X",
        [GameButton.Start]  = "Enter",
        [GameButton.Select] = "Backspace",
    };
}