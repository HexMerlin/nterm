using System.Diagnostics;

namespace NTerm.Controls;

internal sealed class SelectDropdownView<T>(int anchorColumn, int anchorRow)
{
    public int AnchorColumn { get; private set; } = anchorColumn;
    public int AnchorRow { get; private set; } = anchorRow;
    private int LastRenderedLineCount { get; set; }

    private int scrollOffset;
    private int previousWindowHeight = Terminal.WindowHeight;
    private int previousCursorTop = Terminal.CursorTop;

    // Filtering state
    private bool filterEnabled;
    private string filterText = string.Empty;

    public SelectItem<T> Show(
        IReadOnlyList<SelectItem<T>> items,
        int numberOfVisibleItems = 4,
        bool enableFilter = true
    )
    {
        using TerminalState state = new();

        PrepareTerminalForSelection();
        ClearInputBuffer();

        filterEnabled = enableFilter;
        filterText = string.Empty;

        IReadOnlyList<SelectItem<T>> viewItems = items;
        int currentIndex = 0;
        bool selectionMade = false;
        SelectItem<T> selectedItem = SelectItem.Empty<T>();

        while (!selectionMade)
        {
            UpdateOnResize();

            int requiredRowsBelow = Math.Min(numberOfVisibleItems, Math.Max(1, viewItems.Count));
            AnchorRow = TerminalEx.EnsureSpaceBelowAnchor(
                AnchorColumn,
                AnchorRow,
                requiredRowsBelow
            );
            // Display dropdown anchored at the original cursor position
            _ = Render(viewItems, currentIndex, numberOfVisibleItems);

            // Handle user input
            ConsoleKeyInfo keyInfo = Terminal.ReadKey(true);
            (
                SelectItem<T> result,
                int newIndex,
                IReadOnlyList<SelectItem<T>> newViewItems,
                bool cancel
            ) = HandleUserInput(items, viewItems, currentIndex, keyInfo);
            viewItems = newViewItems;
            currentIndex = newIndex;

            if (!result.IsEmpty())
            {
                selectedItem = result;
                selectionMade = true;
            }
            else if (cancel)
            {
                selectedItem = SelectItem.Empty<T>();
                selectionMade = true;
            }
        }

        TerminalEx.ClearArea(AnchorColumn, AnchorRow, LastRenderedLineCount);
        Terminal.SetCursorPosition(AnchorColumn, AnchorRow);

        return selectedItem;
    }

    /// <summary>
    /// Prepares the terminal for selection by hiding the cursor.
    /// </summary>
    private static void PrepareTerminalForSelection()
    {
        try
        {
            Terminal.CursorVisible = false;
        }
        catch (PlatformNotSupportedException)
        {
            // Cursor visibility not supported on this platform
            Debug.WriteLine("Cursor visibility manipulation not supported on this platform.");
        }
    }

    /// <summary>
    /// Clears any buffered input to prevent unwanted key presses.
    /// </summary>
    private static void ClearInputBuffer()
    {
        int clearedKeys = 0;
        const int maxKeysToClear = 1000;

        while (Terminal.KeyAvailable && clearedKeys < maxKeysToClear)
        {
            _ = Terminal.ReadKey(true);
            clearedKeys++;
        }
    }

