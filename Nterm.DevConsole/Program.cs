using System.Globalization;
using System.Reflection;
using Nterm.Core;
using Nterm.Core.Buffer;
using Nterm.Examples;
using Nterm.Sixel;
using Nterm.Common;

namespace Nterm.DevConsole;

/// <summary>
/// Test program demonstrating <see cref="CSharpDocument"/> creation and console rendering.
/// Shows the complete pipeline: C# script → Compilation → SemanticDocumentCSharp → Console output.
/// </summary>
internal static class Program
{

    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        // Create a TerminalImage from embedded resource (using Examples assembly)
        Assembly examplesAssembly = typeof(ConsoleImageDemo).Assembly;
        TerminalImage userImage = TerminalImage.FromEmbeddedResource(
            ConsoleImageDemo.ImageUser,
            examplesAssembly,
            "[👤]",
            Transparency.Default
        );
        
        // Demonstrate AnsiBuffer with text AND images
        AnsiBuffer buffer = new AnsiBuffer();
        buffer.AppendLine("=== AnsiBuffer Demo: Text + SIXEL Graphics ===");
        buffer.AppendLine();

        // Append SIXEL image using Append(string) - EncodedData contains complete SIXEL sequence
     
        buffer.AppendLine("This should be default text on default background");
        buffer.AppendLine("This is GREEN on default background", Color.Green);
        buffer.AppendLine("this is blue text with yellow background", Color.Blue, Color.Yellow);
        buffer.AppendLine("This is default text, on green background", background: Color.Green);
        buffer.AppendLine("This all terminal default colors");

        Console.Write(buffer.ToString()); // Outputs colored text + SIXEL image to terminal
        
        Console.Write(userImage.EncodedData);


    }

    private static async Task Main2()
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
