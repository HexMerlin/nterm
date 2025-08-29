using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SemanticTokens.DevConsole;

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
    private readonly FileStream _stream;

    public RawTTY()
    {
        if (OperatingSystem.IsWindows())
            return;

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

    static Key ParseEscape(byte[] buf, int len, out int consumed)
    {
        // buf[0] == 0x1B guaranteed by caller
        consumed = 1;
        if (len < 2)
            return Key.Unknown;

        byte b1 = buf[1];
        consumed = 2;

        if (b1 == (byte)'O') // SS3 form: ESC O <final>
        {
            if (len < 3)
                return Key.Unknown;
            byte fin = buf[2];
            consumed = 3;
            return fin switch
            {
                (byte)'A' => Key.ArrowUp,
                (byte)'B' => Key.ArrowDown,
                (byte)'C' => Key.ArrowRight,
                (byte)'D' => Key.ArrowLeft,
                _ => Key.Unknown
            };
        }
        else if (b1 == (byte)'[') // CSI form: ESC [ [params...] <final>
        {
            int i = 2;
            // optional params: digits ; digits ; ... (e.g., "1;2")
            while (i < len)
            {
                byte ch = buf[i];
                if ((ch >= (byte)'0' && ch <= (byte)'9') || ch == (byte)';' || ch == (byte)'?')
                {
                    i++;
                    continue;
                }
                // ch should be the final byte in ranges @A–Z or ~, etc.
                consumed = i + 1;
                return ch switch
                {
                    (byte)'A' => Key.ArrowUp,
                    (byte)'B' => Key.ArrowDown,
                    (byte)'C' => Key.ArrowRight,
                    (byte)'D' => Key.ArrowLeft,
                    // add more finals here, e.g. '~' for Home/End/PgUp/PgDn variants
                    _ => Key.Unknown
                };
            }
            // ran out of bytes before final
            return Key.Unknown;
        }

        // Some terminals send ESC <printable> for Alt+key; treat as Unknown here
        return Key.Unknown;
    }

    public void Loop()
    {
        Console.WriteLine("Raw mode. Press 'q' to quit.");

        while (true)
        {
            var buf = new byte[64];
            int n = _stream.Read(buf, 0, buf.Length); // returns quickly due to VTIME
            for (int i = 0; i < n; )
            {
                byte b = buf[i];

                if (b == 0x1B) // ESC: parse full sequence
                {
                    var key = ParseEscape(buf.AsSpan(i).ToArray(), n - i, out var used);
                    i += used;
                    switch (key)
                    {
                        case Key.ArrowUp:
                            Console.WriteLine("Up");
                            break;
                        case Key.ArrowDown:
                            Console.WriteLine("Down");
                            break;
                        case Key.ArrowRight:
                            Console.WriteLine("Right");
                            break;
                        case Key.ArrowLeft:
                            Console.WriteLine("Left");
                            break;
                        default:
                            Console.WriteLine("ESC seq");
                            break;
                    }
                }
                else
                {
                    // plain byte (UTF-8 handling omitted for brevity)
                    if (b == (byte)'q')
                    { /* quit */
                        return;
                    }
                    Console.WriteLine($"Byte: 0x{b:X2} '{(char)b}'");
                    i += 1;
                }
            }
        }
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
