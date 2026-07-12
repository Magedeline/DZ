#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Bosses;

// ══════════════════════════════════════════════════════════════════════════════
//
//  NIGHTMARE SEQUENCE BOSS
//  Inspired by the Dark-Matter lineage of Kirby final bosses:
//    Nightmare (KA) · Dark Matter (KDL2/3) · Zero (KDL3) · Miracle Matter (K64)
//    Zero Two / 0² (K64) · Dark Nebula (KSS) · Void Termina (KSA)
//
//  Seven sequential phases, each with unique identity, colours, attacks,
//  and a phase-transition cutscene beat.
//
//  Phase 1  — Siamo Zero         (flying orb; corrupted dark-Kirby nightmare)
//  Phase 2  — Zero 3             (aerial eye; dark-matter eye form)
//  Phase 3  — Contra Void        (ground + aerial; void-tether dashes)
//  Phase 4  — Soul Tesseract     (dimensional geometry; elemental cycling)
//  Phase 5  — Hyper Meta Morpho Knight (butterfly/sword; close-range slashes)
//  Phase 6  — Nodus Tollens      (corrupted Galactic Nova; cosmic laser array)
//  Phase 7  — ELS / Ellica Doppia (true final; wizard robes; tornado weak-point)
//
//  Design notes
//  ─────────────
//  • Phases 1-2 are FLYING  — player.EnforceLevelBounds = false
//  • Phases 3-7 are GROUND  — bounds restored, boss walks/hovers near ground
//  • Damage model: each phase has its own HP pool (configurable per-phase).
//    Player damages via dashing (StDash). Each phase has an invulnerability
//    window after a hit so attacks can still breathe.
//  • Between phases: screen flash, dialogue beat, boss transforms.
//  • All projectiles reuse WarpStarBullet / WarpStarBigBullet / WarpLaser /
//    WarpMegaLaser from WarpStarAttacks.cs (already compiled in project).
//
// ══════════════════════════════════════════════════════════════════════════════

[CustomEntity("DZ/NightmareSequenceBoss")]
[Tracked(true)]
[HotReloadable]
public partial class NightmareSequenceBoss : Entity
{
    // ── Phase enum ─────────────────────────────────────────────────────────
    public enum BossPhase
    {
        Idle,
        SiamoZero,       // Phase 1  – flying orb
        Zero3,           // Phase 2  – dark-matter eye
        ContraVoid,      // Phase 3  – void dashes, ground starts here
        SoulTesseract,   // Phase 4  – elemental cube
        HyperMetaMorpho, // Phase 5  – butterfly knight
        NodusTollens,    // Phase 6  – corrupted nova
        ELS,             // Phase 7  – Ellica Doppia (true final)
        Defeated
    }

    // ── Phase colours (HUD bar + flash tints) ──────────────────────────────
    public static readonly Color[] PhaseColors = new Color[]
    {
        Calc.HexToColor("8800ff"), // 1  Siamo Zero        — violet
        Calc.HexToColor("0044ff"), // 2  Zero 3            — deep blue
        Calc.HexToColor("ff0044"), // 3  Contra Void       — crimson
        Calc.HexToColor("00ffcc"), // 4  Soul Tesseract    — teal
        Calc.HexToColor("ffdd00"), // 5  Hyper Meta Morpho — gold
        Calc.HexToColor("440044"), // 6  Nodus Tollens     — dark purple
        Calc.HexToColor("ffffff"), // 7  ELS               — white/silver
    };

    // ── Phase HP (hits to deplete — multiplied by difficulty) ──────────────
    private static readonly int[] BaseHitsPerPhase = { 6, 6, 8, 8, 10, 10, 12 };

