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
        //the terminal I run this from (windows terminal) have WHTTE text on BLUE background as default colors
        AnsiBuffer buffer = new AnsiBuffer();
        buffer.AppendLine("This is white (default) text on blue bg (default) - CORRECT!"); 
        buffer.AppendLine("This is RED on BLUE default background - CORRECT!", Color.Red); 
        buffer.AppendLine("this is blue text with yellow background - CORRECT!)", Color.Blue, Color.Yellow); 
        buffer.AppendLine("This is white default text, on green background - CORRECT!", background: Color.Green); 

        buffer.AppendLine("This all terminal default colors (WHITE on BLUE) - CORRECT!"); 
                                                                   

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
