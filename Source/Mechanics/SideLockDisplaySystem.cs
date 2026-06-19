using System;
using global::Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace DZ;

/// <summary>
/// Optional UI enhancement for displaying lock status and progress in the chapter panel.
/// Provides:
/// - Visual lock indicators (lock icon overlay on unavailable sides)
/// - Lock status tooltips ("D-Side: Locked until C-Side complete")
/// - Progress tracker ("Collect X more hearts for D-Side unlock")
/// - Greyed-out appearance for unavailable sides
/// 
/// This is a complementary system to AreaModeExtender's internal unlock logic.
/// Use only if you want explicit visual feedback for lock status.
/// </summary>
public static class SideLockDisplaySystem
{
    /// <summary>Configuration for lock display per side</summary>
    public class LockDisplayConfig
    {
        public int ModeIndex { get; set; }
        public string LockIcon { get; set; }  // e.g., "ui/lock"
        public string LockedLabel { get; set; }  // e.g., "D-Side: Locked"
        public string RequirementText { get; set; }  // e.g., "Beat C-Side to unlock"
        public Color LockTint { get; set; }
    }

    /// <summary>Lock display configs for extended modes</summary>
    public static readonly LockDisplayConfig DSideLockConfig = new()
    {
        ModeIndex = AreaModeExtender.MODE_DSIDE,
        LockIcon = "ui/common/lock",
        LockedLabel = "D-Side: Locked",
        RequirementText = "Beat C-Side to unlock",
        LockTint = new Color(180, 100, 255, 128)  // Purple, semi-transparent
    };

    public static readonly LockDisplayConfig DXSideLockConfig = new()
    {
        ModeIndex = AreaModeExtender.MODE_DXSIDE,
        LockIcon = "ui/common/lock",
        LockedLabel = "DX-Side: Locked",
        RequirementText = "Beat D-Side to unlock",
        LockTint = new Color(50, 0, 80, 128)  // Dark void, semi-transparent
    };

    private static bool _hookInstalled = false;

    // Card bounds used for the lock overlay region (matches the vanilla chapter
    // panel card size used by CosmicChapterPanelHook).
    private static readonly Vector2 CardSize = new(480f, 272f);

    // Vertical offset of the mode label region from the top of the card. The
    // vanilla panel renders the A/B/C/D label near the bottom of the card.
    private const float ModeLabelOffsetY = 200f;
    private static readonly Vector2 ModeRegionSize = new(200f, 48f);

    public static void Load()
    {
        if (_hookInstalled)
            return;
        _hookInstalled = true;

        On.Celeste.OuiChapterPanel.Render += OnChapterPanelRender;
        On.Celeste.OuiChapterPanel.Update += OnChapterPanelUpdate;

        Logger.Log(LogLevel.Info, "DZ", "SideLockDisplaySystem loaded");
    }

    public static void Unload()
    {
        if (!_hookInstalled)
            return;
        _hookInstalled = false;

        On.Celeste.OuiChapterPanel.Render -= OnChapterPanelRender;
        On.Celeste.OuiChapterPanel.Update -= OnChapterPanelUpdate;

        Logger.Log(LogLevel.Info, "DZ", "SideLockDisplaySystem unloaded");
    }

    // ── OuiChapterPanel hooks ────────────────────────────────────────────────

    private static void OnChapterPanelRender(On.Celeste.OuiChapterPanel.orig_Render orig,
        OuiChapterPanel self)
    {
        // Let vanilla (and other hooks, e.g. CosmicChapterPanelHook) draw first.
        orig(self);

        AreaData area = AreaData.Get(self.Area);
        if (area == null || !AreaModeExtender.IsOurMap(area))
            return;

        // Don't render overlays while the panel is sliding in/out of view.
        if (!self.Selected)
            return;

        int currentMode = TryGetCurrentMode(self);
        Vector2 cardPos = GetCardPosition(self);

        // Overlay the lock indicator/tooltip/progress for the currently displayed
        // mode when it is a locked extended side.
        if (IsExtendedMode(currentMode) && !AreaModeExtender.IsSideUnlocked(self.Area, currentMode))
        {
            Vector2 modePos = cardPos + new Vector2(-ModeRegionSize.X / 2f, ModeLabelOffsetY);
            DrawLockIndicator(self.Area, currentMode, modePos, ModeRegionSize);
            DrawLockTooltip(self.Area, currentMode, modePos, ModeRegionSize);
            DrawProgressInfo(self.Area, currentMode, modePos, ModeRegionSize);
        }

        // Always render a compact legend listing every locked extended side so the
        // player can see lock status even while browsing A/B/C.
        DrawLockedSidesLegend(self.Area, area, cardPos);
    }

