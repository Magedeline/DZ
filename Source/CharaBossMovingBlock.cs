namespace MaggyHelper.Entities;
[CustomEntity(ids: "MaggyHelper/CharaBossMovingBlock")]
[Tracked]
[HotReloadable]
public partial class CharaBossMovingBlock : Entity
{
    public static ParticleType PStop;
    public static ParticleType PBreak;
    public int BossNodeIndex;
    private float startDelay;
    private int nodeIndex;
    private Vector2[] nodes;
    private TileGrid sprite;
    private TileGrid highlight;
    private Coroutine moveCoroutine;
    private bool isHighlighted;

    public CharaBossMovingBlock(Vector2[] nodes, float width, float height, int bossNodeIndex)
        : base(nodes[0])
    {
        BossNodeIndex = bossNodeIndex;
        this.nodes = nodes;
        int newSeed = Calc.Random.Next();
        Calc.PushRandom(newSeed);
        sprite = GFX.FGAutotiler.GenerateBox('g', (int)width / 8, (int)height / 8).TileGrid;
        Add(sprite);
        Calc.PopRandom();
        Calc.PushRandom(newSeed);
        highlight = GFX.FGAutotiler.GenerateBox('G', (int)(width / 8.0), (int)height / 8).TileGrid;
        highlight.Alpha = 0.0f;
        Add(highlight);
        Calc.PopRandom();
        Add(new TileInterceptor(sprite, false));
        Add(new LightOcclude());
    }

    public CharaBossMovingBlock(EntityData data, Vector2 offset)
        : this(data.NodesWithPosition(offset), data.Width, data.Height, data.Int(nameof(nodeIndex)))
    {
    }

    public void StartMoving(float delay)
    {
        startDelay = delay;
        Add(moveCoroutine = new Coroutine(moveSequence()));
    }

    private IEnumerator moveSequence()
    {
        CharaBossMovingBlock charaBossMovingBlock1 = this;
        while (true)
        {
            CharaBossMovingBlock charaBossMovingBlock = charaBossMovingBlock1;
            if (!charaBossMovingBlock1.isHighlighted)
            {
                for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime / (0.2f + charaBossMovingBlock1.startDelay + 0.2f))
                {
                    charaBossMovingBlock1.highlight.Alpha = Ease.CubeIn(p);
                    charaBossMovingBlock1.sprite.Alpha = 1f - charaBossMovingBlock1.highlight.Alpha;
                    yield return null;
                }
                charaBossMovingBlock1.highlight.Alpha = 1f;
                charaBossMovingBlock1.sprite.Alpha = 0.0f;
                charaBossMovingBlock1.isHighlighted = true;
            }
            else
                yield return 0.2f + charaBossMovingBlock1.startDelay + 0.2f;
            charaBossMovingBlock1.startDelay = 0.0f;
            ++charaBossMovingBlock1.nodeIndex;
            charaBossMovingBlock1.nodeIndex %= charaBossMovingBlock1.nodes.Length;
            Vector2 from = charaBossMovingBlock1.Position;
            Vector2 to = charaBossMovingBlock1.nodes[charaBossMovingBlock1.nodeIndex];
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.8f, true);
            tween.OnUpdate = t => charaBossMovingBlock.Position = Vector2.Lerp(from, to, t.Eased);
            tween.OnComplete = t =>
            {
                if (charaBossMovingBlock.Scene.CollideCheck<SolidTiles>(charaBossMovingBlock.Position + (to - from).SafeNormalize() * 2f))
                {
                    Audio.Play("event:/game/06_reflection/fallblock_boss_impact", charaBossMovingBlock.Center);
                    charaBossMovingBlock.ImpactParticles(to - from);
                }
                else
                    charaBossMovingBlock.stopParticles(to - from);
            };
            charaBossMovingBlock.Add(tween);
            yield return 0.8f;
        }
    }

    private void stopParticles(Vector2 moved)
    {
        Level level = SceneAs<Level>();
        float direction = moved.Angle();
        if (moved.X > 0.0f)
        {
            Vector2 vector2 = new Vector2(Right - 1f, Top);
            for (int index = 0; index < Height; index += 4)
                level.Particles.Emit(PStop, vector2 + Vector2.UnitY * (2 + index + Calc.Random.Range(-1, 1)), direction);
        }
        else if (moved.X < 0.0f)
        {
            Vector2 vector2 = new Vector2(Left, Top);
            for (int index = 0; index < Height; index += 4)
                level.Particles.Emit(PStop, vector2 + Vector2.UnitY * (2 + index + Calc.Random.Range(-1, 1)), direction);
        }
        if (moved.Y > 0.0f)
        {
            Vector2 vector2 = new Vector2(Left, Bottom - 1f);
            for (int index = 0; index < Width; index += 4)
                level.Particles.Emit(PStop, vector2 + Vector2.UnitX * (2 + index + Calc.Random.Range(-1, 1)), direction);
        }
        else if (moved.Y < 0.0f)
        {
            Vector2 vector2 = new Vector2(Left, Top);
            for (int index = 0; index < Width; index += 4)
                level.Particles.Emit(PStop, vector2 + Vector2.UnitX * (2 + index + Calc.Random.Range(-1, 1)), direction);
        }
    }

    protected virtual void ImpactParticles(Vector2 moved)
    {
        if (moved.X < 0.0f)
        {
            Vector2 vector2 = new Vector2(0.0f, 2f);
            for (int index = 0; index < Height / 8.0; ++index)
            {
                Vector2 point = new Vector2(Left - 1f, Top + 4f + index * 8);
                if (!Scene.CollideCheck<Water>(point) && Scene.CollideCheck<Solid>(point))
                {
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, 0.0f);
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, 0.0f);
                }
            }
        }
        else if (moved.X > 0.0f)
        {
            Vector2 vector2 = new Vector2(0.0f, 2f);
            for (int index = 0; index < Height / 8.0; ++index)
            {
                Vector2 point = new Vector2(Right + 1f, Top + 4f + index * 8);
                if (!Scene.CollideCheck<Water>(point) && Scene.CollideCheck<Solid>(point))
                {
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, 3.14159274f);
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, 3.14159274f);
                }
            }
        }
        if (moved.Y < 0.0f)
        {
            Vector2 vector2 = new Vector2(2f, 0.0f);
            for (int index = 0; index < Width / 8.0; ++index)
            {
                Vector2 point = new Vector2(Left + 4f + index * 8, Top - 1f);
                if (!Scene.CollideCheck<Water>(point) && Scene.CollideCheck<Solid>(point))
                {
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, 1.57079637f);
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, 1.57079637f);
                }
            }
        }
        else if (moved.Y > 0.0f)
        {
            Vector2 vector2 = new Vector2(2f, 0.0f);
            for (int index = 0; index < Width / 8.0; ++index)
            {
                Vector2 point = new Vector2(Left + 4f + index * 8, Bottom + 1f);
                if (!Scene.CollideCheck<Water>(point) && Scene.CollideCheck<Solid>(point))
                {
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, -1.57079637f);
                    SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, -1.57079637f);
                }
            }
        }
    }

    public override void Render()
    {
        Vector2 position = Position;
        base.Render();
        if (highlight.Alpha > 0.0f && highlight.Alpha < 1.0f)
        {
            int num = (int)((1.0f - highlight.Alpha) * 16.0f);
            Rectangle rect = new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
            rect.Inflate(num, num);
            Draw.HollowRect(rect, Color.Lerp(Color.Purple, Color.Pink, 0.7f));
        }
    }
}



