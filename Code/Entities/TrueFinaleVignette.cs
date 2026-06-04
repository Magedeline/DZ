using System.Threading.Tasks;

namespace Celeste.Entities;

/// <summary>
/// A vignette/intro screen for Chapter 20: The Last Push (Desolo Zantas true finale).
/// Displays a dramatic title with layered backgrounds and a central emblem,
/// then transitions into the level. Similar to BeyondSummitVignette but adapted
/// for the true finale chapter.
/// </summary>
[HotReloadable]
public class TrueFinaleVignette : Scene
{
    private const string VignetteMusicEvent = "event:/new_content/music/pusheen/lvl21/climb";

    private Session session;
    private HiresSnow snow;
    private float fade = 1f;
    private float timer;
    private bool ready;

    // Central emblem
    private MTexture emblemTexture;
    private float emblemScale = 0f;
    private float emblemAlpha = 0f;
    private float emblemRotation;

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

    public TrueFinaleVignette(Session session, HiresSnow snow = null)
    {
        this.session = session;
        this.snow = snow ?? new HiresSnow();
        Add(snow);

        titleText = Dialog.Has("MaggyHelper_TRUEFINALE_TITLE")
            ? Dialog.Clean("MaggyHelper_TRUEFINALE_TITLE")
            : "THE LAST PUSH";

        // Force the intended Chapter 20 music for this intro vignette.
        Audio.SetMusic(VignetteMusicEvent, true, true);

        // Load central emblem texture
        string emblemPath = "collectables/heartgem/heartgem00";
        emblemTexture = GFX.Game.Has(emblemPath) ? GFX.Game[emblemPath] : null;

        // Load mountain background
        string bgPath = "bgs/maggy/truefinale/vignette_bg";
        mountainBg = GFX.Game.Has(bgPath) ? GFX.Game[bgPath] : null;

        // Load layered TrueFinaleIntro images if present.
        for (int i = 0; i < IntroLayerNames.Length; i++)
        {
            string layerPath = "TrueFinaleIntro/" + IntroLayerNames[i];
            introLayers[i] = GFX.Game.Has(layerPath) ? GFX.Game[layerPath] : null;
        }

        Entity routineHolder = new Entity();
        routineHolder.Add(new Coroutine(VignetteRoutine()));
        Add(routineHolder);
    }

    private IEnumerator VignetteRoutine()
    {
        // Fade in
        float fadeDuration = 1.5f;
        float t = 0f;
        while (t < 1f)
        {
            t = Calc.Approach(t, 1f, Engine.DeltaTime / fadeDuration);
            fade = 1f - t;
            bgAlpha = t * 0.6f;
            yield return null;
        }

        yield return 0.5f;

        // Reveal central emblem
        if (emblemTexture != null)
        {
            Audio.Play("guid://{1acb8e84-5f79-4dcd-86aa-e8a514535b7d}");

            float revealT = 0f;
            while (revealT < 1f)
            {
                revealT = Calc.Approach(revealT, 1f, Engine.DeltaTime / 0.6f);
                emblemScale = Ease.BackOut(revealT);
                emblemAlpha = revealT;
                yield return null;
            }
            emblemScale = 1f;
            emblemAlpha = 1f;

            yield return 0.3f;
        }

        // Pulse emblem
        for (int pulse = 0; pulse < 2; pulse++)
        {
            emblemScale = 1.2f;
            yield return 0.15f;
            emblemScale = 1f;
            yield return 0.2f;
        }

        yield return 0.3f;

        // Show title
        t = 0f;
        while (t < 1f)
        {
            t = Calc.Approach(t, 1f, Engine.DeltaTime / 1.0f);
            titleAlpha = Ease.SineOut(t);
            yield return null;
        }

        yield return 1.0f;

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
            emblemAlpha *= 1f - Engine.DeltaTime * 2f;
            yield return null;
        }

        // Transition to level
        LevelEnter.Go(session, fromSaveData: false);
    }

    public override void Update()
    {
        base.Update();
        timer += Engine.DeltaTime;
        emblemRotation += Engine.DeltaTime * 0.4f;

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

        // Draw central emblem
        if (emblemTexture != null && emblemAlpha > 0f)
        {
            Vector2 center = new Vector2(960f, 460f);
            float pulse = 1f + (float)Math.Sin(timer * 2f) * 0.05f;
            float scale = emblemScale * 3f * pulse;
            Color emblemColor = Color.White * emblemAlpha;
            emblemTexture.DrawCentered(center, emblemColor, scale, emblemRotation);
        }

        // Draw title text
        if (titleAlpha > 0f)
        {
            ActiveFont.DrawOutline(
                titleText,
                new Vector2(960f, 620f),
                new Vector2(0.5f, 0.5f),
                Vector2.One * 1.4f,
                Color.White * titleAlpha,
                2f,
                Color.Black * titleAlpha * 0.6f
            );

            // Draw "press to continue" prompt
            if (ready && timer % 1f > 0.5f)
            {
                ActiveFont.DrawOutline(
                    Dialog.Has("MaggyHelper_TRUEFINALE_CONTINUE")
                        ? Dialog.Clean("MaggyHelper_TRUEFINALE_CONTINUE")
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

