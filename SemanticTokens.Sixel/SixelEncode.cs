using SemanticTokens.Sixel.Encoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text;
using Size = SemanticTokens.Core.Size;
using Constants = SemanticTokens.Core.Constants;

namespace SemanticTokens.Sixel;

public static class SixelEncode
{

    /// <summary>
    /// Create an encoder instance to convert <paramref name="image"/> to Sixel string
    /// </summary>
    /// <param name="image"></param>
    public static SixelEncoder CreateEncoder(Image<Rgba32> image)
    {
        var format = image.Metadata.DecodedImageFormat?.Name.ToUpperInvariant();
        return format switch
        {
            "GIF" => new GifEncoder(image),
            "PNG" => new PngEncoder(image),
            "WEBP" => new WebpEncoder(image),
            "TIFF" => new TiffEncoder(image),
#if IMAGESHARP4 // ImageSharp v4.0 adds support for CUR and ICO files
            "ICO" => new IcoEncoder(image),
            "CUR" => new CurEncoder(image),
#endif
            _ => new SixelEncoder(image, format),
        };
    }
    /// <summary>
    /// Create an encoder instance to convert the file <paramref name="path"/> to a Sixel string
    /// </summary>
    /// <param name="path">Image file path</param>
    public static SixelEncoder CreateEncoder(string path) => File.Exists(path)
            ? CreateEncoder(Image.Load<Rgba32>(path))
            : throw new FileNotFoundException("File not found", path);

    /// <summary>
    /// Create an encoder instance to convert the file <paramref name="path"/> to a Sixel string
    /// </summary>
    /// <param name="stream">a stream for image</param>
    public static SixelEncoder CreateEncoder(Stream stream) => CreateEncoder(Image.Load<Rgba32>(stream));

    /// <summary>
    /// Encode Image stream to Sixel string using original image dimensions.
    /// </summary>
    /// <param name="stream">Image stream</param>
    /// <param name="transp">Transparency enum</param>
    /// <param name="frame"><see cref="SixLabors.ImageSharp.ImageFrame"/> index, 0=first/only frame, -1=choose best</param>
    /// <returns>Sixel string</returns>
    public static ReadOnlySpan<char> Encode(Stream stream,
                                            Transparency transp = Transparency.Default,
                                            int frame = -1)
    {
        using var img = Image.Load<Rgba32>(stream);
        return Encode(img, transp, frame);
    }
    /// <summary>
    /// Encode <see cref="SixLabors.ImageSharp.Image"/> to Sixel string using original image dimensions.
    /// </summary>
    /// <param name="img">Image data</param>
    /// <param name="transp">Transparency enum</param>
    /// <param name="frame"><see cref="SixLabors.ImageSharp.ImageFrame"/> index, 0=first/only frame, -1=choose best</param>
    /// <returns>Sixel string</returns>
    public static ReadOnlySpan<char> Encode(Image<Rgba32> img,
                                            Transparency transp = Transparency.Default,
                                            int frame = -1)
    {
        // Use original image dimensions - no resizing, no aspect ratio destruction
        int canvasWidth = img.Width;
        int canvasHeight = img.Height;

        var meta = img.Metadata;
        Rgba32? bg = null, tc = null;
        var format = meta.DecodedImageFormat?.Name.ToUpperInvariant();
        int frameCount = img.Frames.Count;

        // Detect images with backgrounds that might be made transparent
        switch (format)
        {
            case "GIF":
                var gifMeta = meta.GetGifMetadata();
                bg = gifMeta.GlobalColorTable?.Span[gifMeta.BackgroundColorIndex].ToPixel<Rgba32>();
                break;
            case "PNG":
                var pngMeta = meta.GetPngMetadata();
                if (pngMeta.ColorType == SixLabors.ImageSharp.Formats.Png.PngColorType.Palette)
                    tc = pngMeta.TransparentColor?.ToPixel<Rgba32>();
                break;
            case "WEBP":
                bg = meta.GetWebpMetadata().BackgroundColor.ToPixel<Rgba32>();
                break;
        }

        // Detect images with multiple frames
        switch (format)
        {
#if IMAGESHARP4 // ImageSharp v4.0 adds support for CUR and ICO files
            case "CUR":
            case "ICO":
                if (frameCount > 1)
                {
                    if (frame > -1)
                    {
                        if (frame < frameCount)
                            img = img.Frames.ExportFrame(frame);
                        else
                            img = img.Frames.ExportFrame(frame % frameCount);
                    }
                    else
                        img = img.Frames.ExportFrame(GetBestFrame(img));
                }
                break;
#endif
            case "GIF":
            case "PNG":  // APNG animations supported
            case "TIFF": // Can contain multiple pages
            case "WEBP":
                if (frameCount > 1 && frame > -1)
                    if (frame < frameCount)
                        img = img.Frames.ExportFrame(frame);
                    else
                        img = img.Frames.ExportFrame(frame % frameCount);
                break;
        }

        // No resizing - preserve original image dimensions and aspect ratio

        // Color Reduction
        img.Mutate(x =>
        {
            x.Quantize(KnownQuantizers.Wu);
        });

        var imageFrame = img.Frames.RootFrame;

        // Building a color palette
        ReadOnlySpan<SemanticTokens.Core.Color> colorPalette = GetColorPalette(imageFrame, transp, tc, bg);
        Size frameSize = new(imageFrame.Width, imageFrame.Height);  // Use actual frame dimensions

        return EncodeFrame(imageFrame, colorPalette, frameSize, transp, tc, bg);
    }

