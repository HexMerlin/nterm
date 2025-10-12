using Nterm.Core.Buffer;
using System;
using System.Collections.Generic;
using System.Text;
using static Nterm.Core.Experimental.VtPanels;

namespace Nterm.Core.Experimental;

public readonly record struct BufferXY(int X, int Y);
public readonly record struct WindowXY(int X, int Y);



public class VirtualTerminal
{
    PanelStack PanelStack { get; }
  
    public VirtualTerminal()
    {
        PanelStack = new PanelStack();
        
    }

    public BufferXY GetMaxXY(Size windowsSize)
    {
        int maxY = 0;
        int maxX = 0;
        foreach (Panel panel in PanelStack.Panels)
        {
            BufferXY maXY = panel.MaxXY;
            maxX = Math.Max(maxX, maXY.X);
            maxY = Math.Max(maxY, maXY.Y);
        }
        return new BufferXY(maxX, maxY);
    }


}

internal class PanelStack
{
    public List<Panel> Panels { get; } = new();

}

public static class VtPanels
{
    private const string CSI = "\x1b[";

    /// Enter a clipped panel at (x,y) with given width/height (0-based cells).
    /// Leaves the cursor at the panel's top-left (ready to write).
    public static void EnterPanel(int x, int y, int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x, nameof(x));
        ArgumentOutOfRangeException.ThrowIfNegative(y, nameof(y));
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1, nameof(width));
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1, nameof(height));

        int right = x + width;
        int bottom = y + height;

        Console.Write(
            CSI + "?69h" +               // DECLRMM on (enable L/R margins)
            CSI + $"{y};{bottom}r" +     // DECSTBM (top/bottom margins)
            CSI + $"{x};{right}s" +      // DECSLRM (left/right margins)
            CSI + "?6h" +                // DECOM on (origin within margins)
            CSI + "H"                    // CUP 1;1 (top-left inside the panel)
        );
    }

    /// Exit the current panel, resetting margins/origin, and place the cursor safely at end.
    public static void ExitPanel()
    {
        const string CSI = "\x1b[";

        Console.Write(
            CSI + "?6l" +   // DECOM off
            CSI + "0;0s" +  // reset L/R margins
            CSI + "0;0r"    // reset T/B margins
        );

        // Move cursor to bottom row, column 1 (clamped by the terminal).
        Console.Write(CSI + "9999;1H");
    }

    public static void SetCursorX(int x) => Console.Write($"{CSI}{x + 1}G");          // CHA (column only)
    public static void SetCursorY(int y) => Console.Write($"{CSI}{y + 1}d");          // VPA (row only)
    public static void SetCursor(int x, int y) => Console.Write($"{CSI}{y + 1};{x + 1}H"); // CUP (row,col)

   

    public class Panel
    {
        private const string CSI = "\x1b[";



        internal BufferXY Position { get; }

        internal int Width { get; }

        internal int Height => TextBuffer.LineCount;

        internal Size Size => new(Size.Width, Height);

        private TextBuffer TextBuffer { get; }

        public Panel(TextBuffer textBuffer, BufferXY position, int width)
        {
            TextBuffer = textBuffer;
            Position = position;
            Width = width;
        }

        public BufferXY MaxXY => new(Position.X + Width, Position.Y + Height);

        /// <summary>
        /// Disable automatic line wrap (DECAWM off).
        /// The cursor will not wrap to the next line at the right edge; further glyphs overstrike the last cell.
        /// </summary>
        public static void TurnOffWordWrap()
        {
            Console.Write(CSI + "?7l"); // DECAWM = off
        }
    }
}

public class GraphicsPanel
{
    internal BufferXY Position { get; }

    internal Size Size { get; }

    List<char> Content { get; } = new();

    public GraphicsPanel(BufferXY position, Size size)
    {
        Position = position;
        Size = size;
    }

    public void Write(ReadOnlySpan<char> imageData)
    {
        Content.AddRange(imageData);
    }

    public void WriteLine()
    {
        Content.Add('\n');
    }
}

public enum AnchorX
{
    WindowLeft,
    WindowCenter,
    WindowRight,
}

public enum AnchorY
{
    Buffer,
    WindowTop,
    WindowCenter,
    WindowBottom,
}

public enum AdaptiveWidth
{
    None,
    WindowFull,
    WindowHalf,
    WindowThird,
    WindowQuarter,
    FitContent
}

public enum AdaptiveHeight
{
    None,
    WindowFull,
    WindowHalf,
    WindowThird,
    WindowQuarter,
    FitContent
}



//public readonly record struct AnchorX
//{
//    private int Value { get; }

//    private AnchorX(int value) { Value = value; }

//    public static AnchorX LeftWindow => new(-1);

//    public static AnchorX CenterWindow => new(-2);

//    public static AnchorX RightWindow => new(-3);

//    public static AnchorX Exact(int x)
//    {
//        ArgumentOutOfRangeException.ThrowIfNegative(x, nameof(x));
//        return new AnchorX(x);
//    }

//    public WindowX GetX(Size contentSize, WindowXY windowSize)
//    {
//        return windowSize.X switch
//        {
//            -1 => new WindowX(0),
//            -2 => new WindowX((windowSize.X - contentSize.Width) / 2),
//            -3 => new WindowX(windowSize.X - contentSize.Width),
//            _ => new WindowX(windowSize.X)
//        };
//    }
//}

//public readonly record struct AnchorY
//{
//    private int Value { get; }

//    private AnchorY(int value) { Value = value; }

//    public static AnchorY WindowTop => new(-1);

//    public static AnchorY WindowCenter => new(-3);

//    public static AnchorY WindowBottom => new(-5);

//    public static AnchorY Exact(int y)
//    {
//        ArgumentOutOfRangeException.ThrowIfNegative(y, nameof(y));
//        return new AnchorY(y);
//    }

//    public WindowY GetY(Size contentSize, WindowXY windowSize)
//    {
//        return windowSize.Y switch
//        {
//            -1 => new WindowY(0),
//            -2 => new WindowY((windowSize.Y - contentSize.Height) / 2),
//            -3 => new WindowY(windowSize.Y - contentSize.Height),
//            _ => new WindowY(windowSize.Y)
//        };
//    }
//}

//public readonly record struct Width 
//{   
//    public int Value { get; }

//    private Width(int value) { Value = value; }
//    public static Width WindowFull => new(-1);
//    public static Width WindowHalf => new(-2);
//    public static Width WindowThird => new(-3);
//    public static Width WindowQuarter => new(-4);
//    public static Width FitContent => new(-5);

//    public static Width Exact(int value)
//    {
//        ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
//        return new Width(value);
//    }

//}

//public readonly record struct Height
//{
//    public int Value { get; }

//    private Height(int value) { Value = value; }
//    public static Height FullScreen => new(-1);
//    public static Height HalfScreen => new(-2);
//    public static Height ThirdScreen => new(-3);
//    public static Height QuarterScreen => new(-4);
//    public static Height FitContent => new(-5);

//    public static Height Exact(int value)
//    {
//        ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
//        return new Height(value);
//    }

//}