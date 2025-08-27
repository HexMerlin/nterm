
//using System.IO;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using System.Text;

//namespace SemanticTokens.Core;

///// <summary>
///// Optimized 24-bit color console with VT output and RAW VT input (Windows-Terminal friendly).
///// Includes SIXEL-friendly image writers and a tiny line editor so Backspace always deletes one char.
///// </summary>
//public static class Console
//{
//    private static readonly Stream Stdout = System.Console.OpenStandardOutput();
//    private static readonly Stream Stdin = System.Console.OpenStandardInput();
//    private static readonly Decoder Utf8Decoder = Encoding.UTF8.GetDecoder();

//    // Use the user's original Lock type if present in the project.
//    private static readonly Lock writeLock = new();

//    private static Color _foregroundColor;
//    private static Color _backgroundColor;

//    static Console()
//    {
//        // Make I/O UTF-8 all the way through
//        System.Console.OutputEncoding = Encoding.UTF8;
//        System.Console.InputEncoding = Encoding.UTF8;

//        WriteFg(Color.FromConsoleColor(System.Console.ForegroundColor));
//        WriteBg(Color.FromConsoleColor(System.Console.BackgroundColor));
//        TryEnableVirtualTerminalOnWindows();

//        // Optional but recommended: choose Backspace semantics (DECBKM) so BS = 0x08
//        // Comment out if you don't want to affect terminal state.
//        Write("\x1b[?67h");
//    }

//    public static string Title
//    {
//        get => System.Console.Title;
//        set => System.Console.Title = value;
//    }

//    #region Colors
//    /// <summary>
//    /// 24-bit foreground color of the console.
//    /// </summary>
//    public static Color ForegroundColor
//    {
//        get => _foregroundColor;
//        set
//        {
//            if (value.IsIgnored) return;
//            if (value.Equals(_foregroundColor)) return;
//            _foregroundColor = value;
//            WriteFg(value);
//        }
//    }

//    /// <summary>
//    /// 24-bit background color of the console.
//    /// </summary>
//    public static Color BackgroundColor
//    {
//        get => _backgroundColor;
//        set
//        {
//            if (value.IsIgnored) return;
//            if (value.Equals(_backgroundColor)) return;
//            _backgroundColor = value;
//            WriteBg(value);
//        }
//    }
//    #endregion

//    #region Window/Buffer
//    public static int WindowTop => System.Console.WindowTop;
//    public static int WindowLeft => System.Console.WindowLeft;
//    public static int WindowWidth => System.Console.WindowWidth;
//    public static int WindowHeight => System.Console.WindowHeight;
//    public static int BufferWidth => System.Console.BufferWidth;
//    public static int BufferHeight => System.Console.BufferHeight;
//    public static int CursorLeft => System.Console.CursorLeft;
//    public static int CursorTop => System.Console.CursorTop;
//    #endregion

//    #region Input (RAW VT aware)
//    /// <summary>
//    /// True if there is input available without blocking.
//    /// </summary>
//    public static bool KeyAvailable
//    {
//        get
//        {
//            if (!OperatingSystem.IsWindows())
//            {
//                // On Unix ptys the standard property works fine.
//                return System.Console.KeyAvailable;
//            }
//            nint hin = GetStdHandle(STD_INPUT_HANDLE);
//            return WaitForSingleObject(hin, 0) == WAIT_OBJECT_0;
//        }
//    }

//    /// <summary>
//    /// Reads the next Unicode scalar value (decoded from UTF-8) from stdin. Returns -1 on EOF.
//    /// </summary>
//    public static int Read()
//    {
//        Span<byte> inBuf = stackalloc byte[4];
//        Span<char> outBuf = stackalloc char[2];

//        int inCount = 0;
//        while (true)
//        {
//            int b = Stdin.ReadByte();
//            if (b < 0)
//                return -1;

//            inBuf[inCount++] = (byte)b;

//            Utf8Decoder.Convert(inBuf[..inCount], outBuf, false, out int bytesUsed, out int charsUsed, out bool completed);
//            if (charsUsed > 0)
//            {
//                return outBuf[0];
//            }

//            // Guard: if we somehow collected 4 bytes and still no char, emit replacement
//            if (inCount == 4)
//                return '?';
//        }
//    }

//    /// <summary>
//    /// Reads a key (with minimal VT sequence handling for arrows, home/end, etc.).
//    /// </summary>
//    public static ConsoleKeyInfo ReadKey(bool intercept = false)
//    {
//        int ch = Read();
//        if (ch == -1) return default;

