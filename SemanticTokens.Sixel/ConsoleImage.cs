using System.Runtime.CompilerServices;
using SemanticTokens.Core;

namespace SemanticTokens.Sixel;

/// <summary>
/// Immutable console image data container.
/// Gold standard user-facing type for console image operations.
/// </summary>
/// <remarks>
/// Pure data container with console-ready encoded image data.
/// Provides immediate output via Console.WriteImage() integration.
/// </remarks>
public readonly struct ConsoleImage : IEquatable<ConsoleImage>
{
    /// <summary>
    /// Image dimensions in pixels.
    /// </summary>
    public readonly Size DisplaySize {  get; }

    /// <summary>
    /// Optimized encoding availability.
    /// </summary>
    /// <returns><see langword="true"/> <b>iff</b> optimized data encoded and terminal capable.</returns>
    public readonly bool HasOptimizedEncoding { get; }

    /// <summary>
    /// Encoded image data as string
    /// </summary>
    private readonly string EncodedData { get; }

    /// <summary>
    /// Display dimensions in character grid cells.
    /// Computed based on terminal capabilities and pixel dimensions.
    /// </summary>
    /// <returns>Character grid dimensions with precision indicator</returns>
    public ConsoleImageCharacterSize CharacterSize => 
        ConsoleImageCharacterSize.FromConsoleImage(this);

    /// <summary>
    /// Display dimensions in character grid cells with safety margin.
    /// Adds +1 cell in each dimension for bulletproof robustness.
    /// </summary>
    /// <returns>Conservative character grid dimensions guaranteed to contain the image</returns>
    public ConsoleImageCharacterSize SafeCharacterSize => 
        ConsoleImageCharacterSize.FromConsoleImage(this, addSafetyMargin: true);


    /// <summary>
    /// Console-ready image data.
    /// </summary>
    /// <returns>Complete data ready for Console.WriteImage() - optimized or fallback text.</returns>
    public ReadOnlySpan<char> ConsoleData => EncodedData.AsSpan();

    /// <summary>
    /// Initializes console image with pre-encoded data.
    /// </summary>
    /// <param name="encodedData">Encoded image data or fallback text</param>
    /// <param name="displaySize">Target display size in pixels</param>
    /// <param name="hasOptimizedEncoding">Indicates whether encodedData contains optimized encoding</param>
    public ConsoleImage(string encodedData, Size displaySize, bool hasOptimizedEncoding)
    {
        EncodedData = encodedData;
        DisplaySize = displaySize;
        HasOptimizedEncoding = hasOptimizedEncoding;
    }


    /// <summary>
    /// Indicates whether this console image is equal to another console image.
    /// </summary>
    /// <param name="other">Console image to compare with this console image</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified console image is equal to this console image</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ConsoleImage other) => 
        EncodedData == other.EncodedData &&
        DisplaySize.Equals(other.DisplaySize) && 
        HasOptimizedEncoding == other.HasOptimizedEncoding;

    /// <summary>
    /// Determines whether this console image is equal to the specified object.
    /// </summary>
    /// <param name="obj">Object to compare with this console image</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified object is equal</returns>
    public override bool Equals(object? obj) => obj is ConsoleImage other && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(EncodedData, DisplaySize, HasOptimizedEncoding);

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
        $"ConsoleImage[{DisplaySize.Width}x{DisplaySize.Height}, {(HasOptimizedEncoding ? "Optimized" : "Fallback")}]";
}
