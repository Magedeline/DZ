namespace Celeste;

using System.Xml;
using global::Celeste.Mod.MaggyHelper;
using global::Celeste.Mod.Meta;
using MonoMod.Utils;

/// <summary>
/// Hooks into vanilla HeartGem and AreaComplete to:
///  1. Redirect completed Maggy chapters to use the modded AreaComplete scene (via LevelExit.Routine).
///  2. Inject mod-specific UI (subtitle, button hint, custom title) via On.Celeste.AreaComplete hooks.
/// </summary>
public static class AreaCompleteHooks
{
    private const string ExtDataKey = "MaggyHelper_ExtData";

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
        On.Celeste.LevelExit.Routine      += OnLevelExitRoutine;
        On.Celeste.AreaComplete.ctor      += OnAreaCompleteCtor;
        On.Celeste.AreaComplete.Begin     += OnAreaCompleteBegin;
        On.Celeste.AreaComplete.Update    += OnAreaCompleteUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper", "AreaCompleteHooks loaded");
    }

    public static void Unload()
    {
        if (!_hooked)
            return;

        _hooked = false;
        On.Celeste.LevelExit.Routine      -= OnLevelExitRoutine;
        On.Celeste.AreaComplete.ctor      -= OnAreaCompleteCtor;
        On.Celeste.AreaComplete.Begin     -= OnAreaCompleteBegin;
        On.Celeste.AreaComplete.Update    -= OnAreaCompleteUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper", "AreaCompleteHooks unloaded");
    }

    // ── Scene swap (LevelExit → modded AreaComplete) ─────────────────────

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

        if (self.completeXml == null || self.completeAtlas == null)
        {
            if (!TryLoadFallbackCompleteScreen(self.session, out XmlElement fallbackXml, out Atlas fallbackAtlas))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"AreaCompleteHooks: custom screen skipped for '{area?.SID}' – complete screen assets unavailable.");
                return;
            }

            self.completeXml  = fallbackXml;
            self.completeAtlas = fallbackAtlas;
        }

        DynamicData data = new DynamicData(self);
        MapMetaCompleteScreen completeMeta = data.TryGet<MapMetaCompleteScreen>("completeMeta", out MapMetaCompleteScreen meta)
            ? meta : null;

        Engine.Scene = new global::Celeste.AreaComplete(
            self.session, self.completeXml, self.completeAtlas, self.snow, completeMeta);
    }

    // ── On.Celeste.AreaComplete hooks ────────────────────────────────────

    private static readonly string[] CompleteMusicSuffixes = { "area", "bside", "cside", "dside" };

    private static void OnAreaCompleteCtor(
        On.Celeste.AreaComplete.orig_ctor orig,
        global::Celeste.AreaComplete self,
        Session session,
        System.Xml.XmlElement xml,
        Atlas atlas,
        HiresSnow snow,
        global::Celeste.Mod.Meta.MapMetaCompleteScreen meta)
    {
        orig(self, session, xml, atlas, snow, meta);

        if (session == null) return;
        AreaData area = AreaData.Get(session.Area);
        if (!AreaModeExtender.IsOurMap(area)) return;

        Audio.SetMusic(null);
    }

    private static void OnAreaCompleteBegin(On.Celeste.AreaComplete.orig_Begin orig, global::Celeste.AreaComplete self)
    {
        orig(self);

        if (self.Session == null) return;
        AreaData area = AreaData.Get(self.Session.Area);
        if (!AreaModeExtender.IsOurMap(area)) return;

        int mode = (int)self.Session.Area.Mode;
        string suffix = mode < CompleteMusicSuffixes.Length ? CompleteMusicSuffixes[mode] : CompleteMusicSuffixes[0];
        Audio.SetMusic($"event:/desolozantas/music/menu/complete_{suffix}");

        var ext = new AreaCompleteExtData();

        string subtitleKey = AreaCompleteExtData.GetSubtitleDialogKey(self.Session);
        if (subtitleKey != null && Dialog.Has(subtitleKey))
            ext.TitleText = Dialog.Clean(subtitleKey);

        new DynamicData(self).Set(ExtDataKey, ext);

        self.Add(new AreaCompleteOverlayRenderer(ext));

        if (self.Session.Area.ID == 16) return;

        string title = AreaCompleteExtData.GetCustomCompleteScreenTitle(self.Session)
                    ?? AreaCompleteExtData.GetDefaultCompleteScreenTitle(self.Session);

        DynamicData dd = new DynamicData(self);
        if (dd.TryGet<AreaCompleteTitle>("title", out AreaCompleteTitle existing) && existing == null && title != null)
        {
            Vector2 origin = new Vector2(960f, 200f);
            float scale    = Math.Min(1600f / ActiveFont.Measure(title).X, 3f);
            dd.Set("title", new AreaCompleteTitle(origin, title, scale));
        }
    }

    private static void OnAreaCompleteUpdate(On.Celeste.AreaComplete.orig_Update orig, global::Celeste.AreaComplete self)
    {
        if (self.Session != null)
        {
            AreaData area = AreaData.Get(self.Session.Area);
            if (AreaModeExtender.IsOurMap(area) && TryHandleCustomConfirm(self))
                return;
        }

        orig(self);

        AreaCompleteExtData ext = GetExt(self);
        if (ext == null) return;

        ext.ButtonTimerDelay -= Engine.DeltaTime;
        if (ext.ButtonTimerDelay <= 0f)
            ext.ButtonTimerEase = Calc.Approach(ext.ButtonTimerEase, 1f, Engine.DeltaTime * 4f);

        ext.TitleWaveTime += Engine.DeltaTime;
        ext.TitleDelay    -= Engine.DeltaTime;
        if (ext.TitleDelay <= 0f)
            ext.TitleEase = Calc.Approach(ext.TitleEase, 1f, Engine.DeltaTime * 2f);
    }

    private static bool TryHandleCustomConfirm(global::Celeste.AreaComplete self)
    {
        DynamicData dd = new DynamicData(self);
        bool finishedSlide = dd.TryGet<bool>("finishedSlide", out bool fs) && fs;
        bool canConfirm    = !dd.TryGet<bool>("canConfirm",    out bool cc) || cc;

        if (!Input.MenuConfirm.Pressed || !finishedSlide || !canConfirm)
            return false;

        Session session = self.Session;

        if (session.Area.ID == 7 && session.Area.Mode == AreaMode.Normal)
        {
            dd.Set("canConfirm", false);
            new FadeWipe(self, wipeIn: false, () =>
            {
                global::Celeste.Mod.MaggyHelper.MaggyHelperModule.LaunchCredits(session);
            });
            return true;
        }

        if (session.Area.SID == global::Celeste.Mod.MaggyHelper.MaggyHelperModule.Chapter16CorruptionSid
            && session.Area.Mode == AreaMode.Normal)
        {
            dd.Set("canConfirm", false);
            new FadeWipe(self, wipeIn: false, () =>
            {
                global::Celeste.Mod.MaggyHelper.MaggyHelperModule.LaunchCredits(session);
            });
            return true;
        }

        return false;
    }

    private static AreaCompleteExtData GetExt(global::Celeste.AreaComplete self)
    {
        DynamicData dd = new DynamicData(self);
        return dd.TryGet<AreaCompleteExtData>(ExtDataKey, out AreaCompleteExtData ext) ? ext : null;
    }

    private sealed class AreaCompleteOverlayRenderer : Entity
    {
        private readonly AreaCompleteExtData _ext;

        public AreaCompleteOverlayRenderer(AreaCompleteExtData ext)
        {
            _ext = ext;
            Depth = -1000000;
        }

        public override void Render()
        {
            _ext.RenderTitle();
            _ext.RenderButtonHint();
        }
    }

    // ── Fallback complete-screen loader ──────────────────────────────────

    /// <summary>
    /// Attempts to load the complete screen XML and atlas for a chapter
    /// that wasn't resolved by Everest's standard meta lookup.
    /// </summary>
    private static bool TryLoadFallbackCompleteScreen(Session session, out XmlElement xml, out Atlas atlas)
    {
        xml   = null;
        atlas = null;

        if (!AreaModeExtender.TryParseMainSideSID(session.Area.SID, out string baseKey, out _))
            return false;

        if (!ChapterScreenNames.TryGetValue(baseKey, out string screenName))
            return false;

        try
        {
            XmlDocument doc = Calc.LoadContentXML("Graphics/CompleteScreens");
            if (doc == null) return false;

            XmlElement screens = doc["Screens"];
            if (screens == null) return false;

            xml = screens[screenName];
            if (xml == null)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"AreaCompleteHooks: no CompleteScreens.xml entry for '{screenName}'");
                return false;
            }

            string atlasName = xml.Attr("atlas", screenName);
            atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", atlasName), Atlas.AtlasDataFormat.PackerNoAtlas);
            return atlas != null;
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"AreaCompleteHooks: failed to load fallback screen for '{screenName}': {e.Message}");
            return false;
        }
    }
}
