namespace Controls;

/// <summary>
/// A CLI select control that allows users to choose from a list of items. This class is a wrapper around the <see cref="SelectControl"/> class.
/// </summary>
public static class Select
{
    /// <summary>
    /// Gets the default select control instance.
    /// </summary>
    public static ISelectControl Control { get; } = new SelectControl();

    /// <summary>
    /// Shows a select control with the specified items and returns the selected item.
    /// </summary>
    /// <param name="items">The list of items to display.</param>
    /// <returns>The selected item, or SelectItem.Empty if cancelled or list is empty.</returns>
    public static SelectItem Show(IEnumerable<SelectItem> items)
    {
        return Control.Show(items);
    }
}
