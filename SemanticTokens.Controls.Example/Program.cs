using System.Text;
using SemanticTokens.Core;

namespace SemanticTokens.Controls.Example;

class Program
{
    static void Main(string[] args)
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        System.Console.InputEncoding = Encoding.UTF8;

        Console.ForegroundColor = Color.White;
        Console.BackgroundColor = Color.Transparent;

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
                Command = () => Console.WriteLine("Doing stuff with Option A")
            },
            new()
            {
                Text = "Option B",
                Command = () => Console.WriteLine("Doing stuff with Option B")
            },
            new()
            {
                Text = "Option C",
                Command = () => Console.WriteLine("Doing stuff with Option C")
            }
        };

        var selectedItem1 = Select.Show(items);
        Console.Write(" and now select another option: ");
        var selectedItem2 = Select.Show(items);

        if (!selectedItem1.IsEmpty() && !selectedItem2.IsEmpty())
        {
            Console.WriteLine($"\nSelected: {selectedItem1.Text} and {selectedItem2.Text}");
            selectedItem1.Command.Invoke();
            selectedItem2.Command.Invoke();
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
