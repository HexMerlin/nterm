using SemanticTokens.Core;
using SemanticTokens.Sixel;
using System.Collections.Immutable;

namespace SemanticTokens.Examples;

public sealed record ChatEntry(ConsoleImage AvatarImage, string SenderName, ImmutableArray<string> TextLines, Color SenderNameColor, Color TextColor)
{
    public void WriteToConsole()
    {
        Console.WriteImage(AvatarImage.ConsoleData);
        Console.Write(" ");
        Console.Write(SenderName, SenderNameColor);
        Console.Write(" ");
        foreach (string line in TextLines)
        {
            Console.WriteLine(line, TextColor);
        }
    }
}
