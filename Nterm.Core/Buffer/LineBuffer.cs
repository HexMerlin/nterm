using System.Runtime.InteropServices;

namespace Nterm.Core.Buffer;

public class LineBuffer
{
    private readonly List<char> buf = [];
    private readonly List<(int pos, CharStyle charStyle)> styles = [];

    internal LineBuffer() { }

    internal LineBuffer(string str, Color foreground = default, Color background = default) : base() => Write(str, foreground, background);

    /// <summary>
    /// Writes the styled document to the terminal, repeating the applied styles to each segment of text.
    /// </summary>
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
    /// Current active style derived from last entry in styles array.
    /// </summary>
    private CharStyle CurrentStyle => styles.Count > 0 ? styles[^1].charStyle : default;

    internal void Write(char ch, Color foreground = default, Color background = default)
    {
        if (ch is '\n' or '\r')
            throw new ArgumentException($"Newline characters are not allowed in {nameof(LineBuffer)}. Use {nameof(TextBuffer)} for multi-line text.", nameof(ch));
        AddCharStyle(foreground, background);
        buf.Add(ch);
    }

    internal void Write(ReadOnlySpan<char> str, Color foreground = default, Color background = default)
    {
        AddCharStyle(foreground, background);
        buf.AddRange(str);
    }

    internal void TrimCapacity()
    {
        buf.Capacity = buf.Count;
        styles.Capacity = styles.Count;
    }

    private void AddCharStyle(Color foreground, Color background)
    {
        CharStyle charStyle = new(foreground, background);
        if (charStyle != CurrentStyle)
            styles.Add((buf.Count, charStyle));
    }

    public override string ToString() => new(CollectionsMarshal.AsSpan(buf));

}
