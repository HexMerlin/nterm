namespace NTerm.Core;

public static class DiscreteScroller
{
    private static readonly Lock writeLock = new();

    private static readonly Lock scrollLock = new();

    private static readonly string Notice = BuildNotice();

    /// <summary>
    /// Ensures there is sufficient empty space (headroom) below the cursor for upcoming output.
    /// If there is not enough space, performs a discrete, page-like scroll using natural newline output
    /// (SIXEL/graphics-friendly), repositions the cursor to the top of the fresh area, and prints a
    /// professional one-line notice (e.g., “Earlier output above — scroll up to see more”).
    /// </summary>
    /// <returns>
    /// <c>true</c> if a discrete scroll occurred and a notice was written; <c>false</c> if sufficient headroom
    /// already existed and nothing was changed.
    /// </returns>
    /// <remarks>
    /// This method is the “no-hassle” version: call it before writing a new chunk of streaming output.
    /// It avoids jittery line-by-line scrolling and preserves advanced console graphics (e.g., SIXEL)
    /// by relying on the console’s natural scroll (writing newlines) rather than buffer moves.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Streaming loop
    /// if (DiscreteScroller.EnsureHeadroom())
    /// {
    ///     // A discrete scroll happened; the notice is already printed at the top.
    /// }
    /// Terminal.WriteLine(bigChunkOfText);
    /// </code>
    /// </example>
    public static bool EnsureHeadroom()
    {
        lock (scrollLock)
        {
            const int DefaultHeadroom = 10; // sensible default for streaming output
            bool didScroll = EnsureHeadroom(DefaultHeadroom, 0);
            if (didScroll)
                Terminal.WriteLine(Notice, Color.DarkGray);
            return didScroll;
        }
    }

    /// <summary>
    /// Ensures there is at least <paramref name="HeadroomRows"/> free lines below the cursor.
    /// If not, performs a discrete scroll by emitting enough newlines to create a clean blank region
    /// and then repositions the cursor to <paramref name="TargetTopFromWindowTop"/> within the visible window.
    /// </summary>
    /// <param name="HeadroomRows">
    /// The minimum number of free lines required below the cursor before writing more content.
    /// If the remaining visible lines are fewer than this value, a discrete scroll is triggered.
    /// </param>
    /// <param name="TargetTopFromWindowTop">
    /// The row (0-based) from the top of the visible window where the cursor should land after a discrete scroll.
    /// Use <c>0</c> to start the next chunk at the very top; use a larger number to leave context above.
    /// </param>
    /// <returns>
    /// <c>true</c> if a discrete scroll occurred; <c>false</c> if there was already sufficient headroom
    /// and no changes were made.
    /// </returns>
    /// <remarks>
    /// This “manual” overload gives you precise control over when to scroll and where to place the cursor afterwards.
    /// Scrolling is implemented with newline bursts (not <c>Console.MoveBufferArea</c>) to play nicely with terminals
    /// that render images/graphics (e.g., SIXEL), preserving prior content in scrollback.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Require at least 12 free lines; after a scroll, land 4 rows from the top:
    /// if (DiscreteScroller.EnsureHeadroom(HeadroomRows: 12, TargetTopFromWindowTop: 4))
    /// {
    ///     Terminal.WriteLine("— scroll up to see more —");
    /// }
    /// Console.WriteLine(nextChunk);
    /// </code>
    /// </example>
    public static bool EnsureHeadroom(int HeadroomRows, int TargetTopFromWindowTop = 0)
    {
        lock (writeLock)
        {
            int windowHeight = Math.Max(1, Terminal.WindowHeight);
            int windowTop = Terminal.WindowTop;
            int bufferHeight = Math.Max(windowHeight, Terminal.BufferHeight);
            int bottomVisible = windowTop + windowHeight - 1;

            int cursorTop = Math.Min(Terminal.CursorTop, bufferHeight - 1);
            int rowsFromBottom = bottomVisible - cursorTop;

            if (rowsFromBottom >= HeadroomRows)
                return false;

            int targetInWin = Math.Clamp(TargetTopFromWindowTop, 0, windowHeight - 1);
            int linesToWrite = Math.Max(1, windowHeight - targetInWin); // create a blank zone

            Terminal.Write(new string('\n', linesToWrite));

            int newWindowTop = Terminal.WindowTop;
            int targetTop = Math.Min(
                newWindowTop + targetInWin,
                Math.Max(0, Terminal.BufferHeight - 1)
            );

            try
            {
                Terminal.SetCursorPosition(0, targetTop);
            }
            catch
            { /* redirected output etc. */
            }
            return true;
        }
    }

    /// <summary>
    /// Forces a clean “new page” for the console output: triggers a discrete scroll regardless of current headroom
    /// and prints the standard notice at the top of the fresh area. Use this to start a new section and push prior
    /// clutter out of view while keeping it available in scrollback.
    /// </summary>
    /// <remarks>
    /// Implemented using natural newline scrolling (SIXEL/graphics-friendly). The cursor is repositioned to the top
    /// of the new page so you can immediately write the next content block beneath the notice.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Start a fresh page before a major section:
    /// DiscreteScroller.NewPage();
    /// Terminal.WriteLine("== Build Summary ==");
    /// // ...write the section...
    /// </code>
    /// </example>
    public static void NewPage()
    {
        lock (scrollLock)
        {
            // Using WindowHeight as Headroom guarantees at least one page scroll.
            _ = EnsureHeadroom(Math.Max(1, Terminal.WindowHeight), 0);
            Terminal.WriteLine(Notice);
        }
    }

    private static string BuildNotice()
    {
        bool unicodeOk = UnicodeFriendly();
        char arrow = unicodeOk ? '▲' : '^';
        _ = unicodeOk ? '─' : '-';
        string text = "Earlier output above — scroll up to see more";
        return $"                           {arrow}  {text}  {arrow}  ";
    }

    private static bool UnicodeFriendly()
    {
        try
        {
            return !System.Console.IsOutputRedirected;
        }
        catch
        {
            return false;
        }
    }
}
