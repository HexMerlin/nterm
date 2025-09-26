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
/// </para>
/// <para>
/// Newline characters are not permitted in a <see cref="LineBuffer"/>. For multi-line output, use <see cref="TextBuffer"/>.
/// </para>
/// <para>This type is not thread-safe.</para>
/// </remarks>
/// <seealso cref="TextBuffer"/>
/// <seealso cref="CharStyle"/>
/// <seealso cref="Color"/>
public sealed class LineBuffer : IEquatable<LineBuffer>
{
    private readonly List<char> buf = [];
    private readonly ValueIntervalList<CharStyle> styleIntervals = new(0);

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
        styleIntervals = other.styleIntervals.Clone();
    }

    /// <summary>
    /// Length of the <see cref="LineBuffer"/>.
    /// </summary>
    public int Length => buf.Count;

    /// <summary>
    /// True if this <see cref="LineBuffer"/> is empty.
    /// </summary>
    internal bool IsEmpty => buf.Count == 0;

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
    /// This method only mutates the buffer;
    /// </remarks>
    internal void Append(char ch, Color foreground = default, Color background = default)
    {
        if (ch is '\n' or '\r')
            throw new ArgumentException(
                $"Newline characters are not allowed in {nameof(LineBuffer)}. Use {nameof(TextBuffer)} for multi-line text.",
                nameof(ch)
            );
        styleIntervals.AddOrReplace(buf.Count, new CharStyle(foreground, background));
        buf.Add(ch);
    }

    internal void Append(LineBuffer line)
    {
        if (line is null)
            return;
        // Clone the source buffer and styles to avoid mutations in the original object during the operation
        List<char> srcBuf = [.. line.buf];

        if (srcBuf.Count == 0)
            return;

        buf.AddRange(srcBuf);
        styleIntervals.Append(line.styleIntervals);
        styleIntervals.Resize(buf.Count);
    }

    /// <summary>
    /// Appends a span of characters to the buffer with the specified colors.
    /// </summary>
    /// <param name="str">The characters to write. Newline characters are not supported.</param>
    /// <param name="foreground">The foreground color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <param name="background">The background color to apply. Defaults to <see cref="Color.Transparent"/>.</param>
    /// <remarks>
    /// This method only mutates the buffer.
    /// Callers should ensure that <paramref name="str"/> does not contain newline characters.
    /// </remarks>
    internal void Append(
        ReadOnlySpan<char> str,
        Color foreground = default,
        Color background = default
    )
    {
        styleIntervals.AddOrReplace(buf.Count, new CharStyle(foreground, background));
        buf.AddRange(str);
        styleIntervals.Resize(buf.Count);
    }

    /// <summary>
    /// Truncates this buffer in-place to the specified maximum number of characters.
    /// </summary>
    /// <param name="maxCharacters">The maximum number of characters to keep.</param>
    /// <returns>This <see cref="LineBuffer"/>.</returns>
    internal LineBuffer Truncate(int maxCharacters)
    {
        // Normalize max to a non-negative value
        maxCharacters = Math.Max(0, maxCharacters);

        if (buf.Count <= maxCharacters)
            return this;

        // Remove extra characters beyond the limit
        buf.RemoveRange(maxCharacters, buf.Count - maxCharacters);
        styleIntervals.Resize(maxCharacters);

        return this;
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

            // Walk style runs via ValueIntervalList and intersect with [start, end)
            foreach (ValueInterval<CharStyle> interval in styleIntervals.GetRanges())
            {
                int runStart = interval.Start;
                int runEnd = interval.End;
                CharStyle charStyle = interval.Value;
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
        // styleIntervals manages its own storage
    }

    internal (List<char> buf, ValueIntervalList<CharStyle> styles) GetInternalData() =>
        (buf, styleIntervals);

    /// <summary>
    /// Returns the plain text contained in the buffer without styling.
    /// </summary>
    public override string ToString() => new(CollectionsMarshal.AsSpan(buf));

    public bool Equals(LineBuffer? other) => Equals(other, null);

    public bool Equals(LineBuffer? other, StringComparison? comparisonType, bool compareStyles) =>
        other is not null
        && Equals(
            CollectionsMarshal.AsSpan(other.buf),
            other.styleIntervals,
            comparisonType,
            compareStyles
        );

    public static bool Equals(
        LineBuffer? left,
        LineBuffer? right,
        StringComparison? comparisonType
    ) => ReferenceEquals(left, right) || left?.Equals(right, comparisonType) == true;

    public static bool operator ==(LineBuffer? left, LineBuffer? right) =>
        Equals(left, right, null);

    public static bool operator !=(LineBuffer? left, LineBuffer? right) =>
        !Equals(left, right, null);

    public bool Equals(LineBuffer? other, StringComparison? comparisonType) =>
        ReferenceEquals(this, other)
        || other != null
            && Equals(
                CollectionsMarshal.AsSpan(other.buf),
                other.styleIntervals,
                comparisonType,
                true
            );

    /// <summary>
    /// Indicates whether this <see cref="LineBuffer"/> is equal to the specified string. Does not consider styles.
    /// </summary>
    /// <param name="other">The string to compare with this <see cref="LineBuffer"/>.</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified string is equal to this <see cref="LineBuffer"/>.</returns>
    public bool TextEquals(ReadOnlySpan<char> other, StringComparison? comparisonType = null) =>
        Equals(other, new ValueIntervalList<CharStyle>(0), comparisonType, false);

    public override bool Equals(object? obj) => obj is LineBuffer other && Equals(other);

    internal bool Equals(
        ReadOnlySpan<char> otherStr,
        ValueIntervalList<CharStyle> otherStyles,
        StringComparison? comparisonType,
        bool compareStyles
    ) =>
        CollectionsMarshal.AsSpan(buf).Equals(otherStr, comparisonType ?? StringComparison.Ordinal)
        && (!compareStyles || styleIntervals.Equals(otherStyles));

    public override int GetHashCode()
    {
        HashCode hc = new();
        foreach (char ch in buf)
            hc.Add(ch);

        hc.Add(styleIntervals);
        return hc.ToHashCode();
    }

    internal void SetStyle(CharStyle style)
    {
        SetStyle(0, Length, style);
    }

    internal void SetStyle(int start, int end, CharStyle style)
    {
        styleIntervals.InsertAndReplace(start, end, style);
    }
}
