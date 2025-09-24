using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
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
/// If post-processing is needed (e.g. prefixing lines with line numbers), iterate over <see cref="Lines"/> property instead and write each line individually.
/// </para>
/// </para>
/// <para>This type is not thread-safe.</para>
/// </remarks>
/// <seealso cref="LineBuffer"/>
/// <seealso cref="Color"/>
public sealed class TextBuffer : IEquatable<TextBuffer>
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
    public TextBuffer(
        ReadOnlySpan<char> str,
        Color foreground = default,
        Color background = default
    )
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
    public static TextBuffer Empty { get; } = new();

    /// <summary>
    /// Indicates whether this <see cref="TextBuffer"/> is empty.
    /// </summary>
    public bool IsEmpty => lines[0].IsEmpty && lines.Count == 1;

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
        if (text.IsEmpty)
            return this;

        // Append the first line to the last line. The motivation for this is that the last
        // line does not end with a newline character.
        CurrentLine.Append(text.lines[0]);
        foreach (LineBuffer line in text.lines[1..])
        {
            Append(line);
        }
        return this;
    }

    /// <summary>
    /// Appends a (cloned) <see cref="LineBuffer"/> to the current line.
    /// </summary>
    /// <param name="line">The <see cref="LineBuffer"/> to append.</param>
    /// <returns>This <see cref="TextBuffer"/> instance</returns>
    internal TextBuffer Append(LineBuffer line)
    {
        CurrentLine.Append(line.Clone());
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
        if (LineCount == 0)
            return [new TextBuffer()];

        if (string.IsNullOrEmpty(pattern))
            return [Clone()];

        // Prepare global view: cumulative starts for each line in the joined string with '\n' separators
        int lineCount = LineCount;
        int[] lineStarts = new int[lineCount];
        int[] lineLengths = new int[lineCount];
        int cumulative = 0;
        for (int i = 0; i < lineCount; i++)
        {
            lineStarts[i] = cumulative;
            int len = Lines[i].Length;
            lineLengths[i] = len;
            cumulative += len;
            if (i < lineCount - 1)
                cumulative += 1; // account for '\n' between lines
        }

        string global = ToString();
        Regex regex = new(pattern, options);
        MatchCollection matches = regex.Matches(global);
        if (matches.Count == 0)
            return [Clone()];

        List<TextBuffer> results = [];
        int cursor = 0;

        foreach (Match m in matches)
        {
            // Preceding non-matching text
            AddSegment(cursor, m.Index - cursor, lineCount, lineStarts, lineLengths, results);

            // Include captured groups like Regex.Split does when the pattern contains captures
            for (int g = 1; g < m.Groups.Count; g++)
            {
                Group grp = m.Groups[g];
                if (grp.Success && grp.Length > 0)
                    AddSegment(grp.Index, grp.Length, lineCount, lineStarts, lineLengths, results);
            }

            cursor = m.Index + m.Length;
        }

        // Trailing remainder
        AddSegment(cursor, global.Length - cursor, lineCount, lineStarts, lineLengths, results);

        return [.. results];
    }

    private static void AppendSliceWithStyles(
        TextBuffer target,
        LineBuffer sourceLine,
        int startLocal,
        int endLocal,
        bool firstSlice
    )
    {
        if (!firstSlice)
            _ = target.AppendLine();

        (List<char> buf, List<(int pos, CharStyle charStyle)> styles) data =
            sourceLine.GetInternalData();
        List<char> buf = data.buf;
        List<(int pos, CharStyle charStyle)> styles = data.styles;

        // Walk style runs and intersect with [startLocal, endLocal)
        for (int i = -1; i < styles.Count; i++)
        {
            int runStart = i >= 0 ? styles[i].pos : 0;
            int runEnd = i < styles.Count - 1 ? styles[i + 1].pos : buf.Count;
            CharStyle charStyle = i >= 0 ? styles[i].charStyle : default;

            int s = Math.Max(runStart, startLocal);
            int e = Math.Min(runEnd, endLocal);
            if (s >= e)
                continue;

            _ = target.Append(
                CollectionsMarshal.AsSpan(buf[s..e]),
                charStyle.Color,
                charStyle.BackColor
            );
        }
    }

    private static int FindLineIndex(
        int globalPos,
        int lineCount,
        int[] lineStarts,
        int[] lineLengths
    )
    {
        // Linear search is acceptable for small line counts; can be optimized to binary search if needed
        for (int i = 0; i < lineCount; i++)
        {
            int start = lineStarts[i];
            int endExclusive = start + lineLengths[i] + (i < lineCount - 1 ? 1 : 0); // include '\n' slot
            if (globalPos < endExclusive)
                return i;
        }
        return Math.Max(0, lineCount - 1);
    }

    private void AddSegment(
        int globalStart,
        int length,
        int lineCount,
        int[] lineStarts,
        int[] lineLengths,
        List<TextBuffer> results
    )
    {
        if (length <= 0)
            return;

        int globalEnd = globalStart + length;

        // Find starting line index
        int lineIndex = FindLineIndex(globalStart, lineCount, lineStarts, lineLengths);
        int localStart = globalStart - lineStarts[lineIndex];
        if (localStart == lineLengths[lineIndex] && lineIndex < lineCount - 1)
        {
            // Start is exactly on a '\n' separator, move to next line
            lineIndex++;
            localStart = 0;
        }

        TextBuffer segment = new();
        bool firstSlice = true;

        while (lineIndex < lineCount)
        {
            int thisLineStartGlobal = lineStarts[lineIndex];
            int thisLineEndGlobalExclusive = thisLineStartGlobal + lineLengths[lineIndex];

            if (globalStart >= thisLineEndGlobalExclusive && lineIndex < lineCount - 1)
            {
                // Starting beyond this line, skip
                lineIndex++;
                localStart = 0;
                continue;
            }

            int sliceStartLocal = localStart;
            int sliceEndLocal = Math.Min(lineLengths[lineIndex], globalEnd - thisLineStartGlobal);
            if (sliceEndLocal <= sliceStartLocal)
            {
                // No more coverage on this line
                break;
            }

            AppendSliceWithStyles(
                segment,
                Lines[lineIndex],
                sliceStartLocal,
                sliceEndLocal,
                firstSlice
            );
            firstSlice = false;

            if (globalEnd <= thisLineEndGlobalExclusive)
                break; // Completed within this line

            // Continue into next line
            lineIndex++;
            localStart = 0;
        }

        results.Add(segment);
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
        ReferenceEquals(left, right)
        || left?.Equals(right, StringComparison.OrdinalIgnoreCase) == true;

    public static bool operator ==(TextBuffer? left, TextBuffer? right) => Equals(left, right);

    public static bool operator !=(TextBuffer? left, TextBuffer? right) => !Equals(left, right);

    public bool Equals(TextBuffer? other) => Equals(other, null, true);

    public bool Equals(TextBuffer? other, StringComparison? comparisonType) =>
        Equals(other, comparisonType, true);

    /// <summary>
    /// Indicates whether this <see cref="TextBuffer"/> is equal to the specified string. Does not consider styles.
    /// </summary>
    /// <param name="other">The string to compare with this <see cref="TextBuffer"/>.</param>
    /// <returns><see langword="true"/> <b>iff</b> the specified string is equal to this <see cref="TextBuffer"/>.</returns>
    public bool TextEquals(ReadOnlySpan<char> other) => Equals(new TextBuffer(other), null, false);

    public override bool Equals(object? obj) => obj is TextBuffer other && Equals(other);

    private bool Equals(TextBuffer? other, StringComparison? comparisonType, bool compareStyles)
    {
        if (other == null)
            return false;

        if (Lines.Count != other.Lines.Count)
            return false;

        for (int i = 0; i < Lines.Count; i++)
        {
            (List<char> otherStr, List<(int, CharStyle)> otherStyles) = other
                .Lines[i]
                .GetInternalData();

            if (
                !Lines[i]
                    .Equals(
                        CollectionsMarshal.AsSpan(otherStr),
                        otherStyles,
                        comparisonType,
                        compareStyles
                    )
            )
            {
                return false;
            }
        }
        return true;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int res = 0x2D2816FE;
            foreach (LineBuffer line in Lines)
            {
                res = res * 31 + line.GetHashCode();
            }

            return res;
        }
    }

    private int GetGlobalLength() => Length + Math.Max(0, LineCount - 1);

    internal void SetColor(Color foreground) => SetColor(0, GetGlobalLength(), foreground, default);

    internal void SetColor(int start, int end, Color foreground, Color background = default)
    {
        int lc = LineCount;
        if (lc == 0)
            return;

        // Build global mapping
        int[] lineStarts = new int[lc];
        int[] lineLengths = new int[lc];
        int cumulative = 0;
        for (int i = 0; i < lc; i++)
        {
            lineStarts[i] = cumulative;
            int len = Lines[i].Length;
            lineLengths[i] = len;
            cumulative += len;
            if (i < lc - 1)
                cumulative += 1; // '\n' separator
        }

        int totalLength = cumulative;
        start = Math.Clamp(start, 0, totalLength);
        end = Math.Clamp(end, 0, totalLength);
        if (end <= start)
            return;
        if (foreground == default && background == default)
            return;

        for (int i = 0; i < lc; i++)
        {
            int ls = lineStarts[i];
            int le = ls + lineLengths[i];
            int segStart = Math.Max(start, ls);
            int segEnd = Math.Min(end, le);
            if (segEnd <= segStart)
                continue;

            int localStart = segStart - ls;
            int localEnd = segEnd - ls;
            lines[i].SetColor(localStart, localEnd, foreground, background);
        }
    }
}
