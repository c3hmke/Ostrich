using Emulation;
using Silk.NET.Maths;

namespace App.View;

/// <summary>
/// Configuration options for the Ostrich 'App' window.
/// </summary>
public class WindowConfig
{

    /// <summary>
    /// Reserved height to draw the menu 'above' the content+padding area.
    /// </summary>
    public int  MenuBarReservePx { get; set; } = 18;
    
    /// <summary>
    /// Pixels used for padding around the content area. This padding 'emulates' the natural blank
    /// area around the real hardware screens.
    /// </summary>
    public int  PaddingPx        { get; set; } = 16;
    
    /// <summary>
    /// Whether VSync is enabled. This a Silk.NET toggle being exposed to the user. VSync on will
    /// draw frames in step with monitor refresh which is smoother but 'slower'. off draws frames
    /// as soon as they are available with reduces input latency but can cause screen tearing.
    /// </summary>
    public bool VSyncEnabled     { get; set; } = true;

    /// <summary>
    /// Integer scaling factor for the content area
    /// </summary>
    public int Scale
    {
        get;
        set
        {
            if (value <= 0 || value == field)
                return;

            field = value;
        }
    } = 3;

    /// <summary>
    /// Size of the full window, including the menu reserve and padding around the content area.
    /// </summary>
    public Vector2D<int> GetWindowSize(IVideoSource screen) => new (
        (screen.Width * Scale) + (PaddingPx * 2),
        (screen.Height * Scale) + (PaddingPx * 2) + MenuBarReservePx);

    /// <summary>
    /// Content area in framebuffer coords (bottom-left origin as expected by glViewport/glScissor).
    /// </summary>
    public (int x, int y, uint w, uint h) GetContentArea(Vector2D<int> framebufferSize) => (
        x: PaddingPx,
        y: PaddingPx, 
        w: (uint) Math.Max(0, framebufferSize.X - (PaddingPx * 2)), 
        h: (uint) Math.Max(0, framebufferSize.Y - (PaddingPx * 2) - MenuBarReservePx));
}