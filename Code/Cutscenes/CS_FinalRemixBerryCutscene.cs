using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// GOLDEN BERRY & PINK PLATINUM BERRY - FINAL REMIX (CH19-21)
/// Asriel, Granny, and Kirby - The Final Promise
///
/// Played when collecting the Golden Berry or Pink Platinum Berry in the
/// final remix chapters (19-21). Features Asriel appearing in golden light,
/// reaching out to Kirby, Granny slowly walking in, and an emotional farewell.
///
/// Dialog triggers:
///   {trigger 0 fade in soft golden light - asriel and granny appears}
///   {trigger 1 asriel reaches out and grabs kirby's hand}
///   {trigger 2 granny slowly walks in from the right}
///   {trigger 3 asriel looks at granny}
///   {trigger 4 asriel turns to kirby with tears in his eyes}
/// </summary>
public class CS_FinalRemixBerryCutscene : CutsceneEntity
{
    #region Constants

    private const string GOLDEN_DIALOG_KEY = "CH19_21_GOLDEN_BERRY";
    private const string PINK_PLAT_DIALOG_KEY = "CH19_21_PINK_PLATINUM_BERRY";
    private const string FLAG_GOLDEN_CUTSCENE_DONE = "final_remix_golden_berry_cutscene";
    private const string FLAG_PINK_PLAT_CUTSCENE_DONE = "final_remix_pink_plat_berry_cutscene";

    // SFX Events
    private const string SFX_GOLDEN_GLOW = "event:/pusheen/extra_content/game/19_spaces/golden_glow";
    private const string SFX_HAND_GRAB = "event:/pusheen/extra_content/char/asriel/emotional_reunion";
    private const string SFX_GRANNY_STEPS = "event:/new_content/char/madeline/screenentry_gran_landing";
    private const string SFX_TEARS = "event:/pusheen/extra_content/char/asriel/fade_to_flower";
    private const string SFX_HEART_GEM_APPEAR = "event:/game/general/heart_gem_appear";
    private const string SFX_HEART_GEM_PULSE = "event:/pusheen/extra_content/game/19_spaces/golden_glow";

    #endregion

    #region Fields

    private Player player;
    private NPC asriel;
    private NPC granny;
    private Entity heartGem;
    private Level level;

    private readonly bool isPinkPlatinum;
    private float goldenGlowAlpha;
    private float heartGemGlowAlpha;
    private ParticleType goldenParticle;
    private ParticleType tearParticle;
    private ParticleType heartGemParticle;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates the Final Remix berry cutscene.
    /// </summary>
    /// <param name="player">The player entity (Kirby)</param>
    /// <param name="isPinkPlatinum">True for Pink Platinum Berry, false for Golden Berry</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public CS_FinalRemixBerryCutscene(Player player, bool isPinkPlatinum)
        : base(fadeInOnSkip: false)
    {
        this.player = player;
        this.isPinkPlatinum = isPinkPlatinum;
        base.Depth = -1000000;

        goldenParticle = new ParticleType
        {
            Color = isPinkPlatinum ? Color.HotPink : Color.Gold,
            Color2 = isPinkPlatinum ? Color.LightPink : Color.Yellow,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 1.2f,
            LifeMax = 2.5f,
            Size = 1.5f,
            SpeedMin = 10f,
            SpeedMax = 25f,
            Direction = -MathHelper.PiOver2,
            DirectionRange = MathHelper.Pi
        };

        tearParticle = new ParticleType
        {
            Color = Color.LightCyan,
            Color2 = Color.White,
            ColorMode = ParticleType.ColorModes.Fade,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.4f,
            LifeMax = 0.8f,
            Size = 1f,
            SpeedMin = 10f,
            SpeedMax = 30f,
            Direction = MathHelper.PiOver2,
            DirectionRange = MathHelper.PiOver4
        };

        heartGemParticle = new ParticleType
        {
            Color = isPinkPlatinum ? Color.Magenta : Color.Gold,
            Color2 = isPinkPlatinum ? Color.White : Color.LightGoldenrodYellow,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            LifeMin = 0.8f,
            LifeMax = 1.8f,
            Size = 2f,
            SpeedMin = 15f,
            SpeedMax = 35f,
            Direction = -MathHelper.PiOver2,
            DirectionRange = MathHelper.Pi
        };
    }

    #endregion

    #region Lifecycle

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        level = scene as Level;
        level.TimerStopped = true;
        level.TimerHidden = true;
        level.SaveQuitDisabled = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnBegin(Level level)
    {
        this.level = level;
        Add(new Coroutine(Cutscene(level)));
    }

