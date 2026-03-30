using System;
using System.Collections;
using MaggyHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace MaggyHelper.Entities.SoulBoosts
{
    /// <summary>
    /// Blue Soul - Integrity
    /// Ability: Increased movement speed
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/IntegritySoulBoost")]
    [Tracked]
    public class IntegritySoulBoost : SoulBoostBase
    {
        public override SoulType Soul => SoulType.Integrity;
        public override string SoulName => "Integrity";
        protected override float AbilityDuration => 5f;

        private float speedMultiplier;

        public IntegritySoulBoost(EntityData data, Vector2 offset)
            : base(
                data.NodesWithPosition(offset),
                data.Bool("canSkip", false),
                data.Bool("oneUse", false),
                data.Float("boostSpeed", 320f)
            )
        {
            speedMultiplier = data.Float("speedMultiplier", 1.5f);
        }

        public IntegritySoulBoost(
            Vector2[] nodes,
            bool canSkip = false,
            bool oneUse = false,
            float boostSpeed = 320f,
            float speedMultiplier = 1.5f
        ) : base(nodes, canSkip, oneUse, boostSpeed)
        {
            this.speedMultiplier = speedMultiplier;
        }

        protected override IEnumerator ApplyAbilityStart(Celeste.Player player)
        {
            // Visual feedback
            Level level = Scene as Level;
            level?.ParticlesFG.Emit(P_Burst, 20, player.Center, Vector2.One * 12f);
            
            Audio.Play("event:/game/general/cassette_bubblereturn", player.Position);
            
            yield return 0.1f;
        }

        protected override IEnumerator ApplyAbilityEnd(Celeste.Player player)
        {
            // Apply speed buff
            player.Add(new IntegrityBuff(AbilityDuration, speedMultiplier));
            
            yield break;
        }

        private class IntegrityBuff : Component
        {
            private float duration;
            private float timer;
            private float speedMult;

            public IntegrityBuff(float duration, float speedMult) : base(true, false)
            {
                this.duration = duration;
                this.timer = duration;
                this.speedMult = speedMult;
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
                    // Increase movement speed
                    if (Math.Abs(player.Speed.X) > 0.1f)
                    {
                        player.Speed.X *= speedMult;
                    }

                    // Speed trail effect
                    if (Scene.OnInterval(0.05f) && Math.Abs(player.Speed.X) > 50f)
                    {
                        TrailManager.Add(player, Calc.HexToColor("0000ff"), 0.3f);
                        
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("0000ff"),
                                Color2 = Color.White,
                                ColorMode = ParticleType.ColorModes.Blink,
                                Size = 0.8f,
                                LifeMin = 0.2f,
                                LifeMax = 0.4f,
                                SpeedMin = 5f,
                                SpeedMax = 15f,
                                DirectionRange = (float)Math.PI * 2f
                            },
                            1,
                            player.Center,
                            Vector2.One * 4f
                        );
                    }

                    // Visual speed lines
                    if (Scene.OnInterval(0.1f))
                    {
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("0000ff"),
                                Color2 = Color.Cyan,
                                ColorMode = ParticleType.ColorModes.Fade,
                                Size = 0.6f,
                                LifeMin = 0.3f,
                                LifeMax = 0.5f,
                                SpeedMin = 20f,
                                SpeedMax = 40f,
                                Direction = player.Speed.X > 0 ? (float)Math.PI : 0f,
                                DirectionRange = 0.3f
                            },
                            2,
                            player.Center,
                            Vector2.One * 8f
                        );
                    }
                }
            }
        }
    }
}
