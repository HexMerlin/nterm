using System.Text;

namespace Nterm.Core.Buffer;

/// <summary>
/// Lightweight mutable buffer storing ANSI/VT-coded text with embedded color sequences.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AnsiBuffer"/> provides a simplified alternative to <see cref="TextBuffer"/>,
/// storing content as a single ANSI-encoded string rather than structured line objects.
/// </para>
/// <para>
/// Color sequences are embedded directly using VT100/ANSI escape codes for 24-bit RGB colors.
/// The buffer does not track terminal state - calling code is responsible for restoring
/// terminal colors after writing buffer content.
/// </para>
/// <para>
/// Extract the complete ANSI-coded string via <see cref="ToString"/> for direct terminal output.
/// </para>
/// </remarks>
public sealed class AnsiBuffer
{
    // ============================================================================
    // ANSI/VT Escape Sequence Constants
    // ============================================================================

    /// <summary>
    /// ESC - Escape character (0x1B).
    /// </summary>
    private const char ESC = '\x1b';

    /// <summary>
    /// CSI - Control Sequence Introducer (ESC[).
    /// </summary>
    private const string CSI = "\x1b[";

    /// <summary>
    /// OSC - Operating System Command introducer (ESC]).
    /// </summary>
    private const string OSC = "\x1b]";

    /// <summary>
    /// ST - String Terminator (ESC\).
    /// </summary>
    private const string ST = "\x1b\\";

    // ============================================================================
    // SGR (Select Graphic Rendition) Sequences
    // ============================================================================

    /// <summary>
    /// SGR - Reset all graphic modes (styles and colors to defaults).
    /// </summary>
    private const string SGR_RESET = "\x1b[0m";

    /// <summary>
    /// SGR - Set foreground to default color.
    /// </summary>
    private const string SGR_FG_DEFAULT = "\x1b[39m";

    /// <summary>
    /// SGR - Set background to default color.
    /// </summary>
    private const string SGR_BG_DEFAULT = "\x1b[49m";

    // ============================================================================
    // Cursor Positioning Sequences
    // ============================================================================

    /// <summary>
    /// CUP - Cursor Position to home (1,1).
    /// </summary>
    private const string CUP_HOME = "\x1b[H";

    // ============================================================================
    // Erase Sequences
    // ============================================================================

    /// <summary>
    /// ED - Erase in Display (entire screen).
    /// </summary>
    private const string ED_ENTIRE_SCREEN = "\x1b[2J";

    /// <summary>
    /// ED - Erase scrollback buffer.
    /// </summary>
    private const string ED_SCROLLBACK = "\x1b[3J";

    /// <summary>
    /// Internal storage for ANSI-coded content.
    /// </summary>
    private readonly StringBuilder _buffer;

    /// <summary>
    /// Initializes a new, empty <see cref="AnsiBuffer"/>.
    /// </summary>
    public AnsiBuffer()
    {
        this._buffer = new StringBuilder();
    }

    /// <summary>
    /// Initializes a new <see cref="AnsiBuffer"/> with specified initial capacity.
    /// </summary>
    /// <param name="capacity">Initial capacity in characters.</param>
    public AnsiBuffer(int capacity)
    {
        this._buffer = new StringBuilder(capacity);
    }

    /// <summary>
    /// Current length of the buffer in characters (including ANSI escape sequences).
    /// </summary>
    public int Length => this._buffer.Length;

    /// <summary>
    /// Current capacity of the internal buffer.
    /// </summary>
    public int Capacity
    {
        get => this._buffer.Capacity;
        set => this._buffer.Capacity = value;
    }

    /// <summary>
    /// Appends text to the buffer with optional foreground and background colors.
    /// </summary>
    /// <param name="text">Text to append.</param>
    /// <param name="foreground">Foreground color. Use <see cref="Color.Transparent"/> (default) to reset to terminal default.</param>
    /// <param name="background">Background color. Use <see cref="Color.Transparent"/> (default) to reset to terminal default.</param>
    /// <returns>This <see cref="AnsiBuffer"/> instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Always emits both foreground and background color sequences:
    /// Non-transparent colors emit RGB sequences; transparent colors emit reset sequences
    /// to restore terminal default colors.
    /// </para>
    /// <para>
    /// Terminal state is explicitly set for each append operation, eliminating color pollution
    /// from previous operations.
    /// </para>
    /// </remarks>
    public AnsiBuffer Append(string text, Color foreground = default, Color background = default)
    {
        AppendColorSequences(foreground, background);
        this._buffer.Append(text);
        return this;
    }