    #endregion

    #region Main Cutscene Sequence

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IEnumerator Cutscene(Level level)
    {
        // Setup player
        player.StateMachine.State = Player.StDummy; // StDummy
        player.DummyGravity = true;
        player.DummyAutoAnimate = true;
        player.Speed = Vector2.Zero;
        player.Facing = Facings.Right;

        yield return 0.3f;

        // Spawn Asriel and Granny off-screen (hidden initially)
        SpawnNPCs(level);

        yield return 0.2f;

        // Choose dialog key based on berry type
        string dialogKey = isPinkPlatinum ? PINK_PLAT_DIALOG_KEY : GOLDEN_DIALOG_KEY;

        // Play the dialog with trigger callbacks
        yield return Textbox.Say(dialogKey, new Func<IEnumerator>[]
        {
            FadeInGoldenLight,       // trigger 0 - fade in soft golden light, asriel and granny appears
            AsrielGrabsKirbyHand,    // trigger 1 - asriel reaches out and grabs kirby's hand
            GrannyWalksIn,           // trigger 2 - granny slowly walks in from the right
            AsrielLooksAtGranny,     // trigger 3 - asriel looks at granny
            AsrielTurnsToKirby,      // trigger 4 - asriel turns to kirby with tears in his eyes
            HeartGemMaterializes     // trigger 5 - a brilliant heart gem materializes in the center of the room
        });

        yield return 0.5f;

        // Fade out the golden glow
        yield return FadeOutGoldenGlow();

        yield return 0.3f;

        EndCutscene(level);
    }

    #endregion

    #region NPC Setup

    private void SpawnNPCs(Level level)
    {
        // Asriel starts off-screen to the left, invisible
        Vector2 asrielPos = player.Position + new Vector2(-80f, 0f);
        asriel = new NPC(asrielPos);
        if (GFX.SpriteBank.Has("asriel"))
        {
            asriel.Add(asriel.Sprite = GFX.SpriteBank.Create("asriel"));
            asriel.Sprite.Play("idle");
            asriel.Sprite.Color = Color.Transparent;
        }
        asriel.Depth = 100;
        Scene.Add(asriel);

        // Granny starts far off-screen to the right, invisible
        Vector2 grannyPos = player.Position + new Vector2(200f, 0f);
        granny = new NPC(grannyPos);
        if (GFX.SpriteBank.Has("granny"))
        {
            granny.Add(granny.Sprite = GFX.SpriteBank.Create("granny"));
            granny.Sprite.Play("idle");
            granny.Sprite.Color = Color.Transparent;
        }
        granny.Depth = 100;
        Scene.Add(granny);
    }

    #endregion

    #region Trigger Actions

    // {trigger 0 fade in soft golden light - asriel and granny appears}
    private IEnumerator FadeInGoldenLight()
    {
        Audio.Play(SFX_GOLDEN_GLOW, player.Position);

        // Slowly fade in golden/pink glow
        Color glowColor = isPinkPlatinum ? Color.HotPink : Color.Gold;

        for (float p = 0f; p < 1f; p += Engine.DeltaTime / 2f)
        {
            goldenGlowAlpha = p * 0.3f;

            // Fade in Asriel
            if (asriel?.Sprite != null)
            {
                asriel.Sprite.Color = Color.White * p;
            }

            // Fade in Granny (semi-transparent, she's a spirit)
            if (granny?.Sprite != null)
            {
                granny.Sprite.Color = Color.White * (p * 0.8f);
            }

            // Emit gentle golden/pink particles
            if (Calc.Random.Chance(0.4f))
            {
                Vector2 emitPos = player.Center + new Vector2(
                    Calc.Random.Range(-60f, 60f),
                    Calc.Random.Range(-40f, 20f));
                level.ParticlesFG.Emit(goldenParticle, emitPos);
            }

            yield return null;
        }

        goldenGlowAlpha = 0.3f;

        // Move Asriel closer to the player
        float asrielTarget = player.X - 40f;
        yield return WalkNpcTo(asriel, asrielTarget, 30f);

        // Face Kirby
        if (asriel?.Sprite != null)
        {
            asriel.Sprite.Scale.X = 1f; // Face right toward Kirby
        }

        yield return 0.3f;
    }

