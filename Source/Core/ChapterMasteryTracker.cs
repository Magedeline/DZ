using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.DZ;
using Celeste.Entities;

namespace DZ;

/// <summary>
/// Tracks per-chapter mastery conditions:
///   - All berries collected
///   - All heart gems collected
///   - All regular bosses defeated
///   - All DX bosses defeated (Final-tier / secret bosses)
///   - Speedrun goal beaten
///   - No damage taken AND no death on the first ever clear attempt
///
/// A chapter achieves Full Mastery when every flag is true, which causes
/// <see cref="CosmicChapterPanelHook"/> to render the Asriel ISO cosmic background
/// on that chapter's panel icon.
/// </summary>
public static class ChapterMasteryTracker
{
    private static bool _hooked;

    // ── Speedrun goals (ticks; 10,000,000 per second) ──────────────────────
    // Adjust these values as chapters are tuned.  Unmapped chapters treat the
    // goal as already beaten so they don't block mastery.
    private static readonly Dictionary<string, long> SpeedrunGoals
        = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase)
    {
        { AreaModeExtender.Build0SID("01_Metropolis"),  TicksFromMinutes(12) },
        { AreaModeExtender.Build0SID("02_Veil"),        TicksFromMinutes(10) },
        { AreaModeExtender.Build0SID("03_Arrival"),     TicksFromMinutes(8)  },
        { AreaModeExtender.Build0SID("04_Chronicles"),  TicksFromMinutes(10) },
        { AreaModeExtender.Build0SID("05_Fractured"),    TicksFromMinutes(12) },
        { AreaModeExtender.Build0SID("06_Fortress"),    TicksFromMinutes(14) },
        { AreaModeExtender.Build0SID("07_Inferno"),     TicksFromMinutes(15) },
        { AreaModeExtender.Build0SID("08_Revelation"),  TicksFromMinutes(18) },
        { AreaModeExtender.Build0SID("09_Apex"),        TicksFromMinutes(20) },
        { AreaModeExtender.Build0SID("10_Ruins"),       TicksFromMinutes(14) },
        { AreaModeExtender.Build0SID("11_Snow"),        TicksFromMinutes(16) },
        { AreaModeExtender.Build0SID("12_Water"),       TicksFromMinutes(18) },
        { AreaModeExtender.Build0SID("13_Fire"),        TicksFromMinutes(20) },
        { AreaModeExtender.Build0SID("14_Digital"),     TicksFromMinutes(22) },
        { AreaModeExtender.Build0SID("15_Castle"),      TicksFromMinutes(25) },
        { AreaModeExtender.Build0SID("16_Corruption"),  TicksFromMinutes(28) },
        { AreaModeExtender.Build0SID("18_Core"),        TicksFromMinutes(30) },
        { AreaModeExtender.Build0SID("19_Space"),       TicksFromMinutes(35) },
        { AreaModeExtender.Build0SID("20_TheEnd"),      TicksFromMinutes(40) },
    };

    // ── Lifecycle ───────────────────────────────────────────────────────────

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.Level.Begin     += OnLevelBegin;
        Everest.Events.Level.OnExit   += OnLevelExit;
        On.Celeste.Player.Die         += OnPlayerDie;

        Logger.Log(LogLevel.Info, "DZ", "ChapterMasteryTracker loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.Level.Begin     -= OnLevelBegin;
        Everest.Events.Level.OnExit   -= OnLevelExit;
        On.Celeste.Player.Die         -= OnPlayerDie;

        UnhookDamageEvent();

        Logger.Log(LogLevel.Info, "DZ", "ChapterMasteryTracker unloaded");
    }

    // ── Event handlers ──────────────────────────────────────────────────────

    private static void OnLevelBegin(On.Celeste.Level.orig_Begin orig, Level self)
    {
        orig(self);
        OnLevelEnter(self);
    }

    private static void OnLevelEnter(Level level)
    {
        var modSession = DZModule.Session;
        if (modSession == null) return;

        string sid = level?.Session?.Area.SID;
        if (string.IsNullOrEmpty(sid) || !AreaModeExtender.IsOurMap(AreaData.Get(level.Session.Area)))
            return;

        // Determine if this is the first-ever attempt at this chapter
        bool firstAttempt = !(DZModule.SaveData?.IsChapterCompleted(sid) ?? false);
        modSession.IsTrackingFirstTry  = firstAttempt;
        modSession.DiedThisRun         = false;
        modSession.TookDamageThisRun   = false;

        // Subscribe to the health manager's damage event for damage-free tracking
        HookDamageEvent();
    }

    private static void OnLevelExit(Level level, LevelExit exit, LevelExit.Mode mode,
        Session session, HiresSnow snow)
    {
        UnhookDamageEvent();

        if (mode != LevelExit.Mode.Completed || session?.Area.SID == null)
            return;

        string sid = session.Area.SID;
        if (!AreaModeExtender.IsOurMap(AreaData.Get(session.Area)))
            return;

        var saveData   = DZModule.SaveData;
        var modSession = DZModule.Session;
        if (saveData == null || modSession == null) return;

        var mastery = saveData.GetOrCreateMastery(sid);
        EvaluateMastery(mastery, sid, session, saveData, modSession);

        Logger.Log(LogLevel.Debug, "DZ",
            $"[Mastery] {sid} | berries={mastery.AllBerriesCollected} " +
            $"hearts={mastery.AllHeartGemsCollected} bosses={mastery.AllBossesDefeated} " +
            $"dx={mastery.AllDXBossesDefeated} speed={mastery.SpeedrunGoalBeaten} " +
            $"firstTry={mastery.FirstTryNoDamageDeath} | FULL={mastery.IsFullMastery}");
    }

    private static PlayerDeadBody OnPlayerDie(
        On.Celeste.Player.orig_Die orig, Player self,
        Microsoft.Xna.Framework.Vector2 dir, bool evenIfInvincible, bool registerDeathInStats)
    {
        if (DZModule.Session?.IsTrackingFirstTry == true)
            DZModule.Session.DiedThisRun = true;

        return orig(self, dir, evenIfInvincible, registerDeathInStats);
    }

    private static void OnDamageTaken(int _amount)
    {
        if (DZModule.Session?.IsTrackingFirstTry == true)
            DZModule.Session.TookDamageThisRun = true;
    }

    // ── Damage event subscription (instance-level) ──────────────────────────

    private static PlayerHealthManager _subscribedManager;

    private static void HookDamageEvent()
    {
        UnhookDamageEvent();
        var hm = PlayerHealthManager.Instance;
        if (hm == null) return;
        _subscribedManager = hm;
        hm.OnDamageTaken += OnDamageTaken;
    }

    private static void UnhookDamageEvent()
    {
        if (_subscribedManager == null) return;
        _subscribedManager.OnDamageTaken -= OnDamageTaken;
        _subscribedManager = null;
    }

    // ── Mastery evaluation ──────────────────────────────────────────────────

    private static void EvaluateMastery(ChapterMasteryRecord mastery, string sid,
        Session session, DZModuleSaveData saveData, DZModuleSession modSession)
    {
        AreaData area = AreaData.Get(session.Area);

        // — All berries —
        int totalBerries = area?.Mode[(int)session.Area.Mode]?.TotalStrawberries ?? 0;
        if (totalBerries > 0)
            mastery.AllBerriesCollected |= session.Strawberries.Count >= totalBerries;
        else
            mastery.AllBerriesCollected = true; // no berries → counts as collected

        // — All heart gems —
        // A chapter's full heart-gem set is A/B/C/D/DX sides; check each mode's heart
        bool allHearts = true;
        int modeCount = area?.Mode?.Length ?? 1;
        for (int m = 0; m < modeCount; m++)
        {
            string heartId = $"{sid}_{AreaModeExtender.GetSideFolder(m)}";
            if (!saveData.HasCollectedHeartGem(heartId))
            {
                allHearts = false;
                break;
            }
        }
        mastery.AllHeartGemsCollected |= allHearts;

        // — Regular & DX bosses for this chapter —
        int chapterNum = ParseChapterNumber(sid);
        if (chapterNum > 0)
        {
            var chBosses = BossRosterRegistry.GetBossesForChapter(chapterNum);

            var mainIds = chBosses.Where(b => !b.IsDXBoss).Select(b => b.Id).ToList();
            mastery.AllBossesDefeated |=
                mainIds.Count == 0 || mainIds.All(id => saveData.HasDefeatedBoss(id));

            var dxIds = chBosses.Where(b => b.IsDXBoss).Select(b => b.Id).ToList();
            mastery.AllDXBossesDefeated |=
                dxIds.Count == 0 || dxIds.All(id => saveData.HasDefeatedBoss(id));
        }
        else
        {
            mastery.AllBossesDefeated   = true;
            mastery.AllDXBossesDefeated = true;
        }

        // — Speedrun goal —
        if (SpeedrunGoals.TryGetValue(sid, out long goal))
            mastery.SpeedrunGoalBeaten |= session.Time <= goal;
        else
            mastery.SpeedrunGoalBeaten = true;

        // — First-try no damage / no death —
        if (modSession.IsTrackingFirstTry &&
            !modSession.DiedThisRun && !modSession.TookDamageThisRun)
        {
            mastery.FirstTryNoDamageDeath = true;
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the chapter number from a SID segment such as "07_Inferno" → 7.
    /// </summary>
    private static int ParseChapterNumber(string sid)
    {
        if (string.IsNullOrEmpty(sid)) return -1;
        string segment = sid.Split('/').LastOrDefault() ?? string.Empty;
        int underscore = segment.IndexOf('_');
        if (underscore > 0 && int.TryParse(segment.Substring(0, underscore), out int n))
            return n;
        return -1;
    }

    private static long TicksFromMinutes(double minutes)
        => (long)(TimeSpan.FromMinutes(minutes).TotalSeconds * 10_000_000L);
}
