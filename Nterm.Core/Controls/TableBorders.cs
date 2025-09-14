namespace Nterm.Core.Controls;

/// <summary>
/// Border style options for table rendering.
/// </summary>
public enum TableBorders
{
    /// <summary>No borders at all - clean text-only output with column alignment.</summary>
    None,

    /// <summary>Complete grid: horizontal lines between every row and vertical lines between every column.</summary>
    Grid,

    /// <summary>Only the outer border + line under header (no internal column separators).</summary>
    OutlineAndHeader,

    /// <summary>Horizontal separators only between data rows (no lines before header or after last row).</summary>
    Rows,

    /// <summary>Line under the header row only (Markdown-style tables).</summary>
    Header
}
