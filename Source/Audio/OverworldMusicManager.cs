using System.Collections.Generic;
using System.IO;
using FMOD.Studio;
using Monocle;

namespace Celeste;

/// <summary>
/// Replaces vanilla overworld/menu music with custom DesoloZantas music events.
/// Hooks into the OuiChapterSelect, Mountain, and related scenes to swap
/// audio whenever the player is in a MaggyHelper context.
/// </summary>
public static class OverworldMusicManager
{
    private static bool _hooked = false;
    private static readonly List<Bank> _loadedBanks = new();
    private const int SummitChapterNumber = 9;

    // ── Custom Music Events (from GUIDs.txt) ─────────────────────────────

    /// <summary>Level select screen music (replaces event:/music/menu/level_select)</summary>
    public const string MUSIC_LEVEL_SELECT = "event:/pusheen/music/menu/level_select";

    /// <summary>A-Side / Normal completion music</summary>
    public const string MUSIC_COMPLETE_AREA = "event:/pusheen/music/menu/complete_area";

    /// <summary>Summit completion music (A-Side final chapters)</summary>
    public const string MUSIC_COMPLETE_SUMMIT = "event:/pusheen/music/menu/complete_summit";

    /// <summary>B-Side completion music</summary>
    public const string MUSIC_COMPLETE_BSIDE = "event:/pusheen/music/menu/complete_bside";

    /// <summary>B-Side summit completion music</summary>
    public const string MUSIC_COMPLETE_BSIDE_SUMMIT = "event:/pusheen/music/menu/complete_bside_summit";

    /// <summary>C-Side completion music</summary>
    public const string MUSIC_COMPLETE_CSIDE = "event:/pusheen/music/menu/complete_cside";

    /// <summary>C-Side summit completion music</summary>
    public const string MUSIC_COMPLETE_CSIDE_SUMMIT = "event:/pusheen/music/menu/complete_cside_summit";

    /// <summary>D-Side and DX-Side completion music (uses DX-side event which exists)</summary>
    public const string MUSIC_COMPLETE_DSIDE_VOID = "event:/pusheen/music/menu/complete_dxside";

    /// <summary>Credits music</summary>
    public const string MUSIC_CREDIT = "event:/pusheen/music/menu/credit";

    /// <summary>Dodge credits music (fallback to credits)</summary>
    public const string MUSIC_DODGE_CREDIT = "event:/pusheen/music/menu/credits";

    /// <summary>Game over screen (fallback to vanilla complete_area)</summary>
    public const string MUSIC_GAMEOVER = "event:/music/menu/complete_area";

    /// <summary>Game over slow version (fallback to vanilla complete_area)</summary>
    public const string MUSIC_GAMEOVER_SLOW = "event:/music/menu/complete_area";

    /// <summary>Goodnight / end of session music (fallback to credits)</summary>
    public const string MUSIC_GOODNIGHT = "event:/pusheen/music/menu/credits";

    /// <summary>Last push / final chapters menu music (fallback to credits)</summary>
    public const string MUSIC_LAST_PUSH = "event:/pusheen/music/menu/credits";

    /// <summary>Respite / save point music (fallback to level_select)</summary>
    public const string MUSIC_RESPITE = "event:/pusheen/music/menu/level_select";

    /// <summary>Trailer music (fallback to credits)</summary>
    public const string MUSIC_TRAILER = "event:/pusheen/music/menu/credits";

    /// <summary>True legend / all-clear music (fallback to complete_summit)</summary>
    public const string MUSIC_TRUE_LEGEND = "event:/pusheen/music/menu/complete_summit";

    /// <summary>Candy parade music (fallback to complete_area)</summary>
    public const string MUSIC_CANDY = "event:/pusheen/music/menu/complete_area";

    /// <summary>Cast screen music (fallback to credits)</summary>
    public const string MUSIC_CAST = "event:/pusheen/music/menu/credits";

    // ── Vanilla → Custom mapping ─────────────────────────────────────────

    /// <summary>
    /// Maps vanilla Celeste music event paths to our custom replacements.
    /// Any event that starts with one of these vanilla paths will be swapped.
    /// </summary>
    private static readonly Dictionary<string, string> MusicReplacements = new()
    {
        // Level select / overworld
        { "event:/music/menu/level_select",           MUSIC_LEVEL_SELECT },
        
        // Completion jingles
        { "event:/music/menu/complete_area",          MUSIC_COMPLETE_AREA },
        { "event:/music/menu/complete_summit",        MUSIC_COMPLETE_SUMMIT },
        { "event:/music/menu/complete_bside",         MUSIC_COMPLETE_BSIDE },
        { "event:/music/menu/complete_cside",         MUSIC_COMPLETE_CSIDE },
        { "event:/music/menu/complete_cside_summit",  MUSIC_COMPLETE_CSIDE_SUMMIT },
        
        // Credits
        { "event:/music/menu/credits",                MUSIC_CREDIT },
        
        // Game over
        { "event:/music/menu/complete_area_noskip",   MUSIC_GAMEOVER },
    };

