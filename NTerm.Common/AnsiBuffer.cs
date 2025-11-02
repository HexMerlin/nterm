using System.Text;

namespace Nterm.Common;

/// <summary>
/// Lightweight mutable buffer storing ANSI/VT-coded text with embedded color sequences.
/// </summary>
/// <remarks>
/// <para>
/// Color sequences embedded directly using VT100/ANSI escape codes for 24-bit RGB colors. Sending no color means terminal default is used.
/// </para>
/// <para>
/// Extract complete ANSI-coded string via <see cref="ToString"/> for direct terminal output.
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
    private readonly StringBuilder buffer;

    /// <summary>
    /// Initializes empty <see cref="AnsiBuffer"/>.
    /// </summary>
    public AnsiBuffer()
    {
        buffer = new StringBuilder();
    }

    /// <summary>
    /// Initializes <see cref="AnsiBuffer"/> with specified initial capacity.
    /// </summary>
    /// <param name="capacity">Initial capacity in characters.</param>
    public AnsiBuffer(int capacity) => buffer = new StringBuilder(capacity);


    /// <summary>
    /// Initializes <see cref="AnsiBuffer"/> with specified initial text content.
    /// </summary>
    /// <param name="text">Initial text content.</param>
    /// <param name="foreground">Foreground color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <param name="background">Background color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    public AnsiBuffer(string text, Color foreground = default, Color background = default) : this() => Append(text, foreground, background);

    /// <summary>
    /// Initializes <see cref="AnsiBuffer"/> with specified initial text content.
    /// </summary>
    /// <param name="text">Initial text content.</param>
    /// <param name="foreground">Foreground color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <param name="background">Background color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    public AnsiBuffer(ReadOnlySpan<char> text, Color foreground = default, Color background = default) : this() => Append(text, foreground, background);

    /// <summary>
    /// Current length of the buffer in characters (including ANSI escape sequences).
    /// </summary>
    public int Length => buffer.Length;

    /// <summary>
    /// Current capacity of the internal buffer.
    /// </summary>
    public int Capacity
    {
        get => buffer.Capacity;
        set => buffer.Capacity = value;
    }

    /// <summary>
    /// Appends character with optional foreground and background colors.
    /// </summary>
    /// <param name="ch">Character to append.</param>
    /// <param name="foreground">Foreground color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <param name="background">Background color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <returns><see langword="this"/> instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Always emits both foreground and background color sequences—non-transparent colors emit
    /// RGB sequences; transparent colors emit reset sequences restoring terminal default colors.
    /// </para>
    /// <para>
    /// Terminal state explicitly set for each append operation, eliminating color pollution
    /// from previous operations.
    /// </para>
    /// </remarks>
    public AnsiBuffer Append(char ch, Color foreground = default, Color background = default)
    {
        AppendColorSequences(foreground, background);
        buffer.Append(ch);
        return this;
    }

    /// <summary>
    /// Appends text with optional foreground and background colors.
    /// </summary>
    /// <param name="text">Text to append.</param>
    /// <param name="foreground">Foreground color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <param name="background">Background color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <returns><see langword="this"/> instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Always emits both foreground and background color sequences—non-transparent colors emit
    /// RGB sequences; transparent colors emit reset sequences restoring terminal default colors.
    /// </para>
    /// <para>
    /// Terminal state explicitly set for each append operation, eliminating color pollution
    /// from previous operations.
    /// </para>
    /// </remarks>
    public AnsiBuffer Append(string text, Color foreground = default, Color background = default)
    {
        AppendColorSequences(foreground, background);
        buffer.Append(text);
        return this;
    }

    /// <summary>
    /// Appends text with optional foreground and background colors.
    /// </summary>
    /// <param name="text">Text to append.</param>
    /// <param name="foreground">Foreground color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <param name="background">Background color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <returns><see langword="this"/> instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Always emits both foreground and background color sequences—non-transparent colors emit
    /// RGB sequences; transparent colors emit reset sequences restoring terminal default colors.
    /// </para>
    /// <para>
    /// Terminal state explicitly set for each append operation, eliminating color pollution
    /// from previous operations.
    /// </para>
    /// </remarks>
    public AnsiBuffer Append(ReadOnlySpan<char> text, Color foreground = default, Color background = default)
    {
        AppendColorSequences(foreground, background);
        buffer.Append(text);
        return this;
    }

    /// <summary>
    /// Appends text followed by line terminator, with optional foreground and background colors.
    /// </summary>
    /// <param name="text">Text to append.</param>
    /// <param name="foreground">Foreground color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <param name="background">Background color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <returns><see langword="this"/> instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <see cref="Append(string, Color, Color)"/> followed by <see cref="AppendLine()"/>.
    /// </remarks>
    public AnsiBuffer AppendLine(string text, Color foreground = default, Color background = default)
    {
        Append(text, foreground, background);
        buffer.AppendLine();
        return this;
    }

    /// <summary>
    /// Appends text followed by line terminator, with optional foreground and background colors.
    /// </summary>
    /// <param name="text">Text to append.</param>
    /// <param name="foreground">Foreground color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <param name="background">Background color. <see cref="Color.Transparent"/> (default) resets to terminal default.</param>
    /// <returns><see langword="this"/> instance for method chaining.</returns>
    /// <remarks>
    /// Equivalent to <see cref="Append(ReadOnlySpan{char}, Color, Color)"/> followed by <see cref="AppendLine()"/>.
    /// </remarks>
    public AnsiBuffer AppendLine(ReadOnlySpan<char> text, Color foreground = default, Color background = default)
    {
        Append(text, foreground, background);
        buffer.AppendLine();
        return this;
    }

    /// <summary>
    /// Appends line terminator.
    /// </summary>
    /// <returns><see langword="this"/> instance for method chaining.</returns>
    public AnsiBuffer AppendLine()
    {
        buffer.AppendLine();
        return this;
    }

    /// <summary>
    /// Clears all content.
    /// </summary>
    /// <returns><see langword="this"/> instance for method chaining.</returns>
    public AnsiBuffer Clear()
    {
        buffer.Clear();
        return this;
    }

    /// <summary>
    /// ANSI-coded string representation of buffer content.
    /// </summary>
    /// <returns>Complete string with embedded ANSI/VT escape sequences for colors.</returns>
    /// <remarks>
    /// Output writable directly to any ANSI/VT100-compatible terminal.
    /// Terminal color state affected by embedded sequences—calling code responsible for
    /// restoring terminal state after output.
    /// </remarks>
    public override string ToString() => buffer.ToString();

    /// <summary>
    /// ANSI sequence resetting terminal with specified default colors.
    /// </summary>
    /// <param name="foreground">Default foreground color. <see cref="Color.Transparent"/> skips foreground reset.</param>
    /// <param name="background">Default background color. <see cref="Color.Transparent"/> skips background reset.</param>
    /// <returns>ANSI escape sequence string clearing screen and setting default colors.</returns>
    /// <remarks>
    /// <para>
    /// Emits OSC (Operating System Command) sequences changing terminal default colors:
    /// </para>
    /// <list type="number">
    /// <item>Set terminal default foreground (OSC 10)—affects what SGR 39 resets to</item>
    /// <item>Set terminal default background (OSC 11)—affects what SGR 49 resets to</item>
    /// <item>Reset all graphic modes (SGR 0)—applies new defaults</item>
    /// <item>Clear entire screen (ED 2J)—fills viewport with new background</item>
    /// <item>Move cursor to home position (CUP H)</item>
    /// </list>
    /// <para>
    /// Use at application startup to establish known default colors independent of user terminal
    /// configuration. Essential for themed applications where text visibility must be guaranteed.
    /// </para>
    /// <para>
    /// After calling, all subsequent uses of <see cref="Color.Transparent"/> reset to these new
    /// defaults instead of terminal original defaults.
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
    /// Appends ANSI color sequences to internal buffer based on provided colors.
    /// </summary>
    /// <param name="foreground">Foreground color. Transparent emits reset to terminal default.</param>
    /// <param name="background">Background color. Transparent emits reset to terminal default.</param>
    /// <remarks>
    /// <para>
    /// Always emits both foreground and background sequences:
    /// </para>
    /// <list type="bullet">
    /// <item>Non-transparent colors emit RGB sequence (<c>ESC[38;2;R;G;Bm</c> or <c>ESC[48;2;R;G;Bm</c>)</item>
    /// <item>Transparent colors emit reset sequence (<c>ESC[39m</c> or <c>ESC[49m</c>) restoring terminal defaults</item>
    /// </list>
    /// </remarks>
    private void AppendColorSequences(Color foreground, Color background)
    {
        string fgSequence = foreground.IsTransparent
            ? SGR_FG_DEFAULT
            : $"{CSI}38;2;{foreground.R};{foreground.G};{foreground.B}m";

        string bgSequence = background.IsTransparent
            ? SGR_BG_DEFAULT
            : $"{CSI}48;2;{background.R};{background.G};{background.B}m";

        buffer.Append(fgSequence + bgSequence);
    }
}
