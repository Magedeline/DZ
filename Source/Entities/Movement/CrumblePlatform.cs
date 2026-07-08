using Microsoft.Xna.Framework;
using DZ.Nez;
using System;
using System.Collections.Generic;
using DZ.Core;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

// ── State enum ─────────────────────────────────────────────────────────────────

/// <summary>Top-level lifecycle state for a <see cref="CrumblePlatform"/>.</summary>
public enum CrumblePlatformState
{
    /// <summary>Fully intact and collidable.</summary>
    Solid,
    /// <summary>Individual columns are shaking before detaching.</summary>
    Shaking,
    /// <summary>Fully crumbled; waiting to respawn.</summary>
    Gone,
    /// <summary>Respawning back into place.</summary>
    Respawning,
}

/// <summary>
/// Port of Celeste's CrumblePlatform.cs to Nez/MonoGame.
///
/// An 8-pixel-tall platform divided into one-tile (8 px) wide columns.
/// When the player stands on it each column begins shaking individually
/// (staggered by column index), then falls away.  After all columns have
/// fallen the platform waits <see cref="RespawnTime"/> seconds and then
/// restores itself.
///
/// Individual column states are tracked in <see cref="_columns"/> so a
/// wave of crumbling propagates across the platform naturally.
/// </summary>
public class CrumblePlatform : CelesteSolid
{
    // ── Tuning constants ──────────────────────────────────────────────────────

    /// <summary>Height of the platform in pixels (fixed, like Celeste).</summary>
    public const float PlatformHeight   = 8f;

    /// <summary>Width of each tile column in pixels.</summary>
    public const float TileSize         = 8f;

    /// <summary>Seconds each column shakes before falling.</summary>
    public const float ShakeTime        = 0.4f;

    /// <summary>Stagger delay between each adjacent column starting to shake (seconds).</summary>
    public const float ShakeStagger     = 0.05f;

    /// <summary>Seconds before the full platform respawns after all columns fall.</summary>
    public const float RespawnTime      = 2f;

    // ── Column state ─────────────────────────────────────────────────────────

    /// <summary>Per-column lifecycle state.</summary>
    private enum ColumnState { Solid, Shaking, Fallen }

    /// <summary>Holds runtime data for one crumble column.</summary>
    private sealed class Column
    {
        public ColumnState State = ColumnState.Solid;
        /// <summary>Countdown until this column transitions to the next state.</summary>
        public float Timer;
        /// <summary>Pixel X offset of this column within the platform.</summary>
        public float LocalX;
    }

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>Top-level platform state.</summary>
    public CrumblePlatformState State { get; private set; } = CrumblePlatformState.Solid;

    /// <summary>Per-column crumble data.</summary>
    private readonly List<Column> _columns = new();

    /// <summary>Number of tile columns derived from <see cref="CelestePlatform.Width"/>.</summary>
    private readonly int _columnCount;

    /// <summary>Respawn countdown (active only in <see cref="CrumblePlatformState.Gone"/>).</summary>
    private float _respawnTimer;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="CrumblePlatform"/>.
    /// </summary>
    /// <param name="position">Top-left world position in pixels.</param>
    /// <param name="width">Width in pixels (rounded to the nearest 8 px tile).</param>
    public CrumblePlatform(Vector2 position, float width)
        : base(position, MathF.Ceiling(width / TileSize) * TileSize, PlatformHeight)
    {
        _columnCount = (int)(Width / TileSize);
        Name         = "CrumblePlatform";

        for (int i = 0; i < _columnCount; i++)
        {
            _columns.Add(new Column { LocalX = i * TileSize });
        }

        // TODO: load sprite (tileable platform texture)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        RestoreAllColumns();
    }

    // ── Main update ───────────────────────────────────────────────────────────

    /// <summary>Drives the column state machines every frame.</summary>
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case CrumblePlatformState.Solid:
            case CrumblePlatformState.Shaking:
                UpdateSolidAndShaking();
                break;

            case CrumblePlatformState.Gone:
                UpdateGone();
                break;

            case CrumblePlatformState.Respawning:
                // Respawn happens instantly; transition back to Solid.
                State = CrumblePlatformState.Solid;
                break;
        }
    }

    // ── State: Solid / Shaking ────────────────────────────────────────────────

    private void UpdateSolidAndShaking()
    {
        float dt     = Time.DeltaTime;
        bool  riding = false;

        var player = GetPlayer();
        if (player != null)
            riding = IsPlayerRiding(player);

        // If the player just landed, start shaking all non-yet-started columns.
        if (riding && State == CrumblePlatformState.Solid)
        {
            State = CrumblePlatformState.Shaking;
            for (int i = 0; i < _columns.Count; i++)
            {
                var col = _columns[i];
                if (col.State == ColumnState.Solid)
                {
                    col.State = ColumnState.Shaking;
                    col.Timer = ShakeTime + i * ShakeStagger;
                }
            }
            // TODO: play sound (crumble warning)
        }

        // Update each column.
        int fallenCount = 0;
        for (int i = 0; i < _columns.Count; i++)
        {
            var col = _columns[i];
            switch (col.State)
            {
                case ColumnState.Shaking:
                    col.Timer -= dt;
                    if (col.Timer <= 0f)
                    {
                        col.State = ColumnState.Fallen;
                        // TODO: emit particles at (Position.X + col.LocalX, Position.Y)
                        // TODO: play sound (tile fall)
                    }
                    break;

                case ColumnState.Fallen:
                    fallenCount++;
                    break;
            }
        }

        // All columns fallen → go fully gone.
        if (fallenCount == _columns.Count && State == CrumblePlatformState.Shaking)
        {
            State       = CrumblePlatformState.Gone;
            Collidable  = false;
            _respawnTimer = RespawnTime;
        }
    }

    // ── State: Gone ───────────────────────────────────────────────────────────

    private void UpdateGone()
    {
        _respawnTimer -= Time.DeltaTime;
        if (_respawnTimer <= 0f)
        {
            RestoreAllColumns();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Resets all columns and makes the platform solid again.</summary>
    private void RestoreAllColumns()
    {
        foreach (var col in _columns)
        {
            col.State = ColumnState.Solid;
            col.Timer = 0f;
        }

        Collidable    = true;
        State         = CrumblePlatformState.Solid;
        _respawnTimer = 0f;
        StopShaking();
        // TODO: emit particles (respawn flash)
        // TODO: play sound (respawn)
    }

    // ── Visual helpers (for renderer use) ─────────────────────────────────────

    /// <summary>
    /// Returns the visual Y offset of the column at index <paramref name="i"/>
    /// based on its shake state (for a simple up/down wiggle).
    /// </summary>
    public float GetColumnShakeOffset(int i)
    {
        if (i < 0 || i >= _columns.Count) return 0f;
        var col = _columns[i];
        if (col.State != ColumnState.Shaking) return 0f;
        // Simple sine-based shake scaled by remaining time.
        return MathF.Sin(col.Timer * 60f) * 1f;
    }

    /// <summary>Returns <c>true</c> if column <paramref name="i"/> has fallen.</summary>
    public bool IsColumnFallen(int i)
        => i >= 0 && i < _columns.Count && _columns[i].State == ColumnState.Fallen;

    /// <summary>
    /// Advances <paramref name="current"/> toward <paramref name="target"/> by at most
    /// <paramref name="maxDelta"/>, never overshooting.
    /// </summary>
    private static float Approach(float current, float target, float maxDelta)
    {
        float diff = target - current;
        return current + Math.Sign(diff) * Math.Min(maxDelta, Math.Abs(diff));
    }
}
