using SemanticTokens.Core;
using SemanticTokens.Sixel;

namespace SemanticTokens.Examples;

public sealed record ChatEntry(ConsoleImage AvatarImage, string SenderName, string Text, Color SenderNameColor, Color TextColor)
{
    public void WriteToConsole()
    {
        Console.WriteImage(AvatarImage.ConsoleData);
        Console.Write(" ");
        Console.Write(SenderName, SenderNameColor);
        Console.Write(" ");
        Console.WriteLine(Text, TextColor);
    }
}
