using System.Collections;
using Celeste.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// VHS-style C-Side intro remix cutscene. Even more distorted than the B-Side version
/// with heavier tracking artifacts, more aggressive color corruption, and a
/// "damaged tape" aesthetic to convey the increased difficulty.
/// 
/// Plays when entering a C-Side chapter for the first time.
/// </summary>
public class CS_Gen_IntroRemix_CSide : Scene
{
    private readonly Session session;

    // VHS visual effect state (more aggressive than B-Side)
    private float vhsTimer;
    private float trackingOffset;
    private float scanlineAlpha = 0.25f;  // Heavier than B-Side
    private float staticNoise;
    private float colorShift;
    private float tapeInsertProgress;
    private bool tapeInserted;
    private bool showingRemixTitle;
    private bool canSkip;
    private float titleAlpha;
    private float overallAlpha;
    private float glitchIntensity;
    private float horizontalTear;
    private int tearY;

    // Chapter info
    private string chapterName;
    private MTexture bgTexture;
    private MTexture vhsOverlay;

    // Audio
    private string bgMusicEvent;

    // Timing (faster pacing for C-Side intensity)
    private const float TAPE_INSERT_DURATION = 1.5f;
    private const float STATIC_INTRO_DURATION = 2.0f;  // Longer static for tension
    private const float TITLE_HOLD_DURATION = 3.0f;
    private const float FADE_OUT_DURATION = 0.8f;

    // C-Side specific: tape damage simulation
    private float tapeWarpAmount;
    private float tapeSpeedVariation = 1f;
    private bool tapeJammed;

    public bool CanPause { get; private set; } = true;

    public CS_Gen_IntroRemix_CSide(Session session)
    {
        this.session = session;
    }

    public override void Begin()
    {
        base.Begin();

        // Get chapter name from area data
        var areaData = AreaData.Get(session.Area);
        chapterName = areaData?.Name != null ? Dialog.Clean(areaData.Name) : "Unknown Chapter";

        // Determine background music event for this C-Side
        bgMusicEvent = areaData?.Mode?.Length > 2
            ? areaData.Mode[2]?.AudioState?.Music?.Event
            : null;

        // Try to load VHS textures
        LoadTextures(areaData);

        // Start the cutscene coroutine (Scene.Add requires Entity, not Component)
        var entity = new Entity();
        entity.Add(new Coroutine(VHSDamagedTapeRoutine()));
        Add(entity);
    }

