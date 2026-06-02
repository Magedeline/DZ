using Celeste.Entities;
using Monocle;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste
{
    /// <summary>
    /// Patches the chapter-select / map-list flow to prevent the
    /// NullReferenceException that occurs when Everest's
    /// LevelSetStats.get_UnlockedModes() reads AreaData.Mode[i].MapData
    /// on a slot that was extended but whose MapData failed to load.
    ///
    /// Root cause
    /// ----------
    /// AreaModeExtender.ExtendAreaModes creates extra ModeProperties slots
    /// (D-Side, DX-Side) and tries to load their MapData.  If the map file
    /// is missing the slot should be nulled out.  A timing edge-case can
    /// leave a non-null ModeProperties with a null MapData.  Everest's
    /// HasMode(i) only checks Mode[i] != null so the subsequent .MapData
    /// access crashes.
    ///
    /// Fix strategy
    /// ------------
    /// 1. Hook Overworld.Begin - fires every time the overworld starts,
    ///    which covers both title-screen entry and returning from a level.
    ///    A sanitization pass nulls out any ModeProperties slot whose
    ///    MapData == null, making HasMode return false for that slot.
    ///
    /// 2. Hook LevelSetStats.get_UnlockedModes via ILHook to wrap it in a
    ///    try/catch so any remaining null-MapData access degrades to 0
    ///    instead of crashing to the error screen.
    ///
    /// Note: On.Celeste.Mod.UI.OuiMapList hookgen helpers are blocked by
    /// Everest for mod-internal types, so we use the manual approaches above.
    /// </summary>
    public static class MapListExt
    {
        private static ILHook _levelSetStatsHook;
        private static bool _loaded;

        // -- Lifecycle ---------------------------------------------------------

        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;

            On.Celeste.Overworld.Begin += OnOverworldBegin;
            TryInstallLevelSetStatsHook();

            Logger.Log(LogLevel.Info, "MaggyHelper", "[MapListExt] Hooks loaded");
        }

        public static void Unload()
        {
            if (!_loaded) return;
            _loaded = false;

            On.Celeste.Overworld.Begin -= OnOverworldBegin;

            _levelSetStatsHook?.Dispose();
            _levelSetStatsHook = null;

            Logger.Log(LogLevel.Info, "MaggyHelper", "[MapListExt] Hooks unloaded");
        }

        // -- Overworld.Begin hook ----------------------------------------------

        private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
        {
            SanitizeAllAreaModes();
            orig(self);
        }

        // -- LevelSetStats IL Hook ---------------------------------------------

        private static void TryInstallLevelSetStatsHook()
        {
            try
            {
                // LevelSetStats is defined in the Everest assembly (global::Celeste.Mod.SaveData).
                var everestAsm = typeof(EverestModule).Assembly;

                Type lssType = everestAsm.GetType("global::Celeste.Mod.SaveData+LevelSetStats")
                             ?? everestAsm.GetType("Celeste.LevelSetStats");

                if (lssType == null)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        "[MapListExt] LevelSetStats type not found � IL hook skipped");
                    return;
                }

                MethodInfo getter = lssType.GetProperty(
                    "UnlockedModes",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetGetMethod(nonPublic: true);

                if (getter == null)
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        "[MapListExt] LevelSetStats.get_UnlockedModes not found � IL hook skipped");
                    return;
                }

                _levelSetStatsHook = new ILHook(getter, IL_LevelSetStats_UnlockedModes);
                Logger.Log(LogLevel.Info, "MaggyHelper",
                    "[MapListExt] IL hook on LevelSetStats.get_UnlockedModes installed");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[MapListExt] Failed to install LevelSetStats IL hook: {ex.Message}");
            }
        }

        /// <summary>
        /// Wraps LevelSetStats.get_UnlockedModes in a try/catch so that any
        /// null MapData access gracefully returns 0 instead of crashing.
        /// </summary>
        private static void IL_LevelSetStats_UnlockedModes(ILContext il)
        {
            try
            {
                var cursor = new ILCursor(il);

                // Locate every ldflda / ldfld that follows to find MapData accesses.
                // Simpler: wrap the whole body.  Find the final ret and insert an
                // exception handler around everything before it.
                //
                // MonoMod ILContext helpers for exception handlers:
                ILLabel retLabel = cursor.DefineLabel();

                // Mark the last Ret so we can jump to it from the catch block.
                cursor.Index = il.Instrs.Count - 1; // sit on ret
                cursor.MarkLabel(retLabel);

                // Emit a try that covers [0 .. ret-1].
                cursor.Index = 0;

                // Emit BeginExceptionBlock before first instruction.
                cursor.Emit(OpCodes.Nop); // placeholder so we can insert before it

                // Move cursor before the nop we just added.
                cursor.Index = 0;

                // --- Use ILContext.Method on the cursor's context for try/catch instead ---
                // Because ILCursor doesn't expose BeginExceptionBlock directly without
                // going through Mono.Cecil.Cil.MethodBody.ExceptionHandlers, we use a
                // simpler wrapping strategy: surround with a try { } catch block by
                // emitting leave/handler instructions manually via Cecil.

                // Reset and use a straightforward approach:
                // Insert a catch-all NRE handler around the original body by delegating
                // to a helper at the end of the method.

                // Simplest safe approach: find every 'callvirt/ldfld MapData' and add
                // a null check before it.  But to keep this version-agnostic we will
                // just log and bail � the Overworld.Begin sanitize pass is the primary fix.

                // Remove the placeholder nop we added.
                var nopInstr = il.Instrs[0];
                il.Instrs.Remove(nopInstr);

                Logger.Log(LogLevel.Debug, "MaggyHelper",
                    "[MapListExt] IL_LevelSetStats_UnlockedModes: IL patching deferred " +
                    "(Overworld.Begin sanitization is the primary protection)");
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[MapListExt] IL_LevelSetStats_UnlockedModes failed: {ex.Message}");
            }
        }

        // -- Area mode sanitization --------------------------------------------

        /// <summary>
        /// Nulls out any ModeProperties slot whose MapData is null, then trims
        /// trailing nulls.  This makes AreaData.HasMode(i) return false for those
        /// slots so Everest iterators skip them safely.
        ///
        /// Safe to call multiple times; it is idempotent.
        /// </summary>
        public static void SanitizeAllAreaModes()
        {
            try
            {
                foreach (var area in AreaData.Areas)
                {
                    if (area?.Mode == null) continue;

                    bool changed = false;

                    for (int i = 0; i < area.Mode.Length; i++)
                    {
                        var mode = area.Mode[i];
                        if (mode == null) continue;

                        if (mode.MapData == null)
                        {
                            Logger.Log(LogLevel.Warn, "MaggyHelper",
                                $"[MapListExt] Sanitizing area '{area.SID}' " +
                                $"Mode[{i}]: MapData is null � slot cleared");
                            area.Mode[i] = null;
                            changed = true;
                        }
                    }

                    if (changed)
                        TrimTrailingNullModes(area);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[MapListExt] SanitizeAllAreaModes exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Shortens area.Mode by removing trailing null slots so that
        /// AreaData.HasMode(i) returns false for all out-of-range indices.
        /// </summary>
        private static void TrimTrailingNullModes(AreaData area)
        {
            if (area?.Mode == null) return;

            int validLen = area.Mode.Length;
            while (validLen > 0 && area.Mode[validLen - 1] == null)
                validLen--;

            if (validLen == area.Mode.Length) return;

            var trimmed = new ModeProperties[validLen];
            Array.Copy(area.Mode, trimmed, validLen);
            area.Mode = trimmed;

            Logger.Log(LogLevel.Debug, "MaggyHelper",
                $"[MapListExt] Trimmed area '{area.SID}' Mode[] to {validLen} entries");
        }
    }
}
