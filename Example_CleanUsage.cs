// Example: Beautiful Clean Usage of ConsoleImage
using SemanticTokens.Core;
using SemanticTokens.Sixel;

namespace Example;

class CleanConsoleImageDemo
{
    static void Main()
    {
        // ✅ Beautiful, clean user experience:
        
        // 1. Load image with fluent builder pattern
        ConsoleImage avatar = ConsoleImageBuilder
            .FromFile("avatar.png")
            .WithCharacterSize(8, 8)
            .WithFallbackText("[👤]")
            .WithTransparency(Transparency.Default)
            .Build();

        // 2. Extract console-ready data (pure data container)
        ReadOnlySpan<char> imageData = avatar.ConsoleData;
        
        // 3. Use Core Console.WriteImage() - user has full control
        Console.WriteImage(imageData);
        
        // Alternative: User can manipulate data before output
        string processedData = ModifyImageData(imageData);
        Console.WriteImage(processedData);
        
        // Show clean properties
        Console.WriteLine($"Image: {avatar.DisplaySize}");
        Console.WriteLine($"Optimized: {avatar.HasOptimizedEncoding}");
    }
    
    static string ModifyImageData(ReadOnlySpan<char> data)
    {
        // User has complete flexibility to process image data
        // Could add borders, modify colors, combine with text, etc.
        return data.ToString();
    }
}

/* 
🎯 ARCHITECTURAL BENEFITS ACHIEVED:

✅ Clean Separation of Concerns:
   - ConsoleImage = Pure data container (Sixel project)
   - ConsoleImageBuilder = Complex encoding logic (Sixel project)  
   - Console.WriteImage() = Output responsibility (Core project)

✅ No Circular Dependencies:
   User Code → Core (Console.WriteImage)
        ↓
        └→ Sixel (ConsoleImageBuilder)
             ↓
             └→ Core (Color types)

✅ User Control & Flexibility:
   - Extract Span<char> for any use case
   - No framework lock-in
   - Perfect for library design

✅ Gold Standard User Experience:
   - ConsoleImage is now truly "Core-worthy"
   - Clean, readable, maintainable
   - Zero SIXEL-specific bloat in the data type
*/
