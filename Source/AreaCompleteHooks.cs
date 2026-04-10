namespace MaggyHelper;

using System.Xml;
using Celeste.Mod.Meta;
using MonoMod.Utils;

/// <summary>
/// Redirects completed Maggy chapters from the vanilla AreaComplete scene to the
/// modded AreaComplete scene once LevelExit finishes building the completion screen.
/// Includes a fallback loader for chapters whose completion screen data wasn't
/// resolved by Everest (due to the custom meta schema).
/// </summary>
public static class AreaCompleteHooks
{
    private static bool _hooked;

    /// <summary>
    /// Maps chapter base keys to their CompleteScreens.xml entry names.
    /// Used when Everest can't resolve the screen from the custom meta schema.
    /// </summary>
    private static readonly Dictionary<string, string> ChapterScreenNames = new(StringComparer.OrdinalIgnoreCase)
    {
        { "07_Hell", "Hell" },
        { "10_Ruins", "Ruins" },
        { "11_Snow", "Snow" },
        { "12_Water", "Water" },
        { "13_Fire", "Fire" },
        { "14_Digital", "Digital" },
        { "15_Castle", "Castle" },
        { "16_Corruption", "Corruption" },
    };

    public static void Load()
    {
        if (_hooked)
            return;

        _hooked = true;
        On.Celeste.LevelExit.Routine += OnLevelExitRoutine;

        Logger.Log(LogLevel.Info, "MaggyHelper", "AreaCompleteHooks loaded");
    }

    public static void Unload()
    {
        if (!_hooked)
            return;

        _hooked = false;
        On.Celeste.LevelExit.Routine -= OnLevelExitRoutine;

        Logger.Log(LogLevel.Info, "MaggyHelper", "AreaCompleteHooks unloaded");
    }

    private static IEnumerator OnLevelExitRoutine(On.Celeste.LevelExit.orig_Routine orig, LevelExit self)
    {
        IEnumerator routine = orig(self);

        while (routine.MoveNext())
            yield return routine.Current;

        TrySwapToCustomAreaComplete(self);
    }

    private static void TrySwapToCustomAreaComplete(LevelExit self)
    {
        if (self?.mode != LevelExit.Mode.Completed || self.session == null)
            return;

        AreaData area = AreaData.Get(self.session.Area);
        if (area == null || !AreaModeExtender.IsOurMap(area))
            return;

        if (Engine.Scene is not global::Celeste.AreaComplete)
            return;

        // Try to load missing completion screen assets for chapters
        // that use the custom meta schema (which Everest can't resolve)
        if (self.completeXml == null || self.completeAtlas == null)
        {
            if (!TryLoadFallbackCompleteScreen(self.session, out XmlElement fallbackXml, out Atlas fallbackAtlas))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"AreaCompleteHooks: custom screen skipped for '{area?.SID}' because complete screen assets were unavailable.");
                return;
            }

            self.completeXml = fallbackXml;
            self.completeAtlas = fallbackAtlas;
        }

        DynamicData data = new DynamicData(self);
        MapMetaCompleteScreen completeMeta = data.TryGet<MapMetaCompleteScreen>("completeMeta", out MapMetaCompleteScreen meta)
            ? meta
            : null;

        Engine.Scene = new AreaComplete(self.session, self.completeXml, self.completeAtlas, self.snow, completeMeta);
    }

    /// <summary>
    /// Attempts to load the complete screen XML and atlas for a chapter
    /// that wasn't resolved by Everest's standard meta lookup.
    /// </summary>
    private static bool TryLoadFallbackCompleteScreen(Session session, out XmlElement xml, out Atlas atlas)
    {
        xml = null;
        atlas = null;

        if (!AreaModeExtender.TryParseMainSideSID(session.Area.SID, out string baseKey, out _))
            return false;

        if (!ChapterScreenNames.TryGetValue(baseKey, out string screenName))
            return false;

        try
        {
            // Load CompleteScreens.xml and find the matching entry
            XmlDocument doc = Calc.LoadContentXML("Graphics/CompleteScreens");
            if (doc == null)
                return false;
            XmlElement screens = doc["Screens"];
            if (screens == null)
                return false;

            xml = screens[screenName];
            if (xml == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"AreaCompleteHooks: no CompleteScreens.xml entry found for '{screenName}'");
                return false;
            }

            // Load the atlas from the atlas folder
            string atlasName = xml.Attr("atlas", screenName);
            atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", atlasName), Atlas.AtlasDataFormat.PackerNoAtlas);
            return atlas != null;
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"AreaCompleteHooks: failed to load fallback complete screen for '{screenName}': {e.Message}");
            return false;
        }
    }
}
