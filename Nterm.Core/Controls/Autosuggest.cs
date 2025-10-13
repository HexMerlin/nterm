using System.Diagnostics;
using Nterm.Core.Buffer;

namespace Nterm.Core.Controls;

public delegate TextItem<TValue> AutosuggestProvider<TValue>(
    string currentText,
    TextItem<TValue>? previousSuggestion = null
);

public sealed class AutosuggestOptions<TValue>
{
    public Color TypedColor { get; init; } = Color.White;
    public Color SuggestionColor { get; init; } = Color.Gray;
    public AutosuggestProvider<TValue>? GetNextSuggestion { get; init; }
    public AutosuggestProvider<TValue>? GetPreviousSuggestion { get; init; }
}

public record AutosuggestResult<TValue>(string TypedText, TextItem<TValue>? LastSuggestion);

public interface IAutosuggest<TValue>
{
    AutosuggestResult<TValue> Read(
        AutosuggestProvider<TValue> suggest,
        AutosuggestOptions<TValue>? options = null
    );
}

public static class Autosuggest
{
    public static AutosuggestResult<TValue> Read<TValue>(
        AutosuggestProvider<TValue> suggest,
        AutosuggestOptions<TValue>? options = null
    ) => new AutosuggestControl<TValue>().Read(suggest, options);
}

public sealed class AutosuggestControl<TValue> : IAutosuggest<TValue>
{
    private int _anchorLeft;
    private int _anchorTop;
    private AutosuggestOptions<TValue> _options = new();

    public AutosuggestResult<TValue> Read(
        AutosuggestProvider<TValue> suggest,
        AutosuggestOptions<TValue>? options = null
    )
    {
        ArgumentNullException.ThrowIfNull(suggest);

        using TerminalState terminalState = new();
        _anchorLeft = terminalState.OriginalCursorLeft;
        _anchorTop = terminalState.OriginalCursorTop;
        _options = options ?? new AutosuggestOptions<TValue>();
        TextItem<TValue> suggestion = GetSuggestionSafe(suggest, string.Empty, null);
        Render(string.Empty, suggestion.Text, 0);
        TextInputController controller =
            new(
                (state, anchorTop) =>
                {
                    // Default render path (text changed): recompute suggestion from text and render
                    suggestion = GetSuggestionSafe(suggest, state.Text, null);
                    Render(state.Text, suggestion.Text, state.CaretIndex);
                }
            );

        controller.KeyUp += (sender, e) =>
        {
            // Handle special keys before default editing
            switch (e.KeyInfo.Key)
            {
                case ConsoleKey.Tab:
                    if (!suggestion.IsEmpty())
                    {
                        TextInputState accepted = e.ProposedState with
                        {
                            Text = suggestion.Text.ToString(),
                            CaretIndex = suggestion.Text.Length
                        };
                        e.ProposedState = accepted;
                        e.Handled = true;
                    }
                    break;
                case ConsoleKey.Enter:
                    if (!suggestion.IsEmpty())
                    {
                        TextInputState accepted = e.ProposedState with
                        {
                            Text = suggestion.Text.ToString(),
                            CaretIndex = suggestion.Text.Length
                        };
                        e.ProposedState = accepted;
                    }
                    e.ProposedState = e.ProposedState with { Done = true };
                    e.Handled = true;
                    break;
                case ConsoleKey.Escape:
                    if (suggestion.IsEmpty())
                    {
                        e.ProposedState = e.ProposedState with { Cancelled = true, Done = true };
                    }
                    suggestion = TextItem.Empty<TValue>();
                    Render(e.ProposedState.Text, suggestion.Text, e.ProposedState.CaretIndex);
                    e.Handled = true;
                    break;
                case ConsoleKey.DownArrow:
                    if (_options.GetNextSuggestion != null)
                    {
                        suggestion = _options.GetNextSuggestion.Invoke(
                            e.ProposedState.Text,
                            suggestion
                        );
                        // Suggestion change without text change – render immediately
                        Render(e.ProposedState.Text, suggestion.Text, e.ProposedState.CaretIndex);
                        e.Handled = true;
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (_options.GetPreviousSuggestion != null)
                    {
                        suggestion = _options.GetPreviousSuggestion.Invoke(
                            e.ProposedState.Text,
                            suggestion
                        );
                        Render(e.ProposedState.Text, suggestion.Text, e.ProposedState.CaretIndex);
                        e.Handled = true;
                    }
                    break;
            }
        };

        TextInputState finalState = controller.Read();

        Render(finalState.Text, finalState.Text, finalState.CaretIndex);

        return new AutosuggestResult<TValue>(finalState.Text, suggestion);
    }

    // Terminal preparation and input buffer clearing is handled by TextInputController

    private void Render(string typedText, TextBuffer suggestion, int caretIndex)
    {
        Terminal.SetCursorPosition(_anchorLeft, _anchorTop);
        TerminalEx.ClearLineFrom(_anchorLeft, _anchorTop);

        TextBuffer display = ComputeDisplayText(typedText, suggestion);
        (int matchStart, int matchLength) = FindFirstMatch(display, typedText);

        WriteColored(display, matchStart, matchLength);
        PlaceCursor(matchStart, Math.Min(caretIndex, matchLength));
    }

    private TextBuffer ComputeDisplayText(string typedText, TextBuffer suggestion)
    {
        if (suggestion.IsEmpty)
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
        TextBuffer display,
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

    private void WriteColored(TextBuffer display, int matchStart, int matchLength)
    {
        int safeMatchStart = Math.Clamp(matchStart, 0, display.Length);
        int safeMatchEnd = Math.Clamp(matchStart + matchLength, safeMatchStart, display.Length);

        display.SetStyle(new CharStyle(_options.SuggestionColor, default));
        display.SetStyle(safeMatchStart, safeMatchEnd, new CharStyle(_options.TypedColor, default));
        Terminal.Write(display);
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

    private static int IndexOfIgnoreCase(TextBuffer source, string value) =>
        string.IsNullOrEmpty(value)
            ? 0
            : source.ToString().IndexOf(value, StringComparison.OrdinalIgnoreCase);

    private static TextBuffer TruncateToWidth(TextBuffer text, int startColumn)
    {
        int maxWidth = Math.Max(0, Terminal.BufferWidth - startColumn);
        return text.TruncateWidth(maxWidth);
    }

    private static TextItem<TValue> GetSuggestionSafe(
        AutosuggestProvider<TValue> provider,
        string typed,
        TextItem<TValue>? previous
    )
    {
        try
        {
            return provider(typed, previous);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Autosuggest provider error: {ex.Message}");
            return TextItem.Empty<TValue>();
        }
    }
}
