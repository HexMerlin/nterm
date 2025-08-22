using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using CoreColor = SemanticTokens.Core.Color;
using SemanticTokens.Sixel.Encoder;

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
    /// Encode Image stream to Sixel string
    /// </summary>
    /// <param name="stream">Image stream</param>
    /// <param name="size">Image size (for scaling), or null</param>
    /// <param name="transp">Transparency enum</param>
    /// <param name="frame"><see cref="SixLabors.ImageSharp.ImageFrame"/> index, 0=first/only frame, -1=choose best</param>
    /// <returns>Sixel string</returns>
    public static ReadOnlySpan<char> Encode(Stream stream,
                                            Size? size = null,
                                            Transparency transp = Transparency.Default,
                                            int frame = -1)
    {
        // First load image without any target size to see original dimensions
        using var originalImg = Image.Load<Rgba32>(stream);
        Console.WriteLine($"[DEBUG] Original source image dimensions: {originalImg.Width}x{originalImg.Height}");
        
        // Reset stream position for actual processing
        stream.Position = 0;
        
        DecoderOptions opt = new();
        if (size?.Width > 0 && size?.Height > 0)
        {
            Console.WriteLine($"[DEBUG] Target size specified: {size?.Width}x{size?.Height}");
            opt = new()
            {
                TargetSize = new(size?.Width ?? 1, size?.Height ?? 1),
            };
        }
        else
        {
            Console.WriteLine("[DEBUG] No target size specified - using original dimensions");
        }
        
        using var img = Image.Load<Rgba32>(opt, stream);
        Console.WriteLine($"[DEBUG] Image loaded with target processing: {img.Width}x{img.Height}");
        return Encode(img, size, transp, frame);
    }
    /// <summary>
    /// Encode <see cref="SixLabors.ImageSharp.Image"/> to Sixel string
    /// </summary>
    /// <param name="img">Image data</param>
    /// <inheritdoc cref="Encode"/>
    public static ReadOnlySpan<char> Encode(Image<Rgba32> img,
                                            Size? size = null,
                                            Transparency transp = Transparency.Default,
                                            int frame = -1)
    {
        Console.WriteLine($"[DEBUG] ===== ENCODE PROCESSING START =====");
        Console.WriteLine($"[DEBUG] Input image dimensions: {img.Width}x{img.Height}");
        Console.WriteLine($"[DEBUG] Requested target size: {(size?.Width ?? -1)}x{(size?.Height ?? -1)}");
        
        int canvasWidth = -1, canvasHeight = -1;
        if (size?.Width < 1 && size?.Height > 0)
        {
            // Keep aspect ratio
            canvasHeight = size?.Height ?? 1;
            canvasWidth = canvasHeight * img.Width / img.Height;
            Console.WriteLine($"[DEBUG] Aspect ratio mode (height specified): target height {canvasHeight} -> calculated width {canvasWidth}");
        }
        else if (size?.Height < 1 && size?.Width > 0)
        {
            // Keep aspect ratio
            canvasWidth = size?.Width ?? 1;
            canvasHeight = canvasWidth * img.Height / img.Width;
            Console.WriteLine($"[DEBUG] Aspect ratio mode (width specified): target width {canvasWidth} -> calculated height {canvasHeight}");
        }
        else if (size?.Height > 0 && size?.Width > 0)
        {
            canvasWidth = size?.Width ?? 1;
            canvasHeight = size?.Height ?? 1;
            Console.WriteLine($"[DEBUG] Explicit size mode: {canvasWidth}x{canvasHeight}");
        }

        // TODO: Use maximum size based on size of terminal window?
        if (canvasWidth < 1)
        {
            canvasWidth = img.Width;
            Console.WriteLine($"[DEBUG] Canvas width defaulted to original: {canvasWidth}");
        }
        if (canvasHeight < 1)
        {
            canvasHeight = img.Height;
            Console.WriteLine($"[DEBUG] Canvas height defaulted to original: {canvasHeight}");
        }
        
        Console.WriteLine($"[DEBUG] Final canvas size calculated: {canvasWidth}x{canvasHeight}");

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
                        img = img.Frames.ExportFrame(GetBestFrame(img, new(canvasWidth, canvasHeight)));
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

        if (canvasWidth > 1 && canvasHeight > 1 && (img.Width != canvasWidth || img.Height != canvasHeight))
        {
            Console.WriteLine($"[DEBUG] ===== RESIZE OPERATION =====");
            Console.WriteLine($"[DEBUG] Before resize: {img.Width}x{img.Height}");
            Console.WriteLine($"[DEBUG] Target resize to: {canvasWidth}x{canvasHeight}");
            Console.WriteLine($"[DEBUG] Aspect ratio change: {(double)img.Width/img.Height:F3} -> {(double)canvasWidth/canvasHeight:F3}");
            
            // Force exact dimensions without aspect ratio preservation
            img.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(canvasWidth, canvasHeight),
                Mode = ResizeMode.Stretch  // Force exact dimensions, ignore aspect ratio
            }));
            
            Console.WriteLine($"[DEBUG] After resize: {img.Width}x{img.Height}");
            Console.WriteLine($"[DEBUG] ===== RESIZE COMPLETE =====");
        }
        else
        {
            Console.WriteLine($"[DEBUG] No resize needed - dimensions already match or invalid canvas size");
        }

        // 減色処理
        // Color Reduction
        img.Mutate(x =>
        {
            x.Quantize(KnownQuantizers.Wu);
        });

        var imageFrame = img.Frames.RootFrame;

        // Debug: Check actual frame dimensions after processing
        Console.WriteLine($"[DEBUG] ===== FINAL PROCESSING =====");
        Console.WriteLine($"[DEBUG] Expected canvas: {canvasWidth}x{canvasHeight}");
        Console.WriteLine($"[DEBUG] Actual frame: {imageFrame.Width}x{imageFrame.Height}");
        Console.WriteLine($"[DEBUG] Frame aspect ratio: {(double)imageFrame.Width/imageFrame.Height:F3}");

        // Building a color palette
        ReadOnlySpan<CoreColor> colorPalette = GetColorPalette(imageFrame, transp, tc, bg);
        Size frameSize = new(imageFrame.Width, imageFrame.Height);  // Use actual frame dimensions
        
        Console.WriteLine($"[DEBUG] Frame size for encoding: {frameSize.Width}x{frameSize.Height}");
        Console.WriteLine($"[DEBUG] Color palette size: {colorPalette.Length} colors");
        Console.WriteLine($"[DEBUG] ===== STARTING SIXEL ENCODING =====");

        return EncodeFrame(imageFrame, colorPalette, frameSize, transp, tc, bg);
    }

    /// <summary>
    /// Encode <see cref="ImageFrame"/> to Sixel string
    /// </summary>
    /// <param name="frame">a frame part of Image data</param>
    /// <param name="colorPalette">Color palette for Sixel</param>
    /// <param name="frameSize">size of the frame</param>
    /// <param name="tc">Transparent <see cref="Color"/> set for the image</param>
    /// <param name="bg">Background <see cref="Color"/> set for the image</param>
    /// <inheritdoc cref="Encode(Image{Rgba32}, Size?, Transparency, int)"/>
    public static string EncodeFrame(ImageFrame<Rgba32> frame,
                                     ReadOnlySpan<CoreColor> colorPalette,
                                     Size frameSize,
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
        for (var i = 0; i < colorPaletteLength; i++)
        {
            // DECGCI (#): Graphics Color Introducer
            var colorValue = colorPalette[i].ToSixelPalette();
            sb.Append($"#{i};2;{colorValue}".AsSpan());
        }
    
        var buffer = new byte[canvasWidth * colorPaletteLength];
        // Flag to indicate whether there is a color palette to display
        var cset = new bool[colorPaletteLength];
        var ch0 = Constants.specialChNr;
        for (var (z, y) = (0, 0); z < (canvasHeight + 5) / 6; z++, y = z * 6)
        {
            if (z > 0)
            {
                // DECGNL (-): Graphics Next Line
                sb.Append('-');
            }

            for (var p = 0; p < 6 && y < canvasHeight; p++, y++)
            {
                for (var x = 0; x < canvasWidth; x++)
                {
                    var rgba = frame[x, y];
                    if (transp == Transparency.TopLeft && rgba == frame[0, 0])
                        continue;
                    if (transp == Transparency.Background && rgba == bg)
                        continue;

                    CoreColor sixelColor = rgba.ToSixelColor(transp, tc, bg);
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
            for (var n = 0; n < colorPaletteLength; n++)
            {
                if (!cset[n]) continue;

                cset[n] = false;
                if (ch0 == Constants.specialChCr && !first)
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
                            sb.Append("!255").Append(sixelChar);
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
                        sb.Append("!255").Append(sixelChar);
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
                ch0 = Constants.specialChCr;
            }
        }
        sb.Append(Constants.ESC + Constants.End);
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
    public static CoreColor[] GetColorPalette(ImageFrame<Rgba32> frame,
                                               Transparency transp = Transparency.Default,
                                               Rgba32? tc = null,
                                               Rgba32? bg = null)
    {
        var palette = new HashSet<CoreColor>();
        frame.ProcessPixelRows(accessor =>
        {
            var pixcelHash = new HashSet<Rgba32>();
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    if (pixcelHash.Add(row[x]))
                    {
                        CoreColor c = row[x].ToSixelColor(transp, tc, bg);
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
    /// Determine best-sized ImageFrame (for CUR and ICO)
    /// </summary>
    /// <param name="stream">Image Stream</param>
    /// <param name="size">Size, null=largest ImageFrame</param>
    /// <returns>int index of best ImageFrame</returns>
    public static int GetBestFrame(Stream stream, Size? size)
    {
        return GetBestFrame(Image.Load<Rgba32>(new(), stream), size);
    }
    /// <param name="img">Image data</param>
    /// <inheritdoc cref="GetBestFrame"></inheritdoc>
    public static int GetBestFrame(Image<Rgba32> img, Size? size)
    {
        size ??= new(-1, -1);
        int? sizeDim;
        int bestFrame = 0, bestDim = 0, maxBpp = 0, i = 0;
        if (size?.Width > size?.Height)
            sizeDim = size?.Width;
        else
            sizeDim = size?.Height;
        foreach (var frame in img.Frames)
        {
            var meta = frame.Metadata.GetIcoMetadata();
            DebugPrint("  " + i + ":" + meta.EncodingWidth + "x" + meta.EncodingHeight + "x" + (int)meta.BmpBitsPerPixel + "b", lf: true);
            if ((int)meta.BmpBitsPerPixel >= maxBpp)
            {
                maxBpp = (int)meta.BmpBitsPerPixel;
                int w = meta.EncodingWidth;
                //int h = meta.EncodingHeight;
                if (w == 0) // oddly, 0 means 256
                    w = 256;
                if ((bestDim <= 0) ||
                    ((sizeDim is null || sizeDim <= 0) && w > bestDim) ||
                    (sizeDim is not null && sizeDim > 0 && w >= sizeDim && w < bestDim) ||
                    (w > bestDim))
                {
                    bestDim = w;
                    bestFrame = i;
                }
            }
            i++;
        }
        DebugPrint("Best ImageFrame: " + bestFrame, lf: true);
        return bestFrame;
    }
#endif
}
