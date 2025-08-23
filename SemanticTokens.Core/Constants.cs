namespace SemanticTokens.Core;
public class Constants
{
    // Core ANSI characters
    public const char ESC = '\x1b';
    public const byte ESC_BYTE = 0x1B;
    public const byte SIXEL_START_BYTE = 0x50; // 'P'

    // Sixel protocol
    public const string SixelStart = "P7;1;q\"1;1";
    public const string SixelEnd = "\\";

    // Terminal synchronization
    public const string SyncBegin = "[?2026h";
    public const string SyncEnd = "[?2026l";

    // Cursor control
    public const string CursorOff = "[?25l";
    public const string CursorOn = "[?25h";
    public const string CursorSave = "[s";
    public const string CursorRestore = "[u";
    public const string CursorUp = "[{0}A";  // Format with line count

    // Screen control
    public const string EraseFromCursor = "[0J";

    // Terminal queries
    public const string DeviceAttributesQuery = "[c";
    public const string WindowPixelSizeQuery = "[14t";      // response: "[4;{height};{width}"
    public const string CellPixelSizeQuery = "[16t";        // response: "[6;{height};{width}"
    public const string SyncSupportQuery = "[?2026$p";      // response ends with 'y'

    // Sixel encoding characters
    public const byte SpecialChNr = 0x6d;
    public const byte SpecialChCr = 0x64;
    public const char SixelNextLine = '-';

    // Common ANSI escape sequence parts
    public const char ColorIntroducer = '#';
    public const char RepeatIntroducer = '!';
}
