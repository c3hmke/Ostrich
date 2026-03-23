using Emulation;

namespace GameBoy.Video;

public class GBVideoSource : IVideoSource
{
    public int Height { get; } = 160;
    public int Width  { get; } = 144;
    
    public ulong FrameId { get; private set; }
    
    private readonly uint[] _frameBuffer = new uint[160 * 144];
    public ReadOnlySpan<uint> GetFrameBuffer()
        => _frameBuffer;

    public void Clear(uint color)
    {
        Array.Fill(_frameBuffer, color);
        FrameId += 1;
    }
}