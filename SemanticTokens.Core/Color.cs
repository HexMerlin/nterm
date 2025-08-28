using System.Runtime.CompilerServices;

namespace SemanticTokens.Core;

/// <summary>
/// Immutable platform-independent struct for 32-bit true colors with alpha channel.
/// Also provides set of predefined named colors.
/// </summary>
/// <remarks>
/// Predefined colors that contain with some additional color:
/// all colors of in <see cref="System.Drawing.KnownColor"/> but without the heavy dependency to System.Drawing.
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
    /// Default Color constructor. Same as <see langword="default"/> and <see cref="Color.Transparent"/>
    /// </summary>
    public Color() => (R, G, B, A) = (0, 0, 0, 0); // Color.Transparent

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct with the specified component values.
    /// </summary>
    /// <param name="r">The red component value (0-255).</param>
    /// <param name="g">The green component value (0-255).</param>
    /// <param name="b">The blue component value (0-255).</param>
    /// <param name="a">The alpha component value (0-255). Defaults to 255 (fully opaque).</param>
    public Color(byte r, byte g, byte b, byte a = 255) => (R, G, B, A) = (r, g, b, a);

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
    /// Indicates whether this is Color.Transparent
    /// </summary>
    public bool IsTransparent => A == 0 && R == 0 && G == 0 && B == 0;

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

    /// <summary>
    /// Creates a color from HLS (Hue, Lightness, Saturation) values.
    /// </summary>
    /// <param name="h">Hue value (0-360 degrees)</param>
    /// <param name="l">Lightness value (0-100)</param>
    /// <param name="s">Saturation value (0-100)</param>
    /// <returns>Color converted from HLS values</returns>
    /// <remarks>
    /// HLS color space conversion algorithm. Used primarily for Sixel color palette generation.
    /// Producer guarantees valid input ranges - no defensive validation performed.
    /// </remarks>
    public static Color FromHLS(int h, int l, int s)
    {
        double r, g, b;
        double max, min;

        if (l > 50)
        {
            max = l + (s * (1.0 - (l / 100.0)));
            min = l - (s * (1.0 - (l / 100.0)));
        }
        else
        {
            max = l + (s * l / 100.0);
            min = l - (s * l / 100.0);
        }

        h = (h + 240) % 360;

        (r, g, b) = h switch
        {
            < 60 => (max, min + ((max - min) * h / 60.0), min),
            < 120 => (min + ((max - min) * (120 - h) / 60.0), max, min),
            < 180 => (min, max, min + ((max - min) * (h - 120) / 60.0)),
            < 240 => (min, min + ((max - min) * (240 - h) / 60.0), max),
            < 300 => (min + ((max - min) * (h - 240) / 60.0), min, max),
            _ => (max, min, min + ((max - min) * (360 - h) / 60.0))
        };

        return new((byte)Math.Round(r * 255.0 / 100.0),
                   (byte)Math.Round(g * 255.0 / 100.0),
                   (byte)Math.Round(b * 255.0 / 100.0));
    }



    /// <summary>
    /// Blends this color with a background color to create a fully opaque result.
    /// </summary>
    /// <param name="background">Background color to blend with</param>
    /// <returns>New blended color with alpha = 255</returns>
    /// <remarks>
    /// Standard alpha blending operation. If this color is fully opaque (A=255), returns unchanged.
    /// If fully transparent (A=0), returns background. Otherwise performs alpha composition.
    /// </remarks>
    public Color BlendWith(Color background)
    {
        if (A == 255) return this;
        if (A == 0) return new(background.R, background.G, background.B, 255);

        double alpha = A / 255.0;
        return new(
            (byte)((R * alpha) + ((1.0 - alpha) * background.R)),
            (byte)((G * alpha) + ((1.0 - alpha) * background.G)),
            (byte)((B * alpha) + ((1.0 - alpha) * background.B)),
            255
        );
    }

    public override string ToString() =>
        TryGetKnownColorName(this, out string name) 
            ? name :
            $"R:{R}, G:{G}, B:{B}{(A == 255 ? "" : $", A:{A}")}";


}
