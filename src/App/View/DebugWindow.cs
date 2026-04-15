using Emulation;
using ImGuiNET;

namespace App.View;

public class DebugWindow
{
    private ICPUState? _state;

    public void SetState(ICPUState? state) => _state = state;
    
    public void Draw()
    {
        if (_state is null)
            return;

        ImGui.Begin("CPU Debug", ImGuiWindowFlags.AlwaysAutoResize);

        ImGui.Text($"PC: 0x{_state.PC:X4}");
        ImGui.Text($"SP: 0x{_state.SP:X4}");
        ImGui.Text($"Cycles: {_state.CycleCount}");
        ImGui.Text($"Halted: {_state.Halted}");

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