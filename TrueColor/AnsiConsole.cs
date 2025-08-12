using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TrueColor;

public static class AnsiConsole
{
    private static readonly Stream Stdout = Console.OpenStandardOutput();

    static AnsiConsole() => TryEnableVirtualTerminalOnWindows();

    /// <summary>
    /// Writes a single character with the specified foreground and background colors.
    /// Emits full SGR every call; no internal caching or state.
    /// </summary>
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
    /// Resets SGR to defaults (optional utility).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reset()
    {
        Span<byte> buf = stackalloc byte[4];
        buf[0] = 0x1B; buf[1] = (byte)'['; buf[2] = (byte)'0'; buf[3] = (byte)'m';
        Stdout.Write(buf);
    }

    // --- Helpers ---

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

    private static Rgb ToRgb(ConsoleColor c) => c switch
    {
        ConsoleColor.Black => new(0, 0, 0),
        ConsoleColor.DarkBlue => new(0, 0, 128),
        ConsoleColor.DarkGreen => new(0, 128, 0),
        ConsoleColor.DarkCyan => new(0, 128, 128),
        ConsoleColor.DarkRed => new(128, 0, 0),
        ConsoleColor.DarkMagenta => new(128, 0, 128),
        ConsoleColor.DarkYellow => new(128, 128, 0),
        ConsoleColor.Gray => new(192, 192, 192),
        ConsoleColor.DarkGray => new(128, 128, 128),
        ConsoleColor.Blue => new(0, 0, 255),
        ConsoleColor.Green => new(0, 255, 0),
        ConsoleColor.Cyan => new(0, 255, 255),
        ConsoleColor.Red => new(255, 0, 0),
        ConsoleColor.Magenta => new(255, 0, 255),
        ConsoleColor.Yellow => new(255, 255, 0),
        _ => new(255, 255, 255) // ConsoleColor.White
    };

    [DllImport("kernel32.dll", SetLastError = true)] private static extern nint GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);
}
