using System.Runtime.CompilerServices;
using Celeste.Entities;
using CutsceneNode = Celeste.Entities.CutsceneNode;
using FMOD.Studio;

namespace Celeste;

public class CS19_BirdIntroCutscene : CutsceneEntity
{
    private Player player;

    private FlingBirdIntroCutscene flingBird;

    private BirdNPC bird;

    private CharaDummy chara;

    private Vector2 birdWaitPosition;

    private EventInstance snapshot;

    private EventInstance crashMusicSfx;

    private bool crashes;

    private Coroutine zoomRoutine;

    public CS19_BirdIntroCutscene(Player player, FlingBirdIntroCutscene flingBird, bool crashes)
    {
        this.player = player;
        this.flingBird = flingBird;
        this.crashes = crashes;
        birdWaitPosition = flingBird.BirdEndPosition;
        if (!crashes)
        {
            Add(new LevelEndingHook(delegate
            {
                Audio.Stop(crashMusicSfx);
            }));
        }
    }

    public override void OnBegin(Level level)
    {
        Add(new Coroutine(Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
        if (crashes)
        {
            Audio.SetMusic("event:/pusheen/final_content/music/lvl19/cinematic/bird_crash_second");
            CustomCharaBoost boost = Scene.Entities.FindFirst<CustomCharaBoost>();
            if (boost != null)
            {
                bool visible = false;
                boost.Collidable = false;
                boost.Active = (boost.Visible = visible);
            }
            yield return flingBird.DoGrabbingRoutine(player);
            yield return CrashSequence(level);
            yield return level.ZoomBack(0.5f);
            if (chara != null)
            {
                chara.Vanish();
            }
            yield return 0.5f;
            if (boost != null)
            {
                Level.Displacement.AddBurst(boost.Center, 0.5f, 8f, 32f, 0.5f);
                Audio.Play("event:/new_content/char/badeline/booster_first_appear", boost.Center);
                bool visible = true;
                boost.Collidable = true;
                boost.Active = (boost.Visible = visible);
                yield return 0.2f;
            }
        }
        else
        {
            Audio.SetMusicParam("bird_grab", 1f);
            crashMusicSfx = Audio.Play("event:/pusheen/final_content/music/lvl19/cinematic/bird_crash_first");
            yield return flingBird.DoGrabbingRoutine(player);
            yield return MissSequence(level);
            StartMusic();
        }
        EndCutscene(level);
    }

    private IEnumerator CrashSequence(Level level)
    {
        flingBird.Sprite.Play("hurt");
        flingBird.X += 8f;
        while (!player.OnGround())
        {
            player.MoveVExact(1);
        }
        while (player.CollideCheck<Solid>())
        {
            player.Y--;
        }
        Engine.TimeRate = 0.65f;
        float ground = player.Position.Y;
        player.Dashes = 1;
        player.Sprite.Play("roll");
        player.Speed.X = 200f;
        player.DummyFriction = false;
        for (float p = 0f; p < 1f; p += Engine.DeltaTime)
        {
            player.Speed.X = Calc.Approach(player.Speed.X, 0f, 160f * Engine.DeltaTime);
            if (player.Speed.X != 0f && Scene.OnInterval(0.1f))
            {
                Dust.BurstFG(player.Position, -(float)Math.PI / 2f, 2);
            }
            flingBird.Position.X += Engine.DeltaTime * 80f * Ease.CubeOut(1f - p);
            flingBird.Position.Y = ground;
            yield return null;
        }
        player.Speed.X = 0f;
        player.DummyFriction = true;
        player.DummyGravity = true;
        yield return 0.25f;
        while (Engine.TimeRate < 1f)
        {
            Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 4f * Engine.DeltaTime);
            yield return null;
        }
        player.ForceCameraUpdate = false;
        yield return 0.6f;
        player.Sprite.Play("rollGetUp");
        yield return 0.8f;
        level.Session.Audio.Music.Event = "event:/pusheen/final_content/music/lvl19/tragiclost";
        level.Session.Audio.Apply(false);
        yield return Textbox.Say("CH19_KILL_THE_BIRD", BirdLooksHurt, BirdSquakOnGround, ApproachBird, ApproachBirdAgain, BadelineAppears, WaitABeat, MadelineSits, BadelineHugs, StandUp, ShiftCameraToBird);
    }

    private IEnumerator MissSequence(Level level)
    {
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
    }

    private IEnumerator BirdTwitches(string sfx = null)
    {
        flingBird.Sprite.Scale.Y = 1.6f;
        if (!string.IsNullOrWhiteSpace(sfx))
        {
            Audio.Play(sfx, flingBird.Position);
        }
        while (flingBird.Sprite.Scale.Y > 1f)
        {
            flingBird.Sprite.Scale.Y = Calc.Approach(flingBird.Sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
            yield return null;
        }
    }

    private IEnumerator BirdLooksHurt()
    {
        yield return 0.8f;
        yield return BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_1");
        yield return 0.4f;
        yield return BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_2");
        yield return 0.5f;
    }

    private IEnumerator BirdSquakOnGround()
    {
        yield return 0.6f;
        yield return BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_3");
        yield return 0.8f;
        Audio.Play("event:/new_content/game/10_farewell/bird_crashscene_recover", flingBird.Position);
        flingBird.RemoveSelf();
        Scene.Add(bird = new BirdNPC(flingBird.Position, BirdNPC.Modes.None));
        bird.Facing = Facings.Right;
        bird.Sprite.Play("recover");
        yield return 0.6f;
        bird.Facing = Facings.Left;
        bird.Sprite.Play("idle");
        bird.X += 3f;
        yield return 0.4f;
        yield return bird.Caw();
    }

    private IEnumerator ApproachBird()
    {
        player.DummyAutoAnimate = true;
        yield return 0.25f;
        yield return bird.Caw();
        Add(new Coroutine(player.DummyWalkTo(player.X + 20f)));
        yield return 0.1f;
        Audio.Play("event:/game/general/bird_startle", bird.Position);
        yield return bird.Startle("event:/new_content/game/10_farewell/bird_crashscene_relocate");
        yield return bird.FlyTo(new Vector2(player.X + 80f, player.Y), 3f, relocateSfx: false);
    }

    private IEnumerator ApproachBirdAgain()
    {
        Audio.Play("event:/new_content/game/10_farewell/bird_crashscene_leave", bird.Position);
        Add(new Coroutine(bird.FlyTo(birdWaitPosition, 2f, relocateSfx: false)));
        yield return player.DummyWalkTo(player.X + 20f);
        snapshot = Audio.CreateSnapshot("snapshot:/game_10_bird_wings_silenced");
        yield return 0.8f;
        bird.RemoveSelf();
        Scene.Add(bird = new BirdNPC(birdWaitPosition, BirdNPC.Modes.WaitForLightningOff));
        bird.Facing = Facings.Right;
        bird.FlyAwayUp = false;
        bird.WaitForLightningPostDelay = 1f;
    }

    private IEnumerator BadelineAppears()
    {
        yield return player.DummyWalkToExact((int)player.X + 20, walkBackwards: false, 0.5f);
        Level.Add(chara = new CharaDummy(player.Position + new Vector2(24f, -8f)));
        Level.Displacement.AddBurst(chara.Center, 0.5f, 8f, 32f, 0.5f);
        Audio.Play("event:/char/badeline/maddy_split", player.Position);
        chara.Sprite.Scale.X = -1f;
        yield return 0.2f;
    }

    private IEnumerator WaitABeat()
    {
        yield return player.DummyWalkToExact((int)player.X - 4, walkBackwards: true, 0.5f);
        yield return 0.8f;
    }

    private IEnumerator MadelineSits()
    {
        yield return 0.5f;
        yield return player.DummyWalkToExact((int)player.X - 16, walkBackwards: false, 0.25f);
        player.DummyAutoAnimate = false;
        player.Sprite.Play("sitDown");
        yield return 1.5f;
    }

    private IEnumerator BadelineHugs()
    {
        yield return 1f;
        yield return chara.FloatTo(chara.Position + new Vector2(0f, 8f), null, faceDirection: true, fadeLight: false, quickEnd: true);
        chara.Floatness = 0f;
        chara.AutoAnimateEnabled = false;
        chara.Sprite.Play("idle");
        Audio.Play("event:/char/badeline/landing", chara.Position);
        yield return 0.5f;
        yield return chara.WalkTo(player.X - 9f, 40f);
        chara.Sprite.Scale.X = 1f;
        yield return 0.2f;
        Audio.Play("event:/char/badeline/duck", chara.Position);
        chara.Depth = player.Depth + 5;
        chara.Sprite.Play("hug");
        yield return 1f;
    }

    private IEnumerator StandUp()
    {
        Audio.Play("event:/char/badeline/stand", chara.Position);
        yield return chara.WalkTo(chara.X - 8f);
        chara.Sprite.Scale.X = 1f;
        yield return 0.2f;
        player.DummyAutoAnimate = true;
        Level.NextColorGrade("none", 0.25f);
        yield return 0.25f;
    }

    private IEnumerator ShiftCameraToBird()
    {
        Audio.ReleaseSnapshot(snapshot);
        snapshot = null;
        Audio.Play("event:/new_content/char/badeline/birdcrash_scene_float", chara.Position);
        Add(new Coroutine(chara.FloatTo(player.Position + new Vector2(-16f, -16f), 1)));
        Level level = Scene as Level;
        player.Facing = Facings.Right;
        yield return level.ZoomAcross(level.ZoomFocusPoint + new Vector2(70f, 0f), 1.5f, 1f);
        yield return 0.4;
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
        Audio.Play("event:/pusheen/final_content/game/19_farewell/glitch_medium");
        Audio.Play("event:/pusheen/final_content/music/lvl19/cinematic/els_intro_laugh");
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
        Audio.Play("event:/pusheen/final_content/music/lvl19/cinematic/els_intro_scream");
        yield return MoonGlitchBackgroundTrigger.GlitchRoutine(1.0f, stayOn: false);
        yield return 2.4f;
        Audio.Play("event:/pusheen/final_content/game/19_farewell/lightning_strike");
        TriggerEnvironmentalEvents();
        StartMusic();
        yield return 1.2f;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void StartMusic()
    {
        Level.Session.Audio.Music.Event = "event:/pusheen/final_content/music/lvl19/part03";
        Level.Session.Audio.Ambience.Event = "event:/pusheen/final_content/env/19_vortex";
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

    public override void OnEnd(Level level)
    {
        Audio.ReleaseSnapshot(snapshot);
        snapshot = null;
        Audio.Stop(crashMusicSfx);
        Engine.TimeRate = 1f;
        player.Speed = Vector2.Zero;
        player.DummyGravity = true;
        player.DummyFriction = true;
        player.DummyAutoAnimate = true;
        player.ForceCameraUpdate = false;
        player.StateMachine.State = Player.StNormal;
        if (crashes)
        {
            CustomCharaBoost charaBoost = base.Scene.Entities.FindFirst<CustomCharaBoost>();
            if (charaBoost != null)
            {
                charaBoost.Active = (charaBoost.Visible = (charaBoost.Collidable = true));
            }
            if (chara != null)
            {
                chara.RemoveSelf();
            }
            if (flingBird != null)
            {
                if (flingBird.CrashSfxEmitter != null)
                {
                    flingBird.CrashSfxEmitter.RemoveSelf();
                }
                flingBird.RemoveSelf();
            }
            if (WasSkipped)
            {
                player.Sprite.Play("idle");
                CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
                if (cutsceneNode != null)
                {
                    player.Position = cutsceneNode.Position.Floor();
                    level.Camera.Position = player.CameraTarget;
                }
                foreach (Lightning item in base.Scene.Entities.FindAll<Lightning>())
                {
                    item.ToggleCheck();
                }
                base.Scene.Tracker.GetEntity<LightningRenderer>()?.ToggleEdges(immediate: true);
                if (bird != null)
                {
                    bird.RemoveSelf();
                }
                base.Scene.Add(bird = new BirdNPC(birdWaitPosition, BirdNPC.Modes.WaitForLightningOff));
                bird.Facing = Facings.Right;
                bird.FlyAwayUp = false;
                bird.WaitForLightningPostDelay = 1f;
                level.SnapColorGrade("none");
                level.Session.Audio.Music.Event = "event:/pusheen/final_content/music/lvl19/tragiclost";
                level.Session.Audio.Apply(false);
            }
        }
        else
        {
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
        }
        level.ResetZoom();
        Glitch.Value = 0f;
    }

    public override void Removed(Scene scene)
    {
        Audio.ReleaseSnapshot(snapshot);
        snapshot = null;
        base.Removed(scene);
    }

    public override void SceneEnd(Scene scene)
    {
        Audio.ReleaseSnapshot(snapshot);
        snapshot = null;
        base.SceneEnd(scene);
    }

    public static void HandlePostCutsceneSpawn(FlingBirdIntroCutscene flingBird, Level level)
    {
        CustomCharaBoost charaBoost = level.Entities.FindFirst<CustomCharaBoost>();
        if (charaBoost != null)
        {
            charaBoost.Active = (charaBoost.Visible = (charaBoost.Collidable = true));
        }
        flingBird?.RemoveSelf();
        BirdNPC birdNPC;
        level.Add(birdNPC = new BirdNPC(flingBird.BirdEndPosition, BirdNPC.Modes.WaitForLightningOff));
        birdNPC.Facing = Facings.Right;
        birdNPC.FlyAwayUp = false;
    }
}
