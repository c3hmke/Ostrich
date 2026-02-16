// ReSharper disable AccessToDisposedClosure : Analyser cannot prove _window.Run() blocks, but it does. When disposing in
//                                             "correct context" SIGABRT happens on other resources resulting in dirty exit.

using System.Drawing;
using App.View;
using Emulation;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace App;

internal class Program
{
    /// Application window configurations
    private static readonly WindowConfig WindowCfg = new();
    private static int? _pendingScale; // New scale to be applied on change
    private static IWindow         _window = null!;
    
    /// Graphics configurations
    private static GL              _gl     = null!;
    private static IInputContext   _input  = null!;
    private static ImGuiController _imGui  = null!;

    /// Emulator core
    private static IEmulator       _emu = null!;
    
    private static void Main()
    {
        _emu = new GameBoy.Emulator();
        
        // Create a Silk.NET window
        var windowOptions = WindowOptions.Default;
        
        windowOptions.WindowBorder = WindowBorder.Fixed;
        windowOptions.VSync = WindowCfg.VSyncEnabled;
        windowOptions.Size  = WindowCfg.GetWindowSize(_emu.Screen);
        
        _window = Window.Create(windowOptions);
        
        // Load : Set up when window is loaded
        _window.Load += () =>
        {
            _gl    = GL.GetApi(_window);
            _input = _window.CreateInput();
            _imGui = new ImGuiController(_gl, _window, _input);
            
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            
            // Ensure viewport matches actual framebuffer size at start
            var fb = _window.FramebufferSize;
            _gl.Viewport(0, 0, (uint)fb.X, (uint)fb.Y);
            
            ApplyWindowCfg();
        };
        
        // Update : Changes to be made when window itself is modified
        _window.Update += delta =>
        {
            // Defer window resizes to avoid native re-entrancy / segfaults
            if (_pendingScale.HasValue && _pendingScale != WindowCfg.Scale)
            {
                WindowCfg.Scale = _pendingScale.Value;
                _pendingScale = null;
                
                ApplyWindowCfg();
            }
        }; 

        // Render : What to do when a frame is rendered
        _window.Render += delta =>
        {
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
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Exit")) _window.Close();
                   
                    ImGui.EndMenu(/*File*/);
                }
                
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.BeginMenu("Scale"))
                    {
                        ScaleItem(2);
                        ScaleItem(3);
                        ScaleItem(4);
                        ScaleItem(5);
                        
                        ImGui.EndMenu(/*Scale*/);
                    }

                    bool isVSyncEnabled = WindowCfg.VSyncEnabled;
                    if (ImGui.Checkbox("VSync", ref isVSyncEnabled))
                    {
                        _window.VSync = !_window.VSync;
                        WindowCfg.VSyncEnabled = _window.VSync;
                    }
                    
                    ImGui.EndMenu(/*View*/);
                }
                
                ImGui.EndMainMenuBar();
            }
            //ImGui.ShowDemoWindow(); // Built-in demo window

            _imGui.Render();
        };
        
        // Resize : Handle what happens when window size changes
        _window.FramebufferResize += size => _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);

        // Closing : Clean up on exit
        _window.Closing += () => _imGui.Dispose();

        _window.Run();          // Run runs the render loop and blocks until return
        _window.Dispose();      // At which point we can dispose the window.
    }
    
    private static void ScaleItem(int scale)
    {
        bool selected = WindowCfg.Scale == scale;       // Set selected scale in menu
        if (ImGui.MenuItem($"{scale}x", "", selected))  // Apply the scaling
            _pendingScale = scale;
    }
    
    private static void ApplyWindowCfg()
    {
        // Size based on emulator-reported resolution + current view config
        _window.Size = WindowCfg.GetWindowSize(_emu.Screen);

        // Recreate ImGui controller so it picks up the new framebuffer size and rebuilds device objects.
        _imGui.Dispose();
        _imGui = new ImGuiController(_gl, _window, _input);

        ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        // Keep window VSync aligned with config
        _window.VSync = WindowCfg.VSyncEnabled;
    }
    
    // /// <summary>
    // /// Retrieve the content area, this is where the emulator FrameBuffer will be displayed.
    // /// </summary>
    // private static (int x, int y, uint w, uint h) GetContentArea()
    // {
    //     var fb = _window.FramebufferSize;
    //     return (
    //         x: PaddingPx,
    //         y: PaddingPx,
    //         w: (uint)Math.Max(0, fb.X - PaddingPx * 2),
    //         h: (uint)Math.Max(0, fb.Y - PaddingPx * 2 - MenuBarReservePx)
    //     );
    // }
}