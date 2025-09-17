using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Nterm.Core;
using Nterm.Document;
using System.Text;

namespace Nterm.Examples;

public class CSharpSyntaxHighlightingDemo
{
    public CSharpSyntaxHighlightingDemo() { }

    /// <summary>
    /// Demonstrates complete CSharpDocument pipeline with C# script syntax highlighting.
    /// Creates a sample C# script, processes it through Roslyn classification,
    /// resolves styling, and renders to console with colors.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    public static async Task Run()
    {
        System.Console.OutputEncoding = Encoding.UTF8;

        //A simple C# script
        string scriptText = """
using System;

static int AddFive(int initial) => initial + 5;

int x = 42;
/*
Some comment section here
*/
string msg = $"Value = {x} ";
Terminal.WriteLine(msg + AddFive(5).ToString()); // Just an inline comment
""";

        // 2) Compile the script
        Script<object> script = CSharpScript.Create(
            scriptText,
            options: ScriptOptions.Default.WithImports("System")
        );

        Microsoft.CodeAnalysis.Compilation compilation = script.GetCompilation();

        // 3) Create CSharpDocument with full classification fidelity
        CSharpDocument document = await CSharpDocument.CreateAsync(compilation);

        // 4) Render to console using rich semantic information
        ConsoleRenderer.Render(document);
        Terminal.WriteLine();

        Terminal.WriteLine("=== CSharp Syntax Highlighting Demo Complete ===");
    }
}
