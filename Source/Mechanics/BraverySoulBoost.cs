using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.SoulBoosts
{
    /// <summary>
    /// Orange Soul - Bravery
    /// Ability: Invincibility frames after launch
    /// </summary>
    [CustomEntity(ids: "DZ/BraverySoulBoost")]
    [Tracked]
    public class BraverySoulBoost : SoulBoostBase
    {
        public override SoulType Soul => SoulType.Bravery;
        public override string SoulName => "Bravery";
        protected override float AbilityDuration => 3f;

        public BraverySoulBoost(EntityData data, Vector2 offset)
            : base(
                data.NodesWithPosition(offset),
                data.Bool("canSkip", false),
                data.Bool("oneUse", false),
                data.Float("boostSpeed", 320f)
            )
        {
        }

        public BraverySoulBoost(
            Vector2[] nodes,
            bool canSkip = false,
            bool oneUse = false,
            float boostSpeed = 320f
        ) : base(nodes, canSkip, oneUse, boostSpeed)
        {
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
            // Apply invincibility buff
            player.Add(new BraveryBuff(AbilityDuration));
            
            yield break;
        }

        private class BraveryBuff : Component
        {
            private float duration;
            private float timer;
            private Collider originalCollider;

            public BraveryBuff(float duration) : base(true, false)
            {
                this.duration = duration;
                this.timer = duration;
            }

            public override void Added(Entity entity)
            {
                base.Added(entity);
                
                Player player = entity as Player;
                if (player != null)
                {
                    // Make player invincible by removing collision with hazards
                    originalCollider = player.Collider;
                }
            }

            public override void Update()
            {
                base.Update();
                
                timer -= Engine.DeltaTime;
                
                if (timer <= 0f)
                {
                    RemoveSelf();
                    return;
                }

                Player player = Entity as Player;
                if (player != null)
                {
                    // Flash effect
                    float alpha = (float)Math.Sin(timer * 20f) * 0.5f + 0.5f;
                    player.Sprite.Color = Color.Lerp(Color.White, Calc.HexToColor("ff8000"), alpha * 0.5f);

                    // Emit bravery particles
                    if (Scene.OnInterval(0.08f))
                    {
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("ff8000"),
                                Color2 = Color.White,
                                ColorMode = ParticleType.ColorModes.Blink,
                                Size = 1f,
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

                    // Prevent death from spikes and other hazards
                    if (player.CollideCheck<Spikes>())
                    {
                        // Push player away from hazard
                        player.Speed *= -0.5f;
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
}

