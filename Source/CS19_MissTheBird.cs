using FMOD.Studio;
using Facings = Celeste.Facings;
using FlingBirdIntroMod = MaggyHelper.Entities.FlingBirdIntro;
using CustomCharaBoost = MaggyHelper.Entities.CustomCharaBoost;

namespace MaggyHelper.Cutscenes
{
    public class CS19_MissTheBird : CutsceneEntity
    {
        public const string Flag = "MissTheBird";
        private global::Celeste.Player player;
        private FlingBirdIntroMod flingBirdmod;
        private BirdNPC bird;
        private Coroutine zoomRoutine;
        private EventInstance crashMusicSfx;
        private readonly TimeRateModifier timeRateModifier;

        public CS19_MissTheBird(global::Celeste.Player player, FlingBirdIntroMod flingBirdmod) : base(true, false)
        {
            this.player = player;
            this.flingBirdmod = flingBirdmod;
            Add(timeRateModifier = new TimeRateModifier(1f, false));
            Add(new LevelEndingHook(delegate {
                Audio.Stop(this.crashMusicSfx, true);
            }));
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level), true));
            StartMusic();
            TriggerEnvironmentalEvents();
        }

        private IEnumerator Cutscene(Level level)
        {
            // Lock player movement at start of cutscene
            this.player.StateMachine.State = 11;
            this.player.DummyGravity = false;
            this.player.Speed = Vector2.Zero;

            // Hide CharaBoost until cutscene ends
            CustomCharaBoost charaBoost = base.Scene.Entities.FindFirst<CustomCharaBoost>();
            if (charaBoost != null)
            {
                charaBoost.Active = charaBoost.Visible = charaBoost.Collidable = false;
            }

            Audio.SetMusicParam("bird_grab", 1f);
            this.crashMusicSfx = Audio.Play("event:/desolozantas/final_content/music/lvl19/cinematic/bird_crash_first");
            // Hold the player while the bird flies along its path
            yield return this.flingBirdmod.DoGrabbingRoutine(this.player);
            this.bird = new BirdNPC(this.flingBirdmod.BirdEndPosition, BirdNPC.Modes.None);
            level.Add(this.bird);
            this.flingBirdmod.RemoveSelf();
            yield return null;
            level.ResetZoom();
            level.Shake(0.5f);
            this.player.Position = this.player.Position.Floor();
            this.player.DummyGravity = true;
            this.player.DummyAutoAnimate = false;
            this.player.DummyFriction = false;
            this.player.ForceCameraUpdate = true;
            this.player.Speed = new Vector2(200f, 200f);
            this.bird.Position += Vector2.UnitX * 16f;
            this.bird.Add(new Coroutine(this.bird.Startle(null, 0.5f, new Vector2(3f, 0.25f)), true));
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate {
                Add(new Coroutine(this.bird.FlyAway(0.2f), true));
                this.bird.Position += new Vector2(0f, -4f);
            }, 0.8f, true));
            // Ground the player safely with a timeout to avoid potential infinite loops
            float maxGroundTime = 2f;
            // Add null/scene checks before OnGround to prevent NullReferenceException
            while (this.player != null && this.player.Scene != null && this.player.Collider != null && !this.player.OnGround() && maxGroundTime > 0f)
            {
                this.player.MoveVExact(1);
                maxGroundTime -= Engine.DeltaTime;
                yield return null;
            }

            // Avoid global time scaling to prevent perceived freezes
            PlayPlayerSpriteSafe(this.player, "roll", "idle");
            while (this.player.Speed.X != 0f)
            {
                this.player.Speed.X = Calc.Approach(this.player.Speed.X, 0f, 120f * Engine.DeltaTime);
                if (Scene.OnInterval(0.1f))
                    Dust.BurstFG(this.player.Position, -1.5707964f, 2);
                yield return null;
            }
            this.player.Speed.X = 0f;
            this.player.DummyFriction = true;
            yield return 0.25f;
            Add(this.zoomRoutine = new Coroutine(level.ZoomTo(new Vector2(160f, 110f), 1.5f, 6f), true));
            yield return 1.5f;
            PlayPlayerSpriteSafe(this.player, "rollGetUp", "idle");
            yield return 0.5f;
            this.player.ForceCameraUpdate = false;
            yield return Textbox.Say("CH19_MISS_THE_BIRD", new Func<IEnumerator>[] {
                StandUpFaceLeft,
                TakeStepLeft,
                TakeStepRight,
                FlickerBlackhole,
                FlickerJumpscareBlackhole,
                OpenBlackhole
            });
            // Show CharaBoost now that cutscene is done
            CustomCharaBoost charaBoostEnd = base.Scene.Entities.FindFirst<CustomCharaBoost>();
            if (charaBoostEnd != null)
            {
                this.Level.Displacement.AddBurst(charaBoostEnd.Center, 0.5f, 8f, 32f, 0.5f, null, null);
                Audio.Play("event:/new_content/char/badeline/booster_first_appear", charaBoostEnd.Center);
                charaBoostEnd.Active = charaBoostEnd.Visible = charaBoostEnd.Collidable = true;
            }
            StartMusic();
            EndCutscene(level);
        }

        private IEnumerator StandUpFaceLeft()
        {
            while (!this.zoomRoutine.Finished)
                yield return null;
            yield return 0.2f;
            Audio.Play("event:/char/madeline/stand", this.player.Position);
            this.player.DummyAutoAnimate = true;
            this.player.Sprite.Play("idle");
            yield return 0.2f;
            this.player.Facing = Facings.Left;
            yield return 0.5f;
        }

        private static void PlayPlayerSpriteSafe(global::Celeste.Player player, string preferredAnim, string fallbackAnim)
        {
            if (player?.Sprite == null)
                return;

            if (player.Sprite.Has(preferredAnim))
                player.Sprite.Play(preferredAnim);
            else if (player.Sprite.Has(fallbackAnim))
                player.Sprite.Play(fallbackAnim);
        }

        private IEnumerator TakeStepLeft()
        {
            yield return this.player.DummyWalkTo(this.player.X - 16f);
        }

        private IEnumerator TakeStepRight()
        {
            yield return this.player.DummyWalkTo(this.player.X + 32f);
        }

        private IEnumerator FlickerBlackhole()
        {
            yield return 0.5f;
            Audio.Play("event:/desolozantas/final_content/music/lvl19/cinematic/els_intro_laugh");
            yield return MoonGlitchBackgroundTrigger.GlitchRoutine(0.5f, false);
            yield return this.player.DummyWalkTo(this.player.X - 8f, true);
            yield return 0.4f;
        }

        private IEnumerator FlickerJumpscareBlackhole()
        {
            yield return 0.5f;
            Audio.Play("event:/desolozantas/final_content/music/lvl19/cinematic/els_intro_scream");
            yield return MoonGlitchBackgroundTrigger.GlitchRoutine(0.5f, false);
            yield return this.player.DummyWalkTo(this.player.X - 8f, true);
            yield return 0.4f;
        }

        private IEnumerator OpenBlackhole()
        {
            yield return 0.2f;
            this.Level.ResetZoom();
            this.Level.Flash(Color.White);
            this.Level.Shake(0.4f);
            this.Level.Add(new LightningStrike(new Vector2(this.player.X, this.Level.Bounds.Top), 80, 240f));
            this.Level.Add(new LightningStrike(new Vector2(this.player.X - 100f, this.Level.Bounds.Top), 90, 240f, 0.5f));
            Audio.Play("event:/new_content/game/10_farewell/lightning_strike");
            TriggerEnvironmentalEvents();
            StartMusic();
            yield return 1.2f;
        }

        private void StartMusic()
        {
            this.Level.Session.Audio.Music.Event = "event:/desolozantas/final_content/music/lvl19/part03";
            this.Level.Session.Audio.Ambience.Event = "event:/desolozantas/final_content/env/19_vortex";
            this.Level.Session.Audio.Apply();
        }

        private void TriggerEnvironmentalEvents()
        {
            CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
            if (cutsceneNode != null)
                RumbleTrigger.ManuallyTrigger(((Entity)cutsceneNode).X, 0f);
            Scene.Entities.FindFirst<MoonGlitchBackgroundTrigger>()?.Invoke();
        }

        public override void OnEnd(Level level)
        {
            Audio.Stop(this.crashMusicSfx, true);
            timeRateModifier.ResetTimeRateMultiplier();
            level.Session.SetFlag(Flag);
            if (this.WasSkipped)
            {
                this.player.Sprite.Play("idle");
                CutsceneNode cutsceneNode = CutsceneNode.Find("player_skip");
                if (cutsceneNode != null)
                {
                    this.player.Position = ((Entity)cutsceneNode).Position.Floor();
                    level.Camera.Position = this.player.CameraTarget;
                }
                if (this.flingBirdmod != null)
                {
                    if (this.flingBirdmod.CrashSfxEmitter != null)
                        this.Scene.Remove(this.flingBirdmod.CrashSfxEmitter);
                    this.flingBirdmod.RemoveSelf();
                }
                if (this.bird != null)
                    this.bird.RemoveSelf();
                TriggerEnvironmentalEvents();
                StartMusic();
            }
            this.player.Speed = Vector2.Zero;
            this.player.DummyAutoAnimate = true;
            this.player.DummyFriction = true;
            this.player.DummyGravity = true;
            this.player.ForceCameraUpdate = false;
            this.player.StateMachine.State = 0;
            // Show CharaBoost when cutscene ends (including skip)
            CustomCharaBoost charaBoostRestore = base.Scene?.Entities?.FindFirst<CustomCharaBoost>();
            if (charaBoostRestore != null)
            {
                charaBoostRestore.Active = charaBoostRestore.Visible = charaBoostRestore.Collidable = true;
            }
        }
    }
}






