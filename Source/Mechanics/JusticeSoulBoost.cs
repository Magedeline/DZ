using System;
using System.Collections;
using System.Collections.Generic;
using DZ;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.SoulBoosts
{
    /// <summary>
    /// Yellow Soul - Justice
    /// Ability: Fires projectiles in the direction of travel
    /// Projectiles can destroy certain hazards and stun enemies
    /// </summary>
    [CustomEntity(ids: "DZ/JusticeSoulBoost")]
    [Tracked]
    public class JusticeSoulBoost : SoulBoostBase
    {
        public override SoulType Soul => SoulType.Justice;
        public override string SoulName => "Justice";
        protected override float AbilityDuration => 2f;

        private int projectileCount;
        private float projectileSpeed;
        private bool spreadShot;

        public JusticeSoulBoost(EntityData data, Vector2 offset)
            : base(
                data.NodesWithPosition(offset),
                data.Bool("canSkip", false),
                data.Bool("oneUse", false),
                data.Float("boostSpeed", 320f)
            )
        {
            projectileCount = data.Int("projectileCount", 5);
            projectileSpeed = data.Float("projectileSpeed", 400f);
            spreadShot = data.Bool("spreadShot", true);
        }

        public JusticeSoulBoost(
            Vector2[] nodes,
            bool canSkip = false,
            bool oneUse = false,
            float boostSpeed = 320f,
            int projectileCount = 5,
            float projectileSpeed = 400f,
            bool spreadShot = true
        ) : base(nodes, canSkip, oneUse, boostSpeed)
        {
            this.projectileCount = projectileCount;
            this.projectileSpeed = projectileSpeed;
            this.spreadShot = spreadShot;
        }

        protected override IEnumerator ApplyAbilityStart(global::Celeste.Player player)
        {
            // Visual feedback
            Level level = Scene as Level;
            level?.ParticlesFG.Emit(P_Burst, 20, player.Center, Vector2.One * 12f);
            
            Audio.Play("event:/game/general/diamond_touch", player.Position);
            
            yield return 0.1f;
        }

        protected override IEnumerator ApplyAbilityEnd(global::Celeste.Player player)
        {
            // Fire projectiles in direction of travel
            Vector2 direction = player.Speed.SafeNormalize();
            if (direction == Vector2.Zero)
            {
                direction = player.Facing == Facings.Right ? Vector2.UnitX : -Vector2.UnitX;
            }

            // Spawn projectiles
            Level level = Scene as Level;
            
            if (spreadShot && projectileCount > 1)
            {
                // Fan spread pattern
                float totalAngle = MathHelper.ToRadians(60f);
                float angleStep = totalAngle / (projectileCount - 1);
                float startAngle = direction.Angle() - totalAngle / 2f;

                for (int i = 0; i < projectileCount; i++)
                {
                    float angle = startAngle + angleStep * i;
                    Vector2 projDir = Calc.AngleToVector(angle, 1f);
                    
                    SpawnJusticeProjectile(player.Center, projDir, level);
                }
            }
            else
            {
                // Single direction
                for (int i = 0; i < projectileCount; i++)
                {
                    SpawnJusticeProjectile(player.Center + new Vector2(0, i * 4f - projectileCount * 2f), direction, level);
                    yield return 0.05f;
                }
            }

            // Apply buff for additional shots on dash
            player.Add(new JusticeBuff(AbilityDuration, projectileSpeed));
            
            yield break;
        }

        private void SpawnJusticeProjectile(Vector2 position, Vector2 direction, Level level)
        {
            JusticeProjectile projectile = new JusticeProjectile(position, direction * projectileSpeed);
            level?.Add(projectile);
            
            Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", position);
        }

        private class JusticeBuff : Component
        {
            private float duration;
            private float timer;
            private float projectileSpeed;
            private float shotCooldown;

            public JusticeBuff(float duration, float projectileSpeed) : base(true, false)
            {
                this.duration = duration;
                this.timer = duration;
                this.projectileSpeed = projectileSpeed;
                this.shotCooldown = 0f;
            }

            public override void Update()
            {
                base.Update();
                
                timer -= Engine.DeltaTime;
                shotCooldown -= Engine.DeltaTime;
                
                if (timer <= 0f)
                {
                    RemoveSelf();
                    return;
                }

                Player player = Entity as Player;
                if (player != null)
                {
                    // Fire projectile on dash
                    if (player.StateMachine.State == Player.StDash && shotCooldown <= 0f)
                    {
                        Vector2 direction = player.DashDir;
                        if (direction == Vector2.Zero)
                        {
                            direction = player.Facing == Facings.Right ? Vector2.UnitX : -Vector2.UnitX;
                        }

                        Level level = Scene as Level;
                        JusticeProjectile projectile = new JusticeProjectile(
                            player.Center, 
                            direction * projectileSpeed
                        );
                        level?.Add(projectile);
                        
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", player.Position);
                        shotCooldown = 0.2f;
                    }

                    // Yellow glow effect
                    float alpha = (float)Math.Sin(timer * 12f) * 0.3f + 0.5f;
                    player.Sprite.Color = Color.Lerp(Color.White, Calc.HexToColor("ffff00"), alpha * 0.4f);

                    // Emit justice particles
                    if (Scene.OnInterval(0.1f))
                    {
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("ffff00"),
                                Color2 = Color.White,
                                ColorMode = ParticleType.ColorModes.Blink,
                                Size = 0.8f,
                                LifeMin = 0.3f,
                                LifeMax = 0.6f,
                                SpeedMin = 10f,
                                SpeedMax = 30f,
                                DirectionRange = (float)Math.PI * 2f
                            },
                            1,
                            player.Center,
                            Vector2.One * 4f
                        );
                    }
                }
            }

            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                
                Player player = entity as Player;
                if (player != null)
                {
                    player.Sprite.Color = Color.White;
                }
            }
        }
    }

    /// <summary>
    /// Projectile fired by Justice Soul Boost
    /// </summary>
    [Tracked]
    public class JusticeProjectile : Entity
    {
        private Vector2 velocity;
        private float lifetime;
        private Sprite sprite;
        private ParticleType P_Trail;

        public JusticeProjectile(Vector2 position, Vector2 velocity) : base(position)
        {
            this.velocity = velocity;
            this.lifetime = 3f;
            
            Depth = -1000000;
            Collider = new Circle(6f);
            
            Add(new PlayerCollider(player => OnPlayerCollide(player)));
            
            // Create simple sprite/image
            Add(sprite = AtlasPathHelper.CreateSprite("objects/DZ/DZ/sevensoulboost/"));
            sprite.AddLoop("vessel_soul05", "justice_projectile", 0.08f);
            sprite.Play("vessel_soul05");
            sprite.CenterOrigin();
            sprite.Color = Calc.HexToColor("ffff00");

            // Trail particle
            P_Trail = new ParticleType
            {
                Source = GFX.Game["particles/shard"],
                Color = Calc.HexToColor("ffff00"),
                Color2 = Color.Orange,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.2f,
                LifeMax = 0.4f,
                Size = 0.6f,
                SpeedMin = 5f,
                SpeedMax = 15f,
                DirectionRange = (float)Math.PI / 4f
            };
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            // Trail particles
            if (Scene.OnInterval(0.03f))
            {
                (Scene as Level)?.ParticlesBG.Emit(P_Trail, 1, Center, Vector2.One * 2f, velocity.Angle() + (float)Math.PI);
            }
            
            // Rotate sprite based on velocity
            sprite.Rotation = velocity.Angle();
            
            // Check if out of bounds or lifetime expired
            Level level = Scene as Level;
            if (lifetime <= 0f || 
                X < level.Bounds.Left - 50f || X > level.Bounds.Right + 50f ||
                Y < level.Bounds.Top - 50f || Y > level.Bounds.Bottom + 50f)
            {
                RemoveSelf();
                return;
            }
            
            // Check for solid collision
            if (CollideCheck<Solid>())
            {
                OnHitSolid();
            }
        }

        private void OnPlayerCollide(global::Celeste.Player player)
        {
            // Projectiles don't hurt player
        }

        private void OnHitSolid()
        {
            // Burst effect
            Level level = Scene as Level;
            level?.ParticlesFG.Emit(P_Trail, 8, Center, Vector2.One * 4f);
            
            Audio.Play("event:/game/general/wall_break_stone", Position);
            
            RemoveSelf();
        }

        public override void Render()
        {
            // Glow effect
            Draw.Circle(Center, 8f, Calc.HexToColor("ffff00") * 0.3f, 8);
            Draw.Circle(Center, 4f, Calc.HexToColor("ffff00") * 0.5f, 4);
            
            base.Render();
        }
    }
}

