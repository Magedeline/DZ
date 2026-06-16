#nullable enable
using Celeste.Entities;

namespace DZ
{
    /// <summary>
    /// Manages the mod's custom overworld visuals: a primary mountain renderer,
    /// an optional lazily-created secondary renderer (unlocked via progression),
    /// the Maggy3D character marker, and the Void-Moon / Astral-Birth-Void skybox
    /// effects.  All renderer calls are wrapped in try/catch so a missing or broken
    /// mountain asset never hard-crashes the game.
    /// </summary>
    [HotReloadable]
    public class OverworldConnector : Entity
    {
        // ── Renderers ─────────────────────────────────────────────────────────

        private readonly MountainRenderer primary;
        private MountainRenderer? secondary;          // created on-demand
        private bool secondaryVisible;
        private bool triedCreateSecondary;

        /// <summary>World-space point where the primary and secondary mountains visually meet.</summary>
        public readonly Vector3 ConnectionPoint;

        /// <summary>World-space origin for the secondary mountain when it first appears.</summary>
        public readonly Vector3 SecondaryStartPoint;

        // ── Maggy3D Marker ────────────────────────────────────────────────────

        /// <summary>Custom Maggy3D marker for the mountain (replaces Maddy3D when enabled).</summary>
        public Maggy3D? MaggyMarker { get; private set; }

        /// <summary>Whether the Maggy3D marker is active.</summary>
        public bool UseMaggyMarker { get; set; }

        private Vector3 markerPosition = Vector3.Zero;

        // ── Void Moon / Astral Void ───────────────────────────────────────────

        /// <summary>Alternative moon rendered with moon.obj + moon_alt.png above the normal moon.</summary>
        public MountainMoonAlt? MoonAlt { get; private set; }

        /// <summary>Astral birth void halo effect drawn behind the alt-moon.</summary>
        public AstralBirthVoidOverlay? AstralVoid { get; private set; }

        /// <summary>Whether the Void-Moon / Astral-Void combo is currently enabled.</summary>
        public bool UseMoonAlt { get; set; }

        // ── Construction ──────────────────────────────────────────────────────

        public OverworldConnector(
            MountainRenderer primaryMountain,
            Vector3 connectionPoint,
            Vector3 secondaryStartPoint)
        {
            primary = primaryMountain ?? throw new ArgumentNullException(nameof(primaryMountain));
            ConnectionPoint = connectionPoint;
            SecondaryStartPoint = secondaryStartPoint;
            Depth = -10000; // render behind Oui / UI
            Add(new Coroutine(DeferredInitRoutine()));
        }

        // ── Deferred Initialisation ───────────────────────────────────────────

        /// <summary>
        /// Waits one frame (so SaveData is populated) then runs auto-unlock checks
        /// and lazily creates the Maggy marker and Void-Moon when appropriate.
        /// </summary>
        private IEnumerator DeferredInitRoutine()
        {
            yield return null; // one-frame delay

            TryAutoUnlockSecondary();

            if (UseMaggyMarker && MaggyMarker == null)
                CreateMaggyMarker();

            if (UseMoonAlt)
                CreateMoonAlt();
            else
                TryAutoEnableVoidMoon();
        }

        // ── Maggy Marker ─────────────────────────────────────────────────────

        private void CreateMaggyMarker()
        {
            try
            {
                MaggyMarker = new Maggy3D(primary);
                Scene?.Add(MaggyMarker);
                IngesteLogger.Info("Maggy3D marker initialised on overworld");
            }
            catch (Exception ex)
            {
                IngesteLogger.Warn($"Failed to create Maggy3D marker: {ex.Message}");
                MaggyMarker = null;
            }
        }

        /// <summary>Enable and show the Maggy3D marker, optionally with a custom texture.</summary>
        public void EnableMaggyMarker(string? customMarkerPath = null)
        {
            UseMaggyMarker = true;

            if (MaggyMarker == null)
                CreateMaggyMarker();

            if (MaggyMarker != null)
            {
                if (customMarkerPath != null)
                    MaggyMarker.SetCustomMarker(customMarkerPath);
                MaggyMarker.Show = true;
            }
        }

        /// <summary>Disable and hide the Maggy3D marker.</summary>
        public void DisableMaggyMarker()
        {
            UseMaggyMarker = false;
            if (MaggyMarker != null)
                MaggyMarker.Show = false;
        }

        /// <summary>Set the world-space position for the Maggy marker.</summary>
        public void SetMarkerPosition(Vector3 position)
        {
            markerPosition = position;
            if (MaggyMarker != null)
                MaggyMarker.Position = position;
        }

        // ── Void Moon / Astral Void ──────────────────────────────────────────

