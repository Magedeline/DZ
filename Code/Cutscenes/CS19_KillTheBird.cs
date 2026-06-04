using Celeste.Entities;
using CutsceneNode = Celeste.Entities.CutsceneNode;
using FMOD.Studio;
using Facings = Celeste.Facings;
using FlingBirdIntro = Celeste.Entities.FlingBirdIntro;
using BirdNPC = Celeste.Entities.BirdNPC;
using IL.Celeste;

namespace Celeste.Cutscenes;

public class CS19_KillTheBird : CutsceneEntity
{
    private global::Celeste.Player player;

    private FlingBirdIntro flingBird;

    private CharaDummy chara;

    private BirdNPC bird;

    private Vector2 birdWaitPosition;

    private EventInstance snapshot;

    public CS19_KillTheBird(global::Celeste.Player player)
    {
        this.player = player;
    }

    public override void OnBegin(Level level)
    {
        // Find the FlingBirdIntroMod in the scene
        flingBird = Scene.Entities.FindFirst<FlingBirdIntro>();
        if (flingBird != null)
        {
            birdWaitPosition = flingBird.BirdEndPosition;
        }
        Add(new Coroutine(Cutscene(level)));
    }

    private IEnumerator Cutscene(Level level)
    {
        Audio.SetMusic("guid://{2b922a75-db87-4bbb-9d8f-ad18017d39c6}");
        CharaBoost boost = Scene.Entities.FindFirst<CharaBoost>();
        if (boost != null)
        {
            bool visible = false;
            boost.Collidable = false;
            boost.Active = (boost.Visible = visible);
        }
        yield return flingBird.DoGrabbingRoutine(player);
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
        level.Session.Audio.Music.Event = "event:/new_content/music/pusheen/lvl19/tragiclost";
        level.Session.Audio.Apply(false);
        yield return Textbox.Say("MAGGYHELPER_CH19_KILL_THE_BIRD", BirdLooksHurt, BirdSquakOnGround, ApproachBird, ApproachBirdAgain, BadelineAppears, WaitABeat, MadelineSits, BadelineHugs, StandUp, ShiftCameraToBird);
        yield return level.ZoomBack(0.5f);
        if (chara != null)
        {
            chara.Vanish();
        }
        yield return 0.5f;
        if (boost != null)
        {
            Level.Displacement.AddBurst(boost.Center, 0.5f, 8f, 32f, 0.5f);
            Audio.Play("guid://{5a554fe3-4398-4960-ab5e-70dc62685ded}", boost.Center);
            bool visible = true;
            boost.Collidable = true;
            boost.Active = (boost.Visible = visible);
            yield return 0.2f;
        }
        EndCutscene(level);
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
        yield return BirdTwitches("guid://{712650d6-3edc-40f8-96e7-c4d70aabc7fb}");
        yield return 0.4f;
        yield return BirdTwitches("guid://{59ecf2bc-ad5d-4055-ad4d-c477789353f9}");
        yield return 0.5f;
    }

    private IEnumerator BirdSquakOnGround()
    {
        yield return 0.6f;
        yield return BirdTwitches("guid://{84666200-9dc1-435c-86dc-c576f7fea100}");
        yield return 0.8f;
        Audio.Play("guid://{2bb2c164-719e-4ead-8dbd-f6943e93ec31}", flingBird.Position);
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
        Audio.Play("guid://{7ea54ed8-0285-4dfc-a1ce-558444a4aa59}", bird.Position);
        yield return bird.Startle("guid://{a1745c53-2c9e-4545-bc28-275b56d913d8}");
        yield return bird.FlyTo(new Vector2(player.X + 80f, player.Y), 3f, relocateSfx: false);
    }

    private IEnumerator ApproachBirdAgain()
    {
        Audio.Play("guid://{d3a1e63e-2265-4652-988f-3d07724ab466}", bird.Position);
        Add(new Coroutine(bird.FlyTo(birdWaitPosition, 2f, relocateSfx: false)));
        yield return player.DummyWalkTo(player.X + 20f);
        snapshot = Audio.CreateSnapshot("guid://{5955bc4d-e3b4-4663-a676-bdaf632c0d78}");
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
        Audio.Play("guid://{450fb5b3-e9e3-45d8-9f34-ba05e292958f}", player.Position);
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
        Audio.Play("guid://{8f924592-8b14-40d6-81af-d42fab0b6da1}", chara.Position);
        yield return 0.5f;
        yield return chara.WalkTo(player.X - 9f, 40f);
        chara.Sprite.Scale.X = 1f;
        yield return 0.2f;
        Audio.Play("guid://{d879f9dd-98d0-479e-9f08-a1848f4a0f5c}", chara.Position);
        chara.Depth = player.Depth + 5;
        chara.Sprite.Play("hug");
        yield return 1f;
    }

    private IEnumerator StandUp()
    {
        Audio.Play("guid://{1a114663-4b93-4aab-ba8c-ca8793f2831e}", chara.Position);
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
        Audio.Play("guid://{a38a422d-553d-4a98-bd77-4fb94c9a40a8}", chara.Position);
        Add(new Coroutine(chara.FloatTo(player.Position + new Vector2(-16f, -16f), 1)));
        Level level = Scene as Level;
        player.Facing = Facings.Right;
        yield return level.ZoomAcross(level.ZoomFocusPoint + new Vector2(70f, 0f), 1.5f, 1f);
        yield return 0.4;
    }

    public override void OnEnd(Level level)
    {
        Audio.ReleaseSnapshot(snapshot);
        snapshot = null;
        if (WasSkipped)
        {
            CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
            if (cutsceneNode != null)
            {
                player.Sprite.Play("idle");
                player.Position = cutsceneNode.Position.Floor();
                level.Camera.Position = player.CameraTarget;
            }
            foreach (Lightning item in base.Scene.Entities.FindAll<Lightning>())
            {
                item.ToggleCheck();
            }
            base.Scene.Tracker.GetEntity<LightningRenderer>()?.ToggleEdges(immediate: true);
            level.Session.Audio.Music.Event = "event:/new_content/music/pusheen/lvl19/tragiclost";
            level.Session.Audio.Apply(false);
        }
        player.Speed = Vector2.Zero;
        player.DummyGravity = true;
        player.DummyFriction = true;
        player.DummyAutoAnimate = true;
        player.ForceCameraUpdate = false;
        player.StateMachine.State = Player.StNormal;
        CharaBoost charaBoost = base.Scene.Entities.FindFirst<CharaBoost>();
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
            if (bird != null)
            {
                bird.RemoveSelf();
            }
            base.Scene.Add(bird = new BirdNPC(birdWaitPosition, BirdNPC.Modes.WaitForLightningOff));
            bird.Facing = Facings.Right;
            bird.FlyAwayUp = false;
            bird.WaitForLightningPostDelay = 1f;
            level.SnapColorGrade("none");
        }
        level.ResetZoom();
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

    public static void HandlePostCutsceneSpawn(global::Celeste.Entities.FlingBirdIntro flingBird, Level level)
    {
        CharaBoost charaBoost = level.Entities.FindFirst<CharaBoost>();
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

