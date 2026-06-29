using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        private bool _lastF9;
        private bool _lastF11;
        private bool _lastF12;

        public override void Update()
        {
            base.Update();

            var settings = DZModule.Settings;
            bool isEnabled = settings != null && settings.HotReloadEnabled;

            // Read keyboard state directly
            bool curF9 = MInput.Keyboard.Check(Keys.F9);
            bool curF11 = MInput.Keyboard.Check(Keys.F11);
            bool curF12 = MInput.Keyboard.Check(Keys.F12);

            // F9: Toggle hot reload on/off, or resume if frozen
            if (curF9 && !_lastF9)
            {
                // If frozen due to error, resume first
                if (LiveWatchPatcher.IsFrozen)
                {
                    LiveWatchPatcher.Resume();
                }
                else if (settings != null)
                {
                    settings.HotReloadEnabled = !settings.HotReloadEnabled;
                    HotReloadUI.Show(settings.HotReloadEnabled ? "Hot Reload Enabled" : "Hot Reload Disabled", 
                        settings.HotReloadEnabled ? Color.LimeGreen : Color.Red);
                }
            }

            // F11 and F12: Only work when hot reload is enabled
            if (isEnabled)
            {
                // F11: Force-reload the DZ.dll assembly at runtime (no restart needed)
                if (curF11 && !_lastF11)
                {
                    HotReloadUI.Show("Force Reload Triggered", Color.Yellow);
                    DllHotReloader.ForceReload();
                }

                // F12: Toggle UI
                if (curF12 && !_lastF12)
                {
                    if (settings != null)
                    {
                        settings.HotReloadShowUI = !settings.HotReloadShowUI;
                        HotReloadUI.Show(settings.HotReloadShowUI ? "Reload UI Enabled" : "Reload UI Disabled", Color.White);
                    }
                }
            }

            _lastF9 = curF9;
            _lastF11 = curF11;
            _lastF12 = curF12;
            _wasEnabled = isEnabled;
        }
    }
}
