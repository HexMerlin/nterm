using System.Diagnostics;

namespace Nterm.Core.Controls;

public readonly record struct TextInputState(
    string Text,
    int CaretIndex,
    bool Done,
    bool Cancelled
);

public sealed class TextInputKeyEventArgs : EventArgs
{
    public ConsoleKeyInfo KeyInfo { get; }
    public TextInputState CurrentState { get; }
    public TextInputState ProposedState { get; set; }
    public bool Handled { get; set; }

    public TextInputKeyEventArgs(ConsoleKeyInfo keyInfo, TextInputState currentState)
    {
        KeyInfo = keyInfo;
        CurrentState = currentState;
        ProposedState = currentState;
        Handled = false;
    }
}

public sealed class TextInputController(Action<TextInputState> render)
{
    public event EventHandler<TextInputKeyEventArgs>? KeyUp;

    public TextInputState Read()
    {
        PrepareTerminal();
        TerminalEx.ClearInputBuffer();

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
                render(next);
            }

            state = next;
        }

        return state;
    }

    private static TextInputState ApplyDefaultEditing(TextInputState state, ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
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
