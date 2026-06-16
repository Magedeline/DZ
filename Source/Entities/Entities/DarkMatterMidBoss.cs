using Microsoft.Xna.Framework;
using Monocle;

namespace DZ
{
    /// <summary>
    /// Dark Matter Mid Boss - A mid-boss entity for the Void Gate Arena.
    /// Represents a challenging enemy that the player must defeat.
    /// </summary>
    [CustomEntity("DZ/DarkMatterMidBoss")]
    [Tracked]
    [HotReloadable]
    public class DarkMatterMidBoss : Entity
    {
        public enum BossState
        {
            Idle,
            Attacking,
            Hurt,
            Dying
        }

        public BossState CurrentState { get; private set; }
        public float Health { get; private set; }
        public const float MaxHealth = 100f;

        private Vector2 targetPosition;
        private float attackTimer;
        private float hurtTimer;

        public DarkMatterMidBoss(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Health = MaxHealth;
            CurrentState = BossState.Idle;
            attackTimer = 0f;
            hurtTimer = 0f;

            Collider = new Hitbox(64, 64, -32, -32);
        }

        public DarkMatterMidBoss(Vector2 position)
            : base(position)
        {
            Health = MaxHealth;
            CurrentState = BossState.Idle;
            attackTimer = 0f;
            hurtTimer = 0f;

            Collider = new Hitbox(64, 64, -32, -32);
        }

        public override void Update()
        {
            base.Update();

            switch (CurrentState)
            {
                case BossState.Idle:
                    UpdateIdle();
                    break;
                case BossState.Attacking:
                    UpdateAttacking();
                    break;
                case BossState.Hurt:
                    UpdateHurt();
                    break;
                case BossState.Dying:
                    UpdateDying();
                    break;
            }
        }

        private void UpdateIdle()
        {
            attackTimer += Engine.DeltaTime;
            if (attackTimer >= 2f)
            {
                attackTimer = 0f;
                CurrentState = BossState.Attacking;
            }
        }

        private void UpdateAttacking()
        {
            attackTimer += Engine.DeltaTime;
            if (attackTimer >= 1f)
            {
                attackTimer = 0f;
                CurrentState = BossState.Idle;
            }
        }

        private void UpdateHurt()
        {
            hurtTimer += Engine.DeltaTime;
            if (hurtTimer >= 0.5f)
            {
                hurtTimer = 0f;
                CurrentState = BossState.Idle;
            }
        }

        private void UpdateDying()
        {
            hurtTimer += Engine.DeltaTime;
            if (hurtTimer >= 2f)
            {
                RemoveSelf();
            }
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                CurrentState = BossState.Dying;
            }
            else
            {
                CurrentState = BossState.Hurt;
            }
        }
    }
}
