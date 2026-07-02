using global::Celeste.Mod.DZ;
using Celeste.Entities;
using BadelineDummy = Celeste.Entities.BadelineDummy;
using Monocle;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste.UI;
using DZ;

namespace Celeste.Cutscenes
{
    public class CS17_Credits : CutsceneEntity
    {
        public const float CameraXOffset = 70f;
        public const float CameraYOffset = -24f;
        public static CS17_Credits Instance;
        public string Event;
        private MTexture gradient = GFX.Gui["creditsgradient"].GetSubtexture(0, 1, 1920, 1);
        private CreditsDZ credits;
        private global::Celeste.Player player;
        private bool autoWalk = true;
        private bool autoUpdateCamera = true;
        private BadelineDummy badeline;
        private bool badelineAutoFloat = true;
        private bool badelineAutoWalk;
        private bool chara_auto_float = true;
        private bool chara_auto_walk = true;
        private float badelineWalkApproach;
        private Vector2 badelineWalkApproachFrom;
        private float walkOffset;
        private bool wasDashAssistOn;
        private CS17_Credits.Fill fillbg;
        private float fade = 1f;
        private DZHiresSnow snow;
        private bool gotoEpilogue;
        private CharaDummy chara;
        private Entity kirbyFollower;
        private bool kirby_auto_walk = true;
        private Entity ralseiFollower;
        private bool ralsei_auto_float = true;
        private bool ralsei_auto_walk = true;

        public CS17_Credits() : base(true, false)
        {
            MInput.Disabled = true;
            CS17_Credits.Instance = this;
            this.Tag = (Tags.Global | Tags.HUD);
            this.wasDashAssistOn = SaveData.Instance.Assists.DashAssist;
            SaveData.Instance.Assists.DashAssist = false;
        }

        public override void OnBegin(Level level)
        {
            Audio.BusMuted("bus:/gameplay_sfx", true);

            if (DZModule.Session != null)
            {
                DZModule.Session.InCredits = true;
                DZModule.Session.CreditsPhase = 1;
                DZModule.Session.CreditsCompleted = false;
            }

            this.gotoEpilogue = !DZModule.IsChapter17EpilogueCompleted();
            this.Add(new Coroutine(this.Routine(), true));
            this.Add(new PostUpdateHook(new Action(this.PostUpdate)));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            (this.Scene as Level).InCredits = true;
        }

