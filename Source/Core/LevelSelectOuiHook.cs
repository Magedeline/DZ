using System.Collections;
using System.Collections.Generic;
using Celeste;
using MonoMod.Utils;

namespace DZ;

/// <summary>
/// Replaces the vanilla level-select music with the DZ/Pusheen equivalent and
/// drives a programmable "darkstar" music parameter based on the selected chapter.
/// </summary>
public static class LevelSelectOuiHook
{
    private const string VanillaLevelSelectMusic = "event:/music/menu/level_select";
    private const string PusheenLevelSelectMusic = "event:/pusheen/music/menu/level_select";
    private const string DarkStarParam = "darkstar";

    /// <summary>
    /// Self-programmable mapping from chapter/area id (0..21) to the darkstar
    /// music parameter value (0, 1, or 2). Edit this dictionary at runtime or
    /// from other DZ code to change the level-select music layering per chapter.
    /// </summary>
    public static readonly Dictionary<int, int> DarkStarByArea = new();

    private static bool _loaded;
    private static int _lastArea = -1;

    public static void Load()
    {
        if (_loaded)
            return;
        _loaded = true;

        InitializeDarkStarDefaults();

        On.Celeste.Audio.SetMusic += OnSetMusic;
        On.Celeste.OuiChapterSelect.Enter += OnChapterSelectEnter;
        On.Celeste.OuiChapterSelect.Update += OnChapterSelectUpdate;

        Logger.Log(LogLevel.Info, "DZ", "[LevelSelectOuiHook] Loaded");
    }

    public static void Unload()
    {
        if (!_loaded)
            return;
        _loaded = false;

        On.Celeste.Audio.SetMusic -= OnSetMusic;
        On.Celeste.OuiChapterSelect.Enter -= OnChapterSelectEnter;
        On.Celeste.OuiChapterSelect.Update -= OnChapterSelectUpdate;

        _lastArea = -1;
        Logger.Log(LogLevel.Info, "DZ", "[LevelSelectOuiHook] Unloaded");
    }

    private static void InitializeDarkStarDefaults()
    {
        DarkStarByArea.Clear();
        for (int i = 0; i <= 18; i++)
            DarkStarByArea[i] = 0;
        DarkStarByArea[1] = 0;
        DarkStarByArea[2] = 0;
        DarkStarByArea[3] = 0;
        DarkStarByArea[4] = 0;
        DarkStarByArea[5] = 0;
        DarkStarByArea[6] = 0;
        DarkStarByArea[7] = 0;
        DarkStarByArea[8] = 0;
        DarkStarByArea[9] = 0;
        DarkStarByArea[10] = 0;
        DarkStarByArea[11] = 0;
        DarkStarByArea[12] = 0;
        DarkStarByArea[13] = 0;
        DarkStarByArea[14] = 0;
        DarkStarByArea[15] = 0;
        DarkStarByArea[16] = 0;
        DarkStarByArea[17] = 0;
        DarkStarByArea[18] = 0;
        DarkStarByArea[19] = 1;
        DarkStarByArea[20] = 1;
        DarkStarByArea[21] = 2;
    }

    private static bool OnSetMusic(On.Celeste.Audio.orig_SetMusic orig, string path, bool startPlaying, bool allowFadeOut)
    {
        bool isLevelSelect = path == VanillaLevelSelectMusic;
        if (isLevelSelect)
            path = PusheenLevelSelectMusic;

        bool result = orig(path, startPlaying, allowFadeOut);
        if (isLevelSelect)
            ApplyDarkStar();
        return result;
    }

    private static IEnumerator OnChapterSelectEnter(
        On.Celeste.OuiChapterSelect.orig_Enter orig,
        OuiChapterSelect self,
        Oui from)
    {
        _lastArea = -1;
        IEnumerator routine = orig(self, from);
        while (routine.MoveNext())
            yield return routine.Current;

        UpdateDarkStar(self);
    }

    private static void OnChapterSelectUpdate(On.Celeste.OuiChapterSelect.orig_Update orig, OuiChapterSelect self)
    {
        orig(self);
        UpdateDarkStar(self);
    }

    private static void UpdateDarkStar(OuiChapterSelect self)
    {
        try
        {
            int area = new DynamicData(self).Get<int>("area");
            if (area != _lastArea)
            {
                _lastArea = area;
                ApplyDarkStar();
            }
        }
        catch { }
    }

    private static void ApplyDarkStar()
    {
        if (!DarkStarByArea.TryGetValue(_lastArea, out int value))
            value = 0;

        Audio.SetMusicParam(DarkStarParam, value);
    }
}
