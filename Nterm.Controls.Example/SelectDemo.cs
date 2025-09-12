using NTerm.Core;

namespace NTerm.Controls.Example;

/// <summary>
/// Demo program to test the Select control functionality.
/// </summary>
public static class SelectDemo
{
    public static void Run()
    {
        Terminal.WriteLine("Select Control Demo");
        Terminal.WriteLine("==================");
        Terminal.WriteLine();

        // Test 1: Basic selection
        Terminal.WriteLine(
            "Test 1: Basic selection (use arrow keys, Enter to select, Escape to cancel)"
        );
        List<SelectItem<Action>> items =
        [
            new() { Text = "Item 1", Value = () => Terminal.WriteLine("Item 1 selected"), },
            new() { Text = "Item 2", Value = () => Terminal.WriteLine("Item 2 selected") },
            new() { Text = "Item 3", Value = () => Terminal.WriteLine("Item 3 selected") }
        ];

        Terminal.Write("Select an item: ");
        SelectItem<Action> selectedItem = SelectMenu.Show(items);

        if (!selectedItem.IsEmpty())
        {
            Terminal.WriteLine($"Selected: {selectedItem.Text}");
            selectedItem.Value.Invoke();
        }
        else
        {
            Terminal.WriteLine("Selection cancelled or no items available");
        }

        Terminal.WriteLine();
        Terminal.WriteLine("Press any key to continue...");
        _ = Terminal.ReadKey();
        Terminal.Clear();

        // Test 2: Empty list
        Terminal.WriteLine("Test 2: Empty list");
        List<SelectItem<Action>> emptyItems = [];
        SelectItem<Action> emptyResult = SelectMenu.Show(emptyItems);

        if (emptyResult.IsEmpty())
        {
            Terminal.WriteLine("Correctly returned empty item for empty list");
        }

        Terminal.WriteLine();
        Terminal.WriteLine("Press any key to continue...");
        _ = Terminal.ReadKey();
        Terminal.Clear();

        // Test 3: Single item
        Terminal.WriteLine("Test 3: Single item");
        List<SelectItem<Action>> singleItem =
        [
            new() { Text = "Only Option", Value = () => Terminal.WriteLine("Only option selected") }
        ];

        SelectItem<Action> singleResult = SelectMenu.Show(singleItem);

        if (!singleResult.IsEmpty())
        {
            Terminal.WriteLine($"Selected: {singleResult.Text}");
            singleResult.Value.Invoke();
        }

        Terminal.WriteLine();
        Terminal.WriteLine("Press any key to continue...");
        _ = Terminal.ReadKey();
        Terminal.Clear();

        // Test 4: Long text items
        Terminal.WriteLine("Test 4: Long text items");
        List<SelectItem<Action>> longItems =
        [
            new()
            {
                Text =
                    "This is a very long item text that might exceed the console width and should be truncated appropriately",
                Value = () => Terminal.WriteLine("Long item 1 selected")
            },
            new()
            {
                Text = "Another long item with lots of text that goes on and on and on",
                Value = () => Terminal.WriteLine("Long item 2 selected")
            },
            new() { Text = "Short", Value = () => Terminal.WriteLine("Short item selected") }
        ];

        SelectItem<Action> longResult = SelectMenu.Show(longItems);

        if (!longResult.IsEmpty())
        {
            Terminal.WriteLine($"Selected: {longResult.Text}");
            longResult.Value.Invoke();
        }

        Terminal.WriteLine();
        Terminal.WriteLine("Demo completed!");
    }
}
