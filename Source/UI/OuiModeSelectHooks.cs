using System;
using System.Collections;
using Celeste;
using Monocle;

namespace Celeste.Mod.DZ
{
    /// <summary>
    /// Hooks into the vanilla file-select slot to add two DZ-specific mode
    /// buttons alongside the vanilla Assist / Variant buttons:
    ///
    ///   • "Gentle Breeze Mode"  -> opens <see cref="OuiGentleBreezeMode"/>
    ///     (beginner-friendly Kirby assist bundle).
    ///   • "Last Endemy Mode"    -> opens <see cref="OuiLastEndemyMode"/>
    ///     (CelesteNet online-multiplayer onboarding &amp; rules agreement).
    ///
    /// Both buttons are inserted right after the vanilla Assist button so they
    /// sit naturally in the existing slot menu, and their labels flip between
    /// ON/OFF based on the current <see cref="DZModuleSettings"/> state.
    /// </summary>
    public static class OuiModeSelectHooks
    {
        private static bool _hooked;

        public static void Load()
        {
            if (_hooked)
                return;
            _hooked = true;

            On.Celeste.OuiFileSelectSlot.CreateButtons += OnCreateButtons;
            Logger.Log(LogLevel.Info, "DZ", "[OuiModeSelectHooks] Loaded");
        }

        public static void Unload()
        {
            if (!_hooked)
                return;
            _hooked = false;

            On.Celeste.OuiFileSelectSlot.CreateButtons -= OnCreateButtons;
            Logger.Log(LogLevel.Info, "DZ", "[OuiModeSelectHooks] Unloaded");
        }

        private static void OnCreateButtons(
            On.Celeste.OuiFileSelectSlot.orig_CreateButtons orig,
            OuiFileSelectSlot self)
        {
            orig(self);

            // Don't add mode buttons to corrupted save slots.
            if (self.Corrupted)
                return;

            var settings = DZModule.Settings;
            bool gentleBreezeOn = settings != null && settings.GentleBreezeMode;
            bool lastEndemyOn = settings != null && settings.LastEndemyMode;

            var buttons = self.buttons;
            if (buttons == null)
                return;

            var gentleButton = new OuiFileSelectSlot.Button
            {
                Label = Dialog.Clean("DZ_FILE_GENTLEBREEZE_" + (gentleBreezeOn ? "ON" : "OFF")),
                Scale = 0.7f,
                Action = () => OpenGentleBreeze(self)
            };

            var lastEndemyButton = new OuiFileSelectSlot.Button
            {
                Label = Dialog.Clean("DZ_FILE_LASTENDEMY_" + (lastEndemyOn ? "ON" : "OFF")),
                Scale = 0.7f,
                Action = () => OpenLastEndemy(self)
            };

            // Insert right after the vanilla Assist button so the mode buttons
            // group together. The assist button is matched by its Action
            // delegate (the slot's own OnAssistSelected method). Falls back to
            // appending at the end if the assist button can't be located.
            int insertAt = buttons.Count;
            for (int i = 0; i < buttons.Count; i++)
            {
                var action = buttons[i].Action;
                if (action != null
                    && ReferenceEquals(action.Target, self)
                    && action.Method.Name == "OnAssistSelected")
                {
                    insertAt = i + 1;
                    break;
                }
            }

            buttons.Insert(insertAt, gentleButton);
            buttons.Insert(insertAt + 1, lastEndemyButton);
        }

        private static void OpenGentleBreeze(OuiFileSelectSlot self)
        {
            if (self.fileSelect == null)
                return;
            // Reuse the vanilla "Assisting" flag so the slot fades its selection
            // visuals while the mode screen is open (reset on re-Select).
            self.Assisting = true;
            self.fileSelect.Overworld.Goto<OuiGentleBreezeMode>().FileSlot = self;
            Audio.Play("event:/ui/main/assist_button_info");
        }

        private static void OpenLastEndemy(OuiFileSelectSlot self)
        {
            if (self.fileSelect == null)
                return;
            self.Assisting = true;
            self.fileSelect.Overworld.Goto<OuiLastEndemyMode>().FileSlot = self;
            Audio.Play("event:/ui/main/assist_button_info");
        }
    }
}
