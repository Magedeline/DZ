using Celeste.Entities;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// CS04DZ_CHaraWarning - Warning cutscene before CharaChaser2 begins pursuit.
    /// Shows Chara appearing briefly to warn/taunt the player before the enhanced chase.
    /// </summary>
    [HotReloadable]
    public class CS04DZ_CHaraWarning : CutsceneEntity
    {
        /// <summary>
        /// Static flag to track if the warning cutscene has been triggered this session.
        /// Reset when the level reloads.
        /// </summary>
        public static bool HasTriggered { get; private set; } = false;

        /// <summary>
        /// Resets the triggered state. Call this on level reload/restart.
        /// </summary>
        public static void ResetTriggeredState() => HasTriggered = false;

        #region Constants
        public const string Flag = "evilDZ_CHara_warning";
        private const float ANXIETY_FADE_SPEED = 3f;
        private const float ANXIETY_INTERVAL = 0.08f;
        private const float ANXIETY_SINE_RATE = 0.4f;
        private const float CHARA_OFFSET_X = 16f;
        private const float CHARA_OFFSET_Y = -32f;
        private const float ZOOM_LEVEL = 1.8f;
        private const float ZOOM_DURATION = 0.4f;
        private const float CAMERA_TRANSITION_TIME = 0.4f;
        private const string MUSIC_EVENT = "event:/pusheen/music/lvl4/chara_warning";
        private const string AUDIO_EVENT = "event:/game/02_old_site/sequence_badeline_intro";
        private const string CHARA_APPEAR_SOUND = "event:/char/chara/appear";
        private const string CHARA_DISAPPEAR_SOUND = "event:/char/chara/disappear";
        private const string CHARA_LAUGH_SOUND = "event:/char/chara/laugh";
        #endregion

        #region Fields
        private global::Celeste.Player player;
        private CharaChaser chara;
        private Vector2 charaStartPosition;
        private Vector2 charaEndPosition;
        private float anxietyFade;
        private float anxietyFadeTarget;
        private SineWave anxietySine;
        private float anxietyJitter;
        private bool charaWasVisible;
        #endregion

        public CS04DZ_CHaraWarning(CharaChaser charaChaser) : base(true, false)
        {
            chara = charaChaser ?? throw new ArgumentNullException(nameof(charaChaser));
            charaStartPosition = charaChaser.Position;
            charaEndPosition = charaChaser.Position + new Vector2(CHARA_OFFSET_X, CHARA_OFFSET_Y);
            charaWasVisible = chara.Visible;
            Add(anxietySine = new SineWave(ANXIETY_SINE_RATE, 0f));
            Distort.AnxietyOrigin = new Vector2(0.5f, 0.5f);
            HasTriggered = true;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        public override void Update()
        {
            base.Update();
            UpdateAnxietyEffects();
        }

        private void UpdateAnxietyEffects()
        {
            anxietyFade = Calc.Approach(anxietyFade, anxietyFadeTarget,
                ANXIETY_FADE_SPEED * Engine.DeltaTime);

            if (Scene.OnInterval(ANXIETY_INTERVAL))
            {
                anxietyJitter = Calc.Random.Range(-0.15f, 0.15f);
            }

            Distort.Anxiety = anxietyFade *
                Math.Max(0f, anxietyJitter + anxietySine.Value * 0.4f);
        }

        private IEnumerator Cutscene(Level level)
        {
            // Wait for player to exist and be grounded
            while ((player = level.Tracker.GetEntity<global::Celeste.Player>()) == null)
            {
                yield return null;
            }

            while (!player.OnGround())
            {
                yield return null;
            }

            // Lock player state
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            
            yield return 0.5f;

            // Start anxiety effects
            anxietyFadeTarget = 1f;
            
            // Set ominous music
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                Audio.SetMusic(MUSIC_EVENT, true, true);
            }

            yield return 0.3f;

            // Screen shake to build tension
            level.Shake(0.3f);
            
            yield return 0.4f;

            // Make Chara appear dramatically
            yield return RevealChara(level);

            // Play warning dialog
            yield return Textbox.Say("DZ_CH4_CHARA_WARNING", new Func<IEnumerator>[] {
                CharaLaugh,
            });

            anxietyFadeTarget = 0f;
            yield return Level.ZoomBack(CAMERA_TRANSITION_TIME);
            
            EndCutscene(level, true);
        }

        private IEnumerator RevealChara(Level level)
        {
            // Play appear sound
            Audio.Play(CHARA_APPEAR_SOUND, chara.Position);
            
            // Make chara visible
            chara.Visible = true;
            chara.Sprite.Scale = Vector2.Zero;
            chara.Sprite.Color = Color.Transparent;
            
            // Displacement burst effect
            level.Displacement.AddBurst(
                chara.Position + new Vector2(0f, -4f), 0.8f, 8f, 64f, 0.6f);
            
            yield return 0.1f;

            // Animate chara appearing
            chara.Hovering = true;
            chara.Sprite.Play("spawn");

            // Scale up animation
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
            {
                chara.Sprite.Scale = Vector2.One * Ease.BackOut(t);
                chara.Sprite.Color = Color.White * Ease.CubeOut(t);
                yield return null;
            }
            chara.Sprite.Scale = Vector2.One;
            chara.Sprite.Color = Color.White;

            // Move to warning position
            Vector2 from = chara.Position;
            Vector2 to = charaEndPosition;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 1.5f)
            {
                chara.Position = from + (to - from) * Ease.CubeInOut(t);
                yield return null;
            }
            chara.Position = to;

            // Zoom camera on Chara
            Add(new Coroutine(CameraTo(
                new Vector2(chara.Position.X - 80f, chara.Position.Y - 60f),
                CAMERA_TRANSITION_TIME, null, 0f)));
            yield return Level.ZoomTo(new Vector2(120f, 100f), ZOOM_LEVEL, ZOOM_DURATION);

            // Face player
            if (player != null)
            {
                player.Facing = player.Position.X > chara.Position.X ? Facings.Left : Facings.Right;
                chara.Sprite.Scale.X = player.Position.X > chara.Position.X ? 1f : -1f;
            }

            // Play angry animation
            chara.Sprite.Play("angry");
            
            yield return 0.5f;
        }

        private IEnumerator CharaLaugh()
        {
            Audio.Play(CHARA_LAUGH_SOUND, chara.Position);
            chara.Sprite.Play("laugh");
            
            // Shake screen slightly during laugh
            Level.Shake(0.2f);
            
            yield return 1f;
            
            chara.Sprite.Play("angry");
            yield return 0.3f;

            // Signal CharaChaser that intro is complete and it should begin chasing
            chara.BeginChasing();
            Level.Session.SetFlag("evilDZ_CHara_warning");
        }

        public override void OnEnd(Level level)
        {
            // Reset anxiety effects
            Distort.Anxiety = 0f;
            anxietyFade = 0f;
            anxietyFadeTarget = 0f;

            // Restore player state
            if (player != null)
            {
                player.StateMachine.Locked = false;
                player.StateMachine.State = Player.StNormal;
            }

            // Restore camera
            level.Zoom = 1f;

            // Set flag so this cutscene doesn't re-trigger
            level.Session.SetFlag(Flag);
        }
    }
}

