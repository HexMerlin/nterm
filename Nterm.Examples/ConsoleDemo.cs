using NTerm.Core;
using NTerm.Sixel;
using System.Text;

namespace NTerm.Examples;
public class ConsoleDemo
{
    public ConsoleDemo() {}

    public void Run()
    {

       // Console.ForegroundColor = Color.MediumSpringGreen;
       // Console.WriteLine();
        Console.WriteLine("=== Terminal Capabilities ===");
        Console.WriteLine($"SIXEL Support: {TerminalCapabilities.IsSixelSupported}");
        Console.WriteLine($"Cell Size: {TerminalCapabilities.CellSize.Width}x{TerminalCapabilities.CellSize.Height}px");
        Console.WriteLine($"Window Size: {TerminalCapabilities.WindowCharacterSize.Width}x{TerminalCapabilities.WindowCharacterSize.Height} chars");
        Console.WriteLine();

        Console.WriteLine("=== Testing New Console API ===");

        Console.ForegroundColor = Color.White;

        //Testing to string conversion to known color "00FFEBCD" (Blanched Almond)
        Color color1 = 0xFFFFEBCDu;
        Console.WriteLine($"Named color code 0xFFFFEBCDu ToString: " + color1.ToString());

        //Testing to string conversion to non-named color - outputs RGB values instead "R:255, G:235, B:206"
        Color color2 = 0xFFFFEBCEu;
        Console.WriteLine("Unnamed color code 0xFFFFEBCEu ToString" + color2.ToString());

        Console.WriteLine();

        // Test 1: Traditional Console API
        Console.ForegroundColor = Color.Yellow;
        Console.WriteLine("This is in YELLOW using Console.ForegroundColor");

        Console.WriteLine("This is RED on BLUE!", Color.Red, Color.Blue);

        Console.WriteLine($"Current Console.ForegroundColor : {Console.ForegroundColor}");
        Console.WriteLine($"Current Console.BackgroundColor : {Console.BackgroundColor}");

        // Test 3: New Write(char) overload using current colors
        Console.Write('X');
        Console.Write('Y');
        Console.Write('Z');
        Console.Write('\n');

        // Test 4: 24-bit color that doesn't exist in ConsoleColor enum
        Console.ForegroundColor = Color.Chocolate;
        Console.WriteLine("This is CHOCOLATE colored text!");
        Console.WriteLine($"Current Console.ForegroundColor : {Console.ForegroundColor}");
        //Yes, Console outputs nearest matching color DarkYellow

        // Test 5: Show our current 24-bit color properties
        Console.WriteLine();
        Console.WriteLine($"Console.ForegroundColor: R={Console.ForegroundColor.R}, G={Console.ForegroundColor.G}, B={Console.ForegroundColor.B}");
        Console.WriteLine($"Console.BackgroundColor: R={Console.BackgroundColor.R}, G={Console.BackgroundColor.G}, B={Console.BackgroundColor.B}");

        // Test 6: Rapid color changes
        Console.WriteLine();
        Console.WriteLine("Rapid color changes test:");
        Color[] colors = [Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Magenta, Color.Cyan];
        for (int i = 0; i < colors.Length; i++)
        {
            Console.ForegroundColor = colors[i];
            Console.Write((char)('1' + i));
            Console.Write(' ');
        }
        Console.Write('\n');


        // Test 7: New string writing methods
        Console.WriteLine();
        Console.WriteLine("=== Testing String Writing Methods ===");

        Console.ForegroundColor = Color.Green;
        Console.Write("Short string test");
        Console.WriteLine(" with WriteLine!");

        Console.WriteLine("This is a complete line in green");

        // Test with colors
        Console.Write("Colored string: ", Color.Yellow, Color.DarkBlue);
        Console.WriteLine("Yellow on dark blue!", Color.Yellow, Color.DarkBlue);

        // Test longer string
        string longText = "This is a longer string to test chunked processing with more than 256 characters. ".PadRight(300, 'x');
        Console.WriteLine(longText, Color.Magenta, Color.Black);

        // Test empty WriteLine
        Console.WriteLine(); // Just newline

    }
}
