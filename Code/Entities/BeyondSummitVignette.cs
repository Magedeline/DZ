using System.Threading.Tasks;

namespace Celeste.Entities;

/// <summary>
/// A vignette/intro screen for the beyond-summit chapter.
/// Displays collected gems orbiting a central point with layered backgrounds,
/// then transitions into the level. Similar to vanilla SummitVignette but for 7 gems.
/// Can be triggered as a standalone entity or used as a chapter intro.
/// </summary>
[HotReloadable]
public class BeyondSummitVignette : Scene
{
    private const int TotalGems = BeyondSummitGem.GemCount;
    private const string VignetteMusicEvent = "event:/music/pusheen/lvl9/main";

    private Session session;
    private HiresSnow snow;
    private float fade = 1f;
    private float timer;
    private bool ready;

    // Gem display
    private readonly MTexture[] gemTextures = new MTexture[TotalGems];
    private readonly bool[] gemCollected = new bool[TotalGems];
    private readonly float[] gemRotations = new float[TotalGems];
    private readonly float[] gemScales = new float[TotalGems];
    private readonly float[] gemAlphas = new float[TotalGems];
    private float orbitRadius = 80f;
    private float orbitAngle;

    // Background
    private readonly MTexture[] introLayers = new MTexture[20];
    private static readonly string[] IntroLayerNames =
    {
        "00", "01a", "01b", "02a", "02b", "03", "04", "05", "06",
        "07a", "07b", "08", "09a", "09b", "10", "11", "12a", "12b", "13", "14"
    };
    private const float IntroLayerScale = 1.5f;
    private MTexture mountainBg;
    private float bgAlpha;

    // Text
    private string titleText;
    private float titleAlpha;

    public BeyondSummitVignette(Session session, HiresSnow snow = null)
    {
        this.session = session;
        this.snow = snow ?? new HiresSnow();
        Add(snow);

        titleText = Dialog.Has("MaggyHelper_BEYONDSUMMIT_TITLE")
            ? Dialog.Clean("MaggyHelper_BEYONDSUMMIT_TITLE")
            : "BEYOND THE SUMMIT";

        // Force the intended Chapter 9 music for this intro vignette.
        Audio.SetMusic(VignetteMusicEvent, true, true);

        // Load gem textures and state
        for (int i = 0; i < TotalGems; i++)
        {
            string path = "collectables/summitgems/" + i + "/gem00";
            gemTextures[i] = GFX.Game.Has(path) ? GFX.Game[path] : null;
            gemCollected[i] = session.GetFlag("beyondsummit_gem_" + i);
            gemRotations[i] = (i / (float)TotalGems) * MathF.PI * 2f;
            gemScales[i] = 0f;
            gemAlphas[i] = 0f;
        }

        // Load mountain background
        string bgPath = "bgs/maggy/beyondsummit/vignette_bg";
        mountainBg = GFX.Game.Has(bgPath) ? GFX.Game[bgPath] : null;

        // Load layered BeyondSummitIntro images if present.
        for (int i = 0; i < IntroLayerNames.Length; i++)
        {
            string layerPath = "BeyondSummitIntro/" + IntroLayerNames[i];
            introLayers[i] = GFX.Game.Has(layerPath) ? GFX.Game[layerPath] : null;
        }

        Entity routineHolder = new Entity();
        routineHolder.Add(new Coroutine(VignetteRoutine()));
        Add(routineHolder);
    }

    private IEnumerator VignetteRoutine()
    {
        // Fade in
        float fadeDuration = 1.2f;
        float t = 0f;
        while (t < 1f)
        {
            t = Calc.Approach(t, 1f, Engine.DeltaTime / fadeDuration);
            fade = 1f - t;
            bgAlpha = t * 0.6f;
            yield return null;
        }

        yield return 0.3f;

        // Reveal gems one by one
        for (int i = 0; i < TotalGems; i++)
        {
            if (gemCollected[i])
            {
                Audio.Play("event:/game/pusheen/09_beyondsummit/gem_unlock_" + (i + 1));

                float revealT = 0f;
                while (revealT < 1f)
                {
                    revealT = Calc.Approach(revealT, 1f, Engine.DeltaTime / 0.3f);
                    gemScales[i] = Ease.BackOut(revealT);
                    gemAlphas[i] = revealT;
                    yield return null;
                }
                gemScales[i] = 1f;
                gemAlphas[i] = 1f;

                yield return 0.15f;
            }
            else
            {
                // Show dim placeholder
                float revealT = 0f;
                while (revealT < 1f)
                {
                    revealT = Calc.Approach(revealT, 1f, Engine.DeltaTime / 0.2f);
                    gemScales[i] = Ease.SineOut(revealT) * 0.5f;
                    gemAlphas[i] = revealT * 0.2f;
                    yield return null;
                }
            }
        }

        yield return 0.4f;

        // Show title
        t = 0f;
        while (t < 1f)
        {
            t = Calc.Approach(t, 1f, Engine.DeltaTime / 0.8f);
            titleAlpha = Ease.SineOut(t);
            yield return null;
        }

        yield return 0.8f;

        // If all gems collected, play completion fanfare
        if (BeyondSummitGem.CheckAllCollected(session))
        {
            Audio.Play("event:/game/pusheen/09_beyondsummit/gem_unlock_complete");

            // Pulse all gems
            for (int pulse = 0; pulse < 3; pulse++)
            {
                for (int i = 0; i < TotalGems; i++)
                    gemScales[i] = 1.3f;
                yield return 0.1f;
                for (int i = 0; i < TotalGems; i++)
                    gemScales[i] = 1f;
                yield return 0.15f;
            }

            yield return 0.5f;
        }

        ready = true;

        // Wait for input
        while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            yield return null;

        // Fade out and transition to level
        t = 0f;
        while (t < 1f)
        {
            t = Calc.Approach(t, 1f, Engine.DeltaTime / 1f);
            fade = t;
            titleAlpha = 1f - t;
            for (int i = 0; i < TotalGems; i++)
                gemAlphas[i] *= 1f - Engine.DeltaTime * 2f;
            yield return null;
        }

        // Transition to level
        LevelEnter.Go(session, fromSaveData: false);
    }

