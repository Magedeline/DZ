using System.Collections;
using Celeste.Cutscenes;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Base cutscene for the top-down soul path boss barriers.
    /// </summary>
    public abstract class SoulBarrierCutscene : CutsceneEntity
    {
        protected readonly SoulPlayer soul;
        protected readonly BossSoulBarrier barrier;
        protected BossesGroup bossGroup;
        protected bool battleWon;

        protected SoulBarrierCutscene(SoulPlayer soul, BossSoulBarrier barrier)
            : base(true, false)
        {
            this.soul = soul;
            this.barrier = barrier;
        }

        protected void SpawnBossGroup(Vector2 position, string groupName, string bossName)
        {
            bossGroup = new BossesGroup(position, groupName);
            bossGroup.AddBossByName(bossName, position);
            Scene.Add(bossGroup);
            bossGroup.OnAllDefeated += OnBossGroupDefeated;
        }

        protected void SpawnBossGroup(Vector2 position, string groupName, params string[] bossNames)
        {
            bossGroup = new BossesGroup(position, groupName);
            int index = 0;
            foreach (string name in bossNames)
            {
                Vector2 offset = new Vector2((index % 3 - 1) * 80f, (index / 3) * 60f);
                bossGroup.AddBossByName(name, position + offset);
                index++;
            }
            Scene.Add(bossGroup);
            bossGroup.OnAllDefeated += OnBossGroupDefeated;
        }

        protected virtual void OnBossGroupDefeated()
        {
            battleWon = true;
        }

        protected void StartBattleAfterDelay(float delay)
        {
            Add(new Coroutine(StartBattleRoutine(delay)));
        }

        private IEnumerator StartBattleRoutine(float delay)
        {
            yield return delay;
            soul?.BattleController.StartBattle(bossGroup);
        }

        protected void BreakBarrierAfterDelay(float delay)
        {
            Add(new Coroutine(BreakBarrierRoutine(delay)));
        }

        private IEnumerator BreakBarrierRoutine(float delay)
        {
            yield return delay;
            barrier?.BreakBarrier();
            soul?.Controller.SetMovementEnabled(true);
        }

        protected void UseBarrierBreakController(Vector2 position)
        {
            var controller = new BarrierBreakController(position);
            Scene.Add(controller);
            Add(new Coroutine(controller.ExecuteBarrierBreakSequence()));
        }

        public override void OnEnd(Level level)
        {
            soul?.Controller.SetMovementEnabled(true);
        }
    }

    /// <summary>
    /// Cutscene for the Titan King barrier.
    /// </summary>
    [HotReloadable]
    public class CS_SoulBarrier_TitanKing : SoulBarrierCutscene
    {
        public CS_SoulBarrier_TitanKing(SoulPlayer soul, BossSoulBarrier barrier) : base(soul, barrier) { }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            level.Shake(1f);
            level.Flash(Color.Orange * 0.5f, true);
            yield return Textbox.Say("DZ_SOULBARRIER_TITAN_KING_INTRO");

            SpawnBossGroup(barrier.Center, "TitanKing", "kingtitan");
            yield return 0.5f;
            StartBattleAfterDelay(0.5f);
            yield return Textbox.Say("DZ_SOULBARRIER_TITAN_KING_FIGHT");

            while (!battleWon) yield return null;
            yield return Textbox.Say("DZ_SOULBARRIER_TITAN_KING_DEFEAT");
            BreakBarrierAfterDelay(0.3f);
            yield return 1f;

            EndCutscene(level);
        }
    }

    /// <summary>
    /// Cutscene for the Guardian Titan barrier.
    /// </summary>
    [HotReloadable]
    public class CS_SoulBarrier_GuardianTitan : SoulBarrierCutscene
    {
        public CS_SoulBarrier_GuardianTitan(SoulPlayer soul, BossSoulBarrier barrier) : base(soul, barrier) { }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            level.Shake(1f);
            yield return Textbox.Say("DZ_SOULBARRIER_GUARDIAN_TITAN_INTRO");

            SpawnBossGroup(barrier.Center, "GuardianTitan", "titanguardian");
            yield return 0.5f;
            StartBattleAfterDelay(0.5f);
            yield return Textbox.Say("DZ_SOULBARRIER_GUARDIAN_TITAN_FIGHT");

            while (!battleWon) yield return null;
            yield return Textbox.Say("DZ_SOULBARRIER_GUARDIAN_TITAN_DEFEAT");
            BreakBarrierAfterDelay(0.3f);
            yield return 1f;

            EndCutscene(level);
        }
    }

    /// <summary>
    /// Cutscene for the Chapter 16 Els barrier.
    /// </summary>
    [HotReloadable]
    public class CS_SoulBarrier_Chapter16Els : SoulBarrierCutscene
    {
        public CS_SoulBarrier_Chapter16Els(SoulPlayer soul, BossSoulBarrier barrier) : base(soul, barrier) { }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            level.Flash(Color.DarkRed * 0.5f, true);
            yield return Textbox.Say("DZ_SOULBARRIER_CH16_ELS_INTRO");

            SpawnBossGroup(barrier.Center, "Chapter16Els", "els");
            yield return 0.5f;
            StartBattleAfterDelay(0.5f);
            yield return Textbox.Say("DZ_SOULBARRIER_CH16_ELS_FIGHT");

            while (!battleWon) yield return null;
            yield return Textbox.Say("DZ_SOULBARRIER_CH16_ELS_DEFEAT");
            BreakBarrierAfterDelay(0.3f);
            yield return 1f;

            EndCutscene(level);
        }
    }

    /// <summary>
    /// Cutscene for the Asriel Angel of Death barrier.
    /// </summary>
    [HotReloadable]
    public class CS_SoulBarrier_AsrielAngelOfDeath : SoulBarrierCutscene
    {
        public CS_SoulBarrier_AsrielAngelOfDeath(SoulPlayer soul, BossSoulBarrier barrier) : base(soul, barrier) { }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            level.Flash(Color.Gold * 0.5f, true);
            yield return Textbox.Say("DZ_SOULBARRIER_ASRIEL_ANGEL_INTRO");

            SpawnBossGroup(barrier.Center, "AsrielAngelOfDeath", "asrielangel");
            yield return 0.5f;
            StartBattleAfterDelay(0.5f);
            yield return Textbox.Say("DZ_SOULBARRIER_ASRIEL_ANGEL_FIGHT");

            while (!battleWon) yield return null;
            yield return Textbox.Say("DZ_SOULBARRIER_ASRIEL_ANGEL_DEFEAT");
            BreakBarrierAfterDelay(0.3f);
            yield return 1f;

            EndCutscene(level);
        }
    }

    /// <summary>
    /// Cutscene for the Els True Final barrier.
    /// </summary>
    [HotReloadable]
    public class CS_SoulBarrier_ElsTrueFinal : SoulBarrierCutscene
    {
        public CS_SoulBarrier_ElsTrueFinal(SoulPlayer soul, BossSoulBarrier barrier) : base(soul, barrier) { }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            level.Flash(Color.Purple * 0.5f, true);
            level.Shake(2f);
            yield return Textbox.Say("DZ_SOULBARRIER_ELS_TRUE_FINAL_INTRO");

            SpawnBossGroup(barrier.Center, "ElsTrueFinal", "elstruefinal");
            yield return 0.5f;
            StartBattleAfterDelay(0.5f);
            yield return Textbox.Say("DZ_SOULBARRIER_ELS_TRUE_FINAL_FIGHT");

            while (!battleWon) yield return null;
            yield return Textbox.Say("DZ_SOULBARRIER_ELS_TRUE_FINAL_DEFEAT");
            BreakBarrierAfterDelay(0.3f);
            yield return 1f;

            EndCutscene(level);
        }
    }

    /// <summary>
    /// Cutscene for the Asriel breaking the Giygas barrier.
    /// </summary>
    [HotReloadable]
    public class CS_SoulBarrier_AsrielBreakGiygas : SoulBarrierCutscene
    {
        public CS_SoulBarrier_AsrielBreakGiygas(SoulPlayer soul, BossSoulBarrier barrier) : base(soul, barrier) { }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            yield return Textbox.Say("DZ_SOULBARRIER_ASRIEL_GIYGAS_INTRO");
            UseBarrierBreakController(barrier.Center);
            yield return 2f;

            SpawnBossGroup(barrier.Center, "AsrielGod", "asrielgod");
            yield return 0.5f;
            StartBattleAfterDelay(0.5f);
            yield return Textbox.Say("DZ_SOULBARRIER_ASRIEL_GIYGAS_FIGHT");

            while (!battleWon) yield return null;
            yield return Textbox.Say("DZ_SOULBARRIER_ASRIEL_GIYGAS_DEFEAT");
            BreakBarrierAfterDelay(0.3f);
            yield return 1f;

            EndCutscene(level);
        }
    }
}
