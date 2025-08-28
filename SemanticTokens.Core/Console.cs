using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SemanticTokens.Core;

/// <summary>
/// Optimized 24‑bit color console writer with VT output and RAW VT input.
/// Keeps SIXEL output exactly as before; replaces reading with a tiny VT-aware editor
/// so Backspace deletes a char and Ctrl+Backspace deletes a word on Windows Terminal.
/// </summary>
public static class Console
{
    private static readonly Stream Stdout = System.Console.OpenStandardOutput();
    private static readonly Stream Stdin = System.Console.OpenStandardInput();
    private static readonly Decoder Utf8Decoder = Encoding.UTF8.GetDecoder();

    private static readonly Lock writeLock = new();

    private static Color lastFg = Color.Transparent;
    private static Color lastBg = Color.Transparent;

    static Console()
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        System.Console.InputEncoding = Encoding.UTF8;

        Windows.TryEnableVirtualTerminalOnWindows(); // enable VT output + raw-ish input on Win
        Posix.TryEnableRawTerminalOnPosix();       // raw input on Linux/macOS (only if tty)

        // Backspace semantics (DECBKM) to stabilize BS vs DEL handling.
        WriteInternal("\x1b[?67h");

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
    public static Color ForegroundColor { get; set; }

    /// <summary>
    /// Default background color of the console.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Clear(Color, bool)"/> to set
    /// the default background color for all subsequent console output.
    /// </remarks>
    public static Color BackgroundColor { get; private set; }

    public static int WindowTop => System.Console.WindowTop;
    public static int WindowLeft => System.Console.WindowLeft;
    public static int WindowWidth => System.Console.WindowWidth;
    public static int WindowHeight => System.Console.WindowHeight;
    public static int BufferWidth => System.Console.BufferWidth;
    public static int BufferHeight => System.Console.BufferHeight;
    public static int CursorLeft => System.Console.CursorLeft;
    public static int CursorTop => System.Console.CursorTop;

 
    /// <summary>True if there is input available without blocking.</summary>
    public static bool KeyAvailable
    {
        get
        {
            if (!OperatingSystem.IsWindows())
                return System.Console.KeyAvailable;

            nint hin = GetStdHandle(STD_INPUT_HANDLE);
            return WaitForSingleObject(hin, 0) == WAIT_OBJECT_0;
        }
    }

    /// <summary>
    /// Exactly the same on-screen effect as a user pressing Backspace.
    /// </summary>
    /// <param name="backspaceCount">Number of backspaces to emit (default=1)</param>
    public static void Backspace(int backspaceCount = 1)
    {
        if (backspaceCount == 1) //fast path for single
        {
            Write("\b \b");
            return;
        }
        StringBuilder sb = new();
        for (int i = 0; i < backspaceCount; i++)
        {
            sb.Append("\b \b");
        }
        Write(sb.ToString());
    }
   
    public static void Reset()
    {
        lock (writeLock)
        {
            WriteInternal($"{Constants.ESC}{Constants.SGR_RESET}");
        }
    }

