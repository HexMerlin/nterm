using TrueColor;

namespace Controls;

/// <summary>
/// Represents the state of the console that can be restored later.
/// </summary>
public readonly struct ConsoleState : IDisposable
{
    public readonly Color OriginalForeground { get; }
    public readonly Color OriginalBackground { get; }
    public readonly int OriginalCursorLeft { get; }
    public readonly int OriginalCursorTop { get; }
    private readonly bool OriginalCursorVisible { get; } = false;
    private readonly bool CursorVisibilitySupported { get; } = false;

    public ConsoleState()
    {
        OriginalForeground = AnsiConsole.ForegroundColor;
        OriginalBackground = AnsiConsole.BackgroundColor;
        OriginalCursorLeft = Console.CursorLeft;
        OriginalCursorTop = Console.CursorTop;

#if platform_windows
        OriginalCursorVisible = Console.CursorVisible;
        CursorVisibilitySupported = true;
#endif
    }

    /// <summary>
    /// Restores the console to its original state.
    /// </summary>
    public void Dispose()
    {
        // Restore colors
        AnsiConsole.BackgroundColor = OriginalBackground;
        AnsiConsole.ForegroundColor = OriginalForeground;

        // Restore cursor visibility if supported
        if (CursorVisibilitySupported)
        {
            Console.CursorVisible = OriginalCursorVisible;
        }
    }
}
