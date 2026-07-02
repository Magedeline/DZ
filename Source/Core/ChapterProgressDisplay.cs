using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.DZ;

/// <summary>
/// Displays chapter progress on the OUI chapter select screen, including:
/// - Completion badges for A/B/C/D/DX-Side and custom sides
/// - Berry collection count
/// - Progress bar with percentage
/// - Detailed stats panel on hover
/// </summary>
public static class ChapterProgressDisplay
{
    private static bool _hooked;
    private static Dictionary<int, ProgressData> _progressCache = new();

    public class SideProgress
    {
        public string Name { get; set; }
        public int Mode { get; set; }
        public bool Completed { get; set; }
        public Color Color { get; set; }
    }

    public class ProgressData
    {
        public int AreaId { get; set; }
        public int BerriesCollected { get; set; }
        public int MaxBerries { get; set; }
        public int HeartGemsCollected { get; set; }
        public int MaxHeartGems { get; set; }
        public List<SideProgress> Sides { get; set; } = new();
        public int DeathCount { get; set; }
        public long CompletionTime { get; set; }

        public float GetCompletionPercentage()
        {
            int total = MaxBerries + MaxHeartGems;
            if (total == 0) return 0f;
            int collected = BerriesCollected + HeartGemsCollected;
            return (float)collected / total * 100f;
        }

        public int GetComplete2Count()
        {
            return Sides.Count(s => s.Completed);
        }
    }

    public static void Load()
    {
        if (_hooked)
            return;

        _hooked = true;

        On.Celeste.OuiChapterSelect.Enter += OnChapterSelectEnter;
        On.Celeste.OuiChapterSelect.Update += OnChapterSelectUpdate;

        Logger.Log(LogLevel.Info, "DZ", "[ChapterProgressDisplay] Loaded");
    }

    public static void Unload()
    {
        if (!_hooked)
            return;

        _hooked = false;

        On.Celeste.OuiChapterSelect.Enter -= OnChapterSelectEnter;
        On.Celeste.OuiChapterSelect.Update -= OnChapterSelectUpdate;

        Logger.Log(LogLevel.Info, "DZ", "[ChapterProgressDisplay] Unloaded");
    }

    private static System.Collections.IEnumerator OnChapterSelectEnter(
        On.Celeste.OuiChapterSelect.orig_Enter orig,
        OuiChapterSelect self,
        Oui from)
    {
        yield return orig(self, from);
        RefreshProgressCache();
    }

    private static void OnChapterSelectUpdate(On.Celeste.OuiChapterSelect.orig_Update orig, OuiChapterSelect self)
    {
        orig(self);

        DynamicData dd = new DynamicData(self);

        try
        {
            var icons = dd.Get<List<OuiChapterSelectIcon>>("icons");
            if (icons != null)
            {
                // Draw progress overlays for all icons
                DrawProgressOverlays(icons);
            }
        }
        catch { }
    }

    private static void DrawProgressOverlays(List<OuiChapterSelectIcon> icons)
    {
        foreach (var icon in icons)
        {
            if (icon == null) continue;

            DynamicData iconData = new DynamicData(icon);
            int areaId = iconData.Get<int>("area");

            if (!_progressCache.TryGetValue(areaId, out var progress))
                continue;

            Vector2 position = icon.Position;

            // Draw completion badges (A/B/C/D-Side)
            DrawCompletionBadges(progress, position);

            // Draw berry count
            DrawBerryCount(progress, position);

            // Draw progress bar
            DrawProgressBar(progress, position);
        }

        // Draw detailed stats panel for selected chapter
        DrawDetailedStatsPanel();
    }