        private IEnumerator Routine()
        {
            this.Level.Background.Backdrops.Add(this.fillbg = new CS17_Credits.Fill());
            this.Level.Completed = true;
            this.Level.Entities.FindFirst<SpeedrunTimerDisplay>()?.RemoveSelf();
            this.Level.Entities.FindFirst<TotalStrawberriesDisplay>()?.RemoveSelf();
            this.Level.Entities.FindFirst<GameplayStats>()?.RemoveSelf();
            yield return null;
            this.Level.Wipe.Cancel();
            yield return 0.5f;
            this.credits = new CreditsDZ();
            this.credits.AllowInput = false;
            yield return 3f;
            this.SetBgFade(0f);
            this.Add(new Coroutine(this.FadeTo(0f), true));
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            yield return this.WaitForPlayer();
            yield return this.FadeTo(1f);
            yield return 1f;
            this.SetBgFade(0.1f);
            yield return this.NextLevel("credits-dashes");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            this.Add(new Coroutine(this.FadeTo(0f), true));
            yield return this.WaitForPlayer();
            yield return this.FadeTo(1f);
            yield return 1f;
            this.SetBgFade(0.2f);
            yield return this.NextLevel("credits-walking");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            this.Add(new Coroutine(this.FadeTo(0f), true));
            yield return 5.8f;
            this.badelineAutoFloat = false;
            yield return 0.5f;
            this.badeline.Sprite.Scale.X = 1f;
            yield return 0.5f;
            this.autoWalk = false;
            this.player.Speed = Vector2.Zero;
            this.player.Facing = Facings.Right;
            yield return 1.5f;
            this.badeline.Sprite.Scale.X = -1f;
            yield return 1f;
            this.badeline.Sprite.Scale.X = -1f;
            this.badelineAutoWalk = true;
            this.badelineWalkApproachFrom = this.badeline.Position;
            this.Add(new Coroutine(this.BadelineApproachWalking(), true));
            yield return 0.7f;
            this.autoWalk = true;
            this.player.Facing = Facings.Left;
            yield return this.WaitForPlayer();
            yield return this.FadeTo(1f);
            yield return 1f;
            this.SetBgFade(0.3f);
            yield return this.NextLevel("credits-tree");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            Petals petals = new Petals();
            this.Level.Foreground.Backdrops.Add(petals);
            this.autoUpdateCamera = false;
            Vector2 target = this.Level.Camera.Position + new Vector2(-220f, 32f);
            this.Level.Camera.Position += new Vector2(-100f, 0f);
            this.badelineWalkApproach = 1f;
            this.badelineAutoFloat = false;
            this.badelineAutoWalk = true;
            this.badeline.Floatness = 0f;
            this.Add(new Coroutine(this.FadeTo(0f), true));
            this.Add(new Coroutine(CutsceneEntity.CameraTo(target, 12f, Ease.Linear, 0f), true));
            yield return 3.5f;
            this.chara.Sprite.Play("idle", false, false);
            this.chara_auto_float = false;
            this.chara_auto_walk = false;
            yield return 0.25f;
            this.badeline.Sprite.Play("idle", false, false);
            this.badelineAutoWalk = false;
            yield return 0.25f;
            this.autoWalk = false;
            this.player.Sprite.Play("idle", false, false);
            this.player.Speed = Vector2.Zero;
            this.player.DummyAutoAnimate = false;
            this.player.Facing = Facings.Right;
            yield return 0.5f;
            this.player.Sprite.Play("sitDown", false, false);
            yield return 4f;
            this.chara.Sprite.Play("laugh", false, false);
            yield return 1.75f;
            yield return this.FadeTo(1f);
            this.Level.Foreground.Backdrops.Remove(petals);
            petals = null;
            yield return 1f;
            this.SetBgFade(0.4f);
            yield return this.NextLevel("credits-clouds");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            this.autoWalk = false;
            this.player.Speed = Vector2.Zero;
            this.autoUpdateCamera = false;
            this.player.ForceCameraUpdate = false;
            this.badeline.Visible = false;
            if (this.kirbyFollower != null) this.kirbyFollower.Visible = false;
            this.kirby_auto_walk = false;
            if (this.ralseiFollower != null) this.ralseiFollower.Visible = false;
            this.ralsei_auto_float = false;
            this.ralsei_auto_walk = false;
            global::Celeste.Player other = null;
            foreach (var entity in this.Scene.Tracker.GetEntities<CreditsTrigger>())
            {
                var creditsTrigger = entity as CreditsTrigger;
                if (creditsTrigger != null && creditsTrigger.Event == "BadelineOffset")
                {
                    other = new global::Celeste.Player(creditsTrigger.Position, (global::Celeste.PlayerSpriteMode)PlayerSpriteMode.Badeline);
                    // other.OverrideHairColor = new Color?(BadelineOldsite.HairColor); // Uncomment if property exists
                    yield return null;
                    other.StateMachine.State = 11;
                    other.Facing = Facings.Left;
                    this.Scene.Add(other);
                }
            }
            this.Add(new Coroutine(this.FadeTo(0f), true));
            this.Level.Camera.Position += new Vector2(0f, -100f);
            Vector2 target2 = this.Level.Camera.Position + new Vector2(0f, 160f);
            this.Add(new Coroutine(CutsceneEntity.CameraTo(target2, 12f, Ease.Linear, 0f), true));
            float playerHighJump = 0f;
            float baddyHighJump = 0f;
            for (float p = 0f; p < 10f; p += Engine.DeltaTime)
            {
                if (((p > 3f && p < 6f) || p > 9f) && this.player.Speed.Y < 0f && this.player.OnGround(4))
                    playerHighJump = 0.25f;
                if (p > 5f && p < 8f && other.Speed.Y < 0f && other.OnGround(4))
                    baddyHighJump = 0.25f;
                if (playerHighJump > 0f)
                {
                    playerHighJump -= Engine.DeltaTime;
                    this.player.Speed = new Vector2(this.player.Speed.X, -200f);
                }
                if (baddyHighJump > 0f)
                {
                    baddyHighJump -= Engine.DeltaTime;
                    other.Speed = new Vector2(other.Speed.X, -200f);
                }
                yield return null;
            }
            yield return this.FadeTo(1f);
            if (this.kirbyFollower != null) this.kirbyFollower.Visible = true;
            this.kirby_auto_walk = true;
            if (this.ralseiFollower != null) this.ralseiFollower.Visible = true;
            this.ralsei_auto_float = true;
            this.ralsei_auto_walk = true;
            other = null;
            yield return 1f;
            this.SetBgFade(0.5f);
            yield return this.NextLevel("credits-resort");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            this.Add(new Coroutine(this.FadeTo(0f), true));
            this.badelineWalkApproach = 1f;
            this.badelineAutoFloat = false;
            this.badelineAutoWalk = true;
            this.badeline.Floatness = 0f;
            Vector2 value = this.GetCreditsTriggerPosition("Oshiro", Vector2.Zero);
            NPC oshiro = new NPC(value + new Vector2(0f, 4f));
            oshiro.Add(oshiro.Sprite = new DZOshiroSprite(1));
            oshiro.MoveAnim = "sweeping";
            oshiro.IdleAnim = "sweeping";
            oshiro.Sprite.Play("sweeping", false, false);
            oshiro.Maxspeed = 10f;
            oshiro.Depth = -60;
            this.Scene.Add(oshiro);
            this.Add(new Coroutine(this.DustyRoutine(oshiro), true));
            yield return 4.8f;
            Vector2 oshiroTarget = oshiro.Position + new Vector2(116f, 0f);
            Coroutine oshiroRoutine = new Coroutine(oshiro.MoveTo(oshiroTarget), true);
            this.Add(oshiroRoutine);
            yield return 2f;
            this.autoUpdateCamera = false;
            yield return CutsceneEntity.CameraTo(new Vector2(this.Level.Bounds.Left + 64, this.Level.Bounds.Top), 2f);
            yield return 5f;
            Entities.BirdNPC bird = new Entities.BirdNPC(oshiro.Position + new Vector2(280f, -160f), Entities.BirdNPC.Modes.None);
            bird.Depth = 10010;
            bird.Light.Visible = false;
            this.Scene.Add(bird);
            bird.Facing = Facings.Left;
            bird.Sprite.Play("fall", false, false);
            Vector2 from = bird.Position;
            Vector2 to = oshiroTarget + new Vector2(50f, -12f);
            baddyHighJump = 0f;
            while (baddyHighJump < 1f)
            {
                bird.Position = from + (to - from) * Ease.QuadOut(baddyHighJump);
                if (baddyHighJump > 0.5f)
                {
                    bird.Sprite.Play("fly", false, false);
                    bird.Depth = -1000000;
                    bird.Light.Visible = true;
                }
                baddyHighJump += Engine.DeltaTime * 0.5f;
                yield return null;
            }
            bird.Position = to;
            oshiroRoutine.RemoveSelf();
            oshiro.Sprite.Play("putBroomAway", false, false);
            oshiro.Sprite.OnFrameChange = (anim) =>
            {
                if (oshiro.Sprite.CurrentAnimationFrame == 10)
                {
                    Entity entity3 = new Entity(oshiro.Position);
                    entity3.Depth = oshiro.Depth + 1;
                    this.Scene.Add(entity3);
                    entity3.Add(new Monocle.Image(GFX.Game["characters/oshiro/broom"]) { Origin = oshiro.Sprite.Origin });
                    oshiro.Sprite.OnFrameChange = null;
                }
            };
            bird.Sprite.Play("idle", false, false);
            yield return 0.5f;
            bird.Sprite.Play("croak", false, false);
            yield return 0.6f;
            from = default(Vector2);
            to = default(Vector2);
            oshiro.Maxspeed = 40f;
            oshiro.MoveAnim = "move";
            oshiro.IdleAnim = "idle";
            yield return oshiro.MoveTo(oshiroTarget + new Vector2(14f, 0f));
            yield return 2f;
            this.Add(new Coroutine(bird.StartleAndFlyAway(), true));
            yield return 0.75f;
            bird.Light.Visible = false;
            bird.Depth = 10010;
            oshiro.Sprite.Scale.X = -1f;
            yield return this.FadeTo(1f);
            oshiroTarget = default(Vector2);
            oshiroRoutine = null;
            bird = null;
            yield return 1f;
            this.SetBgFade(0.6f);
            yield return this.NextLevel("credits-wallslide");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            this.badelineAutoFloat = false;
            this.badeline.Floatness = 0f;
            this.badeline.Sprite.Play("idle", false, false);
            this.badeline.Sprite.Scale.X = 1f;
            foreach (var entity in this.Scene.Tracker.GetEntities<CreditsTrigger>())
            {
                var creditsTrigger = entity as CreditsTrigger;
                if (creditsTrigger != null && creditsTrigger.Event == "BadelineOffset")
                    this.badeline.Position = creditsTrigger.Position + new Vector2(8f, 16f);
            }
            this.Add(new Coroutine(this.FadeTo(0f), true));
            this.Add(new Coroutine(this.WaitForPlayer(), true));
            while (this.player.Position.X > this.badeline.Position.X - 16f)
                yield return null;
            this.badeline.Sprite.Scale.X = -1f;
            yield return 0.1f;
            this.badelineAutoWalk = true;
            this.badelineWalkApproachFrom = this.badeline.Position;
            this.badelineWalkApproach = 0f;
            this.badeline.Sprite.Play("walk", false, false);
            while (this.badelineWalkApproach != 1f)
            {
                this.badelineWalkApproach = Calc.Approach(this.badelineWalkApproach, 1f, Engine.DeltaTime * 4f);
                yield return null;
            }
            while (this.player.Position.X > this.Level.Bounds.X + 160)
                yield return null;
            yield return this.FadeTo(1f);
            yield return 1f;
            this.SetBgFade(0.7f);
            yield return this.NextLevel("credits-payphone");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            this.player.Speed = Vector2.Zero;
            this.player.Facing = Facings.Left;
            this.autoWalk = false;
            this.badeline.Sprite.Play("idle", false, false);
            this.badeline.Floatness = 0f;
            this.badeline.Position = new Vector2(this.badeline.Position.X, this.player.Position.Y);
            this.badeline.Sprite.Scale.X = 1f;
            this.badelineAutoFloat = false;
            this.autoUpdateCamera = false;
            this.Level.Camera.X += 100f;
            Vector2 target3 = this.Level.Camera.Position + new Vector2(-200f, 0f);
            this.Add(new Coroutine(CutsceneEntity.CameraTo(target3, 14f, Ease.Linear, 0f), true));
            this.Add(new Coroutine(this.FadeTo(0f), true));
            yield return 1.5f;
            this.badeline.Sprite.Scale.X = -1f;
            yield return 0.5f;
            this.Add(new Coroutine(this.badeline.FloatTo(this.badeline.Position + new Vector2(16f, -12f), new int?(-1), false, false, false), true));
            yield return 0.5f;
            this.player.Facing = Facings.Right;
            yield return 1.5f;
            Vector2 oshiroTarget2 = this.badeline.Position;
            Vector2 to2 = this.player.Position;
            this.Add(new Coroutine(this.BadelineAround(oshiroTarget2, to2, this.badeline), true));
            yield return 0.5f;
            this.Add(new Coroutine(this.BadelineAround(oshiroTarget2, to2, null), true));
            yield return 0.5f;
            this.Add(new Coroutine(this.BadelineAround(oshiroTarget2, to2, null), true));
            yield return 3f;
            this.chara.Sprite.Play("laugh", false, false);
            yield return 0.5f;
            this.player.Facing = Facings.Left;
            yield return 0.5f;
            this.player.DummyAutoAnimate = false;
            this.player.Sprite.Play("sitDown", false, false);
            yield return 3f;
            yield return this.FadeTo(1f);
            oshiroTarget2 = default(Vector2);
            to2 = default(Vector2);
            yield return 1f;
            this.SetBgFade(0.8f);
            yield return this.NextLevel("credits-city");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            Entities.BirdNPC first = this.Scene.Entities.FindFirst<Entities.BirdNPC>();
            if (first != null)
                first.Facing = Facings.Right;
            this.badelineWalkApproach = 1f;
            this.badelineAutoFloat = false;
            this.badelineAutoWalk = true;
            this.badeline.Floatness = 0f;
            this.Add(new Coroutine(this.FadeTo(0f), true));
            yield return this.WaitForPlayer();
            yield return this.FadeTo(1f);
            yield return 1f;
            this.SetBgFade(0f);
            yield return this.NextLevel("credits-kirbynralsei");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            this.Add(new Coroutine(this.FadeTo(0f), true));
            yield return this.ExtraCreditsKirbyAndDarkeners();
            this.SetBgFade(0.85f);
            yield return this.NextLevel("credits-peakdescent");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            this.Add(new Coroutine(this.FadeTo(0f), true));
            yield return this.ExtraCreditsPeakDescent();
            yield return this.FadeTo(1f);
            this.SetBgFade(0.9f);
            yield return this.NextLevel("credits-prologue");
            yield return this.SetupLevel();
            yield return this.SpawnChara();
            this.badelineWalkApproach = 1f;
            this.badelineAutoFloat = false;
            this.badelineAutoWalk = true;
            this.badeline.Floatness = 0f;
            this.Add(new Coroutine(this.FadeTo(0f), true));
            yield return this.WaitForPlayer();
            yield return this.FadeTo(1f);
            yield return 1f;
            yield return this.FadeTo(1f);
            yield return 1f;
            while (this.credits.BottomTimer < 2f)
                yield return null;
            if (!this.gotoEpilogue)
            {
                this.snow = new DZHiresSnow();
                this.snow.Alpha = 0f;
                this.Level.Add(this.Level.HiresSnow);
            }
            else
            {
                new FadeWipe(this.Level, false, () => { });
            }
        }

