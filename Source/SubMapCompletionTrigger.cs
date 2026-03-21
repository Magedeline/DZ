using MaggyHelper.Entities;

// Removed: using System.ValueTuple;

namespace MaggyHelper.Triggers
{
    /// <summary>
    /// Trigger that detects when a submap is completed and triggers appropriate cutscene
    /// </summary>
    [Tracked(false)]
    [HotReloadable]
    public class SubMapCompletionTrigger : Trigger
    {
        private readonly int chapterNumber;
        private readonly int submapNumber;
        private readonly bool requireAllGems;
        private bool triggered;

        public SubMapCompletionTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            chapterNumber = data.Int(nameof(chapterNumber), 10);
            submapNumber = data.Int(nameof(submapNumber), 1);
            requireAllGems = data.Bool(nameof(requireAllGems), true);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            if (triggered) return;
            
            Level level = Engine.Scene as Level;
            if (level == null) return;

            if (requireAllGems && !allGemsCollected(level))
            {
                level.Add(new MiniTextbox($"SUBMAP_GEMS_REQUIRED_CH{chapterNumber}_{submapNumber}"));
                return;
            }
            
            triggered = true;

            string completionFlag = $"submap_complete_ch{chapterNumber}_{submapNumber}";
            level.Session.SetFlag(completionFlag, true);

            triggerCompletionCutscene(level, player);

            level.OnEndOfFrame += () => {
                SubMapManager.Instance?.ReturnToLobby(level, player);
            };
        }

        private bool allGemsCollected(Level level)
        {
            int requiredGems = getRequiredGemCount();
            int collectedGems = 0;
            
            for (int i = 1; i <= requiredGems; i++)
            {
                string gemFlag = SmallHeartGem.COLLECTED_FLAG_PREFIX + $"ch{chapterNumber}_submap{submapNumber}_gem{i}";
                if (level.Session.GetFlag(gemFlag))
                    collectedGems++;
            }
            
            return collectedGems >= requiredGems;
        }

        private int getRequiredGemCount()
        {
            return (chapterNumber, submapNumber) switch
            {
                (10, 3) => 3,
                (10, 4) => 4,
                (10, 5) => 5,
                (10, 6) => 6,
                (11, 3) => 3,
                (11, 4) => 4,
                (11, 5) => 5,
                (11, 6) => 6,
                (12, 3) => 4,
                (12, 4) => 5,
                (12, 5) => 6,
                (12, 6) => 7,
                (13, 3) => 5,
                (13, 4) => 6,
                (13, 5) => 7,
                (13, 6) => 8,
                (14, 3) => 6,
                (14, 4) => 7,
                (14, 5) => 8,
                (14, 6) => 9,
                _ => 3
            };
        }

        private void triggerCompletionCutscene(Level level, global::Celeste.Player player)
        {
            try
            {
                switch (chapterNumber)
                {
                    case 10:
                        var cs10Type = Type.GetType("Celeste.Mod.DesoloZantasHelper.Cutscenes.CS10_SubMapComplete");
                        if (cs10Type != null)
                        {
                            var cs10 = Activator.CreateInstance(cs10Type, player, submapNumber) as Entity;
                            level.Add(cs10);
                        }
                        break;
                    case 11:
                        var cs11Type = Type.GetType("Celeste.Mod.DesoloZantasHelper.Cutscenes.CS11_SubMapComplete");
                        if (cs11Type != null)
                        {
                            var cs11 = Activator.CreateInstance(cs11Type, player, submapNumber) as Entity;
                            level.Add(cs11);
                        }
                        break;
                    case 12:
                        var cs12Type = Type.GetType("Celeste.Mod.DesoloZantasHelper.Cutscenes.CS12_SubMapComplete");
                        if (cs12Type != null)
                        {
                            var cs12 = Activator.CreateInstance(cs12Type, player, submapNumber) as Entity;
                            level.Add(cs12);
                        }
                        break;
                    case 13:
                    case 14:
                    default:
                        var csGenericType = Type.GetType("Celeste.Mod.DesoloZantasHelper.Cutscenes.CS_GenericSubMapComplete");
                        if (csGenericType != null)
                        {
                            var csGeneric = Activator.CreateInstance(csGenericType, player, chapterNumber, submapNumber) as Entity;
                            level.Add(csGeneric);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to create completion cutscene: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Trigger that shows submap progress information
    /// </summary>
    [Tracked(false)]
    public class SubMapProgressTrigger : Trigger
    {
        private readonly int chapterNumber;
        private readonly int submapNumber;
        private bool triggered;

        public SubMapProgressTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            chapterNumber = data.Int(nameof(chapterNumber), 10);
            submapNumber = data.Int(nameof(submapNumber), 1);
        }

        public override void OnEnter(global::Celeste.Player player)
        {
            base.OnEnter(player);
            
            if (triggered) return;
            triggered = true;
            
            Level level = Engine.Scene as Level;
            if (level == null) return;

            int totalGems = getTotalGemCount();
            int collectedGems = countCollectedGems(level);

            string progressKey = collectedGems >= totalGems ? 
                $"SUBMAP_PROGRESS_COMPLETE_CH{chapterNumber}_{submapNumber}" :
                $"SUBMAP_PROGRESS_CH{chapterNumber}_{submapNumber}";
            
            level.Add(new MiniTextbox(progressKey));

            level.OnEndOfFrame += () => {
                Alarm.Set(this, 2f, () => triggered = false);
            };
        }

        private int getTotalGemCount()
        {
            return (chapterNumber, submapNumber) switch
            {
                (10, 3) => 3, (10, 4) => 4, (10, 5) => 5, (10, 6) => 6,
                (11, 3) => 3, (11, 4) => 4, (11, 5) => 5, (11, 6) => 6,
                (12, 3) => 4, (12, 4) => 5, (12, 5) => 6, (12, 6) => 7,
                (13, 3) => 5, (13, 4) => 6, (13, 5) => 7, (13, 6) => 8,
                (14, 3) => 6, (14, 4) => 7, (14, 5) => 8, (14, 6) => 9,
                _ => 3
            };
        }

        private int countCollectedGems(Level level)
        {
            int count = 0;
            int totalGems = getTotalGemCount();
            
            for (int i = 1; i <= totalGems; i++)
            {
                string gemFlag = SmallHeartGem.COLLECTED_FLAG_PREFIX + $"ch{chapterNumber}_submap{submapNumber}_gem{i}";
                if (level.Session.GetFlag(gemFlag))
                    count++;
            }
            
            return count;
        }
    }
}




