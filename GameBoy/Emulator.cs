using Emulation;
using GameBoy.Video;

namespace GameBoy;

public class Emulator : IEmulator
{
    public IVideoSource Screen { get; } = new GBVideoSource();
}