// Program.cs
using SixPix; // NuGet: SixPix
using System.Reflection;
using System.Text;

namespace SemanticTokens.Sixel;



public sealed class Test
{

    public const string ImageUser = "Images.user.png";
    public const string ImageBot  = "Images.bot.png";
    public const string ImageAI = "Images.ai.png";

    // ---- configurable instance settings ---------------------------------
    private readonly int avatarPx = 128;                 // source avatar size (px)
    private readonly int cellWpx;                        // px per column
    private readonly int cellHpx;                        // px per row
    private readonly int textRowOffset;                  // fine-tune text anchor vs image top (rows)
    private readonly int avatarCols;                     // avatar width in columns
    private readonly int avatarRows;                     // avatar height in rows
    private readonly int textStartCol;                   // starting column for text
    private readonly bool sixelOk;                       // terminal capability
    private readonly SaveMode saveMode;                  // dec | ansi | none

    public Test()
    {
        cellWpx = ReadIntEnv("CHAT_CELLW", 8);
        cellHpx = ReadIntEnv("CHAT_CELLH", 16);
        textRowOffset = ReadIntEnv("CHAT_TEXTOFFSET_ROWS", 1);

        // DEFAULT: ANSI to avoid stray glyphs; set CHAT_SAVE_MODE=dec or =none to change.
        saveMode = ParseSaveMode(Environment.GetEnvironmentVariable("CHAT_SAVE_MODE")) ?? SaveMode.Ansi;

        avatarCols = Math.Max(1, (int)Math.Ceiling((double)avatarPx / cellWpx));

        int overrideRows = ReadIntEnv("CHAT_AVATAR_ROWS", 0);
        avatarRows = overrideRows > 0
                    ? overrideRows
                    : Math.Max(1, (int)Math.Ceiling((double)avatarPx / cellHpx));

        textStartCol = avatarCols + 2; // a little padding between avatar & text
        sixelOk = true; 
    }

    public void Run()
    {
        WriteRgbLine("Embedded SIXEL Chat Demo — 24-bit text + avatar images",
                     fg: (160, 220, 255), bg: (30, 30, 40));
        Console.WriteLine();

        PrintChatMessage(
            resourceSuffix: ImageUser,
            speakerLabel: "User",
            speakerColor: (200, 255, 160),
            text:
@"Hey! Could you summarize today’s headlines? Also—can you show avatars inline?
Let’s make sure text aligns neatly with those images."
        );

        PrintChatMessage(
            resourceSuffix: ImageBot,
            speakerLabel: "Bot",
            speakerColor: (160, 200, 255),
            text:
@"Sure thing! I’ll format the output with bullets and keep it short. 
If your terminal supports SIXEL, you should see avatars on the left."
        );

        PrintChatMessage(
            resourceSuffix: ImageAI,
            speakerLabel: "AI",
            speakerColor: (255, 200, 160),
            text:
@"I’m the LLM-powered one. If SIXEL isn’t available, you’ll just see clean fallback text.
Either way, your 24-bit colors continue to work everywhere."
        );

        Console.WriteLine();
        Reset();
    }

    // ---- message rendering (pre-reserve rows; anchor both at same top) ---
    private void PrintChatMessage(string resourceSuffix, string speakerLabel, (int r, int g, int b) speakerColor, string text)
    {
        Console.WriteLine();     // start block on a fresh line

        // Compute wrapped text up-front
        int consoleWidth = Math.Max(40, SafeBufferWidth());
        int wrapWidth = Math.Max(20, consoleWidth - textStartCol - 1);
        var lines = WrapText(text, wrapWidth);
        int textRows = Math.Max(1, lines.Count);

        // Reserve a block that is tall enough for BOTH avatar and text
        int blockRows = Math.Max(avatarRows, textRows);

        if (saveMode == SaveMode.None)
        {
            // ------- NO SAVE/RESTORE PATH (pure relative moves) ----------
            // Reserve space below current line
            CUD(blockRows + 1); HPA(1);
            // Move back up to the top of the reserved block
            CUU(blockRows + 1);

            // Avatar (top-left of block)
            if (sixelOk)
            {
                try { Console.Write(EncodeSixelFromEmbedded(resourceSuffix)); }
                catch (Exception ex) { Console.WriteLine($"[avatar failed: {resourceSuffix} | {ex.Message}]"); }
            }
            else
            {
                Console.WriteLine($"[avatar: {Path.GetFileName(resourceSuffix)}]");
            }

            // Text: re-anchor to the block top via relative movement
            CUU(avatarRows);
            if (textRowOffset < 0) CUU(-textRowOffset);
            else if (textRowOffset > 0) CUD(textRowOffset);
            HPA(textStartCol);

            Console.Write("\x1b[1m"); WriteRgbInline($"{speakerLabel}:", fg: speakerColor); Console.Write("\x1b[22m"); Console.Write(' ');
            if (lines.Count == 0) Console.WriteLine();
            else
            {
                Console.WriteLine(lines[0]);
                for (int i = 1; i < lines.Count; i++)
                {
                    HPA(textStartCol);
                    Console.WriteLine(lines[i]);
                }
            }

            // Move cursor to the line AFTER the block, ready for next message
            // Ensure we are back near the top before dropping down
            CUU(Math.Min(textRows + Math.Max(0, textRowOffset), blockRows));
            CUD(blockRows + 1);
            HPA(1);
            return;
        }

        // ------------------- SAVE/RESTORE PATH (ANSI default) -------------
        SaveCursor();                         // top anchor
        CUD(blockRows + 1); HPA(1);          // reserve
        RestoreCursor();                      // back to the top of the block

        // Avatar at block top
        if (sixelOk)
        {
            try { Console.Write(EncodeSixelFromEmbedded(resourceSuffix)); }
            catch (Exception ex) { Console.WriteLine($"[avatar failed: {resourceSuffix} | {ex.Message}]"); }
        }
        else
        {
            Console.WriteLine($"[avatar: {Path.GetFileName(resourceSuffix)}]");
        }

        // Text anchored to the SAME block top
        RestoreCursor();
        if (textRowOffset < 0) CUU(-textRowOffset);
        else if (textRowOffset > 0) CUD(textRowOffset);
        HPA(textStartCol);

        Console.Write("\x1b[1m"); WriteRgbInline($"{speakerLabel}:", fg: speakerColor); Console.Write("\x1b[22m"); Console.Write(' ');
        if (lines.Count == 0) Console.WriteLine();
        else
        {
            Console.WriteLine(lines[0]);
            for (int i = 1; i < lines.Count; i++)
            {
                HPA(textStartCol);
                Console.WriteLine(lines[i]);
            }
        }

        // Land below the reserved block for the next message
        RestoreCursor(); CUD(blockRows + 1); HPA(1);
    }

