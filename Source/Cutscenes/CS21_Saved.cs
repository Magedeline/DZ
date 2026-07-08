using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// CS20_Saved cutscene - triggered when talking to NPCs after being saved
/// </summary>
public class CS21_Saved : CutsceneEntity
{
    private Player player;
    private EventInstance snapshot;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS21_Saved(Player player)
        : base(fadeInOnSkip: false)
    {
        this.player = player;
        base.Depth = -1000000;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        Level level = scene as Level;
        level.TimerStopped = true;
        level.TimerHidden = true;
        level.SaveQuitDisabled = true;
        snapshot = Audio.CreateSnapshot("snapshot:/game_10_granny_clouds_dialogue");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        // Setup
        player.StateMachine.State = Player.StDummy; // StDummy
        player.Sprite.Play("idle");
        
        yield return Textbox.Say("DZ_CH20_SAVED");
        
        EndCutscene(level);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        player.StateMachine.State = Player.StNormal; // StNormal
        level.TimerStopped = false;
        level.TimerHidden = false;
        level.SaveQuitDisabled = false;
        Dispose();
        
        // Chain to CS20_RestorationAndFarewell (which includes barrier break sequence)
        level.OnEndOfFrame += () =>
        {
            level.TeleportTo(player, "end-farewell", Player.IntroTypes.Transition);
        };
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        Dispose();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Dispose();
    }

    private void Dispose()
    {
        Audio.ReleaseSnapshot(snapshot);
        snapshot = null;
    }
}
