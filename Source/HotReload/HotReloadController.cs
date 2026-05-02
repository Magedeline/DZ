using System;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.MaggyHelper.HotReload
{
    [Tracked]
    public class HotReloadController : Entity
    {
        public HotReloadController()
        {
            Tag = Tags.Global | Tags.FrozenUpdate | Tags.PauseUpdate;
        }

        public override void Update()
        {
            base.Update();

            var settings = MaggyHelperModule.Settings;
            if (settings == null || !settings.HotReloadEnabled) return;

            if (settings.HotReloadToggle?.Pressed == true)
            {
                settings.HotReloadEnabled = !settings.HotReloadEnabled;
                HotReloadUI.Show(settings.HotReloadEnabled ? "Hot Reload Enabled" : "Hot Reload Disabled", 
                    settings.HotReloadEnabled ? Microsoft.Xna.Framework.Color.LimeGreen : Microsoft.Xna.Framework.Color.Red);
            }

            if (settings.HotReloadManual?.Pressed == true)
            {
                // In a real scenario, this might trigger a file system watcher or signal dotnet watch.
                // Here we just trigger the test classes to show it's working.
                HotReloadUI.Show("Manual Reload Triggered", Microsoft.Xna.Framework.Color.Yellow);
                HotReloadHandler.UpdateApplication(new Type[] { 
                    typeof(ModHotReloadTest), 
                    typeof(global::Celeste.HotReload.GameHotReloadTest) 
                });
            }

            if (settings.HotReloadUI?.Pressed == true)
            {
                settings.HotReloadShowUI = !settings.HotReloadShowUI;
                HotReloadUI.Show(settings.HotReloadShowUI ? "Reload UI Enabled" : "Reload UI Disabled", Microsoft.Xna.Framework.Color.White);
            }
        }
    }
}
