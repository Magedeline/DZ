using FMOD.Studio;
using Monocle;

namespace MaggyHelper;

/// <summary>
/// Replaces vanilla overworld/menu music with custom DesoloZantas music events.
/// Hooks into the OuiChapterSelect, Mountain, and related scenes to swap
/// audio whenever the player is in a MaggyHelper context.
/// </summary>
public static class OverworldMusicManager
{
    private static bool _hooked = false;
    private const int SummitChapterNumber = 9;

    // ── Custom Music Events (from GUIDs.txt) ─────────────────────────────

    /// <summary>Level select screen music (replaces event:/music/menu/level_select)</summary>
    public const string MUSIC_LEVEL_SELECT = "event:/desolozantas/music/menu/level_select";

    /// <summary>A-Side / Normal completion music</summary>
    public const string MUSIC_COMPLETE_AREA = "event:/desolozantas/music/menu/complete_area";

    /// <summary>Summit completion music (A-Side final chapters)</summary>
    public const string MUSIC_COMPLETE_SUMMIT = "event:/desolozantas/music/menu/complete_summit";

    /// <summary>B-Side completion music</summary>
    public const string MUSIC_COMPLETE_BSIDE = "event:/desolozantas/music/menu/complete_bside";

    /// <summary>B-Side summit completion music</summary>
    public const string MUSIC_COMPLETE_BSIDE_SUMMIT = "event:/desolozantas/music/menu/complete_bside_summit";

    /// <summary>C-Side completion music</summary>
    public const string MUSIC_COMPLETE_CSIDE = "event:/desolozantas/music/menu/complete_cside";

    /// <summary>C-Side summit completion music</summary>
    public const string MUSIC_COMPLETE_CSIDE_SUMMIT = "event:/desolozantas/music/menu/complete_cside_summit";

    /// <summary>D-Side and DX-Side completion music</summary>
    public const string MUSIC_COMPLETE_DSIDE_VOID = "event:/desolozantas/music/menu/complete_dside_void";

    /// <summary>Credits music</summary>
    public const string MUSIC_CREDIT = "event:/desolozantas/music/menu/credit";

    /// <summary>Dodge credits music</summary>
    public const string MUSIC_DODGE_CREDIT = "event:/desolozantas/music/menu/dodge_credit";

    /// <summary>Game over screen</summary>
    public const string MUSIC_GAMEOVER = "event:/desolozantas/music/menu/gameover";

    /// <summary>Game over slow version</summary>
    public const string MUSIC_GAMEOVER_SLOW = "event:/desolozantas/music/menu/gameover_slow";

    /// <summary>Goodnight / end of session music</summary>
    public const string MUSIC_GOODNIGHT = "event:/desolozantas/music/menu/goodnight";

    /// <summary>Last push / final chapters menu music</summary>
    public const string MUSIC_LAST_PUSH = "event:/desolozantas/music/menu/last_push";

    /// <summary>Respite / save point music</summary>
    public const string MUSIC_RESPITE = "event:/desolozantas/music/menu/respite";

    /// <summary>Trailer music</summary>
    public const string MUSIC_TRAILER = "event:/desolozantas/music/menu/trailer";

    /// <summary>True legend / all-clear music</summary>
    public const string MUSIC_TRUE_LEGEND = "event:/desolozantas/music/menu/true_legend";

    /// <summary>Candy parade music</summary>
    public const string MUSIC_CANDY = "event:/desolozantas/music/menu/candy";

    /// <summary>Cast screen music</summary>
    public const string MUSIC_CAST = "event:/desolozantas/music/menu/cast";

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

        On.Celeste.Audio.Play_string -= OnAudioPlayString;
        On.Celeste.Audio.SetMusic -= OnAudioSetMusic;
        On.Celeste.Audio.SetAmbience -= OnAudioSetAmbience;
        On.Celeste.OuiChapterSelect.Enter -= OnChapterSelectEnter;

        Logger.Log(LogLevel.Info, "MaggyHelper", "OverworldMusicManager unloaded");
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

