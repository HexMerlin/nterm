using Nterm.Core;

namespace Nterm.Examples;

public class SetCursorDemo
{
    public static void Run()
    {
        int width = Terminal.WindowWidth;
        int height = Terminal.WindowHeight;

        //Terminal.WriteLine(new string('\n', height));

        string tl = "(0,0)";
        string tr = $"({width - 1},0)";
        string bl = $"(0,{height - 1})";
        string br = $"({width - 1},{height - 1})";

        Color fg = Color.Black; // readable on white background
        Color bg = Color.White;

        WriteAt(0, 0, tl, fg, bg);
        WriteAt(Math.Max(0, width - tr.Length), 0, tr, fg, bg);
        WriteAt(0, Math.Max(0, height - 1), bl, fg, bg);
        WriteAt(Math.Max(0, width - br.Length), Math.Max(0, height - 1), br, fg, bg);

        // Center: show buffer size and window size
        string center =
            $"Buffer: {Terminal.BufferWidth}x{Terminal.BufferHeight}  "
            + $"Window: {Terminal.WindowWidth}x{Terminal.WindowHeight}";

        int centerTop = Math.Max(0, height / 2);
        int centerLeft = Math.Max(0, (width - center.Length) / 2);
        WriteAt(centerLeft, centerTop, center, fg, bg);
        Terminal.SetCursorPosition(0, height);
        Terminal.WriteLine();
        Terminal.WriteLine("=== Set Cursor Demo Complete ===");
    }

    private static void WriteAt(int left, int top, string text, Color fg, Color bg)
    {
        Terminal.SetCursorPosition(left, top);
        Terminal.Write(text, fg, bg);
    }
}
