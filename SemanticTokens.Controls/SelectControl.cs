using SemanticTokens.Core;

namespace SemanticTokens.Controls;

/// <summary>
/// Implementation of a CLI select control that allows users to choose from a list of items.
/// </summary>
public class SelectControl : ISelectControl
{
    private const int MaxVisibleItems = 4; // configurable later

    /// <summary>
    /// Shows a select control with the specified items and returns the selected item.
    /// </summary>
    /// <param name="items">The list of items to display.</param>
    /// <returns>The selected item, or SelectItem.Empty if cancelled or list is empty.</returns>
    public SelectItem Show(IEnumerable<SelectItem> items)
    {
        var itemList = ValidateInput(items);
        if (itemList.Count == 0)
        {
            return SelectItem.Empty;
        }

        // Use ConsoleState to automatically manage console state restoration
        using var consoleState = new ConsoleState();

        PrepareConsoleForSelection();
        ClearInputBuffer();

        // Run the selection loop using the view
        return RunSelectionLoop(itemList, consoleState);
    }

    /// <summary>
    /// Validates input parameters and returns a valid list of items.
    /// </summary>
    /// <param name="items">The items to validate.</param>
    /// <returns>A list of valid items.</returns>
    private static List<SelectItem> ValidateInput(IEnumerable<SelectItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return [.. items.Where(item => !item.IsEmpty())];
    }

    /// <summary>
    /// Prepares the console for selection by hiding the cursor.
    /// </summary>
    private static void PrepareConsoleForSelection()
    {
        try
        {
            System.Console.CursorVisible = false;
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
        // Initialize dropdown view and ensure initial space below anchor
        var view = new SelectDropdownView(
            consoleState.OriginalCursorLeft,
            consoleState.OriginalCursorTop,
            MaxVisibleItems
        );

        var selectedItem = view.Show(items);

        // Clean exit: show only the final selected item, clear below, and place cursor after text
        RenderFinalSelection(selectedItem, view.AnchorColumn, view.AnchorRow, consoleState);
        return selectedItem;
    }

    /// <summary>
    /// Renders the current selection state.
    /// </summary>
    /// <param name="items">The list of items.</param>
    /// <param name="currentIndex">The currently selected index.</param>
    /// <param name="startColumn">The column position to start displaying.</param>
    /// <param name="startRow">The row position to display.</param>

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

    /// <summary>
    /// Clears any previously rendered dropdown lines and shows only the selected item.
    /// Ensures cursor ends after the selected text.
    /// </summary>
    private static void RenderFinalSelection(
        SelectItem selectedItem,
        int startColumn,
        int startRow,
        ConsoleState consoleState
    )
    {
        // Clear selected line from startColumn and rows below that were used
        //ConsoleEx.ClearArea(startColumn, startRow, lastRenderedLineCount);

        // Restore original color for final output
        Console.ForegroundColor = consoleState.OriginalForeground;

        // Write only the selected item's text and place cursor after it
        ConsoleEx.SetCursor(startColumn, startRow);
        string displayText = TruncateText(
            selectedItem.Text,
            Math.Max(0, Console.WindowWidth - startColumn)
        );
        Console.Write(displayText);

        // Move cursor to end of the written text
        int finalColumn = Math.Min(
            Console.WindowWidth - 1,
            startColumn + (displayText?.Length ?? 0)
        );
        ConsoleEx.SetCursor(finalColumn, startRow);
    }
}
