using Nterm.Core.Buffer;

namespace Nterm.Core.Controls;

/// <summary>
/// Represents an item with a text representation and a description.
/// </summary>
public class TextItem<TValue>
{
    /// <summary>
    /// The text representation of the item.
    /// </summary>
    public required TextBuffer Text { get; init; }

    /// <summary>
    /// The value of the item.
    /// </summary>
    public required TValue Value { get; init; }

    /// <summary>
    /// Prefix to be displayed before the text. Can be used to display an emoji icon. It will not be filtered.
    /// </summary>
    public TextBuffer Prefix { get; init; } = string.Empty;

    /// <summary>
    /// Optional description of the item. It can be used for filtering.
    /// </summary>
    public TextBuffer Description { get; init; } = string.Empty;

    public bool IsEmpty() => Text.IsEmpty;
}

// Non-generic factory for "empty" items
public static class TextItem
{
    public static TextItem<TValue> Empty<TValue>() =>
        new() { Text = new TextBuffer(), Value = default! };
}
