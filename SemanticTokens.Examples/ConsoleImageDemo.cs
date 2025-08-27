using SemanticTokens.Core;
using SemanticTokens.Sixel;
using System.Reflection;

namespace SemanticTokens.Examples;

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

        UserAvatarImage = ConsoleImage.FromEmbeddedResource(ImageUser, examplesAssembly, "[👤]", Transparency.Default);

        BotAvatarImage = ConsoleImage.FromEmbeddedResource(ImageBot, examplesAssembly, "[🤖]", Transparency.Default);

        AiAvatarImage = ConsoleImage.FromEmbeddedResource(ImageAI, examplesAssembly, "[🧠]", Transparency.Default);
    }



    /// <summary>
    /// Demonstrates console image streaming text output with progressive rendering.
    /// </summary>
    /// <remarks>
    /// Advanced chat dialog with streaming text, demonstrating Write, WriteLineBreak, and ClearText methods.
    /// Text appears progressively with realistic typing delays to showcase streaming capabilities.
    /// </remarks>
    public async Task RunAsync()
    {
        Console.BackgroundColor = Color.Navy;

        Console.WriteLine();
        Console.WriteLine("=== Streaming Chat Demo ===");
        Console.WriteLine();

        // User message with streaming text
        ChatEntryWriter userEntry = new ChatEntryWriter(UserAvatarImage);
        userEntry.BeginWrite();
        
        await WriteStreamingText(userEntry, "[User] ", Color.Cyan);
        await WriteStreamingText(userEntry, "Hey! Can you show me how ", Color.LightCyan);
        userEntry.WriteLineBreak();
        await WriteStreamingText(userEntry, "streaming text output works? ", Color.LightCyan);
        userEntry.WriteLineBreak();
        await WriteStreamingText(userEntry, "This looks pretty cool!", Color.LightCyan);
        
        userEntry.EndWrite();
        Console.WriteLine();

        // Bot response with different streaming patterns
        ChatEntryWriter botEntry = new ChatEntryWriter(BotAvatarImage);
        botEntry.BeginWrite();
        
        await WriteStreamingText(botEntry, "[Bot] ", Color.OrangeRed);
        await WriteStreamingText(botEntry, "Sure thing! Let me demonstrate...", Color.Goldenrod);
        botEntry.WriteLineBreak();
        
        // Simulate typing delay
        await Task.Delay(300);
        
        botEntry.WriteLineBreak();
        await WriteStreamingText(botEntry, "First, I'll write some text progressively.", Color.Goldenrod);
        botEntry.WriteLineBreak();
        
        // Demonstrate clear and rewrite
        await WriteStreamingText(botEntry, "Wait, let me rephrase that...", Color.DarkOrange);
        await Task.Delay(500);
        botEntry.ClearText();
        
        await WriteStreamingText(botEntry, "[Bot] ", Color.OrangeRed);
        await WriteStreamingText(botEntry, "Perfect! Text appears word by word.", Color.Goldenrod);
        botEntry.WriteLineBreak();
        await WriteStreamingText(botEntry, "Line breaks work seamlessly.", Color.Goldenrod);
        botEntry.WriteLineBreak();
        await WriteStreamingText(botEntry, "And clearing text works too! ✨", Color.LightGreen);
        
        botEntry.EndWrite();
        Console.WriteLine();

        // AI response showcasing different colors and timing
        ChatEntryWriter aiEntry = new ChatEntryWriter(AiAvatarImage);
        aiEntry.BeginWrite();
        
        await WriteStreamingText(aiEntry, "[AI] ", Color.GreenYellow);
        await WriteStreamingText(aiEntry, "Impressive! ", Color.LightGreen, 50);
        await WriteStreamingText(aiEntry, "This streaming approach ", Color.LightGreen, 30);
        aiEntry.WriteLineBreak();
        await WriteStreamingText(aiEntry, "enables real-time chat experiences ", Color.LightGreen, 40);
        aiEntry.WriteLineBreak();
        await WriteStreamingText(aiEntry, "with perfect image-text alignment! 🚀", Color.Lime, 60);
        
        aiEntry.EndWrite();
        Console.WriteLine();

        Console.ForegroundColor = Color.White;
        Console.WriteLine("=== Demo Complete ===");
    }

    /// <summary>
    /// Simple synchronous wrapper for async demo.
    /// </summary>
    public void Run() => RunAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Writes text progressively to simulate streaming/typing effect.
    /// </summary>
    /// <param name="writer">ChatEntryWriter to write to</param>
    /// <param name="text">Text to write progressively</param>
    /// <param name="color">Color for the text</param>
    /// <param name="delayMs">Delay between words in milliseconds</param>
    private static async Task WriteStreamingText(ChatEntryWriter writer, string text, Color color, int delayMs = 20)
    {
        if (string.IsNullOrEmpty(text))
            return;

        // Split into words and write progressively
        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i++)
        {
            writer.Write(words[i], color);
            
            // Add space between words (except for last word)
            if (i < words.Length - 1)
                writer.Write(" ", color);
            
            // Delay between words for streaming effect
            if (delayMs > 0)
                await Task.Delay(delayMs);
        }
    }
}
