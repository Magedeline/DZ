namespace Celeste.Entities;

/// <summary>
/// Manages BeyondSummitGem collection and the gate/barrier that opens when all 7 gems are collected.
/// Placed in a room with nodes defining gem display positions around a central gate.
/// Plays unlock sounds sequentially and opens when all gems are gathered.
/// </summary>
[CustomEntity(ids: "DZ/BeyondSummitGemManager")]
[Tracked(true)]
[HotReloadable]
public class BeyondSummitGemManager : Entity
{
    private const int TotalGems = BeyondSummitGem.GemCount; // 7

    private readonly List<Sprite> gemSprites = new();
    private readonly List<Vector2> gemNodes = new();
    private readonly bool[] gemCollected = new bool[TotalGems];
    private readonly float[] gemAlpha = new float[TotalGems];
    private readonly float[] gemScale = new float[TotalGems];

    private bool allCollected;
    private bool opened;
    private readonly string unlockFlag;
    private VertexLight centerLight;
    private BloomPoint centerBloom;
    private Wiggler bounceWiggler;

    public BeyondSummitGemManager(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Depth = -10010;
        unlockFlag = data.Attr("flag", "beyondsummit_gate_open");

        Vector2[] nodes = data.NodesWithPosition(offset);
        for (int i = 0; i < TotalGems; i++)
        {
            if (i + 1 < nodes.Length)
                gemNodes.Add(nodes[i + 1]);
            else
            {
                float angle = (i / (float)TotalGems) * MathF.PI * 2f - MathF.PI / 2f;
                gemNodes.Add(Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 24f);
            }
        }

        Collider = new Hitbox(64f, 64f, -32f, -32f);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        Level level = SceneAs<Level>();

        for (int i = 0; i < TotalGems; i++)
        {
            gemCollected[i] = level.Session.GetFlag("beyondsummit_gem_" + i);
            gemAlpha[i] = gemCollected[i] ? 1f : 0.15f;
            gemScale[i] = gemCollected[i] ? 1f : 0.5f;

            string spritePath = "collectables/summitgems/" + i + "/gem";
            Sprite sp = new Sprite(GFX.Game, spritePath);
            sp.AddLoop("idle", "", 0.08f);
            sp.Play("idle");
            sp.CenterOrigin();
            sp.Color = gemCollected[i] ? Color.White : Color.White * 0.25f;
            gemSprites.Add(sp);
        }

        Add(centerLight = new VertexLight(Color.White, 0.5f, 24, 48));
        Add(centerBloom = new BloomPoint(0.3f, 16f));
        Add(bounceWiggler = Wiggler.Create(0.4f, 4f));
        bounceWiggler.StartZero = true;

        opened = level.Session.GetFlag(unlockFlag);
        allCollected = BeyondSummitGem.CheckAllCollected(level.Session);

        if (allCollected && !opened)
        {
            Add(new Coroutine(UnlockRoutine()));
        }
    }

    public override void Update()
    {
        base.Update();

        Level level = SceneAs<Level>();
        if (level == null) return;

        for (int i = 0; i < TotalGems; i++)
        {
            gemCollected[i] = level.Session.GetFlag("beyondsummit_gem_" + i);

            float targetAlpha = gemCollected[i] ? 1f : 0.15f;
            float targetScale = gemCollected[i] ? 1f : 0.5f;
            gemAlpha[i] = Calc.Approach(gemAlpha[i], targetAlpha, Engine.DeltaTime * 2f);
            gemScale[i] = Calc.Approach(gemScale[i], targetScale, Engine.DeltaTime * 2f);

            if (gemCollected[i])
                gemSprites[i].Color = Color.White;
        }

        if (!allCollected && BeyondSummitGem.CheckAllCollected(level.Session))
        {
            allCollected = true;
            if (!opened)
            {
                Add(new Coroutine(UnlockRoutine()));
            }
        }

        for (int i = 0; i < gemSprites.Count; i++)
        {
            gemSprites[i].Update();
        }
    }

    private IEnumerator UnlockRoutine()
    {
        Level level = SceneAs<Level>();
        level.CanRetry = false;

        yield return 0.5f;

        // Play gem unlock sounds sequentially
        for (int i = 0; i < TotalGems; i++)
        {
            Audio.Play("event:/pusheen/game/09_beyondsummit/gem_unlock_" + (i + 1), Position);
            bounceWiggler.Start();

            // Brief flash for each gem
            float pulse = 0f;
            while (pulse < 1f)
            {
                pulse += Engine.DeltaTime / 0.25f;
                gemScale[i] = 1f + MathF.Sin(pulse * MathF.PI) * 0.3f;
                yield return null;
            }
            gemScale[i] = 1f;

            yield return 0.3f;
        }

        yield return 0.5f;

        // Final unlock burst
        Audio.Play("event:/pusheen/game/09_beyondsummit/gem_unlock_complete", Position);
        level.Flash(Color.White * 0.5f, drawPlayerOver: true);
        level.Shake(0.3f);
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);

        centerBloom.Alpha = 1f;
        centerLight.Alpha = 1f;

        yield return 0.8f;

        opened = true;
        level.Session.SetFlag(unlockFlag);

        yield return 0.3f;

        level.CanRetry = true;
    }

    public override void Render()
    {
        base.Render();

        // Draw the gate/barrier background
        if (!opened)
        {
            Draw.Rect(X - 16f, Y - 16f, 32f, 32f, Color.Black * 0.4f);
        }

        // Draw gems around the gate
        for (int i = 0; i < TotalGems; i++)
        {
            if (i >= gemSprites.Count || i >= gemNodes.Count) break;

            Vector2 pos = gemNodes[i];
            float scale = gemScale[i] * (1f + bounceWiggler.Value * 0.1f);
            float alpha = gemAlpha[i];

            Sprite sp = gemSprites[i];
            Color prevColor = sp.Color;
            sp.Color = prevColor * alpha;
            sp.Scale = Vector2.One * scale;
            sp.RenderPosition = pos;
            sp.Render();
            sp.Color = prevColor;
        }
    }
}

