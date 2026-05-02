using System;
using System.Reflection;
using Celeste.Entities;
using Celeste.Extensions;
using Celeste;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

using CopyAbilityType = Celeste.Entities.Bosses.CopyAbilityType;

namespace Celeste
{
    /// <summary>
    /// Demonstrates advanced MonoMod hooking techniques:
    ///   1. IL Hooks  — Surgically modify individual instructions inside a method
    ///   2. Manual Hook class — Hook private/internal methods that On.* can't reach
    ///
    /// These complement the On.* hooks already used throughout the mod.
    /// </summary>
    public static class MonoModHooks
    {
        // ── Manual Hook references (must be stored so they aren't GC'd) ──────────
        private static Hook dashBeginHook;
        private static Hook wallJumpHook;
        private static On.Celeste.Level.hook_LoadLevel levelLoadLevelHook;

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Call from <see cref="MaggyHelperModule.Load"/> to register all
        /// advanced MonoMod hooks.
        /// </summary>
        public static void Load()
        {
            // ─── 2. Manual Hook (private method) ─────────────────────────
            // Player.DashBegin is private — On.* hooks can't reach it.
            // We use MonoMod's Hook class + reflection to intercept it anyway.
            try
            {
                MethodInfo dashBegin = typeof(Player).GetMethod(
                    "DashBegin",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (dashBegin != null)
                {
                    dashBeginHook = new Hook(
                        dashBegin,
                        typeof(MonoModHooks).GetMethod(
                            nameof(Hook_Player_DashBegin),
                            BindingFlags.Static | BindingFlags.NonPublic));

                    Logger.Log(LogLevel.Info, "MaggyHelper",
                        "[MonoModHooks] Manual Hook on Player.DashBegin registered");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        "[MonoModHooks] Player.DashBegin not found — skipping manual hook");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[MonoModHooks] Failed to hook Player.DashBegin: {ex.Message}");
            }

            // ─── 3. Manual Hook on WallJump (another private method) ─────
            // Modifies wall-jump behavior when Kirby has a copy ability active.
            try
            {
                MethodInfo wallJump = typeof(Player).GetMethod(
                    "WallJump",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (wallJump != null)
                {
                    wallJumpHook = new Hook(
                        wallJump,
                        typeof(MonoModHooks).GetMethod(
                            nameof(Hook_Player_WallJump),
                            BindingFlags.Static | BindingFlags.NonPublic));

                    Logger.Log(LogLevel.Info, "MaggyHelper",
                        "[MonoModHooks] Manual Hook on Player.WallJump registered");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "MaggyHelper",
                        "[MonoModHooks] Player.WallJump not found — skipping manual hook");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[MonoModHooks] Failed to hook Player.WallJump: {ex.Message}");
            }

            // ─── 3. OuiMapList null-MapData guard ───────────────────────────
            MapListExt.Load();

            // ─── 4. Entity Name Remapping (player entity ID normalization) ──────
            // Hook Level.LoadLevel to remap player entity IDs from binary maps
            if (levelLoadLevelHook == null)
            {
                levelLoadLevelHook = new On.Celeste.Level.hook_LoadLevel(Hook_Level_LoadLevel);
                On.Celeste.Level.LoadLevel += levelLoadLevelHook;
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                "[MonoModHooks] All advanced MonoMod hooks loaded");
        }

        /// <summary>
        /// Call from <see cref="MaggyHelperModule.Unload"/> to clean up.
        /// </summary>
        public static void Unload()
        {
            // Remove IL hook
            // IL.Celeste.Player.NormalUpdate -= IL_Player_NormalUpdate;

            // Dispose manual hooks (this un-detours the methods)
            dashBeginHook?.Dispose();
            dashBeginHook = null;

            wallJumpHook?.Dispose();
            wallJumpHook = null;

            // Remove entity name remapping hook
            if (levelLoadLevelHook != null)
                On.Celeste.Level.LoadLevel -= levelLoadLevelHook;
            levelLoadLevelHook = null;

            // Remove OuiMapList guard
            MapListExt.Unload();

            Logger.Log(LogLevel.Info, "MaggyHelper",
                "[MonoModHooks] All advanced MonoMod hooks unloaded");
        }


        // =====================================================================
        //  1.  IL HOOK — Modify Player.NormalUpdate jump constant
        // =====================================================================
        //
        //  HOW IT WORKS:
        //  Celeste's Player.NormalUpdate loads a float constant for jump speed
        //  (–105f). Our IL hook scans for that value and replaces it with a
        //  softer value (–80f) when Kirby float mode is active. This is
        //  *far* more surgical than an On.* hook — we don't need to duplicate
        //  the entire NormalUpdate method, we just tweak one number.
        //
        // =====================================================================

        /* Removed IL Hook as it was incorrect
        private static void IL_Player_NormalUpdate(ILContext il)
        {
            // ...
        }

        private static float ModifyJumpSpeed(float originalSpeed)
        {
            // ...
            return originalSpeed;
        }
        */


        // =====================================================================
        //  2.  MANUAL HOOK — Player.DashBegin (private method)
        // =====================================================================
        //
        //  HOW IT WORKS:
        //  On.Celeste.Player.DashBegin doesn't exist because the method is
        //  private.  We use MonoMod's Hook class with reflection to intercept
        //  it anyway.  The delegate signature must match:
        //      orig(Player self)  →  our method(orig, Player self)
        //
        //  We use this to spawn copy-ability particle effects when Kirby dashes.
        //
        // =====================================================================

        // Delegate matching the original method signature
        private delegate void orig_DashBegin(Player self);

        private static void Hook_Player_DashBegin(orig_DashBegin orig, Player self)
        {
            // Call original first (let the dash start normally)
            orig(self);

            try
            {
                var settings = MaggyHelperModule.Settings;
                var session  = MaggyHelperModule.Session;

                if (settings?.KirbyPlayerEnabled != true ||
                    session == null ||
                    session.CurrentCopyAbility == CopyAbilityType.None)
                    return;

                // Spawn themed particles based on Kirby's current copy ability
                Color particleColor = GetAbilityColor(session.CurrentCopyAbility);

                Level level = self.SceneAs<Level>();
                if (level == null) return;

                // Burst of colored particles at the player position
                for (int i = 0; i < 8; i++)
                {
                    float angle = Calc.Random.NextFloat((float)Math.PI * 2f);
                    float speed = 40f + Calc.Random.NextFloat(60f);

                    level.Particles.Emit(
                        ParticleTypes.Dust,
                        self.Center + Calc.AngleToVector(angle, 4f),
                        particleColor,
                        angle);
                }

                if (settings.DebugMode)
                {
                    Logger.Log(LogLevel.Verbose, "MaggyHelper",
                        $"[Hook] Dash particles spawned for ability: {session.CurrentCopyAbility}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Hook] Error in DashBegin hook: {ex.Message}");
            }
        }


        // =====================================================================
        //  3.  MANUAL HOOK — Player.WallJump (private method)
        // =====================================================================
        //
        //  Adjusts wall-jump force when Kirby has certain copy abilities.
        //  For example, Wing gives extra horizontal boost; Stone reduces it.
        //
        // =====================================================================

        private delegate void orig_WallJump(Player self, int dir);

        private static void Hook_Player_WallJump(orig_WallJump orig, Player self, int dir)
        {
            // Call the original wall jump
            orig(self, dir);

            try
            {
                var settings = MaggyHelperModule.Settings;
                var session  = MaggyHelperModule.Session;

                if (settings?.KirbyPlayerEnabled != true ||
                    session == null ||
                    session.CurrentCopyAbility == CopyAbilityType.None)
                    return;

                switch (session.CurrentCopyAbility)
                {
                    case CopyAbilityType.Wing:
                        // Wing ability: extra horizontal boost on wall jump
                        self.Speed.X *= 1.25f;
                        if (settings.DebugMode)
                            Logger.Log(LogLevel.Verbose, "MaggyHelper",
                                "[Hook] Wing wall-jump boost applied");
                        break;

                    case CopyAbilityType.Stone:
                        // Stone ability: heavier, slower wall jump
                        self.Speed.X *= 0.7f;
                        self.Speed.Y *= 0.85f;
                        if (settings.DebugMode)
                            Logger.Log(LogLevel.Verbose, "MaggyHelper",
                                "[Hook] Stone wall-jump weight applied");
                        break;

                    case CopyAbilityType.Wheel:
                        // Wheel ability: much faster horizontal wall jump
                        self.Speed.X *= 1.5f;
                        if (settings.DebugMode)
                            Logger.Log(LogLevel.Verbose, "MaggyHelper",
                                "[Hook] Wheel wall-jump speed boost applied");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[Hook] Error in WallJump hook: {ex.Message}");
            }
        }


        // =====================================================================
        //  4.  ENTITY NAME REMAPPING — Legacy Kirby player entity normalization
        // =====================================================================
        //
        //  HOW IT WORKS:
        //  Older map files can contain custom player entities from the now-
        //  retired second-player architecture. This hook rewrites only those
        //  legacy IDs to the room-local KirbyPlayerSpawner before the room loads.
        //
        //  This preserves backwards compatibility while keeping the real
        //  global::Celeste.Player authoritative in-game.
        //
        // =====================================================================

        private static void Hook_Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self,
            global::Celeste.Player.IntroTypes playerIntro, bool isFromLoader)
        {
            try
            {
                // Scan only the current level being loaded for player entities
                if (self.Session?.LevelData?.Entities is List<EntityData> entities)
                {
                    foreach (var entityData in entities)
                    {
                        if (entityData?.Name != null)
                        {
                            string lowerName = entityData.Name.ToLowerInvariant();

                            if (lowerName == "maggyhelper/player" ||
                                lowerName == "maggyhelper/kirbyplayer" ||
                                lowerName == "maggyhelperp/layer")
                            {
                                Logger.Log(LogLevel.Info, "MaggyHelper",
                                    $"[EntityRemapper] Remapping legacy entity '{entityData.Name}' → 'MaggyHelper/KirbyPlayerSpawner'");
                                entityData.Name = "MaggyHelper/KirbyPlayerSpawner";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[EntityRemapper] Error in entity remapping hook: {ex.Message}\n{ex.StackTrace}");
            }

            // Call the original LoadLevel
            orig(self, playerIntro, isFromLoader);

            try
            {
                self.Tracker.GetEntity<global::Celeste.Player>()?.RestorePersistentState();
                KirbyPlayerSpawner.EnsureRoomState(self);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    $"[PlayerState] Error restoring persistent player state: {ex.Message}\n{ex.StackTrace}");
            }
        }


        // =====================================================================
        //  Helpers
        // =====================================================================

        /// <summary>
        /// Maps a copy ability to its signature particle color.
        /// </summary>
        private static Color GetAbilityColor(CopyAbilityType ability)
        {
            return ability switch
            {
                CopyAbilityType.Fire    => Color.OrangeRed,
                CopyAbilityType.Ice     => Color.LightCyan,
                CopyAbilityType.Spark   => Color.Yellow,
                CopyAbilityType.Sword   => Color.Silver,
                CopyAbilityType.Cutter  => Color.Gold,
                CopyAbilityType.Beam    => Color.Cyan,
                CopyAbilityType.Stone   => Color.SaddleBrown,
                CopyAbilityType.Needle  => Color.White,
                CopyAbilityType.Parasol => Color.LightPink,
                CopyAbilityType.Wheel   => Color.Red,
                CopyAbilityType.Bomb    => Color.DarkGray,
                CopyAbilityType.Fighter => Color.Orange,
                CopyAbilityType.Suplex  => Color.DarkRed,
                CopyAbilityType.Ninja   => Color.Purple,
                CopyAbilityType.Mirror  => Color.LightGoldenrodYellow,
                CopyAbilityType.Hammer  => Color.Brown,
                CopyAbilityType.Wing    => Color.LightSkyBlue,
                CopyAbilityType.UFO     => Color.LimeGreen,
                CopyAbilityType.Sleep   => Color.LavenderBlush,
                _                       => Color.White,
            };
        }
    }
}
