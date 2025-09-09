using NTerm.Core;
using SixLabors.ImageSharp.PixelFormats;

namespace NTerm.Sixel;

/// <summary>
/// Extension methods for converting between NTerm.Core.Color and ImageSharp types.
/// </summary>
internal static class ColorExtensions
{
    /// <summary>
    /// Converts ImageSharp Rgba32 to Nterm Color.
    /// </summary>
    /// <param name="rgba">ImageSharp pixel value</param>
    /// <returns>Equivalent Color value</returns>
    public static Color ToColor(this Rgba32 rgba) => new(rgba.R, rgba.G, rgba.B, rgba.A);

    /// <summary>
    /// Converts NTerm Color to ImageSharp Rgba32.
    /// </summary>
    /// <param name="color">Color value</param>
    /// <returns>Equivalent ImageSharp Rgba32 value</returns>
    public static Rgba32 ToRgba32(this Color color) => new(color.R, color.G, color.B, color.A);

    /// <summary>
    /// Applies Sixel-specific transparency handling to a color.
    /// </summary>
    /// <param name="color">Color to process</param>
    /// <param name="transparency">Transparency mode for Sixel processing</param>
    /// <param name="transparentColor">Explicit transparent color (optional)</param>
    /// <param name="backgroundColor">Background color for blending (optional)</param>
    /// <returns>Color with transparency resolved for Sixel encoding</returns>
    public static Color ToSixelColor(this Color color,
                                     Transparency transparency = Transparency.Default,
                                     Color? transparentColor = null,
                                     Color? backgroundColor = null)
    {
        // Handle fully transparent pixels
        if (color.A == 0)
        {
            return transparency switch
            {
                Transparency.None => Color.Black,
                Transparency.TopLeft => Color.Black,
                Transparency.Background when backgroundColor.HasValue => backgroundColor.Value,
                _ => Color.Transparent
            };
        }

        // Handle explicit transparent color match
        if (transparentColor.HasValue && transparentColor.Value.Equals(color))
            return Color.Transparent;

        // Handle background transparency
        if (transparency == Transparency.Background && backgroundColor.HasValue && backgroundColor.Value.Equals(color))
            return Color.Transparent;

        // Blend partial transparency with background
        if (color.A is > 0 and < 255)
        {
            Color background = backgroundColor ?? Color.Black;
            return color.BlendWith(background);
        }

        return color;
    }

    /// <summary>
    /// Converts ImageSharp Rgba32 to Nterm Color with Sixel transparency handling.
    /// </summary>
    /// <param name="rgba">ImageSharp pixel value</param>
    /// <param name="transparency">Transparency mode for Sixel processing</param>
    /// <param name="transparentColor">Explicit transparent color (optional)</param>
    /// <param name="backgroundColor">Background color for blending (optional)</param>
    /// <returns>Color with transparency resolved for Sixel encoding</returns>
    public static Color ToSixelColor(this Rgba32 rgba,
                                     Transparency transparency = Transparency.Default,
                                     Rgba32? transparentColor = null,
                                     Rgba32? backgroundColor = null)
    {
        // Convert foreign type to our Color first
        Color color = rgba.ToColor();
        Color? transparentColorConverted = transparentColor?.ToColor();
        Color? backgroundColorConverted = backgroundColor?.ToColor();

        // Apply Sixel-specific transparency logic
        return color.ToSixelColor(transparency, transparentColorConverted, backgroundColorConverted);
    }

    /// <summary>
    /// Converts NTerm Color to Sixel palette format string.
    /// </summary>
    /// <param name="color">Color to convert</param>
    /// <returns>Sixel color palette string in format "R;G;B" with values scaled to 0-100 range</returns>
    /// <remarks>
    /// Sixel protocol requires RGB values in 0-100 range for color palette definitions.
    /// Converts from standard 0-255 RGBA to Sixel's 0-100 RGB format.
    /// </remarks>
    public static ReadOnlySpan<char> ToSixelPalette(this Color color)
    {
        int r = (int)Math.Round(color.R * 100.0 / 255.0);
        int g = (int)Math.Round(color.G * 100.0 / 255.0);
        int b = (int)Math.Round(color.B * 100.0 / 255.0);
        return $"{r};{g};{b}";
    }
}
