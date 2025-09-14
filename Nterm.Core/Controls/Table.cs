using Nterm.Core.Buffer;

namespace Nterm.Core.Controls;

/// <summary>
/// Immutable-dimension table with beautiful Unicode grid formatting and 24‑bit color support.
/// Table dimensions are fixed at construction; individual cells and headers are mutable.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="ToTextBuffer()"/> or <see cref="ToTextBuffer(TableTheme)"/> to render the table
/// into a <see cref="TextBuffer"/> for terminal output. Rendering supports color theming and multiple
/// border styles via <see cref="TableTheme"/> and <see cref="TableBorders"/>.
/// </para>
/// <para>This type is not thread-safe.</para>
/// </remarks>
/// <seealso cref="TableTheme"/>
/// <seealso cref="TableBorders"/>
/// <seealso cref="TextBuffer"/>
public class Table
{
    /// <summary>
    /// Gets the header text for each column.
    /// </summary>
    /// <remarks>
    /// The array length is equal to <see cref="ColCount"/>. An empty string indicates no header for that column.
    /// </remarks>
    public string[] Headers { get; }

    private readonly string[,] cells;

    // Unicode box drawing characters for beautiful grid formatting
    private const char TopLeft = '┌';
    private const char TopRight = '┐';
    private const char BottomLeft = '└';
    private const char BottomRight = '┘';
    private const char Horizontal = '─';
    private const char Vertical = '│';
    private const char Cross = '┼';
    private const char TopTee = '┬';
    private const char BottomTee = '┴';
    private const char LeftTee = '├';
    private const char RightTee = '┤';

    // Layout constants for consistent spacing
    private const int CellPadding = 1; // Padding inside cells (left + right = 2)
    private const int ColumnSpacing = 2; // Spaces between columns when no separators

    /// <summary>
    /// Gets the number of columns in the table.
    /// </summary>
    public int ColCount => cells.GetLength(0);

    /// <summary>
    /// Gets the number of rows in the table.
    /// </summary>
    public int RowCount => cells.GetLength(1);

    /// <summary>
    /// Gets a value indicating whether any header cell contains content.
    /// </summary>
    public bool HasHeaders => Headers.Any(h => h.Length > 0);

    /// <summary>
    /// Gets or sets the content of a cell at the specified column and row.
    /// </summary>
    /// <param name="col">Zero-based column index.</param>
    /// <param name="row">Zero-based row index.</param>
    /// <returns>The cell content; never null (empty string is used instead).</returns>
    /// <remarks>
    /// Setting a null value stores an empty string.
    /// </remarks>
    public string this[int col, int row]
    {
        get => cells[col, row] ?? "";
        set => cells[col, row] = value ?? "";
    }

    /// <summary>
    /// Initializes a new table with fixed dimensions.
    /// </summary>
    /// <param name="colCount">Number of columns to allocate.</param>
    /// <param name="rowCount">Number of rows to allocate.</param>
    /// <remarks>
    /// All headers and cells are initialized to empty strings.
    /// </remarks>
    public Table(int colCount, int rowCount)
    {
        Headers = new string[colCount];
        cells = new string[colCount, rowCount];

        // Initialize empty cells to prevent null values
        for (int col = 0; col < colCount; col++)
        {
            Headers[col] = "";
            for (int row = 0; row < rowCount; row++)
            {
                cells[col, row] = "";
            }
        }
    }

    /// <summary>
    /// Renders the complete table with default theme.
    /// </summary>
    /// <returns>A <see cref="TextBuffer"/> containing styled, formatted output.</returns>
    public TextBuffer ToTextBuffer() => ToTextBuffer(TableTheme.MonokaiMidnight);

