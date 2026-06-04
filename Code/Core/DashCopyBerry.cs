using System;
using System.Collections;
using Celeste.Extensions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// A strawberry-like collectible that Kirby can only absorb by dashing into it.
/// On contact during a dash the player regains their copy ability (dash refill + power state),
/// just like a mushroom-style power-up but Kirby-flavored.
/// Non-Kirby players can still collect it by walking into it normally.
/// </summary>
[CustomEntity("MaggyHelper/DashCopyBerry")]
[Tracked]
public class DashCopyBerry : Actor
{
    // â”€â”€ Particle types â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public static ParticleType P_Glow;
    public static ParticleType P_Absorb;
    public static ParticleType P_Trail;

    // â”€â”€ Constants â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private const float ProximityRadius   = 28f;   // radius Kirby's dash aura checks
    private const float CollectDelay      = 0.45f; // seconds before removal after absorb
    private const float FlashDuration     = 0.35f; // white-flash duration on player after absorb
    private const float WobbleSpeed       = 4.5f;
    private const float PulseSpeed        = 2.2f;

    // Which power state to grant (default = Sword, mapper-configurable)
    private readonly KirbyMode.KirbyPowerState grantedPower;
    private readonly int dashRefillCount;          // how many dashes to restore
    private readonly bool refillOnlyWhenEmpty;     // only trigger when dashes == 0

    // â”€â”€ Instance state â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private Sprite sprite;
    private Wiggler wiggler;
    private BloomPoint bloom;
    private VertexLight light;
    private float wobbleTimer;
    private float pulseTimer;
    private bool absorbed;
    private float absorbTimer;
    private Vector2 startPos;
    private EntityID id;

    // â”€â”€ Constructor â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public DashCopyBerry(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
    {
        this.id      = id;
        startPos     = Position;
        grantedPower = data.Enum("power", KirbyMode.KirbyPowerState.Sword);
        dashRefillCount     = Math.Max(1, data.Int("dashRefill", 1));
        refillOnlyWhenEmpty = data.Bool("refillOnlyWhenEmpty", false);

        Depth    = -100;
        Collider = new Hitbox(18f, 18f, -9f, -12f);

        Add(sprite = GFX.SpriteBank.Has("dashcopyberry")
            ? GFX.SpriteBank.Create("dashcopyberry")
            : GFX.SpriteBank.Create("strawberry"));

        Add(wiggler = Wiggler.Create(0.4f, 4f, v => sprite.Scale = Vector2.One * (1f + v * 0.4f)));
        Add(new PlayerCollider(OnPlayerContact));
        Add(light = new VertexLight(new Color(1f, 0.6f, 0.2f), 1f, 24, 40));
        Add(bloom = new BloomPoint(1.0f, 18f));
    }

