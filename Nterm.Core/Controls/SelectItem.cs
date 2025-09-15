namespace Nterm.Core.Controls;

/// <summary>
/// Represents an item in a select control with text and an associated action.
/// </summary>
public class SelectItem<T>
{
    public required string Text { get; init; }

    public required T Value { get; init; }

    public bool IsEmpty() => string.IsNullOrEmpty(Text);
}

// Non-generic factory for "empty" items
public static class SelectItem
{
    public static SelectItem<T> Empty<T>() => new() { Text = string.Empty, Value = default! };
}
