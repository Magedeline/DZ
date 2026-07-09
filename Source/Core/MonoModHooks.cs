using System;
using System.Reflection;
using Celeste.Entities;
using Celeste.Extensions;
using Celeste;
using Celeste.Editor;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

using CopyAbilityType = Celeste.Entities.Bosses.CopyAbilityType;

namespace DZ
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

        // ── IL Hook reference for WallJump (worked example: On/Hook → IL) ──────
        // Stored in a static field so the ILHook isn't GC'd mid-game.
        // ILHook is the IL equivalent of Hook — used when IL.* delegate events
        // don't exist (e.g. for private methods like Player.WallJump).
        private static ILHook wallJumpILHook;

        private static On.Celeste.Level.hook_LoadLevel levelLoadLevelHook;
        private static Hook levelTemplateCctorHook;
        private static On.Celeste.Level.hook_Update levelUpdateHook;
        private static Hook flingBirdIntroCtor;

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Call from <see cref="DZModule.Load"/> to register all
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

                    Logger.Log(LogLevel.Info, "DZ",
                        "[MonoModHooks] Manual Hook on Player.DashBegin registered");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "DZ",
                        "[MonoModHooks] Player.DashBegin not found — skipping manual hook");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[MonoModHooks] Failed to hook Player.DashBegin: {ex.Message}");
            }

            // ─── 3. IL Hook on WallJump (private method — worked example) ─────
            // Previously this used a manual Hook (On-style wrapper). Now we use
            // an ILHook to surgically inject the speed multiplier right before
            // each 'ret' instruction inside Player.WallJump, without having to
            // call/replace the entire method.
            //
            // ILHook is the IL equivalent of Hook: because Player.WallJump is
            // private, the generated IL.Celeste.Player.WallJump event doesn't
            // exist, so we must use ILHook + reflection, just like we do with
            // Hook for On.* hooks on private methods.
            try
            {
                MethodInfo wallJump = typeof(Player).GetMethod(
                    "WallJump",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (wallJump != null)
                {
                    wallJumpILHook = new ILHook(wallJump, IL_Player_WallJump);

                    Logger.Log(LogLevel.Info, "DZ",
                        "[MonoModHooks] IL Hook on Player.WallJump registered");
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "DZ",
                        "[MonoModHooks] Player.WallJump not found — skipping IL hook");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[MonoModHooks] Failed to IL-hook Player.WallJump: {ex.Message}");
            }

            // ─── 3. OuiMapList null-MapData guard ───────────────────────────
            MapListExt.Load();

            // ─── 3.5. FlingBirdIntro → EnhancedBirdNPC integration ────────────
            try
            {
                Type flingBirdIntroType = Type.GetType("Celeste.Entities.FlingBirdIntro, Celeste");
                Type entityDataType = Type.GetType("Celeste.Editor.EntityData, Celeste");

                if (flingBirdIntroType != null && entityDataType != null)
                {
                    ConstructorInfo flingBirdCtor = flingBirdIntroType.GetConstructor(
                        BindingFlags.Instance | BindingFlags.Public,
                        null,
                        new Type[] { entityDataType, typeof(Vector2) },
                        null);

                    if (flingBirdCtor != null)
                    {
                        flingBirdIntroCtor = new Hook(
                            flingBirdCtor,
                            typeof(MonoModHooks).GetMethod(
                                nameof(Hook_FlingBirdIntro_Ctor),
                                BindingFlags.Static | BindingFlags.NonPublic));

                        Logger.Log(LogLevel.Info, "DZ",
                            "[MonoModHooks] Hook on FlingBirdIntro constructor registered");
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "DZ",
                            "[MonoModHooks] FlingBirdIntro constructor not found — skipping hook");
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "DZ",
                        "[MonoModHooks] FlingBirdIntro type not found — skipping hook");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[MonoModHooks] Failed to hook FlingBirdIntro: {ex.Message}");
            }

            // ─── 4. Dream-block Madeline ↔ Kirby player swap ────────────────────
            DreamBlockPlayerSwapHooks.Load();

            // ─── 5. Entity Name Remapping (player entity ID normalization) ──────
            // Hook Level.LoadLevel to remap player entity IDs from binary maps
            if (levelLoadLevelHook == null)
            {
                levelLoadLevelHook = new On.Celeste.Level.hook_LoadLevel(Hook_Level_LoadLevel);
                On.Celeste.Level.LoadLevel += levelLoadLevelHook;
            }

            // ─── 5.5. Debug Mode Map Editor Hook ────────────────────────────────
            // Hook Level.Update to handle F10 key press for in-game map editor
            if (levelUpdateHook == null)
            {
                levelUpdateHook = new On.Celeste.Level.hook_Update(Hook_Level_Update);
                On.Celeste.Level.Update += levelUpdateHook;
            }

            // ─── 6. LevelTemplate Color Expansion ───────────────────────────────
            // Hook LevelTemplate static constructor to expand color array from 7 to 24
            try
            {
                Type levelTemplateType = Type.GetType("Celeste.Editor.LevelTemplate, Celeste");
                if (levelTemplateType != null)
                {
                    ConstructorInfo levelTemplateCctor = levelTemplateType.GetConstructor(
                        BindingFlags.Static | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null);

                    if (levelTemplateCctor != null)
                    {
                        levelTemplateCctorHook = new Hook(
                            levelTemplateCctor,
                            typeof(MonoModHooks).GetMethod(
                                nameof(Hook_LevelTemplate_Cctor),
                                BindingFlags.Static | BindingFlags.NonPublic));

                        Logger.Log(LogLevel.Info, "DZ",
                            "[MonoModHooks] Hook on LevelTemplate static constructor registered");
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "DZ",
                            "[MonoModHooks] LevelTemplate static constructor not found — skipping hook");
                    }
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "DZ",
                        "[MonoModHooks] LevelTemplate type not found — skipping hook");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[MonoModHooks] Failed to hook LevelTemplate: {ex.Message}");
            }

            Logger.Log(LogLevel.Info, "DZ",
                "[MonoModHooks] All advanced MonoMod hooks loaded");
        }

        /// <summary>
        /// Call from <see cref="DZModule.Unload"/> to clean up.
        /// </summary>
        public static void Unload()
        {
            // Remove IL hook
            // IL.Celeste.Player.NormalUpdate -= IL_Player_NormalUpdate;

            // Dispose manual hooks (this un-detours the methods)
            dashBeginHook?.Dispose();
            dashBeginHook = null;

            // Dispose the IL hook for WallJump (un-detours the method)
            wallJumpILHook?.Dispose();
            wallJumpILHook = null;

            // Remove entity name remapping hook
            if (levelLoadLevelHook != null)
                On.Celeste.Level.LoadLevel -= levelLoadLevelHook;
            levelLoadLevelHook = null;

            // Remove debug mode map editor hook
            if (levelUpdateHook != null)
                On.Celeste.Level.Update -= levelUpdateHook;
            levelUpdateHook = null;

            // Remove LevelTemplate static constructor hook
            levelTemplateCctorHook?.Dispose();
            levelTemplateCctorHook = null;

            // Remove FlingBirdIntro hook
            flingBirdIntroCtor?.Dispose();
            flingBirdIntroCtor = null;

            // Remove OuiMapList guard
            MapListExt.Unload();

            // Remove dream-block swap hooks
            DreamBlockPlayerSwapHooks.Unload();

            Logger.Log(LogLevel.Info, "DZ",
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
                var settings = global::Celeste.Mod.DZ.DZModule.Settings;
                var session  = global::Celeste.Mod.DZ.DZModule.Session;

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
                    Logger.Log(LogLevel.Verbose, "DZ",
                        $"[Hook] Dash particles spawned for ability: {session.CurrentCopyAbility}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[Hook] Error in DashBegin hook: {ex.Message}");
            }
        }


        // =====================================================================
        //  3.  IL HOOK — Player.WallJump (private method)  [Worked Example]
        // =====================================================================
        //
        //  This is the IL-hook version of the previous manual Hook.  Instead of
        //  wrapping the entire method and calling orig(), we surgically inject
        //  the speed multiplier right before each 'ret' instruction inside the
        //  compiled body of Player.WallJump.
        //
        //  WHY IL HERE?
        //  The old Hook worked fine, but it had to call orig() and THEN modify
        //  Speed — meaning the original WallJump code ran fully, then we
        //  patched the result.  The IL hook lets us modify Speed *in-place*
        //  as the method is about to return, with zero extra method calls on
        //  the hot path and no delegate indirection for the normal (non-Kirby)
        //  case.
        //
        //  HOW IT WORKS — step by step:
        //
        //  1.  We receive an ILContext `il` — the compiled instruction stream
        //      of Player.WallJump, already disassembled for us by MonoMod.
        //
        //  2.  We create an ILCursor to walk through instructions.
        //
        //  3.  Player.WallJump returns void, so at every `ret` instruction the
        //      evaluation stack is empty.  To access `this` (the Player) we
        //      must explicitly load it with `ldarg.0` before calling our
        //      delegate.  This is the key stack-balancing rule from the guide:
        //      every code path must leave the stack as the JIT expects.
        //
        //  4.  Before each `ret`, we emit:
        //        ldarg.0                          // push 'this' (Player)
        //        call Action<Player> delegate     // pop Player, run modifier
        //     The delegate pops Player (consuming ldarg.0) and returns void,
        //     so the stack is empty again — safe to `ret`.
        //
        //  5.  We wrap everything in try/catch because a bad IL edit will
        //      throw InvalidProgramException on every frame and crash the game.
        //
        // =====================================================================

        /* ── Previous manual Hook version (kept for reference) ──────────────
        private delegate void orig_WallJump(Player self, int dir);

        private static void Hook_Player_WallJump(orig_WallJump orig, Player self, int dir)
        {
            orig(self, dir);
            // ... then multiply self.Speed.X / .Y based on copy ability
        }
        ──────────────────────────────────────────────────────────────────── */

        private static void IL_Player_WallJump(ILContext il)
        {
            // ── Step 1: wrap in try/catch — a malformed IL edit crashes hard ──
            try
            {
                // ── Step 2: create a cursor to navigate the instruction stream ──
                ILCursor c = new ILCursor(il);

                // ── Step 3: walk to every `ret` and inject our modifier ────────
                // TryGotoNext returns false when there are no more matches,
                // so this naturally handles methods with 1 or many return points.
                while (c.TryGotoNext(MoveType.Before, instr => instr.MatchRet()))
                {
                    // ── Step 4: load `this` (Player self) onto the stack ────────
                    // WallJump is an instance method, so argument 0 is `this`.
                    // At a `ret` in a void method the stack is empty, so we must
                    // push something for our delegate to consume.
                    c.Emit(OpCodes.Ldarg_0);

                    // ── Step 5: emit a delegate call that pops Player, returns void
                    // EmitDelegate generates a static call to a compiler-generated
                    // stub, so there's no reflection overhead at runtime.
                    c.EmitDelegate<Action<Player>>(ApplyWallJumpSpeedModifier);

                    // ── Step 6: advance past this `ret` to find the next one ────
                    // TryGotoNext stops *before* the ret; without advancing we'd
                    // loop forever on the same instruction.
                    c.Index++;
                }

                Logger.Log(LogLevel.Info, "DZ",
                    "[MonoModHooks] IL_Player_WallJump: injected speed modifier before each ret");
            }
            catch (Exception ex)
            {
                // If IL editing fails, log loudly — the method will run unmodified.
                Logger.Log(LogLevel.Error, "DZ",
                    $"[MonoModHooks] IL_Player_WallJump FAILED — method left unhooked: {ex}");
            }
        }

        /// <summary>
        /// The delegate injected by <see cref="IL_Player_WallJump"/>.
        /// Receives the Player instance (`this` from the original method)
        /// and applies the copy-ability speed multiplier in-place.
        ///
        /// This is the exact same logic that used to live in the body of the
        /// old Hook_Player_WallJump, just factored out so EmitDelegate can call it.
        /// </summary>
        private static void ApplyWallJumpSpeedModifier(Player self)
        {
            try
            {
                var settings = global::Celeste.Mod.DZ.DZModule.Settings;
                var session  = global::Celeste.Mod.DZ.DZModule.Session;

                // Non-Kirby or no ability → no modification, zero overhead path
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
                            Logger.Log(LogLevel.Verbose, "DZ",
                                "[IL] Wing wall-jump boost applied");
                        break;

                    case CopyAbilityType.Stone:
                        // Stone ability: heavier, slower wall jump
                        self.Speed.X *= 0.7f;
                        self.Speed.Y *= 0.85f;
                        if (settings.DebugMode)
                            Logger.Log(LogLevel.Verbose, "DZ",
                                "[IL] Stone wall-jump weight applied");
                        break;

                    case CopyAbilityType.Wheel:
                        // Wheel ability: much faster horizontal wall jump
                        self.Speed.X *= 1.5f;
                        if (settings.DebugMode)
                            Logger.Log(LogLevel.Verbose, "DZ",
                                "[IL] Wheel wall-jump speed boost applied");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[IL] Error in WallJump speed modifier: {ex.Message}");
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

                            if (lowerName == "DZ/player" ||
                                lowerName == "DZ/kirbyplayer" ||
                                lowerName == "DZp/layer")
                            {
                                Logger.Log(LogLevel.Info, "DZ",
                                    $"[EntityRemapper] Remapping legacy entity '{entityData.Name}' → 'DZ/KirbyPlayerSpawner'");
                                entityData.Name = "DZ/KirbyPlayerSpawner";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[EntityRemapper] Error in entity remapping hook: {ex.Message}\n{ex.StackTrace}");
            }

            // Call the original LoadLevel
            orig(self, playerIntro, isFromLoader);

            try
            {
                self.Tracker.GetEntity<global::Celeste.Player>()?.RestorePersistentState();
                KirbyPlayerSpawner.EnsureRoomState(self);

                // InGameMapEditor has been removed - reserved for future use
                // if (self.Entities.FindFirst<InGameMapEditor>() == null)
                // {
                //     self.Add(new InGameMapEditor());
                //     Logger.Log(LogLevel.Info, "DZ", "[InGameMapEditor] Added to level");
                // }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[PlayerState] Error restoring persistent player state: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void Hook_Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            // Call the original Update first
            orig(self);

            // F10 was for in-game map editor, which has been removed
            // var settings = DZModule.Settings;
            // if (settings?.InGameMapEditor?.Pressed == true)
            // {
            //     // InGameMapEditor has been removed - reserved for future use
            // }
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


        // =====================================================================
        //  6.  LEVEL TEMPLATE COLOR EXPANSION — 24 colors instead of 7
        // =====================================================================
        //
        //  HOW IT WORKS:
        //  Hooks the LevelTemplate static constructor to expand the fgTilesColor
        //  array from 7 colors to 24 colors, allowing for more room color options.
        //
        // =====================================================================

        private delegate void orig_LevelTemplateCctor();

        private static void Hook_LevelTemplate_Cctor(orig_LevelTemplateCctor orig)
        {
            // Call original static constructor first
            orig();

            try
            {
                // Get the fgTilesColor field and expand it
                Type levelTemplateType = Type.GetType("Celeste.Editor.LevelTemplate, Celeste");
                if (levelTemplateType != null)
                {
                    var fgTilesColorField = levelTemplateType.GetField("fgTilesColor", 
                        BindingFlags.Static | BindingFlags.NonPublic);
                    
                    if (fgTilesColorField != null)
                    {
                        // Get the original 7-color array
                        var originalColors = fgTilesColorField.GetValue(null) as Color[];
                        if (originalColors != null && originalColors.Length == 7)
                        {
                            // Create expanded 24-color array
                            Color[] expandedColors = new Color[24];
                            Array.Copy(originalColors, expandedColors, 7);
                            
                            // Add additional colors (indices 7-9)
                            expandedColors[7] = Calc.HexToColor("ff8c00");
                            expandedColors[8] = Calc.HexToColor("ffd700");
                            expandedColors[9] = Calc.HexToColor("00ff7f");
                            
                            // Shift + D1-D9 colors (indices 10-18)
                            expandedColors[10] = Calc.HexToColor("ff6347");
                            expandedColors[11] = Calc.HexToColor("ff4500");
                            expandedColors[12] = Calc.HexToColor("dc143c");
                            expandedColors[13] = Calc.HexToColor("b22222");
                            expandedColors[14] = Calc.HexToColor("8b0000");
                            expandedColors[15] = Calc.HexToColor("ff1493");
                            expandedColors[16] = Calc.HexToColor("ff69b4");
                            expandedColors[17] = Calc.HexToColor("db7093");
                            expandedColors[18] = Calc.HexToColor("c71585");
                            
                            // Shift + D0 color (index 19)
                            expandedColors[19] = Calc.HexToColor("ff00ff");
                            
                            // Ctrl + D1-D5 colors (indices 20-24)
                            expandedColors[20] = Calc.HexToColor("9400d3");
                            expandedColors[21] = Calc.HexToColor("8a2be2");
                            expandedColors[22] = Calc.HexToColor("7b68ee");
                            expandedColors[23] = Calc.HexToColor("6a5acd");
                            
                            // Set the expanded array back
                            fgTilesColorField.SetValue(null, expandedColors);
                            
                            Logger.Log(LogLevel.Info, "DZ",
                                "[LevelTemplateHook] Expanded fgTilesColor from 7 to 24 colors");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[LevelTemplateHook] Failed to expand color array: {ex.Message}");
            }
        }

        // =====================================================================
        //  FLING BIRD INTRO HOOK — Integrate with EnhancedBirdNPC
        // =====================================================================
        //
        //  Stores reference to FlingBirdIntro so CS19_MissTheBird can access
        //  its properties (sprite, position, nodes) when creating EnhancedBirdNPC.
        //
        // =====================================================================

        private delegate void orig_FlingBirdIntro_Ctor(Celeste.Entities.FlingBirdIntro self, EntityData data, Vector2 levelOffset);

        private static void Hook_FlingBirdIntro_Ctor(orig_FlingBirdIntro_Ctor orig, Celeste.Entities.FlingBirdIntro self, EntityData data, Vector2 levelOffset)
        {
            // Call original constructor
            orig(self, data, levelOffset);

            // FlingBirdIntro is now initialized and ready for use with EnhancedBirdNPC
            // CS19_MissTheBird will find this instance and use its data
        }
    }
}
