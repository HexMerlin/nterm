using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticTokens.Sixel;
internal class Constants
{
    public const char ESC = '\x1b';
    public const string SixelStart = "P7;1;q\"1;1";
    public const string SyncBegin = "[?2026h";
    public const string SyncEnd = "[?2026l";
    public const string CursorOff = "[?25l";
    public const string CursorOn = "[?25h";
    public const string End = "\\";

    public const byte specialChNr = 0x6d;
    public const byte specialChCr = 0x64;
}
