using System.Text;
using Controls;
using SemanticTokens.Core;

namespace ControlsExample;

class Program
{
    static void Main(string[] args)
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        System.Console.InputEncoding = Encoding.UTF8;

        Console.ForegroundColor = Color.White;
        Console.BackgroundColor = Color.Black;

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

        var items = new List<SelectItem>
        {
            new()
            {
                Text = "Option A",
                Action = () => Console.WriteLine("Doing stuff with Option A")
            },
            new()
            {
                Text = "Option B",
                Action = () => Console.WriteLine("Doing stuff with Option B")
            },
            new()
            {
                Text = "Option C",
                Action = () => Console.WriteLine("Doing stuff with Option C")
            }
        };

        var selectedItem1 = Select.Show(items);
        Console.Write(" and now select another option: ");
        var selectedItem2 = Select.Show(items);

        if (!selectedItem1.IsEmpty() && !selectedItem2.IsEmpty())
        {
            Console.WriteLine($"\nSelected: {selectedItem1.Text} and {selectedItem2.Text}");
            selectedItem1.Action?.Invoke();
            selectedItem2.Action?.Invoke();
        }
        else
        {
            Console.WriteLine("\nSelection cancelled");
        }

        //SelectDemo.Run();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
