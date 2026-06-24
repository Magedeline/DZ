using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections;
using System.Collections.Generic;
using KirbyCelesteStandalone.Entities.Player;
using KirbyCelesteStandalone.Entities.Core;
using KirbyCelesteStandalone.Entities.Collectibles;

namespace KirbyCelesteStandalone.Entities.Level;

/// <summary>
/// Port of Celeste's LockBlock.cs.
///
/// A door that requires a Key to unlock. When the player approaches with a key,
/// the key flies to the lock and opens it.
/// </summary>
public class LockBlock : CelesteSolid
{
    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Unique identifier for this lock.</summary>
    public string Id { get; private set; }

    /// <summary>Whether the lock has started unlocking.</summary>
    public bool UnlockingRegistered { get; private set; }

    /// <summary>Whether this lock steps music progress when opened.</summary>
    public bool StepMusicProgress { get; private set; }

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private bool _opening;
    private string _spriteName;
    private string _unlockSfxName;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public LockBlock(Vector2 position, string id, bool stepMusicProgress, string spriteName, string? unlockSfx = null)
        : base(position, 32f, 32f, safe: false)
    {
        Id = id;
        StepMusicProgress = stepMusicProgress;
        _spriteName = spriteName;

        // Determine unlock sound
        if (string.IsNullOrWhiteSpace(unlockSfx))
        {
            _unlockSfxName = "event:/game/03_resort/key_unlock";
            if (spriteName == "temple_a")
                _unlockSfxName = "event:/game/05_mirror_temple/key_unlock_light";
            else if (spriteName == "temple_b")
                _unlockSfxName = "event:/game/05_mirror_temple/key_unlock_dark";
        }
        else
        {
            _unlockSfxName = unlockSfx;
        }
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToScene()
    {
        base.OnAddedToScene();

        // TODO: load sprite "lockdoor_{_spriteName}"
        // TODO: play idle animation
        // TODO: position sprite at center

        // Add player collider for key detection
        var playerTrigger = AddComponent(new BoxCollider(60f, 60f));
        playerTrigger.IsTrigger = true;
        playerTrigger.SetLocalOffset(new Vector2(16f, 16f));
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        if (_opening) return;

        // Check for player with key
        CheckPlayerWithKey();
    }

    // -------------------------------------------------------------------------
    // Interaction
    // -------------------------------------------------------------------------

    private void CheckPlayerWithKey()
    {
        var player = Scene.FindEntityOfType<MadelinePlayer>();
        if (player == null) return;

        // Check if player is within range (60 pixel radius)
        if (Vector2.DistanceSquared(Position + new Vector2(16f, 16f), player.Position) > 60f * 60f)
            return;

        // Check if player has a key
        // TODO: check GameState.Instance.HasKey() or check player followers
        // For now, look for Key components in the scene
        foreach (var key in Scene.FindComponentsOfType<Key>())
        {
            if (key.IsUsed || key.StartedUsing) continue;
            if (!key.IsHeld) continue;

            TryOpen(player, key);
            break;
        }
    }

    private void TryOpen(MadelinePlayer player, Key key)
    {
        if (_opening) return;

        // Check line of sight
        Collidable = false;
        bool hasLineOfSight = !CheckLineOfSightBlocked(player.Position, Center);
        Collidable = true;

        if (!hasLineOfSight) return;

        _opening = true;
        key.StartedUsing = true;

        // Start unlock routine
        // TODO: Add(new Coroutine(UnlockRoutine(key)));
    }

    private bool CheckLineOfSightBlocked(Vector2 from, Vector2 to)
    {
        // Simple line of sight check - cast ray
        var raycastHit = Nez.Physics.Linecast(from, to);
        return raycastHit.Collider != null;
    }

    private IEnumerator UnlockRoutine(Key key)
    {
        // TODO: play sound: _unlockSfxName

        // Use the key
        // TODO: Add(new Coroutine(key.UseRoutine(Center + new Vector2(0f, 2f))));

        yield return 1.2f;

        UnlockingRegistered = true;

        if (StepMusicProgress)
        {
            // TODO: increment level music progress
        }

        // TODO: mark as DoNotLoad
        key.RegisterUsed();

        // Wait for key animation
        while (key.Turning)
            yield return null;

        // Open the door
        Collidable = false;

        // TODO: play open animation
        // TODO: shake level
        // TODO: rumble
        // TODO: play burst animation

        yield return 0.5f;

        // Destroy the lock block
        Destroy();
    }

    /// <summary>
    /// Animation when the lock appears (used for temple locks).
    /// </summary>
    public void Appear()
    {
        Visible = true;
        // TODO: play appear animation

        // TODO: Add(Alarm.Create(() =>
        // {
        //     // TODO: emit particles
        //     // TODO: shake level
        // }, 0.25f));
    }
}