        private IEnumerator SetupLevel()
        {
            this.Level.SnapColorGrade("credits");
            this.player = null;
            while ((this.player = this.Scene.Tracker.GetEntity<global::Celeste.Player>()) == null)
                yield return null;
            this.Level.Add(this.badeline = new BadelineDummy(this.player.Position + new Vector2(16f, -16f)));
            this.badeline.Floatness = 4f;
            this.badelineAutoFloat = true;
            this.badelineAutoWalk = false;
            this.badelineWalkApproach = 0f;
            this.Level.Session.Inventory.Dashes = 5;
            this.player.Dashes = 5;
            this.player.StateMachine.State = Player.StDummy;
            this.player.DummyFriction = false;
            this.player.DummyMaxspeed = false;
            this.player.Facing = Facings.Left;
            this.autoWalk = true;
            this.autoUpdateCamera = true;
            this.Level.CameraOffset.X = 70f;
            this.Level.CameraOffset.Y = -24f;
            this.Level.Camera.Position = this.player.CameraTarget;

            // Spawn Kirby follower
            if (this.kirbyFollower != null && this.kirbyFollower.Scene != null)
                this.kirbyFollower.RemoveSelf();
            var kirbySprite = GFX.SpriteBank.Create("kirby");
            kirbySprite.Play("idle", false, false);
            this.kirbyFollower = new Entity(this.GetCreditsTriggerPosition("KirbyOffset", this.player.Position + new Vector2(28f, -6f)));
            this.kirbyFollower.Depth = -50;
            this.kirbyFollower.Add(kirbySprite);
            this.Level.Add(this.kirbyFollower);
            this.kirby_auto_walk = true;

            // Spawn Ralsei follower
            if (this.ralseiFollower != null && this.ralseiFollower.Scene != null)
                this.ralseiFollower.RemoveSelf();
            var ralseiSprite = GFX.SpriteBank.Create("ralsei");
            ralseiSprite.Play("idle", false, false);
            this.ralseiFollower = new Entity(this.GetCreditsTriggerPosition("RalseiOffset", this.player.Position + new Vector2(44f, 0f)));
            this.ralseiFollower.Depth = -50;
            this.ralseiFollower.Add(ralseiSprite);
            this.Level.Add(this.ralseiFollower);
            this.ralsei_auto_float = true;
            this.ralsei_auto_walk = true;
        }

        private IEnumerator SpawnChara()
        {
            // Remove previous chara if present
            if (this.chara != null && this.chara.Scene != null)
                this.chara.RemoveSelf();

            // Spawn Chara as a passive follower for the credits sequence.
            var newChara = new CharaDummy(this.GetCreditsTriggerPosition("CharaOffset", this.player.Position + new Vector2(32f, 0f)));
            newChara.Collidable = false;
            newChara.Floatness = 0f;
            if (newChara.Sprite != null)
            {
                if (newChara.Sprite.Has("idle"))
                    newChara.Sprite.Play("idle", false, false);
                newChara.Sprite.Scale.X = -1f;
            }
            this.Scene.Add(newChara);
            this.chara = newChara;
            this.chara_auto_float = true;
            this.chara_auto_walk = true;

            yield break;
        }

        private CreditsTrigger FindCreditsTrigger(string eventName)
        {
            if (this.Scene == null)
                return null;

            foreach (var entity in this.Scene.Tracker.GetEntities<CreditsTrigger>())
            {
                CreditsTrigger creditsTrigger = entity as CreditsTrigger;
                if (creditsTrigger != null && creditsTrigger.Event == eventName)
                    return creditsTrigger;
            }

            return null;
        }

        private Vector2 GetCreditsTriggerPosition(string eventName, Vector2 fallback)
        {
            CreditsTrigger creditsTrigger = this.FindCreditsTrigger(eventName);
            return creditsTrigger != null ? creditsTrigger.Position : fallback;
        }

