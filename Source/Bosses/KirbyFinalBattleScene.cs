#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Cutscenes;
using Celeste.Entities;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Bosses
{
    /// <summary>
    /// Master scene controller for the Kirby Flying Final Battle.
    ///
    /// Place ONE instance of this entity in a room of DZ/0/21_LastLevel.bin.
    /// It drives the entire pre-ELS-Termina sequence in-place inside that room —
    /// no separate map file needed.
    ///
    /// ═══════════════════════════════════════════════════
    ///  SEQUENCE OVERVIEW  (all driven by C# coroutines)
    /// ═══════════════════════════════════════════════════
    ///
    ///  PRE-PHASE — The Last Alliance
    ///    • Dream friends and allies arrive.
    ///    • Seven Vessel Goner Souls materialise as coloured vertex lights.
    ///    • ZeroWaveManager activates: six Zero forms (Siamo Zero, Zero 3,
    ///      Contra Void, Tesseract Soul, Hyper Meta Morpho Knight, Nollus Nova)
    ///      must all be defeated. Each defeat triggers a colour burst.
    ///    • When all six fall → colour explosion → transition.
    ///
    ///  PHASE 1 — Warp Star Launch → Flying Into The Void
    ///    • Warp Star bob physics applied directly to the player.
    ///    • AbyssmentBackdrop cycling begins (multi-title streak effect).
    ///    • No ground; player.EnforceLevelBounds = false.
    ///    • Asriel's power keeps them aloft (dialogue + soul light).
    ///    • Determination Soul flares red.
    ///    • Els Termina silhouette appears.
    ///
    ///  PHASE 2 — Side-Scroll Approach + Boss Fight
    ///    • FlyingBattleScrollBackdrop activates (fast left-scroll debris).
    ///    • Ground materialises; level bounds restored.
    ///    • HP-gated FinalBattlePhaseShiftTriggers fire colour/scroll boosts.
    ///    • ELSTerminaFinalBoss is already in the room — this scene just
    ///      prepares the arena and starts the fight.
    ///
    /// ═══════════════════════════════════════════════════
    ///  LEVEL DESIGNER KNOBS  (all EntityData properties)
    /// ═══════════════════════════════════════════════════
    ///   totalZeroForms         int    6       Zero forms to defeat before Warp Star
    ///   healthPerZeroForm      float  80      HP each Zero form has
    ///   hardMode               bool   false   1.5× HP multiplier for all forms
    ///   warpStarBobAmplitude   float  3       Pixels of vertical sine-bob on the player
    ///   warpStarBobSpeed       float  2.5     Bob frequency (radians/second)
    ///   warpStarRideDuration   float  8       Seconds the Warp Star ride lasts
    ///   warpStarFlySpeed       float  120     Horizontal fly speed (px/s) during WarpStarRide + FlyingVoid
    ///   phase2ScrollSpeed      float  350     FlyingBattleScrollBackdrop pixels/second
    ///   allyFormationOffsetX   float  -40     Ally cluster X offset from player
    ///   allyFormationOffsetY   float  -20     Ally cluster Y offset from player
    ///   allySpacing            float  26      Horizontal pixels between each ally
    ///   activeAllies           string "Kirby,Madeline,Badeline,Asriel,Magolor,BandanaDee,Marx,Gooey,Susie"
    ///   phase1MusicEvent       string event:/DZ/new_content/music/lvl21/void_approach
    ///   battleMusicEvent       string event:/DZ/new_content/music/lvl21/els_termina_final
    ///   victoryMusicEvent      string event:/DZ/new_content/music/lvl21/victory
    ///   completionFlag         string ch21_els_termina_final_battle_done
    ///   progressKey            string ch21_final_battle
    ///                                 Session key prefix for death-resume state.
    ///
    /// ═══════════════════════════════════════════════════
    ///  DEATH RESUME
    /// ═══════════════════════════════════════════════════
    ///  Level.Reload() (any death) calls UnloadLevel(), which removes every
    ///  entity not tagged Tags.Global — Tags.Persistent only survives room
    ///  *transitions*. So a death destroys this entity and kills the whole
    ///  BattleSequence() coroutine with it, and the room reloads with a fresh
    ///  copy sitting in BattlePhase.Idle that nothing ever restarts.
    ///
    ///  Progress is therefore checkpointed into the Session (same convention as
    ///  AsrielGodBoss) and Awake() resumes automatically:
    ///     PreBattle    → back into the Zero waves, already-defeated forms skipped
    ///     WarpStarRide → Phase 1 replays from the Warp Star launch
    ///     FlyingVoid   ↗
    ///     Phase2Scroll → straight back into the side-scroll approach
    /// </summary>
    [CustomEntity("DZ/KirbyFinalBattleScene")]
    [Tracked(true)]
    [HotReloadable]
    public class KirbyFinalBattleScene : Entity
    {
        // ── Public phase enum ─────────────────────────────────────────────────
        public enum BattlePhase
        {
            Idle,
            PreBattle,
            WarpStarRide,
            FlyingVoid,
            Phase2Scroll,
            Defeated
        }

        // ── Shared phase colours (readable by PhaseShiftTrigger) ─────────────
        public static readonly Color[] PhaseColors = new Color[]
        {
            Calc.HexToColor("ff4444"), // 0 – full HP      / red
            Calc.HexToColor("ff8800"), // 1 – 75 % HP      / orange
            Calc.HexToColor("ffdd00"), // 2 – 50 % HP      / yellow
            Calc.HexToColor("44ddff"), // 3 – 25 % HP      / cyan
            Calc.HexToColor("ffffff"), // 4 – near-death   / white
        };

        // ── Soul colours for the Seven Vessel Goner Souls ─────────────────────
        private static readonly Color[] SoulColors = new Color[]
        {
            Calc.HexToColor("ff0000"), // Determination
            Calc.HexToColor("ff8000"), // Bravery
            Calc.HexToColor("ffff00"), // Justice
            Calc.HexToColor("00ff00"), // Kindness
            Calc.HexToColor("00ffff"), // Patience
            Calc.HexToColor("0000ff"), // Integrity
            Calc.HexToColor("ff00ff"), // Perseverance
        };

        // ── Zero form aura colours (per form, in defeat order) ───────────────
        private static readonly Color[] ZeroAuraColors = new Color[]
        {
            Calc.HexToColor("8800ff"), // Siamo Zero
            Calc.HexToColor("0044ff"), // Zero 3
            Calc.HexToColor("ff0044"), // Contra Void
            Calc.HexToColor("00ffcc"), // Tesseract Soul
            Calc.HexToColor("888800"), // Hyper Meta Morpho Knight
            Calc.HexToColor("440044"), // Nollus Nova
        };

        private static readonly string[] ZeroFormNames = new string[]
        {
            "Siamo Zero", "Zero 3", "Contra Void",
            "Tesseract Soul", "Hyper Meta Morpho", "Nollus Nova"
        };

        // ── EntityData configuration ──────────────────────────────────────────
        private readonly int    totalZeroForms;
        private readonly float  healthPerZeroForm;
        private readonly bool   hardMode;
        private readonly float  warpStarBobAmplitude;
        private readonly float  warpStarBobSpeed;
        private readonly float  warpStarRideDuration;
        private readonly float  warpStarFlySpeed;
        private readonly float  phase2ScrollSpeed;
        private readonly float  allyFormationOffsetX;
        private readonly float  allyFormationOffsetY;
        private readonly float  allySpacing;
        private readonly string activeAlliesRaw;
        // Both phases share one FMOD event driven by two parameters:
        //   "progress"     – 0..1 continuous progression through the track
        //   "finale_pitch" – 0..2 intensity/pitch modifier
        private const  string  FINALE_EVENT = "event:/DZ/new_content/music/lvl21/finale";
        private readonly string victoryMusicEvent;
        private readonly string completionFlag;
        private readonly string progressKey;

        // ── Session keys for the death-resume checkpoint ─────────────────────
        private string StartedFlag    => progressKey + "_started";
        private string PhaseCounter   => progressKey + "_phase";
        private string ZeroCounter    => progressKey + "_zeros";
        // "Have these lines already been read?" — so a death mid-ride doesn't
        // force the player back through five textboxes on every retry.
        private string Phase1SeenFlag => progressKey + "_p1_seen";
        private string Phase2SeenFlag => progressKey + "_p2_seen";

        // ── Events ────────────────────────────────────────────────────────────
        /// <summary>Fired whenever SetPhase is called. WarpStarReturnController subscribes to this.</summary>
        public event Action<BattlePhase>? OnPhaseChanged;

        // ── Runtime state ─────────────────────────────────────────────────────
        public  BattlePhase CurrentPhase { get; private set; } = BattlePhase.Idle;
        private Level  level = null!;
        private Player? player;

        // Zero wave tracking
        private int   zeroFormsDefeated;
        private int   activeZeroIndex = -1;
        private float[] zeroHealth = Array.Empty<float>();
        private float   zeroMaxHealth;
        private SiamoZeroFinalBoss? activeZeroBoss;

        // Warp star ride — full player controller (freeze + free-flight input)
        private WarpStarRideController? rideController;
        private RidingWarpStar?         ridingStar;
        private float warpStarFlyTimer;

        // Phase 2 colour shift
        private int   phase2ColorIndex;
        private float phase2ColorLerp;

        // ── FMOD parameter state ─────────────────────────────────────────────
        // "progress"  — three stable values separated by two sweeps:
        //   0.00  Phase 1 loop  (held while zeros fight + warp star + void)
        //   0→2   sweep during Phase 1 end / Phase 2 begin transition
        //   2.00  Phase 2 loop  (held during the side-scroll approach)
        //   2→3   sweep at battle end / victory
        // "finale_pitch"  1.0 rest → rises with zero defeats and phase shifts → 2.0 max
        private float musicProgress;       // actual value pushed to FMOD each frame
        private float musicProgressTarget; // what we are lerping toward
        private float musicPitch;          // actual "finale_pitch" value
        private float musicPitchTarget;    // target for pitch
        private bool  finaleStarted;

        // Soul lights (components added to this entity)
        private readonly List<VertexLight> soulLights = new List<VertexLight>(7);

        // Ally images (components added to this entity)
        private readonly List<AllySlot> allySlots = new List<AllySlot>();
        private float   allySineTimer;
        private float   allyMasterAlpha = 1f;

        // Backdrop references (found at Awake time)
        private AbyssmentBackdrop?          abyssment;
        private FlyingBattleScrollBackdrop? scrollBd;

        // Arena profiles ("Conquered Peak, modified"). Spawned here so any boss
        // in the room can reshape the battleground from its attack sequence.
        // Dormant until something applies a profile — see BattleArenaDirector.
        private BattleArenaDirector? arenaDirector;
        private bool ArenaDirected => arenaDirector?.HasActiveProfile ?? false;

        /// <summary>Arena profile per HP zone, parallel to <see cref="PhaseColors"/>.</summary>
        private static readonly string[] Phase2ArenaProfiles =
        {
            "default",  // 0 – full HP
            "wide",     // 1 – 75 %
            "tall",     // 2 – 50 %
            "rush",     // 3 – 25 %
            "collapse", // 4 – near-death
        };

        // ── Zero wave definition ──────────────────────────────────────────────
        private struct ZeroWaveConfig
        {
            public string Variant;
            public string Tier;
            public string AttackSequence;
            public string DialogKey;
        }

        // One entry per form, in defeat order (matches ZeroFormNames / ZeroAuraColors)
        //
        // The "#profile" suffixes hand each attack an arena to land in — see
        // BattleArenaDirector for the palette. The arc across the six waves runs
        // open → constricting, ending with Nollus Nova fighting inside "collapse".
        // These are tuning values: edit the strings, no rebuild of anything else.
        private static readonly ZeroWaveConfig[] DefaultZeroWaves = new ZeroWaveConfig[]
        {
            new ZeroWaveConfig {
                Variant         = "zero",
                Tier            = "soulBlack",
                AttackSequence  = "CrescentBeamShot#wide,EnergySwordCombo,TornadoSlash#tight,RevolutionSword,RisingSpine#wide",
                DialogKey       = "DZ_CH21_ZERO_WAVE_SIAMO_ZERO" },
            new ZeroWaveConfig {
                Variant         = "delta",
                Tier            = "soulBlack",
                AttackSequence  = "CrescentBeamShot#drift,ConqueredPeakCascade#tall,TornadoSlash#tight,EnergyShower#wide",
                DialogKey       = "DZ_CH21_ZERO_WAVE_ZERO_3" },
            new ZeroWaveConfig {
                Variant         = "zero",
                Tier            = "soulBlack",
                AttackSequence  = "EnergySwordCombo#tight,VortexStrike,DoubleSideSlash#rush,EnergyShower#wide",
                DialogKey       = "DZ_CH21_ZERO_WAVE_CONTRA_VOID" },
            new ZeroWaveConfig {
                Variant         = "celestial",
                Tier            = "stellarruss",
                AttackSequence  = "RevolutionSword#wide,ConqueredPeakCascade#tall,EnergyShower,TimeborderCollapse#collapse",
                DialogKey       = "DZ_CH21_ZERO_WAVE_TESSERACT_SOUL" },
            new ZeroWaveConfig {
                Variant         = "delta",
                Tier            = "soulBlack",
                AttackSequence  = "MorphoEmerge#drift,DoubleSideSlash#rush,TornadoSlash#tight,RevolutionSword#wide",
                DialogKey       = "DZ_CH21_ZERO_WAVE_HYPER_META_MORPHO" },
            new ZeroWaveConfig {
                Variant         = "celestial",
                Tier            = "stellarruss",
                AttackSequence  = "TimeborderCollapse#collapse,MorphoEmerge#drift,ConqueredPeakCascade#tall,CrescentBeamShot#rush,VortexStrike#collapse",
                DialogKey       = "DZ_CH21_ZERO_WAVE_NOLLUS_NOVA" },
        };

        // ── Ally slot ─────────────────────────────────────────────────────────
        private struct AllySlot
        {
            public Image Img;
            public float SinePhase;
            public Color BaseColor;
        }

        // ── Ally texture map ──────────────────────────────────────────────────
        private static readonly Dictionary<string, string> AllyPaths =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Kirby",      "objects/DZ/DZ/chars/kirby_dummy"     },
                { "Madeline",   "objects/DZ/DZ/chars/madeline_npc"    },
                { "Badeline",   "objects/DZ/DZ/chars/badeline_npc"    },
                { "Asriel",     "objects/DZ/DZ/chars/asriel_npc"      },
                { "Magolor",    "objects/DZ/DZ/chars/magolor_npc"      },
                { "BandanaDee", "objects/DZ/DZ/chars/bandana_dee_npc" },
                { "Marx",       "objects/DZ/DZ/chars/marx_npc"         },
                { "Gooey",      "objects/DZ/DZ/chars/gooey_npc"        },
                { "Susie",      "objects/DZ/DZ/chars/susie_npc"        },
            };

        private static readonly Dictionary<string, Color> AllyPlaceholderColors =
            new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
            {
                { "Kirby",      Calc.HexToColor("ff99cc") },
                { "Madeline",   Calc.HexToColor("4488ff") },
                { "Badeline",   Calc.HexToColor("9944ff") },
                { "Asriel",     Calc.HexToColor("66ff88") },
                { "Magolor",    Calc.HexToColor("aa44ff") },
                { "BandanaDee", Calc.HexToColor("ffaa44") },
                { "Marx",       Calc.HexToColor("ff4488") },
                { "Gooey",      Calc.HexToColor("44ffcc") },
                { "Susie",      Calc.HexToColor("88aaff") },
            };

        // ── Trail particle ────────────────────────────────────────────────────
        public static readonly ParticleType P_WarpTrail = new ParticleType
        {
            Color           = Calc.HexToColor("FFD700"),
            Color2          = Calc.HexToColor("FFFF00"),
            ColorMode       = ParticleType.ColorModes.Choose,
            FadeMode        = ParticleType.FadeModes.Late,
            LifeMin         = 0.3f,
            LifeMax         = 0.6f,
            SpeedMin        = 20f,
            SpeedMax        = 60f,
            DirectionRange  = MathHelper.TwoPi,
            Size            = 1f
        };

        // ── Constructor ───────────────────────────────────────────────────────
        public KirbyFinalBattleScene(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            totalZeroForms        = Math.Clamp(data.Int("totalZeroForms", 6), 1, 6);
            healthPerZeroForm     = data.Float("healthPerZeroForm", 80f);
            hardMode              = data.Bool("hardMode", false);
            warpStarBobAmplitude  = data.Float("warpStarBobAmplitude", 3f);
            warpStarBobSpeed      = data.Float("warpStarBobSpeed", 2.5f);
            warpStarRideDuration  = data.Float("warpStarRideDuration", 8f);
            warpStarFlySpeed      = data.Float("warpStarFlySpeed", 120f);
            phase2ScrollSpeed     = data.Float("phase2ScrollSpeed", 350f);
            allyFormationOffsetX  = data.Float("allyFormationOffsetX", -40f);
            allyFormationOffsetY  = data.Float("allyFormationOffsetY", -20f);
            allySpacing           = data.Float("allySpacing", 26f);
            activeAlliesRaw       = data.Attr("activeAllies",
                "Kirby,Madeline,Badeline,Asriel,Magolor,BandanaDee,Marx,Gooey,Susie");
            victoryMusicEvent     = data.Attr("victoryMusicEvent",
                "event:/DZ/new_content/music/lvl21/victory");
            completionFlag        = data.Attr("completionFlag",
                "ch21_els_termina_final_battle_done");
            progressKey           = data.Attr("progressKey", "ch21_final_battle");

            zeroMaxHealth = healthPerZeroForm * (hardMode ? 1.5f : 1f);
            Depth = -10000;
            Tag   = Tags.Persistent;
        }

        // ── Static accessor ───────────────────────────────────────────────────
        public static KirbyFinalBattleScene? Get(Scene scene) =>
            scene.Tracker.GetEntity<KirbyFinalBattleScene>();

        // ── Added / Awake ─────────────────────────────────────────────────────
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level  = SceneAs<Level>();
            player = scene.Tracker.GetEntity<Player>();

            // Pre-allocate Zero HP array
            zeroHealth = new float[totalZeroForms];
            for (int i = 0; i < totalZeroForms; i++)
                zeroHealth[i] = zeroMaxHealth;

            BuildAllySlots();

            // One director per room, shared by every boss in it.
            arenaDirector = scene.Tracker.GetEntity<BattleArenaDirector>();
            if (arenaDirector == null)
            {
                arenaDirector = new BattleArenaDirector();
                scene.Add(arenaDirector);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player   ??= scene.Tracker.GetEntity<Player>();
            abyssment  = level.Background.Get<AbyssmentBackdrop>();
            scrollBd   = level.Background.Get<FlyingBattleScrollBackdrop>();

            TryResume();
        }

        // ── Public API — called from EventTrigger / cutscenes / triggers ──────

        /// <summary>Start the full battle sequence. Called via EventTrigger "ch21_final_battle_start".</summary>
        public void StartBattle()
        {
            if (CurrentPhase != BattlePhase.Idle) return;

            // Fresh run — wipe any checkpoint left over from a previous attempt.
            ClearProgress();
            level.Session.SetFlag(StartedFlag);
            Add(new Coroutine(BattleSequence()));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  DEATH RESUME
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Picks the battle back up after a death reloaded the room. Does nothing
        /// on a first entry, or once the battle has been completed.
        /// </summary>
        private void TryResume()
        {
            if (CurrentPhase != BattlePhase.Idle)          return;
            if (!level.Session.GetFlag(StartedFlag))       return;
            if (level.Session.GetFlag(completionFlag))     return;

            zeroFormsDefeated = Math.Clamp(
                level.Session.GetCounter(ZeroCounter), 0, totalZeroForms);

            BattlePhase resume = ResumePhaseFor(level.Session.GetCounter(PhaseCounter));
            if (resume == BattlePhase.Idle) return;

            Add(new Coroutine(BattleSequence(resume)));
        }

        /// <summary>
        /// Maps a checkpointed phase onto the phase the sequence should restart
        /// from. Void flight rewinds to the Warp Star launch — there is no clean
        /// mid-void entry point, and replaying the launch reads better than
        /// materialising in the middle of nowhere.
        /// </summary>
        private static BattlePhase ResumePhaseFor(int savedPhase) =>
            (BattlePhase)savedPhase switch
            {
                BattlePhase.PreBattle    => BattlePhase.PreBattle,
                BattlePhase.WarpStarRide => BattlePhase.WarpStarRide,
                BattlePhase.FlyingVoid   => BattlePhase.WarpStarRide,
                BattlePhase.Phase2Scroll => BattlePhase.Phase2Scroll,
                _                        => BattlePhase.Idle,
            };

        private void SaveProgress()
        {
            if (level == null) return;
            level.Session.SetCounter(PhaseCounter, (int)CurrentPhase);
            level.Session.SetCounter(ZeroCounter,  zeroFormsDefeated);
        }

        /// <summary>Wipes the resume checkpoint. Called on a fresh start and on victory.</summary>
        public void ClearProgress()
        {
            if (level == null) return;
            level.Session.SetFlag(StartedFlag,    false);
            level.Session.SetFlag(Phase1SeenFlag, false);
            level.Session.SetFlag(Phase2SeenFlag, false);
            level.Session.SetCounter(PhaseCounter, 0);
            level.Session.SetCounter(ZeroCounter,  0);
        }

        /// <summary>
        /// Textbox that only plays the first time through. On a death-resume the
        /// wait is kept so pacing survives, but the dialogue is skipped.
        /// </summary>
        private static IEnumerator SayOnce(string key, bool alreadySeen)
        {
            if (alreadySeen)
            {
                yield return 0.3f;
                yield break;
            }
            yield return Textbox.Say(key);
        }

        /// <summary>
        /// Deal damage to the currently active Zero form.
        /// Wire this to the player's dash/attack collision in the boss entity
        /// or call it from a custom PlayerCollider.
        /// </summary>
        public void DamageActiveZero(float amount)
        {
            if (activeZeroIndex < 0 || activeZeroIndex >= totalZeroForms) return;
            zeroHealth[activeZeroIndex] -= amount;
            if (zeroHealth[activeZeroIndex] <= 0f)
            {
                zeroHealth[activeZeroIndex] = 0f;
                Add(new Coroutine(ZeroDefeatedRoutine(activeZeroIndex)));
            }
        }

        /// <summary>
        /// Called by FinalBattlePhaseShiftTrigger when the player enters an HP-zone
        /// boundary during Phase 2.
        /// </summary>
        public void NotifyPhaseShift(int phaseIndex, float scrollBoost)
        {
            phase2ColorIndex = Math.Clamp(phaseIndex, 0, PhaseColors.Length - 1);
            phase2ColorLerp  = 0f;

            // Reshape the battleground for this HP zone. The director owns scroll
            // speed once it is live, so the manual boost only applies while it is
            // dormant — otherwise the two would fight over the same field.
            bool directed = arenaDirector?.Apply(Phase2ArenaProfiles[phase2ColorIndex]) ?? false;

            if (!directed && scrollBd != null && scrollBoost != 0f)
                scrollBd.ScrollSpeedX += scrollBoost;

            // Phase 2 holds progress at 2.00 — only pitch nudges per shift
            musicPitchTarget = Math.Min(musicPitchTarget + 0.1f, 2.0f);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ATTACK FIRE METHODS
        //  Called by the attack pattern coroutines below.
        //  Origin is always the warp star's current position == player.Center
        //  (since the player is riding the star during phases 1+2).
        // ─────────────────────────────────────────────────────────────────────

        // ── Boss-side origin helpers ──────────────────────────────────────────
        // All attacks originate from the right side of the screen (where Nightmare
        // is assumed to be), not from the player's position.

        /// <summary>
        /// Returns the spawn point for attacks: right edge of the camera view,
        /// vertically at the player's current height (or mid-screen if no player).
        /// </summary>
        private Vector2 BossOrigin()
        {
            player ??= Scene.Tracker.GetEntity<Player>();
            float camRight = level.Camera.Right + 20f;
            float aimY     = player?.Center.Y ?? (level.Camera.Top + 122f);
            return new Vector2(camRight, aimY);
        }

        /// <summary>
        /// Returns the spawn point for the downward beam: directly above the player,
        /// off the top of the screen.
        /// </summary>
        private Vector2 AbovePlayerOrigin()
        {
            player ??= Scene.Tracker.GetEntity<Player>();
            float x = player?.Center.X ?? (level.Camera.Left + 192f);
            return new Vector2(x, level.Camera.Top - 30f);
        }

        private Vector2 AimAtPlayer(Vector2 from)
        {
            var vp = Scene.Tracker.GetEntity<Player>();
            var kp = Scene.Tracker.GetEntity<K_Player>();
            Vector2 target = vp?.Center ?? kp?.Center ?? (from - Vector2.UnitX * 100f);
            return (target - from).SafeNormalize(-Vector2.UnitX);
        }

        private float AimAngleAtPlayer(Vector2 from)
        {
            var vp = Scene.Tracker.GetEntity<Player>();
            var kp = Scene.Tracker.GetEntity<K_Player>();
            Vector2 target = vp?.Center ?? kp?.Center ?? (from - Vector2.UnitX * 100f);
            return Calc.Angle(from, target);
        }

        /// <summary>Fire a small star bullet from the right edge toward the player.</summary>
        public void FireStarBullet(Vector2? overrideDir = null)
        {
            Vector2 origin = BossOrigin();
            Vector2 dir    = overrideDir ?? AimAtPlayer(origin);
            Scene.Add(new WarpStarBullet(origin, dir));
        }

        /// <summary>Fire a large slow homing star bullet from the right edge.</summary>
        public void FireBigStarBullet(Vector2? overrideDir = null)
        {
            Vector2 origin = BossOrigin();
            Vector2 dir    = overrideDir ?? AimAtPlayer(origin);
            Scene.Add(new WarpStarBigBullet(origin, dir));
        }

        /// <summary>Fire a standard angled laser from the right edge.</summary>
        public void FireLaser(float? overrideAngle = null)
        {
            Vector2 origin = BossOrigin();
            float   angle  = overrideAngle ?? AimAngleAtPlayer(origin);
            Scene.Add(new WarpLaser(origin, angle));
        }

        /// <summary>Fire a full-screen horizontal mega laser from the right edge.</summary>
        public void FireMegaLaser()
        {
            // Always fires leftward (toward the player from the right)
            Scene.Add(new WarpMegaLaser(BossOrigin(), fireRight: false));
        }

        // Cycles through all 8 orb identities in order so each volley looks different.
        // Order: Siamo Zero → Zero 3 → Contra Void → Tesseract Soul →
        //        Hyper Meta Morpho → Nollus Nova → Els Termina → Default → repeat
        private int _orbIdentityIndex;

        private static readonly NightmareOrb.Identity[] OrbIdentityCycle =
        {
            NightmareOrb.Identity.SiamoZero,
            NightmareOrb.Identity.Zero3,
            NightmareOrb.Identity.ContraVoid,
            NightmareOrb.Identity.TesseractSoul,
            NightmareOrb.Identity.HyperMetaMorpho,
            NightmareOrb.Identity.NollusNova,
            NightmareOrb.Identity.ElsTermina,
            NightmareOrb.Identity.Default,
        };

        /// <summary>
        /// Fire a Nightmare-style orb: single straight horizontal shot from the right.
        /// Each successive call cycles to the next identity (Zero form / ELS / Nightmare).
        /// </summary>
        public void FireNightmareOrb()
        {
            player ??= Scene.Tracker.GetEntity<Player>();
            float camRight = level.Camera.Right + 30f;
            float aimY     = player?.Center.Y ?? (level.Camera.Top + 122f);
            aimY += Calc.Random.Range(-20f, 20f);

            var identity = OrbIdentityCycle[_orbIdentityIndex % OrbIdentityCycle.Length];
            _orbIdentityIndex++;

            Scene.Add(new NightmareOrb(new Vector2(camRight, aimY), identity));
        }

        /// <summary>
        /// Fire the Nightmare downward beam from above the player.
        /// Used during Phase 2 as a faithful recreation of the Phase 2 attack.
        /// </summary>
        public void FireDownwardLaser()
        {
            Scene.Add(new WarpDownwardLaser(AbovePlayerOrigin()));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ATTACK PATTERN COROUTINES
        //  KirbyFinalBattleScene runs these during Phase 2 to keep the player
        //  under fire while riding toward ELS Termina.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Starts the Phase 2 attack loop.  Called internally by Phase2_SideScrollApproach
        /// after the scroll backdrop is live.  Runs until CurrentPhase != Phase2Scroll.
        /// </summary>
        private IEnumerator Phase2AttackLoop()
        {
            // Escalating pattern:
            //  0–30 s  → burst of 3 star bullets every 1.4 s
            //  30–60 s → big star bullet interspersed every 4 s
            //  60–90 s → laser every 8 s
            //  90+ s   → mega laser every 20 s + continued small bullets

            float elapsed     = 0f;
            float bulletTimer = 0f;
            float bigTimer    = 4f;
            float laserTimer  = 8f;
            float downTimer   = 10f;
            float megaTimer   = 20f;

            while (CurrentPhase == BattlePhase.Phase2Scroll)
            {
                elapsed     += Engine.DeltaTime;
                bulletTimer -= Engine.DeltaTime;
                bigTimer    -= Engine.DeltaTime;
                laserTimer  -= Engine.DeltaTime;
                downTimer   -= Engine.DeltaTime;
                megaTimer   -= Engine.DeltaTime;

                // Star bullet burst
                if (bulletTimer <= 0f)
                {
                    bulletTimer = Math.Max(0.6f, 1.4f - elapsed * 0.005f); // speeds up over time
                    yield return FireStarBurst(3);
                }

                // Big bullet
                if (elapsed > 30f && bigTimer <= 0f)
                {
                    bigTimer = 4.5f;
                    FireBigStarBullet();
                    yield return 0.1f;
                }

                // Laser
                if (elapsed > 60f && laserTimer <= 0f)
                {
                    laserTimer = 8f;
                    FireLaser();
                    yield return 0.1f;
                }

                // Downward laser (Nightmare's Phase 2 signature beam from above).
                // Needs its own timer — sharing laserTimer with the angled laser
                // above meant this branch could never be reached.
                if (elapsed > 60f && downTimer <= 0f)
                {
                    downTimer = 10f;
                    FireDownwardLaser();
                    level.Shake(0.6f);
                    yield return 0.1f;
                }

                // Mega laser (full-screen horizontal, unlocks at 90 s)
                if (elapsed > 90f && megaTimer <= 0f)
                {
                    megaTimer = 20f;
                    FireMegaLaser();
                    level.Shake(1.5f);
                    yield return 0.2f;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Phase 1 attack loop — runs as a parallel coroutine during WarpStarRide
        /// and FlyingVoid.  Fires Nightmare-style horizontal orbs from the right edge.
        /// Escalates: faster orbs + occasional spread bursts as time passes.
        /// </summary>
        private IEnumerator Phase1AttackLoop()
        {
            float elapsed  = 0f;
            float orbTimer = 2.5f; // first orb after 2.5 s so the ride has a moment to breathe

            while (CurrentPhase == BattlePhase.WarpStarRide
                || CurrentPhase == BattlePhase.FlyingVoid)
            {
                elapsed   += Engine.DeltaTime;
                orbTimer  -= Engine.DeltaTime;

                if (orbTimer <= 0f)
                {
                    if (elapsed < 10f)
                    {
                        // Early: single straight orb
                        orbTimer = 2.2f;
                        FireNightmareOrb();
                    }
                    else if (elapsed < 20f)
                    {
                        // Mid: two orbs slightly staggered
                        orbTimer = 1.8f;
                        FireNightmareOrb();
                        yield return 0.35f;
                        FireNightmareOrb();
                    }
                    else
                    {
                        // Late: three orbs + a big bullet for pressure
                        orbTimer = 1.5f;
                        FireNightmareOrb();
                        yield return 0.25f;
                        FireNightmareOrb();
                        yield return 0.25f;
                        FireNightmareOrb();
                        yield return 0.15f;
                        FireBigStarBullet();
                    }
                }

                yield return null;
            }
        }

        /// <summary>Fire a spread burst of star bullets from the right edge.</summary>
        private IEnumerator FireStarBurst(int count)
        {
            Vector2 origin = BossOrigin();
            float baseAngle = AimAngleAtPlayer(origin);
            float spreadRad = MathHelper.ToRadians(20f);

            for (int i = 0; i < count; i++)
            {
                float offset = (count > 1)
                    ? Calc.LerpClamp(-spreadRad, spreadRad, (float)i / (count - 1))
                    : 0f;
                Scene.Add(new WarpStarBullet(origin, Calc.AngleToVector(baseAngle + offset, 1f)));
                yield return 0.06f;
            }
        }

        // ── Music helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Smoothly moves musicProgressTarget (and therefore the FMOD "progress"
        /// parameter) from <paramref name="from"/> to <paramref name="to"/> over
        /// <paramref name="duration"/> seconds, then snaps it exactly to the target.
        /// </summary>
        private IEnumerator SweepProgress(float from, float to, float duration)
        {
            musicProgressTarget = from;
            // Wait one frame so the Approach in Update() starts from the right place
            yield return null;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Engine.DeltaTime;
                musicProgressTarget = MathHelper.Lerp(from, to, Math.Min(elapsed / duration, 1f));
                yield return null;
            }
            // Snap exactly so floating-point drift never misses the boundary
            musicProgressTarget = to;
            musicProgress       = to;
            Audio.SetMusicParam("progress", to);
        }

        // ── Ally formation ────────────────────────────────────────────────────
        private void BuildAllySlots()
        {
            allySlots.Clear();
            string[] names = activeAlliesRaw.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i].Trim();
                Image img;

                if (AllyPaths.TryGetValue(name, out string? path) && GFX.Game.Has(path))
                {
                    img = new Image(GFX.Game[path]);
                    img.CenterOrigin();
                }
                else
                {
                    img = new Image(GFX.Game["util/pixel"]);
                    img.Scale = new Vector2(10f, 14f);
                    img.Origin = new Vector2(0.5f, 0.5f);
                }

                Add(img);
                AllyPlaceholderColors.TryGetValue(name, out Color pc);
                if (pc == default) pc = Color.White;

                allySlots.Add(new AllySlot
                {
                    Img       = img,
                    SinePhase = i * (MathHelper.TwoPi / Math.Max(names.Length, 1)),
                    BaseColor = pc
                });
            }
        }

        private IEnumerator FadeAllies(float target, float duration)
        {
            float start = allyMasterAlpha;
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                allyMasterAlpha = MathHelper.Lerp(start, target, t / duration);
                ApplyAllyAlpha();
                yield return null;
            }
            allyMasterAlpha = target;
            ApplyAllyAlpha();
        }

        private void ApplyAllyAlpha()
        {
            foreach (var s in allySlots)
                s.Img.Color = s.BaseColor * allyMasterAlpha;
        }

        // ── Main battle coroutine ─────────────────────────────────────────────
        /// <param name="resumeFrom">
        /// BattlePhase.Idle for a fresh run; anything else re-enters the sequence
        /// at that phase after a death. See TryResume.
        /// </param>
        private IEnumerator BattleSequence(BattlePhase resumeFrom = BattlePhase.Idle)
        {
            bool fresh = resumeFrom == BattlePhase.Idle;

            // Only claim PreBattle when the Zero waves are actually going to run.
            // Phase 1 and Phase 2 set their own phases, and announcing PreBattle
            // on a late resume would rewind the checkpoint we just read.
            if (resumeFrom <= BattlePhase.PreBattle)
                SetPhase(BattlePhase.PreBattle);

            if (fresh)
            {
                // ── PRE-PHASE: allies arrive ──────────────────────────────────
                yield return PrePhase_AlliesArrive();
            }
            else
            {
                // On a resume the allies are already here and the intro has been
                // seen — restore the state silently rather than replaying it.
                RestoreSceneAfterDeath(resumeFrom);
            }

            if (resumeFrom <= BattlePhase.PreBattle)
            {
                // ── PRE-PHASE: defeat all Zero forms ─────────────────────────
                // Skips forms already killed before the death.
                yield return PrePhase_ZeroWaves(skipIntro: !fresh);

                // ── TRANSITION: zeros all defeated ───────────────────────────
                yield return ZerosDefeatedTransition();
            }

            if (resumeFrom <= BattlePhase.FlyingVoid)
            {
                // ── PHASE 1: Warp Star launch + void flight ───────────────────
                yield return Phase1_WarpStarAndVoid();
            }

            // ── PHASE 2: side-scroll approach → arena ────────────────────────
            yield return Phase2_SideScrollApproach();

            // ── PHASE 2 end: sweep progress 2 → 3 to close the music event ────
            yield return SweepProgress(2f, 3f, duration: 3.0f);

            // Battle hands off to ELSTerminaFinalBoss which is already in the room.
            // Set flag so the cutscene system knows we've arrived.
            level.Session.SetFlag(completionFlag);
            ClearProgress();
        }

        /// <summary>
        /// Rebuilds the presentation state a death threw away — music, ally
        /// cluster, soul lights — without replaying any dialogue.
        /// </summary>
        private void RestoreSceneAfterDeath(BattlePhase resumeFrom)
        {
            StartFinaleMusic();

            if (resumeFrom >= BattlePhase.Phase2Scroll)
            {
                // The allies stayed behind at the start of Phase 2.
                allyMasterAlpha = 0f;
                musicProgress = musicProgressTarget = 2f;
                musicPitch    = musicPitchTarget    = 2f;
            }
            else
            {
                allyMasterAlpha = 1f;
                musicPitchTarget = Math.Min(1f + zeroFormsDefeated * 0.08f, 1.5f);
                musicPitch       = musicPitchTarget;
                SpawnSoulLightsInstant();
            }

            ApplyAllyAlpha();
            Audio.SetMusicParam("progress",     musicProgress);
            Audio.SetMusicParam("finale_pitch", musicPitch);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PRE-PHASE: ALLIES ARRIVE
        // ─────────────────────────────────────────────────────────────────────
        private IEnumerator PrePhase_AlliesArrive()
        {
            // Start the shared finale event at the very beginning (progress=0, pitch=1)
            StartFinaleMusic();
            musicProgress      = 0f;
            musicProgressTarget = 0f;
            musicPitch         = 1f;
            musicPitchTarget   = 1f;
            Audio.SetMusicParam("progress",     musicProgress);
            Audio.SetMusicParam("finale_pitch", musicPitch);

            // Fade in ally cluster
            allyMasterAlpha = 0f;
            ApplyAllyAlpha();
            yield return FadeAllies(1f, 1.2f);

            // Flash + dialogue
            level.Flash(Color.Black * 0.6f, false);
            yield return 0.3f;
            yield return Textbox.Say("DZ_CH21_FINAL_BATTLE_ALLIES_ARRIVE");
            yield return 0.2f;

            // Spawn the Seven Vessel Goner Soul lights
            yield return SpawnSoulLights();
            yield return Textbox.Say("DZ_CH21_FINAL_BATTLE_DREAM_FRIENDS");
            yield return 0.4f;
        }

        /// <summary>Starts (or restarts) the shared finale FMOD event.</summary>
        private void StartFinaleMusic()
        {
            Audio.SetMusic(FINALE_EVENT);
            finaleStarted = true;
        }

        /// <summary>
        /// Re-creates the Seven Vessel Goner Soul lights with no dialogue, flash
        /// or SFX. Used by the death resume, where replaying the ceremony would
        /// be tedious.
        /// </summary>
        private void SpawnSoulLightsInstant()
        {
            foreach (VertexLight existing in soulLights) Remove(existing);
            soulLights.Clear();

            Actor? rider = WarpStarRideController.ResolveRider(Scene);
            Vector2 basePos = (rider?.Center ?? Position) + new Vector2(-100f, -30f);

            for (int i = 0; i < 7; i++)
            {
                float angle = MathHelper.TwoPi * i / 7f;
                var light = new VertexLight(SoulColors[i], 1f, 24, 64)
                {
                    Position = basePos + new Vector2(
                        (float)Math.Cos(angle) * 70f,
                        (float)Math.Sin(angle) * 35f)
                };
                Add(light);
                soulLights.Add(light);
            }
        }

        private IEnumerator SpawnSoulLights()
        {
            soulLights.Clear();
            player ??= Scene.Tracker.GetEntity<Player>();
            Vector2 basePos = (player?.Center ?? Position) + new Vector2(-100f, -30f);

            for (int i = 0; i < 7; i++)
            {
                float angle  = MathHelper.TwoPi * i / 7f;
                var light    = new VertexLight(SoulColors[i], 1f, 24, 64)
                {
                    Position = basePos + new Vector2(
                        (float)Math.Cos(angle) * 70f,
                        (float)Math.Sin(angle) * 35f)
                };
                Add(light);
                soulLights.Add(light);
                Audio.Play("event:/new_content/char/DZ/asriel/Asriel_Create", light.Position);
                level.Flash(SoulColors[i] * 0.2f, false);
                yield return 0.1f;
            }
            yield return 0.4f;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PRE-PHASE: ZERO WAVES
        // ─────────────────────────────────────────────────────────────────────
        private IEnumerator PrePhase_ZeroWaves(bool skipIntro = false)
        {
            if (!skipIntro)
            {
                level.Shake(0.4f);
                Glitch.Value = 0.35f;
                yield return 0.2f;
                Glitch.Value = 0f;

                yield return Textbox.Say("DZ_CH21_FINAL_BATTLE_ZEROS_INTRO");
                yield return Textbox.Say("DZ_CH21_FINAL_BATTLE_GONERS_HOLD");
            }

            // Start at the first form still standing, so a death mid-wave does not
            // resurrect the ones already beaten.
            for (int i = zeroFormsDefeated; i < totalZeroForms; i++)
            {
                activeZeroIndex = i;

                // Spawn the boss for this wave
                yield return SpawnZeroWave(i);

                // Wait until DamageActiveZero advances zeroFormsDefeated past this index
                while (zeroFormsDefeated <= i)
                    yield return null;

                // Brief breather between waves
                if (i + 1 < totalZeroForms)
                    yield return 0.8f;
            }

            activeZeroIndex = -1;
        }

        private IEnumerator SpawnZeroWave(int index)
        {
            ZeroWaveConfig cfg = index < DefaultZeroWaves.Length
                ? DefaultZeroWaves[index]
                : DefaultZeroWaves[0];

            // Pre-spawn flash in the Zero's aura colour
            Color aura = index < ZeroAuraColors.Length ? ZeroAuraColors[index] : Color.White;
            level.Flash(aura * 0.4f, true);
            level.Shake(0.5f);
            Audio.Play("event:/DZ/new_content/char/els/spawn", Position);
            yield return 0.3f;

            // Optional per-wave intro dialogue
            if (!string.IsNullOrEmpty(cfg.DialogKey))
                yield return Textbox.Say(cfg.DialogKey);

            // Compute spawn position: top-right quadrant of camera view
            player ??= Scene.Tracker.GetEntity<Player>();
            float spawnX = level.Camera.Right  - 60f;
            float spawnY = (player?.Center.Y ?? (level.Camera.Top + 80f)) - 40f;
            Vector2 spawnPos = new Vector2(spawnX, spawnY);

            // Build a minimal EntityData-equivalent via the direct constructor overload
            var boss = new SiamoZeroFinalBoss(
                position:        spawnPos,
                nodes:           new Vector2[] { new Vector2(spawnX - 120f, spawnY) },
                patternIndex:    0,
                dialog:          false,
                startHit:        true,
                attackSequence:  cfg.AttackSequence,
                siamoTier:       cfg.Tier,
                siamoVariant:    cfg.Variant);

            activeZeroBoss = boss;
            Scene.Add(boss);
        }

        private IEnumerator ZeroDefeatedRoutine(int formIndex)
        {
            Color aura = formIndex < ZeroAuraColors.Length
                ? ZeroAuraColors[formIndex] : Color.White;

            // Visual burst
            level.Shake(0.8f);
            level.Flash(aura * 0.55f, true);
            Audio.Play("event:/DZ/new_content/game/21_desolo_zantas/zero_shatter");
            level.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 32,
                level.Camera.Position + new Vector2(192f, 122f), Vector2.One * 40f);
            level.Displacement.AddBurst(
                level.Camera.Position + new Vector2(192f, 122f), 3f, 64f, 256f, 1.5f);

            Glitch.Value = 0.25f;
            yield return 0.15f;
            Glitch.Value = 0f;

            // Remove the defeated boss entity (deferred to next frame to avoid mid-Update list mutation)
            if (activeZeroBoss != null)
            {
                activeZeroBoss.RemoveSelf();
                activeZeroBoss = null;
            }

            zeroFormsDefeated++;
            SaveProgress();

            // Progress stays at 0.00 throughout Phase 1 — only pitch rises per kill
            musicPitchTarget = Math.Min(1f + zeroFormsDefeated * 0.08f, 1.5f);

            // Pulse soul lights
            foreach (var light in soulLights) light.Alpha = 2f;
            yield return 0.1f;
            foreach (var light in soulLights) light.Alpha = 1f;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TRANSITION: ZEROS DEFEATED
        // ─────────────────────────────────────────────────────────────────────
        private IEnumerator ZerosDefeatedTransition()
        {
            // Rainbow burst through all soul colours
            foreach (var c in SoulColors)
            {
                level.Flash(c * 0.35f, false);
                yield return 0.05f;
            }
            level.Shake(1.2f);
            level.Flash(Color.White, true);
            yield return 0.7f;

            yield return Textbox.Say("DZ_CH21_FINAL_BATTLE_ZEROS_FALLEN");
            yield return 0.4f;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PHASE 1: WARP STAR + VOID FLIGHT
        // ─────────────────────────────────────────────────────────────────────
        private IEnumerator Phase1_WarpStarAndVoid()
        {
            bool seen = level.Session.GetFlag(Phase1SeenFlag);
            level.Session.SetFlag(Phase1SeenFlag);

            // ── progress stays at 0.00 the entire Phase 1 ────────────────────
            // Only pitch rises: warp star launch surges it to 1.6
            musicPitchTarget = 1.6f;

            level.Shake(1.0f);
            Audio.Play("event:/DZ/new_content/game/21_desolo_zantas/warpstar_launch");
            level.Flash(Calc.HexToColor("FFD700") * 0.8f, true);
            yield return SayOnce("DZ_CH21_FINAL_BATTLE_WARPSTAR_RIDE", seen);

            // ── Spawn visible Warp Star under the player ──────────────────────
            // ResolveRider prefers K_Player (Kirby) when it is active: Kirby keeps
            // a hidden vanilla Player "shadow" purely for API compatibility, so
            // asking the Tracker for Player first would grab that non-controlling
            // shadow and every write to it would be silently discarded.
            player ??= Scene.Tracker.GetEntity<Player>();
            Actor? activePlayer = WarpStarRideController.ResolveRider(Scene);
            ridingStar = new RidingWarpStar(activePlayer?.Center ?? Position);
            Scene.Add(ridingStar);

            // ── Full player controller: freezes physics, accepts free-flight
            //    input within a safe local arena for the entire ride + void ──
            // Created unconditionally — it resolves (and re-resolves) the rider
            // itself, so a character swap mid-ride does not leave it inert.
            rideController = new WarpStarRideController(
                activePlayer, warpStarFlySpeed, warpStarBobAmplitude, warpStarBobSpeed);
            Scene.Add(rideController);

            // ── Enable AbyssmentBackdrop rightward scroll for the ride ─────────
            // This sells the sense of flying through space toward Nightmare.
            if (abyssment != null) abyssment.ScrollSpeed = 220f;

            SetPhase(BattlePhase.WarpStarRide);

            // ── Phase 1 attack loop — parallel coroutine, runs through ride+void ─
            Add(new Coroutine(Phase1AttackLoop()));

            // Warp Star bob runs in Update() for warpStarRideDuration seconds
            // progress = 0.00 throughout; pitch nudges to 1.8 mid-ride
            float elapsed = 0f;
            while (elapsed < warpStarRideDuration)
            {
                elapsed += Engine.DeltaTime;
                if (elapsed > warpStarRideDuration * 0.5f && musicPitchTarget < 1.8f)
                    musicPitchTarget = 1.8f;
                yield return null;
            }

            // ── Warp Star ride ends ────────────────────────────────────────────
            // The star itself stays: the player is still riding it through the
            // void. Both it and the controller are torn down together when
            // Phase 2 hands control back.

            // Switch to void flight — still progress = 0.00
            // Slow the backdrop scroll down for the drifting void feeling
            if (abyssment != null)
            {
                abyssment.Cycling     = true;
                abyssment.ScrollSpeed = 80f; // slower drift in the void
            }
            SetPhase(BattlePhase.FlyingVoid);

            yield return SayOnce("DZ_CH21_FINAL_BATTLE_INTO_ABYSS", seen);

            // Abyssment colour cycle — 5 seconds, progress still 0.00
            yield return 5f;

            Audio.Play("event:/DZ/new_content/game/21_desolo_zantas/falling_into_the_void");
            level.Shake(0.4f);
            yield return 0.5f;

            // Asriel joins
            level.Flash(Calc.HexToColor("ff6699") * 0.45f, false);
            Audio.Play("event:/new_content/char/DZ/asriel/Asriel_Create");
            yield return 0.25f;
            yield return SayOnce("DZ_CH21_FINAL_BATTLE_ASRIEL_JOINS", seen);

            // Determination Soul flare
            level.Flash(Calc.HexToColor("ff0000") * 0.4f, false);
            yield return 0.2f;
            yield return SayOnce("DZ_CH21_FINAL_BATTLE_DETERMINATION_SOUL", seen);

            // Els Termina silhouette
            yield return 0.4f;
            level.Flash(Color.Black * 0.65f, false);
            yield return 0.25f;
            yield return SayOnce("DZ_CH21_FINAL_BATTLE_ELS_LOOMS", seen);
            yield return 0.5f;

            // ── Phase 1 end: sweep progress 0 → 2 before Phase 2 takes over ──
            yield return SweepProgress(0f, 2f, duration: 2.0f);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PHASE 2: SIDE-SCROLL APPROACH → ARENA
        // ─────────────────────────────────────────────────────────────────────
        private IEnumerator Phase2_SideScrollApproach()
        {
            bool seen = level.Session.GetFlag(Phase2SeenFlag);
            level.Session.SetFlag(Phase2SeenFlag);

            SetPhase(BattlePhase.Phase2Scroll);
            // progress = 2.00 for the entire Phase 2 loop; pitch maxes out at 2.0
            musicProgressTarget = 2.0f;
            musicPitchTarget    = 2.0f;

            // Stop Abyssment; start scroll backdrop
            if (abyssment != null) abyssment.Cycling = false;

            if (scrollBd != null)
            {
                scrollBd.Active      = true;
                scrollBd.ScrollSpeedX = phase2ScrollSpeed;
            }

            // Hand control back to the player (ground is back)
            player ??= Scene.Tracker.GetEntity<Player>();
            rideController?.RemoveSelf();   // Removed() restores normal control
            rideController = null;
            ridingStar?.RemoveSelf();
            ridingStar = null;

            // Ramp scroll speed visually over 2 s — skipped once an arena profile
            // is driving, since the director cross-fades the speed itself.
            if (scrollBd != null && !ArenaDirected)
            {
                float target = phase2ScrollSpeed;
                scrollBd.ScrollSpeedX = 0f;
                for (float t = 0f; t < 2f; t += Engine.DeltaTime)
                {
                    scrollBd.ScrollSpeedX = Ease.SineIn(t / 2f) * target;
                    yield return null;
                }
                scrollBd.ScrollSpeedX = target;
            }

            level.Flash(Color.White * 0.9f, true);
            level.Shake(1.5f);

            // Fade out allies — they stay behind
            Add(new Coroutine(FadeAllies(0f, 1.5f)));

            yield return SayOnce("DZ_CH21_FINAL_BATTLE_PHASE2_ARRIVAL", seen);

            // Kick off the attack loop — runs as a parallel coroutine for the
            // full duration of Phase2Scroll so it stops automatically when the
            // phase transitions out.
            Add(new Coroutine(Phase2AttackLoop()));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  UPDATE
        // ─────────────────────────────────────────────────────────────────────
        public override void Update()
        {
            base.Update();

            player ??= Scene.Tracker.GetEntity<Player>();

            // Deliberately no early-out on a missing vanilla Player. When Kirby
            // is the active character the rider is a K_Player, and bailing here
            // froze the music parameters, the backdrop colour push and the ally
            // formation for the whole battle.
            Actor? rider = WarpStarRideController.ResolveRider(Scene);

            // ── Drive FMOD parameters every frame ──────────────────────────────
            if (finaleStarted)
            {
                // Smoothly chase targets so the music morphs rather than jumps
                musicProgress = Calc.Approach(musicProgress, musicProgressTarget, Engine.DeltaTime * 0.12f);
                musicPitch    = Calc.Approach(musicPitch,    musicPitchTarget,    Engine.DeltaTime * 0.6f);
                Audio.SetMusicParam("progress",     musicProgress);
                Audio.SetMusicParam("finale_pitch", musicPitch);
            }

            // Simulated forward-flight feel via backdrop scroll speed.
            // Player freeze/input/bob/trail are fully owned by WarpStarRideController.
            // Once an arena profile is live the director owns the backdrop speeds.
            bool directed = ArenaDirected;

            if (CurrentPhase == BattlePhase.WarpStarRide)
            {
                warpStarFlyTimer += Engine.DeltaTime;
                float flyRamp = Math.Min(warpStarFlyTimer / 1.5f, 1f); // ramp up over 1.5s
                if (abyssment != null && !directed)
                    abyssment.ScrollSpeed = 220f + warpStarFlySpeed * flyRamp;
            }
            else if (CurrentPhase == BattlePhase.FlyingVoid)
            {
                warpStarFlyTimer += Engine.DeltaTime;
                if (abyssment != null && !directed)
                    abyssment.ScrollSpeed = 80f + warpStarFlySpeed * 0.5f;
            }
            else
            {
                warpStarFlyTimer = 0f;
            }

            // Phase 2 colour push to scroll backdrop
            if (CurrentPhase == BattlePhase.Phase2Scroll && scrollBd != null)
            {
                int   next = (phase2ColorIndex + 1) % PhaseColors.Length;
                Color tint = Color.Lerp(PhaseColors[phase2ColorIndex], PhaseColors[next], phase2ColorLerp);
                scrollBd.SetPhaseColor(tint, 0.18f);
                phase2ColorLerp = Math.Min(phase2ColorLerp + Engine.DeltaTime * 0.25f, 1f);
            }

            // Ally formation bob
            if (allyMasterAlpha > 0f && allySlots.Count > 0 && rider != null)
            {
                allySineTimer += Engine.DeltaTime * 1.8f;
                Vector2 centre = rider.Center +
                    new Vector2(allyFormationOffsetX, allyFormationOffsetY);
                float totalW = (allySlots.Count - 1) * allySpacing;
                float startX = centre.X - totalW * 0.5f;

                for (int i = 0; i < allySlots.Count; i++)
                {
                    float bobY = (float)Math.Sin(allySineTimer + allySlots[i].SinePhase) * 4f;
                    allySlots[i].Img.Position = new Vector2(startX + i * allySpacing, centre.Y + bobY);
                    // Face player
                    float sx = allySlots[i].Img.Scale.X;
                    allySlots[i].Img.Scale = new Vector2(
                        Math.Abs(sx) * (rider.X < allySlots[i].Img.Position.X ? -1f : 1f),
                        allySlots[i].Img.Scale.Y);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  RENDER  — Zero form HUD bar
        // ─────────────────────────────────────────────────────────────────────
        public override void Render()
        {
            base.Render();

            if (activeZeroIndex < 0 || activeZeroIndex >= totalZeroForms) return;
            if (CurrentPhase != BattlePhase.PreBattle) return;

            float hp      = zeroHealth[activeZeroIndex];
            float pct     = Math.Clamp(hp / zeroMaxHealth, 0f, 1f);
            Color aura    = activeZeroIndex < ZeroAuraColors.Length
                ? ZeroAuraColors[activeZeroIndex] : Color.White;
            string name   = activeZeroIndex < ZeroFormNames.Length
                ? ZeroFormNames[activeZeroIndex] : "???";

            const float barW = 300f, barH = 8f, barY = 12f;
            float screenW = Engine.Width;
            float barLeft = (screenW - barW) * 0.5f;
            float fillW   = barW * pct;

            // Background + fill + outline
            Draw.Rect(barLeft - 1f, barY - 1f, barW + 2f, barH + 2f, Color.Black * 0.8f);
            if (fillW > 0f) Draw.Rect(barLeft, barY, fillW, barH, aura);
            Draw.HollowRect(barLeft, barY, barW, barH, aura * 0.9f);

            // Name label (0.5× scale)
            Vector2 sz = Draw.DefaultFont.MeasureString(name) * 0.5f;
            Draw.SpriteBatch.DrawString(
                Draw.DefaultFont, name,
                new Vector2(screenW * 0.5f - sz.X * 0.5f, barY - sz.Y - 3f),
                Color.White, 0f, Vector2.Zero, 0.5f,
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        // ── Hook registration ─────────────────────────────────────────────────
        public static void Load()
        {
        }

        public static void Unload()
        {
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void SetPhase(BattlePhase phase)
        {
            CurrentPhase = phase;
            SaveProgress();
            OnPhaseChanged?.Invoke(phase);
        }
    }
}
