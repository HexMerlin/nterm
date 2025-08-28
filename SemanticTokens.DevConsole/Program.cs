using SemanticTokens.Examples;
using SemanticTokens.Core;

namespace SemanticTokens.DevConsole;

/// <summary>
/// Test program demonstrating <see cref="SemanticDocumentCSharp"/> creation and console rendering.
/// Shows the complete pipeline: C# script → Compilation → SemanticDocumentCSharp → Console output.
/// </summary>
internal static class Program
{
    private static async Task Main()
    {
        Console.Clear(Color.Navy);

        Console.Title = "Showing some Console Demos";
              
        //comment out to run specific demo

        ConsoleDemo demo1 = new ConsoleDemo();
        demo1.Run();

        CSharpSyntaxHighlightingDemo demo2 = new CSharpSyntaxHighlightingDemo();
        await demo2.Run();

        ConsoleImageDemo demo3 = new ConsoleImageDemo();
        await demo3.RunAsync(); //run demo 3
              
        for (int i = 0; i <= 30; i++) //write some lines
            System.Console.WriteLine(i.ToString());

        await demo3.RunAsync();//run demo 3 again

        Console.Write("\n\n\n");
    }




}