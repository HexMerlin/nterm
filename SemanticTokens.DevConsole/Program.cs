using SemanticTokens.Examples;
using SemanticTokens.Core;
using System.Text;

namespace SemanticTokens.DevConsole;

/// <summary>
/// Test program demonstrating <see cref="SemanticDocumentCSharp"/> creation and console rendering.
/// Shows the complete pipeline: C# script → Compilation → SemanticDocumentCSharp → Console output.
/// </summary>
internal static class Program
{
    private static async Task Main()
    {
        Console.Title = "Image Demo";
        Console.BackgroundColor = Color.Navy;
        Console.Clear();

        //comment out to run specific demo
        ConsoleDemo demo1 = new ConsoleDemo();
        demo1.Run();

        CSharpSyntaxHighlightingDemo demo2 = new CSharpSyntaxHighlightingDemo();
        await demo2.Run();

        ConsoleImageDemo demo3 = new ConsoleImageDemo();
        await demo3.RunAsync();
        await demo3.RunAsync();

    }




}