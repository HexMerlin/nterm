using System.Runtime.InteropServices;
using SemanticTokens.Core;

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
        OriginalForeground = Console.ForegroundColor;
        OriginalBackground = Console.BackgroundColor;
        OriginalCursorLeft = Console.CursorLeft;
        OriginalCursorTop = Console.CursorTop;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            OriginalCursorVisible = System.Console.CursorVisible;
            CursorVisibilitySupported = true;
        }
    }

    /// <summary>
    /// Restores the console to its original state.
    /// </summary>
    public void Dispose()
    {
        // Restore colors
        Console.BackgroundColor = OriginalBackground;
        Console.ForegroundColor = OriginalForeground;

        // Restore cursor visibility if supported
        if (CursorVisibilitySupported)
        {
            System.Console.CursorVisible = OriginalCursorVisible;
        }
    }
}
