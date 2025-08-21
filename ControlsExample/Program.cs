using Controls;
using TrueColor;

namespace ControlsExample;

class Program
{
    static void Main(string[] args)
    {
        AnsiConsole.WriteLine("SemanticTokens Controls Example");
        AnsiConsole.WriteLine("==============================");
        AnsiConsole.WriteLine();

        // Test the Select control with cursor position functionality
        AnsiConsole.WriteLine("Testing Select Control - Cursor Position");
        AnsiConsole.WriteLine("=======================================");
        AnsiConsole.WriteLine();

        AnsiConsole.Write("Type something here: ");
        var userInput = Console.ReadLine();
        AnsiConsole.Write("Now select an option: ");

        var items = new List<SelectItem>
        {
            new() { Text = "Option A", Action = () => Console.WriteLine("You chose Option A") },
            new() { Text = "Option B", Action = () => Console.WriteLine("You chose Option B") },
            new() { Text = "Option C", Action = () => Console.WriteLine("You chose Option C") }
        };

        var selectedItem = Select.Show(items);

        if (!selectedItem.IsEmpty())
        {
            AnsiConsole.WriteLine($"\nSelected: {selectedItem.Text}");
            selectedItem.Action?.Invoke();
        }
        else
        {
            AnsiConsole.WriteLine("\nSelection cancelled");
        }

        //SelectDemo.Run();

        AnsiConsole.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
