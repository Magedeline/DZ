using Microsoft.Xna.Framework;
using Nez;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's ClimbBlocker.cs.
///
/// A non-active, non-visible <see cref="Nez.Component"/> that signals the player
/// system that the owning entity cannot be wall-climbed.
///
/// When <see cref="Edge"/> is <c>true</c> it also blocks the player from
/// latching onto the ledge edge of the entity (used by InvisibleBarrier).
///
/// Usage:
/// <code>
///   myEntity.AddComponent(new ClimbBlocker(edge: false));
/// </code>
///
/// The player's climbing logic should call <see cref="Check(Nez.Scene, Nez.Entity)"/>
/// each frame to determine whether the wall in front of it is climbable.
/// </summary>
public class ClimbBlocker : Nez.Component
{
    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>When <c>false</c>, this blocker is temporarily inactive.</summary>
    public bool Blocking = true;

    /// <summary>
    /// When <c>true</c>, also blocks ledge-edge grabs
    /// (e.g., on <see cref="InvisibleBarrier"/>).
    /// </summary>
    public bool Edge { get; }

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="ClimbBlocker"/>.
    /// </summary>
    /// <param name="edge">
    ///   <c>true</c> to also block edge/ledge grabs.
    /// </param>
    public ClimbBlocker(bool edge)
    {
        Edge = edge;
    }

    // ── Static scene queries ──────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> if <paramref name="entity"/> overlaps any active
    /// <see cref="ClimbBlocker"/> in <paramref name="scene"/>.
    /// </summary>
    public static bool Check(Nez.Scene scene, Nez.Entity entity)
    {
        for (int _cbi = 0; _cbi < scene.Entities.Count; _cbi++)
        {
            var e = scene.Entities[_cbi];
            var cb = e.GetComponent<ClimbBlocker>();
            if (cb == null || !cb.Blocking) continue;

            if (Overlaps(entity, e))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="entity"/> overlaps any active
    /// <see cref="ClimbBlocker"/> when placed at <paramref name="at"/>.
    /// </summary>
    public static bool Check(Nez.Scene scene, Nez.Entity entity, Vector2 at)
    {
        Vector2 orig = entity.Position;
        entity.Position = at;
        bool result = Check(scene, entity);
        entity.Position = orig;
        return result;
    }

    /// <summary>
    /// Returns <c>true</c> if any <see cref="Edge"/>-marked blocker is touched
    /// when <paramref name="player"/> moves <paramref name="dir"/> pixels on X.
    /// </summary>
    public static bool EdgeCheck(Nez.Scene scene, Nez.Entity entity, int dir)
    {
        for (int _cbj = 0; _cbj < scene.Entities.Count; _cbj++)
        {
            var e = scene.Entities[_cbj];
            var cb = e.GetComponent<ClimbBlocker>();
            if (cb == null || !cb.Blocking || !cb.Edge) continue;

            Vector2 testPos = entity.Position + new Vector2(dir, 0f);
            if (OverlapsAt(entity, testPos, e))
                return true;
        }
        return false;
    }

    // ── AABB helpers ──────────────────────────────────────────────────────────

    private static bool Overlaps(Nez.Entity a, Nez.Entity b)
    {
        var colA = a.GetComponent<Nez.Collider>();
        var colB = b.GetComponent<Nez.Collider>();
        if (colA == null || colB == null) return false;

        var ra = new Microsoft.Xna.Framework.Rectangle(
            (int)colA.Bounds.X, (int)colA.Bounds.Y,
            (int)colA.Bounds.Width, (int)colA.Bounds.Height);
        var rb = new Microsoft.Xna.Framework.Rectangle(
            (int)colB.Bounds.X, (int)colB.Bounds.Y,
            (int)colB.Bounds.Width, (int)colB.Bounds.Height);

        return ra.Intersects(rb);
    }

    private static bool OverlapsAt(Nez.Entity a, Vector2 aPos, Nez.Entity b)
    {
        Vector2 orig = a.Position;
        a.Position = aPos;
        bool result = Overlaps(a, b);
        a.Position = orig;
        return result;
    }
}
