using App.Input;
using Emulation;
using ImGuiNET;
using Silk.NET.Input;

namespace App.View;

/// <summary>
/// Container for the menu UI elements drawn by ImGui.
/// </summary>
public class ImGuiUI
{
    public int? PendingScale         { get; private set; }
    public bool ToggleVSyncRequested { get; private set; }
    public bool ExitRequested        { get; private set; }
    
    // Controls UI state
    public bool ControlsWindowOpen { get; private set; }
    public GameButton? PendingRebindButton { get; private set; }

    public void DrawMainMenuBar(WindowConfig windowCfg)
    {
        if (!ImGui.BeginMainMenuBar())
            return;

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Exit"))
                ExitRequested = true;

            ImGui.EndMenu(/* File */);
        }

        if (ImGui.BeginMenu("Emulator"))
        {
            if (ImGui.BeginMenu("Scale"))
            {
                ScaleItem(2, windowCfg.Scale);
                ScaleItem(3, windowCfg.Scale);
                ScaleItem(4, windowCfg.Scale);
                ScaleItem(5, windowCfg.Scale);
                
                ImGui.EndMenu(/* Scale */);
            }

            bool vsync = windowCfg.VSyncEnabled;
            if (ImGui.Checkbox("VSync", ref vsync))
                ToggleVSyncRequested = true;
            
            ImGui.Separator();

            if (ImGui.MenuItem("Controls.."))
                ControlsWindowOpen = true;

            ImGui.EndMenu(/* Emulator */);
        }

        ImGui.EndMainMenuBar();
    }
    
    /// <summary>
    /// Draws the controls window and updates bindings when a rebind is completed.
    /// Call this every frame after DrawMainMenuBar.
    /// </summary>
    public void DrawControlsWindow(InputBindings bindings, KeyboardInput keyboardInput)
    {
        if (!ControlsWindowOpen)
            return;

        bool open = ControlsWindowOpen;
        ImGui.Begin("Controls", ref open, ImGuiWindowFlags.AlwaysAutoResize);
        ControlsWindowOpen = open;

        ImGui.TextUnformatted("Click Rebind, then press a key.");
        ImGui.Separator();

        DrawRebindRow(bindings, keyboardInput, GameButton.Up);
        DrawRebindRow(bindings, keyboardInput, GameButton.Down);
        DrawRebindRow(bindings, keyboardInput, GameButton.Left);
        DrawRebindRow(bindings, keyboardInput, GameButton.Right);

        ImGui.Separator();

        DrawRebindRow(bindings, keyboardInput, GameButton.A);
        DrawRebindRow(bindings, keyboardInput, GameButton.B);

        ImGui.Separator();

        DrawRebindRow(bindings, keyboardInput, GameButton.Start);
        DrawRebindRow(bindings, keyboardInput, GameButton.Select);

        if (PendingRebindButton is GameButton b)
        {
            ImGui.Separator();
            ImGui.TextUnformatted($"Waiting for key for: {b}");
            ImGui.SameLine();

            if (ImGui.SmallButton("Cancel"))
            {
                PendingRebindButton = null;
                keyboardInput.ClearLastKeyDown();
            }

            if (keyboardInput.LastKeyDown is Key k)
            {
                bindings.Rebind(b, k);
                PendingRebindButton = null;
                keyboardInput.ClearLastKeyDown();
            }
        }

        ImGui.End();
    }

    
    public void ResetFrameRequests()
    {
        PendingScale = null;
        ToggleVSyncRequested = false;
        ExitRequested = false;
    }

    private void ScaleItem(int scale, int currentScale)
    {
        bool selected = currentScale == scale;
        if (ImGui.MenuItem($"{scale}x", "", selected))
            PendingScale = scale;
    }
    
    private void DrawRebindRow(InputBindings bindings, KeyboardInput keyboardInput, GameButton button)
    {
        var key = bindings.GetKey(button);

        ImGui.TextUnformatted($"{button,-6} : {key}");
        ImGui.SameLine();

        bool isActive = PendingRebindButton == button;
        if (isActive)
        {
            ImGui.BeginDisabled();
            ImGui.Button("Press key...");
            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"Rebind##{button}"))
            {
                PendingRebindButton = button;
                keyboardInput.ClearLastKeyDown();
            }
        }
    }
}