using App.Input;
using Emulation;
using ImGuiNET;
using NativeFileDialogSharp;
using Silk.NET.Input;

namespace App.View;

/// <summary>
/// Container for the menu UI elements drawn by ImGui.
/// </summary>
public class ImGuiUI
{
    /***********************************************************************************************************\
     *                                             MAIN MENU                                                   *
    \***********************************************************************************************************/
    public void DrawMainMenuBar(WindowConfig windowCfg)
    {
        if (!ImGui.BeginMainMenuBar())
            return;

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Open ROM..."))
            {
                _romLoadWindowOpen = true;
                ImGui.OpenPopup("Open ROM");
            }
            
            
            if (ImGui.MenuItem("Exit")) ExitRequested = true;

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
    
    /***********************************************************************************************************\
     *                                               ROM LOADING                                               *
    \***********************************************************************************************************/
    public bool    OpenROMRequested    { get; private set; }
    public string? PendingROMPath      { get;  private set; }
    private bool   _romLoadWindowOpen;
    private string _romPathBuffer = "";
    private string _startDir;
    
    public void DrawOpenRomModal()
    {
        if (_romLoadWindowOpen)
            ImGui.OpenPopup("Open ROM");

        if (ImGui.BeginPopupModal("Open ROM", ref _romLoadWindowOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.TextUnformatted("ROM Path:");
            ImGui.InputText("##rompath", ref _romPathBuffer, 4096);
            
            ImGui.SameLine();
            if (ImGui.Button("..."))
            {
                // If a rom was loaded, then make that the starting directory
                if (!string.IsNullOrWhiteSpace(_romPathBuffer) && File.Exists(_romPathBuffer))
                    _startDir = Path.GetDirectoryName(_romPathBuffer)!;
                else
                    _startDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                
                var result = Dialog.FileOpen("Game Boy ROMs: gb,gbc,gba", _startDir);
                if (result.IsOk && !string.IsNullOrWhiteSpace(result.Path))
                {
                    _romPathBuffer = result.Path;
                }
            }

            if (ImGui.Button("Open"))
            {
                PendingROMPath = _romPathBuffer.Trim();
                OpenROMRequested = true;

                _romLoadWindowOpen = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                _romLoadWindowOpen = false;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }
    
    /***********************************************************************************************************\
     *                                            CONTROL MAPPING                                              *
    \***********************************************************************************************************/
    public  bool ControlsWindowOpen        { get; private set; }
    public GameButton? PendingRebindButton { get; private set; }
    
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
                ConfigSaveRequested  = true;
                PendingRebindButton = null;
                keyboardInput.ClearLastKeyDown();
            }
        }

        ImGui.End();
    }
    
    /***********************************************************************************************************\
     *                                            GENERAL HELPERS                                              *
    \***********************************************************************************************************/
    // General update flags from the menu. These run OnUpdate.
    public bool ConfigSaveRequested  { get; private set; }
    public bool ExitRequested        { get; private set; }
    
    // Window controls
    public bool ToggleVSyncRequested { get; private set; }
    public int? PendingScale         { get; private set; }
    
    /// <summary> Function to clear all requests so the menu stops blocking. </summary>
    public void ResetFrameRequests()
    {
        PendingScale = null;
        PendingROMPath = null;
        
        OpenROMRequested = false;
        ToggleVSyncRequested = false;
        ConfigSaveRequested = false;
        ExitRequested = false;
    }

    /// <summary> Function to change the integer scaling on the window. </summary>
    private void ScaleItem(int scale, int currentScale)
    {
        bool selected = currentScale == scale;
        if (ImGui.MenuItem($"{scale}x", "", selected))
            PendingScale = scale;
    }
    
    /// <summary> Menu item used to rebind controls for the emulator. </summary>
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