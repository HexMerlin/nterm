using System.Diagnostics;
using System.Globalization;
using System.Buffers;
using SemanticTokens.Core;

namespace SemanticTokens.Sixel;

/// <summary>
/// Terminal capability detection with caching and performance.
/// Single authority for SIXEL support determination and terminal characteristics.
/// </summary>
public static class TerminalCapabilities
{
    private static bool? _isSupported;
    private static Size? _cellSize;

    /// <summary>
    /// Terminal SIXEL support.
    /// </summary>
    /// <returns><see langword="true"/> <b>iff</b> terminal supports SIXEL graphics</returns>
    public static bool IsSixelSupported => _isSupported ??= DetectSixelSupport();

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
        Console.WindowWidth, 
        Console.WindowHeight
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
            return forced;
        }

        try
        {
            // Query device attributes: ESC[c
            ReadOnlySpan<char> response = QueryTerminal(Constants.DeviceAttributesQuery);
            
            // Traditional SIXEL support indicated by parameter "4" in response
            bool hasTraditionalSupport = response.Contains(";4;", StringComparison.Ordinal) ||
                                        response.EndsWith(";4", StringComparison.Ordinal);
            
            // Modern Windows Terminal with SIXEL support often reports parameter "61" and others
            // Let's check for modern terminal patterns that likely support SIXEL
            bool hasModernSupport = response.Contains("61", StringComparison.Ordinal) && 
                                   response.Contains("24", StringComparison.Ordinal); // Common in modern terminals
            
            bool hasSupport = hasTraditionalSupport || hasModernSupport;
            
            return hasSupport;
        }
        catch
        {
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
            ReadOnlySpan<char> response = QueryTerminal(Constants.CellPixelSizeQuery);
            
            // Expected format: [6;height;width;t
            Span<Range> ranges = stackalloc Range[4];
            if (response.Split(ranges, ';') >= 3)
            {
                int height = int.Parse(response[ranges[1]], CultureInfo.InvariantCulture);
                int width = int.Parse(response[ranges[2]], CultureInfo.InvariantCulture);
                Size detectedSize = new Size(width, height);
                return detectedSize;
            }
        }
        catch
        {
        }

        // Default Windows Terminal cell size
        return new Size(10, 20);
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
            ReadOnlySpan<char> response = QueryTerminal(Constants.WindowPixelSizeQuery);
            
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
            ReadOnlySpan<char> response = QueryTerminal(Constants.SyncSupportQuery, 'y');
            
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
    /// Optimized terminal query with timeout.
    /// </summary>
    /// <param name="query">Control sequence to send (without ESC prefix)</param>
    /// <param name="endChars">Response termination characters</param>
    /// <returns>Terminal response excluding control characters</returns>
    public static ReadOnlySpan<char> QueryTerminal(string query, params char[] endChars)
    {
        // Default terminator = last char of query (e.g., 't' for "[14t")
        char defaultTerm = query.Length > 0 ? query[^1] : 't';
        char term = (endChars is { Length: > 0 }) ? endChars[0] : defaultTerm;

        char[] buffer = ArrayPool<char>.Shared.Rent(64);
        int length = 0;

        try
        {
            // Send ESC + query (e.g., "\x1b[14t")
            Console.Write(Constants.ESC);
            Console.Write(query);

            // Collect until we see the terminator or timeout
            Stopwatch sw = Stopwatch.StartNew();
            SpinWait spinner = new SpinWait();

            while (sw.ElapsedMilliseconds < 50)
            {
                if (!System.Console.KeyAvailable)
                {
                    // cheaper than Thread.Sleep(1) in short waits
                    spinner.SpinOnce();
                    continue;
                }

                // Read one char without echo
                char ch = Console.ReadKey(intercept: true).KeyChar;

                if (ch == term)
                    break;

                // Keep only visible payload (digits, ';', '[', etc.). Drop ESC and other controls.
                if (!char.IsControl(ch))
                {
                    if (length == buffer.Length)
                    {
                        char[] bigger = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                        buffer.AsSpan(0, length).CopyTo(bigger);
                        ArrayPool<char>.Shared.Return(buffer);
                        buffer = bigger;
                    }
                    buffer[length++] = ch;
                }
            }
        }
        catch
        {
            // Swallow and return empty
        }

        // Materialize to immutable string, then return span over it
        string result = length == 0 ? string.Empty : new string(buffer, 0, length);
        ArrayPool<char>.Shared.Return(buffer);
        return result.AsSpan();
    }

}