    /// <summary>
    /// Encode <see cref="ImageFrame"/> to Sixel string
    /// </summary>
    /// <param name="frame">a frame part of Image data</param>
    /// <param name="colorPalette">Color palette for Sixel</param>
    /// <param name="frameSize">size of the frame</param>
    /// <param name="tc">Transparent <see cref="SixLabors.ImageSharp.PixelFormats.Rgba32"/> set for the image</param>
    /// <param name="bg">Background <see cref="SixLabors.ImageSharp.PixelFormats.Rgba32"/> set for the image</param>
    /// <inheritdoc cref="Encode(Image{Rgba32}, Size?, Transparency, int)"/>
    public static string EncodeFrame(ImageFrame<Rgba32> frame,
                                     ReadOnlySpan<SemanticTokens.Core.Color> colorPalette,
                                     SemanticTokens.Core.Size frameSize,
                                     Transparency transp = Transparency.Default,
                                     Rgba32? tc = null,
                                     Rgba32? bg = null)
    {
        int canvasWidth = frameSize.Width;
        int canvasHeight = frameSize.Height;

        //
        // https://github.com/mattn/go-sixel/blob/master/sixel.go の丸パクリです！！
        //                                                        It's a complete rip-off!!
        //
        var sb = new StringBuilder();
        // DECSIXEL Introducer(\033P0;0;8q) + DECGRA ("1;1): Set Raster Attributes

        sb.Append(Constants.ESC + Constants.SixelStart)
          .Append($";{canvasWidth};{canvasHeight}".AsSpan());

        int colorPaletteLength = colorPalette.Length;
        for (int i = 0; i < colorPaletteLength; i++)
        {
            // DECGCI (#): Graphics Color Introducer
            ReadOnlySpan<char> colorValue = colorPalette[i].ToSixelPalette();
            sb.Append($"{Constants.ColorIntroducer}{i};2;{colorValue}".AsSpan());
        }
    
        byte[] buffer = new byte[canvasWidth * colorPaletteLength];
        // Flag to indicate whether there is a color palette to display
        bool[] cset = new bool[colorPaletteLength];
        byte ch0 = Constants.SpecialChNr;
        for (int z = 0, y = 0; z < (canvasHeight + 5) / 6; z++, y = z * 6)
        {
            if (z > 0)
            {
                // DECGNL (-): Graphics Next Line
                sb.Append(Constants.SixelNextLine);
            }

            for (int p = 0; p < 6 && y < canvasHeight; p++, y++)
            {
                for (int x = 0; x < canvasWidth; x++)
                {
                    Rgba32 rgba = frame[x, y];
                    if (transp == Transparency.TopLeft && rgba == frame[0, 0])
                        continue;
                    if (transp == Transparency.Background && rgba == bg)
                        continue;

                    SemanticTokens.Core.Color sixelColor = rgba.ToSixelColor(transp, tc, bg);
                    if (sixelColor.A == 0)
                        continue;
                    int idx = colorPalette.IndexOf(sixelColor);
                    if (idx < 0)
                        continue;

                    cset[idx] = true;
                    buffer[(canvasWidth * idx) + x] |= (byte)(1 << p);
                }
            }
            bool first = true;
            for (int n = 0; n < colorPaletteLength; n++)
            {
                if (!cset[n]) continue;

                cset[n] = false;
                if (ch0 == Constants.SpecialChCr && !first)
                {
                    // DECGCR ($): Graphics Carriage Return
                    sb.Append('$');
                }
                first = false;

                sb.Append($"#{n}".AsSpan());
                var cnt = 0;
                byte ch;
                int bufIndex;
                char sixelChar;
                for (var x = 0; x < canvasWidth; x++)
                {
                    // make sixel character from 6 pixels
                    bufIndex = (canvasWidth * n) + x;
                    ch = buffer[bufIndex];
                    buffer[bufIndex] = 0;
                    if (ch0 < 0x40 && ch != ch0)
                    {
                        sixelChar = (char)(63 + ch0);
                        for (; cnt > 255; cnt -= 255)
                        {
                            sb.Append(Constants.RepeatIntroducer).Append("255").Append(sixelChar);
                        }
                        switch (cnt)
                        {
                            case 1:
                                sb.Append(sixelChar);
                                break;
                            case 2:
                                sb.Append([sixelChar, sixelChar]);
                                break;
                            case 3:
                                sb.Append([sixelChar, sixelChar, sixelChar]);
                                break;
                            case > 0:
                                sb.Append($"!{cnt}".AsSpan()).Append(sixelChar);
                                break;
                        }
                        cnt = 0;
                    }
                    ch0 = ch;
                    cnt++;
                }
                if (ch0 != 0)
                {
                    sixelChar = (char)(63 + ch0);
                    for (; cnt > 255; cnt -= 255)
                    {
                        sb.Append(Constants.RepeatIntroducer).Append("255").Append(sixelChar);
                    }
                    switch (cnt)
                    {
                        case 1:
                            sb.Append(sixelChar);
                            break;
                        case 2:
                            sb.Append([sixelChar, sixelChar]);
                            break;
                        case 3:
                            sb.Append([sixelChar, sixelChar, sixelChar]);
                            break;
                        case > 0:
                            sb.Append($"!{cnt}".AsSpan()).Append(sixelChar);
                            break;
                    }
                }
                ch0 = Constants.SpecialChCr;
            }
        }
        sb.Append(Constants.ESC + Constants.SixelEnd);
        return sb.ToString();
    }

