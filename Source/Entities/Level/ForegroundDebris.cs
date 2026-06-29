using Microsoft.Xna.Framework;
using DZ.Nez;
using Entity = DZ.Nez.Entity;
using Camera = DZ.Nez.Camera;
using System;

namespace DZ.Entities.Level;

/// <summary>
/// Foreground debris decoration that appears in front of gameplay elements and
/// shifts with slight parallax to give depth.
/// Ported from Celeste's ForegroundDebris.cs.
///
/// Renders a layered rock sprite (multiple textures stacked) with a small
/// sine-wave bob and a subtle parallax offset relative to the camera.
/// </summary>
public class ForegroundDebris : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly Vector2 _startPosition;
    private readonly float   _parallaxAmount;

    // Sine bobs (one per layer)
    private float[] _sinePhases  = Array.Empty<float>();
    private float[] _sineOffsets = Array.Empty<float>();  // current Y offsets (px)

    // Number of sprite layers (typically 2-3 from the atlas sub-textures)
    private readonly int _layerCount;

    // Sprite variant ("rock_a" or "rock_b")
    private readonly string _variant;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public ForegroundDebris(Vector2 position)
    {
        _startPosition  = position;
        _parallaxAmount = 0.05f + DZ.Nez.Random.NextFloat() * 0.08f;
        _variant        = DZ.Nez.Random.Choose("rock_a", "rock_b");
        _layerCount     = 2; // adjust to match actual atlas sub-texture count
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _startPosition;

        // TODO: load "scenery/fgdebris/<_variant>" atlas sub-textures (reversed)
        // TODO: for each sub-texture add an Image renderer with centered origin
        // TODO: set render depth -999900 (extreme foreground)

        // Initialise per-layer sine state
        _sinePhases  = new float[_layerCount];
        _sineOffsets = new float[_layerCount];
        for (int i = 0; i < _layerCount; i++)
            _sinePhases[i] = DZ.Nez.Random.NextFloat() * MathF.PI * 2f;
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;

        for (int i = 0; i < _layerCount; i++)
        {
            _sinePhases[i]  += dt * 0.4f * MathF.PI * 2f;
            _sineOffsets[i]  = MathF.Sin(_sinePhases[i]) * 2f;
            // TODO: apply _sineOffsets[i] to image[i].LocalOffset.Y
        }
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    /// <summary>
    /// Override render to apply parallax offset.
    /// Called by the Nez renderer each frame.
    /// </summary>
    public void Render(Vector2 cameraPosition, Vector2 screenCentre)
    {
        // Compute parallax shift based on how far camera is from entity
        Vector2 shift = (cameraPosition + screenCentre - _startPosition) * _parallaxAmount;
        Vector2 savedPos = Entity.Position;
        Entity.Position = _startPosition - shift;

        // TODO: render each layer image with its _sineOffsets[i] bob
        //   (draw in reversed order so foremost layer is on top)

        Entity.Position = savedPos;
    }
}
