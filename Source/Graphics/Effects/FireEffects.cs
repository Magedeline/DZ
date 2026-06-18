namespace Celeste.Effects
{
    /// <summary>
    /// Advanced fire elemental effects with multiple fire types
    /// </summary>
    [HotReloadable]
    public static class FireEffects
    {
        // Fire particle types
        public static ParticleType P_Fire;
        public static ParticleType P_Ember;
        public static ParticleType P_Flame;
        public static ParticleType P_Explosion;
        public static ParticleType P_Smoke;
        public static ParticleType P_BlueFire;
        public static ParticleType P_GreenFire;
        public static ParticleType P_WhiteFire;

        // Audio events
        public const string SFX_FIRE_IGNITE = "event:/Mods/pusheen/fire/ignite";
        public const string SFX_FIRE_BURN = "event:/Mods/pusheen/fire/burn";
        public const string SFX_FIRE_EXTINGUISH = "event:/Mods/pusheen/fire/extinguish";
        public const string SFX_FIRE_EXPLOSION = "event:/Mods/pusheen/fire/explosion";
        public const string SFX_FIRE_WHOOSH = "event:/Mods/pusheen/fire/whoosh";

        static FireEffects()
        {
            InitializeParticles();
        }

        private static void InitializeParticles()
        {
            // Standard fire particles
            P_Fire = new ParticleType
            {
                Size = 1f,
                Color = Color.OrangeRed,
                Color2 = Color.Yellow,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 0.5f,
                Acceleration = new Vector2(0f, -30f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            // Ember particles
            P_Ember = new ParticleType
            {
                Size = 0.5f,
                Color = Color.Orange,
                Color2 = Color.Red,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 10f,
                SpeedMax = 25f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -15f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            // Flame particles (larger, more intense)
            P_Flame = new ParticleType
            {
                Size = 1.5f,
                Color = Color.Red,
                Color2 = Color.Orange,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.4f,
                SpeedMin = 30f,
                SpeedMax = 50f,
                DirectionRange = (float)Math.PI * 0.3f,
                Acceleration = new Vector2(0f, -40f),
                SpinMin = -3f,
                SpinMax = 3f
            };

            // Explosion particles
            P_Explosion = new ParticleType
            {
                Size = 2f,
                Color = Color.Yellow,
                Color2 = Color.Red,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                SpeedMin = 50f,
                SpeedMax = 100f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 20f),
                SpinMin = -5f,
                SpinMax = 5f
            };

            // Smoke particles
            P_Smoke = new ParticleType
            {
                Size = 1.2f,
                Color = Color.Gray,
                Color2 = Color.DarkGray,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 2.0f,
                LifeMax = 3.0f,
                SpeedMin = 5f,
                SpeedMax = 15f,
                DirectionRange = (float)Math.PI * 0.4f,
                Acceleration = new Vector2(0f, -20f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            // Blue fire (hotter)
            P_BlueFire = new ParticleType
            {
                Size = 1f,
                Color = Color.CornflowerBlue,
                Color2 = Color.LightBlue,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.5f,
                LifeMax = 1.0f,
                SpeedMin = 25f,
                SpeedMax = 45f,
                DirectionRange = (float)Math.PI * 0.4f,
                Acceleration = new Vector2(0f, -35f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            // Green fire (magical)
            P_GreenFire = new ParticleType
            {
                Size = 1f,
                Color = Color.LimeGreen,
                Color2 = Color.Green,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.7f,
                LifeMax = 1.3f,
                SpeedMin = 20f,
                SpeedMax = 35f,
                DirectionRange = (float)Math.PI * 0.5f,
                Acceleration = new Vector2(0f, -25f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            // White fire (pure/holy)
            P_WhiteFire = new ParticleType
            {
                Size = 1f,
                Color = Color.White,
                Color2 = Color.LightYellow,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                SpeedMin = 15f,
                SpeedMax = 30f,
                DirectionRange = (float)Math.PI * 0.6f,
                Acceleration = new Vector2(0f, -20f),
                SpinMin = -1f,
                SpinMax = 1f
            };
        }

        /// <summary>
        /// Create a fire burst effect at the specified position
        /// </summary>
        public static void CreateFireBurst(Level level, Vector2 position, int particleCount = 20, ParticleType fireType = null)
        {
            if (fireType == null) fireType = P_Fire;
            
            Audio.Play(SFX_FIRE_IGNITE, position);
            level.ParticlesFG.Emit(fireType, particleCount, position, Vector2.One * 8f);
            
            // Add some embers
            level.ParticlesFG.Emit(P_Ember, particleCount / 2, position, Vector2.One * 12f);
            
            // Add smoke
            level.ParticlesBG.Emit(P_Smoke, particleCount / 3, position, Vector2.One * 10f);
        }

        /// <summary>
        /// Create a fire explosion effect
        /// </summary>
        public static void CreateFireExplosion(Level level, Vector2 position, float radius = 32f)
        {
            Audio.Play(SFX_FIRE_EXPLOSION, position);
            
            // Explosion particles
            level.ParticlesFG.Emit(P_Explosion, 30, position, Vector2.One * radius);
            
            // Follow-up fire
            level.ParticlesFG.Emit(P_Fire, 20, position, Vector2.One * radius * 0.8f);
            
            // Embers flying out
            level.ParticlesFG.Emit(P_Ember, 15, position, Vector2.One * radius * 1.2f);
            
            // Smoke cloud
            level.ParticlesBG.Emit(P_Smoke, 25, position, Vector2.One * radius * 1.5f);
            
            // Screen shake
            level.Shake(0.3f);
            
            // Damage nearby entities
            DamageNearbyEntities(level, position, radius);
        }

        /// <summary>
        /// Create a continuous fire trail effect
        /// </summary>
        public static void CreateFireTrail(Level level, Vector2 start, Vector2 end, int segments = 10)
        {
            Audio.Play(SFX_FIRE_WHOOSH, start);
            
            Vector2 direction = (end - start) / segments;
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 position = start + direction * i;
                level.ParticlesFG.Emit(P_Fire, 3, position, Vector2.One * 4f);
                
                if (i % 2 == 0)
                {
                    level.ParticlesFG.Emit(P_Ember, 2, position, Vector2.One * 6f);
                }
            }
        }

        /// <summary>
        /// Create a fire pillar effect
        /// </summary>
        public static void CreateFirePillar(Level level, Vector2 position, float height = 64f, float duration = 2f)
        {
            Audio.Play(SFX_FIRE_BURN, position);
            
            // Create multiple fire bursts along the pillar height
            int segments = (int)(height / 8f);
            Vector2 step = Vector2.UnitY * -8f;
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 segmentPos = position + step * i;
                level.ParticlesFG.Emit(P_Flame, 5, segmentPos, Vector2.One * 6f);
                
                // Delayed embers
                if (i % 3 == 0)
                {
                    level.ParticlesFG.Emit(P_Ember, 3, segmentPos, Vector2.One * 8f);
                }
            }
            
            // Smoke at the top
            Vector2 topPos = position + Vector2.UnitY * -height;
            level.ParticlesBG.Emit(P_Smoke, 10, topPos, Vector2.One * 12f);
        }

        /// <summary>
        /// Create magical fire effects with different colors
        /// </summary>
        public static void CreateMagicalFire(Level level, Vector2 position, FireType fireType, int intensity = 15)
        {
            ParticleType particleType = fireType switch
            {
                FireType.Blue => P_BlueFire,
                FireType.Green => P_GreenFire,
                FireType.White => P_WhiteFire,
                _ => P_Fire
            };

            Audio.Play(SFX_FIRE_IGNITE, position);
            level.ParticlesFG.Emit(particleType, intensity, position, Vector2.One * 10f);
            
            // Add sparkle effect for magical fires
            if (fireType != FireType.Normal)
            {
                level.ParticlesFG.Emit(P_Ember, intensity / 2, position, Vector2.One * 8f);
            }
        }

        /// <summary>
        /// Create a fire shield effect around an entity
        /// </summary>
        public static void CreateFireShield(Level level, Vector2 center, float radius = 24f, int particleCount = 12)
        {
            float angleStep = (float)(Math.PI * 2) / particleCount;
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = angleStep * i;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 position = center + offset;
                
                level.ParticlesFG.Emit(P_Fire, 2, position, Vector2.One * 3f);
                
                if (i % 3 == 0)
                {
                    level.ParticlesFG.Emit(P_Ember, 1, position, Vector2.One * 2f);
                }
            }
        }

        /// <summary>
        /// Apply burning damage to nearby entities
        /// </summary>
        private static void DamageNearbyEntities(Level level, Vector2 center, float radius)
        {
            foreach (Entity entity in level.Entities)
            {
                if (entity is Actor actor && Vector2.Distance(entity.Position, center) <= radius)
                {
                    // Apply fire damage
                    ApplyFireDamage(actor, center);
                }
            }
        }

        /// <summary>
        /// Apply fire damage to a specific entity
        /// </summary>
        private static void ApplyFireDamage(Actor actor, Vector2 fireCenter)
        {
            // Different entities take different fire damage
            if (actor is global::Celeste.Player player)
            {
                // Player takes damage and gets knockback
                Vector2 knockback = (player.Position - fireCenter).SafeNormalize() * 120f;
                player.Die(knockback);
            }
            else if (actor.GetType().Name.Contains("Enemy"))
            {
                // Damage enemy entities
                actor.RemoveSelf();
                
                // Create death particles
                actor.Scene.Add(new FireDeathEffect(actor.Position));
            }
        }
    }

    /// <summary>
    /// Fire type enumeration for different colored fires
    /// </summary>
    public enum FireType
    {
        Normal,     // Orange/Red fire
        Blue,       // Hot blue fire
        Green,      // Magical green fire
        White       // Pure/Holy white fire
    }

    /// <summary>
    /// Special effect entity for fire-related death animations
    /// </summary>
    public class FireDeathEffect : Entity
    {
        private float timer;
        private const float Duration = 1f;

        public FireDeathEffect(Vector2 position) : base(position)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            Level level = scene as Level;
            FireEffects.CreateFireBurst(level, Position, 15);
            
            Audio.Play(FireEffects.SFX_FIRE_IGNITE, Position);
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            
            if (timer >= Duration)
            {
                RemoveSelf();
            }
            else
            {
                // Emit occasional embers
                if (Scene.OnInterval(0.1f))
                {
                    (Scene as Level)?.ParticlesFG.Emit(FireEffects.P_Ember, 1, Position, Vector2.One * 4f);
                }
            }
        }
    }
}



