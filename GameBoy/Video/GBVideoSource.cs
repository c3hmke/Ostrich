using Emulation;

namespace GameBoy.Video;

public class GBVideoSource : IVideoSource
{
    public int Width  { get; } = 144;
    public int Height { get; } = 160;
}