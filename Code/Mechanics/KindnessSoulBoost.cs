using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.SoulBoosts
{
    /// <summary>
    /// Green Soul - Kindness
    /// Ability: Healing / shield effect
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/KindnessSoulBoost")]
    [Tracked]
    public class KindnessSoulBoost : SoulBoostBase
    {
        public override SoulType Soul => SoulType.Kindness;
        public override string SoulName => "Kindness";
        protected override float AbilityDuration => 4f;

        private int shieldHits;

        public KindnessSoulBoost(EntityData data, Vector2 offset)
            : base(
                data.NodesWithPosition(offset),
                data.Bool("canSkip", false),
                data.Bool("oneUse", false),
                data.Float("boostSpeed", 320f)
            )
        {
            shieldHits = data.Int("shieldHits", 3);
        }

        public KindnessSoulBoost(
            Vector2[] nodes,
            bool canSkip = false,
            bool oneUse = false,
            float boostSpeed = 320f,
            int shieldHits = 3
        ) : base(nodes, canSkip, oneUse, boostSpeed)
        {
            this.shieldHits = shieldHits;
        }

        protected override IEnumerator ApplyAbilityStart(global::Celeste.Player player)
        {
            // Visual feedback
            Level level = Scene as Level;
            level?.ParticlesFG.Emit(P_Burst, 20, player.Center, Vector2.One * 12f);
            
            Audio.Play("event:/game/general/seed_touch", player.Position);
            
            yield return 0.1f;
        }

        protected override IEnumerator ApplyAbilityEnd(global::Celeste.Player player)
        {
            // Apply shield buff
            player.Add(new KindnessBuff(AbilityDuration, shieldHits));
            
            yield break;
        }

        private class KindnessBuff : Component
        {
            private float duration;
            private float timer;
            private int hitsRemaining;
            private float shieldRotation;

            public KindnessBuff(float duration, int hits) : base(true, true)
            {
                this.duration = duration;
                this.timer = duration;
                this.hitsRemaining = hits;
            }

            public override void Update()
            {
                base.Update();
                
                timer -= Engine.DeltaTime;
                shieldRotation += Engine.DeltaTime * 2f;
                
                if (timer <= 0f || hitsRemaining <= 0)
                {
                    RemoveSelf();
                    return;
                }

                Player player = Entity as Player;
                if (player != null)
                {
                    // Check for hazard collision and block it
                    if (player.CollideCheck<Spikes>())
                    {
                        hitsRemaining--;
                        
                        // Shield break effect
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("00ff00"),
                                Color2 = Color.White,
                                ColorMode = ParticleType.ColorModes.Blink,
                                Size = 1.5f,
                                LifeMin = 0.5f,
                                LifeMax = 1f,
                                SpeedMin = 40f,
                                SpeedMax = 80f,
                                DirectionRange = (float)Math.PI * 2f
                            },
                            20,
                            player.Center,
                            Vector2.One * 8f
                        );
                        
                        Audio.Play("event:/game/general/crystalheart_pulse", player.Position);
                        
                        // Push player away from hazard
                        player.Speed *= -0.8f;
                        
                        if (hitsRemaining <= 0)
                        {
                            RemoveSelf();
                            return;
                        }
                    }

                    // Emit kindness particles
                    if (Scene.OnInterval(0.15f))
                    {
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("00ff00"),
                                Color2 = Color.White,
                                ColorMode = ParticleType.ColorModes.Blink,
                                Size = 0.8f,
                                LifeMin = 0.4f,
                                LifeMax = 0.8f,
                                SpeedMin = 10f,
                                SpeedMax = 25f,
                                DirectionRange = (float)Math.PI * 2f
                            },
                            1,
                            player.Center,
                            Vector2.One * 6f
                        );
                    }
                }
            }

            public override void Render()
            {
                base.Render();
                
                Player player = Entity as Player;
                if (player != null)
                {
                    // Draw shield circles
                    for (int i = 0; i < hitsRemaining; i++)
                    {
                        float angle = shieldRotation + (i * MathHelper.TwoPi / 3f);
                        float radius = 20f + (float)Math.Sin(timer * 4f) * 2f;
                        Vector2 offset = Calc.AngleToVector(angle, radius);
                        
                        Draw.Circle(player.Center + offset, 4f, Calc.HexToColor("00ff00") * 0.6f, 8);
                        Draw.Circle(player.Center + offset, 2f, Color.White * 0.8f, 6);
                    }
                    
                    // Draw shield aura
                    float alpha = (float)Math.Sin(timer * 6f) * 0.2f + 0.3f;
                    Draw.Circle(player.Center, 24f, Calc.HexToColor("00ff00") * alpha, 16);
                }
            }
        }
    }
}

