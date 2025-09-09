namespace NTerm.Core;

public readonly partial struct Color
{
    public static Color Transparent => new(transparent);
    private const uint transparent = 0x00000000u; // ARGB with 0 alpha

    public static Color ActiveBorder => new(activeBorder);
    private const uint activeBorder = 0xFFB4B4B4u;

    public static Color ActiveCaption => new(activeCaption);
    private const uint activeCaption = 0xFF99B4D1u;

    public static Color ActiveCaptionText => new(activeCaptionText);
    private const uint activeCaptionText = 0xFF000000u;

    public static Color AliceBlue => new(aliceBlue);
    private const uint aliceBlue = 0xFFF0F8FFu;

    public static Color AntiqueWhite => new(antiqueWhite);
    private const uint antiqueWhite = 0xFFFAEBD7u;

    public static Color AppWorkspace => new(appWorkspace);
    private const uint appWorkspace = 0xFFABABABu;

    public static Color Aqua => new(aqua);
    private const uint aqua = 0xFF00FFFFu;

    public static Color Aquamarine => new(aquamarine);
    private const uint aquamarine = 0xFF7FFFD4u;

    public static Color Azure => new(azure);
    private const uint azure = 0xFFF0FFFFu;

    public static Color Beige => new(beige);
    private const uint beige = 0xFFF5F5DCu;

    public static Color Bisque => new(bisque);
    private const uint bisque = 0xFFFFE4C4u;

    public static Color Black => new(black);
    private const uint black = 0xFF000000u;

    public static Color BlanchedAlmond => new(blanchedAlmond);
    private const uint blanchedAlmond = 0xFFFFEBCDu;

    public static Color Blue => new(blue);
    private const uint blue = 0xFF0000FFu;

    public static Color BlueViolet => new(blueViolet);
    private const uint blueViolet = 0xFF8A2BE2u;

    public static Color Brown => new(brown);
    private const uint brown = 0xFFA52A2Au;

    public static Color BurlyWood => new(burlyWood);
    private const uint burlyWood = 0xFFDEB887u;

    public static Color ButtonFace => new(buttonFace);
    private const uint buttonFace = 0xFFF0F0F0u;

    public static Color ButtonHighlight => new(buttonHighlight);
    private const uint buttonHighlight = 0xFFFFFFFFu;

    public static Color ButtonShadow => new(buttonShadow);
    private const uint buttonShadow = 0xFFA0A0A0u;

    public static Color CadetBlue => new(cadetBlue);
    private const uint cadetBlue = 0xFF5F9EA0u;

    public static Color Chartreuse => new(chartreuse);
    private const uint chartreuse = 0xFF7FFF00u;

    public static Color Chocolate => new(chocolate);
    private const uint chocolate = 0xFFD2691Eu;

    public static Color Control => new(control);
    private const uint control = 0xFFF0F0F0u;

    public static Color ControlDark => new(controlDark);
    private const uint controlDark = 0xFFA0A0A0u;

    public static Color ControlDarkDark => new(controlDarkDark);
    private const uint controlDarkDark = 0xFF696969u;

    public static Color ControlLight => new(controlLight);
    private const uint controlLight = 0xFFE3E3E3u;

    public static Color ControlLightLight => new(controlLightLight);
    private const uint controlLightLight = 0xFFFFFFFFu;

    public static Color ControlText => new(controlText);
    private const uint controlText = 0xFF000000u;

    public static Color Coral => new(coral);
    private const uint coral = 0xFFFF7F50u;

    public static Color CornflowerBlue => new(cornflowerBlue);
    private const uint cornflowerBlue = 0xFF6495EDu;

    public static Color Cornsilk => new(cornsilk);
    private const uint cornsilk = 0xFFFFF8DCu;

    public static Color Crimson => new(crimson);
    private const uint crimson = 0xFFDC143Cu;

    public static Color Cyan => new(cyan);
    private const uint cyan = 0xFF00FFFFu;

    public static Color DarkBlue => new(darkBlue);
    private const uint darkBlue = 0xFF00008Bu;

    public static Color DarkCyan => new(darkCyan);
    private const uint darkCyan = 0xFF008B8Bu;

    public static Color DarkGoldenrod => new(darkGoldenrod);
    private const uint darkGoldenrod = 0xFFB8860Bu;

    public static Color DarkGray => new(darkGray);
    private const uint darkGray = 0xFFA9A9A9u;

    public static Color DarkGreen => new(darkGreen);
    private const uint darkGreen = 0xFF006400u;

    public static Color DarkKhaki => new(darkKhaki);
    private const uint darkKhaki = 0xFFBDB76Bu;

    public static Color DarkMagenta => new(darkMagenta);
    private const uint darkMagenta = 0xFF8B008Bu;

    public static Color DarkOliveGreen => new(darkOliveGreen);
    private const uint darkOliveGreen = 0xFF556B2Fu;

    public static Color DarkOrange => new(darkOrange);
    private const uint darkOrange = 0xFFFF8C00u;

    public static Color DarkOrchid => new(darkOrchid);
    private const uint darkOrchid = 0xFF9932CCu;

    public static Color DarkRed => new(darkRed);
    private const uint darkRed = 0xFF8B0000u;

    public static Color DarkSalmon => new(darkSalmon);
    private const uint darkSalmon = 0xFFE9967Au;

    public static Color DarkSeaGreen => new(darkSeaGreen);
    private const uint darkSeaGreen = 0xFF8FBC8Fu;

    public static Color DarkSlateBlue => new(darkSlateBlue);
    private const uint darkSlateBlue = 0xFF483D8Bu;

    public static Color DarkSlateGray => new(darkSlateGray);
    private const uint darkSlateGray = 0xFF2F4F4Fu;

    public static Color DarkTurquoise => new(darkTurquoise);
    private const uint darkTurquoise = 0xFF00CED1u;

    public static Color DarkViolet => new(darkViolet);
    private const uint darkViolet = 0xFF9400D3u;

    public static Color DeepPink => new(deepPink);
    private const uint deepPink = 0xFFFF1493u;

    public static Color DeepSkyBlue => new(deepSkyBlue);
    private const uint deepSkyBlue = 0xFF00BFFFu;

    public static Color Desktop => new(desktop);
    private const uint desktop = 0xFF000000u;

    public static Color DimGray => new(dimGray);
    private const uint dimGray = 0xFF696969u;

    public static Color DodgerBlue => new(dodgerBlue);
    private const uint dodgerBlue = 0xFF1E90FFu;

    public static Color Firebrick => new(firebrick);
    private const uint firebrick = 0xFFB22222u;

    public static Color FloralWhite => new(floralWhite);
    private const uint floralWhite = 0xFFFFFAF0u;

    public static Color ForestGreen => new(forestGreen);
    private const uint forestGreen = 0xFF228B22u;

    public static Color Fuchsia => new(fuchsia);
    private const uint fuchsia = 0xFFFF00FFu;

    public static Color Gainsboro => new(gainsboro);
    private const uint gainsboro = 0xFFDCDCDCu;

    public static Color GhostWhite => new(ghostWhite);
    private const uint ghostWhite = 0xFFF8F8FFu;

    public static Color Gold => new(gold);
    private const uint gold = 0xFFFFD700u;

    public static Color Goldenrod => new(goldenrod);
    private const uint goldenrod = 0xFFDAA520u;

    public static Color GradientActiveCaption => new(gradientActiveCaption);
    private const uint gradientActiveCaption = 0xFFB9D1EAu;

    public static Color GradientInactiveCaption => new(gradientInactiveCaption);
    private const uint gradientInactiveCaption = 0xFFD7E4F2u;

    public static Color Gray => new(gray);
    private const uint gray = 0xFF808080u;

    public static Color GrayText => new(grayText);
    private const uint grayText = 0xFF6D6D6Du;

    public static Color Green => new(green);
    private const uint green = 0xFF008000u;

    public static Color GreenYellow => new(greenYellow);
    private const uint greenYellow = 0xFFADFF2Fu;

    public static Color Highlight => new(highlight);
    private const uint highlight = 0xFF0078D7u;

    public static Color HighlightText => new(highlightText);
    private const uint highlightText = 0xFFFFFFFFu;

    public static Color Honeydew => new(honeydew);
    private const uint honeydew = 0xFFF0FFF0u;

    public static Color HotPink => new(hotPink);
    private const uint hotPink = 0xFFFF69B4u;

    public static Color HotTrack => new(hotTrack);
    private const uint hotTrack = 0xFF0066CCu;

    public static Color InactiveBorder => new(inactiveBorder);
    private const uint inactiveBorder = 0xFFF4F7FCu;

    public static Color InactiveCaption => new(inactiveCaption);
    private const uint inactiveCaption = 0xFFBFCDDBu;

    public static Color InactiveCaptionText => new(inactiveCaptionText);
    private const uint inactiveCaptionText = 0xFF000000u;

    public static Color IndianRed => new(indianRed);
    private const uint indianRed = 0xFFCD5C5Cu;

    public static Color Indigo => new(indigo);
    private const uint indigo = 0xFF4B0082u;

    public static Color Info => new(info);
    private const uint info = 0xFFFFFFE1u;

    public static Color InfoText => new(infoText);
    private const uint infoText = 0xFF000000u;

    public static Color Ivory => new(ivory);
    private const uint ivory = 0xFFFFFFF0u;

    public static Color Khaki => new(khaki);
    private const uint khaki = 0xFFF0E68Cu;

    public static Color Lavender => new(lavender);
    private const uint lavender = 0xFFE6E6FAu;

    public static Color LavenderBlush => new(lavenderBlush);
    private const uint lavenderBlush = 0xFFFFF0F5u;

    public static Color LawnGreen => new(lawnGreen);
    private const uint lawnGreen = 0xFF7CFC00u;

    public static Color LemonChiffon => new(lemonChiffon);
    private const uint lemonChiffon = 0xFFFFFACDu;

    public static Color LightBlue => new(lightBlue);
    private const uint lightBlue = 0xFFADD8E6u;

    public static Color LightCoral => new(lightCoral);
    private const uint lightCoral = 0xFFF08080u;

    public static Color LightCyan => new(lightCyan);
    private const uint lightCyan = 0xFFE0FFFFu;

    public static Color LightGoldenrodYellow => new(lightGoldenrodYellow);
    private const uint lightGoldenrodYellow = 0xFFFAFAD2u;

    public static Color LightGray => new(lightGray);
    private const uint lightGray = 0xFFD3D3D3u;

    public static Color LightGreen => new(lightGreen);
    private const uint lightGreen = 0xFF90EE90u;

    public static Color LightPink => new(lightPink);
    private const uint lightPink = 0xFFFFB6C1u;

    public static Color LightSalmon => new(lightSalmon);
    private const uint lightSalmon = 0xFFFFA07Au;

    public static Color LightSeaGreen => new(lightSeaGreen);
    private const uint lightSeaGreen = 0xFF20B2AAu;

    public static Color LightSkyBlue => new(lightSkyBlue);
    private const uint lightSkyBlue = 0xFF87CEFAu;

    public static Color LightSlateGray => new(lightSlateGray);
    private const uint lightSlateGray = 0xFF778899u;

    public static Color LightSteelBlue => new(lightSteelBlue);
    private const uint lightSteelBlue = 0xFFB0C4DEu;

    public static Color LightYellow => new(lightYellow);
    private const uint lightYellow = 0xFFFFFFE0u;

    public static Color Lime => new(lime);
    private const uint lime = 0xFF00FF00u;

    public static Color LimeGreen => new(limeGreen);
    private const uint limeGreen = 0xFF32CD32u;

    public static Color Linen => new(linen);
    private const uint linen = 0xFFFAF0E6u;

    public static Color Magenta => new(magenta);
    private const uint magenta = 0xFFFF00FFu;

    public static Color Maroon => new(maroon);
    private const uint maroon = 0xFF800000u;

    public static Color MediumAquamarine => new(mediumAquamarine);
    private const uint mediumAquamarine = 0xFF66CDAAu;

    public static Color MediumBlue => new(mediumBlue);
    private const uint mediumBlue = 0xFF0000CDu;

    public static Color MediumOrchid => new(mediumOrchid);
    private const uint mediumOrchid = 0xFFBA55D3u;

    public static Color MediumPurple => new(mediumPurple);
    private const uint mediumPurple = 0xFF9370DBu;

    public static Color MediumSeaGreen => new(mediumSeaGreen);
    private const uint mediumSeaGreen = 0xFF3CB371u;

    public static Color MediumSlateBlue => new(mediumSlateBlue);
    private const uint mediumSlateBlue = 0xFF7B68EEu;

    public static Color MediumSpringGreen => new(mediumSpringGreen);
    private const uint mediumSpringGreen = 0xFF00FA9Au;

    public static Color MediumTurquoise => new(mediumTurquoise);
    private const uint mediumTurquoise = 0xFF48D1CCu;

    public static Color MediumVioletRed => new(mediumVioletRed);
    private const uint mediumVioletRed = 0xFFC71585u;

    public static Color Menu => new(menu);
    private const uint menu = 0xFFF0F0F0u;

    public static Color MenuBar => new(menuBar);
    private const uint menuBar = 0xFFF0F0F0u;

    public static Color MenuHighlight => new(menuHighlight);
    private const uint menuHighlight = 0xFF0078D7u;

    public static Color MenuText => new(menuText);
    private const uint menuText = 0xFF000000u;

    public static Color MidnightBlue => new(midnightBlue);
    private const uint midnightBlue = 0xFF191970u;

    public static Color MintCream => new(mintCream);
    private const uint mintCream = 0xFFF5FFFAu;

    public static Color MistyRose => new(mistyRose);
    private const uint mistyRose = 0xFFFFE4E1u;

    public static Color Moccasin => new(moccasin);
    private const uint moccasin = 0xFFFFE4B5u;

    public static Color NavajoWhite => new(navajoWhite);
    private const uint navajoWhite = 0xFFFFDEADu;

    public static Color Navy => new(navy);
    private const uint navy = 0xFF000080u;

    public static Color OldLace => new(oldLace);
    private const uint oldLace = 0xFFFDF5E6u;

    public static Color Olive => new(olive);
    private const uint olive = 0xFF808000u;

    public static Color OliveDrab => new(oliveDrab);
    private const uint oliveDrab = 0xFF6B8E23u;

    public static Color Orange => new(orange);
    private const uint orange = 0xFFFFA500u;

    public static Color OrangeRed => new(orangeRed);
    private const uint orangeRed = 0xFFFF4500u;

    public static Color Orchid => new(orchid);
    private const uint orchid = 0xFFDA70D6u;

    public static Color PaleGoldenrod => new(paleGoldenrod);
    private const uint paleGoldenrod = 0xFFEEE8AAu;

    public static Color PaleGreen => new(paleGreen);
    private const uint paleGreen = 0xFF98FB98u;

    public static Color PaleTurquoise => new(paleTurquoise);
    private const uint paleTurquoise = 0xFFAFEEEEu;

    public static Color PaleVioletRed => new(paleVioletRed);
    private const uint paleVioletRed = 0xFFDB7093u;

    public static Color PapayaWhip => new(papayaWhip);
    private const uint papayaWhip = 0xFFFFEFD5u;

    public static Color PeachPuff => new(peachPuff);
    private const uint peachPuff = 0xFFFFDAB9u;

    public static Color Peru => new(peru);
    private const uint peru = 0xFFCD853Fu;

    public static Color Pink => new(pink);
    private const uint pink = 0xFFFFC0CBu;

    public static Color Plum => new(plum);
    private const uint plum = 0xFFDDA0DDu;

    public static Color PowderBlue => new(powderBlue);
    private const uint powderBlue = 0xFFB0E0E6u;

    public static Color Purple => new(purple);
    private const uint purple = 0xFF800080u;

    public static Color RebeccaPurple => new(rebeccaPurple);
    private const uint rebeccaPurple = 0xFF663399u;

    public static Color Red => new(red);
    private const uint red = 0xFFFF0000u;

    public static Color RosyBrown => new(rosyBrown);
    private const uint rosyBrown = 0xFFBC8F8Fu;

    public static Color RoyalBlue => new(royalBlue);
    private const uint royalBlue = 0xFF4169E1u;

    public static Color SaddleBrown => new(saddleBrown);
    private const uint saddleBrown = 0xFF8B4513u;

    public static Color Salmon => new(salmon);
    private const uint salmon = 0xFFFA8072u;

    public static Color SandyBrown => new(sandyBrown);
    private const uint sandyBrown = 0xFFF4A460u;

    public static Color ScrollBar => new(scrollBar);
    private const uint scrollBar = 0xFFC8C8C8u;

    public static Color SeaGreen => new(seaGreen);
    private const uint seaGreen = 0xFF2E8B57u;

    public static Color SeaShell => new(seaShell);
    private const uint seaShell = 0xFFFFF5EEu;

    public static Color Sienna => new(sienna);
    private const uint sienna = 0xFFA0522Du;

    public static Color Silver => new(silver);
    private const uint silver = 0xFFC0C0C0u;

    public static Color SkyBlue => new(skyBlue);
    private const uint skyBlue = 0xFF87CEEBu;

    public static Color SlateBlue => new(slateBlue);
    private const uint slateBlue = 0xFF6A5ACDu;

    public static Color SlateGray => new(slateGray);
    private const uint slateGray = 0xFF708090u;

    public static Color Snow => new(snow);
    private const uint snow = 0xFFFFFAFAu;

    public static Color SpringGreen => new(springGreen);
    private const uint springGreen = 0xFF00FF7Fu;

    public static Color SteelBlue => new(steelBlue);
    private const uint steelBlue = 0xFF4682B4u;

    public static Color Tan => new(tan);
    private const uint tan = 0xFFD2B48Cu;

    public static Color Teal => new(teal);
    private const uint teal = 0xFF008080u;

    public static Color Thistle => new(thistle);
    private const uint thistle = 0xFFD8BFD8u;

    public static Color Tomato => new(tomato);
    private const uint tomato = 0xFFFF6347u;

    public static Color Turquoise => new(turquoise);
    private const uint turquoise = 0xFF40E0D0u;

    public static Color Violet => new(violet);
    private const uint violet = 0xFFEE82EEu;

    public static Color Wheat => new(wheat);
    private const uint wheat = 0xFFF5DEB3u;

    public static Color White => new(white);
    private const uint white = 0xFFFFFFFFu;

    public static Color WhiteSmoke => new(whiteSmoke);
    private const uint whiteSmoke = 0xFFF5F5F5u;

    public static Color Window => new(window);
    private const uint window = 0xFFFFFFFFu;

    public static Color WindowFrame => new(windowFrame);
    private const uint windowFrame = 0xFF646464u;

    public static Color WindowText => new(windowText);
    private const uint windowText = 0xFF000000u;

    public static Color Yellow => new(yellow);
    private const uint yellow = 0xFFFFFF00u;

    public static Color YellowGreen => new(yellowGreen);
    private const uint yellowGreen = 0xFF9ACD32u;

    //Added colors in addition to System.Drawing.KnownColor

    public static Color ClarityBlue => new(clarityBlue);
    private const uint clarityBlue = 0xFF012456;

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
            transparent => nameof(Transparent),
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

            //added colors
            clarityBlue => nameof(ClarityBlue),
            _ => ""
        };
        return knownName.Length > 0;
    }
}
