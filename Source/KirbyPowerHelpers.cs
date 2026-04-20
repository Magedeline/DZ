using Celeste.Extensions;

namespace Celeste.Entities
{
    /// <summary>
    /// Helper class for performing Warper-style dash effects.
    /// Used by both legacy KirbyPlayer and new KirbyPlayerExtension.
    /// </summary>
    public static class WarperDashHelper
    {
        private const string SFX_DASH_LEFT = "event:/desolozantas/char/kirby/dash_red_left";
        private const string SFX_DASH_RIGHT = "event:/desolozantas/char/kirby/dash_red_right";
        private const string SFX_DASH_CHARGE = "event:/desolozantas/char/kirby/dash_charge";
        
        /// <summary>
        /// Perform a warper-style dash effect
        /// </summary>
        /// <param name="position">Starting position</param>
        /// <param name="direction">Dash direction</param>
        /// <param name="scene">Current scene</param>
        /// <param name="power">Optional power modifier for enhanced effects</param>
        public static void PerformWarperDash(Vector2 position, Vector2 direction, Scene scene, float power = 1f)
        {
            if (scene is not Level level) return;
            
            // Normalize direction
            if (direction.LengthSquared() > 0)
            {
                direction = direction.SafeNormalize();
            }
            else
            {
                // Default to right if no direction
                direction = Vector2.UnitX;
            }
            
            // Use directional dash events so FMOD routing remains deterministic.
            Audio.Play(direction.X < 0f ? SFX_DASH_LEFT : SFX_DASH_RIGHT, position);
            
            // Create visual effect scaled by power
            int particleCount = (int)(8 * power);
            level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, particleCount, position, Vector2.One * 8f * power);
            
            IngesteLogger.Debug($"Warper dash performed at {position} in direction {direction} with power {power}");
        }

        /// <summary>
        /// Perform a charged warper dash with enhanced effects
        /// </summary>
        public static void PerformChargedWarperDash(Vector2 position, Vector2 direction, Scene scene, float chargeLevel)
        {
            if (scene is not Level level) return;
            
            // Normalize direction
            direction = direction.LengthSquared() > 0 ? direction.SafeNormalize() : Vector2.UnitX;
            
            // Play charged dash sound
            Audio.Play(SFX_DASH_CHARGE, position);
            
            // Enhanced particle effect
            int particleCount = (int)(16 * chargeLevel);
            level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, particleCount, position, Vector2.One * 16f);
            level.ParticlesBG?.Emit(ParticleTypes.Dust, particleCount / 2, position, Vector2.One * 24f);
            
            // Screen shake for fully charged
            if (chargeLevel >= 1f)
            {
                level.Shake(0.2f);
            }
            
