namespace Celeste.Entities
{
    /// <summary>
    /// Optimized RalseiDummy entity - floating ghost companion similar to CharaDummy
    /// Features proper initialization, error handling, and smooth floating movement
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/RalseiDummy")]
    [HotReloadable]
    public class RalseiDummy : Entity
    {
        // Particle type for vanish effect
        public static ParticleType P_Vanish = new ParticleType
        {
            Size = 1f,
            Color = Color.White,
            Color2 = Calc.HexToColor("63C74D"),  // Ralsei's green color
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.8f,
            LifeMax = 1.4f,
            SpeedMin = 12f,
            SpeedMax = 24f,
            DirectionRange = (float)Math.PI * 2f
        };

        // Constants for better maintainability
        private const float DEFAULT_FLOAT_SPEED = 120f;
        private const float DEFAULT_FLOAT_ACCEL = 240f;
        private const float DEFAULT_FLOATNESS = 2f;
        private const float DEFAULT_SINE_WAVE_RATE = 0.25f;
        private const int DEFAULT_LIGHT_START_RADIUS = 20;
        private const int DEFAULT_LIGHT_END_RADIUS = 60;

        // Dash color constants
        private static readonly Color UsedHairColor = Calc.HexToColor("44B7FF");
        private static readonly Color NormalHairColor = Calc.HexToColor("AC3232");
        private static readonly Color TwoDashesHairColor = Calc.HexToColor("ff6def");
        private static readonly Color TripleDashHairColor = Calc.HexToColor("ffa500");
        private static readonly Color QuadDashHairColor = Calc.HexToColor("00ff88");
        private static readonly Color PentaDashHairColor = Calc.HexToColor("00ccff");
        private static readonly Color HexaDashHairColor = Calc.HexToColor("aa44ff");
        private static readonly Color SeptaDashHairColor = Calc.HexToColor("ff2288");
        private static readonly Color OctaDashHairColor = Calc.HexToColor("ffee00");
        private static readonly Color NonaDashHairColor = Calc.HexToColor("22ffdd");
        private static readonly Color DecaDashHairColor = Calc.HexToColor("ffffff");

        // Public properties with proper backing fields
        public Sprite Sprite { get; private set; }
        public Image HairImage { get; private set; }
        public BadelineAutoAnimator AutoAnimator { get; private set; }
        public SineWave Wave { get; private set; }
        public VertexLight Light { get; private set; }

        // Configuration properties
        public float FloatSpeed { get; set; } = DEFAULT_FLOAT_SPEED;
        public float FloatAccel { get; set; } = DEFAULT_FLOAT_ACCEL;
        public float Floatness { get; set; } = DEFAULT_FLOATNESS;

        // Internal state
        private Vector2 floatNormal = Vector2.UnitY;
        private bool isInitialized = false;
        public RalseiDummy(Vector2 position, int index = 0) : base(position)
        {
            try
            {
                Depth = -1;

                // Initialize collider first
                Collider = new Hitbox(6f, 6f, -3f, -7f);

                // Initialize sprite
                InitializeSprite();

                // Initialize hair system - now handled safely
                InitializeHair();

                // Initialize other components
                InitializeComponents();
                InitializeLight();
                InitializeWaveSystem();

                isInitialized = true;
            }
            catch (Exception ex)
            {
                // Log error but don't crash the game
                Logger.Log(LogLevel.Error, "RalseiDummy", $"Failed to initialize RalseiDummy: {ex}");
                // Set minimal working state
                SetMinimalState();
            }
        }

        // Constructor for loading from map data
        public RalseiDummy(EntityData data, Vector2 offset)
            : this(data.Position + offset, 0)
        {
        }

        private void InitializeSprite()
        {
            try
            {
                Sprite = GFX.SpriteBank.Create("ralsei");
            }
            catch
            {
                // Fallback to badeline if ralsei sprite bank fails
                try
                {
                    Sprite = GFX.SpriteBank.Create("player_badeline");
                }
                catch
                {
                    Logger.Log(LogLevel.Error, "RalseiDummy", "Failed to create sprite from sprite bank");
                    return;
                }
            }

            if (Sprite != null)
            {
                Sprite.Play("fallSlow");
                Sprite.Scale.X = -1f;

                // Set up frame change handler with null checks
                Sprite.OnFrameChange = OnSpriteFrameChange;
                Add(Sprite);
            }
        }

