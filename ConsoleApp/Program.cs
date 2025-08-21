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
        AnsiConsole.WriteLine("=== Testing New AnsiConsole API ===");

        Console.OutputEncoding = Encoding.UTF8;
        
        AnsiConsole.ForegroundColor = Color.White;

        //Testing to string conversion to known color "00FFEBCD" (Blanched Almond)
        Color color1 = 0x00FFEBCDu;
        AnsiConsole.WriteLine(color1.ToString());

        //Testing to string conversion to non-named color - outputs RGB values instead "R:255, G:235, B:206"
        Color color2 = 0x00FFEBCEu;
        AnsiConsole.WriteLine(color2.ToString());

        AnsiConsole.WriteLine();

        // Test 1: Traditional Console API
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("This is in YELLOW using Console.ForegroundColor");

        // Test 2: New AnsiConsole property-based API
        AnsiConsole.ForegroundColor = Color.Red;
        AnsiConsole.BackgroundColor = Color.Blue;
        AnsiConsole.WriteLine("This is RED on BLUE using AnsiConsole properties!");

        AnsiConsole.WriteLine($"Current Console.ForegroundColor : {Console.ForegroundColor}");
        AnsiConsole.WriteLine($"Current Console.BackgroundColor : {Console.BackgroundColor}");
        
        // Test 3: New Write(char) overload using current colors
        AnsiConsole.Write('X');
        AnsiConsole.Write('Y');
        AnsiConsole.Write('Z');
        AnsiConsole.Write('\n');

        // Test 4: 24-bit color that doesn't exist in ConsoleColor enum
        AnsiConsole.ForegroundColor = Color.Chocolate;
        AnsiConsole.BackgroundColor = Color.Black;
        AnsiConsole.WriteLine("This is CHOCOLATE colored text!");
        AnsiConsole.WriteLine($"Current Console.ForegroundColor : {Console.ForegroundColor}");
        //Yes, Console outputs nearest matching color DarkYellow

        // Test 5: Show our current 24-bit color properties
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine($"AnsiConsole.ForegroundColor: R={AnsiConsole.ForegroundColor.R}, G={AnsiConsole.ForegroundColor.G}, B={AnsiConsole.ForegroundColor.B}");
        AnsiConsole.WriteLine($"AnsiConsole.BackgroundColor: R={AnsiConsole.BackgroundColor.R}, G={AnsiConsole.BackgroundColor.G}, B={AnsiConsole.BackgroundColor.B}");

        // Test 6: Rapid color changes
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("Rapid color changes test:");
        Color[] colors = [Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Magenta, Color.Cyan];
        for (int i = 0; i < colors.Length; i++)
        {
            AnsiConsole.ForegroundColor = colors[i];
            AnsiConsole.Write((char)('1' + i));
            AnsiConsole.Write(' ');
        }
        AnsiConsole.Write('\n');


        // Test 7: New string writing methods
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("=== Testing String Writing Methods ===");
        
        AnsiConsole.ForegroundColor = Color.Green;
        AnsiConsole.Write("Short string test");
        AnsiConsole.WriteLine(" with WriteLine!");
        
        AnsiConsole.WriteLine("This is a complete line in green");
        
        // Test with colors
        AnsiConsole.Write("Colored string: ", Color.Yellow, Color.DarkBlue);
        AnsiConsole.WriteLine("Yellow on dark blue!", Color.Yellow, Color.DarkBlue);
        
        // Test longer string
        string longText = "This is a longer string to test chunked processing with more than 256 characters. ".PadRight(300, 'x');
        AnsiConsole.WriteLine(longText, Color.Magenta, Color.Black);
        
        // Test empty WriteLine
        AnsiConsole.WriteLine(); // Just newline

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