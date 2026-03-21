using MaggyHelper.Extensions;

namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Comprehensive trigger for managing Kirby mode transformations.
    /// Supports multiple activation modes, visual effects, and settings integration.
    /// Integrates with KirbyPlayerExtension system for full functionality.
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/Kirby_Mode_Toggle_Trigger")]
    [Monocle.Tracked]
    public class KirbyModeToggleTrigger : Trigger
    {
        #region Enums
        
        public enum ActivationMode
        {
            OnEnter,        // Activate when player enters trigger
            OnExit,         // Activate when player exits trigger
            Toggle,         // Toggle mode on enter
            OnStay,         // Activate while player stays inside (continuous)
            Persistent      // Enable and keep enabled until disabled by another trigger
        }

        public enum TransformEffect
        {
            Instant,        // Immediate with flash
            Sparkle,        // Sparkle particles
            Flash,          // Screen flash effect
            Smooth,         // Smooth transition with particles
            Custom          // Use custom effect settings
        }

        public enum TriggerState
        {
            Enable,         // Enable Kirby mode
            Disable,        // Disable Kirby mode
            Toggle          // Toggle current state
        }

        #endregion

        #region Fields

        // Configuration
        private readonly ActivationMode activationMode;
        private readonly TransformEffect transformEffect;
        private readonly TriggerState triggerState;
        private readonly bool oneUse;
        private readonly bool respectSettings;
        private readonly string flagRequired;
        private readonly string flagToSet;
        private readonly bool silentMode;
        private readonly KirbyMode.KirbyPowerState initialPower;
        
        // Visual settings
        private readonly float effectDuration;
        private readonly Color particleColor;
        private readonly int particleCount;
        private readonly bool screenShake;
        private readonly float shakeIntensity;
        
        // Audio settings
        private readonly string transformSound;
        private readonly bool playSound;
        
        // State tracking
        private bool hasTriggered;
        private bool playerInside;
        private float stayTimer;
        private float particleTimer;
        
        // SFX paths
        private const string SFX_DEFAULT_TRANSFORM = "event:/desolozantas/char/kirby/transform_in";
        
        // Particle types
        private static ParticleType P_KirbySparkle;
        private static ParticleType P_KirbyBurst;
        private static ParticleType P_KirbyGlow;
        
        #endregion

        #region Constructor

        public KirbyModeToggleTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            // Activation settings
            activationMode = data.Enum<ActivationMode>(nameof(activationMode), ActivationMode.OnEnter);
            transformEffect = data.Enum<TransformEffect>(nameof(transformEffect), TransformEffect.Sparkle);
            triggerState = data.Enum<TriggerState>(nameof(triggerState), TriggerState.Toggle);
            oneUse = data.Bool(nameof(oneUse), false);
            respectSettings = data.Bool(nameof(respectSettings), true);
            
            // Flag settings
            flagRequired = data.Attr(nameof(flagRequired), "");
            flagToSet = data.Attr(nameof(flagToSet), "");
            silentMode = data.Bool(nameof(silentMode), false);
            
            // Visual settings
            effectDuration = data.Float(nameof(effectDuration), 1.0f);
            particleColor = data.HexColor(nameof(particleColor), Color.Pink);
            particleCount = data.Int(nameof(particleCount), 30);
            screenShake = data.Bool(nameof(screenShake), true);
            shakeIntensity = data.Float(nameof(shakeIntensity), 0.3f);
            
            // Audio settings
            transformSound = data.Attr(nameof(transformSound), SFX_DEFAULT_TRANSFORM);
            playSound = data.Bool(nameof(playSound), true);
            
            // Power state
            var powerStr = data.Attr("initialPower", "None");
            if (Enum.TryParse<KirbyMode.KirbyPowerState>(powerStr, out var power))
            {
                initialPower = power;
            }
            else
            {
                initialPower = KirbyMode.KirbyPowerState.None;
            }
            
            // Initialize state
            hasTriggered = false;
            playerInside = false;
            stayTimer = 0f;
            particleTimer = 0f;
            
            // Initialize particles
            InitializeParticles();
        }

        #endregion

        #region Initialization

        private static void InitializeParticles()
        {
            if (P_KirbySparkle != null) return;

            P_KirbySparkle = new ParticleType
            {
                Size = 1.0f,
                Color = Color.Pink,
                Color2 = Color.HotPink,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                SpeedMin = 10f,
                SpeedMax = 30f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMultiplier = 0.8f
            };

            P_KirbyBurst = new ParticleType
            {
                Size = 2.0f,
                Color = Color.Pink,
                Color2 = Color.LightPink,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Linear,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMultiplier = 0.5f
            };

            P_KirbyGlow = new ParticleType
            {
                Size = 3.0f,
                Color = Color.Pink * 0.5f,
                Color2 = Color.White * 0.3f,
                ColorMode = ParticleType.ColorModes.Static,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 5f,
                SpeedMax = 15f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -20f)
            };
        }

        #endregion

        #region Update & Trigger Logic

        public override void Update()
        {
            base.Update();

            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null) return;

            bool currentlyInside = CollideCheck(player);

            // Handle OnStay activation
            if (activationMode == ActivationMode.OnStay && currentlyInside)
            {
                stayTimer += Engine.DeltaTime;
                if (stayTimer >= 0.5f && !hasTriggered)
                {
                    TryActivate(player);
                }
            }

            // Reset trigger when player exits (for non-one-use triggers)
            if (!oneUse && playerInside && !currentlyInside)
            {
                hasTriggered = false;
                stayTimer = 0f;
            }

            playerInside = currentlyInside;

            // Ambient particle effects
            if (!silentMode)
            {
                particleTimer += Engine.DeltaTime;
                if (particleTimer >= 0.2f)
                {
                    particleTimer = 0f;
                    EmitAmbientParticles();
                }
            }
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            if (activationMode == ActivationMode.OnEnter || activationMode == ActivationMode.Toggle)
            {
                TryActivate(player);
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);
            
            if (activationMode == ActivationMode.OnExit)
            {
                TryActivate(player);
            }
            
            stayTimer = 0f;
        }

        #endregion

        #region Activation Logic

        private void TryActivate(global::Celeste.Player player)
        {
            if (player == null) return;
            if (oneUse && hasTriggered) return;

            var level = Scene as Level;
            if (level == null) return;
            
            // Extra safety check - ensure player is fully initialized
            if (player.Scene == null) return;

            // Check required flag
            if (!string.IsNullOrEmpty(flagRequired) && !level.Session.GetFlag(flagRequired))
            {
                return;
            }

            // Check settings
            if (respectSettings)
            {
                var settings = IngesteModule.Settings; if (settings != null && !settings.KirbyPlayerEnabled)
                {
                    if (!silentMode)
                    {
                        IngesteLogger.Info("Kirby mode trigger blocked by settings");
                    }
                    return;
                }
            }

            // Determine what action to take - wrapped in try-catch for safety
            bool shouldEnable;
            try
            {
                shouldEnable = DetermineTargetState(player);
            }
            catch (System.Exception ex)
            {
                IngesteLogger.Error($"KirbyModeToggleTrigger: Error determining target state: {ex.Message}");
                return;
            }
            
            // Execute transformation
            ExecuteTransformation(player, shouldEnable);
            
            // Set flag if specified
            if (!string.IsNullOrEmpty(flagToSet))
            {
                level.Session.SetFlag(flagToSet, true);
            }

            hasTriggered = true;
        }

        private bool DetermineTargetState(global::Celeste.Player player)
        {
            bool currentlyKirby = false;
            
            try
            {
                currentlyKirby = player?.IsKirbyMode() == true;
            }
            catch (System.NullReferenceException)
            {
                // Player state can be mid-transition during room/entity bootstrap.
                currentlyKirby = false;
            }
            
            return triggerState switch
            {
                TriggerState.Enable => true,
                TriggerState.Disable => false,
                TriggerState.Toggle => !currentlyKirby,
                _ => !currentlyKirby
            };
        }

        private void ExecuteTransformation(global::Celeste.Player player, bool enableKirby)
        {
            if (enableKirby)
            {
                player.EnableKirbyMode();
                
                // Set initial power state if specified
                if (initialPower != KirbyMode.KirbyPowerState.None)
                {
                    player.SetKirbyPowerState(initialPower);
                }
                
                if (!silentMode)
                {
                    IngesteLogger.Info($"Kirby mode ENABLED by trigger{(initialPower != KirbyMode.KirbyPowerState.None ? $" with power: {initialPower}" : "")}");
                }
            }
            else
            {
                player.DisableKirbyMode();
                if (!silentMode)
                {
                    IngesteLogger.Info("Kirby mode DISABLED by trigger");
                }
            }

            // Play effects
            PlayTransformEffect(player.Position, enableKirby);
            
            // Play sound
            if (playSound && !string.IsNullOrEmpty(transformSound))
            {
                Audio.Play(transformSound, player.Position);
            }
        }

        #endregion

        #region Visual Effects

        private void PlayTransformEffect(Vector2 position, bool enabling)
        {
            var level = Scene as Level;
            if (level == null) return;

            switch (transformEffect)
            {
                case TransformEffect.Instant:
                    CreateInstantEffect(level, position);
                    break;
                    
                case TransformEffect.Sparkle:
                    CreateSparkleEffect(level, position, enabling);
                    break;
                    
                case TransformEffect.Flash:
                    CreateFlashEffect(level, position);
                    break;

                case TransformEffect.Smooth:
                    CreateSmoothEffect(level, position, enabling);
                    break;
                    
                case TransformEffect.Custom:
                    CreateCustomEffect(level, position, enabling);
                    break;
            }

            // Screen shake
            if (screenShake)
            {
                level.Shake(shakeIntensity);
            }
        }

        private void CreateInstantEffect(Level level, Vector2 position)
        {
            level.Flash(particleColor * 0.6f, true);
            level.ParticlesFG?.Emit(P_KirbyBurst, particleCount / 2, position, Vector2.One * 16f, particleColor);
        }

        private void CreateSparkleEffect(Level level, Vector2 position, bool enabling)
        {
            // Burst of particles
            level.ParticlesFG?.Emit(P_KirbySparkle, particleCount, position, Vector2.One * 24f, particleColor);
            level.ParticlesBG?.Emit(P_KirbyGlow, particleCount / 3, position, Vector2.One * 20f, particleColor * 0.7f);
            
            // Add coroutine for extended effect
            Add(new Coroutine(SparkleEffectRoutine(level, position, enabling)));
        }

        private IEnumerator SparkleEffectRoutine(Level level, Vector2 position, bool enabling)
        {
            for (float t = 0f; t < effectDuration; t += Engine.DeltaTime)
            {
                if (t < effectDuration * 0.6f)
                {
                    Vector2 offset = new Vector2(
                        Calc.Random.Range(-24f, 24f),
                        Calc.Random.Range(-24f, 24f)
                    );
                    level.ParticlesFG?.Emit(P_KirbySparkle, 2, position + offset, Vector2.One * 8f, particleColor);
                }
                yield return null;
            }
        }

        private void CreateFlashEffect(Level level, Vector2 position)
        {
            level.Flash(particleColor, true);
            level.ParticlesFG?.Emit(P_KirbyBurst, particleCount, position, Vector2.One * 32f, particleColor);
        }

        private void CreateSmoothEffect(Level level, Vector2 position, bool enabling)
        {
            Add(new Coroutine(SmoothEffectRoutine(level, position, enabling)));
        }

        private IEnumerator SmoothEffectRoutine(Level level, Vector2 position, bool enabling)
        {
            float elapsed = 0f;
            while (elapsed < effectDuration)
            {
                elapsed += Engine.DeltaTime;
                float progress = elapsed / effectDuration;
                
                // Emit particles with decreasing intensity
                if (Calc.Random.Chance(1f - progress))
                {
                    Vector2 offset = new Vector2(
                        Calc.Random.Range(-16f, 16f),
                        Calc.Random.Range(-16f, 16f)
                    );
                    level.ParticlesFG?.Emit(P_KirbyGlow, 1, position + offset, Vector2.One * 12f, particleColor);
                }
                
                yield return null;
            }
        }

        private void CreateCustomEffect(Level level, Vector2 position, bool enabling)
        {
            // Custom effect using all parameters
            level.Flash(particleColor * 0.4f);
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = (float)i / particleCount * MathHelper.TwoPi;
                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle) * 20f,
                    (float)Math.Sin(angle) * 20f
                );
                
                level.ParticlesFG?.Emit(P_KirbySparkle, 1, position + offset, Vector2.One * 8f, particleColor);
            }
        }

        private void EmitAmbientParticles()
        {
            if (Calc.Random.Chance(0.2f))
            {
                var level = Scene as Level;
                if (level == null) return;

                Vector2 particlePos = Position + new Vector2(
                    Calc.Random.Range(0, (int)Width),
                    Calc.Random.Range(0, (int)Height)
                );

                level.ParticlesFG?.Emit(P_KirbyGlow, 1, particlePos, Vector2.One * 4f, particleColor * 0.3f);
            }
        }

        #endregion

        #region Rendering

        public override void Render()
        {
            base.Render();

            // Debug rendering
            if (Engine.Commands?.Open ?? false)
            {
                Color debugColor = hasTriggered && oneUse ? Color.Gray : Color.Yellow;
                Draw.HollowRect(Collider, debugColor);
                
                // Draw center marker
                Vector2 center = Center;
                Draw.Line(center - Vector2.UnitX * 4, center + Vector2.UnitX * 4, debugColor);
                Draw.Line(center - Vector2.UnitY * 4, center + Vector2.UnitY * 4, debugColor);
                
                // Draw state indicator
                string stateText = triggerState.ToString();
                Vector2 textPos = Position + new Vector2(4, 4);
                Draw.Point(textPos, debugColor);
            }
        }

        #endregion
    }
}




