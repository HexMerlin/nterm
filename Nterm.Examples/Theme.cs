
using Microsoft.CodeAnalysis.Classification;
using NTerm.Document;
using C = NTerm.Core.Color;

namespace NTerm.Examples;

/// <summary>
/// Default theme for <see cref="StyledDocument"/> classifications.
/// </summary>
public static class Theme
{
    // Private helpers used by GetStyle below
    private static CharStyle Default => new(C.Gray, C.Black);
    private static CharStyle Keyword => Default with { Color = C.Blue };
    private static CharStyle Type => Default with { Color = C.DarkCyan };
    private static CharStyle Member => Default with { Color = C.Magenta };
    private static CharStyle String => Default with { Color = C.Orange };
    private static CharStyle Number => Default with { Color = C.Cyan };
    private static CharStyle Comment => Default with { Color = C.DarkGreen };
    private static CharStyle Operator => Default with { Color = C.DarkGray };

    /// <summary>
    /// Maps Roslyn ClassificationTypeNames to complete style descriptors.
    /// Single source of truth for all visual properties.
    /// </summary>
    /// <param name="classificationType">Roslyn classification type name from <see cref="ClassificationTypeNames"/>.</param>
    /// <returns>Complete style descriptor containing all visual properties for the classification.</returns>
    /// <remarks>
    /// <para>Returns styled variations of template instances using <c>with</c> expressions.</para>
    /// <para>Unknown classification types return <see cref="Default"/> style.</para>
    /// </remarks>
    public static CharStyle GetStyle(string classificationType) => classificationType switch
    {
        // Keywords and control flow
        ClassificationTypeNames.Keyword or
        ClassificationTypeNames.PreprocessorKeyword => Keyword,
        ClassificationTypeNames.ControlKeyword => Keyword with { Color = C.DarkBlue },

        // Types and type-related
        ClassificationTypeNames.ClassName or
        ClassificationTypeNames.StructName or
        ClassificationTypeNames.InterfaceName or
        ClassificationTypeNames.EnumName or
        ClassificationTypeNames.DelegateName => Type,
        ClassificationTypeNames.TypeParameterName => Type with { Color = C.Cyan },
        ClassificationTypeNames.NamespaceName => Type with { Color = C.DarkGray },

        // Members and symbols
        ClassificationTypeNames.FieldName or
        ClassificationTypeNames.EnumMemberName or
        ClassificationTypeNames.ConstantName or
        ClassificationTypeNames.LocalName or
        ClassificationTypeNames.ParameterName or
        ClassificationTypeNames.Identifier => Default,
        ClassificationTypeNames.MethodName => Member,
        ClassificationTypeNames.ExtensionMethodName => Member with { Color = C.DarkMagenta },
        ClassificationTypeNames.PropertyName => Member with { Color = C.Yellow },
        ClassificationTypeNames.EventName => Member with { Color = C.DarkGoldenrod },

        // Literals and values
        ClassificationTypeNames.StringLiteral or
        ClassificationTypeNames.VerbatimStringLiteral => String,
        ClassificationTypeNames.NumericLiteral => Number,

        // Comments and documentation
        ClassificationTypeNames.Comment => Comment,
        ClassificationTypeNames.XmlDocCommentComment or
        ClassificationTypeNames.XmlDocCommentText => Comment with { Color = C.Green },

        // Operators and punctuation
        ClassificationTypeNames.Operator or
        ClassificationTypeNames.OperatorOverloaded => Operator,

        // Special cases
        ClassificationTypeNames.ExcludedCode => Operator,
        ClassificationTypeNames.StaticSymbol => Default with { Color = C.White },

        _ => Default
    };
}
