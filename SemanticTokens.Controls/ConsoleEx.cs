namespace SemanticTokens.Controls;

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

    public static int EnsureSpaceBelow(int startColumn, int startRow, int requiredRows)
    {
        int windowHeight = Console.WindowHeight;
        if (windowHeight < 2)
        {
            // No space below, so don't move the anchor.
            return startRow;
        }

        // Reserve up to MaxVisibleItems rows BELOW the current line for the list viewport
        int requiredBelow = Math.Min(4, Math.Max(1, requiredRows));
        int rowsBelow = Math.Max(0, windowHeight - 1 - startRow);
        int needed = Math.Max(0, requiredBelow - rowsBelow);
        if (needed == 0 || needed > windowHeight - 1)
        {
            return startRow;
        }

        // Write the minimal number of newlines to create room.
        // This may cause the terminal to scroll up if we're at the bottom.
        SetCursor(startColumn, startRow);
        Console.Write(new string('\n', Math.Min(windowHeight - 1, requiredRows)));

        int adjustedStartRow = Math.Max(0, startRow - needed);

        // Restore cursor to the anchor position
        SetCursor(startColumn, adjustedStartRow);

        return adjustedStartRow;
    }
}
