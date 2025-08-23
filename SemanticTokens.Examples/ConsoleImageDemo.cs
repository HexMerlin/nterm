using SemanticTokens.Core;
using SemanticTokens.Sixel;
using System.Reflection;

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

/// <summary>
/// Perfect console image demonstration using Optimized ConsoleImage API.
/// </summary>
/// <remarks>
/// Replaces all ugly manual ANSI cursor manipulation with clean single-execution-path design.
/// Demonstrates authority-driven architecture with automatic SIXEL/fallback handling.
/// </remarks>
public sealed class ConsoleImageDemo
{
    public const string ImageUser = "Images.user.png";
    public const string ImageBot = "Images.bot.png";
    public const string ImageAI = "Images.ai.png";

    // Pre-built optimized images (authority pattern)
    private ConsoleImage UserAvatarImage { get; }
    private ConsoleImage BotAvatarImage { get; }
    private ConsoleImage AiAvatarImage { get; }

    /// <summary>
    /// Constructs demo with pre-encoded console images.
    /// </summary>
    public ConsoleImageDemo()
    {
        // Build perfect console images once during construction
        // Use Examples assembly since that's where the embedded resources are
        Assembly examplesAssembly = typeof(ConsoleImageDemo).Assembly;

        UserAvatarImage = ConsoleImageBuilder.FromEmbeddedResource(ImageUser, examplesAssembly)
            .WithCharacterSize(8, 8)
            .WithFallbackText("[👤]")
            .WithTransparency(Transparency.Default)
            .Build();

        BotAvatarImage = ConsoleImageBuilder.FromEmbeddedResource(ImageBot, examplesAssembly)
            .WithCharacterSize(8, 8)
            .WithFallbackText("[🤖]")
            .WithTransparency(Transparency.Default)
            .Build();

        AiAvatarImage = ConsoleImageBuilder.FromEmbeddedResource(ImageAI, examplesAssembly)
            .WithCharacterSize(8, 8)
            .WithFallbackText("[🧠]")
            .WithTransparency(Transparency.Default)
            .Build();
    }



    /// <summary>
    /// Demonstrates example console image usage.
    /// </summary>
    /// <remarks>
    /// Chat diaglog example with automatic SIXEL/fallback handling for images.
    /// </remarks>
    public void Run()
    {
        Console.WriteLine();

        ChatEntry userEntry = new ChatEntry(UserAvatarImage, "[User]", @"Hey! Can you show avatars inline?
Let's make sure text aligns neatly with those images.", Color.Cyan, Color.LightCyan);

        ChatEntry botEntry = new ChatEntry(BotAvatarImage, "[Bot]", @"Sure thing! I'll format the output with bullets and keep it short. 
If your terminal supports SIXEL, you should see avatars on the left.", Color.OrangeRed, Color.Goldenrod);

        ChatEntry aiEntry = new ChatEntry(AiAvatarImage, "[AI]", @"Hi I'm an LLM-powered AI. If SIXEL isn't available, you'll just see clean fallback text.
Either way, your 24-bit colors continue to work everywhere.", Color.GreenYellow, Color.LightGreen);


        userEntry.WriteToConsole();
        Console.WriteLine();
        botEntry.WriteToConsole();
        Console.WriteLine();
        aiEntry.WriteToConsole();
        Console.WriteLine();

        Console.ForegroundColor = Color.White;
    }
}
