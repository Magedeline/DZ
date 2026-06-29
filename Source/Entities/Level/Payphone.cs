using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using System;

namespace DZ.Entities.Level;

/// <summary>
/// Payphone decoration with flickering light.  Ported from Celeste's Payphone.cs.
///
/// When <see cref="Broken"/> is false the phone's vertex light and bloom flicker
/// at randomised intervals.  Breaking the phone hides all lights and silences
/// the ambient buzz.
/// </summary>
public class Payphone : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public state
    // -------------------------------------------------------------------------

    /// <summary>When true the phone's lights and buzz are turned off.</summary>
    public bool Broken { get; set; }

    // -------------------------------------------------------------------------
    // Lighting sub-components
    // -------------------------------------------------------------------------

    private VertexLight? _light;
    private BloomPoint?  _bloom;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private float _lightFlickerTimer;
    private float _lightFlickerFor = 0.1f;
    private bool  _lightOn = true;

    // Light offset (matching Celeste's -6, -45)
    private static readonly Vector2 LightOffset = new Vector2(-6f, -45f);

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public Payphone(Vector2 position)
    {
        _spawnPosition = position;
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // Add lighting components
        _light = Entity.AddComponent(new VertexLight(LightOffset, Color.White, 1f, 8, 96));
        _light.Spotlight          = true;
        _light.SpotlightDirection = MathF.PI * 0.5f; // pointing downward

        _bloom = Entity.AddComponent(new BloomPoint(LightOffset, 0.8f, 8f));

        // TODO: add "payphone" sprite renderer, play "idle" animation
        // TODO: add "blink" image overlay (initially hidden)
        // TODO: play ambient sound: event:/env/local/02_old_site/phone_lamp (param "on" = 1)
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        if (!Broken)
        {
            _lightFlickerTimer -= dt;

            if (_lightFlickerTimer <= 0f)
            {
                // Rapid flicker phase: toggle every ~0.025 s
                // We use a simple sub-timer approximation
                bool flickerOn = DZ.Nez.Random.NextFloat() > 0.5f;
                SetLightVisible(flickerOn);
                // TODO: play "blink" image visible = !flickerOn

                if (_lightFlickerTimer < -_lightFlickerFor)
                {
                    // End flicker phase; pick next stable-on duration
                    _lightFlickerTimer = DZ.Nez.Random.Choose(0.4f, 0.6f, 0.8f, 1f);
                    _lightFlickerFor   = DZ.Nez.Random.Choose(0.1f, 0.2f, 0.05f);
                    SetLightVisible(true);
                    // TODO: set sound param "on" = 1
                }
            }
        }
        else
        {
            SetLightVisible(false);
            // TODO: set sound param "on" = 0
        }

        // TODO: "eat" animation frame-6 → emit P_Snow / P_SnowB particles
        // TODO: "eat" animation last-5 frames → rumble light
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetLightVisible(bool on)
    {
        if (_light  != null) _light.Enabled  = on;
        if (_bloom  != null) _bloom.Enabled  = on;
        _lightOn = on;
    }
}
