using System.Diagnostics;

namespace Nterm.Core.Controls;

public delegate string AutosuggestProvider(string currentText, string previousSuggestion = "");

public sealed class AutosuggestOptions
{
    public Color TypedColor { get; init; } = Color.White;
    public Color SuggestionColor { get; init; } = Color.Gray;
    public AutosuggestProvider? GetNextSuggestion { get; init; }
    public AutosuggestProvider? GetPreviousSuggestion { get; init; }
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
        string suggestion = GetSuggestionSafe(suggest, string.Empty);

        TextInputController controller = new();

        TextInputKeyHandler handleSpecial = (ref TextInputState s, ConsoleKeyInfo keyInfo) =>
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Tab:
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        s.Text = suggestion;
                        s.CaretIndex = s.Text.Length;
                        suggestion = GetSuggestionSafe(suggest, s.Text);
                    }
                    Render(s.Text, suggestion, s.CaretIndex);
                    return true;
                case ConsoleKey.Enter:
                    if (!string.IsNullOrEmpty(suggestion))
                    {
                        s.Text = suggestion;
                        s.CaretIndex = s.Text.Length;
                    }
                    s.Done = true;
                    return true;
                case ConsoleKey.Escape:
                    if (s.Text.Length > 0)
                    {
                        s.Text = string.Empty;
                        s.CaretIndex = 0;
                        suggestion = GetSuggestionSafe(suggest, s.Text);
                        Render(s.Text, suggestion, s.CaretIndex);
                    }
                    else
                    {
                        s.Cancelled = true;
                        s.Done = true;
                    }
                    return true;
                case ConsoleKey.DownArrow:
                    if (_options.GetNextSuggestion != null)
                    {
                        suggestion = _options.GetNextSuggestion.Invoke(s.Text, suggestion);
                        Render(s.Text, suggestion, s.CaretIndex);
                    }
                    return true;
                case ConsoleKey.UpArrow:
                    if (_options.GetPreviousSuggestion != null)
                    {
                        suggestion = _options.GetPreviousSuggestion.Invoke(s.Text, suggestion);
                        Render(s.Text, suggestion, s.CaretIndex);
                    }
                    return true;
                default:
                    return false;
            }
        };

        TextInputState finalState = controller.Read(
            handleSpecial,
            (s, key) =>
            {
                // Update suggestion only when text changes (roughly: on char, backspace, delete)
                bool textChangingKey =
                    key.Key is ConsoleKey.Backspace or ConsoleKey.Delete
                    || (key.KeyChar != '\0' && !char.IsControl(key.KeyChar));
                if (textChangingKey)
                {
                    suggestion = GetSuggestionSafe(suggest, s.Text);
                }
                Render(s.Text, suggestion, s.CaretIndex);
            }
        );

        Render(finalState.Text, finalState.Text, finalState.CaretIndex);

        return finalState.Cancelled ? string.Empty : finalState.Text;
    }

    // Terminal preparation and input buffer clearing is handled by TextInputController

    private void Render(string typedText, string? suggestion, int caretIndex)
    {
        Terminal.SetCursorPosition(_anchorLeft, _anchorTop);
        TerminalEx.ClearLineFrom(_anchorLeft, _anchorTop);

        string display = ComputeDisplayText(typedText, suggestion);
        (int matchStart, int matchLength) = FindFirstMatch(display, typedText);

        WriteColored(display, matchStart, matchLength);
        PlaceCursor(matchStart, Math.Min(caretIndex, matchLength));
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

    private void PlaceCursor(int matchStart, int caretWithinMatch)
    {
        int caretColumn =
            _anchorLeft
            + Math.Clamp(
                matchStart + caretWithinMatch,
                0,
                Math.Max(0, Terminal.BufferWidth - _anchorLeft)
            );
        Terminal.SetCursorPosition(caretColumn, _anchorTop);
    }

    private static int IndexOfIgnoreCase(string source, string value) =>
        string.IsNullOrEmpty(value) ? 0 : source.IndexOf(value, StringComparison.OrdinalIgnoreCase);

    private static string TruncateToWidth(string text, int startColumn)
    {
        int maxWidth = Math.Max(0, Terminal.BufferWidth - startColumn);
        return text.Length <= maxWidth ? text : text[..Math.Min(maxWidth, text.Length)];
    }

    private static string GetSuggestionSafe(AutosuggestProvider provider, string typed)
    {
        try
        {
            return provider(typed);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Autosuggest provider error: {ex.Message}");
            return string.Empty;
        }
    }
}
