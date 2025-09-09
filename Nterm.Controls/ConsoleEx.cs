namespace NTerm.Controls;

internal static class ConsoleEx
{
    public static void SetCursor(int left, int top)
    {
        int clampedLeft = Math.Clamp(left, 0, Math.Max(0, Console.WindowWidth - 1));
        int clampedTop = Math.Clamp(top, 0, Math.Max(0, Console.WindowHeight - 1));
        try
        {
            Console.SetCursorPosition(clampedLeft, clampedTop);
        }
        catch
        {
            // Ignore if setting cursor position is not supported
        }
    }

    public static void ClearLineFrom(int startColumn, int row)
    {
        if (row < 0 || row >= Console.WindowHeight)
            return;
        SetCursor(startColumn, row);
        // Erase to the end of the line (reset line width)
        // \u001b is the escape character, [K is the erase command
        Console.Write("\u001b[K");
        SetCursor(startColumn, row);
    }

    public static void ClearArea(int startColumn, int startRow, int lineCount)
    {
        for (int i = 0; i < lineCount; i++)
        {
            int row = startRow + i;
            if (row >= 0 && row < Console.WindowHeight)
            {
                ClearLineFrom(startColumn, row);
            }
        }
    }

    /// <summary>
    /// Makes space for the required rows below the anchor and sets the cursor to it.
    /// </summary>
    /// <param name="columnAnchor">The column of the anchor.</param>
    /// <param name="rowAnchor">The row of the anchor.</param>
    /// <param name="requiredRows">The number of rows to make space for.</param>
    /// <returns>The new row of the anchor after making space.</returns>
    public static int EnsureSpaceBelowAnchor(int columnAnchor, int rowAnchor, int requiredRows)
    {
        int windowHeight = Console.WindowHeight;
        if (windowHeight < 2)
        {
            // No space below, so don't move the anchor.
            return rowAnchor;
        }

        // Reserve up to MaxVisibleItems rows BELOW the current line for the list viewport
        int requiredBelow = Math.Min(4, Math.Max(1, requiredRows));
        int rowsBelow = Math.Max(0, windowHeight - 1 - rowAnchor);
        int needed = Math.Max(0, requiredBelow - rowsBelow);
        if (needed == 0 || needed > windowHeight - 1)
        {
            return rowAnchor;
        }

        // Write the required number of newlines (from start row) to create room.
        // This may cause the terminal to scroll up if we're at the bottom.
        SetCursor(columnAnchor, rowAnchor);
        Console.Write(new string('\n', Math.Min(windowHeight - 1, requiredRows)));

        int adjustedStartRow = Math.Max(0, rowAnchor - needed);

        // Restore cursor to the anchor position
        SetCursor(columnAnchor, adjustedStartRow);

        return adjustedStartRow;
    }
}
