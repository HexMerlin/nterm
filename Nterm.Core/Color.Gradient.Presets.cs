using System;
using System.Collections.Generic;
using System.Text;

namespace Nterm.Core;
public readonly partial struct Color
{
    /// <summary>
    /// Electric blue to neon magenta to cyber yellow; ultra-bright, high-contrast neon sweep.
    /// </summary>
    public static Color[] GradientNeonSunburst => [new Color(0, 166, 255), new Color(255, 0, 200), new Color(255, 230, 0)];

    /// <summary>
    /// Aqua to hot pink to neon orange; tropical neon blend with vivid warmth in the highlights.
    /// </summary>
    public static Color[] GradientNeonTropic => [new Color(0, 255, 240), new Color(255, 0, 168), new Color(255, 77, 0)];

    /// <summary>
    /// Red-pink to amber to electric violet to aqua-green; fast, saturated spectral wrap with crisp transitions.
    /// </summary>
    public static Color[] GradientPrismRush => [new Color(255, 0, 80), new Color(255, 168, 0), new Color(122, 0, 255), new Color(0, 255, 213)];

    /// <summary>
    /// Cyan to light gray to magenta to light gray to acid green; vivid color bursts separated by soft neutral bridges.
    /// </summary>
    public static Color[] GradientVaporSignal => [new Color(0, 240, 255), new Color(176, 176, 176), new Color(255, 0, 200), new Color(176, 176, 176), new Color(0, 255, 59)];

    /// <summary>
    /// Red to cyan to yellow; bold primary-adjacent triad with bright, saturated mids and highs.
    /// </summary>
    public static Color[] GradientChromaticShock => [new Color(255, 0, 0), new Color(0, 255, 255), new Color(255, 255, 0)];

    /// <summary>
    /// Deep indigo to soft pink; moody, luminous “purple paradise” vibe.
    /// </summary>
    public static Color[] GradientPurpleParadise => [new Color(29, 43, 100), new Color(248, 205, 218)];

    /// <summary>
    /// Ocean-navy to tropical aqua; cool, fresh, and glassy.
    /// </summary>
    public static Color[] GradientAquaMarine => [new Color(26, 41, 128), new Color(38, 208, 206)];

    /// <summary>
    /// Fiery orange-red into hot magenta; loud and energetic.
    /// </summary>
    public static Color[] GradientBloodyMary => [new Color(255, 81, 47), new Color(221, 36, 118)];

    /// <summary>
    /// Bright cherry red into warm coral; punchy and warm.
    /// </summary>
    public static Color[] GradientCherry => [new Color(235, 51, 73), new Color(244, 92, 67)];

    /// <summary>
    /// Deep teal dusk into blazing sunset orange; dramatic contrast.
    /// </summary>
    public static Color[] GradientSunset => [new Color(11, 72, 107), new Color(245, 98, 23)];

    /// <summary>
    /// Neon green to electric blue; crisp, modern “rainbow blue” arc.
    /// </summary>
    public static Color[] GradientRainbowBlue => [new Color(0, 242, 96), new Color(5, 117, 230)];

    /// <summary>
    /// Mint-green into steel blue; calm river tones with depth.
    /// </summary>
    public static Color[] GradientEndlessRiver => [new Color(67, 206, 162), new Color(24, 90, 157)];

    /// <summary>
    /// Vivid magenta to deep plum; rich, night-club violet.
    /// </summary>
    public static Color[] GradientAubergine => [new Color(170, 7, 107), new Color(97, 4, 95)];

    /// <summary>
    /// Charcoal blue to brushed silver; sleek, industrial neutral.
    /// </summary>
    public static Color[] GradientTitanium => [new Color(40, 48, 72), new Color(133, 147, 152)];

    /// <summary>
    /// Midnight teal → ocean slate → steel blue; cool, cinematic tri-tone.
    /// </summary>
    public static Color[] GradientMoonlitAsteroid => [new Color(15, 32, 39), new Color(32, 58, 67), new Color(44, 83, 100)];

}
