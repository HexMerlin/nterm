using SemanticTokens.Core;

namespace Controls;

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

        // Ensure there is enough space below to render the dropdown without hiding the selected line
        int requiredRowsBelow = Math.Min(MaxVisibleItems, Math.Max(1, itemList.Count));
        int adjustedStartRow = ConsoleEx.EnsureSpaceBelow(
            consoleState.OriginalCursorLeft,
            consoleState.OriginalCursorTop,
            requiredRowsBelow
        );

        // Run the selection loop anchored at adjusted start row
        return RunSelectionLoop(
            itemList,
            consoleState,
            consoleState.OriginalCursorLeft,
            adjustedStartRow
        );
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
    private static SelectItem RunSelectionLoop(
        List<SelectItem> items,
        ConsoleState consoleState,
        int anchorColumn,
        int anchorRow
    )
    {
        int currentIndex = 0;
        bool selectionMade = false;
        SelectItem selectedItem = SelectItem.Empty;
        int displayStartColumn = anchorColumn;
        int displayStartRow = anchorRow; // may be adjusted by EnsureSpaceBelow
        int lastRenderedLineCount = 0;
        int scrollOffset = 0; // top index of the viewport rendered below the current line
        int prevWindowWidth = Console.WindowWidth;
        int prevWindowHeight = Console.WindowHeight;

        while (!selectionMade)
        {
            // Handle terminal resize: re-ensure space and re-anchor without moving the selection line
            if (Console.WindowWidth != prevWindowWidth || Console.WindowHeight != prevWindowHeight)
            {
                // Clear previously rendered area before re-anchoring
                ConsoleEx.ClearArea(displayStartColumn, displayStartRow, lastRenderedLineCount);
                int requiredRowsBelow = Math.Min(MaxVisibleItems, Math.Max(1, items.Count));
                int adjusted = ConsoleEx.EnsureSpaceBelow(
                    displayStartColumn,
                    displayStartRow,
                    requiredRowsBelow
                );
                displayStartRow = adjusted;
                lastRenderedLineCount = 0;
                prevWindowWidth = Console.WindowWidth;
                prevWindowHeight = Console.WindowHeight;
            }

            // Display dropdown anchored at the original cursor position
            var (renderedCount, newScrollOffset) = RenderDropdown(
                items,
                currentIndex,
                displayStartColumn,
                displayStartRow,
                lastRenderedLineCount,
                scrollOffset
            );
            lastRenderedLineCount = renderedCount;
            scrollOffset = newScrollOffset;

            // Handle user input
            var keyInfo = Console.ReadKey(true);
            var (result, newIndex) = HandleUserInput(items, currentIndex, keyInfo);
            currentIndex = newIndex;

            if (!result.IsEmpty())
            {
                selectedItem = result;
                selectionMade = true;
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                // On cancel, clear any rendered lines and exit cleanly
                ConsoleEx.ClearArea(displayStartColumn, displayStartRow, lastRenderedLineCount);
                // Restore cursor to original position
                ConsoleEx.SetCursor(displayStartColumn, displayStartRow);
                return SelectItem.Empty;
            }
        }

        // Clean exit: show only the final selected item, clear below, and place cursor after text
        RenderFinalSelection(
            selectedItem,
            displayStartColumn,
            displayStartRow,
            lastRenderedLineCount,
            consoleState
        );
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
        return keyInfo.Key switch
        {
            ConsoleKey.UpArrow
                => (SelectItem.Empty, (currentIndex + items.Count - 1) % items.Count),
            ConsoleKey.DownArrow => (SelectItem.Empty, (currentIndex + 1) % items.Count),
            ConsoleKey.Enter => (items[currentIndex], currentIndex),
            ConsoleKey.Escape => (SelectItem.Empty, currentIndex),
            _ => (SelectItem.Empty, currentIndex),
        };
    }

    /// <summary>
    /// Renders the current selection state.
    /// </summary>
    /// <param name="items">The list of items.</param>
    /// <param name="currentIndex">The currently selected index.</param>
    /// <param name="startColumn">The column position to start displaying.</param>
    /// <param name="startRow">The row position to display.</param>
    private static (int renderedCount, int newScrollOffset) RenderDropdown(
        List<SelectItem> items,
        int selectedIndex,
        int startColumn,
        int startRow,
        int previouslyRenderedCount,
        int currentScrollOffset
    )
    {
        int windowWidth = Console.WindowWidth;
        int windowHeight = Console.WindowHeight;

        // Clamp starting column to window bounds
        startColumn = Math.Clamp(startColumn, 0, Math.Max(0, windowWidth - 1));
        startRow = Math.Clamp(startRow, 0, Math.Max(0, windowHeight - 1));

        // Determine viewport size BELOW the current line
        int availableRowsBelow = Math.Max(0, (windowHeight - 1) - startRow);
        int rowsToRender = Math.Min(MaxVisibleItems, availableRowsBelow);
        if (rowsToRender < 0)
        {
            rowsToRender = 0;
        }

        // Adjust scroll offset to keep selection visible in viewport
        int maxOffset = Math.Max(0, items.Count - Math.Max(0, rowsToRender));
        int offset = Math.Clamp(currentScrollOffset, 0, maxOffset);
        if (selectedIndex < offset)
        {
            offset = selectedIndex;
        }
        else if (rowsToRender > 0 && selectedIndex >= offset + rowsToRender)
        {
            offset = selectedIndex - rowsToRender + 1;
        }
        offset = Math.Clamp(offset, 0, maxOffset);

        // Render selected item on the original line (always visible), underlined
        DisplayItem(items[selectedIndex], true, startColumn, startRow, underline: true);

        // Render the fixed-size viewport directly below
        int actuallyRenderedRows = 0;
        for (int i = 0; i < rowsToRender; i++)
        {
            int row = startRow + 1 + i;
            if (row < 0 || row >= windowHeight)
                continue;

            if (offset + i < items.Count)
            {
                bool isSelectedInList = (offset + i) == selectedIndex;
                DisplayItem(
                    items[offset + i],
                    isSelectedInList,
                    startColumn,
                    row,
                    underline: false
                );
            }
            else
            {
                ConsoleEx.ClearLineFrom(startColumn, row);
            }
            actuallyRenderedRows++;
        }

        int totalRenderedLines = 1 + actuallyRenderedRows; // selected line + viewport rows

        // Clear any leftover lines from previous render if shrinking
        if (previouslyRenderedCount > totalRenderedLines)
        {
            for (
                int row = startRow + totalRenderedLines;
                row < startRow + previouslyRenderedCount;
                row++
            )
            {
                if (row >= 0 && row < windowHeight)
                {
                    ConsoleEx.ClearLineFrom(startColumn, row);
                }
            }
        }

        return (totalRenderedLines, offset);
    }

    /// <summary>
    /// Displays a single item with optional highlighting.
    /// </summary>
    /// <param name="item">The item to display.</param>
    /// <param name="isSelected">Whether the item is currently selected.</param>
    /// <param name="startColumn">The column position to start displaying the item.</param>
    /// <param name="startRow">The row position to display the item.</param>
    private static void DisplayItem(
        SelectItem item,
        bool isSelected,
        int startColumn,
        int startRow,
        bool underline = false
    )
    {
        string prefix = underline
            ? string.Empty
            : isSelected
                ? "• "
                : "  ";
        // Position cursor and clear line
        ConsoleEx.SetCursor(startColumn, startRow);
        ConsoleEx.ClearLineFrom(startColumn, startRow);

        // Get truncated text for display
        int windowWidth = Console.WindowWidth;
        string rawText = (prefix ?? string.Empty) + (item.Text ?? string.Empty);
        var displayText = TruncateText(rawText, Math.Max(0, windowWidth - startColumn));

        if (isSelected)
        {
            Console.ForegroundColor = Color.Yellow;
        }
        else
        {
            Console.ForegroundColor = Color.White;
        }

        if (underline)
        {
            Console.Write("\u001b[4m");
            Console.Write(displayText);
            Console.Write("\u001b[24m");
        }
        else
        {
            Console.Write(displayText);
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

    /// <summary>
    /// Clears any previously rendered dropdown lines and shows only the selected item.
    /// Ensures cursor ends after the selected text.
    /// </summary>
    private static void RenderFinalSelection(
        SelectItem selectedItem,
        int startColumn,
        int startRow,
        int lastRenderedLineCount,
        ConsoleState consoleState
    )
    {
        // Clear selected line from startColumn and rows below that were used
        ConsoleEx.ClearArea(startColumn, startRow, lastRenderedLineCount);

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
