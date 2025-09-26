using System.Diagnostics;
using System.Text.RegularExpressions;
using Nterm.Core.Buffer;

namespace Nterm.Core.Controls;

internal sealed class SelectDropdownView<T>(int anchorColumn, int anchorRow)
{
    public Color ForegroundColor { get; init; } = Color.White;
    public Color SelectedColor { get; init; } = Color.Yellow;
    public Color FilterColor { get; init; } = Color.Gray;
    public Color NoItemsColor { get; init; } = Color.Gray;

    private int AnchorColumn { get; set; } = anchorColumn;
    private int AnchorRow { get; set; } = anchorRow;
    private int LastRenderedLineCount { get; set; }
    private int ScrollOffset { get; set; }
    private int PreviousBufferHeight { get; set; } = Terminal.BufferHeight;
    private int PreviousCursorTop { get; set; } = Terminal.CursorTop;
    private bool FilterEnabled { get; set; }
    private string FilterText { get; set; } = string.Empty;

    public TextItem<T> Show(
        IReadOnlyList<TextItem<T>> items,
        int numberOfVisibleItems = 4,
        bool enableFilter = true
    )
    {
        using TerminalState state = new();

        PrepareTerminalForSelection();
        TerminalEx.ClearInputBuffer();

        FilterEnabled = enableFilter;
        FilterText = string.Empty;

        IReadOnlyList<TextItem<T>> viewItems = items;
        int currentIndex = 0;
        bool selectionMade = false;
        TextItem<T> selectedItem = TextItem.Empty<T>();

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
                TextItem<T> result,
                int newIndex,
                IReadOnlyList<TextItem<T>> newViewItems,
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
                selectedItem = TextItem.Empty<T>();
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

    private void UpdateOnResize()
    {
        int bufferHeight = Terminal.BufferHeight;

        if (bufferHeight < 2)
        {
            AnchorRow = Terminal.CursorTop;
            return;
        }

        int bufferDiff = bufferHeight - PreviousBufferHeight;
        if (bufferDiff == 0)
            return;

        int currentCursorTop = Terminal.CursorTop;
        int cursorDiff = currentCursorTop - PreviousCursorTop;

        // Adjust anchor only when the terminal actually scrolled content
        // Heuristic: if cursor moved by the same delta as the buffer height change,
        // the viewport scrolled with the resize (e.g., when cursor was pinned to bottom).
        // If cursor did not move, the content stayed fixed relative to the top, so keep anchor.
        if (Math.Abs(cursorDiff) == Math.Abs(bufferDiff))
        {
            AnchorRow = Math.Clamp(AnchorRow + cursorDiff, 0, bufferHeight - 1);
        }
        // else: ambiguous case, avoid shifting anchor to prevent jumps

        PreviousBufferHeight = bufferHeight;
        PreviousCursorTop = currentCursorTop;
    }

    /// <summary>
    /// Handles user input and returns the result, the new index, possibly updated filtered view, and whether cancel was requested.
    /// </summary>
    private (
        TextItem<T> result,
        int newIndex,
        IReadOnlyList<TextItem<T>> viewItems,
        bool cancel
    ) HandleUserInput(
        IReadOnlyList<TextItem<T>> allItems,
        IReadOnlyList<TextItem<T>> currentViewItems,
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
            IReadOnlyList<TextItem<T>> updated = ApplyFilter(allItems, FilterText);
            int nextIndex = updated.Count == 0 ? 0 : Math.Clamp(currentIndex, 0, updated.Count - 1);
            return (TextItem.Empty<T>(), nextIndex, updated, false);
        }

        if (FilterEnabled && keyInfo.Key == ConsoleKey.Escape)
        {
            if (!string.IsNullOrEmpty(FilterText))
            {
                // Clear filter first ESC
                FilterText = string.Empty;
                return (TextItem.Empty<T>(), 0, allItems, false);
            }
            // No filter to clear: treat as cancel
            return (TextItem.Empty<T>(), currentIndex, currentViewItems, true);
        }

        if (FilterEnabled && keyInfo.KeyChar != '\0' && !char.IsControl(keyInfo.KeyChar))
        {
            FilterText += keyInfo.KeyChar;
            IReadOnlyList<TextItem<T>> updated = ApplyFilter(allItems, FilterText);
            int nextIndex = updated.Count == 0 ? 0 : Math.Clamp(currentIndex, 0, updated.Count - 1);
            return (TextItem.Empty<T>(), nextIndex, updated, false);
        }

        // Navigation and selection within current filtered view
        switch (keyInfo.Key)
        {
            case ConsoleKey.UpArrow:
            {
                if (currentViewItems.Count == 0)
                    return (TextItem.Empty<T>(), 0, currentViewItems, false);
                int nextIndex =
                    (currentIndex + currentViewItems.Count - 1)
                    % Math.Max(1, currentViewItems.Count);
                return (TextItem.Empty<T>(), nextIndex, currentViewItems, false);
            }
            case ConsoleKey.DownArrow:
            {
                if (currentViewItems.Count == 0)
                    return (TextItem.Empty<T>(), 0, currentViewItems, false);
                int nextIndex = (currentIndex + 1) % Math.Max(1, currentViewItems.Count);
                return (TextItem.Empty<T>(), nextIndex, currentViewItems, false);
            }
            case ConsoleKey.Enter:
            {
                if (currentViewItems.Count == 0)
                    return (TextItem.Empty<T>(), currentIndex, currentViewItems, false);
                return (currentViewItems[currentIndex], currentIndex, currentViewItems, false);
            }
            default:
                return (TextItem.Empty<T>(), currentIndex, currentViewItems, false);
        }
    }

