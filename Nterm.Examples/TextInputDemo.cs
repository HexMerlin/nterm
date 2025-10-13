using Nterm.Core.Controls;

namespace Nterm.Examples;

public class TextInputDemo
{
    public static void Run()
    {
        Terminal.WriteLine("======TextInputDemo========");
        Terminal.WriteLine();
        Terminal.Write("Type something: ");
        int anchorLeft = Terminal.CursorLeft;
        int anchorTop = Terminal.CursorTop;
        int lastRowCount = 1;

        TextInputController textInput =
            new(state =>
            {
                int bufferWidth = Math.Max(1, Terminal.BufferWidth);

                // Compute wrapped lines: first line starts at anchorLeft with reduced capacity
                int firstLineCapacity = Math.Max(0, bufferWidth - anchorLeft);

                // Build the visual lines from the text based on capacities
                List<string> lines = new();
                if (state.Text.Length == 0)
                {
                    lines.Add(string.Empty);
                }
                else
                {
                    int remaining = state.Text.Length;
                    int pos = 0;

                    int takeFirst = Math.Min(firstLineCapacity, remaining);
                    if (takeFirst > 0)
                    {
                        lines.Add(state.Text.Substring(pos, takeFirst));
                        pos += takeFirst;
                        remaining -= takeFirst;
                    }
                    else
                    {
                        // No space on first line; begin on next line
                        lines.Add(string.Empty);
                    }

                    while (remaining > 0)
                    {
                        int take = Math.Min(bufferWidth, remaining);
                        lines.Add(state.Text.Substring(pos, take));
                        pos += take;
                        remaining -= take;
                    }
                }

                // Ensure there is enough space below the anchor to draw 'lines' rows
                anchorTop = TerminalEx.EnsureSpaceBelowAnchor(anchorLeft, anchorTop, lines.Count);

                // If caret is exactly at a wrap boundary, ensure we allocate an extra visual row
                int caretIndex = Math.Clamp(state.CaretIndex, 0, state.Text.Length);
                int provisionalCaretRow;
                if (firstLineCapacity == 0)
                {
                    provisionalCaretRow = (caretIndex / bufferWidth) + 1;
                }
                else if (caretIndex < firstLineCapacity)
                {
                    provisionalCaretRow = 0;
                }
                else
                {
                    int rem = caretIndex - firstLineCapacity;
                    provisionalCaretRow = 1 + (rem / bufferWidth);
                }

                while (provisionalCaretRow >= lines.Count)
                {
                    lines.Add(string.Empty);
                }

                // Clear all lines previously used (or currently needed), taking into account first line offset
                int rowsToClear = Math.Max(lastRowCount, lines.Count);
                for (int i = 0; i < rowsToClear; i++)
                {
                    int left = i == 0 ? anchorLeft : 0;
                    TerminalEx.ClearLineFrom(left, anchorTop + i);
                }

                // Render the lines at the proper positions
                for (int i = 0; i < lines.Count; i++)
                {
                    int left = i == 0 ? anchorLeft : 0;
                    Terminal.SetCursorPosition(left, anchorTop + i);
                    Terminal.Write(lines[i]);
                }

                // Compute caret row/column from CaretIndex across wrapped lines
                int caretRow;
                int caretCol;
                if (firstLineCapacity == 0)
                {
                    // All text starts on the next visual line
                    caretRow = (caretIndex / bufferWidth) + 1;
                    caretCol = caretIndex % bufferWidth;
                }
                else
                {
                    if (caretIndex < firstLineCapacity)
                    {
                        caretRow = 0;
                        caretCol = anchorLeft + caretIndex;
                    }
                    else
                    {
                        int remainingAfterFirst = caretIndex - firstLineCapacity;
                        caretRow = 1 + (remainingAfterFirst / bufferWidth);
                        caretCol = remainingAfterFirst % bufferWidth;
                    }
                }

                // Clamp caret within visible bounds of our drawn area
                caretRow = Math.Clamp(caretRow, 0, Math.Max(0, lines.Count - 1));
                caretCol = Math.Max(0, Math.Min(caretCol, bufferWidth - 1));

                Terminal.SetCursorPosition(caretCol, anchorTop + caretRow);

                lastRowCount = lines.Count;
            });
        TextInputState state = textInput.Read();

        Terminal.WriteLine();
        Terminal.WriteLine($"Typed text: {state.Text}");
    }
}
