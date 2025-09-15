#if IMAGESHARP4 // ImageSharp v4.0 adds support for CUR and ICO files
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Cur;
using SixLabors.ImageSharp.PixelFormats;

namespace Nterm.Sixel.Encoder;

public class CurEncoder : SixelEncoder
{
    public CurEncoder(Image<Rgba32> img) : base(img, "CUR")
    {
        Metadata = img.Metadata.GetCurMetadata();
        // Note: CUR format supports images up to 256x256 pixels
        // For larger images, the frame selection logic will choose appropriate size
    }

    public CurMetadata Metadata { get; }

    public override bool ReverseTransparencyOnAnimate => false;

    /// <summary>
    /// Encode a <see cref="ImageFrame"/> into a Sixel string.
    /// The image frame is choosed automaticaly. (typically the root frame)j
    /// </summary>
    public override string Encode()
    {
        return EncodeFrame(GetBestFrame());
    }

    protected override string EncodeFrameInternal(ImageFrame<Rgba32> frame)
    {
        // Quantize the image if not already done
        // and get the color palette for the frame
        if (!Quantized)
            Quantize();
        // Get width and height of the frame metadata
        // The CUR format supports images up to 256 x 256 pixels
        var metadata = frame.Metadata.GetIcoMetadata();
        var size = new Size(metadata.EncodingWidth == 0 ? 256 : metadata.EncodingWidth,
                            metadata.EncodingHeight == 0 ? 256 : metadata.EncodingHeight);
        return SixelEncode.EncodeFrame(frame,
                                       GetColorPalette(frame),
                                       size,
                                       TransparencyMode,
                                       TransparentColor,
                                       BackgroundColor);
    }

    public override int GetFrameDelay(int frameIndex)
    {
        var delay = FrameDelays[Math.Min(frameIndex, FrameDelays.Length - 1)];
        return delay < 0 ? 500 : delay;
    }

    /// <summary>
    /// Determine best-quality ImageFrame (for CUR and ICO)
    /// </summary>
    /// <returns>int index of best ImageFrame</returns>
    public int GetBestFrame()
    {
        return SixelEncode.GetBestFrame(Image);
    }
}
#endif