    // {trigger 1 asriel reaches out and grabs kirby's hand}
    private IEnumerator AsrielGrabsKirbyHand()
    {
        Audio.Play(SFX_HAND_GRAB, asriel.Position);

        // Camera zooms in slightly
        Vector2 zoomTarget = new Vector2(
            (player.X + asriel.X) * 0.5f - level.Camera.X,
            90f);
        Add(new Coroutine(level.ZoomTo(zoomTarget, 1.4f, 0.8f)));

        // Asriel reaches out (play animation if available)
        if (asriel?.Sprite != null && asriel.Sprite.Has("reach"))
        {
            asriel.Sprite.Play("reach");
        }

        yield return 0.6f;

        // Kirby faces Asriel
        player.Facing = Facings.Left;

        yield return 0.4f;

        // Subtle screen shake for the emotional connection
        level.Shake(0.15f);

        // Burst of golden particles at the point of contact
        Vector2 handPoint = Vector2.Lerp(asriel.Center, player.Center, 0.5f);
        for (int i = 0; i < 15; i++)
        {
            level.ParticlesFG.Emit(goldenParticle,
                handPoint + new Vector2(Calc.Random.Range(-8f, 8f), Calc.Random.Range(-8f, 8f)));
        }

        yield return 0.5f;
    }

    // {trigger 2 granny slowly walks in from the right}
    private IEnumerator GrannyWalksIn()
    {
        Audio.Play(SFX_GRANNY_STEPS, granny.Position);

        // Granny walks slowly from the right toward the scene
        float grannyTarget = player.X + 60f;

        if (granny?.Sprite != null)
        {
            granny.Sprite.Scale.X = -1f; // Face left
            if (granny.Sprite.Has("walk"))
            {
                granny.Sprite.Play("walk");
            }
        }

        // Very slow walk speed for granny
        float speed = 20f;
        float direction = Math.Sign(grannyTarget - granny.X);

        while (Math.Abs(granny.X - grannyTarget) > 2f)
        {
            granny.X += direction * speed * Engine.DeltaTime;

            // Gentle particles trailing behind
            if (Calc.Random.Chance(0.15f))
            {
                level.ParticlesFG.Emit(goldenParticle, granny.Center + new Vector2(10f, 0f));
            }

            yield return null;
        }

        granny.X = grannyTarget;

        if (granny?.Sprite != null)
        {
            granny.Sprite.Play("idle");
        }

        yield return 0.5f;
    }

    // {trigger 3 asriel looks at granny}
    private IEnumerator AsrielLooksAtGranny()
    {
        // Asriel turns to face Granny (right)
        if (asriel?.Sprite != null)
        {
            // Brief pause before turning
            yield return 0.2f;

            asriel.Sprite.Scale.X = -1f; // Turn to face right (where granny is)

            // Gentle camera pan to include granny
            Vector2 targetCamera = new Vector2(
                (asriel.X + granny.X) * 0.5f - 160f,
                level.Camera.Y);
            yield return CameraTo(targetCamera, 0.8f, Ease.SineInOut);
        }

        yield return 0.4f;
    }

    // {trigger 4 asriel turns to kirby with tears in his eyes}
    private IEnumerator AsrielTurnsToKirby()
    {
        Audio.Play(SFX_TEARS, asriel.Position);

        yield return 0.3f;

        // Asriel turns back to face Kirby
        if (asriel?.Sprite != null)
        {
            asriel.Sprite.Scale.X = 1f; // Face right toward Kirby

            // Play crying/emotional animation if available
            if (asriel.Sprite.Has("tears"))
            {
                asriel.Sprite.Play("tears");
            }
        }

        // Camera zooms in on Asriel's face
        Vector2 zoomTarget = new Vector2(
            asriel.X - level.Camera.X,
            90f);
        Add(new Coroutine(level.ZoomTo(zoomTarget, 1.6f, 1f)));

        // Emit tear particles
        for (float t = 0f; t < 1.5f; t += Engine.DeltaTime)
        {
            if (Calc.Random.Chance(0.3f) && asriel != null)
            {
                level.ParticlesFG.Emit(tearParticle,
                    asriel.Center + new Vector2(Calc.Random.Range(-4f, 4f), -8f));
            }

            yield return null;
        }

        // Slight screen shake for emotional weight
        level.Shake(0.1f);

        yield return 0.5f;

        // Zoom back out
        Add(new Coroutine(level.ZoomBack(1.5f)));

        yield return 0.3f;
    }

