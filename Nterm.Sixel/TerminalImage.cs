using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;
using System.Runtime.CompilerServices;
using Size = NTerm.Core.Size;

namespace NTerm.Sixel;

/// <summary>
/// Immutable SIXEL image data for display in a terminal.
/// </summary>
/// <remarks>
/// Pure data container with SIXEL-encoded image data ready for console output.
/// Contains either optimized SIXEL encoding or fallback text representation.
/// </remarks>
public readonly struct TerminalImage : IEquatable<TerminalImage>
{
    /// <summary>
    /// Image dimensions in pixels.
    /// </summary>
    public readonly Size DisplaySize { get; }

    /// <summary>
    /// Optimized encoding availability.
    /// </summary>
    /// <returns><see langword="true"/> <b>iff</b> optimized SIXEL data encoded successfully.</returns>
    public readonly bool HasOptimizedEncoding { get; }

    ///// <summary>
    ///// Terminal-ready image data.
    ///// </summary>
    ///// <returns>Complete SIXEL data ready for terminal output - optimized encoding or fallback text.</returns>
    public readonly string EncodedData { get; }

    /// <summary>
    /// Calculates image dimensions in character grid cells.
    /// </summary>
    /// <param name="cellSizeInPixels">Terminal character cell size in pixels</param>
    /// <returns>Image dimensions in character cells (width×height)</returns>
    public Size GetSizeInCharacters(Size cellSizeInPixels)
    {
        return new Size(
            (int)Math.Ceiling((double)DisplaySize.Width / cellSizeInPixels.Width),
            (int)Math.Ceiling((double)DisplaySize.Height / cellSizeInPixels.Height)
        );
    }

    /// <summary>
    /// Initializes console image with pre-encoded data.
    /// </summary>
    /// <param name="encodedData">Encoded image data or fallback text</param>
    /// <param name="displaySize">Target display size in pixels</param>
    /// <param name="hasOptimizedEncoding">Indicates whether encodedData contains optimized encoding</param>
    public TerminalImage(string encodedData, Size displaySize, bool hasOptimizedEncoding)
    {
        EncodedData = encodedData;
        DisplaySize = displaySize;
        HasOptimizedEncoding = hasOptimizedEncoding;
    }

    /// <summary>
    /// Creates ConsoleImage from file path using original image dimensions.
    /// </summary>
    /// <param name="filePath">Path to image file</param>
    /// <param name="fallbackText">Text to display when SIXEL encoding fails</param>
    /// <param name="transparency">Transparency handling mode</param>
    /// <returns>Console image ready for output</returns>
    /// <exception cref="FileNotFoundException">Image file not found</exception>
    public static TerminalImage FromFile(string filePath,
                                       string fallbackText = "[image]",
                                       Transparency transparency = Transparency.Default)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Image file not found: {filePath}");

        using FileStream stream = File.OpenRead(filePath);
        return FromStream(stream, fallbackText, transparency);
    }

    /// <summary>
    /// Creates ConsoleImage from stream using original image dimensions.
    /// </summary>
    /// <param name="stream">Image data stream</param>
    /// <param name="fallbackText">Text to display when SIXEL encoding fails</param>
    /// <param name="transparency">Transparency handling mode</param>
    /// <returns>Console image ready for output</returns>
    public static TerminalImage FromStream(Stream stream,
                                         string fallbackText = "[image]",
                                         Transparency transparency = Transparency.Default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrEmpty(fallbackText);

        try
        {
            // Get original image dimensions
            using Image<Rgba32> originalImg = Image.Load<Rgba32>(stream);
            Size originalSize = new(originalImg.Width, originalImg.Height);

            // Reset stream position for SIXEL encoding
            stream.Position = 0;

            // Encode to SIXEL using original dimensions
            ReadOnlySpan<char> sixelData = SixelEncode.Encode(
                stream,
                transparency,
                frame: -1
            );

            return new TerminalImage(sixelData.ToString(), originalSize, hasOptimizedEncoding: true);
        }
        catch
        {
            // Fallback - use reasonable default size for fallback text
            Size fallbackSize = new(320, 240);
            return new TerminalImage(fallbackText, fallbackSize, hasOptimizedEncoding: false);
        }
    }

    /// <summary>
    /// Creates ConsoleImage from embedded resource using original image dimensions.
    /// </summary>
    /// <param name="resourceSuffix">Resource name suffix for lookup</param>
    /// <param name="fallbackText">Text to display when SIXEL encoding fails</param>
    /// <param name="transparency">Transparency handling mode</param>
    /// <returns>Console image ready for output</returns>
    /// <exception cref="FileNotFoundException">Embedded resource not found</exception>
    public static TerminalImage FromEmbeddedResource(string resourceSuffix,
                                                   string fallbackText = "[image]",
                                                   Transparency transparency = Transparency.Default) => FromEmbeddedResource(resourceSuffix, Assembly.GetCallingAssembly(), fallbackText, transparency);

    /// <summary>
    /// Creates ConsoleImage from embedded resource in specific assembly using original image dimensions.
    /// </summary>
    /// <param name="resourceSuffix">Resource name suffix for lookup</param>
    /// <param name="assembly">Assembly containing the embedded resources</param>
    /// <param name="fallbackText">Text to display when SIXEL encoding fails</param>
    /// <param name="transparency">Transparency handling mode</param>
    /// <returns>Console image ready for output</returns>
    /// <exception cref="FileNotFoundException">Embedded resource not found</exception>
    public static TerminalImage FromEmbeddedResource(string resourceSuffix,
                                                   Assembly assembly,
                                                   string fallbackText = "[image]",
                                                   Transparency transparency = Transparency.Default)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceSuffix);
        ArgumentNullException.ThrowIfNull(assembly);

        string[] allResources = assembly.GetManifestResourceNames();
        string? resourceName = allResources
            .FirstOrDefault(n => n.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            throw new FileNotFoundException($"Embedded resource not found: {resourceSuffix} in assembly {assembly.GetName().Name}");

        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded resource stream is null: {resourceName}");

        return FromStream(stream, fallbackText, transparency);
    }

    /// <summary>
    /// Indicates whether this console image is equal to another console image.
    /// </summary>
    /// <param name="other">Console image to compare with this console image</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified console image is equal to this console image</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(TerminalImage other) =>
        EncodedData == other.EncodedData &&
        DisplaySize.Equals(other.DisplaySize) &&
        HasOptimizedEncoding == other.HasOptimizedEncoding;

    /// <summary>
    /// Determines whether this console image is equal to the specified object.
    /// </summary>
    /// <param name="obj">Object to compare with this console image</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified object is equal</returns>
    public override bool Equals(object? obj) => obj is TerminalImage other && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(EncodedData, DisplaySize, HasOptimizedEncoding);

    /// <summary>
    /// Determines whether two specified console images have the same value.
    /// </summary>
    /// <param name="left">First console image to compare</param>
    /// <param name="right">Second console image to compare</param>
    /// <returns><see langword="true"/> <b>iff</b> the value of <paramref name="left"/> is the same as the value of <paramref name="right"/></returns>
    public static bool operator ==(TerminalImage left, TerminalImage right) => left.Equals(right);

    /// <summary>
    /// Determines whether two specified console images have different values.
    /// </summary>
    /// <param name="left">First console image to compare</param>
    /// <param name="right">Second console image to compare</param>
    /// <returns><see langword="true"/> <b>iff</b> the value of <paramref name="left"/> is different from the value of <paramref name="right"/></returns>
    public static bool operator !=(TerminalImage left, TerminalImage right) => !left.Equals(right);

    public override string ToString() =>
        $"ConsoleImage[{DisplaySize.Width}x{DisplaySize.Height}, {(HasOptimizedEncoding ? "Optimized" : "Fallback")}]";
}