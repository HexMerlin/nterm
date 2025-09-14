using System.Runtime.InteropServices;

namespace Nterm.Core.Buffer;

/// <summary>
/// Mutable, single-line text accumulator that tracks style changes across a contiguous sequence of characters.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="LineBuffer"/> records characters and the positions at which a new <see cref="CharStyle"/> is applied.
/// When <see cref="Flush"/> is called, the content is emitted to the terminal as contiguous segments with the
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
public class LineBuffer
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
    /// <see cref="Write(ReadOnlySpan{char}, Color, Color)"/> with the same arguments.
    /// </remarks>
    internal LineBuffer(string str, Color foreground = default, Color background = default) : base() => Write(str, foreground, background);

    /// <summary>
    /// Writes the buffered content to the terminal as styled segments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The buffer is partitioned using the recorded style boundaries. For each segment, the corresponding
    /// foreground and background colors are applied, and the characters are written to the terminal.
    /// </para>
    /// <para>
    /// Invoking <see cref="Flush"/> does not clear the buffer.
    /// </para>
    /// </remarks>
    public void Flush()
    {
        TrimCapacity();
        Span<char> span = CollectionsMarshal.AsSpan(buf);

        for (int i = -1; i < styles.Count; i++)
        {
            int start = i >= 0 ? styles[i].pos : 0;
            int end = i < styles.Count - 1 ? styles[i + 1].pos : buf.Count;
            CharStyle charStyle = i >= 0 ? styles[i].charStyle : default;
            Terminal.Write(span[start..end], charStyle.Color, charStyle.BackColor);
        }
    }

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
    /// This method only mutates the buffer; no terminal output occurs until <see cref="Flush"/> is called.
    /// </remarks>
    internal void Write(char ch, Color foreground = default, Color background = default)
    {
        if (ch is '\n' or '\r')
            throw new ArgumentException($"Newline characters are not allowed in {nameof(LineBuffer)}. Use {nameof(TextBuffer)} for multi-line text.", nameof(ch));
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
    /// This method only mutates the buffer; no terminal output occurs until <see cref="Flush"/> is called.
    /// Callers should ensure that <paramref name="str"/> does not contain newline characters.
    /// </remarks>
    internal void Write(ReadOnlySpan<char> str, Color foreground = default, Color background = default)
    {
        AddCharStyle(foreground, background);
        buf.AddRange(str);
    }

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

    /// <summary>
    /// Returns the plain text contained in the buffer without styling.
    /// </summary>
    public override string ToString() => new(CollectionsMarshal.AsSpan(buf));

}
