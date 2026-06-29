using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;

namespace DZ.Entities.Level;

/// <summary>
/// One-shot ambient sound emitter that removes itself when the sound finishes.
/// Ported from Celeste's SoundEmitter.cs.
///
/// <para>
/// Usage – static factory:
/// <code>
///   SoundEmitter.Play("event:/sfx/…");
///   SoundEmitter.Play("event:/sfx/…", followEntity);
/// </code>
/// </para>
///
/// The emitter component is added to a new entity in the current scene.
/// When the sound finishes playing the entity is automatically destroyed.
/// </summary>
public class SoundEmitter : DZ.Nez.Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public
    // -------------------------------------------------------------------------

    /// <summary>SFX event path that is playing.</summary>
    public string SfxEvent { get; }

    /// <summary>Whether the sound is still playing (approximate).</summary>
    public bool IsPlaying { get; private set; } = true;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private readonly DZ.Nez.Entity _followTarget;
    private readonly Vector2     _offset;

    // Simple duration timer as a proxy for "is playing" when no audio system exists
    private float _lifetimeTimer;
    private const float FallbackDuration = 5f; // seconds until self-remove

    // -------------------------------------------------------------------------
    // Static factories
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a sound emitter at the current scene's world origin.
    /// </summary>
    public static SoundEmitter Play(string sfx)
    {
        var entity  = new DZ.Nez.Entity("SoundEmitter");
        var emitter = new SoundEmitter(sfx);
        entity.AddComponent(emitter);

        // TODO: Core.Scene?.AddEntity(entity) — wire to current Nez scene
        return emitter;
    }

    /// <summary>
    /// Creates a sound emitter that follows <paramref name="follow"/>.
    /// </summary>
    public static SoundEmitter Play(string sfx, DZ.Nez.Entity follow, Vector2 offset = default)
    {
        var entity  = new DZ.Nez.Entity("SoundEmitter");
        var emitter = new SoundEmitter(sfx, follow, offset);
        entity.AddComponent(emitter);

        // TODO: Core.Scene?.AddEntity(entity)
        return emitter;
    }

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    public SoundEmitter(string sfx)
    {
        SfxEvent = sfx;
        // TODO: play sound event: sfx at Entity.Position
    }

    public SoundEmitter(string sfx, DZ.Nez.Entity follow, Vector2 offset)
    {
        SfxEvent      = sfx;
        _followTarget = follow;
        _offset       = offset;
        // TODO: play sound event: sfx at follow.Position + offset
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        if (_followTarget != null)
            Entity.Position = _followTarget.Position + _offset;
        // TODO: play sound: SfxEvent at Entity.Position
    }

    // -------------------------------------------------------------------------
    // IUpdatable
    // -------------------------------------------------------------------------

    public override void Update()
    {
        float dt = Time.DeltaTime;

        // Follow target position
        if (_followTarget != null)
            Entity.Position = _followTarget.Position + _offset;

        // TODO: check actual audio system "IsPlaying" status
        _lifetimeTimer += dt;
        if (_lifetimeTimer >= FallbackDuration)
        {
            IsPlaying = false;
            Entity.Destroy();
        }
    }

    // -------------------------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------------------------

    /// <summary>Stops the sound early (e.g. on level transition).</summary>
    public void Stop()
    {
        IsPlaying = false;
        // TODO: stop FMOD event instance
    }

    public override void OnRemovedFromEntity()
    {
        Stop();
        base.OnRemovedFromEntity();
    }
}
