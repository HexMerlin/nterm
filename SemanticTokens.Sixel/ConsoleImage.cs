using System.Reflection;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;

namespace SemanticTokens.Sixel;

/// <summary>
/// Immutable console image with optimized SIXEL encoding and automatic terminal capability handling.
/// </summary>
/// <remarks>
/// Authority for console image representation. Encodes once during construction,
/// provides immediate console-ready output via ultra-optimized single execution path.
/// </remarks>
public readonly struct ConsoleImage : IEquatable<ConsoleImage>
{
    private readonly string _encodedData;
    private readonly Size _displaySize;
    private readonly bool _hasSixelData;

    /// <summary>
    /// Display dimensions in pixels.
    /// </summary>
    public Size DisplaySize => _displaySize;

    /// <summary>
    /// SIXEL encoding availability.
    /// </summary>
    /// <returns><see langword="true"/> <b>iff</b> SIXEL data encoded and terminal capable.</returns>
    public bool HasSixelData => _hasSixelData;

    /// <summary>
    /// Console-ready image data.
    /// </summary>
    /// <returns>Complete data ready for Console.WriteImage() - SIXEL or fallback text.</returns>
    public ReadOnlySpan<char> ConsoleData => _encodedData.AsSpan();

    /// <summary>
    /// Initializes console image with pre-encoded data.
    /// </summary>
    /// <param name="encodedData">SIXEL-encoded data or fallback text</param>
    /// <param name="displaySize">Target display size in pixels</param>
    /// <param name="hasSixelData">Indicates whether encodedData contains SIXEL</param>
    internal ConsoleImage(string encodedData, Size displaySize, bool hasSixelData)
    {
        _encodedData = encodedData;
        _displaySize = displaySize;
        _hasSixelData = hasSixelData;
    }

    /// <summary>
    /// Factory: Create ConsoleImage from file path.
    /// </summary>
    /// <param name="filePath">Path to image file</param>
    /// <returns>Builder for fluent configuration</returns>
    public static ConsoleImageBuilder FromFile(string filePath) => new(new FileImageSource(filePath));

    /// <summary>
    /// Factory: Create ConsoleImage from stream.
    /// </summary>
    /// <param name="stream">Image data stream</param>
    /// <returns>Builder for fluent configuration</returns>
    public static ConsoleImageBuilder FromStream(Stream stream) => new(new StreamImageSource(stream));

    /// <summary>
    /// Factory: Create ConsoleImage from embedded resource.
    /// </summary>
    /// <param name="resourceSuffix">Resource name suffix for lookup</param>
    /// <returns>Builder for fluent configuration</returns>
    /// <remarks>Uses the calling assembly to resolve embedded resources.</remarks>
    public static ConsoleImageBuilder FromEmbeddedResource(string resourceSuffix) => 
        new(new EmbeddedResourceImageSource(resourceSuffix));

    /// <summary>
    /// Factory: Create ConsoleImage from embedded resource in specific assembly.
    /// </summary>
    /// <param name="resourceSuffix">Resource name suffix for lookup</param>
    /// <param name="assembly">Assembly containing the embedded resources</param>
    /// <returns>Builder for fluent configuration</returns>
    public static ConsoleImageBuilder FromEmbeddedResource(string resourceSuffix, Assembly assembly) => 
        new(new EmbeddedResourceImageSource(resourceSuffix, assembly));

    /// <summary>
    /// Indicates whether this console image is equal to another console image.
    /// </summary>
    /// <param name="other">Console image to compare with this console image</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified console image is equal to this console image</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ConsoleImage other) => 
        _encodedData == other._encodedData && 
        _displaySize.Equals(other._displaySize) && 
        _hasSixelData == other._hasSixelData;

    /// <summary>
    /// Determines whether this console image is equal to the specified object.
    /// </summary>
    /// <param name="obj">Object to compare with this console image</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified object is equal</returns>
    public override bool Equals(object? obj) => obj is ConsoleImage other && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_encodedData, _displaySize, _hasSixelData);

    /// <summary>
    /// Determines whether two specified console images have the same value.
    /// </summary>
    /// <param name="left">First console image to compare</param>
    /// <param name="right">Second console image to compare</param>
    /// <returns><see langword="true"/> <b>iff</b> the value of <paramref name="left"/> is the same as the value of <paramref name="right"/></returns>
    public static bool operator ==(ConsoleImage left, ConsoleImage right) => left.Equals(right);

    /// <summary>
    /// Determines whether two specified console images have different values.
    /// </summary>
    /// <param name="left">First console image to compare</param>
    /// <param name="right">Second console image to compare</param>
    /// <returns><see langword="true"/> <b>iff</b> the value of <paramref name="left"/> is different from the value of <paramref name="right"/></returns>
    public static bool operator !=(ConsoleImage left, ConsoleImage right) => !left.Equals(right);

    public override string ToString() => 
        $"ConsoleImage[{DisplaySize.Width}x{DisplaySize.Height}, {(HasSixelData ? "SIXEL" : "Fallback")}]";
}
