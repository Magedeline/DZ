using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    /// <summary>
    /// Base class for boss characters
    /// </summary>
    [Tracked]
    public abstract class BossCharacter : Actor
    {
        public enum BossState
        {
            Dormant,
            Intro,
            Phase1,
            Phase2,
            Phase3,
            Defeated,
            Transitioning
        }

        public BossState CurrentState { get; protected set; }
        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public float InvincibilityTimer { get; protected set; }
        public bool IsDefeated => CurrentState == BossState.Defeated;
        
        protected Sprite sprite;
        protected StateMachine stateMachine;
        protected Vector2 arenaCenter;
        protected float arenaRadius = 200f;
        protected List<Entity> bossEntities = new List<Entity>();
        
        public BossCharacter(Vector2 position, int maxHealth)
            : base(position)
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            arenaCenter = position;
            CurrentState = BossState.Dormant;
            
            Collider = new Hitbox(24f, 24f, -12f, -24f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Depth = -10000;
        }

        public override void Update()
        {
            base.Update();
            
            if (InvincibilityTimer > 0)
            {
                InvincibilityTimer -= Engine.DeltaTime;
            }
        }

        public virtual void TakeDamage(int damage)
        {
            if (InvincibilityTimer > 0 || IsDefeated)
                return;

            Health -= damage;
            InvincibilityTimer = 0.5f;
            
            OnTakeDamage(damage);
            
            if (Health <= 0)
            {
                Defeat();
            }
        }

        protected abstract void OnTakeDamage(int damage);

        protected virtual void Defeat()
        {
            CurrentState = BossState.Defeated;
            Add(new Coroutine(DefeatSequence()));
        }

        protected virtual IEnumerator DefeatSequence()
        {
            yield return 1f;
            RemoveSelf();
        }

        protected virtual void SpawnMinion(Entity minion)
        {
            Scene.Add(minion);
            bossEntities.Add(minion);
        }

        protected virtual void CleanupMinions()
        {
            foreach (Entity entity in bossEntities)
            {
                entity?.RemoveSelf();
            }
            bossEntities.Clear();
        }

        public override void Render()
        {
            if (InvincibilityTimer > 0 && Scene.OnInterval(0.05f))
            {
                return; // Flashing effect
            }
            
            DrawHealthBar();
            base.Render();
        }

        protected virtual void DrawHealthBar()
        {
            if (CurrentState == BossState.Dormant || CurrentState == BossState.Intro)
                return;

            float healthPercent = (float)Health / MaxHealth;
            Vector2 barPos = Position + new Vector2(-50f, -40f);
            float barWidth = 100f;
            
            Draw.Rect(barPos.X, barPos.Y, barWidth, 6f, Color.Black * 0.7f);
            Draw.Rect(barPos.X + 1f, barPos.Y + 1f, (barWidth - 2f) * healthPercent, 4f, Color.Red);
        }
    }

    /// <summary>
    /// Mid-boss character - less health and simpler patterns than full bosses
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/MidBoss")]
    [Tracked]
    public class MidBoss : BossCharacter
    {
        private float attackInterval = 3f;
        private float attackTimer;
        private int attackPattern = 0;
        
        public MidBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Int("health", 10))
        {
            attackInterval = data.Float("attackInterval", 3f);
            attackTimer = attackInterval;
            
            Add(sprite = new Sprite(GFX.Game, "characters/bosses/midboss/"));
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("attack", "attack", 0.08f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            
            CurrentState = BossState.Dormant;
        }

        public override void Update()
        {
            base.Update();
            
            if (CurrentState == BossState.Phase1)
            {
                attackTimer -= Engine.DeltaTime;
                
                if (attackTimer <= 0)
                {
                    PerformAttack();
                    attackTimer = attackInterval;
                    attackPattern = (attackPattern + 1) % 3;
                }
            }
        }

        public void StartBattle()
        {
            CurrentState = BossState.Phase1;
            Add(new Coroutine(IntroSequence()));
        }

        private IEnumerator IntroSequence()
        {
            yield return 1f;
            Audio.Play("event:/game/general/thing_booped");
        }

        private void PerformAttack()
        {
            sprite.Play("attack");
            
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
                return;

            switch (attackPattern)
            {
                case 0:
                    // Shoot three projectiles in spread
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 direction = (player.Center - Position).SafeNormalize();
                        direction = direction.Rotate(i * 0.3f);
                        EnergyBall projectile = new EnergyBall(Position, direction, 120f, Color.Purple);
                        Scene.Add(projectile);
                    }
                    break;
                    
                case 1:
                    // Spawn homing projectile
                    Vector2 dir = (player.Center - Position).SafeNormalize();
                    HomingProjectile homing = new HomingProjectile(Position, dir, 80f);
                    Scene.Add(homing);
                    break;
                    
                case 2:
                    // Ground slam
                    Add(new Coroutine(GroundSlamAttack()));
                    break;
            }
        }

        private IEnumerator GroundSlamAttack()
        {
            // Jump up
            Vector2 startPos = Position;
            for (float t = 0; t < 1f; t += Engine.DeltaTime * 2f)
            {
                Position = startPos + new Vector2(0, -100f * t);
                yield return null;
            }
            
            // Slam down
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                Position.X = player.X;
            }
            
            for (float t = 0; t < 1f; t += Engine.DeltaTime * 3f)
            {
                Position.Y = startPos.Y - 100f + (100f * t);
                yield return null;
            }
            
            Position.Y = startPos.Y;
            
            // Create shockwave
            Level level = Scene as Level;
            level?.Shake(0.5f);
            Audio.Play("event:/game/general/thing_booped");
        }

        protected override void OnTakeDamage(int damage)
        {
            Audio.Play("event:/game/general/thing_booped");
            
            // Flash effect
            sprite.Color = Color.Red;
            Alarm.Set(this, 0.1f, () => sprite.Color = Color.White);
        }

        protected override IEnumerator DefeatSequence()
        {
            sprite.Play("idle");
            
            // Death animation
            for (float t = 0; t < 1f; t += Engine.DeltaTime)
            {
                sprite.Color = Color.Lerp(Color.White, Color.Transparent, t);
                sprite.Scale = Vector2.One * (1f + t * 0.5f);
                yield return null;
            }
            
            RemoveSelf();
        }
    }

    /// <summary>
    /// Example full boss with multiple phases
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/FullBoss")]
    [Tracked]
    public class FullBoss : BossCharacter
    {
        private float phase1AttackInterval = 2.5f;
        private float phase2AttackInterval = 1.5f;
        private float attackTimer;
        
        public FullBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Int("health", 30))
        {
            attackTimer = phase1AttackInterval;
            
            Add(sprite = new Sprite(GFX.Game, "characters/bosses/fullboss/"));
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("phase1", "phase1", 0.08f);
            sprite.AddLoop("phase2", "phase2", 0.06f);
            sprite.AddLoop("attack", "attack", 0.08f);
            sprite.Play("idle");
            sprite.CenterOrigin();
        }

        public void StartBattle()
        {
            Add(new Coroutine(IntroSequence()));
        }

        private IEnumerator IntroSequence()
        {
            CurrentState = BossState.Intro;
            
            // Camera shake, dramatic entrance
            Level level = Scene as Level;
            level?.Shake(1f);
            
            yield return 2f;
            
            CurrentState = BossState.Phase1;
            sprite.Play("phase1");
        }

        public override void Update()
        {
            base.Update();
            
            if (CurrentState == BossState.Phase1 || CurrentState == BossState.Phase2)
            {
                attackTimer -= Engine.DeltaTime;
                
                if (attackTimer <= 0)
                {
                    PerformAttack();
                    
                    if (CurrentState == BossState.Phase1)
                        attackTimer = phase1AttackInterval;
                    else
                        attackTimer = phase2AttackInterval;
                }
            }
        }

        protected override void OnTakeDamage(int damage)
        {
            Audio.Play("event:/game/general/thing_booped");
            
            float healthPercent = (float)Health / MaxHealth;
            
            // Transition to phase 2 at 50% health
            if (healthPercent <= 0.5f && CurrentState == BossState.Phase1)
            {
                Add(new Coroutine(TransitionToPhase2()));
            }
        }

        private IEnumerator TransitionToPhase2()
        {
            CurrentState = BossState.Transitioning;
            
            CleanupMinions();
            
            Level level = Scene as Level;
            level?.Shake(0.5f);
            
            yield return 1f;
            
            CurrentState = BossState.Phase2;
            sprite.Play("phase2");
            InvincibilityTimer = 2f;
        }

        private void PerformAttack()
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player == null)
                return;

            sprite.Play("attack");
            
            if (CurrentState == BossState.Phase1)
            {
                // Phase 1 attack patterns
                Phase1Attack(player);
            }
            else if (CurrentState == BossState.Phase2)
            {
                // Phase 2 attack patterns (more intense)
                Phase2Attack(player);
            }
        }

        private void Phase1Attack(Player player)
        {
            // Circular projectile pattern
            for (int i = 0; i < 8; i++)
            {
                float angle = (MathF.PI * 2f / 8f) * i;
                Vector2 direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                EnergyBall projectile = new EnergyBall(Position, direction, 100f, Color.Blue);
                Scene.Add(projectile);
            }
        }

        private void Phase2Attack(Player player)
        {
            // More complex patterns
            
            // Spiral pattern
            for (int i = 0; i < 12; i++)
            {
                float angle = (MathF.PI * 2f / 12f) * i + Scene.TimeActive;
                Vector2 direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                EnergyBall projectile = new EnergyBall(Position, direction, 120f, Color.Red);
                Scene.Add(projectile);
            }
            
            // Spawn homing projectiles
            for (int i = 0; i < 3; i++)
            {
                Vector2 dir = (player.Center - Position).SafeNormalize();
                HomingProjectile homing = new HomingProjectile(Position, dir, 90f);
                Scene.Add(homing);
            }
        }

        protected override IEnumerator DefeatSequence()
        {
            CleanupMinions();
            
            Level level = Scene as Level;
            level?.Shake(1f);
            
            // Dramatic defeat animation
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                if (Scene.OnInterval(0.1f))
                {
                    sprite.Color = Calc.Random.Choose(Color.White, Color.Red, Color.Black);
                }
                yield return null;
            }
            
            // Final explosion effect
            yield return 1f;
            
            // Set completion flag
            level?.Session.SetFlag("boss_defeated");
            
            RemoveSelf();
        }
    }

    /// <summary>
    /// Mini-boss that guards checkpoints or areas
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/MiniBoss")]
    [Tracked]
    public class MiniBoss : BossCharacter
    {
        private Vector2 patrolStart;
        private Vector2 patrolEnd;
        private bool movingToEnd = true;
        private float moveSpeed = 40f;
        
        public MiniBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Int("health", 5))
        {
            patrolStart = Position;
            patrolEnd = Position + new Vector2(data.Float("patrolDistance", 80f), 0f);
            
            Add(sprite = new Sprite(GFX.Game, "characters/bosses/miniboss/"));
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("walk", "walk", 0.08f);
            sprite.Play("walk");
            sprite.CenterOrigin();
            
            CurrentState = BossState.Phase1;
            Collider = new Hitbox(16f, 20f, -8f, -20f);
        }

        public override void Update()
        {
            base.Update();
            
            if (CurrentState == BossState.Phase1)
            {
                // Simple patrol behavior
                Vector2 target = movingToEnd ? patrolEnd : patrolStart;
                Vector2 direction = (target - Position).SafeNormalize();
                
                MoveH(direction.X * moveSpeed * Engine.DeltaTime);
                
                if (Vector2.Distance(Position, target) < 4f)
                {
                    movingToEnd = !movingToEnd;
                }
                
                sprite.Scale.X = Math.Sign(direction.X);
            }
        }

        protected override void OnTakeDamage(int damage)
        {
            Audio.Play("event:/game/general/thing_booped");
        }

        protected override IEnumerator DefeatSequence()
        {
            yield return 0.5f;
            RemoveSelf();
        }
    }

    public class EnergyBall : Entity
    {
        private Vector2 direction;
        private float speed;
        private Color color;

        public EnergyBall(Vector2 position, Vector2 direction, float speed, Color color)
            : base(position)
        {
            this.direction = direction;
            this.speed = speed;
            this.color = color;
            Depth = -10010;
        }

        public override void Update()
        {
            base.Update();
            Position += direction * speed * Engine.DeltaTime;
        }

        public override void Render()
        {
            base.Render();
            Draw.Circle(Position, 6f, color, 12);
        }
    }

    public class HomingProjectile : Entity
    {
        private Vector2 direction;
        private float speed;
        private float homingStrength = 2f;
        private Entity target;

        public HomingProjectile(Vector2 position, Vector2 direction, float speed)
            : base(position)
        {
            this.direction = direction.SafeNormalize();
            this.speed = speed;
            Depth = -10010;
        }

        public override void Update()
        {
            base.Update();
            if (target == null || target.Scene != Scene)
            {
                target = Scene?.Tracker.GetEntity<Player>();
            }

            if (target != null)
            {
                Vector2 toTarget = (target.Center - Position).SafeNormalize();
                direction = Vector2.Lerp(direction, toTarget, homingStrength * Engine.DeltaTime).SafeNormalize();
            }

            Position += direction * speed * Engine.DeltaTime;
        }

        public override void Render()
        {
            base.Render();
            Draw.Circle(Position, 7f, Color.Lime, 14);
        }
    }
}
