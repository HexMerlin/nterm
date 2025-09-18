namespace Nterm.Core.Controls;

/// <summary>
/// Represents an item with a text representation.
/// </summary>
public class TextItem<TValue>
{
    public required string Text { get; init; }

    public required TValue Value { get; init; }

    public bool IsEmpty() => string.IsNullOrEmpty(Text);
}

// Non-generic factory for "empty" items
public static class TextItem
{
    public static TextItem<TValue> Empty<TValue>() =>
        new() { Text = string.Empty, Value = default! };
}
