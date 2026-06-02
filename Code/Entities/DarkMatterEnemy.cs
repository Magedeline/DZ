using System;
using System.Collections;
using Celeste.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Dark Matter Enemy - A void-based enemy that deals variable damage (2-7 hits)
    /// Floats menacingly and attacks players with dark energy
    /// Sprite path: characters/darkmatter/
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/DarkMatterEnemy")]
    [Tracked]
    [HotReloadable]
    public class DarkMatterEnemy : Entity
    {
        #region Constants
        
        private const float MOVEMENT_SPEED = 40f;
        private const float DETECTION_RANGE = 200f;
        private const float ATTACK_RANGE = 100f;
        private const float ATTACK_COOLDOWN = 2f;
        
        #endregion
        
        #region Fields
        
        // Combat stats
        private int health;
        private int maxHealth;
        private int minDamage;
        private int maxDamage;
        private float invulnerabilityTimer;
        private const float INVULNERABILITY_DURATION = 0.5f;
        
        // Movement
        private Vector2 targetPosition;
        private Vector2 homePosition;
        private float patrolRadius;
        private bool isChasing;
        
        // Combat
        private float attackCooldownTimer;
        private bool isAttacking;
        private EnemyState currentState;
        
        // Visual
        private Sprite sprite;
        private VertexLight light;
        private ParticleType darkParticle;
        private float flashTimer;
        
        // Level reference
        private Level level;
        
        #endregion
        
        #region Enums
        
        private enum EnemyState
        {
            Patrol,
            Chase,
            Attack,
            Hurt,
            Dead
        }
        
        #endregion
        
        #region Constructor
        
        public DarkMatterEnemy(EntityData data, Vector2 offset) 
            : base(data.Position + offset)
        {
            maxHealth = data.Int("health", 10);
            health = maxHealth;
            minDamage = data.Int("minDamage", 2);
            maxDamage = data.Int("maxDamage", 7);
            patrolRadius = data.Float("patrolRadius", 64f);
            
            homePosition = Position;
            currentState = EnemyState.Patrol;
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            
            SetupSprite();
            SetupLight();
            SetupParticles();
            
            Add(new PlayerCollider(OnPlayerCollide));
            
            Depth = -10000;
        }
        
        #endregion
        
        #region Setup
        
        private void SetupSprite()
        {
            sprite = new Sprite(GFX.Game, "characters/darkmatter/");
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("chase", "chase", 0.08f);
            sprite.Add("attack", "attack", 0.05f, "idle");
            sprite.Add("hurt", "hurt", 0.05f, "idle");
            sprite.Add("death", "death", 0.08f);
            
            sprite.Play("idle");
            sprite.CenterOrigin();
            
            Add(sprite);
        }
        
        private void SetupLight()
        {
            light = new VertexLight(Color.Purple * 0.8f, 1f, 32, 64);
            Add(light);
        }
        
        private void SetupParticles()
        {
            darkParticle = new ParticleType
            {
                Source = GFX.Game["particles/blob"],
                Color = Color.Purple,
                Color2 = Color.DarkViolet,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.3f,
                LifeMax = 0.8f,
                Size = 1f,
                SpeedMin = 10f,
                SpeedMax = 30f,
                SpeedMultiplier = 0.2f,
                DirectionRange = (float)Math.PI * 2f
            };
        }
        
        #endregion
        
        #region Lifecycle
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            targetPosition = homePosition;
        }
        
        public override void Update()
        {
            base.Update();
            
            if (currentState == EnemyState.Dead)
                return;
            
            // Update timers
            if (invulnerabilityTimer > 0)
                invulnerabilityTimer -= Engine.DeltaTime;
            
            if (attackCooldownTimer > 0)
                attackCooldownTimer -= Engine.DeltaTime;
            
            if (flashTimer > 0)
            {
                flashTimer -= Engine.DeltaTime;
                sprite.Color = Color.Lerp(Color.White, Color.Purple, 1f - (flashTimer / 0.2f));
            }
            else
            {
                sprite.Color = Color.White;
            }
            
            // State machine
            switch (currentState)
            {
                case EnemyState.Patrol:
                    UpdatePatrol();
                    break;
                case EnemyState.Chase:
                    UpdateChase();
                    break;
                case EnemyState.Attack:
                    UpdateAttack();
                    break;
                case EnemyState.Hurt:
                    UpdateHurt();
                    break;
            }
            
            // Emit dark particles
            if (Scene.OnInterval(0.1f))
            {
                level?.ParticlesFG.Emit(darkParticle, Position);
            }
        }
        
        #endregion
        
        #region State Updates
        
        private void UpdatePatrol()
        {
            sprite.Play("idle");
            
            // Check for player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < DETECTION_RANGE)
            {
                currentState = EnemyState.Chase;
                isChasing = true;
                return;
            }
            
            // Patrol around home position
            if (Vector2.Distance(Position, targetPosition) < 5f)
            {
                // Pick new patrol point
                float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                targetPosition = homePosition + Calc.AngleToVector(angle, Calc.Random.Range(0f, patrolRadius));
            }
            
            MoveTowards(targetPosition, MOVEMENT_SPEED * 0.5f);
        }
        
        private void UpdateChase()
        {
            sprite.Play("chase");
            
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null)
            {
                currentState = EnemyState.Patrol;
                isChasing = false;
                targetPosition = homePosition;
                return;
            }
            
            float distanceToPlayer = Vector2.Distance(Position, player.Position);
            
            // Return to patrol if too far
            if (distanceToPlayer > DETECTION_RANGE * 1.5f)
            {
                currentState = EnemyState.Patrol;
                isChasing = false;
                targetPosition = homePosition;
                return;
            }
            
            // Attack if in range
            if (distanceToPlayer < ATTACK_RANGE && attackCooldownTimer <= 0)
            {
                currentState = EnemyState.Attack;
                isAttacking = true;
                Add(new Coroutine(AttackRoutine()));
                return;
            }
            
            // Chase player
            MoveTowards(player.Position, MOVEMENT_SPEED);
        }
        
        private void UpdateAttack()
        {
            // Handled by coroutine
        }
        
        private void UpdateHurt()
        {
            // Return to chase after hurt animation
            if (sprite.CurrentAnimationID == "idle")
            {
                currentState = EnemyState.Chase;
            }
        }
        
        #endregion
        
        #region Combat
        
        private IEnumerator AttackRoutine()
        {
            sprite.Play("attack");
            attackCooldownTimer = ATTACK_COOLDOWN;
            
            // Wind up
            yield return 0.3f;
            
            // Fire dark energy projectile
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                Vector2 direction = (player.Position - Position).SafeNormalize();
                Scene.Add(new DarkEnergyProjectile(Position, direction, Calc.Random.Range(minDamage, maxDamage + 1)));
                
                Audio.Play("event:/char/badeline/boss_bullet", Position);
                
                // Screen shake
                level?.Shake(0.2f);
            }
            
            // Recovery
            yield return 0.3f;
            
            isAttacking = false;
            currentState = EnemyState.Chase;
        }
        
        public void TakeDamage(int damage)
        {
            if (invulnerabilityTimer > 0 || currentState == EnemyState.Dead)
                return;
            
            health -= damage;
            invulnerabilityTimer = INVULNERABILITY_DURATION;
            flashTimer = 0.2f;
            
            Audio.Play("event:/char/badeline/boss_bullet_impact", Position);
            
            // Hit particles
            for (int i = 0; i < 8; i++)
            {
                level?.ParticlesFG.Emit(darkParticle, Position);
            }
            
            if (health <= 0)
            {
                Die();
            }
            else
            {
                currentState = EnemyState.Hurt;
                sprite.Play("hurt");
            }
        }
        
        private void Die()
        {
            currentState = EnemyState.Dead;
            sprite.Play("death");
            Collidable = false;
            
            Audio.Play("event:/char/badeline/disappear", Position);
            
            // Death explosion
            for (int i = 0; i < 20; i++)
            {
                level?.ParticlesFG.Emit(darkParticle, Position + Calc.Random.Range(Vector2.One * -8f, Vector2.One * 8f));
            }
            
            level?.Shake(0.3f);
            
            // Notify arena
            var arena = Scene.Tracker.GetEntity<VoidGateArena>();
            arena?.OnEnemyDefeated(this);
            
            Add(new Coroutine(RemoveAfterDeath()));
        }
        
        private IEnumerator RemoveAfterDeath()
        {
            yield return 1f;
            RemoveSelf();
        }
        
        private void OnPlayerCollide(global::Celeste.Player player)
        {
            if (currentState == EnemyState.Dead)
                return;
            
            // Deal damage to player
            int damage = Calc.Random.Range(minDamage, maxDamage + 1);
            PlayerHealthManager.TryDamagePlayer(damage, Position);
            
            // Knockback
            Vector2 direction = (player.Position - Position).SafeNormalize();
            player.Speed = direction * 200f;
        }
        
        #endregion
        
        #region Movement
        
        private void MoveTowards(Vector2 target, float speed)
        {
            Vector2 direction = (target - Position).SafeNormalize();
            Position += direction * speed * Engine.DeltaTime;
            
            // Face movement direction
            if (direction.X != 0)
            {
                sprite.Scale.X = Math.Sign(direction.X);
            }
        }
        
        #endregion
        
        #region Rendering
        
        public override void Render()
        {
            sprite.DrawOutline();
            base.Render();
        }
        
        #endregion
    }
    
    #region Dark Energy Projectile
    
    /// <summary>
    /// Projectile fired by Dark Matter enemies
    /// </summary>
    [Tracked]
    public class DarkEnergyProjectile : Entity
    {
        private Vector2 velocity;
        private int damage;
        private Sprite sprite;
        private float lifetime;
        private const float MAX_LIFETIME = 5f;
        private Level level;
        
        public DarkEnergyProjectile(Vector2 position, Vector2 direction, int damage)
            : base(position)
        {
            this.damage = damage;
            velocity = direction * 150f;
            
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            
            sprite = new Sprite(GFX.Game, "characters/darkmatter/");
            sprite.AddLoop("projectile", "projectile", 0.05f);
            sprite.Play("projectile");
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
            
            Position += velocity * Engine.DeltaTime;
            lifetime += Engine.DeltaTime;
            
            // Rotate sprite
            sprite.Rotation += Engine.DeltaTime * 10f;
            
            // Check for collision with solids
            if (CollideCheck<Solid>() || lifetime > MAX_LIFETIME)
            {
                Explode();
                return;
            }
            
            // Particle trail
            if (Scene.OnInterval(0.05f))
            {
                var particle = new ParticleType
                {
                    Color = Color.Purple,
                    Color2 = Color.DarkViolet,
                    ColorMode = ParticleType.ColorModes.Blink,
                    Size = 0.5f,
                    LifeMin = 0.2f,
                    LifeMax = 0.4f
                };
                level?.ParticlesFG.Emit(particle, Position);
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
            
            // Explosion particles
            for (int i = 0; i < 10; i++)
            {
                float angle = (i / 10f) * (float)Math.PI * 2f;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                var particle = new ParticleType
                {
                    Color = Color.Purple,
                    Color2 = Color.DarkViolet,
                    Size = 1f,
                    LifeMin = 0.3f,
                    LifeMax = 0.6f
                };
                level?.ParticlesFG.Emit(particle, Position + dir * 4f);
            }
            
            RemoveSelf();
        }
        
        public override void Render()
        {
            sprite.DrawOutline();
            base.Render();
        }
    }
    
    #endregion
}
