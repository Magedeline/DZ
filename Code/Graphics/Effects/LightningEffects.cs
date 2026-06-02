namespace Celeste.Effects
{
    /// <summary>
    /// Advanced lightning elemental effects with electrical discharge, chain lightning, and electromagnetic fields
    /// </summary>
    [HotReloadable]
    public static class LightningEffects
    {
        // Lightning particle types
        public static ParticleType P_Lightning;
        public static ParticleType P_Spark;
        public static ParticleType P_ElectricField;
        public static ParticleType P_Plasma;
        public static ParticleType P_StaticCharge;
        public static ParticleType P_ElectricArc;
        public static ParticleType P_Discharge;
        public static ParticleType P_Electromagnetic;

        // Audio events
        public const string SFX_LIGHTNING_ZAP = "event:/pusheen/lightning/zap";
        public const string SFX_LIGHTNING_THUNDER = "event:/pusheen/lightning/thunder";
        public const string SFX_LIGHTNING_CRACKLE = "event:/pusheen/lightning/crackle";
        public const string SFX_LIGHTNING_DISCHARGE = "event:/pusheen/lightning/discharge";
        public const string SFX_LIGHTNING_STATIC = "event:/pusheen/lightning/static";

        private static readonly Dictionary<Entity, ElectricalEffect> chargedEntities = new Dictionary<Entity, ElectricalEffect>();

        static LightningEffects()
        {
            InitializeParticles();
        }

        private static void InitializeParticles()
        {
            // Lightning bolt particles
            P_Lightning = new ParticleType
            {
                Size = 1f,
                Color = Color.Yellow,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.1f,
                LifeMax = 0.3f,
                SpeedMin = 80f,
                SpeedMax = 150f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.Zero,
                SpinMin = -10f,
                SpinMax = 10f
            };

            // Electric sparks
            P_Spark = new ParticleType
            {
                Size = 0.5f,
                Color = Color.LightYellow,
                Color2 = Color.Yellow,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.2f,
                LifeMax = 0.6f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, 20f),
                SpinMin = -5f,
                SpinMax = 5f
            };

            // Electric field
            P_ElectricField = new ParticleType
            {
                Size = 1.2f,
                Color = Color.CornflowerBlue,
                Color2 = Color.LightBlue,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.0f,
                LifeMax = 2.0f,
                SpeedMin = 5f,
                SpeedMax = 15f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.Zero,
                SpinMin = -2f,
                SpinMax = 2f
            };

            // Plasma particles
            P_Plasma = new ParticleType
            {
                Size = 1.5f,
                Color = Color.Magenta,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.5f,
                LifeMax = 1.0f,
                SpeedMin = 20f,
                SpeedMax = 40f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.Zero,
                SpinMin = -3f,
                SpinMax = 3f
            };

            // Static charge buildup
            P_StaticCharge = new ParticleType
            {
                Size = 0.8f,
                Color = Color.LightYellow,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.5f,
                SpeedMin = 10f,
                SpeedMax = 25f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = new Vector2(0f, -10f),
                SpinMin = -2f,
                SpinMax = 2f
            };

            // Electric arc
            P_ElectricArc = new ParticleType
            {
                Size = 1f,
                Color = Color.Cyan,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.3f,
                LifeMax = 0.7f,
                SpeedMin = 60f,
                SpeedMax = 120f,
                DirectionRange = (float)Math.PI * 0.5f,
                Acceleration = Vector2.Zero,
                SpinMin = -8f,
                SpinMax = 8f
            };

            // Lightning discharge
            P_Discharge = new ParticleType
            {
                Size = 2f,
                Color = Color.Yellow,
                Color2 = Color.Orange,
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.InAndOut,
                LifeMin = 0.2f,
                LifeMax = 0.5f,
                SpeedMin = 100f,
                SpeedMax = 200f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.Zero,
                SpinMin = -15f,
                SpinMax = 15f
            };

            // Electromagnetic field
            P_Electromagnetic = new ParticleType
            {
                Size = 1f,
                Color = Color.Purple,
                Color2 = Color.Blue,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 1.5f,
                LifeMax = 3.0f,
                SpeedMin = 8f,
                SpeedMax = 20f,
                DirectionRange = (float)Math.PI * 2f,
                Acceleration = Vector2.Zero,
                SpinMin = -1f,
                SpinMax = 1f
            };
        }

        /// <summary>
        /// Create a lightning strike effect
        /// </summary>
        public static void CreateLightningStrike(Level level, Vector2 start, Vector2 end)
        {
            Audio.Play(SFX_LIGHTNING_ZAP, start);
            Audio.Play(SFX_LIGHTNING_THUNDER, start);

            // Create main lightning bolt
            CreateLightningBolt(level, start, end);
            
            // Create branching lightning
            CreateBranchingLightning(level, start, end, 3);
            
            // Impact effects
            level.ParticlesFG.Emit(P_Discharge, 20, end, Vector2.One * 16f);
            level.ParticlesFG.Emit(P_Spark, 30, end, Vector2.One * 12f);
            
            // Screen flash
            level.Flash(Color.White * 0.5f, true);
            level.Shake(0.4f);
            
            // Damage along the bolt path
            DamageAlongLine(level, start, end, 16f);
        }

        /// <summary>
        /// Create a lightning bolt between two points
        /// </summary>
        public static void CreateLightningBolt(Level level, Vector2 start, Vector2 end)
        {
            Vector2 direction = (end - start);
            int segments = (int)(direction.Length() / 8f);
            Vector2 step = direction / segments;
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 position = start + step * i;
                
                // Add randomness to make it look jagged
                position += new Vector2(
                    Calc.Random.Range(-4f, 4f),
                    Calc.Random.Range(-4f, 4f)
                );
                
                level.ParticlesFG.Emit(P_Lightning, 2, position, Vector2.One * 3f);
                
                if (i % 2 == 0)
                {
                    level.ParticlesFG.Emit(P_ElectricArc, 1, position, Vector2.One * 2f);
                }
            }
        }

        /// <summary>
        /// Create branching lightning effects
        /// </summary>
        public static void CreateBranchingLightning(Level level, Vector2 start, Vector2 end, int branches)
        {
            Vector2 mainDirection = (end - start);
            float mainLength = mainDirection.Length();
            
            for (int i = 0; i < branches; i++)
            {
                // Create branch at random point along main bolt
                float t = Calc.Random.Range(0.2f, 0.8f);
                Vector2 branchStart = start + mainDirection * t;
                
                // Random branch direction
                float angle = Calc.Random.Range(-(float)Math.PI * 0.5f, (float)Math.PI * 0.5f);
                Vector2 branchDirection = Vector2.Transform(mainDirection.SafeNormalize(), Matrix.CreateRotationZ(angle));
                Vector2 branchEnd = branchStart + branchDirection * Calc.Random.Range(mainLength * 0.2f, mainLength * 0.6f);
                
                CreateLightningBolt(level, branchStart, branchEnd);
            }
        }

        /// <summary>
        /// Create chain lightning that jumps between entities
        /// </summary>
        public static void CreateChainLightning(Level level, Vector2 origin, float maxRange = 64f, int maxChains = 5)
        {
            Audio.Play(SFX_LIGHTNING_CRACKLE, origin);
            
            HashSet<Entity> targets = new HashSet<Entity>();
            Vector2 currentPos = origin;
            
            // Find nearby entities to chain to
            for (int chain = 0; chain < maxChains; chain++)
            {
                Entity nearestTarget = FindNearestTarget(level, currentPos, maxRange, targets);
                if (nearestTarget == null) break;
                
                // Create lightning between current position and target
                CreateLightningBolt(level, currentPos, nearestTarget.Position);
                
                // Damage the target
                ApplyLightningDamage(nearestTarget, currentPos);
                
                // Add sparks at target
                level.ParticlesFG.Emit(P_Spark, 10, nearestTarget.Position, Vector2.One * 8f);
                
                targets.Add(nearestTarget);
                currentPos = nearestTarget.Position;
                maxRange *= 0.8f; // Reduce range for each chain
            }
        }

        /// <summary>
        /// Create an electric field area effect
        /// </summary>
        public static void CreateElectricField(Level level, Vector2 center, float radius = 48f, float duration = 3f)
        {
            Audio.Play(SFX_LIGHTNING_STATIC, center);
            
            // Create the electric field entity
            level.Add(new ElectricFieldEntity(center, radius, duration));
            
            // Initial field setup particles
            level.ParticlesFG.Emit(P_ElectricField, 25, center, Vector2.One * radius);
            level.ParticlesFG.Emit(P_Electromagnetic, 15, center, Vector2.One * radius * 1.2f);
        }

        /// <summary>
        /// Create plasma ball effect
        /// </summary>
        public static void CreatePlasmaBall(Level level, Vector2 center, float radius = 24f)
        {
            Audio.Play(SFX_LIGHTNING_DISCHARGE, center);
            
            // Dense plasma core
            level.ParticlesFG.Emit(P_Plasma, 20, center, Vector2.One * radius * 0.5f);
            
            // Electric field around it
            level.ParticlesFG.Emit(P_ElectricField, 15, center, Vector2.One * radius);
            
            // Sparks flying out
            level.ParticlesFG.Emit(P_Spark, 25, center, Vector2.One * radius * 1.5f);
            
            // Electromagnetic disturbance
            level.ParticlesBG.Emit(P_Electromagnetic, 10, center, Vector2.One * radius * 2f);
        }

        /// <summary>
        /// Charge an entity with static electricity
        /// </summary>
        public static void ChargeEntity(Entity entity, float chargeLevel = 1f, float duration = 5f)
        {
            if (chargedEntities.ContainsKey(entity))
            {
                chargedEntities[entity].AddCharge(chargeLevel);
                return;
            }

            Audio.Play(SFX_LIGHTNING_STATIC, entity.Position);
            
            var electricalEffect = new ElectricalEffect(entity, chargeLevel, duration);
            chargedEntities[entity] = electricalEffect;
            
            // Visual charge effect
            if (entity.Scene is Level level)
            {
                level.ParticlesFG.Emit(P_StaticCharge, 10, entity.Position, Vector2.One * 8f);
            }
        }

        /// <summary>
        /// Discharge electricity from a charged entity
        /// </summary>
        public static void DischargeEntity(Entity entity)
        {
            if (!chargedEntities.ContainsKey(entity)) return;

            var effect = chargedEntities[entity];
            Audio.Play(SFX_LIGHTNING_DISCHARGE, entity.Position);
            
            if (entity.Scene is Level level)
            {
                // Create discharge effect
                level.ParticlesFG.Emit(P_Discharge, 15, entity.Position, Vector2.One * 12f);
                level.ParticlesFG.Emit(P_Spark, 20, entity.Position, Vector2.One * 16f);
                
                // Chain lightning to nearby entities
                CreateChainLightning(level, entity.Position, 32f, 3);
            }
            
            chargedEntities.Remove(entity);
        }

        /// <summary>
        /// Create electromagnetic pulse
        /// </summary>
        public static void CreateEMP(Level level, Vector2 center, float radius = 80f)
        {
            Audio.Play(SFX_LIGHTNING_THUNDER, center);
            
            // Expanding electromagnetic wave
            level.ParticlesFG.Emit(P_Electromagnetic, 40, center, Vector2.One * radius);
            level.ParticlesFG.Emit(P_ElectricField, 30, center, Vector2.One * radius * 0.8f);
            
            // Screen effects
            level.Flash(Color.Blue * 0.3f, true);
            level.Shake(0.5f);
            
            // Disable electronic entities in range
            DisableElectronicsInRange(level, center, radius);
        }

        /// <summary>
        /// Create electric shield around an entity
        /// </summary>
        public static void CreateElectricShield(Level level, Vector2 center, float radius = 32f, int sparkCount = 20)
        {
            float angleStep = (float)(Math.PI * 2) / sparkCount;
            
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = angleStep * i;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 position = center + offset;
                
                level.ParticlesFG.Emit(P_Spark, 2, position, Vector2.One * 3f);
                
                if (i % 3 == 0)
                {
                    level.ParticlesFG.Emit(P_ElectricArc, 1, position, Vector2.One * 2f);
                }
            }
        }

        /// <summary>
        /// Find the nearest entity to chain lightning to
        /// </summary>
        private static Entity FindNearestTarget(Level level, Vector2 position, float maxRange, HashSet<Entity> excludeSet)
        {
            Entity nearest = null;
            float nearestDistanceSq = maxRange * maxRange;
            
            foreach (Entity entity in level.Entities)
            {
                if (excludeSet.Contains(entity)) continue;
                
                float distanceSq = Vector2.DistanceSquared(entity.Position, position);
                if (distanceSq < nearestDistanceSq)
                {
                    nearest = entity;
                    nearestDistanceSq = distanceSq;
                }
            }
            
            return nearest;
        }

        /// <summary>
        /// Apply lightning damage along a line
        /// </summary>
        private static void DamageAlongLine(Level level, Vector2 start, Vector2 end, float width)
        {
            Vector2 direction = (end - start);
            int segments = (int)(direction.Length() / 8f);
            Vector2 step = direction / segments;
            
            for (int i = 0; i < segments; i++)
            {
                Vector2 position = start + step * i;
                DamageAtPosition(level, position, width);
            }
        }

        /// <summary>
        /// Apply lightning damage at a specific position
        /// </summary>
        private static void DamageAtPosition(Level level, Vector2 position, float radius)
        {
            float radiusSq = radius * radius;
            foreach (Entity entity in level.Entities)
            {
                if (Vector2.DistanceSquared(entity.Position, position) <= radiusSq)
                {
                    ApplyLightningDamage(entity, position);
                }
            }
        }

        /// <summary>
        /// Apply lightning damage to a specific entity
        /// </summary>
        private static void ApplyLightningDamage(Entity entity, Vector2 lightningCenter)
        {
            if (entity is global::Celeste.Player player)
            {
                // Stun player briefly then damage
                ChargeEntity(player, 2f, 1f);
                
                Vector2 knockback = (player.Position - lightningCenter).SafeNormalize() * 150f;
                player.Die(knockback);
            }
            else if (entity is global::Celeste.Entities.Enemy)
            {
                // Charge enemy and potentially chain to others
                ChargeEntity(entity, 1f, 2f);
                
                // Remove enemy with electric death effect
                entity.Scene.Add(new ElectricDeathEffect(entity.Position));
                entity.RemoveSelf();
            }
        }

        /// <summary>
        /// Disable electronic entities in range (placeholder)
        /// </summary>
        private static void DisableElectronicsInRange(Level level, Vector2 center, float radius)
        {
            float radiusSq = radius * radius;
            foreach (Entity entity in level.Entities)
            {
                if (Vector2.DistanceSquared(entity.Position, center) <= radiusSq)
                {
                    // This would disable specific electronic entities
                    // For now, just add visual effect
                    level.ParticlesFG.Emit(P_Spark, 5, entity.Position, Vector2.One * 6f);
                }
            }
        }

        /// <summary>
        /// Update all charged entities
        /// </summary>
        private static readonly List<Entity> toRemoveCharged = new List<Entity>();

        public static void UpdateChargedEntities()
        {
            toRemoveCharged.Clear();
            
            foreach (var kvp in chargedEntities)
            {
                if (kvp.Key.Scene == null || kvp.Value.Update())
                {
                    toRemoveCharged.Add(kvp.Key);
                }
            }
            
            foreach (var entity in toRemoveCharged)
            {
                chargedEntities.Remove(entity);
            }
        }

        /// <summary>
        /// Clear all charged entity state. Call on level transitions to prevent leaks.
        /// </summary>
        public static void ClearAll()
        {
            chargedEntities.Clear();
        }
    }

    /// <summary>
    /// Represents an electrical charge effect on an entity
    /// </summary>
    public class ElectricalEffect
    {
        public Entity Entity { get; }
        public float ChargeLevel { get; private set; }
        public float Duration { get; }
        
        private float timer;
        private float sparkTimer;

        public ElectricalEffect(Entity entity, float chargeLevel, float duration)
        {
            Entity = entity;
            ChargeLevel = chargeLevel;
            Duration = duration;
        }

        public void AddCharge(float amount)
        {
            ChargeLevel += amount;
        }

        public bool Update()
        {
            timer += Engine.DeltaTime;
            sparkTimer += Engine.DeltaTime;
            
            // Emit sparks periodically
            if (sparkTimer >= 0.3f && Entity.Scene is Level level)
            {
                int sparkCount = (int)(ChargeLevel * 2);
                level.ParticlesFG.Emit(LightningEffects.P_Spark, sparkCount, Entity.Position, Vector2.One * 6f);
                sparkTimer = 0f;
            }
            
            return timer >= Duration;
        }
    }

    /// <summary>
    /// Electric field area effect entity
    /// </summary>
    public class ElectricFieldEntity : Entity
    {
        private float radius;
        private float duration;
        private float timer;
        private float damageTimer;

        public ElectricFieldEntity(Vector2 position, float radius, float duration) : base(position)
        {
            this.radius = radius;
            this.duration = duration;
            
            Collider = new Circle(radius);
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            damageTimer += Engine.DeltaTime;
            
            // Emit field particles
            if (Scene.OnInterval(0.1f))
            {
                (Scene as Level)?.ParticlesFG.Emit(LightningEffects.P_ElectricField, 3, Position, Vector2.One * radius);
            }
            
            // Damage entities in field periodically
            if (damageTimer >= 0.5f)
            {
                DamageEntitiesInField();
                damageTimer = 0f;
            }
            
            if (timer >= duration)
            {
                RemoveSelf();
            }
        }

        private void DamageEntitiesInField()
        {
            foreach (Actor actor in CollideAll<Actor>())
            {
                LightningEffects.ChargeEntity(actor, 0.5f, 1f);
            }

            foreach (Solid solid in CollideAll<Solid>())
            {
                LightningEffects.ChargeEntity(solid, 0.5f, 1f);
            }
        }
    }

    /// <summary>
    /// Electric death effect for destroyed entities
    /// </summary>
    public class ElectricDeathEffect : Entity
    {
        private float timer;
        private const float Duration = 0.8f;

        public ElectricDeathEffect(Vector2 position) : base(position)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            Level level = scene as Level;
            LightningEffects.CreatePlasmaBall(level, Position, 16f);
            
            Audio.Play(LightningEffects.SFX_LIGHTNING_DISCHARGE, Position);
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
                // Emit occasional sparks
                if (Scene.OnInterval(0.1f))
                {
                    (Scene as Level)?.ParticlesFG.Emit(LightningEffects.P_Spark, 2, Position, Vector2.One * 6f);
                }
            }
        }
    }
}



