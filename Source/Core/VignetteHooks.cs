#nullable enable
using Celeste.Cutscenes;
using Celeste.Entities;
using Celeste.Triggers;
using Session = Celeste.Session;
using LevelExit = Celeste.LevelExit;
using HiresSnow = Celeste.HiresSnow;
using AreaData = Celeste.AreaData;

namespace DZ;

/// <summary>
/// Centralizes vignette display by hooking into LevelEnter.Go and LevelExit.ctor.
/// Shows chapter-specific intro/outro vignettes based on session state and save data.
/// </summary>
public static class VignetteHooks
{
    private static bool _hooked;

    // ── Public API ────────────────────────────────────────────────────────

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.LevelEnter.Go += OnLevelEnterGo;
        On.Celeste.LevelExit.ctor += OnLevelExitCtor;
        Logger.Log(LogLevel.Info, "DZ", "VignetteHooks loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.LevelEnter.Go -= OnLevelEnterGo;
        On.Celeste.LevelExit.ctor -= OnLevelExitCtor;
        Logger.Log(LogLevel.Info, "DZ", "VignetteHooks unloaded");
    }

    // ── Intro hook ────────────────────────────────────────────────────────

    private static void OnLevelEnterGo(On.Celeste.LevelEnter.orig_Go orig,
        Celeste.Session session, bool fromSaveData)
    {
        // Only intercept fresh starts of our maps on the A-Side
        if (!fromSaveData
            && session.StartedFromBeginning
            && (int)session.Area.Mode == AreaModeExtender.MODE_NORMAL)
        {
            var area = AreaData.Get(session.Area);
            if (AreaModeExtender.IsOurMap(area))
            {
                var chapter = AreaMapData.FindByAnySID(area.SID);
                if (chapter != null && IsVignetteEnabledForChapter(chapter.Number))
                {
                    Scene? vignette = CreateIntroVignette(session, chapter);
                    if (vignette != null)
                    {
                        Engine.Scene = vignette;
                        return;
                    }
                }
            }
        }

        orig(session, fromSaveData);
    }

    private static bool IsVignetteEnabledForChapter(int chapterNumber)
    {
        return chapterNumber == 0 || chapterNumber == 3 || chapterNumber == 10 || chapterNumber == 18 || chapterNumber == 21;
    }

    // ── Outro hook ────────────────────────────────────────────────────────

    private static void OnLevelExitCtor(On.Celeste.LevelExit.orig_ctor orig,
        LevelExit self, LevelExit.Mode mode, Session session, HiresSnow snow)
    {
        orig(self, mode, session, snow);

        if (mode != LevelExit.Mode.Completed || session == null)
            return;

        var area = AreaData.Get(session.Area);
        if (!AreaModeExtender.IsOurMap(area))
            return;

        if ((int)session.Area.Mode != AreaModeExtender.MODE_NORMAL)
            return;

        var chapter = AreaMapData.FindByAnySID(area.SID);
        if (chapter == null)
            return;

        Scene? outro = CreateOutroVignette(session, chapter);
        if (outro != null)
        {
            Engine.Scene = outro;
        }
    }

    // ── Factory methods ───────────────────────────────────────────────────

    private static Scene? CreateIntroVignette(Session session, AreaMapData.ChapterDef chapter)
    {
        string flagKey = $"seen_intro_vignette_{chapter.Number}";

        switch (chapter.Number)
        {
            case 0:
                // On the very first launch the player has not seen the mod intro yet.
                // Show the interactive vessel-creation sequence; it chains to
                // Cs00IntroVignette on its own so we do NOT mark the flag here — the
                // intro vignette will see itself as unseen and play normally afterward.
                var saveData = global::Celeste.Mod.DZ.DZModule.SaveData;
                if (saveData != null && !saveData.HasSeenModIntro)
                {
                    // Mark HasSeenModIntro now so the vessel creation scene isn't
                    // re-shown if the player dies or quits mid-sequence.
                    // VesselCreationVignette also sets this flag, but setting it here
                    // prevents a second instance from showing on scene re-enter.
                    saveData.HasSeenModIntro = true;
                    return new VesselCreationVignette(session);
                }
                // Second (or later) fresh start: show the brief intro vignette once.
                if (!HasSeenVignette(flagKey))
                {
                    MarkVignetteSeen(flagKey);
                    return new Cs00IntroVignette(session);
                }
                break;

            case 3:
                if (!HasSeenVignette(flagKey))
                {
                    MarkVignetteSeen(flagKey);
                    return new Cs03IntroVignette(session);
                }
                break;

            case 10:
                if (!HasSeenVignette(flagKey))
                {
                    MarkVignetteSeen(flagKey);
                    return new Cs10IntroVignetteAlt(session);
                }
                break;

            case 18:
                if (!HasSeenVignette(flagKey))
                {
                    MarkVignetteSeen(flagKey);
                    return new Cs18IntroVignette(session);
                }
                break;

            case 21:
                if (!HasSeenVignette(flagKey))
                {
                    MarkVignetteSeen(flagKey);
                    return new TrueFinaleVignette(session);
                }
                break;
        }

        return null;
    }

    private static Scene? CreateOutroVignette(Session session, AreaMapData.ChapterDef chapter)
    {
        string flagKey = $"seen_outro_vignette_{chapter.Number}";

        switch (chapter.Number)
        {
            case 3:
                if (!HasSeenVignette(flagKey))
                {
                    MarkVignetteSeen(flagKey);
                    return new Cs03OutroVignette(session);
                }
                break;

            case 4:
                if (!HasSeenVignette(flagKey))
                {
                    MarkVignetteSeen(flagKey);
                    return new Cs04LegendVignette(session);
                }
                break;
        }

        return null;
    }

    // ── Save data helpers ─────────────────────────────────────────────────

    private static bool HasSeenVignette(string key)
    {
        return global::Celeste.Mod.DZ.DZModule.SaveData?.HasAchievement(key) == true;
    }

    private static void MarkVignetteSeen(string key)
    {
        global::Celeste.Mod.DZ.DZModule.SaveData?.UnlockAchievement(key);
    }
}
