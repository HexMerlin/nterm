namespace Nterm.Core;

/// <summary>
/// Size struct with Width and Height
/// </summary>
public readonly struct Size : IEquatable<Size>
{
    public int Width { get; }

    public int Height { get; }

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public bool Equals(Size other) => Width == other.Width && Height == other.Height;

    public override bool Equals(object? obj) => obj is Size size && Equals(size);

    public override int GetHashCode() => throw new NotImplementedException();

    public static bool operator ==(Size left, Size right) => left.Equals(right);

    public static bool operator !=(Size left, Size right) => !(left == right);

    public override string ToString() => $"{Width}x{Height} (W x H)";
}
