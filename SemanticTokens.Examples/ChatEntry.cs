using SemanticTokens.Core;
using SemanticTokens.Sixel;
using System.Collections.Immutable;

namespace SemanticTokens.Examples;

public sealed record ChatEntry(ConsoleImage AvatarImage, string SenderName, ImmutableArray<string> TextLines, Color SenderNameColor, Color TextColor)
{
    public void WriteToConsole()
    {
        // Capture starting cursor position
        int startLeft = System.Console.CursorLeft;
        int startTop = System.Console.CursorTop;
        
        // Write the avatar image
        Console.WriteImage(AvatarImage.ConsoleData);
        
        // Calculate image dimensions in character cells
        ConsoleImageCharacterSize imageSize = AvatarImage.CharacterSize;
        
        // Position cursor to the right of the image, aligned with image top
        int textLeft = startLeft + imageSize.Columns + 1; // +1 for small margin
        int textTop = startTop;
        
        // Write sender name and text lines
        Console.SetCursorPosition(textLeft, textTop);
        Console.Write(SenderName, SenderNameColor);
        Console.Write(" ");
        
        // Write each text line, positioning cursor at the start of each line
        for (int i = 0; i < TextLines.Length; i++)
        {
            Console.SetCursorPosition(textLeft, textTop + i);
            if (i == 0)
            {
                // First line already has sender name, continue after it
                int nameWidth = SenderName.Length + 1; // +1 for space after name
                Console.SetCursorPosition(textLeft + nameWidth, textTop);
            }
            
            Console.Write(TextLines[i], TextColor);
        }
        
        // Position cursor after the chat entry (below the image or text, whichever is taller)
        int finalTop = Math.Max(startTop + imageSize.Rows, textTop + TextLines.Length);
        Console.SetCursorPosition(0, finalTop);
    }
}
