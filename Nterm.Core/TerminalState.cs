namespace Nterm.Core;

/// <summary>
/// Represents the state of the console that can be restored later.
/// </summary>
public sealed class TerminalState : IDisposable
{
    public Color OriginalForeground { get; }
    public Color OriginalBackground { get; }
    public int OriginalCursorLeft { get; }
    public int OriginalCursorTop { get; }

    // Assume that cursor is visible on all platforms. Only on Windows we can check if it is visible.
    public bool OriginalCursorVisible { get; } = true;

    public TerminalState()
    {
        OriginalForeground = Terminal.ForegroundColor;
        OriginalBackground = Terminal.BackgroundColor;
        OriginalCursorLeft = Terminal.CursorLeft;
        OriginalCursorTop = Terminal.CursorTop;

        if (OperatingSystem.IsWindows())
        {
            OriginalCursorVisible = Terminal.CursorVisible;
        }
    }

    /// <summary>
    /// Restores the console to its original state.
    /// </summary>
    public void Dispose()
    {
        // Restore default foreground color
        Terminal.ForegroundColor = OriginalForeground;
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            Terminal.CursorVisible = OriginalCursorVisible;
        }
    }
}
