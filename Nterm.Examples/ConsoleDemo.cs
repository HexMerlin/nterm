using NTerm.Core;

namespace NTerm.Examples;
public class ConsoleDemo
{
    public ConsoleDemo() { }

    public void Run()
    {

        // Console.ForegroundColor = Color.MediumSpringGreen;
        // Console.WriteLine();
        Terminal.WriteLine("=== Terminal Capabilities ===");
        Terminal.WriteLine($"SIXEL Support: {TerminalCapabilities.IsSixelSupported}");
        Terminal.WriteLine($"Cell Size: {TerminalCapabilities.CellSize.Width}x{TerminalCapabilities.CellSize.Height}px");
        Terminal.WriteLine($"Window Size: {TerminalCapabilities.WindowCharacterSize.Width}x{TerminalCapabilities.WindowCharacterSize.Height} chars");
        Terminal.WriteLine();

        Terminal.WriteLine("=== Testing New Console API ===");

        Terminal.ForegroundColor = Color.White;

        //Testing to string conversion to known color "00FFEBCD" (Blanched Almond)
        Color color1 = 0xFFFFEBCDu;
        Terminal.WriteLine($"Named color code 0xFFFFEBCDu ToString: " + color1.ToString());

        //Testing to string conversion to non-named color - outputs RGB values instead "R:255, G:235, B:206"
        Color color2 = 0xFFFFEBCEu;
        Terminal.WriteLine("Unnamed color code 0xFFFFEBCEu ToString" + color2.ToString());

        Terminal.WriteLine();

        // Test 1: Traditional Console API
        Terminal.ForegroundColor = Color.Yellow;
        Terminal.WriteLine("This is in YELLOW using Console.ForegroundColor");

        Terminal.WriteLine("This is RED on BLUE!", Color.Red, Color.Blue);

        Terminal.WriteLine($"Current Console.ForegroundColor : {Terminal.ForegroundColor}");
        Terminal.WriteLine($"Current Console.BackgroundColor : {Terminal.BackgroundColor}");

        // Test 3: New Write(char) overload using current colors
        Terminal.Write('X');
        Terminal.Write('Y');
        Terminal.Write('Z');
        Terminal.Write('\n');

        // Test 4: 24-bit color that doesn't exist in ConsoleColor enum
        Terminal.ForegroundColor = Color.Chocolate;
        Terminal.WriteLine("This is CHOCOLATE colored text!");
        Terminal.WriteLine($"Current Console.ForegroundColor : {Terminal.ForegroundColor}");
        //Yes, Console outputs nearest matching color DarkYellow

        // Test 5: Show our current 24-bit color properties
        Terminal.WriteLine();
        Terminal.WriteLine($"Console.ForegroundColor: R={Terminal.ForegroundColor.R}, G={Terminal.ForegroundColor.G}, B={Terminal.ForegroundColor.B}");
        Terminal.WriteLine($"Console.BackgroundColor: R={Terminal.BackgroundColor.R}, G={Terminal.BackgroundColor.G}, B={Terminal.BackgroundColor.B}");

        // Test 6: Rapid color changes
        Terminal.WriteLine();
        Terminal.WriteLine("Rapid color changes test:");
        Color[] colors = [Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Magenta, Color.Cyan];
        for (int i = 0; i < colors.Length; i++)
        {
            Terminal.ForegroundColor = colors[i];
            Terminal.Write((char)('1' + i));
            Terminal.Write(' ');
        }
        Terminal.Write('\n');

        // Test 7: New string writing methods
        Terminal.WriteLine();
        Terminal.WriteLine("=== Testing String Writing Methods ===");

        Terminal.ForegroundColor = Color.Green;
        Terminal.Write("Short string test");
        Terminal.WriteLine(" with WriteLine!");

        Terminal.WriteLine("This is a complete line in green");

        // Test with colors
        Terminal.Write("Colored string: ", Color.Yellow, Color.DarkBlue);
        Terminal.WriteLine("Yellow on dark blue!", Color.Yellow, Color.DarkBlue);

        // Test longer string
        string longText = "This is a longer string to test chunked processing with more than 256 characters. ".PadRight(300, 'x');
        Terminal.WriteLine(longText, Color.Magenta, Color.Black);

        // Test empty WriteLine
        Terminal.WriteLine(); // Just newline

    }
}
