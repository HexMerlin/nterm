
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SemanticTokens.Document;
using SemanticTokens.Examples;
using SemanticTokens.Core;
using System.Text;
using SemanticTokens.Sixel;

namespace SemanticTokens.DevConsole;

/// <summary>
/// Test program demonstrating <see cref="SemanticDocumentCSharp"/> creation and console rendering.
/// Shows the complete pipeline: C# script → Compilation → SemanticDocumentCSharp → Console output.
/// </summary>
internal static class Program
{
    private static async Task Main()
    {
        //comment out to run specific demo

        ConsoleDemo demo1 = new ConsoleDemo();
        demo1.Run();

        CSharpSyntaxHighlightingDemo demo2 = new CSharpSyntaxHighlightingDemo();
        await demo2.Run();

        ConsoleImageDemo demo3 = new ConsoleImageDemo();
        demo3.Run();

  

    }




}