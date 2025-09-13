using NTerm.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Size = NTerm.Core.Size;

namespace NTerm.Sixel.Encoder;

/// <summary>
/// Generic encoder to convert an image to Sixel strings
/// </summary>
public class SixelEncoder(Image<Rgba32> img, string? format) : IDisposable
{
    private bool disposedValue;

    public Image<Rgba32> Image { get; } = img;
    public string? Format { get; } = format;

    /// <summary>
    /// Image dimensions in pixels (read-only).
    /// </summary>
    public Size CanvasSize => ToSize(Image.Size);

    public Rgba32? BackgroundColor { get; init; }
    public Rgba32? TransparentColor { get; init; }
    public Transparency TransparencyMode { get; set; } = Transparency.Default;

    public int FrameCount => Image.Frames.Count;
    public virtual bool CanAnimate => Image.Frames.Count > 1;
    public virtual bool ReverseTransparencyOnAnimate => true;

    public virtual uint RepeatCount => 0;

    /// <summary>
    /// Delay milliseconds for each frame; anything less than 0 means use the default value for the image frame.
    /// </summary>
    protected int[] FrameDelays { get; set; } = [-1];

    private static Size ToSize(SixLabors.ImageSharp.Size size) => new(size.Width, size.Height);

    /// <summary>
    /// Flag if the quantization process has been performed for avoid duplicate processing
    /// </summary>
    public bool Quantized { get; protected set; }

    /// <summary>
    /// Quantize the image
    /// </summary>
    /// <param name="force">force even if already done</param>.
    /// <returns>This Encoder</returns>
    public SixelEncoder Quantize(bool force = false)
    {
        if (!force && Quantized)
            return this;

        Image.Mutate(static context =>
        {
            _ = context.Quantize(KnownQuantizers.Wu);
        });
        Quantized = true;
        return this;
    }

    /// <summary>
    /// Create the color palette for the <paramref name="frame"/>
    /// </summary>
    /// <param name="frame"></param>
    protected virtual ReadOnlySpan<Core.Color> GetColorPalette(ImageFrame<Rgba32> frame)
    {
        if (!Quantized)
            _ = Quantize();
        return SixelEncode.GetColorPalette(
            frame,
            TransparencyMode,
            TransparentColor,
            BackgroundColor
        );
    }

    /// <summary>
    /// Encode the <see cref="ImageFrame"/> into a Sixel string.
    /// </summary>
    /// <param name="frame"></param>
    /// <returns>Sixel string</returns>
    protected virtual string EncodeFrameInternal(ImageFrame<Rgba32> frame)
    {
        // Quantize the image if not already done
        // and get the color palette for the frame
        if (!Quantized)
            _ = Quantize();
        return SixelEncode.EncodeFrame(
            frame,
            GetColorPalette(frame),
            CanvasSize,
            TransparencyMode,
            TransparentColor,
            BackgroundColor
        );
    }

    /// <summary>
    /// Encode the <see cref="ImageFrame"/> into a Sixel string.
    /// </summary>
    /// <param name="frame"></param>
    /// <returns>Sixel string</returns>
    public string EncodeFrame(ImageFrame<Rgba32> frame) => EncodeFrameInternal(frame);

    /// <summary>
    /// Encode the <see cref="ImageFrame"/> at the <paramref name="frameIndex"/> into a Sixel string.
    /// </summary>
    /// <param name="frameIndex"></param>
    /// <inheritdoc cref="EncodeFrame(ImageFrame{Rgba32})"/>
    public string EncodeFrame(int frameIndex) =>
        EncodeFrameInternal(Image.Frames[frameIndex % FrameCount]);

    /// <summary>
    /// Encode a <see cref="ImageFrame"/> into a Sixel string.
    /// The image frame is choosed automaticaly. (typically the root frame)j
    /// </summary>
    /// <inheritdoc cref="EncodeFrame(ImageFrame{Rgba32})"/>
    public virtual string Encode() => EncodeFrame(Image.Frames.RootFrame);

    /// <summary>
    /// Get Sixel strings for each frames
    /// </summary>
    public IEnumerable<string> EncodeFrames()
    {
        ImageFrameCollection<Rgba32> frames = Image.Frames;
        for (int i = 0; i < frames.Count; i++)
        {
            yield return EncodeFrame(frames[i]);
        }
    }

    /// <summary>
    /// Set frame delays in milliseconds
    /// </summary>
    public void SetFrameDelays(params int[] delays) => FrameDelays = delays;

    /// <summary>
    /// Get frame delay in milliseconds
    /// </summary>
    /// <param name="frameIndex">Index of the image frames</param>
    /// <returns>Frame delay in milliseconds</returns>
    public virtual int GetFrameDelay(int frameIndex)
    {
        int delay = FrameDelays[Math.Min(frameIndex, FrameDelays.Length - 1)];
        return delay < 0 ? 100 : delay;
    }