    public override void Update()
    {
        base.Update();
        timer += Engine.DeltaTime;
        orbitAngle += Engine.DeltaTime * 0.3f;

        // Update gem rotations
        for (int i = 0; i < TotalGems; i++)
        {
            gemRotations[i] += Engine.DeltaTime * 0.5f;
        }

        if (snow != null)
            snow.Alpha = Calc.Approach(snow.Alpha, ready ? 0.3f : 0.6f, Engine.DeltaTime);
    }

    public override void Render()
    {
        Draw.SpriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            null, null, null,
            Engine.ScreenMatrix
        );

        // Clear background
        Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black);

        // Draw mountain background
        void DrawIntroLayer(int index, float alphaMul = 1f)
        {
            if (bgAlpha <= 0f || index < 0 || index >= introLayers.Length || introLayers[index] == null)
                return;

            // The imported layers are authored at 1280x720; scale to 1920x1080.
            introLayers[index].DrawCentered(
                new Vector2(960f, 540f),
                Color.White * bgAlpha * alphaMul,
                IntroLayerScale
            );
        }

        // Back stack: sky glow, distant clouds, ambient particles, and birds.
        DrawIntroLayer(0, 0.95f);   // 00
        DrawIntroLayer(1, 0.9f);    // 01a
        DrawIntroLayer(2, 0.9f);    // 01b
        DrawIntroLayer(3, 0.85f);   // 02a
        DrawIntroLayer(4, 0.85f);   // 02b
        DrawIntroLayer(5, 0.8f);    // 03
        DrawIntroLayer(7, 0.7f);    // 05
        DrawIntroLayer(15, 0.55f);  // 11
        DrawIntroLayer(16, 0.6f);   // 12a
        DrawIntroLayer(17, 0.6f);   // 12b

        if (mountainBg != null && bgAlpha > 0f)
        {
            mountainBg.DrawCentered(
                new Vector2(960f, 540f),
                Color.White * bgAlpha
            );
        }

        // Front stack: near trees, foreground snow bands, and hero props.
        DrawIntroLayer(6, 0.8f);    // 04
        DrawIntroLayer(8, 0.9f);    // 06
        DrawIntroLayer(9, 0.9f);    // 07a
        DrawIntroLayer(10, 0.9f);   // 07b
        DrawIntroLayer(11, 1f);     // 08
        DrawIntroLayer(12, 0.85f);  // 09a
        DrawIntroLayer(13, 0.85f);  // 09b
        DrawIntroLayer(14, 0.95f);  // 10
        DrawIntroLayer(18, 1f);     // 13
        DrawIntroLayer(19, 1f);     // 14 (optional top-most overlay)

        // Draw gems orbiting center
        Vector2 center = new Vector2(960f, 460f);
        for (int i = 0; i < TotalGems; i++)
        {
            if (gemTextures[i] == null || gemAlphas[i] <= 0f) continue;

            float angle = (i / (float)TotalGems) * MathF.PI * 2f + orbitAngle - MathF.PI / 2f;
            float radius = orbitRadius + MathF.Sin(timer * 1.5f + i) * 4f;
            Vector2 pos = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;

            Color color;
            if (gemCollected[i])
                color = BeyondSummitGem.GemColors[i % BeyondSummitGem.GemColors.Length] * gemAlphas[i];
            else
                color = Color.Gray * gemAlphas[i] * 0.5f;

            float scale = gemScales[i] * 2f;
            gemTextures[i].DrawCentered(pos, color, scale, gemRotations[i]);
        }

        // Draw title text
        if (titleAlpha > 0f)
        {
            ActiveFont.DrawOutline(
                titleText,
                new Vector2(960f, 620f),
                new Vector2(0.5f, 0.5f),
                Vector2.One * 1.2f,
                Color.White * titleAlpha,
                2f,
                Color.Black * titleAlpha * 0.6f
            );

            // Draw "press to continue" prompt
            if (ready && timer % 1f > 0.5f)
            {
                ActiveFont.DrawOutline(
                    Dialog.Has("MaggyHelper_BEYONDSUMMIT_CONTINUE") 
                        ? Dialog.Clean("MaggyHelper_BEYONDSUMMIT_CONTINUE") 
                        : "Press CONFIRM to continue",
                    new Vector2(960f, 700f),
                    new Vector2(0.5f, 0.5f),
                    Vector2.One * 0.6f,
                    Color.White * titleAlpha * 0.7f,
                    1f,
                    Color.Black * titleAlpha * 0.4f
                );
            }
        }

        // Fade overlay
        if (fade > 0f)
        {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * fade);
        }

        Draw.SpriteBatch.End();
    }

    public override void End()
    {
        base.End();
    }
}
