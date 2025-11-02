using Nterm.Common;

namespace Nterm.Common.Controls;

/// <summary>
/// Theme describing border style and colors used when rendering a <see cref="Table"/>.
/// </summary>
/// <param name="Borders">The border style to render (see <see cref="TableBorders"/>).</param>
/// <param name="BorderColor">Color used for border characters and separators.</param>
/// <param name="HeaderTextColor">Text color for header cells.</param>
/// <param name="FirstColumnTextColor">Text color for the first (left-most) data column.</param>
/// <param name="OtherTextColor">Text color for all non-first data columns.</param>
/// <param name="BackgroundColor">Background color hint for the table. Rendering may choose to ignore this.</param>
/// <remarks>
/// <para>
/// The <see cref="BackgroundColor"/> is provided for completeness; current rendering writes only foreground colors.
/// Consumers can honor it when integrating with terminals that support background color operations.
/// </para>
/// </remarks>
/// <seealso cref="Table"/>
/// <seealso cref="TableBorders"/>
public record TableTheme(
    TableBorders Borders,
    Color BorderColor,
    Color HeaderTextColor,
    Color FirstColumnTextColor,
    Color OtherTextColor,
    Color BackgroundColor)
{
    /// <summary>
    /// Initializes a new theme with grid borders and white foreground colors on a transparent background.
    /// </summary>
    public TableTheme() : this(
        TableBorders.Grid,
        Color.White,
        Color.White,
        Color.White,
        Color.White,
        Color.Transparent)
    { }

    // ----- Themes for Darker backgrounds -----

    /// <summary>
    /// Monokai-inspired theme optimized for dark backgrounds.
    /// </summary>
    public static TableTheme MonokaiMidnight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(117, 113, 94),
        HeaderTextColor: new Color(249, 38, 114),
        FirstColumnTextColor: new Color(166, 226, 46),
        OtherTextColor: new Color(248, 248, 242),
        BackgroundColor: default);

    /// <summary>
    /// Dracula-inspired theme optimized for dark backgrounds.
    /// </summary>
    public static TableTheme Dracula => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(68, 71, 90),
        HeaderTextColor: new Color(255, 121, 198),
        FirstColumnTextColor: new Color(80, 250, 123),
        OtherTextColor: new Color(248, 248, 242),
        BackgroundColor: default);

    /// <summary>
    /// Nord-inspired theme optimized for dark backgrounds.
    /// </summary>
    public static TableTheme NordicNight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(76, 86, 106),
        HeaderTextColor: new Color(235, 203, 139),
        FirstColumnTextColor: new Color(136, 192, 208),
        OtherTextColor: new Color(216, 222, 233),
        BackgroundColor: default);

    /// <summary>
    /// Neon-accented theme suited for high-contrast, dark backgrounds.
    /// </summary>
    public static TableTheme CyberpunkNeon => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(113, 28, 145),
        HeaderTextColor: new Color(234, 0, 217),
        FirstColumnTextColor: new Color(10, 189, 198),
        OtherTextColor: new Color(199, 199, 199),
        BackgroundColor: default);

    /// <summary>
    /// Solarized Dark palette.
    /// </summary>
    public static TableTheme SolarizedDark => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(88, 110, 117),
        HeaderTextColor: new Color(181, 137, 0),
        FirstColumnTextColor: new Color(38, 139, 210),
        OtherTextColor: new Color(131, 148, 150),
        BackgroundColor: default);

    // ----- Themes for Lighter backgrounds -----

    /// <summary>
    /// Solarized Light palette.
    /// </summary>
    public static TableTheme SolarizedLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(147, 161, 161),
        HeaderTextColor: new Color(203, 75, 22),
        FirstColumnTextColor: new Color(38, 139, 210),
        OtherTextColor: new Color(101, 123, 131),
        BackgroundColor: default);

    /// <summary>
    /// GitHub-like light theme.
    /// </summary>
    public static TableTheme GitHubLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(208, 215, 222),
        HeaderTextColor: new Color(9, 105, 218),
        FirstColumnTextColor: new Color(36, 41, 46),
        OtherTextColor: new Color(87, 96, 106),
        BackgroundColor: default);

    /// <summary>
    /// Gruvbox Light palette.
    /// </summary>
    public static TableTheme GruvboxLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(189, 174, 147),
        HeaderTextColor: new Color(7, 102, 120),
        FirstColumnTextColor: new Color(175, 58, 3),
        OtherTextColor: new Color(60, 56, 54),
        BackgroundColor: default);

    /// <summary>
    /// Atom One Light theme.
    /// </summary>
    public static TableTheme AtomOneLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(160, 161, 167),
        HeaderTextColor: new Color(166, 38, 164),
        FirstColumnTextColor: new Color(80, 161, 79),
        OtherTextColor: new Color(56, 58, 66),
        BackgroundColor: default);

    /// <summary>
    /// Material-inspired light theme.
    /// </summary>
    public static TableTheme MaterialLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(211, 225, 232),
        HeaderTextColor: new Color(0, 188, 212),
        FirstColumnTextColor: new Color(247, 109, 71),
        OtherTextColor: new Color(84, 110, 122),
        BackgroundColor: default);
}