        private void InitializeHair()
        {
            try
            {
                MTexture hairTexture = GFX.Game["characters/ralsei/hair00"];
                HairImage = new Image(hairTexture);
                HairImage.CenterOrigin();
                HairImage.Position = new Vector2(0f, -8f);
                HairImage.Color = NormalHairColor;
                Add(HairImage);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "RalseiDummy", $"Hair image initialization failed: {ex.Message}");
                HairImage = null;
            }
        }

        private void InitializeComponents()
        {
            try
            {
                AutoAnimator = new BadelineAutoAnimator();
                Add(AutoAnimator);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "RalseiDummy", $"Failed to initialize auto animator: {ex.Message}");
            }
        }

        private void InitializeLight()
        {
            try
            {
                Light = new VertexLight(
                    new Vector2(0f, -8f),
                    Color.PaleGreen,
                    1f,
                    DEFAULT_LIGHT_START_RADIUS,
                    DEFAULT_LIGHT_END_RADIUS
                );
                Add(Light);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "RalseiDummy", $"Failed to initialize light: {ex.Message}");
            }
        }

        private void InitializeWaveSystem()
        {
            try
            {
                Wave = new SineWave(DEFAULT_SINE_WAVE_RATE, 0f);
                Wave.OnUpdate = OnWaveUpdate;
                Add(Wave);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "RalseiDummy", $"Failed to initialize wave system: {ex.Message}");
            }
        }

        private void SetMinimalState()
        {
            if (Sprite == null)
            {
                try
                {
                    Sprite = GFX.SpriteBank.Create("ralsei");
                    Sprite.Play("fallSlow");
                    Add(Sprite);
                    isInitialized = true;
                }
                catch
                {
                    Logger.Log(LogLevel.Error, "RalseiDummy", "Critical failure: Unable to create minimal sprite");
                }
            }
        }

        private void OnSpriteFrameChange(string animationName)
        {
            if (Sprite == null) return;

            try
            {
                int currentFrame = Sprite.CurrentAnimationFrame;

                // Check for footstep frames in walking/running animations
                if (IsFootstepFrame(animationName, currentFrame))
                {
                    Audio.Play("guid://{ff6442f1-66c2-4dba-8d57-038dda8fd296}", Position);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "RalseiDummy", $"Error in sprite frame change: {ex.Message}");
            }
        }

        private static bool IsFootstepFrame(string animationName, int frame)
        {
            return (animationName == "walk" || animationName == "runSlow" || animationName == "runFast")
                   && (frame == 0 || frame == 6);
        }

        private void OnWaveUpdate(float waveValue)
        {
            if (Sprite == null) return;

            try
            {
                Sprite.Position = floatNormal * waveValue * Floatness;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "RalseiDummy", $"Error in wave update: {ex.Message}");
            }
        }

        public void Appear(Level level, bool silent = false)
        {
            if (!isInitialized || level == null) return;

            try
            {
                if (!silent)
                {
                    Audio.Play("guid://{ad25a031-880b-4f88-ac12-82d6c52fbdea}", Position);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }

                level.Displacement?.AddBurst(Center, 0.5f, 24f, 96f, 0.4f, null, null);
                level.Particles?.Emit(P_Vanish, 12, Center, Vector2.One * 6f);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "RalseiDummy", $"Error in Appear: {ex.Message}");
            }
        }

        public void Vanish()
        {
            if (!isInitialized) return;

            try
            {
                Audio.Play("guid://{16b40879-0a79-4e42-8c91-fe419a8e186c}", Position);
                CreateShockwave();

                var level = SceneAs<Level>();
                level?.Particles?.Emit(P_Vanish, 12, Center, Vector2.One * 6f);

                RemoveSelf();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "RalseiDummy", $"Error in Vanish: {ex.Message}");
                // Still try to remove self even if effects fail
                try
                {
                    RemoveSelf();
                }
                catch (Exception fallbackEx)
                {
                    Logger.Log(LogLevel.Error, "RalseiDummy", $"Failed to remove self after vanish error: {fallbackEx.Message}");
                }
            }
        }

        private void CreateShockwave()
        {
            try
            {
                var level = SceneAs<Level>();
                level?.Displacement?.AddBurst(Center, 0.5f, 24f, 96f, 0.4f, null, null);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "RalseiDummy", $"Error creating shockwave: {ex.Message}");
            }
        }

        /// <summary>
        /// Float to a target position with acceleration-based movement (primary overload).
        /// Matches the CharaDummy/BadelineDummy floating ghost pattern.
        /// </summary>
        public IEnumerator FloatTo(Vector2 target, int? turnAtEndTo = null, bool faceDirection = true, bool fadeLight = false, bool quickEnd = false)
        {
            if (!isInitialized || Sprite == null) yield break;

            Sprite.Play("fallSlow");

            // Set facing direction
            if (faceDirection && Math.Sign(target.X - X) != 0)
            {
                Sprite.Scale.X = Math.Sign(target.X - X);
            }

            Vector2 direction = (target - Position).SafeNormalize();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            float currentSpeed = 0f;

            // Movement phase
            while (Position != target)
            {
                currentSpeed = Calc.Approach(currentSpeed, FloatSpeed, FloatAccel * Engine.DeltaTime);
                Position = Calc.Approach(Position, target, currentSpeed * Engine.DeltaTime);
                Floatness = Calc.Approach(Floatness, 4f, 8f * Engine.DeltaTime);
                floatNormal = Calc.Approach(floatNormal, perpendicular, Engine.DeltaTime * 12f);

                if (fadeLight && Light != null)
                {
                    Light.Alpha = Calc.Approach(Light.Alpha, 0f, Engine.DeltaTime * 2f);
                }

                yield return null;
            }

            // Settle phase
            if (quickEnd)
            {
                Floatness = DEFAULT_FLOATNESS;
            }
            else
            {
                while (Math.Abs(Floatness - DEFAULT_FLOATNESS) > 0.01f)
                {
                    Floatness = Calc.Approach(Floatness, DEFAULT_FLOATNESS, 8f * Engine.DeltaTime);
                    yield return null;
                }
                Floatness = DEFAULT_FLOATNESS;
            }

            // Final facing direction
            if (turnAtEndTo.HasValue && Sprite != null)
            {
                Sprite.Scale.X = turnAtEndTo.Value;
            }
        }

        /// <summary>
        /// Float to a target position with explicit speed and easing parameters.
        /// Used by cutscenes that need fine-grained control over movement.
        /// </summary>
        public IEnumerator FloatTo(Vector2 targetPosition, int faceDirection, bool useSpeedCalculation, bool easeIntoMovement, float speed)
        {
            if (!isInitialized || Sprite == null) yield break;

            if (faceDirection != 0)
            {
                Sprite.Scale.X = faceDirection;
            }

            Sprite.Play("fallSlow");

            Vector2 startPosition = Position;
            float duration = useSpeedCalculation ? Vector2.Distance(startPosition, targetPosition) / speed : 1f;
            if (duration <= 0f) duration = 0.1f;

            for (float t = 0f; t < 1f; t += Engine.DeltaTime / duration)
            {
                float eased = easeIntoMovement ? Ease.SineInOut(t) : t;
                Position = Vector2.Lerp(startPosition, targetPosition, eased);
                yield return null;
            }
            Position = targetPosition;
        }

        public IEnumerator WalkTo(float targetX, float speed = 64f)
        {
            if (!isInitialized || Sprite == null) yield break;

            Floatness = 0f;
            Sprite.Play("walk");

            if (Math.Sign(targetX - X) != 0)
            {
                Sprite.Scale.X = Math.Sign(targetX - X);
            }

            while (Math.Abs(X - targetX) > 0.1f)
            {
                X = Calc.Approach(X, targetX, Engine.DeltaTime * speed);
                yield return null;
            }

            X = targetX; // Ensure exact positioning
            Sprite.Play("idle");
        }

        public IEnumerator SmashBlock(Vector2 target)
        {
            if (!isInitialized || Sprite == null) yield break;

            var level = SceneAs<Level>();
            level?.Displacement?.AddBurst(Position, 0.5f, 24f, 96f, 1f, null, null);

            Sprite.Play("dreamDashLoop");
            Vector2 startPosition = Position;

            // Move to target
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime * 6f)
            {
                Position = Vector2.Lerp(startPosition, target, Ease.CubeOut(progress));
                yield return null;
            }

            // Break block
            try
            {
                var dashBlock = Scene?.Entities?.FindFirst<DashBlock>();
                dashBlock?.Break(Position, new Vector2(0f, -1f), false, true);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "RalseiDummy", $"Error breaking block: {ex.Message}");
            }

            Sprite.Play("idle");

            // Return to start
            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime * 4f)
            {
                Position = Vector2.Lerp(target, startPosition, Ease.CubeOut(progress));
                yield return null;
            }

            Sprite.Play("fallSlow");
        }

        public new IEnumerator CollidePoint(Vector2 target)
        {
            if (!isInitialized || Sprite == null) yield break;

            Sprite.Play("dreamDashLoop");
            Vector2 startPosition = Position;

            for (float progress = 0f; progress < 1f; progress += Engine.DeltaTime * 6f)
            {
                Position = Vector2.Lerp(startPosition, target, Ease.CubeOut(progress));
                yield return null;
            }

            Position = target;
            Sprite.Play("fallSlow");
        }

        private static Color GetDashColor(int dashes)
        {
            return dashes switch
            {
                >= 10 => DecaDashHairColor,
                9 => NonaDashHairColor,
                8 => OctaDashHairColor,
                7 => SeptaDashHairColor,
                6 => HexaDashHairColor,
                5 => PentaDashHairColor,
                4 => QuadDashHairColor,
                3 => TripleDashHairColor,
                2 => TwoDashesHairColor,
                _ => NormalHairColor,
            };
        }

        private void UpdateHairColor()
        {
            if (HairImage == null) return;

            Player player = Scene?.Tracker?.GetEntity<Player>();
            if (player == null) return;

            Color targetColor;
            if (player.Dashes == 0 && player.Dashes < player.MaxDashes)
                targetColor = UsedHairColor;
            else
                targetColor = GetDashColor(player.Dashes);

            HairImage.Color = Color.Lerp(HairImage.Color, targetColor, 6f * Engine.DeltaTime);

            // Flip hair image to match sprite facing
            if (Sprite != null)
                HairImage.Scale.X = Math.Sign(Sprite.Scale.X);
        }

        public override void Update()
        {
            if (!isInitialized) return;

            try
            {
                UpdateHairColor();
                base.Update();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "RalseiDummy", $"Error in Update: {ex.Message}");
            }
        }

        public override void Render()
        {
            if (!isInitialized || Sprite == null) return;

            try
            {
                // Store original render position for pixel-perfect rendering
                Vector2 originalRenderPosition = Sprite.RenderPosition;
                Sprite.RenderPosition = Sprite.RenderPosition.Floor();

                base.Render();

                // Restore original render position
                Sprite.RenderPosition = originalRenderPosition;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "RalseiDummy", $"Error in Render: {ex.Message}");
                // Try basic render as fallback
                try
                {
                    base.Render();
                }
                catch (Exception fallbackEx)
                {
                    Logger.Log(LogLevel.Error, "RalseiDummy", $"Critical render failure: {fallbackEx.Message}");
                }
            }
        }

        public override void Removed(Scene scene)
        {
            try
            {
                isInitialized = false;
                base.Removed(scene);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "RalseiDummy", $"Error in Removed: {ex.Message}");
            }
        }
    }
}

