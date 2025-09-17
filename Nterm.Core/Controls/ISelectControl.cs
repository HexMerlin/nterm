namespace Nterm.Core.Controls;

/// <summary>
/// Interface for a select control that allows users to choose from a list of items.
/// </summary>
public interface ISelectControl<T>
{
    /// <summary>
    /// Shows a select control with the specified items and returns the selected item.
    /// </summary>
    /// <param name="items">The list of items to display.</param>
    /// <param name="numberOfVisibleItems">Maximum number of items to render below the anchor.</param>
    /// <param name="enableFilter">Whether to enable interactive typing filter (default: true).</param>
    /// <returns>The selected item, or SelectItem.Empty if cancelled or list is empty.</returns>
    TextItem<T> Show(IEnumerable<TextItem<T>> items, int numberOfVisibleItems = 4);
}
