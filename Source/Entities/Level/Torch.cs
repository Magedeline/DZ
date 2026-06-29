using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Collider = DZ.Nez.Collider;
using BoxCollider = DZ.Nez.BoxCollider;
using System;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Lit/unlit torch that the player activates by touching.
/// Ported from Celeste's Torch.cs (Mirror Temple).
///
/// Unlit torches wait for the player to touch them; on contact they switch to
/// the lit colour, emit particles, and remember their state via a session flag
/// so they remain lit after room transitions.
/// </summary>
public class Torch : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    public const float BloomAlpha  = 0.5f;
    public const int   StartRadius = 48;
    public const int   EndRadius   = 64;

    /// <summary>Lit colour: white-cyan mix.</summary>
    public static readonly Color LitColor      = Color.Lerp(Color.White, Color.Cyan, 0.5f);

    /// <summary>Initial lit colour burst: white-orange mix.</summary>
    public static readonly Color StartLitColor = Color.Lerp(Color.White, Color.Orange, 0.5f);

    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------

    public bool IsLit { get; private set; }

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    private readonly string _flagKey;
    private readonly bool   _startLit;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private VertexLight _light;
    private BloomPoint  _bloom;

    // Light-in animation
    private bool  _animating;
    private float _animTimer;
    private const float AnimDuration = 0.5f; // seconds for light-in effect

    private MadelinePlayer _player;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">World position.</param>
    /// <param name="flagKey">Session-flag key that persists lit state.</param>
    /// <param name="startLit">True if this torch begins already lit.</param>
    public Torch(Vector2 position, string flagKey, bool startLit = false)
    {
        _spawnPosition = position;
        _flagKey       = flagKey;
        _startLit      = startLit;
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        _light = Entity.AddComponent(new VertexLight(LitColor, 1f, StartRadius, EndRadius));
        _bloom = Entity.AddComponent(new BloomPoint(BloomAlpha, 8f));

        // Hidden by default
        if (_light != null) _light.Enabled = false;
        if (_bloom  != null) _bloom.Enabled  = false;

        // TODO: add BoxCollider 32x32 offset (-16,-16) trigger
        // TODO: add sprite: startLit ? "litTorch" : "torch"

        // TODO: check session flag _flagKey – if set, call Light() immediately
        if (_startLit) Light(instant: true);
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;
        _player ??= Entity.Scene?.FindEntityOfType<MadelinePlayer>();

        if (!IsLit && _player != null && IsPlayerOverlapping())
            OnPlayerContact();

        if (_animating)
        {
            _animTimer += dt;
            float t = Math.Min(1f, _animTimer / AnimDuration);
            // BackOut ease approximation: overshoot then settle
            float eased = BackOut(t);
            if (_light != null)
            {
                _light.Color       = Color.Lerp(Color.White, LitColor, eased);
                _light.StartRadius = StartRadius + (1f - eased) * 32f;
                _light.EndRadius   = EndRadius   + (1f - eased) * 32f;
            }
            if (_bloom != null)
                _bloom.Alpha = BloomAlpha + 0.5f * (1f - eased);

            if (t >= 1f) _animating = false;
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void OnPlayerContact()
    {
        if (IsLit) return;
        Light(instant: false);
        // TODO: play sound: event:/game/05_mirror_temple/torch_activate
        // TODO: set session flag _flagKey
        // TODO: emit: Torch.P_OnLight particles (12 count, radius 3)
    }

    private void Light(bool instant)
    {
        IsLit = true;
        if (_light != null) _light.Enabled = true;
        if (_bloom  != null) _bloom.Enabled  = true;

        if (instant)
        {
            if (_light != null)
            {
                _light.Color       = LitColor;
                _light.StartRadius = StartRadius;
                _light.EndRadius   = EndRadius;
            }
            if (_bloom != null) _bloom.Alpha = BloomAlpha;
        }
        else
        {
            // Start light-in animation burst
            if (_light != null) _light.Color = StartLitColor;
            _animTimer = 0f;
            _animating = true;
        }

        // TODO: play "turnOn" or "on" animation on sprite
    }

    private bool IsPlayerOverlapping()
    {
        if (_player == null) return false;
        Vector2 pPos = _player.Position;
        Vector2 ePos = Entity.Position;
        return Math.Abs(pPos.X - ePos.X) < 16f && Math.Abs(pPos.Y - ePos.Y) < 16f;
    }

    /// <summary>Simple back-out ease: overshoot then settle.</summary>
    private static float BackOut(float t)
    {
        const float s = 1.70158f;
        t -= 1f;
        return t * t * ((s + 1f) * t + s) + 1f;
    }
}
