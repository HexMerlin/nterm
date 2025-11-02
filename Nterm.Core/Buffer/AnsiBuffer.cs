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
    /// <summary>
    /// ANSI escape character (ESC, 0x1B).
    /// </summary>
    private const char ESC = '\x1b';

    /// <summary>
    /// Control Sequence Introducer for Select Graphic Rendition (SGR) - foreground RGB color.
    /// Format: ESC[38;2;{r};{g};{b}m
    /// </summary>
    private const string SGR_FG_RGB_PREFIX = "[38;2;";

    /// <summary>
    /// Control Sequence Introducer for Select Graphic Rendition (SGR) - background RGB color.
    /// Format: ESC[48;2;{r};{g};{b}m
    /// </summary>
    private const string SGR_BG_RGB_PREFIX = "[48;2;";

    /// <summary>
    /// SGR sequence terminator.
    /// </summary>
    private const char SGR_END = 'm';

    /// <summary>
    /// SGR reset sequence - resets all graphic modes (styles and colors).
    /// </summary>
    private const string SGR_RESET = "[0m";

    /// <summary>
    /// SGR foreground color reset to default.
    /// </summary>
    private const string SGR_FG_DEFAULT = "[39m";

    /// <summary>
    /// SGR background color reset to default.
    /// </summary>
    private const string SGR_BG_DEFAULT = "[49m";

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
        // Foreground: emit RGB sequence or reset to terminal default
        this._buffer.Append(ESC);
        if (foreground.IsTransparent)
        {
            this._buffer.Append(SGR_FG_DEFAULT);
        }
        else
        {
            this._buffer.Append(SGR_FG_RGB_PREFIX);
            this._buffer.Append(foreground.R);
            this._buffer.Append(';');
            this._buffer.Append(foreground.G);
            this._buffer.Append(';');
            this._buffer.Append(foreground.B);
            this._buffer.Append(SGR_END);
        }

        // Background: emit RGB sequence or reset to terminal default
        this._buffer.Append(ESC);
        if (background.IsTransparent)
        {
            this._buffer.Append(SGR_BG_DEFAULT);
        }
        else
        {
            this._buffer.Append(SGR_BG_RGB_PREFIX);
            this._buffer.Append(background.R);
            this._buffer.Append(';');
            this._buffer.Append(background.G);
            this._buffer.Append(';');
            this._buffer.Append(background.B);
            this._buffer.Append(SGR_END);
        }
    }
}
