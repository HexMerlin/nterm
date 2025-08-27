using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SemanticTokens.Core;

/// <summary>
/// Optimized 24-bit color console writer.
/// </summary>
public static class Console
{
    private static readonly Stream Stdout = System.Console.OpenStandardOutput();

    private static readonly Lock writeLock = new();

    static Console()
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        WriteFg(Color.FromConsoleColor(System.Console.ForegroundColor));
        WriteBg(Color.FromConsoleColor(System.Console.BackgroundColor));
        TryEnableVirtualTerminalOnWindows();
    }

    public static string Title
    {
        get => OperatingSystem.IsWindows() ? System.Console.Title : "";
        set
        {
            if (OperatingSystem.IsWindows()) 
                System.Console.Title = value;
        }
    }

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
            if (value.IsIgnored) return; //value is ignored color - skip setting
            if (value == field) return;
            field = value;
            WriteFg(value);
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
            if (value.IsIgnored) return; //value is ignored color - skip setting
            if (value == field) return;
            field = value;
            WriteBg(value);
        }
    }

    public static int WindowTop => System.Console.WindowTop;

    public static int WindowLeft => System.Console.WindowLeft;

    public static int WindowWidth => System.Console.WindowWidth;

    public static int WindowHeight => System.Console.WindowHeight;
    public static int BufferWidth => System.Console.BufferWidth;

    public static int BufferHeight => System.Console.BufferHeight;

    public static int CursorLeft => System.Console.CursorLeft;

    public static int CursorTop => System.Console.CursorTop;

    public static int Read() => System.Console.Read();

    public static ConsoleKeyInfo ReadKey(bool intercept = false) => System.Console.ReadKey(intercept); 

    public static string ReadLine() => System.Console.ReadLine()!;

    public static bool KeyAvailable => System.Console.KeyAvailable;

    public static void SetCursorPosition(int left, int top)
    {
        lock (writeLock)
        {
            if (left < 0) left = 0;
            if (top < 0) top = 0;

            // Ensure width is valid relative to the window
            int minWidth = Math.Max(System.Console.WindowWidth, 1);
            int minHeight = Math.Max(System.Console.WindowHeight, 1);

            // Clamp left to current width (or expand first if you prefer)
            if (left >= System.Console.BufferWidth)
                left = System.Console.BufferWidth - 1;

            // Expand buffer height if needed
            if (top >= System.Console.BufferHeight)
            {
                int newHeight = top + 1; // or add margin, e.g. + 50
                try
                {
                    System.Console.SetBufferSize(Math.Max(Console.BufferWidth, minWidth), Math.Max(newHeight, minHeight));
                }
                catch
                {
                    // Fallback: clamp if the host doesn't allow resizing
                    top = System.Console.BufferHeight - 1;
                }
            }

            System.Console.SetCursorPosition(left, top);
        }
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
        lock (writeLock)
        {
            Span<byte> charBuf = stackalloc byte[4]; // Max UTF-8 char length
            int charLen = EncodeCharUtf8(ch, charBuf);
            Stdout.Write(charBuf[..charLen]);
        }
    }


    /// <summary>
    /// Writes a character with specified colors using optimized color caching.
    /// </summary>
    /// <param name="ch">Character to write</param>
    /// <param name="foreground">Foreground color.</param>
    /// <param name="background">Background color. Optional.</param>
    /// <remarks>
    /// Updates the ForegroundColor and BackgroundColor properties if colors have changed.
    /// Avoids redundant color escape sequences when colors haven't changed.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(char ch, Color foreground, Color background = default)
    {
        // Set colors using properties (which handle ANSI output automatically)
        ForegroundColor = foreground;
        BackgroundColor = background;
        
        // Write the character
        Write(ch);
    }

    /// <summary>
    /// Writes a string using current foreground and background colors.
    /// </summary>
    /// <param name="str">String to write</param>
    /// <remarks>
    /// Optimized for both short and long strings using bulk UTF-8 encoding.
    /// Uses current ForegroundColor and BackgroundColor properties.
    /// </remarks>
    public static void Write(ReadOnlySpan<char> str)
    {
        lock (writeLock)
        {
            if (str.IsEmpty) return;

            // Optimize for common short strings with stack allocation
            if (str.Length <= 256)
            {
                Span<byte> buffer = stackalloc byte[str.Length * 4]; // Max UTF-8 expansion
                int bytesWritten = EncodeStringUtf8(str, buffer);
                Stdout.Write(buffer[..bytesWritten]);
            }
            else
            {
                // For longer strings, use chunked processing to avoid large stack allocation
                const int chunkSize = 256;
                Span<byte> buffer = stackalloc byte[chunkSize * 4];

                for (int i = 0; i < str.Length; i += chunkSize)
                {
                    ReadOnlySpan<char> chunk = str.Slice(i, Math.Min(chunkSize, str.Length - i));
                    int bytesWritten = EncodeStringUtf8(chunk, buffer);
                    Stdout.Write(buffer[..bytesWritten]);
                }
            }
        }
    }

    /// <summary>
    /// Writes a string with specified colors.
    /// </summary>
    /// <param name="str">String to write</param>
    /// <param name="foreground">Foreground color</param>
    /// <param name="background">Background color. Optional.</param>
    /// <remarks>
    /// Sets colors using properties then delegates to non-colored Write method.
    /// </remarks>
    public static void Write(ReadOnlySpan<char> str, Color foreground, Color background = default)
    {
        ForegroundColor = foreground;
        BackgroundColor = background;
        Write(str);
    }

    /// <summary>
    /// Writes a string followed by a line terminator using current colors.
    /// </summary>
    /// <param name="str">String to write (empty string writes just newline)</param>
    /// <remarks>
    /// Uses LF (\n) line terminator only, never CRLF (\r\n).
    /// Optimized for both short and long strings using bulk UTF-8 encoding.
    /// </remarks>
    public static void WriteLine(ReadOnlySpan<char> str = "")
    {
        lock (writeLock)
        {
            if (str.IsEmpty)
            {
                // Just write LF
                Stdout.Write("\n"u8);
                return;
            }

            // Optimize for common short strings with stack allocation
            if (str.Length <= 255) // Reserve 1 char for \n
            {
                Span<byte> buffer = stackalloc byte[(str.Length + 1) * 4]; // +1 for \n, max UTF-8 expansion
                int bytesWritten = EncodeStringUtf8(str, buffer);
                buffer[bytesWritten++] = (byte)'\n'; // Add LF
                Stdout.Write(buffer[..bytesWritten]);
            }
            else
            {
                // For longer strings, write string then newline separately
                Write(str);
                Stdout.Write("\n"u8);
            }
        }
    }

    /// <summary>
    /// Writes a string followed by a line terminator with specified colors.
    /// </summary>
    /// <param name="str">String to write</param>
    /// <param name="foreground">Foreground color</param>
    /// <param name="background">Background color. Optional.</param>
    /// <remarks>
    /// Sets colors using properties then delegates to non-colored WriteLine method.
    /// Uses LF (\n) line terminator only, never CRLF (\r\n).
    /// </remarks>
    public static void WriteLine(ReadOnlySpan<char> str, Color foreground, Color background = default)
    {
        ForegroundColor = foreground;
        BackgroundColor = background;
        WriteLine(str);
    }

    /// <summary>
    /// Writes pre-encoded image data to console.
    /// </summary>
    /// <param name="imageData">Complete image data including all control sequences</param>
    /// <remarks>
    /// Ultra-optimized direct image output. Image data must be complete and ready for terminal consumption.
    /// Leverages existing optimized UTF-8 encoding infrastructure for maximum performance.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteImage(ReadOnlySpan<char> imageData)
    {
        Write(imageData);
    }

    public static void Reset()
    {
        lock (writeLock)
        {
            Write($"{Constants.ESC}{Constants.SGR_RESET}");
        }
    }

    public static void Clear(Color backgroundColor = default, bool clearScrollback = false)
    {
        lock (writeLock)
        {
            BackgroundColor = backgroundColor; //is ignored if no value is provided

            //Apply current background color(and foreground if you want a default too)

            WriteBg(BackgroundColor);
            // WriteFg(ForegroundColor); // uncomment if you want default fg reapplied

            // Erase full display and home cursor
            Write($"{Constants.ESC}{Constants.EraseDisplayAll}");
            Write($"{Constants.ESC}{Constants.CursorHome}");

            // Optionally clear scrollback
            if (clearScrollback)
                Write($"{Constants.ESC}{Constants.EraseScrollback}");

            //Reset SGR state to keep your BG / FG active as defaults
            // If you want to go back to terminal theme defaults instead, call WriteReset()
        }
    }


    private static void WriteBg(Color c) =>
        Write($"{Constants.ESC}{Constants.SGR_BG_TRUECOLOR_PREFIX}{c.R};{c.G};{c.B}{Constants.SGR_END}");

    private static void WriteFg(Color c) =>
        Write($"{Constants.ESC}{Constants.SGR_FG_TRUECOLOR_PREFIX}{c.R};{c.G};{c.B}{Constants.SGR_END}");

    #region Private Methods

    /// <summary>
    /// Writes ANSI foreground color escape sequence to stdout.
    /// </summary>
    /// <param name="color">Foreground color to set</param>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static void WriteForegroundColorToStdout(Color color)
    //{
    //    Span<byte> buf = stackalloc byte[32]; // ESC[38;2;255;255;255m = max 19 bytes
    //    ReadOnlySpan<byte> prefix = "\x1B[38;2;"u8; // ESC[38;2; in single operation
    //    int i = 0;
        
    //    prefix.CopyTo(buf);
    //    i += prefix.Length;
    //    i += WriteUInt8(color.R, buf[i..]); buf[i++] = (byte)';';
    //    i += WriteUInt8(color.G, buf[i..]); buf[i++] = (byte)';';
    //    i += WriteUInt8(color.B, buf[i..]); buf[i++] = (byte)'m';
        
    //    Stdout.Write(buf[..i]);
    //}

    /// <summary>
    /// Writes ANSI background color escape sequence to stdout.
    /// </summary>
    /// <param name="color">Background color to set</param>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static void WriteBackgroundColorToStdout(Color color)
    //{
    //    Span<byte> buf = stackalloc byte[32]; // ESC[48;2;255;255;255m = max 19 bytes
    //    ReadOnlySpan<byte> prefix = "\x1B[48;2;"u8; // ESC[48;2; in single operation
    //    int i = 0;
        
    //    prefix.CopyTo(buf);
    //    i += prefix.Length;
    //    i += WriteUInt8(color.R, buf[i..]); buf[i++] = (byte)';';
    //    i += WriteUInt8(color.G, buf[i..]); buf[i++] = (byte)';';
    //    i += WriteUInt8(color.B, buf[i..]); buf[i++] = (byte)'m';
        
    //    Stdout.Write(buf[..i]);
    //}

    /// <summary>
    /// Encodes a string span to UTF-8 bytes optimized for console output.
    /// </summary>
    /// <param name="chars">Characters to encode</param>
    /// <param name="dest">Destination buffer for UTF-8 bytes</param>
    /// <returns>Number of UTF-8 bytes written</returns>
    /// <remarks>
    /// Optimized bulk UTF-8 encoding using the same logic as EncodeCharUtf8.
    /// Handles ASCII fast path and full Unicode character ranges efficiently.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EncodeStringUtf8(ReadOnlySpan<char> chars, Span<byte> dest)
    {
        int bytesWritten = 0;
        
        for (int i = 0; i < chars.Length; i++)
        {
            char ch = chars[i];
            
            // ASCII fast path - most common case
            if (ch <= 0x7F)
            {
                dest[bytesWritten++] = (byte)ch;
                continue;
            }
            
            // Non-ASCII character - use existing encoding logic
            if (ch < 0xD800 || ch > 0xDFFF)
            {
                if (ch <= 0x7FF)
                {
                    dest[bytesWritten++] = (byte)(0xC0 | (ch >> 6));
                    dest[bytesWritten++] = (byte)(0x80 | (ch & 0x3F));
                }
                else
                {
                    dest[bytesWritten++] = (byte)(0xE0 | (ch >> 12));
                    dest[bytesWritten++] = (byte)(0x80 | ((ch >> 6) & 0x3F));
                    dest[bytesWritten++] = (byte)(0x80 | (ch & 0x3F));
                }
            }
            else
            {
                // Invalid surrogate pair - replace with ?
                dest[bytesWritten++] = (byte)'?';
            }
        }
        
        return bytesWritten;
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
        if (!OperatingSystem.IsWindows()) return;

        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;
        const int STD_OUTPUT_HANDLE = -11;
        const int STD_INPUT_HANDLE = -10;

        // Output: enable VT processing for SGR/OSC writes
        nint hout = GetStdHandle(STD_OUTPUT_HANDLE);
        if (hout != nint.Zero && GetConsoleMode(hout, out uint outMode))
        {
            SetConsoleMode(hout, outMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }

        // Input: enable VT input so OSC replies arrive as input chars
        nint hin = GetStdHandle(STD_INPUT_HANDLE);
        if (hin != nint.Zero && GetConsoleMode(hin, out uint inMode))
        {
            SetConsoleMode(hin, inMode | ENABLE_VIRTUAL_TERMINAL_INPUT);
        }
    }


    [DllImport("kernel32.dll", SetLastError = true)] private static extern nint GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    #endregion Private Methods
}
