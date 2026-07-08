using System;
using System.Collections;
using Celeste.Mod.DZ;
using Celeste.Entities;
using Monocle;

namespace Celeste.Mod.DZ
{
    /// <summary>
    /// Hooks into <see cref="OuiChapterPanel"/> to intercept the "Play" action
    /// and show the <see cref="OuiPlayerSelect"/> screen before the level begins.
    ///
    /// Behaviour:
    /// <list type="bullet">
    ///   <item>
    ///     If <see cref="PlayerSelectionManager.AutoSelectEnabled"/> is true the
    ///     recommended character (if any) is applied silently and the level starts
    ///     immediately — no extra screen is shown.
    ///   </item>
    ///   <item>
    ///     Otherwise <see cref="OuiPlayerSelect"/> is pushed onto the Overworld
    ///     stack.  Confirming it applies the choice and re-fires the start action;
    ///     cancelling returns to the chapter panel.
    ///   </item>
    ///   <item>
    ///     Only DZ maps (checked via <see cref="AreaModeExtender.IsOurMap"/>) show
    ///     the picker.  Vanilla chapters fall through unmodified.
    ///   </item>
    /// </list>
    /// </summary>
    public static class PlayerSelectHooks
    {
        private static bool _loaded;

        // Tracks whether we already injected the player-select step for a given
        // chapter-panel start so we don't double-intercept.
        private static bool _intercepting;

        public static void Load()
        {
            if (_loaded)
                return;
            _loaded = true;

            On.Celeste.OuiChapterPanel.Start += OnChapterPanelStart;

            Logger.Log(LogLevel.Info, "DZ", "[PlayerSelectHooks] Loaded");
        }

        public static void Unload()
        {
            if (!_loaded)
                return;
            _loaded = false;

            On.Celeste.OuiChapterPanel.Start -= OnChapterPanelStart;

            Logger.Log(LogLevel.Info, "DZ", "[PlayerSelectHooks] Unloaded");
        }

        // ── Hook ─────────────────────────────────────────────────────────────

        private static void OnChapterPanelStart(
            On.Celeste.OuiChapterPanel.orig_Start orig,
            OuiChapterPanel self,
            string checkpoint)
        {
            // Only intercept DZ maps
            AreaData area = AreaData.Get(self.Area);
            if (!AreaModeExtender.IsOurMap(area) || _intercepting)
            {
                orig(self, checkpoint);
                return;
            }

            // If auto-select is enabled apply recommended silently, then start
            if (PlayerSelectionManager.AutoSelectEnabled)
            {
                PlayerSelectionManager.ApplyRecommendedPlayerIfAuto();
                orig(self, checkpoint);
                return;
            }

            // Show the player-select screen via a coroutine added to the panel.
            // We must avoid blocking the main thread, so we launch a Coroutine
            // on the OuiChapterPanel itself and pass orig/checkpoint into it.
            self.Add(new Coroutine(PlayerSelectRoutine(self, orig, checkpoint)));
        }

        private static IEnumerator PlayerSelectRoutine(
            OuiChapterPanel self,
            On.Celeste.OuiChapterPanel.orig_Start orig,
            string checkpoint)
        {
            bool confirmed = false;
            bool callbackFired = false;

            // Properly transition to the player select screen using Overworld.Goto
            var selectScreen = self.Overworld.Goto<OuiPlayerSelect>();
            selectScreen.OnConfirm = () => { confirmed = true; callbackFired = true; };
            selectScreen.OnCancel  = () => { confirmed = false; callbackFired = true; };

            // Wait until the player makes a choice (callback fires)
            while (!callbackFired)
                yield return null;

            if (!confirmed)
            {
                // User cancelled — navigate back to the chapter panel
                // The Overworld will call Leave() on the player select screen
                self.Overworld.Goto<OuiChapterPanel>();
                yield break;
            }

            // Wait a frame to allow the Overworld to process the transition
            yield return null;

            // Start the level now that the player has confirmed their choice
            _intercepting = true;
            try
            {
                orig(self, checkpoint);
            }
            finally
            {
                _intercepting = false;
            }
        }

    }
}
