using System.Collections.Generic;
using System.Diagnostics;

namespace Nterm.Core.Controls;

/// <summary>
/// Represents the state of the text input, including the current text, caret position, and status flags.
/// </summary>
/// <param name="Text"></param>
/// <param name="CaretIndex"></param>
/// <param name="Done"></param>
/// <param name="Cancelled"></param>
public readonly record struct TextInputState(
    string Text,
    int CaretIndex,
    bool Done,
    bool Cancelled
);

/// <summary>
/// Event arguments for key events in the <see cref="TextInputController"/>.
/// </summary>
/// <param name="keyInfo">The key information of the pressed key.</param>
/// <param name="currentState">The current state of the text input.</param>
public sealed class TextInputKeyEventArgs(ConsoleKeyInfo keyInfo, TextInputState currentState)
    : EventArgs
{
    /// <summary>
    /// The key information of the pressed key.
    /// </summary>
    public ConsoleKeyInfo KeyInfo { get; } = keyInfo;

    /// <summary>
    /// The current state of the text input.
    /// </summary>
    public TextInputState CurrentState { get; } = currentState;

    /// <summary>
    /// The proposed state of the text input after handling the key event.
    /// </summary>
    public TextInputState ProposedState { get; set; } = currentState;

    /// <summary>
    /// Indicates whether the key event has been handled. If true, the <see cref="ProposedState"/> will be used;
    /// otherwise, the default editing behavior will be applied to the proposed state.
    /// </summary>
    public bool Handled { get; set; }
}

public delegate void TextInputRenderer(TextInputState state, int anchorTop);

/// <summary>
/// A controller for reading text input from the console with basic editing capabilities.
/// </summary>
/// <param name="customRenderer"></param>
/// <remarks>
/// The <see cref="TextInputController"/> provides a way to read text input from the console
/// with basic editing capabilities such as handling backspace, delete, and arrow keys.
/// It also supports custom rendering of the input state through a user-defined renderer function.
///
/// The default rendering behavior is similar to typical console input, where the text wraps when
/// it reaches the end of the console width. The caret position is updated accordingly.
///
/// The controller raises a <see cref="KeyUp"/> event for each key press, allowing subscribers
/// to customize the behavior of the text input. If the event handler sets the <see cref="TextInputKeyEventArgs.Handled"/>
/// property to true, the proposed state from the event args will be used; otherwise, the default editing behavior
/// will be applied to the proposed state.
///
/// The default editing behavior includes:
/// - Enter: Completes the input.
/// - Backspace: Deletes the character before the caret.
/// - Delete: Deletes the character at the caret.
/// - Left Arrow: Moves the caret left.
/// - Right Arrow: Moves the caret right.
/// - Printable characters: Inserts the character at the caret position.
/// </remarks>
public sealed class TextInputController(TextInputRenderer? customRenderer = null)
{
    private readonly TextInputRenderer? _customRenderer = customRenderer;

    public event EventHandler<TextInputKeyEventArgs>? KeyUp;

    public TextInputState Read()
    {
        PrepareTerminal();
        TerminalEx.ClearInputBuffer();

        TextInputRenderer renderer = _customRenderer ?? CreateDefaultRenderer(Terminal.CursorLeft);

        TextInputState state = new(string.Empty, 0, false, false);

        while (!state.Done)
        {
            ConsoleKeyInfo keyInfo = Terminal.ReadKey(true);

            TextInputKeyEventArgs args = new(keyInfo, state);
            KeyUp?.Invoke(this, args);

            TextInputState next = args.Handled
                ? args.ProposedState
                : ApplyDefaultEditing(args.ProposedState, keyInfo);

            if (
                !string.Equals(next.Text, state.Text, StringComparison.Ordinal)
                || next.CaretIndex != state.CaretIndex
            )
            {
                renderer(next, Terminal.CursorTop);
            }

            state = next;
        }

        return state;
    }

    public static TextInputRenderer CreateDefaultRenderer(int anchorLeft)
    {
        int lastRowCount = 1;

        return (state, currentAnchorTop) =>
        {
            int bufferWidth = Math.Max(1, Terminal.BufferWidth);
            int firstLineCapacity = Math.Max(0, bufferWidth - anchorLeft);

            List<string> lines = BuildWrappedLines(state.Text, firstLineCapacity, bufferWidth);

            currentAnchorTop = TerminalEx.EnsureSpaceBelowAnchor(
                anchorLeft,
                currentAnchorTop,
                lines.Count
            );

            int caretIndex = Math.Clamp(state.CaretIndex, 0, state.Text.Length);
            int caretRow = ComputeCaretRow(caretIndex, firstLineCapacity, bufferWidth);
            EnsureCaretRowAllocated(lines, caretRow);

            ClearRows(Math.Max(lastRowCount, lines.Count), anchorLeft, currentAnchorTop);
            RenderLines(lines, anchorLeft, currentAnchorTop);

            int caretCol = ComputeCaretColumn(
                caretIndex,
                firstLineCapacity,
                bufferWidth,
                anchorLeft
            );

            caretRow = Math.Clamp(caretRow, 0, Math.Max(0, lines.Count - 1));
            caretCol = Math.Clamp(caretCol, 0, Math.Max(0, bufferWidth - 1));

            Terminal.SetCursorPosition(caretCol, currentAnchorTop + caretRow);

            lastRowCount = lines.Count;
        };
    }