    // ---- SIXEL encoding from embedded resource (kept static) ------------
    private static string EncodeSixelFromEmbedded(string resourceSuffix)
    {
        var asm = Assembly.GetExecutingAssembly();
        string? resName = asm.GetManifestResourceNames()
                             .FirstOrDefault(n => n.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));
        if (resName == null)
            throw new FileNotFoundException($"Embedded resource not found (suffix): {resourceSuffix}");

        using var stream = asm.GetManifestResourceStream(resName)
                       ?? throw new FileNotFoundException($"Embedded resource stream is null: {resName}");

        ReadOnlySpan<char> sixel = SixPix.Sixel.Encode(stream); // stream -> SIXEL (includes DCS/ST)
        return sixel.ToString();
    }

    // ---- capability ------------------------------------------------------
    private bool SupportsSixel()
    {
        var force = Environment.GetEnvironmentVariable("FORCE_SIXEL");
        if (!string.IsNullOrEmpty(force) && force != "0") return true;

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WT_SESSION"))) return true;

        var term = Environment.GetEnvironmentVariable("TERM") ?? "";
        if (term.IndexOf("sixel", StringComparison.OrdinalIgnoreCase) >= 0) return true;

        return false;
    }

    // ---- ANSI helpers ----------------------------------------------------
    private enum SaveMode { Dec, Ansi, None }

    private static SaveMode? ParseSaveMode(string? s)
    {
        if (string.IsNullOrEmpty(s)) return null;
        return s.Trim().ToLowerInvariant() switch
        {
            "dec" => SaveMode.Dec,
            "ansi" => SaveMode.Ansi,
            "none" => SaveMode.None,
            _ => null
        };
    }

    private void SaveCursor()
    {
        if (saveMode == SaveMode.Dec) Console.Write("\x1b7");   // DEC Save Cursor (DECSC)
        else Console.Write("\x1b[s");  // ANSI Save (SCP)
    }

    private void RestoreCursor()
    {
        if (saveMode == SaveMode.Dec) Console.Write("\x1b8");   // DEC Restore Cursor (DECRC)
        else Console.Write("\x1b[u");  // ANSI Restore (SCP)
    }

    private void HPA(int col) => Console.Write($"\x1b[{col}G"); // Horizontal Position Absolute
    private void CUD(int n) => Console.Write($"\x1b[{n}B");   // Cursor Down
    private void CUU(int n) => Console.Write($"\x1b[{n}A");   // Cursor Up

    // color writers (inline vs line)
    private void WriteRgbInline(string text, (int r, int g, int b)? fg = null, (int r, int g, int b)? bg = null)
    {
        if (fg.HasValue) Console.Write($"\x1b[38;2;{fg.Value.r};{fg.Value.g};{fg.Value.b}m");
        if (bg.HasValue) Console.Write($"\x1b[48;2;{bg.Value.r};{bg.Value.g};{bg.Value.b}m");
        Console.Write(text);
        Reset();
    }
    private void WriteRgbLine(string text, (int r, int g, int b)? fg = null, (int r, int g, int b)? bg = null)
    {
        WriteRgbInline(text, fg, bg);
        Console.WriteLine();
    }

    // ---- misc utils ------------------------------------------------------
    private static int ReadIntEnv(string name, int fallback)
        => int.TryParse(Environment.GetEnvironmentVariable(name), out var v) && v > 0 ? v : fallback;

    private static int SafeBufferWidth()
    {
        try { return Console.BufferWidth; }
        catch { return 80; } // redirected output
    }

    private static List<string> WrapText(string text, int width)
    {
        var lines = new List<string>();
        foreach (var para in (text ?? "").Replace("\r", "").Split('\n'))
        {
            var words = para.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            int col = 0;

            foreach (var w in words)
            {
                int need = (col == 0 ? w.Length : 1 + w.Length);
                if (col + need > width)
                {
                    lines.Add(sb.ToString());
                    sb.Clear();
                    sb.Append(w);
                    col = w.Length;
                }
                else
                {
                    if (col != 0) { sb.Append(' '); col++; }
                    sb.Append(w);
                    col += w.Length;
                }
            }
            lines.Add(sb.ToString());
            lines.Add(""); // blank line between paragraphs
        }
        if (lines.Count > 0 && lines[^1] == "") lines.RemoveAt(lines.Count - 1);
        return lines;
    }

    private void Reset() => Console.Write("\x1b[0m");
}
