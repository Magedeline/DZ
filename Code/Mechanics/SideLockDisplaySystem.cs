using System;
using global::Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

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

    public static void Load()
    {
        if (_hookInstalled)
            return;
        _hookInstalled = true;

        // Hook chapter panel rendering or input to intercept locked side selection
        // This depends on your chapter panel implementation
        // For example: On.Celeste.OuiChapterPanel.ctor += OnChapterPanelCtor;
        // Or: On.Celeste.OuiChapterPanel.Update += OnChapterPanelUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper", "SideLockDisplaySystem loaded");
    }

    public static void Unload()
    {
        if (!_hookInstalled)
            return;
        _hookInstalled = false;

        Logger.Log(LogLevel.Info, "MaggyHelper", "SideLockDisplaySystem unloaded");
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
            bool hasCollected = MaggyHelperModule.SaveData?.HasCollectedHeartGem(heartId) == true;

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

