using DZ;

namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/StarBlock")]
    [Tracked(true)]
    public class StarBlock : Entity
    {
        private bool isBroken;
        private int width;
        private int height;
        private const int grid_size = 8;
        private MTexture texture;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData data)
        {
            Vector2 position = data.Position + offset;
            int width = data.Width;
            int height = data.Height;
            return new StarBlock(position, width, height);
        }

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

            // K_Player passes its shadow player to PlayerCollider. Resolve to the real K_Player.
            if (K_PlayerHooks.ShadowPlayers.Contains(player))
            {
                K_Player kPlayer = Scene?.Tracker.GetEntity<K_Player>();
                if (kPlayer != null)
                {
                    HandleKPlayerCollision(kPlayer);
                    return;
                }
            }

            // Vanilla player path: Kirby can inhale-break this block; everyone can dash-break it.
            if (player.IsKirbyMode() && IsKirbyInhaling(player))
            {
                Audio.Play("event:/DZ/char/kirby/inhale_start", Position);
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
            if (isBroken)
            {
                return;
            }

            var level = Scene as Level;
            if (level == null)
            {
                return;
            }

            // K_Player takes precedence so we use its actual dash/inhale state instead of the shadow player.
            K_Player kPlayer = level.Tracker.GetEntity<K_Player>();
            if (kPlayer != null && Collider.Collide(kPlayer.Collider))
            {
                HandleKPlayerCollision(kPlayer);
                return;
            }

            // Fallback: real vanilla player.
            global::Celeste.Player player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && !K_PlayerHooks.ShadowPlayers.Contains(player) && Collider.Collide(player.Collider))
            {
                if (player.IsKirbyMode() && IsKirbyInhaling(player))
                {
                    Audio.Play("event:/DZ/char/kirby/inhale_start", Position);
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

        private void HandleKPlayerCollision(K_Player kPlayer)
        {
            if (DZModule.Session?.IsKirbyModeActive == true && kPlayer.kirbyController?.IsInhaling == true)
            {
                Audio.Play("event:/DZ/char/kirby/inhale_start", Position);
                Break();
                return;
            }

            if (kPlayer.DashAttacking)
            {
                Audio.Play("event:/game/general/diamond_touch", Position);
                Break();
            }
        }

        private static MTexture ResolveTexture(int width, int height)
        {
            int area = width * height;
            string path = area >= 256
                ? "objects/DZ/starblock/oversized"
                : area >= 128
                    ? "objects/DZ/starblock/large"
                    : "objects/DZ/starblock/normal";

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





