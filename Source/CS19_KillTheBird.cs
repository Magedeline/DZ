using Celeste.Entities;
using CutsceneNode = Celeste.Entities.CutsceneNode;
using FMOD.Studio;
using Facings = Celeste.Facings;
using FlingBirdIntroMod = Celeste.Entities.FlingBirdIntro;
using FlingBirdIntro = Celeste.Entities.FlingBirdIntro;
using BirdNPC = Celeste.Entities.BirdNPC;

namespace Celeste.Cutscenes
{
    public class CS19_KillTheBird : CutsceneEntity
    {
        private global::Celeste.Player player;
        private FlingBirdIntroMod flingBird;
        private CharaDummy chara;
        private BirdNPC bird;
        private Vector2 birdWaitPosition;
        private EventInstance snapshot;
        private CharaDummy undyne;
        private CharaDummy toriel;
        private CharaDummy theo;
        private CharaDummy asgore;
        private CharaDummy starsi;
        private CharaDummy ralsei;
        private CharaDummy sans;
        private CharaDummy papyrus;
        private CharaDummy alphy;
        private CharaDummy noelle;
        private CharaDummy suzy;
        private CharaDummy berdly;

        public CS19_KillTheBird(global::Celeste.Player player, FlingBirdIntroMod flingBird) : base(true, false)
        {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.flingBird = flingBird ?? throw new ArgumentNullException(nameof(flingBird));
            this.birdWaitPosition = flingBird.BirdEndPosition;
        }

        public override void OnBegin(Level level)
        {
            base.Add(new Coroutine(this.Cutscene(level), true));
        }

