using System;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.DZ.HotReload
{
    /// <summary>
    /// Guards against an Everest bug where ModContent.Update(prev, next) throws
    /// ArgumentNullException (null dictionary key) when a changed file does not
    /// map to a virtual asset path (e.g. build intermediates churning during
    /// dotnet watch rebuilds). Without this guard, every rebuild floods the log
    /// with "[QueuedTask] A queued task ... failed." errors.
    /// </summary>
    public static class EverestContentUpdateGuard
    {
        private static Hook hook;

        public static void Load()
        {
            if (hook != null) return;

            MethodInfo target = typeof(ModContent).GetMethod(
                "Update",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(ModAsset), typeof(ModAsset) },
                null);

            if (target == null)
            {
                Logger.Log(LogLevel.Warn, "DZ", "[HotReload] Could not find ModContent.Update(ModAsset, ModAsset) to guard.");
                return;
            }

            hook = new Hook(target, GuardedUpdate);
            Logger.Log(LogLevel.Verbose, "DZ", "[HotReload] Installed ModContent.Update null-key guard.");
        }

        public static void Unload()
        {
            hook?.Dispose();
            hook = null;
        }

        private delegate void orig_Update(ModContent self, ModAsset prev, ModAsset next);

        private static void GuardedUpdate(orig_Update orig, ModContent self, ModAsset prev, ModAsset next)
        {
            // Skip updates for assets with no virtual path - inserting or removing
            // them from Everest's asset map would throw ArgumentNullException (null key).
            if ((next != null && next.PathVirtual == null) || (prev != null && prev.PathVirtual == null))
            {
                if (DZModule.Settings?.HotReloadVerbose == true)
                    Logger.Log(LogLevel.Verbose, "DZ", $"[HotReload] Skipped content update for asset with null virtual path: {(next as FileSystemModAsset ?? prev as FileSystemModAsset)?.Path ?? "<unknown>"}");
                return;
            }

            try
            {
                orig(self, prev, next);
            }
            catch (ArgumentNullException ex)
            {
                Logger.Log(LogLevel.Warn, "DZ", $"[HotReload] Suppressed ModContent.Update null-key error: {ex.Message}");
            }
        }
    }
}
