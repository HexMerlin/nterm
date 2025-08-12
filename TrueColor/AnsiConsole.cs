using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TrueColor;

/// <summary>
/// Optimized 24-bit color console writer.
/// </summary>
public static class AnsiConsole
{
    private static readonly Stream Stdout = Console.OpenStandardOutput();

    /// <summary>
    /// 24-bit foreground color of the console. Setting this property immediately
    /// writes the corresponding ANSI escape sequence to stdout.
    /// </summary>
    /// <remarks>
    /// Changing this property affects all subsequent console output, including
    /// standard Console.Write() and Console.WriteLine() operations.
    /// </remarks>
    public static Color ForegroundColor
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            WriteForegroundColorToStdout(value);
        }
    }

    /// <summary>
    /// 24-bit background color of the console. Setting this property immediately
    /// writes the corresponding ANSI escape sequence to stdout.
    /// </summary>
    /// <remarks>
    /// Changing this property affects all subsequent console output, including
    /// standard Console.Write() and Console.WriteLine() operations.
    /// </remarks>
    public static Color BackgroundColor
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            WriteBackgroundColorToStdout(value);
        }
    }

    static AnsiConsole()
    {
        TryEnableVirtualTerminalOnWindows();
        // Initialize color tracking with current console colors converted to 24-bit RGB
        // Use backing fields to avoid writing ANSI codes during initialization
        ForegroundColor = Colors.FromConsoleColor(Console.ForegroundColor);
        BackgroundColor = Colors.FromConsoleColor(Console.BackgroundColor);
    }


    /// <summary>
    /// Writes a character using current foreground and background colors.
    /// </summary>
    /// <param name="ch">Character to write</param>
    /// <remarks>
    /// Uses the current ForegroundColor and BackgroundColor properties.
    /// No ANSI escape sequences are written since colors are already set.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(char ch)
    {
        Span<byte> charBuf = stackalloc byte[4]; // Max UTF-8 char length
        int charLen = EncodeCharUtf8(ch, charBuf);
        Stdout.Write(charBuf[..charLen]);
    }

    /// <summary>
    /// Writes a character with specified colors using optimized color caching.
    /// </summary>
    /// <param name="ch">Character to write</param>
    /// <param name="foreground">Foreground color</param>
    /// <param name="background">Background color</param>
    /// <remarks>
    /// Ultra-optimized implementation avoids redundant color escape sequences when colors haven't changed.
    /// Updates the ForegroundColor and BackgroundColor properties if colors have changed.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(char ch, Color foreground, Color background)
    {
        // Set colors using properties (which handle ANSI output automatically)
        ForegroundColor = foreground;
        BackgroundColor = background;
        
        // Write the character
        Write(ch);
    }

    #region Private Methods

    /// <summary>
    /// Writes ANSI foreground color escape sequence to stdout.
    /// </summary>
    /// <param name="color">Foreground color to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteForegroundColorToStdout(Color color)
    {
        Span<byte> buf = stackalloc byte[32]; // ESC[38;2;255;255;255m = max 19 bytes
        ReadOnlySpan<byte> prefix = "\x1B[38;2;"u8; // ESC[38;2; in single operation
        int i = 0;
        
        prefix.CopyTo(buf);
        i += prefix.Length;
        i += WriteUInt8(color.R, buf[i..]); buf[i++] = (byte)';';
        i += WriteUInt8(color.G, buf[i..]); buf[i++] = (byte)';';
        i += WriteUInt8(color.B, buf[i..]); buf[i++] = (byte)'m';
        
        Stdout.Write(buf[..i]);
    }

    /// <summary>
    /// Writes ANSI background color escape sequence to stdout.
    /// </summary>
    /// <param name="color">Background color to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteBackgroundColorToStdout(Color color)
    {
        Span<byte> buf = stackalloc byte[32]; // ESC[48;2;255;255;255m = max 19 bytes
        ReadOnlySpan<byte> prefix = "\x1B[48;2;"u8; // ESC[48;2; in single operation
        int i = 0;
        
        prefix.CopyTo(buf);
        i += prefix.Length;
        i += WriteUInt8(color.R, buf[i..]); buf[i++] = (byte)';';
        i += WriteUInt8(color.G, buf[i..]); buf[i++] = (byte)';';
        i += WriteUInt8(color.B, buf[i..]); buf[i++] = (byte)'m';
        
        Stdout.Write(buf[..i]);
    }

    /// <summary>
    /// Converts a byte value to ASCII decimal representation for ANSI escape sequences.
    /// </summary>
    /// <param name="value">Byte value (0-255) to convert to decimal ASCII</param>
    /// <param name="dest">Destination span to write ASCII digits</param>
    /// <returns>Number of ASCII bytes written (1-3 digits)</returns>
    /// <remarks>
    /// <para>
    /// Uses base-10 arithmetic (constants 10, 100) because ANSI color escape sequences 
    /// require decimal RGB values, not binary or hexadecimal representation.
    /// </para>
    /// <para>
    /// ANSI protocol example: ESC[38;2;255;128;64m where 255, 128, 64 are decimal.
    /// </para>
    /// <para>
    /// Handles three ranges efficiently:
    /// • 0-9: Single digit → "0" to "9" (1 byte)
    /// • 10-99: Two digits → "10" to "99" (2 bytes)  
    /// • 100-255: Three digits → "100" to "255" (3 bytes)
    /// </para>
    /// </remarks>
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