    private static void DrawDetailedStatsPanel()
    {
        int selectedArea = global::Celeste.Mod.DZ.OuiChapterSelectCustom.GetSelectedAreaForStats();
        float alpha = global::Celeste.Mod.DZ.OuiChapterSelectCustom.GetStatsDisplayAlpha();

        if (selectedArea < 0 || alpha <= 0f)
            return;

        if (!_progressCache.TryGetValue(selectedArea, out var progress))
            return;

        var area = AreaData.Get(selectedArea);
        if (area == null || area.SID == null)
            return;

        // Draw stats panel on the right side of screen
        Vector2 panelPos = new Vector2(280, 60);
        float panelWidth = 200f;
        float panelHeight = 140f;
        Color panelColor = Color.Black * (0.7f * alpha);

        // Background
        Draw.Rect(panelPos, panelWidth, panelHeight, panelColor);
        Draw.HollowRect(panelPos, panelWidth, panelHeight, Color.White * (0.5f * alpha));

        // Title
        Vector2 textPos = panelPos + new Vector2(10, 10);
        string chapterName = area.Name ?? $"Chapter {area.ID}";

        // Draw stats
        float lineHeight = 18f;
        textPos.Y += lineHeight;

        // Completion status - show all sides
        string sideStatus = "Sides: ";
        foreach (var side in progress.Sides)
        {
            sideStatus += $"{side.Name}:{(side.Completed ? "✓" : "-")} ";
        }

        // Berry count
        textPos.Y += lineHeight;
        string berryStatus = $"Berries: {progress.BerriesCollected}/{progress.MaxBerries}";

        // Heart gems
        textPos.Y += lineHeight;
        string heartStatus = $"Hearts: {progress.HeartGemsCollected}/{progress.MaxHeartGems}";

        // Completion percentage
        textPos.Y += lineHeight;
        float completionPercent = progress.GetCompletionPercentage();
        string percentStatus = $"Completion: {completionPercent:F0}%";

        // Sides completed
        textPos.Y += lineHeight;
        string sidesStatus = $"Sides: {progress.GetComplete2Count()}/{progress.Sides.Count}";

        // Death count (if available)
        if (progress.DeathCount > 0)
        {
            textPos.Y += lineHeight;
            string deathStatus = $"Deaths: {progress.DeathCount}";
        }
    }

    private static void DrawCompletionBadges(ProgressData progress, Vector2 position)
    {
        float badgeSize = 10f;
        float spacing = 1f;
        Vector2 badgeStart = position + new Vector2(60, 35);

        for (int i = 0; i < progress.Sides.Count; i++)
        {
            var side = progress.Sides[i];
            Vector2 badgePos = badgeStart + new Vector2(i * (badgeSize + spacing), 0);

            if (!side.Completed)
            {
                Draw.Rect(badgePos, badgeSize, badgeSize, Color.DarkGray * 0.5f);
            }
            else
            {
                Draw.Rect(badgePos, badgeSize, badgeSize, side.Color);
                Draw.Rect(badgePos + Vector2.One, badgeSize - 2, badgeSize - 2, Color.Black * 0.3f);
            }
        }
    }

    private static void DrawBerryCount(ProgressData progress, Vector2 position)
    {
        if (progress.MaxBerries <= 0)
            return;

        // Draw berry indicator rectangles
        Vector2 berryIndicatorStart = position + new Vector2(35, 55);
        float squareSize = 4f;
        float spacing = 1f;

        // Draw collected berries
        for (int i = 0; i < progress.MaxBerries; i++)
        {
            Vector2 squarePos = berryIndicatorStart + new Vector2(i * (squareSize + spacing), 0);
            if (i < progress.BerriesCollected)
            {
                Draw.Rect(squarePos, squareSize, squareSize, Color.DeepSkyBlue);
            }
            else
            {
                Draw.Rect(squarePos, squareSize, squareSize, Color.DarkGray * 0.5f);
            }
        }
    }

    private static void DrawProgressBar(ProgressData progress, Vector2 position)
    {
        float barWidth = 70f;
        float barHeight = 4f;
        Vector2 barPos = position + new Vector2(15, 68);

        float percentage = progress.GetCompletionPercentage() / 100f;
        Color barColor = GetProgressBarColor(percentage);

        Draw.Rect(barPos, barWidth, barHeight, Color.DarkGray);
        Draw.Rect(barPos, barWidth * percentage, barHeight, barColor);
        Draw.HollowRect(barPos, barWidth, barHeight, Color.White * 0.5f);
    }

    private static Color GetProgressBarColor(float percentage)
    {
        if (percentage >= 1f) return Color.Gold;
        if (percentage >= 0.75f) return Color.LimeGreen;
        if (percentage >= 0.5f) return Color.Yellow;
        if (percentage >= 0.25f) return Color.Orange;
        return Color.Red;
    }

    private static void RefreshProgressCache()
    {
        _progressCache.Clear();

        try
        {
            var save = SaveData.Instance;
            var modSave = global::Celeste.Mod.DZ.DZModule.SaveData;

            if (save == null || modSave == null)
                return;

            foreach (var area in AreaData.Areas)
            {
                if (area == null || area.SID == null)
                    continue;

                var progress = new ProgressData
                {
                    AreaId = area.ID,
                };

                // Get all available sides for this chapter
                Buil2List(save, area, progress);

                CountChapterCollectibles(save, modSave, area.SID, area.ID, progress);
                GetChapterStats(modSave, area.SID, progress);

                _progressCache[area.ID] = progress;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ",
                $"[ChapterProgressDisplay] Error refreshing progress cache: {ex.Message}");
        }
    }

