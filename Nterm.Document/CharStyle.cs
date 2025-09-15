using Nterm.Core;

namespace Nterm.Document;

/// <summary>
/// Descriptor for semantic and visual properties for rendering characters.
/// </summary>
/// <param name="Color">Foreground color for the character.</param>
/// <param name="BackColor">Background color for the character.</param>
/// <remarks>
/// <para>Used in <see cref="SemanticDocumentCSharp"/> to pair each character with its style.</para>
/// </remarks>
public readonly record struct CharStyle(Color Color, Color BackColor);
