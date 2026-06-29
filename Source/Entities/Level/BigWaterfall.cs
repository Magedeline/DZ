using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Camera = DZ.Nez.Camera;
using System;
using System.Collections.Generic;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's BigWaterfall.cs.
///
/// A wide backdrop waterfall that can appear in either the foreground (FG) or
/// background (BG) layer.  The two layers have different parallax values,
/// colours, and rendering styles.
///
/// FG waterfall: solid fill + thin edge lines + displacement shader hook (TODO).
/// BG waterfall: striped sine-wave animated bands.
///
/// Audio (looping sfx) and transition-fade listener are TODO.
/// </summary>
public class BigWaterfall : DZ.Nez.Entity
{
    // ── Layer enum ────────────────────────────────────────────────────────────

    public enum WaterfallLayer { FG, BG }

    // ── Configuration ─────────────────────────────────────────────────────────

    private readonly WaterfallLayer _layer;
    private readonly float          _width;
    private readonly float          _height;
    private readonly float          _parallax;
    private readonly List<float>    _lines = new();
    private readonly Color          _surfaceColor;
    private readonly Color          _fillColor;

    // ── Animation ─────────────────────────────────────────────────────────────

    private float _sine;
    private float _fade = 1f;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="BigWaterfall"/>.
    /// </summary>
    /// <param name="position">Top-left world position.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="layer">Foreground or background layer.</param>
    public BigWaterfall(Vector2 position, float width, float height, WaterfallLayer layer)
    {
        Position = position;
        _width   = width;
        _height  = height;
        _layer   = layer;
        Name     = "BigWaterfall";

        if (layer == WaterfallLayer.FG)
        {
            _parallax     = 0.1f + DZ.Nez.Random.NextFloat() * 0.2f;
            _surfaceColor = Water.SurfaceColor;
            _fillColor    = Water.FillColor;

            _lines.Add(3f);
            _lines.Add(width - 4f);

            // TODO: play looping FG waterfall SFX
        }
        else
        {
            _parallax     = -(0.7f + DZ.Nez.Random.NextFloat() * 0.2f);
            _surfaceColor = new Color(0x89, 0xDB, 0xF0) * 0.5f;
            _fillColor    = new Color(0x29, 0xA7, 0xEA) * 0.3f;

            _lines.Add(6f);
            _lines.Add(width - 7f);
        }

        // Add random interior lines if wide enough.
        if (width > 16f)
        {
            int count = DZ.Nez.Random.Range(0, (int)(width / 16f));
            for (int i = 0; i < count; i++)
                _lines.Add(8f + DZ.Nez.Random.NextFloat(width - 16f));
        }
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public override void Update()
    {
        base.Update();
        _sine += Time.DeltaTime;
    }

    // ── Render ────────────────────────────────────────────────────────────────

    public override void Render()
    {
        // Compute parallax render position (camera not directly available — use Position).
        // TODO: apply camera parallax offset via Scene.Camera when available.
        float renderX = Position.X;

        Color fill    = _fillColor    * _fade;
        Color surface = _surfaceColor * _fade;

        var batcher = Graphics.Instance.Batcher;
        batcher.DrawRect(renderX, Position.Y, _width, _height, fill);

        if (_layer == WaterfallLayer.FG)
        {
            batcher.DrawRect(renderX - 1f, Position.Y, 3f, _height, surface);
            batcher.DrawRect(renderX + _width - 2f, Position.Y, 3f, _height, surface);
            foreach (float line in _lines)
                batcher.DrawRect(renderX + line, Position.Y, 1f, _height, surface);
        }
        else
        {
            const int stripH = 3;
            float yStart = Position.Y;
            float yEnd   = Position.Y + _height;

            for (float y = yStart; y < yEnd; y += stripH)
            {
                int offset = (int)(Math.Sin(y / 6.0 - _sine * 8.0) * 2.0);
                batcher.DrawRect(renderX,              y, 4 + offset,  stripH, surface);
                batcher.DrawRect(renderX + _width - 4 + offset, y, 4 - offset, stripH, surface);
                foreach (float line in _lines)
                    batcher.DrawRect(renderX + offset + line, y, 1f, stripH, surface);
            }
        }
    }
}
