using SemanticTokens.Core;

namespace SemanticTokens.Controls;

internal sealed class SelectDropdownView(int anchorColumn, int anchorRow, int maxVisibleItems)
{
    public int AnchorColumn { get; private set; } = anchorColumn;
    public int AnchorRow { get; private set; } = anchorRow;
    private readonly int maxVisibleItems = maxVisibleItems;
    private int LastRenderedLineCount { get; set; }

    private int scrollOffset;
    private int previousWindowHeight = Console.WindowHeight;
    private int previousCursorTop = Console.CursorTop;

    public SelectItem Show(IReadOnlyList<SelectItem> items)
    {
        int currentIndex = 0;
        bool selectionMade = false;
        SelectItem selectedItem = SelectItem.Empty;

        while (!selectionMade)
        {
            UpdateOnResize();

            int requiredRowsBelow = Math.Min(maxVisibleItems, Math.Max(1, items.Count));
            AnchorRow = ConsoleEx.EnsureSpaceBelowAnchor(
                AnchorColumn,
                AnchorRow,
                requiredRowsBelow
            );
            // Display dropdown anchored at the original cursor position
            Render(items, currentIndex);

            // Handle user input
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            (SelectItem result, int newIndex) = HandleUserInput(items, currentIndex, keyInfo);
            currentIndex = newIndex;

            if (!result.IsEmpty())
            {
                selectedItem = result;
                selectionMade = true;
            }
            else if (IsCancel(keyInfo))
            {
                selectedItem = SelectItem.Empty;
                selectionMade = true;
            }
        }

        ConsoleEx.ClearArea(AnchorColumn, AnchorRow, LastRenderedLineCount);
        ConsoleEx.SetCursor(AnchorColumn, AnchorRow);

        return selectedItem;
    }

    private void UpdateOnResize()
    {
        int windowHeight = Console.WindowHeight;

        if (windowHeight < 2)
        {
            AnchorRow = Console.CursorTop;
            return;
        }

        int windowDiff = windowHeight - previousWindowHeight;
        if (windowDiff == 0)
            return;

        int currentCursorTop = Console.CursorTop;
        int cursorDiff = currentCursorTop - previousCursorTop;

        // Adjust anchor only when the terminal actually scrolled content
        // Heuristic: if cursor moved by the same delta as the window height change,
        // the viewport scrolled with the resize (e.g., when cursor was pinned to bottom).
        // If cursor did not move, the content stayed fixed relative to the top, so keep anchor.
        if (Math.Abs(cursorDiff) == Math.Abs(windowDiff))
        {
            AnchorRow = Math.Clamp(AnchorRow + cursorDiff, 0, windowHeight - 1);
        }
        // else: ambiguous case, avoid shifting anchor to prevent jumps

        previousWindowHeight = windowHeight;
        previousCursorTop = currentCursorTop;
    }

    private static bool IsCancel(ConsoleKeyInfo keyInfo) => keyInfo.Key == ConsoleKey.Escape;

    /// <summary>
    /// Handles user input and returns the result along with the new index.
    /// </summary>
    /// <param name="items">The list of items.</param>
    /// <param name="currentIndex">The current selected index.</param>
    /// <param name="keyInfo">The key that was pressed.</param>
    /// <returns>A tuple containing the selected item (or SelectItem.Empty) and the new index.</returns>
    private static (SelectItem result, int newIndex) HandleUserInput(
        IReadOnlyList<SelectItem> items,
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

    private int Render(IReadOnlyList<SelectItem> items, int selectedIndex)
    {
        int rowsToRender = CalculateRowsToRender();
        scrollOffset = CalculateScrollOffset(
            selectedIndex,
            rowsToRender,
            scrollOffset,
            items.Count
        );

        // Render selected item at anchor line (underlined to distinguish from list below)
        DisplayItem(
            items[selectedIndex],
            isSelected: true,
            AnchorColumn,
            AnchorRow,
            underline: true
        );

        int actuallyRenderedRows = RenderViewport(items, selectedIndex, rowsToRender, scrollOffset);
        int totalRendered = 1 + actuallyRenderedRows; // selected + viewport rows

        ClearShrinkingTail(totalRendered);
        LastRenderedLineCount = totalRendered;

        // Stabilize the cursor position to the anchor after rendering to make
        // resize detection reliable (cursorDiff reflects only external changes).
        ConsoleEx.SetCursor(AnchorColumn, AnchorRow);
        previousCursorTop = Console.CursorTop;
        return totalRendered;
    }

    private int CalculateRowsToRender()
    {
        int windowHeight = Console.WindowHeight;
        int availableRowsBelow = Math.Max(0, windowHeight - 1 - AnchorRow);
        int rowsToRender = Math.Min(maxVisibleItems, availableRowsBelow);
        return Math.Max(0, rowsToRender);
    }

    private static int CalculateScrollOffset(
        int selectedIndex,
        int rowsToRender,
        int currentScrollOffset,
        int itemCount
    )
    {
        int maxOffset = Math.Max(0, itemCount - Math.Max(0, rowsToRender));
        int offset = Math.Clamp(currentScrollOffset, 0, maxOffset);
        if (selectedIndex < offset)
        {
            offset = selectedIndex;
        }
        else if (rowsToRender > 0 && selectedIndex >= offset + rowsToRender)
        {
            offset = selectedIndex - rowsToRender + 1;
        }
        return Math.Clamp(offset, 0, maxOffset);
    }

    private int RenderViewport(
        IReadOnlyList<SelectItem> items,
        int selectedIndex,
        int rowsToRender,
        int offset
    )
    {
        int windowHeight = Console.WindowHeight;
        int actuallyRenderedRows = 0;

        for (int i = 0; i < rowsToRender; i++)
        {
            int row = AnchorRow + 1 + i;
            if (row < 0 || row >= windowHeight)
                continue;

            if (offset + i < items.Count)
            {
                bool isSelectedInList = (offset + i) == selectedIndex;
                DisplayItem(
                    items[offset + i],
                    isSelectedInList,
                    AnchorColumn,
                    row,
                    underline: false
                );
            }
            else
            {
                ConsoleEx.ClearLineFrom(AnchorColumn, row);
            }
            actuallyRenderedRows++;
        }

        return actuallyRenderedRows;
    }

    private void ClearShrinkingTail(int totalRenderedLines)
    {
        if (LastRenderedLineCount <= totalRenderedLines)
            return;

        for (
            int row = AnchorRow + totalRenderedLines;
            row < AnchorRow + LastRenderedLineCount;
            row++
        )
        {
            if (row >= 0 && row < Console.WindowHeight)
                ConsoleEx.ClearLineFrom(AnchorColumn, row);
        }
    }

    private static void DisplayItem(
        SelectItem item,
        bool isSelected,
        int startColumn,
        int startRow,
        bool underline
    )
    {
        string prefix = underline
            ? string.Empty
            : isSelected
                ? "• "
                : "  ";
        ConsoleEx.SetCursor(startColumn, startRow);
        ConsoleEx.ClearLineFrom(startColumn, startRow);

        string rawText = (prefix ?? string.Empty) + (item.Text ?? string.Empty);
        string displayText = TruncateText(rawText, Math.Max(0, Console.WindowWidth - startColumn));

        Console.ForegroundColor = isSelected ? Color.Yellow : Color.White;
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

    private static string TruncateText(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxWidth)
            return text;
        return text[..Math.Min(maxWidth, text.Length)];
    }
}
