using Microsoft.Xna.Framework;
using Nez;
using static Nez.Time;
using System;
using System.Collections;
using System.Collections.Generic;
using KirbyCelesteStandalone.Entities.Player;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's BadelineBoost.cs.
///
/// A boost pad that launches the player through a series of nodes.
/// Badeline appears and throws the player. Used for long-distance travel.
/// </summary>
public class BadelineBoost : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const float MoveSpeed = 320f;

    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Whether the player can skip this boost sequence.</summary>
    public bool CanSkip { get; private set; }

    /// <summary>Whether this is the final Chapter 9 boost.</summary>
    public bool FinalCh9Boost { get; private set; }

    /// <summary>Whether this is the golden run final boost.</summary>
    public bool FinalCh9GoldenBoost { get; private set; }

    /// <summary>Whether to show final Chapter 9 dialog.</summary>
    public bool FinalCh9Dialog { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Vector2[] _nodes;
    private int _nodeIndex;
    private bool _travelling;
    private MadelinePlayer? _holding;
    private float _canSkipTimer;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public BadelineBoost(Vector2[] nodes, bool lockCamera = true, bool canSkip = false,
        bool finalCh9Boost = false, bool finalCh9GoldenBoost = false, bool finalCh9Dialog = false)
    {
        _nodes = nodes;
        CanSkip = canSkip;
        FinalCh9Boost = finalCh9Boost;
        FinalCh9GoldenBoost = finalCh9GoldenBoost;
        FinalCh9Dialog = finalCh9Dialog;
        // Depth = -1000000; // TODO: Depth not available in Nez.Entity

        if (lockCamera)
        {
            // TODO: Add(new CameraLocker(Level.CameraLockModes.BoostSequence, 0f, 160f));
        }
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();
        Entity.Position = _nodes[0];

        // 16 radius circle collider
        var collider = Entity.AddComponent(new CircleCollider(16f));
        collider.IsTrigger = true;

        // TODO: load sprite "badelineBoost"
        // TODO: add stretch image
        // TODO: add light and bloom
        // TODO: add wiggler

        // Check if in fake wall and adjust depth
        // TODO: if (CollideCheck<FakeWall>()) Depth = -12500;
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public void Update()
    {
        // Emit ambience particles
        // TODO: if (Scene.OnInterval(0.1f)) emit particles
    }

    // -------------------------------------------------------------------------
    // Interaction
    // -------------------------------------------------------------------------

    private void OnPlayer(MadelinePlayer player)
    {
        if (_travelling) return;

        // TODO: Add(new Coroutine(BoostRoutine(player)));
    }

    private IEnumerator BoostRoutine(MadelinePlayer player)
    {
        _holding = player;
        _travelling = true;
        _nodeIndex++;

        bool finalBoost = _nodeIndex >= _nodes.Length;
        bool endLevel = false;

        // Determine if this ends the level
        if (FinalCh9GoldenBoost)
        {
            endLevel = true;
        }
        else if (finalBoost && FinalCh9Boost)
        {
            // Check if player has golden strawberry
            bool hasGolden = false;
            // TODO: check player followers for golden strawberry
            endLevel = !hasGolden;
        }

        // Play sounds
        if (FinalCh9Boost)
        {
            // TODO: play sound: event:/new_content/char/badeline/booster_finalfinal_part1
        }
        else if (!finalBoost)
        {
            // TODO: play sound: event:/char/badeline/booster_begin
        }
        else
        {
            // TODO: play sound: event:/char/badeline/booster_final
        }

        // Drop held item
        // TODO: if (player.Holding != null) player.Drop();

        // Put player in dummy state
        // TODO: player.StateMachine.State = StDummy;
        // TODO: player.DummyAutoAnimate = false;
        // TODO: player.DummyGravity = false;

        // Refill dashes
        // TODO: if (player.Inventory.Dashes > 1) player.Dashes = 1; else player.RefillDash();
        player.Stamina = 110f;
        player.Speed = Vector2.Zero;

        // Create Badeline dummy
        // TODO: var badeline = new BadelineDummy(Entity.Position);
        // TODO: Scene.Add(badeline);

        // Position player and Badeline
        int facingDir = MathF.Sign(player.Position.X - Entity.Position.X);
        if (facingDir == 0) facingDir = -1;

        // player.Facing = -facingDir; // TODO: Facing set accessor is inaccessible
        // TODO: badeline.Sprite.Scale.X = facingDir;

        // Animate to positions
        Vector2 playerFrom = player.Position;
        Vector2 playerTo = Entity.Position + new Vector2(facingDir * 4f, -3f);
        // TODO: Vector2 badelineFrom = badeline.Position;
        // TODO: Vector2 badelineTo = Entity.Position + new Vector2(-facingDir * 4f, 3f);

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.DeltaTime;
            float t = elapsed / duration;
            player.Position = Vector2.Lerp(playerFrom, playerTo, t);
            // TODO: badeline.Position = Vector2.Lerp(badelineFrom, badelineTo, t);
            yield return null;
        }

        // Final boost setup
        if (finalBoost)
        {
            // TODO: zoom camera
            // TODO: Engine.TimeRate = 0.5f;
        }
        else
        {
            // TODO: play sound: event:/char/badeline/booster_throw
        }

        // TODO: badeline.Sprite.Play("boost");
        yield return 0.1f;

        if (!player.Dead)
        {
            // TODO: player.MoveV(5f);
        }

        yield return 0.1f;

        if (endLevel)
        {
            // TODO: level.TimerStopped = true;
            // TODO: level.RegisterAreaComplete();
        }

        if (finalBoost && FinalCh9Boost)
        {
            // TODO: Scene.Add(new CS10_FinalLaunch(player, this, FinalCh9Dialog));
            // player.Active = false; // TODO: Active not available in Nez.Entity
            // TODO: badeline.Active = false;
            // Entity.Active = false; // TODO: Active not available in Nez.Entity
            yield break;
        }

        // Restore dash after boost
        // TODO: Add(Alarm.Create(() =>
        // {
        //     if (player.Dashes < player.MaxDashes) player.Dashes++;
        //     Scene.Remove(badeline);
        //     // TODO: emit displacement burst
        // }, 0.15f));

        // Shake and launch player
        // TODO: level.Shake();
        _holding = null;

        if (!finalBoost)
        {
            // Launch to next node
            // TODO: player.BadelineBoostLaunch(CenterX);

            Vector2 from = Entity.Position;
            Vector2 to = _nodes[_nodeIndex];
            float moveDuration = MathF.Min(3f, Vector2.Distance(from, to) / MoveSpeed);

            elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.DeltaTime;
                float t = Ease.SineInOut(elapsed / moveDuration);
                Entity.Position = Vector2.Lerp(from, to, t);

                // TODO: stretch effect
                // TODO: add trails
                // TODO: emit particles

                yield return null;
            }

            // Arrive at next boost location
            // TODO: sprite.Visible = true;
            // TODO: stretch.Visible = false;
            _travelling = false;

            // Continue if there are more nodes
            if (_nodeIndex < _nodes.Length - 1)
            {
                // TODO: relocateSfx.Play("event:/char/badeline/booster_relocate");
                yield return 0.5f;
            }
            else
            {
                // Remove this boost
                Entity.Destroy();
            }
        }
        else
        {
            // Final boost done
            Entity.Destroy();
        }
    }

    // -------------------------------------------------------------------------
    // Easing
    // -------------------------------------------------------------------------

    private static class Ease
    {
        public static float SineInOut(float t) => -(MathF.Cos(MathF.PI * t) - 1f) / 2f;
    }
}