    // ── Hook Management ──────────────────────────────────────────────────

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.Audio.Init += OnAudioInit;

        // Hook Audio.Play to intercept music events when in our maps
        On.Celeste.Audio.Play_string += OnAudioPlayString;
        On.Celeste.Audio.SetMusic += OnAudioSetMusic;
        On.Celeste.Audio.SetAmbience += OnAudioSetAmbience;

        // Hook into OuiChapterSelect to play our level select music
        On.Celeste.OuiChapterSelect.Enter += OnChapterSelectEnter;

        Logger.Log(LogLevel.Info, "MaggyHelper", "OverworldMusicManager loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.Audio.Init -= OnAudioInit;
        On.Celeste.Audio.Play_string -= OnAudioPlayString;
        On.Celeste.Audio.SetMusic -= OnAudioSetMusic;
        On.Celeste.Audio.SetAmbience -= OnAudioSetAmbience;
        On.Celeste.OuiChapterSelect.Enter -= OnChapterSelectEnter;

        foreach (var bank in _loadedBanks)
        {
            try { bank.unload(); } catch { }
        }
        _loadedBanks.Clear();

        Logger.Log(LogLevel.Info, "MaggyHelper", "OverworldMusicManager unloaded");
    }

    // ── Bank Loading ─────────────────────────────────────────────────────

    private static void OnAudioInit(On.Celeste.Audio.orig_Init orig)
    {
        orig();
        if (_loadedBanks.Count > 0 || global::Celeste.Audio.System == null) return;

        foreach (var mod in global::Celeste.Mod.Everest.Modules)
        {
            if (mod.Metadata?.Name != "DesoloZantas_Audio") continue;

            string audioPath = Path.Combine(mod.Metadata.PathDirectory, "Audio");
            if (!Directory.Exists(audioPath)) break;

            foreach (var bankFile in Directory.GetFiles(audioPath, "dz_*.bank"))
            {
                FMOD.RESULT result = global::Celeste.Audio.System.loadBankFile(
                    bankFile, LOAD_BANK_FLAGS.NORMAL, out Bank bank);

                if (result == FMOD.RESULT.OK && bank.isValid())
                {
                    bank.loadSampleData();
                    _loadedBanks.Add(bank);
                    Logger.Log(LogLevel.Info, "MaggyHelper", $"Loaded audio bank: {Path.GetFileName(bankFile)}");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to load bank {Path.GetFileName(bankFile)}: {result}");
                }
            }
            break;
        }
    }

    // ── Audio.Play Hook ──────────────────────────────────────────────────

    /// <summary>
    /// Intercepts Audio.Play(string) calls and replaces vanilla events
    /// with our custom ones when the player is in a MaggyHelper context.
    /// </summary>
    private static EventInstance OnAudioPlayString(On.Celeste.Audio.orig_Play_string orig, string path)
    {
        if (ShouldReplaceMusic() && path != null)
        {
            string replaced = TryReplace(path);
            if (replaced != null)
            {
                Logger.Log(LogLevel.Debug, "MaggyHelper",
                    $"OverworldMusic: Replaced '{path}' → '{replaced}'");
                return orig(replaced);
            }
        }
        return orig(path);
    }

    // ── Audio.SetMusic Hook ──────────────────────────────────────────────

    /// <summary>
    /// Intercepts Audio.SetMusic to replace the music track for overworld/menus.
    /// </summary>
    private static bool OnAudioSetMusic(On.Celeste.Audio.orig_SetMusic orig,
        string path, bool startPlaying, bool allowFadeOut)
    {
        if (ShouldReplaceMusic() && path != null)
        {
            string replaced = TryReplace(path);
            if (replaced != null)
            {
                Logger.Log(LogLevel.Debug, "MaggyHelper",
                    $"OverworldMusic: SetMusic replaced '{path}' → '{replaced}'");
                return orig(replaced, startPlaying, allowFadeOut);
            }
        }
        return orig(path, startPlaying, allowFadeOut);
    }

    // ── Audio.SetAmbience Hook ───────────────────────────────────────────

    /// <summary>
    /// Intercepts Audio.SetAmbience for overworld ambience replacement.
    /// </summary>
    private static bool OnAudioSetAmbience(On.Celeste.Audio.orig_SetAmbience orig,
        string path, bool startPlaying)
    {
        if (ShouldReplaceMusic() && path != null)
        {
            string replaced = TryReplace(path);
            if (replaced != null)
            {
                return orig(replaced, startPlaying);
            }
        }
        return orig(path, startPlaying);
    }

    // ── OuiChapterSelect Hook ────────────────────────────────────────────