        private IEnumerator ExtraCreditsKirbyAndDarkeners()
        {
            bool prevAutoWalk = this.autoWalk;
            bool prevAutoUpdateCamera = this.autoUpdateCamera;
            bool prevBadelineVisible = this.badeline?.Visible ?? false;
            bool prevBadelineAutoFloat = this.badelineAutoFloat;
            bool prevBadelineAutoWalk = this.badelineAutoWalk;
            bool prevCharaVisible = this.chara?.Visible ?? false;
            bool previous_chara_auto_float = this.chara_auto_float;
            bool previous_chara_auto_walk = this.chara_auto_walk;
            bool prevPlayerVisible = this.player?.Visible ?? true;
            bool prevPlayerAutoAnimate = this.player?.DummyAutoAnimate ?? true;

            this.autoWalk = false;
            this.autoUpdateCamera = false;
            if (this.player != null)
            {
                this.player.Speed = Vector2.Zero;
                this.player.DummyAutoAnimate = false;
                this.player.Sprite.Play("idle", false, false);
                this.player.Visible = false;
            }
            if (this.badeline != null)
            {
                this.badeline.Visible = false;
                this.badelineAutoFloat = false;
                this.badelineAutoWalk = false;
            }
            if (this.chara != null)
            {
                this.chara.Visible = false;
                this.chara_auto_float = false;
                this.chara_auto_walk = false;
            }
            bool prevKirbyVisible1 = this.kirbyFollower?.Visible ?? false;
            bool previous_kirby_auto_walk_1 = this.kirby_auto_walk;
            if (this.kirbyFollower != null) this.kirbyFollower.Visible = false;
            this.kirby_auto_walk = false;
            bool prevRalseiVisible1 = this.ralseiFollower?.Visible ?? false;
            bool previous_ralsei_auto_float_1 = this.ralsei_auto_float;
            bool previous_ralsei_auto_walk_1 = this.ralsei_auto_walk;
            if (this.ralseiFollower != null) this.ralseiFollower.Visible = false;
            this.ralsei_auto_float = false;
            this.ralsei_auto_walk = false;

            Vector2 cameraOrigin = this.Level.Camera.Position;
            Vector2 basePos = cameraOrigin + new Vector2(200f, 160f);

            // Kirby leads while Chara and Ralsei hover alongside him.
            Entity chara = SpawnCreditsChara(basePos + new Vector2(-8f, 0f));
            Entity kirby = SpawnCreditsKirby(basePos + new Vector2(24f, 0f));
            Entity ralsei = SpawnCreditsRalsei(basePos + new Vector2(52f, 0f));

            // Placeholder square drawboards for bandanaDee, metaKnight, kingDedede
            Entity bandanaDee = new Entity(basePos + new Vector2(-32f, 0f));
            bandanaDee.Depth = -50;
            bandanaDee.Add(new SquareDrawboard());
            this.Scene.Add(bandanaDee);

            Entity metaKnight = new Entity(basePos + new Vector2(80f, 0f));
            metaKnight.Depth = -50;
            metaKnight.Add(new SquareDrawboard());
            this.Scene.Add(metaKnight);

            Entity kingDedede = new Entity(basePos + new Vector2(-56f, -8f));
            kingDedede.Depth = -50;
            kingDedede.Add(new SquareDrawboard());
            this.Scene.Add(kingDedede);

            // Waddle Dee and Magolor use atlas sprites
            Entity waddleDee = new Entity(basePos + new Vector2(104f, 0f));
            waddleDee.Depth = -50;
            Monocle.Image waddleDeeImg = new Monocle.Image(GFX.Game["characters/Enemies/ememy/waddledee/idle00"]);
            waddleDeeImg.CenterOrigin();
            waddleDee.Add(waddleDeeImg);
            this.Scene.Add(waddleDee);

            Entity magolor = new Entity(basePos + new Vector2(-80f, 0f));
            magolor.Depth = -50;
            Monocle.Image magolorImg = new Monocle.Image(GFX.Game["characters/magolor/idle00"]);
            magolorImg.CenterOrigin();
            magolor.Add(magolorImg);
            this.Scene.Add(magolor);

            Sprite charaSprite = chara.Get<Sprite>();
            if (charaSprite != null && charaSprite.Has("walk"))
                charaSprite.Play("walk", false, false);

            Sprite kirbySprite = kirby.Get<Sprite>();
            if (kirbySprite != null && kirbySprite.Has("walk"))
                kirbySprite.Play("walk", false, false);

            Sprite ralseiSprite = ralsei.Get<Sprite>();
            if (ralseiSprite != null && ralseiSprite.Has("walk"))
                ralseiSprite.Play("walk", false, false);

            // Animate NPCs walking if they can
            foreach (Entity npc in new[] { waddleDee, magolor })
            {
                Sprite s = npc.Get<Sprite>();
                if (s != null && s.Has("walk"))
                    s.Play("walk", false, false);
            }

            for (float t = 0f; t < 1.6f; t += Engine.DeltaTime)
            {
                chara.X -= 10f * Engine.DeltaTime;
                kirby.X -= 12f * Engine.DeltaTime;
                ralsei.X -= 10f * Engine.DeltaTime;
                bandanaDee.X -= 10f * Engine.DeltaTime;
                metaKnight.X -= 14f * Engine.DeltaTime;
                kingDedede.X -= 8f * Engine.DeltaTime;
                waddleDee.X -= 11f * Engine.DeltaTime;
                magolor.X -= 13f * Engine.DeltaTime;
                yield return null;
            }

            if (charaSprite != null && charaSprite.Has("idle"))
                charaSprite.Play("idle", false, false);

            if (kirbySprite != null && kirbySprite.Has("idle"))
                kirbySprite.Play("idle", false, false);

            if (ralseiSprite != null && ralseiSprite.Has("idle"))
                ralseiSprite.Play("idle", false, false);

            // Set all NPCs to idle
            foreach (Entity npc in new[] { waddleDee, magolor })
            {
                Sprite s = npc.Get<Sprite>();
                if (s != null && s.Has("idle"))
                    s.Play("idle", false, false);
            }

            yield return 1.2f;

            chara.RemoveSelf();
            kirby.RemoveSelf();
            ralsei.RemoveSelf();
            bandanaDee.RemoveSelf();
            metaKnight.RemoveSelf();
            kingDedede.RemoveSelf();
            waddleDee.RemoveSelf();
            magolor.RemoveSelf();

            if (this.player != null)
            {
                this.player.DummyAutoAnimate = prevPlayerAutoAnimate;
                this.player.Visible = prevPlayerVisible;
            }
            if (this.badeline != null)
            {
                this.badeline.Visible = prevBadelineVisible;
                this.badelineAutoFloat = prevBadelineAutoFloat;
                this.badelineAutoWalk = prevBadelineAutoWalk;
            }
            if (this.chara != null)
                this.chara.Visible = prevCharaVisible;
            this.chara_auto_float = previous_chara_auto_float;
            this.chara_auto_walk = previous_chara_auto_walk;
            if (this.kirbyFollower != null) this.kirbyFollower.Visible = prevKirbyVisible1;
            this.kirby_auto_walk = previous_kirby_auto_walk_1;
            if (this.ralseiFollower != null) this.ralseiFollower.Visible = prevRalseiVisible1;
            this.ralsei_auto_float = previous_ralsei_auto_float_1;
            this.ralsei_auto_walk = previous_ralsei_auto_walk_1;
            this.autoWalk = prevAutoWalk;
            this.autoUpdateCamera = prevAutoUpdateCamera;
        }

        private IEnumerator ExtraCreditsPeakDescent()
        {
            bool prevAutoWalk = this.autoWalk;
            bool prevAutoUpdateCamera = this.autoUpdateCamera;
            bool prevBadelineVisible = this.badeline?.Visible ?? true;
            bool prevBadelineAutoFloat = this.badelineAutoFloat;
            bool prevBadelineAutoWalk = this.badelineAutoWalk;
            bool prevCharaVisible = this.chara?.Visible ?? true;
            bool previous_chara_auto_float = this.chara_auto_float;
            bool previous_chara_auto_walk = this.chara_auto_walk;
            bool prevPlayerVisible = this.player?.Visible ?? true;
            bool prevPlayerAutoAnimate = this.player?.DummyAutoAnimate ?? true;

            this.autoWalk = false;
            this.autoUpdateCamera = false;
            if (this.player != null)
            {
                this.player.Speed = Vector2.Zero;
                this.player.DummyAutoAnimate = false;
                this.player.Sprite.Play("idle", false, false);
                this.player.Visible = false;
            }
            if (this.badeline != null)
            {
                this.badeline.Visible = false;
                this.badelineAutoFloat = false;
                this.badelineAutoWalk = false;
            }
            if (this.chara != null)
            {
                this.chara.Visible = false;
                this.chara_auto_float = false;
                this.chara_auto_walk = false;
            }
            bool prevKirbyVisible2 = this.kirbyFollower?.Visible ?? false;
            bool previous_kirby_auto_walk_2 = this.kirby_auto_walk;
            if (this.kirbyFollower != null) this.kirbyFollower.Visible = false;
            this.kirby_auto_walk = false;
            bool prevRalseiVisible2 = this.ralseiFollower?.Visible ?? false;
            bool previous_ralsei_auto_float_2 = this.ralsei_auto_float;
            bool previous_ralsei_auto_walk_2 = this.ralsei_auto_walk;
            if (this.ralseiFollower != null) this.ralseiFollower.Visible = false;
            this.ralsei_auto_float = false;
            this.ralsei_auto_walk = false;

            Vector2 cameraOrigin = this.Level.Camera.Position;
            List<Entity> monsters = new List<Entity>();
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = cameraOrigin + new Vector2(220f + i * 16f, 160f);
                Entity monster = new Entity(pos);
                monster.Depth = -50;
                monster.Add(new SquareDrawboard());
                this.Scene.Add(monster);
                monsters.Add(monster);
            }

