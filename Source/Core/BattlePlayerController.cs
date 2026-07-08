using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Entities
{
    /// <summary>
    /// Controller component that links a player (soul or K_Player) to a BossesGroup.
    /// Handles battle start/end, win/lose conditions, and UI feedback for grouped boss encounters.
    /// </summary>
    public class BattlePlayerController : Component
    {
        private Actor actor;
        private Level level;
        private BossesGroup activeGroup;
        private bool inBattle;
        private float battleTimer;
        private bool isDefeated;

        public bool InBattle => inBattle;
        public float BattleTimer => battleTimer;
        public BossesGroup ActiveGroup => activeGroup;
        public bool IsDefeated => isDefeated;

        public Action OnBattleWon;
        public Action OnBattleLost;

        private static bool _hooksLoaded = false;

        public static void Load()
        {
            if (_hooksLoaded) return;
            Logger.Log(LogLevel.Info, "DZ", "[BattlePlayerController] Loaded");
            _hooksLoaded = true;
        }

        public static void Unload()
        {
            if (!_hooksLoaded) return;
            Logger.Log(LogLevel.Info, "DZ", "[BattlePlayerController] Unloaded");
            _hooksLoaded = false;
        }

        public BattlePlayerController()
            : base(active: true, visible: false)
        {
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            actor = entity as Actor;
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            if (inBattle)
            {
                battleTimer += Engine.DeltaTime;
                UpdateBattleState();
            }
        }

        public void StartBattle(BossesGroup group)
        {
            if (group == null || inBattle) return;

            activeGroup = group;
            inBattle = true;
            battleTimer = 0f;
            isDefeated = false;

            DZModule.Session.BossFightActive = true;
            DZModule.Session.CurrentBossName = group.GroupName;

            group.OnAllDefeated += () =>
            {
                if (isDefeated) return;
                EndBattle(true);
            };

            group.StartBattle();
            Audio.Play("event:/game/general/strawberry_pulse", actor?.Position ?? Vector2.Zero);
        }

        public void EndBattle(bool won)
        {
            if (!inBattle) return;

            inBattle = false;
            DZModule.Session.BossFightActive = false;

            if (won)
            {
                DZModule.Session.BossesDefeated += activeGroup?.BossCount ?? 0;
                DZModule.SaveData?.RecordBossDefeat(activeGroup?.GroupName ?? "Unknown");
                OnBattleWon?.Invoke();
            }
            else
            {
                isDefeated = true;
                OnBattleLost?.Invoke();
            }
        }

        private void UpdateBattleState()
        {
            if (activeGroup == null || activeGroup.AllDefeated)
            {
                EndBattle(true);
                return;
            }

            if (actor != null && actor.Scene != null)
            {
                // Player can still move; battle state is tracked by the group.
            }
        }

        public void Reset()
        {
            inBattle = false;
            activeGroup = null;
            battleTimer = 0f;
            isDefeated = false;
            DZModule.Session.BossFightActive = false;
        }
    }
}