    /// <summary>
    /// Get Image format string from <see cref="SixLabors.ImageSharp.Metadata.ImageMetadata"/>
    /// </summary>
    /// <param name="stream">Image Stream</param>
    /// <returns>Format name string, e.g. "PNG"</returns>
    public static string GetFormat(Stream stream)
    {
        return GetFormat(Image.Load<Rgba32>(new(), stream));
    }
    /// <param name="img">Image data</param>
    /// <inheritdoc cref="GetFormat"></inheritdoc>
    public static string GetFormat(Image<Rgba32> img) => img.Metadata.DecodedImageFormat?.Name ?? "Unknown";

    /// <summary>
    /// Get suggested number of times to repeat animation (GIF, APNG, or WEBP)
    /// </summary>
    /// <param name="stream">Image Stream</param>
    /// <returns>int number of repeats, 0=continuous, -1=not applicable</returns>
    public static int GetRepeatCount(Stream stream) => GetRepeatCount(Image.Load<Rgba32>(new(), stream));
    /// <param name="img">Image data</param>
    /// <inheritdoc cref="GetRepeatCount"></inheritdoc>
    public static int GetRepeatCount(Image<Rgba32> img)
    {
        var meta = img.Metadata;
        switch (meta.DecodedImageFormat?.Name.ToUpperInvariant())
        {
            case "GIF":
                return (int?)meta.GetGifMetadata().RepeatCount ?? -1;
            case "PNG":
                return (int?)meta.GetPngMetadata().RepeatCount ?? -1;
            case "WEBP":
                return (int?)meta.GetWebpMetadata().RepeatCount ?? -1;
        }
        return -1;
    }