            // Placeholder square drawboard
            Vector2 drawboardPos = cameraOrigin + new Vector2(140f, 80f);
            Entity drawboard = new Entity(drawboardPos);
            drawboard.Depth = -60;
            drawboard.Add(new SquareDrawboard());
            this.Scene.Add(drawboard);

            Vector2 velocity = new Vector2(-28f, 22f);
            for (float t = 0f; t < 4.5f; t += Engine.DeltaTime)
            {
                for (int i = 0; i < monsters.Count; i++)
                {
                    Entity monster = monsters[i];
                    monster.Position += velocity * Engine.DeltaTime;
                    monster.Y += (float)Math.Sin((this.Scene.TimeActive + i) * 2f) * 0.15f;
                }
                yield return null;
            }

            foreach (Entity monster in monsters)
                monster.RemoveSelf();
            monsters.Clear();
            drawboard.RemoveSelf();

            if (this.player != null)
            {
                this.player.DummyAutoAnimate = prevPlayerAutoAnimate;
                this.player.Visible = prevPlayerVisible;
            }
            if (this.badeline != null)
            {
                this.badeline.Visible = prevBadelineVisible;
                this.badelineAutoFloat = prevBadelineAutoFloat;
                this.badelineAutoWalk = prevBadelineAutoWalk;
            }
            if (this.chara != null)
                this.chara.Visible = prevCharaVisible;
            this.chara_auto_float = previous_chara_auto_float;
            this.chara_auto_walk = previous_chara_auto_walk;
            if (this.kirbyFollower != null) this.kirbyFollower.Visible = prevKirbyVisible2;
            this.kirby_auto_walk = previous_kirby_auto_walk_2;
            if (this.ralseiFollower != null) this.ralseiFollower.Visible = prevRalseiVisible2;
            this.ralsei_auto_float = previous_ralsei_auto_float_2;
            this.ralsei_auto_walk = previous_ralsei_auto_walk_2;
            this.autoWalk = prevAutoWalk;
            this.autoUpdateCamera = prevAutoUpdateCamera;
        }

        private Entity SpawnCreditsKirby(Vector2 position)
        {
            Sprite sprite = GFX.SpriteBank.Create("kirby");
            if (sprite.Has("idle"))
                sprite.Play("idle", false, false);
            sprite.CenterOrigin();
            sprite.Scale.X = -1f;
            return SpawnCreditsSprite(position, sprite, bob: false, bobAmount: 0f);
        }

        private Entity SpawnCreditsChara(Vector2 position)
        {
            Sprite sprite = GFX.SpriteBank.Create("DZ_chara");
            if (sprite.Has("idle"))
                sprite.Play("idle", false, false);
            sprite.CenterOrigin();
            sprite.Scale.X = -1f;
            return SpawnCreditsSprite(position, sprite, bob: true, bobAmount: 1.5f);
        }

        private Entity SpawnCreditsRalsei(Vector2 position)
        {
            Sprite sprite = GFX.SpriteBank.Create("ralsei");
            if (sprite.Has("idle"))
                sprite.Play("idle", false, false);
            sprite.CenterOrigin();
            sprite.Scale.X = -1f;
            return SpawnCreditsSprite(position, sprite, bob: true, bobAmount: 1.5f);
        }

        private Entity SpawnCreditsNPC(Vector2 position, string spriteBankId, bool bob, float bobAmount)
        {
            Sprite sprite = GFX.SpriteBank.Create(spriteBankId);
            if (sprite.Has("idle"))
                sprite.Play("idle", false, false);
            sprite.CenterOrigin();
            return SpawnCreditsSprite(position, sprite, bob, bobAmount);
        }

        private Entity SpawnCreditsSprite(Vector2 position, Sprite sprite, bool bob, float bobAmount)
        {
            Entity entity = new Entity(position);
            entity.Depth = -50;
            entity.Add(sprite);
            if (bob)
            {
                SineWave wave = new SineWave(0.45f, 0f);
                wave.OnUpdate = f => { sprite.Position = new Vector2(0f, f * bobAmount); };
                entity.Add(wave);
            }
            this.Scene.Add(entity);
            return entity;
        }

        private IEnumerator WaitForPlayer()
        {
            while (this.player.Position.X > this.Level.Bounds.X + 160)
            {
                if (this.Event != null)
                    yield return this.DoEvent(this.Event);
                this.Event = null;
                yield return null;
            }
        }

        private IEnumerator NextLevel(string name)
        {
            if (this.player != null)
                this.player.RemoveSelf();
            this.player = null;
            this.Level.OnEndOfFrame += () =>
            {
                this.Level.UnloadLevel();
                this.Level.Session.Level = name;
                this.Level.Session.RespawnPoint = new Vector2?(this.Level.GetSpawnPoint(new Vector2(this.Level.Bounds.Left, this.Level.Bounds.Top)));
                this.Level.LoadLevel(global::Celeste.Player.IntroTypes.None, false);
                this.Level.Wipe.Cancel();
            };
            yield return null;
            yield return null;
        }

        private IEnumerator FadeTo(float value)
        {
            while ((this.fade = Calc.Approach(this.fade, value, Engine.DeltaTime * 0.5f)) != value)
                yield return null;
            this.fade = value;
        }

        private IEnumerator BadelineApproachWalking()
        {
            while (this.badelineWalkApproach < 1f)
            {
                this.badeline.Floatness = Calc.Approach(this.badeline.Floatness, 0f, Engine.DeltaTime * 8f);
                this.badelineWalkApproach = Calc.Approach(this.badelineWalkApproach, 1f, Engine.DeltaTime * 0.6f);
                yield return null;
            }
        }

        private IEnumerator DustyRoutine(Entity oshiro)
        {
            List<Entity> dusty = new List<Entity>();
            float timer = 0f;
            Vector2 offset = oshiro.Position + new Vector2(220f, -24f);
            Vector2 start = offset;
            for (int i = 0; i < 3; i++)
            {
                Entity entity = new Entity(offset + new Vector2(i * 24, 0f));
                entity.Depth = -50;
                entity.Add(new DustGraphic(true, true));
                Monocle.Image image = new Monocle.Image(GFX.Game[$"decals/3-resort/brokenbox_{(char)(97 + i)}"]);
                image.JustifyOrigin(0.5f, 1f);
                image.Position = new Vector2(0f, -4f);
                entity.Add(image);
                this.Scene.Add(entity);
                dusty.Add(entity);
            }
            yield return 3.8f;
            while (true)
            {
                for (int j = 0; j < dusty.Count; j++)
                {
                    Entity entity2 = dusty[j];
                    entity2.X = offset.X + j * 24;
                    entity2.Y = offset.Y + (float)Math.Sin(timer * 4f + j * 0.8f) * 4f;
                }
                if (offset.X < this.Level.Bounds.Left + 120)
                    offset.Y = Calc.Approach(offset.Y, start.Y + 16f, Engine.DeltaTime * 16f);
                offset.X -= 26f * Engine.DeltaTime;
                timer += Engine.DeltaTime;
                yield return null;
            }
        }

