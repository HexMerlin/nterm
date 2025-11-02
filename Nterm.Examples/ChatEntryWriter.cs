using Nterm.Core;
using Nterm.Sixel;

using Nterm.Common;
namespace Nterm.Examples;

/// <summary>
/// Streaming chat message writer with avatar image and progressive text output.
/// Optimized for terminal rendering with precise character-cell alignment.
/// </summary>
/// <param name="AvatarImage">Terminal-ready avatar image with SIXEL or fallback encoding</param>
/// <remarks>
/// <para>
/// Provides side-by-side layout: avatar image left-aligned, streaming text content right-aligned with precise vertical positioning.
/// Supports progressive text output through Write, WriteLineBreak, ClearText, and EndWrite methods.
/// </para>
/// <para>
/// Console rendering preserves cursor state and maintains text positioning relative to avatar image bounds.
/// Character cell calculations leverage terminal capability detection for pixel-perfect alignment.
/// </para>
/// </remarks>
public sealed class ChatEntryWriter
{
    private const int TextMargin = 1;       // Character spacing between image and text

    /// <summary>
    /// Console-ready avatar image with SIXEL or fallback encoding.
    /// </summary>
    public TerminalImage AvatarImage { get; }

    private readonly Color DefaultForegroundColor;
    private readonly Color DefaultBackgroundColor;

    // State tracking for streaming text output
    private int _startLeft;
    private int _startTop;
    private int _textLeft;
    private int _textTop;
    private int _currentTextLine;
    private int _currentTextColumn;
    private bool _isWriting;

    /// <summary>
    /// Initializes streaming chat entry writer with avatar image.
    /// </summary>
    /// <param name="avatarImage">Console-ready avatar image with SIXEL or fallback encoding</param>
    public ChatEntryWriter(TerminalImage avatarImage)
    {
        AvatarImage = avatarImage;
        DefaultForegroundColor = Terminal.ForegroundColor;
        DefaultBackgroundColor = Terminal.BackgroundColor;
    }

    /// <summary>
    /// Begins streaming chat entry output by writing avatar image and positioning cursor for text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rendering sequence: captures cursor position → writes avatar image → calculates text positioning →
    /// positions cursor ready for streaming text output.
    /// </para>
    /// <para>
    /// Text horizontal positioning: <c>textLeft = imageLeft + imageWidth + 1</c> for consistent margin.
    /// Text vertical positioning: top-aligned with image for optimal visual layout.
    /// </para>
    /// </remarks>
    public void BeginWrite()
    {
        if (_isWriting)
            throw new InvalidOperationException("ChatEntryWriter is already in writing mode. Call EndWrite() first.");

        _startLeft = Terminal.CursorLeft;
        _startTop = Terminal.CursorTop;

        Terminal.WriteImage(AvatarImage.EncodedData);

        Size cellSize = new(10, 20); // Standard monospace cell size
        Size imageSize = AvatarImage.GetSizeInCharacters(cellSize);
        _textLeft = _startLeft + imageSize.Width + TextMargin;
        _textTop = _startTop + 1;
        _currentTextLine = 0;
        _currentTextColumn = 0;
        _isWriting = true;

        // Position cursor ready for text output
        Terminal.SetCursorPosition(_textLeft, _textTop);
    }

    /// <summary>
    /// Ends writing of the ChatEntryWriter. The cursor is reset to the absolute beginning of the next line that is ensured to not overwrite any 
    /// content in the ChatEntryWriter (Avatar or added text). 
    /// This is to allow for other following console output to start on a new fresh line.
    /// After the call to EndWrite we can consider the ChatEntryWriter to be 'Closed' and it will never be referenced to, or written to again.
    /// </summary>
    public void EndWrite()
    {
        if (!_isWriting)
            throw new InvalidOperationException("ChatEntryWriter is not in writing mode. Call BeginWrite() first.");

        Size cellSize = new(10, 20); // Standard monospace cell size
        Size imageSize = AvatarImage.GetSizeInCharacters(cellSize);
        int finalTop = _startTop + imageSize.Height;
        Terminal.SetCursorPosition(0, finalTop);

        _isWriting = false;

        //restore original foreground color before exiting
        Terminal.ForegroundColor = DefaultForegroundColor;
    }

    /// <summary>
    /// Writes text to current text cursor position of the ChatEntryWriter by appending to the current line. 
    /// The cursor position should be moved forward accordingly to allow for successive calls
    /// Its purpose is enable streaming text output in the ChatEntryWriter
    /// Important simplification: We can assume input text contains no newlines so that does need to be handled
    /// </summary>
    /// <param name="text">Partial text to be written.</param>
    /// <param name="foregroundColor">Foreground color for the written text.</param>
    public void Write(string text, Color foregroundColor)
    {
        if (!_isWriting)
            throw new InvalidOperationException("ChatEntryWriter is not in writing mode. Call BeginWrite() first.");

        if (string.IsNullOrEmpty(text))
            return;

        // Write text at current position
        Terminal.Write(text, foregroundColor);

        // Update horizontal position tracking
        _currentTextColumn += text.Length;
    }

    /// <summary>
    /// Ends the current line of ChatEntryWriter text. The cursor is moved down and to the default column start position (right of the image)
    /// </summary>
    public void WriteLineBreak()
    {
        if (!_isWriting)
            throw new InvalidOperationException("ChatEntryWriter is not in writing mode. Call BeginWrite() first.");

        // Move to next line
        _currentTextLine++;
        _currentTextColumn = 0;

        // Position cursor at start of next text line
        Terminal.SetCursorPosition(_textLeft, _textTop + _currentTextLine);
    }

    /// <summary>
    /// Clears all text in the ChatEntryWriter. The cursor is repositioned to allow for adding new text
    /// </summary>
    public void ClearText()
    {
        if (!_isWriting)
            throw new InvalidOperationException("ChatEntryWriter is not in writing mode. Call BeginWrite() first.");

        // Clear all text lines that were written
        // We need to clear from line 0 to _currentTextLine (inclusive)
        int textAreaWidth = Terminal.WindowWidth - _textLeft;
        string clearLine = new(' ', Math.Max(0, textAreaWidth));

        for (int line = 0; line <= _currentTextLine; line++)
        {
            Terminal.SetCursorPosition(_textLeft, _textTop + line);
            Terminal.Write(clearLine, DefaultForegroundColor, DefaultBackgroundColor);
        }

        // Reset to initial text position
        _currentTextLine = 0;
        _currentTextColumn = 0;
        Terminal.SetCursorPosition(_textLeft, _textTop);
    }
}
