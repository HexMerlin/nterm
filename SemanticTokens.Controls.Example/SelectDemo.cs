namespace SemanticTokens.Controls.Example;

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
        var items = new List<SelectItem<Action>>
        {
            new() { Text = "Item 1", Value = () => Console.WriteLine("Item 1 selected"), },
            new() { Text = "Item 2", Value = () => Console.WriteLine("Item 2 selected") },
            new() { Text = "Item 3", Value = () => Console.WriteLine("Item 3 selected") }
        };

        Console.Write("Select an item: ");
        var selectedItem = Select.Show(items);

        if (!selectedItem.IsEmpty())
        {
            Console.WriteLine($"Selected: {selectedItem.Text}");
            selectedItem.Value.Invoke();
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
        var emptyItems = new List<SelectItem<Action>>();
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
        var singleItem = new List<SelectItem<Action>>
        {
            new() { Text = "Only Option", Value = () => Console.WriteLine("Only option selected") }
        };

        var singleResult = Select.Show(singleItem);

        if (!singleResult.IsEmpty())
        {
            Console.WriteLine($"Selected: {singleResult.Text}");
            singleResult.Value.Invoke();
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();

        // Test 4: Long text items
        Console.WriteLine("Test 4: Long text items");
        var longItems = new List<SelectItem<Action>>
        {
            new()
            {
                Text =
                    "This is a very long item text that might exceed the console width and should be truncated appropriately",
                Value = () => Console.WriteLine("Long item 1 selected")
            },
            new()
            {
                Text = "Another long item with lots of text that goes on and on and on",
                Value = () => Console.WriteLine("Long item 2 selected")
            },
            new() { Text = "Short", Value = () => Console.WriteLine("Short item selected") }
        };

        var longResult = Select.Show(longItems);

        if (!longResult.IsEmpty())
        {
            Console.WriteLine($"Selected: {longResult.Text}");
            longResult.Value.Invoke();
        }

        Console.WriteLine();
        Console.WriteLine("Demo completed!");
    }
}
