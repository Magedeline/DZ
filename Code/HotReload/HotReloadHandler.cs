using System;
using System.Reflection;
using System.Reflection.Metadata;
using Celeste.Mod.MaggyHelper;
using Celeste.Mod.MaggyHelper.HotReload;
using Microsoft.Xna.Framework;
using Monocle;

[assembly: MetadataUpdateHandler(typeof(Celeste.HotReload.HotReloadHandler))]

namespace Celeste.HotReload
{
    public static class HotReloadHandler
    {
        public static void ClearCache(Type[] types)
        {
            if (MaggyHelperModule.Settings?.HotReloadVerbose == true)
            {
                Logger.Log(LogLevel.Verbose, "MaggyHelper", $"[HotReload] ClearCache called for {types?.Length ?? 0} types");
            }
        }

        public static void UpdateApplication(Type[] types)
        {
            if (MaggyHelperModule.Settings?.HotReloadEnabled == false) return;

            int reloadedCount = 0;
            if (types != null)
            {
                foreach (var type in types)
                {
                    if (type == null) continue;

                    bool isReloadable = false;
                    foreach (var attr in type.GetCustomAttributes(false))
                    {
                        if (attr.GetType().Name == "HotReloadableAttribute")
                        {
                            isReloadable = true;
                            break;
                        }
                    }

                    if (isReloadable)
                    {
                        reloadedCount++;
                        var onReload = type.GetMethod("OnReload", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                        if (onReload != null)
                        {
                            try
                            {
                                onReload.Invoke(null, null);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(LogLevel.Warn, "MaggyHelper", $"[HotReload] Failed to call OnReload for {type.Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper", $"[HotReload] Assembly updated. {reloadedCount} hot-reloadable types affected.");

            if (MaggyHelperModule.Settings?.HotReloadShowUI == true)
            {
                HotReloadUI.Show($"HOT RELOAD: {reloadedCount} types updated", Color.LimeGreen);
            }
            
            if (MaggyHelperModule.Settings?.HotReloadSound == true)
            {
                try { Audio.Play("event:/ui/main/button_select"); } catch {}
            }
        }
    }
}
