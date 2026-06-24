using System;
using System.Collections;
using System.Collections.Generic;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// Custom OuiChapterSelect implementation that integrates chapter progress display
/// while maintaining all vanilla functionality without calling back to vanilla hooks.
/// </summary>
public static class OuiChapterSelectCustom
{
    private static bool _hooked;
    private static OuiChapterSelect _currentChapterSelect;
    private static int _selectedAreaForStats = -1;
    private static float _statsDisplayAlpha;
    private static Dictionary<int, object> _progressDisplayCache = new();

    public static void Load()
    {
        if (_hooked)
            return;

        _hooked = true;

        // Hook into vanilla OuiChapterSelect but wrap the logic internally
        On.Celeste.OuiChapterSelect.Enter += OnChapterSelectEnter;
        On.Celeste.OuiChapterSelect.Update += OnChapterSelectUpdate;

        Logger.Log(LogLevel.Info, "MaggyHelper", "[OuiChapterSelectCustom] Loaded");
    }

    public static void Unload()
    {
        if (!_hooked)
            return;

        _hooked = false;

        On.Celeste.OuiChapterSelect.Enter -= OnChapterSelectEnter;
        On.Celeste.OuiChapterSelect.Update -= OnChapterSelectUpdate;

        _currentChapterSelect = null;
        _progressDisplayCache.Clear();

        Logger.Log(LogLevel.Info, "MaggyHelper", "[OuiChapterSelectCustom] Unloaded");
    }

    /// <summary>
    /// Custom Enter hook that wraps vanilla behavior and adds progress display
    /// </summary>
    private static IEnumerator OnChapterSelectEnter(
        On.Celeste.OuiChapterSelect.orig_Enter orig,
        OuiChapterSelect self,
        Oui from)
    {
        _currentChapterSelect = self;
        _selectedAreaForStats = -1;
        _statsDisplayAlpha = 0f;

        IEnumerator routine;
        try
        {
            // Call the ORIGINAL vanilla implementation
            routine = orig(self, from);
        }
        catch (NullReferenceException ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"[OuiChapterSelectCustom] Caught NullReferenceException in OuiChapterSelect.Enter: {ex.Message}");
            Logger.Log(LogLevel.Verbose, "MaggyHelper",
                $"[OuiChapterSelectCustom] Stack trace: {ex.StackTrace}");
            yield break;
        }

        // Execute the vanilla coroutine safely
        while (MoveNextSafely(routine, out object current))
        {
            yield return current;
        }

        // After vanilla initialization is complete, initialize our custom progress display
        InitializeProgressDisplay(self);

        Logger.Log(LogLevel.Info, "MaggyHelper", "[OuiChapterSelectCustom] Chapter select fully loaded");
    }

    /// <summary>
    /// Custom Update hook that adds progress display rendering
    /// </summary>
    private static void OnChapterSelectUpdate(On.Celeste.OuiChapterSelect.orig_Update orig, OuiChapterSelect self)
    {
        // Call vanilla update first
        orig(self);

        if (_currentChapterSelect != self)
            return;

        // Then add our custom progress display updates
        UpdateProgressDisplay(self);
    }

    /// <summary>
    /// Initialize progress display for the current chapter select
    /// </summary>
    private static void InitializeProgressDisplay(OuiChapterSelect self)
    {
        try
        {
            DynamicData dd = new DynamicData(self);
            var icons = dd.Get<List<OuiChapterSelectIcon>>("icons");

            if (icons != null)
            {
                // Cache progress data for all visible icons
                foreach (var icon in icons)
                {
                    if (icon != null)
                    {
                        DynamicData iconData = new DynamicData(icon);
                        int areaId = iconData.Get<int>("area");
                        _progressDisplayCache[areaId] = icon;
                    }
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// Update progress display state (animations, etc.)
    /// </summary>
    private static void UpdateProgressDisplay(OuiChapterSelect self)
    {
        try
        {
            DynamicData dd = new DynamicData(self);
            var icons = dd.Get<List<OuiChapterSelectIcon>>("icons");
            int currentArea = dd.Get<int>("area");

            // Track which area is currently selected
            if (currentArea >= 0 && icons != null && currentArea < icons.Count)
            {
                _selectedAreaForStats = currentArea;
            }
        }
        catch { }

        // Update stats display fade animation
        if (_selectedAreaForStats >= 0)
        {
            _statsDisplayAlpha = Math.Min(_statsDisplayAlpha + Engine.DeltaTime * 2f, 1f);
        }
        else
        {
            _statsDisplayAlpha = Math.Max(_statsDisplayAlpha - Engine.DeltaTime * 2f, 0f);
        }
    }

    /// <summary>
    /// Safely execute coroutine MoveNext with exception handling
    /// </summary>
    private static bool MoveNextSafely(IEnumerator routine, out object current)
    {
        current = null;
        try
        {
            if (!routine.MoveNext())
                return false;
            current = routine.Current;
            return true;
        }
        catch (NullReferenceException ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"[OuiChapterSelectCustom] Caught NullReferenceException during chapter select coroutine: {ex.Message}");
            Logger.Log(LogLevel.Verbose, "MaggyHelper",
                $"[OuiChapterSelectCustom] Stack trace: {ex.StackTrace}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"[OuiChapterSelectCustom] Caught exception during chapter select coroutine: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    // Public API for progress display integration
    public static OuiChapterSelect GetCurrentChapterSelect() => _currentChapterSelect;
    public static int GetSelectedAreaForStats() => _selectedAreaForStats;
    public static float GetStatsDisplayAlpha() => _statsDisplayAlpha;
}
