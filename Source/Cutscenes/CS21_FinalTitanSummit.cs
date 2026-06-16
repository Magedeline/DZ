using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// Chapter 21 - Final Titan Summit to Titan Els
/// This cutscene triggers the Titan Els confrontation within Chapter 21
/// before the epilogue sequence.
/// </summary>
[Tracked]
[HotReloadable]
public class CS21_FinalTitanSummit : CutsceneEntity
{
    public const string FLAG = "ch21_final_titan_summit_complete";
    public const string BEGIN_FLAG = "ch21_final_titan_summit_begin";

    private readonly global::Celeste.Player player;
    private FinalTitanSummitBackgroundManager backgroundManager;
    private bool summitStarted;

    public CS21_FinalTitanSummit(global::Celeste.Player player) : base(true, false)
    {
        this.player = player ?? throw new ArgumentNullException(nameof(player));
    }

    public override void OnBegin(Level level)
    {
        level.Session.SetFlag(BEGIN_FLAG);
        Add(new Coroutine(Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
        if (player?.StateMachine == null) yield break;

        // Initialize summit state
        player.StateMachine.State = Player.StDummy; // Dummy state
        summitStarted = true;

        // Activate the Titan Summit background manager if present
        backgroundManager = level.Tracker.GetEntity<FinalTitanSummitBackgroundManager>();
        if (backgroundManager != null)
        {
            // Trigger the summit visuals
            yield return 1f;
        }

        // Fade in summit atmosphere
        yield return 2f;

        // Dialog: Approach to the summit
        yield return Textbox.Say("DZ_CH21_FINAL_SUMMIT_APPROACH");

        yield return 1f;

        // Environmental effects intensify
        Audio.Play("event:/new_content/game/pusheen/21_desolo_zantas/multiple_lightning_strike");

        yield return 2f;

        // Dialog: Titan Els presence detected
        yield return Textbox.Say("DZ_CH21_TITAN_ELS_MANIFESTS");

        yield return 1f;

        // The summit sequence leads into the final confrontation
        // This sets up the transition to the Titan Els boss fight
        level.Session.SetFlag("titan_els_summit_reached");

        yield return 1f;

        EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
        if (player != null)
        {
            player.StateMachine.State = Player.StNormal; // Normal state
        }

        level.Session.SetFlag(FLAG);

        // This flag triggers the transition to the Titan Els encounter
        // The actual boss fight is handled by separate trigger entities
        level.Session.SetFlag("ready_for_titan_els_encounter");
    }

    public override void Update()
    {
        base.Update();

        // Sync with background manager if active
        if (summitStarted && backgroundManager != null && backgroundManager.Scene == null)
        {
            backgroundManager = null;
        }
    }
}
