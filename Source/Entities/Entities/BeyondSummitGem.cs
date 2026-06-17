using System.Threading.Tasks;

namespace Celeste.Entities;

[CustomEntity(ids: "DZ/BeyondSummitGem")]
[Tracked(true)]
[HotReloadable]
public class BeyondSummitGem : Entity
{
    public static readonly Color[] GemColors = new Color[7]
    {
        Calc.HexToColor("9B3FB5"),  // 0 - purple
        Calc.HexToColor("3232FF"),  // 1 - blue
        Calc.HexToColor("E01919"),  // 2 - red
        Calc.HexToColor("1BBE1B"),  // 3 - green
        Calc.HexToColor("FFD700"),  // 4 - gold
        Calc.HexToColor("FF6A00"),  // 5 - orange
        Calc.HexToColor("FF1493"),  // 6 - pink
    };

    public const int GemCount = 7;

    public int GemID;
    private bool collected;
    private string spritePath;
    private Sprite sprite;
    private Wiggler scaleWiggler;
    private Wiggler moveWiggler;
    private Vector2 moveWiggleDir;
    private BloomPoint bloom;
    private VertexLight light;
    private float bounceSfxDelay;
    private Vector2 startPosition;
    private SineWave floatSine;
    private ParticleType P_Shimmer;

    public BeyondSummitGem(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        GemID = data.Int("gem", 0);
        Depth = -10010;
        Collider = new Hitbox(12f, 12f, -6f, -6f);

        spritePath = data.Attr("sprite", "");
        if (string.IsNullOrEmpty(spritePath))
            spritePath = "collectables/summitgems/" + GemID + "/gem";

        Add(sprite = new Sprite(GFX.Game, spritePath));
        sprite.AddLoop("idle", "", 0.08f);
        sprite.Play("idle");
        sprite.CenterOrigin();

        Add(scaleWiggler = Wiggler.Create(0.5f, 4f, v => sprite.Scale = Vector2.One * (1f + v * 0.3f)));
        Add(moveWiggler = Wiggler.Create(0.8f, 2f));
        moveWiggler.StartZero = true;

        Add(bloom = new BloomPoint(0.5f, 12f));
        Add(light = new VertexLight(Color.White, 0.5f, 16, 28));
        Add(floatSine = new SineWave(0.6f, 0f));
        floatSine.Randomize();

        Color gemColor = GemID < GemColors.Length ? GemColors[GemID] : Color.White;
        P_Shimmer = new ParticleType
        {
            Source = GFX.Game["particles/blob"],
            Color = gemColor,
            Color2 = Color.White,
            ColorMode = ParticleType.ColorModes.Blink,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 0.5f,
            SizeRange = 0.2f,
            SpeedMin = 4f,
            SpeedMax = 12f,
            DirectionRange = MathF.PI * 2f,
            LifeMin = 0.4f,
            LifeMax = 0.8f
        };
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        startPosition = Position;

        Level level = SceneAs<Level>();
        if (level.Session.GetFlag("beyondsummit_gem_" + GemID))
        {
            collected = true;
            Visible = false;
            Collidable = false;
        }
    }

    public override void Update()
    {
        base.Update();
        if (collected) return;

        bounceSfxDelay -= Engine.DeltaTime;
        Position = startPosition + Vector2.UnitY * floatSine.Value * 2f
            + moveWiggleDir * moveWiggler.Value * -8f;

        if (Scene.OnInterval(0.1f))
        {
            SceneAs<Level>().Particles.Emit(
                P_Shimmer,
                1,
                Center,
                Vector2.One * 6f
            );
        }

        CelestePlayer player = CollideFirst<CelestePlayer>();
        if (player != null)
        {
            Collect(player);
        }
    }

    private void Collect(CelestePlayer player)
    {
        if (collected) return;
        collected = true;
        Collidable = false;

        Level level = SceneAs<Level>();
        level.Session.SetFlag("beyondsummit_gem_" + GemID);

        Audio.Play("event:/pusheen/game/09_beyondsummit/gem_get", Position);

        Add(new Coroutine(CollectRoutine(player)));
    }

    private IEnumerator CollectRoutine(CelestePlayer player)
    {
        Level level = SceneAs<Level>();
        level.CanRetry = false;

        scaleWiggler.Start();

        Color gemColor = GemID < GemColors.Length ? GemColors[GemID] : Color.White;
        for (int i = 0; i < 12; i++)
        {
            level.Particles.Emit(
                P_Shimmer,
                Center + Calc.Random.Range(-Vector2.One * 4f, Vector2.One * 4f),
                gemColor
            );
        }

        Vector2 startPos = Position;
        Vector2 target = new Vector2(level.Camera.X + 160f, level.Camera.Y + 90f);

        float duration = 0.6f;
        float t = 0f;
        while (t < 1f)
        {
            t = Calc.Approach(t, 1f, Engine.DeltaTime / duration);
            Position = Vector2.Lerp(startPos, target, Ease.CubeIn(t));
            sprite.Scale = Vector2.One * (1f + t * 0.5f);
            bloom.Alpha = 0.5f + t * 0.5f;
            yield return null;
        }

        level.Flash(gemColor * 0.25f, drawPlayerOver: true);
        level.Shake(0.15f);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

        yield return 0.4f;

        level.CanRetry = true;
        RemoveSelf();
    }

    public override void Render()
    {
        if (!collected)
        {
            float glow = 0.8f + floatSine.Value * 0.2f;
            sprite.DrawOutline(1);
            base.Render();
        }
        else
        {
            base.Render();
        }
    }

    public static bool CheckAllCollected(Session session)
    {
        for (int i = 0; i < GemCount; i++)
        {
            if (!session.GetFlag("beyondsummit_gem_" + i))
                return false;
        }
        return true;
    }
}

