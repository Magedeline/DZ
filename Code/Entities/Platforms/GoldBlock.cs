namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/GoldBlock")]
    [HotReloadable]
    public class GoldBlock : Solid
    {
        private MTexture texture;
        private bool broken;
        private new float shakeTimer;
        private Vector2 shakeOffset;

        public GoldBlock(Vector2 position, int width, int height)
            : base(position, width, height, true)
        {
            texture = AtlasPathHelper.GetTexture("objects/goldblock");
            OnDashCollide = (global::Celeste.Player player, Vector2 direction) => OnDashed(player, direction);
            base.Depth = -9000;
        }

        public GoldBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        private DashCollisionResults OnDashed(global::Celeste.Player player, Vector2 direction)
        {
            if (!broken)
            {
                Break(player.Center, direction);
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }

        private void Break(Vector2 from, Vector2 direction)
        {
            broken = true;
            Collidable = false;
            
            Audio.Play("event:/game/general/wall_break_stone", Position);
            
            Level level = SceneAs<Level>();
            level.Shake(0.2f);
            
            // Create debris particles
            for (int x = 0; x < Width; x += 8)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    Vector2 debrisPos = Position + new Vector2(x + 4, y + 4);
                    level.Particles.Emit(Player.P_CassetteFly, 2, debrisPos, Vector2.One * 2f);
                    
                    Debris debris = Engine.Pooler.Create<Debris>()
                        .Init(debrisPos, '4', playSound: false)
                        .BlastFrom(from);
                    level.Add(debris);
                }
            }
            
            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            if (shakeTimer > 0f)
            {
                shakeTimer -= Engine.DeltaTime;
                shakeOffset = Calc.Random.ShakeVector();
            }
            else
            {
                shakeOffset = Vector2.Zero;
            }
        }

        public override void Render()
        {
            if (!broken)
            {
                // Render gold block texture tiled
                for (int x = 0; x < Width; x += texture.Width)
                {
                    for (int y = 0; y < Height; y += texture.Height)
                    {
                        texture.Draw(Position + shakeOffset + new Vector2(x, y));
                    }
                }
            }
        }
    }
}

