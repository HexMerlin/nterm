namespace Nterm.Core.Controls;

/// <summary>
/// Represents an item with a text representation and a description.
/// </summary>
public class TextItem<TValue>
{
    /// <summary>
    /// The text representation of the item.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// The value of the item.
    /// </summary>
    public required TValue Value { get; init; }

    /// <summary>
    /// Optional description of the item.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    public bool IsEmpty() => string.IsNullOrEmpty(Text);
}

// Non-generic factory for "empty" items
public static class TextItem
{
    public static TextItem<TValue> Empty<TValue>() =>
        new() { Text = string.Empty, Value = default! };
}
