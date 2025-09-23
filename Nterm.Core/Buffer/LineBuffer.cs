using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Nterm.Core.Buffer;

/// <summary>
/// Mutable, single-line text accumulator that tracks style changes across a contiguous sequence of characters.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="LineBuffer"/> records characters and the positions at which a new <see cref="CharStyle"/> is applied.
/// When <see cref="Write"/> is called, the content is emitted to the terminal as contiguous segments with the
/// appropriate colors.
/// </para>
/// <para>
/// Newline characters are not permitted in a <see cref="LineBuffer"/>. For multi-line output, use <see cref="TextBuffer"/>.
/// </para>
/// <para>This type is not thread-safe.</para>
/// </remarks>
/// <seealso cref="TextBuffer"/>
/// <seealso cref="CharStyle"/>
/// <seealso cref="Color"/>
public class LineBuffer : IEquatable<LineBuffer>
{
    private readonly List<char> buf = [];
    private readonly List<(int pos, CharStyle charStyle)> styles = [];

    /// <summary>
    /// Initializes a new, empty <see cref="LineBuffer"/>.
    /// </summary>
    /// <remarks>
    /// Intended to be composed by <see cref="TextBuffer"/> for multi-line scenarios.
    /// </remarks>
    internal LineBuffer() { }

    /// <summary>
    /// Initializes a new <see cref="LineBuffer"/> and writes the specified text with the given colors.
    /// </summary>
    /// <param name="str">The text to write into the buffer. Must not contain newline characters.</param>
    /// <param name="foreground">The foreground color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <param name="background">The background color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <remarks>
    /// This constructor is equivalent to creating an empty buffer and then calling
    /// <see cref="Append(ReadOnlySpan{char}, Color, Color)"/> with the same arguments.
    /// </remarks>
    internal LineBuffer(string str, Color foreground = default, Color background = default)
        : base() => Append(str, foreground, background);

    private LineBuffer(LineBuffer other)
    {
        buf = [.. other.buf];
        styles = [.. other.styles];
    }

    /// <summary>
    /// Length of the <see cref="LineBuffer"/>.
    /// </summary>
    public int Length => buf.Count;

    /// <summary>
    /// Idicates whether this <see cref="LineBuffer"/> is empty.
    /// </summary>
    internal bool IsEmpty => buf.Count == 0;

    /// <summary>
    /// Gets the current active style derived from the last entry in the style list.
    /// </summary>
    private CharStyle CurrentStyle => styles.Count > 0 ? styles[^1].charStyle : default;

    /// <summary>
    /// Appends a single character to the buffer with the specified colors.
    /// </summary>
    /// <param name="ch">The character to write. Must not be a newline character.</param>
    /// <param name="foreground">The foreground color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <param name="background">The background color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="ch"/> is a newline character. Use <see cref="TextBuffer"/> for multi-line text.
    /// </exception>
    /// <remarks>
    /// This method only mutates the buffer; no terminal output occurs until <see cref="Write"/> is called.
    /// </remarks>
    internal void Append(char ch, Color foreground = default, Color background = default)
    {
        if (ch is '\n' or '\r')
            throw new ArgumentException(
                $"Newline characters are not allowed in {nameof(LineBuffer)}. Use {nameof(TextBuffer)} for multi-line text.",
                nameof(ch)
            );
        AddCharStyle(foreground, background);
        buf.Add(ch);
    }

    /// <summary>
    /// Appends a span of characters to the buffer with the specified colors.
    /// </summary>
    /// <param name="str">The characters to write. Newline characters are not supported.</param>
    /// <param name="foreground">The foreground color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <param name="background">The background color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <remarks>
    /// This method only mutates the buffer; no terminal output occurs until <see cref="Write"/> is called.
    /// Callers should ensure that <paramref name="str"/> does not contain newline characters.
    /// </remarks>
    internal void Append(
        ReadOnlySpan<char> str,
        Color foreground = default,
        Color background = default
    )
    {
        AddCharStyle(foreground, background);
        buf.AddRange(str);
    }

