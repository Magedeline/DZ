using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using DZ.Entities.Player;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's SafeGroundBlocker.cs.
///
/// A non-updating, non-rendering <see cref="DZ.Nez.Component"/> that, when
/// attached to an entity, prevents the player from treating that entity as
/// "safe ground" (i.e., a respawn point after death).
///
/// Usage:
/// <code>
///   someEntity.AddComponent(new SafeGroundBlocker());
/// </code>
///
/// The game's player/respawn system should call <see cref="Check"/> on all
/// <see cref="SafeGroundBlocker"/> components in the scene to determine
/// whether a given surface is safe.
/// </summary>
public class SafeGroundBlocker : DZ.Nez.Component
{
    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// When <c>false</c>, this blocker is temporarily inactive and
    /// <see cref="Check"/> always returns <c>false</c>.
    /// </summary>
    public bool Blocking = true;

    /// <summary>
    /// Optional secondary collider to test against instead of the entity's
    /// default collider.  Null means use the entity's own collider.
    /// </summary>
    public DZ.Nez.Collider CheckWith;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="SafeGroundBlocker"/>.
    /// </summary>
    /// <param name="checkWith">
    ///   Optional override collider for the check.  Null = use entity collider.
    /// </param>
    public SafeGroundBlocker(DZ.Nez.Collider checkWith = null)
    {
        CheckWith = checkWith;
    }

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> if the given <paramref name="player"/> overlaps
    /// the entity (or <see cref="CheckWith"/> collider) while this blocker
    /// is active.
    /// </summary>
    /// <param name="player">The player entity to test against.</param>
    public bool Check(MadelinePlayer player)
    {
        if (!Blocking || Entity == null) return false;

        // Determine which collider to use.
        var col = CheckWith ?? Entity.GetComponent<DZ.Nez.Collider>();
        if (col == null) return false;

        // Axis-aligned bounding-box overlap between player and our collider.
        var playerBounds = new Microsoft.Xna.Framework.Rectangle(
            (int)player.Position.X, (int)player.Position.Y,
            (int)player.Width,      (int)player.Height);

        var myBounds = col.Bounds;

        return playerBounds.Intersects(new Microsoft.Xna.Framework.Rectangle(
            (int)myBounds.X, (int)myBounds.Y,
            (int)myBounds.Width, (int)myBounds.Height));
    }

    /// <summary>
    /// Convenience: checks all <see cref="SafeGroundBlocker"/> components in
    /// <paramref name="scene"/> and returns <c>true</c> if any block the player.
    /// </summary>
    public static bool CheckAll(DZ.Nez.Scene scene, MadelinePlayer player)
    {
        for (int _sgbi = 0; _sgbi < scene.Entities.Count; _sgbi++)
        {
            var blocker = scene.Entities[_sgbi].GetComponent<SafeGroundBlocker>();
            if (blocker != null && blocker.Check(player))
                return true;
        }
        return false;
    }
}
