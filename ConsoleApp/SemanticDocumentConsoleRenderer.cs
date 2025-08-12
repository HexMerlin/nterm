using SemanticDocuments;

namespace ConsoleApp;

/// <summary>
/// Ultra-simple console renderer for <see cref="ISemanticDocument"/>.
/// Demonstrates how easy it is to create renderers for the ISemanticDocument format.
/// Applies pre-computed visual properties - no semantic decisions required.
/// All classification resolution handled by producer-side semantic document implementations.
/// </summary>
/// <remarks>
/// <para><strong>Implementation Pattern:</strong></para>
/// <para>1. Iterate through the semantic document's styled characters</para>
/// <para>2. Apply each character's style properties (colors, etc.)</para>
/// <para>3. Render the character</para>
/// <para><strong>Performance:</strong></para>
/// <para>Console class optimizes color changes internally - no manual optimization needed.</para>
/// <para>Restores original console colors when finished.</para>
/// <para><strong>Other Renderer Examples:</strong></para>
/// <para>HTML export, RTF documents, terminal emulators, code editors, etc.</para>
/// </remarks>
public static class SemanticDocumentConsoleRenderer
{
    /// <summary>
    /// Renders <see cref="ISemanticDocument"/> to console using pre-computed character visual properties.
    /// Single responsibility: Apply colors and render characters in sequence.
    /// Zero semantic complexity - all decisions made during document creation.
    /// Perfect separation of content and style.
    /// </summary>
    /// <param name="document"><see cref="ISemanticDocument"/> containing styled characters to render.</param>
    /// <remarks>
    /// <para>Ultra-lean implementation demonstrating renderer simplicity:</para>
    /// <para>1. Save original colors</para>
    /// <para>2. For each character: set colors and write character</para>
    /// <para>3. Restore original colors</para>
    /// <para>Console class optimizes color changes internally - no manual optimization needed.</para>
    /// </remarks>
    public static void Render(SemanticDocument document)
    {
        (ConsoleColor, ConsoleColor) initColors = (Console.ForegroundColor, Console.BackgroundColor);
                
        foreach ((char character, SemanticCharStyle style) in document)
        {
            Console.ForegroundColor = style.Color;
            Console.BackgroundColor = style.BackColor;
            Console.Write(character);
        }
        
        (Console.ForegroundColor, Console.BackgroundColor) = initColors;
    }
}
