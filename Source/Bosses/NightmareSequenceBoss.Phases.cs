#nullable enable
using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Bosses;

// ══════════════════════════════════════════════════════════════════════════════
//  NightmareSequenceBoss — Phase coroutines
//  All 7 phases + phase-transition + defeat sequence live here.
// ══════════════════════════════════════════════════════════════════════════════

public partial class NightmareSequenceBoss
{
    // ── Master battle sequence ──────────────────────────────────────────────
    private IEnumerator BattleSequence()
    {
        // Single FMOD event for the entire fight — els_progress drives all transitions
        Audio.SetMusic(FINALE_EVENT);
        elsProgressActive = true;
        Audio.SetMusicParam("els_progress", 0f);

        // ── PHASE 1 — Siamo Zero (flying orb) ────────────────────────────
        yield return PhaseIntro(BossPhase.SiamoZero, 0,
            "Siamo Zero", PhaseColors[0],
            "DZ_NIGHTMARE_P1_INTRO");
        yield return Phase1_SiamoZero();
        AdvanceElsProgress(); // → target 1

        // ── PHASE 2 — Zero 3 (dark-matter eye) ────────────────────────────
        yield return PhaseTransition(BossPhase.Zero3, 1,
            "Zero 3", PhaseColors[1],
            "DZ_NIGHTMARE_P2_INTRO");
        yield return Phase2_Zero3();
        AdvanceElsProgress(); // → target 1

        // ── PHASE 3 — Contra Void (ground begins) ─────────────────────────
        yield return PhaseTransitionToGround(BossPhase.ContraVoid, 2,
            "Contra Void", PhaseColors[2],
            "DZ_NIGHTMARE_P3_INTRO");
        yield return Phase3_ContraVoid();
        AdvanceElsProgress(); // → target 1

        // ── PHASE 4 — Soul Tesseract ───────────────────────────────────────
        yield return PhaseTransition(BossPhase.SoulTesseract, 3,
            "Soul Tesseract", PhaseColors[3],
            "DZ_NIGHTMARE_P4_INTRO");
        yield return Phase4_SoulTesseract();
        AdvanceElsProgress(); // → target 1

        // ── PHASE 5 — Hyper Meta Morpho Knight ────────────────────────────
        yield return PhaseTransition(BossPhase.HyperMetaMorpho, 4,
            "Hyper Meta Morpho Knight", PhaseColors[4],
            "DZ_NIGHTMARE_P5_INTRO");
        yield return Phase5_HyperMetaMorpho();
        AdvanceElsProgress(); // → target 1

        // ── PHASE 6 — Nodus Tollens ────────────────────────────────────────
        yield return PhaseTransition(BossPhase.NodusTollens, 5,
            "Nodus Tollens", PhaseColors[5],
            "DZ_NIGHTMARE_P6_INTRO");
        yield return Phase6_NodusTollens();
        // Phase 6 defeat — sweep els_progress to 1.0 over 2 s
        yield return SweepElsProgress(elsProgress, 1f, 2.0f);

        // ── Hard-snap els_progress to 2.0 — FMOD transitions the finale event internally
        elsProgress       = 2f;
        elsProgressTarget = 2f;
        Audio.SetMusicParam("els_progress", 2f);

        // ── PHASE 7 — ELS / Ellica Doppia ─────────────────────────────────
        yield return PhaseTransition(BossPhase.ELS, 6,
            "ELS — Ellica Doppia", PhaseColors[6],
            "DZ_NIGHTMARE_P7_INTRO");
        yield return Phase7_ELS();

        // ── FINAL CLIMAX — runs before the defeat cutscene ─────────────────
        yield return FinalClimax();

        // ── DEFEAT ─────────────────────────────────────────────────────────
        yield return DefeatSequence();
    }

    // ── Phase intro (very first phase — no predecessor) ───────────────────
    private IEnumerator PhaseIntro(BossPhase phase, int index,
        string name, Color color, string dialogKey)
    {
        inPhaseTransition = true;
        SetPhase(phase, index);
        SetPhaseName(name);

        Position = RandomAirPos();
        anchorSet = false;

        level.Flash(color * 0.5f, true);
        level.Shake(0.5f);
        Audio.Play(SFX_SPAWN, Position);
        EmitBurst(24, color);

        yield return 0.4f;
        yield return Textbox.Say(dialogKey);
        yield return 0.3f;

        inPhaseTransition = false;
    }

    // ── Standard aerial-to-aerial transition ──────────────────────────────
    private IEnumerator PhaseTransition(BossPhase phase, int index,
        string name, Color color, string dialogKey)
    {
        inPhaseTransition = true;

        // Dramatic shatter beat
        level.Shake(0.8f);
        level.Flash(PhaseColors[index - 1] * 0.6f, true);
        Audio.Play(SFX_STARDEATH, Position);
        EmitBurst(40, PhaseColors[index - 1]);

        yield return 0.3f;
        Glitch.Value = 0.3f;
        yield return 0.15f;
        Glitch.Value = 0f;

        SetPhase(phase, index);
        SetPhaseName(name);

        // Snap to new position in a flash
        Visible = false;
        yield return 0.1f;
        Position = isFlying ? RandomAirPos() : RandomGroundPos();
        anchorSet = false;
        level.Flash(color * 0.55f, true);
        Audio.Play(SFX_REVIVAL, Position);
        EmitBurst(32, color);
        Visible = true;

        yield return 0.5f;
        yield return Textbox.Say(dialogKey);
        yield return 0.3f;

        inPhaseTransition = false;
    }

