using System.Globalization;
using Nterm.Core;
using Nterm.Core.Buffer;
using Nterm.Examples;

namespace Nterm.DevConsole;

/// <summary>
/// Test program demonstrating <see cref="CSharpDocument"/> creation and console rendering.
/// Shows the complete pipeline: C# script → Compilation → SemanticDocumentCSharp → Console output.
/// </summary>
internal static class Program
{

    private static async Task Main()
    {
        // Reset terminal to known default colors: Red text on Gray background
        Console.Write(AnsiBuffer.Reset(foreground: Color.Red, background: Color.Gray));
        
        //the terminal I run this from (windows terminal) have WHITE text on BLUE background as default colors
        //but after Reset() above, we should have RED text on GRAY background as new defaults
        AnsiBuffer buffer = new AnsiBuffer();
        buffer.AppendLine("This should be RED (new default) text on GRAY bg (new default)");
        buffer.AppendLine("This is GREEN on GRAY default background", Color.Green);
        buffer.AppendLine("this is blue text with yellow background", Color.Blue, Color.Yellow);
        buffer.AppendLine("This is RED default text, on green background", background: Color.Green);

        buffer.AppendLine("This all terminal default colors (RED on GRAY)");


        Console.Write(buffer.ToString()); // Outputs colored text to terminal

    }

    //private static async Task Main()
    //{
    //    Terminal.Title = "Showing some Console Demos";
    //    Terminal.Clear(new Color(0, 0, 40));
    //    //comment out to run specific demo

    //    ConsoleDemo.Run();

    //    await CSharpSyntaxHighlightingDemo.Run();

    //    ConsoleImageDemo demo3 = new();
    //    await demo3.RunAsync(); //run demo 3

    //    for (int i = 0; i <= 30; i++) //write some lines
    //        Terminal.WriteLine(i.ToString(CultureInfo.InvariantCulture));

    //    await demo3.RunAsync(); //run demo 3 again

    //    Terminal.Write("\n\n\n");
    //}
}
