using System;
using System.Collections;
using Celeste.Helpers;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.Enemies
{
    /// <summary>
    /// Base class for Kirby-style mid-bosses (mini-bosses)
    /// Mid-bosses have more health than regular enemies, unique attack patterns,
    /// and drop copy abilities when defeated
    /// </summary>
    public abstract class KirbyMidBoss : BossActor
    {
        #region Fields

        protected Sprite sprite;
        protected Vector2 startPosition;
        protected float attackCooldown;
        protected float attackTimer;
        protected int phase;
        protected int maxPhases;

        // Movement
        protected float moveSpeed;
        protected Vector2 targetPosition;
        protected bool isMoving;

        // Vulnerability
        protected float vulnerableTimer;
        protected bool isVulnerable;
        private bool isDefeated => IsDefeated;

        #endregion

        #region Abstract Properties


        public abstract string BossName { get; }
        public abstract int MaxHealthPerPhase { get; }
        public abstract float AttackCooldownTime { get; }
        public abstract KirbyMode.KirbyPowerState DroppedAbility { get; }

        #endregion

        #region Constructor

        public KirbyMidBoss(Vector2 position, string spritePath) : base(
            position,
            spritePath,
            Vector2.One,
            200f,
            true,
            true,
            1f,
            new Hitbox(24f, 24f, -12f, -24f))
        {
            startPosition = position;
            moveSpeed = 60f;
            phase = 1;
            maxPhases = 3;
            attackCooldown = 1f;
            isVulnerable = true;

            MaxHealth = MaxHealthPerPhase * maxPhases;
            Health = MaxHealth;

            Depth = -100;

            // Setup sprite
            Add(sprite = new Sprite(GFX.Game, spritePath));
            SetupAnimations();
            sprite.Play("idle");

            // Add player collider for contact damage
            Add(new PlayerCollider(OnPlayerContact));
        }

        protected abstract void SetupAnimations();

        #endregion

        #region Lifecycle

        public override void Update()
        {
            base.Update();

            if (isDefeated) return;

            // Update attack cooldown
            if (attackTimer > 0)
                attackTimer -= Engine.DeltaTime;

            // Update vulnerability timer
            if (vulnerableTimer > 0)
            {
                vulnerableTimer -= Engine.DeltaTime;
                if (vulnerableTimer <= 0)
                {
                    isVulnerable = true;
                    OnBecomeVulnerable();
                }
            }

            // AI Update
            UpdateAI();

            // Phase management based on health
            int newPhase = Math.Min(maxPhases, ((MaxHealth - Health) / MaxHealthPerPhase) + 1);
            if (newPhase != phase)
            {
                phase = newPhase;
                OnPhaseChange(phase);
            }
        }

        protected abstract void UpdateAI();

        #endregion

        #region Phase Management

        protected virtual void OnPhaseChange(int newPhase)
        {
            // Speed up in later phases
            moveSpeed = 60f + (newPhase - 1) * 20f;

            // Visual feedback
            if (sprite != null) sprite.Color = Color.White;
            Audio.Play("event:/char/badeline/boss_bullet", Position);

            Logger.Log(LogLevel.Info, "KirbyMidBoss", $"{BossName} entered phase {newPhase}");
        }

        protected virtual void OnBecomeVulnerable()
        {
            sprite?.Play("idle");
        }

        #endregion

        #region Combat

        protected void TryAttack()
        {
            if (attackTimer <= 0 && !isDefeated)
            {
                PerformAttack();
                attackTimer = AttackCooldownTime;
                isVulnerable = false;
                vulnerableTimer = 2f; // Vulnerable for 2 seconds after attacking
            }
        }

        protected abstract void PerformAttack();

        protected virtual void OnPlayerContact(global::Celeste.Player player)
        {
            if (isDefeated) return;

            // Deal contact damage
            if (player.IsKirbyMode())
            {
                var healthManager = PlayerHealthManager.Instance;
                if (healthManager != null)
                {
                    healthManager.Damage(1);
                }
                else
                {
                    PlayerHealthManager.TryDamagePlayer(1, Position);
                }
            }
            else
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
        }

        public override void TakeDamage(int damage)
        {
            if (!isVulnerable || isDefeated) return;

            base.TakeDamage(damage);

            // Visual feedback
            if (sprite != null) sprite.Color = Color.Red;
            Audio.Play("event:/char/badeline/boss_bullet_impact", Position);

            // Knockback
            Position += new Vector2(0, -5f);

            // Check for phase defeat (not full defeat yet)
            int healthInPhase = Health % MaxHealthPerPhase;
            if (healthInPhase == 0 && Health > 0)
            {
                // Phase defeated - become vulnerable for longer
                isVulnerable = true;
                vulnerableTimer = 3f;
                OnPhaseDefeated();
            }
        }

        protected virtual void OnPhaseDefeated()
        {
            sprite?.Play("hurt");
            Audio.Play("event:/Mods/pusheen/char/bosses/large_explosion", Position);
        }

        protected virtual void OnDefeated()
        {
            IsDefeated = true;

            // Drop copy ability
            if (DroppedAbility != KirbyMode.KirbyPowerState.None)
            {
                DropCopyAbility();
            }

            // Create defeat effect
            var level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 30; i++)
                {
                    float angle = (i / 30f) * (float)Math.PI * 2f;
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    level.ParticlesFG.Emit(ParticleTypes.SparkyDust, Position + dir * 16f);
                }

                level.Shake(0.3f);
                Audio.Play("event:/game/general/thing_booped", Position);
            }

            // Remove after delay
            Add(new Coroutine(RemoveAfterDefeat()));
        }

        private IEnumerator RemoveAfterDefeat()
        {
            sprite?.Play("defeat");
            yield return 1f;
            RemoveSelf();
        }

        private void DropCopyAbility()
        {
            // Spawn a visual representation of the ability
            var abilityDrop = new CopyAbilityDrop(Position, DroppedAbility);
            Scene.Add(abilityDrop);
        }

        #endregion

        #region Movement Helpers

        protected void MoveTowards(Vector2 target, float speed)
        {
            Vector2 direction = (target - Position).SafeNormalize();
            Position += direction * speed * Engine.DeltaTime;

            // Face movement direction
            if (direction.X != 0 && sprite != null)
            {
                sprite.Scale.X = Math.Sign(direction.X);
            }
        }

        protected bool IsPlayerInRange(float range)
        {
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null) return false;

            return Vector2.Distance(Position, player.Position) < range;
        }

        protected Vector2 GetPlayerPosition()
        {
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            return player?.Position ?? Position;
        }

        #endregion
    }

    /// <summary>
    /// Visual representation of a dropped copy ability
    /// </summary>
    public class CopyAbilityDrop : Entity
    {
        private KirbyMode.KirbyPowerState ability;
        private float bounceTimer;
        private float bounceOffset;
        private ParticleType abilityParticle;

        public CopyAbilityDrop(Vector2 position, KirbyMode.KirbyPowerState ability) : base(position)
        {
            this.ability = ability;
            Depth = -50;
            Collider = new Hitbox(20f, 20f, -10f, -20f);

            // Add player collider
            Add(new PlayerCollider(OnPlayerCollect));

            // Setup particles based on ability
            abilityParticle = new ParticleType
            {
                Source = GFX.Game["particles/shard"],
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                Size = 1f,
                SpeedMin = 10f,
                SpeedMax = 30f
            };

            // Set particle color based on ability
            abilityParticle.Color = GetAbilityColor(ability);
            abilityParticle.Color2 = Color.White;
        }

        public override void Update()
        {
            base.Update();

            // Bounce animation
            bounceTimer += Engine.DeltaTime * 3f;
            bounceOffset = (float)Math.Sin(bounceTimer) * 3f;

            // Emit particles
            if (Scene.OnInterval(0.2f))
            {
                var level = Scene as Level;
                level?.ParticlesFG.Emit(abilityParticle, Position + new Vector2(0, bounceOffset));
            }
        }

        private void OnPlayerCollect(global::Celeste.Player player)
        {
            // Grant ability
            player.SetKirbyPowerState(ability);

            // Visual feedback
            var level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = (i / 20f) * (float)Math.PI * 2f;
                    Vector2 dir = Calc.AngleToVector(angle, 1f);
                    level.ParticlesFG.Emit(abilityParticle, Position + dir * 12f);
                }

                level.Flash(GetAbilityColor(ability) * 0.3f);
            }

            Audio.Play("event:/game/general/thing_booped", Position);

            RemoveSelf();
        }

        public override void Render()
        {
            // Draw ability icon
            string iconPath = $"abilities/{ability.ToString().ToLower()}/icon";
            if (GFX.Game.Has(iconPath))
            {
                var texture = GFX.Game[iconPath];
                texture.DrawCentered(Position + new Vector2(0, bounceOffset), Color.White, 1f);
            }
            else
            {
                // Fallback - draw colored circle
                Draw.Circle(Position + new Vector2(0, bounceOffset), 10f, GetAbilityColor(ability), 16);
            }
        }

        private Color GetAbilityColor(KirbyMode.KirbyPowerState ability)
        {
            return ability switch
            {
                KirbyMode.KirbyPowerState.Fire => Color.Red,
                KirbyMode.KirbyPowerState.Ice => Color.Cyan,
                KirbyMode.KirbyPowerState.Spark => Color.Yellow,
                KirbyMode.KirbyPowerState.Sword => Color.Orange,
                KirbyMode.KirbyPowerState.Stone => Color.Gray,
                KirbyMode.KirbyPowerState.Beam => Color.Purple,
                KirbyMode.KirbyPowerState.Cutter => Color.Green,
                KirbyMode.KirbyPowerState.Mirror => Color.Silver,
                _ => Color.Pink
            };
        }
    }

    #region Specific Mid-Bosses

    /// <summary>
    /// Poppy Bros Jr - Bomb-throwing mid-boss
    /// </summary>
    [CustomEntity("DZ/PoppyBrosJr")]
    [Tracked]
    public class PoppyBrosJr : KirbyMidBoss
    {
        public override string BossName => "Poppy Bros Jr";
        public override int MaxHealthPerPhase => 10;
        public override float AttackCooldownTime => 2f;
        public override KirbyMode.KirbyPowerState DroppedAbility => KirbyMode.KirbyPowerState.Bomb;

        private float jumpTimer;
        private bool isJumping;
        private float jumpHeight;

        public PoppyBrosJr(EntityData data, Vector2 offset) : base(data.Position + offset, "characters/poppybrosjr/")
        {
        }

        protected override void SetupAnimations()
        {
            // Placeholder art - only "idle" frame exists currently
            sprite.AddLoop("idle", "idle", 0.15f);
            sprite.AddLoop("jump", "idle", 0.1f);
            sprite.Add("throw", "idle", 0.08f, "idle");
            sprite.Add("hurt", "idle", 0.1f, "idle");
            sprite.Add("defeat", "idle", 0.08f);
        }

        protected override void UpdateAI()
        {
            var playerPos = GetPlayerPosition();
            float distanceToPlayer = Vector2.Distance(Position, playerPos);

            // Jump around
            jumpTimer -= Engine.DeltaTime;
            if (jumpTimer <= 0 && !isJumping)
            {
                jumpTimer = Calc.Random.Range(1f, 2f);
                StartJump();
            }

            if (isJumping)
            {
                // Update jump arc
                jumpHeight -= Engine.DeltaTime * 200f;
                Position.Y += jumpHeight * Engine.DeltaTime;

                // Land check
                if (jumpHeight < 0 && CollideCheck<Solid>(Position + Vector2.UnitY * 4f))
                {
                    isJumping = false;
                    jumpHeight = 0;
                    sprite.Play("idle");
                }
            }

            // Try to attack when in range
            if (distanceToPlayer < 150f && !isJumping)
            {
                TryAttack();
            }
        }

        private void StartJump()
        {
            isJumping = true;
            jumpHeight = 100f;
            sprite.Play("jump");

            // Jump towards player
            var playerPos = GetPlayerPosition();
            Vector2 direction = (playerPos - Position).SafeNormalize();
            Position.X += direction.X * 20f;
        }

        protected override void PerformAttack()
        {
            sprite.Play("throw");

            // Throw bomb
            var playerPos = GetPlayerPosition();
            Vector2 direction = (playerPos - Position).SafeNormalize();

            Scene.Add(new global::Celeste.Projectiles.BombProjectile(
                Position + new Vector2(0, -16f),
                direction * 100f,
                2 // Damage
            ));

            Audio.Play("event:/char/badeline/boss_bullet", Position);
        }
    }

    /// <summary>
    /// Bonkers - Hammer-wielding mid-boss
    /// </summary>
    [CustomEntity("DZ/Bonkers")]
    [Tracked]
    public class Bonkers : KirbyMidBoss
    {
        public override string BossName => "Bonkers";
        public override int MaxHealthPerPhase => 12;
        public override float AttackCooldownTime => 2.5f;
        public override KirbyMode.KirbyPowerState DroppedAbility => KirbyMode.KirbyPowerState.Hammer;

        private float chargeTimer;
        private bool isCharging;

        public Bonkers(EntityData data, Vector2 offset) : base(data.Position + offset, "characters/bonkers/")
        {
            moveSpeed = 45f;
        }

        protected override void SetupAnimations()
        {
            // Only a single "boss" frame is currently available, so map all
            // states to it to keep Bonkers visible until full art exists.
            sprite.AddLoop("idle", "boss", 0.15f);
            sprite.AddLoop("walk", "boss", 0.12f);
            sprite.Add("charge", "boss", 0.08f, "attack");
            sprite.Add("attack", "boss", 0.05f, "idle");
            sprite.Add("hurt", "boss", 0.1f, "idle");
            sprite.Add("defeat", "boss", 0.08f);
        }

        protected override void UpdateAI()
        {
            var playerPos = GetPlayerPosition();
            float distanceToPlayer = Vector2.Distance(Position, playerPos);

            if (isCharging)
            {
                chargeTimer -= Engine.DeltaTime;
                if (chargeTimer <= 0)
                {
                    PerformHammerSlam();
                }
            }
            else
            {
                // Walk towards player
                if (distanceToPlayer > 40f)
                {
                    MoveTowards(playerPos, moveSpeed);
                    sprite.Play("walk");
                }
                else
                {
                    // Close enough - start charge
                    StartCharge();
                }
            }
        }

        private void StartCharge()
        {
            isCharging = true;
            chargeTimer = 0.8f;
            sprite.Play("charge");

            // Face player
            var playerPos = GetPlayerPosition();
            sprite.Scale.X = playerPos.X > Position.X ? 1 : -1;
        }

        private void PerformHammerSlam()
        {
            isCharging = false;
            sprite.Play("attack");

            // Damage player if close
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < 50f)
            {
                var healthManager = PlayerHealthManager.Instance;
                if (healthManager != null)
                {
                    healthManager.Damage(2);
                }
                else
                {
                    PlayerHealthManager.TryDamagePlayer(2, Position);
                }
            }

            // Screen shake
            var level = Scene as Level;
            level?.Shake(0.2f);

            // Spawn shockwave effect
            // (Could add actual shockwave projectile here)

            Audio.Play("event:/char/badeline/boss_bullet", Position);

            attackTimer = AttackCooldownTime;
        }

        protected override void PerformAttack()
        {
            // Handled in charge logic
        }
    }

    /// <summary>
    /// Bugzzy - Suplex-wrestling mid-boss
    /// </summary>
    [CustomEntity("DZ/Bugzzy")]
    [Tracked]
    public class Bugzzy : KirbyMidBoss
    {
        public override string BossName => "Bugzzy";
        public override int MaxHealthPerPhase => 10;
        public override float AttackCooldownTime => 2f;
        public override KirbyMode.KirbyPowerState DroppedAbility => KirbyMode.KirbyPowerState.Suplex;

        private float flightTimer;
        private bool isFlying;
        private float flightHeight;

        public Bugzzy(EntityData data, Vector2 offset) : base(data.Position + offset, "characters/bugzzy/")
        {
            moveSpeed = 70f;
        }

        protected override void SetupAnimations()
        {
            // Placeholder art - only "idle" frame exists currently
            sprite.AddLoop("idle", "idle", 0.15f);
            sprite.AddLoop("fly", "idle", 0.08f);
            sprite.Add("dive", "idle", 0.05f, "idle");
            sprite.Add("grab", "idle", 0.08f, "idle");
            sprite.Add("hurt", "idle", 0.1f, "idle");
            sprite.Add("defeat", "idle", 0.08f);
        }

        protected override void UpdateAI()
        {
            var playerPos = GetPlayerPosition();

            if (isFlying)
            {
                // Fly above player
                float targetY = playerPos.Y - 80f;
                Position.Y = Calc.Approach(Position.Y, targetY, moveSpeed * Engine.DeltaTime);
                Position.X = Calc.Approach(Position.X, playerPos.X, moveSpeed * Engine.DeltaTime);

                flightTimer -= Engine.DeltaTime;
                if (flightTimer <= 0)
                {
                    PerformDive();
                }
            }
            else
            {
                // Walk on ground
                if (Vector2.Distance(Position, playerPos) > 60f)
                {
                    MoveTowards(playerPos, moveSpeed * 0.5f);
                    sprite.Play("idle");
                }

                TryAttack();
            }
        }

        protected override void PerformAttack()
        {
            // Start flying
            isFlying = true;
            flightTimer = 2f;
            sprite.Play("fly");
            Audio.Play("event:/char/badeline/boss_bullet", Position);
        }

        private void PerformDive()
        {
            isFlying = false;
            sprite.Play("dive");

            // Dash toward ground
            Vector2 diveDir = new Vector2(0, 1);
            Position += diveDir * 200f * Engine.DeltaTime;

            // Damage if hits player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < 40f)
            {
                var healthManager = PlayerHealthManager.Instance;
                if (healthManager != null)
                {
                    healthManager.Damage(2);
                }
                else
                {
                    PlayerHealthManager.TryDamagePlayer(2, Position);
                }

                // Grab and slam (visual only for now)
                sprite.Play("grab");
            }

            attackTimer = AttackCooldownTime;
        }
    }

    #endregion
}