    /// <summary>
    /// Get Sixel strings for each frames asynchrously
    /// </summary>
    /// <param name="overwriteRepeat">
    /// Number of repeat count.
    /// 0 means infinite; less than 0 means use the default count specified for the image. (default = <c>-1</c>)
    /// <param name="startFrame">
    /// Start index of frame to encode (default = <c>0</c>). Negative value is index from the last.
    /// </param>
    /// <param name="endFrame">
    /// End index of frame to encode (default = <c>-1</c>). Negative value is index from the last.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token for the async operation
    /// </param>
    public async IAsyncEnumerable<string> EncodeFramesAsync(
        int overwriteRepeat = -1,
        int startFrame = 0,
        int endFrame = -1,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        ImageFrameCollection<Rgba32> frames = Image.Frames;
        if (frames.Count < 2)
        {
            yield return EncodeFrame(frames[0]);
            yield break;
        }

        startFrame %= frames.Count;
        endFrame %= frames.Count;
        if (startFrame < 0)
            startFrame += frames.Count;
        if (endFrame < 0)
            endFrame += frames.Count;
        if (startFrame > endFrame)
            (startFrame, endFrame) = (endFrame, startFrame);

        IEnumerable<(int Index, int FrameIndex)> frameIndexEnumerator()
        {
            int count = endFrame - startFrame + 1;
            for (int i = 0; i < count; i++)
            {
                yield return (i, i + startFrame);
            }
        }

        uint repeatCount = overwriteRepeat >= 0 ? (uint)overwriteRepeat : RepeatCount;
        int[] delayMiliseconds = frameIndexEnumerator()
            .Select(t => GetFrameDelay(t.FrameIndex))
            .ToArray();

        // cache of Sixel strings
        string[] sixelFrames = new string[frames.Count];

        // Asynchronously store images as Sixel strings
        using BlockingCollection<bool> mutex = [];
        Task sixelFramesTask = Task.Run(
            () =>
            {
                foreach ((int i, int frameIndex) in frameIndexEnumerator())
                {
                    sixelFrames[i] = EncodeFrame(frames[frameIndex]);
                    mutex.Add(true);
                }
                mutex.CompleteAdding();
            },
            cancellationToken
        );

        DateTime start;
        // The first time of loop:
        // as soon as encoded in Sixel string
        foreach ((int i, _) in frameIndexEnumerator())
        {
            start = DateTime.Now;
            _ = mutex.Take(cancellationToken);
            yield return sixelFrames[i];
            if (delayMiliseconds[i] > 0)
            {
                int elaps = (int)(DateTime.Now - start).TotalMilliseconds;
                if (elaps < delayMiliseconds[i])
                    await Task.Delay(delayMiliseconds[i], cancellationToken);
            }
        }
        await sixelFramesTask;

        int count = 0;
        while (repeatCount < 1 || repeatCount > ++count)
        {
            foreach ((int i, _) in frameIndexEnumerator())
            {
                yield return sixelFrames[i];
                if (delayMiliseconds[i] > 0)
                {
                    await Task.Delay(delayMiliseconds[i], cancellationToken);
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Write to <see cref="Console.Out"/> Sixel strings encoded for each frame
    /// </summary>
    /// <param name="cellSize">Terminal character cell size in pixels</param>
    /// <param name="syncSupported">Whether terminal supports synchronized output</param>
    /// <inheritdoc cref="EncodeFramesAsync(int, int, int, int, CancellationToken)"/>
    /// <exception cref="NotSupportedException">
    /// This format does not support animation
    /// </exception>
    public async Task Animate(
        Size cellSize,
        bool syncSupported = false,
        int overwriteRepeat = -1,
        int startFrame = 0,
        int endFrame = -1,
        CancellationToken cancellationToken = default
    )
    {
        // Check if the image is animated
        if (!CanAnimate)
        {
            if (FrameCount < 2)
                throw new NotSupportedException($"This image has only one frame.");
            else
                throw new NotSupportedException(
                    $"This format does not support animation: {Format}"
                );
        }

        bool isOpaque = TransparencyMode == Transparency.None;

        int lines = (int)Math.Ceiling((double)Image.Height / cellSize.Height);
        // Allocate rows for the image height
        Terminal.Write(new string('\n', lines));
        // Move up cursor the rows
        Terminal.Write($"{Constants.ESC}{string.Format(Constants.CursorUp, lines)}");
        // Save the cursor position
        Terminal.Write($"{Constants.ESC}{Constants.CursorSave}");
        string beginSync = "",
            endSync = "";
        if (syncSupported)
        {
            beginSync = Constants.ESC + Constants.SyncBegin;
            endSync = Constants.ESC + Constants.SyncEnd;
        }
        try
        {
            await foreach (
                string sixelString in EncodeFramesAsync(
                    overwriteRepeat,
                    startFrame,
                    endFrame,
                    cancellationToken
                )
            )
            {
                if (isOpaque)
                {
                    // Restore the cursor position and then output sixel string
                    Terminal.Write($"{Constants.ESC}{Constants.CursorRestore}{sixelString}");
                }
                else
                {
                    // Restore the cursor position and erase from cursor until end of screen,
                    // and then output sixel string; do the erase and draw in one batch update if possible
                    Terminal.Write(
                        $"{beginSync}{Constants.ESC}{Constants.CursorRestore}{Constants.ESC}{Constants.EraseFromCursor}{sixelString}{endSync}"
                    );
                }
            }
        }
        catch (TaskCanceledException)
        {
            // for ignoring Non Local Exits
        }
    }

    /// <summary>
    /// Animate with standard terminal assumptions (10x20 pixel cells, no sync).
    /// </summary>
    /// <inheritdoc cref="Animate(Size, bool, int, int, int, CancellationToken)"/>
    public async Task Animate(
        int overwriteRepeat = -1,
        CancellationToken cancellationToken = default
    )
    {
        // Use standard monospace cell size assumptions
        Size standardCellSize = new(10, 20);
        await Animate(
            standardCellSize,
            syncSupported: false,
            overwriteRepeat,
            0,
            -1,
            cancellationToken
        );
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Image.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
