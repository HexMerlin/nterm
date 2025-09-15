
namespace Nterm.Core.Buffer;

/// <summary>
/// Descriptor for visual properties for rendering text.
/// </summary>
/// <param name="Color">Foreground color.</param>
/// <param name="BackColor">Background color.</param>
public readonly record struct CharStyle(Color Color, Color BackColor);