    /// <summary>
    /// Clears the entire console screen and moves the cursor to the home position (1,1).
    /// </summary>
    /// <param name="backgroundColor">Optional new background color to set when clearing.</param>
    /// <param name="clearScrollback"></param>
    /// <remarks>
    /// If a background color is specified, it is also set as the current <see cref="BackgroundColor"/>.
    /// If no background color is specified, the current <see cref="BackgroundColor"/> is used.
    /// </remarks>
    public static void Clear(Color backgroundColor = default, bool clearScrollback = false)
    {
        lock (writeLock)
        {
            if (backgroundColor != default)
                BackgroundColor = backgroundColor;

            WriteBg(BackgroundColor);
            WriteInternal($"{Constants.ESC}{Constants.EraseDisplayAll}");
            WriteInternal($"{Constants.ESC}{Constants.CursorHome}");

            if (clearScrollback)
                WriteInternal($"{Constants.ESC}{Constants.EraseScrollback}");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(char ch, Color foreground = default, Color background = default)
    {
        lock (writeLock)
        {
            if (foreground == default)
                foreground = ForegroundColor;

            if (background == default)
                background = BackgroundColor;

            if (foreground != lastFg)
            {
                WriteFg(foreground);
                lastFg = foreground;
            }

            if (background != lastBg)
            {
                WriteBg(background);
                lastBg = background;
            }

            WriteInternal(ch);
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
    public static void Write(ReadOnlySpan<char> str, Color foreground = default, Color background = default)
    {
        lock (writeLock)
        {
            if (str.IsEmpty) return;

            if (foreground == default)
                foreground = ForegroundColor;

            if (background == default)
                background = BackgroundColor;

            if (foreground != lastFg)
            {
                WriteFg(foreground);
                lastFg = foreground;
            }

            if (background != lastBg)
            {
                WriteBg(background);
                lastBg = background;
            }

            WriteInternal(str);
        }
    }
  
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void WriteInternal(char ch)
    {
        lock (writeLock)
        {
            Span<byte> charBuf = stackalloc byte[4];
            int charLen = EncodeCharUtf8(ch, charBuf);
            Stdout.Write(charBuf[..charLen]);
        }
    }

    internal static void WriteInternal(ReadOnlySpan<char> str)
    {
        lock (writeLock)
        {
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
    /// Writes a string followed by a line terminator with specified colors.
    /// </summary>
    /// <param name="str">String to write</param>
    /// <param name="foreground">Foreground color</param>
    /// <param name="background">Background color. Optional.</param>
    /// <remarks>
    /// Sets colors using properties then delegates to non-colored WriteLine method.
    /// Uses LF (\n) line terminator only, never CRLF (\r\n).
    /// </remarks>
    public static void WriteLine(ReadOnlySpan<char> str, Color foreground = default, Color background = default)
    {
        Write(str, foreground, background);
        WriteLine();
    }

    public static void WriteLine()
    {
        Stdout.Write("\n"u8);
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


    /// <summary>
    /// Read next Unicode scalar from stdin (decoded from UTF‑8). Returns -1 on EOF.
    /// </summary>
    public static int Read()
    {
        Span<byte> inBuf = stackalloc byte[4];
        Span<char> outBuf = stackalloc char[2];
        int inCount = 0;

        while (true)
        {
            int b = Stdin.ReadByte();
            if (b < 0) return -1;

            inBuf[inCount++] = (byte)b;

            Utf8Decoder.Convert(inBuf[..inCount], outBuf, false,
                out int bytesUsed, out int charsUsed, out bool completed);

            if (charsUsed > 0)
                return outBuf[0];

            // Guard: if we somehow collected 4 bytes and still no char, emit replacement
            if (inCount == 4)
                return '?';
        }
    }

    /// <summary>
    /// Read a key (VT‑aware). Distinguishes Backspace vs Ctrl+Backspace via BS(0x08) vs DEL(0x7F).
    /// Handles basic CSI/SS3 cursor keys and Delete (CSI 3~).
    /// </summary>
    public static ConsoleKeyInfo ReadKey(bool intercept = false)
    {
        int ch = Read();
        if (ch == -1) return default;

        // ESC‑prefixed sequences
        if (ch == 0x1B)
        {
            return DecodeEscapeSequence(intercept);
        }

        // Backspace family: BS (0x08) and DEL (0x7F)
        // DECBKM set => Backspace sends BS; Ctrl+Backspace toggles to DEL (WT implements this).
        // We normalize both to ConsoleKey.Backspace and set Control=true for DEL so callers can tell.
        if (ch == 0x08 || ch == 0x7F)
        {
            bool isCtrl = (ch == 0x7F);
            if (!intercept) Backspace(1);
            return new ConsoleKeyInfo('\b', System.ConsoleKey.Backspace, false, false, isCtrl);
        }

        // Enter: CR or LF
        if (ch == '\n' || ch == '\r')
        {
            if (!intercept) Write("\n");
            return new ConsoleKeyInfo('\n', System.ConsoleKey.Enter, false, false, false);
        }

        // Regular printable
        if (!intercept) Write((char)ch);
        return new ConsoleKeyInfo((char)ch, KeyFromChar((char)ch), false, false, false);
    }

    /// <summary>
    /// Simple cooked line editor:
    /// - Backspace → delete 1 char (rune‑aware)
    /// - Ctrl+Backspace → delete previous word
    /// - Enter returns the line
    /// </summary>
    /// <param name="clearOnExit">If true, erases the line on Enter; otherwise leaves it on screen.</param>
    public static string ReadLine(bool clearOnExit = false)
    {
        StringBuilder sb = new StringBuilder(128);

        while (true)
        {
            var key = ReadKey(intercept: true);

            if (key.Key == System.ConsoleKey.Enter)
            {
                if (clearOnExit)
                {
                    Backspace(sb.Length);
                }
                else Write("\n");
                return sb.ToString();
            }

            if (key.Key == System.ConsoleKey.Backspace)
            {
                if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    // Ctrl+Backspace: delete previous word
                    int toErase = ComputeWordEraseCount(sb);
                    if (toErase > 0)
                    {
                        sb.Remove(sb.Length - toErase, toErase);
                        Backspace(toErase);
                    }
                }
                else
                {
                    // Plain Backspace: delete one rune
                    if (sb.Length > 0)
                    {
                        int removeLen = 1;
                        if (sb.Length >= 2 && char.IsLowSurrogate(sb[^1]) && char.IsHighSurrogate(sb[^2]))
                            removeLen = 2;
                        sb.Remove(sb.Length - removeLen, removeLen);
                        Backspace(removeLen);
                    }
                }
                continue;
            }

            // Ignore other control keys in this minimal editor
            if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                Write(key.KeyChar);
            }
        }
    }

    public static void SetCursorPosition(int left, int top)
    {
        lock (writeLock)
        {
            if (left < 0) left = 0;
            if (top < 0) top = 0;

            int minWidth = Math.Max(System.Console.WindowWidth, 1);
            int minHeight = Math.Max(System.Console.WindowHeight, 1);

            if (left >= System.Console.BufferWidth)
                left = System.Console.BufferWidth - 1;

            if (top >= System.Console.BufferHeight)
            {
                int newHeight = top + 1;
                try
                {
                    System.Console.SetBufferSize(Math.Max(Console.BufferWidth, minWidth), Math.Max(newHeight, minHeight));
                }
                catch
                {
                    top = System.Console.BufferHeight - 1;
                }
            }

            System.Console.SetCursorPosition(left, top);
        }
    }

    private static void WriteBg(Color c) =>
        WriteInternal($"{Constants.ESC}{Constants.SGR_BG_TRUECOLOR_PREFIX}{c.R};{c.G};{c.B}{Constants.SGR_END}");

    private static void WriteFg(Color c) =>
        WriteInternal($"{Constants.ESC}{Constants.SGR_FG_TRUECOLOR_PREFIX}{c.R};{c.G};{c.B}{Constants.SGR_END}");

    #region Private Methods

    private static int ComputeWordEraseCount(StringBuilder sb)
    {
        if (sb.Length == 0) return 0;

        int end = sb.Length;
        int i = end;

        // 1) Skip any trailing spaces (these should be erased too)
        while (i > 0 && char.IsWhiteSpace(sb[i - 1])) i--;

        // 2) Then consume the non-space run as the "word"
        while (i > 0 && !char.IsWhiteSpace(sb[i - 1])) i--;

        // Erase count = trailing spaces + word
        return end - i;
    }

    private static System.ConsoleKey KeyFromChar(char ch)
    {
        if (ch >= 'A' && ch <= 'Z') return (System.ConsoleKey)ch;
        if (ch >= 'a' && ch <= 'z') return (System.ConsoleKey)char.ToUpperInvariant(ch);
        if (ch >= '0' && ch <= '9') return (System.ConsoleKey)('D' + (ch - '0'));
        return System.ConsoleKey.NoName;
    }

    private static ConsoleKeyInfo DecodeEscapeSequence(bool intercept)
    {
        // Read a small burst of pending bytes to coalesce an ESC sequence.
        Span<byte> buf = stackalloc byte[8];
        int n = 0;
        var start = Environment.TickCount64;

        while (n < buf.Length)
        {
            if (OperatingSystem.IsWindows())
            {
                if (WaitForSingleObject(GetStdHandle(STD_INPUT_HANDLE), 0) != WAIT_OBJECT_0) break;
            }
            else if (!System.Console.KeyAvailable) break;

            int b = Stdin.ReadByte();
            if (b < 0) break;
            buf[n++] = (byte)b;

            if (Environment.TickCount64 - start > 2) break; // short idle window
        }

        ReadOnlySpan<byte> seq = buf[..n];

        if (seq.Length == 0)
        {
            if (!intercept) Write("\x1b");
            return new ConsoleKeyInfo('\x1b', System.ConsoleKey.Escape, false, false, false);
        }

        // CSI: ESC [ ... final
        if (seq[0] == (byte)'[')
        {
            if (seq.Length >= 1)
            {
                byte final = seq[^1];
                switch (final)
                {
                    case (byte)'A': return new ConsoleKeyInfo('\0', System.ConsoleKey.UpArrow, false, false, false);
                    case (byte)'B': return new ConsoleKeyInfo('\0', System.ConsoleKey.DownArrow, false, false, false);
                    case (byte)'C': return new ConsoleKeyInfo('\0', System.ConsoleKey.RightArrow, false, false, false);
                    case (byte)'D': return new ConsoleKeyInfo('\0', System.ConsoleKey.LeftArrow, false, false, false);
                    case (byte)'H': return new ConsoleKeyInfo('\0', System.ConsoleKey.Home, false, false, false);
                    case (byte)'F': return new ConsoleKeyInfo('\0', System.ConsoleKey.End, false, false, false);
                    case (byte)'~':
                        int num = 0;
                        for (int i = 1; i < seq.Length - 1; i++)
                            if (seq[i] >= '0' && seq[i] <= '9')
                                num = num * 10 + (seq[i] - '0');
                        return num switch
                        {
                            2 => new ConsoleKeyInfo('\0', System.ConsoleKey.Insert, false, false, false),
                            3 => new ConsoleKeyInfo('\0', System.ConsoleKey.Delete, false, false, false),
                            5 => new ConsoleKeyInfo('\0', System.ConsoleKey.PageUp, false, false, false),
                            6 => new ConsoleKeyInfo('\0', System.ConsoleKey.PageDown, false, false, false),
                            _ => new ConsoleKeyInfo('\0', System.ConsoleKey.NoName, false, false, false)
                        };
                }
            }
        }

        // SS3: ESC O ...
        if (seq.Length >= 2 && seq[0] == (byte)'O')
        {
            return seq[1] switch
            {
                (byte)'P' => new ConsoleKeyInfo('\0', System.ConsoleKey.F1, false, false, false),
                (byte)'Q' => new ConsoleKeyInfo('\0', System.ConsoleKey.F2, false, false, false),
                (byte)'R' => new ConsoleKeyInfo('\0', System.ConsoleKey.F3, false, false, false),
                (byte)'S' => new ConsoleKeyInfo('\0', System.ConsoleKey.F4, false, false, false),
                (byte)'H' => new ConsoleKeyInfo('\0', System.ConsoleKey.Home, false, false, false),
                (byte)'F' => new ConsoleKeyInfo('\0', System.ConsoleKey.End, false, false, false),
                _ => new ConsoleKeyInfo('\0', System.ConsoleKey.NoName, false, false, false)
            };
        }

        // Unknown sequence → deliver a literal ESC (don’t swallow input).
        if (!intercept) Write("\x1b");
        return new ConsoleKeyInfo('\x1b', System.ConsoleKey.Escape, false, false, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EncodeStringUtf8(ReadOnlySpan<char> chars, Span<byte> dest)
    {
        int bytesWritten = 0;

        for (int i = 0; i < chars.Length; i++)
        {
            char ch = chars[i];

            if (ch <= 0x7F)
            {
                dest[bytesWritten++] = (byte)ch;
                continue;
            }

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

    // ===== P-Invoke =====

    [DllImport("kernel32.dll", SetLastError = true)] internal static extern nint GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)] internal static extern uint WaitForSingleObject(nint hHandle, uint dwMilliseconds);

    internal const int STD_OUTPUT_HANDLE = -11;
    internal const int STD_INPUT_HANDLE = -10;

    internal const uint WAIT_OBJECT_0 = 0x00000000;

    #endregion
}
