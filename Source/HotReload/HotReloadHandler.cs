using System;
using System.Reflection;
using System.Reflection.Metadata;
using Celeste.Mod.DZ;
using Celeste.Mod.DZ.HotReload;
using Microsoft.Xna.Framework;
using Monocle;

[assembly: MetadataUpdateHandler(typeof(Celeste.HotReload.HotReloadHandler))]

namespace Celeste.HotReload
{
    /// <summary>
    /// Handles hot reload notifications from two sources:
    /// 1. .NET MetadataUpdateHandler (dotnet watch with Hot Reload patching)
    /// 2. Everest's CodeReload assembly swap (full rebuild + reload) via NotifyEverestReload()
    /// </summary>
    public static class HotReloadHandler
    {
        public static void ClearCache(Type[] types)
        {
            if (DZModule.Settings?.HotReloadVerbose == true)
            {
                Logger.Log(LogLevel.Verbose, "DZ", $"[HotReload] ClearCache called for {types?.Length ?? 0} types");
            }
        }

        public static void UpdateApplication(Type[] types)
        {
            if (DZModule.Settings?.HotReloadEnabled == false) return;

            int reloadedCount = 0;
            if (types != null)
            {
                foreach (var type in types)
                {
                    if (type == null) continue;

                    if (IsHotReloadable(type))
                    {
                        reloadedCount++;
                        InvokeOnReload(type);
                    }
                }
            }

            Logger.Log(LogLevel.Info, "DZ", $"[HotReload] Assembly updated. {reloadedCount} hot-reloadable types affected.");
            Notify($"HOT RELOAD: {reloadedCount} types updated", Color.LimeGreen);
        }

        /// <summary>
        /// Called from DZModule.Load() when Everest swaps in a freshly
        /// rebuilt mod assembly while the game is already running.
        /// Invokes OnReload() on all [HotReloadable] types in the new assembly.
        /// </summary>
        public static void NotifyEverestReload()
        {
            if (DZModule.Settings?.HotReloadEnabled == false) return;

            int reloadedCount = 0;
            Type[] types;
            try
            {
                types = typeof(HotReloadHandler).Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }

            foreach (var type in types)
            {
                if (type == null || !IsHotReloadable(type)) continue;
                reloadedCount++;
                InvokeOnReload(type);
            }

            Logger.Log(LogLevel.Info, "DZ", $"[HotReload] Everest reloaded mod assembly. {reloadedCount} hot-reloadable types notified.");
            Notify($"HOT RELOAD: assembly swapped ({reloadedCount} types notified)", Color.LimeGreen);
        }

        private static bool IsHotReloadable(Type type)
        {
            foreach (var attr in type.GetCustomAttributes(false))
            {
                if (attr.GetType().Name == "HotReloadableAttribute")
                    return true;
            }
            return false;
        }

        private static void InvokeOnReload(Type type)
        {
            var onReload = type.GetMethod("OnReload", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            if (onReload == null) return;
            try
            {
                onReload.Invoke(null, null);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ", $"[HotReload] Failed to call OnReload for {type.Name}: {ex.Message}");
            }
        }

        private static void Notify(string message, Color color)
        {
            if (DZModule.Settings?.HotReloadShowUI == true)
            {
                HotReloadUI.Show(message, color);
            }

            if (DZModule.Settings?.HotReloadSound == true)
            {
                try { Audio.Play("event:/ui/main/button_select"); } catch {}
            }
        }
    }
}
