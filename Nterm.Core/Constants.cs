namespace Nterm.Core;

public class Constants
{
    // Core ANSI characters
    public const char ESC = '\x1b';

    public const byte ESC_BYTE = 0x1B;

    //// --- CSI / SGR / OSC basics ---
    //// NOTE: All codes below are the bracketed/parameter part (you prefix them with ESC).
    //public const string CSI = "["; // Control Sequence Introducer ("[")
    //public const string SGR_RESET = "[0m"; // Reset all attributes

    //// True-color SGR prefixes and terminator

    public const string SGR_FG_TRUECOLOR_PREFIX = "[38;2;"; // foreground: 38;2;R;G;B

    public const string SGR_BG_TRUECOLOR_PREFIX = "[48;2;"; // background: 48;2;R;G;B
    public const char SGR_END = 'm';

    //// Cursor control
    public const string CursorOff = "[?25l";
    public const string CursorOn = "[?25h";
    public const string CursorSave = "[s";
    public const string CursorRestore = "[u";
    public const string CursorUp = "[{0}A"; // format with count
    public const string CursorHome = "[H";

    //// Screen control
    public const string EraseFromCursor = "[0J";
    public const string EraseDisplayAll = "[2J";
    public const string EraseEntireScrollback = "[3J"; 

    //public const string EraseScrollback = "[3J";

    // Terminal synchronization
    public const string SyncBegin = "[?2026h";
    public const string SyncEnd = "[?2026l";

    // Terminal queries (CSI-style examples)
    public const string DeviceAttributesQuery = "[c";
    public const string WindowPixelSizeQuery = "[14t"; // response "[4;h;w"
    public const string CellPixelSizeQuery = "[16t"; // response "[6;h;w"
    public const string SyncSupportQuery = "[?2026$p"; // ends 'y'

    // CSI Select Graphic Rendition (SGR)
    public const string Underline = "\u001b[4m";
    public const string UnderlineEnd = "\u001b[24m";

    //// Sixel protocol
    public const byte SIXEL_START_BYTE = 0x50; // 'P'
    public const string SixelStart = "P7;1;q\"1;1";
    public const string SixelEnd = "\\";

    //// Sixel encoding characters
    public const byte SpecialChNr = 0x6d;
    public const byte SpecialChCr = 0x64;
    public const char SixelNextLine = '-';

    //// Common ANSI escape sequence parts
    public const char ColorIntroducer = '#';
    public const char RepeatIntroducer = '!';
}