    private int Render(
        IReadOnlyList<TextItem<T>> items,
        int selectedIndex,
        int numberOfVisibleItems
    )
    {
        if (numberOfVisibleItems == 1)
        {
            TextBuffer singleItemText = items.Count > 0 ? items[selectedIndex].Text : FilterText;
            DisplayAnchorItem(singleItemText, FilterText, AnchorColumn, AnchorRow);
            return 1;
        }

        TextBuffer anchorText = items.Count > 0 ? items[selectedIndex].Text : FilterText;
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
        int bufferHeight = Terminal.BufferHeight;
        int availableRowsBelow = Math.Max(0, bufferHeight - 1 - AnchorRow);
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
        IReadOnlyList<TextItem<T>> items,
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

        int bufferHeight = Terminal.BufferHeight;
        int actuallyRenderedRows = 0;

        for (int i = 0; i < rowsToRender; i++)
        {
            int row = AnchorRow + 1 + i;
            if (row < 0 || row >= bufferHeight)
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
            if (row >= 0 && row < Terminal.BufferHeight)
                TerminalEx.ClearLineFrom(AnchorColumn, row);
        }
    }

    private void DisplayAnchorItem(
        TextBuffer text,
        string filterText,
        int startColumn,
        int startRow
    )
    {
        Terminal.SetCursorPosition(startColumn, startRow);
        TerminalEx.ClearLineFrom(startColumn, startRow);

        TextBuffer displayText = text.TruncateWidth(
            Math.Max(0, Terminal.BufferWidth - startColumn)
        );

        TextBuffer[] textParts = string.IsNullOrEmpty(filterText)
            ? [displayText.Clone()]
            : displayText.Split($"({Regex.Escape(filterText)})", RegexOptions.IgnoreCase);

        bool isFilterFound = false;

        Terminal.Write(Constants.Underline);
        foreach (TextBuffer part in textParts)
        {
            if (part.Equals(filterText, StringComparison.OrdinalIgnoreCase) && !isFilterFound)
            {
                part.SetStyle(new CharStyle(ForegroundColor, default));
                // Only color the first occurrence of the filter text
                isFilterFound = true;
            }
            else
            {
                part.SetStyle(
                    FilterText.Length > 0
                        ? new CharStyle(FilterColor, default)
                        : new CharStyle(SelectedColor, default)
                );
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

    private void DisplayListItem(TextItem<T> item, bool isSelected, int startColumn, int startRow)
    {
        TextBuffer text =
            new(isSelected ? "• " : "  ", isSelected ? SelectedColor : ForegroundColor);
        Terminal.SetCursorPosition(startColumn, startRow);
        TerminalEx.ClearLineFrom(startColumn, startRow);

        TextBuffer itemText = item.Text.Clone();
        if (isSelected)
            itemText.SetStyle(new CharStyle(SelectedColor, default));
        text.Append(itemText);

        if (!item.Description.IsEmpty)
        {
            TextBuffer description = new(" ", Color.Gray);
            description.Append(item.Description.Clone());
            description.SetStyle(new CharStyle(Color.Gray, default));
            text.Append(description);
        }

        int maxWidth = Math.Max(0, Terminal.BufferWidth - startColumn);
        text.TruncateWidth(maxWidth);
        Terminal.Write(text);
    }

    private static IReadOnlyList<TextItem<T>> ApplyFilter(
        IReadOnlyList<TextItem<T>> items,
        string query
    ) => string.IsNullOrWhiteSpace(query) ? items : [.. items.Where(i => IsMatch(i.Text, query))];

    private static bool IsMatch(TextBuffer source, string query) =>
        !source.IsEmpty && source.ToString().Contains(query, StringComparison.OrdinalIgnoreCase);
}
