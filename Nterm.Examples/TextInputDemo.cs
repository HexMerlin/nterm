using Nterm.Core.Controls;

namespace Nterm.Examples;

public class TextInputDemo
{
    public static void Run()
    {
        Terminal.WriteLine("======TextInputDemo========");
        Terminal.WriteLine();
        Terminal.Write("Type something: ");
        TextInputController textInput = new();
        TextInputState state = textInput.Read();

        Terminal.WriteLine();
        Terminal.WriteLine($"Typed text: {state.Text}");
    }
}
