namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/StarBlock")]
    [Tracked(true)]
    public abstract class StarBlock : Entity
    {
        private bool isBroken;
        private int width;
        private int height;
        private const int grid_size = 8;
        private MTexture texture;

        public StarBlock(Vector2 position, int width, int height) : base(position)
        {
            this.width = width;
            this.height = height;
            Collider = new Hitbox(width, height, -width / 2f, -height / 2f);
            Add(new PlayerCollider(OnPlayer));
            Depth = -10000;
            texture = ResolveTexture(width, height);
        }

        public Vector2 Speed { get; set; }

        private void OnPlayer(global::Celeste.Player player)
        {
            if (isBroken || !Collider.Collide(player.Collider))
            {
                return;
            }

            // Kirby can inhale-break this block; everyone can dash-break it.
            if (player.IsKirbyMode() && IsKirbyInhaling(player))
            {
                Audio.Play("event:/desolozantas/char/kirby/inhale_start", Position);
                Break();
                return;
            }

            if (player.DashAttacking)
            {
                Audio.Play("event:/game/general/diamond_touch", Position);
                Break();
            }
        }

        public override void Update()
        {
            base.Update();
            var player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && !isBroken && Collider.Collide(player.Collider))
            {
                if (player.IsKirbyMode() && IsKirbyInhaling(player))
                {
                    Audio.Play("event:/desolozantas/char/kirby/inhale_start", Position);
                    Break();
                    return;
                }

                if (!player.IsKirbyMode() && player.DashAttacking)
                {
                    Break();
                }
            }
        }

        public void Break()
        {
            if (isBroken) return;
            isBroken = true;
            Audio.Play("event:/game/general/diamond_break", Position);
            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new Particle(Position, Calc.Random.Choose(Color.Yellow, Color.Orange, Color.Red)));
            }
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();

            if (texture != null)
            {
                texture.Draw(
                    Position + new Vector2(-width / 2f, -height / 2f),
                    Vector2.Zero,
                    Color.White,
                    new Vector2(width / (float)texture.Width, height / (float)texture.Height));
            }
            else
            {
                Draw.Rect(Collider.Bounds.X, Collider.Bounds.Y, Collider.Bounds.Width, Collider.Bounds.Height, Color.Yellow);
            }
        }

        public void Resize(int newWidth, int newHeight)
        {
            width = snapToGrid(newWidth);
            height = snapToGrid(newHeight);
            Collider.Width = width;
            Collider.Height = height;
            Collider.Position = new Vector2(-width / 2f, -height / 2f);
            texture = ResolveTexture(width, height);
        }

        private int snapToGrid(int value)
        {
            return (value / grid_size) * grid_size;
        }

        private bool IsKirbyInhaling(global::Celeste.Player player)
        {
            if (Scene is not Level level || !player.IsKirbyMode())
            {
                return false;
            }

            var legacy = level.Tracker.GetEntity<KirbyMode>();
            if (legacy != null && legacy.IsInhaling)
            {
                return true;
            }

            return false;
        }

        private static MTexture ResolveTexture(int width, int height)
        {
            int area = width * height;
            string path = area >= 256
                ? "objects/starblock/oversized"
                : area >= 128
                    ? "objects/starblock/large"
                    : "objects/starblock/normal";

            return GFX.Game.Has(path) ? GFX.Game[path] : null;
        }

        private class Particle : Entity
        {
            private Vector2 velocity;
            private Color color;
            private float timer;

            public Particle(Vector2 position, Color color) : base(position)
            {
                this.color = color;
                velocity = Calc.Random.Range(new Vector2(-50f, -50f), new Vector2(50f, 50f));
                timer = 0.5f;
            }

            public override void Update()
            {
                base.Update();
                Position += velocity * Engine.DeltaTime;
                velocity *= 0.9f;
                timer -= Engine.DeltaTime;
                if (timer <= 0) RemoveSelf();
            }

            public override void Render()
            {
                Draw.Rect(Position.X - 2, Position.Y - 2, 4, 4, color);
            }
        }
    }
}