        /// <summary>
        /// Auto-enables the Void-Moon / Astral-Void when the player has completed
        /// chapter 19 (save-data flag) or when chapter 20 (TheEnd) is selected.
        /// </summary>
        private void TryAutoEnableVoidMoon()
        {
            if (UseMoonAlt) return;

            try
            {
                bool unlocked = DZModule.SaveData?.VoidMoonUnlocked == true;

                if (!unlocked && primary != null)
                {
                    int area = primary.Area;
                    if (area >= 0 && area < AreaData.Areas.Count)
                    {
                        var ad = AreaData.Get(area);
                        if (ad?.SID != null &&
                            ad.SID.Contains("20_TheEnd", StringComparison.OrdinalIgnoreCase))
                        {
                            unlocked = true;
                        }
                    }
                }

                if (unlocked)
                {
                    EnableMoonAlt();
                    IngesteLogger.Info("Void Moon auto-enabled (save data or chapter-20 selected)");
                }
            }
            catch (Exception ex)
            {
                IngesteLogger.Warn($"TryAutoEnableVoidMoon failed: {ex.Message}");
            }
        }

        private void CreateMoonAlt()
        {
            try
            {
                MoonAlt = new MountainMoonAlt(primary);
                MoonAlt.LoadContent();
                Scene?.Add(MoonAlt);

                AstralVoid = new AstralBirthVoidOverlay(primary);
                AstralVoid.LoadContent();
                Scene?.Add(AstralVoid);

                IngesteLogger.Info("Moon-Alt + Astral Birth Void initialised on overworld");
            }
            catch (Exception ex)
            {
                IngesteLogger.Warn($"Failed to create Moon-Alt: {ex.Message}");
                MoonAlt = null;
                AstralVoid = null;
            }
        }

        /// <summary>Enable the alt-moon and astral void. Created lazily if needed.</summary>
        public void EnableMoonAlt(Vector3? moonPosition = null)
        {
            UseMoonAlt = true;

            if (MoonAlt == null || AstralVoid == null)
                CreateMoonAlt();

            if (moonPosition.HasValue)
                SetMoonAltPosition(moonPosition.Value);

            if (MoonAlt != null) MoonAlt.Show = true;
            if (AstralVoid != null) AstralVoid.Show = true;
        }

        /// <summary>Disable and hide the alt-moon and astral void.</summary>
        public void DisableMoonAlt()
        {
            UseMoonAlt = false;
            if (MoonAlt != null) MoonAlt.Show = false;
            if (AstralVoid != null) AstralVoid.Show = false;
        }

        /// <summary>Reposition both the alt-moon and the astral void effect.</summary>
        public void SetMoonAltPosition(Vector3 position)
        {
            if (MoonAlt != null) MoonAlt.Position = position;
            if (AstralVoid != null) AstralVoid.Position = position;
        }

        // ── Secondary Renderer ───────────────────────────────────────────────

        private void TryAutoUnlockSecondary()
        {
            if (secondaryVisible || triedCreateSecondary)
                return;
            if (SaveData.Instance != null && SaveData.Instance.UnlockedAreas >= 2)
                UnlockSecondary();
        }

        /// <summary>Reveal the secondary mountain renderer with an unlock animation.</summary>
        public void UnlockSecondary()
        {
            if (secondaryVisible)
                return;

            EnsureSecondary();
            if (secondary == null)
                return; // creation failed gracefully

            secondaryVisible = true;
            secondary.Visible = true;
            Audio.Play("event:/ui/pusheen/unlock_newmountian_icon");
            Add(new Coroutine(UnlockAnimationRoutine()));
        }

        private void EnsureSecondary()
        {
            if (secondary != null || triedCreateSecondary)
                return;

            triedCreateSecondary = true;
            try
            {
                secondary = new MountainRenderer { Visible = false };
                Scene?.Add(secondary);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"Secondary MountainRenderer creation failed: {ex.Message}");
                secondary = null;
            }
        }

        private IEnumerator UnlockAnimationRoutine()
        {
            const float duration = 1.8f;
            float t = 0f;
            while (t < duration)
            {
                t += Engine.DeltaTime;
                yield return null;
            }

            MaggyMarker?.TriggerWiggle();
        }

        // ── Frame Loop ───────────────────────────────────────────────────────

        public override void Update()
        {
            base.Update();
            TryAutoUnlockSecondary();

            SafeUpdate(primary);
            if (secondaryVisible)
                SafeUpdate(secondary);

            if (UseMaggyMarker && MaggyMarker != null)
                MaggyMarker.Position = markerPosition;

            // Keep Void-Moon visibility in sync with our flag
            if (MoonAlt != null) MoonAlt.Show = UseMoonAlt;
            if (AstralVoid != null) AstralVoid.Show = UseMoonAlt;
        }

        public override void Render()
        {
            base.Render();
            SafeRender(primary);
            if (secondaryVisible)
                SafeRender(secondary);
        }

        // ── Safe Wrappers ────────────────────────────────────────────────────

        private static void SafeUpdate(MountainRenderer? renderer)
        {
            if (renderer == null) return;
            try { renderer.Update(null); }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"MountainRenderer.Update failed: {ex.Message}");
            }
        }

        private static void SafeRender(MountainRenderer? renderer)
        {
            if (renderer == null) return;
            try { renderer.Render(null); }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"MountainRenderer.Render failed: {ex.Message}");
            }
        }
    }
}