//        // ESC-prefixed sequences
//        if (ch == 0x1B)
//        {
//            return DecodeEscapeSequence(intercept);
//        }

//        // Normalize Backspace: treat BS and DEL as Backspace
//        if (ch == 0x08 || ch == 0x7F)
//        {
//            if (!intercept) EchoBackspace();
//            return new ConsoleKeyInfo('\b', System.ConsoleKey.Backspace, false, false, false);
//        }

//        // Enter: CR or LF
//        if (ch == '\r' || ch == '\n')
//        {
//            if (!intercept) Write("\n");
//            return new ConsoleKeyInfo('\n', System.ConsoleKey.Enter, false, false, false);
//        }

//        // Regular printable
//        if (!intercept) Write((char)ch);
//        return new ConsoleKeyInfo((char)ch, KeyFromChar((char)ch), false, false, false);
//    }

//    /// <summary>
//    /// Simple cooked line editor that echoes characters, handles BS/DEL one-char delete,
//    /// and returns on Enter. Works consistently with VT input on.
//    /// </summary>
//    public static string ReadLine()
//    {
//        var sb = new StringBuilder(128);

//        while (true)
//        {
//            var key = ReadKey(intercept: true);
//            if (key.Key == System.ConsoleKey.Enter)
//            {
//                Write("\n");
//                return sb.ToString();
//            }

//            if (key.Key == System.ConsoleKey.Backspace)
//            {
//                if (sb.Length > 0)
//                {
//                    int removeLen = 1;
//                    // rune-aware backspace: remove trailing surrogate pair
//                    if (sb.Length >= 2 && char.IsLowSurrogate(sb[^1]) && char.IsHighSurrogate(sb[^2]))
//                        removeLen = 2;
//                    sb.Remove(sb.Length - removeLen, removeLen);
//                    EchoBackspace(removeLen);
//                }
//                continue;
//            }

//            // For simplicity, ignore control keys other than BS/Enter in this sample
//            if (!char.IsControl(key.KeyChar))
//            {
//                sb.Append(key.KeyChar);
//                Write(key.KeyChar);
//            }
//        }
//    }

//    private static System.ConsoleKey KeyFromChar(char ch)
//    {
//        if (ch >= 'A' && ch <= 'Z') return (System.ConsoleKey)ch;
//        if (ch >= 'a' && ch <= 'z') return (System.ConsoleKey)char.ToUpperInvariant(ch);
//        if (ch >= '0' && ch <= '9') return (System.ConsoleKey)('D' + (ch - '0')); // ConsoleKey.D0..D9
//        return System.ConsoleKey.NoName;
//    }

//    private static ConsoleKeyInfo DecodeEscapeSequence(bool intercept)
//    {
//        // Inline ReadPending(max = 8)
//        Span<byte> buf = stackalloc byte[8];
//        int n = 0;

//        // Small timed window to coalesce an ESC sequence
//        var start = Environment.TickCount64;
//        while (n < buf.Length)
//        {
//            if (OperatingSystem.IsWindows())
//            {
//                if (WaitForSingleObject(GetStdHandle(STD_INPUT_HANDLE), 0) != WAIT_OBJECT_0)
//                    break;
//            }
//            else if (!System.Console.KeyAvailable)
//            {
//                break;
//            }

//            int b = Stdin.ReadByte();
//            if (b < 0) break;
//            buf[n++] = (byte)b;

//            // stop after ~2ms of idle
//            if (Environment.TickCount64 - start > 2)
//                break;
//        }

//        ReadOnlySpan<byte> seq = buf[..n];

//        // CSI sequences: ESC [ A/B/C/D (arrows), H (home), F (end), 3~ (delete)
//        if (seq.Length == 0)
//        {
//            // lone ESC
//            if (!intercept) Write("\x1b");
//            return new ConsoleKeyInfo('\x1b', System.ConsoleKey.Escape, false, false, false);
//        }