    /// <summary>
    /// Renders the complete table with specified theme.
    /// Headers are displayed if any header has content.
    /// </summary>
    /// <param name="theme">Theme containing border style and styles to use for rendering.</param>
    /// <returns>A <see cref="TextBuffer"/> containing styled, formatted output.</returns>
    /// <remarks>
    /// If <see cref="TableBorders.Header"/> is selected but no headers are present, the border style falls back to <see cref="TableBorders.None"/>.
    /// </remarks>
    public TextBuffer ToTextBuffer(TableTheme theme)
    {
        TextBuffer output = new();

        if (ColCount == 0 || RowCount == 0)
            return output;

        // Header style with no headers falls back to None
        TableBorders borders = theme.Borders;
        if (borders == TableBorders.Header && !HasHeaders)
            borders = TableBorders.None;

        int[] columnWidths = CalculateColumnWidths();
        BorderConfig borderConfig = GetBorderConfiguration(borders);

        Write(output, columnWidths, borderConfig, theme);
        return output;
    }
    /// <summary>
    /// Calculates minimum required width for each column based on headers and cell content.
    /// </summary>
    /// <returns>Array of column widths where index corresponds to column number.</returns>
    private int[] CalculateColumnWidths()
    {
        int[] widths = new int[ColCount];

        // Check header widths
        for (int col = 0; col < ColCount; col++)
        {
            widths[col] = Math.Max(widths[col], Headers[col].Length);
        }

        // Check cell content widths
        for (int col = 0; col < ColCount; col++)
        {
            for (int row = 0; row < RowCount; row++)
            {
                widths[col] = Math.Max(widths[col], this[col, row].Length);
            }
        }

        // Ensure minimum width of 1 for empty columns
        for (int col = 0; col < ColCount; col++)
        {
            widths[col] = Math.Max(1, widths[col]);
        }

        return widths;
    }

    /// <summary>
    /// Renders horizontal border line with specified corner and junction characters.
    /// </summary>
    private void WriteHorizontalBorderWithJunctions(TextBuffer bufferedOutput, int[] columnWidths, char left, char junction, char right, Color color)
    {
        bufferedOutput.Append(left, color);

        for (int col = 0; col < ColCount; col++)
        {
            bufferedOutput.Append(new string(Horizontal, columnWidths[col] + (CellPadding * 2)), color);

            if (col < ColCount - 1)
                bufferedOutput.Append(junction, color);
        }

        bufferedOutput.Append(right, color);
        bufferedOutput.AppendLine();
    }

    /// <summary>
    /// Border position for character selection.
    /// </summary>
    private enum BorderPosition { Top, Bottom, HeaderSeparator, RowSeparator }

    /// <summary>
    /// Border configuration defining which types of lines should be drawn.
    /// </summary>
    /// <param name="TopBorder">Draw top border line</param>
    /// <param name="BottomBorder">Draw bottom border line</param>
    /// <param name="SideBorders">Draw left/right side borders</param>
    /// <param name="RowSeparators">Draw horizontal lines between data rows</param>
    /// <param name="ColumnSeparators">Draw vertical lines between columns</param>
    /// <param name="HeaderSeparator">Draw line under header row</param>
    private record BorderConfig(bool TopBorder, bool BottomBorder, bool SideBorders, bool RowSeparators, bool ColumnSeparators, bool HeaderSeparator);

    /// <summary>
    /// Determines border configuration for the specified border style.
    /// </summary>
    private static BorderConfig GetBorderConfiguration(TableBorders borders) => borders switch
    {
        TableBorders.None => new(false, false, false, false, false, false),
        TableBorders.Grid => new(true, true, true, true, true, true),
        TableBorders.OutlineAndHeader => new(true, true, true, false, false, true),
        TableBorders.Rows => new(false, false, false, true, false, true),
        TableBorders.Header => new(false, false, false, false, false, true),
        _ => throw new ArgumentOutOfRangeException(nameof(borders))
    };

    /// <summary>
    /// Renders table using the specified border configuration and theme.
    /// </summary>
    private void Write(TextBuffer textBuffer, int[] columnWidths, BorderConfig config, TableTheme theme)
    {
        // Top border
        if (config.TopBorder)
            WriteHorizontalBorder(textBuffer, columnWidths, config, BorderPosition.Top, theme.BorderColor);

        // Header row
        if (HasHeaders)
        {
            WriteContentRow(textBuffer, columnWidths, config, theme, true);

            // Header separator
            if (config.HeaderSeparator)
                WriteHorizontalBorder(textBuffer, columnWidths, config, BorderPosition.HeaderSeparator, theme.BorderColor);
        }

        // Data rows
        for (int row = 0; row < RowCount; row++)
        {
            WriteContentRow(textBuffer, columnWidths, config, theme, false, row);

            // Row separator (except after last row)
            if (config.RowSeparators && row < RowCount - 1)
                WriteHorizontalBorder(textBuffer, columnWidths, config, BorderPosition.RowSeparator, theme.BorderColor);
        }

        // Bottom border
        if (config.BottomBorder)
            WriteHorizontalBorder(textBuffer, columnWidths, config, BorderPosition.Bottom, theme.BorderColor);
    }

