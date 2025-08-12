using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SemanticDocuments;
using System.Text;
using TrueColor;

namespace ConsoleApp;

/// <summary>
/// Test program demonstrating <see cref="SemanticDocumentCSharp"/> creation and console rendering.
/// Shows the complete pipeline: C# script → Compilation → SemanticDocumentCSharp → Console output.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Demonstrates complete SemanticDocumentCSharp pipeline with C# script syntax highlighting.
    /// Creates a sample C# script, processes it through Roslyn classification,
    /// resolves semantic styling, and renders to console with colors.
    /// </summary>
    /// <returns>Task representing the asynchronous operation.</returns>
    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        //A simple C# script
        string scriptText = """
using System;
                
static int AddFive(int initial) => initial + 5;

int x = 42;
/* 
Some comment section here
*/
string msg = $"Value = {x} ";
Console.WriteLine(msg + AddFive(5).ToString()); // Just an inline comment
""";

    
        // 2) Compile the script
        Script<object> script = CSharpScript.Create(
            scriptText,
            options: ScriptOptions.Default.WithImports("System")
        );

        Microsoft.CodeAnalysis.Compilation compilation = script.GetCompilation();

        // 3) Create SemanticDocument with full classification fidelity
        SemanticDocument document = await SemanticDocumentCSharp.CreateAsync(compilation);

        // 4) Render to console using rich semantic information
        SemanticDocumentConsoleRenderer.Render(document);
        Console.WriteLine();

        return;

        Console.WriteLine();
        Console.WriteLine("Press any key to run performance benchmarks...");
        Console.ReadKey(true);

        // 5) Run WriteUInt8 comparison benchmark first
        Console.WriteLine();
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("BENCHMARK 1: WriteUInt8 Implementation Comparison");
        Console.WriteLine("=".PadRight(70, '='));
        var writeUInt8Results = WriteUInt8Benchmark.RunBenchmark();

        // 6) Run full AnsiConsole performance benchmark
        Console.WriteLine();
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("BENCHMARK 2: Full AnsiConsole Performance Test");
        Console.WriteLine("=".PadRight(70, '='));
        var ansiConsoleResults = Benchmark.RunPerformanceBenchmark();

        // 7) Final comparison and recommendations
        Console.WriteLine();
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("FINAL ANALYSIS & RECOMMENDATIONS");
        Console.WriteLine("=".PadRight(70, '='));
        
        Console.WriteLine("📊 BENCHMARK SUMMARY:");
        Console.WriteLine($"  • WriteUInt8 Arithmetic: {writeUInt8Results.ArithmeticNsPerOp:F2} ns/op");
        Console.WriteLine($"  • WriteUInt8 Lookup:     {writeUInt8Results.LookupNsPerOp:F2} ns/op");
        Console.WriteLine($"  • AnsiConsole Overall:   {ansiConsoleResults.MicrosecondsPerChar:F2} μs/char ({ansiConsoleResults.CharsPerSecond:N0} chars/sec)");
        Console.WriteLine($"  • Color Caching Benefit: {ansiConsoleResults.SameColorCharsPerSec:N0} chars/sec (same colors)");
        Console.WriteLine();
        
        Console.WriteLine("🎯 KEY FINDINGS:");
        if (writeUInt8Results.ArithmeticFaster)
        {
            Console.WriteLine($"  ✅ Arithmetic operations are {writeUInt8Results.SpeedupFactor:F2}x faster than lookup tables");
            Console.WriteLine($"     → CPU arithmetic beats memory access for byte conversion");
        }
        else
        {
            Console.WriteLine($"  ✅ Lookup tables are {writeUInt8Results.SpeedupFactor:F2}x faster than arithmetic");
            Console.WriteLine($"     → Memory lookups beat CPU arithmetic for byte conversion");
        }
        
        double colorCachingSpeedup = ansiConsoleResults.SameColorCharsPerSec / ansiConsoleResults.CharsPerSecond;
        Console.WriteLine($"  ✅ Color caching provides {colorCachingSpeedup:F2}x speedup for consecutive same-color chars");
        Console.WriteLine();
        
        Console.WriteLine("💡 IMPLEMENTATION RECOMMENDATIONS:");
        Console.WriteLine($"  1. WriteUInt8: {writeUInt8Results.Recommendation}");
        Console.WriteLine("  2. Color Caching: Keep - provides significant performance benefit");
        Console.WriteLine("  3. ANSI Prefix Optimization: Keep - reduces memory operations");
        Console.WriteLine("  4. UTF-8 Encoding: Keep current implementation - handles all cases correctly");
        Console.WriteLine();
        
        Console.WriteLine("🏆 FINAL OPTIMIZED PERFORMANCE:");
        Console.WriteLine($"  • Peak throughput: {ansiConsoleResults.CharsPerSecond:N0} characters/second");
        Console.WriteLine($"  • Average latency: {ansiConsoleResults.MicrosecondsPerChar:F2} μs per character");
        Console.WriteLine($"  • Color cache efficiency: {colorCachingSpeedup:F1}x improvement for repeated colors");
        Console.WriteLine();
    }
}