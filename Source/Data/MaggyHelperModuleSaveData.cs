using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DZ
{
    /// <summary>
    /// Per-chapter mountain camera data saved for 3D overworld persistence.
    /// </summary>
    public class MountainCameraSaveData
    {
        public Vector3 IdlePos { get; set; }
        public Vector3 IdleTarget { get; set; }
        public Vector3 SelectPos { get; set; }
        public Vector3 SelectTarget { get; set; }
        public Vector3 ZoomPos { get; set; }
        public Vector3 ZoomTarget { get; set; }
        public Vector3 Cursor { get; set; }
        public int MountainState { get; set; }
        public string CustomModelDir { get; set; }
    }
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

    /// <summary>
    /// Persistent save data for KIRBY_CELESTE mod.
    /// Tracks: Progression flags, unlocks, achievements, boss defeats,
    /// overworld 3D state, chapter completion, mastery records.
    /// </summary>
    public class DZModuleSaveData : EverestModuleSaveData
    {
        // Progression flags
        public bool HasSeenModIntro { get; set; }
        public bool VoidMoonUnlocked { get; set; }
        public bool PendingUnlockChapter10OnRestart { get; set; }
        public bool UnlockedChapter10 { get; set; }
        public bool PendingUnlockChapter16OnRestart { get; set; }
        public bool PendingUnlockChapter19OnRestart { get; set; }
        public bool PendingRestartToChapter19 { get; set; }
        public bool PendingUnlockChapter20OnRestart { get; set; }
        public bool PendingUnlockChapter21OnRestart { get; set; }
        public bool BossRushUnlocked { get; set; }
        public bool UnlockedChapter19 { get; set; }
        public bool UnlockedChapter21 { get; set; }
        public bool FinalDlcContentUnlocked { get; set; }
        public bool TrueFinaleUnlocked { get; set; }
        public bool Chapter19Complete { get; set; }
        public bool Chapter17EpilogueCompleted { get; set; }

        // ── Overworld 3D Save Data ──────────────────────────────────────────
        /// <summary>Whether custom mountain models have been registered.</summary>
        public bool MountainModelsRegistered { get; set; }

        /// <summary>Saved camera positions per chapter.</summary>
        public Dictionary<int, MountainCameraSaveData> SavedMountainCameras { get; set; } = new();

        /// <summary>Last viewed chapter in overworld (for camera persistence).</summary>
        public int LastOverworldChapter { get; set; } = -1;

        /// <summary>Last mountain state viewed.</summary>
        public int LastMountainState { get; set; }

        /// <summary>Whether the player has seen the DZ mountain intro.</summary>
        public bool HasSeenDZMountainIntro { get; set; }

        /// <summary>Preferred camera distance preference.</summary>
        public float PreferredCameraZoom { get; set; } = 1.0f;

        /// <summary>Whether fog effects are enabled (persisted).</summary>
        public bool FogEffectsEnabled { get; set; } = true;
        // ──────────────────────────────────────────────────────────────────────

        // ── Area Data Save State ──────────────────────────────────────────────
        /// <summary>Dictionary of chapter SIDs to their registered side availability.</summary>
        public Dictionary<string, ChapterSideAvailability> ChapterSideAvailability { get; set; } = new();

        /// <summary>Saved completion times per chapter per side (in ticks).</summary>
        public Dictionary<string, long> ChapterCompletionTimes { get; set; } = new();

        /// <summary>Saved death counts per chapter per side.</summary>
        public Dictionary<string, int> ChapterDeathCounts { get; set; } = new();

        /// <summary>Custom music overrides per chapter SID.</summary>
        public Dictionary<string, string> ChapterMusicOverrides { get; set; } = new();

        /// <summary>Whether hardcoded runtime data has been applied.</summary>
        public bool RuntimeDataApplied { get; set; }

        /// <summary>Last time the area registry was updated.</summary>
        public long LastAreaRegistryUpdate { get; set; }
        // ──────────────────────────────────────────────────────────────────────

        // Popstar berry collection (persisted across restarts)
        public HashSet<string> CollectedPopstarBerries { get; set; } = new HashSet<string>();

        // Unlock tracking
        public HashSet<string> Unlocked1IDs { get; set; } = new HashSet<string>();
        public HashSet<string> UnlockedDSideIDs { get; set; } = new HashSet<string>();
        public List<string> PendingDSideUnlockIDs { get; set; } = new List<string>();
        public HashSet<string> UnlockedRemixExtraIDs { get; set; } = new HashSet<string>();
        public HashSet<string> UnlockedModes { get; set; } = new HashSet<string>();

        // Postcard dialog tracking (which chapters have shown their postcard)
        public HashSet<int> PostcardsShown { get; set; } = new HashSet<int>();
        public bool Chapter18OutroPostcardShown { get; set; }
        public bool dUnlockPostcardShown { get; set; }
        public bool UltraCompletionPostcardShown { get; set; }

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

        // ── Overworld 3D Helpers ────────────────────────────────────────────
        public void SaveMountainCamera(int chapterNumber, MountainCameraSaveData data)
        {
            SavedMountainCameras[chapterNumber] = data;
        }

        public MountainCameraSaveData GetMountainCamera(int chapterNumber)
        {
            return SavedMountainCameras.TryGetValue(chapterNumber, out var data) ? data : null;
        }

        public void ClearMountainCameraOverrides()
        {
            SavedMountainCameras.Clear();
        }
        // ──────────────────────────────────────────────────────────────────────

        // ── Area Data Helpers ───────────────────────────────────────────────
        public void RecordChapterCompletion(string chapterSid, int sideIndex, long completionTime)
        {
            string key = $"{chapterSid}:{sideIndex}";
            ChapterCompletionTimes[key] = completionTime;
        }

        public long? GetChapterCompletionTime(string chapterSid, int sideIndex)
        {
            string key = $"{chapterSid}:{sideIndex}";
            return ChapterCompletionTimes.TryGetValue(key, out var time) ? time : null;
        }

        public void RecordChapterDeath(string chapterSid, int sideIndex)
        {
            string key = $"{chapterSid}:{sideIndex}";
            ChapterDeathCounts.TryGetValue(key, out int count);
            ChapterDeathCounts[key] = count + 1;
        }

        public int GetChapterDeathCount(string chapterSid, int sideIndex)
        {
            string key = $"{chapterSid}:{sideIndex}";
            return ChapterDeathCounts.TryGetValue(key, out int count) ? count : 0;
        }

        public void SetChapterMusicOverride(string chapterSid, string musicEvent)
        {
            if (string.IsNullOrEmpty(musicEvent))
                ChapterMusicOverrides.Remove(chapterSid);
            else
                ChapterMusicOverrides[chapterSid] = musicEvent;
        }

        public string GetChapterMusicOverride(string chapterSid)
        {
            return ChapterMusicOverrides.TryGetValue(chapterSid, out var music) ? music : null;
        }
        // ──────────────────────────────────────────────────────────────────────
    }

    /// <summary>
    /// Tracks which sides are available/unlocked for a chapter.
    /// </summary>
    public class ChapterSideAvailability
    {
        public string ChapterSID { get; set; }
        public bool Has0 { get; set; } = true;
        public bool Has1 { get; set; }
        public bool Has2 { get; set; }
        public bool HasDSide { get; set; }
        public bool HasDXSide { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
