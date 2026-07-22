using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace DZ;

/// <summary>
/// Manages HeartGem collection across all 5 sides (A/B/C/D/DX).
/// Hooks into Celeste's HeartGem entity to properly:
/// 1. Track collection in save data for extended modes
/// 2. Play correct heart gem sounds per side
/// 3. Show the correct heart gem color per side
/// 4. Unlock the next side upon collection
/// 5. Trigger the appropriate postcard sequence
/// </summary>
public static class HeartGemManager
{
    private static bool _hooked = false;

    // Heart gem sprite colors for each side
    public static readonly Color[] HeartColors =
    {
        new Color(0x31, 0x8B, 0xEB),  // Blue  (A-Side)
        new Color(0xEB, 0x31, 0x31),  // Red   (B-Side)
        new Color(0xEB, 0xD5, 0x31),  // Gold  (C-Side)
        new Color(0xB0, 0x31, 0xEB),  // Purple/Rainbow (D-Side)
        new Color(0x20, 0x00, 0x40),  // Void/Dark (DX-Side)
    };

    // Heart gem particle colors
    public static readonly Color[] HeartShineColors =
    {
        Color.LightBlue,   // A-Side
        Color.IndianRed,   // B-Side
        Color.Gold,        // C-Side
        Color.MediumPurple, // D-Side
        Color.DarkViolet,  // DX-Side
    };

    // Heart gem sprite IDs
    public static readonly string[] HeartSpriteIds =
    {
        "DZ_heartgem0",   // Blue
        "DZ_heartgem1",   // Red
        "DZ_heartgem2",   // Gold
        "DZ_heartgem3",   // Rainbow (custom)
        "heartgem4",   // Void (custom)
    };

    // ── Hook Management ──────────────────────────────────────────────────

    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;

        On.Celeste.HeartGem.Awake += OnHeartGemAwake;
        On.Celeste.HeartGem.CollectRoutine += OnHeartGemCollectRoutine;
        On.Celeste.HeartGem.RegisterAsCollected += OnHeartGemRegisterAsCollected;

