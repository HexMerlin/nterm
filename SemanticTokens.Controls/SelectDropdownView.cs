using SemanticTokens.Core;

namespace SemanticTokens.Controls;

internal sealed class SelectDropdownView(int anchorColumn, int anchorRow, int maxVisibleItems)
{
    public int AnchorColumn { get; private set; } = anchorColumn;
    public int AnchorRow { get; private set; } = anchorRow;
    private readonly int maxVisibleItems = maxVisibleItems;
    public int LastRenderedLineCount { get; private set; }

    private int scrollOffset;
    private int previousWindowWidth = ConsoleEx.WindowWidth;
    private int previousWindowHeight = ConsoleEx.WindowHeight;

    public void EnsureInitialSpace(int requiredRowsBelow)
    {
        AnchorRow = ConsoleEx.EnsureSpaceBelow(AnchorColumn, AnchorRow, requiredRowsBelow);
    }

    public void UpdateOnResize(int itemCount)
    {
        if (!HasWindowResized())
            return;

        ConsoleEx.ClearArea(AnchorColumn, AnchorRow, LastRenderedLineCount);
        int requiredRowsBelow = Math.Min(maxVisibleItems, Math.Max(1, itemCount));
        AnchorRow = ConsoleEx.EnsureSpaceBelow(AnchorColumn, AnchorRow, requiredRowsBelow);
        LastRenderedLineCount = 0;
        previousWindowWidth = ConsoleEx.WindowWidth;
        previousWindowHeight = ConsoleEx.WindowHeight;
    }

    public int Render(IReadOnlyList<SelectItem> items, int selectedIndex)
    {
        ClampStartPosition();

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
        return totalRendered;
    }

    private void ClampStartPosition()
    {
        int windowWidth = ConsoleEx.WindowWidth;
        int windowHeight = ConsoleEx.WindowHeight;
        AnchorColumn = Math.Clamp(AnchorColumn, 0, Math.Max(0, windowWidth - 1));
        AnchorRow = Math.Clamp(AnchorRow, 0, Math.Max(0, windowHeight - 1));
    }

    private int CalculateRowsToRender()
    {
        int windowHeight = ConsoleEx.WindowHeight;
        int availableRowsBelow = Math.Max(0, (windowHeight - 1) - AnchorRow);
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
        int windowHeight = ConsoleEx.WindowHeight;
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

        int windowHeight = ConsoleEx.WindowHeight;
        for (
            int row = AnchorRow + totalRenderedLines;
            row < AnchorRow + LastRenderedLineCount;
            row++
        )
        {
            if (row >= 0 && row < windowHeight)
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

        int windowWidth = ConsoleEx.WindowWidth;
        string rawText = (prefix ?? string.Empty) + (item.Text ?? string.Empty);
        string displayText = TruncateText(rawText, Math.Max(0, windowWidth - startColumn));

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

    private bool HasWindowResized() =>
        ConsoleEx.WindowWidth != previousWindowWidth
        || ConsoleEx.WindowHeight != previousWindowHeight;
}
