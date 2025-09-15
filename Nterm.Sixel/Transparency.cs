namespace Nterm.Sixel;

public enum Transparency
{
    Default,    // Standard transparency (palette or alpha channel)
    TopLeft,    // Make the color found at the top left corner (0, 0) transparent
    Background, // Make the background color transparent (for some GIF or WebP images)
    None        // No transparency
}
