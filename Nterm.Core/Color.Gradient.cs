#nullable enable
using System.Runtime.CompilerServices;

namespace Nterm.Core;

public readonly partial struct Color
{
  
    /// <summary>
    /// Generate a perceptual gradient (OKLCH piecewise-linear) over <paramref name="length"/> steps.
    /// Input <paramref name="stops"/> are treated as equidistant along the gradient.
    /// Alpha in stops is ignored; output always uses A=255 for terminal friendliness.
    /// </summary>
    /// <param name="length">Number of colors to produce (≥ 2).</param>
    /// <param name="stops">At least two colors; treated as equally spaced stops.</param>
    public static Color[] Gradient(int length, params ReadOnlySpan<Color> stops)
    {
        if (length < 2) throw new ArgumentException("length must be ≥ 2", nameof(length));
        if (stops.Length < 2) throw new ArgumentException("stops must contain ≥ 2 colors", nameof(stops));

        int m = stops.Length;
        int outLast = length - 1;
        int segLast = m - 2;

        var result = new Color[length];

        for (int k = 0; k < length; k++)
        {
            float u = outLast == 0 ? 0f : (float)k / outLast;

            // Map u to segment index i and local parameter τ
            float s = u * (m - 1);
            int i = (int)MathF.Floor(s);
            if (i > segLast) i = segLast;          // guard for u=1
            float τ = s - i;

            // Convert the two adjacent stops to OKLCH (on-the-fly to avoid allocations)
            var c0 = stops[i];
            var c1 = stops[i + 1];

            ToOKLCH(c0, out float L0, out float C0, out float h0);
            ToOKLCH(c1, out float L1, out float C1, out float h1);

            // Linear in L and C
            float L = L0 + (L1 - L0) * τ;
            float C = C0 + (C1 - C0) * τ;

            // Hue interpolation with robust gray handling
            float h;
            if (C0 < C_Eps && C1 >= C_Eps)
            {
                h = h1; // carry chromatic hue across gray
            }
            else if (C0 >= C_Eps && C1 < C_Eps)
            {
                h = h0;
            }
            else if (C0 < C_Eps && C1 < C_Eps)
            {
                h = h0; // any constant hue is fine at near-gray
            }
            else
            {
                h = LerpHueShortest(h0, h1, τ);
            }

            result[k] = FromOKLCH_GamutMapped(L, C, h);
        }

        return result;
    }

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


    // ========= Internal implementation details =========

    private const float C_Eps = 5e-4f;        // chroma epsilon for gray handling in OKLCH
    private const float PI = MathF.PI;
    private const float Deg2Rad = PI / 180f;
    private const float Rad2Deg = 180f / PI;

    // -- sRGB companding (float32)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float SrgbToLinear(float v)
        => v <= 0.04045f ? v / 12.92f : MathF.Pow((v + 0.055f) / 1.055f, 2.4f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float LinearToSrgb(float v)
        => v <= 0.0031308f ? v * 12.92f : 1.055f * MathF.Pow(v, 1f / 2.4f) - 0.055f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ToByte01(float v)
    {
        v = Clamp01(v);
        // Round-to-nearest
        int i = (int)(v * 255f + 0.5f);
        if ((uint)i > 255u) i = i < 0 ? 0 : 255;
        return (byte)i;
    }

    // -- OKLab <-> linear sRGB (Björn Ottosson, public domain constants)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LinearRgbToOklab(float r, float g, float b, out float L, out float a, out float c_b)
    {
        // LMS
        float l = 0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b;
        float m = 0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b;
        float s = 0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b;

        float l_ = MathF.Cbrt(l);
        float m_ = MathF.Cbrt(m);
        float s_ = MathF.Cbrt(s);

        L = 0.2104542553f * l_ + 0.7936177850f * m_ - 0.0040720468f * s_;
        a = 1.9779984951f * l_ - 2.4285922050f * m_ + 0.4505937099f * s_;
        c_b = 0.0259040371f * l_ + 0.7827717662f * m_ - 0.8086757660f * s_;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OklabToLinearRgb(float L, float a, float b, out float r, out float g, out float bl)
    {
        float l_ = L + 0.3963377774f * a + 0.2158037573f * b;
        float m_ = L - 0.1055613458f * a - 0.0638541728f * b;
        float s_ = L - 0.0894841775f * a - 1.2914855480f * b;

        float l = l_ * l_ * l_;
        float m = m_ * m_ * m_;
        float s = s_ * s_ * s_;

        r = +4.0767416621f * l - 3.3077115913f * m + 0.2309699292f * s;
        g = -1.2684380046f * l + 2.6097574011f * m - 0.3413193965f * s;
        bl = -0.0041960863f * l - 0.7034186147f * m + 1.7076147010f * s;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OklabToOklch(float L, float a, float b, out float oL, out float C, out float hDeg)
    {
        oL = L;
        C = MathF.Sqrt(a * a + b * b);
        if (C < C_Eps)
        {
            hDeg = 0f; // arbitrary when near gray
        }
        else
        {
            float h = MathF.Atan2(b, a) * Rad2Deg;
            if (h < 0f) h += 360f;
            hDeg = h;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OklchToOklab(float L, float C, float hDeg, out float oL, out float a, out float b)
    {
        oL = L;
        float hRad = hDeg * Deg2Rad;
        float cos = MathF.Cos(hRad);
        float sin = MathF.Sin(hRad);
        a = C * cos;
        b = C * sin;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SrgbToOklch(byte r8, byte g8, byte b8, out float L, out float C, out float hDeg)
    {
        float r = SrgbToLinear(r8 / 255f);
        float g = SrgbToLinear(g8 / 255f);
        float b = SrgbToLinear(b8 / 255f);
        LinearRgbToOklab(r, g, b, out float oL, out float a, out float bb);
        OklabToOklch(oL, a, bb, out L, out C, out hDeg);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ToOKLCH(in Color c, out float L, out float C, out float hDeg)
        => SrgbToOklch(c.R, c.G, c.B, out L, out C, out hDeg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool InGamut01(float rLin, float gLin, float bLin)
        => rLin >= 0f && rLin <= 1f && gLin >= 0f && gLin <= 1f && bLin >= 0f && bLin <= 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float LerpHueShortest(float h0, float h1, float t)
    {
        float dh = ((h1 - h0 + 540f) % 360f) - 180f;
        float h = h0 + t * dh;
        h %= 360f;
        if (h < 0f) h += 360f;
        return h;
    }

    /// <summary>
    /// Convert OKLCH to sRGB with chroma-scaled gamut mapping (capped bisection).
    /// </summary>
    private static Color FromOKLCH_GamutMapped(float L, float C, float hDeg)
    {
        // Try full chroma first
        OklchToOklab(L, C, hDeg, out float oL, out float oa, out float ob);
        OklabToLinearRgb(oL, oa, ob, out float r, out float g, out float b);

        if (!InGamut01(r, g, b))
        {
            // Scale chroma C by s ∈ [0,1] using bisection (8 iters is plenty)
            float lo = 0f, hi = 1f;
            for (int iter = 0; iter < 8; iter++)
            {
                float mid = 0.5f * (lo + hi);
                OklchToOklab(L, mid * C, hDeg, out float Lm, out float am, out float bm);
                OklabToLinearRgb(Lm, am, bm, out float rM, out float gM, out float bM);
                if (InGamut01(rM, gM, bM)) { lo = mid; r = rM; g = gM; b = bM; }
                else hi = mid;
            }
            // r,g,b already hold the best in-gamut (lo) values
            r = Clamp01(r); g = Clamp01(g); b = Clamp01(b);
        }

        // Linear -> sRGB encode
        float sr = LinearToSrgb(r);
        float sg = LinearToSrgb(g);
        float sb = LinearToSrgb(b);

        return new Color(ToByte01(sr), ToByte01(sg), ToByte01(sb), 255);
    }
}