    private static void OnChapterPanelUpdate(On.Celeste.OuiChapterPanel.orig_Update orig,
        OuiChapterPanel self)
    {
        // Block confirming into a locked extended side before vanilla handles the
        // input. Skipping one Update frame only at the moment of the press is
        // invisible to the player and prevents the level from being entered.
        if (self.Selected && Input.MenuConfirm.Pressed)
        {
            int currentMode = TryGetCurrentMode(self);
            if (IsExtendedMode(currentMode) && !AreaModeExtender.IsSideUnlocked(self.Area, currentMode))
            {
                if (!TrySelectSide(self.Area, currentMode))
                    return; // TrySelectSide already played the invalid sound
            }
        }

        orig(self);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static bool IsExtendedMode(int modeIndex)
        => modeIndex == AreaModeExtender.MODE_DSIDE || modeIndex == AreaModeExtender.MODE_DXSIDE;

    /// <summary>
    /// Reads the chapter panel's currently displayed mode index via DynamicData.
    /// Vanilla names the field <c>mode</c>; some builds expose <c>Mode</c>/
    /// <c>currentMode</c>. Falls back to A-Side (0) when unavailable.
    /// </summary>
    private static int TryGetCurrentMode(OuiChapterPanel self)
    {
        try
        {
            var dyn = DynamicData.For(self);
            if (dyn.TryGet("mode", out object m) && m is int mi)
                return mi;
            if (dyn.TryGet("Mode", out m) && m is int mi2)
                return mi2;
            if (dyn.TryGet("currentMode", out m) && m is int mi3)
                return mi3;
            if (dyn.TryGet("CurrentMode", out m) && m is int mi4)
                return mi4;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"[SideLockDisplaySystem] Failed to read current mode: {ex.Message}");
        }

        return AreaModeExtender.MODE_NORMAL;
    }

    /// <summary>
    /// Reads the panel's screen position via DynamicData, falling back to the
    /// screen centre (matches CosmicChapterPanelHook behaviour).
    /// </summary>
    private static Vector2 GetCardPosition(OuiChapterPanel self)
    {
        try
        {
            var dyn = DynamicData.For(self);
            if (dyn.TryGet("position", out object posObj) || dyn.TryGet("Position", out posObj))
            {
                if (posObj is Vector2 v)
                    return v;
            }
        }
        catch
        {
            // Fall through to default below.
        }

        return new Vector2(960f, 540f);
    }

    /// <summary>
    /// Draws a small legend below the chapter card listing each locked extended
    /// side (D/DX) that exists for this chapter, with its lock reason.
    /// </summary>
    private static void DrawLockedSidesLegend(AreaKey area, AreaData areaData, Vector2 cardPos)
    {
        if (areaData?.Mode == null)
            return;

        float y = cardPos.Y + CardSize.Y / 2f + 16f;
        bool drewAny = false;

        for (int mode = AreaModeExtender.MODE_DSIDE; mode <= AreaModeExtender.MODE_DXSIDE; mode++)
        {
            if (mode >= areaData.Mode.Length || areaData.Mode[mode] == null)
                continue;

            if (AreaModeExtender.IsSideUnlocked(area, mode))
                continue;

            string reason = GetLockReason(area, mode);
            if (string.IsNullOrEmpty(reason))
                continue;

            Vector2 pos = new(cardPos.X, y);
            string line = $"{GetSideName(mode)}: {reason}";
            ActiveFont.DrawOutline(line, pos, new Vector2(0.5f, 0f), Vector2.One * 0.45f,
                new Color(220, 220, 220), 1f, Color.Black);

            y += 18f;
            drewAny = true;
        }

        // If nothing was drawn, the legend silently disappears — no extra state to
        // reset.
        _ = drewAny;
    }

    /// <summary>
    /// Checks if a side is locked and returns the lock config if so.
    /// </summary>
    public static LockDisplayConfig GetLockConfig(AreaKey area, int modeIndex)
    {
        if (AreaModeExtender.IsSideUnlocked(area, modeIndex))
            return null;  // Not locked

        return modeIndex switch
        {
            AreaModeExtender.MODE_DSIDE => DSideLockConfig,
            AreaModeExtender.MODE_DXSIDE => DXSideLockConfig,
            _ => null
        };
    }

    /// <summary>
    /// Gets a user-friendly message explaining why a side is locked.
    /// </summary>
    public static string GetLockReason(AreaKey area, int modeIndex)
    {
        if (AreaModeExtender.IsSideUnlocked(area, modeIndex))
            return null;

        var config = GetLockConfig(area, modeIndex);
        if (config == null)
            return "This side is not available";

        return config.RequirementText;
    }