//        if (seq[0] == (byte)'[')
//        {
//            if (seq.Length >= 1)
//            {
//                byte final = seq[^1];
//                switch (final)
//                {
//                    case (byte)'A': return new ConsoleKeyInfo('\0', System.ConsoleKey.UpArrow, false, false, false);
//                    case (byte)'B': return new ConsoleKeyInfo('\0', System.ConsoleKey.DownArrow, false, false, false);
//                    case (byte)'C': return new ConsoleKeyInfo('\0', System.ConsoleKey.RightArrow, false, false, false);
//                    case (byte)'D': return new ConsoleKeyInfo('\0', System.ConsoleKey.LeftArrow, false, false, false);
//                    case (byte)'H': return new ConsoleKeyInfo('\0', System.ConsoleKey.Home, false, false, false);
//                    case (byte)'F': return new ConsoleKeyInfo('\0', System.ConsoleKey.End, false, false, false);
//                    case (byte)'~':
//                        // parse numbers like 3~ (Delete), 2~ (Insert), 5~/6~ (PgUp/PgDn)
//                        int num = 0;
//                        for (int i = 1; i < seq.Length - 1; i++)
//                            if (seq[i] >= '0' && seq[i] <= '9')
//                                num = num * 10 + (seq[i] - '0');
//                        return num switch
//                        {
//                            2 => new ConsoleKeyInfo('\0', System.ConsoleKey.Insert, false, false, false),
//                            3 => new ConsoleKeyInfo('\0', System.ConsoleKey.Delete, false, false, false),
//                            5 => new ConsoleKeyInfo('\0', System.ConsoleKey.PageUp, false, false, false),
//                            6 => new ConsoleKeyInfo('\0', System.ConsoleKey.PageDown, false, false, false),
//                            _ => new ConsoleKeyInfo('\0', System.ConsoleKey.NoName, false, false, false)
//                        };
//                }
//            }
//        }

//        // SS3: ESC O P..S (F1..F4), ESC O H/F (Home/End)
//        if (seq.Length >= 2 && seq[0] == (byte)'O')
//        {
//            return seq[1] switch
//            {
//                (byte)'P' => new ConsoleKeyInfo('\0', System.ConsoleKey.F1, false, false, false),
//                (byte)'Q' => new ConsoleKeyInfo('\0', System.ConsoleKey.F2, false, false, false),
//                (byte)'R' => new ConsoleKeyInfo('\0', System.ConsoleKey.F3, false, false, false),
//                (byte)'S' => new ConsoleKeyInfo('\0', System.ConsoleKey.F4, false, false, false),
//                (byte)'H' => new ConsoleKeyInfo('\0', System.ConsoleKey.Home, false, false, false),
//                (byte)'F' => new ConsoleKeyInfo('\0', System.ConsoleKey.End, false, false, false),
//                _ => new ConsoleKeyInfo('\0', System.ConsoleKey.NoName, false, false, false)
//            };
//        }

//        // Unknown sequence → return Escape to avoid swallowing input
//        if (!intercept) Write("\x1b");
//        return new ConsoleKeyInfo('\x1b', System.ConsoleKey.Escape, false, false, false);
//    }

//    private static void EchoBackspace(int runeWidth = 1)
//    {
//        // Move left, erase, move left (once per rune; assumes width 1 for BMP chars)
//        for (int i = 0; i < runeWidth; i++)
//        {
//            Write("\b \b");
//        }
//    }
//    #endregion

//    #region Output
//    public static void SetCursorPosition(int left, int top)
//    {
//        lock (writeLock)
//        {
//            if (left < 0) left = 0;
//            if (top < 0) top = 0;

//            int minWidth = Math.Max(System.Console.WindowWidth, 1);
//            int minHeight = Math.Max(System.Console.WindowHeight, 1);

//            if (left >= System.Console.BufferWidth)
//                left = System.Console.BufferWidth - 1;

//            if (top >= System.Console.BufferHeight)
//            {
//                int newHeight = top + 1;
//                try
//                {
//                    System.Console.SetBufferSize(Math.Max(BufferWidth, minWidth), Math.Max(newHeight, minHeight));
//                }
//                catch
//                {
//                    top = System.Console.BufferHeight - 1;
//                }
//            }

//            System.Console.SetCursorPosition(left, top);
//        }
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static void Write(char ch)
//    {
//        lock (writeLock)
//        {
//            Span<byte> charBuf = stackalloc byte[4]; // Max UTF-8 char length
//            int charLen = EncodeCharUtf8(ch, charBuf);
//            Stdout.Write(charBuf[..charLen]);
//        }
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static void Write(char ch, Color foreground, Color background = default)
//    {
//        ForegroundColor = foreground;
//        BackgroundColor = background;
//        Write(ch);
//    }

