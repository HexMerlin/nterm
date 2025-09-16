using System.Diagnostics;

namespace Nterm.Core.Controls;

public sealed class TextInputState
{
    public string Text { get; set; } = string.Empty;
    public int CaretIndex { get; set; }
    public bool Done { get; set; }
    public bool Cancelled { get; set; }
}

public delegate bool TextInputKeyHandler(ref TextInputState state, ConsoleKeyInfo keyInfo);

public sealed class TextInputController
{
    public TextInputState Read(
        TextInputKeyHandler? handleSpecialKey,
        Action<TextInputState, ConsoleKeyInfo>? onKeystroke = null
    )
    {
        PrepareTerminal();
        TerminalEx.ClearInputBuffer();

        TextInputState state =
            new()
            {
                Text = string.Empty,
                CaretIndex = 0,
                Done = false,
                Cancelled = false,
            };

        while (!state.Done)
        {
            ConsoleKeyInfo keyInfo = Terminal.ReadKey(true);

            bool handledByCaller = false;
            if (handleSpecialKey != null)
            {
                handledByCaller = handleSpecialKey(ref state, keyInfo);
            }

            if (!handledByCaller)
            {
                HandleDefaultEditing(ref state, keyInfo);
            }

            onKeystroke?.Invoke(state, keyInfo);
        }

        return state;
    }

    private static void HandleDefaultEditing(ref TextInputState state, ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.Backspace:
                if (state.CaretIndex > 0 && state.Text.Length > 0)
                {
                    state.Text = state.Text.Remove(state.CaretIndex - 1, 1);
                    state.CaretIndex--;
                }
                break;
            case ConsoleKey.Delete:
                if (state.CaretIndex < state.Text.Length)
                {
                    state.Text = state.Text.Remove(state.CaretIndex, 1);
                }
                break;
            case ConsoleKey.LeftArrow:
                if (state.CaretIndex > 0)
                    state.CaretIndex--;
                break;
            case ConsoleKey.RightArrow:
                if (state.CaretIndex < state.Text.Length)
                    state.CaretIndex++;
                break;
            default:
                if (keyInfo.KeyChar != '\0' && !char.IsControl(keyInfo.KeyChar))
                {
                    string ch = keyInfo.KeyChar.ToString();
                    if (state.CaretIndex >= state.Text.Length)
                    {
                        state.Text += ch;
                    }
                    else
                    {
                        state.Text = state.Text.Insert(state.CaretIndex, ch);
                    }
                    state.CaretIndex++;
                }
                break;
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
