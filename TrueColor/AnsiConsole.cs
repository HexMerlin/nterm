using System;
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

    static AnsiConsole()
    {
        TryEnableVirtualTerminalOnWindows();
    }

    /// <summary>
    /// Writes a character with specified colors.
    /// </summary>
    /// <param name="ch">Character to write</param>
    /// <param name="fg">Foreground color</param>
    /// <param name="bg">Background color</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(char ch, Rgb fg, Rgb bg)
    {
        // Worst-case length fits comfortably in 64 bytes:
        // ESC[38;2;r;g;bmESC[48;2;r;g;bm<char>
        Span<byte> buf = stackalloc byte[64];
        int i = 0;

        // ESC[
        buf[i++] = 0x1B; buf[i++] = (byte)'[';

        // 38;2;r;g;bm
        i += WriteAscii("38;2;", buf[i..]);
        i += WriteUInt8(fg.R, buf[i..]); buf[i++] = (byte)';';
        i += WriteUInt8(fg.G, buf[i..]); buf[i++] = (byte)';';
        i += WriteUInt8(fg.B, buf[i..]); buf[i++] = (byte)'m';

        // ESC[
        buf[i++] = 0x1B; buf[i++] = (byte)'[';

        // 48;2;r;g;bm
        i += WriteAscii("48;2;", buf[i..]);
        i += WriteUInt8(bg.R, buf[i..]); buf[i++] = (byte)';';
        i += WriteUInt8(bg.G, buf[i..]); buf[i++] = (byte)';';
        i += WriteUInt8(bg.B, buf[i..]); buf[i++] = (byte)'m';

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
    }

    #region Private Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteAscii(string s, Span<byte> dest)
    {
        for (int j = 0; j < s.Length; j++) dest[j] = (byte)s[j];
        return s.Length;
    }

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
        if (!OperatingSystem.IsWindows()) return;

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
