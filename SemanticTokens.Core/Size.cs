namespace SemanticTokens.Core;

/// <summary>
/// Size struct with Width and Height
/// </summary>
public readonly struct Size : IEquatable<Size>
{

    public int Width { get; }

    public int Height { get; }

    public Size(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }

    public bool Equals(Size other) => Width == other.Width && Height == other.Height;

}