    // ── Audio SFX ─────────────────────────────────────────────────────────
    private const string SFX_HIT        = "event:/DZ/new_content/char/els/Els_Scream_Hit";
    private const string SFX_SPAWN      = "event:/DZ/new_content/char/els/spawn";
    private const string SFX_CHARGE     = "event:/DZ/new_content/char/els/ElsDZ_CHarge";
    private const string SFX_SLICE      = "event:/DZ/new_content/char/els/Els_Slice";
    private const string SFX_RIFT       = "event:/DZ/new_content/char/els/Els_Rift";
    private const string SFX_TELEPORT   = "event:/DZ/new_content/char/els/teleport";
    private const string SFX_IMPACT     = "event:/DZ/new_content/char/els/big_hit";
    private const string SFX_BEAM       = "event:/DZ/new_content/char/els/Els_BeamSlash";
    private const string SFX_SHELL      = "event:/DZ/new_content/char/els/Els_Shell_Screamer";
    private const string SFX_STARDEATH  = "event:/DZ/new_content/char/els/Els_StarDeath";
    private const string SFX_BUILD      = "event:/DZ/new_content/char/els/Els_Build";
    private const string SFX_PREDEATH   = "event:/DZ/new_content/char/els/Els_Predeath";
    private const string SFX_FINALCRY   = "event:/DZ/new_content/char/els/Els_Final_Cry";
    private const string SFX_REVIVAL    = "event:/DZ/new_content/char/els/revival";
    private const string SFX_DARKSPAWN  = "event:/DZ/new_content/char/els/dark_matter_spawn";

    // ── Music ─────────────────────────────────────────────────────────────
    private const string FINALE_EVENT    = "event:/DZ/new_content/music/lvl21/finale";
    private const string MUSIC_VICTORY   = "event:/DZ/new_content/music/lvl21/victory";

    // ── Constants ─────────────────────────────────────────────────────────
    private const float HIT_INVULN_TIME     = 1.2f;
    private const float PHASE_TRANS_PAUSE   = 1.5f;
    private const float GROUND_HOVER_Y_OFF  = -60f;  // pixels above ground

    // ── Fields ─────────────────────────────────────────────────────────────
    private Level level = null!;
    private Player? player;

    public  BossPhase CurrentPhase { get; private set; } = BossPhase.Idle;
    private int   phaseIndex        = 0;  // 0-based index into phases array
    private int   currentHits       = 0;
    private int   hitsToDeplete     = 0;
    private float invulnTimer       = 0f;
    private bool  isInvuln          = false;
    private bool  isDead            = false;
    private bool  inPhaseTransition = false;
    private bool  isFlying          = true; // false once ground phases begin

    // Visual
    private Sprite  orbSprite   = null!;  // placeholder sprite
    private Color   bodyColor   = Color.White;
    private Color   targetColor = Color.White;
    private float   colorLerp   = 0f;
    private float   floatSine   = 0f;
    private float   floatAnchorY = 0f;
    private bool    anchorSet   = false;
    private VertexLight bossLight = null!;
    private Wiggler hitWiggler   = null!;

    // Movement
    private Vector2 velocity     = Vector2.Zero;
    private Vector2 moveTarget   = Vector2.Zero;
    private bool    movingToTarget = false;

    // HUD
    private string  phaseName    = "";

    // ── FMOD els_progress ─────────────────────────────────────────────────
    // Mapping:
    //   Phases 1–6 defeated → 0.0 → 1.0  (each phase adds 1/6)
    //   Phase 7 start       → 2.0  (hard snap, ELS music loop)
    //   Final climax end    → 3.0  (sweep triggers fade-out reverb tail)
    private float elsProgress       = 0f;
    private float elsProgressTarget = 0f;
    private bool  elsProgressActive = false;

    // ── Particles ─────────────────────────────────────────────────────────
    public static readonly ParticleType P_Burst = new ParticleType
    {
        Color          = Calc.HexToColor("8800ff"),
        Color2         = Calc.HexToColor("ffffff"),
        ColorMode      = ParticleType.ColorModes.Blink,
        FadeMode       = ParticleType.FadeModes.Late,
        LifeMin        = 0.4f,
        LifeMax        = 1.0f,
        Size           = 1f,
        SpeedMin       = 60f,
        SpeedMax       = 180f,
        DirectionRange = MathHelper.TwoPi
    };

