using System;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Projectiles
{
    /// <summary>
    /// Beam whip projectile fired by Waddle Doo
    /// </summary>
    [Tracked]
    public class BeamWhip : Entity
    {
        private float lifetime;
        private float maxLifetime = 0.5f;
        private int damage = 1;
        private float direction;
        private Sprite sprite;
        private Level level;

        public BeamWhip(Vector2 position, float dir) : base(position)
        {
            direction = dir;
            Depth = -50;
            Collider = new Hitbox(32f, 8f, direction > 0 ? 0 : -32f, -4f);

            Add(sprite = new Sprite(GFX.Game, "projectiles/beam/"));
            sprite.AddLoop("whip", "whip", 0.05f);
            sprite.Play("whip");
            sprite.Scale.X = direction;

            Add(new PlayerCollider(OnPlayerHit));
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
            if (lifetime > maxLifetime)
            {
                RemoveSelf();
            }
        }

        private void OnPlayerHit(global::Celeste.Player player)
        {
            if (player.IsKirbyMode())
            {
                var controller = KirbyHealthController.Instance;
                if (controller != null)
                {
                    controller.DamageFromEnemy(Position, damage);
                }
                else
                {
                    PlayerHealthManager.TryDamagePlayer(damage, Position);
                }
            }
            else
            {
                player.Die((player.Position - Position).SafeNormalize());
            }

            // Don't remove - beam passes through
        }
    }

    /// <summary>
    /// Bomb projectile thrown by Poppy Bros Jr
    /// </summary>
    [Tracked]
    public class BombProjectile : Entity
    {
        private Vector2 velocity;
        private int damage;
        private float gravity = 400f;
        private float lifetime;
        private float maxLifetime = 3f;
        private Sprite sprite;
        private Level level;
        private bool exploded;

        public BombProjectile(Vector2 position, Vector2 vel, int dmg) : base(position)
        {
            velocity = vel;
            damage = dmg;
            Depth = -50;
            Collider = new Circle(8f, 0f, -8f);

            Add(sprite = new Sprite(GFX.Game, "projectiles/bomb/"));
            sprite.AddLoop("spin", "spin", 0.1f);
            sprite.Play("spin");

            Add(new PlayerCollider(OnPlayerHit));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            if (exploded) return;

            // Apply gravity
            velocity.Y += gravity * Engine.DeltaTime;

            // Move
            Position += velocity * Engine.DeltaTime;

            // Rotate based on horizontal speed
            sprite.Rotation += velocity.X * 0.01f * Engine.DeltaTime;

            // Bounce off solids
            if (CollideCheck<Solid>())
            {
                velocity.Y *= -0.6f;
                velocity.X *= 0.8f;
                Position.Y -= 4f;
            }

            // Explode after lifetime or when hitting floor with low velocity
            lifetime += Engine.DeltaTime;
            if (lifetime > maxLifetime || (velocity.Y > 0 && CollideCheck<Solid>(Position + Vector2.UnitY * 4f)))
            {
                Explode();
            }
        }

        private void OnPlayerHit(global::Celeste.Player player)
        {
            Explode();
        }

        private void Explode()
        {
            if (exploded) return;
            exploded = true;

            Audio.Play("event:/char/badeline/boss_bullet_impact", Position);

            // Explosion particles
            for (int i = 0; i < 15; i++)
            {
                float angle = (i / 15f) * (float)Math.PI * 2f;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                level?.ParticlesFG.Emit(ParticleTypes.Dust, Position + dir * 8f);
            }

            // Screen shake
            level?.Shake(0.2f);

            // Damage nearby player
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < 40f)
            {
                if (player.IsKirbyMode())
                {
                    var controller = KirbyHealthController.Instance;
                    if (controller != null)
                    {
                        controller.DamageFromEnemy(Position, damage);
                    }
                    else
                    {
                        PlayerHealthManager.TryDamagePlayer(damage, Position);
                    }
                }
                else
                {
                    player.Die((player.Position - Position).SafeNormalize());
                }
            }

            sprite.Play("explode");
            Add(new Coroutine(RemoveAfterExplosion()));
        }

        private IEnumerator RemoveAfterExplosion()
        {
            yield return 0.3f;
            RemoveSelf();
        }
    }

    /// <summary>
    /// Boss star projectile fired by Kirby Boss
    /// </summary>
    [Tracked]
    public class BossStarProjectile : Entity
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime = 5f;
        private Sprite sprite;
        private Level level;

        public BossStarProjectile(Vector2 position, Vector2 vel) : base(position)
        {
            velocity = vel;
            Depth = -50;
            Collider = new Hitbox(12f, 12f, -6f, -6f);

            Add(sprite = new Sprite(GFX.Game, "projectiles/star/"));
            sprite.AddLoop("star", "star", 0.05f);
            sprite.Play("star");

            Add(new PlayerCollider(OnPlayerHit));
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

            sprite.Rotation += Engine.DeltaTime * 5f;

            // Trail particles
            if (Scene.OnInterval(0.05f))
            {
                level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, Position);
            }

            if (lifetime > maxLifetime || CollideCheck<Solid>())
            {
                RemoveSelf();
            }
        }

        private void OnPlayerHit(global::Celeste.Player player)
        {
            if (player.IsKirbyMode())
            {
                var controller = KirbyHealthController.Instance;
                if (controller != null)
                {
                    controller.DamageFromBoss(Position, 1);
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

            RemoveSelf();
        }
    }

    /// <summary>
    /// Fire projectile
    /// </summary>
    [Tracked]
    public class FireProjectile : Entity
    {
        private int direction;
        private float lifetime;
        private float maxLifetime = 2f;
        private Sprite sprite;
        private Level level;

        public FireProjectile(Vector2 position, int dir) : base(position)
        {
            direction = dir;
            Depth = -50;
            Collider = new Hitbox(16f, 8f, direction > 0 ? 0 : -16f, -4f);

            Add(sprite = new Sprite(GFX.Game, "projectiles/fire/"));
            sprite.AddLoop("fire", "fire", 0.08f);
            sprite.Play("fire");
            sprite.Scale.X = dir;

            Add(new PlayerCollider(OnPlayerHit));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            Position.X += direction * 120f * Engine.DeltaTime;
            lifetime += Engine.DeltaTime;

            if (Scene.OnInterval(0.1f))
            {
                level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, Position);
            }

            if (lifetime > maxLifetime || CollideCheck<Solid>())
            {
                RemoveSelf();
            }
        }

        private void OnPlayerHit(global::Celeste.Player player)
        {
            if (player.IsKirbyMode())
            {
                var controller = KirbyHealthController.Instance;
                if (controller != null)
                {
                    controller.DamageFromEnemy(Position, 1);
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

            RemoveSelf();
        }
    }

    /// <summary>
    /// Ice projectile
    /// </summary>
    [Tracked]
    public class IceProjectile : Entity
    {
        private int direction;
        private float lifetime;
        private float maxLifetime = 2f;
        private Sprite sprite;
        private Level level;

        public IceProjectile(Vector2 position, int dir) : base(position)
        {
            direction = dir;
            Depth = -50;
            Collider = new Hitbox(12f, 8f, direction > 0 ? 0 : -12f, -4f);

            Add(sprite = new Sprite(GFX.Game, "projectiles/ice/"));
            sprite.AddLoop("ice", "ice", 0.08f);
            sprite.Play("ice");
            sprite.Scale.X = dir;

            Add(new PlayerCollider(OnPlayerHit));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            Position.X += direction * 100f * Engine.DeltaTime;
            lifetime += Engine.DeltaTime;

            if (Scene.OnInterval(0.1f))
            {
                level?.ParticlesFG.Emit(ParticleTypes.Dust, Position);
            }

            if (lifetime > maxLifetime || CollideCheck<Solid>())
            {
                // Freeze effect on impact
                Audio.Play("event:/game/general/thing_booped", Position);
                RemoveSelf();
            }
        }

        private void OnPlayerHit(global::Celeste.Player player)
        {
            if (player.IsKirbyMode())
            {
                var controller = KirbyHealthController.Instance;
                if (controller != null)
                {
                    controller.DamageFromEnemy(Position, 1);
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

            RemoveSelf();
        }
    }

    /// <summary>
    /// Beam projectile (wider than beam whip, for boss use)
    /// </summary>
    [Tracked]
    public class BeamProjectile : Entity
    {
        private int direction;
        private float lifetime;
        private float maxLifetime = 1.5f;
        private Sprite sprite;
        private Level level;

        public BeamProjectile(Vector2 position, int dir) : base(position)
        {
            direction = dir;
            Depth = -50;
            Collider = new Hitbox(40f, 8f, direction > 0 ? 0 : -40f, -4f);

            Add(sprite = new Sprite(GFX.Game, "projectiles/beam/"));
            sprite.AddLoop("beam", "beam", 0.05f);
            sprite.Play("beam");
            sprite.Scale.X = dir;

            Add(new PlayerCollider(OnPlayerHit));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();

            Position.X += direction * 150f * Engine.DeltaTime;
            lifetime += Engine.DeltaTime;

            if (Scene.OnInterval(0.05f))
            {
                level?.ParticlesFG.Emit(ParticleTypes.SparkyDust, Position);
            }

            if (lifetime > maxLifetime || CollideCheck<Solid>())
            {
                RemoveSelf();
            }
        }

        private void OnPlayerHit(global::Celeste.Player player)
        {
            if (player.IsKirbyMode())
            {
                var controller = KirbyHealthController.Instance;
                if (controller != null)
                {
                    controller.DamageFromEnemy(Position, 1);
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

            RemoveSelf();
        }
    }
}

