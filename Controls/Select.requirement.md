# Select control requirement

This is a control in the context of a CLI. The user can select an item by pressing the enter key. The selected item is returned to the caller. It is similar to a dropdown list in a web context, but it is very limited in the initial version.

## Items

It shows a list of specified items. Each item has the following properties:

* Text: The text that is visible in the list
* A function (callback) that is triggered when the item is selected. The control does not know what the function does. It is up to the caller to define the behavior. The control does not call it either, it returns the selected item and it is up to the caller to call the function.

## Features

* Down arrow key selects the next item, if any.
* Up arrow key selects the previous item, if any.
* Enter key selects the current item.
* Escape key cancels the selection.
* Highlight the selected item in yellow color.
* The selected item should be in the position where the cursor is. That means that previous items are not visible (above).
* The first item is selected by default.
* It is not possible to type while the control is active.

## The control does NOT

* Present the list visually, it only displays the selected item on the line where the cursor is.
* Handle empty lists. If the list is empty, return a "empty" item, with empty string and a callback that does nothing.

## Desired interface

Calling the select controller should look something like this:

```
var items = [
    { Text: "Item 1", Action: () => { Console.WriteLine("Item 1 selected"); } },
    { Text: "Item 2", Action: () => { Console.WriteLine("Item 2 selected"); } },
    { Text: "Item 3", Action: () => { Console.WriteLine("Item 3 selected"); } }
];

var selectedItem = Select.Show(items); // Returns the selected item

```

## Implementation

### AI Implementation Guidelines

**CRITICAL REQUIREMENTS:**
1. **Use the existing TrueColor project** for console color management
2. **Target .NET 10.0** as specified in ConsoleApp.csproj
3. **Handle all edge cases** mentioned in the requirements
4. **Follow the exact interface** specified above

### Data Structure
```csharp
public class SelectItem
{
    public string Text { get; set; } = string.Empty;
    public Action? Action { get; set; }

    public static SelectItem Empty { get; } = new SelectItem
    {
        Text = string.Empty,
        Action = () => { }
    };
}

public static class SelectItemExtensions
{
    public static bool IsEmpty(this SelectItem item) =>
        item == SelectItem.Empty || string.IsNullOrEmpty(item.Text);
}
```

### Core Implementation Requirements

#### 1. **Input Handling**
- Use `Console.ReadKey(true)` to capture key presses without echoing
- Handle: `ConsoleKey.UpArrow`, `ConsoleKey.DownArrow`, `ConsoleKey.Enter`, `ConsoleKey.Escape`
- Block all other input while control is active
- Clear any buffered input before starting

#### 2. **Display Management**
- **Single line display**: Only show the currently selected item at cursor position
- **Yellow highlighting**: Use `TrueColor.AnsiConsole.ForegroundColor = Color.Yellow` for selected item
- **Cursor positioning**: Use `Console.SetCursorPosition()` to control display location
- **Text truncation**: Handle items longer than console width gracefully
- **Color restoration**: Restore original colors when control exits

#### 3. **State Management**
- Track current selection index (0-based)
- Handle empty lists by returning `SelectItem.Empty` (null object pattern)
- Implement circular navigation (last item → first item, first item → last item)
- Store original cursor position and restore on exit

#### 4. **Edge Case Handling**
- **Empty list**: Return `SelectItem.Empty` immediately
- **Null items**: Skip or handle gracefully
- **Console errors**: Handle color support failures gracefully
- **Window resize**: Detect and handle console size changes
- **Non-interactive console**: Detect if console supports input

### Implementation Steps

#### Step 1: Basic Structure
```csharp
public static class Select
{
    public static SelectItem Show(IEnumerable<SelectItem> items)
    {
        // Implementation here
    }
}
```

#### Step 2: Input Loop
```csharp
// Capture and handle key presses
while (true)
{
    var key = Console.ReadKey(true);
    switch (key.Key)
    {
        case ConsoleKey.UpArrow:
            // Navigate up
            break;
        case ConsoleKey.DownArrow:
            // Navigate down
            break;
        case ConsoleKey.Enter:
            // Return selected item
            break;
        case ConsoleKey.Escape:
            // Return SelectItem.Empty (cancelled)
            break;
    }
}
```

#### Step 3: Display Logic
```csharp
private static void DisplayItem(SelectItem item, bool isSelected)
{
    if (isSelected)
    {
        AnsiConsole.ForegroundColor = Color.Yellow;
        Console.Write(item.Text);
        // Restore original color
    }
    else
    {
        Console.Write(item.Text);
    }
}
```

#### Step 4: Error Handling
```csharp
// Handle color support failures
try
{
    AnsiConsole.ForegroundColor = Color.Yellow;
}
catch
{
    // Fallback to different highlighting (bold, underline, etc.)
    Console.Write("\x1b[1m"); // Bold
}
```

### Testing Requirements
- Test with empty lists
- Test with single item
- Test with many items (circular navigation)
- Test with long item texts
- Test escape key cancellation
- Test in different console environments
- Test color support and fallbacks

### Integration Notes
- The control should work seamlessly with the existing TrueColor utilities
- Should not interfere with other console operations
- Must restore console state on exit (colors, cursor position)
- Should be thread-safe for basic usage patterns

### Performance Considerations
- Minimize console writes during navigation
- Use efficient color change detection
- Avoid unnecessary cursor repositioning
- Handle input buffering efficiently
