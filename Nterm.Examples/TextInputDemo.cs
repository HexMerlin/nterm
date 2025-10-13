using System.Threading.Tasks.Dataflow;
using Nterm.Core.Controls;

namespace Nterm.Examples;

public class TextInputDemo
{
    public static void Run()
    {
        Terminal.WriteLine("======TextInputDemo========");
        Terminal.WriteLine();
        Terminal.Write("Type something (Hint: type @): ");
        TextInputController textInput = new();
        textInput.KeyUp += (sender, args) =>
        {
            if (args.KeyInfo.KeyChar == '@')
            {
                Terminal.Write('@');
                TextItem<FileSystemInfo> file = FilePicker.Show();
                if (file != null)
                {
                    string proposedText =
                        args.CurrentState.Text.Insert(args.CurrentState.CaretIndex, file.Value.Name)
                        + " ";
                    args.ProposedState = args.CurrentState with
                    {
                        Text = proposedText,
                        CaretIndex = args.CurrentState.CaretIndex + file.Value.Name.Length + 1 // plus space
                    };
                    args.Handled = true;
                }
            }
        };
        TextInputState state = textInput.Read();

        Terminal.WriteLine();
        Terminal.WriteLine($"Typed text: {state.Text}");
    }
}
