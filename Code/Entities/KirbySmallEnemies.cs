using System;
using Celeste.Entities;
using Celeste.Extensions;
using Celeste.Projectiles;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.Enemies
{
    /// <summary>
    /// Base class for Kirby-style small enemies that can be inhaled and grant copy abilities
    /// </summary>
    public abstract class KirbySmallEnemy : Actor
    {
        #region Fields

        protected Sprite sprite;
        protected StateMachine stateMachine;
        protected Vector2 startPosition;
        protected bool facingRight;
        protected float walkSpeed;
        protected float patrolDistance;
        protected float patrolTimer;
        protected float stunTimer;
        protected int health;
        protected int maxHealth;
        protected float invincibilityTimer;
        protected bool isBeingInhaled;
        protected bool isDefeated;

        // Copy ability this enemy grants when inhaled
        public abstract KirbyMode.KirbyPowerState CopyAbility { get; }

        // Whether this enemy can be inhaled
        public virtual bool CanBeInhaled => true;

        // Damage dealt on contact
        public virtual int ContactDamage => 1;

        #endregion

        #region Constructor

        public KirbySmallEnemy(Vector2 position, string spritePath) : base(position)
        {
            startPosition = position;
            facingRight = true;
            walkSpeed = 40f;
            patrolDistance = 64f;
            maxHealth = 1;
            health = maxHealth;

            Collider = new Hitbox(16f, 16f, -8f, -16f);
            Depth = -100;

            // Setup sprite
            Add(sprite = new Sprite(GFX.Game, spritePath));
            SetupAnimations();
            sprite.Play("idle");

            // Setup state machine
            Add(stateMachine = new StateMachine());
            SetupStates();
        }

        #endregion

        #region Virtual Methods

        protected abstract void SetupAnimations();
        protected abstract void SetupStates();
        protected abstract void UpdateBehavior();

        #endregion

        #region Lifecycle

        public override void Update()
        {
            base.Update();

            if (isDefeated) return;

            // Update invincibility
            if (invincibilityTimer > 0)
                invincibilityTimer -= Engine.DeltaTime;

            // Update stun
            if (stunTimer > 0)
            {
                stunTimer -= Engine.DeltaTime;
                if (stunTimer <= 0)
                {
                    sprite.Play("idle");
                }
            }

            // Check for inhale
            if (!isBeingInhaled && CanBeInhaled)
            {
                CheckForInhale();
            }

            // Update behavior
            if (!isBeingInhaled && stunTimer <= 0)
            {
                UpdateBehavior();
            }

            // Check for player collision
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && CollideCheck(player) && !isBeingInhaled)
            {
                OnTouchPlayer(player);
            }
        }

        #endregion

        #region Inhale System

        private void CheckForInhale()
        {
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player == null) return;

            // Check if player is in inhale state and within range
            float inhaleRange = 80f;
            float distance = Vector2.Distance(Position, player.Position);

            if (distance < inhaleRange && IsPlayerInhaling(player))
            {
                StartBeingInhaled(player);
            }
        }

        private bool IsPlayerInhaling(global::Celeste.Player player)
        {
            // Check if player state matches Kirby inhale state
            // This should match the state defined in Player.cs (StKirbyInhale = 26)
            return player.StateMachine?.State == 26;
        }

        private void StartBeingInhaled(global::Celeste.Player player)
        {
            isBeingInhaled = true;
            sprite.Play("inhaled");
        }

        public void UpdateInhale(Vector2 inhaleSource)
        {
            if (!isBeingInhaled) return;

            // Move toward inhale source
            Vector2 direction = (inhaleSource - Position).SafeNormalize();
            Position += direction * walkSpeed * 3f * Engine.DeltaTime;

            // Check if consumed
            if (Vector2.Distance(Position, inhaleSource) < 16f)
            {
                OnConsumed();
            }
        }

        protected virtual void OnConsumed()
        {
            // Grant copy ability to player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && CopyAbility != KirbyMode.KirbyPowerState.None)
            {
                player.SetKirbyPowerState(CopyAbility);

                // Show ability get effect
                var level = Scene as Level;
                if (level != null)
                {
                    level.ParticlesFG.Emit(ParticleTypes.SparkyDust, 20, Position, Vector2.One * 16f);
                    Audio.Play("event:/game/general/thing_booped", Position);
                }
            }

            RemoveSelf();
        }

        #endregion

        #region Combat

        protected virtual void OnTouchPlayer(global::Celeste.Player player)
        {
            if (isDefeated || isBeingInhaled) return;

            // Try to damage player through health system
            if (player.IsKirbyMode())
            {
                var controller = KirbyHealthController.Instance;
                if (controller != null)
                {
                    controller.DamageFromEnemy(Position, ContactDamage);
                }
                else
                {
                    PlayerHealthManager.TryDamagePlayer(ContactDamage, Position);
                }
            }
            else
            {
                // Normal death for non-Kirby mode
                player.Die((player.Position - Position).SafeNormalize());
            }

            // Bounce away
            Vector2 bounceDir = (Position - player.Position).SafeNormalize();
            Position += bounceDir * 20f;
        }

        public virtual void TakeDamage(int damage, Vector2 source)
        {
            if (isDefeated || invincibilityTimer > 0) return;

            health -= damage;
            invincibilityTimer = 0.1f;

            // Flash effect
            sprite.Color = Color.Red;
            Add(new Coroutine(FlashRoutine()));

            // Knockback
            Vector2 knockback = (Position - source).SafeNormalize() * 30f;
            Position += knockback;

            if (health <= 0)
            {
                Die();
            }
            else
            {
                stunTimer = 0.5f;
                sprite.Play("hurt");
            }
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            yield return 0.1f;
            sprite.Color = Color.White;
        }

        protected virtual void Die()
        {
            isDefeated = true;
            sprite.Play("defeat");

            // Death particles
            var level = Scene as Level;
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 10, Position, Vector2.One * 8f);
            Audio.Play("event:/game/general/thing_booped", Position);

            Add(new Coroutine(RemoveAfterDelay(0.5f)));
        }

        private IEnumerator RemoveAfterDelay(float delay)
        {
            yield return delay;
            RemoveSelf();
        }

        #endregion
    }

    #region Waddle Dee

    /// <summary>
    /// Basic Kirby enemy - Waddle Dee. Simple patrol behavior, no ability granted.
    /// </summary>
    [CustomEntity("MaggyHelper/WaddleDee")]
    [Tracked]
    public class WaddleDee : KirbySmallEnemy
    {
        public override KirbyMode.KirbyPowerState CopyAbility => KirbyMode.KirbyPowerState.None;
        public override int ContactDamage => 1;

        private float walkTimer;
        private bool isWalking;

        public WaddleDee(EntityData data, Vector2 offset) : base(data.Position + offset, "characters/waddledee/")
        {
            walkSpeed = 30f;
            patrolDistance = 48f;
        }

        protected override void SetupAnimations()
        {
            sprite.AddLoop("idle", "idle", 0.15f);
            sprite.AddLoop("walk", "walk", 0.12f);
            sprite.Add("hurt", "hurt", 0.1f, "idle");
            sprite.Add("defeat", "defeat", 0.08f);
            sprite.Add("inhaled", "inhaled", 0.1f);
        }

        protected override void SetupStates()
        {
            // Waddle Dee doesn't use state machine, uses simple patrol behavior
        }

        protected override void UpdateBehavior()
        {
            walkTimer -= Engine.DeltaTime;

            if (walkTimer <= 0)
            {
                walkTimer = Calc.Random.Range(1f, 3f);
                isWalking = !isWalking;
                facingRight = Calc.Random.Chance(0.5f);

                if (isWalking)
                {
                    sprite.Play("walk");
                }
                else
                {
                    sprite.Play("idle");
                }
            }

            if (isWalking)
            {
                float direction = facingRight ? 1 : -1;
                MoveH(direction * walkSpeed * Engine.DeltaTime);

                // Turn around at edges or walls
                if (CollideCheck<Solid>(Position + Vector2.UnitX * direction * 8f) ||
                    !CollideCheck<Solid>(Position + new Vector2(direction * 8f, 16f)))
                {
                    facingRight = !facingRight;
                }
            }

            sprite.Scale.X = facingRight ? 1 : -1;
        }
    }

    #endregion

    #region Waddle Doo

    /// <summary>
    /// Waddle Doo - Grants Beam ability when inhaled
    /// </summary>
    [CustomEntity("MaggyHelper/WaddleDoo")]
    [Tracked]
    public class WaddleDoo : KirbySmallEnemy
    {
        public override KirbyMode.KirbyPowerState CopyAbility => KirbyMode.KirbyPowerState.Beam;
        public override int ContactDamage => 1;

        private float beamAttackTimer;
        private bool isChargingBeam;

        public WaddleDoo(EntityData data, Vector2 offset) : base(data.Position + offset, "characters/waddledoo/")
        {
            walkSpeed = 25f;
            maxHealth = 2;
            health = maxHealth;
        }

        protected override void SetupAnimations()
        {
            sprite.AddLoop("idle", "idle", 0.15f);
            sprite.AddLoop("walk", "walk", 0.12f);
            sprite.Add("charge", "charge", 0.08f, "idle");
            sprite.Add("attack", "attack", 0.1f, "idle");
            sprite.Add("hurt", "hurt", 0.1f, "idle");
            sprite.Add("defeat", "defeat", 0.08f);
            sprite.Add("inhaled", "inhaled", 0.1f);
        }

        protected override void SetupStates()
        {
            // Simple patrol + beam attack
        }

        protected override void UpdateBehavior()
        {
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();

            // Check if player is in range for beam attack
            if (player != null)
            {
                float distance = Vector2.Distance(Position, player.Position);
                float xDistance = Math.Abs(player.Position.X - Position.X);
                bool facingPlayer = (player.Position.X > Position.X) == facingRight;

                if (distance < 120f && xDistance < 100f && facingPlayer && beamAttackTimer <= 0)
                {
                    StartBeamAttack();
                }
            }

            if (isChargingBeam)
            {
                beamAttackTimer -= Engine.DeltaTime;

                if (beamAttackTimer <= 0.3f && sprite.CurrentAnimationID == "charge")
                {
                    FireBeam();
                }

                if (beamAttackTimer <= 0)
                {
                    isChargingBeam = false;
                    sprite.Play("idle");
                }
            }
            else
            {
                // Patrol behavior
                float direction = facingRight ? 1 : -1;
                MoveH(direction * walkSpeed * Engine.DeltaTime);

                // Turn around at edges or walls
                if (CollideCheck<Solid>(Position + Vector2.UnitX * direction * 8f) ||
                    !CollideCheck<Solid>(Position + new Vector2(direction * 8f, 16f)))
                {
                    facingRight = !facingRight;
                }

                sprite.Scale.X = facingRight ? 1 : -1;

                // Randomly idle
                if (Calc.Random.Chance(0.01f))
                {
                    sprite.Play("idle");
                }
                else if (sprite.CurrentAnimationID == "idle" && Calc.Random.Chance(0.02f))
                {
                    sprite.Play("walk");
                }
            }

            if (beamAttackTimer > 0 && !isChargingBeam)
            {
                beamAttackTimer -= Engine.DeltaTime;
            }
        }

        private void StartBeamAttack()
        {
            isChargingBeam = true;
            beamAttackTimer = 1f;
            sprite.Play("charge");

            // Face player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                facingRight = player.Position.X > Position.X;
                sprite.Scale.X = facingRight ? 1 : -1;
            }
        }

        private void FireBeam()
        {
            sprite.Play("attack");
            Audio.Play("event:/char/badeline/boss_bullet", Position);

            // Spawn beam whip projectile
            float direction = facingRight ? 1 : -1;
            Scene.Add(new global::Celeste.Projectiles.BeamWhip(Position + new Vector2(direction * 16f, 0f), direction));
        }
    }

    #endregion

    #region Bronto Burt

    /// <summary>
    /// Flying enemy that swoops at the player
    /// </summary>
    [CustomEntity("MaggyHelper/BrontoBurt")]
    [Tracked]
    public class BrontoBurt : KirbySmallEnemy
    {
        public override KirbyMode.KirbyPowerState CopyAbility => KirbyMode.KirbyPowerState.None;
        public override int ContactDamage => 1;
        public override bool CanBeInhaled => true;

        private float flyHeight;
        private float swoopTimer;
        private bool isSwooping;
        private Vector2 targetPosition;

        public BrontoBurt(EntityData data, Vector2 offset) : base(data.Position + offset, "characters/brontoburt/")
        {
            walkSpeed = 60f;
            flyHeight = data.Float("flyHeight", 60f);
            maxHealth = 1;
            health = maxHealth;

            // Flying enemies don't collide with ground
            Collider = new Hitbox(16f, 16f, -8f, -8f);
        }

        protected override void SetupAnimations()
        {
            sprite.AddLoop("fly", "fly", 0.08f);
            sprite.AddLoop("swoop", "swoop", 0.05f);
            sprite.Add("hurt", "hurt", 0.1f, "fly");
            sprite.Add("defeat", "defeat", 0.08f);
            sprite.Add("inhaled", "inhaled", 0.1f);
        }

        protected override void SetupStates()
        {
            sprite.Play("fly");
        }

        protected override void UpdateBehavior()
        {
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();

            if (isSwooping)
            {
                // Swoop toward target
                Vector2 direction = (targetPosition - Position).SafeNormalize();
                Position += direction * walkSpeed * 1.5f * Engine.DeltaTime;

                // End swoop when close to target or timer expires
                swoopTimer -= Engine.DeltaTime;
                if (swoopTimer <= 0 || Vector2.Distance(Position, targetPosition) < 10f)
                {
                    isSwooping = false;
                    sprite.Play("fly");
                }
            }
            else
            {
                // Hover in place
                Position.Y = startPosition.Y + (float)Math.Sin(Scene.TimeActive * 2f) * 8f;

                // Check for player to swoop at
                if (player != null)
                {
                    float xDistance = Math.Abs(player.Position.X - Position.X);
                    float yDistance = player.Position.Y - Position.Y;

                    if (xDistance < 100f && yDistance > 0 && yDistance < 150f && swoopTimer <= 0)
                    {
                        StartSwoop(player.Position);
                    }
                }

                // Patrol horizontally
                float patrolX = startPosition.X + (float)Math.Sin(Scene.TimeActive * 0.5f) * patrolDistance;
                Position.X = Calc.Approach(Position.X, patrolX, walkSpeed * 0.3f * Engine.DeltaTime);
            }

            if (swoopTimer > 0 && !isSwooping)
            {
                swoopTimer -= Engine.DeltaTime;
            }

            // Face movement direction
            sprite.Scale.X = (Position.X > startPosition.X) ? 1 : -1;
        }

        private void StartSwoop(Vector2 target)
        {
            isSwooping = true;
            swoopTimer = 2f;
            targetPosition = target;
            sprite.Play("swoop");
        }
    }

    #endregion

    #region Gordo

    /// <summary>
    /// Spiky invincible enemy - cannot be inhaled or damaged
    /// </summary>
    [CustomEntity("MaggyHelper/Gordo")]
    [Tracked]
    public class Gordo : KirbySmallEnemy
    {
        public override KirbyMode.KirbyPowerState CopyAbility => KirbyMode.KirbyPowerState.Needle;
        public override int ContactDamage => 2;
        public override bool CanBeInhaled => false;

        private Vector2 moveDirection;
        private float bounceSpeed;

        public Gordo(EntityData data, Vector2 offset) : base(data.Position + offset, "characters/gordo/")
        {
            bounceSpeed = data.Float("speed", 40f);
            moveDirection = new Vector2(data.Float("directionX", 1f), data.Float("directionY", 0f));
            if (moveDirection == Vector2.Zero) moveDirection = Vector2.UnitX;
            moveDirection.Normalize();

            maxHealth = 9999; // Invincible
            health = maxHealth;

            Collider = new Circle(8f, 0f, -8f);
        }

        protected override void SetupAnimations()
        {
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.Play("idle");
        }

        protected override void SetupStates()
        {
        }

        protected override void UpdateBehavior()
        {
            // Simple bouncing movement
            Position += moveDirection * bounceSpeed * Engine.DeltaTime;

            // Bounce off solids
            if (CollideCheck<Solid>(Position + moveDirection * 8f))
            {
                moveDirection.X *= -1;
            }

            // Bounce off floor/ceiling
            if (CollideCheck<Solid>(Position + Vector2.UnitY * moveDirection.Y * 8f))
            {
                moveDirection.Y *= -1;
            }

            // Rotate sprite
            sprite.Rotation += Engine.DeltaTime * 2f;
        }

        public override void TakeDamage(int damage, Vector2 source)
        {
            // Gordo is invincible - don't take damage
            // Just play a ping sound
            Audio.Play("event:/game/general/thing_booped", Position);
        }
    }

    #endregion
}

