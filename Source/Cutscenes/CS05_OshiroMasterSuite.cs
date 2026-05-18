using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Entities;
using Celeste.NPCs;
using BadelineDummy = Celeste.Entities.BadelineDummy;
using NPC = Celeste.NPCs.NPC;

namespace Celeste.Cutscenes;

public class CS05_OshiroMasterSuite : CutsceneEntity
{
    public const string Flag = "oshiro_resort_suite";

    private Player player;

    private NPC oshiro;

    private BadelineDummy badeline;

    private RalseiDummy ralsei;

    private Entities.ResortMirror mirror;

    public CS05_OshiroMasterSuite(NPC oshiro)
    {
        this.oshiro = oshiro;
    }

    public override void OnBegin(Level level)
    {
        mirror = base.Scene.Entities.FindFirst<Entities.ResortMirror>();
        Add(new Coroutine(Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
        while (true)
        {
            player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                break;
            }
            yield return null;
        }
        Audio.SetMusic(null);
        yield return 0.4f;
        player.StateMachine.State = 11;
        player.StateMachine.Locked = true;
        if (oshiro != null)
        {
            Add(new Coroutine(player.DummyWalkTo(oshiro.X + 10f)));
        }
        yield return 1f;

        // Spawn Badeline at start
        badeline = new BadelineDummy(player.Position + new Vector2(-24f, -16f));
        Scene.Add(badeline);

        Audio.SetMusic("event:/desolozantas/music/lvl5/oshiro_theme");
        yield return Textbox.Say("CH5_OSHIRO_SUITE", BadelineLookAround, RalseiAppear, CharaAppearInMirror, CharaBreakMirror, PlayerStepCloser, EveryoneJumpBack);

        // After dialogue: Chara breaks ceiling and exits
        yield return SceneAs<Level>().ZoomBack(0.5f);
        // Note: evil/badeline exit sound would go here if needed
        yield return badeline.FloatTo(new Vector2(badeline.X, level.Bounds.Top - 0.8f));
        Scene.Remove(badeline);

        // Restore lighting
        while (level.Lighting.Alpha != level.BaseLightingAlpha)
        {
            level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, level.BaseLightingAlpha, Engine.DeltaTime * 0.5f);
            yield return null;
        }

        EndCutscene(level);
    }

    // Trigger 0: Badeline looks around the room
    private IEnumerator BadelineLookAround()
    {
        yield return 1f;
        yield return badeline.FloatTo(badeline.Position + new Vector2(0f, 8f), null, faceDirection: true, fadeLight: false, quickEnd: true);
        badeline.Floatness = 0f;
        yield return 0.5f;
        badeline.Sprite.Play("idle");
        Audio.Play("event:/char/badeline/landing", badeline.Position);
        yield return 1f;
        yield return badeline.WalkTo(player.X - 0f, 20f);
        badeline.Sprite.Scale.X = 1f;
        yield return 0.2f;
        Audio.Play("event:/char/badeline/duck", badeline.Position);
        yield return 0.3f;
        badeline.Sprite.Play("duck");
        yield return 1f;
        badeline.Sprite.Play("lookUp");
        yield return 1f;
        badeline.Sprite.Play("idle");
        yield return 0.4f;
        badeline.Sprite.Scale.X = 1f;
        yield return badeline.FloatTo(new Vector2(player.X - 2f, badeline.Y), null, false);
        yield return 0.5f;
        Level level = SceneAs<Level>();
        yield return level.ZoomTo(new Vector2(190f, 110f), 2f, 0.5f);
    }

    // Trigger 1: Ralsei appears
    private IEnumerator RalseiAppear()
    {
        ralsei = new RalseiDummy(badeline.Position + new Vector2(24f, -8f));
        Scene.Add(ralsei);
        Level.Displacement.AddBurst(ralsei.Center, 0.5f, 8f, 32f, 0.5f);
        Audio.Play("event:/char/badeline/maddy_split", badeline.Position);
        ralsei.Sprite.Scale.X = -1f;
        yield return 0.2f;
    }

    // Trigger 2: Chara appears in mirror
    private IEnumerator CharaAppearInMirror()
    {
        if (mirror != null)
        {
            mirror.EvilAppear();
            SetEvilMusic();
            Audio.Play("event:/desolozantas/game/05_restore/suite_chara_intro", mirror.Position);
            Vector2 from = Level.ZoomFocusPoint;
            Vector2 to = new Vector2(216f, 110f);
            for (float p = 0f; p < 1f; p += Engine.DeltaTime * 2f)
            {
                Level.ZoomFocusPoint = from + (to - from) * Ease.SineInOut(p);
                yield return null;
            }
            yield return null;
        }
    }

    // Trigger 3: Chara breaks mirror
    private IEnumerator CharaBreakMirror()
    {
        if (mirror != null)
        {
            Audio.Play("event:/desolozantas/game/05_restore/suite_bad_mirrorbreak", mirror.Position);
            yield return mirror.SmashRoutine();
            yield return 1.2f;
            if (oshiro != null && oshiro.Sprite != null)
            {
                oshiro.Sprite.Scale.X = 1f;
            }
            yield return Level.ZoomBack(0.5f);
        }
    }

    // Trigger 4: Player steps closer
    private IEnumerator PlayerStepCloser()
    {
        if (oshiro != null)
        {
            yield return player.DummyWalkToExact((int)oshiro.X - 16);
        }
    }

    // Trigger 5: Everyone jumps back
    private IEnumerator EveryoneJumpBack()
    {
        if (oshiro != null)
        {
            yield return player.DummyWalkToExact((int)oshiro.X - 24, walkBackwards: true);
        }
        yield return 0.8f;
    }

    private void SetEvilMusic()
    {
        if (Level.Session.Area.Mode == AreaMode.Normal)
        {
            Level.Session.Audio.Music.Event = "event:/desolozantas/music/lvl2/evil_chara";
            Level.Session.Audio.Apply(false);
        }
    }

    public override void OnEnd(Level level)
    {
        if (WasSkipped)
        {
            if (badeline != null)
            {
                base.Scene.Remove(badeline);
            }
            if (mirror != null)
            {
                mirror.Broken();
            }
            base.Scene.Entities.FindFirst<DashBlock>()?.RemoveAndFlagAsGone();
            if (oshiro != null && oshiro.Sprite != null)
            {
                oshiro.Sprite.Play("idle_ground");
            }
        }
        if (oshiro != null && oshiro.Talker != null)
        {
            oshiro.Talker.Enabled = true;
        }
        if (player != null)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }
        level.Lighting.Alpha = level.BaseLightingAlpha;
        level.Session.SetFlag("oshiro_resort_suite");
        SetEvilMusic();
    }
}
