namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/CharaBossFallingBlocks")]
    [Tracked]
    [HotReloadable]
    public class CharaBossFallingBlock : Entity
    {
        public static ParticleType PFallDustA;
        public static ParticleType PFallDustB;
        public static ParticleType PLandDust;
        public bool Triggered;
        public float FallDelay;
        private char tileType;
        private int surfaceSoundIndex;
        private TileGrid tiles;
        private TileGrid highlight;
        private bool climbFall;
        private bool charaBoss;
        public float Fade = 16f;
        public float Spikey;
        public float SmallWaveAmplitude = 1f;
        public float BigWaveAmplitude = 4f;
        public float CurveAmplitude = 12f;
        public float UpdateMultiplier = 1f;
        public Color SurfaceColor = Color.White;
        public Color EdgeColor = Color.LightGray;
        public Color CenterColor = Color.DarkGray;
        private float timer = Calc.Random.NextFloat(100f);
        private VertexPositionColor[] verts;
        private Bubble[] bubbles;
        private SurfaceBubble[] surfaceBubbles;
        private int surfaceBubbleIndex;
        private List<List<MTexture>> surfaceBubbleAnimations;
        private float height;
        private float width;

        public new float Width
        {
            get => width;
            private set => width = value;
        }

        public new float Height
        {
            get => height;
            private set => height = value;
        }

        // Helper properties for positioning
        public new float Left => Position.X;
        public new float Right => Position.X + Width;
        public new float Top => Position.Y;
        public new float Bottom => Position.Y + Height;

        private struct Bubble
        {
            public Vector2 Position;
            public float Speed;
            public float Alpha;
        }
        private struct SurfaceBubble
        {
            public float X;
            public float Frame;
            public byte Animation;
        }
        public CharaBossFallingBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            initialize(data, offset);
        }

        private void initialize(EntityData data, Vector2 offset)
        {
            this.charaBoss = data.Bool(nameof(charaBoss), false);
            this.climbFall = data.Bool(nameof(climbFall), false);
            Width = data.Width;
            Height = data.Height;
            char tile = data.Char("tiletype", '3');
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            Add(tiles = GFX.FGAutotiler.GenerateBox(tile, data.Width / 8, data.Height / 8).TileGrid);

            if (charaBoss)
            {
                Add(highlight = GFX.FGAutotiler.GenerateBox('G', data.Width / 8, data.Height / 8).TileGrid);
                highlight.Alpha = 0.0f;
            }

            Add(new Coroutine(sequence()));
            Add(new LightOcclude());
            Add(new TileInterceptor(tiles, false));
            tileType = tile;
            surfaceSoundIndex = SurfaceIndex.TileToIndex[tile];
            if (data.Bool("behind", false))
                Depth = 5000;
            // Initialize LavaRect properties
            resize(data.Width, data.Height, 8);
            Calc.PopRandom();
        }

        private IEnumerator sequence()
        {
            while (!Triggered) yield return null;
            yield return FallDelay;
            Audio.Play("event:/game/06_reflection/fallblock_shake", Position);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            var timer = 0.4f;
            while (timer > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
                if (highlight != null)
                    highlight.Alpha = Calc.Approach(highlight.Alpha, 1f, Engine.DeltaTime * 4f);
            }
            if (highlight != null)
                highlight.Alpha = 0f;
            if (charaBoss)
                Audio.Play("event:/game/06_reflection/fallblock_boss", Position);
            else
                Audio.Play("event:/game/06_reflection/fallblock", Position);
            var speed = 0f;
            var maxSpeed = 160f;
            while (true)
            {
                speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                Position.Y += speed * Engine.DeltaTime;
                if (Top > (Scene as Level).Bounds.Bottom + 16)
                {
                    RemoveSelf();
                    yield break;
                }
                yield return null;
            }
        }

        public override void Update()
        {
            base.Update();
            // LavaRect Update logic
            timer += UpdateMultiplier * Engine.DeltaTime;
            if (UpdateMultiplier != 0.0f)
            if (bubbles != null)
            {
                for (int i = 0; i < bubbles.Length; i++)
                {
                    bubbles[i].Position.Y -= UpdateMultiplier * bubbles[i].Speed * Engine.DeltaTime;
                    if (bubbles[i].Position.Y < 2.0f - wave((int)(bubbles[i].Position.X / 8), Width))
                    {
                        bubbles[i].Position.Y = Height - 1f;
                        if (Calc.Random.Chance(0.75f) && surfaceBubbles != null)
                        {
                            surfaceBubbles[surfaceBubbleIndex].X = bubbles[i].Position.X;
                            surfaceBubbles[surfaceBubbleIndex].Frame = 0.0f;
                            surfaceBubbles[surfaceBubbleIndex].Animation = (byte)Calc.Random.Next(surfaceBubbleAnimations?.Count ?? 1);
                            surfaceBubbleIndex = (surfaceBubbleIndex + 1) % surfaceBubbles.Length;
                        }
                    }
                }
            }
        }

        private void resize(float width, float height, int step)
        {
            Width = width;
            Height = height;
            verts = new VertexPositionColor[(int)((width / step * 2.0 + height / step * 2.0 + 4.0) * 3 * 6 + 6)];
            bubbles = new Bubble[(int)(width * height * 0.005)];
            surfaceBubbles = new SurfaceBubble[(int)Math.Max(4.0, bubbles.Length * 0.25)];
            for (int i = 0; i < bubbles.Length; i++)
            {
                bubbles[i].Position = new Vector2(1f + Calc.Random.NextFloat(width - 2f), Calc.Random.NextFloat(height));
                bubbles[i].Speed = Calc.Random.Range(4, 12);
                bubbles[i].Alpha = Calc.Random.Range(0.4f, 0.8f);
            }
            for (int i = 0; i < surfaceBubbles.Length; i++)
                surfaceBubbles[i].X = -1f;
            surfaceBubbleAnimations = new List<List<MTexture>>();
            surfaceBubbleAnimations.Add(GFX.Game.GetAtlasSubtextures("danger/lava/bubble_a"));
        }

        private float wave(int step, float length)
        {
            int val = step * 8;
            float num1 = Calc.ClampedMap(val, 0.0f, length * 0.1f) * Calc.ClampedMap(val, length * 0.9f, length, 1f, 0.0f);
            float num2 = (float)(Math.Sin(val * 0.25 + timer * 4.0) * SmallWaveAmplitude +
                                 Math.Sin(val * 0.05 + timer * 0.5) * BigWaveAmplitude);
            return num2 * num1;
        }
    }
}



