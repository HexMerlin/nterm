using NTerm.Core;
using NTerm.Examples;

namespace NTerm.DevConsole;

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

        ConsoleDemo demo1 = new();
        demo1.Run();

        CSharpSyntaxHighlightingDemo demo2 = new();
        await demo2.Run();

        ConsoleImageDemo demo3 = new();
        await demo3.RunAsync(); //run demo 3

        for (int i = 0; i <= 30; i++) //write some lines
            System.Console.WriteLine(i.ToString());

        await demo3.RunAsync();//run demo 3 again

        Terminal.Write("\n\n\n");
    }
}