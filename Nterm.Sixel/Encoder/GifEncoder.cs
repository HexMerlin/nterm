using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

namespace Nterm.Sixel.Encoder;
public class GifEncoder : SixelEncoder
{
    public GifEncoder(Image<Rgba32> img) : base(img, "GIF")
    {
        Metadata = img.Metadata.GetGifMetadata();
        BackgroundColor = Metadata.GlobalColorTable?.Span[Metadata.BackgroundColorIndex].ToPixel<Rgba32>();
        // Gif format is already 256 colors, don't need to quantize
        Quantized = true;
    }

    public GifMetadata Metadata { get; }

    public override uint RepeatCount => Metadata.RepeatCount;

    /// <remarks>
    /// Gif format is already 256 colors, skip quantization.
    /// And detect transparency color from local color table.
    /// </remarks>
    /// <inheritdoc cref="SixelEncoder.EncodeFrameInternal(ImageFrame{Rgba32})"/>
    protected override string EncodeFrameInternal(ImageFrame<Rgba32> frame)
    {
        GifFrameMetadata meta = frame.Metadata.GetGifMetadata();
        Rgba32? bgColor = meta.LocalColorTable?.Span[meta.TransparencyIndex].ToPixel<Rgba32>() ?? BackgroundColor;
        return SixelEncode.EncodeFrame(frame,
                                 GetColorPalette(frame),
                                 CanvasSize,
                                 TransparencyMode,
                                 TransparentColor,
                                 bgColor);
    }

    public override int GetFrameDelay(int frameIndex)
    {
        int delay = FrameDelays[Math.Min(frameIndex, FrameDelays.Length - 1)];
        if (delay < 0)
        {
            ImageFrame<Rgba32> frame = Image.Frames[frameIndex];
            return frame.Metadata.GetGifMetadata().FrameDelay * 1000 / 100;
        }
        return delay;
    }
}
