using System.Runtime.Versioning;
using System.Text;

namespace Nterm.Examples;

public class ConsoleWindowDemo
{
    private static int SaveBufferWidth { get; set; }
    private static int SaveBufferHeight { get; set; }
    private static int SaveWindowHeight { get; set; }
    private static int SaveWindowWidth { get; set; }
    private static bool SaveCursorVisible { get; set; }

    [SupportedOSPlatform("windows")]
    public static void Run()
    {
        string m1 =
            "1) Press the cursor keys to move the console window.\n"
            + "2) Press any key to begin. When you're finished...\n"
            + "3) Press the Escape key to quit.";
        string g1 = "+----";
        string g2 = "|    ";
        string grid1;
        string grid2;
        StringBuilder sbG1 = new();
        StringBuilder sbG2 = new();
        ConsoleKeyInfo cki;
        int y;
        //
        try
        {
            SaveBufferWidth = Console.BufferWidth;
            SaveBufferHeight = Console.BufferHeight;
            SaveWindowHeight = Console.WindowHeight;
            SaveWindowWidth = Console.WindowWidth;
            SaveCursorVisible = Console.CursorVisible;
            //
            Console.Clear();
            Console.WriteLine(m1);
            _ = Console.ReadKey(true);

            // Set the smallest possible window size before setting the buffer size.
            Console.SetWindowSize(1, 1);
            Console.SetBufferSize(80, 80);
            Console.SetWindowSize(40, 20);

            // Create grid lines to fit the buffer. (The buffer width is 80, but
            // this same technique could be used with an arbitrary buffer width.)
            for (y = 0; y < Console.BufferWidth / g1.Length; y++)
            {
                _ = sbG1.Append(g1);
                _ = sbG2.Append(g2);
            }
            _ = sbG1.Append(g1, 0, Console.BufferWidth % g1.Length);
            _ = sbG2.Append(g2, 0, Console.BufferWidth % g2.Length);
            grid1 = sbG1.ToString();
            grid2 = sbG2.ToString();

            Console.CursorVisible = false;
            Console.Clear();
            for (y = 0; y < Console.BufferHeight - 1; y++)
            {
                if (y % 3 == 0)
                    Console.Write(grid1);
                else
                    Console.Write(grid2);
            }

            Console.SetWindowPosition(0, 0);
            do
            {
                cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (Console.WindowLeft > 0)
                            Console.SetWindowPosition(Console.WindowLeft - 1, Console.WindowTop);
                        break;
                    case ConsoleKey.UpArrow:
                        if (Console.WindowTop > 0)
                            Console.SetWindowPosition(Console.WindowLeft, Console.WindowTop - 1);
                        break;
                    case ConsoleKey.RightArrow:
                        if (Console.WindowLeft < (Console.BufferWidth - Console.WindowWidth))
                            Console.SetWindowPosition(Console.WindowLeft + 1, Console.WindowTop);
                        break;
                    case ConsoleKey.DownArrow:
                        if (Console.WindowTop < (Console.BufferHeight - Console.WindowHeight))
                            Console.SetWindowPosition(Console.WindowLeft, Console.WindowTop + 1);
                        break;
                }
            } while (cki.Key != ConsoleKey.Escape); // end do-while
        } // end try
        catch (IOException e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            Console.Clear();
            Console.SetWindowSize(1, 1);
            Console.SetBufferSize(SaveBufferWidth, SaveBufferHeight);
            Console.SetWindowSize(SaveWindowWidth, SaveWindowHeight);
            Console.CursorVisible = SaveCursorVisible;
        }
    }
}
/*
This example produces results similar to the following:

1) Press the cursor keys to move the console window.
2) Press any key to begin. When you're finished...
3) Press the Escape key to quit.

...

+----+----+----+-
|    |    |    |
|    |    |    |
+----+----+----+-
|    |    |    |
|    |    |    |
+----+----+----+-

*/