        Logger.Log(LogLevel.Info, "DZ", "HeartGemManager loaded");
    }

    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;

        On.Celeste.HeartGem.Awake -= OnHeartGemAwake;
        On.Celeste.HeartGem.CollectRoutine -= OnHeartGemCollectRoutine;
        On.Celeste.HeartGem.RegisterAsCollected -= OnHeartGemRegisterAsCollected;

        Logger.Log(LogLevel.Info, "DZ", "HeartGemManager unloaded");
    }

    // ── HeartGem Awake ───────────────────────────────────────────────────

    /// <summary>
    /// When a HeartGem wakes up in the scene, set its visual appearance
    /// according to which side we're on.
    /// </summary>
    private static void OnHeartGemAwake(On.Celeste.HeartGem.orig_Awake orig, HeartGem self, Scene scene)
    {
        orig(self, scene);

        var level = scene as Level;
        if (level == null) return;

        var area = AreaData.Get(level.Session.Area);
        if (!AreaModeExtender.IsOurMap(area))
            return;

        int mode = (int)level.Session.Area.Mode;

        // For extended modes, update the heart gem sprite
        if (mode >= AreaModeExtender.MODE_2)
        {
            SetHeartGemVisuals(self, mode);
        }

        // Check if already collected
        if (IsHeartGemCollected(level.Session))
        {
            // If collected, the heart gem shouldn't appear (vanilla handles 0-2)
            if (mode >= AreaModeExtender.MODE_2)
            {
                self.RemoveSelf();
            }
        }
    }

    /// <summary>
    /// Sets the visual appearance of a HeartGem for extended modes.
    /// </summary>
    private static void SetHeartGemVisuals(HeartGem gem, int mode)
    {
        if (mode < 0 || mode >= HeartColors.Length)
            return;

        try
        {
            // Update the heart gem's color using its internal sprite
            var spriteField = typeof(HeartGem).GetField("sprite",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (spriteField?.GetValue(gem) is Sprite sprite)
            {
                // Try to play the appropriate sprite animation
                string spriteId = mode < HeartSpriteIds.Length ? HeartSpriteIds[mode] : HeartSpriteIds[0];

                // For custom modes beyond gold (index 2), use the gold sprite with color tinting
                if (mode >= AreaModeExtender.MODE_2)
                {
                    sprite.Color = HeartColors[mode];
                }
            }

            // Update shine particles
            var shineParticleField = typeof(HeartGem).GetField("shineParticle",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (shineParticleField != null && mode < HeartShineColors.Length)
            {
                // The shine particle type would need to be updated to our custom color
                // This is handled by setting the P_BlueShine equivalent for our modes
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"Failed to set heart gem visuals for mode {mode}: {ex.Message}");
        }
    }

    // ── HeartGem Collection ──────────────────────────────────────────────

    /// <summary>
    /// Hooks into the collect routine to add our custom collection logic
    /// for D-Side and DX-Side heart gems.
    /// </summary>
    private static IEnumerator OnHeartGemCollectRoutine(
        On.Celeste.HeartGem.orig_CollectRoutine orig, HeartGem self, Player player)
    {
        var level = self.Scene as Level;
        if (level == null)
        {
            // Fallback: run original
            var origRoutine = orig(self, player);
            while (origRoutine.MoveNext())
                yield return origRoutine.Current;
            yield break;
        }

        var area = AreaData.Get(level.Session.Area);
        int mode = (int)level.Session.Area.Mode;

        // For extended modes on our maps, play custom sounds
        if (AreaModeExtender.IsOurMap(area) && mode >= AreaModeExtender.MODE_2)
        {
            // Play the appropriate heart gem collection sound
            string getSound = mode < AreaModeExtender.HeartGemGetSounds.Length
                ? AreaModeExtender.HeartGemGetSounds[mode]
                : AreaModeExtender.HeartGemGetSounds[0];

            Audio.Play(getSound, self.Position);
        }

        // Run the original collect routine
        var routine = orig(self, player);
        while (routine.MoveNext())
            yield return routine.Current;
    }

    /// <summary>
    /// Builds the localized poetry string key for a heart gem collection.
    /// Maps to soul_DZ_{Side}_{Chapter}_{Suffix} entries in English.txt.
    /// </summary>
    public static string BuildPoemId(Session session)
    {
        var area = AreaData.Get(session.Area);
        if (area == null) return "";

        string sid = area.SID;
        int lastSlash = sid.LastIndexOf('/');
        string chapterName = lastSlash >= 0 ? sid.Substring(lastSlash + 1) : sid;

        string sideName = session.Area.Mode switch
        {
            AreaMode.BSide => "1",
            AreaMode.CSide => "2",
            _ when (int)session.Area.Mode == AreaModeExtender.MODE_DSIDE => "3",
            _ when (int)session.Area.Mode == AreaModeExtender.MODE_DXSIDE => "DXSide",
            _ => "0"
        };

        // Construct the registry key: soul_DZ_0_01_City
        // The suffix pattern matches the English.txt keys
        string suffix = session.Area.Mode switch
        {
            AreaMode.BSide => "_B",
            AreaMode.CSide => "_C",
            _ when (int)session.Area.Mode == AreaModeExtender.MODE_DSIDE => "_D",
            _ when (int)session.Area.Mode == AreaModeExtender.MODE_DXSIDE => "_DX",
            _ => "_A"
        };

        return $"soul_DZ_{sideName}_{chapterName}{suffix}";
    }

    /// <summary>
    /// Checks if a localized poetry string exists in the dialog registry.
    /// </summary>
    public static bool HasLocalizedPoem(Session session)
    {
        string poemId = BuildPoemId(session);
        if (string.IsNullOrEmpty(poemId)) return false;

        // Dialog.Clean returns the key itself if not found, so check if it differs
        string cleaned = Dialog.Clean(poemId);
        return cleaned != poemId;
    }

    /// <summary>
    /// When the heart gem registers as collected, track it in our save data
    /// for extended modes. Overrides poemId with localized registry values.
    /// </summary>
    private static void OnHeartGemRegisterAsCollected(
        On.Celeste.HeartGem.orig_RegisterAsCollected orig, HeartGem self, Level level, string poemId)
    {
        if (level?.Session == null)
        {
            orig(self, level, poemId);
            return;
        }

        var area = AreaData.Get(level.Session.Area);
        bool isOurMap = area != null && AreaModeExtender.IsOurMap(area);

        // For our maps, resolve poem from localized registry instead of using hardcoded/default values
        string resolvedPoemId = poemId;
        if (isOurMap)
        {
            string localizedPoem = BuildPoemId(level.Session);
            if (!string.IsNullOrEmpty(localizedPoem) && HasLocalizedPoem(level.Session))
            {
                resolvedPoemId = localizedPoem;
                Logger.Log(LogLevel.Info, "DZ",
                    $"HeartGem poem resolved from registry: {resolvedPoemId}");
            }
            else
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"No localized poem found for {localizedPoem}, falling back to: {poemId}");
            }
        }

        // Call original with potentially overridden poemId
        orig(self, level, resolvedPoemId);

        if (!isOurMap)
            return;

        int mode = (int)level.Session.Area.Mode;

        // For extended modes, additionally track in our save system
        if (mode >= AreaModeExtender.MODE_2)
        {
            DZSaveFacade.TryRecordExtendedHeartGem(level.Session);

            Logger.Log(LogLevel.Info, "DZ",
                $"Heart gem registered: {DZSaveFacade.BuildExtendedHeartId(area.SID, mode)}");

            // Set the "heart collected" flag for this side
            level.Session.SetFlag($"heartgem_{AreaModeExtender.GetModeName(mode)}_collected");

            // Also write the vanilla heart-gem flag so Everest/overworld UI recognizes it.
            SetVanillaHeartGemFlag(area.ID, mode, true);
        }

        // Set area mode completion
        if ((int)level.Session.Area.Mode >= AreaModeExtender.MODE_2)
        {
            // Mark this side as completed via our custom tracking
            string completionKey = $"{area.SID}_{AreaModeExtender.GetModeName(mode)}_completed";
            DZModule.SaveData?.UnlockAchievement(completionKey);
        }

        DZProgressionManager.RefreshProgression();
    }

    // ── Utility Methods ──────────────────────────────────────────────────

    /// <summary>
    /// Checks if the heart gem for the current session's area+mode has been collected.
    /// </summary>
    public static bool IsHeartGemCollected(Session session)
    {
        return DZSaveFacade.HasHeartGem(session);
    }

    /// <summary>
    /// Gets the total number of heart gems collected across all sides for a chapter.
    /// </summary>
    public static int GetTotalHeartsForChapter(int areaId)
    {
        return DZSaveFacade.CountHeartsForChapter(areaId);
    }

    /// <summary>
    /// Gets the total heart gems collected across all chapters and all modes.
    /// </summary>
    public static int GetTotalHeartsOverall()
    {
        int total = 0;
        for (int i = 0; i < AreaData.Areas.Count; i++)
        {
            total += GetTotalHeartsForChapter(i);
        }
        return total;
    }

    /// <summary>
    /// Writes the vanilla AreaModeStats.HeartGem flag for the given area and mode.
    /// This makes Everest and the overworld chapter panel recognize the heart as collected.
    /// </summary>
    private static void SetVanillaHeartGemFlag(int areaId, int mode, bool value)
    {
        try
        {
            SaveData save = SaveData.Instance;
            if (save == null || areaId < 0 || areaId >= save.Areas_Safe.Count)
                return;

            object areaStats = save.Areas_Safe[areaId];
            if (areaStats == null)
                return;

            System.Reflection.PropertyInfo modesProp = areaStats.GetType().GetProperty("Modes");
            if (modesProp?.GetValue(areaStats) is not System.Collections.IList modes || mode < 0 || mode >= modes.Count)
                return;

            object modeStats = modes[mode];
            if (modeStats == null)
                return;

            System.Reflection.PropertyInfo heartProp = modeStats.GetType().GetProperty("HeartGem")
                ?? modeStats.GetType().GetProperty("heartGem");
            heartProp?.SetValue(modeStats, value);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"Failed to set vanilla heart gem flag for area {areaId} mode {mode}: {ex.Message}");
        }
    }
}
