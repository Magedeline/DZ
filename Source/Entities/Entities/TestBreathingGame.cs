namespace Celeste.Entities
{
    /// <summary>
    /// Heartbeat Breathing Minigame
    /// The player must press a button in sync with a heartbeat rhythm.
    /// A pulsing heart visual expands and contracts; the player must tap
    /// at the peak of each beat. Staying in rhythm raises a "calm" meter.
    /// Missing beats or pressing at the wrong time raises a "panic" meter.
    /// Reaching full calm completes the game and sets a session flag.
    /// </summary>
    [Tracked]
    [HotReloadable]
    public class TestBreathingGame : Entity
    {
        // â”€â”€ Configuration â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private readonly float beatInterval;      // seconds between heartbeats
        private readonly float requiredCalm;      // calm amount needed to win (0-1)
        private readonly string completionFlag;   // session flag set on success
        private readonly float hitWindow;         // tolerance window in seconds
        private readonly int maxMisses;           // misses before fail (0 = no fail)
        private readonly bool freezePlayer = true;  // whether to manage player StateMachine

        // â”€â”€ Game State â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private bool gameActive;
        private bool gameComplete;
        private bool gameFailed;
        private float calmMeter;          // 0 â†’ 1, reaching requiredCalm wins
        private float panicMeter;         // 0 â†’ 1, visual feedback only
        private int consecutiveHits;
        private int totalHits;
        private int totalMisses;

        // â”€â”€ Public API â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        /// <summary>True once the player has filled the calm meter.</summary>
        public bool Completed => gameComplete;
        /// <summary>True if the player exceeded maxMisses.</summary>
        public bool Failed => gameFailed;
        /// <summary>Current panic level (0-1) for external effects.</summary>
        public float PanicLevel => panicMeter;
        /// <summary>Current calm level (0-1) for external effects.</summary>
        public float CalmLevel => calmMeter;

        // â”€â”€ Heartbeat Timing â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private float beatTimer;          // counts up toward beatInterval
        private float beatPhase;          // 0 â†’ 1 within one beat cycle
        private bool beatPeakReached;     // true once we cross the peak this cycle
        private bool inputConsumed;       // prevent double-tap in one beat

        // â”€â”€ Visual â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private float heartScale = 1f;
        private float heartTargetScale = 1f;
        private float heartAlpha = 1f;
        private float ringScale;          // expanding ring on tap
        private float ringAlpha;
        private Color heartColor = Color.Red;
        private Color ringColor = Color.White;
        private float screenShake;
        private float flashAlpha;         // white flash on good hit

        // â”€â”€ UI â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private float uiAlpha;
        private string feedbackText = "";
        private float feedbackTimer;
        private Color feedbackColor = Color.White;

        // â”€â”€ Player â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private global::Celeste.Player player;

        // â”€â”€ Constants â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        private const float CALM_PER_HIT = 0.04f;
        private const float CALM_PER_PERFECT = 0.08f;
        private const float CALM_DRAIN = 0.005f;
        private const float PANIC_PER_MISS = 0.15f;
        private const float PANIC_DECAY = 0.3f;
        private const float HEART_PULSE_MIN = 0.85f;
        private const float HEART_PULSE_MAX = 1.35f;
        private const float PERFECT_WINDOW_RATIO = 0.35f; // fraction of hitWindow for "perfect"

        public TestBreathingGame(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            beatInterval = data.Float("beatInterval", 0.85f);
            requiredCalm = data.Float("requiredCalm", 0.95f);
            completionFlag = data.Attr("completionFlag", "BREATHING_GAME_COMPLETE");
            hitWindow = data.Float("hitWindow", 0.25f);
            maxMisses = data.Int("maxMisses", 0);

            Tag = Tags.TransitionUpdate | Tags.HUD;
            Depth = -1000000; // render on top
        }

        /// <summary>
        /// Programmatic constructor for use inside cutscenes or other code.
        /// </summary>
        public TestBreathingGame(
            bool freezePlayer = true,
            float beatInterval = 0.85f,
            float requiredCalm = 0.95f,
            float hitWindow = 0.25f,
            int maxMisses = 0,
            string completionFlag = "BREATHING_GAME_COMPLETE")
            : base(Vector2.Zero)
        {
            this.freezePlayer = freezePlayer;
            this.beatInterval = beatInterval;
            this.requiredCalm = requiredCalm;
            this.hitWindow = hitWindow;
            this.maxMisses = maxMisses;
            this.completionFlag = completionFlag;

            Tag = Tags.TransitionUpdate | Tags.HUD;
            Depth = -1000000;
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  Lifecycle
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        public override void Added(Scene scene)
        {
            base.Added(scene);

            player = scene.Tracker.GetEntity<global::Celeste.Player>();
            Add(new Coroutine(GameSequence()));
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  Core Loop (Coroutine)
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        private IEnumerator GameSequence()
        {
            var level = Scene as Level;

            // â”€â”€ Freeze player movement â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (freezePlayer && player != null)
                player.StateMachine.State = global::Celeste.Player.StDummy;

            // â”€â”€ Fade in â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            while (uiAlpha < 1f)
            {
                uiAlpha = Math.Min(uiAlpha + Engine.DeltaTime * 2f, 1f);
                yield return null;
            }

            // â”€â”€ Countdown â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            ShowFeedback("Breatheâ€¦", Color.LightCyan, 1.5f);
            Audio.Play("event:/ui/game/memorial_dream_text_in");
            yield return 1.5f;

            ShowFeedback("Follow the heartbeat", Color.LightCyan, 1.5f);
            yield return 1.5f;

            // â”€â”€ Start game â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            gameActive = true;
            beatTimer = 0f;
            beatPhase = 0f;
            calmMeter = 0f;
            panicMeter = 0f;

            // Main loop: coroutine yields every frame; Update() handles input
            while (gameActive)
            {
                // Win condition
                if (calmMeter >= requiredCalm)
                {
                    gameActive = false;
                    gameComplete = true;
                    break;
                }

                // Fail condition (optional)
                if (maxMisses > 0 && totalMisses >= maxMisses)
                {
                    gameActive = false;
                    gameFailed = true;
                    break;
                }

                yield return null;
            }

            // â”€â”€ Outcome â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (gameComplete)
            {
                ShowFeedback("Calm restored", Color.LightGreen, 2f);
                Audio.Play("event:/ui/game/memorial_dream_text_in");
                if (level != null)
                    level.Session.SetFlag(completionFlag, true);
            }
            else if (gameFailed)
            {
                ShowFeedback("Lost focusâ€¦", Color.Salmon, 2f);
                Audio.Play("event:/ui/game/memorial_dream_text_in");
            }

            yield return 2.5f;

            // â”€â”€ Fade out â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            while (uiAlpha > 0f)
            {
                uiAlpha = Math.Max(uiAlpha - Engine.DeltaTime * 2f, 0f);
                yield return null;
            }

            // â”€â”€ Release player â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (freezePlayer && player != null)
                player.StateMachine.State = global::Celeste.Player.StNormal;

            RemoveSelf();
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  Update
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        public override void Update()
        {
            base.Update();

            float dt = Engine.DeltaTime;

            // â”€â”€ Heartbeat rhythm â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (gameActive)
            {
                beatTimer += dt;
                beatPhase = beatTimer / beatInterval;

                // Peak is at phase = 0.5
                if (beatPhase >= 1f)
                {
                    // Completed one full beat cycle
                    if (!inputConsumed && beatPeakReached)
                    {
                        // Player missed this beat
                        OnMiss();
                    }

                    beatTimer -= beatInterval;
                    beatPhase = beatTimer / beatInterval;
                    beatPeakReached = false;
                    inputConsumed = false;
                }

                // Detect peak crossing for heartbeat sound
                if (!beatPeakReached && beatPhase >= 0.5f)
                {
                    beatPeakReached = true;
                    Audio.Play("event:/new_content/inthedark_heartbeat");
                }

                // â”€â”€ Heart visual scale â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                // Smooth "lub-dub": two bumps per cycle
                float t = beatPhase * MathHelper.TwoPi;
                float pulse = (float)(Math.Sin(t) * 0.5f + 0.5f);           // primary beat
                float dub   = (float)(Math.Sin(t * 2f - 1.2f) * 0.2f);      // secondary "dub"
                heartTargetScale = MathHelper.Lerp(HEART_PULSE_MIN, HEART_PULSE_MAX, pulse + Math.Max(dub, 0f));

                // â”€â”€ Input check â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                if (!inputConsumed && (Input.Jump.Pressed || Input.Dash.Pressed || Input.Grab.Pressed))
                {
                    inputConsumed = true;
                    EvaluatePress();
                }

                // â”€â”€ Calm drain (slowly lose calm if idle) â”€â”€â”€â”€â”€â”€â”€â”€
                calmMeter = Math.Max(0f, calmMeter - CALM_DRAIN * dt);

                // â”€â”€ Panic decay â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
                panicMeter = Math.Max(0f, panicMeter - PANIC_DECAY * dt);
            }

            // â”€â”€ Animate heart scale â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            heartScale = MathHelper.Lerp(heartScale, heartTargetScale, 1f - (float)Math.Pow(0.001, dt));

            // â”€â”€ Colour shift: red â†’ pink as calm increases â”€â”€â”€â”€â”€â”€â”€
            heartColor = Color.Lerp(Color.Red, Color.HotPink, calmMeter);
            if (panicMeter > 0.3f)
                heartColor = Color.Lerp(heartColor, Color.DarkRed, (panicMeter - 0.3f) / 0.7f);

            // â”€â”€ Ring expand animation â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (ringAlpha > 0f)
            {
                ringScale += dt * 3f;
                ringAlpha = Math.Max(0f, ringAlpha - dt * 2.5f);
            }

            // â”€â”€ Flash decay â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            flashAlpha = Math.Max(0f, flashAlpha - dt * 4f);

            // â”€â”€ Screen shake decay â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            screenShake = Math.Max(0f, screenShake - dt * 6f);

            // â”€â”€ Feedback text timer â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (feedbackTimer > 0f)
                feedbackTimer -= dt;
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  Input Evaluation
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        private void EvaluatePress()
        {
            // How close is the press to the peak (phase = 0.5)?
            float distFromPeak = Math.Abs(beatPhase - 0.5f);
            float distInSeconds = distFromPeak * beatInterval;

            if (distInSeconds <= hitWindow)
            {
                bool perfect = distInSeconds <= hitWindow * PERFECT_WINDOW_RATIO;

                if (perfect)
                {
                    OnPerfectHit();
                }
                else
                {
                    OnGoodHit();
                }
            }
            else
            {
                OnMiss();
            }
        }

        private void OnPerfectHit()
        {
            totalHits++;
            consecutiveHits++;
            float bonus = 1f + consecutiveHits * 0.1f; // streak bonus
            calmMeter = Math.Min(1f, calmMeter + CALM_PER_PERFECT * bonus);
            panicMeter = Math.Max(0f, panicMeter - 0.1f);

            ShowFeedback("Perfect!", Color.Gold, 0.6f);
            TriggerRing(Color.Gold);
            flashAlpha = 0.3f;

            Audio.Play("event:/ui/game/increment_heartgem");
        }

        private void OnGoodHit()
        {
            totalHits++;
            consecutiveHits++;
            float bonus = 1f + consecutiveHits * 0.05f;
            calmMeter = Math.Min(1f, calmMeter + CALM_PER_HIT * bonus);
            panicMeter = Math.Max(0f, panicMeter - 0.05f);

            ShowFeedback("Good", Color.LightGreen, 0.4f);
            TriggerRing(Color.LightGreen);
            flashAlpha = 0.15f;

            Audio.Play("event:/ui/game/increment_heartgem");
        }

        private void OnMiss()
        {
            totalMisses++;
            consecutiveHits = 0;
            panicMeter = Math.Min(1f, panicMeter + PANIC_PER_MISS);
            calmMeter = Math.Max(0f, calmMeter - 0.02f);

            ShowFeedback("Miss", Color.Salmon, 0.5f);
            screenShake = 0.4f;

            Audio.Play("event:/ui/game/memorial_dream_text_in");
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  Helpers
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        private void ShowFeedback(string text, Color color, float duration)
        {
            feedbackText = text;
            feedbackColor = color;
            feedbackTimer = duration;
        }

        private void TriggerRing(Color color)
        {
            ringScale = heartScale;
            ringAlpha = 1f;
            ringColor = color;
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  Render  (Tags.HUD â†’ drawn in screen-space)
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        public override void Render()
        {
            base.Render();

            if (uiAlpha <= 0f) return;

            var level = Scene as Level;
            if (level == null) return;

            // Screen-space centre
            float cx = Engine.Width / 2f;
            float cy = Engine.Height / 2f;

            Vector2 shake = Vector2.Zero;
            if (screenShake > 0f)
            {
                shake = new Vector2(
                    Calc.Random.Range(-2f, 2f) * screenShake,
                    Calc.Random.Range(-2f, 2f) * screenShake
                );
            }

            Vector2 centre = new Vector2(cx, cy) + shake;

            // â”€â”€ Semi-transparent backdrop â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            Draw.Rect(-10, -10, Engine.Width + 20, Engine.Height + 20,
                Color.Black * (0.55f + panicMeter * 0.2f) * uiAlpha);

            // â”€â”€ White flash â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (flashAlpha > 0f)
                Draw.Rect(-10, -10, Engine.Width + 20, Engine.Height + 20, Color.White * flashAlpha * uiAlpha);

            // â”€â”€ Expanding ring â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (ringAlpha > 0f)
            {
                DrawHollowCircle(centre, ringScale * 60f, ringColor * ringAlpha * uiAlpha, 3f);
            }

            // â”€â”€ Heart shape (drawn as a filled circle cluster) â”€â”€â”€
            DrawHeart(centre, heartScale * 50f, heartColor * heartAlpha * uiAlpha);

            // â”€â”€ Calm meter bar (bottom) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            float barWidth = 300f;
            float barHeight = 14f;
            Vector2 barPos = new Vector2(cx - barWidth / 2f, Engine.Height - 80f);

            // Background
            Draw.Rect(barPos.X - 2, barPos.Y - 2, barWidth + 4, barHeight + 4, Color.DarkSlateGray * uiAlpha);
            // Fill
            Color barColor = Color.Lerp(Color.OrangeRed, Color.LightGreen, calmMeter);
            Draw.Rect(barPos.X, barPos.Y, barWidth * calmMeter, barHeight, barColor * uiAlpha);
            // Threshold marker
            float threshX = barPos.X + barWidth * requiredCalm;
            Draw.Line(threshX, barPos.Y - 4, threshX, barPos.Y + barHeight + 4, Color.White * 0.7f * uiAlpha);

            // Label
            ActiveFont.DrawOutline(
                "Calm",
                new Vector2(cx, barPos.Y - 20f),
                new Vector2(0.5f, 1f),
                Vector2.One * 0.45f,
                Color.White * uiAlpha,
                2f,
                Color.Black * uiAlpha
            );

            // â”€â”€ Streak counter â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (consecutiveHits >= 3 && gameActive)
            {
                ActiveFont.DrawOutline(
                    $"x{consecutiveHits}",
                    centre + new Vector2(80f, -20f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.5f,
                    Color.Gold * uiAlpha,
                    2f,
                    Color.Black * uiAlpha
                );
            }

            // â”€â”€ Feedback text â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (feedbackTimer > 0f)
            {
                float alpha = Math.Min(feedbackTimer * 3f, 1f);
                ActiveFont.DrawOutline(
                    feedbackText,
                    centre + new Vector2(0f, -90f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.65f,
                    feedbackColor * alpha * uiAlpha,
                    2f,
                    Color.Black * alpha * uiAlpha
                );
            }

            // â”€â”€ Instruction line â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            if (gameActive)
            {
                ActiveFont.DrawOutline(
                    "Press on the beat",
                    new Vector2(cx, Engine.Height - 40f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.35f,
                    Color.Gray * 0.7f * uiAlpha,
                    1f,
                    Color.Black * 0.4f * uiAlpha
                );
            }
        }

        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
        //  Drawing Helpers
        // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

        /// <summary>
        /// Draws a simple heart shape using overlapping circles and a triangle.
        /// </summary>
        private void DrawHeart(Vector2 centre, float size, Color color)
        {
            float r = size * 0.4f;

            // Two upper circles
            Vector2 leftCircle  = centre + new Vector2(-r * 0.55f, -r * 0.25f);
            Vector2 rightCircle = centre + new Vector2( r * 0.55f, -r * 0.25f);

            DrawFilledCircle(leftCircle,  r, color);
            DrawFilledCircle(rightCircle, r, color);

            // Lower triangle
            Vector2 topLeft  = centre + new Vector2(-size * 0.52f, -r * 0.15f);
            Vector2 topRight = centre + new Vector2( size * 0.52f, -r * 0.15f);
            Vector2 bottom   = centre + new Vector2(0f, size * 0.55f);

            DrawFilledTriangle(topLeft, topRight, bottom, color);
        }

        private void DrawFilledCircle(Vector2 centre, float radius, Color color)
        {
            // Approximate with concentric rings of lines
            int segments = 24;
            for (float r = 0; r <= radius; r += 1.5f)
            {
                for (int i = 0; i < segments; i++)
                {
                    float a1 = MathHelper.TwoPi * i / segments;
                    float a2 = MathHelper.TwoPi * (i + 1) / segments;
                    Vector2 p1 = centre + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * r;
                    Vector2 p2 = centre + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * r;
                    Draw.Line(p1, p2, color);
                }
            }
        }

        private void DrawHollowCircle(Vector2 centre, float radius, Color color, float thickness)
        {
            int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float a1 = MathHelper.TwoPi * i / segments;
                float a2 = MathHelper.TwoPi * (i + 1) / segments;
                Vector2 p1 = centre + new Vector2((float)Math.Cos(a1), (float)Math.Sin(a1)) * radius;
                Vector2 p2 = centre + new Vector2((float)Math.Cos(a2), (float)Math.Sin(a2)) * radius;
                Draw.Line(p1, p2, color, thickness);
            }
        }

        private void DrawFilledTriangle(Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            // Scanline fill via horizontal lines
            // Sort by Y
            if (a.Y > b.Y) (a, b) = (b, a);
            if (a.Y > c.Y) (a, c) = (c, a);
            if (b.Y > c.Y) (b, c) = (c, b);

            float totalHeight = c.Y - a.Y;
            if (totalHeight < 1f) return;

            for (float y = a.Y; y <= c.Y; y += 1f)
            {
                float t1 = (y - a.Y) / totalHeight;
                float x1 = MathHelper.Lerp(a.X, c.X, t1);

                float x2;
                if (y < b.Y)
                {
                    float segH = b.Y - a.Y;
                    if (segH < 1f) segH = 1f;
                    float t2 = (y - a.Y) / segH;
                    x2 = MathHelper.Lerp(a.X, b.X, t2);
                }
                else
                {
                    float segH = c.Y - b.Y;
                    if (segH < 1f) segH = 1f;
                    float t2 = (y - b.Y) / segH;
                    x2 = MathHelper.Lerp(b.X, c.X, t2);
                }

                if (x1 > x2) (x1, x2) = (x2, x1);
                Draw.Line(new Vector2(x1, y), new Vector2(x2, y), color);
            }
        }
    }
}

