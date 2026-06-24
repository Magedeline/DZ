using Microsoft.Xna.Framework;
using Nez;
using System;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Bonfire / campfire decoration with animated flame, vertex light, and bloom.
/// Ported from Celeste's Bonfire.cs.
///
/// Three modes:
///   <see cref="BonfireMode.Unlit"/>   – cold, dark, no light.
///   <see cref="BonfireMode.Lit"/>     – burning, flickering light and bloom.
///   <see cref="BonfireMode.Smoking"/> – extinguished, smoke animation only.
/// </summary>
public class Bonfire : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public types
    // -------------------------------------------------------------------------

    public enum BonfireMode { Unlit, Lit, Smoking }

    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------

    /// <summary>Whether the bonfire has been activated at least once.</summary>
    public bool Activated { get; set; }

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    private BonfireMode _mode;

    // -------------------------------------------------------------------------
    // Lighting sub-components
    // -------------------------------------------------------------------------

    private VertexLight? _light;
    private BloomPoint?  _bloom;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private float _brightness;
    private float _multiplier;
    private float _wiggleTimer;
    private bool  _wiggling;

    // Flicker interval
    private float _flickerTimer;
    private const float FlickerInterval = 0.25f;

    private static readonly Vector2 LightOffset = new Vector2(0f, -6f);
    private static readonly Color   LightColor  = Color.PaleVioletRed;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">World position.</param>
    /// <param name="mode">Initial bonfire state.</param>
    public Bonfire(Vector2 position, BonfireMode mode = BonfireMode.Unlit)
    {
        _spawnPosition = position;
        _mode = mode;
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        _light = Entity.AddComponent(new VertexLight(LightOffset, LightColor, 1f, 32, 64));
        _bloom = Entity.AddComponent(new BloomPoint(LightOffset, 1f, 32f));

        // TODO: add "campfire" sprite renderer
        SetMode(_mode);
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        if (_mode == BonfireMode.Lit)
        {
            _multiplier = Approach(_multiplier, 1f, dt * 2f);

            _flickerTimer += dt;
            if (_flickerTimer >= FlickerInterval)
            {
                _flickerTimer = 0f;
                _brightness   = 0.5f + Nez.Random.NextFloat() * 0.5f;
                StartWiggle();
            }
        }

        if (_wiggling)
        {
            _wiggleTimer += dt * 4f;
            float wiggleVal = MathF.Sin(_wiggleTimer) * MathF.Exp(-_wiggleTimer * 0.5f);
            if (_light != null) _light.Alpha = Math.Min(1f, _brightness + wiggleVal * 0.25f) * _multiplier;
            if (_bloom  != null) _bloom.Alpha = _light?.Alpha ?? 0f;
            if (_wiggleTimer > MathF.PI * 2f) _wiggling = false;
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void SetMode(BonfireMode mode)
    {
        _mode = mode;
        switch (mode)
        {
            case BonfireMode.Lit:
                if (Activated)
                {
                    // Re-lit (player re-entered room)
                    // TODO: play sound: event:/env/local/campfire_start
                    // TODO: play sprite: session.Dreaming ? "startDream" : "start"
                }
                else
                {
                    // Already burning on room enter
                    // TODO: play sound loop: event:/env/local/campfire_loop
                    // TODO: play sprite: session.Dreaming ? "burnDream" : "burn"
                }
                break;

            case BonfireMode.Smoking:
                // TODO: play sprite "smoking"
                break;

            case BonfireMode.Unlit:
            default:
                // TODO: play sprite "idle"
                if (_bloom  != null) _bloom.Alpha  = 0f;
                if (_light  != null) _light.Alpha  = 0f;
                _brightness = 0f;
                break;
        }
        Activated = true;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void StartWiggle()
    {
        _wiggling    = true;
        _wiggleTimer = 0f;
    }

    private static float Approach(float v, float target, float maxDelta)
        => v < target ? Math.Min(v + maxDelta, target) : Math.Max(v - maxDelta, target);
}