    public static readonly ParticleType P_Nova = new ParticleType
    {
        Color          = Calc.HexToColor("ffdd00"),
        Color2         = Calc.HexToColor("ff6600"),
        ColorMode      = ParticleType.ColorModes.Blink,
        FadeMode       = ParticleType.FadeModes.Late,
        LifeMin        = 0.6f,
        LifeMax        = 1.4f,
        Size           = 2f,
        SpeedMin       = 80f,
        SpeedMax       = 240f,
        DirectionRange = MathHelper.TwoPi
    };

    // ── Difficulty ─────────────────────────────────────────────────────────
    private readonly bool hardMode;
    private readonly string completionFlag;
    private readonly bool fromCutscene;

    // ── Constructor ────────────────────────────────────────────────────────
    public NightmareSequenceBoss(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        hardMode       = data.Bool("hardMode", false);
        completionFlag = data.Attr("completionFlag", "nightmare_sequence_boss_defeated");
        fromCutscene   = data.Bool("fromCutscene", false);

        Collider = new Hitbox(80f, 80f, -40f, -40f);
        Depth    = -12500;

        orbSprite = GFX.SpriteBank.Create("DZ_NightmareBoss");
        orbSprite.Play("idle");

        bossLight  = new VertexLight(Color.White, 1f, 64, 160);
        hitWiggler = Wiggler.Create(0.6f, 4f, v => orbSprite.Scale = Vector2.One * (1f + v * 0.2f));

        Add(orbSprite);
        Add(bossLight);
        Add(hitWiggler);
        Add(new PlayerCollider(OnPlayerDash, Collider, null));
    }

    // ── Scene lifecycle ────────────────────────────────────────────────────
    public override void Added(Scene scene)
    {
        base.Added(scene);
        level  = SceneAs<Level>();
        player = scene.Tracker.GetEntity<Player>();

        if (!fromCutscene)
            Visible = false;
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        player ??= scene.Tracker.GetEntity<Player>();
    }

    // ── Public API ─────────────────────────────────────────────────────────
    public void StartBattle()
    {
        if (CurrentPhase != BossPhase.Idle) return;
        Visible = true;
        Add(new Coroutine(BattleSequence()));
    }

    // ── Collision ──────────────────────────────────────────────────────────
    private void OnPlayerDash(Player p)
    {
        if (isDead || inPhaseTransition || isInvuln) return;
        if (p.StateMachine.State != Player.StDash) return;

        TakeDamage();
    }

    private void TakeDamage()
    {
        isInvuln   = true;
        invulnTimer = HIT_INVULN_TIME;
        currentHits++;

        Audio.Play(SFX_HIT, Position);
        hitWiggler.Start();
        orbSprite.Color = Color.Red;
        level.Shake(0.25f);

        if (currentHits >= hitsToDeplete)
        {
            currentHits = hitsToDeplete;
            Add(new Coroutine(PhaseDefeatedRoutine()));
        }
    }