    private void UpdateOnResize()
    {
        int windowHeight = Terminal.WindowHeight;

        if (windowHeight < 2)
        {
            AnchorRow = Terminal.CursorTop;
            return;
        }

        int windowDiff = windowHeight - previousWindowHeight;
        if (windowDiff == 0)
            return;

        int currentCursorTop = Terminal.CursorTop;
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

    /// <summary>
    /// Handles user input and returns the result, the new index, possibly updated filtered view, and whether cancel was requested.
    /// </summary>
    private (
        SelectItem<T> result,
        int newIndex,
        IReadOnlyList<SelectItem<T>> viewItems,
        bool cancel
    ) HandleUserInput(
        IReadOnlyList<SelectItem<T>> allItems,
        IReadOnlyList<SelectItem<T>> currentViewItems,
        int currentIndex,
        ConsoleKeyInfo keyInfo
    )
    {
        // Typing and editing the filter
        if (filterEnabled && keyInfo.Key == ConsoleKey.Backspace)
        {
            if (filterText.Length > 0)
            {
                filterText = filterText[..^1];
            }
            IReadOnlyList<SelectItem<T>> updated = ApplyFilter(allItems, filterText);
            int nextIndex = updated.Count == 0 ? 0 : Math.Clamp(currentIndex, 0, updated.Count - 1);
            return (SelectItem.Empty<T>(), nextIndex, updated, false);
        }

        if (filterEnabled && keyInfo.Key == ConsoleKey.Escape)
        {
            if (!string.IsNullOrEmpty(filterText))
            {
                // Clear filter first ESC
                filterText = string.Empty;
                return (SelectItem.Empty<T>(), 0, allItems, false);
            }
            // No filter to clear: treat as cancel
            return (SelectItem.Empty<T>(), currentIndex, currentViewItems, true);
        }

        if (filterEnabled && keyInfo.KeyChar != '\0' && !char.IsControl(keyInfo.KeyChar))
        {
            filterText += keyInfo.KeyChar;
            IReadOnlyList<SelectItem<T>> updated = ApplyFilter(allItems, filterText);
            int nextIndex = updated.Count == 0 ? 0 : Math.Clamp(currentIndex, 0, updated.Count - 1);
            return (SelectItem.Empty<T>(), nextIndex, updated, false);
        }

        // Navigation and selection within current filtered view
        switch (keyInfo.Key)
        {
            case ConsoleKey.UpArrow:
            {
                if (currentViewItems.Count == 0)
                    return (SelectItem.Empty<T>(), 0, currentViewItems, false);
                int nextIndex =
                    (currentIndex + currentViewItems.Count - 1)
                    % Math.Max(1, currentViewItems.Count);
                return (SelectItem.Empty<T>(), nextIndex, currentViewItems, false);
            }
            case ConsoleKey.DownArrow:
            {
                if (currentViewItems.Count == 0)
                    return (SelectItem.Empty<T>(), 0, currentViewItems, false);
                int nextIndex = (currentIndex + 1) % Math.Max(1, currentViewItems.Count);
                return (SelectItem.Empty<T>(), nextIndex, currentViewItems, false);
            }
            case ConsoleKey.Enter:
            {
                if (currentViewItems.Count == 0)
                    return (SelectItem.Empty<T>(), currentIndex, currentViewItems, false);
                return (currentViewItems[currentIndex], currentIndex, currentViewItems, false);
            }
            default:
                return (SelectItem.Empty<T>(), currentIndex, currentViewItems, false);
        }
    }

    private int Render(
        IReadOnlyList<SelectItem<T>> items,
        int selectedIndex,
        int numberOfVisibleItems
    )
    {
        int rowsToRender = CalculateRowsToRender(numberOfVisibleItems);
        scrollOffset = CalculateScrollOffset(
            selectedIndex,
            rowsToRender,
            scrollOffset,
            items.Count
        );

        // Render anchor line. If filtering is enabled and there is text, show the typed characters.
        if (filterEnabled && !string.IsNullOrEmpty(filterText))
        {
            DisplayAnchorItem(filterText, AnchorColumn, AnchorRow);
        }
        else
        {
            // Render selected item at anchor line (underlined to distinguish from list below)
            if (items.Count > 0)
            {
                DisplayAnchorItem(items[selectedIndex].Text, AnchorColumn, AnchorRow);
            }
            else
            {
                Terminal.SetCursorPosition(AnchorColumn, AnchorRow);
                TerminalEx.ClearLineFrom(AnchorColumn, AnchorRow);
            }
        }

        int actuallyRenderedRows = RenderViewport(items, selectedIndex, rowsToRender, scrollOffset);
        int totalRendered = 1 + actuallyRenderedRows; // selected + viewport rows

        ClearShrinkingTail(totalRendered);
        LastRenderedLineCount = totalRendered;

        // Stabilize the cursor position to the anchor after rendering to make
        // resize detection reliable (cursorDiff reflects only external changes).
        Terminal.SetCursorPosition(AnchorColumn, AnchorRow);
        previousCursorTop = Terminal.CursorTop;
        return totalRendered;
    }

    private int CalculateRowsToRender(int numberOfVisibleItems)
    {
        int windowHeight = Terminal.WindowHeight;
        int availableRowsBelow = Math.Max(0, windowHeight - 1 - AnchorRow);
        int rowsToRender = Math.Min(numberOfVisibleItems, availableRowsBelow);
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
        IReadOnlyList<SelectItem<T>> items,
        int selectedIndex,
        int rowsToRender,
        int offset
    )
    {
        if (rowsToRender <= 1)
            return 0;

        int windowHeight = Terminal.WindowHeight;
        int actuallyRenderedRows = 0;

        for (int i = 0; i < rowsToRender; i++)
        {
            int row = AnchorRow + 1 + i;
            if (row < 0 || row >= windowHeight)
                continue;

            if (offset + i < items.Count)
            {
                bool isSelectedInList = (offset + i) == selectedIndex;
                DisplayListItem(items[offset + i], isSelectedInList, AnchorColumn, row);
            }
            else
            {
                TerminalEx.ClearLineFrom(AnchorColumn, row);
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
            if (row >= 0 && row < Terminal.WindowHeight)
                TerminalEx.ClearLineFrom(AnchorColumn, row);
        }
    }

    private static void DisplayAnchorItem(string text, int startColumn, int startRow)
    {
        Terminal.SetCursorPosition(startColumn, startRow);
        TerminalEx.ClearLineFrom(startColumn, startRow);

        string displayText = TruncateText(
            text ?? string.Empty,
            Math.Max(0, Terminal.WindowWidth - startColumn)
        );

        Terminal.ForegroundColor = Color.Yellow;
        Terminal.Write("\u001b[4m");
        Terminal.Write(displayText);
        Terminal.Write("\u001b[24m");
    }

    private static void DisplayListItem(
        SelectItem<T> item,
        bool isSelected,
        int startColumn,
        int startRow
    )
    {
        string prefix = isSelected ? "• " : "  ";
        Terminal.SetCursorPosition(startColumn, startRow);
        TerminalEx.ClearLineFrom(startColumn, startRow);

        string rawText = (prefix ?? string.Empty) + (item.Text ?? string.Empty);
        string displayText = TruncateText(rawText, Math.Max(0, Terminal.WindowWidth - startColumn));

        Terminal.ForegroundColor = isSelected ? Color.Yellow : Color.White;
        Terminal.Write(displayText);
    }

    private static string TruncateText(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxWidth)
            return text;
        return text[..Math.Min(maxWidth, text.Length)];
    }

    private static IReadOnlyList<SelectItem<T>> ApplyFilter(
        IReadOnlyList<SelectItem<T>> items,
        string query
    )
    {
        if (string.IsNullOrWhiteSpace(query))
            return items;

        return [.. items.Where(i => IsMatch(i.Text, query))];
    }

    private static bool IsMatch(string? source, string query)
    {
        if (string.IsNullOrEmpty(source))
            return false;
        return source.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
