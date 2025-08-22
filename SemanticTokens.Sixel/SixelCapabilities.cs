using System.Globalization;
using System.Text;
using SixLabors.ImageSharp;

namespace SemanticTokens.Sixel;

/// <summary>
/// Terminal capability detection with caching and ultra-performance.
/// Single authority for SIXEL support determination and terminal characteristics.
/// </summary>
public static class SixelCapabilities
{
    private static bool? _isSupported;
    private static Size? _cellSize;

    /// <summary>
    /// Terminal SIXEL support.
    /// </summary>
    /// <returns><see langword="true"/> <b>iff</b> terminal supports SIXEL graphics</returns>
    public static bool IsSupported => _isSupported ??= DetectSixelSupport();

    /// <summary>
    /// Terminal cell dimensions in pixels.
    /// </summary>
    /// <returns>Character cell size in pixels</returns>
    public static Size CellSize => _cellSize ??= DetectCellSize();

    /// <summary>
    /// Terminal window size in pixels.
    /// </summary>
    /// <returns>Window dimensions in pixels</returns>
    public static Size WindowPixelSize => DetectWindowPixelSize();

    /// <summary>
    /// Terminal window size in characters.
    /// </summary>
    /// <returns>Window dimensions in character cells</returns>
    public static Size WindowCharacterSize => new(
        SemanticTokens.Core.Console.WindowWidth, 
        SemanticTokens.Core.Console.WindowHeight
    );

    /// <summary>
    /// Terminal synchronized output support.
    /// </summary>
    /// <returns><see langword="true"/> <b>iff</b> terminal supports synchronized output updates</returns>
    public static bool IsSyncSupported => DetectSyncSupported();

    /// <summary>
    /// Detects SIXEL support via device attributes query.
    /// </summary>
    /// <returns><see langword="true"/> <b>iff</b> terminal responds with SIXEL capability</returns>
    private static bool DetectSixelSupport()
    {
        // Manual override for testing - check environment variable
        string? forceSupport = Environment.GetEnvironmentVariable("FORCE_SIXEL_SUPPORT");
        if (!string.IsNullOrEmpty(forceSupport) && bool.TryParse(forceSupport, out bool forced))
        {
            SemanticTokens.Core.Console.WriteLine($"[DEBUG] SIXEL support forced via environment: {forced}");
            return forced;
        }

        try
        {
            // Query device attributes: ESC[c
            ReadOnlySpan<char> response = QueryTerminal(Constants.DeviceAttributesQuery);
            
            // Debug: show what we got back
            SemanticTokens.Core.Console.WriteLine($"[DEBUG] Terminal device attributes response: '{response.ToString()}'");
            
            // Traditional SIXEL support indicated by parameter "4" in response
            bool hasTraditionalSupport = response.Contains(";4;", StringComparison.Ordinal) ||
                                        response.EndsWith(";4", StringComparison.Ordinal);
            
            // Modern Windows Terminal with SIXEL support often reports parameter "61" and others
            // Let's check for modern terminal patterns that likely support SIXEL
            bool hasModernSupport = response.Contains("61", StringComparison.Ordinal) && 
                                   response.Contains("24", StringComparison.Ordinal); // Common in modern terminals
            
            bool hasSupport = hasTraditionalSupport || hasModernSupport;
            
            SemanticTokens.Core.Console.WriteLine($"[DEBUG] SIXEL support detected: {hasSupport} (traditional: {hasTraditionalSupport}, modern: {hasModernSupport})");
            return hasSupport;
        }
        catch (Exception ex)
        {
            SemanticTokens.Core.Console.WriteLine($"[DEBUG] SIXEL detection failed: {ex.Message}");
            return false; // Assume no SIXEL support on any query failure
        }
    }

