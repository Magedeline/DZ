using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using System;
using System.Collections.Generic;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Cobweb decoration stretched between two anchor points with optional
/// offshoot threads.  Ported from Celeste's Cobweb.cs.
///
/// The main strand is drawn as a catenary (simple quadratic Bezier with a
/// drooping control point).  Each offshoot anchors to a solid surface.
/// The web sways gently via a sine-wave timer.
///
/// Player contact: not implemented as damage – the original Cobweb only
/// provides a visual.  To slow the player add a trigger zone separately.
/// </summary>
public class Cobweb : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Color _color;
    private Color _edgeColor;

    private Vector2 _anchorA;
    private Vector2 _anchorB;

    private readonly List<Vector2> _offshoots    = new();
    private readonly List<float>   _offshootEnds = new();

    private float _waveTimer;

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <param name="anchorA">First anchor in world space (also entity position).</param>
    /// <param name="anchorB">Second anchor in world space.</param>
    /// <param name="offshootNodes">Additional wall-anchored offshoot positions.</param>
    public Cobweb(Vector2 anchorA, Vector2 anchorB, IEnumerable<Vector2> offshootNodes = null)
    {
        _anchorA   = anchorA;
        _anchorB   = anchorB;
        _waveTimer = DZ.Nez.Random.NextFloat();

        if (offshootNodes != null)
        {
            bool first = true;
            foreach (var node in offshootNodes)
            {
                if (first) { first = false; continue; } // first node = anchorB
                _offshoots.Add(node);
                _offshootEnds.Add(0.3f + DZ.Nez.Random.NextFloat() * 0.4f);
            }
        }

        // Default colours (overridden per area data in the full game)
        _color     = new Color(180, 170, 200);
        _edgeColor = Color.Lerp(_color, new Color(15, 14, 23), 0.2f);
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _anchorA;

        // TODO: resolve area-specific CobwebColor from scene/AreaData
        // TODO: check both anchors are touching solids; if not → remove self
        // TODO: validate offshoot anchors against solids, remove invalid ones
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        _waveTimer += Time.DeltaTime;
    }

    // -------------------------------------------------------------------------
    // Rendering (call from scene renderer)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Renders the cobweb.  Call from your scene's render pass.
    /// </summary>
    public override void Render()
    {
        DrawCobweb(_anchorA, _anchorB, steps: 12, drawOffshoots: true);
    }

    // -------------------------------------------------------------------------
    // Drawing helpers
    // -------------------------------------------------------------------------

    private void DrawCobweb(Vector2 a, Vector2 b, int steps, bool drawOffshoots)
    {
        // Build a drooping Bezier control point
        Vector2 ctrl = (a + b) * 0.5f
                     + Vector2.UnitY * (8f + MathF.Sin(_waveTimer) * 4f);

        if (drawOffshoots && _offshoots.Count > 0)
        {
            for (int i = 0; i < _offshoots.Count; i++)
            {
                // Sample the main curve at offshoot attachment percent
                float t   = _offshootEnds[i];
                float inv = 1f - t;
                Vector2 mainPt = inv * inv * a + 2f * inv * t * ctrl + t * t * b;
                DrawCobweb(_offshoots[i], mainPt, steps: 4, drawOffshoots: false);
            }
        }

        // Draw the main strand as line segments along the quadratic Bezier
        Vector2 prev = a;
        for (int i = 1; i <= steps; i++)
        {
            float t    = i / (float)steps;
            float inv  = 1f - t;
            Vector2 pt = inv * inv * a + 2f * inv * t * ctrl + t * t * b;

            Color c = (i <= 2 || i >= steps - 1) ? _edgeColor : _color;
            // TODO: Draw.Line(prev, pt, c)   (use Nez Debug.DrawLine or custom renderer)
            Vector2 d = prev - pt;
            float dl = d.Length();
            prev = pt + (dl > 0f ? d / dl : Vector2.Zero);
        }
    }
}
