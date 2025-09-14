
namespace Nterm.Core.Buffer;

public class TextBuffer
{
    private readonly List<LineBuffer> lines = [new LineBuffer()];

    public TextBuffer() { }

    public TextBuffer(string str, Color foreground = default, Color background = default) : base() => Write(str, foreground, background);

    public IReadOnlyList<LineBuffer> Lines => lines;

    public void Write(char ch, Color foreground = default, Color background = default) => lines[^1].Write(ch, foreground, background);

    public void Write(ReadOnlySpan<char> str, Color foreground = default, Color background = default)
    {
        int lineCount = 0;
        foreach (ReadOnlySpan<char> line in str.EnumerateLines())
        {
            lines[^1].Write(line, foreground, background);
            if (lineCount > 0) WriteLine();
            lineCount++;
        }
    }

    public void WriteLine(ReadOnlySpan<char> str, Color foreground = default, Color background = default)
    {
        Write(str, foreground, background);
        WriteLine();
    }

    public void WriteLine()
    {
        lines[^1].TrimCapacity();
        lines.Add(new LineBuffer());
    }

    /// <summary>
    /// Writes the styled document to the terminal, applying the appropriate styles to each segment of text.
    /// </summary>
    public void Flush()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (i > 0) Terminal.WriteLine(); //write newline between lines
            lines[i].Flush();

        }
    }

    public override string ToString() => string.Join('\n', lines);

}
