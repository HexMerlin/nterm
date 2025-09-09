using System.Runtime.InteropServices;

namespace NTerm.Core;

internal static class Posix
{
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
#pragma warning disable IDE1006 // Naming Styles
    public struct termios
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
    {
        public uint c_iflag,
            c_oflag,
            c_cflag,
            c_lflag;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] c_cc;
        public uint c_ispeed,
            c_ospeed;
    }

    // P/Invoke
    [DllImport("libc", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    static extern int isatty(int fd);

    [DllImport("libc", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    static extern int tcgetattr(int fd, out termios termios_p);

    [DllImport("libc", SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
    static extern int tcsetattr(int fd, int optional_actions, ref termios termios_p);

    [DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
    static extern void cfmakeraw(ref termios termios_p);

    const int TCSANOW = 0;

    // We only need this one flag to re-enable signals (Ctrl+C) after cfmakeraw.
    // Value is correct on Linux & macOS (ISIG = 0x00000001).
    const uint ISIG = 0x00000001;

    static bool _installed;
    static termios _orig;

    public static void TryEnableRawTerminalOnPosix()
    {
        if (OperatingSystem.IsWindows())
            return; // only POSIX
        const int fd = 0; // stdin

        // Only if stdin is a real terminal (avoid when piped)
        if (isatty(fd) == 0)
            return;

        if (tcgetattr(fd, out var tio) != 0)
            return;
        _orig = tio;

        // Set a correct raw profile (byte-at-a-time, no echo, no canonical, etc.)
        cfmakeraw(ref tio);

        // Keep Ctrl+C behaving like usual (deliver SIGINT).
        tio.c_lflag |= ISIG;

        // Make sure control char array exists (some runtimes marshal null).
        tio.c_cc ??= new byte[32];

        // Apply
        if (tcsetattr(fd, TCSANOW, ref tio) != 0)
            return;

        _installed = true;

        // Restore on exit so the shell isn’t left in raw mode
        AppDomain.CurrentDomain.ProcessExit += (_, __) =>
        {
            try
            {
                if (_installed)
                    tcsetattr(fd, TCSANOW, ref _orig);
            }
            catch { }
        };
    }
}