    /// <summary>
    /// When entering the chapter select screen, play our custom level select music.
    /// </summary>
    private static IEnumerator OnChapterSelectEnter(
        On.Celeste.OuiChapterSelect.orig_Enter orig, OuiChapterSelect self,
        Oui from)
    {
        var routine = orig(self, from);
        while (routine.MoveNext())
            yield return routine.Current;

        // After the original enter completes, swap the music
        if (IsInMaggyHelperLevelSet())
        {
            Audio.SetMusic(MUSIC_LEVEL_SELECT);
        }
    }

    // ── Utility ──────────────────────────────────────────────────────────

    /// <summary>
    /// Attempts to find a replacement for the given vanilla event path.
    /// Returns null if no replacement is needed.
    /// </summary>
    private static string TryReplace(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        // Direct match
        if (MusicReplacements.TryGetValue(path, out string replacement))
            return replacement;

        return null;
    }

    /// <summary>
    /// Determines whether we should be replacing music right now.
    /// Returns true when:
    /// - We're in the overworld and the selected chapter is a MaggyHelper map, OR
    /// - We're in a MaggyHelper level
    /// </summary>
    private static bool ShouldReplaceMusic()
    {
        // Check if we're in a MaggyHelper level
        if (Engine.Scene is Level level)
        {
            return IsOurSID(level.Session?.Area.GetSID());
        }

        // Check if we're in the overworld with a MaggyHelper chapter selected
        if (Engine.Scene is Overworld overworld)
        {
            return IsInMaggyHelperLevelSet();
        }

        return false;
    }

    /// <summary>
    /// Checks if the currently selected area in the overworld belongs to MaggyHelper.
    /// </summary>
    private static bool IsInMaggyHelperLevelSet()
    {
        try
        {
            if (SaveData.Instance == null)
                return false;

            int lastArea = SaveData.Instance.LastArea_Safe.ID;
            if (lastArea >= 0 && lastArea < AreaData.Areas.Count)
            {
                return IsOurSID(AreaData.Get(lastArea)?.SID);
            }
        }
        catch
        {
            // Silently fail
        }
        return false;
    }

    /// <summary>
    /// Checks if an area SID belongs to MaggyHelper.
    /// </summary>
    private static bool IsOurSID(string sid)
    {
        return sid != null && (
            sid.StartsWith("Maggy/", StringComparison.OrdinalIgnoreCase) ||
            sid.StartsWith("MaggyHelper/", StringComparison.OrdinalIgnoreCase) ||
            sid.StartsWith("DesoloZantas/", StringComparison.OrdinalIgnoreCase)
        );
    }

    // ── Public API for other systems ─────────────────────────────────────

    /// <summary>
    /// Gets the appropriate completion music event for the given mode.
    /// Used by AreaComplete and other completion screens.
    /// </summary>
    public static string GetCompletionMusic(int mode)
    {
        return GetCompletionMusic(mode, isSummit: false);
    }

    /// <summary>
    /// Gets the appropriate completion music event for the given session.
    /// Summit chapters use their dedicated variants, while D / DX use the void theme.
    /// </summary>
    public static string GetCompletionMusic(Session session)
    {
        int mode = session != null ? (int) session.Area.Mode : AreaModeExtender.MODE_NORMAL;
        return GetCompletionMusic(mode, IsSummitChapter(session));
    }

    private static string GetCompletionMusic(int mode, bool isSummit)
    {
        return mode switch
        {
            AreaModeExtender.MODE_NORMAL => isSummit ? MUSIC_COMPLETE_SUMMIT : MUSIC_COMPLETE_AREA,
            AreaModeExtender.MODE_BSIDE => isSummit ? MUSIC_COMPLETE_BSIDE_SUMMIT : MUSIC_COMPLETE_BSIDE,
            AreaModeExtender.MODE_CSIDE => isSummit ? MUSIC_COMPLETE_CSIDE_SUMMIT : MUSIC_COMPLETE_CSIDE,
            AreaModeExtender.MODE_DSIDE or AreaModeExtender.MODE_DXSIDE => MUSIC_COMPLETE_DSIDE_VOID,
            _ => isSummit ? MUSIC_COMPLETE_SUMMIT : MUSIC_COMPLETE_AREA,
        };
    }

    /// <summary>
    /// Gets the appropriate completion music for a summit chapter.
    /// </summary>
    public static string GetSummitCompletionMusic(int mode)
    {
        return GetCompletionMusic(mode, isSummit: true);
    }

    private static bool IsSummitChapter(Session session)
    {
        if (session == null)
            return false;

        AreaData area = AreaData.Get(session.Area);
        if (area == null)
            return false;

        if (AreaModeExtender.IsOurMap(area))
            return AreaMapData.FindByAnySID(area.SID)?.Number == SummitChapterNumber;

        return area.ID == 7
            || (area.SID != null && area.SID.IndexOf("Summit", StringComparison.OrdinalIgnoreCase) >= 0);
    }
}

