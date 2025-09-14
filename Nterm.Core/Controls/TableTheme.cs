namespace Nterm.Core.Controls;

public record TableTheme(
    TableBorders Borders,
    Color BorderColor,
    Color HeaderTextColor,
    Color FirstColumnTextColor,
    Color OtherTextColor,
    Color BackgroundColor)
{
    public TableTheme() : this(
        TableBorders.Grid,
        Color.White,
        Color.White,
        Color.White,
        Color.White,
        Color.Transparent)
    { }

    // ----- Themes for Darker backgrounds -----

    public static TableTheme MonokaiMidnight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(117, 113, 94),
        HeaderTextColor: new Color(249, 38, 114),
        FirstColumnTextColor: new Color(166, 226, 46),
        OtherTextColor: new Color(248, 248, 242),
        BackgroundColor: default);

    public static TableTheme Dracula => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(68, 71, 90),
        HeaderTextColor: new Color(255, 121, 198),
        FirstColumnTextColor: new Color(80, 250, 123),
        OtherTextColor: new Color(248, 248, 242),
        BackgroundColor: default);

    public static TableTheme NordicNight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(76, 86, 106),
        HeaderTextColor: new Color(235, 203, 139),
        FirstColumnTextColor: new Color(136, 192, 208),
        OtherTextColor: new Color(216, 222, 233),
        BackgroundColor: default);

    public static TableTheme CyberpunkNeon => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(113, 28, 145),
        HeaderTextColor: new Color(234, 0, 217),
        FirstColumnTextColor: new Color(10, 189, 198),
        OtherTextColor: new Color(199, 199, 199),
        BackgroundColor: default);

    public static TableTheme SolarizedDark => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(88, 110, 117),
        HeaderTextColor: new Color(181, 137, 0),
        FirstColumnTextColor: new Color(38, 139, 210),
        OtherTextColor: new Color(131, 148, 150),
        BackgroundColor: default);

    // ----- Themes for Lighter backgrounds -----

    public static TableTheme SolarizedLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(147, 161, 161),
        HeaderTextColor: new Color(203, 75, 22),
        FirstColumnTextColor: new Color(38, 139, 210),
        OtherTextColor: new Color(101, 123, 131),
        BackgroundColor: default);

    public static TableTheme GitHubLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(208, 215, 222),
        HeaderTextColor: new Color(9, 105, 218),
        FirstColumnTextColor: new Color(36, 41, 46),
        OtherTextColor: new Color(87, 96, 106),
        BackgroundColor: default);

    public static TableTheme GruvboxLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(189, 174, 147),
        HeaderTextColor: new Color(7, 102, 120),
        FirstColumnTextColor: new Color(175, 58, 3),
        OtherTextColor: new Color(60, 56, 54),
        BackgroundColor: default);

    public static TableTheme AtomOneLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(160, 161, 167),
        HeaderTextColor: new Color(166, 38, 164),
        FirstColumnTextColor: new Color(80, 161, 79),
        OtherTextColor: new Color(56, 58, 66),
        BackgroundColor: default);

    public static TableTheme MaterialLight => new(
        Borders: TableBorders.Grid,
        BorderColor: new Color(211, 225, 232),
        HeaderTextColor: new Color(0, 188, 212),
        FirstColumnTextColor: new Color(247, 109, 71),
        OtherTextColor: new Color(84, 110, 122),
        BackgroundColor: default);
}
