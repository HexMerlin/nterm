using System.Collections;
using System.Collections.Immutable;

namespace NTerm.Document;

/// <summary>
/// Abstract record for a semantic document .
/// Provides immutable, read-only access to characters paired with their semantic or visual styles.
/// Designed for easy renderer implementation across multiple output formats.
/// </summary>
/// <remarks>
/// <para><strong>Document Concept:</strong></para>
/// <para>Represents a complete document with resolved styling - not a stream.</para>
/// <para>Supports random access, length queries, and efficient enumeration.</para>
/// <para><strong>Renderer Implementation:</strong></para>
/// <code>
/// foreach ((char character, SemanticCharStyle style) in document)
/// {
///     // Render character accoring to style
/// }
/// </code>
///</remarks>
public abstract record StyledDocument : IReadOnlyList<(char, CharStyle)>
{
    private readonly ImmutableArray<(char Character, CharStyle Style)> Content;

    public int Count => Content.Length;

    public (char, CharStyle) this[int index] => Content[index];

    public StyledDocument(ImmutableArray<(char Character, CharStyle Style)> content) => Content = content;

    // Fast path: pattern-based foreach picks this (struct, no allocations).
    public ImmutableArray<(char, CharStyle)>.Enumerator GetEnumerator()
        => Content.GetEnumerator();

    // Interface paths (used if caller has IEnumerable<T>):
    IEnumerator<(char, CharStyle)> IEnumerable<(char, CharStyle)>.GetEnumerator()
        => ((IEnumerable<(char, CharStyle)>)Content).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)Content).GetEnumerator();
}
