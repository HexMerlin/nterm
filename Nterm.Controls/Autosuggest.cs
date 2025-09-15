using System.Diagnostics;

namespace NTerm.Controls;

public delegate string? AutosuggestProvider(string currentText);

public sealed class AutosuggestOptions
{
    public Color TypedColor { get; init; } = Color.White;
    public Color SuggestionColor { get; init; } = Color.Gray;
}

public interface IAutosuggest
{
    string Read(AutosuggestProvider suggest, AutosuggestOptions? options = null);
}

public static class Autosuggest
{
    public static string Read(AutosuggestProvider suggest, AutosuggestOptions? options = null) =>
        new AutosuggestControl().Read(suggest, options);
}

public sealed class AutosuggestControl : IAutosuggest
{
    private int _anchorLeft;
    private int _anchorTop;
    private AutosuggestOptions _options = new();

    public string Read(AutosuggestProvider suggest, AutosuggestOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(suggest);

        using TerminalState state = new();
        _anchorLeft = state.OriginalCursorLeft;
        _anchorTop = state.OriginalCursorTop;
        _options = options ?? new AutosuggestOptions();

        PrepareTerminal();
        ClearInputBuffer();

        string typedText = string.Empty;
        string? suggestion = GetSuggestionSafe(suggest, typedText);

        bool done = false;
        while (!done)
        {
            Render(typedText, suggestion);

            ConsoleKeyInfo keyInfo = Terminal.ReadKey(true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.Tab:
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        typedText = suggestion;
                        suggestion = GetSuggestionSafe(suggest, typedText);
                    }
                    break;
                case ConsoleKey.Enter:
                    done = true;
                    break;
                case ConsoleKey.Backspace:
                    if (typedText.Length > 0)
                    {
                        typedText = typedText[..^1];
                        suggestion = GetSuggestionSafe(suggest, typedText);
                    }
                    break;
                default:
                    if (keyInfo.KeyChar != '\0' && !char.IsControl(keyInfo.KeyChar))
                    {
                        typedText += keyInfo.KeyChar;
                        suggestion = GetSuggestionSafe(suggest, typedText);
                    }
                    break;
            }
        }

        // Final render shows accepted text without trailing suggestion
        Render(typedText, typedText);

        return typedText;
    }

    private static void PrepareTerminal()
    {
        try
        {
            Terminal.CursorVisible = true;
        }
        catch (PlatformNotSupportedException)
        {
            Debug.WriteLine("Cursor visibility manipulation not supported on this platform.");
        }
    }

    private static void ClearInputBuffer()
    {
        int clearedKeys = 0;
        const int maxKeysToClear = 1000;
        while (Terminal.KeyAvailable && clearedKeys < maxKeysToClear)
        {
            _ = Terminal.ReadKey(true);
            clearedKeys++;
        }
    }

    private void Render(string typedText, string? suggestion)
    {
        Terminal.SetCursorPosition(_anchorLeft, _anchorTop);
        TerminalEx.ClearLineFrom(_anchorLeft, _anchorTop);

        string display = ComputeDisplayText(typedText, suggestion);
        (int matchStart, int matchLength) = FindFirstMatch(display, typedText);

        WriteColored(display, matchStart, matchLength);
        PlaceCursor(matchStart, matchLength);
    }

    private string ComputeDisplayText(string typedText, string? suggestion)
    {
        if (string.IsNullOrEmpty(suggestion))
            return typedText;

        // If typed text is contained in suggestion, display the whole suggestion
        int idx = IndexOfIgnoreCase(suggestion, typedText);
        if (idx >= 0)
            return TruncateToWidth(suggestion, _anchorLeft);

        // Otherwise, display typed + suggestion appended
        string combined = typedText + suggestion;
        return TruncateToWidth(combined, _anchorLeft);
    }

    private static (int matchStart, int matchLength) FindFirstMatch(
        string display,
        string typedText
    )
    {
        if (string.IsNullOrEmpty(typedText))
            return (0, 0);
        int idx = IndexOfIgnoreCase(display, typedText);
        if (idx < 0)
            return (0, Math.Min(typedText.Length, display.Length));
        return (idx, Math.Min(typedText.Length, Math.Max(0, display.Length - idx)));
    }

    private void WriteColored(string display, int matchStart, int matchLength)
    {
        int safeMatchStart = Math.Clamp(matchStart, 0, display.Length);
        int safeMatchEnd = Math.Clamp(matchStart + matchLength, safeMatchStart, display.Length);

        string left = display[..safeMatchStart];
        string mid = display[safeMatchStart..safeMatchEnd];
        string right = display[safeMatchEnd..];

        Terminal.ForegroundColor = _options.SuggestionColor;
        Terminal.Write(left);

        Terminal.ForegroundColor = _options.TypedColor;
        Terminal.Write(mid);

        Terminal.ForegroundColor = _options.SuggestionColor;
        Terminal.Write(right);
    }

    private void PlaceCursor(int matchStart, int matchLength)
    {
        int caretColumn =
            _anchorLeft
            + Math.Clamp(
                matchStart + matchLength,
                0,
                Math.Max(0, Terminal.BufferWidth - _anchorLeft)
            );
        Terminal.SetCursorPosition(caretColumn, _anchorTop);
    }

    private static int IndexOfIgnoreCase(string source, string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;
        return source.IndexOf(value, StringComparison.OrdinalIgnoreCase);
    }

    private static string TruncateToWidth(string text, int startColumn)
    {
        int maxWidth = Math.Max(0, Terminal.BufferWidth - startColumn);
        if (text.Length <= maxWidth)
            return text;
        return text[..Math.Min(maxWidth, text.Length)];
    }

    private static string? GetSuggestionSafe(AutosuggestProvider provider, string typed)
    {
        try
        {
            return provider(typed);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Autosuggest provider error: {ex.Message}");
            return null;
        }
    }
}
