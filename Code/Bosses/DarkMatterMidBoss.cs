using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Dark Matter Mid Boss - A powerful void entity with enhanced attacks and phases
    /// Much stronger than regular Dark Matter enemies with multiple attack patterns
    /// Sprite path: characters/darkmatter_boss/
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/DarkMatterMidBoss")]
    [Tracked]
    [HotReloadable]
    public class DarkMatterMidBoss : BossActor
    {
        #region Constants
        
        private const int PHASE_1_HP = 50;
        private const int PHASE_2_HP = 30;
        private const float MOVEMENT_SPEED = 60f;
        private const float TELEPORT_COOLDOWN = 5f;
        private const float ATTACK_COOLDOWN = 1.5f;
        
        #endregion
        
        #region Fields
        
        // Combat
        private int currentHealth;
        private int maxHealth;
        private BossPhase currentPhase;
        private float invulnerabilityTimer;
        private const float INVULNERABILITY_DURATION = 0.8f;
        
        // Attack patterns
        private List<AttackType> phase1Attacks;
        private List<AttackType> phase2Attacks;
        private int attackIndex;
        private float attackCooldownTimer;
        private bool isAttacking;
        
        // Movement
        private List<Vector2> teleportNodes;
        private float teleportCooldownTimer;
        
        // Visual
        private Sprite bodySprite;
        private Sprite auraSprite;
        private VertexLight light;
        private float flashTimer;
        
        // Level reference
        private Level level;
        private global::Celeste.Player player;
        
        // Arena reference
        private VoidGateArena arena;
        
        #endregion
        
        #region Enums
        
        private enum BossPhase
        {
            Intro,
            Phase1,      // Standard attacks
            Transition,  // Phase change
            Phase2,      // Enhanced attacks, faster, more aggressive
            Defeated
        }
        
        private enum AttackType
        {
            DarkWave,           // Wave of dark energy
            VoidSphere,         // Spawns dark spheres that home in
            TeleportSlash,      // Teleports and slashes
            DarkRain,           // Rain of dark projectiles
            VoidPulse,          // AoE pulse attack
            DarkSpiral,         // Spiral of projectiles
            VoidStorm,          // Enhanced dark rain (Phase 2)
            DimensionalRift     // Creates rifts that damage (Phase 2)
        }
        
        #endregion
        
        #region Constructor
        
        public DarkMatterMidBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, 
                   spriteName: "characters/darkmatter_boss/body",
                   spriteScale: Vector2.One,
                   maxFall: 0f,  // Floats
                   collidable: true,
                   solidCollidable: false,
                   gravityMult: 0f,
                   collider: new Hitbox(24f, 24f, -12f, -12f))
        {
            maxHealth = data.Int("health", 80);
            currentHealth = maxHealth;
            currentPhase = BossPhase.Intro;
            
            SetupNodes(data);
            SetupAttackPatterns();
            SetupSprites();
            SetupLight();
            
            Add(new PlayerCollider(OnPlayerCollide));
            
            Depth = -10000;
        }
        
        #endregion
        
        #region Setup
        
        private void SetupNodes(EntityData data)
        {
            teleportNodes = new List<Vector2>();
            
            // Get nodes from entity data
            if (data.Nodes != null && data.Nodes.Length > 0)
            {
                foreach (var node in data.Nodes)
                {
                    teleportNodes.Add(node + data.Position);
                }
            }
            else
            {
                // Default nodes around spawn point
                for (int i = 0; i < 4; i++)
                {
                    float angle = (i / 4f) * (float)Math.PI * 2f;
                    teleportNodes.Add(Position + Calc.AngleToVector(angle, 100f));
                }
            }
        }
        
        private void SetupAttackPatterns()
        {
            phase1Attacks = new List<AttackType>
            {
                AttackType.DarkWave,
                AttackType.VoidSphere,
                AttackType.DarkRain,
                AttackType.VoidPulse,
                AttackType.TeleportSlash
            };
            
            phase2Attacks = new List<AttackType>
            {
                AttackType.VoidStorm,
                AttackType.DimensionalRift,
                AttackType.DarkSpiral,
                AttackType.TeleportSlash,
                AttackType.VoidPulse
            };
        }
        
        private void SetupSprites()
        {
            // Body sprite
            bodySprite = new Sprite(GFX.Game, "characters/darkmatter_boss/");
            bodySprite.AddLoop("idle", "idle", 0.1f);
            bodySprite.AddLoop("attack", "attack", 0.08f);
            bodySprite.Add("hurt", "hurt", 0.05f, "idle");
            bodySprite.Add("phase_transition", "transition", 0.1f, "idle");
            bodySprite.Add("defeat", "defeat", 0.1f);
            
            bodySprite.Play("idle");
            bodySprite.CenterOrigin();
            Add(bodySprite);
            
            // Aura sprite (overlay)
            auraSprite = new Sprite(GFX.Game, "characters/darkmatter_boss/");
            auraSprite.AddLoop("aura", "aura", 0.08f);
            auraSprite.Play("aura");
            auraSprite.CenterOrigin();
            Add(auraSprite);
        }
        
        private void SetupLight()
        {
            light = new VertexLight(Color.DarkViolet, 1f, 64, 128);
            Add(light);
        }
        
        #endregion
        
        #region Lifecycle
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            arena = scene.Tracker.GetEntity<VoidGateArena>();
            
            // Start intro
            Add(new Coroutine(IntroSequence()));
        }
        
        public override void Update()
        {
            base.Update();
            
            if (currentPhase == BossPhase.Defeated)
                return;
            
            // Update timers
            if (invulnerabilityTimer > 0)
                invulnerabilityTimer -= Engine.DeltaTime;
            
            if (attackCooldownTimer > 0)
                attackCooldownTimer -= Engine.DeltaTime;
            
            if (teleportCooldownTimer > 0)
                teleportCooldownTimer -= Engine.DeltaTime;
            
            if (flashTimer > 0)
            {
                flashTimer -= Engine.DeltaTime;
                bodySprite.Color = Color.Lerp(Color.White, Color.Purple, 1f - (flashTimer / 0.2f));
            }
            else
            {
                bodySprite.Color = Color.White;
            }
            
            // Get player reference
            if (player == null)
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            
            // Emit particles
            if (Scene.OnInterval(0.1f))
            {
                EmitDarkParticles();
            }
            
            // Combat logic
            if (currentPhase == BossPhase.Phase1 || currentPhase == BossPhase.Phase2)
            {
                UpdateCombat();
            }
        }
        
        #endregion
        
        #region Combat
        
        private void UpdateCombat()
        {
            if (isAttacking)
                return;
            
            // Teleport if cooldown ready and far from player
            if (player != null && teleportCooldownTimer <= 0)
            {
                float distance = Vector2.Distance(Position, player.Position);
                if (distance > 200f)
                {
                    TeleportToNearestNode();
                    teleportCooldownTimer = TELEPORT_COOLDOWN;
                    return;
                }
            }
            
            // Execute attacks
            if (attackCooldownTimer <= 0 && !isAttacking)
            {
                ExecuteNextAttack();
            }
        }
        
        private void ExecuteNextAttack()
        {
            List<AttackType> currentPattern = currentPhase == BossPhase.Phase1 ? phase1Attacks : phase2Attacks;
            
            AttackType attack = currentPattern[attackIndex];
            attackIndex = (attackIndex + 1) % currentPattern.Count;
            
            Add(new Coroutine(ExecuteAttack(attack)));
        }
        
        private IEnumerator ExecuteAttack(AttackType attack)
        {
            isAttacking = true;
            bodySprite.Play("attack");
            
            yield return 0.3f;
            
            switch (attack)
            {
                case AttackType.DarkWave:
                    yield return DarkWaveAttack();
                    break;
                case AttackType.VoidSphere:
                    yield return VoidSphereAttack();
                    break;
                case AttackType.TeleportSlash:
                    yield return TeleportSlashAttack();
                    break;
                case AttackType.DarkRain:
                    yield return DarkRainAttack();
                    break;
                case AttackType.VoidPulse:
                    yield return VoidPulseAttack();
                    break;
                case AttackType.DarkSpiral:
                    yield return DarkSpiralAttack();
                    break;
                case AttackType.VoidStorm:
                    yield return VoidStormAttack();
                    break;
                case AttackType.DimensionalRift:
                    yield return DimensionalRiftAttack();
                    break;
            }
            
            attackCooldownTimer = ATTACK_COOLDOWN * (currentPhase == BossPhase.Phase2 ? 0.7f : 1f);
            isAttacking = false;
            bodySprite.Play("idle");
        }
        
        #endregion
        
        #region Attack Implementations
        
        private IEnumerator DarkWaveAttack()
        {
            Audio.Play("event:/char/badeline/boss_bullet", Position);
            
            // Fire wave of projectiles
            for (int i = -2; i <= 2; i++)
            {
                Vector2 direction = player != null ? 
                    (player.Position - Position).SafeNormalize().Rotate(i * 0.3f) : 
                    Vector2.UnitX.Rotate(i * 0.3f);
                
                Scene.Add(new DarkEnergyProjectile(Position, direction, Calc.Random.Range(2, 5)));
            }
            
            yield return 0.5f;
        }
        
        private IEnumerator VoidSphereAttack()
        {
            Audio.Play("event:/char/badeline/boss_bullet", Position);
            
            // Spawn homing spheres
            for (int i = 0; i < 3; i++)
            {
                Scene.Add(new VoidSphere(Position, Calc.Random.Range(3, 6)));
                yield return 0.3f;
            }
        }
        
        private IEnumerator TeleportSlashAttack()
        {
            // Teleport behind player
            if (player != null)
            {
                TeleportEffect(Position);
                Position = player.Position + new Vector2(player.Facing == Facings.Right ? -40f : 40f, -20f);
                TeleportEffect(Position);
                
                Audio.Play("event:/char/badeline/disappear", Position);
                
                // Slash attack
                yield return 0.2f;
                Vector2 slashDir = (player.Position - Position).SafeNormalize();
                Scene.Add(new DarkEnergyProjectile(Position, slashDir, Calc.Random.Range(4, 8)));
            }
            
            yield return 0.5f;
        }
        
        private IEnumerator DarkRainAttack()
        {
            Audio.Play("event:/char/badeline/boss_bullet", Position);
            
            // Rain projectiles from above
            for (int i = 0; i < 8; i++)
            {
                if (player != null)
                {
                    Vector2 spawnPos = new Vector2(
                        player.X + Calc.Random.Range(-80f, 80f),
                        level.Bounds.Top - 20f
                    );
                    
                    Scene.Add(new DarkEnergyProjectile(spawnPos, Vector2.UnitY, Calc.Random.Range(2, 4)));
                }
                yield return 0.15f;
            }
        }
        
        private IEnumerator VoidPulseAttack()
        {
            Audio.Play("event:/char/badeline/boss_laser_charge", Position);
            
            // Charge up
            for (int i = 0; i < 10; i++)
            {
                EmitDarkParticles();
                yield return 0.05f;
            }
            
            Audio.Play("event:/char/badeline/boss_laser_fire", Position);
            level?.Shake(0.5f);
            
            // Create pulse
            Scene.Add(new VoidPulse(Position, Calc.Random.Range(5, 8)));
            
            yield return 0.5f;
        }
        
        private IEnumerator DarkSpiralAttack()
        {
            Audio.Play("event:/char/badeline/boss_bullet", Position);
            
            // Spiral of projectiles
            for (int i = 0; i < 16; i++)
            {
                float angle = (i / 16f) * (float)Math.PI * 2f + Scene.TimeActive * 2f;
                Vector2 direction = Calc.AngleToVector(angle, 1f);
                Scene.Add(new DarkEnergyProjectile(Position, direction, Calc.Random.Range(3, 6)));
                yield return 0.08f;
            }
        }
        
        private IEnumerator VoidStormAttack()
        {
            Audio.Play("event:/char/badeline/boss_bullet", Position);
            
            // Enhanced rain attack
            for (int i = 0; i < 15; i++)
            {
                if (player != null)
                {
                    Vector2 spawnPos = new Vector2(
                        player.X + Calc.Random.Range(-120f, 120f),
                        level.Bounds.Top - 20f
                    );
                    
                    Scene.Add(new DarkEnergyProjectile(spawnPos, Vector2.UnitY, Calc.Random.Range(4, 7)));
                }
                yield return 0.1f;
            }
        }
        
        private IEnumerator DimensionalRiftAttack()
        {
            Audio.Play("event:/char/badeline/boss_laser_charge", Position);
            
            // Create rifts at player positions
            for (int i = 0; i < 3; i++)
            {
                if (player != null)
                {
                    Scene.Add(new DimensionalRift(player.Position, Calc.Random.Range(4, 7)));
                }
                yield return 0.5f;
            }
        }
        
        #endregion
        
        #region Damage & Health
        
        public override void TakeDamage(int damage)
        {
            if (invulnerabilityTimer > 0 || currentPhase == BossPhase.Defeated)
                return;
            
            currentHealth -= damage;
            invulnerabilityTimer = INVULNERABILITY_DURATION;
            flashTimer = 0.2f;
            
            Audio.Play("event:/char/badeline/boss_bullet_impact", Position);
            level?.Shake(0.3f);
            
            // Hit particles
            for (int i = 0; i < 12; i++)
            {
                EmitDarkParticles();
            }
            
            // Phase transition
            if (currentHealth <= PHASE_2_HP && currentPhase == BossPhase.Phase1)
            {
                Add(new Coroutine(PhaseTransition()));
            }
            else if (currentHealth <= 0)
            {
                Add(new Coroutine(DefeatSequence()));
            }
            else
            {
                bodySprite.Play("hurt");
            }
        }
        
        #endregion
        
        #region Sequences
        
        private IEnumerator IntroSequence()
        {
            currentPhase = BossPhase.Intro;
            
            // Fade in effect
            for (float t = 0; t < 1f; t += Engine.DeltaTime)
            {
                bodySprite.Color = Color.White * t;
                auraSprite.Color = Color.White * t;
                yield return null;
            }
            
            Audio.Play("event:/char/badeline/appear", Position);
            
            // Screen shake
            level?.Shake(0.5f);
            
            // Particle burst
            for (int i = 0; i < 30; i++)
            {
                EmitDarkParticles();
            }
            
            yield return 1f;
            
            currentPhase = BossPhase.Phase1;
        }
        
        private IEnumerator PhaseTransition()
        {
            currentPhase = BossPhase.Transition;
            isAttacking = true;
            
            bodySprite.Play("phase_transition");
            Audio.Play("event:/char/badeline/boss_transform", Position);
            
            // Intense particles
            for (int i = 0; i < 50; i++)
            {
                EmitDarkParticles();
            }
            
            level?.Shake(1f);
            level?.Flash(Color.Purple);
            
            yield return 2f;
            
            // Phase 2 changes
            light.Color = Color.Lerp(Color.DarkViolet, Color.Red, 0.5f);
            auraSprite.Color = Color.Lerp(Color.White, Color.Red, 0.3f);
            
            currentPhase = BossPhase.Phase2;
            isAttacking = false;
            attackIndex = 0;
        }
        
        private IEnumerator DefeatSequence()
        {
            currentPhase = BossPhase.Defeated;
            Collidable = false;
            
            bodySprite.Play("defeat");
            Audio.Play("event:/char/badeline/disappear", Position);
            
            // Death explosion
            for (int i = 0; i < 60; i++)
            {
                EmitDarkParticles();
                if (i % 5 == 0)
                    level?.Shake(0.3f);
                yield return 0.05f;
            }
            
            level?.Shake(1f);
            level?.Flash(Color.Purple);
            
            // Notify arena
            arena?.OnBossDefeated(this);
            
            yield return 2f;
            
            RemoveSelf();
        }
        
        #endregion
        
        #region Movement
        
        private void TeleportToNearestNode()
        {
            if (teleportNodes.Count == 0)
                return;
            
            TeleportEffect(Position);
            
            // Find closest node to player
            Vector2 bestNode = teleportNodes[0];
            if (player != null)
            {
                float bestDistance = float.MaxValue;
                foreach (var node in teleportNodes)
                {
                    float distToPlayer = Vector2.Distance(node, player.Position);
                    if (distToPlayer < bestDistance && distToPlayer > 50f)
                    {
                        bestDistance = distToPlayer;
                        bestNode = node;
                    }
                }
            }
            else
            {
                bestNode = teleportNodes[Calc.Random.Next(teleportNodes.Count)];
            }
            
            Position = bestNode;
            TeleportEffect(Position);
            
            Audio.Play("event:/char/badeline/disappear", Position);
        }
        
        private void TeleportEffect(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                float angle = (i / 15f) * (float)Math.PI * 2f;
                Vector2 offset = Calc.AngleToVector(angle, 20f);
                level?.ParticlesFG.Emit(ParticleTypes.Dust, position + offset, Color.Purple);
            }
        }
        
        #endregion
        
        #region Collision
        
        private void OnPlayerCollide(global::Celeste.Player player)
        {
            if (currentPhase == BossPhase.Defeated)
                return;
            
            // Deal contact damage
            PlayerHealthManager.TryDamagePlayer(Calc.Random.Range(3, 6), Position);
            
            // Knockback
            Vector2 direction = (player.Position - Position).SafeNormalize();
            player.Speed = direction * 250f;
        }
        
        #endregion
        
        #region Particles
        
        private void EmitDarkParticles()
        {
            var particle = new ParticleType
            {
                Color = Color.Purple,
                Color2 = Color.DarkViolet,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                LifeMin = 0.3f,
                LifeMax = 0.8f,
                SpeedMin = 10f,
                SpeedMax = 30f
            };
            
            level?.ParticlesFG.Emit(particle, Position + Calc.Random.Range(Vector2.One * -12f, Vector2.One * 12f));
        }
        
        #endregion
        
        #region Rendering
        
        public override void Render()
        {
            // Draw glow
            Draw.Circle(Position, 32f, Color.Purple * 0.3f, 8);
            
            bodySprite.DrawOutline();
            base.Render();
        }
        
        #endregion
    }
    
    #region Additional Entities
    
    /// <summary>
    /// Homing void sphere
    /// </summary>
    [Tracked]
    public class VoidSphere : Entity
    {
        private Vector2 velocity;
        private int damage;
        private float lifetime;
        private Sprite sprite;
        private Level level;
        
        public VoidSphere(Vector2 position, int damage) : base(position)
        {
            this.damage = damage;
            Collider = new Hitbox(10f, 10f, -5f, -5f);
            
            sprite = new Sprite(GFX.Game, "characters/darkmatter/");
            sprite.AddLoop("sphere", "sphere", 0.08f);
            sprite.Play("sphere");
            sprite.CenterOrigin();
            Add(sprite);
            
            Add(new PlayerCollider(OnPlayerHit));
            Depth = -100;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }
        
        public override void Update()
        {
            base.Update();
            
            lifetime += Engine.DeltaTime;
            if (lifetime > 10f)
            {
                RemoveSelf();
                return;
            }
            
            // Home towards player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                velocity = Calc.Approach(velocity, direction * 100f, 200f * Engine.DeltaTime);
            }
            
            Position += velocity * Engine.DeltaTime;
            
            // Check collision with solids
            if (CollideCheck<Solid>())
            {
                Explode();
            }
        }
        
        private void OnPlayerHit(global::Celeste.Player player)
        {
            PlayerHealthManager.TryDamagePlayer(damage, Position);
            Explode();
        }
        
        private void Explode()
        {
            Audio.Play("event:/char/badeline/boss_bullet_impact", Position);
            RemoveSelf();
        }
        
        public override void Render()
        {
            sprite.DrawOutline();
            base.Render();
        }
    }
    
    /// <summary>
    /// Void pulse AoE attack
    /// </summary>
    [Tracked]
    public class VoidPulse : Entity
    {
        private float radius;
        private float maxRadius;
        private int damage;
        private bool hasHit;
        private Level level;
        
        public VoidPulse(Vector2 position, int damage) : base(position)
        {
            this.damage = damage;
            maxRadius = 150f;
            Depth = -50;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }
        
        public override void Update()
        {
            base.Update();
            
            radius += 400f * Engine.DeltaTime;
            
            if (!hasHit && radius > 50f)
            {
                // Check player collision
                var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null && Vector2.Distance(Position, player.Position) < radius)
                {
                    PlayerHealthManager.TryDamagePlayer(damage, Position);
                    hasHit = true;
                }
            }
            
            if (radius >= maxRadius)
            {
                RemoveSelf();
            }
        }
        
        public override void Render()
        {
            float alpha = 1f - (radius / maxRadius);
            Draw.Circle(Position, radius, Color.Purple * alpha, 16);
            Draw.Circle(Position, radius - 2f, Color.DarkViolet * alpha * 0.5f, 16);
        }
    }
    
    /// <summary>
    /// Dimensional rift that damages over time
    /// </summary>
    [Tracked]
    public class DimensionalRift : Entity
    {
        private int damage;
        private float lifetime;
        private float damageTimer;
        private Sprite sprite;
        private Level level;
        
        public DimensionalRift(Vector2 position, int damage) : base(position)
        {
            this.damage = damage;
            Collider = new Hitbox(32f, 48f, -16f, -24f);
            
            sprite = new Sprite(GFX.Game, "characters/darkmatter/");
            sprite.AddLoop("rift", "rift", 0.08f);
            sprite.Play("rift");
            sprite.CenterOrigin();
            Add(sprite);
            
            Add(new PlayerCollider(OnPlayerStay));
            Depth = -50;
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            Audio.Play("event:/char/badeline/disappear", Position);
        }
        
        public override void Update()
        {
            base.Update();
            
            lifetime += Engine.DeltaTime;
            damageTimer += Engine.DeltaTime;
            
            if (lifetime > 4f)
            {
                RemoveSelf();
            }
        }
        
        private void OnPlayerStay(global::Celeste.Player player)
        {
            if (damageTimer >= 0.5f)
            {
                PlayerHealthManager.TryDamagePlayer(damage, Position);
                damageTimer = 0f;
            }
        }
        
        public override void Render()
        {
            sprite.DrawOutline();
            base.Render();
        }
    }
    
    #endregion
}

