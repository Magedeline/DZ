using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using System;
using DZ.Entities.Core;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's Gondola.cs (Chapter 7 — The Summit).
///
/// A cable-car solid that swings on a rope between two cliff anchors.
/// When <c>inCliffside</c> mode is active the gondola rocks with a damped
/// spring on its <see cref="Rotation"/> value.
///
/// Key mechanics:
/// <list type="bullet">
///   <item>Rope rendered between <see cref="LeftCliffside"/> and
///         <see cref="RightCliffside"/> anchor entities.</item>
///   <item>Gondola can be moved to its destination via coroutine (TODO).</item>
///   <item><see cref="BreakLever"/> launches the lever sprite into the air.</item>
/// </list>
///
/// Sprite loading and coroutine movement are TODO.
/// </summary>
public class Gondola : CelesteSolid
{
    // ── Configuration ─────────────────────────────────────────────────────────

    public Vector2 Start       { get; private set; }
    public Vector2 Destination { get; private set; }
    public Vector2 Halfway     { get; private set; }

    // ── Anchor entities (added to scene in OnAddedToScene) ───────────────────

    public DZ.Nez.Entity LeftCliffside;
    public DZ.Nez.Entity RightCliffside;

    // ── Rotation spring ───────────────────────────────────────────────────────

    public new virtual float Rotation      { get; set; }
    public float RotationSpeed { get; set; }

    // ── State ─────────────────────────────────────────────────────────────────

    private readonly bool _inCliffside;
    private bool          _brokenLever;
    private Vector2       _leverPos;
    private float         _leverRot;
    private Vector2       _leverVel;
    private bool          _leverBreaking;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Gondola"/>.
    /// </summary>
    /// <param name="position">Center-bottom world position of the gondola body.</param>
    /// <param name="destination">Node position (end of the rope).</param>
    /// <param name="inCliffside">
    ///   <c>true</c> if the gondola starts at its origin/cliffside anchor.
    /// </param>
    public Gondola(Vector2 position, Vector2 destination, bool inCliffside = true)
        : base(position + new Vector2(-32f, 0f), 64f, 8f, safe: true)
    {
        Start        = position;
        Destination  = destination;
        Halfway      = (position + destination) / 2f;
        _inCliffside = inCliffside;
        Name         = "Gondola";
        // TODO: load gondola sprites (front, back, lever, top)
    }

    // ── Nez lifecycle ─────────────────────────────────────────────────────────

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // Spawn cliff anchor entities.
        LeftCliffside  = new DZ.Nez.Entity { Position = Position + new Vector2(-124f, 0f) };
        RightCliffside = new DZ.Nez.Entity { Position = Destination + new Vector2(144f, -104f) };
        Scene.AddEntity(LeftCliffside);
        Scene.AddEntity(RightCliffside);

        if (!_inCliffside)
        {
            // Gondola starts at destination — skip to end position.
            Position = Destination;
            // TODO: add JumpThru above gondola at destination
        }

        _leverPos = Position + new Vector2(0f, -52f);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        float dt = Time.DeltaTime;

        if (_inCliffside)
        {
            // Damped spring rotation.
            float springK = (float)Math.Abs(Rotation) < 0.5f ? 8f : 6f;
            if (Math.Abs(Rotation) < 0.25f) springK *= 0.5f;
            if (Math.Abs(Rotation) < 0.5f)  springK *= 0.5f;

            RotationSpeed += -Math.Sign(Rotation) * springK * dt;
            Rotation      += RotationSpeed * dt;
            Rotation       = Math.Clamp(Rotation, -0.4f, 0.4f);

            if (Math.Abs(Rotation) < 0.02f && Math.Abs(RotationSpeed) < 0.2f)
                Rotation = RotationSpeed = 0f;
        }

        // Update lever if breaking.
        if (_leverBreaking)
        {
            _leverVel.Y += 400f * dt;
            _leverPos   += _leverVel * dt;
            _leverRot   += 2f * dt;
        }

        base.Update();
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public override void Render()
    {
        var batcher = Graphics.Instance.Batcher;
        // Render rope.
        if (LeftCliffside != null && RightCliffside != null)
        {
            Vector2 leftAnchor  = (LeftCliffside.Position  + new Vector2(40f, -12f));
            Vector2 rightAnchor = (RightCliffside.Position + new Vector2(-40f, -4f));
            Vector2 gondolaTop  = Position + new Vector2(0f, -55f);

            Vector2 rawDir  = rightAnchor - leftAnchor;
            float   rawLen  = rawDir.Length();
            Vector2 dir     = rawLen > 0f ? rawDir / rawLen : Vector2.UnitX;
            Vector2 connL   = gondolaTop - dir * 6f;
            Vector2 connR   = gondolaTop + dir * 6f;

            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Vector2.UnitY * i;
                batcher.DrawLine(leftAnchor  + offset, connL + offset, Color.Black);
                batcher.DrawLine(connR + offset,       rightAnchor + offset, Color.Black);
            }
        }

        // Gondola body placeholder.
        batcher.DrawRect(Position.X, Position.Y, Width, Height, Color.Gray);
    }

    // ── Lever ─────────────────────────────────────────────────────────────────

    /// <summary>Starts the lever-breaking animation.</summary>
    public void BreakLever()
    {
        _brokenLever   = true;
        _leverBreaking = true;
        _leverVel      = new Vector2(240f, -130f);
    }

    /// <summary>Starts pulling-sides animation (TODO: sprite).</summary>
    public void PullSides()  { /* TODO */ }

    /// <summary>Cancels pulling-sides animation (TODO: sprite).</summary>
    public void CancelPullSides() { /* TODO */ }

    // ── Rotated floor helper ──────────────────────────────────────────────────

    /// <summary>
    /// Returns the world-space position on the rotated gondola floor at a
    /// given local offset from the gondola centre.
    /// </summary>
    public Vector2 GetRotatedFloorPositionAt(float x, float y = 52f)
    {
        float angle  = Rotation + MathF.PI / 2f;
        var   up     = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        var   right  = new Vector2(-up.Y, up.X);
        return Position + new Vector2(0f, -52f) + up * y - right * x;
    }
}
