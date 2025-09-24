namespace Nterm.Core.Buffer;

public static class TextBufferExtensions
{
    /// <summary>
    /// Truncates the <see cref="TextBuffer"/> to the specified maximum width.
    /// </summary>
    /// <param name="maxWidth">The maximum width to truncate to.</param>
    /// <returns>A new <see cref="TextBuffer"/> instance</returns>
    public static TextBuffer TruncateWidth(this TextBuffer text, int maxWidth)
    {
        if (text.IsEmpty)
            return text.Clone();

        TextBuffer result = new();
        foreach (LineBuffer line in text.Lines)
        {
            result.Append(line.Truncate(maxWidth));
        }

        return result;
    }

    public static TextBuffer TruncateCharacters(this TextBuffer text, int maxCharacters)
    {
        if (text.IsEmpty)
            return text.Clone();

        TextBuffer result = new();
        foreach (LineBuffer line in text.Lines)
        {
            if (maxCharacters <= 0)
                break;

            LineBuffer truncatedLine = line.Truncate(Math.Min(maxCharacters, line.Length));
            result.Append(truncatedLine);

            maxCharacters -= line.Length;
        }

        return text;
    }
}