    /// <summary>
    /// Renders a content row (header or data) with appropriate separators.
    /// </summary>
    private void WriteContentRow(TextBuffer textBuffer, int[] columnWidths, BorderConfig config, TableTheme theme, bool isHeader, int rowIndex = 0)
    {
        // Left border
        if (config.SideBorders)
        {
            textBuffer.Append(Vertical, theme.BorderColor);
            textBuffer.Append(' ', theme.BorderColor); // Only padding on the inside
        }

        // Column content
        for (int col = 0; col < ColCount; col++)
        {
            string content = isHeader ? Headers[col] : this[col, rowIndex];
            Color textColor = GetCellColor(col, isHeader, theme);
            textBuffer.Append(content.PadRight(columnWidths[col]), textColor);

            // Column separator or spacing
            if (col < ColCount - 1)
                WriteColumnSeparator(textBuffer, config.ColumnSeparators, theme.BorderColor);
        }

        // Right border
        if (config.SideBorders)
        {
            textBuffer.Append(' ', theme.BorderColor); // Only padding on the inside
            textBuffer.Append(Vertical, theme.BorderColor);
        }

        textBuffer.AppendLine();
    }

    /// <summary>
    /// Determines appropriate text color for a cell based on position and content type.
    /// </summary>
    private static Color GetCellColor(int columnIndex, bool isHeader, TableTheme theme) =>
        isHeader ? theme.HeaderTextColor :
        columnIndex == 0 ? theme.FirstColumnTextColor :
        theme.OtherTextColor;

    /// <summary>
    /// Calculates total content width: sum of column widths + spacing between columns.
    /// </summary>
    private int CalculateContentWidth(int[] columnWidths) => columnWidths.Sum() + ((ColCount - 1) * ColumnSpacing);

    /// <summary>
    /// Renders horizontal border with configuration-appropriate characters.
    /// </summary>
    private void WriteHorizontalBorder(TextBuffer textBuffer, int[] columnWidths, BorderConfig config, BorderPosition position, Color borderColor)
    {
        if (config.SideBorders && config.ColumnSeparators)
        {
            // Full border with junctions
            (char left, char junction, char right) = GetBorderChars(position);
            WriteHorizontalBorderWithJunctions(textBuffer, columnWidths, left, junction, right, borderColor);
        }
        else if (config.SideBorders)
        {
            // Side borders only
            (char left, char _, char right) = GetBorderChars(position);
            WriteHorizontalBorderWithSides(textBuffer, columnWidths, left, right, borderColor);
        }
        else
        {
            // Simple line only
            WriteHorizontalLine(textBuffer, columnWidths, borderColor);
        }
    }

    /// <summary>
    /// Returns appropriate border characters for the specified position.
    /// </summary>
    private static (char left, char junction, char right) GetBorderChars(BorderPosition position) => position switch
    {
        BorderPosition.Top => (TopLeft, TopTee, TopRight),
        BorderPosition.Bottom => (BottomLeft, BottomTee, BottomRight),
        BorderPosition.HeaderSeparator or BorderPosition.RowSeparator => (LeftTee, Cross, RightTee),
        _ => throw new ArgumentOutOfRangeException(nameof(position))
    };

    /// <summary>
    /// Renders horizontal border with only left/right characters (no column junctions).
    /// </summary>
    private void WriteHorizontalBorderWithSides(TextBuffer textBuffer, int[] columnWidths, char left, char right, Color borderColor)
    {
        int contentWidth = CalculateContentWidth(columnWidths);
        textBuffer.Append(left, borderColor);
        textBuffer.Append(new string(Horizontal, contentWidth + (CellPadding * 2)), borderColor);
        textBuffer.Append(right, borderColor);
        textBuffer.AppendLine();
    }

    /// <summary>
    /// Renders simple horizontal line without borders.
    /// </summary>
    private void WriteHorizontalLine(TextBuffer textBuffer, int[] columnWidths, Color borderColor)
    {
        int totalWidth = CalculateContentWidth(columnWidths);
        textBuffer.Append(new string(Horizontal, totalWidth), borderColor);
        textBuffer.AppendLine();
    }

    /// <summary>
    /// Writes a character with padding spaces around it.
    /// </summary>
    private static void WriteCharWithPadding(TextBuffer textBuffer, char character, Color color)
    {
        textBuffer.Append(' ', color);
        textBuffer.Append(character, color);
        textBuffer.Append(' ', color);
    }

    /// <summary>
    /// Writes column separator: either vertical bar with spaces or just spacing.
    /// </summary>
    private static void WriteColumnSeparator(TextBuffer textBuffer, bool hasColumnSeparators, Color borderColor)
    {
        if (hasColumnSeparators)
            WriteCharWithPadding(textBuffer, Vertical, borderColor);
        else
            textBuffer.Append(new string(' ', ColumnSpacing));
    }
}
