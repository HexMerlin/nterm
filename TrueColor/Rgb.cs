using System.Runtime.CompilerServices;

namespace TrueColor;

public readonly struct Rgb : IEquatable<Rgb>
{
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    public Rgb(byte r, byte g, byte b) => (R, G, B) = (r, g, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Rgb other) => R == other.R && G == other.G && B == other.B;
    public override bool Equals(object? obj) => obj is Rgb o && Equals(o);
    public override int GetHashCode() => (R << 16) | (G << 8) | B;
    public static bool operator ==(Rgb a, Rgb b) => a.Equals(b);
    public static bool operator !=(Rgb a, Rgb b) => !a.Equals(b);
}
