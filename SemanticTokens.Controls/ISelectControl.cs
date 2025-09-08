namespace SemanticTokens.Controls;

/// <summary>
/// Interface for a select control that allows users to choose from a list of items.
/// </summary>
public interface ISelectControl
{
    /// <summary>
    /// Shows a select control with the specified items and returns the selected item.
    /// </summary>
    /// <param name="items">The list of items to display.</param>
    /// <returns>The selected item, or SelectItem.Empty if cancelled or list is empty.</returns>
    SelectItem Show(IEnumerable<SelectItem> items, int numberOfVisibleItems = 4);
}
