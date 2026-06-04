using System.Collections;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities.SoulBoosts
{
    /// <summary>
    /// Red Soul - Determination
    /// Ability: Extra dash + increased launch power
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/DeterminationSoulBoost")]
    [Tracked]
    public class DeterminationSoulBoost : SoulBoostBase
    {
        public override SoulType Soul => SoulType.Determination;
        public override string SoulName => "Determination";
        protected override float AbilityDuration => 3f;

        private int extraDashes;

        public DeterminationSoulBoost(EntityData data, Vector2 offset)
            : base(
                data.NodesWithPosition(offset),
                data.Bool("canSkip", false),
                data.Bool("oneUse", false),
                data.Float("boostSpeed", 320f)
            )
        {
            extraDashes = data.Int("extraDashes", 1);
        }

        public DeterminationSoulBoost(
            Vector2[] nodes,
            bool canSkip = false,
            bool oneUse = false,
            float boostSpeed = 320f,
            int extraDashes = 1
        ) : base(nodes, canSkip, oneUse, boostSpeed)
        {
            this.extraDashes = extraDashes;
        }

        protected override IEnumerator ApplyAbilityStart(global::Celeste.Player player)
        {
            // Grant extra dashes
            player.Dashes = Math.Min(player.Inventory.Dashes + extraDashes, 2);
            
            // Visual feedback
            Level level = Scene as Level;
            level?.ParticlesFG.Emit(P_Burst, 20, player.Center, Vector2.One * 12f);
            
            Audio.Play("guid://{a47180e1-9bc2-4dbc-86c7-6402cea2e2be}", player.Position);
            
            yield return 0.1f;
        }

        protected override IEnumerator ApplyAbilityEnd(global::Celeste.Player player)
        {
            // Increased launch power (1.5x normal)
            player.Speed *= 1.5f;
            
            // Add determination buff component
            player.Add(new DeterminationBuff(AbilityDuration));
            
            yield break;
        }

        private class DeterminationBuff : Component
        {
            private float duration;
            private float timer;

            public DeterminationBuff(float duration) : base(true, false)
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

                // Emit determination particles
                if (Scene.OnInterval(0.1f))
                {
                    Player player = Entity as Player;
                    if (player != null)
                    {
                        (Scene as Level)?.ParticlesFG.Emit(
                            new ParticleType
                            {
                                Source = GFX.Game["particles/shard"],
                                Color = Calc.HexToColor("ff0000"),
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
        }
    }
}

