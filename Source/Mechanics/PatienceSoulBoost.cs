using System;
using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.SoulBoosts
{
    /// <summary>
    /// Cyan Soul - Patience
    /// Ability: Slow-motion effect for precise platforming
    /// </summary>
    [CustomEntity(ids: "DZ/PatienceSoulBoost")]
    [Tracked]
    public class PatienceSoulBoost : SoulBoostBase
    {
        public override SoulType Soul => SoulType.Patience;
        public override string SoulName => "Patience";
        protected override float AbilityDuration => 4f;

        private float slowMotionFactor;

        public PatienceSoulBoost(EntityData data, Vector2 offset)
            : base(
                data.NodesWithPosition(offset),
                data.Bool("canSkip", false),
                data.Bool("oneUse", false),
                data.Float("boostSpeed", 320f)
            )
        {
            slowMotionFactor = data.Float("slowMotionFactor", 0.5f);
        }

        public PatienceSoulBoost(
            Vector2[] nodes,
            bool canSkip = false,
            bool oneUse = false,
            float boostSpeed = 320f,
            float slowMotionFactor = 0.5f
        ) : base(nodes, canSkip, oneUse, boostSpeed)
        {
            this.slowMotionFactor = slowMotionFactor;
        }

        protected override IEnumerator ApplyAbilityStart(global::Celeste.Player player)
        {
            // Visual feedback
            Level level = Scene as Level;
            level?.ParticlesFG.Emit(P_Burst, 20, player.Center, Vector2.One * 12f);
            
            Audio.Play("event:/game/general/cassette_bubblereturn", player.Position);
            
            yield return 0.1f;
        }

        protected override IEnumerator ApplyAbilityEnd(global::Celeste.Player player)
        {
            // Apply slow-motion buff
            player.Add(new PatienceBuff(AbilityDuration, slowMotionFactor));
            
            yield break;
        }

        private class PatienceBuff : Component
        {
            private float duration;
            private float timer;
            private float slowFactor;
            private readonly TimeRateModifier timeRateModifier;

            public PatienceBuff(float duration, float slowFactor) : base(true, false)
            {
                this.duration = duration;
                this.timer = duration;
                this.slowFactor = slowFactor;
                this.timeRateModifier = new TimeRateModifier(1f, false);
            }

            public override void Added(Entity entity)
            {
                base.Added(entity);
                entity.Add(timeRateModifier);
                timeRateModifier.SetTimeRateMultiplier(slowFactor);
            }

            public override void Update()
            {
                base.Update();
                
                // Use raw delta time since we're in slow motion
                timer -= Engine.RawDeltaTime;
                
                if (timer <= 0f)
                {
                    RemoveSelf();
                    return;
                }

                // Emit patience particles
                if (Scene.OnRawInterval(0.05f))
                {
                    Player player = Entity as Player;
                    if (player != null)
                    {
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("00ffff"),
                                Color2 = Color.White,
                                ColorMode = ParticleType.ColorModes.Blink,
                                Size = 0.8f,
                                LifeMin = 0.5f,
                                LifeMax = 1f,
                                SpeedMin = 5f,
                                SpeedMax = 15f,
                                DirectionRange = (float)Math.PI * 2f
                            },
                            1,
                            player.Center,
                            Vector2.One * 6f
                        );
                    }
                }
            }

            public override void Removed(Entity entity)
            {
                base.Removed(entity);
                timeRateModifier.ResetTimeRateMultiplier();
                timeRateModifier.RemoveSelf();
            }
        }
    }
}

