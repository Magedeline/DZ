using System.Collections;
using FMOD.Studio;
using Celeste.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Cutscenes;

/// <summary>
/// VHS-style B-Side intro remix cutscene, similar to vanilla Celeste's 
/// "cs_gen_intro_remix" but with VHS tape visual effects (scanlines, 
/// tracking noise, color distortion, tape-insert animation).
/// 
/// Plays when entering a B-Side chapter for the first time.
/// </summary>
public class CS_Gen_IntroRemix_BSide : Scene
{
    private readonly Session session;

    // VHS visual effect state
    private float vhsTimer;
    private float trackingOffset;
    private float scanlineAlpha = 0.15f;
    private float staticNoise;
    private float colorShift;
    private float tapeInsertProgress;
    private bool tapeInserted;
    private bool showingRemixTitle;
    private bool canSkip;
    private float titleAlpha;
    private float overallAlpha;

    // Chapter info
    private string chapterName;
    private string remixArtist;
    private MTexture bgTexture;
    private MTexture vhsOverlay;
    private MTexture tapeTexture;

    // Audio
    private string bgMusicEvent;

    // Timing
    private const float TAPE_INSERT_DURATION = 2.0f;
    private const float STATIC_INTRO_DURATION = 1.5f;
    private const float TITLE_HOLD_DURATION = 3.0f;
    private const float FADE_OUT_DURATION = 1.0f;

    public bool CanPause { get; private set; } = true;

    public CS_Gen_IntroRemix_BSide(Session session)
    {
        this.session = session;
    }

    public override void Begin()
    {
        base.Begin();

        // Get chapter name from area data
        var areaData = AreaData.Get(session.Area);
        chapterName = areaData?.Name != null ? Dialog.Clean(areaData.Name) : "Unknown Chapter";
        remixArtist = "B-Side Remix";

        // Determine background music event for this B-Side
        bgMusicEvent = areaData?.Mode?.Length > 1
            ? areaData.Mode[1]?.AudioState?.Music?.Event
            : null;

        // Try to load VHS textures
        LoadTextures(areaData);

        // Start the cutscene coroutine (Scene.Add requires Entity, not Component)
        var entity = new Entity();
        entity.Add(new Coroutine(VHSRemixRoutine()));
        Add(entity);
    }

    private void LoadTextures(AreaData areaData)
    {
        try
        {
            // Try loading chapter-specific B-Side art
            string chapterKey = areaData?.SID?.Split('/').LastOrDefault() ?? "";
            string bgPath = $"bside_intros/{chapterKey}";

            if (GFX.Gui.Has(bgPath))
                bgTexture = GFX.Gui[bgPath];
            else if (GFX.Gui.Has("bside_intros/default"))
                bgTexture = GFX.Gui["bside_intros/default"];

            if (GFX.Gui.Has("vhs/overlay"))
                vhsOverlay = GFX.Gui["vhs/overlay"];

            if (GFX.Gui.Has("vhs/tape"))
                tapeTexture = GFX.Gui["vhs/tape"];
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Failed to load VHS textures: {ex.Message}");
        }
    }

    /// <summary>
    /// Main VHS intro remix sequence:
    /// 1. Black screen → VHS static snow fades in
    /// 2. "Tape insert" visual effect with mechanical sound
    /// 3. VHS tracking lines + color shift as the remix title appears
    /// 4. Chapter name + "B-SIDE REMIX" text with VHS scanlines
    /// 5. VHS glitch transition → fade to gameplay
    /// </summary>
    private IEnumerator VHSRemixRoutine()
    {
        // Phase 1: VHS static intro
        overallAlpha = 0f;
        yield return VHSStaticIntro();

        // Phase 2: Tape insert animation
        yield return TapeInsertSequence();

        // Phase 3: Show remix title with VHS effects
        yield return ShowRemixTitle();

        // Phase 4: Glitch transition out
        yield return VHSGlitchOut();

        // Phase 5: Start the actual level
        StartLevel();
    }

    /// <summary>VHS static snow fading in from black</summary>
    private IEnumerator VHSStaticIntro()
    {
        // Play VHS tape insert SFX
        Audio.Play("event:/ui/pusheen/vhs_tape_insert");

        for (float t = 0f; t < STATIC_INTRO_DURATION; t += Engine.DeltaTime)
        {
            overallAlpha = Ease.CubeIn(t / STATIC_INTRO_DURATION);
            staticNoise = 0.8f + 0.2f * (float)Math.Sin(t * 30f);
            yield return null;
        }
        overallAlpha = 1f;
    }

