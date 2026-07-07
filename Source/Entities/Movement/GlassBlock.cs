using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using System.Collections.Generic;
using DZ.Entities.Core;

namespace DZ.Entities.Movement;

/// <summary>
/// Port of Celeste's GlassBlock.cs.
///
/// A solid block that is visually transparent — only its exposed edges are
/// rendered as white lines.  When <see cref="Sinks"/> is <c>true</c>, the
/// block sinks slowly under the player's weight (identical lerp to
/// <see cref="GoldenBlock"/> / <see cref="StarJumpBlock"/>).
///
/// Edge-line geometry is computed at scene-add time by scanning which tile
/// faces are open (not adjacent to another solid or glass block).
/// </summary>
public class GlassBlock : CelesteSolid
{
    // ── Tuning ────────────────────────────────────────────────────────────────

    private const float SinkHold      = 0.1f;
    private const float SinkLerpRate  = 1f;
    private const float SinkMaxOffset = 12f;

    // ── Configuration ─────────────────────────────────────────────────────────

    /// <summary>Whether the block sinks when the player stands on it.</summary>
    public bool Sinks { get; }

    // ── State ─────────────────────────────────────────────────────────────────

    private readonly float  _startY;
    private float           _yLerp;
    private float           _sinkTimer;
    private List<(Vector2 A, Vector2 B)> _lines = new();
    private readonly Color  _lineColor = Color.White;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="GlassBlock"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="sinks">Whether the block sinks under the player.</param>
    public GlassBlock(Vector2 position, float width, float height, bool sinks)
        : base(position, width, height, safe: false)
    {
        Sinks   = sinks;
        _startY = position.Y;
        Name    = "GlassBlock";
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();
        BuildEdgeLines();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();

        if (!Sinks) return;

        float dt = Time.DeltaTime;

        if (HasPlayerRider())
            _sinkTimer = SinkHold;
        else if (_sinkTimer > 0f)
            _sinkTimer -= dt;

        _yLerp = _sinkTimer > 0f
            ? Approach(_yLerp, 1f, SinkLerpRate * dt)
            : Approach(_yLerp, 0f, SinkLerpRate * dt);

        float targetY = Lerp(_startY, _startY + SinkMaxOffset, SineInOut(_yLerp));
        MoveV(targetY - Position.Y);
    }

    // ── Edge line building ────────────────────────────────────────────────────

    /// <summary>Scans the four sides and builds the list of visible edge segments.</summary>
    private void BuildEdgeLines()
    {
        _lines.Clear();
        int tilesX = (int)(Width  / 8);
        int tilesY = (int)(Height / 8);

        // Top edge  (normal = up)
        Ad2(new Vector2(0, 0),          new Vector2(0, -1), tilesX);
        // Right edge (normal = right)
        Ad2(new Vector2(tilesX - 1, 0), new Vector2(1,  0), tilesY);
        // Bottom edge (normal = down)
        Ad2(new Vector2(tilesX - 1, tilesY - 1), new Vector2(0, 1), tilesX);
        // Left edge  (normal = left)
        Ad2(new Vector2(0, tilesY - 1), new Vector2(-1, 0), tilesY);
    }

    private void Ad2(Vector2 start, Vector2 normal, int tiles)
    {
        // Tangent direction along the edge.
        Vector2 tangent = new Vector2(-normal.Y, normal.X);

        int index = 0;
        while (index < tiles)
        {
            Vector2 tile = start + tangent * index;
            if (IsOpen(tile + normal))
            {
                // Begin a run of open tiles.
                Vector2 segA = tile * 8f + new Vector2(4f) - tangent * 4f + normal * 4f;

                while (index < tiles && IsOpen(start + tangent * index + normal))
                    index++;

                Vector2 endTile = start + tangent * index;
                Vector2 segB    = endTile * 8f + new Vector2(4f) - tangent * 4f + normal * 4f;

                _lines.Add((segA + normal, segB + normal));
            }
            else
            {
                index++;
            }
        }
    }

    /// <summary>Returns true if the given tile offset is not blocked by a solid or glass block.</summary>
    private bool IsOpen(Vector2 tile)
    {
        if (Scene == null) return true;

        Vector2 point = new Vector2(
            (float)(Position.X + tile.X * 8.0 + 4.0),
            (float)(Position.Y + tile.Y * 8.0 + 4.0));

        for (int _gli = 0; _gli < Scene.Entities.Count; _gli++)
        {
            var e = Scene.Entities[_gli];
            if (e == this) continue;
            if (e is CelesteSolid solid && solid.Collidable && solid.Bounds.Contains(point))
                return false;
            if (e is GlassBlock gb && gb.Bounds.Contains(point))
                return false;
        }
        return true;
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public override void Render()
    {
        var batcher = Graphics.Instance.Batcher;
        foreach (var (a, b) in _lines)
            batcher.DrawLine(Position + a, Position + b, _lineColor);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private new bool HasPlayerRider()
    {
        if (Scene == null) return false;
        for (int _gli2 = 0; _gli2 < Scene.Entities.Count; _gli2++)
            if (Scene.Entities[_gli2] is DZ.Entities.Player.MadelinePlayer p && IsRiding(p))
                return true;
        return false;
    }

    private bool IsRiding(DZ.Entities.Player.MadelinePlayer player) =>
        Math.Abs(player.Position.Y + player.Height - Position.Y) <= 2f
        && player.Position.X + player.Width > Position.X
        && player.Position.X < Position.X + Width;

    private static float Approach(float val, float target, float maxMove) =>
        val < target ? Math.Min(val + maxMove, target) : Math.Max(val - maxMove, target);

    private static float Lerp(float a, float b, float t) =>
        a + (b - a) * Math.Clamp(t, 0f, 1f);

    private static float SineInOut(float t) =>
        (float)(-(Math.Cos(Math.PI * t) - 1.0) / 2.0);
}
