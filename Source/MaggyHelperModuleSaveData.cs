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
        public HashSet<string> BossesExampleStoneFlags { get; set; } = new HashSet<string>();
        private HashSet<string> CollectedHeartGems { get; set; } = new HashSet<string>();
        private HashSet<string> CollectedSoulFragments { get; set; } = new HashSet<string>();
        private Dictionary<string, int> SoulBarrierFragmentCounts { get; set; } = new Dictionary<string, int>();
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

        public bool CollectSoulFragment(string fragmentKey, string barrierId)
        {
            if (string.IsNullOrEmpty(fragmentKey))
            {
                return false;
            }

            if (!CollectedSoulFragments.Add(fragmentKey))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(barrierId))
            {
                SoulBarrierFragmentCounts.TryGetValue(barrierId, out int count);
                SoulBarrierFragmentCounts[barrierId] = count + 1;
            }

            return true;
        }

        public int GetCollectedSoulFragmentsForBarrier(string barrierId)
        {
            if (string.IsNullOrEmpty(barrierId))
            {
                return 0;
            }

            return SoulBarrierFragmentCounts.TryGetValue(barrierId, out int count) ? count : 0;
        }

        public int GetTotalCollectedSoulFragments()
        {
            return CollectedSoulFragments.Count;
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