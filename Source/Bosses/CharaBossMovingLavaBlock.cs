namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/MovingLavaBlock")]
    [Tracked]
    [HotReloadable]
    public class MovingLavaBlock : Solid
    {
        public static ParticleType PStop;
        public static ParticleType PBreak;
        public int BossNodeIndex { get; private set; }
        private float startDelay;
        private int nodeIndex;
        private Vector2[] nodes;
        private readonly TileGrid sprite;
        private TileGrid highlight;
        private Coroutine moveCoroutine;
        private const float movement_threshold = 0.1f;
        private const float movement_lerp_factor = 0.1f;
        private const float node_delay = 1.0f;

        public MovingLavaBlock(EntityData data, Vector2 offset)
            : this(
                data.NodesWithPosition(offset), 
                data.Width, 
                data.Height, 
                data.Int(nameof(BossNodeIndex)),
                GFX.FGAutotiler.GenerateBox(data.Char("tiletype", 'g'), data.Width / 8, data.Height / 8).TileGrid,
                GFX.FGAutotiler.GenerateBox('G', data.Width / 8, data.Height / 8).TileGrid)
        {
        }

        public MovingLavaBlock(Vector2[] nodes, float width, float height, int bossNodeIndex, TileGrid sprite, TileGrid highlight)
            : base(nodes?[0] ?? throw new ArgumentException("Nodes array cannot be null or empty.", nameof(nodes)), width, height, false)
        {
            if (nodes.Length == 0)
                throw new ArgumentException("Nodes array cannot be empty.", nameof(nodes));
            BossNodeIndex = bossNodeIndex;
            this.sprite = sprite;
            this.highlight = highlight;
            this.nodes = nodes;
            // Initialize sprite
            Add(sprite);
            // Initialize highlight
            highlight.Alpha = 0.0f;
            Add(highlight);
            // Add components
            Add(new TileInterceptor(sprite, false));
            Add(new LightOcclude());
        }

        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            sprite.Position = amount;
        }
        public void StartMoving(float delay)
        {
            startDelay = delay;
            if (moveCoroutine != null)
                Remove(moveCoroutine);
            Add(moveCoroutine = new Coroutine(moveSequence()));
        }
        private IEnumerator moveSequence()
        {
            yield return new WaitForSeconds(startDelay);
            while (true)
            {
                Vector2 target = nodes[nodeIndex];
                while (Vector2.Distance(Position, target) > movement_threshold)
                {
                    Position = Vector2.Lerp(Position, target, movement_lerp_factor);
                    yield return null;
                }
                nodeIndex = (nodeIndex + 1) % nodes.Length;
                yield return new WaitForSeconds(node_delay);
            }
        }
        private void emitParticles(Level level, Vector2 start, float length, Vector2 offset, float direction)
        {
            for (int i = 0; i < length; i += 4)
            {
                level.Particles.Emit(PStop, start + offset * (2 + i + Calc.Random.Range(-1, 1)), direction);
            }
        }
        private void breakParticles()
        {
            Vector2 center = Center;
            for (int x = 0; x < Width; x += 4)
            {
                for (int y = 0; y < Height; y += 4)
                {
                    Vector2 position = Position + new Vector2(2 + x, 2 + y);
                    SceneAs<Level>().Particles.Emit(PBreak, 1, position, Vector2.One * 2f, (position - center).Angle());
                }
            }
        }
        public override void Render()
        {
            Vector2 originalPosition = Position;
            Position += Shake;
            base.Render();
            if (highlight.Alpha > 0.0f && highlight.Alpha < 1.0f)
            {
                int inflation = (int)((1.0f - highlight.Alpha) * 16.0f);
                Rectangle rect = new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
                rect.Inflate(inflation, inflation);
                Draw.HollowRect(rect, Color.Lerp(Color.Purple, Color.Pink, 0.7f));
            }
            Position = originalPosition;
        }
        public void Destroy(float delay)
        {
            if (Scene == null)
                return;
            if (moveCoroutine != null)
                Remove(moveCoroutine);
            if (delay <= 0.0f)
            {
                finish();
            }
            else
            {
                StartShaking(delay);
                Alarm.Set(this, delay, finish);
            }
        }
        private void finish()
        {
            RemoveSelf();
            SceneAs<Level>().Particles.Emit(PBreak, (int)(Width * Height / 64), Center, new Vector2(Width / 2, Height / 2));
        }
    }

    internal class WaitForSeconds(float duration)
    {
        private float elapsed = 0f;

        public bool MoveNext()
        {
            elapsed += Engine.DeltaTime;
            return elapsed < duration;
        }

        public void Reset()
        {
            elapsed = 0f;
        }

        public object Current => null;
    }
}




