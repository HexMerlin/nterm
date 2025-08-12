using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TrueColor;

/// <summary>
/// Optimized 24-bit color console writer.
/// </summary>
public static class AnsiConsole
{
    private static readonly Stream Stdout = Console.OpenStandardOutput();

    private static readonly ConsoleColor OriginalForegroundColor = Console.ForegroundColor;

    private static readonly ConsoleColor OriginalBackground = Console.BackgroundColor;

    /// <summary>
    /// Cached foreground color to avoid redundant escape sequences.
    /// </summary>
    private static Color? _cachedForeground;

    /// <summary>
    /// Cached background color to avoid redundant escape sequences.
    /// </summary>
    private static Color? _cachedBackground;

    /// <summary>
    /// Pre-computed byte sequences for common ANSI color prefixes.
    /// </summary>
    private static ReadOnlySpan<byte> ForegroundPrefix => "38;2;"u8;
    private static ReadOnlySpan<byte> BackgroundPrefix => "48;2;"u8;

    static AnsiConsole()
    {
        TryEnableVirtualTerminalOnWindows();
    }

    /// <summary>
    /// Writes a character with specified colors using optimized color caching.
    /// </summary>
    /// <param name="ch">Character to write</param>
    /// <param name="foreground">Foreground color</param>
    /// <param name="background">Background color</param>
    /// <remarks>
    /// Ultra-optimized implementation avoids redundant color escape sequences when colors haven't changed.
    /// Uses pre-computed lookup tables and minimal memory operations for maximum throughput.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(char ch, Color foreground, Color background)
    {
        // Check if colors have changed to avoid redundant escape sequences
        bool foregroundChanged = _cachedForeground != foreground;
        bool backgroundChanged = _cachedBackground != background;

        if (!foregroundChanged && !backgroundChanged)
        {
            // Only write the character - colors haven't changed
            Span<byte> charBuf = stackalloc byte[4]; // Max UTF-8 char length
            int charLen = EncodeCharUtf8(ch, charBuf);
            Stdout.Write(charBuf[..charLen]);
            return;
        }

        // Worst-case length: ESC[38;2;255;255;255mESC[48;2;255;255;255m + 4-byte UTF-8 char
        Span<byte> buf = stackalloc byte[64];
        int i = 0;

        // Write foreground color if changed
        if (foregroundChanged)
        {
            buf[i++] = 0x1B; buf[i++] = (byte)'[';
            ForegroundPrefix.CopyTo(buf[i..]);
            i += 5; // "38;2;" is 5 bytes
            i += WriteUInt8(foreground.R, buf[i..]); buf[i++] = (byte)';';
            i += WriteUInt8(foreground.G, buf[i..]); buf[i++] = (byte)';';
            i += WriteUInt8(foreground.B, buf[i..]); buf[i++] = (byte)'m';
            _cachedForeground = foreground;
        }

        // Write background color if changed
        if (backgroundChanged)
        {
            buf[i++] = 0x1B; buf[i++] = (byte)'[';
            BackgroundPrefix.CopyTo(buf[i..]);             
            i += 5; // "48;2;" is 5 bytes
            i += WriteUInt8(background.R, buf[i..]); buf[i++] = (byte)';';
            i += WriteUInt8(background.G, buf[i..]); buf[i++] = (byte)';';
            i += WriteUInt8(background.B, buf[i..]); buf[i++] = (byte)'m';
            _cachedBackground = background;
        }

        // char (UTF-8)
        i += EncodeCharUtf8(ch, buf[i..]);

        Stdout.Write(buf[..i]);
    }

    /// <summary>
    /// Restores colors of the standard Console to their initial values.
    /// </summary>
    /// <remarks>
    /// The standard Console does not keep an explicit memory for its set color, but depend on last ANSI codes written.
    /// Since this class writes ANSI codes to standard output, the color settings of the Console are inadvertently modified.
    /// This method restores them to their original colors.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RestoreOriginalColors()
    {
        Console.ForegroundColor = OriginalForegroundColor;
        Console.BackgroundColor = OriginalBackground;
        InvalidateColorCache();
    }

    /// <summary>
    /// Invalidates cached color state. Call when external operations may have changed console colors.
    /// </summary>
    /// <remarks>
    /// Forces next Write operation to output complete color escape sequences regardless of cached state.
    /// Use when Console colors have been modified outside of this class.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvalidateColorCache()
    {
        _cachedForeground = null;
        _cachedBackground = null;
    }

    #region Private Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteUInt8(byte value, Span<byte> dest)
    {
        if (value < 10) { dest[0] = (byte)('0' + value); return 1; }
        if (value < 100)
        {
            dest[0] = (byte)('0' + (value / 10));
            dest[1] = (byte)('0' + (value % 10));
            return 2;
        }
        int hundreds = value / 100;
        int rem = value - hundreds * 100;
        dest[0] = (byte)('0' + hundreds);
        dest[1] = (byte)('0' + (rem / 10));
        dest[2] = (byte)('0' + (rem % 10));
        return 3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EncodeCharUtf8(char ch, Span<byte> dest)
    {
        if (ch <= 0x7F) { dest[0] = (byte)ch; return 1; }
        if (ch < 0xD800 || ch > 0xDFFF)
        {
            if (ch <= 0x7FF)
            {
                dest[0] = (byte)(0xC0 | (ch >> 6));
                dest[1] = (byte)(0x80 | (ch & 0x3F));
                return 2;
            }
            dest[0] = (byte)(0xE0 | (ch >> 12));
            dest[1] = (byte)(0x80 | ((ch >> 6) & 0x3F));
            dest[2] = (byte)(0x80 | (ch & 0x3F));
            return 3;
        }
        dest[0] = (byte)'?';
        return 1;
    }

    private static void TryEnableVirtualTerminalOnWindows()
    {
        if (!OperatingSystem.IsWindows()) return; //this stuff is only required on Windows

        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        const int STD_OUTPUT_HANDLE = -11;

        nint h = GetStdHandle(STD_OUTPUT_HANDLE);
        if (h == nint.Zero) return;
        if (!GetConsoleMode(h, out uint mode)) return;
        _ = SetConsoleMode(h, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
    }

    [DllImport("kernel32.dll", SetLastError = true)] private static extern nint GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    #endregion Private Methods
}
