using TrueColor;

namespace Controls;

/// <summary>
/// Implementation of a CLI select control that allows users to choose from a list of items.
/// </summary>
public class SelectControl : ISelectControl
{
    /// <summary>
    /// Shows a select control with the specified items and returns the selected item.
    /// </summary>
    /// <param name="items">The list of items to display.</param>
    /// <returns>The selected item, or SelectItem.Empty if cancelled or list is empty.</returns>
    public SelectItem Show(IEnumerable<SelectItem> items)
    {
        // Validate input and prepare items list
        var itemList = ValidateInput(items);
        if (itemList.Count == 0)
        {
            return SelectItem.Empty;
        }

        // Use ConsoleState to automatically manage console state restoration
        using var consoleState = new ConsoleState();

        // Prepare console for selection
        PrepareConsoleForSelection();

        // Clear any buffered input
        ClearInputBuffer();

        // Run the selection loop
        return RunSelectionLoop(itemList, consoleState);
    }

    /// <summary>
    /// Validates input parameters and returns a valid list of items.
    /// </summary>
    /// <param name="items">The items to validate.</param>
    /// <returns>A list of valid items.</returns>
    private static List<SelectItem> ValidateInput(IEnumerable<SelectItem> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (Console.WindowWidth <= 0)
            throw new InvalidOperationException("Console window width must be positive");

        return [.. items.Where(item => item != null)];
    }

    /// <summary>
    /// Prepares the console for selection by hiding the cursor.
    /// </summary>
    private static void PrepareConsoleForSelection()
    {
        try
        {
            Console.CursorVisible = false;
        }
        catch (PlatformNotSupportedException)
        {
            // Cursor visibility not supported on this platform
        }
    }

    /// <summary>
    /// Clears any buffered input to prevent unwanted key presses.
    /// </summary>
    private static void ClearInputBuffer()
    {
        int clearedKeys = 0;
        const int maxKeysToClear = 100;

        while (Console.KeyAvailable && clearedKeys < maxKeysToClear)
        {
            Console.ReadKey(true);
            clearedKeys++;
        }
    }

    /// <summary>
    /// Runs the main selection loop.
    /// </summary>
    /// <param name="items">The list of items to select from.</param>
    /// <param name="consoleState">The console state for position tracking.</param>
    /// <returns>The selected item.</returns>
    private static SelectItem RunSelectionLoop(List<SelectItem> items, ConsoleState consoleState)
    {
        int currentIndex = 0;
        bool selectionMade = false;
        SelectItem selectedItem = SelectItem.Empty;
        int displayStartColumn = consoleState.OriginalCursorLeft;
        int displayStartRow = consoleState.OriginalCursorTop;

        while (!selectionMade)
        {
            // Display current item at the original cursor position
            RenderSelection(items, currentIndex, displayStartColumn, displayStartRow);

            // Handle user input
            var keyInfo = Console.ReadKey(true);
            var (result, newIndex) = HandleUserInput(items, currentIndex, keyInfo);
            currentIndex = newIndex;

            if (!result.IsEmpty())
            {
                selectedItem = result;
                selectionMade = true;
            }
        }

        return selectedItem;
    }

    /// <summary>
    /// Handles user input and returns the result along with the new index.
    /// </summary>
    /// <param name="items">The list of items.</param>
    /// <param name="currentIndex">The current selected index.</param>
    /// <param name="keyInfo">The key that was pressed.</param>
    /// <returns>A tuple containing the selected item (or SelectItem.Empty) and the new index.</returns>
    private static (SelectItem result, int newIndex) HandleUserInput(
        List<SelectItem> items,
        int currentIndex,
        ConsoleKeyInfo keyInfo
    )
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.UpArrow:
                // Navigate to previous item (circular)
                var newIndexUp = currentIndex == 0 ? items.Count - 1 : currentIndex - 1;
                return (SelectItem.Empty, newIndexUp);

            case ConsoleKey.DownArrow:
                // Navigate to next item (circular)
                var newIndexDown = currentIndex == items.Count - 1 ? 0 : currentIndex + 1;
                return (SelectItem.Empty, newIndexDown);

            case ConsoleKey.Enter:
                // Select current item
                return (items[currentIndex], currentIndex);

            case ConsoleKey.Escape:
                // Cancel selection
                return (SelectItem.Empty, currentIndex);

            // Ignore all other keys
            default:
                return (SelectItem.Empty, currentIndex);
        }
    }

    /// <summary>
    /// Renders the current selection state.
    /// </summary>
    /// <param name="items">The list of items.</param>
    /// <param name="currentIndex">The currently selected index.</param>
    /// <param name="startColumn">The column position to start displaying.</param>
    /// <param name="startRow">The row position to display.</param>
    private static void RenderSelection(
        List<SelectItem> items,
        int currentIndex,
        int startColumn,
        int startRow
    )
    {
        DisplayItem(items[currentIndex], true, startColumn, startRow);
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
        int windowWidth = Console.WindowWidth;
        var clearLength = windowWidth - startColumn;
        Console.Write(new string(' ', clearLength));
        Console.SetCursorPosition(startColumn, startRow);

        // Get truncated text for display
        var displayText = TruncateText(item.Text, windowWidth - startColumn);

        if (isSelected)
        {
            // Highlight selected item in yellow
            AnsiConsole.ForegroundColor = Color.Yellow;
            AnsiConsole.Write(displayText);
        }
        else
        {
            // Display normal text using AnsiConsole to respect current colors
            AnsiConsole.Write(displayText);
        }
    }

    /// <summary>
    /// Truncates text to fit within the specified maximum width.
    /// </summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="maxWidth">The maximum width allowed.</param>
    /// <returns>The truncated text.</returns>
    private static string TruncateText(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxWidth)
            return text;

        // Account for multi-byte characters by using Substring
        return text[..Math.Min(maxWidth, text.Length)];
    }
}