    /// <summary>
    /// Detects terminal cell size in pixels.
    /// </summary>
    /// <returns>Cell dimensions in pixels</returns>
    private static Size DetectCellSize()
    {
        try
        {
            // Query cell size: ESC[16t
            ReadOnlySpan<char> response = QueryTerminal("[16t");
            SemanticTokens.Core.Console.WriteLine($"[DEBUG] Cell size query response: '{response.ToString()}'");
            
            // Expected format: [6;height;width;t
            Span<Range> ranges = stackalloc Range[4];
            if (response.Split(ranges, ';') >= 3)
            {
                int height = int.Parse(response[ranges[1]], CultureInfo.InvariantCulture);
                int width = int.Parse(response[ranges[2]], CultureInfo.InvariantCulture);
                Size detectedSize = new Size(width, height);
                SemanticTokens.Core.Console.WriteLine($"[DEBUG] Cell size detected from terminal: {width}x{height} pixels");
                return detectedSize;
            }
        }
        catch (Exception ex)
        {
            SemanticTokens.Core.Console.WriteLine($"[DEBUG] Cell size detection failed: {ex.Message}");
        }

        // Default Windows Terminal cell size
        Size defaultSize = new Size(10, 20);
        SemanticTokens.Core.Console.WriteLine($"[DEBUG] Using default cell size: {defaultSize.Width}x{defaultSize.Height} pixels");
        return defaultSize;
    }

    /// <summary>
    /// Detects terminal window size in pixels.
    /// </summary>
    /// <returns>Window dimensions in pixels</returns>
    private static Size DetectWindowPixelSize()
    {
        try
        {
            // Query window pixel size: ESC[14t
            ReadOnlySpan<char> response = QueryTerminal("[14t");
            
            // Expected format: [4;height;width;t
            Span<Range> ranges = stackalloc Range[4];
            if (response.Split(ranges, ';') >= 3)
            {
                int height = int.Parse(response[ranges[1]], CultureInfo.InvariantCulture);
                int width = int.Parse(response[ranges[2]], CultureInfo.InvariantCulture);
                return new Size(width, height);
            }
        }
        catch
        {
            // Fall through to calculated size
        }

        // Calculate from character size and cell size
        Size charSize = WindowCharacterSize;
        Size cellSize = CellSize;
        return new Size(charSize.Width * cellSize.Width, charSize.Height * cellSize.Height);
    }

    /// <summary>
    /// Detects synchronized output support.
    /// </summary>
    /// <returns><see langword="true"/> <b>iff</b> terminal supports synchronized output updates</returns>
    private static bool DetectSyncSupported()
    {
        try
        {
            // Query synchronized output support: ESC[?2026$p
            ReadOnlySpan<char> response = QueryTerminal("[?2026$p", 'y');
            
            // Synchronized output support indicated by "1" in response
            // Expected format: [?2026;1$y where "1" indicates support
            return !response.IsEmpty && !response.Contains("2026;0$", StringComparison.Ordinal);
        }
        catch
        {
            return false; // Assume no sync support on any query failure
        }
    }

    /// <summary>
    /// Ultra-optimized terminal query with timeout.
    /// </summary>
    /// <param name="query">Control sequence to send (without ESC prefix)</param>
    /// <param name="endChars">Response termination characters</param>
    /// <returns>Terminal response excluding control characters</returns>
    private static ReadOnlySpan<char> QueryTerminal(string query, params char[] endChars)
    {
        char[] defaultEndChars = [query[^1]]; // Use last character as default terminator
        char[] terminators = endChars.Length > 0 ? endChars : defaultEndChars;
        
        var response = new StringBuilder(64);

        try
        {
            // Send query with 100ms timeout
            using var cts = new CancellationTokenSource(100);
            
            SemanticTokens.Core.Console.Write($"\x1B{query}");
            
            // Collect response until terminator or timeout
            DateTime start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < 100)
            {
                if (!SemanticTokens.Core.Console.KeyAvailable)
                {
                    Thread.Sleep(1);
                    continue;
                }

                char ch = SemanticTokens.Core.Console.ReadKey(intercept: true).KeyChar;
                
                if (Array.IndexOf(terminators, ch) >= 0)
                    break;
                    
                if (!char.IsControl(ch))
                    response.Append(ch);
            }
        }
        catch
        {
            // Return empty on any failure
        }

        return response.ToString().AsSpan();
    }
}
