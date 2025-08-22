using TrueColor;

namespace Controls;

/// <summary>
/// A CLI select control that allows users to choose from a list of items.
/// </summary>
public static class Select
{
    /// <summary>
    /// Shows a select control with the specified items and returns the selected item.
    /// </summary>
    /// <param name="items">The list of items to display.</param>
    /// <returns>The selected item, or SelectItem.Empty if cancelled or list is empty.</returns>
    public static SelectItem Show(IEnumerable<SelectItem> items)
    {
        // Handle null or empty items
        var itemList = items?.Where(item => item != null).ToList() ?? new List<SelectItem>();

        if (itemList.Count == 0)
        {
            return SelectItem.Empty;
        }

        // Use ConsoleState to automatically manage console state restoration
        using var consoleState = new ConsoleState();

        // Hide cursor during selection
        try
        {
            Console.CursorVisible = false;
        }
        catch (PlatformNotSupportedException)
        {
            // Cursor visibility not supported on this platform
        }

        // Clear any buffered input
        while (Console.KeyAvailable)
        {
            Console.ReadKey(true);
        }

        int currentIndex = 0;
        bool selectionMade = false;
        SelectItem selectedItem = SelectItem.Empty;
        int displayStartColumn = consoleState.OriginalCursorLeft;
        int displayStartRow = consoleState.OriginalCursorTop;

        while (!selectionMade)
        {
            // Display current item at the original cursor position
            DisplayItem(itemList[currentIndex], true, displayStartColumn, displayStartRow);

            // Wait for key input
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    // Navigate to previous item (circular)
                    currentIndex = currentIndex == 0 ? itemList.Count - 1 : currentIndex - 1;
                    break;

                case ConsoleKey.DownArrow:
                    // Navigate to next item (circular)
                    currentIndex = currentIndex == itemList.Count - 1 ? 0 : currentIndex + 1;
                    break;

                case ConsoleKey.Enter:
                    // Select current item
                    selectedItem = itemList[currentIndex];
                    selectionMade = true;
                    break;

                case ConsoleKey.Escape:
                    // Cancel selection
                    selectedItem = SelectItem.Empty;
                    selectionMade = true;
                    break;

                // Ignore all other keys
                default:
                    break;
            }
        }

        return selectedItem;
        // ConsoleState.Dispose() will be called automatically when exiting the using scope
    }

    /// <summary>
    /// Displays a single item with optional highlighting.
    /// </summary>
    /// <param name="item">The item to display.</param>
    /// <param name="isSelected">Whether the item is currently selected.</param>
    /// <param name="startColumn">The column position to start displaying the item.</param>
    /// <param name="startRow">The row position to display the item.</param>
    private static void DisplayItem(SelectItem item, bool isSelected, int startColumn, int startRow)
    {
        // Position cursor at the specified location
        Console.SetCursorPosition(startColumn, startRow);

        // Clear from current position to end of line
        var clearLength = Console.WindowWidth - startColumn;
        Console.Write(new string(' ', clearLength));
        Console.SetCursorPosition(startColumn, startRow);

        // Truncate text if it's longer than available space
        var displayText = item.Text;
        var maxWidth = Console.WindowWidth - startColumn; // Available space from cursor position
        if (displayText.Length > maxWidth)
        {
            displayText = displayText[..maxWidth];
        }

        if (isSelected)
        {
            // Highlight selected item in yellow
            try
            {
                AnsiConsole.ForegroundColor = Color.Yellow;
                AnsiConsole.Write(displayText);
            }
            catch
            {
                // Fallback to bold if color fails
                AnsiConsole.Write("\x1b[1m"); // Bold
                AnsiConsole.Write(displayText);
                AnsiConsole.Write("\x1b[0m"); // Reset
            }
        }
        else
        {
            // Display normal text using AnsiConsole to respect current colors
            AnsiConsole.Write(displayText);
        }
    }
}