        private IEnumerator Cutscene(Level level)
        {
            // Lock player movement at start of cutscene
            this.player.StateMachine.State = 11;
            this.player.DummyGravity = false;
            this.player.Speed = Vector2.Zero;

            Audio.SetMusic("event:/desolozantas/final_content/music/lvl19/cinematic/bird_crash_second", true, true);
            BadelineBoost boost = base.Scene.Entities.FindFirst<BadelineBoost>();
            if (boost != null)
            {
                boost.Active = (boost.Visible = (boost.Collidable = false));
            }
            // Hide CharaBoost until cutscene ends
            CustomCharaBoost charaBoost = base.Scene.Entities.FindFirst<CustomCharaBoost>();
            if (charaBoost != null)
            {
                charaBoost.Active = charaBoost.Visible = charaBoost.Collidable = false;
            }
            yield return this.flingBird.DoGrabbingRoutine(this.player);
            this.flingBird.Sprite.Play("hurt", false, false);
            this.flingBird.X += 8f;
            // Ground the player safely with a timeout to avoid potential infinite loops
            float maxGroundTime = 2f;
            // Add null/scene checks before OnGround to prevent NullReferenceException
            while (this.player != null && this.player.Scene != null && this.player.Collider != null && !this.player.OnGround(1) && maxGroundTime > 0f)
            {
                this.player.MoveVExact(1, null, null);
                maxGroundTime -= Engine.DeltaTime;
                yield return null;
            }
            // Attempt to unstuck the player from solids with a max iteration guard
            int maxUnstuck = 32;
            // Add null/scene checks before CollideCheck to prevent NullReferenceException
            while (this.player != null && this.player.Scene != null && this.player.Collider != null && this.player.CollideCheck<Solid>() && maxUnstuck-- > 0)
            {
                this.player.Y -= 1f;
                yield return null;
            }
            Engine.TimeRate = 0.65f;
            float ground = this.player.Position.Y;
            this.player.Dashes = 1;
            PlayPlayerSpriteSafe(this.player, "roll", "idle");
            this.player.Speed.X = 200f;
            this.player.DummyFriction = false;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime)
            {
                this.player.Speed.X = Calc.Approach(this.player.Speed.X, 0f, 160f * Engine.DeltaTime);
                if (this.player.Speed.X != 0f && base.Scene.OnInterval(0.1f))
                {
                    Dust.BurstFG(this.player.Position, -1.5707964f, 2, 4f, null);
                }
                this.flingBird.Position.X += Engine.DeltaTime * 80f * Ease.CubeOut(1f - p);
                this.flingBird.Position.Y = ground;
                yield return null;
            }
            this.player.Speed.X = 0f;
            this.player.DummyFriction = true;
            this.player.DummyGravity = true;
            yield return 0.25f;
            while (Engine.TimeRate < 1f)
            {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 4f * Engine.DeltaTime);
                yield return null;
            }
            this.player.ForceCameraUpdate = false;
            yield return 0.6f;
            PlayPlayerSpriteSafe(this.player, "rollGetUp", "idle");
            yield return 0.8f;
            level.Session.Audio.Music.Event = "event:/desolozantas/final_content/music/lvl19/tragiclost";
            level.Session.Audio.Apply(false);
            yield return Textbox.Say("CH19_BROKEN_STAR_WARRIOR", new Func<IEnumerator>[]
            {
                new Func<IEnumerator>(this.BirdLooksHurt),
                new Func<IEnumerator>(this.BirdSquakOnGround),
                new Func<IEnumerator>(this.ApproachBird),
                new Func<IEnumerator>(this.ApproachBirdAgain),
                new Func<IEnumerator>(this.CharaAppears),
                new Func<IEnumerator>(this.WaitABeat),
                new Func<IEnumerator>(this.KirbySits),
                new Func<IEnumerator>(this.CharaHugs),
                new Func<IEnumerator>(this.StandUp),
                new Func<IEnumerator>(this.ShiftCameraToBird)
            });
            yield return level.ZoomBack(0.5f);
            if (this.chara != null)
            {
                this.chara.Vanish();
                this.chara = null;
            }
            yield return 0.4f;
            yield return this.MonsterKindComfortsKirby();
            if (boost != null)
            {
                this.Level.Displacement.AddBurst(boost.Center, 0.5f, 8f, 32f, 0.5f, null, null);
                Audio.Play("event:/new_content/char/badeline/booster_first_appear", boost.Center);
                boost.Active = (boost.Visible = (boost.Collidable = true));
                yield return 0.2f;
            }
            // Show CharaBoost now that cutscene is done
            CustomCharaBoost charaBoostEnd = base.Scene.Entities.FindFirst<CustomCharaBoost>();
            if (charaBoostEnd != null)
            {
                this.Level.Displacement.AddBurst(charaBoostEnd.Center, 0.5f, 8f, 32f, 0.5f, null, null);
                Audio.Play("event:/new_content/char/badeline/booster_first_appear", charaBoostEnd.Center);
                charaBoostEnd.Active = charaBoostEnd.Visible = charaBoostEnd.Collidable = true;
                yield return 0.2f;
            }
            base.EndCutscene(level, true);
            yield break;
        }

        private IEnumerator BirdTwitches(string sfx = null)
        {
            this.flingBird.Sprite.Scale.Y = 1.6f;
            if (!string.IsNullOrWhiteSpace(sfx))
            {
                Audio.Play(sfx, this.flingBird.BirdEndPosition);
            }
            while (this.flingBird.Sprite.Scale.Y > 1f)
            {
                this.flingBird.Sprite.Scale.Y = Calc.Approach(this.flingBird.Sprite.Scale.Y, 1f, 2f * Engine.DeltaTime);
                yield return null;
            }
            yield break;
        }

        private static void PlayPlayerSpriteSafe(global::Celeste.Player player, string preferredAnim, string fallbackAnim)
        {
            if (player?.Sprite == null)
                return;

            if (player.Sprite.Has(preferredAnim))
                player.Sprite.Play(preferredAnim, false, false);
            else if (player.Sprite.Has(fallbackAnim))
                player.Sprite.Play(fallbackAnim, false, false);
        }

        private IEnumerator BirdLooksHurt()
        {
            yield return 0.8f;
            yield return this.BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_1");
            yield return 0.4f;
            yield return this.BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_2");
            yield return 0.5f;
            yield break;
        }

        private IEnumerator BirdSquakOnGround()
        {
            yield return 0.6f;
            yield return this.BirdTwitches("event:/new_content/game/10_farewell/bird_crashscene_twitch_3");
            yield return 0.8f;
            Audio.Play("event:/new_content/game/10_farewell/bird_crashscene_recover", this.flingBird.BirdEndPosition);
            this.RemoveFlingBirdIfExists();
            base.Scene.Add(this.bird = new BirdNPC(this.flingBird.BirdEndPosition, BirdNPC.Modes.None));
            this.bird.Facing = Facings.Right;
            this.bird.Sprite.Play("recover", false, false);
            yield return 0.6f;
            this.bird.Facing = Facings.Left;
            this.bird.Sprite.Play("idle", false, false);
            this.bird.X += 3f;
            yield return 0.4f;
            yield return this.bird.Caw();
            yield break;
        }

        private IEnumerator ApproachBird()
        {
            this.player.DummyAutoAnimate = true;
            yield return 0.25f;
            yield return this.bird.Caw();
            base.Add(new Coroutine(this.player.DummyWalkTo(this.player.X + 20f, false, 1f, false), true));
            yield return 0.1f;
            Audio.Play("event:/game/general/bird_startle", this.bird.Position);
            yield return this.bird.Startle("event:/new_content/game/10_farewell/bird_crashscene_relocate", 0.8f, null);
            yield return this.bird.FlyTo(new Vector2(this.player.X + 80f, this.player.Y), 3f, false);
            yield break;
        }

        private IEnumerator ApproachBirdAgain()
        {
            Audio.Play("event:/new_content/game/10_farewell/bird_crashscene_leave", this.bird.Position);
            base.Add(new Coroutine(this.bird.FlyTo(this.birdWaitPosition, 2f, false), true));
            yield return this.player.DummyWalkTo(this.player.X + 20f, false, 1f, false);
            this.snapshot = Audio.CreateSnapshot("snapshot:/game_10_bird_wings_silenced", true);
            yield return 0.8f;
            this.bird.RemoveSelf();
            base.Scene.Add(this.bird = new BirdNPC(this.birdWaitPosition, BirdNPC.Modes.WaitForLightningOff));
            this.bird.Facing = Facings.Right;
            this.bird.FlyAwayUp = false;
            this.bird.WaitForLightningPostDelay = 1f;
            yield break;
        }

        private IEnumerator CharaAppears()
        {
            yield return this.player.DummyWalkToExact((int)this.player.X + 20, false, 0.5f, false);
            this.Level.Add(this.chara = new CharaDummy(this.player.Position + new Vector2(24f, -8f)));
            this.Level.Displacement.AddBurst(this.chara.Center, 0.5f, 8f, 32f, 0.5f, null, null);
            Audio.Play("event:/char/badeline/maddy_split", this.player.Position);
            this.chara.Sprite.Scale.X = -1f;
            yield return 0.2f;
            yield break;
        }

        private IEnumerator WaitABeat()
        {
            yield return this.player.DummyWalkToExact((int)this.player.X - 4, true, 0.5f, false);
            yield return 0.8f;
            yield break;
        }

        private IEnumerator KirbySits()
        {
            yield return 0.5f;
            yield return this.player.DummyWalkToExact((int)this.player.X - 16, false, 0.25f, false);
            this.player.DummyAutoAnimate = false;
            this.player.Sprite.Play("sitDown", false, false);
            yield return 1.5f;
            yield break;
        }

        private IEnumerator CharaHugs()
        {
            yield return 1f;
            yield return this.chara.FloatTo(this.chara.Position + new Vector2(0f, 8f), null, true, false, true);
            this.chara.Floatness = 0f;
            this.chara.AutoAnimator.Enabled = false;
            this.chara.Sprite.Play("idle", false, false);
            Audio.Play("event:/char/badeline/landing", this.chara.Position);
            yield return 0.5f;
            yield return this.chara.WalkTo(this.player.X - 9f, 40f);
            this.chara.Sprite.Scale.X = 1f;
            yield return 0.2f;
            Audio.Play("event:/char/badeline/duck", this.chara.Position);
            this.chara.Depth = this.player.Depth + 5;
            this.chara.Sprite.Play("hug", false, false);
            yield return 1f;
            yield break;
        }

        private IEnumerator StandUp()
        {
            Audio.Play("event:/char/badeline/stand", this.chara.Position);
            yield return this.chara.WalkTo(this.chara.X - 8f, 64f);
            this.chara.Sprite.Scale.X = 1f;
            yield return 0.2f;
            this.player.DummyAutoAnimate = true;
            this.Level.NextColorGrade("none", 0.25f);
            yield return 0.25f;
            yield break;
        }

        private IEnumerator ShiftCameraToBird()
        {
            Audio.ReleaseSnapshot(this.snapshot);
            this.snapshot = null;
            Audio.Play("event:/new_content/char/badeline/birdcrash_scene_float", this.chara.Position);
            base.Add(new Coroutine(this.chara.FloatTo(this.player.Position + new Vector2(-16f, -16f), new int?(1), true, false, false), true));
            Level level = base.Scene as Level;
            this.player.Facing = Facings.Right;
            yield return level.ZoomAcross(level.ZoomFocusPoint + new Vector2(70f, 0f), 1.5f, 1f);
            yield return 0.4f;
            yield break;
        }

        private void InitializeMonsterKind()
        {
            if (this.undyne != null)
            {
                return;
            }

            float baseX = this.player.X;
            float baseY = this.player.Y;

            this.undyne = new CharaDummy(new Vector2(baseX - 160f, baseY));
            this.toriel = new CharaDummy(new Vector2(baseX - 150f, baseY));
            this.theo = new CharaDummy(new Vector2(baseX - 140f, baseY));
            this.asgore = new CharaDummy(new Vector2(baseX - 130f, baseY));
            this.starsi = new CharaDummy(new Vector2(baseX - 120f, baseY));
            this.ralsei = new CharaDummy(new Vector2(baseX - 110f, baseY));
            this.sans = new CharaDummy(new Vector2(baseX + 110f, baseY));
            this.papyrus = new CharaDummy(new Vector2(baseX + 120f, baseY));
            this.alphy = new CharaDummy(new Vector2(baseX + 140f, baseY));
            this.noelle = new CharaDummy(new Vector2(baseX + 150f, baseY));
            this.suzy = new CharaDummy(new Vector2(baseX + 160f, baseY));
            this.berdly = new CharaDummy(new Vector2(baseX + 170f, baseY));

            this.undyne.Visible = false;
            this.toriel.Visible = false;
            this.theo.Visible = false;
            this.asgore.Visible = false;
            this.starsi.Visible = false;
            this.ralsei.Visible = false;
            this.sans.Visible = false;
            this.papyrus.Visible = false;
            this.alphy.Visible = false;
            this.noelle.Visible = false;
            this.suzy.Visible = false;
            this.berdly.Visible = false;

            this.Level.Add(this.undyne);
            this.Level.Add(this.toriel);
            this.Level.Add(this.theo);
            this.Level.Add(this.asgore);
            this.Level.Add(this.starsi);
            this.Level.Add(this.ralsei);
            this.Level.Add(this.sans);
            this.Level.Add(this.papyrus);
            this.Level.Add(this.alphy);
            this.Level.Add(this.noelle);
            this.Level.Add(this.suzy);
            this.Level.Add(this.berdly);
        }

        private IEnumerator MonsterKindComfortsKirby()
        {
            this.InitializeMonsterKind();
            this.player.DummyAutoAnimate = false;
            PlayPlayerSpriteSafe(this.player, "duck", "idle");
            Audio.Play("event:/game/general/world_noise", this.player.Position);

            this.undyne.Visible = true;
            this.toriel.Visible = true;
            this.theo.Visible = true;
            this.asgore.Visible = true;
            this.starsi.Visible = true;
            this.ralsei.Visible = true;
            this.sans.Visible = true;
            this.papyrus.Visible = true;
            this.alphy.Visible = true;
            this.noelle.Visible = true;
            this.suzy.Visible = true;
            this.berdly.Visible = true;

            float baseX = this.player.X;
            Add(new Coroutine(this.WalkCharacterTo(this.undyne, baseX - 72f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.toriel, baseX - 60f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.theo, baseX - 48f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.asgore, baseX - 36f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.starsi, baseX - 24f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.ralsei, baseX - 12f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.sans, baseX + 12f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.papyrus, baseX + 24f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.alphy, baseX + 48f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.noelle, baseX + 60f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.suzy, baseX + 72f), true));
            Add(new Coroutine(this.WalkCharacterTo(this.berdly, baseX + 84f), true));

            yield return 1.8f;
            this.player.DummyAutoAnimate = true;
            PlayPlayerSpriteSafe(this.player, "idle", "idle");
            yield return 0.4f;
        }

        private IEnumerator WalkCharacterTo(CharaDummy character, float x)
        {
            if (character != null)
            {
                yield return character.WalkTo(x);
            }
        }

        public override void OnEnd(Level level)
        {
            Audio.ReleaseSnapshot(this.snapshot);
            this.snapshot = null;
            Engine.TimeRate = 1f;
            if (this.WasSkipped)
            {
                CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
                if (cutsceneNode != null)
                {
                    this.player.Sprite.Play("idle", false, false);
                    this.player.Position = ((Entity)cutsceneNode).Position.Floor();
                    level.Camera.Position = this.player.CameraTarget;
                }
                foreach (Lightning lightning in base.Scene.Entities.FindAll<Lightning>())
                {
                    lightning.ToggleCheck();
                }
                LightningRenderer entity = base.Scene.Tracker.GetEntity<LightningRenderer>();
                if (entity != null)
                {
                    entity.ToggleEdges(true);
                }
                level.Session.Audio.Music.Event = "event:/desolozantas/final_content/music/lvl19/tragiclost";
                level.Session.Audio.Apply(false);
            }
            this.player.Speed = Vector2.Zero;
            this.player.DummyGravity = true;
            this.player.DummyFriction = true;
            this.player.DummyAutoAnimate = true;
            this.player.ForceCameraUpdate = false;
            this.player.StateMachine.State = 0;
            BadelineBoost badelineBoost = base.Scene.Entities.FindFirst<BadelineBoost>();
            if (badelineBoost != null)
            {
                badelineBoost.Active = (badelineBoost.Visible = (badelineBoost.Collidable = true));
            }
            // Show CharaBoost when cutscene ends (including skip)
            CustomCharaBoost charaBoostRestore = base.Scene.Entities.FindFirst<CustomCharaBoost>();
            if (charaBoostRestore != null)
            {
                charaBoostRestore.Active = charaBoostRestore.Visible = charaBoostRestore.Collidable = true;
            }
            this.RemoveIfExists(this.chara);
            this.RemoveIfExists(this.undyne);
            this.RemoveIfExists(this.toriel);
            this.RemoveIfExists(this.theo);
            this.RemoveIfExists(this.asgore);
            this.RemoveIfExists(this.starsi);
            this.RemoveIfExists(this.ralsei);
            this.RemoveIfExists(this.sans);
            this.RemoveIfExists(this.papyrus);
            this.RemoveIfExists(this.alphy);
            this.RemoveIfExists(this.noelle);
            this.RemoveIfExists(this.suzy);
            this.RemoveIfExists(this.berdly);
            this.RemoveFlingBirdIfExists();
            if (this.bird != null)
            {
                this.bird.RemoveSelf();
            }
            if (this.WasSkipped)
            {
                level.SnapColorGrade("none");
            }
            level.ResetZoom();
        }

        private void RemoveFlingBirdIfExists()
        {
            if (this.flingBird == null)
            {
                return;
            }

            if (this.flingBird.CrashSfxEmitter != null)
            {
                this.flingBird.CrashSfxEmitter.RemoveSelf();
            }

            if (this.flingBird.Scene != null)
            {
                this.flingBird.RemoveSelf();
            }
        }

        private void RemoveIfExists(Entity entity)
        {
            if (entity != null && entity.Scene != null)
            {
                entity.RemoveSelf();
            }
        }

        public override void Removed(Scene scene)
        {
            Audio.ReleaseSnapshot(this.snapshot);
            this.snapshot = null;
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Audio.ReleaseSnapshot(this.snapshot);
            this.snapshot = null;
            base.SceneEnd(scene);
        }

        public static void HandlePostCutsceneSpawn(FlingBirdIntro bird, Level level)
        {
            if (bird == null || level == null)
                return;

            try
            {
                // Clean up the bird
                if (bird.CrashSfxEmitter != null)
                    bird.CrashSfxEmitter.RemoveSelf();
                bird.RemoveSelf();
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
    }
}





