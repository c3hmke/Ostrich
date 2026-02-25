// ReSharper disable AccessToDisposedClosure : Analyser cannot prove _window.Run() blocks, but it does. When disposing in
//                                             "correct context" SIGABRT happens on other resources resulting in dirty exit.

using System.Drawing;
using App.Config;
using App.Input;
using App.View;
using Emulation;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace App;

internal class Program
{
    /// Application window configurations
    private static readonly WindowConfig  WindowCfg = new();
    private static readonly ImGuiUI       UI        = new();
    private static InputBindings          _bindings = null!;
    private static AppConfig              _cfg      = null!;
    private static KeyboardInput          _input    = null!;
    private static IWindow                _window   = null!;
    
    private static int? _pendingScale; // New scale to be applied on change
    
    /// Graphics configurations
    private static GL              _gl        = null!;
    private static IInputContext   _inputCtx  = null!;
    private static ImGuiController _imGui     = null!;

    /// Emulator core
    private static IEmulator       _emu = null!;
    
    private static void Main()
    {
        // Load app configurations
        _cfg = ConfigStore.Load();

        WindowCfg.Scale        = _cfg.Window.Scale;
        WindowCfg.VSyncEnabled = _cfg.Window.VSyncEnabled;

        _bindings = new InputBindings(LoadBindings(_cfg));
        
        // Create an emulator instance
        _emu = new GameBoy.Emulator();
        
        // Create a Silk.NET window
        var windowOptions = WindowOptions.Default;
        
        windowOptions.WindowBorder = WindowBorder.Fixed;
        windowOptions.VSync = WindowCfg.VSyncEnabled;
        windowOptions.Size  = WindowCfg.GetWindowSize(_emu.Screen);
        
        _window = Window.Create(windowOptions);
        
        // Hook in the Window lifecycle functions:
        _window.Load    += OnLoad;       // Set up when window is loaded
        _window.Update  += OnUpdate;     // Changes to be made when window itself is modified
        _window.Render  += OnRender;     // What to do when a frame is rendered
        _window.Closing += OnClosing;    // Clean up on exit
        
        _window.FramebufferResize +=     // Handle what happens when window size changes
            (size) => _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);

        _window.Run();          // Run runs the render loop and blocks until return
        _window.Dispose();      // At which point we can dispose the window.
    }

    private static void OnLoad()
    {
        _inputCtx = _window.CreateInput();
        _input    = new KeyboardInput(_emu.Input, _bindings);

        _input.Attach(_inputCtx);

        _gl       = GL.GetApi(_window);
        _imGui    = new ImGuiController(_gl, _window, _inputCtx);
            
        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            
        // Ensure viewport matches actual framebuffer size at start
        var fb = _window.FramebufferSize;
        _gl.Viewport(0, 0, (uint)fb.X, (uint)fb.Y);
            
        ApplyWindowCfg();
    }

    private static void OnUpdate(double delta)
    {
        // Defer window resizes to avoid native re-entrancy / segfaults
        if (_pendingScale.HasValue && _pendingScale != WindowCfg.Scale)
        {
            WindowCfg.Scale = _pendingScale.Value;
            _pendingScale = null;
                
            ApplyWindowCfg();
            
            if (UI.ConfigSaveRequested) SaveAppConfig();
            // TODO: Apply deferred vsync toggle here too (keeps render side clean).
        }
    }

    private static void OnRender(double delta)
    {
        UI.ResetFrameRequests();
            
        // --- Window Clear ---
        _gl.ClearColor(Color.FromArgb(255, (int) (.45f * 255), (int) (.55f * 255), (int) (.60f * 255)));
        _gl.Clear((uint) ClearBufferMask.ColorBufferBit);
            
        // --- Content Area Setup ---
        var fb =  _window.FramebufferSize;
        (int cx, int cy, uint cw, uint ch) = WindowCfg.GetContentArea(fb);
            
        _gl.Viewport(cx, cy, cw, ch);               // Set the Viewport size
            
        _gl.Enable(GLEnum.ScissorTest);             // Hard clip so nothing can draw into the padding area.
        _gl.Scissor(cx, cy, cw, ch); 
            
        // !! TEMP !! visualize the content area
        _gl.ClearColor(0.15f, 0.15f, 0.15f, 1f);
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        // !! ---- !!
            
        _gl.Disable(GLEnum.ScissorTest);            // End to the ContentArea
        _gl.Viewport(0, 0, (uint)fb.X, (uint)fb.Y); // Restore so ImGui draws correctly
            
        // --- ImGui ---
        _imGui.Update((float) delta);               // Make sure ImGui is up-to-date
        UI.DrawMainMenuBar(WindowCfg);
        UI.DrawControlsWindow(_bindings, _input);
        
        // --------- Input test code ----------------
        ImGui.SetNextWindowBgAlpha(0.35f);
        ImGui.Begin("Input", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize);

        ImGui.Text("Pressed:");

        bool any = false;
        foreach (GameButton b in Enum.GetValues<GameButton>())
        {
            if (_emu.InputState.IsPressed(b))
            {
                ImGui.SameLine();
                ImGui.TextUnformatted(b.ToString());
                any = true;
            }
        }

        if (!any)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted("(none)");
        }

        ImGui.End();
        // --------- Input test code ----------------
            
        /// Handle UI Intents
        if (UI.ExitRequested) _window.Close();
        if (UI.PendingScale is int requestedScale) 
            _pendingScale = requestedScale;
        if (UI.ToggleVSyncRequested)
        {
            WindowCfg.VSyncEnabled = !WindowCfg.VSyncEnabled;
            _window.VSync = WindowCfg.VSyncEnabled;
        }

        _imGui.Render();
    }

    private static void OnClosing()
    {
        SaveAppConfig();
        _imGui.Dispose();
    }
    
    private static void SaveAppConfig()
    {
        _cfg.Window.Scale = WindowCfg.Scale;
        _cfg.Window.VSyncEnabled = WindowCfg.VSyncEnabled;

        _cfg.Input.ButtonToKey = _bindings.All.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

        ConfigStore.Save(_cfg);
    }
    
    private static void ApplyWindowCfg()
    {
        // Size based on emulator-reported resolution + current view config
        _window.Size = WindowCfg.GetWindowSize(_emu.Screen);

        // Recreate ImGui controller so it picks up the new framebuffer size and rebuilds device objects.
        _imGui.Dispose();
        _imGui = new ImGuiController(_gl, _window, _inputCtx);

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        // Keep window VSync aligned with config
        _window.VSync = WindowCfg.VSyncEnabled;
    }
    
    private static Dictionary<GameButton, Key> LoadBindings(AppConfig cfg)
    {
        var dict = new Dictionary<GameButton, Key>();

        foreach (var kv in cfg.Input.ButtonToKey)
        {
            if (Enum.TryParse<Key>(kv.Value, ignoreCase: true, out var key))
                dict[kv.Key] = key;
        }

        // Ensure required buttons exist (fallbacks)
        void Ensure(GameButton b, Key k) { if (!dict.ContainsKey(b)) dict[b] = k; }

        Ensure(GameButton.Up, Key.Up);
        Ensure(GameButton.Down, Key.Down);
        Ensure(GameButton.Left, Key.Left);
        Ensure(GameButton.Right, Key.Right);
        Ensure(GameButton.A, Key.Z);
        Ensure(GameButton.B, Key.X);
        Ensure(GameButton.Start, Key.Enter);
        Ensure(GameButton.Select, Key.Backspace);

        return dict;
    }
}