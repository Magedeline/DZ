namespace MaggyHelper.Entities
{
    /// <summary>
    /// A large eyeball boss entity for Chapter 7 (Infernal Reflections).
    /// Bounces the player away on contact, fires shockwaves periodically,
    /// and can be destroyed by throwing a TheoCrystal into it.
    /// Destroying it triggers level completion.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/InfernoBigEyeball")]
    [Tracked]
    public class InfernoBigEyeball : Entity
    {
        private Sprite sprite;
        private Image pupil;
        private bool triggered;
        private Vector2 pupilTarget;
        private float pupilDelay;
        private Wiggler bounceWiggler;
        private Wiggler pupilWiggler;
        private float shockwaveTimer;
        private bool shockwaveFlag;
        private float pupilSpeed = 40f;
        private bool bursting;
        private float glitchStrength;

        public InfernoBigEyeball(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(sprite = GFX.SpriteBank.Create("temple_eyeball"));
            Add(pupil = new Image(GFX.Game["danger/templeeye/pupil"]));
            pupil.CenterOrigin();

            Collider = new Hitbox(48f, 64f, -24f, -32f);

            Add(new PlayerCollider(OnPlayer));
            Add(new HoldableCollider(OnHoldable));
            Add(bounceWiggler = Wiggler.Create(0.5f, 3f));
            Add(pupilWiggler = Wiggler.Create(0.5f, 3f));

            shockwaveTimer = 2f;
            Depth = -10000;
        }

        public override void Update()
        {
            base.Update();

            if (bursting)
                return;

            Level level = SceneAs<Level>();
            CelestePlayer player = Scene.Tracker.GetEntity<CelestePlayer>();

            if (player != null && !triggered)
            {
                // Track distance from player for music and glitch effects
                float dist = Vector2.Distance(Position, player.Center);
                float musicParam = Calc.ClampedMap(dist, 256f, 32f, 0f, 1f);
                Audio.SetMusicParam("boss_proximity", musicParam);

                // Glitch effect intensifies as player gets closer
                glitchStrength = Calc.ClampedMap(dist, 128f, 16f, 0f, 0.15f);
                Glitch.Value = glitchStrength;

                // Fire shockwaves on a timer
                shockwaveTimer -= Engine.DeltaTime;
                if (shockwaveTimer <= 0f && !shockwaveFlag)
                {
                    shockwaveFlag = true;
                    shockwaveTimer = 3f;

                    level.Add(Engine.Pooler.Create<InfernoBigEyeballShockwave>()
                        .Init(Position));
                    Audio.Play("event:/game/05_mirror_temple/eye_pulse", Position);
                    pupilWiggler.Start();
                }
                else if (shockwaveTimer <= 0f)
                {
                    shockwaveFlag = false;
                    shockwaveTimer = 2f;
                }
            }

            // Pupil tracking
            TheoCrystal theo = Scene.Tracker.GetEntity<TheoCrystal>();
            if (theo != null)
            {
                pupilTarget = (theo.Center - Position).SafeNormalize() * 10f;
            }
            else if (player != null)
            {
                pupilTarget = (player.Center - Position).SafeNormalize() * 10f;
            }

            if (pupilDelay <= 0f)
            {
                pupil.Position = Calc.Approach(pupil.Position, pupilTarget,
                    pupilSpeed * Engine.DeltaTime);
            }
            else
            {
                pupilDelay -= Engine.DeltaTime;
            }
        }

        private void OnPlayer(CelestePlayer player)
        {
            if (triggered)
                return;

            Audio.Play("event:/game/05_mirror_temple/eyewall_bounce", player.Position);
            player.ExplodeLaunch(player.Center + Vector2.UnitX * 20f, false, false);
            bounceWiggler.Start();
        }

        private void OnHoldable(Holdable h)
        {
            if (h.Entity is TheoCrystal theoCrystal && !triggered
                && theoCrystal.Speed.X > 32f && !theoCrystal.Hold.IsHeld)
            {
                theoCrystal.Speed = new Vector2(-50f, -10f);
                triggered = true;
                bounceWiggler.Start();
                Collidable = false;

                Audio.SetAmbience(null);
                Audio.Play("event:/game/05_mirror_temple/eyewall_destroy", Position);
                Alarm.Set(this, 1.3f, () => Audio.SetMusic(null));

                Add(new Coroutine(BurstRoutine()));
            }
        }

        private IEnumerator BurstRoutine()
        {
            bursting = true;
            Level level = Scene as Level;

            level.StartCutscene(OnSkip, fadeInOnSkip: false,
                endingChapterAfterCutscene: true);
            level.RegisterAreaComplete();

            Celeste.Celeste.Freeze(0.1f);
            yield return null;

            // Reset glitch
            Glitch.Value = 0f;

            // Shake the screen
            level.Shake(0.5f);

            // Play burst animation
            sprite.Play("burst");
            pupil.Visible = false;

            // Burst all InfernoEyes in the scene
            foreach (InfernoEye eye in Scene.Tracker.GetEntities<InfernoEye>())
            {
                eye.Burst();
            }

            yield return 2f;

            // Flash and fade to white
            Fader fader = new Fader();
            Scene.Add(fader);

            float duration = 3f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                fader.Fade = Calc.Approach(fader.Fade, 1f, Engine.DeltaTime / duration);
                yield return null;
            }

            yield return 1f;

            level.CompleteArea(spotlightWipe: false, skipScreenWipe: false,
                skipCompleteScreen: false);
        }

        private void OnSkip(Level level)
        {
            Glitch.Value = 0f;
            level.CompleteArea(spotlightWipe: false, skipScreenWipe: false,
                skipCompleteScreen: false);
        }

        public override void Render()
        {
            sprite.Scale.X = 1f + 0.15f * bounceWiggler.Value;
            pupil.Scale = Vector2.One * (1f + pupilWiggler.Value * 0.15f);
            base.Render();
        }

        private class Fader : Entity
        {
            public float Fade;

            public Fader()
            {
                Tag = Tags.HUD;
                Depth = -1000000;
            }

            public override void Render()
            {
                Draw.Rect(-10f, -10f,
                    Engine.Width + 20, Engine.Height + 20,
                    Color.White * Fade);
            }
        }
    }
}
