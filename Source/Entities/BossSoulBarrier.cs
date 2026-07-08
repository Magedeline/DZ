using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// A barrier that blocks the soul's path and triggers a boss cutscene when touched.
    /// Used by the top-down 8-directional soul/battle path to gate boss encounters.
    /// </summary>
    [CustomEntity("DZ/BossSoulBarrier")]
    [Tracked]
    [HotReloadable]
    public class BossSoulBarrier : Solid
    {
        public enum BarrierBoss
        {
            TitanKing,
            GuardianTitan,
            Chapter16Els,
            AsrielAngelOfDeath,
            ElsTrueFinal,
            AsrielBreakGiygas
        }

        private BarrierBoss bossType;
        private string barrierId;
        private bool triggered;
        private bool breakAfterCutscene;
        private Color barrierColor;
        private float pulseTimer;
        private List<SoulPlayer> touchingSouls = new List<SoulPlayer>();

        public BarrierBoss BossType => bossType;
        public string BarrierId => barrierId;
        public bool IsTriggered => triggered;

        public BossSoulBarrier(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, false)
        {
            bossType = (BarrierBoss)data.Int("bossType", 0);
            barrierId = data.Attr("barrierId", "");
            breakAfterCutscene = data.Bool("breakAfterCutscene", true);
            barrierColor = bossType switch
            {
                BarrierBoss.TitanKing => Color.Orange,
                BarrierBoss.GuardianTitan => Color.Gray,
                BarrierBoss.Chapter16Els => Color.DarkRed,
                BarrierBoss.AsrielAngelOfDeath => Color.Gold,
                BarrierBoss.ElsTrueFinal => Color.Purple,
                BarrierBoss.AsrielBreakGiygas => Color.Black,
                _ => Color.HotPink
            };
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!string.IsNullOrEmpty(barrierId))
            {
                var session = (scene as Level)?.Session;
                if (session != null && session.GetFlag("boss_soul_barrier_" + barrierId + "_broken"))
                {
                    RemoveSelf();
                }
            }
        }

        public override void Update()
        {
            base.Update();
            pulseTimer += Engine.DeltaTime * 2f;

            if (triggered) return;

            foreach (SoulPlayer soul in Scene.Tracker.GetEntities<SoulPlayer>())
            {
                if (soul != null && CollideCheck(soul))
                {
                    if (!touchingSouls.Contains(soul))
                    {
                        touchingSouls.Add(soul);
                        OnSoulCollide(soul);
                    }
                }
                else
                {
                    touchingSouls.Remove(soul);
                }
            }
        }

        public void OnSoulCollide(SoulPlayer soul)
        {
            if (triggered || soul == null) return;
            triggered = true;

            soul.Controller.SetMovementEnabled(false);
            soul.Controller.Reset();

            var cutscene = CreateCutscene(soul);
            if (cutscene != null)
            {
                Scene.Add(cutscene);
            }
        }

        private CutsceneEntity CreateCutscene(SoulPlayer soul)
        {
            return bossType switch
            {
                BarrierBoss.TitanKing => new Cutscenes.CS_SoulBarrier_TitanKing(soul, this),
                BarrierBoss.GuardianTitan => new Cutscenes.CS_SoulBarrier_GuardianTitan(soul, this),
                BarrierBoss.Chapter16Els => new Cutscenes.CS_SoulBarrierDZ_CHapter16Els(soul, this),
                BarrierBoss.AsrielAngelOfDeath => new Cutscenes.CS_SoulBarrier_AsrielAngelOfDeath(soul, this),
                BarrierBoss.ElsTrueFinal => new Cutscenes.CS_SoulBarrier_ElsTrueFinal(soul, this),
                BarrierBoss.AsrielBreakGiygas => new Cutscenes.CS_SoulBarrier_AsrielBreakGiygas(soul, this),
                _ => null
            };
        }

        public void BreakBarrier()
        {
            if (!breakAfterCutscene) return;

            Audio.Play("event:/game/general/wall_break_stone", Center);
            Level level = Scene as Level;
            if (level != null)
            {
                level.Shake(1f);
                for (int i = 0; i < 20; i++)
                {
                    level.Particles.Emit(ParticleTypes.SparkyDust, Center + new Vector2(Calc.Random.NextFloat(Width), Calc.Random.NextFloat(Height)), barrierColor);
                }
            }

            if (!string.IsNullOrEmpty(barrierId))
            {
                var session = (Scene as Level)?.Session;
                session?.SetFlag("boss_soul_barrier_" + barrierId + "_broken", true);
            }

            RemoveSelf();
        }

        public override void Render()
        {
            float pulse = 0.7f + (float)Math.Sin(pulseTimer) * 0.3f;
            Draw.Rect(X, Y, Width, Height, barrierColor * pulse);

            string label = bossType switch
            {
                BarrierBoss.TitanKing => "Titan King",
                BarrierBoss.GuardianTitan => "Guardian Titan",
                BarrierBoss.Chapter16Els => "Els",
                BarrierBoss.AsrielAngelOfDeath => "Asriel",
                BarrierBoss.ElsTrueFinal => "ELS True Final",
                BarrierBoss.AsrielBreakGiygas => "Giygas",
                _ => "Barrier"
            };

            ActiveFont.DrawOutline(
                label,
                Center,
                new Vector2(0.5f, 0.5f),
                Vector2.One * 0.65f,
                Color.White,
                2f,
                Color.Black
            );
        }
    }
}
