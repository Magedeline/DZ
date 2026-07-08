using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.SoulBoosts
{
    /// <summary>
    /// Purple Soul - Perseverance
    /// Ability: Extended stamina / no stamina drain
    /// </summary>
    [CustomEntity(ids: "DZ/PerseveranceSoulBoost")]
    [Tracked]
    public class PerseveranceSoulBoost : SoulBoostBase
    {
        public override SoulType Soul => SoulType.Perseverance;
        public override string SoulName => "Perseverance";
        protected override float AbilityDuration => 6f;

        public PerseveranceSoulBoost(EntityData data, Vector2 offset)
            : base(
                data.NodesWithPosition(offset),
                data.Bool("canSkip", false),
                data.Bool("oneUse", false),
                data.Float("boostSpeed", 320f)
            )
        {
        }

        public PerseveranceSoulBoost(
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
            // Apply infinite stamina buff
            player.Add(new PerseveranceBuff(AbilityDuration));
            
            yield break;
        }

        private class PerseveranceBuff : Component
        {
            private float duration;
            private float timer;

            public PerseveranceBuff(float duration) : base(true, false)
            {
                this.duration = duration;
                this.timer = duration;
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
                    // Infinite stamina
                    player.Stamina = 110f;

                    // Purple aura effect
                    float alpha = (float)Math.Sin(timer * 8f) * 0.3f + 0.5f;
                    
                    // Emit perseverance particles
                    if (Scene.OnInterval(0.1f))
                    {
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("ff00ff"),
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

                    // Climbing particles
                    if (player.StateMachine.State == Player.StClimb && Scene.OnInterval(0.05f))
                    {
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("ff00ff"),
                                Color2 = Calc.HexToColor("cc00cc"),
                                ColorMode = ParticleType.ColorModes.Fade,
                                Size = 1f,
                                LifeMin = 0.3f,
                                LifeMax = 0.6f,
                                SpeedMin = 15f,
                                SpeedMax = 30f,
                                DirectionRange = (float)Math.PI * 2f
                            },
                            2,
                            player.Center,
                            Vector2.One * 4f
                        );
                    }
                }
            }
        }
    }
}

