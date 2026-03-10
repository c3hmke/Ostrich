namespace Emulation;

/// <summary>
/// Represents a concrete emulator instance for a specific system.
///
/// The frontend should interact with an emulator exclusively through
/// this interface. It exposes the system’s subsystems (such as video,
/// audio, and input).
///
/// An <see cref="IEmulator"/> implementation owns the full lifecycle
/// of the emulated machine, including initialization, execution,
/// and internal timing. The frontend must not assume any specific
/// hardware characteristics beyond what is exposed through the
/// provided interfaces.
/// </summary>
public interface IEmulator
{
    IVideoSource Screen      { get; }   // Interact with the screen.
    IInputSink   Input       { get; }   // Used to capture all user input.
    IInputState  InputState  { get; }   // Current state of user input.
    
    bool         IsROMLoaded { get; }   // Lifecycle hook for FE.

    /// <summary>
    /// Restore the machine state without requiring a new instance
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Load a ROM into the internal memory.
    /// </summary>
    void LoadROM(byte[] rom, string path);

    /// <summary>
    /// Advances the emulation until a complete video frame is ready.
    /// </summary>
    void StepFrame();
}