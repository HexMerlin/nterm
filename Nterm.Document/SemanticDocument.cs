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
public abstract record SemanticDocument : IReadOnlyList<(char, SemanticCharStyle)>
{
    private readonly ImmutableArray<(char Character, SemanticCharStyle Style)> Content;

    public int Count => Content.Length;

    public (char, SemanticCharStyle) this[int index] => Content[index];

    public SemanticDocument(ImmutableArray<(char Character, SemanticCharStyle Style)> content) => Content = content;

    // Fast path: pattern-based foreach picks this (struct, no allocations).
    public ImmutableArray<(char, SemanticCharStyle)>.Enumerator GetEnumerator()
        => Content.GetEnumerator();

    // Interface paths (used if caller has IEnumerable<T>):
    IEnumerator<(char, SemanticCharStyle)> IEnumerable<(char, SemanticCharStyle)>.GetEnumerator()
        => ((IEnumerable<(char, SemanticCharStyle)>)Content).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)Content).GetEnumerator();
}
