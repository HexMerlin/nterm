using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;

namespace SemanticTokens.Sixel;

/// <summary>
/// Represents console image dimensions in character cells.
/// Provides precise character grid calculations for SIXEL images.
/// </summary>
public readonly struct ConsoleImageCharacterSize : IEquatable<ConsoleImageCharacterSize>
{
    /// <summary>
    /// Width in character columns.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Height in character rows.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Precision confidence level.
    /// </summary>
    public CharacterSizePrecision Precision { get; }

    /// <summary>
    /// Initializes character size with specified dimensions.
    /// </summary>
    /// <param name="columns">Width in character columns</param>
    /// <param name="rows">Height in character rows</param>
    /// <param name="precision">Calculation precision level</param>
    public ConsoleImageCharacterSize(int columns, int rows, CharacterSizePrecision precision = CharacterSizePrecision.Exact)
    {
        Columns = columns;
        Rows = rows;
        Precision = precision;
    }

    /// <summary>
    /// Calculates character dimensions from pixel size and terminal capabilities.
    /// </summary>
    /// <param name="pixelSize">Image size in pixels</param>
    /// <param name="addSafetyMargin">Add +1 cell in each dimension for guaranteed robustness</param>
    /// <returns>Character grid dimensions with precision indicator</returns>
    public static ConsoleImageCharacterSize FromPixelSize(ConsoleImageSize pixelSize, bool addSafetyMargin = false)
    {
        Size cellSize = SixelCapabilities.CellSize;
        
        // Calculate exact character dimensions using ceiling to ensure bounding box coverage
        int columns = (int)Math.Ceiling((double)pixelSize.Width / cellSize.Width);
        int rows = (int)Math.Ceiling((double)pixelSize.Height / cellSize.Height);
        
        // Add safety margin for bulletproof robustness if requested
        if (addSafetyMargin)
        {
            columns += 1;
            rows += 1;
        }
        
        // Determine precision based on detection confidence and safety margin
        CharacterSizePrecision precision = addSafetyMargin 
            ? CharacterSizePrecision.Exact  // Safety margin guarantees exactness
            : DeterminePrecision(cellSize);
        
        return new ConsoleImageCharacterSize(columns, rows, precision);
    }

    /// <summary>
    /// Calculates character dimensions from ConsoleImage.
    /// </summary>
    /// <param name="image">Console image with pixel dimensions</param>
    /// <param name="addSafetyMargin">Add +1 cell in each dimension for guaranteed robustness</param>
    /// <returns>Character grid dimensions</returns>
    public static ConsoleImageCharacterSize FromConsoleImage(ConsoleImage image, bool addSafetyMargin = false) =>
        FromPixelSize(image.DisplaySize, addSafetyMargin);

    private static CharacterSizePrecision DeterminePrecision(Size cellSize)
    {
        // Default size indicates detection failure - use estimated precision
        if (cellSize.Width == 10 && cellSize.Height == 20)
            return CharacterSizePrecision.Estimated;
            
        // Very small or very large cells might indicate detection issues
        if (cellSize.Width < 6 || cellSize.Width > 20 || 
            cellSize.Height < 10 || cellSize.Height > 40)
            return CharacterSizePrecision.Approximate;
            
        // Standard ranges indicate reliable detection
        return CharacterSizePrecision.Exact;
    }

    /// <summary>
    /// Indicates whether this size is equal to another size.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ConsoleImageCharacterSize other) => 
        Columns == other.Columns && Rows == other.Rows && Precision == other.Precision;

    public override bool Equals(object? obj) => obj is ConsoleImageCharacterSize other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Columns, Rows, Precision);
    
    public static bool operator ==(ConsoleImageCharacterSize left, ConsoleImageCharacterSize right) => left.Equals(right);
    public static bool operator !=(ConsoleImageCharacterSize left, ConsoleImageCharacterSize right) => !left.Equals(right);

    public override string ToString() => $"{Columns}×{Rows} chars ({Precision})";
}

/// <summary>
/// Precision level for character size calculations.
/// </summary>
public enum CharacterSizePrecision
{
    /// <summary>
    /// Exact calculation with high confidence (±0 characters).
    /// Terminal cell size detected accurately.
    /// </summary>
    Exact,
    
    /// <summary>
    /// Approximate calculation (±1 character possible).
    /// Terminal cell size detected but with some uncertainty.
    /// </summary>
    Approximate,
    
    /// <summary>
    /// Estimated calculation using default values (±1-2 characters possible).
    /// Terminal cell size detection failed, using fallback assumptions.
    /// </summary>
    Estimated
}
