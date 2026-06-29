using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using System;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's SeekerStatue.cs.
///
/// A statue that breaks and spawns a Seeker when the player is nearby
/// or passes a certain point.
/// </summary>
public class SeekerStatue : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Enum
    // -------------------------------------------------------------------------

    public enum HatchMode
    {
        Distance,
        PlayerRightOfX
    }

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>The hatch mode that triggers the statue.</summary>
    public HatchMode Hatch { get; private set; }

    /// <summary>Whether the statue has hatched.</summary>
    public bool Hatched => _hatched;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2 _spawnPosition;
    private bool _hatched;
    private bool _hatching;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public SeekerStatue(Vector2 position, HatchMode hatch)
    {
        _spawnPosition = position;
        Hatch = hatch;
        Entity.UpdateOrder = 8999;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _spawnPosition;

        // TODO: load sprite "seeker"
        // TODO: play statue animation
        // TODO: set up OnLastFrame callback
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        if (_hatched || _hatching) return;

        var player = Entity.Scene?.FindEntityOfType<MadelinePlayer>();
        if (player == null) return;

        bool shouldHatch = false;

        switch (Hatch)
        {
            case HatchMode.Distance:
                // Hatch when player is within 220 pixels
                if ((player.Position - Entity.Position).Length() < 220f)
                    shouldHatch = true;
                break;

            case HatchMode.PlayerRightOfX:
                // Hatch when player is 32 pixels to the right
                if (player.Position.X > Entity.Position.X + 32f)
                    shouldHatch = true;
                break;
        }

        if (shouldHatch)
        {
            HatchStatue();
        }
    }

    // -------------------------------------------------------------------------
    // Hatch
    // -------------------------------------------------------------------------

    private void HatchStatue()
    {
        _hatching = true;

        // Emit break out particles
        BreakOutParticles();

        // TODO: play sound: event:/game/05_mirror_temple/seeker_statue_break

        // Set up callback for when animation finishes
        // TODO: set up OnLastFrame callback for "hatch" animation
        // OnLastFrame = f =>
        // {
        //     if (f == "hatch")
        //     {
        //         SpawnSeeker();
        //     }
        // };

        // TODO: sprite.Play("hatch");

        // Additional particle bursts
        // TODO: Add(Alarm.Set(() => BreakOutParticles(), 0.8f));
    }

    private void SpawnSeeker()
    {
        _hatched = true;

        // TODO: Scene.Add(new Seeker(Entity.Position, entityData, levelOffset)
        // {
        //     Light = { Alpha = 0f }
        // });

        Entity.Destroy();
    }

    private void BreakOutParticles()
    {
        // Emit particles in a circle
        for (float angle = 0f; angle < MathF.PI * 2f; angle += 0.17453292f) // ~10 degrees
        {
            float spread = (DZ.Nez.Random.NextFloat() - 0.5f) * MathF.PI / 45f; // ±2 degrees
            float distance = DZ.Nez.Random.Range(12, 20);

            Vector2 direction = new Vector2(MathF.Cos(angle + spread), MathF.Sin(angle + spread));
            Vector2 position = Entity.Position + direction * distance;

            // TODO: emit Seeker.P_BreakOut particle
        }
    }
}

/// <summary>
/// Stub for Seeker entity.
/// </summary>
public class Seeker : Entity
{
    // TODO: implement Seeker entity

    public Seeker(Vector2 position)
    {
        Position = position;
    }
}
