using MaggyHelper.Entities;

namespace MaggyHelper.Cutscenes
{
    [HotReloadable]
    public class CS02_CharaIntro : CutsceneEntity
    {
        /// <summary>
        /// Static flag to track if the intro cutscene has been triggered this session.
        /// Reset when the level reloads.
        /// </summary>
        public static bool HasTriggered { get; private set; } = false;

        /// <summary>
        /// Resets the triggered state. Call this on level reload/restart.
        /// </summary>
        public static void ResetTriggeredState() => HasTriggered = false;

        #region Constants
        private const float ANXIETY_FADE_SPEED = 2.5f;
        private const float ANXIETY_INTERVAL = 0.1f;
        private const float ANXIETY_SINE_RATE = 0.3f;
        private const float CHARA_OFFSET_X = 8f;
        private const float CHARA_OFFSET_Y = -24f;
        private const float ZOOM_LEVEL = 2f;
        private const float ZOOM_DURATION = 0.5f;
        private const float CAMERA_TRANSITION_TIME = 0.5f;
        private const string MUSIC_EVENT = "event:/desolozantas/music/lvl2/chara_intro";
        private const string AUDIO_EVENT = "event:/desolozantas/game/02_nightmare/sequence_chara_intro";
        private const string CHARA_DISAPPEAR_SOUND = "event:/char/chara/disappear";
        #endregion

        #region Fields
        private global::Celeste.Player player;
        private CharaChaser chara;
        private Vector2 charaEndPosition;
        private float anxietyFade;
        private float anxietyFadeTarget;
        private SineWave anxietySine;
        private float anxietyJitter;
        private bool hasEnded;
        #endregion

        public CS02_CharaIntro(CharaChaser charaChaser) : base(true, false)
        {
            chara = charaChaser ?? throw new ArgumentNullException(nameof(charaChaser));
            charaEndPosition = charaChaser.Position + new Vector2(CHARA_OFFSET_X, CHARA_OFFSET_Y);
            Add(anxietySine = new SineWave(ANXIETY_SINE_RATE, 0f));
            Distort.AnxietyOrigin = new Vector2(0.5f, 0.75f);
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
                anxietyJitter = Calc.Random.Range(-0.1f, 0.1f);
            }

            Distort.Anxiety = anxietyFade *
                Math.Max(0f, anxietyJitter + anxietySine.Value * 0.3f);
        }

        private IEnumerator Cutscene(Level level)
        {
            anxietyFadeTarget = 1f;

            while ((player = level.Tracker.GetEntity<global::Celeste.Player>()) == null)
            {
                yield return null;
            }

            while (!player.OnGround())
            {
                yield return null;
            }

            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return 1f;

            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                Audio.SetMusic(MUSIC_EVENT, true, true);
            }

            yield return Textbox.Say("CH2_CHARA_INTRO", new Func<IEnumerator>[] {
                TurnAround,
                RevealChara,
            });

            anxietyFadeTarget = 0f;
            yield return Level.ZoomBack(CAMERA_TRANSITION_TIME);
            EndCutscene(level, true);
        }

        private IEnumerator TurnAround()
        {
            player.Facing = Facings.Left;
            yield return 0.2f;

            Add(new Coroutine(CameraTo(
                new Vector2(Level.Bounds.X, Level.Camera.Y),
                CAMERA_TRANSITION_TIME, null, 0f)));
            yield return Level.ZoomTo(new Vector2(84f, 135f), ZOOM_LEVEL, ZOOM_DURATION);
            yield return 0.2f;
        }

        private bool PlayCharaAnimation(string animName)
        {
            if (chara?.Sprite != null && chara.Sprite.Has(animName))
            {
                chara.Sprite.Play(animName);
                return true;
            }
            return false;
        }

        private void SetCharaHovering(bool value)
        {
            if (chara != null)
            {
                chara.Hovering = value;
            }
        }

        private IEnumerator RevealChara()
        {
            Audio.Play(AUDIO_EVENT, chara.Position);
            yield return 0.1f;

            Level.Displacement.AddBurst(
                chara.Position + new Vector2(0f, -4f), 0.8f, 8f, 48f, 0.5f);
            yield return 0.1f;

            SetCharaHovering(true);
            if (!PlayCharaAnimation("spawn"))
            {
                PlayCharaAnimation("fallSlow");
            }

            Vector2 from = chara.Position;
            Vector2 to = charaEndPosition;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime)
            {
                chara.Position = from + (to - from) * Ease.CubeInOut(t);
                yield return null;
            }

            player.Facing = player.Position.X > chara.Position.X ? Facings.Left : Facings.Right;
            yield return 1f;
        }

        public override void OnEnd(Level level)
        {
            if (hasEnded)
            {
                return;
            }

            hasEnded = true;

            try
            {
                Audio.SetMusic(null, true, true);
                Distort.Anxiety = 0f;

                if (player != null)
                {
                    CutsceneManager.ResetPlayerState(player);
                    player.StateMachine.Locked = false;
                    player.Facing = Facings.Left;
                    player.StateMachine.State = 0;
                    player.JustRespawned = true;
                }

                if (chara != null)
                {
                    chara.Position = charaEndPosition;
                    SetCharaHovering(false);
                    PlayCharaAnimation("fallSlow");
                    
                    // Start the exit sequence
                    level.Add(new ExitSequencer(chara, level, this));
                }

                level.Session.SetFlag("evil_chara_intro", true);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper",
                    $"Error in CS02_CharaIntro.OnEnd: {ex}");
            }
        }

        private class ExitSequencer : Entity
        {
            private readonly CharaChaser chara;
            private readonly Level level;
            private CS02_CharaIntro parent;

            public ExitSequencer(CharaChaser charaChaser, Level level, CS02_CharaIntro parent)
            {
                chara = charaChaser;
                this.level = level;
                this.parent = parent;
                Add(new Coroutine(Sequence()));
            }

            private IEnumerator Sequence()
            {
                yield return 0.5f;

                if (chara == null)
                {
                    RemoveSelf();
                    yield break;
                }

                yield return 1f;

                // Play disappear effects
                Audio.Play(CHARA_DISAPPEAR_SOUND, chara.Position);
                level.Displacement.AddBurst(chara.Position, 0.5f, 24f, 96f, 0.4f);
                
                if (CharaChaser.P_Vanish != null)
                {
                    level.Particles.Emit(CharaChaser.P_Vanish, 12, chara.Position, Vector2.One * 6f);
                }

                // Remove the chara from the level
                chara.RemoveSelf();

                RemoveSelf();
            }
        }
    }
}




