using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's WhiteBlock.cs.
///
/// A jump-through platform found in Chapter 4 (Cliffside).  When a player
/// crouches on it for <see cref="DuckDuration"/> seconds the block activates:
/// it becomes non-collidable, raises the player's render depth so they appear
/// behind background tiles, and adds a temporary solid grid built from the
/// level's background tile data.
///
/// The block resets (disables) once the Heart Gem has been collected, or when
/// the activated state ends and the player returns.
///
/// Notes:
/// - The background-solid-grid creation is simplified; full SaveData/TileBounds
///   integration is marked TODO.
/// - No sprite atlas loading — TODO: wire up texture.
/// </summary>
public class WhiteBlock : CelesteJumpThru
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float DuckDuration = 3f; // seconds of continuous crouching to activate

    // ── Width ─────────────────────────────────────────────────────────────────

    private const int BlockWidth = 48;

    // ── State ─────────────────────────────────────────────────────────────────

    private bool  _enabled   = true;
    private bool  _activated = false;
    private float _playerDuckTimer;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="WhiteBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    public WhiteBlock(Vector2 position)
        : base(position, BlockWidth)
    {
        Name = "WhiteBlock";
        // TODO: load "objects/whiteblock" sprite
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        // If a heart gem has already been collected, disable immediately.
        // TODO: check SaveData / session HeartGem flag
        // Disable();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        if (!_enabled) return;

        if (!_activated)
        {
            // Look for a ducking player on top.
            var player = GetPlayerRider();
            bool playerDucking = false; // TODO: read player.Ducking

            if (player != null && playerDucking)
            {
                _playerDuckTimer += Time.DeltaTime;
                if (_playerDuckTimer >= DuckDuration)
                    Activate(player);
            }
            else
            {
                _playerDuckTimer = 0f;
            }

            // TODO: check HeartGem collected → Disable()
        }
        else
        {
            // Once activated, disable once the heart gem is gone.
            // TODO: check scene for HeartGem entity; if absent, Disable()
        }
    }

    // ── Activate / Disable ────────────────────────────────────────────────────

    private void Activate(DZ.Entities.Player.MadelinePlayer player)
    {
        // TODO: play "event:/game/04_cliffside/whiteblock_fallthru" sound
        _activated = true;
        Collidable = false;

        // TODO: add background solid grid from level BG tile data
        // This requires access to the level's BgData VirtualMap<char> — stub for now.
    }

    private void Disable()
    {
        _enabled   = false;
        Collidable = false;
        // TODO: tint sprite to Color.White * 0.25f
    }

    // ── Rider helpers ─────────────────────────────────────────────────────────

    private DZ.Entities.Player.MadelinePlayer GetPlayerRider()
    {
        if (Scene == null) return null;
        for (int _wi = 0; _wi < Scene.Entities.Count; _wi++)
            if (Scene.Entities[_wi] is DZ.Entities.Player.MadelinePlayer p && IsRiding(p))
                return p;
        return null;
    }

    private bool IsRiding(DZ.Entities.Player.MadelinePlayer player) =>
        Math.Abs(player.Position.Y + player.Height - Position.Y) <= 2f
        && player.Position.X + player.Width > Position.X
        && player.Position.X < Position.X + Width;
}
