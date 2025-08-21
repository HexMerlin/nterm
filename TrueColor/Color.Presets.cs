using System.ComponentModel;
using System.Xml.Linq;

namespace TrueColor;

public readonly partial struct Color
{
    public static Color ActiveBorder => new(activeBorder);
    private const uint activeBorder = 0x00B4B4B4u;

    public static Color ActiveCaption => new(activeCaption);
    private const uint activeCaption = 0x0099B4D1u;

    public static Color ActiveCaptionText => new(activeCaptionText);
    private const uint activeCaptionText = 0x00000000u;

    public static Color AliceBlue => new(aliceBlue);
    private const uint aliceBlue = 0x00F0F8FFu;

    public static Color AntiqueWhite => new(antiqueWhite);
    private const uint antiqueWhite = 0x00FAEBD7u;

    public static Color AppWorkspace => new(appWorkspace);
    private const uint appWorkspace = 0x00ABABABu;

    public static Color Aqua => new(aqua);
    private const uint aqua = 0x0000FFFFu;

    public static Color Aquamarine => new(aquamarine);
    private const uint aquamarine = 0x007FFFD4u;

    public static Color Azure => new(azure);
    private const uint azure = 0x00F0FFFFu;

    public static Color Beige => new(beige);
    private const uint beige = 0x00F5F5DCu;

    public static Color Bisque => new(bisque);
    private const uint bisque = 0x00FFE4C4u;

    public static Color Black => new(black);
    private const uint black = 0x00000000u;

    public static Color BlanchedAlmond => new(blanchedAlmond);
    private const uint blanchedAlmond = 0x00FFEBCDu;

    public static Color Blue => new(blue);
    private const uint blue = 0x000000FFu;

    public static Color BlueViolet => new(blueViolet);
    private const uint blueViolet = 0x008A2BE2u;

    public static Color Brown => new(brown);
    private const uint brown = 0x00A52A2Au;

    public static Color BurlyWood => new(burlyWood);
    private const uint burlyWood = 0x00DEB887u;

    public static Color ButtonFace => new(buttonFace);
    private const uint buttonFace = 0x00F0F0F0u;

    public static Color ButtonHighlight => new(buttonHighlight);
    private const uint buttonHighlight = 0x00FFFFFFu;

    public static Color ButtonShadow => new(buttonShadow);
    private const uint buttonShadow = 0x00A0A0A0u;

    public static Color CadetBlue => new(cadetBlue);
    private const uint cadetBlue = 0x005F9EA0u;

    public static Color Chartreuse => new(chartreuse);
    private const uint chartreuse = 0x007FFF00u;

    public static Color Chocolate => new(chocolate);
    private const uint chocolate = 0x00D2691Eu;

    public static Color Control => new(control);
    private const uint control = 0x00F0F0F0u;

    public static Color ControlDark => new(controlDark);
    private const uint controlDark = 0x00A0A0A0u;

    public static Color ControlDarkDark => new(controlDarkDark);
    private const uint controlDarkDark = 0x00696969u;

    public static Color ControlLight => new(controlLight);
    private const uint controlLight = 0x00E3E3E3u;

    public static Color ControlLightLight => new(controlLightLight);
    private const uint controlLightLight = 0x00FFFFFFu;

    public static Color ControlText => new(controlText);
    private const uint controlText = 0x00000000u;

    public static Color Coral => new(coral);
    private const uint coral = 0x00FF7F50u;

    public static Color CornflowerBlue => new(cornflowerBlue);
    private const uint cornflowerBlue = 0x006495EDu;

    public static Color Cornsilk => new(cornsilk);
    private const uint cornsilk = 0x00FFF8DCu;

    public static Color Crimson => new(crimson);
    private const uint crimson = 0x00DC143Cu;

    public static Color Cyan => new(cyan);
    private const uint cyan = 0x0000FFFFu;

    public static Color DarkBlue => new(darkBlue);
    private const uint darkBlue = 0x0000008Bu;

    public static Color DarkCyan => new(darkCyan);
    private const uint darkCyan = 0x00008B8Bu;

    public static Color DarkGoldenrod => new(darkGoldenrod);
    private const uint darkGoldenrod = 0x00B8860Bu;

    public static Color DarkGray => new(darkGray);
    private const uint darkGray = 0x00A9A9A9u;

    public static Color DarkGreen => new(darkGreen);
    private const uint darkGreen = 0x00006400u;

    public static Color DarkKhaki => new(darkKhaki);
    private const uint darkKhaki = 0x00BDB76Bu;

    public static Color DarkMagenta => new(darkMagenta);
    private const uint darkMagenta = 0x008B008Bu;

    public static Color DarkOliveGreen => new(darkOliveGreen);
    private const uint darkOliveGreen = 0x00556B2Fu;

    public static Color DarkOrange => new(darkOrange);
    private const uint darkOrange = 0x00FF8C00u;

    public static Color DarkOrchid => new(darkOrchid);
    private const uint darkOrchid = 0x009932CCu;

    public static Color DarkRed => new(darkRed);
    private const uint darkRed = 0x008B0000u;

    public static Color DarkSalmon => new(darkSalmon);
    private const uint darkSalmon = 0x00E9967Au;

    public static Color DarkSeaGreen => new(darkSeaGreen);
    private const uint darkSeaGreen = 0x008FBC8Fu;

    public static Color DarkSlateBlue => new(darkSlateBlue);
    private const uint darkSlateBlue = 0x00483D8Bu;

    public static Color DarkSlateGray => new(darkSlateGray);
    private const uint darkSlateGray = 0x002F4F4Fu;

    public static Color DarkTurquoise => new(darkTurquoise);
    private const uint darkTurquoise = 0x0000CED1u;

    public static Color DarkViolet => new(darkViolet);
    private const uint darkViolet = 0x009400D3u;

    public static Color DeepPink => new(deepPink);
    private const uint deepPink = 0x00FF1493u;

    public static Color DeepSkyBlue => new(deepSkyBlue);
    private const uint deepSkyBlue = 0x0000BFFFu;

    public static Color Desktop => new(desktop);
    private const uint desktop = 0x00000000u;

    public static Color DimGray => new(dimGray);
    private const uint dimGray = 0x00696969u;

    public static Color DodgerBlue => new(dodgerBlue);
    private const uint dodgerBlue = 0x001E90FFu;

    public static Color Firebrick => new(firebrick);
    private const uint firebrick = 0x00B22222u;

    public static Color FloralWhite => new(floralWhite);
    private const uint floralWhite = 0x00FFFAF0u;

    public static Color ForestGreen => new(forestGreen);
    private const uint forestGreen = 0x00228B22u;

    public static Color Fuchsia => new(fuchsia);
    private const uint fuchsia = 0x00FF00FFu;

    public static Color Gainsboro => new(gainsboro);
    private const uint gainsboro = 0x00DCDCDCu;

    public static Color GhostWhite => new(ghostWhite);
    private const uint ghostWhite = 0x00F8F8FFu;

    public static Color Gold => new(gold);
    private const uint gold = 0x00FFD700u;

    public static Color Goldenrod => new(goldenrod);
    private const uint goldenrod = 0x00DAA520u;

    public static Color GradientActiveCaption => new(gradientActiveCaption);
    private const uint gradientActiveCaption = 0x00B9D1EAu;

    public static Color GradientInactiveCaption => new(gradientInactiveCaption);
    private const uint gradientInactiveCaption = 0x00D7E4F2u;

    public static Color Gray => new(gray);
    private const uint gray = 0x00808080u;

    public static Color GrayText => new(grayText);
    private const uint grayText = 0x006D6D6Du;

    public static Color Green => new(green);
    private const uint green = 0x00008000u;

    public static Color GreenYellow => new(greenYellow);
    private const uint greenYellow = 0x00ADFF2Fu;

    public static Color Highlight => new(highlight);
    private const uint highlight = 0x000078D7u;

    public static Color HighlightText => new(highlightText);
    private const uint highlightText = 0x00FFFFFFu;

    public static Color Honeydew => new(honeydew);
    private const uint honeydew = 0x00F0FFF0u;

    public static Color HotPink => new(hotPink);
    private const uint hotPink = 0x00FF69B4u;

    public static Color HotTrack => new(hotTrack);
    private const uint hotTrack = 0x000066CCu;

    public static Color InactiveBorder => new(inactiveBorder);
    private const uint inactiveBorder = 0x00F4F7FCu;

    public static Color InactiveCaption => new(inactiveCaption);
    private const uint inactiveCaption = 0x00BFCDDBu;

    public static Color InactiveCaptionText => new(inactiveCaptionText);
    private const uint inactiveCaptionText = 0x00000000u;

    public static Color IndianRed => new(indianRed);
    private const uint indianRed = 0x00CD5C5Cu;

    public static Color Indigo => new(indigo);
    private const uint indigo = 0x004B0082u;

    public static Color Info => new(info);
    private const uint info = 0x00FFFFE1u;

    public static Color InfoText => new(infoText);
    private const uint infoText = 0x00000000u;

    public static Color Ivory => new(ivory);
    private const uint ivory = 0x00FFFFF0u;

    public static Color Khaki => new(khaki);
    private const uint khaki = 0x00F0E68Cu;

    public static Color Lavender => new(lavender);
    private const uint lavender = 0x00E6E6FAu;

    public static Color LavenderBlush => new(lavenderBlush);
    private const uint lavenderBlush = 0x00FFF0F5u;

    public static Color LawnGreen => new(lawnGreen);
    private const uint lawnGreen = 0x007CFC00u;

    public static Color LemonChiffon => new(lemonChiffon);
    private const uint lemonChiffon = 0x00FFFACDu;

    public static Color LightBlue => new(lightBlue);
    private const uint lightBlue = 0x00ADD8E6u;

    public static Color LightCoral => new(lightCoral);
    private const uint lightCoral = 0x00F08080u;

    public static Color LightCyan => new(lightCyan);
    private const uint lightCyan = 0x00E0FFFFu;

    public static Color LightGoldenrodYellow => new(lightGoldenrodYellow);
    private const uint lightGoldenrodYellow = 0x00FAFAD2u;

    public static Color LightGray => new(lightGray);
    private const uint lightGray = 0x00D3D3D3u;

    public static Color LightGreen => new(lightGreen);
    private const uint lightGreen = 0x0090EE90u;

    public static Color LightPink => new(lightPink);
    private const uint lightPink = 0x00FFB6C1u;

    public static Color LightSalmon => new(lightSalmon);
    private const uint lightSalmon = 0x00FFA07Au;

    public static Color LightSeaGreen => new(lightSeaGreen);
    private const uint lightSeaGreen = 0x0020B2AAu;

    public static Color LightSkyBlue => new(lightSkyBlue);
    private const uint lightSkyBlue = 0x0087CEFAu;

    public static Color LightSlateGray => new(lightSlateGray);
    private const uint lightSlateGray = 0x00778899u;

    public static Color LightSteelBlue => new(lightSteelBlue);
    private const uint lightSteelBlue = 0x00B0C4DEu;

    public static Color LightYellow => new(lightYellow);
    private const uint lightYellow = 0x00FFFFE0u;

    public static Color Lime => new(lime);
    private const uint lime = 0x0000FF00u;

    public static Color LimeGreen => new(limeGreen);
    private const uint limeGreen = 0x0032CD32u;

    public static Color Linen => new(linen);
    private const uint linen = 0x00FAF0E6u;

    public static Color Magenta => new(magenta);
    private const uint magenta = 0x00FF00FFu;

    public static Color Maroon => new(maroon);
    private const uint maroon = 0x00800000u;

    public static Color MediumAquamarine => new(mediumAquamarine);
    private const uint mediumAquamarine = 0x0066CDAAu;

    public static Color MediumBlue => new(mediumBlue);
    private const uint mediumBlue = 0x000000CDu;

    public static Color MediumOrchid => new(mediumOrchid);
    private const uint mediumOrchid = 0x00BA55D3u;

    public static Color MediumPurple => new(mediumPurple);
    private const uint mediumPurple = 0x009370DBu;

    public static Color MediumSeaGreen => new(mediumSeaGreen);
    private const uint mediumSeaGreen = 0x003CB371u;

    public static Color MediumSlateBlue => new(mediumSlateBlue);
    private const uint mediumSlateBlue = 0x007B68EEu;

    public static Color MediumSpringGreen => new(mediumSpringGreen);
    private const uint mediumSpringGreen = 0x0000FA9Au;

    public static Color MediumTurquoise => new(mediumTurquoise);
    private const uint mediumTurquoise = 0x0048D1CCu;

    public static Color MediumVioletRed => new(mediumVioletRed);
    private const uint mediumVioletRed = 0x00C71585u;

    public static Color Menu => new(menu);
    private const uint menu = 0x00F0F0F0u;

    public static Color MenuBar => new(menuBar);
    private const uint menuBar = 0x00F0F0F0u;

    public static Color MenuHighlight => new(menuHighlight);
    private const uint menuHighlight = 0x000078D7u;

    public static Color MenuText => new(menuText);
    private const uint menuText = 0x00000000u;

    public static Color MidnightBlue => new(midnightBlue);
    private const uint midnightBlue = 0x00191970u;

    public static Color MintCream => new(mintCream);
    private const uint mintCream = 0x00F5FFFAu;

    public static Color MistyRose => new(mistyRose);
    private const uint mistyRose = 0x00FFE4E1u;

    public static Color Moccasin => new(moccasin);
    private const uint moccasin = 0x00FFE4B5u;

    public static Color NavajoWhite => new(navajoWhite);
    private const uint navajoWhite = 0x00FFDEADu;

    public static Color Navy => new(navy);
    private const uint navy = 0x00000080u;

    public static Color OldLace => new(oldLace);
    private const uint oldLace = 0x00FDF5E6u;

    public static Color Olive => new(olive);
    private const uint olive = 0x00808000u;

    public static Color OliveDrab => new(oliveDrab);
    private const uint oliveDrab = 0x006B8E23u;

    public static Color Orange => new(orange);
    private const uint orange = 0x00FFA500u;

    public static Color OrangeRed => new(orangeRed);
    private const uint orangeRed = 0x00FF4500u;

    public static Color Orchid => new(orchid);
    private const uint orchid = 0x00DA70D6u;

    public static Color PaleGoldenrod => new(paleGoldenrod);
    private const uint paleGoldenrod = 0x00EEE8AAu;

    public static Color PaleGreen => new(paleGreen);
    private const uint paleGreen = 0x0098FB98u;

    public static Color PaleTurquoise => new(paleTurquoise);
    private const uint paleTurquoise = 0x00AFEEEEu;

    public static Color PaleVioletRed => new(paleVioletRed);
    private const uint paleVioletRed = 0x00DB7093u;

    public static Color PapayaWhip => new(papayaWhip);
    private const uint papayaWhip = 0x00FFEFD5u;

    public static Color PeachPuff => new(peachPuff);
    private const uint peachPuff = 0x00FFDAB9u;

    public static Color Peru => new(peru);
    private const uint peru = 0x00CD853Fu;

    public static Color Pink => new(pink);
    private const uint pink = 0x00FFC0CBu;

    public static Color Plum => new(plum);
    private const uint plum = 0x00DDA0DDu;

    public static Color PowderBlue => new(powderBlue);
    private const uint powderBlue = 0x00B0E0E6u;

    public static Color Purple => new(purple);
    private const uint purple = 0x00800080u;

    public static Color RebeccaPurple => new(rebeccaPurple);
    private const uint rebeccaPurple = 0x00663399u;

    public static Color Red => new(red);
    private const uint red = 0x00FF0000u;

    public static Color RosyBrown => new(rosyBrown);
    private const uint rosyBrown = 0x00BC8F8Fu;

    public static Color RoyalBlue => new(royalBlue);
    private const uint royalBlue = 0x004169E1u;

    public static Color SaddleBrown => new(saddleBrown);
    private const uint saddleBrown = 0x008B4513u;

    public static Color Salmon => new(salmon);
    private const uint salmon = 0x00FA8072u;

    public static Color SandyBrown => new(sandyBrown);
    private const uint sandyBrown = 0x00F4A460u;

    public static Color ScrollBar => new(scrollBar);
    private const uint scrollBar = 0x00C8C8C8u;

    public static Color SeaGreen => new(seaGreen);
    private const uint seaGreen = 0x002E8B57u;

    public static Color SeaShell => new(seaShell);
    private const uint seaShell = 0x00FFF5EEu;

    public static Color Sienna => new(sienna);
    private const uint sienna = 0x00A0522Du;

    public static Color Silver => new(silver);
    private const uint silver = 0x00C0C0C0u;

    public static Color SkyBlue => new(skyBlue);
    private const uint skyBlue = 0x0087CEEBu;

    public static Color SlateBlue => new(slateBlue);
    private const uint slateBlue = 0x006A5ACDu;

    public static Color SlateGray => new(slateGray);
    private const uint slateGray = 0x00708090u;

    public static Color Snow => new(snow);
    private const uint snow = 0x00FFFAFAu;

    public static Color SpringGreen => new(springGreen);
    private const uint springGreen = 0x0000FF7Fu;

    public static Color SteelBlue => new(steelBlue);
    private const uint steelBlue = 0x004682B4u;

    public static Color Tan => new(tan);
    private const uint tan = 0x00D2B48Cu;

    public static Color Teal => new(teal);
    private const uint teal = 0x00008080u;

    public static Color Thistle => new(thistle);
    private const uint thistle = 0x00D8BFD8u;

    public static Color Tomato => new(tomato);
    private const uint tomato = 0x00FF6347u;

    public static Color Transparent => new(transparent);
    private const uint transparent = 0x00FFFFFFu;

    public static Color Turquoise => new(turquoise);
    private const uint turquoise = 0x0040E0D0u;

    public static Color Violet => new(violet);
    private const uint violet = 0x00EE82EEu;
    
    public static Color Wheat => new(wheat);
    private const uint wheat = 0x00F5DEB3u;

    public static Color White => new(white);
    private const uint white = 0x00FFFFFFu;

    public static Color WhiteSmoke => new(whiteSmoke);
    private const uint whiteSmoke = 0x00F5F5F5u;

    public static Color Window => new(window);
    private const uint window = 0x00FFFFFFu;

    public static Color WindowFrame => new(windowFrame);
    private const uint windowFrame = 0x00646464u;

    public static Color WindowText => new(windowText);
    private const uint windowText = 0x00000000u;

    public static Color Yellow => new(yellow);
    private const uint yellow = 0x00FFFF00u;

    public static Color YellowGreen => new(yellowGreen);
    private const uint yellowGreen = 0x009ACD32u;


    /// <summary>
    /// Attempts to get the known color name for the specified color value.
    /// </summary>
    /// <param name="color">The color to find a known name for.</param>
    /// <param name="knownName">The known color name if found; otherwise, an empty string.</param>
    /// <returns><see langword="true"/> if a known color name was found for the specified color; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method performs a lookup against all predefined color constants to find a matching color name.
    /// The comparison is based on the exact RGB values of the color.
    /// </remarks>
    public static bool TryGetKnownColorName(Color color, out string knownName)
    {
        uint hexValue = color.ToUint();
        knownName = hexValue switch
        {
            activeBorder => nameof(ActiveBorder),
            activeCaption => nameof(ActiveCaption),
            aliceBlue => nameof(AliceBlue),
            antiqueWhite => nameof(AntiqueWhite),
            appWorkspace => nameof(AppWorkspace),
            aquamarine => nameof(Aquamarine),
            azure => nameof(Azure),
            beige => nameof(Beige),
            bisque => nameof(Bisque),
            black => nameof(Black),
            blanchedAlmond => nameof(BlanchedAlmond),
            blue => nameof(Blue),
            blueViolet => nameof(BlueViolet),
            brown => nameof(Brown),
            burlyWood => nameof(BurlyWood),
            buttonFace => nameof(ButtonFace),
            buttonShadow => nameof(ButtonShadow),
            cadetBlue => nameof(CadetBlue),
            chartreuse => nameof(Chartreuse),
            chocolate => nameof(Chocolate),
            controlLight => nameof(ControlLight),
            coral => nameof(Coral),
            cornflowerBlue => nameof(CornflowerBlue),
            cornsilk => nameof(Cornsilk),
            crimson => nameof(Crimson),
            cyan => nameof(Cyan),
            darkBlue => nameof(DarkBlue),
            darkCyan => nameof(DarkCyan),
            darkGoldenrod => nameof(DarkGoldenrod),
            darkGray => nameof(DarkGray),
            darkGreen => nameof(DarkGreen),
            darkKhaki => nameof(DarkKhaki),
            darkMagenta => nameof(DarkMagenta),
            darkOliveGreen => nameof(DarkOliveGreen),
            darkOrange => nameof(DarkOrange),
            darkOrchid => nameof(DarkOrchid),
            darkRed => nameof(DarkRed),
            darkSalmon => nameof(DarkSalmon),
            darkSeaGreen => nameof(DarkSeaGreen),
            darkSlateBlue => nameof(DarkSlateBlue),
            darkSlateGray => nameof(DarkSlateGray),
            darkTurquoise => nameof(DarkTurquoise),
            darkViolet => nameof(DarkViolet),
            deepPink => nameof(DeepPink),
            deepSkyBlue => nameof(DeepSkyBlue),
            dimGray => nameof(DimGray),
            dodgerBlue => nameof(DodgerBlue),
            firebrick => nameof(Firebrick),
            floralWhite => nameof(FloralWhite),
            forestGreen => nameof(ForestGreen),
            gainsboro => nameof(Gainsboro),
            ghostWhite => nameof(GhostWhite),
            gold => nameof(Gold),
            goldenrod => nameof(Goldenrod),
            gradientActiveCaption => nameof(GradientActiveCaption),
            gradientInactiveCaption => nameof(GradientInactiveCaption),
            gray => nameof(Gray),
            grayText => nameof(GrayText),
            green => nameof(Green),
            greenYellow => nameof(GreenYellow),
            highlight => nameof(Highlight),
            honeydew => nameof(Honeydew),
            hotPink => nameof(HotPink),
            hotTrack => nameof(HotTrack),
            inactiveBorder => nameof(InactiveBorder),
            inactiveCaption => nameof(InactiveCaption),
            indianRed => nameof(IndianRed),
            indigo => nameof(Indigo),
            info => nameof(Info),
            ivory => nameof(Ivory),
            khaki => nameof(Khaki),
            lavender => nameof(Lavender),
            lavenderBlush => nameof(LavenderBlush),
            lawnGreen => nameof(LawnGreen),
            lemonChiffon => nameof(LemonChiffon),
            lightBlue => nameof(LightBlue),
            lightCoral => nameof(LightCoral),
            lightCyan => nameof(LightCyan),
            lightGoldenrodYellow => nameof(LightGoldenrodYellow),
            lightGray => nameof(LightGray),
            lightGreen => nameof(LightGreen),
            lightPink => nameof(LightPink),
            lightSalmon => nameof(LightSalmon),
            lightSeaGreen => nameof(LightSeaGreen),
            lightSkyBlue => nameof(LightSkyBlue),
            lightSlateGray => nameof(LightSlateGray),
            lightSteelBlue => nameof(LightSteelBlue),
            lightYellow => nameof(LightYellow),
            lime => nameof(Lime),
            limeGreen => nameof(LimeGreen),
            linen => nameof(Linen),
            magenta => nameof(Magenta),
            maroon => nameof(Maroon),
            mediumAquamarine => nameof(MediumAquamarine),
            mediumBlue => nameof(MediumBlue),
            mediumOrchid => nameof(MediumOrchid),
            mediumPurple => nameof(MediumPurple),
            mediumSeaGreen => nameof(MediumSeaGreen),
            mediumSlateBlue => nameof(MediumSlateBlue),
            mediumSpringGreen => nameof(MediumSpringGreen),
            mediumTurquoise => nameof(MediumTurquoise),
            mediumVioletRed => nameof(MediumVioletRed),
            midnightBlue => nameof(MidnightBlue),
            mintCream => nameof(MintCream),
            mistyRose => nameof(MistyRose),
            moccasin => nameof(Moccasin),
            navajoWhite => nameof(NavajoWhite),
            navy => nameof(Navy),
            oldLace => nameof(OldLace),
            olive => nameof(Olive),
            oliveDrab => nameof(OliveDrab),
            orange => nameof(Orange),
            orangeRed => nameof(OrangeRed),
            orchid => nameof(Orchid),
            paleGoldenrod => nameof(PaleGoldenrod),
            paleGreen => nameof(PaleGreen),
            paleTurquoise => nameof(PaleTurquoise),
            paleVioletRed => nameof(PaleVioletRed),
            papayaWhip => nameof(PapayaWhip),
            peachPuff => nameof(PeachPuff),
            peru => nameof(Peru),
            pink => nameof(Pink),
            plum => nameof(Plum),
            powderBlue => nameof(PowderBlue),
            purple => nameof(Purple),
            rebeccaPurple => nameof(RebeccaPurple),
            red => nameof(Red),
            rosyBrown => nameof(RosyBrown),
            royalBlue => nameof(RoyalBlue),
            saddleBrown => nameof(SaddleBrown),
            salmon => nameof(Salmon),
            sandyBrown => nameof(SandyBrown),
            scrollBar => nameof(ScrollBar),
            seaGreen => nameof(SeaGreen),
            seaShell => nameof(SeaShell),
            sienna => nameof(Sienna),
            silver => nameof(Silver),
            skyBlue => nameof(SkyBlue),
            slateBlue => nameof(SlateBlue),
            slateGray => nameof(SlateGray),
            snow => nameof(Snow),
            springGreen => nameof(SpringGreen),
            steelBlue => nameof(SteelBlue),
            tan => nameof(Tan),
            teal => nameof(Teal),
            thistle => nameof(Thistle),
            tomato => nameof(Tomato),
            turquoise => nameof(Turquoise),
            violet => nameof(Violet),
            wheat => nameof(Wheat),
            white => nameof(White),
            whiteSmoke => nameof(WhiteSmoke),  
            windowFrame => nameof(WindowFrame),
            yellow => nameof(Yellow),
            yellowGreen => nameof(YellowGreen),
            _ => ""
        };
        return knownName.Length > 0;
    }

}
