using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using System;

namespace DZ.Entities.Level;

/// <summary>
/// Component that renders a string of decorative cloth flags hung between two
/// points.  Ported from Celeste's Flagline.cs.
///
/// The line is drawn as a catenary (simple quadratic Bezier that droops by
/// distance/8 units) with small coloured rectangles ("cloth") spaced along it.
/// Each cloth piece has a slight secondary droop and sways with a sine timer.
/// </summary>
public class Flagline : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Nested types
    // -------------------------------------------------------------------------

    private struct Cloth
    {
        public int ColorIndex;
        public int Height;
        public int Length;
        public int Step;
    }

    // -------------------------------------------------------------------------
    // Public configuration
    // -------------------------------------------------------------------------

    /// <summary>World-space end point of the flag line.</summary>
    public Vector2 To { get; set; }

    /// <summary>Droop amount relative to cloth length (0 = taut, 1 = very droopy).</summary>
    public float ClothDroopAmount { get; set; } = 0.6f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly Color[]  _colors;
    private readonly Color[]  _highlights;
    private readonly Color    _lineColor;
    private readonly Color    _pinColor;
    private readonly Cloth[]  _clothes;
    private float             _waveTimer;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public Flagline(
        Vector2  to,
        Color    lineColor,
        Color    pinColor,
        Color[]  colors,
        int      minFlagHeight,
        int      maxFlagHeight,
        int      minFlagLength,
        int      maxFlagLength,
        int      minSpace,
        int      maxSpace)
    {
        To          = to;
        _lineColor  = lineColor;
        _pinColor   = pinColor;
        _colors     = colors;
        _waveTimer  = DZ.Nez.Random.NextFloat() * MathF.PI * 2f;

        _highlights = new Color[colors.Length];
        for (int i = 0; i < colors.Length; i++)
            _highlights[i] = Color.Lerp(colors[i], Color.White, 0.1f);

        _clothes = new Cloth[10];
        for (int i = 0; i < _clothes.Length; i++)
        {
            _clothes[i] = new Cloth
            {
                ColorIndex = DZ.Nez.Random.NextInt(colors.Length),
                Height     = minFlagHeight + DZ.Nez.Random.NextInt(maxFlagHeight - minFlagHeight + 1),
                Length     = minFlagLength + DZ.Nez.Random.NextInt(maxFlagLength - minFlagLength + 1),
                Step       = minSpace      + DZ.Nez.Random.NextInt(maxSpace      - minSpace      + 1),
            };
        }
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
    /// Draws the flagline.  <c>Entity.Position</c> is the start anchor.
    /// </summary>
    public override void Render()
    {
        Vector2 from  = Entity.Position;
        // Always render left-to-right for consistent droop direction
        Vector2 begin = from.X <= To.X ? from : To;
        Vector2 end   = from.X <= To.X ? To   : from;

        float  lineLen = Vector2.Distance(begin, end);
        float  droop   = lineLen / 8f;

        // Main catenary control point (droops down by droop amount, sways with wave)
        Vector2 ctrl = (begin + end) * 0.5f
                     + Vector2.UnitY * (droop + MathF.Sin(_waveTimer) * droop * 0.3f);

        Vector2 prev    = begin;
        float   percent = 0f;
        int     idx     = 0;
        bool    isCloth = false;

        while (percent < 1f)
        {
            Cloth c = _clothes[idx % _clothes.Length];
            float step = (isCloth ? c.Length : c.Step) / lineLen;
            percent += step;
            percent  = Math.Min(percent, 1f);

            float t   = percent;
            float inv = 1f - t;
            Vector2 pt = inv * inv * begin + 2f * inv * t * ctrl + t * t * end;

            // TODO: Draw.Line(prev, pt, _lineColor)

            if (percent < 1f && isCloth)
            {
                float clothDroop = c.Length * ClothDroopAmount;
                float clothSway  = MathF.Sin(_waveTimer * 2f + percent) * clothDroop * 0.4f;
                Vector2 cCtrl    = (prev + pt) * 0.5f
                                 + new Vector2(0f, clothDroop + clothSway);

                // Draw cloth rectangle by sampling the cloth Bezier
                Vector2 cPrev = prev;
                for (float s = 1f; s <= c.Length; s += 1f)
                {
                    float sp   = s / c.Length;
                    float sinv = 1f - sp;
                    Vector2 cPt = sinv * sinv * prev + 2f * sinv * sp * cCtrl + sp * sp * pt;

                    if (Math.Abs(cPt.X - cPrev.X) > 0.01f)
                    {
                        float w = cPt.X - cPrev.X + 1f;
                        // TODO: Draw.Rect(cPrev.X, cPrev.Y, w, c.Height, _colors[c.ColorIndex])
                        cPrev = cPt;
                    }
                }

                // Highlights and pins at left/right edges
                // TODO: Draw.Rect(prev.X, prev.Y, 1, c.Height, _highlights[c.ColorIndex])
                // TODO: Draw.Rect(pt.X, pt.Y, 1, c.Height, _highlights[c.ColorIndex])
                // TODO: Draw.Rect(prev.X, prev.Y - 1, 1, 3, _pinColor)
                // TODO: Draw.Rect(pt.X, pt.Y - 1, 1, 3, _pinColor)
                idx++;
            }

            prev    = pt;
            isCloth = !isCloth;
        }
    }
}
