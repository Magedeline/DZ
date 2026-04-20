namespace Celeste.Effects
{
    /// <summary>
    /// Advanced ice elemental effects with freezing, crystallization, and snow
    /// </summary>
    [HotReloadable]
    public static class IceEffects
    {
        // Ice particle types
        public static ParticleType P_Ice;
        public static ParticleType P_Snow;
        public static ParticleType P_Frost;
        public static ParticleType P_Crystal;
        public static ParticleType P_Blizzard;
        public static ParticleType P_Icicle;
        public static ParticleType P_FreezeShatter;
        public static ParticleType P_MistyCold;

        // Audio events
        public const string SFX_ICE_FREEZE = "event:/desolozantas/ice/freeze";
        public const string SFX_ICE_SHATTER = "event:/desolozantas/ice/shatter";
        public const string SFX_ICE_CRYSTALLIZE = "event:/desolozantas/ice/crystallize";
        public const string SFX_ICE_BLIZZARD = "event:/desolozantas/ice/blizzard";
        public const string SFX_ICE_CRACK = "event:/desolozantas/ice/crack";

        private static readonly Dictionary<Entity, FreezeEffect> frozenEntities = new Dictionary<Entity, FreezeEffect>();

        static IceEffects()
        {
            InitializeParticles();
        }

        private static void InitializeParticles()
        {
            // Standard ice particles
            P_Ice = new ParticleType
            {
                Size = 1f,
                Color = Color.LightBlue,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 1.8f,
                SpeedMin = 10f,
                SpeedMax = 25f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 15f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            // Snow particles
            P_Snow = new ParticleType
            {
                Size = 0.7f,
                Color = Color.White,
                Color2 = Color.LightGray,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 2.0f,
                LifeMax = 4.0f,
                SpeedMin = 5f,
                SpeedMax = 15f,
                DirectionRange = (float)Math.PI * 0.3f,
                Acceleration = new Vector2(0f, 10f),
                SpinMin = -0.5f,
                SpinMax = 0.5f
            };

            // Frost particles
            P_Frost = new ParticleType
            {
                Size = 0.8f,
                Color = Color.PaleTurquoise,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                SpeedMin = 20f,
                SpeedMax = 35f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 20f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            // Crystal particles
            P_Crystal = new ParticleType
            {
                Size = 1.2f,
                Color = Color.CornflowerBlue,
                Color2 = Color.LightBlue,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 1.5f,
                LifeMax = 2.5f,
                SpeedMin = 8f,
                SpeedMax = 18f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 5f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            // Blizzard particles (high intensity)
            P_Blizzard = new ParticleType
            {
                Size = 1f,
                Color = Color.White,
                Color2 = Color.LightBlue,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.6f,
                LifeMax = 1.2f,
                SpeedMin = 30f,
                SpeedMax = 60f,
                DirectionRange = (float)Math.PI * 0.8f,
                Acceleration = new Vector2(-20f, 25f), // Wind effect
                SpinMin = -3f,
                SpinMax = 3f
            };

            // Icicle particles
            P_Icicle = new ParticleType
            {
                Size = 1.5f,
                Color = Color.LightBlue,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 15f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 0.5f,
                Acceleration = new Vector2(0f, 30f),
                SpinMin = -1f,
                SpinMax = 1f
            };

            // Freeze shatter particles
            P_FreezeShatter = new ParticleType
            {
                Size = 0.8f,
                Color = Color.LightBlue,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 50f),
                SpinMin = -5f,
                SpinMax = 5f
            };

            // Misty cold air
            P_MistyCold = new ParticleType
            {
                Size = 1.5f,
                Color = Color.LightBlue * 0.6f,
                Color2 = Color.White * 0.4f,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 2.5f,
                LifeMax = 4.0f,
                SpeedMin = 3f,
                SpeedMax = 10f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -5f),
                SpinMin = -0.5f,
                SpinMax = 0.5f
            };
        }

        /// <summary>
        /// Create an ice burst effect at the specified position
        /// </summary>
        public static void CreateIceBurst(Level level, Vector2 position, int particleCount = 20)
        {
            Audio.Play(SFX_ICE_FREEZE, position);
            level.ParticlesFG.Emit(P_Ice, particleCount, position, Vector2.One * 12f);
            level.ParticlesFG.Emit(P_Frost, particleCount / 2, position, Vector2.One * 8f);
            level.ParticlesBG.Emit(P_MistyCold, particleCount / 3, position, Vector2.One * 15f);
        }

        /// <summary>
        /// Create a freezing explosion with crystallization
        /// </summary>
        public static void CreateFreezeExplosion(Level level, Vector2 position, float radius = 32f)
        {
            Audio.Play(SFX_ICE_CRYSTALLIZE, position);
            
            // Crystal formation
            level.ParticlesFG.Emit(P_Crystal, 25, position, Vector2.One * radius);
            
            // Ice shards
            level.ParticlesFG.Emit(P_Ice, 20, position, Vector2.One * radius * 0.8f);
            
            // Frost spread
            level.ParticlesFG.Emit(P_Frost, 30, position, Vector2.One * radius * 1.2f);
            
            // Cold mist
            level.ParticlesBG.Emit(P_MistyCold, 15, position, Vector2.One * radius * 1.5f);
            
            // Freeze nearby entities
            FreezeNearbyEntities(level, position, radius);
        }

        /// <summary>
        /// Create a snowfall effect
        /// </summary>
        public static void CreateSnowfall(Level level, Vector2 area, int intensity = 10)
        {
            // Create snow particles across the area
            for (int i = 0; i < intensity; i++)
            {
                Vector2 snowPos = area + new Vector2(
                    Calc.Random.Range(-area.X, area.X),
                    Calc.Random.Range(-area.Y, area.Y)
                );
                
                level.ParticlesBG.Emit(P_Snow, 1, snowPos, Vector2.One * 4f);
            }
        }

        /// <summary>
        /// Create a blizzard effect
        /// </summary>
        public static void CreateBlizzard(Level level, Vector2 center, float radius = 64f, float duration = 3f)
        {
            Audio.Play(SFX_ICE_BLIZZARD, center);
            
            // High-intensity wind and snow
            level.ParticlesFG.Emit(P_Blizzard, 40, center, Vector2.One * radius);
            level.ParticlesFG.Emit(P_Snow, 30, center, Vector2.One * radius * 1.2f);
            level.ParticlesBG.Emit(P_MistyCold, 20, center, Vector2.One * radius * 1.5f);
            
            // Screen shake from the storm
            level.Shake(0.2f);
            
            // Slow down entities in the blizzard
            SlowNearbyEntities(level, center, radius, 0.5f, duration);
        }

        /// <summary>
        /// Create ice wall/barrier effect
        /// </summary>
        public static void CreateIceWall(Level level, Vector2 start, Vector2 end, float height = 32f)
        {
            Audio.Play(SFX_ICE_CRYSTALLIZE, start);
            
            Vector2 direction = (end - start).SafeNormalize();
            float distance = Vector2.Distance(start, end);
            int segments = (int)(distance / 8f);
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 segmentPos = start + direction * (i * 8f);
                
                // Create ice crystals along the wall
                level.ParticlesFG.Emit(P_Crystal, 5, segmentPos, Vector2.One * 4f);
                level.ParticlesFG.Emit(P_Ice, 3, segmentPos, Vector2.One * 6f);
                
                // Add height to the wall
                for (int h = 0; h < height / 8f; h++)
                {
                    Vector2 heightPos = segmentPos + Vector2.UnitY * (-h * 8f);
                    level.ParticlesFG.Emit(P_Frost, 2, heightPos, Vector2.One * 3f);
                }
            }
        }

        /// <summary>
        /// Create icicle spike attack
        /// </summary>
        public static void CreateIcicleSpikes(Level level, Vector2 origin, Vector2 direction, int spikeCount = 5)
        {
            Audio.Play(SFX_ICE_CRACK, origin);
            
            Vector2 step = direction * 16f;
            
            for (int i = 0; i < spikeCount; i++)
            {
                Vector2 spikePos = origin + step * i;
                
                // Create icicle particles
                level.ParticlesFG.Emit(P_Icicle, 8, spikePos, Vector2.One * 6f);
                level.ParticlesFG.Emit(P_Ice, 4, spikePos, Vector2.One * 4f);
                
                // Damage entities at spike position
                DamageAtPosition(level, spikePos, 12f);
            }
        }

        /// <summary>
        /// Freeze an entity temporarily
        /// </summary>
        public static void FreezeEntity(Entity entity, float duration = 2f)
        {
            if (frozenEntities.ContainsKey(entity)) return;

            Audio.Play(SFX_ICE_FREEZE, entity.Position);
            
            var freezeEffect = new FreezeEffect(entity, duration);
            frozenEntities[entity] = freezeEffect;
            
            // Visual freeze effect
            if (entity.Scene is Level level)
            {
                level.ParticlesFG.Emit(P_Ice, 15, entity.Position, Vector2.One * 8f);
                level.ParticlesFG.Emit(P_Crystal, 5, entity.Position, Vector2.One * 6f);
            }
        }

        /// <summary>
        /// Unfreeze an entity
        /// </summary>
        public static void UnfreezeEntity(Entity entity)
        {
            if (!frozenEntities.ContainsKey(entity)) return;

            Audio.Play(SFX_ICE_SHATTER, entity.Position);
            
            frozenEntities[entity].Break();
            frozenEntities.Remove(entity);
            
            // Visual unfreeze effect
            if (entity.Scene is Level level)
            {
                level.ParticlesFG.Emit(P_FreezeShatter, 20, entity.Position, Vector2.One * 10f);
            }
        }

        /// <summary>
        /// Create ice shield around an entity
        /// </summary>
        public static void CreateIceShield(Level level, Vector2 center, float radius = 28f, int particleCount = 15)
        {
            float angleStep = (float)(Math.PI * 2) / particleCount;
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = angleStep * i;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 position = center + offset;
                
                level.ParticlesFG.Emit(P_Crystal, 2, position, Vector2.One * 3f);
                
                if (i % 2 == 0)
                {
                    level.ParticlesFG.Emit(P_Ice, 1, position, Vector2.One * 2f);
                }
            }
        }

        /// <summary>
        /// Freeze entities in a radius
        /// </summary>
        private static void FreezeNearbyEntities(Level level, Vector2 center, float radius)
        {
            foreach (Entity entity in level.Entities)
            {
                if (Vector2.Distance(entity.Position, center) <= radius)
                {
                    FreezeEntity(entity, 3f);
                }
            }
        }

        /// <summary>
        /// Slow down entities in a radius
        /// </summary>
        private static void SlowNearbyEntities(Level level, Vector2 center, float radius, float slowFactor, float duration)
        {
            foreach (Entity entity in level.Entities)
            {
                if (Vector2.Distance(entity.Position, center) <= radius && entity is Actor actor)
                {
                    // Apply slow effect (this would need to be implemented per entity type)
                    ApplySlowEffect(actor, slowFactor, duration);
                }
            }
        }

        /// <summary>
        /// Apply ice damage at a specific position
        /// </summary>
        private static void DamageAtPosition(Level level, Vector2 position, float radius)
        {
            foreach (Entity entity in level.Entities)
            {
                if (Vector2.Distance(entity.Position, position) <= radius)
                {
                    ApplyIceDamage(entity, position);
                }
            }
        }

        /// <summary>
        /// Apply ice damage to a specific entity
        /// </summary>
        private static void ApplyIceDamage(Entity entity, Vector2 iceCenter)
        {
            if (entity is global::Celeste.Player player)
            {
                // Freeze player briefly then damage
                FreezeEntity(player, 1f);
                
                // Add a delayed damage effect
                player.Scene.Add(new IceDamageDelay(player, 1f));
            }
            else if (entity.GetType().Name.Contains("Enemy"))
            {
                FreezeEntity(entity, 2f);
            }
        }

        /// <summary>
        /// Apply slow effect to an actor
        /// </summary>
        private static void ApplySlowEffect(Actor actor, float slowFactor, float duration)
        {
            // This would need entity-specific implementation
            // For now, just add a visual effect
            if (actor.Scene is Level level)
            {
                level.ParticlesFG.Emit(P_Frost, 3, actor.Position, Vector2.One * 4f);
            }
        }

        private static float lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 0.1f; // Update every 100ms instead of every frame

        /// <summary>
        /// Update frozen entities (call this from the level update) - Optimized
        /// </summary>
        public static void UpdateFrozenEntities()
        {
            // Throttle updates to reduce performance impact
            float currentTime = Engine.Scene.TimeActive;
            if (currentTime - lastUpdateTime < UPDATE_INTERVAL)
                return;
                
            lastUpdateTime = currentTime;
            
            var toRemove = new List<Entity>();
            
            foreach (var kvp in frozenEntities)
            {
                if (kvp.Key.Scene == null || kvp.Value.Update())
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (var entity in toRemove)
            {
                if (frozenEntities.ContainsKey(entity))
                {
                    UnfreezeEntity(entity);
                }
            }
        }

        /// <summary>
        /// Clear all frozen entity state. Call on level transitions to prevent leaks.
        /// </summary>
        public static void ClearAll()
        {
            frozenEntities.Clear();
            lastUpdateTime = 0f;
        }
    }

    /// <summary>
    /// Represents a frozen entity state
    /// </summary>
    public class FreezeEffect
    {
        public Entity Entity { get; }
        public float Duration { get; }
        
        private float timer;
        private Vector2 originalPosition;
        private bool isFrozen;

        public FreezeEffect(Entity entity, float duration)
        {
            Entity = entity;
            Duration = duration;
            originalPosition = entity.Position;
            isFrozen = true;
        }

        public bool Update()
        {
            if (!isFrozen) return true;
            
            timer += Engine.DeltaTime;
            
            // Keep entity frozen in place
            if (Entity is Actor actor)
            {
                actor.Position = originalPosition;
            }
            
            // Emit frost particles occasionally
            if (Entity.Scene is Level level && level.OnInterval(0.2f))
            {
                level.ParticlesFG.Emit(IceEffects.P_Frost, 1, Entity.Position, Vector2.One * 4f);
            }
            
            return timer >= Duration;
        }

        public void Break()
        {
            isFrozen = false;
        }
    }

    /// <summary>
    /// Delayed ice damage effect
    /// </summary>
    public class IceDamageDelay : Entity
    {
        private Entity target;
        private float delay;
        private float timer;

        public IceDamageDelay(Entity target, float delay) : base(target.Position)
        {
            this.target = target;
            this.delay = delay;
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            
            if (timer >= delay)
            {
                if (target is global::Celeste.Player player)
                {
                    Vector2 knockback = Vector2.UnitY * -100f;
                    player.Die(knockback);
                }
                
                RemoveSelf();
            }
        }
    }
}



