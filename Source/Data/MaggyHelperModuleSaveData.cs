using System.Collections.Generic;

namespace Celeste.Mod.MaggyHelper
{
    /// <summary>
    /// Per-chapter mastery record. Every flag must be true for the Asriel cosmic
    /// background to appear on the chapter panel icon.
    /// </summary>
    public class ChapterMasteryRecord
    {
        public bool AllBerriesCollected  { get; set; }
        public bool AllHeartGemsCollected { get; set; }
        public bool AllBossesDefeated    { get; set; }
        public bool AllDXBossesDefeated  { get; set; }
        public bool SpeedrunGoalBeaten   { get; set; }
        public bool FirstTryNoDamageDeath { get; set; }

        public bool IsFullMastery =>
            AllBerriesCollected && AllHeartGemsCollected &&
            AllBossesDefeated   && AllDXBossesDefeated   &&
            SpeedrunGoalBeaten  && FirstTryNoDamageDeath;
    }

    public class MaggyHelperModuleSaveData : EverestModuleSaveData
    {
        // Progression flags
        public bool HasSeenModIntro { get; set; }
        public bool VoidMoonUnlocked { get; set; }
        public bool PendingUnlockChapter10OnRestart { get; set; }
        public bool UnlockedChapter10 { get; set; }
        public bool PendingUnlockChapter16OnRestart { get; set; }
        public bool PendingUnlockChapter19OnRestart { get; set; }
        public bool PendingUnlockChapter20OnRestart { get; set; }
        public bool PendingUnlockChapter21OnRestart { get; set; }
        public bool BossRushUnlocked { get; set; }
        public bool UnlockedChapter19 { get; set; }
        public bool UnlockedChapter21 { get; set; }
        public bool FinalDlcContentUnlocked { get; set; }
        public bool TrueFinaleUnlocked { get; set; }
        public bool Chapter19Complete { get; set; }

        // Unlock tracking
        public HashSet<string> UnlockedBSideIDs { get; set; } = new HashSet<string>();
        public HashSet<string> UnlockedCSideIDs { get; set; } = new HashSet<string>();
        public List<string> PendingCSideUnlockIDs { get; set; } = new List<string>();
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

        public bool HasDefeatedBoss(string bossName)
        {
            return DefeatedBosses.Contains(bossName);
        }

        public void CompleteChapter(string chapterSid)
        {
            CompletedChapters.Add(chapterSid);
        }

        public bool IsChapterCompleted(string chapterSid)
        {
            return CompletedChapters.Contains(chapterSid);
        }

        // ── Mastery ──────────────────────────────────────────────────────────
        public Dictionary<string, ChapterMasteryRecord> MasteryRecords { get; set; }
            = new Dictionary<string, ChapterMasteryRecord>(StringComparer.OrdinalIgnoreCase);

        public ChapterMasteryRecord GetOrCreateMastery(string chapterSid)
        {
            if (!MasteryRecords.TryGetValue(chapterSid, out var rec))
            {
                rec = new ChapterMasteryRecord();
                MasteryRecords[chapterSid] = rec;
            }
            return rec;
        }

        public bool HasFullMastery(string chapterSid)
            => MasteryRecords.TryGetValue(chapterSid, out var rec) && rec.IsFullMastery;
    }
}