    /// <summary>
    /// Gets progress towards unlocking a locked side.
    /// Returns a string like "Progress: 1/3 heart gems" or null if fully available.
    /// </summary>
    public static string GetUnlockProgress(AreaKey area, int modeIndex)
    {
        if (AreaModeExtender.IsSideUnlocked(area, modeIndex))
            return null;  // Already unlocked

        if (modeIndex < 1 || modeIndex > AreaModeExtender.MODE_DXSIDE)
            return null;

        int previousMode = modeIndex - 1;
        var save = SaveData.Instance;

        if (save == null)
            return null;

        // Check if previous mode is complete
        if (previousMode < 3)  // Vanilla modes
        {
            var areaStats = save.Areas_Safe?[area.ID];
            if (areaStats == null)
                return null;

            if (previousMode < areaStats.Modes?.Length)
            {
                bool completed = areaStats.Modes[previousMode]?.Completed ?? false;
                bool hasHeart = areaStats.Modes[previousMode]?.HeartGem ?? false;

                if (!completed)
                    return "Progress: Beat the previous side";

                if (!hasHeart)
                    return "Progress: Collect the heart gem";

                return null;  // Should be unlocked
            }
        }
        else  // Extended modes
        {
            var areaData = AreaData.Get(area);
            string heartId = $"{areaData?.SID}_{AreaModeExtender.GetModeName(previousMode)}";
            bool hasCollected = DZModule.SaveData?.HasCollectedHeartGem(heartId) == true;

            if (!hasCollected)
                return "Progress: Collect the heart gem from the previous side";
        }

        return null;
    }

    /// <summary>
    /// Attempts to get a user-friendly side name for a mode index.
    /// </summary>
    public static string GetSideName(int modeIndex)
    {
        return modeIndex switch
        {
            AreaModeExtender.MODE_NORMAL => "A-Side",
            AreaModeExtender.MODE_BSIDE => "B-Side",
            AreaModeExtender.MODE_CSIDE => "C-Side",
            AreaModeExtender.MODE_DSIDE => "D-Side",
            AreaModeExtender.MODE_DXSIDE => "DX-Side",
            _ => $"Side {modeIndex}"
        };
    }

    /// <summary>
    /// Checks if a side tab should appear greyed-out/disabled in the UI.
    /// </summary>
    public static bool ShouldGreyOut(AreaKey area, int modeIndex)
    {
        return !AreaModeExtender.IsSideUnlocked(area, modeIndex);
    }

    /// <summary>
    /// Renders a lock indicator overlay for a locked side.
    /// Call this from your chapter panel rendering code at the position of the side tab.
    /// </summary>
    public static void DrawLockIndicator(AreaKey area, int modeIndex, Vector2 position, Vector2 size)
    {
        if (AreaModeExtender.IsSideUnlocked(area, modeIndex))
            return;  // Not locked, don't draw

        var config = GetLockConfig(area, modeIndex);
        if (config == null)
            return;

        // Semi-transparent overlay
        Draw.Rect(position.X, position.Y, size.X, size.Y, config.LockTint);

        // Try to draw lock icon centered on the side button
        try
        {
            var lockIcon = GFX.Gui[config.LockIcon];
            if (lockIcon != null)
            {
                Vector2 center = position + size / 2f;
                lockIcon.DrawCentered(center, Color.White);
            }
        }
        catch
        {
            // Icon not available; skip drawing it
        }

        // Draw lock label text below the tab
        string label = config.LockedLabel;
        if (!string.IsNullOrEmpty(label))
        {
            Vector2 labelPos = position + new Vector2(size.X / 2f, size.Y + 4f);
            ActiveFont.DrawOutline(label, labelPos, new Vector2(0.5f, 0f), Vector2.One * 0.6f, Color.White, 2f, Color.Black);
        }
    }

    /// <summary>
    /// Optionally render a tooltip below a mode tab showing lock status.
    /// </summary>
    public static void DrawLockTooltip(AreaKey area, int modeIndex, Vector2 position, Vector2 size)
    {
        string reason = GetLockReason(area, modeIndex);
        if (string.IsNullOrEmpty(reason))
            return;

        // Draw at the bottom of the tab with a small text
        Vector2 tooltipPos = position + new Vector2(size.X / 2f, size.Y + 20f);
        ActiveFont.DrawOutline(reason, tooltipPos, new Vector2(0.5f, 0f), Vector2.One * 0.5f, Color.White, 1f, Color.Black);
    }

    /// <summary>
    /// Optionally render progress text for a locked side.
    /// Shows "Progress: Beat the previous side" style messages.
    /// </summary>
    public static void DrawProgressInfo(AreaKey area, int modeIndex, Vector2 position, Vector2 size)
    {
        string progress = GetUnlockProgress(area, modeIndex);
        if (string.IsNullOrEmpty(progress))
            return;

        Vector2 progressPos = position + new Vector2(size.X / 2f, size.Y + 35f);
        ActiveFont.DrawOutline(progress, progressPos, new Vector2(0.5f, 0f), Vector2.One * 0.45f, new Color(200, 200, 200), 1f, Color.Black);
    }

    /// <summary>
    /// Utility: Prevents clicking a locked side tab.
    /// Call this in your chapter panel input handler.
    /// </summary>
    public static bool TrySelectSide(AreaKey area, int modeIndex)
    {
        if (!AreaModeExtender.IsSideUnlocked(area, modeIndex))
        {
            // Optional: Play a "locked" sound effect
            Audio.Play("event:/ui/main/button_invalid");
            return false;  // Selection blocked
        }

        return true;  // Selection allowed
    }
}

