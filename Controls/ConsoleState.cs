using System.Runtime.InteropServices;
using TrueColor;

namespace Controls;

/// <summary>
/// Represents the state of the console that can be restored later.
/// </summary>
public sealed class ConsoleState : IDisposable
{
    public Color OriginalForeground { get; }
    public Color OriginalBackground { get; }
    public int OriginalCursorLeft { get; }
    public int OriginalCursorTop { get; }
    private bool OriginalCursorVisible { get; } = false;
    private bool CursorVisibilitySupported { get; } = false;

    public ConsoleState()
    {
        OriginalForeground = AnsiConsole.ForegroundColor;
        OriginalBackground = AnsiConsole.BackgroundColor;
        OriginalCursorLeft = Console.CursorLeft;
        OriginalCursorTop = Console.CursorTop;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            OriginalCursorVisible = Console.CursorVisible;
            CursorVisibilitySupported = true;
        }
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
