namespace NTerm.Controls;

/// <summary>
/// Represents an item in a select control with text and an associated action.
/// </summary>
public class SelectItem<T>
{
    /// <summary>
    /// The text that is visible in the list.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// A function (callback) that user code can call when the item is selected and the list is closed.
    /// </summary>
    public T Value { get; init; } = default!;

    /// <summary>
    /// Gets an empty select item with no text and a no-op action.
    /// </summary>
    public static SelectItem<T> Empty { get; } = new();

    public bool IsEmpty() => string.IsNullOrEmpty(Text);
}
