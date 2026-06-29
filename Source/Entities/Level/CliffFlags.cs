using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;

namespace DZ.Entities.Level;

/// <summary>
/// Small decorative pennant flags strung between two cliff anchor points.
/// Ported from Celeste's CliffFlags.cs.
///
/// Delegates all rendering to a <see cref="Flagline"/> component configured
/// with the four traditional cliff flag colours, short cloth pieces, and a
/// subtle drooping amount.
/// </summary>
public class CliffFlags : DZ.Nez.Component
{
    // -------------------------------------------------------------------------
    // Colour palette (matching original hex values)
    // -------------------------------------------------------------------------

    private static readonly Color[] FlagColors = new Color[]
    {
        new Color(0xD8, 0x5F, 0x2F), // burnt orange
        new Color(0xD8, 0x2F, 0x63), // crimson rose
        new Color(0x2F, 0xD8, 0xA2), // seafoam
        new Color(0xD8, 0xD6, 0x2F), // golden yellow
    };

    private static readonly Color LineColor = Color.Lerp(Color.Gray, Color.DarkBlue, 0.25f);
    private static readonly Color PinColor  = Color.Gray;

    // -------------------------------------------------------------------------
    // Nested component
    // -------------------------------------------------------------------------

    private Flagline? _flagline;

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    private readonly Vector2 _from;
    private readonly Vector2 _to;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="from">First anchor point in world space.</param>
    /// <param name="to">Second anchor point in world space.</param>
    public CliffFlags(Vector2 from, Vector2 to)
    {
        _from = from;
        _to   = to;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _from;

        // Add the Flagline component to the same entity
        _flagline = Entity.AddComponent(new Flagline(
            to:              _to,
            lineColor:       LineColor,
            pinColor:        PinColor,
            colors:          FlagColors,
            minFlagHeight:   10,
            maxFlagHeight:   10,
            minFlagLength:   10,
            maxFlagLength:   10,
            minSpace:         2,
            maxSpace:         8));

        _flagline.ClothDroopAmount = 0.2f;

        // Render depth 8999 (behind mid-ground)
        // TODO: set Entity render layer
    }
}
