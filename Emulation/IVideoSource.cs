namespace Emulation;

/// <summary>
/// Represents the video output surface of an emulator core.
///
/// This interface is presentation-agnostic:
/// it must not contain any windowing, rendering, or UI logic.
/// It only describes the shape and lifecycle of the video signal
/// produced by the emulator.
/// 
/// The reported <see cref="Width"/> and <see cref="Height"/> values
/// represent the exact pixel dimensions of the emulator’s framebuffer
/// before any scaling, padding, or post-processing.
/// </summary>
public interface IVideoSource
{
    /// <summary>
    /// Native width of the emulator framebuffer in pixels.
    /// </summary>
    int Width  { get; }
    
    /// <summary>
    /// Native height of the emulator framebuffer in pixels.
    /// </summary>
    int Height { get; }
}