    private static void RenderLines(List<string> content, int anchorLeft, int currentAnchorTop)
    {
        for (int i = 0; i < content.Count; i++)
        {
            int left = i == 0 ? anchorLeft : 0;
            Terminal.SetCursorPosition(left, currentAnchorTop + i);
            Terminal.Write(content[i]);
        }
    }

    private static void ClearRows(int rowsToClear, int anchorLeft, int currentAnchorTop)
    {
        for (int i = 0; i < rowsToClear; i++)
        {
            int left = i == 0 ? anchorLeft : 0;
            TerminalEx.ClearLineFrom(left, currentAnchorTop + i);
        }
    }

    private static List<string> BuildWrappedLines(
        string text,
        int firstLineCapacity,
        int bufferWidth
    )
    {
        List<string> lines = new();

        if (text.Length == 0)
        {
            lines.Add(string.Empty);
            return lines;
        }

        int remaining = text.Length;
        int position = 0;

        int takeFirst = Math.Min(firstLineCapacity, remaining);
        if (takeFirst > 0)
        {
            lines.Add(text.Substring(position, takeFirst));
            position += takeFirst;
            remaining -= takeFirst;
        }
        else
        {
            lines.Add(string.Empty);
        }

        while (remaining > 0)
        {
            int take = Math.Min(bufferWidth, remaining);
            lines.Add(text.Substring(position, take));
            position += take;
            remaining -= take;
        }

        return lines;
    }

    private static int ComputeCaretRow(int caretIndex, int firstLineCapacity, int bufferWidth)
    {
        if (firstLineCapacity == 0)
        {
            return (caretIndex / bufferWidth) + 1;
        }

        if (caretIndex < firstLineCapacity)
        {
            return 0;
        }

        int remainingAfterFirst = caretIndex - firstLineCapacity;
        return 1 + (remainingAfterFirst / bufferWidth);
    }

    private static void EnsureCaretRowAllocated(List<string> lines, int caretRow)
    {
        while (caretRow >= lines.Count)
        {
            lines.Add(string.Empty);
        }
    }

    private static int ComputeCaretColumn(
        int caretIndex,
        int firstLineCapacity,
        int bufferWidth,
        int anchorLeft
    )
    {
        if (firstLineCapacity == 0)
        {
            return caretIndex % bufferWidth;
        }

        if (caretIndex < firstLineCapacity)
        {
            return anchorLeft + caretIndex;
        }

        int remainingAfterFirst = caretIndex - firstLineCapacity;
        return remainingAfterFirst % bufferWidth;
    }

    private static TextInputState ApplyDefaultEditing(TextInputState state, ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.Enter:
                return state with { Done = true };
            case ConsoleKey.Backspace:
                if (state.CaretIndex > 0 && state.Text.Length > 0)
                {
                    string newText = state.Text.Remove(state.CaretIndex - 1, 1);
                    return state with { Text = newText, CaretIndex = state.CaretIndex - 1 };
                }
                return state;
            case ConsoleKey.Delete:
                if (state.CaretIndex < state.Text.Length)
                {
                    string newText = state.Text.Remove(state.CaretIndex, 1);
                    return state with { Text = newText };
                }
                return state;
            case ConsoleKey.LeftArrow:
                return state.CaretIndex > 0
                    ? state with
                    {
                        CaretIndex = state.CaretIndex - 1
                    }
                    : state;
            case ConsoleKey.RightArrow:
                return state.CaretIndex < state.Text.Length
                    ? state with
                    {
                        CaretIndex = state.CaretIndex + 1
                    }
                    : state;
            default:
                if (keyInfo.KeyChar != '\0' && !char.IsControl(keyInfo.KeyChar))
                {
                    string ch = keyInfo.KeyChar.ToString();
                    string newText =
                        state.CaretIndex >= state.Text.Length
                            ? state.Text + ch
                            : state.Text.Insert(state.CaretIndex, ch);
                    return state with { Text = newText, CaretIndex = state.CaretIndex + 1 };
                }
                return state;
        }
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
}
