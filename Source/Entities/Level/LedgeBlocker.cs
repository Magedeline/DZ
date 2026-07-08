using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using System;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's LedgeBlocker.cs.
///
/// A non-active, non-visible <see cref="DZ.Nez.Component"/> that prevents the
/// player from performing ledge-hop, jump-thru boost, or dash-correct
/// manoeuvres on the entity it is attached to.
///
/// Usage:
/// <code>
///   myEntity.AddComponent(new LedgeBlocker());
/// </code>
///
/// The player's movement code should call the relevant static check methods
/// to determine whether an action is blocked.
/// </summary>
public class LedgeBlocker : DZ.Nez.Component
{
    // ── State ─────────────────────────────────────────────────────────────────

    /// <summary>When <c>false</c>, all blocking is suspended.</summary>
    public bool Blocking = true;

    /// <summary>
    /// Optional per-player predicate; when non-null the block only applies
    /// if the predicate returns <c>true</c> for the querying player.
    /// </summary>
    public Func<MadelinePlayer, bool> BlockChecker;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="LedgeBlocker"/>.
    /// </summary>
    /// <param name="blockChecker">
    ///   Optional predicate; null means always block when <see cref="Blocking"/> is true.
    /// </param>
    public LedgeBlocker(Func<MadelinePlayer, bool> blockChecker = null)
    {
        BlockChecker = blockChecker;
    }

    // ── Hop-block check ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> if this blocker prevents a ledge-hop for
    /// <paramref name="player"/> (checks 8 px ahead in facing direction).
    /// </summary>
    public bool HopBlockCheck(MadelinePlayer player)
    {
        if (!Blocking || Entity == null) return false;

        // Check if player would overlap us 8 px ahead.
        Vector2 testPos = player.Position + new Vector2((int)player.Facing * 8f, 0f);
        if (!OverlapsAt(player, testPos)) return false;

        return BlockChecker == null || BlockChecker(player);
    }

    // ── Jump-thru boost check ─────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> if this blocker prevents a jump-thru boost
    /// (checks 2 px above the player).
    /// </summary>
    public bool JumpThruBoostCheck(MadelinePlayer player)
    {
        if (!Blocking || Entity == null) return false;

        Vector2 testPos = player.Position - new Vector2(0f, 2f);
        if (!OverlapsAt(player, testPos)) return false;

        return BlockChecker == null || BlockChecker(player);
    }

    // ── Dash-correct check ────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> if this blocker prevents a dash correction at
    /// the player's current position.
    /// </summary>
    public bool DashCorrectCheck(MadelinePlayer player)
    {
        if (!Blocking || Entity == null) return false;

        if (!OverlapsAt(player, player.Position)) return false;

        return BlockChecker == null || BlockChecker(player);
    }

    // ── AABB helper ───────────────────────────────────────────────────────────

    private bool OverlapsAt(MadelinePlayer player, Vector2 testPos)
    {
        if (Entity == null) return false;

        var col = Entity.GetComponent<DZ.Nez.Collider>();
        if (col == null) return false;

        var eb = new Microsoft.Xna.Framework.Rectangle(
            (int)col.Bounds.X, (int)col.Bounds.Y,
            (int)col.Bounds.Width, (int)col.Bounds.Height);

        var pb = new Microsoft.Xna.Framework.Rectangle(
            (int)testPos.X, (int)testPos.Y,
            (int)player.Width, (int)player.Height);

        return eb.Intersects(pb);
    }
}
