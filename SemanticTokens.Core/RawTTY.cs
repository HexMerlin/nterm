using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SemanticTokens.Core;

/// <summary>
/// Raw TTY input for Linux and macOS.
/// </summary>
/// <remarks>
/// This class is used to get raw TTY input for Linux and macOS.
/// </remarks>
public class RawTTY : IDisposable
{
    const int O_RDONLY = 0;
    const int STDIN_FILENO = 0;

    [StructLayout(LayoutKind.Sequential)]
    struct termios
    {
        public uint c_iflag,
            c_oflag,
            c_cflag,
            c_lflag;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] c_cc;
        public uint c_ispeed,
            c_ospeed; // present on glibc/x86_64
    }

    [DllImport("libc", SetLastError = true)]
    static extern int open(string pathname, int flags);

    [DllImport("libc", SetLastError = true)]
    static extern int close(int fd);

    [DllImport("libc", SetLastError = true)]
    static extern int tcgetattr(int fd, out termios termios_p);

    [DllImport("libc", SetLastError = true)]
    static extern int tcsetattr(int fd, int optional_actions, ref termios termios_p);

    [DllImport("libc", SetLastError = true)]
    static extern void cfmakeraw(ref termios termios_p);

    const int TCSANOW = 0;

    // c_cc indexes (Linux): VMIN = 6, VTIME = 5 (values depend on system headers).
    const int VTIME = 5;
    const int VMIN = 6;

    private int _fd = -1;
    private termios _orig;
    private readonly Stream _stream;

    public RawTTY()
    {
        if (OperatingSystem.IsWindows())
        {
            // On Windows, set stream to null but do not throw in constructor
            _stream = Stream.Null;
            return;
        }

        // Prefer /dev/tty to bypass redirection
        _fd = open("/dev/tty", O_RDONLY);
        if (_fd < 0)
        {
            // Fallback: try stdin (only works if stdin is a tty)
            _fd = STDIN_FILENO;
        }

        if (tcgetattr(_fd, out _orig) != 0)
            throw new InvalidOperationException("tcgetattr failed");

        var raw = _orig;
        cfmakeraw(ref raw);

        raw.c_oflag = _orig.c_oflag; // keep output processing (incl. NL -> CRLF) as before

        // Ensure blocking read of at least 1 byte
        if (raw.c_cc == null || raw.c_cc.Length < 7)
            raw.c_cc = new byte[32];
        raw.c_cc[VMIN] = 0; // return as soon as any bytes are available
        raw.c_cc[VTIME] = 1; // 0.1s read timeout to let multi-byte sequences arrive

        if (tcsetattr(_fd, TCSANOW, ref raw) != 0)
            throw new InvalidOperationException("tcsetattr failed");

        // Use a FileStream on the same fd for convenience
        var safe = new SafeFileHandle(new IntPtr(_fd), ownsHandle: false);
        _stream = new FileStream(safe, FileAccess.Read, bufferSize: 1, isAsync: false);
    }

    public Stream GetStream()
    {
        return _stream;
    }

    enum Key
    {
        Char,
        ArrowUp,
        ArrowDown,
        ArrowRight,
        ArrowLeft,
        Unknown
    }

    public void Dispose()
    {
        if (_fd >= 0)
        {
            // restore original termios (use STDIN if we fell back)
            try
            {
                tcsetattr(_fd, TCSANOW, ref _orig);
            }
            catch { }
        }
        _stream?.Dispose();
        // only close if we opened /dev/tty
        // (we used ownsHandle=false for safety)
        // close(_fd) could close stdin if fallback; avoid that here
    }
}
