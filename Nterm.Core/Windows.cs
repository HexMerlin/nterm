using System.Runtime.InteropServices;

namespace NTerm.Core;
internal static class Windows
{
    internal static void TryEnableVirtualTerminalOnWindows()
    {
        if (!OperatingSystem.IsWindows()) return;

        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

        const uint ENABLE_PROCESSED_INPUT = 0x0001; // keep so Ctrl+C still works
        const uint ENABLE_LINE_INPUT = 0x0002; // cooked mode (we will clear)
        const uint ENABLE_ECHO_INPUT = 0x0004; // host echo (we will clear)
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040; // (must be cleared with EXTENDED_FLAGS)
        const uint ENABLE_EXTENDED_FLAGS = 0x0080;

        nint hout = Terminal.GetStdHandle(Terminal.STD_OUTPUT_HANDLE);
        if (hout != nint.Zero && GetConsoleMode(hout, out uint outMode))
        {
            _ = SetConsoleMode(hout, outMode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }

        nint hin = Terminal.GetStdHandle(Terminal.STD_INPUT_HANDLE);
        if (hin != nint.Zero && GetConsoleMode(hin, out uint inMode))
        {
            inMode |= ENABLE_EXTENDED_FLAGS; // allow clearing QUICK_EDIT
            inMode &= ~(ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT | ENABLE_QUICK_EDIT_MODE); // raw-ish
            inMode |= ENABLE_PROCESSED_INPUT | ENABLE_VIRTUAL_TERMINAL_INPUT;
            _ = SetConsoleMode(hin, inMode);
        }

        [DllImport("kernel32.dll", SetLastError = true)] static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    }
}
