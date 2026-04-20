namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/LuckyBlock")]
    public class LuckyBlock : Solid
    {
        public static ParticleType P_Coin;

        private MTexture texture;
        private bool hit;
        private Wiggler wiggler;
        private Vector2 originalPosition;
        private int hitsRemaining;
        private string rewardType;

        public LuckyBlock(Vector2 position, int width, int height, int maxHits, string rewardType)
            : base(position, width, height, true)
        {
            this.hitsRemaining = maxHits;
            this.rewardType = rewardType;
            originalPosition = position;
            
            texture = AtlasPathHelper.GetTexture("objects/luckyblock");
            OnDashCollide = (global::Celeste.Player player, Vector2 direction) => OnDashed(player, direction);
            
            Add(wiggler = Wiggler.Create(0.3f, 4f));
            base.Depth = -9000;
        }

        public LuckyBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, 
                   data.Int("maxHits", 3), data.Attr("rewardType", "coin"))
        {
        }

        private DashCollisionResults OnDashed(global::Celeste.Player player, Vector2 direction)
        {
            // Only trigger when hit from below
            if (!hit && hitsRemaining > 0 && player != null)
            {
                Hit(player);
                return DashCollisionResults.Rebound;
            }
            return DashCollisionResults.NormalCollision;
        }

        public override void Update()
        {
            base.Update();
            
            // Check if player jumps and hits from below
            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && !hit && hitsRemaining > 0)
            {
                if (player.Speed.Y < 0 && player.Top <= Bottom && player.Top >= Bottom - 8)
                {
                    if (player.Left < Right && player.Right > Left)
                    {
                        Hit(player);
                    }
                }
            }
            
            // Apply wiggle effect
            if (wiggler.Active)
            {
                Position = originalPosition + new Vector2(0, -wiggler.Value * 4f);
            }
        }

        private void Hit(global::Celeste.Player player)
        {
            if (hitsRemaining <= 0) return;
            
            hit = true;
            hitsRemaining--;
            wiggler.Start();
            
            Audio.Play("event:/game/general/coin", Position);
            
            Level level = SceneAs<Level>();
            
            // Spawn reward based on type
            SpawnReward(level);
            
            // Emit particles
            level.ParticlesFG.Emit(P_Coin, 8, TopCenter, Vector2.One * 4f, -(float)Math.PI / 2f);
            
            Add(new Coroutine(HitCooldown()));
        }

        private void SpawnReward(Level level)
        {
            Vector2 spawnPos = TopCenter - new Vector2(0, 8);
            
            // Create floating coin/item effect
            for (int i = 0; i < 4; i++)
            {
                level.ParticlesFG.Emit(P_Coin, spawnPos + new Vector2(Calc.Random.Range(-4f, 4f), Calc.Random.Range(-8f, 0f)), 
                    -(float)Math.PI / 2f + Calc.Random.Range(-0.3f, 0.3f));
            }
        }

        private IEnumerator HitCooldown()
        {
            yield return 0.3f;
            hit = false;
            Position = originalPosition;
        }

        public override void Render()
        {
            Color color = hitsRemaining > 0 ? Color.White : Color.Gray;
            
            for (int x = 0; x < Width; x += texture.Width)
            {
                for (int y = 0; y < Height; y += texture.Height)
                {
                    texture.Draw(Position + new Vector2(x, y), Vector2.Zero, color);
                }
            }
        }

        public static void LoadParticles()
        {
            P_Coin = new ParticleType
            {
                Color = Color.Gold,
                Color2 = Color.Yellow,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                Size = 1f,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                SpeedMultiplier = 0.3f,
                DirectionRange = (float)Math.PI / 4f
            };
        }
    }
}
