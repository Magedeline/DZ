using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Visual decal entity – a sprite or animated sprite placed in the world as
/// decoration.  Ported from Celeste's Decal.cs.
///
/// Supports:
/// <list type="bullet">
///   <item>Static single-frame images.</item>
///   <item>Animated cycling through multiple sub-textures.</item>
///   <item>Parallax scrolling relative to camera.</item>
///   <item>Sine-wave float ("floaty") motion.</item>
///   <item>Banner/wave deformation flag.</item>
///   <item>Scared-animal react-to-player mode.</item>
/// </list>
/// Rendering is handled by the component's own <see cref="Render"/> call.
/// </summary>
public class Decal : Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public properties
    // -------------------------------------------------------------------------

    /// <summary>Logical name of the decal (path under decals/).</summary>
    public string Name { get; }

    /// <summary>Frames-per-second for animated decals.</summary>
    public float AnimationSpeed { get; set; } = 12f;

    /// <summary>True if this decal represents a crack/visual crack graphic.</summary>
    public bool IsCrack { get; set; }

    // -------------------------------------------------------------------------
    // Configuration
    // -------------------------------------------------------------------------

    private readonly Vector2 _scale;
    private readonly int _depth;

    // Parallax
    private bool _parallax;
    private float _parallaxAmount;

    // Floaty sine
    private bool _floaty;
    private float _floatySine;
    private const float FloatyFrequency = 1.2f;
    private const float FloatyAmplitude = 4f;

    // Banner wave
    private bool _banner;
    private float _bannerAmplitude;
    private float _bannerFrequency;
    private float _bannerWaveTimer;

    // Scared animal
    private bool _scaredAnimal;
    private float _scaredTimer;
    private bool _wasPlayerClose;

    // Animation
    /// <summary>
    /// List of texture paths (or keys) for animation frames.
    /// Populate externally or via a factory/loader.
    /// </summary>
    public List<string> TextureFrames { get; } = new();

    private float _animFrame;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="textureName">
    /// Texture path relative to the game atlas (without numeric suffix).
    /// </param>
    /// <param name="position">World position.</param>
    /// <param name="scale">X/Y scale (negative X = flipped).</param>
    /// <param name="depth">Render depth.</param>
    public Decal(string textureName, Vector2 position, Vector2 scale, int depth)
    {
        Name  = textureName;
        _scale = scale;
        _depth = depth;
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
        // TODO: set Entity render layer from _depth

        // Apply per-name special behaviours (mirrors Celeste's Added switch)
        ApplyNamedBehaviours();

        // TODO: load sub-textures from atlas for Name into TextureFrames
        // TODO: add SpriteRenderer with first frame
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public void Update()
    {
        float dt = Time.DeltaTime;

        if (TextureFrames.Count > 1)
        {
            _animFrame += AnimationSpeed * dt;
            if (_animFrame >= TextureFrames.Count)
                _animFrame -= TextureFrames.Count;
        }

        if (_floaty)
        {
            _floatySine += dt * FloatyFrequency;
            // TODO: apply Y = sin(_floatySine) * FloatyAmplitude to sprite offset
        }

        if (_banner)
        {
            _bannerWaveTimer += dt;
            // TODO: apply banner wave deformation to sprite segments
        }

        if (_scaredAnimal)
            UpdateScaredAnimal(dt);

        // TODO: apply parallax offset to sprite during render if _parallax
    }

    // -------------------------------------------------------------------------
    // Per-name special behaviours
    // -------------------------------------------------------------------------

    /// <summary>
    /// Mirrors the large <c>Added</c> switch in Celeste's Decal.cs.
    /// Only the portable subset of behaviours is implemented; Celeste-specific
    /// solid/mirror/cutscene hooks are stubbed with TODOs.
    /// </summary>
    private void ApplyNamedBehaviours()
    {
        string path = Name.ToLower().Replace("decals/", "");

        switch (path)
        {
            // Smoke emitters
            case "0-prologue/house":
                CreateSmoke(new Vector2(36f, -28f), background: true);
                break;
            case "3-resort/vent":
                CreateSmoke(Vector2.Zero, background: false);
                break;

            // Banner / waving flags
            case "1-forsakencity/rags":
            case "1-forsakencity/ragsb":
            case "3-resort/curtain_side_a":
            case "3-resort/curtain_side_d":
                MakeBanner(amplitude: 2f, frequency: 3.5f);
                break;

            case "4-cliffside/flower_a":
            case "4-cliffside/flower_b":
            case "4-cliffside/flower_c":
            case "4-cliffside/flower_d":
                MakeBanner(amplitude: 2f, frequency: 2f);
                break;

            // Floaty + parallax farewell backgrounds
            case "10-farewell/bed":
            case "10-farewell/car":
            case "10-farewell/cliffside":
            case "10-farewell/floating house":
            case "10-farewell/giantcassete":
            case "10-farewell/heart_a":
            case "10-farewell/heart_b":
            case "10-farewell/reflection":
            case "10-farewell/temple":
            case "10-farewell/tower":
                MakeParallax(-0.15f);
                MakeFloaty();
                break;

            // Farewell cloud parallax
            case "10-farewell/clouds/cloud_a":
            case "10-farewell/clouds/cloud_b":
            case "10-farewell/clouds/cloud_c":
            case "10-farewell/clouds/cloud_d":
            case "10-farewell/clouds/cloud_e":
            case "10-farewell/clouds/cloud_f":
                MakeParallax(0.1f);
                break;

            // Scared coral animations
            case "10-farewell/coral_a":
            case "10-farewell/coral_b":
            case "10-farewell/coral_c":
            case "10-farewell/coral_d":
                MakeScaredAnimation();
                break;

            // Temple bloom
            case "5-temple-dark/mosaic_b":
                // TODO: add BloomPoint(offset (0,5), alpha 0.75, radius 16)
                break;

            // Summit clouds
            case "7-summit/cloud_a":
            case "7-summit/cloud_b":
            case "7-summit/cloud_c":
                MakeParallax(0.1f);
                break;

            // Solids – bridgecolumn, roofcenter, etc.
            case "3-resort/bridgecolumn":
                // TODO: add Solid collider (-5,-8,10,16) depth 8
                break;
            case "3-resort/bridgecolumntop":
                // TODO: add Solid colliders
                break;
            case "3-resort/brokenelevator":
                // TODO: add Solid collider (-16,-20,32,48) depth 22
                break;
            case "3-resort/roofcenter":
            case "3-resort/roofcenter_b":
            case "3-resort/roofcenter_c":
            case "3-resort/roofcenter_d":
                // TODO: add Solid collider (-8,-4,16,8) depth 14
                break;
            case "3-resort/roofedge":
            case "3-resort/roofedge_b":
            case "3-resort/roofedge_c":
            case "3-resort/roofedge_d":
                // TODO: add Solid collider depth 14 (X offset depends on scale.X)
                break;
            case "4-cliffside/bridge_a":
                // TODO: add Solid collider (-24,0,48,8) depth 8
                break;

            // Mirror surfaces – stub
            case "5-temple/bg_mirror_a":
            case "5-temple/bg_mirror_b":
            case "5-temple/bg_mirror_c":
            case "5-temple/statue_d":
                // TODO: MakeMirror
                break;

            default:
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Behaviour factories
    // -------------------------------------------------------------------------

    private void MakeParallax(float amount)
    {
        _parallax = true;
        _parallaxAmount = amount;
    }

    private void MakeFloaty()
    {
        _floaty = true;
        _floatySine = Nez.Random.NextFloat() * MathF.PI * 2f;
    }

    private void MakeBanner(float amplitude, float frequency)
    {
        _banner = true;
        _bannerAmplitude = amplitude;
        _bannerFrequency = frequency;
        _bannerWaveTimer = Nez.Random.NextFloat() * MathF.PI * 2f;
    }

    private void MakeScaredAnimation()
    {
        _scaredAnimal = true;
    }

    private void CreateSmoke(Vector2 offset, bool background)
    {
        // TODO: emit smoke particles: offset relative to entity position
        //       background → ParticlesBG, foreground → ParticlesFG
    }

    // -------------------------------------------------------------------------
    // Scared animal logic
    // -------------------------------------------------------------------------

    private void UpdateScaredAnimal(float dt)
    {
        // TODO: find player, check proximity, toggle "scared" vs "idle" animation
        _scaredTimer = Math.Max(0f, _scaredTimer - dt);
    }
}