    // {trigger 5 a brilliant heart gem materializes in the center of the room bathed in golden light}
    private IEnumerator HeartGemMaterializes()
    {
        // Camera pans to center of room
        Vector2 roomCenter = new Vector2(
            (player.X + (asriel?.X ?? player.X)) * 0.5f,
            player.Y - 40f);

        Vector2 targetCamera = new Vector2(roomCenter.X - 160f, level.Camera.Y);
        yield return CameraTo(targetCamera, 1f, Ease.SineInOut);

        yield return 0.3f;

        // Zoom in toward where the gem will appear
        Vector2 zoomTarget = new Vector2(roomCenter.X - level.Camera.X, 80f);
        Add(new Coroutine(level.ZoomTo(zoomTarget, 1.5f, 1.2f)));

        yield return 0.5f;

        // Initial flash of light
        Audio.Play(SFX_HEART_GEM_APPEAR, roomCenter);
        level.Flash(isPinkPlatinum ? Color.HotPink * 0.4f : Color.Gold * 0.4f, false);
        level.Shake(0.2f);

        yield return 0.3f;

        // Create the heart gem entity at center
        heartGem = new Entity(roomCenter) { Depth = -100 };
        string heartSprite = isPinkPlatinum ? "heartgem4" : "heartgem0"; // pink or blue/gold
        if (GFX.SpriteBank.Has(heartSprite))
        {
            Sprite gemSprite = GFX.SpriteBank.Create(heartSprite);
            gemSprite.Play("spin");
            gemSprite.Color = Color.Transparent;
            heartGem.Add(gemSprite);
        }

        // Add bloom and light to the gem
        heartGem.Add(new BloomPoint(0f, 16f));
        heartGem.Add(new VertexLight(Color.White, 1f, 32, 64));
        Scene.Add(heartGem);

        yield return 0.2f;

        // Slowly materialize the heart gem with intensifying golden light
        heartGemGlowAlpha = 0f;
        float materializeTime = 3f;

        for (float t = 0f; t < materializeTime; t += Engine.DeltaTime)
        {
            float progress = t / materializeTime;
            float easedProgress = Ease.SineOut(progress);

            // Fade in the gem sprite
            Sprite sprite = heartGem.Get<Sprite>();
            if (sprite != null)
            {
                sprite.Color = Color.White * easedProgress;
            }

            // Intensify bloom
            BloomPoint bloom = heartGem.Get<BloomPoint>();
            if (bloom != null)
            {
                bloom.Alpha = easedProgress;
            }

            // Build up the golden glow in the room
            heartGemGlowAlpha = easedProgress * 0.35f;

            // Emit particles that spiral toward the gem
            if (Calc.Random.Chance(0.3f + progress * 0.4f))
            {
                float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
                float radius = 60f * (1f - progress * 0.5f);
                Vector2 emitPos = roomCenter + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius);
                level.ParticlesFG.Emit(heartGemParticle, emitPos);
            }

            // Periodic pulses of light
            if (progress > 0.3f && Calc.Random.Chance(0.02f))
            {
                level.Shake(0.05f);
                Audio.Play(SFX_HEART_GEM_PULSE, roomCenter);
            }

            yield return null;
        }

        // Final materialization burst
        level.Flash(isPinkPlatinum ? Color.HotPink * 0.6f : Color.Gold * 0.6f, false);
        level.Shake(0.4f);
        Audio.Play(SFX_HEART_GEM_APPEAR, roomCenter);

        // Burst of particles outward
        for (int i = 0; i < 40; i++)
        {
            float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
            float dist = Calc.Random.Range(4f, 20f);
            Vector2 emitPos = roomCenter + new Vector2(
                (float)Math.Cos(angle) * dist,
                (float)Math.Sin(angle) * dist);
            level.ParticlesFG.Emit(heartGemParticle, emitPos);
        }

        yield return 0.5f;

        // Gem now fully visible, pulsing gently
        heartGemGlowAlpha = 0.25f;

        // Hold for emotional weight
        yield return 1f;

        // Slowly zoom back
        Add(new Coroutine(level.ZoomBack(2f)));

        // Fade the heart gem glow down to ambient
        for (float p = heartGemGlowAlpha; p > 0.1f; p -= Engine.DeltaTime / 2f)
        {
            heartGemGlowAlpha = p;
            yield return null;
        }
        heartGemGlowAlpha = 0.1f;

