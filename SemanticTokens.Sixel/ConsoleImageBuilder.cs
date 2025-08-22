using SixLabors.ImageSharp;
using System.Reflection;

namespace SemanticTokens.Sixel;

/// <summary>
/// Fluent builder for ConsoleImage with ultra-optimized encoding pipeline.
/// Single-use authority-driven builder following perfect execution path philosophy.
/// </summary>
public sealed class ConsoleImageBuilder
{
    private IImageSource Source { get; }
    private Size? _targetPixelSize;
    private Size? _targetCharacterSize;
    private string _fallbackText = "[image]";
    private Transparency _transparency = Transparency.Default;
    private bool _keepAspectRatio = true;

    internal ConsoleImageBuilder(IImageSource source) => Source = source;

    /// <summary>
    /// Target size in pixels.
    /// </summary>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    /// <returns>This builder for fluent configuration</returns>
    public ConsoleImageBuilder WithPixelSize(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        _targetPixelSize = new Size(width, height);
        return this;
    }

    /// <summary>
    /// Target size in pixels.
    /// </summary>
    /// <param name="size">Size in pixels</param>
    /// <returns>This builder for fluent configuration</returns>
    public ConsoleImageBuilder WithPixelSize(Size size)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size.Width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size.Height);
        _targetPixelSize = size;
        return this;
    }

    /// <summary>
    /// Target size in terminal character cells.
    /// </summary>
    /// <param name="cols">Width in character columns</param>
    /// <param name="rows">Height in character rows</param>
    /// <returns>This builder for fluent configuration</returns>
    public ConsoleImageBuilder WithCharacterSize(int cols, int rows)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cols);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        _targetCharacterSize = new Size(cols, rows);
        return this;
    }

    /// <summary>
    /// Fallback text when SIXEL unavailable.
    /// </summary>
    /// <param name="text">Text to display when SIXEL not supported</param>
    /// <returns>This builder for fluent configuration</returns>
    public ConsoleImageBuilder WithFallbackText(string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);
        _fallbackText = text;
        return this;
    }

    /// <summary>
    /// Transparency handling mode.
    /// </summary>
    /// <param name="mode">Transparency processing mode</param>
    /// <returns>This builder for fluent configuration</returns>
    public ConsoleImageBuilder WithTransparency(Transparency mode)
    {
        _transparency = mode;
        return this;
    }

    /// <summary>
    /// Aspect ratio preservation control.
    /// </summary>
    /// <param name="preserve">Whether to preserve original aspect ratio</param>
    /// <returns>This builder for fluent configuration</returns>
    public ConsoleImageBuilder PreserveAspectRatio(bool preserve = true)
    {
        _keepAspectRatio = preserve;
        return this;
    }

    /// <summary>
    /// Constructs perfect ConsoleImage with optimal encoding.
    /// </summary>
    /// <returns>Immutable ConsoleImage ready for Console.WriteImage()</returns>
    /// <exception cref="FileNotFoundException">Image source not found</exception>
    /// <exception cref="InvalidOperationException">Image encoding failed</exception>
    public ConsoleImage Build()
    {
        try
        {
            using Stream imageStream = Source.OpenStream();
            return BuildFromStream(imageStream);
        }
        catch (Exception ex) when (ex is not FileNotFoundException and not InvalidOperationException)
        {
            throw new InvalidOperationException($"Image encoding failed: {ex.Message}", ex);
        }
    }

    private ConsoleImage BuildFromStream(Stream imageStream)
    {
        Size targetSize = ComputeTargetSize();
        Console.WriteLine($"[DEBUG] Target size calculated: {targetSize.Width}x{targetSize.Height}");
        
        // Ultra-optimized single execution path
        if (SixelCapabilities.IsSupported)
        {
            try
            {
                // Encode to SIXEL using optimized pipeline
                ReadOnlySpan<char> sixelData = SixelEncode.Encode(
                    imageStream, 
                    targetSize, 
                    _transparency, 
                    frame: -1
                );

                return new ConsoleImage(sixelData.ToString(), targetSize, hasSixelData: true);
            }
            catch (Exception ex)
            {
                // Fail-fast to fallback on any encoding error
                Console.WriteLine($"[DEBUG] SIXEL encoding failed: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("[DEBUG] SIXEL not supported by terminal");
        }

        // Fallback path - terminal doesn't support SIXEL or encoding failed
        return new ConsoleImage(_fallbackText, targetSize, hasSixelData: false);
    }

    private Size ComputeTargetSize()
    {
        // Character-based sizing takes precedence (authority-driven)
        if (_targetCharacterSize.HasValue)
        {
            Size charSize = _targetCharacterSize.Value;
            Size cellSize = SixelCapabilities.CellSize;
            
            if (_keepAspectRatio)
            {
                // For aspect ratio preservation, adjust character grid to maintain square pixels
                // when cell dimensions are not square
                if (cellSize.Width != cellSize.Height)
                {
                    // Calculate square pixel dimensions using the requested character count
                    // We'll use the smaller dimension to ensure square pixels
                    int targetPixelDimension = Math.Min(
                        charSize.Width * cellSize.Width,
                        charSize.Height * cellSize.Height
                    );
                    
                    Size result = new Size(targetPixelDimension, targetPixelDimension);
                    Console.WriteLine($"[DEBUG] Character-based sizing with aspect ratio preservation:");
                    Console.WriteLine($"[DEBUG]   Requested: {charSize.Width}x{charSize.Height} chars * {cellSize.Width}x{cellSize.Height} px/cell");
                    Console.WriteLine($"[DEBUG]   Would give: {charSize.Width * cellSize.Width}x{charSize.Height * cellSize.Height} px");
                    Console.WriteLine($"[DEBUG]   Adjusted to square: {result.Width}x{result.Height} px");
                    return result;
                }
            }
            
            // Standard character-based sizing (no aspect ratio adjustment)
            Size standardResult = new Size(charSize.Width * cellSize.Width, charSize.Height * cellSize.Height);
            Console.WriteLine($"[DEBUG] Character-based sizing: {charSize.Width}x{charSize.Height} chars * {cellSize.Width}x{cellSize.Height} px/cell = {standardResult.Width}x{standardResult.Height} px");
            return standardResult;
        }

        // Use explicit pixel size if provided
        if (_targetPixelSize.HasValue)
        {
            Console.WriteLine($"[DEBUG] Using explicit pixel size: {_targetPixelSize.Value.Width}x{_targetPixelSize.Value.Height}");
            return _targetPixelSize.Value;
        }

        // Default: reasonable console image size
        Console.WriteLine("[DEBUG] Using default size: 320x240");
        return new Size(320, 240);
    }
}

/// <summary>
/// Image source abstraction for ultra-clean source handling.
/// </summary>
internal interface IImageSource
{
    Stream OpenStream();
}

/// <summary>
/// File-based image source.
/// </summary>
internal sealed class FileImageSource(string filePath) : IImageSource
{
    private readonly string _filePath = filePath;

    public Stream OpenStream()
    {
        return File.Exists(_filePath) 
            ? File.OpenRead(_filePath)
            : throw new FileNotFoundException($"Image file not found: {_filePath}");
    }
}

/// <summary>
/// Stream-based image source.
/// </summary>
internal sealed class StreamImageSource(Stream stream) : IImageSource
{
    private readonly Stream _stream = stream;

    public Stream OpenStream() => _stream;
}

/// <summary>
/// Embedded resource image source with optimized resource resolution.
/// </summary>
internal sealed class EmbeddedResourceImageSource(string resourceSuffix) : IImageSource
{
    private readonly string _resourceSuffix = resourceSuffix;

    public Stream OpenStream()
    {
        // Get the assembly containing the embedded resources (SemanticTokens.Sixel)
        Assembly assembly = typeof(ConsoleImage).Assembly;
        var allResources = assembly.GetManifestResourceNames();
        
        // Debug: show all available resources
        Console.WriteLine($"[DEBUG] Looking for resource ending with: {_resourceSuffix}");
        Console.WriteLine($"[DEBUG] Available resources: {string.Join(", ", allResources)}");
        
        string? resourceName = allResources
            .FirstOrDefault(n => n.EndsWith(_resourceSuffix, StringComparison.OrdinalIgnoreCase));
        
        return resourceName != null
            ? assembly.GetManifestResourceStream(resourceName) 
              ?? throw new FileNotFoundException($"Embedded resource stream is null: {resourceName}")
            : throw new FileNotFoundException($"Embedded resource not found: {_resourceSuffix}");
    }
}