    /// <summary>VHS tape being inserted with mechanical movement</summary>
    private IEnumerator TapeInsertSequence()
    {
        for (float t = 0f; t < TAPE_INSERT_DURATION; t += Engine.DeltaTime)
        {
            tapeInsertProgress = Ease.CubeOut(t / TAPE_INSERT_DURATION);

            // Tracking distortion during tape insertion
            trackingOffset = (float)Math.Sin(t * 15f) * (1f - tapeInsertProgress) * 20f;

            // Static noise decreases as tape settles
            staticNoise = (1f - tapeInsertProgress) * 0.5f;

            yield return null;
        }

        tapeInserted = true;
        tapeInsertProgress = 1f;
        trackingOffset = 0f;

        // Brief pause after tape insertion
        yield return 0.3f;

        // Play the B-Side remix music
        if (!string.IsNullOrEmpty(bgMusicEvent))
        {
            Audio.SetMusic(bgMusicEvent);
        }
    }

    /// <summary>Chapter title + B-SIDE REMIX with VHS scanlines</summary>
    private IEnumerator ShowRemixTitle()
    {
        showingRemixTitle = true;
        canSkip = true;

        // Fade in title
        for (float t = 0f; t < 0.5f; t += Engine.DeltaTime)
        {
            titleAlpha = Ease.CubeOut(t / 0.5f);

            // Occasional tracking glitch
            if (vhsTimer % 0.3f < Engine.DeltaTime)
                trackingOffset = Calc.Random.NextFloat(6f) - 3f;

            yield return null;

            if (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed)
                yield break;
        }

        titleAlpha = 1f;

        // Hold title with VHS effects
        for (float t = 0f; t < TITLE_HOLD_DURATION; t += Engine.DeltaTime)
        {
            vhsTimer += Engine.DeltaTime;

            // Periodic tracking distortion
            if (vhsTimer % 2f < Engine.DeltaTime)
                trackingOffset = Calc.Random.NextFloat(4f) - 2f;
            else
                trackingOffset = Calc.Approach(trackingOffset, 0f, Engine.DeltaTime * 8f);

            // Color shift wobble
            colorShift = (float)Math.Sin(vhsTimer * 3f) * 2f;

            // Random static bursts
            if (vhsTimer % 0.8f < Engine.DeltaTime && Calc.Random.NextFloat() > 0.7f)
                staticNoise = 0.3f;
            else
                staticNoise = Calc.Approach(staticNoise, 0.02f, Engine.DeltaTime * 2f);

            yield return null;

            if (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed)
                yield break;
        }
    }

    /// <summary>VHS glitch effect transitioning out</summary>
    private IEnumerator VHSGlitchOut()
    {
        canSkip = false;

        for (float t = 0f; t < FADE_OUT_DURATION; t += Engine.DeltaTime)
        {
            float progress = t / FADE_OUT_DURATION;
            overallAlpha = 1f - Ease.CubeIn(progress);
            titleAlpha = 1f - Ease.CubeIn(progress);

            // Heavy tracking/glitch during transition
            trackingOffset = (float)Math.Sin(t * 40f) * progress * 30f;
            staticNoise = progress * 0.8f;
            colorShift = progress * 10f;

            yield return null;
        }

        overallAlpha = 0f;
    }

    private void StartLevel()
    {
        // Clean up audio
        Audio.SetMusic(null);

        // Load into the actual B-Side level
        Engine.Scene = new LevelLoader(session);
    }

    public override void Update()
    {
        base.Update();
        vhsTimer += Engine.DeltaTime;

        // Allow skipping during title display
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
            // VHS static noise background
            RenderVHSStatic(screenBounds);

            // Background image (if available)
            if (bgTexture != null && tapeInserted)
            {
                RenderBackgroundWithVHSEffect(screenBounds);
            }

            // VHS scanlines overlay
            RenderScanlines(screenBounds);

            // Tape insert animation
            if (!tapeInserted && tapeTexture != null)
            {
                RenderTapeInsert();
            }

            // Remix title text
            if (showingRemixTitle && titleAlpha > 0f)
            {
                RenderRemixTitle();
            }

            // VHS tracking bars
            RenderTrackingBars(screenBounds);

            // VHS date/time stamp
            RenderVHSTimestamp();
        }