    // ── Transition that also plants the boss on the ground ─────────────────
    private IEnumerator PhaseTransitionToGround(BossPhase phase, int index,
        string name, Color color, string dialogKey)
    {
        inPhaseTransition = true;
        isFlying          = false;   // ground from here on

        // Restore level bounds for ground phases
        if (player != null) player.EnforceLevelBounds = true;

        level.Shake(1.0f);
        level.Flash(PhaseColors[index - 1] * 0.6f, true);
        Audio.Play(SFX_STARDEATH, Position);
        EmitBurst(48, PhaseColors[index - 1]);

        yield return 0.4f;
        Glitch.Value = 0.45f;
        yield return 0.2f;
        Glitch.Value = 0f;

        SetPhase(phase, index);
        SetPhaseName(name);

        Visible  = false;
        yield return 0.15f;
        Position = RandomGroundPos();
        anchorSet = false;
        level.Flash(color * 0.6f, true);
        level.Shake(0.6f);
        Audio.Play(SFX_REVIVAL, Position);
        EmitBurst(40, color);
        Visible  = true;

        yield return 0.6f;
        yield return Textbox.Say(dialogKey);
        yield return 0.3f;

        inPhaseTransition = false;
    }

    // ── Waits until this phase's HP is exhausted ───────────────────────────
    private IEnumerator WaitForPhaseEnd()
    {
        while (currentHits < hitsToDeplete && !isDead)
            yield return null;
    }

