using NTerm.Core;
using System.Diagnostics;

namespace NTerm.Controls;

/// <summary>
/// Implementation of a CLI select control that allows users to choose from a list of items.
/// When the item is selected it will be printed at the cursor position as normal text.
///
/// For more control and advanced usage, use <see cref="SelectDropdownView"/> directly.
/// </summary>
public class SelectControl<T> : ISelectControl<T>
{
    /// <summary>
    /// Shows a select control with the specified items and returns the selected item.
    /// </summary>
    /// <param name="items">The list of items to display.</param>
    /// <returns>The selected item, or SelectItem.Empty if cancelled or list is empty.</returns>
    public SelectItem<T> Show(IEnumerable<SelectItem<T>> items, int numberOfVisibleItems = 4)
    {
        // Use TerminalState to automatically manage terminal state restoration
        using TerminalState terminalState = new();

        List<SelectItem<T>> itemList = ValidateInput(items);
        if (itemList.Count == 0)
        {
            return SelectItem<T>.Empty;
        }

        PrepareTerminalForSelection();
        ClearInputBuffer();

        SelectDropdownView<T> view =
            new(terminalState.OriginalCursorLeft, terminalState.OriginalCursorTop);

        SelectItem<T> selectedItem = view.Show(itemList, numberOfVisibleItems);

        RenderFinalSelection(selectedItem);
        return selectedItem;
    }

    /// <summary>
    /// Validates input parameters and returns a valid list of items.
    /// </summary>
    /// <param name="items">The items to validate.</param>
    /// <returns>A list of valid items.</returns>
    private static List<SelectItem<T>> ValidateInput(IEnumerable<SelectItem<T>> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return [.. items.Where(item => !item.IsEmpty())];
    }

    /// <summary>
    /// Prepares the console for selection by hiding the cursor.
    /// </summary>
    private static void PrepareTerminalForSelection()
    {
        try
        {
            System.Console.CursorVisible = false;
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
        const int maxKeysToClear = 100;

        while (Terminal.KeyAvailable && clearedKeys < maxKeysToClear)
        {
            _ = Terminal.ReadKey(true);
            clearedKeys++;
        }
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
    private static void RenderFinalSelection(SelectItem<T> selectedItem)
    {
        string displayText = TruncateText(
            selectedItem.Text,
            Math.Max(0, Terminal.WindowWidth - Terminal.CursorLeft)
        );
        Terminal.Write(displayText);
    }
}
