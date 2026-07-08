namespace Celeste.Entities;

/// <summary>
/// Decorative cloud entity for beyond-summit areas.
/// Drifts slowly across the screen with parallax depth.
/// Similar to vanilla SummitCloud but with customizable colors and behavior.
/// </summary>
[CustomEntity(ids: "DZ/BeyondSummitCloudEntity")]
[Tracked(true)]
[HotReloadable]
public class BeyondSummitCloud : Entity
{
    private struct CloudParticle
    {
        public Vector2 Position;
        public float Speed;
        public int TextureIndex;
        public float Alpha;
        public float Scale;
    }

    private readonly List<MTexture> textures;
    private readonly CloudParticle[] particles;
    private readonly Color cloudColor;
    private readonly Color highlightColor;
    private readonly bool dark;
    private readonly float scrollSpeedMultiplier;
    private readonly int particleCount;
    private float cameraOffsetY;

    public BeyondSummitCloud(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        dark = data.Bool("dark", false);
        scrollSpeedMultiplier = data.Float("speedMultiplier", 1f);
        particleCount = Math.Clamp(data.Int("particleCount", 16), 4, 64);

        if (dark)
        {
            cloudColor = Calc.HexToColor(data.Attr("color", "082644"));
            highlightColor = Calc.HexToColor(data.Attr("highlightColor", "0a3a6b"));
        }
        else
        {
            cloudColor = Calc.HexToColor(data.Attr("color", "b64a86"));
            highlightColor = Calc.HexToColor(data.Attr("highlightColor", "d988b7"));
        }

        Depth = -1000000;
        Tag = (int)Tags.TransitionUpdate;

        textures = GFX.Game.GetAtlasSubtextures("scenery/launch/cloud");

        particles = new CloudParticle[particleCount];
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i] = new CloudParticle
            {
                Position = new Vector2(
                    Calc.Random.NextFloat(320f),
                    Calc.Random.NextFloat(900f)
                ),
                Speed = Calc.Random.Range(80, 240) * scrollSpeedMultiplier,
                TextureIndex = Calc.Random.Next(textures.Count),
                Alpha = Calc.Random.Range(0.3f, 0.8f),
                Scale = Calc.Random.Range(0.6f, 1.3f)
            };
        }
    }

    public override void Update()
    {
        base.Update();

        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Position.Y += particles[i].Speed * Engine.DeltaTime;
        }

        Level level = SceneAs<Level>();
        if (level != null)
        {
            cameraOffsetY = level.Camera.Y;
        }
    }

    public override void Render()
    {
        if (textures.Count == 0) return;

        Level level = SceneAs<Level>();
        if (level == null) return;

        Vector2 cameraPos = level.Camera.Position;

        for (int i = 0; i < particles.Length; i++)
        {
            ref CloudParticle p = ref particles[i];
            Vector2 pos = p.Position;
            pos.Y = Mod(pos.Y, 900f) - 360f;
            Vector2 renderPos = pos + cameraPos;

            Color color = Color.Lerp(cloudColor, highlightColor, (float)Math.Sin(Scene.TimeActive + i) * 0.5f + 0.5f);
            MTexture tex = textures[p.TextureIndex % textures.Count];
            tex.DrawCentered(renderPos, color * p.Alpha, p.Scale);
        }
    }

    private static float Mod(float x, float m) => (x % m + m) % m;
}
