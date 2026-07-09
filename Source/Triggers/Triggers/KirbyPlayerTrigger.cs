using Celeste.Entities;
using Celeste.Extensions;
using Monocle;

namespace Celeste.Triggers
{
    /// <summary>
    /// Trigger to transform the player to/from Kirby mode.
    /// Supports multiple activation types and visual transformation effects.
    /// </summary>
    [CustomEntity(ids: "DZ/Kirby_Player_Trigger")]
    [Tracked]
    public class KirbyPlayerTrigger : Trigger
    {
        #region Enums
        
        public enum ActivationType
        {
            OnEnter,    // Activate when player enters
            OnExit,     // Activate when player exits
            Toggle      // Toggle between normal and Kirby mode
        }

        public enum TransformationType
        {
            Instant,    // Immediate transformation
            Animated,   // Play transformation animation
            Fade        // Fade out/in transformation
        }

        #endregion

        #region Fields

        private readonly ActivationType activationType;
        private readonly TransformationType transformationType;
        private readonly bool oneUse;
        private readonly string transformAnimation;
        private readonly float transformDuration;
        private readonly bool preserveVelocity;
        private readonly KirbyMode.KirbyPowerState initialPower;
        private readonly string requiredFlag;
        private readonly bool playSound;
        
        private bool hasTriggered;
        private float particleTimer;

        // SFX paths
        private const string SFX_TRANSFORM = "event:/DZ/char/kirby/transform_in";

        #endregion

        #region Constructor

        public KirbyPlayerTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            activationType = data.Enum<ActivationType>(nameof(activationType), ActivationType.OnEnter);
            transformationType = data.Enum<TransformationType>(nameof(transformationType), TransformationType.Animated);
            oneUse = data.Bool(nameof(oneUse), false);
            transformAnimation = data.Attr(nameof(transformAnimation), "transform");
            transformDuration = data.Float(nameof(transformDuration), 1.0f);
            preserveVelocity = data.Bool(nameof(preserveVelocity), true);
            requiredFlag = data.Attr("requiredFlag", "");
            playSound = data.Bool("playSound", true);
            
            // Parse initial power state
            var powerStr = data.Attr("initialPower", "None");
            if (Enum.TryParse<KirbyMode.KirbyPowerState>(powerStr, out var power))
            {
                initialPower = power;
            }
            else
            {
                initialPower = KirbyMode.KirbyPowerState.None;
            }
            
            hasTriggered = false;
        }

        #endregion

        #region Update

        public override void Update()
        {
            base.Update();

            // Reset trigger for non-one-use triggers
            if (!oneUse && activationType != ActivationType.Toggle)
            {
                // Check for either K_Player or vanilla Player
                var kPlayer = Scene.Tracker.GetEntity<K_Player>();
                var vanillaPlayer = Scene.Tracker.GetEntity<global::Celeste.Player>();
                var anyPlayer = kPlayer != null ? (Actor)kPlayer : vanillaPlayer;
                if (anyPlayer != null && !CollideCheck(anyPlayer))
                {
                    hasTriggered = false;
                }
            }

            // Update particle effects
            particleTimer += Engine.DeltaTime;
            if (particleTimer > 0.4f)
            {
                particleTimer = 0f;
                EmitAmbientParticles();
            }
        }

        private void EmitAmbientParticles()
        {
            if (Calc.Random.Chance(0.25f) && Scene is Level level)
            {
                Vector2 sparklePos = Position + new Vector2(
                    Calc.Random.Range(0, (int)Width),
                    Calc.Random.Range(0, (int)Height)
                );
                level.ParticlesBG?.Emit(ParticleTypes.SparkyDust, 1, sparklePos, Vector2.One * 4f);
            }
        }

        #endregion