    private static void Buil2List(SaveData save, AreaData area, ProgressData progress)
    {
        var sideColors = new Color[]
        {
            Color.LimeGreen,           // A-Side
            new Color(100, 150, 255),  // B-Side
            new Color(255, 100, 150),  // C-Side
            new Color(255, 200, 50),   // D-Side
            new Color(200, 150, 255),  // DX-Side
            new Color(100, 255, 255),  // Custom sides
        };

        var sideNames = new string[] { "A", "B", "C", "D", "DX" };

        // Check standard sides (A, B, C, D)
        for (int i = 0; i < Math.Min(4, area.Mode.Length); i++)
        {
            if (area.Mode[i] != null)
            {
                bool completed = IsModeSideCompleted(save, area.ID, i);
                progress.Sides.Add(new SideProgress
                {
                    Name = i < sideNames.Length ? sideNames[i] : $"S{i}",
                    Mode = i,
                    Completed = completed,
                    Color = sideColors[i % sideColors.Length]
                });
            }
        }

        // Check for extended sides (D-Side, DX-Side) using AreaModeExtender
        try
        {
            // D-Side
            if (global::Celeste.AreaModeExtender.GetSaveAreaModeCount(area.ID) > 3)
            {
                bool 2Completed = IsModeSideCompleted(save, area.ID, 3);
                if (!progress.Sides.Any(s => s.Mode == 3))
                {
                    progress.Sides.Add(new SideProgress
                    {
                        Name = "D",
                        Mode = 3,
                        Completed = 2Completed,
                        Color = sideColors[3]
                    });
                }
            }

            // DX-Side
            if (global::Celeste.AreaModeExtender.GetSaveAreaModeCount(area.ID) > 4)
            {
                bool dxSideCompleted = IsModeSideCompleted(save, area.ID, 4);
                if (!progress.Sides.Any(s => s.Mode == 4))
                {
                    progress.Sides.Add(new SideProgress
                    {
                        Name = "DX",
                        Mode = 4,
                        Completed = dxSideCompleted,
                        Color = sideColors[4]
                    });
                }
            }
        }
        catch
        {
            // Fall back to vanilla behavior if AreaModeExtender is not available
        }
    }

    private static bool IsModeSideCompleted(SaveData save, int areaId, int mode)
    {
        try
        {
            var stats = save.Areas_Safe.ElementAtOrDefault(areaId);
            if (stats == null)
                return false;

            var modes = stats.Modes;
            if (mode < 0 || mode >= modes.Length)
                return false;

            return modes[mode].Completed;
        }
        catch
        {
            return false;
        }
    }

    private static void CountChapterCollectibles(SaveData save,
        global::Celeste.Mod.DZ.DZModuleSaveData modSave,
        string chapterSid, int areaId, ProgressData progress)
    {
        try
        {
            var area = AreaData.Get(areaId);
            if (area == null || area.Mode == null || area.Mode.Length == 0)
                return;

            var modeData = area.Mode[0];
            progress.MaxBerries = modeData.TotalStrawberries;

            var stats = save.Areas_Safe.ElementAtOrDefault(areaId);
            if (stats != null && stats.Modes.Length > 0)
            {
                progress.BerriesCollected = stats.Modes[0].TotalStrawberries;
            }

            int heartCount = 0;
            for (int i = 0; i < 3; i++)
            {
                string heartId = $"{chapterSid}_heartgem_{i}";
                if (modSave.HasCollectedHeartGem(heartId))
                    heartCount++;
            }
            progress.HeartGemsCollected = heartCount;
            progress.MaxHeartGems = 3;
        }
        catch { }
    }

    private static void GetChapterStats(
        global::Celeste.Mod.DZ.DZModuleSaveData modSave,
        string chapterSid, ProgressData progress)
    {
        try
        {
            string deathKey = $"{chapterSid}_deaths";
            if (modSave.ChapterDeathCounts.TryGetValue(deathKey, out int deaths))
            {
                progress.DeathCount = deaths;
            }

            string timeKey = $"{chapterSid}_time";
            if (modSave.ChapterCompletionTimes.TryGetValue(timeKey, out long time))
            {
                progress.CompletionTime = time;
            }
        }
        catch { }
    }
}
