using System;
using System.Collections.Generic;
using Celeste.Bosses;
using Celeste.Entities.Bosses;
using Celeste.Helpers;
using DZ;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Manages a group of bosses that must all be defeated to advance.
    /// Used by the top-down soul/battle path to orchestrate multi-boss encounters.
    /// </summary>
    [CustomEntity("DZ/BossesGroup")]
    [Tracked]
    [HotReloadable]
    public class BossesGroup : Entity
    {
        private readonly List<Entity> bosses = new List<Entity>();
        private readonly List<string> bossNames = new List<string>();
        private bool battleStarted;
        private bool allDefeated;
        private float defeatedTimer;
        private string groupName = "BossGroup";

        public string GroupName => groupName;
        public bool AllDefeated => allDefeated;
        public bool BattleStarted => battleStarted;
        public int BossCount => bosses.Count;
        public IReadOnlyList<Entity> Bosses => bosses;

        public Action OnAllDefeated;
        public Action OnBattleStarted;

        public BossesGroup(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            groupName = data.Attr("groupName", "BossGroup");
            string names = data.Attr("bossNames", "");
            if (!string.IsNullOrWhiteSpace(names))
            {
                foreach (string name in names.Split(','))
                {
                    string trimmed = name.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        bossNames.Add(trimmed);
                }
            }
        }

        public BossesGroup(Vector2 position, string name = "BossGroup")
            : base(position)
        {
            groupName = name;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (bossNames.Count > 0)
            {
                SpawnBossesFromNames();
            }
        }

        public override void Update()
        {
            base.Update();
            if (battleStarted && !allDefeated)
            {
                CheckDefeatState();
            }
            if (allDefeated && defeatedTimer < 1f)
            {
                defeatedTimer += Engine.DeltaTime;
            }
        }

        public void AddBoss(Entity boss)
        {
            if (boss == null || bosses.Contains(boss)) return;
            bosses.Add(boss);
        }

        public void AddBossByName(string name, Vector2 position)
        {
            Entity boss = CreateBossByName(name, position);
            if (boss != null)
            {
                AddBoss(boss);
                Scene?.Add(boss);
            }
        }

        public void StartBattle()
        {
            if (battleStarted) return;
            battleStarted = true;
            foreach (Entity boss in bosses)
            {
                if (boss != null && boss.Scene != null)
                {
                    boss.Visible = true;
                    boss.Collidable = true;
                }
            }
            OnBattleStarted?.Invoke();
        }

        private void CheckDefeatState()
        {
            foreach (Entity boss in bosses)
            {
                if (boss != null && !IsBossDefeated(boss))
                    return;
            }
            allDefeated = true;
            OnAllDefeated?.Invoke();
        }

        private void SpawnBossesFromNames()
        {
            Vector2 basePos = Position;
            int index = 0;
            foreach (string name in bossNames)
            {
                Vector2 offset = new Vector2((index % 3 - 1) * 80f, (index / 3) * 60f);
                AddBossByName(name, basePos + offset);
                index++;
            }
        }

        public static bool IsBossDefeated(Entity boss)
        {
            if (boss is BossActor ba)
                return ba.IsDefeated;
            if (boss is Boss b)
                return b.IsDefeated;
            if (boss is ELSTerminaBoss els)
                return els.Dead;
            if (boss is ELSTerminaFinalBoss final)
                return final.Dead;
            return false;
        }

        public static Entity CreateBossByName(string name, Vector2 position)
        {
            string lower = name.ToLowerInvariant();
            EntityData data = new EntityData
            {
                Position = position,
                Values = new Dictionary<string, object>()
            };

            try
            {
                return lower switch
                {
                    "kingtitan" or "king_titan" or "titanking" => new KingTitanBoss(data, Vector2.Zero),
                    "titanguardian" or "guardian_titan" or "titantis" => new TitantisBoss(data, Vector2.Zero),
                    "els" or "chapter16els" or "els_ch16" => new ELSTerminaBoss(data, Vector2.Zero),
                    "asrielangel" or "asriel_angel_of_death" or "angelofdeath" => new AsrielAngelOfDeathBoss(position),
                    "elstruefinal" or "els_true_final" or "elsfinal" => new ELSTerminaFinalBoss(data, Vector2.Zero),
                    "asrielgod" or "asriel_god" or "godasriel" => new AsrielGodBoss(data, Vector2.Zero),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ", $"[BossesGroup] Failed to create boss '{name}': {ex.Message}");
                return null;
            }
        }

        public override void Removed(Scene scene)
        {
            foreach (Entity boss in bosses)
            {
                if (boss != null && boss.Scene == scene)
                    boss.RemoveSelf();
            }
            bosses.Clear();
            base.Removed(scene);
        }
    }
}
