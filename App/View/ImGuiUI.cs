using ImGuiNET;

namespace App.View;

/// <summary>
/// Container for the menu UI elements drawn by ImGui.
/// </summary>
public class ImGuiUI
{
    public int? PendingScale         { get; private set; }
    public bool ToggleVSyncRequested { get; private set; }
    public bool ExitRequested        { get; private set; }

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

        if (ImGui.BeginMenu("View"))
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

            ImGui.EndMenu(/* View */);
        }

        ImGui.EndMainMenuBar();
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
}