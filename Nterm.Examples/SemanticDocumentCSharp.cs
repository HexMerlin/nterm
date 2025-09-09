using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NTerm.Document;
using System.Collections.Immutable;

namespace NTerm.Examples;

/// <summary>
/// Immutable document format containing styled characters ready for rendering.
/// Separation of content and style - each character paired with semantic style properties (that can be ignored).
/// Semantic complexity resolved during creation - renderers are trivial to implement.
/// </summary>
/// <remarks>
/// <para><strong>Creating New Renderers:</strong></para>
/// <para>Simply iterate through the document and apply each character's style:</para>
/// <para>Examples: HTML renderer, RTF export, terminal output, syntax highlighting in editors.</para>
/// <para>All semantic decisions pre-computed - no classification logic needed in renderers.</para>
/// </remarks>
public record SemanticDocumentCSharp : SemanticDocument
{
    public SemanticDocumentCSharp(ImmutableArray<(char Character, SemanticCharStyle Style)> content) : base(content) { }

    /// <summary>
    /// Creates a SemanticDocumentCSharp from a C# compilation.
    /// Supports both C# scripts and regular C# code based on compilation's SourceCodeKind.
    /// </summary>
    /// <remarks>
    /// Resolves overlapping classifications using simple span-based rules.
    /// Fully in-memory operation with zero file system dependencies.
    /// </remarks>
    /// <param name="compilation">Compilation to classify.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>SemanticDocumentCSharp containing fully resolved styled characters.</returns>
    public static async Task<SemanticDocumentCSharp> CreateAsync(Compilation compilation, CancellationToken ct = default)
    {
        (IEnumerable<ClassifiedSpan> spans, SourceText text) = await ClassifyAsync(compilation, ct).ConfigureAwait(false);
        ImmutableArray<(char Character, SemanticCharStyle Style)> semanticCharacters = [.. ResolveSemanticCharacters(text, spans)];
        return new SemanticDocumentCSharp(semanticCharacters);
    }

    /// <summary>
    /// Performs in-memory Roslyn classification of compilation source.
    /// Internal implementation detail - handles Roslyn infrastructure setup.
    /// Automatically detects and preserves SourceCodeKind (Script or Regular) from compilation.
    /// </summary>
    /// <param name="compilation">Compilation containing syntax tree to classify.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Classified spans and corresponding source text.</returns>
    private static async Task<(IEnumerable<ClassifiedSpan> spans, SourceText text)> ClassifyAsync(
        Compilation compilation, CancellationToken ct = default)
    {
        SyntaxTree tree = compilation.SyntaxTrees.Single();
        SourceText text = await tree.GetTextAsync(ct).ConfigureAwait(false);

        // Extract parse options from existing compilation - preserves SourceCodeKind
        CSharpParseOptions originalParseOptions = (CSharpParseOptions)tree.Options;
        SourceCodeKind sourceKind = originalParseOptions.Kind;
        string documentName = sourceKind == SourceCodeKind.Script ? "Script.csx" : "Code.cs";

        AdhocWorkspace workspace = new(); // in-memory workspace
        Project project = workspace.AddProject("Scratch", LanguageNames.CSharp);
        Microsoft.CodeAnalysis.Document document = workspace.AddDocument(project.Id, documentName, text);

        IEnumerable<ClassifiedSpan> spans = await Classifier.GetClassifiedSpansAsync(
            document, new TextSpan(0, text.Length), ct).ConfigureAwait(false);

        return (spans, text);
    }

    /// <summary>
    /// Resolves overlapping classifications into final styled characters.
    /// Uses simple span-based overlap resolution - shorter spans win, then direct comparison.
    /// Producer-side semantic resolution - renderer receives pre-computed visual properties.
    /// Complete separation of content and style.
    /// </summary>
    /// <param name="text">Source text containing characters to process.</param>
    /// <param name="spans">Classified spans from Roslyn analysis.</param>
    /// <returns>Array of tuples containing characters and their resolved styles.</returns>
    private static (char Character, SemanticCharStyle Style)[] ResolveSemanticCharacters(SourceText text, IEnumerable<ClassifiedSpan> spans)
    {
        ReadOnlySpan<char> textSpan = text.ToString().AsSpan();
        int length = text.Length;

        // Track winning classification per character position
        Dictionary<int, ClassifiedSpan> classificationByPosition = [];

        foreach (ClassifiedSpan span in spans)
        {
            if (span.TextSpan.Length <= 0 || span.TextSpan.End > length) continue;

            for (int i = span.TextSpan.Start; i < span.TextSpan.End; i++)
            {
                classificationByPosition[i] = classificationByPosition.TryGetValue(i, out ClassifiedSpan existing)
                    ? ResolveOverlap(existing, span)
                    : span;
            }
        }

        // Create styled characters with resolved visual properties
        (char Character, SemanticCharStyle Style)[] styledCharacters = new (char, SemanticCharStyle)[length];
        SemanticCharStyle defaultStyle = Theme.GetStyle("");

        for (int i = 0; i < length; i++)
        {
            char character = textSpan[i];

            SemanticCharStyle style = classificationByPosition.TryGetValue(i, out ClassifiedSpan classification)
                ? Theme.GetStyle(classification.ClassificationType)
                : defaultStyle;

            styledCharacters[i] = (character, style);
        }

        return styledCharacters;
    }

    /// <summary>
    /// Resolves overlap between two classified spans using simple rules.
    /// Rule 1: Shorter span wins (more specific classification).
    /// Rule 2: For identical spans, resolve by classification type comparison.
    /// </summary>
    /// <param name="spanA">First classified span.</param>
    /// <param name="spanB">Second classified span.</param>
    /// <returns>Winning classified span.</returns>
    private static ClassifiedSpan ResolveOverlap(ClassifiedSpan spanA, ClassifiedSpan spanB)
    {
        // Rule 1: Shorter span wins (more specific classification)
        if (spanA.TextSpan.Length != spanB.TextSpan.Length)
            return spanA.TextSpan.Length < spanB.TextSpan.Length ? spanA : spanB;

        // Rule 2: Identical spans - resolve by classification type
        string winningType = (spanA.ClassificationType, spanB.ClassificationType) switch
        {
            // Add more precedence rules below if needed

            // MethodName beats StaticSymbol
            (ClassificationTypeNames.StaticSymbol, ClassificationTypeNames.MethodName) => spanB.ClassificationType,
            (ClassificationTypeNames.MethodName, ClassificationTypeNames.StaticSymbol) => spanA.ClassificationType,

            // Stable fallback for unknown pairs
            _ => string.Compare(spanA.ClassificationType, spanB.ClassificationType, StringComparison.Ordinal) <= 0
                 ? spanA.ClassificationType : spanB.ClassificationType
        };

        return winningType == spanA.ClassificationType ? spanA : spanB;
    }
}
