using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;

namespace NTerm.Core;

/// <summary>
/// Optimized 24‑bit color console writer with VT output and RAW VT input.
/// Keeps SIXEL output exactly as before; replaces reading with a tiny VT-aware editor
/// so Backspace deletes a char and Ctrl+Backspace deletes a word on Windows Terminal.
/// </summary>
public static class Terminal
{
    private static readonly Lock writeLock = new();

    private static Color lastFg = Color.Transparent;
    private static Color lastBg = Color.Transparent;

    static Terminal()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
    }

    public static string Title
    {
        get => OperatingSystem.IsWindows() ? Console.Title : "";
        set
        {
            if (OperatingSystem.IsWindows())
                Console.Title = value;
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

    public static int WindowTop => Console.WindowTop;
    public static int WindowLeft => Console.WindowLeft;
    public static int WindowWidth => Console.WindowWidth;
    public static int WindowHeight => Console.WindowHeight;
    public static int BufferWidth => Console.BufferWidth;
    public static int BufferHeight => Console.BufferHeight;
    public static int CursorLeft { get => Console.CursorLeft; set => Console.CursorLeft = value; }
    public static int CursorTop { get => Console.CursorTop; set => Console.CursorTop = value; }

    public static bool CursorVisible
    {
        [SupportedOSPlatform("windows")]
        get => Console.CursorVisible;
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        set => Console.CursorVisible = value;
    }

    /// <summary>True if there is input available without blocking.</summary>
    public static bool KeyAvailable => Console.KeyAvailable;

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
            _ = sb.Append("\b \b");
        }
        Write(sb.ToString());
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
    public static void Clear(Color backgroundColor = default)
    {
        Console.Clear();
        lock (writeLock)
        {
            if (backgroundColor != default)
                BackgroundColor = backgroundColor;

            WriteBg(BackgroundColor);
            WriteInternal($"{Constants.ESC}{Constants.EraseDisplayAll}");
            WriteInternal($"{Constants.ESC}{Constants.CursorHome}");
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
            Console.Write(ch);
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
    public static void Write(
        ReadOnlySpan<char> str,
        Color foreground = default,
        Color background = default
    )
    {
        lock (writeLock)
        {
            if (str.IsEmpty)
                return;

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
    public static void WriteLine(
        ReadOnlySpan<char> str,
        Color foreground = default,
        Color background = default
    )
    {
        Write(str, foreground, background);
        WriteLine();
    }

    public static void WriteLine() => Console.WriteLine();

    /// <summary>
    /// Writes pre-encoded image data to console.
    /// </summary>
    /// <param name="imageData">Complete image data including all control sequences</param>
    /// <remarks>
    /// Ultra-optimized direct image output. Image data must be complete and ready for terminal consumption.
    /// Leverages existing optimized UTF-8 encoding infrastructure for maximum performance.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteImage(ReadOnlySpan<char> imageData) => Write(imageData);

    internal static void WriteInternal(ReadOnlySpan<char> str) => Console.Write(str);

    /// <summary>
    /// Read a key (VT‑aware). Distinguishes Backspace vs Ctrl+Backspace via BS(0x08) vs DEL(0x7F).
    /// Handles basic CSI/SS3 cursor keys and Delete (CSI 3~).
    /// </summary>
    public static ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);

    public static string ReadLine() => Console.ReadLine() ?? "";

    public static void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

    #region Private Methods

    private static void WriteBg(Color c) =>
        WriteInternal(
            $"{Constants.ESC}{Constants.SGR_BG_TRUECOLOR_PREFIX}{c.R};{c.G};{c.B}{Constants.SGR_END}"
        );

    private static void WriteFg(Color c) =>
        WriteInternal(
            $"{Constants.ESC}{Constants.SGR_FG_TRUECOLOR_PREFIX}{c.R};{c.G};{c.B}{Constants.SGR_END}"
        );

    #endregion
}