    /// <summary>
    /// Get number of ImageFrames (GIF, APNG, or WEBP for animation frames; TIFF for multiple pages; CUR, ICO for various sizes)
    /// </summary>
    /// <param name="stream">Image Stream</param>
    /// <returns>int number of ImageFrames</returns>
    public static int GetNumFrames(Stream stream) => GetNumFrames(Image.Load<Rgba32>(new(), stream));
    /// <param name="img">SixLabors.ImageSharp.Image data</param>
    /// <inheritdoc cref="GetNumFrames"></inheritdoc>
    public static int GetNumFrames(Image<Rgba32> img) => img.Frames.Count;

    /// <summary>
    /// Build color palette for Sixel
    /// </summary>
    public static SemanticTokens.Core.Color[] GetColorPalette(ImageFrame<Rgba32> frame,
                                               Transparency transp = Transparency.Default,
                                               Rgba32? tc = null,
                                               Rgba32? bg = null)
    {
        HashSet<SemanticTokens.Core.Color> palette = new();
        frame.ProcessPixelRows(accessor =>
        {
            HashSet<Rgba32> pixcelHash = new();
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    if (pixcelHash.Add(row[x]))
                    {
                        SemanticTokens.Core.Color c = row[x].ToSixelColor(transp, tc, bg);
                        if (c.A == 0)
                            continue;
                        palette.Add(c);
                    }
                }
            }
        });
        return [.. palette];
    }

#if IMAGESHARP4 // ImageSharp v4.0 adds support for CUR and ICO files
    /// <summary>
    /// Determine best ImageFrame with highest quality (for CUR and ICO).
    /// </summary>
    /// <param name="stream">Image Stream</param>
    /// <returns>int index of best ImageFrame</returns>
    public static int GetBestFrame(Stream stream)
    {
        return GetBestFrame(Image.Load<Rgba32>(new(), stream));
    }
    
    /// <summary>
    /// Determine best ImageFrame with highest quality (for CUR and ICO).
    /// </summary>
    /// <param name="img">Image data</param>
    /// <returns>int index of best ImageFrame</returns>
    public static int GetBestFrame(Image<Rgba32> img)
    {
        int bestFrame = 0, bestDim = 0, maxBpp = 0, i = 0;
        
        foreach (var frame in img.Frames)
        {
            var meta = frame.Metadata.GetIcoMetadata();
            if ((int)meta.BmpBitsPerPixel >= maxBpp)
            {
                maxBpp = (int)meta.BmpBitsPerPixel;
                int w = meta.EncodingWidth;
                if (w == 0) // oddly, 0 means 256
                    w = 256;
                    
                // Choose largest dimension with highest bit depth
                if (w > bestDim)
                {
                    bestDim = w;
                    bestFrame = i;
                }
            }
            i++;
        }
        return bestFrame;
    }
#endif
}