        Draw.SpriteBatch.End();
    }

    private void RenderVHSStatic(Rectangle bounds)
    {
        if (staticNoise <= 0f) return;

        // Draw random static noise pixels using rectangles
        var rng = new Pcg32Random(unchecked((uint)(int)(vhsTimer * 1000f)));
        int dotCount = (int)(staticNoise * 300);

        for (int i = 0; i < dotCount; i++)
        {
            int x = rng.Next(bounds.Width);
            int y = rng.Next(bounds.Height);
            int w = rng.Next(2, 8);
            int h = rng.Next(1, 3);
            float brightness = (float)rng.NextDouble();

            Draw.Rect(x, y + trackingOffset, w, h,
                Color.White * (staticNoise * brightness * overallAlpha * 0.5f));
        }
    }

    private void RenderBackgroundWithVHSEffect(Rectangle bounds)
    {
        // Draw background with VHS color shift
        float r = 1f + colorShift * 0.01f;
        float g = 1f - colorShift * 0.005f;
        float b = 1f + colorShift * 0.008f;

        var tint = new Color(
            (int)MathHelper.Clamp(r * 255, 0, 255),
            (int)MathHelper.Clamp(g * 255, 0, 255),
            (int)MathHelper.Clamp(b * 255, 0, 255)
        ) * overallAlpha;

        bgTexture.Draw(new Vector2(0, trackingOffset), Vector2.Zero, tint);
    }

    private void RenderScanlines(Rectangle bounds)
    {
        // Horizontal scanlines across the screen
        for (int y = 0; y < bounds.Height; y += 3)
        {
            Draw.Rect(0, y + trackingOffset, bounds.Width, 1,
                Color.Black * (scanlineAlpha * overallAlpha));
        }
    }

    private void RenderTapeInsert()
    {
        // Animate tape sliding in from bottom
        float tapeY = MathHelper.Lerp(1200f, 400f, tapeInsertProgress);
        tapeTexture.DrawCentered(new Vector2(960f, tapeY),
            Color.White * overallAlpha, 1f);
    }

    private void RenderRemixTitle()
    {
        float centerX = 960f;
        float centerY = 540f;

        // Chapter name with VHS wobble
        float wobbleX = (float)Math.Sin(vhsTimer * 4f) * colorShift * 0.5f;
        float wobbleY = trackingOffset;

        // Red/cyan chromatic aberration for VHS look
        float aberration = 2f + colorShift * 0.3f;

        // Draw text with chromatic aberration (red channel offset)
        ActiveFont.DrawOutline(
            chapterName,
            new Vector2(centerX + aberration + wobbleX, centerY - 40 + wobbleY),
            new Vector2(0.5f, 0.5f),
            Vector2.One * 1.8f,
            Color.Red * (titleAlpha * overallAlpha * 0.3f),
            2f, Color.Black * 0f
        );

        // Cyan channel offset
        ActiveFont.DrawOutline(
            chapterName,
            new Vector2(centerX - aberration + wobbleX, centerY - 40 + wobbleY),
            new Vector2(0.5f, 0.5f),
            Vector2.One * 1.8f,
            Color.Cyan * (titleAlpha * overallAlpha * 0.3f),
            2f, Color.Black * 0f
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

        // "B-SIDE REMIX" subtitle
        ActiveFont.DrawOutline(
            "B-SIDE REMIX",
            new Vector2(centerX + wobbleX, centerY + 50 + wobbleY),
            new Vector2(0.5f, 0.5f),
            Vector2.One * 1.0f,
            Color.Red * (titleAlpha * overallAlpha * 0.9f),
            2f, Color.Black
        );

        // Bottom "PLAY ►" indicator
        if (canSkip)
        {
            float blinkAlpha = (float)(Math.Sin(vhsTimer * 3f) * 0.3f + 0.7f);
            ActiveFont.Draw(
                "► PLAY",
                new Vector2(100f, 980f),
                Vector2.Zero,
                Vector2.One * 0.6f,
                Color.White * (titleAlpha * overallAlpha * blinkAlpha)
            );
        }
    }

    private void RenderTrackingBars(Rectangle bounds)
    {
        if (Math.Abs(trackingOffset) < 1f) return;

        // Draw horizontal tracking distortion bars
        float barY = (vhsTimer * 100f) % bounds.Height;
        float barHeight = Math.Abs(trackingOffset) * 2f;

        Draw.Rect(0, barY, bounds.Width, barHeight,
            Color.White * (0.2f * overallAlpha));
        Draw.Rect(0, barY + barHeight, bounds.Width, 1,
            Color.Black * (0.5f * overallAlpha));
    }

    private void RenderVHSTimestamp()
    {
        if (!tapeInserted) return;

        // VHS-style date/time in bottom-right corner
        string timestamp = DateTime.Now.ToString("MM/dd/yyyy  hh:mm:ss tt");
        float blinkAlpha = (float)(Math.Sin(vhsTimer * 2f) * 0.1f + 0.9f);

        ActiveFont.Draw(
            timestamp,
            new Vector2(1750f, 1020f),
            new Vector2(1f, 1f),
            Vector2.One * 0.4f,
            Color.White * (overallAlpha * blinkAlpha * 0.6f)
        );

        // "REC ●" indicator
        float recBlink = (float)(Math.Sin(vhsTimer * 1.5f) > 0 ? 1f : 0f);
        ActiveFont.Draw(
            "● REC",
            new Vector2(1800f, 50f),
            new Vector2(1f, 0f),
            Vector2.One * 0.5f,
            Color.Red * (overallAlpha * recBlink * 0.8f)
        );
    }

    public override void End()
    {
        base.End();
        Audio.SetMusic(null);
    }
}
