using System.Text;
using SemanticTokens.Core;

namespace SemanticTokens.Controls.Example;

class Program
{
    static void Main(string[] args)
    {
        Console.ForegroundColor = Color.White;

        Console.Title = "SemanticTokens Controls Example";

        Console.WriteLine("SemanticTokens Controls Example");
        Console.WriteLine("==============================");
        Console.WriteLine();

        // Test the Select control with cursor position functionality
        Console.WriteLine("Testing Select Control - Cursor Position");
        Console.WriteLine("=======================================");
        Console.WriteLine();

        Console.Write("Type something here: ");
        var userInput = Console.ReadLine();
        Console.Write("Now select an option: ");

        var items = new List<SelectItem<Action>>
        {
            new()
            {
                Text = "Option A",
                Value = () => Console.WriteLine("Doing stuff with Option A")
            },
            new()
            {
                Text = "Option B",
                Value = () => Console.WriteLine("Doing stuff with Option B")
            },
            new()
            {
                Text = "Option C",
                Value = () => Console.WriteLine("Doing stuff with Option C")
            },
            new() { Text = "Option D" },
            new() { Text = "Option E" },
            new() { Text = "Option F" },
            new() { Text = "Option G" },
            new() { Text = "Option H" },
            new() { Text = "Option I" },
            new() { Text = "Option J" },
            new() { Text = "Option K" },
            new() { Text = "Option L" },
            new() { Text = "Option M" },
        };

        var selectedItem1 = Select.Show(items);
        if (selectedItem1.IsEmpty())
        {
            Console.WriteLine("Selection cancelled");
            return;
        }

        Console.Write(" and now select another option: ");
        var selectedItem2 = Select.Show(items, 1);

        if (!selectedItem1.IsEmpty() && !selectedItem2.IsEmpty())
        {
            Console.WriteLine($"\nSelected: {selectedItem1.Text} and {selectedItem2.Text}");
            selectedItem1.Value.Invoke();
            selectedItem2.Value.Invoke();
        }
        else
        {
            Console.WriteLine("Selection cancelled");
            return;
        }

        Console.Write("Type something else here: ");
        var userInput2 = Console.ReadLine();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
