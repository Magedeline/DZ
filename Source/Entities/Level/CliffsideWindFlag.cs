using Microsoft.Xna.Framework;
using Nez;
using System;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Cliffside wind-blown flag decoration.
/// Ported from Celeste's CliffsideWindFlag.cs.
///
/// The flag is sliced into individual 1-pixel-wide vertical segments whose
/// horizontal and vertical offsets are animated based on the current wind speed.
/// Each segment is driven by a sine wave so the fabric ripples naturally.
/// </summary>
public class CliffsideWindFlag : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Nested types
    // -------------------------------------------------------------------------

    private class Segment
    {
        /// <summary>Atlas sub-texture key for this column slice.</summary>
        public string TextureKey = string.Empty;
        /// <summary>Current render offset from entity position.</summary>
        public Vector2 Offset;
    }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Segment[] _segments;
    private float _sine;
    private float _random;
    private int   _sign = 1;

    /// <summary>
    /// Current wind magnitude (0-1).  Set externally each frame from the scene's
    /// wind controller, or leave at 0 for still air.
    /// </summary>
    public float WindMagnitude { get; set; } = 0f;

    /// <summary>
    /// Wind sign: +1 = blowing right, -1 = blowing left.
    /// Derived from <see cref="WindMagnitude"/> sign when updated externally.
    /// </summary>
    public int WindSign { get; set; } = 1;

    /// <summary>Flag atlas index (selects which flag graphic to use).</summary>
    public int FlagIndex { get; }

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="position">World position (flag pole base).</param>
    /// <param name="flagIndex">Atlas sub-texture row index.</param>
    /// <param name="segmentCount">
    /// Number of 1-pixel-wide segments.  Matches the flag texture width.
    /// </param>
    public CliffsideWindFlag(Vector2 position, int flagIndex, int segmentCount = 16)
    {
        _spawnPosition = position;
        FlagIndex      = flagIndex;
        _segments      = new Segment[segmentCount];
        for (int i = 0; i < _segments.Length; i++)
        {
            _segments[i] = new Segment
            {
                TextureKey = $"scenery/cliffside/flag_{flagIndex}_col{i}",
                Offset     = new Vector2(i, 0f)
            };
        }
        _sine   = Nez.Random.NextFloat() * MathF.PI * 2f;
        _random = Nez.Random.NextFloat();
    }

    private readonly Vector2 _spawnPosition;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;
        // TODO: load "scenery/cliffside/flag" atlas and slice into per-column sub-textures
        // Initial snap (no animation yet)
        InitializePositions();
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        // Update sign from external wind
        if (WindMagnitude != 0f)
            _sign = WindSign;

        float wind = Math.Abs(WindMagnitude); // normalised 0-1
        _sine += dt * (4f + wind * 4f) * (0.8f + _random * 0.2f);

        for (int i = 0; i < _segments.Length; i++)
            UpdateSegment(i, wind, snap: false);
    }

    // -------------------------------------------------------------------------
    // Rendering (call from scene renderer)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Draws each flag segment.  Call from the scene's render pass.
    /// </summary>
    public void Render()
    {
        for (int i = 0; i < _segments.Length; i++)
        {
            float wave = (float)(i / (double)_segments.Length * Sin(-i * 0.1f + _sine) * 2.0);
            // TODO: draw sub-texture _segments[i].TextureKey at
            //   Entity.Position + _segments[i].Offset + Vector2.UnitY * wave
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void InitializePositions()
    {
        float wind = Math.Abs(WindMagnitude);
        for (int i = 0; i < _segments.Length; i++)
            UpdateSegment(i, wind, snap: true);
    }

    private void UpdateSegment(int i, float wind, bool snap)
    {
        var seg  = _segments[i];
        float t1 = i * _sign * (0.2f + wind * 0.8f * (0.8f + _random * 0.2f))
                 * (0.9f + Sin(_sine) * 0.1f);

        float sineLow = (float)Math.Sin(_sine * 0.5 - i * 0.1)
                      * (i / (float)_segments.Length) * i * 0.2f;
        float target1 = MathHelper.Lerp(sineLow, t1, (float)Math.Ceiling(wind));
        float target2 = i / (float)_segments.Length * Math.Max(0.1f, 1f - wind) * 16f;

        if (snap)
        {
            seg.Offset.X = target1;
            seg.Offset.Y = target2;
        }
        else
        {
            seg.Offset.X = Approach(seg.Offset.X, target1, Time.DeltaTime * 40f);
            seg.Offset.Y = Approach(seg.Offset.Y, target2, Time.DeltaTime * 40f);
        }
    }

    private static float Sin(float x) => -(float)Math.Sin(x);

    private static float Approach(float v, float target, float maxDelta)
        => v < target ? Math.Min(v + maxDelta, target) : Math.Max(v - maxDelta, target);
}
