using Emulation;
using ImGuiNET;
using Silk.NET.Maths;

namespace App.View;

public class DebugWindow
{
    private ICPUState?   _state;
    private IInputState? _inputState;

    public void SetState(ICPUState? state) => _state = state;
    public void SetInputState(IInputState? inputState) => _inputState = inputState;
    
    public void Draw(Vector2D<int> framebufferSize, WindowConfig cfg)
    {
        int x = framebufferSize.X - cfg.PaddingPx - cfg.DebugPaneWidthPx;
        int topY = cfg.MenuBarReservePx + cfg.PaddingPx;
        int totalH = framebufferSize.Y - cfg.MenuBarReservePx - (cfg.PaddingPx * 2);

        const int inputPaneHeight = 64;
        const int verticalPaneGap = 8;

        int inputH = Math.Min(inputPaneHeight, Math.Max(56, totalH));
        int cpuY = topY + inputH + verticalPaneGap;
        int cpuH = Math.Max(56, totalH - inputH - verticalPaneGap);

        ImGui.SetNextWindowPos(new System.Numerics.Vector2(x, topY), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(cfg.DebugPaneWidthPx, inputH), ImGuiCond.Always);

        ImGui.Begin("Input", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

        ImGui.Text("Pressed:");

        bool any = false;
        foreach (GameButton button in Enum.GetValues<GameButton>())
        {
            if (!(_inputState?.IsPressed(button) ?? false))
                continue;

            ImGui.SameLine();
            ImGui.TextUnformatted(button.ToString());
            any = true;
        }

        if (!any)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted("(none)");
        }

        ImGui.End();

        ImGui.SetNextWindowPos(new System.Numerics.Vector2(x, cpuY), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(cfg.DebugPaneWidthPx, cpuH), ImGuiCond.Always);

        ImGui.Begin("CPU State", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

        if (_state is null)
        {
            ImGui.TextUnformatted("CPU unavailable.");
            ImGui.End();
            return;
        }

        ImGui.Text($"PC: 0x{_state.PC:X4}");
        ImGui.Text($"SP: 0x{_state.SP:X4}");
        ImGui.Text($"Cycles: {_state.CycleCount}");
        ImGui.Text($"Halted: {_state.Halted}");
        ImGui.Text($"Stopped: {_state.Stopped}");

        ImGui.Separator();
        ImGui.Text("Registers");

        ImGui.Text($"AF: 0x{(_state.A << 8 | _state.F):X4}");
        ImGui.Text($"BC: 0x{(_state.B << 8 | _state.C):X4}");
        ImGui.Text($"DE: 0x{(_state.D << 8 | _state.E):X4}");
        ImGui.Text($"HL: 0x{(_state.H << 8 | _state.L):X4}");

        ImGui.Separator();
        ImGui.Text($"Individual: A=0x{_state.A:X2} B=0x{_state.B:X2} C=0x{_state.C:X2}");
        ImGui.Text($"             D=0x{_state.D:X2} E=0x{_state.E:X2} F=0x{_state.F:X2}");
        ImGui.Text($"             H=0x{_state.H:X2} L=0x{_state.L:X2}");

        ImGui.Separator();
        ImGui.Text("Flags");
        ImGui.Text($"Z: {(_state.FlagZ ? 1 : 0)}  N: {(_state.FlagN ? 1 : 0)}  H: {(_state.FlagH ? 1 : 0)}  C: {(_state.FlagC ? 1 : 0)}");

        ImGui.End();
    }
}
