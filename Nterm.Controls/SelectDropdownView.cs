using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NTerm.Controls;

internal sealed class SelectDropdownView<T>(int anchorColumn, int anchorRow)
{
    public Color ForegroundColor { get; init; } = Color.White;
    public Color MenuColor { get; init; } = Color.Yellow;
    public Color FilterColor { get; init; } = Color.Gray;
    public Color NoItemsColor { get; init; } = Color.Gray;

    private int AnchorColumn { get; set; } = anchorColumn;
    private int AnchorRow { get; set; } = anchorRow;
    private int LastRenderedLineCount { get; set; }
    private int ScrollOffset { get; set; }
    private int PreviousWindowHeight { get; set; } = Terminal.WindowHeight;
    private int PreviousCursorTop { get; set; } = Terminal.CursorTop;
    private bool FilterEnabled { get; set; }
    private string FilterText { get; set; } = string.Empty;

    public SelectItem<T> Show(
        IReadOnlyList<SelectItem<T>> items,
        int numberOfVisibleItems = 4,
        bool enableFilter = true
    )
    {
        using TerminalState state = new();

        PrepareTerminalForSelection();
        ClearInputBuffer();

        FilterEnabled = enableFilter;
        FilterText = string.Empty;

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

        int windowDiff = windowHeight - PreviousWindowHeight;
        if (windowDiff == 0)
            return;

        int currentCursorTop = Terminal.CursorTop;
        int cursorDiff = currentCursorTop - PreviousCursorTop;

        // Adjust anchor only when the terminal actually scrolled content
        // Heuristic: if cursor moved by the same delta as the window height change,
        // the viewport scrolled with the resize (e.g., when cursor was pinned to bottom).
        // If cursor did not move, the content stayed fixed relative to the top, so keep anchor.
        if (Math.Abs(cursorDiff) == Math.Abs(windowDiff))
        {
            AnchorRow = Math.Clamp(AnchorRow + cursorDiff, 0, windowHeight - 1);
        }
        // else: ambiguous case, avoid shifting anchor to prevent jumps

        PreviousWindowHeight = windowHeight;
        PreviousCursorTop = currentCursorTop;
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
        if (FilterEnabled && keyInfo.Key == ConsoleKey.Backspace)
        {
            if (FilterText.Length > 0)
            {
                FilterText = FilterText[..^1];
            }
            IReadOnlyList<SelectItem<T>> updated = ApplyFilter(allItems, FilterText);
            int nextIndex = updated.Count == 0 ? 0 : Math.Clamp(currentIndex, 0, updated.Count - 1);
            return (SelectItem.Empty<T>(), nextIndex, updated, false);
        }

        if (FilterEnabled && keyInfo.Key == ConsoleKey.Escape)
        {
            if (!string.IsNullOrEmpty(FilterText))
            {
                // Clear filter first ESC
                FilterText = string.Empty;
                return (SelectItem.Empty<T>(), 0, allItems, false);
            }
            // No filter to clear: treat as cancel
            return (SelectItem.Empty<T>(), currentIndex, currentViewItems, true);
        }

        if (FilterEnabled && keyInfo.KeyChar != '\0' && !char.IsControl(keyInfo.KeyChar))
        {
            FilterText += keyInfo.KeyChar;
            IReadOnlyList<SelectItem<T>> updated = ApplyFilter(allItems, FilterText);
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
        if (numberOfVisibleItems == 1)
        {
            string singleItemText = items.Count > 0 ? items[selectedIndex].Text : FilterText;
            DisplayAnchorItem(singleItemText, FilterText, AnchorColumn, AnchorRow);
            return 1;
        }

        string anchorText = items.Count > 0 ? items[selectedIndex].Text : FilterText;
        // Render selected item at anchor line (underlined to distinguish from list below)
        DisplayAnchorItem(anchorText, FilterText, AnchorColumn, AnchorRow);

        int rowsToRender = CalculateRowsToRender(Math.Min(items.Count, numberOfVisibleItems));
        ScrollOffset = CalculateScrollOffset(
            selectedIndex,
            rowsToRender,
            ScrollOffset,
            items.Count
        );

        int actuallyRenderedRows = RenderViewport(items, selectedIndex, rowsToRender, ScrollOffset);
        int totalRendered = 1 + actuallyRenderedRows; // selected + viewport rows

        ClearShrinkingTail(totalRendered);
        LastRenderedLineCount = totalRendered;

        // Stabilize the cursor position to the anchor after rendering to make
        // resize detection reliable (cursorDiff reflects only external changes).
        Terminal.SetCursorPosition(AnchorColumn, AnchorRow);
        PreviousCursorTop = Terminal.CursorTop;
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
        if (rowsToRender == 0)
        {
            DisplayNoItems();
            return 0;
        }

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

    private void DisplayAnchorItem(string text, string filterText, int startColumn, int startRow)
    {
        Terminal.SetCursorPosition(startColumn, startRow);
        TerminalEx.ClearLineFrom(startColumn, startRow);

        string displayText = TruncateText(
            text ?? string.Empty,
            Math.Max(0, Terminal.WindowWidth - startColumn)
        );

        string[] textParts = Regex.Split(
            displayText,
            $"({Regex.Escape(filterText)})",
            RegexOptions.IgnoreCase
        );

        bool isFilterFound = false;

        Terminal.Write(Constants.Underline);
        foreach (string part in textParts)
        {
            if (part.Equals(filterText, StringComparison.OrdinalIgnoreCase) && !isFilterFound)
            {
                Terminal.ForegroundColor = ForegroundColor;
                // Only color the first occurrence of the filter text
                isFilterFound = true;
            }
            else
            {
                Terminal.ForegroundColor = FilterText.Length > 0 ? FilterColor : MenuColor;
            }
            Terminal.Write(part);
        }

        Terminal.Write(Constants.UnderlineEnd);
    }

    private void DisplayNoItems()
    {
        Terminal.SetCursorPosition(AnchorColumn, AnchorRow + 1);
        TerminalEx.ClearLineFrom(AnchorColumn, AnchorRow + 1);
        Terminal.ForegroundColor = NoItemsColor;
        Terminal.Write("No items found");
    }

    private void DisplayListItem(SelectItem<T> item, bool isSelected, int startColumn, int startRow)
    {
        string prefix = isSelected ? "• " : "  ";
        Terminal.SetCursorPosition(startColumn, startRow);
        TerminalEx.ClearLineFrom(startColumn, startRow);

        string rawText = (prefix ?? string.Empty) + (item.Text ?? string.Empty);
        string displayText = TruncateText(rawText, Math.Max(0, Terminal.WindowWidth - startColumn));

        Terminal.ForegroundColor = isSelected ? MenuColor : ForegroundColor;
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
