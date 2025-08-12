using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SemanticDocuments;
using System.Text;
using TrueColor;

namespace ConsoleApp;

/// <summary>
/// Test program demonstrating <see cref="SemanticDocumentCSharp"/> creation and console rendering.
/// Shows the complete pipeline: C# script → Compilation → SemanticDocumentCSharp → Console output.
/// </summary>
internal static class Program
{


    private static void Main()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("This is in YELLOW.");
        AnsiConsole.Write('\n', Colors.Red, Colors.Blue);
        Console.WriteLine($"Current Console.ForegroundColor : {Console.ForegroundColor}");
        Console.WriteLine($"Current Console.BackgroundColor : {Console.BackgroundColor}");
        Console.WriteLine("This is RED on BLUE background");

        //Now setting color to some color that does not exist in enum ConsoleColor
        AnsiConsole.Write('\n', Colors.Chocolate, Colors.Black);
        
        Console.WriteLine($"Current Console.ForegroundColor : {Console.ForegroundColor}");
        //Yes, Console outputs nearest matching color DarkYellow

        Console.WriteLine();


    }

    /// <summary>
    /// Demonstrates complete SemanticDocumentCSharp pipeline with C# script syntax highlighting.
    /// Creates a sample C# script, processes it through Roslyn classification,
    /// resolves semantic styling, and renders to console with colors.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    private static async Task Main2()
    {
        Console.OutputEncoding = Encoding.UTF8;

        //A simple C# script
        string scriptText = """
using System;
                
static int AddFive(int initial) => initial + 5;

int x = 42;
/* 
Some comment section here
*/
string msg = $"Value = {x} ";
Console.WriteLine(msg + AddFive(5).ToString()); // Just an inline comment
""";

    
        // 2) Compile the script
        Script<object> script = CSharpScript.Create(
            scriptText,
            options: ScriptOptions.Default.WithImports("System")
        );

        Microsoft.CodeAnalysis.Compilation compilation = script.GetCompilation();

        // 3) Create SemanticDocument with full classification fidelity
        SemanticDocument document = await SemanticDocumentCSharp.CreateAsync(compilation);

        // 4) Render to console using rich semantic information
        SemanticDocumentConsoleRenderer.Render(document);
        Console.WriteLine();

    }
}