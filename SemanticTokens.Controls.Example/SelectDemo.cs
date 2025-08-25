using Controls;
using SemanticTokens.Core;

namespace ControlsExample;

/// <summary>
/// Demo program to test the Select control functionality.
/// </summary>
public static class SelectDemo
{
    public static void Run()
    {
        Console.WriteLine("Select Control Demo");
        Console.WriteLine("==================");
        Console.WriteLine();

        // Test 1: Basic selection
        Console.WriteLine(
            "Test 1: Basic selection (use arrow keys, Enter to select, Escape to cancel)"
        );
        var items = new List<SelectItem>
        {
            new() { Text = "Item 1", Action = () => Console.WriteLine("Item 1 selected") },
            new() { Text = "Item 2", Action = () => Console.WriteLine("Item 2 selected") },
            new() { Text = "Item 3", Action = () => Console.WriteLine("Item 3 selected") }
        };

        Console.Write("Select an item: ");
        var selectedItem = Select.Show(items);

        if (!selectedItem.IsEmpty())
        {
            Console.WriteLine($"Selected: {selectedItem.Text}");
            selectedItem.Action?.Invoke();
        }
        else
        {
            Console.WriteLine("Selection cancelled or no items available");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();

        // Test 2: Empty list
        Console.WriteLine("Test 2: Empty list");
        var emptyItems = new List<SelectItem>();
        var emptyResult = Select.Show(emptyItems);

        if (emptyResult.IsEmpty())
        {
            Console.WriteLine("Correctly returned empty item for empty list");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();

        // Test 3: Single item
        Console.WriteLine("Test 3: Single item");
        var singleItem = new List<SelectItem>
        {
            new() { Text = "Only Option", Action = () => Console.WriteLine("Only option selected") }
        };

        var singleResult = Select.Show(singleItem);

        if (!singleResult.IsEmpty())
        {
            Console.WriteLine($"Selected: {singleResult.Text}");
            singleResult.Action?.Invoke();
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();

        // Test 4: Long text items
        Console.WriteLine("Test 4: Long text items");
        var longItems = new List<SelectItem>
        {
            new()
            {
                Text =
                    "This is a very long item text that might exceed the console width and should be truncated appropriately",
                Action = () => Console.WriteLine("Long item 1 selected")
            },
            new()
            {
                Text = "Another long item with lots of text that goes on and on and on",
                Action = () => Console.WriteLine("Long item 2 selected")
            },
            new() { Text = "Short", Action = () => Console.WriteLine("Short item selected") }
        };

        var longResult = Select.Show(longItems);

        if (!longResult.IsEmpty())
        {
            Console.WriteLine($"Selected: {longResult.Text}");
            longResult.Action?.Invoke();
        }

        Console.WriteLine();
        Console.WriteLine("Demo completed!");
    }
}
