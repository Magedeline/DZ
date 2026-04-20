using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using FMOD.Studio;
using IL.Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using FlingBirdIntroMod = Celeste.Entities.FlingBirdIntro;

namespace Celeste;

public class CS19_MissTheBird : CutsceneEntity
{
    public const string Flag = "MissTheBird";

    private Player player;

    private FlingBirdIntroMod flingBird;

    private BirdNPC bird;

    private Coroutine zoomRoutine;

    private EventInstance crashMusicSfx;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS19_MissTheBird(Player player, FlingBirdIntroMod flingBird)
    {
        this.player = player;
        this.flingBird = flingBird;
        Add(new LevelEndingHook(delegate
        {
            Audio.Stop(crashMusicSfx);
        }));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        Audio.SetMusicParam("bird_grab", 1f);
        crashMusicSfx = Audio.Play("event:/desolozantas/final_content/music/lvl19/cinematic/bird_crash_first");
        yield return flingBird.DoGrabbingRoutine(player);
        bird = new BirdNPC(flingBird.Position, BirdNPC.Modes.None);
        level.Add(bird);
        flingBird.RemoveSelf();
        yield return null;
        level.ResetZoom();
        level.Shake(0.5f);
        player.Position = player.Position.Floor();
        player.DummyGravity = true;
        player.DummyAutoAnimate = false;
        player.DummyFriction = false;
        player.ForceCameraUpdate = true;
        player.Speed = new Vector2(200f, 200f);
        bird.Position += Vector2.UnitX * 16f;
        bird.Add(new Coroutine(bird.Startle(null, 0.5f, new Vector2(3f, 0.25f))));
        Add(Alarm.Create(Alarm.AlarmMode.Oneshot, [MethodImpl(MethodImplOptions.NoInlining)] () =>
        {
            bird.Sprite.Play("hoverStressed");
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, [MethodImpl(MethodImplOptions.NoInlining)] () =>
            {
                Add(new Coroutine(bird.FlyAway(0.2f)));
                bird.Position += new Vector2(0f, -4f);
            }, 0.8f, start: true));
        }, 0.1f, start: true));
        while (!player.OnGround())
        {
            player.MoveVExact(1);
        }
        Engine.TimeRate = 0.5f;
        player.Sprite.Play("roll");
        while (player.Speed.X != 0f)
        {
            player.Speed.X = Calc.Approach(player.Speed.X, 0f, 120f * Engine.DeltaTime);
            if (Scene.OnInterval(0.1f))
            {
                Dust.BurstFG(player.Position, -MathF.PI / 2f, 2);
            }
            yield return null;
        }
        while (Engine.TimeRate < 1f)
        {
            Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 4f * Engine.DeltaTime);
            yield return null;
        }
        player.Speed.X = 0f;
        player.DummyFriction = true;
        yield return 0.25f;
        Add(zoomRoutine = new Coroutine(level.ZoomTo(new Vector2(160f, 110f), 1.5f, 6f)));
        yield return 1.5f;
        player.Sprite.Play("rollGetUp");
        yield return 0.5f;
        player.ForceCameraUpdate = false;
        yield return Textbox.Say("CH19_MISS_THE_BIRD", StandUpFaceLeft, TakeStepLeft, TakeStepRight, FlickerBlackhole, OpenBlackhole);
        StartMusic();
        EndCutscene(level);
    }

    private IEnumerator StandUpFaceLeft()
    {
        while (!zoomRoutine.Finished)
        {
            yield return null;
        }
        yield return 0.2f;
        Audio.Play("event:/char/madeline/stand", player.Position);
        player.DummyAutoAnimate = true;
        player.Sprite.Play("idle");
        yield return 0.2f;
        player.Facing = Facings.Left;
        yield return 0.5f;
    }

    private IEnumerator TakeStepLeft()
    {
        yield return player.DummyWalkTo(player.X - 16f);
    }

    private IEnumerator TakeStepRight()
    {
        yield return player.DummyWalkTo(player.X + 32f);
    }

    private IEnumerator FlickerBlackhole()
    {
        yield return 0.5f;
        Audio.Play("event:/desolozantas/final_content/game/19_farewell/glitch_medium");
        Audio.Play("event:/desolozantas/final_content/music/lvl19/cinematic/els_intro_laugh");
        yield return MoonGlitchBackgroundTrigger.GlitchRoutine(0.5f, stayOn: false);
        yield return player.DummyWalkTo(player.X - 8f, walkBackwards: true);
        yield return 0.4f;
    }

    private IEnumerator OpenBlackhole()
    {
        yield return 0.2f;
        Level.ResetZoom();
        Level.Flash(Color.White);
        Level.Shake(0.4f);
        Level.Add(new LightningStrike(new Vector2(player.X, Level.Bounds.Top), 80, 240f));
        Level.Add(new LightningStrike(new Vector2(player.X - 100f, Level.Bounds.Top), 90, 240f, 0.5f));
        Audio.Play("event:/desolozantas/final_content/music/lvl19/cinematic/els_intro_scream");
        yield return MoonGlitchBackgroundTrigger.GlitchRoutine(1.0f, stayOn: false);
        yield return 2.4f;
        Audio.Play("event:/desolozantas/final_content/game/19_farewell/lightning_strike");
        TriggerEnvironmentalEvents();
        StartMusic();
        yield return 1.2f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void StartMusic()
    {
        Level.Session.Audio.Music.Event = "event:/desolozantas/final_content/music/lvl19/part03";
        Level.Session.Audio.Ambience.Event = "event:/desolozantas/final_content/env/19_vortex";
        Level.Session.Audio.Apply(false);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void TriggerEnvironmentalEvents()
    {
        CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
        if (cutsceneNode != null)
        {
            RumbleTrigger.ManuallyTrigger(cutsceneNode.X, 0f);
        }
        base.Scene.Entities.FindFirst<MoonGlitchBackgroundTrigger>()?.Invoke();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        Audio.Stop(crashMusicSfx);
        Engine.TimeRate = 1f;
        level.Session.SetFlag("MissTheBird");
        if (WasSkipped)
        {
            player.Sprite.Play("idle");
            CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
            if (cutsceneNode != null)
            {
                player.Position = cutsceneNode.Position.Floor();
                level.Camera.Position = player.CameraTarget;
            }
            if (flingBird != null)
            {
                if (flingBird.CrashSfxEmitter != null)
                {
                    flingBird.CrashSfxEmitter.RemoveSelf();
                }
                flingBird.RemoveSelf();
            }
            if (bird != null)
            {
                bird.RemoveSelf();
            }
            TriggerEnvironmentalEvents();
            StartMusic();
        }
        player.Speed = Vector2.Zero;
        player.DummyAutoAnimate = true;
        player.DummyFriction = true;
        player.DummyGravity = true;
        player.ForceCameraUpdate = false;
        player.StateMachine.State = 0;
    }
}