    private void LoadTextures(AreaData areaData)
    {
        try
        {
            string chapterKey = areaData?.SID?.Split('/').LastOrDefault() ?? "";
            string bgPath = $"cside_intros/{chapterKey}";

            if (GFX.Gui.Has(bgPath))
                bgTexture = GFX.Gui[bgPath];
            else if (GFX.Gui.Has("cside_intros/default"))
                bgTexture = GFX.Gui["cside_intros/default"];

            if (GFX.Gui.Has("vhs/overlay_damaged"))
                vhsOverlay = GFX.Gui["vhs/overlay_damaged"];
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "DZ", $"Failed to load C-Side VHS textures: {ex.Message}");
        }
    }

    /// <summary>
    /// C-Side damaged tape VHS sequence:
    /// 1. Heavy static â†’ "damaged tape" loading with glitches
    /// 2. Tape jams briefly â†’ resumes with warped audio pitch
    /// 3. Title appears through heavy VHS corruption
    /// 4. Horizontal tear effects + color bleeding
    /// 5. Signal loss â†’ hard cut to gameplay
    /// </summary>
    private IEnumerator VHSDamagedTapeRoutine()
    {
        // Phase 1: Heavy VHS static with damaged tape feel
        overallAlpha = 0f;
        yield return DamagedStaticIntro();

        // Phase 2: Tape insert with jamming
        yield return DamagedTapeInsert();

        // Phase 3: Tape jam! Brief freeze
        yield return TapeJamSequence();

        // Phase 4: Title through corruption
        yield return ShowCorruptedTitle();

        // Phase 5: Signal loss transition
        yield return SignalLossOut();

        // Start the actual C-Side level
        StartLevel();
    }

    /// <summary>Heavy static with increasing VHS distortion</summary>
    private IEnumerator DamagedStaticIntro()
    {
        Audio.Play("event:/pusheen/ui/vhs_tape_damaged");

        for (float t = 0f; t < STATIC_INTRO_DURATION; t += Engine.DeltaTime)
        {
            float progress = t / STATIC_INTRO_DURATION;
            overallAlpha = Ease.CubeIn(progress);
            staticNoise = 0.9f;

            // Flickering brightness (damaged tape)
            if (Calc.Random.NextFloat() > 0.85f)
                overallAlpha *= Calc.Random.Range(0.3f, 1f);

            // Random horizontal tears
            if (Calc.Random.NextFloat() > 0.9f)
            {
                tearY = Calc.Random.Next(1080);
                horizontalTear = Calc.Random.Range(10f, 50f);
            }
            else
            {
                horizontalTear = Calc.Approach(horizontalTear, 0f, Engine.DeltaTime * 100f);
            }

            yield return null;
        }
        overallAlpha = 1f;
    }

    /// <summary>Tape insert with intermittent jamming sounds</summary>
    private IEnumerator DamagedTapeInsert()
    {
        for (float t = 0f; t < TAPE_INSERT_DURATION; t += Engine.DeltaTime)
        {
            float progress = t / TAPE_INSERT_DURATION;
            tapeInsertProgress = Ease.CubeOut(progress);

            // More aggressive tracking than B-Side
            trackingOffset = (float)Math.Sin(t * 25f) * (1f - tapeInsertProgress) * 40f;

            // Tape speed variation (warping sound)
            tapeSpeedVariation = 1f + (float)Math.Sin(t * 8f) * (1f - progress) * 0.3f;
            tapeWarpAmount = (1f - progress) * 0.5f;

            staticNoise = (1f - tapeInsertProgress) * 0.6f;

            yield return null;
        }

        tapeInserted = true;
        tapeInsertProgress = 1f;
    }

    /// <summary>Tape jams briefly â€” screen freezes with loud click</summary>
    private IEnumerator TapeJamSequence()
    {
        tapeJammed = true;
        Audio.Play("event:/pusheen/ui/vhs_tape_jam");

        // Freeze frame with heavy static
        staticNoise = 0.7f;
        trackingOffset = 15f;

        yield return 0.8f;

        // Tape unjams with a pop
        tapeJammed = false;
        Audio.Play("event:/pusheen/ui/vhs_tape_resume");

        // Quick tracking recovery
        for (float t = 0f; t < 0.5f; t += Engine.DeltaTime)
        {
            trackingOffset = 15f * (1f - Ease.CubeOut(t / 0.5f));
            staticNoise = 0.7f * (1f - Ease.CubeOut(t / 0.5f));
            yield return null;
        }

        trackingOffset = 0f;
        staticNoise = 0.05f;

        // Start C-Side remix music
        if (!string.IsNullOrEmpty(bgMusicEvent))
        {
            Audio.SetMusic(bgMusicEvent);
        }

        yield return 0.3f;
    }

    /// <summary>Title with heavy VHS corruption effects</summary>
    private IEnumerator ShowCorruptedTitle()
    {
        showingRemixTitle = true;
        canSkip = true;

        // Fade in title with corruption
        for (float t = 0f; t < 0.8f; t += Engine.DeltaTime)
        {
            titleAlpha = Ease.CubeOut(t / 0.8f);
            glitchIntensity = (1f - t / 0.8f) * 0.5f;

            // Heavy tracking glitches
            if (vhsTimer % 0.15f < Engine.DeltaTime)
            {
                trackingOffset = Calc.Random.NextFloat(10f) - 5f;
                if (Calc.Random.NextFloat() > 0.8f)
                {
                    tearY = Calc.Random.Next(1080);
                    horizontalTear = Calc.Random.Range(20f, 80f);
                }
            }

            yield return null;

            if (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed)
                yield break;
        }

        titleAlpha = 1f;

        // Hold title with aggressive VHS effects
        for (float t = 0f; t < TITLE_HOLD_DURATION; t += Engine.DeltaTime)
        {
            vhsTimer += Engine.DeltaTime;

            // More frequent tracking distortion than B-Side
            if (vhsTimer % 1f < Engine.DeltaTime)
                trackingOffset = Calc.Random.NextFloat(8f) - 4f;
            else
                trackingOffset = Calc.Approach(trackingOffset, 0f, Engine.DeltaTime * 6f);

            // Heavy color corruption
            colorShift = (float)Math.Sin(vhsTimer * 5f) * 4f;

            // Periodic horizontal tears
            if (vhsTimer % 0.6f < Engine.DeltaTime && Calc.Random.NextFloat() > 0.5f)
            {
                tearY = Calc.Random.Next(1080);
                horizontalTear = Calc.Random.Range(15f, 60f);
            }
            else
            {
                horizontalTear = Calc.Approach(horizontalTear, 0f, Engine.DeltaTime * 80f);
            }

            // Random static bursts (more frequent than B-Side)
            if (vhsTimer % 0.5f < Engine.DeltaTime && Calc.Random.NextFloat() > 0.5f)
                staticNoise = Calc.Random.Range(0.2f, 0.5f);
            else
                staticNoise = Calc.Approach(staticNoise, 0.05f, Engine.DeltaTime * 3f);

            // Glitch intensity oscillation
            glitchIntensity = (float)Math.Sin(vhsTimer * 2f) * 0.15f;

            yield return null;

            if (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed)
                yield break;
        }
    }

    /// <summary>Signal loss effect â€” screen tears apart then cuts to black</summary>
    private IEnumerator SignalLossOut()
    {
        canSkip = false;

        // Signal degradation
        for (float t = 0f; t < FADE_OUT_DURATION; t += Engine.DeltaTime)
        {
            float progress = t / FADE_OUT_DURATION;

            // Screen tears apart
            trackingOffset = (float)Math.Sin(t * 60f) * progress * 50f;
            horizontalTear = progress * 100f;
            tearY = (int)(540f + (float)Math.Sin(t * 20f) * progress * 300f);

            // Signal loss static
            staticNoise = progress;
            colorShift = progress * 20f;
            glitchIntensity = progress;

            // Brightness flicker out
            overallAlpha = 1f - Ease.CubeIn(progress * 0.8f);

            yield return null;
        }

        // Hard cut to black
        overallAlpha = 0f;
        yield return 0.3f;
    }

    private void StartLevel()
    {
        Audio.SetMusic(null);
        Engine.Scene = new LevelLoader(session);
    }

    public override void Update()
    {
        base.Update();
        vhsTimer += Engine.DeltaTime;

        if (canSkip && (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed))
        {
            Audio.Play("event:/ui/main/button_lowkey");
            StartLevel();
        }
    }

    public override void Render()
    {
        base.Render();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);

        var screenBounds = new Rectangle(0, 0, 1920, 1080);

        // Black background
        Draw.Rect(screenBounds, Color.Black);

        if (overallAlpha > 0f)
        {
            // VHS static noise (heavier than B-Side)
            RenderHeavyStatic(screenBounds);

            // Background image with corruption
            if (bgTexture != null && tapeInserted)
            {
                RenderCorruptedBackground(screenBounds);
            }

            // Horizontal tear effect (C-Side exclusive)
            RenderHorizontalTears(screenBounds);

            // Heavy scanlines
            RenderDamagedScanlines(screenBounds);

            // Glitch blocks (C-Side exclusive)
            if (glitchIntensity > 0f)
            {
                RenderGlitchBlocks(screenBounds);
            }

            // Remix title with corruption
            if (showingRemixTitle && titleAlpha > 0f)
            {
                RenderCorruptedTitle();
            }

            // Tracking bars
            RenderHeavyTrackingBars(screenBounds);

            // VHS overlay
            RenderVHSOverlay();

            // Tape jam indicator
            if (tapeJammed)
            {
                RenderTapeJamIndicator();
            }
        }

        Draw.SpriteBatch.End();
    }

    private void RenderHeavyStatic(Rectangle bounds)
    {
        if (staticNoise <= 0f) return;

        var rng = new Pcg32Random(unchecked((uint)(int)(vhsTimer * 1500f)));
        int dotCount = (int)(staticNoise * 500);  // More dots than B-Side

        for (int i = 0; i < dotCount; i++)
        {
            int x = rng.Next(bounds.Width);
            int y = rng.Next(bounds.Height);
            int w = rng.Next(1, 12);  // Wider noise bands
            int h = rng.Next(1, 4);
            float brightness = (float)rng.NextDouble();

            Draw.Rect(x, y + trackingOffset, w, h,
                Color.White * (staticNoise * brightness * overallAlpha * 0.6f));
        }
    }

    private void RenderCorruptedBackground(Rectangle bounds)
    {
        // Heavier color corruption than B-Side
        float r = 1f + colorShift * 0.02f + glitchIntensity * 0.1f;
        float g = 1f - colorShift * 0.01f - glitchIntensity * 0.05f;
        float b = 1f + colorShift * 0.015f + glitchIntensity * 0.08f;

        var tint = new Color(
            (int)MathHelper.Clamp(r * 255, 0, 255),
            (int)MathHelper.Clamp(g * 255, 0, 255),
            (int)MathHelper.Clamp(b * 255, 0, 255)
        ) * overallAlpha;

        // Warp the image vertically based on tape damage
        float warpY = tapeWarpAmount * (float)Math.Sin(vhsTimer * 10f) * 10f;
        bgTexture.Draw(new Vector2(0, trackingOffset + warpY), Vector2.Zero, tint);
    }

    private void RenderHorizontalTears(Rectangle bounds)
    {
        if (horizontalTear <= 0f) return;

        // Horizontal tear â€” offset a strip of the screen
        int stripHeight = (int)MathHelper.Clamp(horizontalTear * 0.5f, 2, 30);
        float offset = horizontalTear * (float)Math.Sin(vhsTimer * 50f);

        Draw.Rect(offset, tearY, bounds.Width, stripHeight,
            Color.White * (0.3f * overallAlpha));
        Draw.Rect(0, tearY - 1, bounds.Width, 1,
            Color.Black * (0.5f * overallAlpha));
        Draw.Rect(0, tearY + stripHeight, bounds.Width, 1,
            Color.Black * (0.5f * overallAlpha));
    }

    private void RenderDamagedScanlines(Rectangle bounds)
    {
        // Thicker, more visible scanlines for C-Side
        for (int y = 0; y < bounds.Height; y += 2)
        {
            float intensity = scanlineAlpha;
            // Varying intensity for damaged tape effect
            if (y % 6 == 0)
                intensity *= 1.5f;

            Draw.Rect(0, y + trackingOffset, bounds.Width, 1,
                Color.Black * (intensity * overallAlpha));
        }
    }

    private void RenderGlitchBlocks(Rectangle bounds)
    {
        // Random rectangular glitch blocks (data corruption effect)
        var rng = new Pcg32Random(unchecked((uint)(int)(vhsTimer * 2000f)));
        int blockCount = (int)(glitchIntensity * 10);

        for (int i = 0; i < blockCount; i++)
        {
            int x = rng.Next(bounds.Width);
            int y = rng.Next(bounds.Height);
            int w = rng.Next(20, 200);
            int h = rng.Next(5, 30);

            Color blockColor = rng.NextDouble() > 0.5f
                ? new Color(rng.Next(256), rng.Next(256), rng.Next(256))
                : Color.Black;

            Draw.Rect(x, y, w, h, blockColor * (glitchIntensity * overallAlpha * 0.4f));
        }
    }

    private void RenderCorruptedTitle()
    {
        float centerX = 960f;
        float centerY = 540f;
        float wobbleX = (float)Math.Sin(vhsTimer * 6f) * colorShift;
        float wobbleY = trackingOffset;

        // Heavier chromatic aberration than B-Side
        float aberration = 4f + colorShift * 0.5f + glitchIntensity * 8f;

        // Red channel
        ActiveFont.DrawOutline(
            chapterName,
            new Vector2(centerX + aberration + wobbleX, centerY - 40 + wobbleY),
            new Vector2(0.5f, 0.5f),
            Vector2.One * 1.8f,
            Color.Red * (titleAlpha * overallAlpha * 0.4f),
            2f, Color.Transparent
        );

        // Blue channel (instead of cyan for more intensity)
        ActiveFont.DrawOutline(
            chapterName,
            new Vector2(centerX - aberration + wobbleX, centerY - 40 + wobbleY - aberration * 0.3f),
            new Vector2(0.5f, 0.5f),
            Vector2.One * 1.8f,
            Color.Blue * (titleAlpha * overallAlpha * 0.4f),
            2f, Color.Transparent
        );

        // Green channel offset
        ActiveFont.DrawOutline(
            chapterName,
            new Vector2(centerX + wobbleX, centerY - 40 + wobbleY + aberration * 0.2f),
            new Vector2(0.5f, 0.5f),
            Vector2.One * 1.8f,
            Color.Green * (titleAlpha * overallAlpha * 0.2f),
            2f, Color.Transparent
        );

        // Main white text
        ActiveFont.DrawOutline(
            chapterName,
            new Vector2(centerX + wobbleX, centerY - 40 + wobbleY),
            new Vector2(0.5f, 0.5f),
            Vector2.One * 1.8f,
            Color.White * (titleAlpha * overallAlpha),
            2f, Color.Black
        );

        // "C-SIDE REMIX" subtitle â€” gold color with heavier distortion
        float subtitleWobble = (float)Math.Sin(vhsTimer * 8f) * 3f;
        ActiveFont.DrawOutline(
            "C-SIDE REMIX",
            new Vector2(centerX + wobbleX + subtitleWobble, centerY + 50 + wobbleY),
            new Vector2(0.5f, 0.5f),
            Vector2.One * 1.0f,
            Color.Gold * (titleAlpha * overallAlpha * 0.9f),
            2f, Color.Black
        );

        // Skip prompt
        if (canSkip)
        {
            float blinkAlpha = (float)(Math.Sin(vhsTimer * 4f) * 0.3f + 0.7f);
            ActiveFont.Draw(
                "â–º PLAY",
                new Vector2(100f, 980f),
                Vector2.Zero,
                Vector2.One * 0.6f,
                Color.White * (titleAlpha * overallAlpha * blinkAlpha)
            );
        }
    }

    private void RenderHeavyTrackingBars(Rectangle bounds)
    {
        if (Math.Abs(trackingOffset) < 0.5f) return;

        // Multiple tracking bars for more distortion
        for (int i = 0; i < 3; i++)
        {
            float barY = ((vhsTimer * (80f + i * 30f)) + i * 200f) % bounds.Height;
            float barHeight = Math.Abs(trackingOffset) * (1.5f + i * 0.5f);

            Draw.Rect(0, barY, bounds.Width, barHeight,
                Color.White * (0.15f * overallAlpha));
            Draw.Rect(0, barY + barHeight, bounds.Width, 2,
                Color.Black * (0.4f * overallAlpha));
        }
    }

    private void RenderVHSOverlay()
    {
        // VHS timestamp with distortion
        if (!tapeInserted) return;

        string timestamp = DateTime.Now.ToString("MM/dd/yyyy  hh:mm:ss tt");
        float blinkAlpha = (float)(Math.Sin(vhsTimer * 2f) * 0.1f + 0.9f);

        // Timestamp with color corruption
        float tsColorShift = (float)Math.Sin(vhsTimer * 7f) * 2f;
        var tsColor = new Color(
            (int)MathHelper.Clamp(255 + tsColorShift * 20, 0, 255),
            (int)MathHelper.Clamp(255 - tsColorShift * 10, 0, 255),
            255
        );

        ActiveFont.Draw(
            timestamp,
            new Vector2(1750f, 1020f),
            new Vector2(1f, 1f),
            Vector2.One * 0.4f,
            tsColor * (overallAlpha * blinkAlpha * 0.5f)
        );

        // "REC â—" with flickering
        float recBlink = (float)(Math.Sin(vhsTimer * 1.5f) > 0 ? 1f : 0f);
        ActiveFont.Draw(
            "â— REC",
            new Vector2(1800f, 50f),
            new Vector2(1f, 0f),
            Vector2.One * 0.5f,
            Color.Red * (overallAlpha * recBlink * 0.9f)
        );

        // "SP" speed mode indicator
        ActiveFont.Draw(
            "SP",
            new Vector2(100f, 50f),
            Vector2.Zero,
            Vector2.One * 0.5f,
            Color.White * (overallAlpha * 0.6f)
        );
    }

    private void RenderTapeJamIndicator()
    {
        // Large "â–Œâ–Œ" pause symbol when tape is jammed
        ActiveFont.DrawOutline(
            "â–Œâ–Œ TRACKING",
            new Vector2(960f, 540f),
            new Vector2(0.5f, 0.5f),
            Vector2.One * 2f,
            Color.White * (overallAlpha * 0.8f),
            3f, Color.Black
        );
    }

    public override void End()
    {
        base.End();
        Audio.SetMusic(null);
    }
}

