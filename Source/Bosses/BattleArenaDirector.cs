#nullable enable
using System;
using System.Collections.Generic;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Bosses
{
    // ══════════════════════════════════════════════════════════════════════════
    //
    //  BATTLE ARENA DIRECTOR  —  "Conquered Peak, modified"
    //
    //  The Conquered-Peak pattern engine (SiamoZeroFinalBoss / AsrielGodBoss)
    //  already lets a designer author a fight as a comma-separated attack list.
    //  Until now a pattern could only change what the boss *throws at you*.
    //  This lets a pattern change the battleground it throws it across.
    //
    //  A pattern token may carry an arena profile with a '#' suffix:
    //
    //      "CrescentBeamShot, ConqueredPeakCascade#tall, TimeborderCollapse#collapse"
    //      "EnergyShower#wide:1.2"        ← profile and the existing delay grammar
    //
    //  '#' is used rather than ':' or '@' because those two are already the
    //  delay/arg separators in ParseAttackSequenceEntry, and reusing them would
    //  make "Attack@1.4" ambiguous.
    //
    //  When a step with a profile runs, the director cross-fades the arena into
    //  it: flight bounds, fly speed, bob, debris scroll, void drift, camera lock
    //  and colour grade. Geometry and motion only — screen tint stays owned by
    //  KirbyFinalBattleScene so the two never fight over the same field.
    //
    //  Until something actually applies a profile the director is dormant and
    //  the battle behaves exactly as it did before.
    //
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// A named battleground configuration. Everything here is safe to change
    /// mid-flight: WarpStarRideController clamps movement rather than
    /// teleporting, so shrinking the arena squeezes the player instead of
    /// snapping them.
    /// </summary>
    public sealed class ArenaProfile
    {
        public string Name = "";

        // ── Flight arena (WarpStarRideController) ────────────────────────────
        public float ArenaHalfWidth  = 130f;
        public float ArenaHalfHeight = 70f;
        public float FlySpeed        = 120f;
        public float BobAmplitude    = 3f;
        public float BobSpeed        = 2.5f;
        public bool  LockCamera;

        // ── Backdrops ────────────────────────────────────────────────────────
        public float ScrollSpeedX     = 350f;
        public float ScrollSpeedY     = 0f;
        public float AbyssScrollSpeed = 80f;
        public bool  AbyssCycling;

        /// <summary>Celeste colour-grade key. Empty leaves the current grade alone.</summary>
        public string ColorGrade = "";

        /// <summary>Seconds to cross-fade into this profile.</summary>
        public float TransitionTime = 0.6f;

        public ArenaProfile Clone() => (ArenaProfile)MemberwiseClone();

        /// <summary>Component-wise interpolation. Discrete fields snap at the midpoint.</summary>
        public static ArenaProfile Lerp(ArenaProfile a, ArenaProfile b, float t)
        {
            return new ArenaProfile
            {
                Name             = b.Name,
                ArenaHalfWidth   = MathHelper.Lerp(a.ArenaHalfWidth,   b.ArenaHalfWidth,   t),
                ArenaHalfHeight  = MathHelper.Lerp(a.ArenaHalfHeight,  b.ArenaHalfHeight,  t),
                FlySpeed         = MathHelper.Lerp(a.FlySpeed,         b.FlySpeed,         t),
                BobAmplitude     = MathHelper.Lerp(a.BobAmplitude,     b.BobAmplitude,     t),
                BobSpeed         = MathHelper.Lerp(a.BobSpeed,         b.BobSpeed,         t),
                ScrollSpeedX     = MathHelper.Lerp(a.ScrollSpeedX,     b.ScrollSpeedX,     t),
                ScrollSpeedY     = MathHelper.Lerp(a.ScrollSpeedY,     b.ScrollSpeedY,     t),
                AbyssScrollSpeed = MathHelper.Lerp(a.AbyssScrollSpeed, b.AbyssScrollSpeed, t),
                LockCamera       = t < 0.5f ? a.LockCamera   : b.LockCamera,
                AbyssCycling     = t < 0.5f ? a.AbyssCycling : b.AbyssCycling,
                ColorGrade       = b.ColorGrade,
                TransitionTime   = b.TransitionTime,
            };
        }
    }

    /// <summary>
    /// Owns the named arena profiles and drives the live arena toward whichever
    /// one is active. Spawned automatically by KirbyFinalBattleScene; find it
    /// with <see cref="Get"/>.
    /// </summary>
    [Tracked]
    [HotReloadable]
    public class BattleArenaDirector : Entity
    {
        public static BattleArenaDirector? Get(Scene? scene) =>
            scene?.Tracker.GetEntity<BattleArenaDirector>();

        private readonly Dictionary<string, ArenaProfile> profiles =
            new Dictionary<string, ArenaProfile>(StringComparer.OrdinalIgnoreCase);

        private Level level = null!;
        private FlyingBattleScrollBackdrop? scrollBd;
        private AbyssmentBackdrop?          abyssment;

        private ArenaProfile  current = new ArenaProfile();
        private ArenaProfile? from;
        private ArenaProfile? target;
        private float transitionTimer;
        private float transitionLength;

        /// <summary>Name of the profile most recently applied, or "" while dormant.</summary>
        public string ActiveProfileName { get; private set; } = "";

        /// <summary>
        /// False until something applies a profile. While false the director
        /// writes nothing, so the battle keeps its original hand-tuned behaviour.
        /// </summary>
        public bool HasActiveProfile => ActiveProfileName.Length > 0;

        /// <summary>The live, mid-transition arena values.</summary>
        public ArenaProfile Current => current;

        public BattleArenaDirector()
        {
            Tag = Tags.Persistent;
            Depth = -10000;
            RegisterBuiltInProfiles();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scrollBd  = level.Background.Get<FlyingBattleScrollBackdrop>();
            abyssment = level.Background.Get<AbyssmentBackdrop>();
        }

        // ── Profile table ────────────────────────────────────────────────────

        /// <summary>Adds or replaces a profile. Name match is case-insensitive.</summary>
        public void RegisterProfile(ArenaProfile profile)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.Name)) return;
            profiles[profile.Name.Trim()] = profile;
        }

        public bool HasProfile(string name) =>
            !string.IsNullOrWhiteSpace(name) && profiles.ContainsKey(name.Trim());

        /// <summary>
        /// Cross-fades the arena into the named profile. Unknown names are
        /// ignored and logged — a typo in a Loenn attackSequence string should
        /// be visible in the log, not a silent no-op.
        /// </summary>
        /// <returns>
        /// True if the director is now driving this profile, so the caller knows
        /// to stop writing the fields the director has taken over.
        /// </returns>
        public bool Apply(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            name = name.Trim();

            if (!profiles.TryGetValue(name, out ArenaProfile? next))
            {
                Logger.Log(LogLevel.Warn, "DZ",
                    $"[BattleArenaDirector] Unknown arena profile '{name}' — ignoring.");
                return false;
            }

            if (string.Equals(ActiveProfileName, name, StringComparison.OrdinalIgnoreCase))
                return true;

            // First application snaps: there is no sensible "from" to fade out of.
            from   = HasActiveProfile ? current.Clone() : next.Clone();
            target = next;
            transitionLength = HasActiveProfile ? Math.Max(next.TransitionTime, 0f) : 0f;
            transitionTimer  = 0f;
            ActiveProfileName = name;

            if (!string.IsNullOrEmpty(next.ColorGrade))
                level.NextColorGrade(next.ColorGrade, 1f);

            return true;
        }

        /// <summary>Stops driving the arena and hands the fields back to their previous owners.</summary>
        public void Release()
        {
            ActiveProfileName = "";
            from = target = null;
        }

        // ── Drive ────────────────────────────────────────────────────────────
        public override void Update()
        {
            base.Update();
            if (!HasActiveProfile || target == null) return;

            if (transitionLength > 0f && transitionTimer < transitionLength)
            {
                transitionTimer += Engine.DeltaTime;
                float t = Ease.CubeInOut(Math.Min(transitionTimer / transitionLength, 1f));
                current = ArenaProfile.Lerp(from ?? target, target, t);
            }
            else
            {
                current = target;
            }

            Push();
        }

        private void Push()
        {
            var ride = Scene.Tracker.GetEntity<WarpStarRideController>();
            if (ride != null)
            {
                ride.ArenaHalfWidth  = current.ArenaHalfWidth;
                ride.ArenaHalfHeight = current.ArenaHalfHeight;
                ride.MoveSpeed       = current.FlySpeed;
                ride.BobAmplitude    = current.BobAmplitude;
                ride.BobSpeed        = current.BobSpeed;
                ride.LockCamera      = current.LockCamera;
            }

            scrollBd ??= level.Background.Get<FlyingBattleScrollBackdrop>();
            if (scrollBd != null)
            {
                scrollBd.ScrollSpeedX = current.ScrollSpeedX;
                scrollBd.ScrollSpeedY = current.ScrollSpeedY;
            }

            abyssment ??= level.Background.Get<AbyssmentBackdrop>();
            if (abyssment != null)
            {
                abyssment.ScrollSpeed = current.AbyssScrollSpeed;
                abyssment.Cycling     = current.AbyssCycling;
            }
        }

        // ── Built-in palette ─────────────────────────────────────────────────
        // Seven shapes that read differently the moment you are inside them.
        // Pair them with the attack that suits the space: cascades want "tall",
        // sweeping beams want "tight", the endgame wants "collapse".
        private void RegisterBuiltInProfiles()
        {
            RegisterProfile(new ArenaProfile
            {
                Name = "default",
                ArenaHalfWidth = 130f, ArenaHalfHeight = 70f,
                FlySpeed = 120f, BobAmplitude = 3f, BobSpeed = 2.5f,
                ScrollSpeedX = 350f, AbyssScrollSpeed = 80f,
            });

            // Corridor. Little room left or right — dodging becomes precise.
            RegisterProfile(new ArenaProfile
            {
                Name = "tight",
                ArenaHalfWidth = 90f, ArenaHalfHeight = 48f,
                FlySpeed = 140f, BobAmplitude = 2f, BobSpeed = 3f,
                ScrollSpeedX = 480f, AbyssScrollSpeed = 110f,
                LockCamera = true, TransitionTime = 0.5f,
            });

            // Open sky. Room to run, so the boss can afford wide spreads.
            RegisterProfile(new ArenaProfile
            {
                Name = "wide",
                ArenaHalfWidth = 180f, ArenaHalfHeight = 92f,
                FlySpeed = 150f, BobAmplitude = 4f, BobSpeed = 2f,
                ScrollSpeedX = 260f, AbyssScrollSpeed = 70f,
                TransitionTime = 0.8f,
            });

            // Vertical lane — for anything that rains down. ConqueredPeakCascade.
            RegisterProfile(new ArenaProfile
            {
                Name = "tall",
                ArenaHalfWidth = 96f, ArenaHalfHeight = 110f,
                FlySpeed = 130f, BobAmplitude = 3f, BobSpeed = 2.5f,
                ScrollSpeedX = 300f, AbyssScrollSpeed = 90f,
                TransitionTime = 0.7f,
            });

            // Speed section. Everything moves, including you.
            RegisterProfile(new ArenaProfile
            {
                Name = "rush",
                ArenaHalfWidth = 120f, ArenaHalfHeight = 60f,
                FlySpeed = 190f, BobAmplitude = 1.5f, BobSpeed = 4f,
                ScrollSpeedX = 620f, AbyssScrollSpeed = 160f,
                LockCamera = true, TransitionTime = 0.4f,
            });

            // Slow, floaty, weightless. The void between attacks.
            RegisterProfile(new ArenaProfile
            {
                Name = "drift",
                ArenaHalfWidth = 150f, ArenaHalfHeight = 80f,
                FlySpeed = 90f, BobAmplitude = 6f, BobSpeed = 1.4f,
                ScrollSpeedX = 120f, AbyssScrollSpeed = 40f,
                AbyssCycling = true, TransitionTime = 1.2f,
            });

            // Endgame. The walls have closed in and there is nowhere left.
            RegisterProfile(new ArenaProfile
            {
                Name = "collapse",
                ArenaHalfWidth = 70f, ArenaHalfHeight = 40f,
                FlySpeed = 160f, BobAmplitude = 2f, BobSpeed = 3.5f,
                ScrollSpeedX = 700f, AbyssScrollSpeed = 200f,
                LockCamera = true, TransitionTime = 0.35f,
            });
        }
    }
}
