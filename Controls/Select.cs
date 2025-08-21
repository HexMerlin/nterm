using TrueColor;

namespace Controls;

/// <summary>
/// Represents an item in a select control with text and an associated action.
/// </summary>
public class SelectItem
{
    /// <summary>
    /// The text that is visible in the list.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The function (callback) that is triggered when the item is selected.
    /// </summary>
    public Action? Action { get; set; }

    /// <summary>
    /// Gets an empty select item with no text and a no-op action.
    /// </summary>
    public static SelectItem Empty { get; } =
        new SelectItem { Text = string.Empty, Action = () => { } };
}

/// <summary>
/// Extension methods for SelectItem.
/// </summary>
public static class SelectItemExtensions
{
    /// <summary>
    /// Determines if a select item is empty.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is empty or has no text.</returns>
    public static bool IsEmpty(this SelectItem item) =>
        item == SelectItem.Empty || string.IsNullOrEmpty(item.Text);
}

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

        // Store original console state
        var originalForegroundColor = AnsiConsole.ForegroundColor;
        var originalBackgroundColor = AnsiConsole.BackgroundColor;
        var originalCursorLeft = Console.CursorLeft;
        var originalCursorTop = Console.CursorTop;
        bool originalCursorVisible = false;
        try
        {
            originalCursorVisible = Console.CursorVisible;
        }
        catch (PlatformNotSupportedException)
        {
            // Cursor visibility not supported on this platform
        }

        try
        {
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
            int displayStartColumn = originalCursorLeft;
            int displayStartRow = originalCursorTop;

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
        }
        finally
        {
            // Reset any ANSI color codes and restore original console state
            AnsiConsole.BackgroundColor = originalBackgroundColor;
            AnsiConsole.ForegroundColor = originalForegroundColor;
            try
            {
                Console.CursorVisible = originalCursorVisible;
            }
            catch (PlatformNotSupportedException)
            {
                // Cursor visibility not supported on this platform
            }
        }
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
                Console.Write(displayText);
            }
            catch
            {
                // Fallback to bold if color fails
                Console.Write("\x1b[1m"); // Bold
                Console.Write(displayText);
                Console.Write("\x1b[0m"); // Reset
            }
        }
        else
        {
            // Display normal text
            Console.Write(displayText);
        }
    }
}
