using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Nterm.Core.Buffer;

/// <summary>
/// Mutable text accumulator that batches styled text across multiple logical lines.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="TextBuffer"/> is composed of one or more <see cref="LineBuffer"/> instances. Text written
/// via <see cref="Append(char, Color, Color)"/>, <see cref="Append(ReadOnlySpan{char}, Color, Color)"/>,
/// or <see cref="AppendLine(ReadOnlySpan{char}, Color, Color)"/> is appended to the current line, and
/// <see cref="AppendLine()"/> starts a new one.
/// </para>
/// <para>
/// Calling <see cref="Write()"/> renders the accumulated content to the terminal, delegating styling
/// application to the underlying line buffers. Writing does not clear the buffer; it may be written out
/// multiple times if desired.
/// <para>
/// If post-processing is needed (e.g. prefixing lines with line numbers), iterate over <see cref="Lines"/> property instead and write each line individually.
/// </para>
/// </para>
/// <para>This type is not thread-safe.</para>
/// </remarks>
/// <seealso cref="LineBuffer"/>
/// <seealso cref="Color"/>
public class TextBuffer : IEquatable<TextBuffer>
{
    /// <summary>
    /// Backing store for logical lines contained in this buffer.
    /// </summary>
    private readonly List<LineBuffer> lines = [new LineBuffer()];

    /// <summary>
    /// Initializes a new, empty <see cref="TextBuffer"/> containing a single empty line.
    /// </summary>
    public TextBuffer() { }

    /// <summary>
    /// Initializes a new <see cref="TextBuffer"/> and writes the specified text with the given colors.
    /// </summary>
    /// <param name="str">The text to write into the buffer.</param>
    /// <param name="foreground">The foreground color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <param name="background">The background color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <remarks>
    /// This constructor is equivalent to creating an empty buffer and then calling
    /// <see cref="Append(ReadOnlySpan{char}, Color, Color)"/> with the same arguments.
    /// </remarks>
    public TextBuffer(string str, Color foreground = default, Color background = default)
        : base() => Append(str, foreground, background);

    private TextBuffer(TextBuffer other)
    {
        lines = [.. other.lines.Select(l => l.Clone())];
    }

    /// <summary>
    /// Gets the sequence of logical lines contained in this buffer.
    /// </summary>
    /// <value>A read-only view of the internal line collection.</value>
    public IReadOnlyList<LineBuffer> Lines => lines;

    /// <summary>
    /// An empty <see cref="TextBuffer"/>.
    /// </summary>
    public static TextBuffer Empty => new();

    /// <summary>
    /// Indicates whether this <see cref="TextBuffer"/> is empty.
    /// </summary>
    public bool IsEmpty => lines[0].IsEmpty;

    /// <summary>
    /// Number of lines in the <see cref="TextBuffer"/>.
    /// </summary>
    public int LineCount => lines.Count;

    /// <summary>
    /// Length of the <see cref="TextBuffer"/>.
    /// </summary>
    public int Length => lines.Sum(l => l.Length);

    /// <summary>
    /// Maximum width of any line in the <see cref="TextBuffer"/>.
    /// </summary>
    public int MaxWidth => lines.Max(l => l.Length);

    private LineBuffer CurrentLine => lines[^1];

    /// <summary>
    /// Appends a single character to the current line with the specified colors.
    /// </summary>
    /// <param name="ch">The character to write.</param>
    /// <param name="foreground">The foreground color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <param name="background">The background color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <returns>This <see cref="TextBuffer"/> instance</returns>
    /// <remarks>
    /// This method mutates the current line. No terminal output occurs until <see cref="Write()"/> is called.
    /// </remarks>
    public TextBuffer Append(char ch, Color foreground = default, Color background = default)
    {
        CurrentLine.Append(ch, foreground, background);
        return this;
    }

    public TextBuffer Append(TextBuffer text)
    {
        foreach (LineBuffer line in text.lines)
        {
            Append(line);
        }
        return this;
    }

    internal TextBuffer Append(LineBuffer line)
    {
        lines.Add(line.Clone());
        return this;
    }

