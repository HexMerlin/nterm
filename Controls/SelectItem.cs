namespace Controls;

/// <summary>
/// Represents an item in a select control with text and an associated action.
/// </summary>
public class SelectItem
{
    /// <summary>
    /// The text that is visible in the list.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The function (callback) that is triggered when the item is selected.
    /// </summary>
    public Action? Action { get; set; }

    /// <summary>
    /// Gets an empty select item with no text and a no-op action.
    /// </summary>
    public static SelectItem Empty { get; } =
        new SelectItem { Text = string.Empty, Action = () => { } };

    public bool IsEmpty() => string.IsNullOrEmpty(Text);
}
