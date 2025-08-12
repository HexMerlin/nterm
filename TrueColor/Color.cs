using System.Runtime.CompilerServices;

namespace TrueColor;

/// <summary>
/// Immutable struct for 24-bit true colors
/// </summary>
public readonly struct Color : IEquatable<Color>
{
    /// <summary>
    /// Red component value of this color (0-255).
    /// </summary>
    public byte R { get; }

    /// <summary>
    /// Green component value of this color (0-255).
    /// </summary>
    public byte G { get; }

    /// <summary>
    /// Blue component value of this color (0-255).
    /// </summary>
    public byte B { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct with the specified component values.
    /// </summary>
    /// <param name="r">The red component value (0-255).</param>
    /// <param name="g">The green component value (0-255).</param>
    /// <param name="b">The blue component value (0-255).</param>
    public Color(byte r, byte g, byte b) => (R, G, B) = (r, g, b);

    /// <summary>
    /// Indicates whether this color is equal to another color.
    /// </summary>
    /// <param name="other">The color to compare with this color.</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified color is equal to this color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Color other) => R == other.R && G == other.G && B == other.B;

    /// <summary>
    /// Determines whether this color is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this color.</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified object is equal.</returns>
    public override bool Equals(object? obj) => obj is Color o && Equals(o);

    ///<inheritdoc/>
    public override int GetHashCode() => (R << 16) | (G << 8) | B;

    /// <summary>
    /// Determines whether two specified colors have the same value.
    /// </summary>
    /// <param name="a">The first color to compare.</param>
    /// <param name="b">The second color to compare.</param>
    /// <returns><see langword="true"/> <b>iff</b> the value of <paramref name="a"/> is the same as the value of <paramref name="b"/>.</returns>
    public static bool operator ==(Color a, Color b) => a.Equals(b);

    /// <summary>
    /// Determines whether two specified colors have different values.
    /// </summary>
    /// <param name="a">The first color to compare.</param>
    /// <param name="b">The second color to compare.</param>
    /// <returns><see langword="true"/> <b>iff</b> the value of <paramref name="a"/> is different from the value of <paramref name="b"/>.</returns>
    public static bool operator !=(Color a, Color b) => !a.Equals(b);
}