    // ── Update ─────────────────────────────────────────────────────────────
    public override void Update()
    {
        base.Update();

        player ??= Scene.Tracker.GetEntity<Player>();

        // Invulnerability countdown
        if (isInvuln)
        {
            invulnTimer -= Engine.DeltaTime;
            if (invulnTimer <= 0f)
            {
                isInvuln = false;
                if (orbSprite.Color != bodyColor)
                {
                    colorLerp = 0f;
                }
            }
            // Flash while invuln
            Visible = Scene.OnInterval(0.05f) ? false : true;
        }
        else
        {
            Visible = !isDead;
        }

        // Smooth colour lerp
        colorLerp = Math.Min(colorLerp + Engine.DeltaTime * 3f, 1f);
        bodyColor = Color.Lerp(bodyColor, targetColor, colorLerp);
        if (!isInvuln) orbSprite.Color = bodyColor;

        // Light matches body colour
        bossLight.Color = bodyColor;

        // Flying bob (phases 1-2)
        if (isFlying && CurrentPhase != BossPhase.Idle && CurrentPhase != BossPhase.Defeated)
        {
            if (!anchorSet) { floatAnchorY = Y; anchorSet = true; }
            floatSine += Engine.DeltaTime * 2.2f;
            Y = floatAnchorY + (float)Math.Sin(floatSine) * 8f;
        }

        // Smooth movement toward target
        if (movingToTarget)
        {
            Vector2 diff = moveTarget - Position;
            if (diff.LengthSquared() < 4f)
            {
                Position      = moveTarget;
                movingToTarget = false;
            }
            else
            {
                Position += diff.SafeNormalize() * Math.Min(diff.Length(), 280f * Engine.DeltaTime);
            }
        }

        // Level bounds: flying phases ignore walls
        if (player != null)
        {
            if (isFlying) player.EnforceLevelBounds = false;
        }

        // ── Drive els_progress FMOD parameter every frame ──────────────────
        if (elsProgressActive)
        {
            elsProgress = Calc.Approach(elsProgress, elsProgressTarget, Engine.DeltaTime * 0.15f);
            Audio.SetMusicParam("els_progress", elsProgress);
        }
    }

