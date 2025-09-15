namespace Nterm.Core;

internal static class TerminalEx
{
    public static void ClearLineFrom(int startColumn, int row)
    {
        if (row < 0 || row >= Terminal.BufferHeight)
            return;
        Terminal.SetCursorPosition(startColumn, row);
        // Erase to the end of the line (reset line width)
        // \u001b is the escape character, [K is the erase command
        Terminal.Write("\u001b[K");
        Terminal.SetCursorPosition(startColumn, row);
    }

    public static void ClearArea(int startColumn, int startRow, int lineCount)
    {
        for (int i = 0; i < lineCount; i++)
        {
            int row = startRow + i;
            if (row >= 0 && row < Terminal.BufferHeight)
            {
                ClearLineFrom(startColumn, row);
            }
        }
    }

    /// <summary>
    /// Makes space for the required rows below the anchor and keeps the cursor at the (new) anchor.
    /// </summary>
    /// <param name="columnAnchor">The column of the anchor.</param>
    /// <param name="rowAnchor">The row of the anchor.</param>
    /// <param name="requiredRows">The number of rows to make space for.</param>
    /// <returns>The new row of the anchor after making space.</returns>
    public static int EnsureSpaceBelowAnchor(int columnAnchor, int rowAnchor, int requiredRows)
    {
        int bufferHeight = Terminal.BufferHeight;
        if (bufferHeight < 2)
        {
            // No space below, so don't move the anchor.
            return rowAnchor;
        }

        // Reserve up to MaxVisibleItems rows BELOW the current line for the list viewport
        int requiredBelow = Math.Min(4, Math.Max(1, requiredRows));
        int rowsBelow = Math.Max(0, bufferHeight - 1 - rowAnchor);
        int needed = Math.Max(0, requiredBelow - rowsBelow);
        if (needed == 0 || needed > bufferHeight - 1)
        {
            return rowAnchor;
        }

        // Write the required number of newlines (from start row) to create room.
        // This may cause the terminal to scroll up if we're at the bottom.
        Terminal.SetCursorPosition(columnAnchor, rowAnchor);
        Terminal.Write(new string('\n', Math.Min(bufferHeight - 1, requiredRows)));

        int adjustedStartRow = Math.Max(0, rowAnchor - needed);

        // Restore cursor to the anchor position
        Terminal.SetCursorPosition(columnAnchor, adjustedStartRow);

        return adjustedStartRow;
    }
}