    // â”€â”€ Scene lifecycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (scene is Level level && level.Session.Strawberries.Contains(id))
            RemoveSelf();
    }

    // â”€â”€ Main update â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public override void Update()
    {
        if (!absorbed)
        {
            wobbleTimer += Engine.DeltaTime * WobbleSpeed;
            pulseTimer  += Engine.DeltaTime * PulseSpeed;
            sprite.Y     = (float)Math.Sin(wobbleTimer) * 2.5f;

            // Pulsing amber/orange color
            float t      = (float)Math.Sin(pulseTimer) * 0.5f + 0.5f;
            var   col    = Color.Lerp(new Color(1f, 0.55f, 0.1f), new Color(1f, 0.85f, 0.35f), t);
            sprite.Color = col;
            light.Color  = col;

            // Ambient glow particles
            if (Scene is Level lvl && lvl.OnInterval(0.12f) && P_Glow != null)
                lvl.ParticlesFG?.Emit(P_Glow, 1,
                    Position + new Vector2(Calc.Random.Range(-9f, 9f), Calc.Random.Range(-9f, 9f)),
                    Vector2.One * 4f);

            // Proximity pulse for dashing Kirby (visual cue only)
            CheckDashProximity();
        }
        else
        {
            absorbTimer += Engine.DeltaTime;
            if (absorbTimer >= CollectDelay)
                RemoveSelf();
        }

        base.Update();
    }

    // â”€â”€ Proximity visual cue â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private void CheckDashProximity()
    {
        if (Scene is not Level level)
            return;

        var player = level.Tracker.GetEntity<Player>();
        if (player == null || !player.IsKirbyMode())
            return;

        float dist = Vector2.Distance(player.Center, Center);
        if (dist < ProximityRadius && player.DashAttacking)
        {
            // Emit trail particles toward player as a visual draw
            if (level.OnInterval(0.05f) && P_Trail != null)
            {
                Vector2 dir = (player.Center - Center).SafeNormalize();
                level.ParticlesFG?.Emit(P_Trail, 2,
                    Center + dir * 8f, Vector2.One * 6f);
            }
        }
    }

    // â”€â”€ Collision handler â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private void OnPlayerContact(Player player)
    {
        if (absorbed)
            return;

        bool isKirby = player.IsKirbyMode();

        if (isKirby)
        {
            // Kirby must be dash-attacking to absorb
            if (!player.DashAttacking)
                return;

            // Optionally only trigger when dashes are depleted
            if (refillOnlyWhenEmpty && player.Dashes > 0)
                return;

            AbsorbByKirby(player);
        }
        else
        {
            // Normal player: collect like a regular berry
            CollectNormal(player);
        }
    }

    // â”€â”€ Kirby absorption â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private void AbsorbByKirby(Player player)
    {
        if (P_Glow == null || P_Absorb == null || P_Trail == null)
            LoadParticles();

        absorbed   = true;
        Collidable = false;
        wiggler.Start();

        // Refill dashes
        int newDashes = Math.Min(player.MaxDashes, player.Dashes + dashRefillCount);
        player.Dashes = newDashes;

        // Grant copy ability power state
        player.SetKirbyPowerState(grantedPower);

        // Register as collected in session
        if (Scene is Level level)
        {
            level.Session.Strawberries.Add(id);
            level.Session.UpdateLevelStartDashes();

            // Visual effects
            level.Flash(new Color(1f, 0.7f, 0.1f, 0.18f), true);
            level.Displacement.AddBurst(Center, 0.5f, 4f, 48f, 0.35f, Ease.QuadOut, Ease.QuadOut);
            if (P_Absorb != null)
                level.ParticlesFG?.Emit(P_Absorb, 20, Center, Vector2.One * 14f);
        }

        global::Celeste.Audio.Play("guid://{5c271b0b-eba2-400b-8011-e1d673324674}", Position);
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);

        // White flash on the player sprite briefly
        Add(new Coroutine(AbsorbRoutine(player)));
    }

    private IEnumerator AbsorbRoutine(Player player)
    {
        // Tween berry toward player
        float elapsed = 0f;
        Vector2 origin = Position;
        while (elapsed < CollectDelay * 0.7f)
        {
            elapsed  += Engine.DeltaTime;
            float pct = elapsed / (CollectDelay * 0.7f);
            Position  = Vector2.Lerp(origin, player.Center, Ease.CubeOut(pct));

            if (Scene is Level lv && P_Trail != null && lv.OnInterval(0.04f))
                lv.ParticlesFG?.Emit(P_Trail, 3, Position, Vector2.One * 5f);

            yield return null;
        }

        // Final burst at player
        if (Scene is Level level && P_Absorb != null)
            level.ParticlesFG?.Emit(P_Absorb, 12, player.Center, Vector2.One * 8f);

        // Player sprite flash (squish + color)
        if (player.Sprite != null)
        {
            player.Sprite.Scale = new Vector2(1.4f, 0.7f);
            yield return 0.08f;
            player.Sprite.Scale = new Vector2(0.8f, 1.3f);
            yield return 0.08f;
            player.Sprite.Scale = Vector2.One;
        }
    }

    // â”€â”€ Normal (non-Kirby) collection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private void CollectNormal(Player player)
    {
        absorbed   = true;
        Collidable = false;
        wiggler.Start();

        if (Scene is Level level)
        {
            level.Session.Strawberries.Add(id);
            level.Session.UpdateLevelStartDashes();
            if (P_Glow != null)
                level.ParticlesFG?.Emit(P_Glow, 10, Center, Vector2.One * 10f);
        }

        global::Celeste.Audio.Play("guid://{525d00db-bc31-4054-9f61-0238bfd62941}", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
    }

    // â”€â”€ Static particle init â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public static void LoadParticles()
    {
        P_Glow = new ParticleType
        {
            Size            = 1.2f,
            Color           = new Color(1f, 0.7f, 0.1f),
            Color2          = new Color(1f, 0.95f, 0.5f),
            ColorMode       = ParticleType.ColorModes.Blink,
            FadeMode        = ParticleType.FadeModes.Late,
            LifeMin         = 0.6f,
            LifeMax         = 1.2f,
            SpeedMin        = 6f,
            SpeedMax        = 14f,
            DirectionRange  = (float)(Math.PI * 2),
            SpeedMultiplier = 0.6f
        };

        P_Absorb = new ParticleType
        {
            Size            = 2.0f,
            Color           = new Color(1f, 0.6f, 0.05f),
            Color2          = Color.White,
            ColorMode       = ParticleType.ColorModes.Fade,
            FadeMode        = ParticleType.FadeModes.Late,
            LifeMin         = 0.8f,
            LifeMax         = 1.6f,
            SpeedMin        = 20f,
            SpeedMax        = 50f,
            DirectionRange  = (float)(Math.PI * 2),
            Acceleration    = new Vector2(0f, 20f)
        };

        P_Trail = new ParticleType
        {
            Size            = 1.5f,
            Color           = new Color(1f, 0.75f, 0.2f),
            Color2          = new Color(1f, 0.4f, 0.0f),
            ColorMode       = ParticleType.ColorModes.Choose,
            FadeMode        = ParticleType.FadeModes.Linear,
            LifeMin         = 0.25f,
            LifeMax         = 0.55f,
            SpeedMin        = 10f,
            SpeedMax        = 22f,
            DirectionRange  = (float)(Math.PI * 2),
            SpeedMultiplier = 0.4f
        };
    }
}

