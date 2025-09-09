using System.Runtime.InteropServices;
using NTerm.Core;

namespace NTerm.Controls;

/// <summary>
/// Represents the state of the console that can be restored later.
/// </summary>
public sealed class ConsoleState : IDisposable
{
    public Color OriginalForeground { get; }
    public Color OriginalBackground { get; }
    public int OriginalCursorLeft { get; }
    public int OriginalCursorTop { get; }

    // Assume that cursor is visible on all platforms. Only on Windows we can check if it is visible.
    public bool OriginalCursorVisible { get; } = true;

    public ConsoleState()
    {
        OriginalForeground = Console.ForegroundColor;
        OriginalBackground = Console.BackgroundColor;
        OriginalCursorLeft = Console.CursorLeft;
        OriginalCursorTop = Console.CursorTop;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            OriginalCursorVisible = System.Console.CursorVisible;
        }
    }

    /// <summary>
    /// Restores the console to its original state.
    /// </summary>
    public void Dispose()
    {
        // Restore default foreground color
        Console.ForegroundColor = OriginalForeground;

        // Restore cursor visibility if supported
        if (
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
        )
        {
            System.Console.CursorVisible = OriginalCursorVisible;
        }
    }
}
