using System.Collections;
using System.Collections.Generic;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes
{
    /// <summary>
    /// Chapter 21 — Els Termina Final Battle Introduction.
    ///
    /// Sequence overview
    /// ─────────────────
    /// PRE-PHASE (The Last Alliance):
    ///   Kirby, Madeline and their dream-friend allies face down the collected
    ///   darkness: Siamo Zero, Zero 3, Contra Void, Tesseract Soul, Hyper Meta
    ///   Morpho Knight and Nollus Nova.  The Seven Vessel Goner Souls hold the
    ///   line while the heroes fight.  When every Zero form is defeated the
    ///   screen erupts in colour.
    ///
    /// PHASE 1 — Flying into the Void:
    ///   Kirby and Madeline ride the Warp Star deep into the Abyss.  The
    ///   Abyssment floods the screen with imagery from across all past, present
    ///   and future titles.  There is no ground — Asriel's residual power keeps
    ///   them aloft.  Determination Soul and Asriel (now remorseful) fly
    ///   alongside them toward Els Termina.
    ///
    /// PHASE 2 — Side-Scrolling Approach:
    ///   Fast-paced horizontal scroll fills every border except where the
    ///   player and boss stand.  Colour shifts mark HP milestones.  Ground
    ///   materialises when they reach the final destination.
    ///
    /// Triggered by CS21_ElsTerminaFinalBattleIntroTrigger (session flag
    /// "ch21_els_termina_final_battle_intro_done").
    /// </summary>
    [Tracked]
    public class CS21_ElsTerminaFinalBattleIntro : CutsceneEntity
    {
        // ── Session flags ──────────────────────────────────────────────────
        public const string FLAG_DONE            = "ch21_els_termina_final_battle_intro_done";
        public const string FLAG_ZEROS_DEFEATED  = "ch21_all_zeros_defeated";
        public const string FLAG_WARPSTAR_LAUNCH = "ch21_warpstar_launched";
        public const string FLAG_PHASE2_REACHED  = "ch21_els_termina_phase2_reached";

        // ── Dialog keys ───────────────────────────────────────────────────
        private const string DLG_ALLIES_ARRIVE       = "DZ_CH21_FINAL_BATTLE_ALLIES_ARRIVE";
        private const string DLG_DREAM_FRIENDS        = "DZ_CH21_FINAL_BATTLE_DREAM_FRIENDS";
        private const string DLG_ZEROS_INTRO          = "DZ_CH21_FINAL_BATTLE_ZEROS_INTRO";
        private const string DLG_SEVEN_GONERS_HOLD    = "DZ_CH21_FINAL_BATTLE_GONERS_HOLD";
        private const string DLG_ZEROS_FALLEN         = "DZ_CH21_FINAL_BATTLE_ZEROS_FALLEN";
        private const string DLG_WARPSTAR_RIDE        = "DZ_CH21_FINAL_BATTLE_WARPSTAR_RIDE";
        private const string DLG_INTO_THE_ABYSS       = "DZ_CH21_FINAL_BATTLE_INTO_ABYSS";
        private const string DLG_ASRIEL_JOINS          = "DZ_CH21_FINAL_BATTLE_ASRIEL_JOINS";
        private const string DLG_DETERMINATION_SOUL   = "DZ_CH21_FINAL_BATTLE_DETERMINATION_SOUL";
        private const string DLG_ELS_TERMINA_LOOMS    = "DZ_CH21_FINAL_BATTLE_ELS_LOOMS";
        private const string DLG_PHASE2_ARRIVAL       = "DZ_CH21_FINAL_BATTLE_PHASE2_ARRIVAL";

        // ── Audio ─────────────────────────────────────────────────────────
        private const string MUS_VOID_APPROACH  = "event:/DZ/new_content/music/lvl21/void_approach";
        private const string MUS_WARPSTAR_RIDE  = "event:/DZ/new_content/music/lvl21/warpstar_ride";
        private const string MUS_ELS_TERMINA    = "event:/DZ/new_content/music/lvl21/els_termina_final";
        private const string SFX_ZERO_SHATTER   = "event:/DZ/new_content/game/21_desolo_zantas/zero_shatter";
        private const string SFX_WARPSTAR_LAUNCH = "event:/DZ/new_content/game/21_desolo_zantas/warpstar_launch";
        private const string SFX_ABYSS_RUMBLE   = "event:/DZ/new_content/game/21_desolo_zantas/falling_into_the_void";
        private const string SFX_SOUL_SURGE     = "event:/new_content/char/DZ/asriel/Asriel_Create";

        // ── Colour palette (HP phase shifts for Phase 2) ──────────────────
        private static readonly Color[] PhaseColors = new[]
        {
            Calc.HexToColor("ff4444"), // Phase 1 – full HP / red wrath
            Calc.HexToColor("ff8800"), // Phase 2 – 75 % HP  / orange rage
            Calc.HexToColor("ffdd00"), // Phase 3 – 50 % HP  / yellow desperation
            Calc.HexToColor("44ddff"), // Phase 4 – 25 % HP  / cyan collapse
            Calc.HexToColor("ffffff"), // Final   – near-death / pure white
        };

        // ── Soul colours for the Seven Vessel Goner Souls ─────────────────
        private static readonly Color[] SoulColors = new[]
        {
            Calc.HexToColor("ff0000"), // Determination
            Calc.HexToColor("ff8000"), // Bravery
            Calc.HexToColor("ffff00"), // Justice
            Calc.HexToColor("00ff00"), // Kindness
            Calc.HexToColor("00ffff"), // Patience
            Calc.HexToColor("0000ff"), // Integrity
            Calc.HexToColor("ff00ff"), // Perseverance
        };

        // ── Fields ────────────────────────────────────────────────────────
        private global::Celeste.Player player;

        // Overlay / fade
        private float overlayAlpha = 0f;
        private Color  overlayColor = Color.Black;

        // Colour-shift fields used during Phase 2 (rendered separately)
        private float  phase2ColorLerp = 0f;
        private int    phase2ColorIndex = 0;
        private bool   phase2Active = false;

        // Scrolling speed multiplier for Phase 2 backdrop feel
        private float  scrollSpeed = 0f;

        // Warp-star ride float offset
        private float  warpStarSineTimer = 0f;
        private bool   onWarpStar = false;

        // Soul-light list for the Seven Goner Souls
        private readonly List<VertexLight> soulLights = new List<VertexLight>(7);

        // ── Constructor ───────────────────────────────────────────────────
        public CS21_ElsTerminaFinalBattleIntro(global::Celeste.Player player)
            : base(fadeInOnSkip: false)
        {
            this.player = player;
            Depth = 10010;
        }

        // ── Static factory ────────────────────────────────────────────────
        public static void Trigger(Level level)
        {
            if (level.Session.GetFlag(FLAG_DONE)) return;

            var p = level.Tracker.GetEntity<global::Celeste.Player>();
            if (p == null) return;

            level.Add(new CS21_ElsTerminaFinalBattleIntro(p));
        }

        // ── Awake ─────────────────────────────────────────────────────────
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (player == null)
                player = Scene.Tracker.GetEntity<global::Celeste.Player>();
        }

        // ── OnBegin ───────────────────────────────────────────────────────
        public override void OnBegin(Level level)
        {
            if (player == null)
            {
                EndCutscene(level);
                return;
            }

            level.TimerStopped    = true;
            level.TimerHidden     = true;
            level.SaveQuitDisabled = true;
            level.PauseLock       = true;

            player.StateMachine.State = Player.StDummy;
            player.DummyAutoAnimate   = true;

            Add(new Coroutine(MainSequence(level)));
        }

        // ══════════════════════════════════════════════════════════════════
        //  MAIN SEQUENCE
        // ══════════════════════════════════════════════════════════════════
        private IEnumerator MainSequence(Level level)
        {
            // ── Fade in ────────────────────────────────────────────────────
            overlayColor = Color.Black;
            overlayAlpha = 1f;

            yield return FadeOverlay(1f, 0f, 1.5f);

            // ── PRE-PHASE: The Last Alliance ───────────────────────────────
            yield return PrePhase_AlliesArrive(level);

            // ── PRE-PHASE: Battle the Zeros ────────────────────────────────
            yield return PrePhase_ZeroBattle(level);

            // ── TRANSITION: Zeros defeated — colour burst ──────────────────
            yield return ZerosDefeatedBurst(level);

            // ── PHASE 1: Warp Star launch ──────────────────────────────────
            yield return Phase1_WarpStarLaunch(level);

            // ── PHASE 1: Flying into the Void ─────────────────────────────
            yield return Phase1_FlyingIntoVoid(level);

            // ── PHASE 1: Asriel and Determination Soul join ────────────────
            yield return Phase1_AsrielAndSoul(level);

            // ── TRANSITION → PHASE 2: Ground appears ──────────────────────
            yield return TransitionToPhase2(level);

            // ── Mark done and hand off to the boss fight ───────────────────
            level.Session.SetFlag(FLAG_DONE);
            EndCutscene(level);
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRE-PHASE  —  THE LAST ALLIANCE
        // ══════════════════════════════════════════════════════════════════

        private IEnumerator PrePhase_AlliesArrive(Level level)
        {
            // Black flash to transition into battle-hall
            level.Flash(Color.Black * 0.8f, false);
            yield return 0.4f;

            Audio.SetMusic(MUS_VOID_APPROACH);

            // Dream friends and allies rush in
            yield return Textbox.Say(DLG_ALLIES_ARRIVE);
            yield return 0.3f;

            // Seven Vessel Goner Souls materialise as coloured lights
            yield return SpawnSoulLights(level);

            yield return Textbox.Say(DLG_DREAM_FRIENDS);
            yield return 0.4f;
        }

        private IEnumerator SpawnSoulLights(Level level)
        {
            soulLights.Clear();
            Vector2 basePos = player.Center + new Vector2(-120f, -40f);

            for (int i = 0; i < 7; i++)
            {
                float angle  = MathHelper.TwoPi * i / 7f;
                float radius = 80f;
                Vector2 pos  = basePos + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius * 0.5f);

                // VertexLight is a Component — add it to this CutsceneEntity
                var light = new VertexLight(SoulColors[i], 1f, 24, 64)
                {
                    Position = pos
                };
                Add(light);
                soulLights.Add(light);

                Audio.Play(SFX_SOUL_SURGE, pos);
                level.Flash(SoulColors[i] * 0.25f, false);
                yield return 0.12f;
            }

            yield return 0.5f;
        }

        // ══════════════════════════════════════════════════════════════════
        //  PRE-PHASE  —  ZERO BATTLE MONTAGE
        // ══════════════════════════════════════════════════════════════════

        private IEnumerator PrePhase_ZeroBattle(Level level)
        {
            // Introduce the Zero collective
            level.Shake(0.5f);
            Glitch.Value = 0.4f;
            yield return 0.2f;
            Glitch.Value = 0f;

            yield return Textbox.Say(DLG_ZEROS_INTRO);

            // Seven Goner Souls hold the line — flicker in solidarity
            yield return Textbox.Say(DLG_SEVEN_GONERS_HOLD);

            // Visual montage: six Zero forms defeated one by one
            string[] zeroNames = new[]
            {
                "Siamo Zero", "Zero 3", "Contra Void",
                "Tesseract Soul", "Hyper Meta Morpho Knight", "Nollus Nova"
            };

            Color[] zeroColors = new[]
            {
                Calc.HexToColor("8800ff"),
                Calc.HexToColor("0044ff"),
                Calc.HexToColor("ff0044"),
                Calc.HexToColor("00ffcc"),
                Calc.HexToColor("888800"),
                Calc.HexToColor("440044"),
            };

            for (int i = 0; i < zeroNames.Length; i++)
            {
                yield return ZeroBossShatter(level, zeroColors[i]);
            }

            // Soul-lights pulse together when all Zeros fall
            foreach (var light in soulLights)
                light.Alpha = 2f;

            yield return 0.3f;

            foreach (var light in soulLights)
                light.Alpha = 1f;
        }

        private IEnumerator ZeroBossShatter(Level level, Color zeroColor)
        {
            // Camera shake + colour flash per Zero
            level.Shake(0.7f);
            level.Flash(zeroColor * 0.6f, true);
            Audio.Play(SFX_ZERO_SHATTER);

            // Particle burst in Zero's colour
            for (int d = 0; d < 360; d += 20)
            {
                float angle  = MathHelper.ToRadians(d);
                Vector2 pos  = level.Camera.Position + new Vector2(160f, 90f); // screen centre
                Vector2 dir  = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                level.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 3, pos + dir * 30f, Vector2.One * 5f, zeroColor, angle);
            }

            Glitch.Value = 0.25f;
            yield return 0.15f;
            Glitch.Value = 0f;
            yield return 0.4f;
        }

        // ══════════════════════════════════════════════════════════════════
        //  TRANSITION  —  ZEROS DEFEATED / COLOUR BURST
        // ══════════════════════════════════════════════════════════════════

        private IEnumerator ZerosDefeatedBurst(Level level)
        {
            level.Session.SetFlag(FLAG_ZEROS_DEFEATED);

            // Rainbow burst — all soul colours at once
            foreach (var c in SoulColors)
            {
                level.Flash(c * 0.4f, false);
                yield return 0.06f;
            }

            level.Shake(1.2f);
            level.Flash(Color.White, true);
            yield return 0.8f;

            yield return Textbox.Say(DLG_ZEROS_FALLEN);
            yield return 0.5f;
        }

        // ══════════════════════════════════════════════════════════════════
        //  PHASE 1  —  WARP STAR LAUNCH
        // ══════════════════════════════════════════════════════════════════

        private IEnumerator Phase1_WarpStarLaunch(Level level)
        {
            Audio.SetMusic(MUS_WARPSTAR_RIDE);

            // Camera zoom in slightly to sell launch moment
            float origZoom = level.Zoom;
            yield return ZoomTo(level, 1.3f, 0.6f, Ease.SineIn);

            level.Shake(1.0f);
            Audio.Play(SFX_WARPSTAR_LAUNCH);
            level.Flash(Calc.HexToColor("FFD700") * 0.8f, true);

            yield return Textbox.Say(DLG_WARPSTAR_RIDE);

            // Player starts "riding" — gentle float bob
            onWarpStar = true;

            yield return ZoomTo(level, origZoom, 0.8f, Ease.SineOut);

            level.Session.SetFlag(FLAG_WARPSTAR_LAUNCH);
            yield return 0.3f;
        }

        // ══════════════════════════════════════════════════════════════════
        //  PHASE 1  —  FLYING INTO THE VOID
        // ══════════════════════════════════════════════════════════════════

        private IEnumerator Phase1_FlyingIntoVoid(Level level)
        {
            // Overlay hint: the Abyssment of past/present/future titles
            yield return Textbox.Say(DLG_INTO_THE_ABYSS);

            // Rapid colour cycle to evoke the multi-title Abyssment
            yield return AbyssmentColorCycle(level, 5f);

            // Rumble — flying deeper
            Audio.Play(SFX_ABYSS_RUMBLE);
            level.Shake(0.4f);
            yield return 0.6f;
        }

        private IEnumerator AbyssmentColorCycle(Level level, float duration)
        {
            // Flicker through hues representing dozens of series
            Color[] seriesColors = new[]
            {
                Calc.HexToColor("ff4444"), // Kirby
                Calc.HexToColor("4488ff"), // Celeste
                Calc.HexToColor("ffcc00"), // Undertale / Deltarune
                Calc.HexToColor("44ff88"), // Dream Land
                Calc.HexToColor("aa44ff"), // Dark Matter
                Calc.HexToColor("ff8800"), // Star Allies
                Calc.HexToColor("ffffff"), // Pure light
                Calc.HexToColor("000088"), // Deep void
            };

            float timer = 0f;
            int   idx   = 0;

            while (timer < duration)
            {
                level.Flash(seriesColors[idx % seriesColors.Length] * 0.2f, false);
                idx++;
                timer += 0.35f;
                yield return 0.35f;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  PHASE 1  —  ASRIEL AND DETERMINATION SOUL JOIN
        // ══════════════════════════════════════════════════════════════════

        private IEnumerator Phase1_AsrielAndSoul(Level level)
        {
            // Asriel flies up alongside — his power is the "ground" here
            level.Flash(Calc.HexToColor("ff6699") * 0.5f, false);
            Audio.Play(SFX_SOUL_SURGE);
            yield return 0.3f;

            yield return Textbox.Say(DLG_ASRIEL_JOINS);

            // Determination Soul pulses red
            level.Flash(Calc.HexToColor("ff0000") * 0.45f, false);
            yield return 0.25f;

            yield return Textbox.Say(DLG_DETERMINATION_SOUL);

            // Els Termina silhouette looms on the horizon — brief black flash
            yield return 0.4f;
            level.Flash(Color.Black * 0.7f, false);
            yield return 0.3f;

            yield return Textbox.Say(DLG_ELS_TERMINA_LOOMS);
            yield return 0.5f;
        }

        // ══════════════════════════════════════════════════════════════════
        //  TRANSITION  —  GROUND MATERIALISES / PHASE 2
        // ══════════════════════════════════════════════════════════════════

        private IEnumerator TransitionToPhase2(Level level)
        {
            // Fast-paced side-scroll starts — mark Phase 2
            phase2Active = true;
            scrollSpeed  = 0f;

            // Ramp the scroll speed over 2 s to simulate arriving at the arena
            float rampTime = 2f;
            for (float t = 0f; t < rampTime; t += Engine.DeltaTime)
            {
                scrollSpeed = Ease.SineIn(t / rampTime) * 3f;
                yield return null;
            }

            Audio.SetMusic(MUS_ELS_TERMINA);
            level.Flash(Color.White * 0.9f, true);
            level.Shake(1.5f);

            yield return Textbox.Say(DLG_PHASE2_ARRIVAL);

            // Ground has arrived — player can stand
            onWarpStar   = false;
            phase2Active  = false;
            scrollSpeed   = 0f;

            // Start Phase 2 colour-shift coroutine
            Add(new Coroutine(Phase2ColorShiftLoop(level)));

            level.Session.SetFlag(FLAG_PHASE2_REACHED);

            // Brief pause before handing off to the actual boss fight
            yield return 1.0f;
        }

        // ══════════════════════════════════════════════════════════════════
        //  PHASE 2  —  COLOUR-SHIFT LOOP  (runs until cutscene ends)
        // ══════════════════════════════════════════════════════════════════

        private IEnumerator Phase2ColorShiftLoop(Level level)
        {
            // Continuously pulse through phase colours, driven by elapsed time.
            // The actual HP-gating is handled by the boss entity; this just keeps
            // the visual effect running until the cutscene terminates.
            float cycleTime = 4f;

            while (true)
            {
                int   next    = (phase2ColorIndex + 1) % PhaseColors.Length;
                float elapsed = 0f;

                while (elapsed < cycleTime)
                {
                    phase2ColorLerp = elapsed / cycleTime;
                    elapsed        += Engine.DeltaTime;
                    yield return null;
                }

                phase2ColorIndex = next;
                phase2ColorLerp  = 0f;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  UPDATE
        // ══════════════════════════════════════════════════════════════════

        public override void Update()
        {
            base.Update();

            // Warp star ride — gentle vertical sine bob
            if (onWarpStar && player != null)
            {
                warpStarSineTimer += Engine.DeltaTime * 2.5f;
                player.Y = player.Y + (float)Math.Sin(warpStarSineTimer) * 0.8f * Engine.DeltaTime * 60f;
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  RENDER
        // ══════════════════════════════════════════════════════════════════

        public override void Render()
        {
            base.Render();

            // Full-screen black/white fade overlay
            if (overlayAlpha > 0f)
                Draw.Rect(-10f, -10f, 1940f, 1100f, overlayColor * overlayAlpha);

            // Phase 2 colour tint overlay — very subtle, purely additive feel
            if (phase2Active && phase2ColorLerp > 0f)
            {
                Color a = PhaseColors[phase2ColorIndex];
                Color b = PhaseColors[(phase2ColorIndex + 1) % PhaseColors.Length];
                Color tint = Color.Lerp(a, b, phase2ColorLerp) * 0.18f;
                Draw.Rect(-10f, -10f, 1940f, 1100f, tint);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        //  ON END
        // ══════════════════════════════════════════════════════════════════

        public override void OnEnd(Level level)
        {
            // Restore player control
            if (player?.StateMachine != null)
                player.StateMachine.State = Player.StNormal;

            player.DummyAutoAnimate = true;

            // Restore level state
            level.TimerStopped    = false;
            level.TimerHidden     = false;
            level.SaveQuitDisabled = false;
            level.PauseLock       = false;
            level.Zoom            = 1f;
            Glitch.Value          = 0f;

            // Remove any lingering soul lights (they are Components on this entity)
            foreach (var light in soulLights)
                Remove(light);
            soulLights.Clear();

            // Ensure the intro flag is set even on skip
            level.Session.SetFlag(FLAG_DONE);
        }

        // ══════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════

        private IEnumerator FadeOverlay(float from, float to, float duration)
        {
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                overlayAlpha = MathHelper.Lerp(from, to, Ease.CubeOut(t / duration));
                yield return null;
            }
            overlayAlpha = to;
        }

        private IEnumerator ZoomTo(Level level, float target, float duration, Ease.Easer easer)
        {
            float start = level.Zoom;
            for (float t = 0f; t < duration; t += Engine.DeltaTime)
            {
                level.Zoom = Calc.LerpClamp(start, target, easer(t / duration));
                yield return null;
            }
            level.Zoom = target;
        }
    }
}
