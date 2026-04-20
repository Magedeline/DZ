#nullable enable
using Celeste.Entities;

namespace Celeste
{
    /// <summary>
    /// Manages the Void Moon (<see cref="MountainMoonAlt"/> + <see cref="AstralBirthVoidOverlay"/>)
    /// on the overworld mountain.  Hooks into Overworld lifecycle events to create, position,
    /// show/hide, and clean up the void moon entities.
    ///
    /// The void moon is shown when:
    ///   1. <see cref="MaggyHelperModule.SaveData"/>.<c>VoidMoonUnlocked</c> is true (set after completing Ch19), AND
    ///   2. The currently-viewed area's SID contains <c>20_TheEnd</c>.
    /// </summary>
    [HotReloadable]
    public static class VoidMoonManager
    {
        // ── Live entity references (null when not on the overworld) ──
        private static MountainMoonAlt? moonAlt;
        private static AstralBirthVoidOverlay? astralVoid;
        private static FloatingDesoloMountain? floatingMountain;
        private static VoidMoonUpdater? updater;
        private static bool hooked;

        // ─────────────────────────────────────────────────────────
        // Hook registration (call from MaggyHelperHooks)
        // ─────────────────────────────────────────────────────────

        public static void Load()
        {
            if (hooked) return;
            On.Celeste.Overworld.Begin += OnOverworldBegin;
            On.Celeste.Overworld.End   += OnOverworldEnd;
            hooked = true;
            IngesteLogger.Info("VoidMoonManager: hooks loaded");
        }

        public static void Unload()
        {
            if (!hooked) return;
            On.Celeste.Overworld.Begin -= OnOverworldBegin;
            On.Celeste.Overworld.End   -= OnOverworldEnd;
            hooked = false;
            CleanUp();
            IngesteLogger.Info("VoidMoonManager: hooks unloaded");
        }

        // ─────────────────────────────────────────────────────────
        // Overworld lifecycle hooks
        // ─────────────────────────────────────────────────────────

        private static void OnOverworldBegin(On.Celeste.Overworld.orig_Begin orig, Overworld self)
        {
            orig(self);

            try
            {
                // Always add the updater; it will dynamically show/hide the moon
                // based on the currently-selected chapter / save data.
                var mountain = self.Mountain;
                if (mountain == null)
                {
                    IngesteLogger.Warn("VoidMoonManager: Overworld.Mountain is null – skipping");
                    return;
                }

                // Create moon entities (initially hidden – the updater decides visibility)
                moonAlt = new MountainMoonAlt(mountain);
                moonAlt.LoadContent();
                moonAlt.Show = false;
                self.Add(moonAlt);

                astralVoid = new AstralBirthVoidOverlay(mountain);
                astralVoid.LoadContent();
                astralVoid.Show = false;
                self.Add(astralVoid);

                floatingMountain = new FloatingDesoloMountain(mountain);
                floatingMountain.LoadContent();
                floatingMountain.Show = false;
                self.Add(floatingMountain);

                // Lightweight position/visibility updater
                updater = new VoidMoonUpdater(mountain, moonAlt, astralVoid, floatingMountain);
                self.Add(updater);

                IngesteLogger.Info("VoidMoonManager: entities created on overworld");
            }
            catch (Exception ex)
            {
                IngesteLogger.Warn($"VoidMoonManager: failed to create void moon – {ex.Message}");
                CleanUp();
            }
        }

        private static void OnOverworldEnd(On.Celeste.Overworld.orig_End orig, Overworld self)
        {
            CleanUp();
            orig(self);
        }

        private static void CleanUp()
        {
            moonAlt    = null;
            astralVoid = null;
            floatingMountain = null;
            updater    = null;
        }

        // ─────────────────────────────────────────────────────────
        // Visibility predicate
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the void moon should currently be visible.
        /// </summary>
        internal static bool ShouldShowVoidMoon(int? currentArea = null)
        {
            // Only show the void moon when the player is viewing Chapter 20 (The End)
            // and has unlocked it by completing Chapter 19.
            if (MaggyHelperModule.SaveData?.VoidMoonUnlocked != true)
                return false;

            int area = currentArea ?? -1;
            if (area >= 0 && area < AreaData.Areas.Count)
            {
                var ad = AreaData.Get(area);
                if (ad?.SID != null &&
                    ad.SID.Contains("20_TheEnd", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        // ─────────────────────────────────────────────────────────
        // Inner updater entity
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Lightweight entity added to the Overworld scene.
        /// Each frame it:
        ///   1. Checks whether the void moon should be shown.
        ///   2. Updates the moon/void positions relative to the mountain camera
        ///      so they always appear in the sky above the current focal point.
        /// </summary>
        [HotReloadable]
        private class VoidMoonUpdater : Entity
        {
            private readonly MountainRenderer mountain;
            private readonly MountainMoonAlt moonAlt;
            private readonly AstralBirthVoidOverlay astralVoid;
            private readonly FloatingDesoloMountain floatingMountain;

            /// <summary>Vertical offset above the camera target for the moon base position.</summary>
            private const float SkyOffsetY = 1.8f;
            private static readonly Vector3 FloatingMountainAnchorOffset = new Vector3(0.8f, 0.9f, -0.2f);

            public VoidMoonUpdater(
                MountainRenderer mountain,
                MountainMoonAlt moonAlt,
                AstralBirthVoidOverlay astralVoid,
                FloatingDesoloMountain floatingMountain)
            {
                this.mountain   = mountain;
                this.moonAlt    = moonAlt;
                this.astralVoid = astralVoid;
                this.floatingMountain = floatingMountain;
                Depth = -9600; // update priority between moon (-9500) and overlay (-9400)
            }

            public override void Update()
            {
                base.Update();

                // Resolve visibility
                int area = mountain.Area;
                bool show = ShouldShowVoidMoon(area);
                moonAlt.Show    = show;
                astralVoid.Show = show;
                floatingMountain.Show = show;

                if (!show) return;

                // ── Dynamic positioning ─────────────────────────────
                // Place the moon above the camera's target so it is always
                // visible "in the sky" regardless of which chapter is selected.
                // The entities' built-in Offsets (1.5 / 2.5 Y) add additional
                // height, so we only need a modest base offset here.
                try
                {
                    var cam = mountain.Model.Camera;
                    var target = cam.Target;
                    var pos = new Vector3(target.X, target.Y + SkyOffsetY, target.Z);
                    moonAlt.Position    = pos;
                    astralVoid.Position = pos;
                    floatingMountain.Position = pos + FloatingMountainAnchorOffset;
                }
                catch
                {
                    // Camera unavailable – leave at last known position
                }
            }
        }
    }
}