        yield return 0.5f;
    }

    #endregion

    #region Helper Methods

    private IEnumerator FadeOutGoldenGlow()
    {
        for (float p = goldenGlowAlpha; p > 0f; p -= Engine.DeltaTime / 1.5f)
        {
            goldenGlowAlpha = p;
            yield return null;
        }
        goldenGlowAlpha = 0f;
    }

    private IEnumerator WalkNpcTo(NPC npc, float targetX, float speed = 40f)
    {
        if (npc == null) yield break;

        float direction = Math.Sign(targetX - npc.X);
        if (npc.Sprite != null)
        {
            npc.Sprite.Scale.X = direction;
            if (npc.Sprite.Has("walk"))
            {
                npc.Sprite.Play("walk");
            }
        }

        while (Math.Abs(npc.X - targetX) > 2f)
        {
            npc.X += direction * speed * Engine.DeltaTime;
            yield return null;
        }

        npc.X = targetX;
        if (npc.Sprite != null)
        {
            npc.Sprite.Play("idle");
        }
    }

    private IEnumerator CameraTo(Vector2 target, float duration, Ease.Easer ease = null)
    {
        ease ??= Ease.Linear;
        Vector2 from = level.Camera.Position;

        for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
        {
            level.Camera.Position = Vector2.Lerp(from, target, ease(p));
            yield return null;
        }

        level.Camera.Position = target;
    }

    #endregion

    #region Render

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        base.Render();

        // Render the soft golden/pink glow overlay
        if (goldenGlowAlpha > 0f)
        {
            Color glowColor = isPinkPlatinum
                ? Color.HotPink * goldenGlowAlpha
                : Color.Gold * goldenGlowAlpha;

            Draw.Rect(
                level.Camera.X - 1f,
                level.Camera.Y - 1f,
                322f, 182f,
                glowColor);
        }

        // Render the heart gem golden light overlay
        if (heartGemGlowAlpha > 0f)
        {
            Color gemGlow = isPinkPlatinum
                ? Color.Lerp(Color.HotPink, Color.White, 0.3f) * heartGemGlowAlpha
                : Color.Lerp(Color.Gold, Color.White, 0.3f) * heartGemGlowAlpha;

            Draw.Rect(
                level.Camera.X - 1f,
                level.Camera.Y - 1f,
                322f, 182f,
                gemGlow);
        }
    }

    #endregion

    #region Cleanup

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void OnEnd(Level level)
    {
        // Set completion flag
        string flag = isPinkPlatinum ? FLAG_PINK_PLAT_CUTSCENE_DONE : FLAG_GOLDEN_CUTSCENE_DONE;
        level.Session.SetFlag(flag, true);

        // Restore player state
        if (player != null)
        {
            player.StateMachine.State = Player.StNormal; // StNormal
            player.DummyAutoAnimate = true;
            player.DummyGravity = true;
            player.Speed = Vector2.Zero;
        }

        // Clean up NPCs and heart gem
        asriel?.RemoveSelf();
        granny?.RemoveSelf();
        heartGem?.RemoveSelf();

        // Restore level state
        level.TimerStopped = false;
        level.TimerHidden = false;
        level.SaveQuitDisabled = false;

        // Reset zoom
        level.ZoomSnap(new Vector2(160f, 90f), 1f);
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        CleanupNPCs();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        CleanupNPCs();
    }

    private void CleanupNPCs()
    {
        asriel?.RemoveSelf();
        granny?.RemoveSelf();
        heartGem?.RemoveSelf();
    }

    #endregion
}

/// <summary>
/// Trigger entity that starts the Final Remix Berry Cutscene when the player
/// collects the Golden Berry or Pink Platinum Berry in the final remix chapters.
/// Place this trigger in the collection area of CH19-21.
/// </summary>
[CustomEntity(ids: "MaggyHelper/FinalRemixBerryCutsceneTrigger")]
[Tracked]
public class FinalRemixBerryCutsceneTrigger : Trigger
{
    private bool triggered;

    public FinalRemixBerryCutsceneTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        triggered = false;
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        if (triggered) return;

        Level level = Scene as Level;
        if (level == null) return;

        // Check if the player is carrying a Golden Berry
        bool hasGolden = false;
        bool hasPinkPlat = false;

        foreach (Follower follower in player.Leader.Followers)
        {
            if (follower.Entity is GoldenStrawberry)
            {
                hasGolden = true;
            }
            else if (follower.Entity is PinkPlatinumBerry)
            {
                hasPinkPlat = true;
            }
        }

        if (!hasGolden && !hasPinkPlat) return;

        // Determine which cutscene to play (Pink Platinum takes priority)
        bool isPinkPlat = hasPinkPlat;

        // Check if already played
        string flag = isPinkPlat
            ? "final_remix_pink_plat_berry_cutscene"
            : "final_remix_golden_berry_cutscene";

        if (level.Session.GetFlag(flag)) return;

        // Start the cutscene
        triggered = true;
        Scene.Add(new CS_FinalRemixBerryCutscene(player, isPinkPlat));
    }
}
