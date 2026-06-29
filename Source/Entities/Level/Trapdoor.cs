using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System.Collections;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's Trapdoor.cs.
///
/// A 24 × 4 px collidable trapdoor that opens when the player touches it.
///
/// Two open animations:
/// <list type="bullet">
///   <item>From top (player falls onto it) → plays "open" animation forward.</item>
///   <item>From bottom (player hits from below) → plays "open_partial" backward
///         then forward.</item>
/// </list>
///
/// Sprite loading is TODO.  Current implementation uses a placeholder grey rect.
/// </summary>
public class Trapdoor : DZ.Nez.Entity
{
    // ── Dimensions ────────────────────────────────────────────────────────────

    private const float DoorW = 24f;
    private const float DoorH =  4f;
    private const float DoorOffsetY = 6f;

    // ── State ─────────────────────────────────────────────────────────────────

    private bool _opened = false;
    private bool _collidable = true;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Trapdoor"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    public Trapdoor(Vector2 position)
    {
        Position = position;
        Name     = "Trapdoor";
        // TODO: load "trapdoor" sprite bank entry
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        if (_opened || !_collidable || Scene == null) return;

        // Door collision rect in world space.
        var doorRect = new Microsoft.Xna.Framework.Rectangle(
            (int)Position.X,
            (int)(Position.Y + DoorOffsetY),
            (int)DoorW,
            (int)DoorH);

        for (int _ti = 0; _ti < Scene.Entities.Count; _ti++)
        {
            var e = Scene.Entities[_ti];
            if (e is not MadelinePlayer player) continue;

            var pb = new Microsoft.Xna.Framework.Rectangle(
                (int)player.Position.X, (int)player.Position.Y,
                (int)player.Width,      (int)player.Height);

            if (!doorRect.Intersects(pb)) continue;

            Open(player);
            break;
        }
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public void Render()
    {
        if (_opened) return;
        // TODO: render trapdoor sprite
        Graphics.Instance.Batcher.DrawRect(Position.X, Position.Y + DoorOffsetY, DoorW, DoorH, Color.DarkGray);
    }

    // ── Open ──────────────────────────────────────────────────────────────────

    private void Open(MadelinePlayer player)
    {
        _opened    = true;
        _collidable = false;

        bool fromTop = player.Speed.Y >= 0f;

        if (fromTop)
        {
            // TODO: play "trapdoor_fromtop" sound + "open" animation
        }
        else
        {
            // TODO: play "trapdoor_frombottom" sound
            // Run the open-from-below coroutine (flip → partial open → reverse → finish).
            // Coroutine omitted — sprite logic TODO.
        }
    }
}
