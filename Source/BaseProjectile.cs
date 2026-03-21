using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities.Projectiles
{
    /// <summary>
    /// Base class for all projectile entities
    /// </summary>
    public abstract class BaseProjectile : Entity
    {
        protected Sprite sprite;
        protected Vector2 velocity;
        protected float lifetime;
        protected float maxLifetime;
        protected bool destroyOnWall;
        protected bool damagesPlayer;
        protected bool damagesEnemies;
        protected int damage;
        
        protected BaseProjectile(Vector2 position, Vector2 velocity) : base(position)
        {
            this.velocity = velocity;
            maxLifetime = 5f;
            destroyOnWall = true;
            damagesPlayer = false;
            damagesEnemies = true;
            damage = 1;
            
            Depth = -50;
        }

        public override void Update()
        {
            base.Update();
            
            lifetime += Engine.DeltaTime;
            
            // Move
            Position += velocity * Engine.DeltaTime;
            
            // Check lifetime
            if (lifetime > maxLifetime)
            {
                Destroy();
                return;
            }
            
            // Check wall collision
            if (destroyOnWall && CollideCheck<Solid>())
            {
                OnHitWall();
                return;
            }
            
            // Check player collision
            if (damagesPlayer)
            {
                var player = CollideFirst<Player>();
                if (player != null)
                {
                    OnHitPlayer(player);
                }
            }
            
            // Check enemy collision
            if (damagesEnemies)
            {
                var enemy = CollideFirst<Enemy>();
                if (enemy != null)
                {
                    OnHitEnemy(enemy);
                }
            }
        }

        protected virtual void OnHitWall()
        {
            PlayHitEffect();
            Destroy();
        }

        protected virtual void OnHitPlayer(Player player)
        {
            player.Die(velocity.SafeNormalize());
            PlayHitEffect();
            Destroy();
        }

        protected virtual void OnHitEnemy(Enemy enemy)
        {
            enemy.TakeDamage(damage);
            PlayHitEffect();
            Destroy();
        }

        protected virtual void PlayHitEffect()
        {
            Audio.Play("event:/game/general/thing_booped", Position);
        }

        protected virtual void Destroy()
        {
            RemoveSelf();
        }
    }

    /// <summary>
    /// Fire projectile for Fire copy ability
    /// </summary>
    public class FireProjectile : BaseProjectile
    {
        public FireProjectile(Vector2 position, int facing) 
            : base(position, new Vector2(facing * 200f, 0))
        {
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            maxLifetime = 1f;
            
            // Add fire particles on update
        }

        public override void Update()
        {
            base.Update();
            
            // Fire trail
            var level = Scene as Level;
            if (Scene.OnInterval(0.05f))
            {
                level?.Particles.Emit(
                    ParticleTypes.Dust,
                    Center,
                    velocity.Angle() + MathHelper.Pi
                );
            }
        }
    }

    /// <summary>
    /// Ice projectile for Ice copy ability
    /// </summary>
    public class IceProjectile : BaseProjectile
    {
        public IceProjectile(Vector2 position, int facing) 
            : base(position, new Vector2(facing * 150f, 0))
        {
            Collider = new Hitbox(16f, 8f, -8f, -4f);
            maxLifetime = 0.5f;
            
            // Ice breath effect
        }

        protected override void OnHitEnemy(Enemy enemy)
        {
            // Freeze enemy
            base.OnHitEnemy(enemy);
        }
    }

    /// <summary>
    /// Sword slash for Sword copy ability
    /// </summary>
    public class SwordSlash : BaseProjectile
    {
        public SwordSlash(Vector2 position, int facing) 
            : base(position + new Vector2(facing * 20f, 0), Vector2.Zero)
        {
            Collider = new Hitbox(30f, 20f, facing > 0 ? 0 : -30f, -10f);
            maxLifetime = 0.2f;
            destroyOnWall = false;
        }
    }

    /// <summary>
    /// Electric field for Spark copy ability
    /// </summary>
    public class SparkField : BaseProjectile
    {
        public SparkField(Vector2 position) 
            : base(position, Vector2.Zero)
        {
            Collider = new Circle(30f);
            maxLifetime = 1f;
            destroyOnWall = false;
        }

        public override void Update()
        {
            // Follow player
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                Position = player.Center;
            }
            
            base.Update();
            
            // Spark particles
            var level = Scene as Level;
            if (Scene.OnInterval(0.05f))
            {
                level?.Particles.Emit(
                    ParticleTypes.Dust,
                    Center + Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, 25f),
                    Color.Yellow
                );
            }
        }
    }

    /// <summary>
    /// Beam projectile for Beam copy ability and Waddle Doo
    /// </summary>
    public class BeamProjectile : BaseProjectile
    {
        public BeamProjectile(Vector2 position, int facing) 
            : base(position, new Vector2(facing * 180f, 0))
        {
            Collider = new Hitbox(20f, 8f, -10f, -4f);
            maxLifetime = 2f;
        }

        public override void Update()
        {
            base.Update();
            
            // Wave motion
            Position.Y += (float)System.Math.Sin(lifetime * 20f) * 2f;
        }
    }

    /// <summary>
    /// Boss star projectile
    /// </summary>
    public class BossStarProjectile : BaseProjectile
    {
        public BossStarProjectile(Vector2 position, Vector2 velocity) 
            : base(position, velocity)
        {
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            damagesPlayer = true;
            damagesEnemies = false;
            maxLifetime = 3f;
        }
    }

    /// <summary>
    /// Sword beam from Meta Knight
    /// </summary>
    public class SwordBeam : BaseProjectile
    {
        public SwordBeam(Vector2 position, Vector2 velocity) 
            : base(position, velocity)
        {
            Collider = new Hitbox(24f, 8f, -12f, -4f);
            damagesPlayer = true;
            damagesEnemies = false;
            maxLifetime = 2f;
        }
    }

    /// <summary>
    /// Ground shockwave from Dedede
    /// </summary>
    public class Shockwave : BaseProjectile
    {
        private int direction;
        
        public Shockwave(Vector2 position, int direction) 
            : base(position, new Vector2(direction * 150f, 0))
        {
            this.direction = direction;
            Collider = new Hitbox(16f, 24f, -8f, -24f);
            damagesPlayer = true;
            damagesEnemies = false;
            maxLifetime = 2f;
            destroyOnWall = true;
        }

        public override void Update()
        {
            base.Update();
            
            // Stay on ground
            while (!CollideCheck<Solid>(Position + Vector2.UnitY))
            {
                Position.Y += 1f;
                if (Position.Y > (Scene as Level).Bounds.Bottom + 100)
                {
                    RemoveSelf();
                    return;
                }
            }
        }
    }
}
