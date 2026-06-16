using System;
using Monocle;

namespace Celeste.Mod.DZ.HotReload
{
    [Tracked]
    public class HotReloadController : Entity
    {
        public HotReloadController()
        {
            Tag = Tags.Global | Tags.FrozenUpdate | Tags.PauseUpdate;
        }

        private bool _wasEnabled;

        public override void Update()
        {
            base.Update();

            var settings = DZModule.Settings;
            bool isEnabled = settings != null && settings.HotReloadEnabled;

            // Only process input when hot reload is enabled or was just toggled
            if (!isEnabled && !_wasEnabled)
                return;

            _wasEnabled = isEnabled;

            if (!isEnabled)
                return;

            if (settings.HotReloadToggle?.Pressed == true)
            {
                settings.HotReloadEnabled = false;
                HotReloadUI.Show("Hot Reload Disabled", Microsoft.Xna.Framework.Color.Red);
            }

            if (settings.HotReloadManual?.Pressed == true)
            {
                HotReloadUI.Show("Manual Reload Triggered", Microsoft.Xna.Framework.Color.Yellow);
                global::Celeste.HotReload.HotReloadHandler.NotifyEverestReload();
            }

            if (settings.HotReloadUIBinding?.Pressed == true)
            {
                settings.HotReloadShowUI = !settings.HotReloadShowUI;
                HotReloadUI.Show(settings.HotReloadShowUI ? "Reload UI Enabled" : "Reload UI Disabled", Microsoft.Xna.Framework.Color.White);
            }
        }
    }
}
