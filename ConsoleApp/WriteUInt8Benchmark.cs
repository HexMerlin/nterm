using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ConsoleApp;

/// <summary>
/// Benchmarks comparing lookup table vs arithmetic operations for WriteUInt8.
/// Tests whether pre-computed arrays provide performance benefits over CPU arithmetic.
/// </summary>
public static class WriteUInt8Benchmark
{
    // Original arithmetic-based implementation
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteUInt8Arithmetic(byte value, Span<byte> dest)
    {
        if (value < 10) { dest[0] = (byte)('0' + value); return 1; }
        if (value < 100)
        {
            dest[0] = (byte)('0' + (value / 10));
            dest[1] = (byte)('0' + (value % 10));
            return 2;
        }
        int hundreds = value / 100;
        int rem = value - hundreds * 100;
        dest[0] = (byte)('0' + hundreds);
        dest[1] = (byte)('0' + (rem / 10));
        dest[2] = (byte)('0' + (rem % 10));
        return 3;
    }

    // Lookup table implementation (current optimized version)
    private static readonly byte[][] ByteToAsciiLookup = InitializeByteLookup();
    private static readonly byte[] ByteLengthLookup = InitializeLengthLookup();

    private static byte[][] InitializeByteLookup()
    {
        byte[][] lookup = new byte[256][];
        for (int i = 0; i <= 255; i++)
        {
            if (i < 10)
            {
                lookup[i] = [(byte)('0' + i)];
            }
            else if (i < 100)
            {
                lookup[i] = [(byte)('0' + (i / 10)), (byte)('0' + (i % 10))];
            }
            else
            {
                int hundreds = i / 100;
                int rem = i - hundreds * 100;
                lookup[i] = [(byte)('0' + hundreds), (byte)('0' + (rem / 10)), (byte)('0' + (rem % 10))];
            }
        }
        return lookup;
    }

    private static byte[] InitializeLengthLookup()
    {
        byte[] lookup = new byte[256];
        for (int i = 0; i <= 255; i++)
        {
            lookup[i] = i < 10 ? (byte)1 : i < 100 ? (byte)2 : (byte)3;
        }
        return lookup;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int WriteUInt8Lookup(byte value, Span<byte> dest)
    {
        ReadOnlySpan<byte> bytes = ByteToAsciiLookup[value];
        bytes.CopyTo(dest);
        return ByteLengthLookup[value];
    }

    /// <summary>
    /// Results from WriteUInt8 performance comparison.
    /// </summary>
    public record BenchmarkResults(
        double ArithmeticMs,
        double LookupMs,
        double ArithmeticNsPerOp,
        double LookupNsPerOp,
        bool ArithmeticFaster,
        double SpeedupFactor,
        string Recommendation);

    /// <summary>
    /// Executes comprehensive benchmark comparing arithmetic vs lookup table approaches.
    /// </summary>
    public static BenchmarkResults RunBenchmark()
    {
        const int iterations = 2_000_000;
        const int warmupIterations = 50_000;

        Console.WriteLine($"Testing {iterations:N0} byte-to-ASCII conversions...");
        Console.WriteLine();

        // Create test data - all possible byte values distributed randomly
        byte[] testValues = new byte[iterations];
        Random rng = new(42); // Fixed seed for reproducible results
        for (int i = 0; i < iterations; i++)
        {
            testValues[i] = (byte)rng.Next(0, 256);
        }

        Span<byte> buffer = stackalloc byte[4]; // Max 3 digits + safety

        // ======================
        // Test 1: Arithmetic Implementation
        // ======================
        Console.WriteLine("🔢 Arithmetic Implementation (Division/Modulo):");

        // Warmup
        for (int i = 0; i < warmupIterations; i++)
        {
            WriteUInt8Arithmetic(testValues[i % testValues.Length], buffer);
        }

        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            WriteUInt8Arithmetic(testValues[i], buffer);
        }
        sw.Stop();

        long arithmeticTicks = sw.ElapsedTicks;
        double arithmeticMs = sw.Elapsed.TotalMilliseconds;
        double arithmeticNsPerOp = sw.Elapsed.TotalNanoseconds / iterations;
        
        Console.WriteLine($"   Time: {arithmeticMs:F1} ms");
        Console.WriteLine($"   Rate: {iterations / sw.Elapsed.TotalSeconds:N0} ops/sec");
        Console.WriteLine($"   Avg:  {arithmeticNsPerOp:F2} ns per conversion");
        Console.WriteLine();

        // ======================
        // Test 2: Lookup Table Implementation
        // ======================
        Console.WriteLine("📋 Lookup Table Implementation (Pre-computed Arrays):");

        // Warmup
        for (int i = 0; i < warmupIterations; i++)
        {
            WriteUInt8Lookup(testValues[i % testValues.Length], buffer);
        }

        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            WriteUInt8Lookup(testValues[i], buffer);
        }
        sw.Stop();

        long lookupTicks = sw.ElapsedTicks;
        double lookupMs = sw.Elapsed.TotalMilliseconds;
        double lookupNsPerOp = sw.Elapsed.TotalNanoseconds / iterations;
        
        Console.WriteLine($"   Time: {lookupMs:F1} ms");
        Console.WriteLine($"   Rate: {iterations / sw.Elapsed.TotalSeconds:N0} ops/sec");
        Console.WriteLine($"   Avg:  {lookupNsPerOp:F2} ns per conversion");
        Console.WriteLine();

        // ======================
        // Direct Performance Comparison
        // ======================
        Console.WriteLine("🏆 PERFORMANCE COMPARISON:");
        Console.WriteLine(new string('-', 40));

        double speedupFactor = (double)arithmeticTicks / lookupTicks;
        bool arithmeticFaster = arithmeticTicks < lookupTicks;
        string recommendation;

        if (!arithmeticFaster)
        {
            Console.WriteLine($"✅ LOOKUP TABLES WIN: {speedupFactor:F2}x faster");
            Console.WriteLine($"   Performance gain: {(1 - lookupMs / arithmeticMs) * 100:F1}%");
            Console.WriteLine($"   Time saved: {arithmeticMs - lookupMs:F1} ms");
            Console.WriteLine();
            Console.WriteLine("💡 RECOMMENDATION: Keep lookup table optimization");
            recommendation = "Keep lookup table optimization";
        }
        else
        {
            Console.WriteLine($"✅ ARITHMETIC WINS: {1/speedupFactor:F2}x faster");
            Console.WriteLine($"   Lookup overhead: {(lookupMs / arithmeticMs - 1) * 100:F1}%");
            Console.WriteLine($"   Time penalty: {lookupMs - arithmeticMs:F1} ms");
            Console.WriteLine();
            Console.WriteLine("💡 RECOMMENDATION: Revert to arithmetic implementation");
            recommendation = "Revert to arithmetic implementation";
        }
        
        Console.WriteLine($"   Difference: {Math.Abs(arithmeticNsPerOp - lookupNsPerOp):F2} ns per operation");
        Console.WriteLine();

        return new BenchmarkResults(
            ArithmeticMs: arithmeticMs,
            LookupMs: lookupMs,
            ArithmeticNsPerOp: arithmeticNsPerOp,
            LookupNsPerOp: lookupNsPerOp,
            ArithmeticFaster: arithmeticFaster,
            SpeedupFactor: arithmeticFaster ? 1/speedupFactor : speedupFactor,
            Recommendation: recommendation);
    }
}