    /// <summary>
    /// Appends a span of characters to the buffer with the specified colors.
    /// </summary>
    /// <param name="str">The characters to write. May contain line breaks.</param>
    /// <param name="foreground">The foreground color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <param name="background">The background color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <returns>This <see cref="TextBuffer"/> instance</returns>
    /// <remarks>
    /// <para>
    /// If <paramref name="str"/> contains line breaks, it is enumerated using
    /// <see cref="System.MemoryExtensions.EnumerateLines(ReadOnlySpan{char})"/>, and each segment is written
    /// to the current line. After writing each subsequent segment (beyond the first), a new line is started
    /// to separate segments.
    /// </para>
    /// <para>No terminal output occurs until <see cref="Write()"/> is called.</para>
    /// </remarks>
    public TextBuffer Append(
        ReadOnlySpan<char> str,
        Color foreground = default,
        Color background = default
    )
    {
        int lineCount = 0;
        foreach (ReadOnlySpan<char> line in str.EnumerateLines())
        {
            if (lineCount > 0)
                _ = AppendLine();
            CurrentLine.Append(line, foreground, background);
            lineCount++;
        }
        return this;
    }

    /// <summary>
    /// Appends the specified text to the buffer and then starts a new line.
    /// </summary>
    /// <param name="str">The text to write.</param>
    /// <param name="foreground">The foreground color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <param name="background">The background color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <returns>This <see cref="TextBuffer"/> instance</returns>
    /// <remarks>No terminal output occurs until <see cref="Write()"/> is called.</remarks>
    public TextBuffer AppendLine(
        ReadOnlySpan<char> str,
        Color foreground = default,
        Color background = default
    ) => Append(str, foreground, background).AppendLine();

    /// <summary>
    /// Ends the current line and starts a new, empty line.
    /// </summary>
    /// <returns>This <see cref="TextBuffer"/> instance</returns>
    /// <remarks>
    /// Trims the capacity of the current line before creating the next line to minimize memory usage.
    /// No terminal output occurs until <see cref="Write()"/> is called.
    /// </remarks>
    public TextBuffer AppendLine()
    {
        CurrentLine.TrimCapacity();
        lines.Add(new LineBuffer());
        return this;
    }

    /// <summary>
    /// Truncates the <see cref="TextBuffer"/> to the specified maximum width.
    /// </summary>
    /// <param name="maxWidth">The maximum width to truncate to.</param>
    /// <returns>A new <see cref="TextBuffer"/> instance</returns>
    public TextBuffer TruncateWidth(int maxWidth)
    {
        if (IsEmpty)
            return new(this);

        TextBuffer result = new();
        foreach (LineBuffer line in Lines)
        {
            result.Append(line.Truncate(maxWidth));
        }

        return this;
    }

    public TextBuffer TruncateCharacters(int maxCharacters)
    {
        if (IsEmpty)
            return new(this);

        TextBuffer result = new();
        foreach (LineBuffer line in Lines)
        {
            if (maxCharacters <= 0)
                break;

            result.Append(line.Truncate(maxCharacters));
        }

        return result;
    }

    /// <summary>
    /// Splits the <see cref="TextBuffer"/> into an array of new <see cref="TextBuffer"/> instances.
    /// </summary>
    /// <param name="pattern">The regular expression pattern to split on.</param>
    /// <param name="options">The regular expression options to use.</param>
    /// <returns>An array of new <see cref="TextBuffer"/> instances.</returns>
    public TextBuffer[] Split(
        [StringSyntax("Regex", ["options"])] string pattern,
        RegexOptions options
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Clones the <see cref="TextBuffer"/>.
    /// </summary>
    /// <returns>A new <see cref="TextBuffer"/> with the same content.</returns>
    public TextBuffer Clone() => new(this);

    /// <summary>
    /// Returns the concatenated textual representation of the buffer.
    /// </summary>
    /// <returns>A string formed by joining lines with a newline character ('\n').</returns>
    /// <remarks>
    /// Styling information is not included; only the plain text content is returned.
    /// </remarks>
    public override string ToString() => string.Join('\n', Lines);

    public static explicit operator string(TextBuffer textBuffer) => textBuffer.ToString();

    public static implicit operator TextBuffer(string str) => new(str);

    public static bool Equals(TextBuffer? left, TextBuffer? right) =>
        left == right || left?.Equals(right) == true;

    public static bool operator ==(TextBuffer? left, TextBuffer? right) => Equals(left, right);

    public static bool operator !=(TextBuffer? left, TextBuffer? right) => !Equals(left, right);

    public bool Equals(TextBuffer? other) => other != null && Lines.SequenceEqual(other.Lines);

    public override bool Equals(object? obj) => obj is TextBuffer other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Lines);
}
