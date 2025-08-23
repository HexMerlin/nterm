using SemanticTokens.Core;
using SemanticTokens.Sixel;
using System.Collections.Immutable;

namespace SemanticTokens.Examples;

/// <summary>
/// Immutable chat message entry containing avatar image and formatted text content.
/// Optimized for console rendering with precise character-cell alignment.
/// </summary>
/// <param name="AvatarImage">Console-ready avatar image with SIXEL or fallback encoding</param>
/// <param name="SenderName">Sender display name</param>
/// <param name="TextLines">Message text lines in display order</param>
/// <param name="SenderNameColor">24-bit color for sender name rendering</param>
/// <param name="TextColor">24-bit color for message text rendering</param>
/// <remarks>
/// <para>
/// Provides side-by-side layout: avatar image left-aligned, text content right-aligned with precise vertical positioning.
/// Text positioning adapts based on message length: messages with ≥3 lines align with image top,
/// shorter messages offset +1 row for natural visual balance.
/// </para>
/// <para>
/// Console rendering preserves cursor state and positions final cursor below the complete chat entry bounds.
/// Character cell calculations leverage terminal capability detection for pixel-perfect alignment.
/// </para>
/// </remarks>
public sealed record ChatEntry(ConsoleImage AvatarImage, string SenderName, ImmutableArray<string> TextLines, Color SenderNameColor, Color TextColor)
{
    private const int TextMargin = 1;       // Character spacing between image and text
    private const int ShortMessageLines = 3; // Threshold for vertical offset adjustment

    /// <summary>
    /// Renders complete chat entry to console with side-by-side image and text layout.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rendering sequence: captures cursor position → writes avatar image → calculates text positioning →
    /// writes sender name and message lines → positions cursor after entry bounds.
    /// </para>
    /// <para>
    /// Text vertical alignment: top-aligned <c>iff</c> <c>TextLines.Length ≥ 3</c>, otherwise offset +1 row.
    /// Horizontal positioning: <c>textLeft = imageLeft + imageWidth + 1</c> for consistent margin.
    /// </para>
    /// </remarks>
    public void WriteToConsole()
    {
        (int startLeft, int startTop) = (System.Console.CursorLeft, System.Console.CursorTop);
        
        Console.WriteImage(AvatarImage.ConsoleData);
        
        ConsoleImageCharacterSize imageSize = AvatarImage.CharacterSize;
        int textLeft = startLeft + imageSize.Columns + TextMargin;
        int textTop = startTop + (TextLines.Length < ShortMessageLines ? 1 : 0);
        
        // Write sender name on first text line
        Console.SetCursorPosition(textLeft, textTop);
        Console.Write(SenderName, SenderNameColor);
        Console.Write(" ");
        
        // Write message text lines
        int senderNameWidth = SenderName.Length + 1;
        for (int i = 0; i < TextLines.Length; i++)
        {
            int lineLeft = textLeft + (i == 0 ? senderNameWidth : 0);
            Console.SetCursorPosition(lineLeft, textTop + i);
            Console.Write(TextLines[i], TextColor);
        }
        
        // Position cursor after chat entry bounds
        int finalTop = Math.Max(startTop + imageSize.Rows, textTop + TextLines.Length);
        Console.SetCursorPosition(0, finalTop);
    }
}
