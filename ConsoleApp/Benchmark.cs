using System.Diagnostics;
using TrueColor;

namespace ConsoleApp;

/// <summary>
/// Simple benchmark demonstrating AnsiConsole performance optimizations.
/// Measures character-per-second throughput for console writing operations.
/// </summary>
public static class Benchmark
{
    /// <summary>
    /// Results from full AnsiConsole performance benchmark.
    /// </summary>
    public record AnsiConsoleBenchmarkResults(
        long CharactersWritten,
        double TotalTimeMs,
        double CharsPerSecond,
        double MicrosecondsPerChar,
        long SameColorChars,
        double SameColorTimeMs,
        double SameColorCharsPerSec);

    /// <summary>
    /// Executes performance benchmark measuring optimized AnsiConsole throughput.
    /// </summary>
    /// <remarks>
    /// Tests realistic syntax highlighting scenario with alternating colors to demonstrate:
    /// 1. Color caching efficiency when consecutive characters share colors
    /// 2. Lookup table performance for RGB component conversion
    /// 3. Optimized ANSI sequence construction
    /// </remarks>
    public static AnsiConsoleBenchmarkResults RunPerformanceBenchmark()
    {
        const int iterations = 50_000;
        const string testText = "Hello, World! This is a performance test for AnsiConsole optimization.";
        
        // Define colors that would typically be used in syntax highlighting
        Color[] foregroundColors = 
        [
            new(220, 220, 220), // Light gray (normal text)
            new(86, 156, 214),  // Blue (keywords)
            new(206, 145, 120), // Orange (strings)
            new(78, 201, 176),  // Green (comments)
            new(220, 220, 170)  // Yellow (identifiers)
        ];
        
        Color backgroundColor = new(30, 30, 30); // Dark background
        
        Console.WriteLine("AnsiConsole Performance Benchmark");
        Console.WriteLine("=================================");
        Console.WriteLine($"Writing {iterations:N0} characters with color changes...");
        Console.WriteLine();
        
        // Warm up
        AnsiConsole.SyncWithConsoleColors();
        for (int i = 0; i < 1000; i++)
        {
            char ch = testText[i % testText.Length];
            Color fg = foregroundColors[i % foregroundColors.Length];
            AnsiConsole.Write(ch, fg, backgroundColor);
        }
        
        // Reset cache and measure performance
        AnsiConsole.SyncWithConsoleColors();
        
        Stopwatch sw = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            char ch = testText[i % testText.Length];
            Color fg = foregroundColors[i % foregroundColors.Length];
            AnsiConsole.Write(ch, fg, backgroundColor);
        }
        
        sw.Stop();
        
        double totalTimeMs = sw.Elapsed.TotalMilliseconds;
        double charsPerSecond = iterations / sw.Elapsed.TotalSeconds;
        double microsecondsPerChar = sw.Elapsed.TotalMicroseconds / iterations;
        
        Console.WriteLine();
        Console.WriteLine($"Completed: {iterations:N0} characters in {totalTimeMs:N0} ms");
        Console.WriteLine($"Throughput: {charsPerSecond:N0} characters/second");
        Console.WriteLine($"Average: {microsecondsPerChar:F2} μs per character");
        
        // Demonstrate color caching efficiency
        Console.WriteLine();
        Console.WriteLine("Color Caching Efficiency Test:");
        Console.WriteLine("==============================");
        
        const int sameColorIterations = 10_000;
        Color sameColor = new(100, 150, 200);
        
        // Test with same colors (should benefit from caching)
        AnsiConsole.SyncWithConsoleColors();
        sw.Restart();
        
        for (int i = 0; i < sameColorIterations; i++)
        {
            AnsiConsole.Write('X', sameColor, backgroundColor);
        }
        
        sw.Stop();
        
        double sameColorTimeMs = sw.Elapsed.TotalMilliseconds;
        double sameColorCharsPerSec = sameColorIterations / sw.Elapsed.TotalSeconds;
        
        Console.WriteLine($"Same colors: {sameColorIterations:N0} chars in {sameColorTimeMs:N0} ms ({sameColorCharsPerSec:N0} chars/sec)");
        
        AnsiConsole.RestoreOriginalColors();

        return new AnsiConsoleBenchmarkResults(
            CharactersWritten: iterations,
            TotalTimeMs: totalTimeMs,
            CharsPerSecond: charsPerSecond,
            MicrosecondsPerChar: microsecondsPerChar,
            SameColorChars: sameColorIterations,
            SameColorTimeMs: sameColorTimeMs,
            SameColorCharsPerSec: sameColorCharsPerSec);
    }
}
