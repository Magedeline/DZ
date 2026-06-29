using Microsoft.Xna.Framework;
using DZ.Nez;
using Scene = DZ.Nez.Scene;
using Entity = DZ.Nez.Entity;
using Component = DZ.Nez.Component;
using System;
using System.Collections.Generic;
using DZ.Entities.Player;

namespace DZ.Entities.Level;

/// <summary>
/// Port of Celeste's Switch.cs.
///
/// Component that manages the activation state of touch switches and other
/// switch-based puzzles. Tracks when all switches in a level are activated.
/// </summary>
public class Switch : Component, IUpdatable
{
    // -------------------------------------------------------------------------
    // Public fields / properties
    // -------------------------------------------------------------------------

    /// <summary>Whether this switch resets when the player touches ground.</summary>
    public bool GroundReset { get; private set; }

    /// <summary>Called when the switch is activated.</summary>
    public Action OnActivate;

    /// <summary>Called when the switch is deactivated.</summary>
    public Action OnDeactivate;

    /// <summary>Called when all switches in the level are finished.</summary>
    public Action OnFinish;

    /// <summary>Called when starting in a finished state.</summary>
    public Action OnStartFinished;

    /// <summary>Whether this switch is currently activated.</summary>
    public bool Activated { get; private set; }

    /// <summary>Whether all switches in the level are finished.</summary>
    public bool Finished { get; private set; }

    /// <summary>Static event fired when any switch is activated.</summary>
    public static event Action OnAnySwitchActivated;

    /// <summary>Static event fired when all switches are finished.</summary>
    public static event Action OnAllSwitchesFinished;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public Switch(bool groundReset)
    {
        GroundReset = groundReset;
    }

    // -------------------------------------------------------------------------
    // Nez lifecycle
    // -------------------------------------------------------------------------

    public override void OnAddedToEntity()
    {
        base.OnAddedToEntity();

        // Check if level flag is already set
        if (CheckLevelFlag())
        {
            StartFinished();
        }
    }

    // -------------------------------------------------------------------------
    // Update
    // -------------------------------------------------------------------------

    public override void Update()
    {
        if (!GroundReset || !Activated || Finished) return;

        // Check if player is on ground to reset
        var player = Entity.Scene?.FindEntityOfType<MadelinePlayer>();
        if (player == null) return;

        // TODO: check if player is on ground and reset if so
        // if (player.OnGround()) Deactivate();
    }

    // -------------------------------------------------------------------------
    // Activation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Activates this switch. Returns true if this activation finished all switches.
    /// </summary>
    public bool Activate()
    {
        if (Finished || Activated) return false;

        Activated = true;
        OnActivate?.Invoke();
        OnAnySwitchActivated?.Invoke();

        return CheckAllSwitchesFinished();
    }

    /// <summary>
    /// Deactivates this switch (if GroundReset is true).
    /// </summary>
    public void Deactivate()
    {
        if (Finished || !Activated) return;

        Activated = false;
        OnDeactivate?.Invoke();
    }

    /// <summary>
    /// Marks all switches as finished.
    /// </summary>
    public void Finish()
    {
        if (Finished) return;

        Finished = true;
        OnFinish?.Invoke();
        OnAllSwitchesFinished?.Invoke();

        // Set level flag
        SetLevelFlag();
    }

    /// <summary>
    /// Starts the switch in a finished state (already completed).
    /// </summary>
    public void StartFinished()
    {
        if (Finished) return;

        Finished = true;
        Activated = true;
        OnStartFinished?.Invoke();
    }

    // -------------------------------------------------------------------------
    // Static methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Checks if any switch in the scene is finished.
    /// </summary>
    public static bool Check(Scene scene)
    {
        var switches = scene.FindComponentsOfType<Switch>().ToList();
        foreach (var sw in switches)
        {
            if (sw.Finished) return true;
        }
        return false;
    }

    private bool CheckAllSwitchesFinished()
    {
        if (Entity.Scene == null) return false;

        var switches = Entity.Scene.FindComponentsOfType<Switch>().ToList();
        foreach (var sw in switches)
        {
            if (!sw.Activated) return false;
        }

        // All activated - finish them all
        foreach (var sw in switches)
        {
            sw.Finish();
        }

        return true;
    }

    private bool CheckLevelFlag()
    {
        // TODO: check GameState.Instance.GetFlag("switches_" + levelName)
        return false;
    }

    private void SetLevelFlag()
    {
        // TODO: GameState.Instance.SetFlag("switches_" + levelName, true)
    }
}

/// <summary>
/// Mathematical helper functions.
/// </summary>
public static class Calc
{
    public static float Approach(float val, float target, float maxMove)
    {
        return val > target ? Math.Max(val - maxMove, target) : Math.Min(val + maxMove, target);
    }
}
