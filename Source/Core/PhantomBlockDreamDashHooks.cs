using System.Reflection;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace DZ
{
    /// <summary>
    /// Lets the vanilla <see cref="global::Celeste.Player"/> dream-dash through a
    /// <see cref="Celeste.Entities.PhantomBlock"/> — a dream-dashable <see cref="global::Celeste.Solid"/>
    /// that is NOT a <see cref="global::Celeste.DreamBlock"/>.
    ///
    /// Because PhantomBlock is not a DreamBlock, the vanilla dream-dash state machine
    /// (which only looks for <c>CollideFirst&lt;DreamBlock&gt;</c>) would neither enter
    /// nor stay in the dream-dash state for it, and <c>DreamDashedIntoSolid</c> would
    /// kill the player (PhantomBlock is a Solid).  These hooks add PhantomBlock support
    /// alongside the vanilla DreamBlock behaviour without replacing the whole state.
    ///
    /// Hooks (all private Player methods, applied via manual <see cref="Hook"/>):
    ///   <list type="bullet">
    ///     <item><c>DreamDashCheck</c>  — fall back to PhantomBlock when no DreamBlock is found.</item>
    ///     <item><c>DreamDashedIntoSolid</c> — never die while inside a PhantomBlock.</item>
    ///     <item><c>DreamDashUpdate</c> — keep the dash alive while inside a PhantomBlock
    ///         (the vanilla code would end the dash once the grace timer expires even
    ///         though the player is still inside the block).</item>
    ///     <item><c>DreamDashEnd</c>    — fire <see cref="Celeste.Entities.PhantomBlock.OnPlayerExit"/>
    ///         and apply the dream-jump grace for a PhantomBlock exit.</item>
    ///   </list>
    ///
    /// The current PhantomBlock target is stored on the player via DynamicData
    /// (<see cref="PhantomKey"/>), which <see cref="DreamBlockPlayerSwapHooks"/> also
    /// reads to skip the Madeline↔Kirby swap for PhantomBlock dashes.
    /// </summary>
    internal static class PhantomBlockDreamDashHooks
    {
        // DynamicData key for the current PhantomBlock dream-dash target on the Player.
        public const string PhantomKey = "DZ_PhantomBlock";

        // Manual hooks — kept alive to avoid GC disposal.
        private static Hook _dreamDashCheckHook;
        private static Hook _dreamDashedIntoSolidHook;
        private static Hook _dreamDashUpdateHook;
        private static Hook _dreamDashEndHook;

        private static MethodInfo _createTrail;
        private static bool _loaded;

        // orig delegate types matching the private Player method signatures.
        private delegate bool orig_DreamDashCheck(global::Celeste.Player self, Vector2 dir);
        private delegate bool orig_DreamDashedIntoSolid(global::Celeste.Player self);
        private delegate int  orig_DreamDashUpdate(global::Celeste.Player self);
        private delegate void orig_DreamDashEnd(global::Celeste.Player self);

        // ── Load / Unload ───────────────────────────────────────────────────────
        internal static void Load()
        {
            if (_loaded)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    "[PhantomBlockDreamDash] Load() called while already loaded — skipping");
                return;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

            _dreamDashCheckHook = CreateHook(
                typeof(global::Celeste.Player).GetMethod("DreamDashCheck", flags),
                nameof(Hook_DreamDashCheck));

            _dreamDashedIntoSolidHook = CreateHook(
                typeof(global::Celeste.Player).GetMethod("DreamDashedIntoSolid", flags),
                nameof(Hook_DreamDashedIntoSolid));

            _dreamDashUpdateHook = CreateHook(
                typeof(global::Celeste.Player).GetMethod("DreamDashUpdate", flags),
                nameof(Hook_DreamDashUpdate));

            _dreamDashEndHook = CreateHook(
                typeof(global::Celeste.Player).GetMethod("DreamDashEnd", flags),
                nameof(Hook_DreamDashEnd));

            _createTrail = typeof(global::Celeste.Player).GetMethod("CreateTrail", flags);

            _loaded = true;
            Logger.Log(LogLevel.Info, "DZ", "[PhantomBlockDreamDash] Hooks loaded");
        }

        internal static void Unload()
        {
            if (!_loaded)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    "[PhantomBlockDreamDash] Unload() called while not loaded — skipping");
                return;
            }

            _dreamDashCheckHook?.Dispose();
            _dreamDashedIntoSolidHook?.Dispose();
            _dreamDashUpdateHook?.Dispose();
            _dreamDashEndHook?.Dispose();
            _dreamDashCheckHook = null;
            _dreamDashedIntoSolidHook = null;
            _dreamDashUpdateHook = null;
            _dreamDashEndHook = null;
            _createTrail = null;

            _loaded = false;
            Logger.Log(LogLevel.Info, "DZ", "[PhantomBlockDreamDash] Hooks unloaded");
        }

        private static Hook CreateHook(MethodInfo target, string hookName)
        {
            if (target == null)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[PhantomBlockDreamDash] Target method not found for hook '{hookName}' — skipping");
                return null;
            }

            MethodInfo hook = typeof(PhantomBlockDreamDashHooks).GetMethod(
                hookName, BindingFlags.Static | BindingFlags.NonPublic);
            return new Hook(target, hook);
        }

        // ── DreamDashCheck ──────────────────────────────────────────────────────
        private static bool Hook_DreamDashCheck(orig_DreamDashCheck orig,
            global::Celeste.Player self, Vector2 dir)
        {
            if (orig(self, dir))
                return true;

            // PhantomBlock fallback — mirrors the vanilla DreamBlock corner-correction
            // but stores the target in DynamicData (PhantomKey) instead of the private
            // `dreamBlock` field (which is typed DreamBlock and can't hold a PhantomBlock).
            try
            {
                if (!(self.Inventory.DreamDash && self.DashAttacking))
                    return false;
                if (!(dir.X == Math.Sign(self.DashDir.X) || dir.Y == Math.Sign(self.DashDir.Y)))
                    return false;

                var phantom = self.CollideFirst<Celeste.Entities.PhantomBlock>(self.Position + dir);
                if (phantom == null)
                    return false;

                if (self.CollideCheck<global::Celeste.Solid, Celeste.Entities.PhantomBlock>(self.Position + dir))
                {
                    Vector2 side = new Vector2(Math.Abs(dir.Y), Math.Abs(dir.X));
                    bool checkNegative, checkPositive;
                    if (dir.X != 0f)
                    {
                        checkNegative = self.Speed.Y <= 0f;
                        checkPositive = self.Speed.Y >= 0f;
                    }
                    else
                    {
                        checkNegative = self.Speed.X <= 0f;
                        checkPositive = self.Speed.X >= 0f;
                    }

                    if (checkNegative)
                    {
                        for (int i = -1; i >= -4; i--)
                        {
                            Vector2 at = self.Position + dir + side * i;
                            if (!self.CollideCheck<global::Celeste.Solid, Celeste.Entities.PhantomBlock>(at))
                            {
                                self.Position += side * i;
                                SetPhantom(self, phantom);
                                return true;
                            }
                        }
                    }

                    if (checkPositive)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            Vector2 at = self.Position + dir + side * i;
                            if (!self.CollideCheck<global::Celeste.Solid, Celeste.Entities.PhantomBlock>(at))
                            {
                                self.Position += side * i;
                                SetPhantom(self, phantom);
                                return true;
                            }
                        }
                    }

                    return false;
                }

                SetPhantom(self, phantom);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[PhantomBlockDreamDash] Error in DreamDashCheck hook: {ex.Message}");
                return false;
            }
        }

        // ── DreamDashedIntoSolid ────────────────────────────────────────────────
        // PhantomBlock is a Solid, so the vanilla check would consider the player
        // "dashed into a solid" and kill them while they're legitimately dream-dashing
        // through it.  Skip the death whenever the player is inside a PhantomBlock.
        private static bool Hook_DreamDashedIntoSolid(orig_DreamDashedIntoSolid orig,
            global::Celeste.Player self)
        {
            if (self.CollideCheck<Celeste.Entities.PhantomBlock>())
                return false;
            return orig(self);
        }

        // ── DreamDashUpdate ─────────────────────────────────────────────────────
        // Vanilla ends the dream dash once `dreamDashCanEndTimer <= 0` and no DreamBlock
        // is overlapping.  While inside a PhantomBlock (no DreamBlock) we temporarily
        // fake the timer so vanilla keeps us in StDreamDash, then restore the real value
        // and add the trail/displacement cosmetics that vanilla's inside-block branch
        // would have produced.
        private static int Hook_DreamDashUpdate(orig_DreamDashUpdate orig, global::Celeste.Player self)
        {
            DynamicData dyn = DynamicData.For(self);
            bool insidePhantom = self.CollideCheck<Celeste.Entities.PhantomBlock>();

            float realTimer = 0f;
            bool faked = false;

            if (insidePhantom)
            {
                realTimer = dyn.Get<float>("dreamDashCanEndTimer");
                if (realTimer <= 0f)
                {
                    dyn.Set("dreamDashCanEndTimer", 1f);
                    faked = true;
                }
            }

            int result = orig(self);

            if (faked)
                dyn.Set("dreamDashCanEndTimer", realTimer);

            // Add the inside-block cosmetics (trail + displacement) that vanilla skipped
            // because CollideFirst<DreamBlock>() was null for the PhantomBlock.
            if (insidePhantom)
            {
                var phantom = self.CollideFirst<Celeste.Entities.PhantomBlock>();
                if (phantom != null)
                {
                    var level = self.SceneAs<Level>();
                    if (self.Scene.OnInterval(0.1f) && _createTrail != null)
                        _createTrail.Invoke(self, null);

                    if (level != null && level.OnInterval(0.04f))
                    {
                        var burst = level.Displacement.AddBurst(self.Center, 0.3f, 0f, 40f);
                        burst.WorldClipCollider = phantom.Collider;
                        burst.WorldClipPadding = 2;
                    }
                }
            }

            return result;
        }

        // ── DreamDashEnd ────────────────────────────────────────────────────────
        // Vanilla only calls `dreamBlock.OnPlayerExit` / applies the dream-jump grace
        // when the private `dreamBlock` field is set.  For a PhantomBlock that field is
        // null (we track it via DynamicData), so replicate the exit handling here.
        private static void Hook_DreamDashEnd(orig_DreamDashEnd orig, global::Celeste.Player self)
        {
            orig(self);

            try
            {
                var dyn = DynamicData.For(self);
                var phantom = dyn.Get<Celeste.Entities.PhantomBlock>(PhantomKey);
                if (phantom == null)
                    return;

                if (self.DashDir.X != 0f)
                {
                    dyn.Set("dreamJump", true);
                    dyn.Set("jumpGraceTimer", 0.1f);
                }
                else
                {
                    dyn.Set("jumpGraceTimer", 0f);
                }

                phantom.OnPlayerExit(self);
                dyn.Set(PhantomKey, null);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[PhantomBlockDreamDash] Error in DreamDashEnd hook: {ex.Message}");
            }
        }

        // ── Helpers ─────────────────────────────────────────────────────────────
        private static void SetPhantom(global::Celeste.Player player, Celeste.Entities.PhantomBlock phantom)
        {
            DynamicData.For(player).Set(PhantomKey, phantom);
        }
    }
}