//    public static void Write(ReadOnlySpan<char> str)
//    {
//        lock (writeLock)
//        {
//            if (str.IsEmpty) return;

//            if (str.Length <= 256)
//            {
//                Span<byte> buffer = stackalloc byte[str.Length * 4];
//                int bytesWritten = EncodeStringUtf8(str, buffer);
//                Stdout.Write(buffer[..bytesWritten]);
//            }
//            else
//            {
//                const int chunkSize = 256;
//                Span<byte> buffer = stackalloc byte[chunkSize * 4];

//                for (int i = 0; i < str.Length; i += chunkSize)
//                {
//                    ReadOnlySpan<char> chunk = str.Slice(i, Math.Min(chunkSize, str.Length - i));
//                    int bytesWritten = EncodeStringUtf8(chunk, buffer);
//                    Stdout.Write(buffer[..bytesWritten]);
//                }
//            }
//        }
//    }

//    public static void Write(ReadOnlySpan<char> str, Color foreground, Color background = default)
//    {
//        ForegroundColor = foreground;
//        BackgroundColor = background;
//        Write(str);
//    }

//    public static void WriteLine(ReadOnlySpan<char> str = "")
//    {
//        lock (writeLock)
//        {
//            if (str.IsEmpty)
//            {
//                Stdout.Write("\n"u8);
//                return;
//            }

//            if (str.Length <= 255)
//            {
//                Span<byte> buffer = stackalloc byte[(str.Length + 1) * 4];
//                int bytesWritten = EncodeStringUtf8(str, buffer);
//                buffer[bytesWritten++] = (byte)'\n';
//                Stdout.Write(buffer[..bytesWritten]);
//            }
//            else
//            {
//                Write(str);
//                Stdout.Write("\n"u8);
//            }
//        }
//    }

//    public static void WriteLine(ReadOnlySpan<char> str, Color foreground, Color background = default)
//    {
//        ForegroundColor = foreground;
//        BackgroundColor = background;
//        WriteLine(str);
//    }

//    /// <summary>
//    /// Writes pre-encoded image data (e.g., SIXEL) to the console without modification.
//    /// </summary>
//    /// <param name="imageData">Complete image data including all control sequences (DCS ... ST/BEL)</param>
//    /// <remarks>
//    /// Ultra-optimized direct image output. Image data must be complete and ready for terminal consumption.
//    /// </remarks>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static void WriteImage(ReadOnlySpan<char> imageData)
//    {
//        // Emit raw 8-bit bytes for VT sequences (preserves C0/C1 controls like DCS/ST).
//        // This avoids UTF-8 transcoding which can break 8-bit controls by expanding them.
//        lock (writeLock)
//        {
//            const int chunkSize = 512;
//            Span<byte> buffer = stackalloc byte[chunkSize];

//            int i = 0;
//            while (i < imageData.Length)
//            {
//                int n = Math.Min(chunkSize, imageData.Length - i);
//                for (int j = 0; j < n; j++)
//                {
//                    // Truncate to 8-bit: ESC, BEL, C0/C1, and ASCII graphics are preserved.
//                    buffer[j] = (byte)imageData[i + j];
//                }
//                Stdout.Write(buffer[..n]);
//                i += n;
//            }
//        }
//    }

//    /// <summary>
//    /// Writes raw bytes (prefer this for binary-safe emission of VT sequences like SIXEL).
//    /// </summary>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static void WriteImage(ReadOnlySpan<byte> imageData)
//    {
//        lock (writeLock)
//        {
//            Stdout.Write(imageData);
//        }
//    }

//    /// <summary>
//    /// Writes raw bytes to stdout as-is (helper for advanced callers).
//    /// </summary>
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public static void WriteRaw(ReadOnlySpan<byte> data)
//    {
//        lock (writeLock)
//        {
//            Stdout.Write(data);
//        }
//    }

//    public static void Reset()
//    {
//        lock (writeLock)
//        {
//            Write($"{Constants.ESC}{Constants.SGR_RESET}");
//        }
//    }

//    public static void Clear(Color backgroundColor = default, bool clearScrollback = false)
//    {
//        lock (writeLock)
//        {
//            BackgroundColor = backgroundColor; // ignored if default

//            WriteBg(BackgroundColor);
//            Write($"{Constants.ESC}{Constants.EraseDisplayAll}");
//            Write($"{Constants.ESC}{Constants.CursorHome}");

