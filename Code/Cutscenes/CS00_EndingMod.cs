using System;
using System.Collections;
using System.Runtime.CompilerServices;
using global::Celeste.Mod.MaggyHelper.Cutscenes;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

[Tracked(false)]
public class CS00_EndingMod : CutsceneEntity
{
    private class EndingCutsceneDelay : Entity
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public EndingCutsceneDelay()
        {
            Add(new Coroutine(Routine()));
        }

        private IEnumerator Routine()
        {
            yield return 3f;
            Level level = Scene as Level;
            Player player = level.Tracker.GetEntity<Player>();
            level.Add(new CS00_Logo(player, level.Tracker.GetEntity<CS00_EndingMod>()));
        }
    }

    private Player player;

    private Entities.BirdNPC bird;

    private Entities.Bridge bridge;

    private bool keyOffed;

    private PrologueEndingVignetteText endingText;

    private TimeRateModifier timeRateModifier;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS00_EndingMod(Player player, Entities.BirdNPC bird, Entities.Bridge bridge)
        : base(fadeInOnSkip: false, endingChapterAfter: false)
    {
        this.player = player;
        this.bird = bird;
        this.bridge = bridge;
        Add(timeRateModifier = new TimeRateModifier(1f, false));
    }

    public CS00_EndingMod(Player player)
        : base(fadeInOnSkip: false, endingChapterAfter: false)
    {
        this.player = player;
        Add(timeRateModifier = new TimeRateModifier(1f, false));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        timeRateModifier.SetTimeRateMultiplier(0f);
        while (timeRateModifier.CurrentTimeRate() > 0f)
        {
            yield return null;
            if (timeRateModifier.CurrentTimeRate() < 0.5f && bridge != null)
            {
                bridge.StopCollapseLoop();
            }
            level.StopShake();
            MInput.GamePads[Input.Gamepad].StopRumble();
            timeRateModifier.SetTimeRateMultiplier(Calc.Approach(timeRateModifier.CurrentTimeRate(), 0f, Engine.RawDeltaTime * 2f));
        }
        timeRateModifier.SetTimeRateMultiplier(0f);
        player.StateMachine.State = Player.StDummy;
        player.Facing = Facings.Right;
        yield return WaitFor(1f);
        EventInstance instance = Audio.Play("event:/game/general/bird_in", bird.Position);
        bird.Facing = Facings.Left;
        bird.Sprite.Play("fall");
        float percent = 0f;
        Vector2 from = bird.Position;
        Vector2 to = bird.StartPosition;
        while (percent < 1f)
        {
            bird.Position = from + (to - from) * Ease.QuadOut(percent);
            Audio.Position(instance, bird.Position);
            if (percent > 0.5f)
            {
                bird.Sprite.Play("fly");
            }
            percent += Engine.RawDeltaTime * 0.5f;
            yield return null;
        }
        bird.Position = to;
        Audio.Play("event:/game/general/bird_land_dirt", bird.Position);
        Dust.Burst(bird.Position, -MathF.PI / 2f, 12, null);
        bird.Sprite.Play("idle");
        yield return WaitFor(0.5f);
        bird.Sprite.Play("peck");
        yield return WaitFor(1.1f);
        yield return bird.ShowTutorial(new BirdTutorialGui(bird, new Vector2(0f, -16f), Dialog.Clean("tutorial_dash"), new Vector2(1f, -1f), "+", BirdTutorialGui.ButtonPrompt.Dash), caw: true);
        while (true)
        {
            Vector2 aimVector = Input.GetAimVector();
            if (aimVector.X > 0f && aimVector.Y < 0f && Input.Dash.Pressed)
            {
                break;
            }
            yield return null;
        }
        player.StateMachine.State = Player.StBirdDashTutorial;
        player.Dashes = 0;
        level.Session.Inventory.Dashes = 1;
        timeRateModifier.ResetTimeRateMultiplier();
        keyOffed = true;
        Audio.CurrentMusicEventInstance.triggerCue();
        bird.Add(new Coroutine(bird.HideTutorial()));
        yield return 0.25f;
        bird.Add(new Coroutine(bird.StartleAndFlyAway()));
        while (!player.Dead && !player.OnGround())
        {
            yield return null;
        }
        yield return 2f;
        Audio.SetMusic("event:/music/pusheen/lvl0/title_ping", true, true);
        yield return 2f;
        endingText = new PrologueEndingVignetteText(instant: false);
        Scene.Add(endingText);
        Snow bgSnow = level.Background.Get<Snow>();
        Snow fgSnow = level.Foreground.Get<Snow>();
        level.Add(level.HiresSnow = new HiresSnow());
        level.HiresSnow.Alpha = 0f;
        float ease = 0f;
        while (ease < 1f)
        {
            ease += Engine.DeltaTime * 0.25f;
            float num = Ease.CubeInOut(ease);
            if (fgSnow != null)
            {
                fgSnow.Alpha -= Engine.DeltaTime * 0.5f;
            }
            if (bgSnow != null)
            {
                bgSnow.Alpha -= Engine.DeltaTime * 0.5f;
            }
            level.HiresSnow.Alpha = Calc.Approach(level.HiresSnow.Alpha, 1f, Engine.DeltaTime * 0.5f);
            endingText.Position = new Vector2(960f, 540f - 1080f * (1f - num));
            level.Camera.Y = (float)level.Bounds.Top - 3900f * num;
            yield return null;
        }
        EndCutscene(level);
    }

    private IEnumerator WaitFor(float time)
    {
        for (float t = 0f; t < time; t += Engine.RawDeltaTime)
        {
            yield return null;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        if (WasSkipped)
        {
            if (bird != null)
            {
                bird.Visible = false;
            }
            if (player != null)
            {
                player.Position = new Vector2(2120f, 40f);
                player.StateMachine.State = Player.StDummy;
                player.DummyAutoAnimate = false;
                player.Sprite.Play("tired");
                player.Speed = Vector2.Zero;
            }
            if (!keyOffed)
            {
                Audio.CurrentMusicEventInstance.triggerCue();
            }
            if (level.HiresSnow == null)
            {
                level.Add(level.HiresSnow = new HiresSnow());
            }
            level.HiresSnow.Alpha = 1f;
            Snow snow = level.Background.Get<Snow>();
            if (snow != null)
            {
                snow.Alpha = 0f;
            }
            Snow snow2 = level.Foreground.Get<Snow>();
            if (snow2 != null)
            {
                snow2.Alpha = 0f;
            }
            if (endingText != null)
            {
                level.Remove(endingText);
            }
            level.Add(endingText = new PrologueEndingVignetteText(instant: true));
            endingText.Position = new Vector2(960f, 540f);
            level.Camera.Y = level.Bounds.Top - 3900;
        }
        level.PauseLock = true;
        level.Entities.FindFirst<SpeedrunTimerDisplay>().CompleteTimer = 10f;
        level.Add(new EndingCutsceneDelay());
    }
}
