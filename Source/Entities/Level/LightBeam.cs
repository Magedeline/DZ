using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using System;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Angled light-beam decoration.  Ported from Celeste's LightBeam.cs.
///
/// The beam fades in/out depending on how close the player is to the beam's
/// axis.  When alpha is above 0.5 it periodically emits <c>P_Glow</c> particles.
/// The beam itself is drawn as a series of overlapping transparent texture
/// strips that animate via a sine-wave timer to create a shimmering effect.
/// </summary>
public class LightBeam : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public configuration
    // -------------------------------------------------------------------------

    /// <summary>Width of the light beam (pixels).</summary>
    public int LightWidth  { get; set; } = 32;

    /// <summary>Length of the light beam (pixels).</summary>
    public int LightLength { get; set; } = 128;

    /// <summary>Angle of the beam in radians.</summary>
    public float Rotation { get; set; } = 0f;

    /// <summary>Optional session flag that must be set for the beam to display.</summary>
    public string Flag { get; set; } = string.Empty;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private float _alpha;
    private float _timer;
    private float _particleTimer;
    private const float ParticleInterval = 0.8f;

    // Beam colour (matching Celeste's default cyan-white)
    private readonly Color _color = new Color(0.8f, 1f, 1f);

    // Lazy player reference
    private MadelinePlayer? _player;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">World position of the beam origin point.</param>
    /// <param name="lightWidth">Width of the beam band.</param>
    /// <param name="lightLength">Length of the beam.</param>
    /// <param name="rotationDegrees">Rotation in degrees (converted internally).</param>
    /// <param name="flag">Session flag name; empty = always shown.</param>
    public LightBeam(Vector2 position, int lightWidth, int lightLength,
                     float rotationDegrees = 0f, string flag = "")
    {
        _spawnPosition = position;
        LightWidth     = lightWidth;
        LightLength    = lightLength;
        Rotation       = rotationDegrees * MathF.PI / 180f;
        Flag           = flag;
        _timer         = DZ.Nez.Random.NextFloat() * 1000f;
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;
        // Render depth -9998 (near top of stack)
        // TODO: set Entity render layer
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;
        _timer += dt;

        _player ??= Entity.Scene?.FindEntityOfType<MadelinePlayer>();

        bool flagOk = string.IsNullOrEmpty(Flag);
        // TODO: check session flag: flagOk = string.IsNullOrEmpty(Flag) || session.GetFlag(Flag)

        if (_player != null && flagOk)
        {
            // Compute closest point on beam axis to player
            Vector2 beamDir = AngleToVector(Rotation + MathF.PI * 0.5f);
            Vector2 toPlayer = _player.Position - Entity.Position;

            // Project player onto beam axis
            float along = Vector2.Dot(toPlayer, beamDir);
            float target = Math.Min(1f, Math.Max(0f, along - 8f) / LightLength);

            // Check if player is within beam width
            Vector2 projected = Entity.Position + beamDir * along;
            float lateral = Vector2.Distance(projected, _player.Position);
            if (lateral > LightWidth * 0.5f)
                target = 1f;

            // TODO: if scene is transitioning, target = 0
            _alpha = Approach(_alpha, target, dt * 4f);
        }
        // else alpha stays at 0

        // Particle emission when beam is bright
        if (_alpha >= 0.5f)
        {
            _particleTimer += dt;
            if (_particleTimer >= ParticleInterval)
            {
                _particleTimer = 0f;
                // TODO: emit LightBeam.P_Glow particle along beam width
            }
        }
    }

    // -------------------------------------------------------------------------
    // Rendering (stub – wire into a custom renderer)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Renders the beam texture strips.
    /// Call from the scene renderer after acquiring the "util/lightbeam" texture.
    /// </summary>
    public void Render()
    {
        if (_alpha <= 0f) return;

        // Base wide strip
        DrawStrip(0f, LightWidth,
                  LightLength - 4f + MathF.Sin(_timer * 2f) * 4f, 0.4f);

        // Shimmering sub-strips at 4-pixel steps across the width
        for (int i = 0; i < LightWidth; i += 4)
        {
            float t     = _timer + i * 0.6f;
            float w     = 4f + MathF.Sin(t * 0.5f + 1.2f) * 4f;
            float off   = MathF.Sin((t + i * 32f) * 0.1f
                        + MathF.Sin(t * 0.05f + i * 0.1f) * 0.25f)
                        * (LightWidth * 0.5f - w * 0.5f);
            float len   = LightLength + MathF.Sin(t * 0.25f) * 8f;
            float a     = 0.6f + MathF.Sin(t + 0.8f) * 0.3f;
            DrawStrip(off, w, len, a);
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void DrawStrip(float offset, float width, float length, float a)
    {
        if (width < 1f) return;
        // TODO: draw "util/lightbeam" texture:
        //   position  = Entity.Position + AngleToVector(Rotation) * offset
        //   origin    = (0, 0.5)
        //   color     = _color * a * _alpha
        //   scale     = (1/texWidth * length, width)
        //   rotation  = Rotation + PI/2
    }

    private static Vector2 AngleToVector(float angle)
        => new Vector2(MathF.Cos(angle), MathF.Sin(angle));

    private static float Approach(float v, float target, float maxDelta)
        => v < target ? Math.Min(v + maxDelta, target) : Math.Max(v - maxDelta, target);
}
