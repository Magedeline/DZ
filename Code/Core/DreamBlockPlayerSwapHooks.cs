using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using CopyAbilityType = Celeste.Entities.Bosses.CopyAbilityType;
using KirbyModeClass = Celeste.Extensions.KirbyMode;

namespace Celeste
{
    /// <summary>
    /// When the player (Madeline or Kirby) enters a dream block the two characters
    /// swap roles:  Madeline → Kirby mode ON,  Kirby mode ON → Madeline.
    /// The original state is stored per-player via DynamicData and restored on exit.
    ///
    /// Hooks used:
    ///   Manual Hook on Player.DreamDashBegin  — fires when the player enters a dream block
    ///   On.Celeste.DreamBlock.OnPlayerExit    — fires when the player exits a dream block
    /// </summary>
    internal static class DreamBlockPlayerSwapHooks
    {
        // DynamicData key used to store the pre-entry sprite mode on the player instance
        private const string PreEnterSpriteModeKey  = "KIRBY_CELESTE_DreamSwap_OrigSpriteMode";
        private const string PreEnterKirbyActiveKey = "KIRBY_CELESTE_DreamSwap_OrigKirbyActive";

        // Manual hook reference — must be kept alive to avoid GC disposal
        // Manual hook reference — must be kept alive to avoid GC disposal
        private static Hook dreamDashBeginHook;

        // Guard flag to prevent double loading
        private static bool isLoaded = false;

        // Delegate matching the private Player.DreamDashBegin signature
        private delegate void orig_DreamDashBegin(global::Celeste.Player self);

        internal static void Load()
        {
            // Prevent loading twice
            if (isLoaded)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE",
                    "[DreamBlockPlayerSwap] Load() called while already loaded — skipping");
                return;
            }

            // DreamDashBegin is private — hook it manually via reflection
            try
            {
                MethodInfo dreamDashBegin = typeof(global::Celeste.Player).GetMethod(
                    "DreamDashBegin",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (dreamDashBegin != null)
                {
                    dreamDashBeginHook = new Hook(
                        dreamDashBegin,
                        typeof(DreamBlockPlayerSwapHooks).GetMethod(
                            nameof(Hook_Player_DreamDashBegin),
                            BindingFlags.Static | BindingFlags.NonPublic));

                    Logger.Log(LogLevel.Info, "KIRBY_CELESTE",
                        "[DreamBlockPlayerSwap] Manual hook on Player.DreamDashBegin registered");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "KIRBY_CELESTE",
                        "[DreamBlockPlayerSwap] Player.DreamDashBegin not found — enter swap skipped");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE",
                    $"[DreamBlockPlayerSwap] Failed to hook DreamDashBegin: {ex.Message}");
            }

            On.Celeste.DreamBlock.OnPlayerExit += OnDreamBlockExit;
            isLoaded = true;
            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "[DreamBlockPlayerSwap] Hooks loaded");
        }

        internal static void Unload()
        {
            if (!isLoaded)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE",
                    "[DreamBlockPlayerSwap] Unload() called while not loaded — skipping");
                return;
            }

            dreamDashBeginHook?.Dispose();
            dreamDashBeginHook = null;

            On.Celeste.DreamBlock.OnPlayerExit -= OnDreamBlockExit;
            isLoaded = false;
            Logger.Log(LogLevel.Info, "KIRBY_CELESTE", "[DreamBlockPlayerSwap] Hooks unloaded");
        }

        // ── Enter — hooked via Player.DreamDashBegin ──────────────────────────

        private static void Hook_Player_DreamDashBegin(orig_DreamDashBegin orig,
            global::Celeste.Player self)
        {
            orig(self);

            try
            {
                var session = global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModule.Session;
                if (session == null) return;

                var dyn = DynamicData.For(self);

                // Save current state so OnPlayerExit can restore it
                dyn.Set(PreEnterSpriteModeKey,   self.Sprite.Mode);
                dyn.Set(PreEnterKirbyActiveKey,  session.IsKirbyModeActive);

                if (session.IsKirbyModeActive)
                {
                    SwapToMadeline(self, session);
                    Logger.Log(LogLevel.Verbose, "KIRBY_CELESTE",
                        "[DreamBlockPlayerSwap] Enter: Kirby → Madeline swap");
                }
                else
                {
                    SwapToKirby(self, session);
                    Logger.Log(LogLevel.Verbose, "KIRBY_CELESTE",
                        "[DreamBlockPlayerSwap] Enter: Madeline → Kirby swap");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE",
                    $"[DreamBlockPlayerSwap] Error in DreamDashBegin hook: {ex.Message}");
            }
        }

        // ── Exit ──────────────────────────────────────────────────────────────

        private static void OnDreamBlockExit(On.Celeste.DreamBlock.orig_OnPlayerExit orig,
            DreamBlock self, global::Celeste.Player player)
        {
            orig(self, player);

            try
            {
                var session = global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModule.Session;
                if (session == null || player == null) return;

                var dyn = DynamicData.For(player);

                if (dyn.TryGet(PreEnterKirbyActiveKey, out bool origKirbyActive))
                {
                    // Restore to whatever the player was before entering
                    if (origKirbyActive)
                        SwapToKirby(player, session);
                    else
                        SwapToMadeline(player, session);

                    dyn.Set(PreEnterSpriteModeKey,  (object)null);
                    dyn.Set(PreEnterKirbyActiveKey, (object)null);

                    Logger.Log(LogLevel.Verbose, "KIRBY_CELESTE",
                        $"[DreamBlockPlayerSwap] Exit: restored to {(origKirbyActive ? "Kirby" : "Madeline")}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "KIRBY_CELESTE",
                    $"[DreamBlockPlayerSwap] Error in OnPlayerExit: {ex.Message}");
            }
        }

        // ── Swap helpers ──────────────────────────────────────────────────────

        private static void SwapToKirby(global::Celeste.Player player, global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSession session)
        {
            session.IsKirbyModeActive = true;

            // MadelineAsBadeline is the closest built-in stand-in for Kirby's dark colouring
            player.ResetSprite(PlayerSpriteMode.MadelineAsBadeline);
            player.Hair.Color = Player.NormalBadelineHairColor;

            // Notify the tracked KirbyMode entity
            var kirbyEntity = player.Scene?.Tracker?.GetEntity<KirbyModeClass>();
            if (kirbyEntity != null)
                kirbyEntity.IsActive = true;

            EmitSwapParticles(player, Color.HotPink);
        }

        private static void SwapToMadeline(global::Celeste.Player player, global::Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSession session)
        {
            session.IsKirbyModeActive = false;
            session.CurrentCopyAbility = CopyAbilityType.None;

            player.ResetSprite(PlayerSpriteMode.Madeline);
            player.Hair.Color = Player.NormalHairColor;

            // Notify the tracked KirbyMode entity
            var kirbyEntity = player.Scene?.Tracker?.GetEntity<KirbyModeClass>();
            if (kirbyEntity != null)
            {
                kirbyEntity.IsActive = false;
                kirbyEntity.CurrentPower = KirbyModeClass.KirbyPowerState.None;
            }

            EmitSwapParticles(player, Color.CornflowerBlue);
        }

        private static void EmitSwapParticles(global::Celeste.Player player, Color color)
        {
            var level = player.SceneAs<Level>();
            if (level == null) return;

            for (int i = 0; i < 12; i++)
            {
                float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                level.Particles.Emit(
                    ParticleTypes.Dust,
                    player.Center + Calc.AngleToVector(angle, 5f),
                    color,
                    angle);
            }
        }
    }
}