        private IEnumerator BadelineAround(Vector2 start, Vector2 around, BadelineDummy badeline = null)
        {
            bool removeAtEnd = badeline == null;
            if (badeline == null)
                this.Scene.Add(badeline = new BadelineDummy(start));
            badeline.Sprite.Play("fallSlow", false, false);
            float angle = Calc.Angle(around, start);
            float dist = (around - start).Length();
            float duration = 3f;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                badeline.Position = around + Calc.AngleToVector(angle - p * 2f * 6.2831855f, dist + Calc.YoYo(p) * 16f + (float)Math.Sin(p * 6.2831855f * 4f) * 5f);
                badeline.Sprite.Scale.X = Math.Sign(around.X - badeline.X);
                if (!removeAtEnd)
                    this.player.Facing = (global::Celeste.Facings)Math.Sign(badeline.X - this.player.Position.X);
                if (this.Scene.OnInterval(0.1f))
                    TrailManager.Add(badeline, Color.Magenta, 1f);
                yield return null;
            }
            if (removeAtEnd)
                badeline.Vanish();
            else
                chara.Sprite.Play("laugh", false, false);
        }

        private IEnumerator DoEvent(string e)
        {
            switch (e)
            {
                case "WaitJumpDash":
                    yield return this.EventWaitJumpDash();
                    break;
                case "WaitJumpDoubleDash":
                    yield return this.EventWaitJumpDoubleDash();
                    break;
                case "WaitJumpQuintripleDash":
                    yield return this.EventWaitJumpQuintripleDash();
                    break;
                case "ClimbDown":
                    yield return this.EventClimbDown();
                    break;
                case "Wait":
                    yield return this.EventWait();
                    break;
            }
        }

        private IEnumerator EventWaitJumpDash()
        {
            this.autoWalk = false;
            this.player.DummyFriction = true;
            this.player.Speed = Vector2.Zero;
            this.player.Dashes = 2;
            yield return 0.1f;
            this.PlayerJump(-1);
            yield return 0.2f;
            this.player.Dashes = 2;
            this.player.OverrideDashDirection = new Vector2(-1f, -1f);
            this.player.StateMachine.State = this.player.StartDash();
            yield return 0.6f;
            this.player.OverrideDashDirection = null;
            this.player.StateMachine.State = Player.StDummy;
            this.autoWalk = true;
        }

        private IEnumerator EventWaitJumpDoubleDash()
        {
            this.autoWalk = false;
            this.player.DummyFriction = true;
            this.player.Speed = Vector2.Zero;
            yield return 0.1f;
            this.player.Facing = Facings.Right;
            yield return 0.25f;

            // Chara approaches from behind
            if (this.chara != null)
            {
                Vector2 charaFrom = this.chara.Position;
                Vector2 charaTo = this.player.Position + new Vector2(24f, 0f);
                for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.4f)
                {
                    this.chara.Position = Vector2.Lerp(charaFrom, charaTo, Ease.CubeIn(t));
                    yield return null;
                }
                this.chara.Position = charaTo;
            }
            yield return 0.15f;

            // Badeline combines with player
            yield return this.BadelineCombine();

            // Chara merges into player too — both give dashes
            if (this.chara != null)
            {
                Vector2 charaFrom2 = this.chara.Position;
                for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.25f)
                {
                    this.chara.Position = Vector2.Lerp(charaFrom2, this.player.Position, Ease.CubeIn(t));
                    yield return null;
                }
                this.chara.Visible = false;
                this.chara_auto_float = false;
                this.chara_auto_walk = false;
                this.Level.Displacement.AddBurst(this.player.Position, 0.3f, 8f, 24f, 0.4f);
            }
            // Kirby merges into player too
            if (this.kirbyFollower != null)
            {
                Vector2 kirbyFrom = this.kirbyFollower.Position;
                for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.25f)
                {
                    this.kirbyFollower.Position = Vector2.Lerp(kirbyFrom, this.player.Position, Ease.CubeIn(t));
                    yield return null;
                }
                this.kirbyFollower.Visible = false;
                this.kirby_auto_walk = false;
            }
            // Ralsei merges into player too
            if (this.ralseiFollower != null)
            {
                Vector2 ralseiFrom = this.ralseiFollower.Position;
                for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.25f)
                {
                    this.ralseiFollower.Position = Vector2.Lerp(ralseiFrom, this.player.Position, Ease.CubeIn(t));
                    yield return null;
                }
                this.ralseiFollower.Visible = false;
                this.ralsei_auto_float = false;
                this.ralsei_auto_walk = false;
            }
            this.player.Dashes = 2;
            yield return 0.5f;
            this.player.Facing = Facings.Left;
            yield return 0.7f;
            this.PlayerJump(-1);
            yield return 0.4f;
            this.player.OverrideDashDirection = new Vector2(-1f, -1f);
            this.player.StateMachine.State = this.player.StartDash();
            yield return 0.6f;
            this.player.OverrideDashDirection = new Vector2(-1f, 0f);
            this.player.StateMachine.State = this.player.StartDash();
            yield return 0.6f;
            this.player.OverrideDashDirection = null;
            this.player.StateMachine.State = Player.StDummy;
            this.autoWalk = true;
            while (!this.player.OnGround())
                yield return null;
            this.autoWalk = false;
            this.player.DummyFriction = true;
            this.player.Dashes = 2;
            yield return 0.5f;
            this.player.Facing = Facings.Right;
            yield return 1f;
            // Badeline and Chara reappear from the player
            this.Level.Displacement.AddBurst(this.player.Position, 0.4f, 8f, 32f, 0.5f);
            this.badeline.Position = this.player.Position;
            this.badeline.Visible = true;
            this.badelineAutoFloat = true;
            if (this.chara != null)
            {
                this.chara.Position = this.player.Position + new Vector2(24f, 0f);
                this.chara.Visible = true;
                this.chara_auto_float = true;
                this.chara_auto_walk = true;
            }
            if (this.kirbyFollower != null)
            {
                this.kirbyFollower.Position = this.player.Position + new Vector2(28f, -6f);
                this.kirbyFollower.Visible = true;
                this.kirby_auto_walk = true;
            }
            if (this.ralseiFollower != null)
            {
                this.ralseiFollower.Position = this.player.Position + new Vector2(44f, 0f);
                this.ralseiFollower.Visible = true;
                this.ralsei_auto_float = true;
                this.ralsei_auto_walk = true;
            }
            this.player.Dashes = 1;
            yield return 0.8f;
            this.player.Facing = Facings.Left;
            this.autoWalk = true;
            this.player.DummyFriction = false;
        }

        private IEnumerator EventWaitJumpQuintripleDash()
        {
            this.autoWalk = false;
            this.player.DummyFriction = true;
            this.player.Speed = Vector2.Zero;
            this.player.Dashes = 5;
            yield return 0.1f;
            this.player.Facing = Facings.Right;
            yield return 0.25f;

            if (this.chara != null)
            {
                Vector2 charaFrom = this.chara.Position;
                Vector2 charaTo = this.player.Position + new Vector2(24f, 0f);
                for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.4f)
                {
                    this.chara.Position = Vector2.Lerp(charaFrom, charaTo, Ease.CubeIn(t));
                    yield return null;
                }
                this.chara.Position = charaTo;
            }
            yield return 0.15f;

            yield return this.BadelineCombine();

            if (this.chara != null)
            {
                Vector2 charaFrom2 = this.chara.Position;
                for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.25f)
                {
                    this.chara.Position = Vector2.Lerp(charaFrom2, this.player.Position, Ease.CubeIn(t));
                    yield return null;
                }
                this.chara.Visible = false;
                this.chara_auto_float = false;
                this.chara_auto_walk = false;
                this.Level.Displacement.AddBurst(this.player.Position, 0.3f, 8f, 24f, 0.4f);
            }
            if (this.kirbyFollower != null)
            {
                Vector2 kirbyFrom = this.kirbyFollower.Position;
                for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.25f)
                {
                    this.kirbyFollower.Position = Vector2.Lerp(kirbyFrom, this.player.Position, Ease.CubeIn(t));
                    yield return null;
                }
                this.kirbyFollower.Visible = false;
                this.kirby_auto_walk = false;
            }
            if (this.ralseiFollower != null)
            {
                Vector2 ralseiFrom = this.ralseiFollower.Position;
                for (float t = 0f; t < 1f; t += Engine.DeltaTime / 0.25f)
                {
                    this.ralseiFollower.Position = Vector2.Lerp(ralseiFrom, this.player.Position, Ease.CubeIn(t));
                    yield return null;
                }
                this.ralseiFollower.Visible = false;
                this.ralsei_auto_float = false;
                this.ralsei_auto_walk = false;
            }

            this.player.Dashes = 5;
            yield return 0.5f;
            this.player.Facing = Facings.Left;
            yield return 0.7f;
            this.PlayerJump(-1);

            Vector2[] dashDirections =
            {
                new Vector2(-1f, -1f),
                new Vector2(-1f, -1f),
                new Vector2(-1f, -1f),
                new Vector2(-1f, -1f),
                new Vector2(-1f, 0f)
            };

            for (int index = 0; index < dashDirections.Length; index++)
            {
                yield return index == 0 ? 0.35f : 0.25f;
                this.player.Dashes = Math.Max(this.player.Dashes, 1);
                this.player.OverrideDashDirection = dashDirections[index];
                this.player.StateMachine.State = this.player.StartDash();
                yield return 0.4f;
            }

            this.player.OverrideDashDirection = null;
            this.player.StateMachine.State = Player.StDummy;
            this.autoWalk = true;
            while (!this.player.OnGround())
                yield return null;
            this.autoWalk = false;
            this.player.DummyFriction = true;
            this.player.Dashes = 5;
            yield return 0.5f;
            this.player.Facing = Facings.Right;
            yield return 1f;
            this.Level.Displacement.AddBurst(this.player.Position, 0.4f, 8f, 32f, 0.5f);
            this.badeline.Position = this.player.Position;
            this.badeline.Visible = true;
            this.badelineAutoFloat = true;
            if (this.chara != null)
            {
                this.chara.Position = this.player.Position + new Vector2(24f, 0f);
                this.chara.Visible = true;
                this.chara_auto_float = true;
                this.chara_auto_walk = true;
            }
            if (this.kirbyFollower != null)
            {
                this.kirbyFollower.Position = this.player.Position + new Vector2(28f, -6f);
                this.kirbyFollower.Visible = true;
                this.kirby_auto_walk = true;
            }
            if (this.ralseiFollower != null)
            {
                this.ralseiFollower.Position = this.player.Position + new Vector2(44f, 0f);
                this.ralseiFollower.Visible = true;
                this.ralsei_auto_float = true;
                this.ralsei_auto_walk = true;
            }
            this.player.Dashes = 1;
            yield return 0.8f;
            this.player.Facing = Facings.Left;
            this.autoWalk = true;
            this.player.DummyFriction = false;
        }

        private IEnumerator EventClimbDown()
        {
            this.autoWalk = false;
            this.player.DummyFriction = true;
            yield return 0.1f;
            this.PlayerJump(-1);
            yield return 0.4f;
            while (!this.player.CollideCheck<Solid>(this.player.Position + new Vector2(-1f, 0f)))
                yield return null;
            this.player.DummyAutoAnimate = false;
            this.player.Sprite.Play("wallslide", false, false);
            while (this.player.CollideCheck<Solid>(this.player.Position + new Vector2(-1f, 32f)))
            {
                this.player.CreateWallSlideParticles(-1);
                this.player.Speed = new Vector2(this.player.Speed.X, Math.Min(this.player.Speed.Y, 40f));
                yield return null;
            }
            this.PlayerJump(1);
            yield return 0.4f;
            while (!this.player.CollideCheck<Solid>(this.player.Position + new Vector2(1f, 0f)))
                yield return null;
            this.player.DummyAutoAnimate = false;
            this.player.Sprite.Play("wallslide", false, false);
            while (!this.player.CollideCheck<Solid>(this.player.Position + new Vector2(0f, 32f)))
            {
                this.player.CreateWallSlideParticles(1);
                this.player.Speed = new Vector2(this.player.Speed.X, Math.Min(this.player.Speed.Y, 40f));
                yield return null;
            }
            this.PlayerJump(-1);
            yield return 0.4f;
            this.autoWalk = true;
        }

        private IEnumerator EventWait()
        {
            this.badeline.Sprite.Play("idle", false, false);
            this.badelineAutoWalk = false;
            this.autoWalk = false;
            this.player.DummyFriction = true;
            yield return 0.1f;
            this.player.DummyAutoAnimate = false;
            this.player.Speed = Vector2.Zero;
            yield return 0.5f;
            this.player.Sprite.Play("lookUp", false, false);
            yield return 2f;
            Entities.BirdNPC first = this.Scene.Entities.FindFirst<Entities.BirdNPC>();
            if (first != null)
                first.AutoFly = true;
            yield return 0.1f;
            this.player.Sprite.Play("idle", false, false);
            yield return 1f;
            this.autoWalk = true;
            this.player.DummyFriction = false;
            this.player.DummyAutoAnimate = true;
            this.badelineAutoWalk = true;
            this.badelineWalkApproach = 0f;
            this.badelineWalkApproachFrom = this.badeline.Position;
            this.badeline.Sprite.Play("walk", false, false);
            while (this.badelineWalkApproach < 1f)
            {
                this.badelineWalkApproach += Engine.DeltaTime * 4f;
                yield return null;
            }
        }

        private IEnumerator BadelineCombine()
        {
            Vector2 from = this.badeline.Position;
            this.badelineAutoFloat = false;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.25f)
            {
                this.badeline.Position = Vector2.Lerp(from, this.player.Position, Ease.CubeIn(p));
                yield return null;
            }
            this.badeline.Visible = false;
            this.Level.Displacement.AddBurst(this.player.Position, 0.4f, 8f, 32f, 0.5f);
        }

        private void PlayerJump(int direction)
        {
            this.player.Facing = (global::Celeste.Facings)direction;
            this.player.DummyFriction = false;
            this.player.DummyAutoAnimate = true;
            this.player.Speed = new Vector2(direction * 120, this.player.Speed.Y);
            this.player.Jump();
            this.player.AutoJump = true;
            this.player.AutoJumpTimer = 2f;
        }

        private void SetBgFade(float alpha)
        {
            this.fillbg.Color = Color.Black * alpha;
        }

        public override void Update()
        {
            MInput.Disabled = false;
            if (this.Level.CanPause && (Input.Pause.Pressed || Input.ESC.Pressed))
            {
                Input.Pause.ConsumeBuffer();
                Input.ESC.ConsumeBuffer();
                this.Level.Pause(minimal: true);
            }
            MInput.Disabled = true;
            if (this.player != null && this.player.Scene != null)
            {
                if (this.player.OverrideDashDirection.HasValue)
                {
                    Input.MoveX.Value = (int)this.player.OverrideDashDirection.Value.X;
                    Input.MoveY.Value = (int)this.player.OverrideDashDirection.Value.Y;
                }
                if (this.autoWalk)
                {
                    if (this.player.OnGround())
                    {
                        this.player.Speed = new Vector2(-44.8f, this.player.Speed.Y);
                        bool flag1 = this.player.CollideCheck<Solid>(this.player.Position + new Vector2(-20f, 0f));
                        bool flag2 = !this.player.CollideCheck<Solid>(this.player.Position + new Vector2(-8f, 1f)) && !this.player.CollideCheck<Solid>(this.player.Position + new Vector2(-8f, 32f));
                        if (flag1 || flag2)
                        {
                            this.player.Jump();
                            this.player.AutoJump = true;
                            this.player.AutoJumpTimer = flag1 ? 0.6f : 2f;
                        }
                    }
                    else
                        this.player.Speed = new Vector2(-64f, this.player.Speed.Y);
                }
                if (this.badeline != null && this.badelineAutoFloat)
                {
                    Vector2 position = this.badeline.Position;
                    Vector2 value = this.player.Position + new Vector2(16f, -16f);
                    this.badeline.Position = position + (value - position) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                    this.badeline.Sprite.Scale.X = -1f;
                }
                if (this.badeline != null && this.badelineAutoWalk)
                {
                    global::Celeste.Player.ChaserState chaseState;
                    if (this.player.GetChasePosition(this.Scene.TimeActive, 0.35f + (float)Math.Sin(this.walkOffset) * 0.1f, out chaseState))
                    {
                        if (chaseState.OnGround)
                            this.walkOffset += Engine.DeltaTime;
                        if (this.badelineWalkApproach >= 1f)
                        {
                            this.badeline.Position = chaseState.Position;
                            if (this.badeline.Sprite.Has(chaseState.Animation))
                                this.badeline.Sprite.Play(chaseState.Animation, false, false);
                            this.badeline.Sprite.Scale.X = (float)chaseState.Facing;
                        }
                        else
                            this.badeline.Position = Vector2.Lerp(this.badelineWalkApproachFrom, chaseState.Position, this.badelineWalkApproach);
                    }
                }
                if (this.chara != null && this.chara.Visible)
                {
                    if (this.chara_auto_walk)
                    {
                        Vector2 charaTarget = this.player.Position + new Vector2(24f, 0f);
                        this.chara.Position += (charaTarget - this.chara.Position) * (1f - (float)Math.Pow(0.01, Engine.DeltaTime));
                    }
                    this.chara.Floatness = Calc.Approach(this.chara.Floatness, this.chara_auto_float ? 1.5f : 0f, Engine.DeltaTime * 8f);

                    if (this.chara.Sprite != null)
                    {
                        this.chara.Sprite.Scale.X = -1f;
                        bool moving = this.chara_auto_walk && Math.Abs(this.player.Speed.X) > 10f;
                        string wantAnim = moving ? "walk" : "idle";
                        if (this.chara.Sprite.CurrentAnimationID != wantAnim && this.chara.Sprite.Has(wantAnim))
                            this.chara.Sprite.Play(wantAnim, false, false);
                    }
                }
                if (Math.Abs(this.player.Speed.X) > 90f)
                    this.player.Speed = new Vector2(Calc.Approach(this.player.Speed.X, 90f * Math.Sign(this.player.Speed.X), 1000f * Engine.DeltaTime), this.player.Speed.Y);

                // Kirby follows behind the player without the floating bob.
                if (this.kirbyFollower != null && this.kirby_auto_walk)
                {
                    Vector2 kirbyTarget = this.player.Position + new Vector2(28f, -6f);
                    this.kirbyFollower.Position += (kirbyTarget - this.kirbyFollower.Position) * (1f - (float)Math.Pow(0.01, Engine.DeltaTime));
                    var kSprite = this.kirbyFollower.Get<Sprite>();
                    if (kSprite != null)
                    {
                        kSprite.Scale.X = -1f;
                        bool moving = Math.Abs(this.player.Speed.X) > 10f;
                        string wantAnim = moving ? "walk" : "idle";
                        if (kSprite.CurrentAnimationID != wantAnim && kSprite.Has(wantAnim))
                            kSprite.Play(wantAnim, false, false);
                    }
                }
                // Ralsei follows behind the player (further back)
                if (this.ralseiFollower != null && (this.ralsei_auto_float || this.ralsei_auto_walk))
                {
                    Vector2 ralseiTarget = this.ralseiFollower.Position;
                    if (this.ralsei_auto_walk)
                        ralseiTarget = this.player.Position + new Vector2(44f, 0f);
                    if (this.ralsei_auto_float)
                        ralseiTarget.Y += (float)Math.Sin(this.Scene.TimeActive * 2.0f) * 2f;
                    this.ralseiFollower.Position += (ralseiTarget - this.ralseiFollower.Position) * (1f - (float)Math.Pow(0.015, Engine.DeltaTime));
                    var rSprite = this.ralseiFollower.Get<Sprite>();
                    if (rSprite != null)
                    {
                        rSprite.Scale.X = -1f;
                        bool rMoving = this.ralsei_auto_walk && Math.Abs(this.player.Speed.X) > 10f;
                        string rWantAnim = rMoving ? "walk" : "idle";
                        if (rSprite.CurrentAnimationID != rWantAnim && rSprite.Has(rWantAnim))
                            rSprite.Play(rWantAnim, false, false);
                    }
                }
            }
            if (this.credits != null)
            {
                try
                {
                    this.credits.Update();
                }
                catch (System.NullReferenceException)
                {
                    // Credits renderer list has a null reference, skip update to prevent crash
                }
            }
            base.Update();
        }

        public new void PostUpdate()
        {
            if (this.player != null && this.player.Scene != null && this.autoUpdateCamera)
            {
                Vector2 position = this.Level.Camera.Position;
                Vector2 cameraTarget = this.player.CameraTarget;
                if (!this.player.OnGround())
                    cameraTarget.Y = (this.Level.Camera.Y * 2f + cameraTarget.Y) / 3f;
                this.Level.Camera.Position = position + (cameraTarget - position) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                this.Level.Camera.X = (int)cameraTarget.X;
            }
        }

        public override void Render()
        {
            bool flag = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
            if (!this.Level.Paused)
            {
                if (flag)
                    this.gradient.Draw(new Vector2(1720f, -10f), Vector2.Zero, Color.White * 0.6f, new Vector2(-1f, 1100f));
                else
                    this.gradient.Draw(new Vector2(200f, -10f), Vector2.Zero, Color.White * 0.6f, new Vector2(1f, 1100f));
            }
            if (this.fade > 0f)
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeInOut(this.fade));
            if (this.credits != null && !this.Level.Paused)
                this.credits.Render(new Vector2(flag ? 100f : 1820f, 0f), flag ? 0f : 1f);
            base.Render();
        }

        public override void OnEnd(Level level)
        {
            // Reset player input and movement to prevent stuck movement
            if (this.player != null)
            {
                Input.MoveX.Value = 0;
                Input.MoveY.Value = 0;
                this.player.Speed = Vector2.Zero;
                this.player.OverrideDashDirection = null;
            }
            
            SaveData.Instance.Assists.DashAssist = this.wasDashAssistOn;
            Audio.BusMuted("bus:/gameplay_sfx", false);
            CS17_Credits.Instance = null;
            MInput.Disabled = false;
            if (!this.gotoEpilogue)
            {
                if (DZModule.Session != null)
                {
                    DZModule.Session.InCredits = false;
                    DZModule.Session.CreditsPhase = 0;
                    DZModule.Session.CreditsCompleted = DZModule.IsChapter17EpilogueCompleted();
                }

                Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaComplete);
            }
            else
            {
                DZModule.LaunchChapter17Epilogue();
            }
        }

        private class Fill : Backdrop
        {
            public override void Render(Scene scene)
            {
                Draw.Rect(-10f, -10f, 340f, 200f, this.Color);
            }
        }

        private class SquareDrawboard : Component
        {
            public float Size = 64f;
            public Color Color = Color.White * 0.5f;

            public SquareDrawboard() : base(true, true) { }

            public override void Render()
            {
                Vector2 pos = this.Entity.Position;
                Draw.HollowRect(pos.X - Size / 2f, pos.Y - Size / 2f, Size, Size, Color);
            }
        }
    }
}
