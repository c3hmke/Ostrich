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
    IVideoSource Screen { get; }
    IInputSink   Input  { get; }
    
    IInputState  InputState { get; }

    void LoadROM(byte[] rom, string path);
}