        #region Trigger Events

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            if (activationType == ActivationType.OnEnter || activationType == ActivationType.Toggle)
            {
                // Check if K_Player is present (K_Player takes priority over vanilla Player)
                var kPlayer = Scene?.Tracker.GetEntity<K_Player>();
                if (kPlayer != null && CollideCheck(kPlayer))
                {
                    TriggerTransformation(kPlayer);
                }
                else if (player != null)
                {
                    TriggerTransformation(player);
                }
            }
        }

        public override void OnLeave(global::Celeste.Player player)
        {
            base.OnLeave(player);
            if (activationType == ActivationType.OnExit)
            {
                // Check if K_Player is present (K_Player takes priority over vanilla Player)
                var kPlayer = Scene?.Tracker.GetEntity<K_Player>();
                if (kPlayer != null && !CollideCheck(kPlayer))
                {
                    TriggerTransformation(kPlayer);
                }
                else if (player != null)
                {
                    TriggerTransformation(player);
                }
            }
        }

        #endregion

        #region Transformation Logic

        private void TriggerTransformation(K_Player kPlayer)
        {
            if (oneUse && hasTriggered) return;
            if (kPlayer == null) return;

            var level = Scene as Level;
            if (level == null) return;

            // Check required flag
            if (!string.IsNullOrEmpty(requiredFlag) && !level.Session.GetFlag(requiredFlag))
            {
                return;
            }

            hasTriggered = true;

            // Store velocity if needed
            Vector2 storedVelocity = preserveVelocity ? kPlayer.Speed : Vector2.Zero;

            // Check if player is in Kirby mode
            if (kPlayer.KirbyModeActive)
            {
                TransformKPlayerToNormal(kPlayer);
            }
            else
            {
                TransformKPlayerToKirby(kPlayer);
            }

            // Restore velocity if preserveVelocity is enabled
            if (preserveVelocity)
            {
                kPlayer.Speed = storedVelocity;
            }
        }

        private void TriggerTransformation(global::Celeste.Player player)
        {
            if (oneUse && hasTriggered) return;
            if (player == null) return;

            var level = Scene as Level;
            if (level == null) return;

            // Check required flag
            if (!string.IsNullOrEmpty(requiredFlag) && !level.Session.GetFlag(requiredFlag))
            {
                return;
            }

            hasTriggered = true;

            // Store velocity if needed
            Vector2 storedVelocity = preserveVelocity ? player.Speed : Vector2.Zero;

            // Check if player is in Kirby mode
            if (player.IsKirbyMode())
            {
                TransformToNormalPlayer(player);
            }
            else
            {
                TransformToKirbyPlayer(player);
            }

            // Restore velocity if preserveVelocity is enabled
            if (preserveVelocity)
            {
                player.Speed = storedVelocity;
            }
        }

        private void TransformToNormalPlayer(global::Celeste.Player player)
        {
            if (player == null) return;

            // Play transformation effect
            PlayTransformationEffect(player.Position, false);

            // Disable Kirby mode
            player.DisableKirbyMode();
            
            IngesteLogger.Info("Player transformed back to normal via KirbyPlayerTrigger");
        }

        private void TransformToKirbyPlayer(global::Celeste.Player player)
        {
            if (player == null) return;

            // Play transformation effect
            PlayTransformationEffect(player.Position, true);

            // Enable Kirby mode
            player.EnableKirbyMode();

            // Set initial power state if specified
            if (initialPower != KirbyMode.KirbyPowerState.None)
            {
                player.SetKirbyPowerState(initialPower);
            }

            IngesteLogger.Info($"Player transformed to Kirby via KirbyPlayerTrigger{(initialPower != KirbyMode.KirbyPowerState.None ? $" with power: {initialPower}" : "")}");
        }

        private void TransformKPlayerToKirby(K_Player kPlayer)
        {
            if (kPlayer == null) return;

            // Play transformation effect
            PlayTransformationEffect(kPlayer.Position, true);

            // Enable Kirby mode on K_Player
            kPlayer.KirbyModeActive = true;

            // Enable health system
            var healthManager = PlayerHealthManager.GetOrCreate(kPlayer.Scene as Level, kPlayer.MaxHealth);
            healthManager.EnableKirbyMode(kPlayer.MaxHealth);

            // Ensure hat and scarf are visible
            if (kPlayer.HatScarf != null)
            {
                kPlayer.HatScarf.Visible = true;
                kPlayer.HatScarf.Color = kPlayer.Hair.Color;
            }

            IngesteLogger.Info($"K_Player transformed to Kirby mode via KirbyPlayerTrigger");
        }

        private void TransformKPlayerToNormal(K_Player kPlayer)
        {
            if (kPlayer == null) return;

            // Play transformation effect
            PlayTransformationEffect(kPlayer.Position, false);

            // Disable Kirby mode on K_Player
            kPlayer.KirbyModeActive = false;

            // Disable health system
            var healthManager = PlayerHealthManager.Instance;
            if (healthManager != null)
            {
                healthManager.DisableKirbyMode();
            }

            // Hide hat and scarf
            if (kPlayer.HatScarf != null)
            {
                kPlayer.HatScarf.Visible = false;
            }

            IngesteLogger.Info("K_Player transformed back to normal via KirbyPlayerTrigger");
        }

        #endregion

        #region Visual Effects

        private void PlayTransformationEffect(Vector2 position, bool toKirby)
        {
            var level = Scene as Level;
            if (level == null) return;

            // Play sound
            if (playSound)
            {
                Audio.Play(SFX_TRANSFORM, position);
            }

            // Create visual effect based on transformation type
            switch (transformationType)
            {
                case TransformationType.Instant:
                    CreateInstantEffect(level, position, toKirby);
                    break;
                case TransformationType.Animated:
                    CreateAnimatedEffect(level, position, toKirby);
                    break;
                case TransformationType.Fade:
                    CreateFadeEffect(level, position, toKirby);
                    break;
            }
        }

        private void CreateInstantEffect(Level level, Vector2 position, bool toKirby)
        {
            Color effectColor = toKirby ? Color.Pink : Color.LightBlue;
            level.Flash(effectColor * 0.4f, true);
            level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, 15, position, Vector2.One * 16f);
        }

        private void CreateAnimatedEffect(Level level, Vector2 position, bool toKirby)
        {
            Color effectColor = toKirby ? Color.Pink : Color.LightBlue;
            
            // Burst of particles
            level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, 20, position, Vector2.One * 20f);
            level.ParticlesBG?.Emit(ParticleTypes.Dust, 10, position, Vector2.One * 24f);
            
            // Add coroutine for extended effect
            Add(new Coroutine(AnimatedEffectRoutine(level, position, effectColor)));
        }

        private IEnumerator AnimatedEffectRoutine(Level level, Vector2 position, Color color)
        {
            for (float t = 0f; t < transformDuration; t += Engine.DeltaTime)
            {
                if (t < transformDuration * 0.5f)
                {
                    Vector2 offset = new Vector2(
                        Calc.Random.Range(-16f, 16f),
                        Calc.Random.Range(-16f, 16f)
                    );
                    level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, 2, position + offset, Vector2.One * 8f);
                }
                yield return null;
            }
        }

        private void CreateFadeEffect(Level level, Vector2 position, bool toKirby)
        {
            Color effectColor = toKirby ? Color.Pink : Color.LightBlue;
            level.Flash(effectColor * 0.3f);
            level.ParticlesFG?.Emit(ParticleTypes.Dust, 12, position, Vector2.One * 18f);
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
                
                var center = Center;
                Draw.Line(center - Vector2.UnitX * 4, center + Vector2.UnitX * 4, debugColor);
                Draw.Line(center - Vector2.UnitY * 4, center + Vector2.UnitY * 4, debugColor);
            }
        }

        #endregion
    }
}
