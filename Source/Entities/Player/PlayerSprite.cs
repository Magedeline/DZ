using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;
using System.Collections.Generic;
using Component = Nez.Component;

namespace KirbyCelesteStandalone.Entities.Player;

/// <summary>
/// Port of the conceptual role of Celeste's PlayerSprite.cs as a Nez
/// <see cref="Component"/>.
///
/// <c>PlayerSprite</c> acts as the animation controller for the player entity.
/// It tracks which animation is currently playing, advances frame timing, and
/// exposes metadata used by other systems (e.g. <see cref="PlayerHair"/>).
///
/// Actual texture loading and draw-calls are delegated to a
/// <see cref="SpriteRenderer"/> component that is added to the same entity
/// (see the TODO notes inside <see cref="OnAddedToEntity"/>).
///
/// Animation states mirror the subset used in Celeste:
/// <c>Idle</c>, <c>Run</c>, <c>Jump</c>, <c>Fall</c>, <c>Climb</c>, <c>Dash</c>.
/// </summary>
public class PlayerSprite : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Animation catalogue
    // -------------------------------------------------------------------------

    /// <summary>
    /// Recognised animation identifiers — a subset of Celeste's sprite XML IDs.
    /// Extend this list as you add sprite frames.
    /// </summary>
    public static class Animations
    {
        public const string Idle   = "idle";
        public const string Run    = "run";
        public const string Jump   = "jump";
        public const string Fall   = "fall";
        public const string Climb  = "climb";
        public const string Dash   = "dash";
    }

    // -------------------------------------------------------------------------
    // Frame timing metadata (populate when you have a real atlas)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Per-animation frame counts.  When an atlas is wired up replace the
    /// placeholder values with the real frame counts from your sprite sheet.
    /// </summary>
    private static readonly Dictionary<string, int> AnimationFrameCounts = new()
    {
        { Animations.Idle,  4 },
        { Animations.Run,   8 },
        { Animations.Jump,  2 },
        { Animations.Fall,  2 },
        { Animations.Climb, 6 },
        { Animations.Dash,  2 },
    };

    /// <summary>
    /// Per-animation frame duration (seconds per frame).
    /// Adjust to match the frame rates used in your sprite sheet.
    /// </summary>
    private static readonly Dictionary<string, float> AnimationFrameDurations = new()
    {
        { Animations.Idle,  0.15f },
        { Animations.Run,   0.08f },
        { Animations.Jump,  0.12f },
        { Animations.Fall,  0.12f },
        { Animations.Climb, 0.10f },
        { Animations.Dash,  0.07f },
    };

    // -------------------------------------------------------------------------
    // Hair metadata
    // -------------------------------------------------------------------------

    /// <summary>
    /// Hair segment count associated with the current animation.
    /// Celeste changes this per-anim (e.g. more segments while climbing).
    /// Default is 4 matching <see cref="PlayerHair.HairCount"/>.
    /// </summary>
    public int HairCount { get; private set; } = 4;

    /// <summary>
    /// Local-space offset from <see cref="Entity.Position"/> at which hair
    /// segments originate (the "hair anchor" on the sprite head).
    /// Set this to match the pixel position on your actual frames.
    /// </summary>
    public Vector2 HairOffset { get; private set; } = new Vector2(0f, -4f);

    /// <summary>Whether the current animation has hair attached.</summary>
    public bool HasHair { get; private set; } = true;

    // -------------------------------------------------------------------------
    // Transform
    // -------------------------------------------------------------------------

    /// <summary>
    /// Non-uniform scale applied to the sprite renderer.
    /// Flip X to mirror the sprite; scale Y for squash-and-stretch effects.
    /// </summary>
    public Vector2 Scale { get; set; } = Vector2.One;

    // -------------------------------------------------------------------------
    // Animation state
    // -------------------------------------------------------------------------

    /// <summary>The name of the animation that is currently playing.</summary>
    public string CurrentAnimation { get; private set; } = Animations.Idle;

    /// <summary>Zero-based index of the frame currently being displayed.</summary>
    public int CurrentFrame { get; private set; }

    /// <summary>Accumulated time within the current frame (seconds).</summary>
    private float _frameTimer;

    /// <summary>Whether the animation is currently looping.</summary>
    private bool _looping = true;

    /// <summary>Whether the animation has finished playing (non-looping).</summary>
    public bool AnimationFinished { get; private set; }

    /// <summary>
    /// Callback invoked once when a non-looping animation completes.
    /// Reset automatically when <see cref="Play"/> is called.
    /// </summary>
    public Action? OnAnimationComplete;

    // -------------------------------------------------------------------------
    // Render position (convenience)
    // -------------------------------------------------------------------------

    /// <summary>
    /// World-space position at which the sprite is rendered.
    /// In this simplified port this is identical to the entity position;
    /// override or extend for sub-pixel or camera-offset rendering.
    /// </summary>
    public Vector2 RenderPosition => Entity.Position;

    // -------------------------------------------------------------------------
    // Component references
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reference to the <see cref="SpriteRenderer"/> attached to this entity.
    /// Set in <see cref="OnAddedToEntity"/>; used to push scale changes.
    /// </summary>
    private SpriteRenderer? _spriteRenderer;

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // Add (or retrieve) a SpriteRenderer so callers can reference it.
        _spriteRenderer = Entity.GetComponent<SpriteRenderer>()
                       ?? Entity.AddComponent(new SpriteRenderer());

        // TODO: load sprite atlas and register animation frames, e.g.:
        //   var atlas = Entity.Scene.Content.LoadTexture("player_atlas");
        //   _spriteRenderer.SetSprite(new Sprite(atlas));
        //   // then wire up frame regions per animation name.

        // Start on idle.
        Play(Animations.Idle, restart: true);
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    /// <summary>
    /// Advances the active animation by one frame's worth of time and pushes
    /// the current scale to the <see cref="SpriteRenderer"/>.
    /// </summary>
    public void Update()
    {
        AdvanceAnimation(Time.DeltaTime);
        PushScaleToRenderer();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Switches to the animation identified by <paramref name="animName"/>.
    /// </summary>
    /// <param name="animName">
    ///   One of the constants in <see cref="Animations"/>, or any custom
    ///   animation name registered in the atlas.
    /// </param>
    /// <param name="restart">
    ///   If <c>true</c>, restart the animation from frame 0 even if the same
    ///   animation is already playing.
    /// </param>
    public void Play(string animName, bool restart = false)
    {
        if (!restart && CurrentAnimation == animName) return;

        CurrentAnimation   = animName;
        CurrentFrame       = 0;
        _frameTimer        = 0f;
        AnimationFinished  = false;
        OnAnimationComplete = null;

        // Update hair metadata for this animation.
        UpdateHairMetadata(animName);

        // TODO: push the first frame texture region to the SpriteRenderer.
        // e.g. _spriteRenderer?.SetSprite(GetFrameSprite(animName, 0));
    }

    /// <summary>
    /// Plays <paramref name="animName"/> and invokes <paramref name="onComplete"/>
    /// once the non-looping animation finishes.
    /// </summary>
    public void PlayOneShot(string animName, Action? onComplete = null)
    {
        Play(animName, restart: true);
        _looping = false;
        OnAnimationComplete = onComplete;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Ticks the frame timer and advances frames as needed.</summary>
    private void AdvanceAnimation(float dt)
    {
        if (AnimationFinished) return;

        _frameTimer += dt;

        float frameDuration = GetFrameDuration(CurrentAnimation);
        int   totalFrames   = GetFrameCount(CurrentAnimation);

        while (_frameTimer >= frameDuration)
        {
            _frameTimer -= frameDuration;
            CurrentFrame++;

            if (CurrentFrame >= totalFrames)
            {
                if (_looping)
                {
                    CurrentFrame = 0;
                }
                else
                {
                    CurrentFrame      = totalFrames - 1;
                    AnimationFinished = true;
                    OnAnimationComplete?.Invoke();
                    OnAnimationComplete = null;
                    break;
                }
            }

            // TODO: push the new frame texture region to the SpriteRenderer.
            // e.g. _spriteRenderer?.SetSprite(GetFrameSprite(CurrentAnimation, CurrentFrame));
        }
    }

    /// <summary>
    /// Synchronises <see cref="Scale"/> to the attached <see cref="SpriteRenderer"/>.
    /// </summary>
    private void PushScaleToRenderer()
    {
        if (_spriteRenderer == null) return;

        // SpriteRenderer exposes FlipX / FlipY rather than a raw scale vector.
        // Negative X scale → flip horizontally.
        if (Scale.X < 0)
        {
            _spriteRenderer.FlipX = true;
            _spriteRenderer.RenderLayer = _spriteRenderer.RenderLayer; // no-op; keep layer
        }
        else
        {
            _spriteRenderer.FlipX = false;
        }

        // TODO: push actual scale to renderer transform, e.g.:
        // Entity.Transform.Scale = new Vector2(Math.Abs(Scale.X), Scale.Y);
    }

    /// <summary>Updates hair count and offset for the given animation name.</summary>
    private void UpdateHairMetadata(string animName)
    {
        // Customise these per-animation to match Celeste's sprite XML.
        switch (animName)
        {
            case Animations.Climb:
                HairCount  = 4;
                HairOffset = new Vector2(0f, -6f);
                HasHair    = true;
                break;

            case Animations.Dash:
                HairCount  = 3; // fewer segments during fast dash
                HairOffset = new Vector2(0f, -3f);
                HasHair    = true;
                break;

            default:
                HairCount  = 4;
                HairOffset = new Vector2(0f, -4f);
                HasHair    = true;
                break;
        }
    }

    /// <summary>Returns the frame count for <paramref name="animName"/>, defaulting to 1.</summary>
    private static int GetFrameCount(string animName)
        => AnimationFrameCounts.TryGetValue(animName, out int count) ? count : 1;

    /// <summary>
    /// Returns seconds per frame for <paramref name="animName"/>, defaulting to 0.1s.
    /// </summary>
    private static float GetFrameDuration(string animName)
        => AnimationFrameDurations.TryGetValue(animName, out float dur) ? dur : 0.1f;
}
