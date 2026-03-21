using System.Collections.Generic;

namespace Celeste.Mod.MaggyHelper
{
    public class MaggyHelperModuleSaveData : EverestModuleSaveData
    {
        // Progression flags
        public bool HasSeenModIntro { get; set; }
        public bool VoidMoonUnlocked { get; set; }
        public bool PendingUnlockChapter16OnRestart { get; set; }
        public bool PendingUnlockChapter19OnRestart { get; set; }
        public bool PendingUnlockChapter20OnRestart { get; set; }
        public bool BossRushUnlocked { get; set; }
        public bool UnlockedChapter19 { get; set; }
        public bool FinalDlcContentUnlocked { get; set; }
        public bool Chapter19Complete { get; set; }

        // Unlock tracking
        public HashSet<string> UnlockedBSideIDs { get; set; } = new HashSet<string>();
        public HashSet<string> UnlockedCSideIDs { get; set; } = new HashSet<string>();
        public HashSet<string> UnlockedRemixExtraIDs { get; set; } = new HashSet<string>();
        public HashSet<string> UnlockedModes { get; set; } = new HashSet<string>();

        // Achievement tracking
        private HashSet<string> Achievements { get; set; } = new HashSet<string>();
        private HashSet<string> CollectedHeartGems { get; set; } = new HashSet<string>();
        private HashSet<string> CompletedChapters { get; set; } = new HashSet<string>();
        private HashSet<string> DefeatedBosses { get; set; } = new HashSet<string>();

        // Stats
        public int TotalBossesDefeated { get; set; }
        public int TotalEnemiesDefeated { get; set; }

        public bool HasAchievement(string key)
        {
            return Achievements.Contains(key);
        }

        public void UnlockAchievement(string key)
        {
            Achievements.Add(key);
        }

        public void CollectHeartGem(string heartId)
        {
            CollectedHeartGems.Add(heartId);
        }

        public bool HasCollectedHeartGem(string heartId)
        {
            return CollectedHeartGems.Contains(heartId);
        }

        public void RecordBossDefeat(string bossName)
        {
            DefeatedBosses.Add(bossName);
            TotalBossesDefeated++;
        }

        public void CompleteChapter(string chapterSid)
        {
            CompletedChapters.Add(chapterSid);
        }

        public bool IsChapterCompleted(string chapterSid)
        {
            return CompletedChapters.Contains(chapterSid);
        }
    }
}