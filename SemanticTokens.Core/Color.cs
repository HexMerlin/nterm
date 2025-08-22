using System.Runtime.CompilerServices;

namespace SemanticTokens.Core;

/// <summary>
/// Immutable platform-independent struct for 32-bit true colors with alpha channel.
/// Also provides set of predefined named colors.
/// </summary>
/// <remarks>
/// Predefined colors are equivalent to set in <see cref="System.Drawing.KnownColor"/> but without the heavy dependency to System.Drawing.
/// </remarks>
public readonly partial struct Color : IEquatable<Color>
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
    /// Alpha component value of this color (0-255).
    /// </summary>
    /// <remarks>
    /// Value of 255 indicates fully opaque, 0 indicates fully transparent.
    /// </remarks>
    public byte A { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct with the specified component values.
    /// </summary>
    /// <param name="r">The red component value (0-255).</param>
    /// <param name="g">The green component value (0-255).</param>
    /// <param name="b">The blue component value (0-255).</param>
    /// <param name="a">The alpha component value (0-255). Defaults to 255 (fully opaque).</param>
    public Color(byte r, byte g, byte b, byte a = 255) => (R, G, B, A) = (r, g, b, a);

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct with the specified RGB component values and full opacity.
    /// </summary>
    /// <param name="r">The red component value (0-255).</param>
    /// <param name="g">The green component value (0-255).</param>
    /// <param name="b">The blue component value (0-255).</param>
    public Color(byte r, byte g, byte b) => (R, G, B, A) = (r, g, b, 255);

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct from a 32-bit uint (0xAARRGGBB).
    /// </summary>
    /// <param name="value">The 32-bit unsigned integer in ARGB format.</param>
    public Color(uint value)
    {
        A = (byte)((value >> 24) & 0xFF);
        R = (byte)((value >> 16) & 0xFF);
        G = (byte)((value >> 8) & 0xFF);
        B = (byte)(value & 0xFF);
    }

    /// <summary>
    /// Implicitly converts an 32-bit unsigned integer (0xAARRGGBB) to a <see cref="Color"/>.
    /// </summary>
    /// <param name="uint">The color value to convert.</param>
    /// <returns>A color with the designated value.</returns>
    public static implicit operator Color(uint value) => new(value);

    /// <summary>
    /// Indicates whether this color is equal to another color.
    /// </summary>
    /// <param name="other">The color to compare with this color.</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified color is equal to this color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;

    /// <summary>
    /// Determines whether this color is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this color.</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified object is equal.</returns>
    public override bool Equals(object? obj) => obj is Color o && Equals(o);

    ///<inheritdoc/>
    public override int GetHashCode() => (A << 24) | (R << 16) | (G << 8) | B;

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

    /// <summary>
    /// Converts <see cref="ConsoleColor"/> to <see cref="Color"/>.
    /// </summary>
    /// <param name="c">Console color to convert</param>
    /// <remarks>The conversion is not guaranteed to be exact.</remarks>
    /// <returns>RGB color value</returns>
    public static Color FromConsoleColor(ConsoleColor c) => c switch
    {
        ConsoleColor.Black => Black,
        ConsoleColor.DarkBlue => DarkBlue,
        ConsoleColor.DarkGreen => DarkGreen,
        ConsoleColor.DarkCyan => DarkCyan,
        ConsoleColor.DarkRed => DarkRed,
        ConsoleColor.DarkMagenta => DarkMagenta,
        ConsoleColor.DarkYellow => Goldenrod,
        ConsoleColor.Gray => Gray,
        ConsoleColor.DarkGray => DarkGray,
        ConsoleColor.Blue => Blue,
        ConsoleColor.Green => Green,
        ConsoleColor.Cyan => Cyan,
        ConsoleColor.Red => Red,
        ConsoleColor.Magenta => Magenta,
        ConsoleColor.Yellow => Yellow,
        _ => White // ConsoleColor.White
    };

    /// <summary>
    /// Converts this color to a 32-bit unsigned integer in ARGB format.
    /// </summary>
    /// <returns>32-bit unsigned integer representation (0xAARRGGBB).</returns>
    public uint ToUint() => ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | B;

    public override string ToString()
    {
        if (TryGetKnownColorName(this, out string name))
            return name;
        return A == 255 ? $"R:{R}, G:{G}, B:{B}" : $"R:{R}, G:{G}, B:{B}, A:{A}";
    }


}