//            if (clearScrollback)
//                Write($"{Constants.ESC}{Constants.EraseScrollback}");
//        }
//    }

//    private static void WriteBg(Color c) =>
//        Write($"{Constants.ESC}{Constants.SGR_BG_TRUECOLOR_PREFIX}{c.R};{c.G};{c.B}{Constants.SGR_END}");

//    private static void WriteFg(Color c) =>
//        Write($"{Constants.ESC}{Constants.SGR_FG_TRUECOLOR_PREFIX}{c.R};{c.G};{c.B}{Constants.SGR_END}");

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static int EncodeStringUtf8(ReadOnlySpan<char> chars, Span<byte> dest)
//    {
//        int bytesWritten = 0;
//        for (int i = 0; i < chars.Length; i++)
//        {
//            char ch = chars[i];
//            if (ch <= 0x7F)
//            {
//                dest[bytesWritten++] = (byte)ch;
//                continue;
//            }
//            if (ch < 0xD800 || ch > 0xDFFF)
//            {
//                if (ch <= 0x7FF)
//                {
//                    dest[bytesWritten++] = (byte)(0xC0 | (ch >> 6));
//                    dest[bytesWritten++] = (byte)(0x80 | (ch & 0x3F));
//                }
//                else
//                {
//                    dest[bytesWritten++] = (byte)(0xE0 | (ch >> 12));
//                    dest[bytesWritten++] = (byte)(0x80 | ((ch >> 6) & 0x3F));
//                    dest[bytesWritten++] = (byte)(0x80 | (ch & 0x3F));
//                }
//            }
//            else
//            {
//                dest[bytesWritten++] = (byte)'?';
//            }
//        }
//        return bytesWritten;
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static int EncodeCharUtf8(char ch, Span<byte> dest)
//    {
//        if (ch <= 0x7F) { dest[0] = (byte)ch; return 1; }
//        if (ch < 0xD800 || ch > 0xDFFF)
//        {
//            if (ch <= 0x7FF)
//            {
//                dest[0] = (byte)(0xC0 | (ch >> 6));
//                dest[1] = (byte)(0x80 | (ch & 0x3F));
//                return 2;
//            }
//            dest[0] = (byte)(0xE0 | (ch >> 12));
//            dest[1] = (byte)(0x80 | ((ch >> 6) & 0x3F));
//            dest[2] = (byte)(0x80 | (ch & 0x3F));
//            return 3;
//        }
//        dest[0] = (byte)'?';
//        return 1;
//    }
//    #endregion

//    #region Win32 VT mode
//    private static void TryEnableVirtualTerminalOnWindows()
//    {
//        if (!OperatingSystem.IsWindows()) return;

//        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
//        const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

//        const uint ENABLE_PROCESSED_INPUT = 0x0001;
//        const uint ENABLE_LINE_INPUT = 0x0002;
//        const uint ENABLE_ECHO_INPUT = 0x0004;
//        const uint ENABLE_QUICK_EDIT_MODE = 0x0040; // needs EXTENDED_FLAGS set
//        const uint ENABLE_EXTENDED_FLAGS = 0x0080;

//        nint hout = GetStdHandle(STD_OUTPUT_HANDLE);
//        if (hout != nint.Zero && GetConsoleMode(hout, out uint outMode))
//        {
//            SetConsoleMode(hout, outMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
//        }

//        nint hin = GetStdHandle(STD_INPUT_HANDLE);
//        if (hin != nint.Zero && GetConsoleMode(hin, out uint inMode))
//        {
//            inMode |= ENABLE_EXTENDED_FLAGS;
//            // Raw-ish: turn off cooked editor and echo; keep PROCESSED_INPUT so Ctrl+C still works
//            inMode &= ~(ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT | ENABLE_QUICK_EDIT_MODE);
//            SetConsoleMode(hin, inMode | ENABLE_VIRTUAL_TERMINAL_INPUT);
//        }
//    }
//    #endregion

//    #region P/Invoke
//    private const int STD_OUTPUT_HANDLE = -11;
//    private const int STD_INPUT_HANDLE = -10;

//    private const uint WAIT_OBJECT_0 = 0x00000000;

//    [DllImport("kernel32.dll", SetLastError = true)] private static extern nint GetStdHandle(int nStdHandle);
//    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);
//    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);
//    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(nint hHandle, uint dwMilliseconds);
//    #endregion
//}
