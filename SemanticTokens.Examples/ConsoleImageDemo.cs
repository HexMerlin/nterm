using System.Reflection;
using SemanticTokens.Core;
using SemanticTokens.Sixel;

namespace SemanticTokens.Examples;

/// <summary>
/// Perfect console image demonstration using ultra-optimized ConsoleImage API.
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
    private readonly ConsoleImage _userAvatar;
    private readonly ConsoleImage _botAvatar;
    private readonly ConsoleImage _aiAvatar;

    /// <summary>
    /// Constructs demo with pre-encoded console images.
    /// </summary>
    public ConsoleImageDemo()
    {
        Console.WriteLine("[DEBUG] ===== BUILDING CONSOLE IMAGES =====");
        
        // Build perfect console images once during construction
        // Use Examples assembly since that's where the embedded resources are
        Assembly examplesAssembly = typeof(ConsoleImageDemo).Assembly;
        
        Console.WriteLine($"[DEBUG] Building User Avatar with 8x8 character size...");
        _userAvatar = ConsoleImage.FromEmbeddedResource(ImageUser, examplesAssembly)
            .WithCharacterSize(8, 8)
            .WithFallbackText("[👤]")
            .WithTransparency(Transparency.Default)
            .Build();

        Console.WriteLine($"[DEBUG] Building Bot Avatar with 8x8 character size...");
        _botAvatar = ConsoleImage.FromEmbeddedResource(ImageBot, examplesAssembly)
            .WithCharacterSize(8, 8)
            .WithFallbackText("[🤖]")
            .WithTransparency(Transparency.Default)
            .Build();

        Console.WriteLine($"[DEBUG] Building AI Avatar with 8x8 character size...");
        _aiAvatar = ConsoleImage.FromEmbeddedResource(ImageAI, examplesAssembly)
            .WithCharacterSize(8, 8)
            .WithFallbackText("[🧠]")
            .WithTransparency(Transparency.Default)
            .Build();
            
        Console.WriteLine("[DEBUG] ===== ALL CONSOLE IMAGES BUILT =====");
    }

    /// <summary>
    /// Demonstrates perfect console image usage.
    /// </summary>
    /// <remarks>
    /// Ultra-clean chat interface replacing all ugly cursor manipulation with
    /// simple Console.WriteImage() calls. Single execution path with automatic
    /// SIXEL/fallback handling.
    /// </remarks>
    public void Run()
    {
        Console.WriteLine();
        Console.WriteLine("=== Perfect Console Image Demo ===");
        Console.WriteLine();

        PrintChatMessage(
            avatar: _userAvatar,
            speakerLabel: "User",
            speakerColor: new Color(200, 255, 160),
            text: @"Hey! Could you summarize today's headlines? Also—can you show avatars inline?
Let's make sure text aligns neatly with those images."
        );

        PrintChatMessage(
            avatar: _botAvatar,
            speakerLabel: "Bot", 
            speakerColor: new Color(160, 200, 255),
            text: @"Sure thing! I'll format the output with bullets and keep it short. 
If your terminal supports SIXEL, you should see avatars on the left."
        );

        PrintChatMessage(
            avatar: _aiAvatar,
            speakerLabel: "AI",
            speakerColor: new Color(255, 200, 160),
            text: @"I'm the LLM-powered one. If SIXEL isn't available, you'll just see clean fallback text.
Either way, your 24-bit colors continue to work everywhere."
        );

        Console.WriteLine();
        Console.WriteLine("=== Terminal Capabilities ===");
        Console.WriteLine($"SIXEL Support: {SixelCapabilities.IsSupported}");
        Console.WriteLine($"Cell Size: {SixelCapabilities.CellSize.Width}x{SixelCapabilities.CellSize.Height}px");
        Console.WriteLine($"Window Size: {SixelCapabilities.WindowCharacterSize.Width}x{SixelCapabilities.WindowCharacterSize.Height} chars");
        Console.WriteLine();
    }

    /// <summary>
    /// Perfect chat message rendering with ultra-clean API.
    /// </summary>
    /// <param name="avatar">Pre-encoded console image</param>
    /// <param name="speakerLabel">Speaker name</param>
    /// <param name="speakerColor">Speaker name color</param>
    /// <param name="text">Message text</param>
    /// <remarks>
    /// Single execution path replacing hundreds of lines of ugly cursor manipulation.
    /// Authority-driven: ConsoleImage handles encoding, Console handles output.
    /// </remarks>
    private static void PrintChatMessage(ConsoleImage avatar, string speakerLabel, SemanticTokens.Core.Color speakerColor, string text)
    {
        // Perfect single-line image output
        Console.WriteImage(avatar.ConsoleData);
        Console.Write(" ");
        
        // Perfect colored speaker label
        Console.Write(speakerLabel, speakerColor, SemanticTokens.Core.Color.Black);
        Console.Write(": ");
        
        // Perfect text output
        Console.WriteLine(text);
        Console.WriteLine();
    }

}