    /// <summary>
    /// Truncates the buffer to the specified maximum number of characters.
    /// </summary>
    /// <param name="maxCharacters">The maximum number of characters to truncate to.</param>
    /// <returns>The number of characters removed from the buffer.</returns>
    internal LineBuffer Truncate(int maxCharacters)
    {
        if (buf.Count <= maxCharacters)
            return new(this);
        LineBuffer result = new();
        for (int i = -1; i < styles.Count; i++)
        {
            int start = i >= 0 ? styles[i].pos : 0;
            int end = i < styles.Count - 1 ? styles[i + 1].pos : buf.Count;
            CharStyle charStyle = i >= 0 ? styles[i].charStyle : default;
            result.Append(
                CollectionsMarshal.AsSpan(buf[start..end]),
                charStyle.Color,
                charStyle.BackColor
            );
        }

        return result;
    }

    public LineBuffer[] Split(
        [StringSyntax("Regex", ["options"])] string pattern,
        RegexOptions options
    )
    {
        // Fast-path: empty buffer → single empty segment
        if (buf.Count == 0)
            return [new LineBuffer()];

        if (string.IsNullOrEmpty(pattern))
            return [Clone()];

        string input = ToString();
        Regex regex = new(pattern, options);
        MatchCollection matches = regex.Matches(input);

        if (matches.Count == 0)
            return [Clone()];

        List<LineBuffer> parts = [];
        int cursor = 0;

        foreach (Match m in matches)
        {
            // Preceding non-matching text
            AddSegment(cursor, m.Index - cursor);

            // Include captured groups like Regex.Split does when the pattern contains captures
            for (int g = 1; g < m.Groups.Count; g++)
            {
                Group grp = m.Groups[g];
                if (grp.Success && grp.Length > 0)
                    AddSegment(grp.Index, grp.Length);
            }

            cursor = m.Index + m.Length;
        }

        // Trailing remainder
        AddSegment(cursor, buf.Count - cursor);

        return [.. parts];

        void AddSegment(int start, int length)
        {
            if (length <= 0)
                return;

            int end = start + length;
            LineBuffer segment = new();

            // Walk style runs and intersect with [start, end)
            for (int i = -1; i < styles.Count; i++)
            {
                int runStart = i >= 0 ? styles[i].pos : 0;
                int runEnd = i < styles.Count - 1 ? styles[i + 1].pos : buf.Count;
                CharStyle charStyle = i >= 0 ? styles[i].charStyle : default;

                int s = Math.Max(runStart, start);
                int e = Math.Min(runEnd, end);
                if (s >= e)
                    continue;

                segment.Append(
                    CollectionsMarshal.AsSpan(buf[s..e]),
                    charStyle.Color,
                    charStyle.BackColor
                );
            }

            parts.Add(segment);
        }
    }

    internal LineBuffer Clone() => new(this);

    /// <summary>
    /// Trims internal list capacities to their current counts to reduce memory usage.
    /// </summary>
    internal void TrimCapacity()
    {
        buf.Capacity = buf.Count;
        styles.Capacity = styles.Count;
    }

    /// <summary>
    /// Records a style change at the current buffer position if it differs from the active style.
    /// </summary>
    /// <param name="foreground">The desired foreground color.</param>
    /// <param name="background">The desired background color.</param>
    /// <remarks>
    /// If the requested style equals the <see cref="CurrentStyle"/>, no entry is added, avoiding redundant segments.
    /// </remarks>
    private void AddCharStyle(Color foreground, Color background)
    {
        CharStyle charStyle = new(foreground, background);
        if (charStyle != CurrentStyle)
            styles.Add((buf.Count, charStyle));
    }

    internal (List<char> buf, List<(int pos, CharStyle charStyle)> styles) GetInternalData() =>
        (buf, styles);

    /// <summary>
    /// Returns the plain text contained in the buffer without styling.
    /// </summary>
    public override string ToString() => new(CollectionsMarshal.AsSpan(buf));

    public static bool Equals(LineBuffer? left, LineBuffer? right) =>
        left == right || left?.Equals(right) == true;

    public static bool operator ==(LineBuffer? left, LineBuffer? right) => Equals(left, right);

    public static bool operator !=(LineBuffer? left, LineBuffer? right) => !Equals(left, right);

    public bool Equals(LineBuffer? other) =>
        other != null && buf.SequenceEqual(other.buf) && styles.SequenceEqual(other.styles);

    public override bool Equals(object? obj) => obj is LineBuffer other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(buf, styles);

    internal void SetColor(Color foreground = default, Color background = default) =>
        throw new NotImplementedException();

    internal void SetColor(int start, int end, Color foreground, Color background) =>
        throw new NotImplementedException();
}