    /// <summary>
    /// Appends text to the buffer followed by a line terminator, with optional foreground and background colors.
    /// </summary>
    /// <param name="text">Text to append.</param>
    /// <param name="foreground">Foreground color. Use <see cref="Color.Transparent"/> (default) to reset to terminal default.</param>
    /// <param name="background">Background color. Use <see cref="Color.Transparent"/> (default) to reset to terminal default.</param>
    /// <returns>This <see cref="AnsiBuffer"/> instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to calling <see cref="Append(string, Color, Color)"/> followed by <see cref="AppendLine()"/>.
    /// </remarks>
    public AnsiBuffer AppendLine(string text, Color foreground = default, Color background = default)
    {
        Append(text, foreground, background);
        this._buffer.AppendLine();
        return this;
    }

    /// <summary>
    /// Appends a line terminator to the buffer.
    /// </summary>
    /// <returns>This <see cref="AnsiBuffer"/> instance for method chaining.</returns>
    public AnsiBuffer AppendLine()
    {
        this._buffer.AppendLine();
        return this;
    }

    /// <summary>
    /// Clears all content from the buffer.
    /// </summary>
    /// <returns>This <see cref="AnsiBuffer"/> instance for method chaining.</returns>
    public AnsiBuffer Clear()
    {
        this._buffer.Clear();
        return this;
    }

    /// <summary>
    /// ANSI-coded string representation of the buffer content.
    /// </summary>
    /// <returns>Complete string with embedded ANSI/VT escape sequences for colors.</returns>
    /// <remarks>
    /// Output can be written directly to any ANSI/VT100-compatible terminal.
    /// Terminal color state will be affected by embedded sequences - calling code
    /// is responsible for restoring terminal state after output.
    /// </remarks>
    public override string ToString() => this._buffer.ToString();

    /// <summary>
    /// ANSI sequence to reset terminal with specified default colors.
    /// </summary>
    /// <param name="foreground">Default foreground color. Use <see cref="Color.Transparent"/> to skip foreground reset.</param>
    /// <param name="background">Default background color. Use <see cref="Color.Transparent"/> to skip background reset.</param>
    /// <returns>ANSI escape sequence string that clears screen and sets default colors.</returns>
    /// <remarks>
    /// <para>
    /// Emits minimal OSC (Operating System Command) sequences to change terminal's default colors:
    /// 1. Set terminal default foreground (OSC 10) - affects what SGR 39 resets to
    /// 2. Set terminal default background (OSC 11) - affects what SGR 49 resets to
    /// 3. Reset all graphic modes (SGR 0) - applies new defaults
    /// 4. Clear entire screen (ED 2J) - fills viewport with new background
    /// 5. Move cursor to home position (CUP H)
    /// </para>
    /// <para>
    /// Use at application startup to establish known default colors independent of
    /// user's terminal configuration. Essential for themed applications where text
    /// visibility must be guaranteed.
    /// </para>
    /// <para>
    /// After calling this, all subsequent uses of <see cref="Color.Transparent"/> will
    /// reset to these new defaults instead of the terminal's original defaults.
    /// </para>
    /// <para>
    /// Example: <c>Console.Write(AnsiBuffer.Reset(Color.White, Color.Black));</c>
    /// </para>
    /// </remarks>
    public static string Reset(Color foreground = default, Color background = default)
    {
        string oscForeground = foreground.IsTransparent
            ? string.Empty
            : $"{OSC}10;rgb:{foreground.R:x2}/{foreground.G:x2}/{foreground.B:x2}{ST}";

        string oscBackground = background.IsTransparent
            ? string.Empty
            : $"{OSC}11;rgb:{background.R:x2}/{background.G:x2}/{background.B:x2}{ST}";

        return oscForeground + oscBackground + SGR_RESET + ED_ENTIRE_SCREEN + CUP_HOME;
    }

    /// <summary>
    /// Appends ANSI color sequences to the internal buffer based on provided colors.
    /// </summary>
    /// <param name="foreground">Foreground color. Transparent emits reset to terminal default.</param>
    /// <param name="background">Background color. Transparent emits reset to terminal default.</param>
    /// <remarks>
    /// Always emits both foreground and background sequences:
    /// - Non-transparent colors emit RGB sequence (ESC[38;2;R;G;Bm or ESC[48;2;R;G;Bm)
    /// - Transparent colors emit reset sequence (ESC[39m or ESC[49m) to restore terminal defaults
    /// </remarks>
    private void AppendColorSequences(Color foreground, Color background)
    {
        string fgSequence = foreground.IsTransparent
            ? SGR_FG_DEFAULT
            : $"{CSI}38;2;{foreground.R};{foreground.G};{foreground.B}m";

        string bgSequence = background.IsTransparent
            ? SGR_BG_DEFAULT
            : $"{CSI}48;2;{background.R};{background.G};{background.B}m";

        this._buffer.Append(fgSequence + bgSequence);
    }
}