    // ── Render — HUD health bar ─────────────────────────────────────────────
    public override void Render()
    {
        base.Render();

        if (CurrentPhase == BossPhase.Idle || CurrentPhase == BossPhase.Defeated) return;

        int   phaseNum = phaseIndex + 1;
        float pct      = hitsToDeplete > 0
            ? 1f - (float)currentHits / hitsToDeplete : 1f;
        Color bar      = phaseIndex < PhaseColors.Length ? PhaseColors[phaseIndex] : Color.White;

        const float W = 280f, H = 7f, TOP = 10f;
        float left    = (Engine.Width - W) * 0.5f;
        float fillW   = W * Math.Max(pct, 0f);

        Draw.Rect(left - 1f, TOP - 1f, W + 2f, H + 2f, Color.Black * 0.85f);
        if (fillW > 0f) Draw.Rect(left, TOP, fillW, H, bar);
        Draw.HollowRect(left, TOP, W, H, bar * 0.9f);

        string label = $"[{phaseNum}/7]  {phaseName}";
        Vector2 sz   = Draw.DefaultFont.MeasureString(label) * 0.45f;
        Draw.SpriteBatch.DrawString(Draw.DefaultFont, label,
            new Vector2(Engine.Width * 0.5f - sz.X * 0.5f, TOP - sz.Y - 2f),
            Color.White, 0f, Vector2.Zero, 0.45f,
            Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private void SetPhase(BossPhase phase, int index)
    {
        CurrentPhase = phase;
        phaseIndex   = index;
        currentHits  = 0;
        int baseHits = index < BaseHitsPerPhase.Length ? BaseHitsPerPhase[index] : 8;
        hitsToDeplete = hardMode ? (int)(baseHits * 1.5f) : baseHits;

        targetColor = index < PhaseColors.Length ? PhaseColors[index] : Color.White;
        colorLerp   = 0f;
        bossLight.Color = targetColor;
    }

    private void SetPhaseName(string name) => phaseName = name;

    /// <summary>
    /// Smoothly sweeps <c>els_progress</c> from <paramref name="from"/> to
    /// <paramref name="to"/> over <paramref name="duration"/> seconds, then snaps.
    /// </summary>
    private IEnumerator SweepElsProgress(float from, float to, float duration)
    {
        elsProgressActive = true;
        elsProgressTarget = from;
        yield return null;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed          += Engine.DeltaTime;
            elsProgressTarget = MathHelper.Lerp(from, to, Math.Min(elapsed / duration, 1f));
            yield return null;
        }
        // Hard snap so floating-point drift never misses the boundary
        elsProgressTarget = to;
        elsProgress       = to;
        Audio.SetMusicParam("els_progress", to);
    }

    /// <summary>
    /// Sets <c>els_progress</c> target to 1.0 immediately.
    /// Called after each of phases 1–5 is defeated — the parameter stays at 1
    /// throughout phases 1–5 and then sweeps properly after phase 6.
    /// </summary>
    private void AdvanceElsProgress()
    {
        elsProgressActive = true;
        elsProgressTarget = 1f;
    }

    /// <summary>Instantly teleport the boss, with flash + SFX.</summary>
    private void Teleport(Vector2 dest)
    {
        level.Shake(0.2f);
        Visible  = false;
        Position = dest;
        Audio.Play(SFX_TELEPORT, Position);
        level.Flash(targetColor * 0.25f, false);
        Visible = true;
    }

    /// <summary>Smooth glide to a destination over <paramref name="duration"/> seconds.</summary>
    private IEnumerator GlideTo(Vector2 dest, float duration)
    {
        Vector2 start = Position;
        for (float t = 0f; t < duration; t += Engine.DeltaTime)
        {
            Position = Vector2.Lerp(start, dest, Ease.SineInOut(t / duration));
            yield return null;
        }
        Position = dest;
    }

    /// <summary>Safe spawn point: somewhere in the upper portion of the camera view.</summary>
    private Vector2 RandomAirPos()
    {
        float x = Calc.Random.Range(level.Camera.Left + 60f, level.Camera.Right - 60f);
        float y = Calc.Random.Range(level.Camera.Top  + 40f, level.Camera.Top + 100f);
        return new Vector2(x, y);
    }

    /// <summary>Safe spawn near the ground.</summary>
    private Vector2 RandomGroundPos()
    {
        float x = Calc.Random.Range(level.Camera.Left + 60f, level.Camera.Right - 60f);
        float y = level.Camera.Bottom - 80f + GROUND_HOVER_Y_OFF;
        return new Vector2(x, y);
    }

    private Vector2 PlayerCenter =>
        player?.Center ?? (level.Camera.Position + new Vector2(192f, 122f));

    private void EmitBurst(int count, Color c)
    {
        ParticleType pt = new ParticleType(P_Burst) { Color = c, Color2 = Color.White };
        level.ParticlesFG.Emit(pt, count, Center, Vector2.One * 30f);
    }

    // ── Projectile helpers ─────────────────────────────────────────────────

    private void SpawnBullet(Vector2 dir)
        => Scene.Add(new WarpStarBullet(Center, dir));

    private void SpawnBigBullet(Vector2 dir)
        => Scene.Add(new WarpStarBigBullet(Center, dir));

    private void SpawnLaser(float angle)
        => Scene.Add(new WarpLaser(Center, angle));

    private void SpawnMegaLaser()
        => Scene.Add(new WarpMegaLaser(Center, fireRight: Center.X < PlayerCenter.X));

    private Vector2 DirToPlayer => (PlayerCenter - Center).SafeNormalize(-Vector2.UnitX);
    private float   AngleToPlayer => Calc.Angle(Center, PlayerCenter);

    /// <summary>Fire a ring of <paramref name="count"/> bullets evenly spread around 360°.</summary>
    private void FireRing(int count, float speed = 220f)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = MathHelper.TwoPi * i / count;
            SpawnBullet(Calc.AngleToVector(angle, 1f));
        }
    }

    /// <summary>Fire a spread of <paramref name="count"/> bullets aimed at the player.</summary>
    private IEnumerator FireSpread(int count, float spreadDeg = 25f, float delayBetween = 0.07f)
    {
        float baseAngle = AngleToPlayer;
        float spreadRad = MathHelper.ToRadians(spreadDeg);
        for (int i = 0; i < count; i++)
        {
            float offset = count > 1
                ? Calc.LerpClamp(-spreadRad, spreadRad, (float)i / (count - 1))
                : 0f;
            SpawnBullet(Calc.AngleToVector(baseAngle + offset, 1f));
            if (i < count - 1) yield return delayBetween;
        }
    }
}
