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
        Console.WriteLine("=== Testing New AnsiConsole API ===");
        Console.WriteLine();

        // Test 1: Traditional Console API
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("This is in YELLOW using Console.ForegroundColor");

        // Test 2: New AnsiConsole property-based API
        AnsiConsole.ForegroundColor = Colors.Red;
        AnsiConsole.BackgroundColor = Colors.Blue;
        Console.WriteLine("This is RED on BLUE using AnsiConsole properties!");
        
        Console.WriteLine($"Current Console.ForegroundColor : {Console.ForegroundColor}");
        Console.WriteLine($"Current Console.BackgroundColor : {Console.BackgroundColor}");
        
        // Test 3: New Write(char) overload using current colors
        AnsiConsole.Write('X');
        AnsiConsole.Write('Y');
        AnsiConsole.Write('Z');
        AnsiConsole.Write('\n');

        // Test 4: 24-bit color that doesn't exist in ConsoleColor enum
        AnsiConsole.ForegroundColor = Colors.Chocolate;
        AnsiConsole.BackgroundColor = Colors.Black;
        Console.WriteLine("This is CHOCOLATE colored text!");
        Console.WriteLine($"Current Console.ForegroundColor : {Console.ForegroundColor}");
        //Yes, Console outputs nearest matching color DarkYellow

        // Test 5: Show our current 24-bit color properties
        Console.WriteLine();
        Console.WriteLine($"AnsiConsole.ForegroundColor: R={AnsiConsole.ForegroundColor.R}, G={AnsiConsole.ForegroundColor.G}, B={AnsiConsole.ForegroundColor.B}");
        Console.WriteLine($"AnsiConsole.BackgroundColor: R={AnsiConsole.BackgroundColor.R}, G={AnsiConsole.BackgroundColor.G}, B={AnsiConsole.BackgroundColor.B}");

        // Test 6: Rapid color changes
        Console.WriteLine();
        Console.WriteLine("Rapid color changes test:");
        Color[] colors = [Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Magenta, Colors.Cyan];
        for (int i = 0; i < colors.Length; i++)
        {
            AnsiConsole.ForegroundColor = colors[i];
            AnsiConsole.Write((char)('1' + i));
            AnsiConsole.Write(' ');
        }
        AnsiConsole.Write('\n');

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