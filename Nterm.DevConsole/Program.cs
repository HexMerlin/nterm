using System.Globalization;
using Nterm.Core;
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
        Terminal.Title = "Showing some Console Demos";
        Terminal.Clear(new Color(0, 0, 40));
        //comment out to run specific demo

        ConsoleDemo.Run();

        await CSharpSyntaxHighlightingDemo.Run();

        ConsoleImageDemo demo3 = new();
        await demo3.RunAsync(); //run demo 3

        for (int i = 0; i <= 30; i++) //write some lines
            Terminal.WriteLine(i.ToString(CultureInfo.InvariantCulture));

        await demo3.RunAsync(); //run demo 3 again

        Terminal.Write("\n\n\n");
    }
}
