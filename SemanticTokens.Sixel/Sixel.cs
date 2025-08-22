using System.Diagnostics;
using SemanticTokens.Core;

namespace SemanticTokens.Sixel;

//public partial class Sixel
//{
//    [Conditional("SIXEL_DEBUG")]
//    private static void DebugPrint(ReadOnlySpan<char> msg, ConsoleColor fg = ConsoleColor.Magenta, bool lf = false)
//    {
//        Color currentFg = Console.ForegroundColor;
//        Console.ForegroundColor = fg;
//        if (lf)
//            Console.WriteLine(msg);
//        else
//            Console.Write(msg);
//        Console.ForegroundColor = currentFg;
//    }
//}