    // ── Called by TakeDamage when HP hits 0 ───────────────────────────────
    private IEnumerator PhaseDefeatedRoutine()
    {
        // The BattleSequence coroutine is already waiting in WaitForPhaseEnd,
        // so just signal by setting currentHits = hitsToDeplete (already done).
        // Visual stagger while the transition takes over.
        Audio.Play(SFX_IMPACT, Position);
        level.Shake(0.5f);
        yield break;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PHASE 1 — SIAMO ZERO
    //  Nightmare's Power Orb reimagined as the corrupted dark-Kirby nightmare.
    //  FLYING. Shoots spread stars, charges horizontally, barrage bursts.
    //  Reference: Nightmare Power Orb (KA) · Real Dark Matter (KDL2)
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator Phase1_SiamoZero()
    {
        Add(new Coroutine(Phase1_AttackLoop()));
        yield return WaitForPhaseEnd();
    }

    private IEnumerator Phase1_AttackLoop()
    {
        float elapsed  = 0f;
        float atk      = 0f;

        while (CurrentPhase == BossPhase.SiamoZero && !isDead)
        {
            elapsed += Engine.DeltaTime;
            atk     -= Engine.DeltaTime;

            if (!inPhaseTransition && !isInvuln && atk <= 0f)
            {
                // Escalate pattern with time
                int roll = elapsed < 10f ? Calc.Random.Next(2)
                         : elapsed < 20f ? Calc.Random.Next(3)
                         : Calc.Random.Next(4);

                switch (roll)
                {
                    case 0:
                        // Single-star shot → Nightmare "Star Shot"
                        yield return P1_StarShot();
                        atk = 1.6f;
                        break;
                    case 1:
                        // Three-spread → Nightmare "Triple Star Shot"
                        yield return P1_TripleStarShot();
                        atk = 2.0f;
                        break;
                    case 2:
                        // Horizontal charge → Nightmare "Charge"
                        yield return P1_Charge();
                        atk = 2.5f;
                        break;
                    case 3:
                        // Erratic barrage → Nightmare "Dark Magic Barrage"
                        yield return P1_DarkMagicBarrage();
                        atk = 2.8f;
                        break;
                }
            }

            yield return null;
        }
    }

    // Drifts toward the player's Y then fires a single bullet left.
    private IEnumerator P1_StarShot()
    {
        // Drift vertically toward player
        float targetY = PlayerCenter.Y;
        yield return GlideTo(new Vector2(Position.X, targetY), 0.4f);
        Audio.Play(SFX_RIFT, Position);
        SpawnBullet(DirToPlayer);
        yield return 0.1f;
    }

    // Fires three bullets in a spread toward the player.
    private IEnumerator P1_TripleStarShot()
    {
        Audio.Play(SFX_RIFT, Position);
        yield return FireSpread(3, 22f, 0.07f);
        yield return 0.2f;
    }

    // Rumbles, then dashes horizontally across the screen.
    private IEnumerator P1_Charge()
    {
        Audio.Play(SFX_CHARGE, Position);
        level.Shake(0.3f);
        yield return 0.5f; // wind-up

        // Dash from current X to the opposite horizontal edge
        float destX    = Position.X < level.Camera.X + 160f
            ? level.Camera.Right - 30f : level.Camera.Left + 30f;
        Vector2 dest   = new Vector2(destX, Position.Y);
        float   speed  = 500f;
        float   dist   = Math.Abs(dest.X - Position.X);
        float   dur    = dist / speed;

        Audio.Play(SFX_SHELL, Position);
        yield return GlideTo(dest, dur);
        level.Shake(0.2f);
        yield return 0.3f;
    }

    // Moves erratically and fires five bullets in a fan.
    private IEnumerator P1_DarkMagicBarrage()
    {
        Audio.Play(SFX_CHARGE, Position);

        // Three random repositions
        for (int i = 0; i < 3; i++)
        {
            yield return GlideTo(RandomAirPos(), 0.25f);
            yield return 0.05f;
        }

        Audio.Play(SFX_RIFT, Position);
        // Five-bullet fan
        for (int i = 0; i < 5; i++)
        {
            float angle = AngleToPlayer + MathHelper.ToRadians(Calc.Random.Range(-35f, 35f));
            SpawnBullet(Calc.AngleToVector(angle, 1f));
            yield return 0.06f;
        }
        yield return 0.2f;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PHASE 2 — ZERO 3
    //  Dark Matter eye form. Still FLYING.
    //  Fires dark beam arcs, launches appendages as homing shots,
    //  full-ring laser bursts. Reference: Real Dark Matter (KDL2) · Zero (KDL3)
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator Phase2_Zero3()
    {
        Add(new Coroutine(Phase2_AttackLoop()));
        yield return WaitForPhaseEnd();
    }

    private IEnumerator Phase2_AttackLoop()
    {
        float elapsed = 0f;
        float atk     = 0f;

        while (CurrentPhase == BossPhase.Zero3 && !isDead)
        {
            elapsed += Engine.DeltaTime;
            atk     -= Engine.DeltaTime;

            if (!inPhaseTransition && !isInvuln && atk <= 0f)
            {
                int roll = elapsed < 12f ? Calc.Random.Next(2)
                         : Calc.Random.Next(3);

                switch (roll)
                {
                    case 0:
                        // Diagonal beam arcs — Zero's dark laser beams
                        yield return P2_DarkBeamArcs();
                        atk = 2.2f;
                        break;
                    case 1:
                        // Appendage launch — deflectable homing orbs
                        yield return P2_AppendageLaunch();
                        atk = 2.8f;
                        break;
                    case 2:
                        // Full ring burst
                        yield return P2_RingBurst();
                        atk = 3.0f;
                        break;
                }
            }

            yield return null;
        }
    }

    // Fires lasers at four diagonal angles toward the player.
    private IEnumerator P2_DarkBeamArcs()
    {
        Audio.Play(SFX_CHARGE, Position);
        yield return 0.4f;
        Audio.Play(SFX_BEAM, Position);
        float baseAngle = AngleToPlayer;
        float[] offsets = { -30f, -10f, 10f, 30f };
        foreach (float off in offsets)
        {
            SpawnLaser(baseAngle + MathHelper.ToRadians(off));
            yield return 0.08f;
        }
        yield return 0.2f;
    }

    // Fires four big bullets (appendages) that home in on the player.
    private IEnumerator P2_AppendageLaunch()
    {
        Audio.Play(SFX_BUILD, Position);
        yield return 0.3f;
        for (int i = 0; i < 4; i++)
        {
            float angle = MathHelper.TwoPi * i / 4f;
            SpawnBigBullet(Calc.AngleToVector(angle, 1f));
            yield return 0.12f;
        }
        yield return 0.3f;
    }

    // 8-way ring burst — Zero's dark beams in all directions.
    private IEnumerator P2_RingBurst()
    {
        Audio.Play(SFX_CHARGE, Position);
        level.Shake(0.3f);
        yield return 0.5f;
        Audio.Play(SFX_BEAM, Position);
        FireRing(8);
        level.Flash(PhaseColors[1] * 0.3f, false);
        yield return 0.3f;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PHASE 3 — CONTRA VOID
    //  GROUND starts here. Void-tether dashes, crimson sweeps,
    //  summons mini-orb projectiles that orbit and scatter.
    //  Reference: Dark Matter Blade (KDL2) · Contra-Kirby nightmare concept
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator Phase3_ContraVoid()
    {
        Add(new Coroutine(Phase3_AttackLoop()));
        yield return WaitForPhaseEnd();
    }

    private IEnumerator Phase3_AttackLoop()
    {
        float elapsed = 0f;
        float atk     = 0f;

        while (CurrentPhase == BossPhase.ContraVoid && !isDead)
        {
            elapsed += Engine.DeltaTime;
            atk     -= Engine.DeltaTime;

            if (!inPhaseTransition && !isInvuln && atk <= 0f)
            {
                int roll = elapsed < 10f ? Calc.Random.Next(2)
                         : Calc.Random.Next(3);

                switch (roll)
                {
                    case 0:
                        // Ground sweep — Nightmare Wizard "Sweep" + Dark Matter dash
                        yield return P3_VoidSweep();
                        atk = 2.0f;
                        break;
                    case 1:
                        // Star volley aimed at player — Nightmare "Palm Star Shot"
                        yield return P3_StarVolley();
                        atk = 2.2f;
                        break;
                    case 2:
                        // Orbiting scatter — mini dark matter swarm
                        yield return P3_OrbitingScatter();
                        atk = 3.0f;
                        break;
                }
            }

            yield return null;
        }
    }

    // Glides quickly across the ground toward the player, then retreats.
    private IEnumerator P3_VoidSweep()
    {
        Audio.Play(SFX_CHARGE, Position);
        float sweepX = PlayerCenter.X + (Position.X < PlayerCenter.X ? 80f : -80f);
        float sweepY = RandomGroundPos().Y;
        yield return GlideTo(new Vector2(sweepX, sweepY), 0.35f);
        Audio.Play(SFX_IMPACT, Position);
        level.Shake(0.3f);
        EmitBurst(16, PhaseColors[2]);
        yield return 0.4f;
        // Retreat to opposite side
        yield return GlideTo(new Vector2(
            level.Camera.X + 160f + (Position.X > level.Camera.X + 160f ? -100f : 100f),
            sweepY), 0.5f);
        yield return 0.2f;
    }

    // Opens cloak — fires spread from current pos aimed at player.
    private IEnumerator P3_StarVolley()
    {
        Audio.Play(SFX_RIFT, Position);
        level.Flash(PhaseColors[2] * 0.25f, false);
        yield return 0.35f;
        // Five-bullet arc (Nightmare "Open Cloak Star Shot")
        yield return FireSpread(5, 30f, 0.06f);
        yield return 0.3f;
    }

    // Fires a ring, then each bullet briefly orbits and scatters outward.
    private IEnumerator P3_OrbitingScatter()
    {
        Audio.Play(SFX_DARKSPAWN, Position);
        level.Shake(0.2f);
        yield return 0.4f;
        // Two rings, staggered
        FireRing(6);
        yield return 0.25f;
        FireRing(6, 200f);
        yield return 0.3f;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PHASE 4 — SOUL TESSERACT
    //  Elemental cube cycling (Miracle Matter inspired).
    //  Cycles through Fire / Ice / Electric identities — each sub-phase has
    //  unique projectile behaviour. GROUND.
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator Phase4_SoulTesseract()
    {
        Add(new Coroutine(Phase4_AttackLoop()));
        yield return WaitForPhaseEnd();
    }

    private int _tesseractElement = 0; // 0=Fire 1=Ice 2=Electric

    private IEnumerator Phase4_AttackLoop()
    {
        float elapsed = 0f;
        float atk     = 0f;

        while (CurrentPhase == BossPhase.SoulTesseract && !isDead)
        {
            elapsed += Engine.DeltaTime;
            atk     -= Engine.DeltaTime;

            if (!inPhaseTransition && !isInvuln && atk <= 0f)
            {
                // Rotate through elements
                switch (_tesseractElement % 3)
                {
                    case 0:
                        yield return P4_FireElement();
                        atk = 2.5f;
                        break;
                    case 1:
                        yield return P4_IceElement();
                        atk = 2.5f;
                        break;
                    case 2:
                        yield return P4_ElectricElement();
                        atk = 2.5f;
                        break;
                }
                _tesseractElement++;
            }

            yield return null;
        }
    }

    // Fire element: dashes, drops fire balls to the ground.
    private IEnumerator P4_FireElement()
    {
        targetColor = Calc.HexToColor("ff4400");
        Audio.Play(SFX_CHARGE, Position);
        level.Flash(Color.OrangeRed * 0.3f, false);
        yield return 0.3f;
        // Dash + fire scatter
        yield return GlideTo(new Vector2(PlayerCenter.X + 60f, RandomGroundPos().Y), 0.3f);
        Audio.Play(SFX_SHELL, Position);
        // Fire downward bursts
        for (int i = 0; i < 3; i++)
        {
            SpawnBullet(new Vector2(Calc.Random.Range(-0.5f, 0.5f), 1f).SafeNormalize());
            yield return 0.12f;
        }
        yield return 0.4f;
        targetColor = PhaseColors[3];
    }

    // Ice element: moves in circular pattern, fires freezing eye beam along the ground.
    private IEnumerator P4_IceElement()
    {
        targetColor = Calc.HexToColor("88ddff");
        Audio.Play(SFX_CHARGE, Position);
        level.Flash(Color.LightBlue * 0.3f, false);
        yield return 0.3f;
        // Circular drift
        float startAngle = Calc.Angle(level.Camera.Position + new Vector2(192f, 122f), Position);
        for (int i = 0; i < 8; i++)
        {
            float a = startAngle + i * (MathHelper.TwoPi / 8f);
            Vector2 orbitPos = (level.Camera.Position + new Vector2(192f, 122f))
                + Calc.AngleToVector(a, 80f);
            yield return GlideTo(orbitPos, 0.18f);
        }
        // Fire wide horizontal laser sweep
        Audio.Play(SFX_BEAM, Position);
        SpawnLaser(AngleToPlayer);
        SpawnLaser(AngleToPlayer + MathHelper.ToRadians(15f));
        SpawnLaser(AngleToPlayer - MathHelper.ToRadians(15f));
        yield return 0.4f;
        targetColor = PhaseColors[3];
    }

    // Electric element: borders corners with lightning — mega laser.
    private IEnumerator P4_ElectricElement()
    {
        targetColor = Calc.HexToColor("aaff44");
        Audio.Play(SFX_CHARGE, Position);
        level.Flash(Color.GreenYellow * 0.3f, false);
        yield return 0.45f;
        Audio.Play(SFX_BEAM, Position);
        level.Shake(0.8f);
        level.Flash(PhaseColors[3] * 0.4f, false);
        SpawnMegaLaser();
        yield return 0.5f;
        // Corner bullets
        Vector2[] corners =
        {
            new Vector2(level.Camera.Left + 20f,  level.Camera.Top    + 20f),
            new Vector2(level.Camera.Right - 20f, level.Camera.Top    + 20f),
            new Vector2(level.Camera.Left + 20f,  level.Camera.Bottom - 20f),
            new Vector2(level.Camera.Right - 20f, level.Camera.Bottom - 20f),
        };
        foreach (var c in corners)
        {
            Scene.Add(new WarpStarBullet(c, (PlayerCenter - c).SafeNormalize()));
            yield return 0.05f;
        }
        yield return 0.4f;
        targetColor = PhaseColors[3];
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PHASE 5 — HYPER META MORPHO KNIGHT
    //  Butterfly / sword hybrid. Close-range slashes, butterfly dive,
    //  feather spread. GROUND. Reference: Morpho Knight Delta (project)
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator Phase5_HyperMetaMorpho()
    {
        Add(new Coroutine(Phase5_AttackLoop()));
        yield return WaitForPhaseEnd();
    }

    private IEnumerator Phase5_AttackLoop()
    {
        float elapsed = 0f;
        float atk     = 0f;

        while (CurrentPhase == BossPhase.HyperMetaMorpho && !isDead)
        {
            elapsed += Engine.DeltaTime;
            atk     -= Engine.DeltaTime;

            if (!inPhaseTransition && !isInvuln && atk <= 0f)
            {
                int roll = elapsed < 12f ? Calc.Random.Next(2)
                         : Calc.Random.Next(3);

                switch (roll)
                {
                    case 0:
                        // Sword slash combo
                        yield return P5_SwordSlash();
                        atk = 1.8f;
                        break;
                    case 1:
                        // Butterfly dive — Nightmare "Twister Dash"
                        yield return P5_ButterflyDive();
                        atk = 2.5f;
                        break;
                    case 2:
                        // Feather spread — Morpho wing scatter
                        yield return P5_FeatherSpread();
                        atk = 2.8f;
                        break;
                }
            }

            yield return null;
        }
    }

    // Glides to player, fires tight burst (simulates sword slash).
    private IEnumerator P5_SwordSlash()
    {
        Audio.Play(SFX_SLICE, Position);
        yield return GlideTo(PlayerCenter + new Vector2(0f, -40f), 0.25f);
        yield return FireSpread(3, 15f, 0.05f);
        Audio.Play(SFX_IMPACT, Position);
        level.Shake(0.2f);
        yield return GlideTo(RandomGroundPos(), 0.3f);
        yield return 0.2f;
    }

    // Rises off screen, slams down upside-down — Nightmare "Upside-Down Tornado".
    private IEnumerator P5_ButterflyDive()
    {
        Audio.Play(SFX_SHELL, Position);
        float aboveScreen = level.Camera.Top - 60f;
        float targetX = PlayerCenter.X;
        yield return GlideTo(new Vector2(targetX, aboveScreen), 0.35f);
        yield return 0.2f;
        // Slam down fast
        Audio.Play(SFX_CHARGE, Position);
        level.Shake(0.4f);
        yield return GlideTo(new Vector2(targetX, level.Camera.Bottom - 60f), 0.22f);
        Audio.Play(SFX_IMPACT, Position);
        level.Shake(0.6f);
        EmitBurst(24, PhaseColors[4]);
        // Feathers scatter on impact
        yield return FireSpread(5, 45f, 0.05f);
        yield return 0.4f;
    }

    // Fires feathers (big bullets) in a wide fan from current position.
    private IEnumerator P5_FeatherSpread()
    {
        Audio.Play(SFX_BUILD, Position);
        level.Flash(PhaseColors[4] * 0.25f, false);
        yield return 0.3f;
        for (int i = 0; i < 6; i++)
        {
            float angle = AngleToPlayer + MathHelper.ToRadians(Calc.LerpClamp(-60f, 60f, i / 5f));
            SpawnBigBullet(Calc.AngleToVector(angle, 1f));
            yield return 0.07f;
        }
        yield return 0.3f;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PHASE 6 — NODUS TOLLENS (Corrupted Galactic Nova)
    //  Cosmic laser array, nova rings, time-distortion barrage.
    //  GROUND. Reference: 0² (K64) halo-cycle + Galactic Nova (KSS)
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator Phase6_NodusTollens()
    {
        Add(new Coroutine(Phase6_AttackLoop()));
        yield return WaitForPhaseEnd();
    }

    private IEnumerator Phase6_AttackLoop()
    {
        float elapsed = 0f;
        float atk     = 0f;
        int   cycle   = 0;

        while (CurrentPhase == BossPhase.NodusTollens && !isDead)
        {
            elapsed += Engine.DeltaTime;
            atk     -= Engine.DeltaTime;

            if (!inPhaseTransition && !isInvuln && atk <= 0f)
            {
                switch (cycle % 4)
                {
                    case 0:
                        // Halo laser — 0² eye-cycle step
                        yield return P6_HaloLaser();
                        atk = 2.2f;
                        break;
                    case 1:
                        // Nova ring — expanding ring of bullets
                        yield return P6_NovaRing();
                        atk = 2.5f;
                        break;
                    case 2:
                        // Time barrage — rapid small shots
                        yield return P6_TimeBarrage();
                        atk = 2.0f;
                        break;
                    case 3:
                        // Mega laser sweep
                        yield return P6_CosmicSweep();
                        atk = 3.5f;
                        break;
                }
                cycle++;
            }

            yield return null;
        }
    }

    // Fires 5 lasers in a wide horizontal arc.
    private IEnumerator P6_HaloLaser()
    {
        Audio.Play(SFX_CHARGE, Position);
        yield return 0.5f;
        Audio.Play(SFX_BEAM, Position);
        level.Flash(PhaseColors[5] * 0.4f, false);
        for (int i = 0; i < 5; i++)
        {
            float angle = AngleToPlayer + MathHelper.ToRadians(Calc.LerpClamp(-40f, 40f, i / 4f));
            SpawnLaser(angle);
            yield return 0.06f;
        }
        yield return 0.3f;
    }

    // Fires an expanding ring of 12 bullets.
    private IEnumerator P6_NovaRing()
    {
        Audio.Play(SFX_DARKSPAWN, Position);
        level.Shake(0.3f);
        yield return 0.35f;
        FireRing(12);
        level.Flash(PhaseColors[5] * 0.3f, false);
        EmitBurst(20, PhaseColors[5]);
        yield return 0.1f;
        FireRing(12, 180f);
        yield return 0.4f;
    }

    // Rapid-fire stream of bullets aimed at the player.
    private IEnumerator P6_TimeBarrage()
    {
        Audio.Play(SFX_CHARGE, Position);
        yield return 0.2f;
        for (int i = 0; i < 8; i++)
        {
            float jitter = MathHelper.ToRadians(Calc.Random.Range(-10f, 10f));
            SpawnBullet(Calc.AngleToVector(AngleToPlayer + jitter, 1f));
            Audio.Play(SFX_RIFT, Position);
            yield return 0.09f;
        }
        yield return 0.4f;
    }

    // Full-screen mega laser + follow-up ring.
    private IEnumerator P6_CosmicSweep()
    {
        Audio.Play(SFX_CHARGE, Position);
        level.Shake(0.5f);
        yield return 0.7f;
        Audio.Play(SFX_SHELL, Position);
        level.Shake(1.0f);
        level.Flash(PhaseColors[5] * 0.5f, true);
        SpawnMegaLaser();
        yield return 0.4f;
        FireRing(8, 160f);
        yield return 0.5f;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PHASE 7 — ELS / ELLICA DOPPIA
    //  The true final form. Wizard robes, exposed tornado weak-point.
    //  Nightmare Wizard moveset: Palm Shot, Finger Barrage, Sweep, Cloak Shot,
    //  Twister Dash, Upside-Down Tornado (low HP only).
    //  GROUND. Reference: Nightmare Wizard (KA) · Void Termina Phase 4 (KSA)
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator Phase7_ELS()
    {
        Add(new Coroutine(Phase7_AttackLoop()));
        yield return WaitForPhaseEnd();
    }

    private IEnumerator Phase7_AttackLoop()
    {
        float elapsed  = 0f;
        float atk      = 0f;
        bool  lowHP    = false;

        while (CurrentPhase == BossPhase.ELS && !isDead)
        {
            elapsed += Engine.DeltaTime;
            atk     -= Engine.DeltaTime;

            // Low-HP threshold — unlocks Upside-Down Tornado
            if (!lowHP && currentHits >= hitsToDeplete / 2)
            {
                lowHP = true;
                level.Flash(PhaseColors[6] * 0.4f, false);
                level.Shake(0.5f);
                Audio.Play(SFX_PREDEATH, Position);
            }

            if (!inPhaseTransition && !isInvuln && atk <= 0f)
            {
                // Randomly teleport to a new ground position between attacks
                // (Nightmare Wizard teleports constantly)
                if (Calc.Random.Chance(0.5f))
                {
                    Audio.Play(SFX_TELEPORT, Position);
                    Visible  = false;
                    yield return 0.15f;
                    Position = RandomGroundPos();
                    anchorSet = false;
                    level.Flash(PhaseColors[6] * 0.2f, false);
                    Visible  = true;
                    yield return 0.1f;
                }

                // Pick attack — low HP unlocks extra move
                int maxRoll = lowHP ? 5 : 4;
                int roll    = Calc.Random.Next(maxRoll);

                switch (roll)
                {
                    case 0:
                        // Palm Star Shot — arc of stars from front, weak-point briefly exposed
                        yield return P7_PalmStarShot();
                        atk = 1.8f;
                        break;
                    case 1:
                        // Pointed Finger Barrage — double arc at low HP
                        yield return P7_PointedFingerShot(lowHP);
                        atk = 2.2f;
                        break;
                    case 2:
                        // Sweep — glides across the floor toward the player
                        yield return P7_Sweep();
                        atk = 2.0f;
                        break;
                    case 3:
                        // Open Cloak Star Shot — five bullets in a fan
                        yield return P7_OpenCloakShot();
                        atk = 2.4f;
                        break;
                    case 4:
                        // Upside-Down Tornado (low HP only) — off-screen slam
                        yield return P7_UpsideDownTornado();
                        atk = 3.0f;
                        break;
                }
            }

            yield return null;
        }
    }

    // Opens palm, fires a spread arc — player can dash into body while it's exposed.
    private IEnumerator P7_PalmStarShot()
    {
        Audio.Play(SFX_RIFT, Position);
        level.Flash(PhaseColors[6] * 0.2f, false);
        yield return 0.3f;
        yield return FireSpread(4, 28f, 0.07f);
        yield return 0.4f;
    }

    // Charges up, then fires a barrage — double arc when low HP.
    private IEnumerator P7_PointedFingerShot(bool doubleArc)
    {
        Audio.Play(SFX_CHARGE, Position);
        yield return 0.5f;
        Audio.Play(SFX_RIFT, Position);

        if (doubleArc)
        {
            // Upper arc
            yield return FireSpread(4, 20f, 0.06f);
            yield return 0.1f;
            // Lower arc offset by -30°
            float baseAngle = AngleToPlayer - MathHelper.ToRadians(30f);
            for (int i = 0; i < 4; i++)
            {
                float ang = baseAngle + MathHelper.ToRadians(i * 13f);
                SpawnBullet(Calc.AngleToVector(ang, 1f));
                yield return 0.06f;
            }
        }
        else
        {
            yield return FireSpread(5, 22f, 0.06f);
        }
        yield return 0.4f;
    }

    // Glides forward along the ground — Nightmare Wizard "Sweep".
    private IEnumerator P7_Sweep()
    {
        Audio.Play(SFX_CHARGE, Position);
        float dir   = Position.X < PlayerCenter.X ? 1f : -1f;
        float destX = Position.X + dir * 280f;
        destX       = Math.Clamp(destX, level.Camera.Left + 30f, level.Camera.Right - 30f);
        yield return GlideTo(new Vector2(destX, Position.Y), 0.32f);
        Audio.Play(SFX_IMPACT, Position);
        level.Shake(0.2f);
        yield return 0.3f;
    }

    // Opens cloak and fires five bullets in a fan (two each side + one below).
    private IEnumerator P7_OpenCloakShot()
    {
        // Drift to centre — Nightmare always moves to a central position for this
        float centreX = level.Camera.Left + (level.Camera.Right - level.Camera.Left) * 0.5f;
        yield return GlideTo(new Vector2(centreX, Position.Y), 0.35f);

        Audio.Play(SFX_SHELL, Position);
        level.Flash(PhaseColors[6] * 0.3f, false);
        yield return 0.2f;

        // Fan: 2 left, 2 right, 1 below
        float[] angles =
        {
            AngleToPlayer - MathHelper.ToRadians(50f),
            AngleToPlayer - MathHelper.ToRadians(25f),
            AngleToPlayer + MathHelper.ToRadians(25f),
            AngleToPlayer + MathHelper.ToRadians(50f),
            MathHelper.PiOver2, // straight down
        };
        foreach (float angle in angles)
        {
            SpawnBullet(Calc.AngleToVector(angle, 1f));
            yield return 0.07f;
        }
        yield return 0.4f;
    }

    // Rises off-screen upside-down, then slams straight down — low HP only.
    private IEnumerator P7_UpsideDownTornado()
    {
        Audio.Play(SFX_SHELL, Position);
        float targetX = PlayerCenter.X;
        yield return GlideTo(new Vector2(targetX, level.Camera.Top - 60f), 0.4f);
        yield return 0.2f;
        Audio.Play(SFX_CHARGE, Position);
        level.Shake(0.5f);
        // Slam downward
        yield return GlideTo(new Vector2(targetX, level.Camera.Bottom - 50f), 0.18f);
        Audio.Play(SFX_IMPACT, Position);
        level.Shake(0.9f);
        level.Flash(PhaseColors[6] * 0.4f, false);
        EmitBurst(30, PhaseColors[6]);
        // Scatter on impact
        FireRing(6, 200f);
        yield return 0.5f;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  FINAL CLIMAX
    //  ELS fires 6 laser bursts; Kirby counters each with a WarpStarBullet
    //  volley shot from the player's position.
    //  On the 7th exchange: ELS fires a WarpMegaLaser, Kirby fires a
    //  WarpMegaLaser back — then els_progress sweeps 2 → 3 (reverb fade).
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator FinalClimax()
    {
        inPhaseTransition = true;

        // Centre the boss for the cinematic exchange
        float centreX = level.Camera.Left + (level.Camera.Right - level.Camera.Left) * 0.5f;
        float centreY = level.Camera.Top  + (level.Camera.Bottom - level.Camera.Top) * 0.5f;
        yield return GlideTo(new Vector2(centreX, centreY), 0.6f);

        Audio.Play(SFX_PREDEATH, Position);
        level.Shake(0.4f);
        yield return Textbox.Say("DZ_NIGHTMARE_FINAL_CLIMAX");
        yield return 0.3f;

        // ── 6 alternating laser exchanges ─────────────────────────────────
        for (int round = 1; round <= 6; round++)
        {
            // ELS fires a spread of lasers at the player
            Audio.Play(SFX_CHARGE, Position);
            level.Shake(0.3f);
            yield return 0.35f;

            Audio.Play(SFX_BEAM, Position);
            level.Flash(PhaseColors[6] * 0.35f, false);
            // Spread gets tighter each round (starts wide, closes in)
            float spreadDeg = MathHelper.Lerp(40f, 10f, (round - 1) / 5f);
            int   laserCount = round <= 3 ? 3 : 5;
            for (int i = 0; i < laserCount; i++)
            {
                float off = laserCount > 1
                    ? MathHelper.ToRadians(Calc.LerpClamp(-spreadDeg, spreadDeg, (float)i / (laserCount - 1)))
                    : 0f;
                SpawnLaser(AngleToPlayer + off);
                yield return 0.05f;
            }

            // Brief pause so the player sees the lasers coming
            yield return 0.25f;

            // Kirby counter-fires from the player's position toward the boss
            player ??= Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                Audio.Play(SFX_RIFT, player.Center);
                Vector2 kirbyDir = (Center - player.Center).SafeNormalize(Vector2.UnitX);
                // Kirby fires a volley of star bullets back at ELS
                int kirbyShots = 2 + round; // 3 … 8 shots, escalating
                float kirbySpread = MathHelper.ToRadians(15f);
                for (int k = 0; k < kirbyShots; k++)
                {
                    float kOff = kirbyShots > 1
                        ? Calc.LerpClamp(-kirbySpread, kirbySpread, (float)k / (kirbyShots - 1))
                        : 0f;
                    Scene.Add(new WarpStarBullet(
                        player.Center,
                        Calc.AngleToVector(Calc.Angle(player.Center, Center) + kOff, 1f)));
                    yield return 0.04f;
                }
            }

            level.Shake(0.2f);
            yield return 0.5f;
        }

        // ── 7th exchange — simultaneous mega lasers ────────────────────────
        Audio.Play(SFX_FINALCRY, Position);
        level.Shake(0.8f);
        yield return 0.5f;

        // ELS fires mega laser toward the player
        Audio.Play(SFX_SHELL, Position);
        level.Flash(PhaseColors[6] * 0.5f, true);
        SpawnMegaLaser();

        // Kirby fires mega laser back at ELS simultaneously
        player ??= Scene.Tracker.GetEntity<Player>();
        if (player != null)
        {
            Audio.Play(SFX_SHELL, player.Center);
            // Kirby's mega laser fires rightward (toward ELS on right side)
            Scene.Add(new WarpMegaLaser(player.Center, fireRight: player.Center.X < Center.X));
        }

        level.Shake(2.0f);
        EmitBurst(80, Color.White);
        level.Flash(Color.White, true);
        yield return 0.3f;

        // ── Sweep els_progress 2 → 3 over 3 s (triggers reverb/fade tail in FMOD) ──
        yield return SweepElsProgress(2f, 3f, 3.0f);

        yield return 0.4f;
        inPhaseTransition = false;
    }

    // ══════════════════════════════════════════════════════════════════════
    //  DEFEAT SEQUENCE
    // ══════════════════════════════════════════════════════════════════════
    private IEnumerator DefeatSequence()
    {
        isDead            = true;
        inPhaseTransition = true;
        CurrentPhase      = BossPhase.Defeated;

        // Restore player bounds
        if (player != null) player.EnforceLevelBounds = true;

        Audio.Play(SFX_FINALCRY, Position);
        level.Shake(1.5f);

        // Rainbow colour cascade through all phase colours
        // (els_progress is already at 3.0 from FinalClimax — music is fading)
        foreach (Color c in PhaseColors)
        {
            level.Flash(c * 0.5f, false);
            Audio.Play(SFX_STARDEATH, Position);
            yield return 0.1f;
        }

        Glitch.Value = 0.6f;
        yield return 0.15f;
        Glitch.Value = 0.3f;
        yield return 0.15f;
        Glitch.Value = 0f;

        // Massive explosion burst
        for (int i = 0; i < 5; i++)
        {
            level.Shake(2.0f);
            EmitBurst(60, PhaseColors[i % PhaseColors.Length]);
            level.Flash(Color.White * 0.8f, true);
            yield return 0.18f;
        }

        level.Flash(Color.White, true);
        Audio.Play(SFX_PREDEATH, Position);
        yield return 0.8f;

        // Dialogue
        yield return Textbox.Say("DZ_NIGHTMARE_DEFEAT");
        yield return 0.5f;

        // Victory music + completion flag
        Audio.SetMusic(MUSIC_VICTORY);
        level.Session.SetFlag(completionFlag);

        // ── ELS final explosion ──────────────────────────────────────────────────
        // All seven phase colours cascade outward as the boss dissolves
        Audio.Play(SFX_FINALCRY, Position);
        level.Shake(2.5f);
        for (int i = 0; i < PhaseColors.Length; i++)
        {
            EmitBurst(48, PhaseColors[i]);
            level.Flash(PhaseColors[i] * 0.55f, true);
            level.Displacement.AddBurst(Center, 4f, 32f + i * 24f, 320f + i * 40f, 2f);
            Audio.Play(SFX_STARDEATH, Position);
            yield return 0.08f;
        }

        // Final white nova
        EmitBurst(120, Color.White);
        level.Flash(Color.White, true);
        level.Shake(3.0f);
        yield return 0.5f;

        // Fade the boss sprite out
        for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
        {
            orbSprite.Color = Color.Lerp(Color.White, Color.Transparent, Ease.SineIn(t));
            bossLight.Alpha = 1f - Ease.SineIn(t);
            yield return null;
        }

        // ── Teleport Kirby, Madeline, and allies to the "end-saved" room ──────────
        // Brief white-out before the warp
        level.Flash(Color.White * 0.95f, true);
        yield return 0.6f;

        // Dialogue beat before the warp
        yield return Textbox.Say("DZ_NIGHTMARE_ASCENT");
        yield return 0.3f;

        // Revival SFX from the player's position
        player ??= Scene.Tracker.GetEntity<Player>();
        if (player != null)
        {
            Audio.Play(SFX_REVIVAL, player.Center);
            level.Flash(Color.White, true);
            yield return 0.2f;
        }

        // Remove boss entity before the level warp so it doesn't persist
        RemoveSelf();

        // Teleport to the "end-saved" room (level name within the same chapter)
        level.TeleportTo(player, "end-saved", Player.IntroTypes.None);
    }
}
