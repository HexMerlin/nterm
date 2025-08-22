using System.Runtime.CompilerServices;

namespace SemanticTokens.Sixel;

/// <summary>
/// Represents image dimensions in pixels for console image operations.
/// Immutable value type optimized for console graphics.
/// </summary>
public readonly struct ConsoleImageSize : IEquatable<ConsoleImageSize>
{
    /// <summary>
    /// Width in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Height in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Initializes a new instance with specified dimensions.
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    public ConsoleImageSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Indicates whether this size is equal to another size.
    /// </summary>
    /// <param name="other">Size to compare with this size</param>
    /// <returns><see langword="true"/> <b>iff</b> dimensions are identical</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ConsoleImageSize other) => Width == other.Width && Height == other.Height;

    /// <summary>
    /// Determines whether this size is equal to the specified object.
    /// </summary>
    /// <param name="obj">Object to compare with this size</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified object is equal</returns>
    public override bool Equals(object? obj) => obj is ConsoleImageSize other && Equals(other);

    ///<inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Width, Height);

    /// <summary>
    /// Determines whether two specified sizes have the same value.
    /// </summary>
    /// <param name="left">First size to compare</param>
    /// <param name="right">Second size to compare</param>
    /// <returns><see langword="true"/> <b>iff</b> dimensions are identical</returns>
    public static bool operator ==(ConsoleImageSize left, ConsoleImageSize right) => left.Equals(right);

    /// <summary>
    /// Determines whether two specified sizes have different values.
    /// </summary>
    /// <param name="left">First size to compare</param>
    /// <param name="right">Second size to compare</param>
    /// <returns><see langword="true"/> <b>iff</b> dimensions are different</returns>
    public static bool operator !=(ConsoleImageSize left, ConsoleImageSize right) => !left.Equals(right);

    public override string ToString() => $"{Width}x{Height}";
}
