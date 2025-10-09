# Goal & Scope

Introduce a simple, cell-first composition mechanism for terminal UIs—centered on **Panel**, **PanelStack**, and **BackBuffer**—to augment the existing `nterm` library. The purpose is to reliably compose text and graphics (e.g., Sixel) into discrete, fixed-size “boxes” and produce a final frame for presentation to modern terminals (Windows Terminal, xterm, etc.), without introducing layout engines, z-coordinates, or complex collision handling.

# Core Concepts

**Cell**

* The atomic visual unit in a terminal grid. A cell may carry a character (or empty) and styling attributes; panels may also map image content into cell-aligned regions.

**Panel**

* A fixed-size, rectangular, cell-based box with its own local coordinate space where the top-left is `(0,0)`.
* Positioned in **scene coordinates** by an origin `(X, Y)` that refers to the panel’s top-left corner.
* May contain text, images, or both. Word-wrapping, clipping decisions, and content mixing are handled by user code prior to writing to the panel.
* Panels are independent; they do not manage layout, anchoring relationships, or collisions.

**PanelStack**

* An **ordered collection of Panels** representing what is to be drawn.
* **Single source of truth for visibility and overlap**: rendering proceeds from the first to the last panel; where panels overlap, the **later panel wins** (its cells overwrite earlier ones).
* Adding a panel places it visually “on top”; removing it reveals what was underneath.
* Reordering panels is the mechanism for changing visual precedence (e.g., pop-ups, dialogs, transient overlays).
* No z-coordinates or implicit layout rules are involved.

**Viewport / Terminal Size**

* The target visible area (columns × rows) for a render pass. It defines what can appear on screen this frame.

**Scrollback Buffer (SB)**

* The complete scrollable content of a terminal, including the visible area and its preceding history. It represents the full textual content available for review beyond the current viewport.

**BackBuffer**

* The composed, off-screen, rectangular cell surface sized to the viewport.
* Represents the final frame intended for presentation to the terminal.
* When rendered, the content of the **BackBuffer** fully replaces the **Scrollback Buffer**, ensuring the entire terminal content is consistent with the new frame. (Partial updates to only the visible portion are possible but considered special cases, as they can be error-prone when window resizing occurs.)
* Any panel content outside the viewport is ignored during composition.

# How They Fit Together (Conceptual Flow)

1. **Prepare Panels**: User code writes text/graphics into one or more Panels using their local coordinates.
2. **Arrange Panels**: Panels are placed in scene space via their `(X, Y)` origins and added to the **PanelStack** in the intended visual order.
3. **Compose**: Given a viewport size, the system traverses the **PanelStack** from first to last, applying each panel’s visible cells onto the **BackBuffer**. Overlaps are resolved by order—**last panel wins**.
4. **Present**: The **BackBuffer** replaces the entire **Scrollback Buffer** of the terminal, ensuring all displayed and historical content reflects the latest composition.
5. **Transient UI (e.g., input pop-ups)**: Create a temporary Panel, add it to the end of the **PanelStack** (now on top), render while interacting, then remove it to restore what was beneath and render again.

# What This Achieves

* **Deterministic composition** with a minimal mental model: “ordered panels; last wins.”
* **Terminal-native constraints**: cell-based grids, fixed sizes, explicit positions, no automatic wrapping or layout surprises.
* **Unified text + graphics**: panels can hold both, enabling rich overlays and dialogs within terminal limits.
* **Simple transient UI**: pop-ups and prompts are just panels added/removed from the stack.

# Notes for nterm Integration

* Treat this as an additive composition layer atop `nterm` primitives: Panels for content, PanelStack for ordering, BackBuffer for the final frame directed to `nterm`’s output path.
* Keep naming canonical across the codebase: **Panel**, **PanelStack**, **BackBuffer**, and **Scrollback Buffer (SB)** as the terminal’s corresponding structure.
