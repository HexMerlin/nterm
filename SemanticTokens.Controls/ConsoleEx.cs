using System;

namespace Controls;

internal static class ConsoleEx
{
    public static int WindowWidth => Console.WindowWidth;
    public static int WindowHeight => Console.WindowHeight;

    public static void SetCursor(int left, int top)
    {
        int clampedLeft = Math.Clamp(left, 0, Math.Max(0, WindowWidth - 1));
        int clampedTop = Math.Clamp(top, 0, Math.Max(0, WindowHeight - 1));
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
        if (row < 0 || row >= WindowHeight)
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
            if (row >= 0 && row < WindowHeight)
            {
                ClearLineFrom(startColumn, row);
            }
        }
    }

    public static int EnsureSpaceBelow(int startColumn, int startRow, int requiredRows)
    {
        int windowHeight = WindowHeight;
        int rowsBelow = Math.Max(0, (windowHeight - 1) - startRow);
        int needed = Math.Max(0, requiredRows - rowsBelow);
        if (needed == 0)
        {
            return startRow;
        }

        SetCursor(startColumn, startRow);
        for (int i = 0; i < needed; i++)
        {
            Console.WriteLine("");
        }

        int scrolled = Math.Max(0, (startRow + needed) - (windowHeight - 1));
        int adjustedStartRow = Math.Max(0, startRow - scrolled);
        SetCursor(startColumn, adjustedStartRow);
        return adjustedStartRow;
    }
}