            IngesteLogger.Debug($"Charged warper dash at {position}, charge level: {chargeLevel}");
        }
    }

    /// <summary>
    /// Legacy alias for WarperDashHelper (backwards compatibility)
    /// </summary>
    [Obsolete("Use WarperDashHelper instead")]
    public static class WarperDashSimple
    {
        public static void PerformWarperDash(Vector2 position, Vector2 direction, Scene scene)
            => WarperDashHelper.PerformWarperDash(position, direction, scene);
    }
    
    /// <summary>
    /// Helper class for Kirby Knight transformation.
    /// Supports both legacy KirbyPlayer and new KirbyPlayerExtension.
    /// </summary>
    public static class KirbyKnightHelper
    {
        private const string SFX_KNIGHT_TRANSFORM = "event:/desolozantas/char/kirby/kirby_knight/backflip";
        private const string SFX_KNIGHT_ATTACK = "event:/desolozantas/char/kirby/kirby_knight/punch_A";
        private const string SFX_KNIGHT_SPECIAL = "event:/desolozantas/char/kirby/kirby_knight/spin";
        
        /// <summary>
        /// Whether the knight form is currently active
        /// </summary>
        public static bool IsKnightActive { get; private set; }
        
        /// <summary>
        /// Knight attack damage multiplier
        /// </summary>
        public static float KnightDamageMultiplier { get; set; } = 2f;
        
        /// <summary>
        /// Try to transform to the knight form
        /// </summary>
        /// <param name="scene">Current scene</param>
        /// <param name="forceTransform">If true, always transform regardless of conditions</param>
        /// <returns>True if transformation occurred</returns>
        public static bool TryTransformToKnight(Scene scene, bool forceTransform = false)
        {
            if (scene is not Level level) return false;
            
            // Only allow transformation in chapters 19-20 or when forced
            var areaId = level.Session?.Area.ID ?? 0;
            bool isLateChapter = areaId >= 19;
            
            if (!forceTransform && !isLateChapter)
            {
                return false;
            }
            
            if (IsKnightActive)
            {
                return false;
            }
            
            IsKnightActive = true;
            
            // Set power state on KirbyPlayer if present
            var kirby = level.Tracker.GetEntity<KirbyMode>();
            kirby?.SetPowerState(KirbyMode.KirbyPowerState.Knight);
            
            // Play transformation sound
            Vector2 effectPos = level.Camera.Position + new Vector2(160f, 90f);
            Audio.Play(SFX_KNIGHT_TRANSFORM, effectPos);
            
            // Create transformation effect
            level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, 20, effectPos, Vector2.One * 32f);
            level.Flash(Color.Gold * 0.3f, true);
            level.Shake(0.3f);
            
            IngesteLogger.Info("Kirby Knight transformation activated");
            return true;
        }
        
        /// <summary>
        /// Perform a knight attack
        /// </summary>
        /// <param name="position">Attack origin position</param>
        /// <param name="scene">Current scene</param>
        /// <param name="direction">Attack direction</param>
        public static void PerformKnightAttack(Vector2 position, Scene scene, Vector2? direction = null)
        {
            if (!IsKnightActive) return;
            if (scene is not Level level) return;
            
            Vector2 dir = direction ?? Vector2.UnitX;
            if (dir.LengthSquared() > 0)
            {
                dir = dir.SafeNormalize();
            }
            
            // Play attack sound
            Audio.Play(SFX_KNIGHT_ATTACK, position);
            
            // Create attack effect in attack direction
            Vector2 effectOffset = dir * 16f;
            level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, 15, position + effectOffset, Vector2.One * 20f);
            
            IngesteLogger.Debug($"Knight attack performed at {position} in direction {dir}");
        }
        
        /// <summary>
        /// Perform a special knight attack (charged or ultimate)
        /// </summary>
        public static void PerformKnightSpecialAttack(Vector2 position, Scene scene, float chargeLevel = 1f)
        {
            if (!IsKnightActive) return;
            if (scene is not Level level) return;
            
            Audio.Play(SFX_KNIGHT_SPECIAL, position);
            
            int particleCount = (int)(25 * chargeLevel);
            level.ParticlesFG?.Emit(ParticleTypes.SparkyDust, particleCount, position, Vector2.One * 32f);
            level.Flash(Color.Gold * 0.2f * chargeLevel, true);
            level.Shake(0.2f * chargeLevel);
            
            IngesteLogger.Debug($"Knight special attack at {position} with charge {chargeLevel}");
        }
        
        /// <summary>
        /// Reset the knight form
        /// </summary>
        public static void ResetKnight()
        {
            IsKnightActive = false;
            IngesteLogger.Debug("Kirby Knight form reset");
        }
    }

    /// <summary>
    /// Legacy alias for KirbyKnightHelper (backwards compatibility)
    /// </summary>
    [Obsolete("Use KirbyKnightHelper instead")]
    public static class KirbyKnightSimple
    {
        public static bool IsKnightActive => KirbyKnightHelper.IsKnightActive;
        
        public static void TryTransformToKnight(Scene scene, bool forceTransform = false)
            => KirbyKnightHelper.TryTransformToKnight(scene, forceTransform);
        
        public static void PerformKnightAttack(Vector2 position, Scene scene)
            => KirbyKnightHelper.PerformKnightAttack(position, scene);
        
        public static void ResetKnight()
            => KirbyKnightHelper.ResetKnight();
    }
}
