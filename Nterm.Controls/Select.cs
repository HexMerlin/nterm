namespace NTerm.Controls;

/// <summary>
/// A CLI select control that allows users to choose from a list of items. This class is a wrapper around the <see cref="SelectControl"/> class.
/// </summary>
public static class Select
{
    /// <summary>
    /// Shows a select control with the specified items and returns the selected item.
    /// </summary>
    /// <param name="items">The list of items to display.</param>
    /// <returns>The selected item, or SelectItem.Empty if cancelled or list is empty.</returns>
    public static SelectItem<T> Show<T>(
        IEnumerable<SelectItem<T>> items,
        int numberOfVisibleItems = 4
    ) => new SelectControl<T>().Show(items, numberOfVisibleItems);
}
