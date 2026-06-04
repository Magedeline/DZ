namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/WhiteBlock")]
    public class WhiteBlock : Solid
    {
        private MTexture texture;
        private bool activated;
        private float fadeTimer;
        private float alpha = 1f;

        public WhiteBlock(Vector2 position, int width, int height)
            : base(position, width, height, true)
        {
            texture = AtlasPathHelper.GetTexture("objects/whiteblock");
            base.Depth = -9000;
        }

        public WhiteBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        public override void Update()
        {
            base.Update();
            
            // Check for player standing on block
            global::Celeste.Player player = GetPlayerOnTop();
            if (player != null && !activated)
            {
                activated = true;
                fadeTimer = 0.5f;
                Audio.Play("guid://{a9cc805b-cc3c-4e29-af3b-bc69914e5a10}", Position);
            }
            
            if (activated)
            {
                fadeTimer -= Engine.DeltaTime;
                alpha = Math.Max(0f, fadeTimer / 0.5f);
                
                if (fadeTimer <= 0f)
                {
                    Collidable = false;
                    Visible = false;
                    
                    // Respawn after delay
                    Add(new Coroutine(RespawnRoutine()));
                }
            }
        }

        private IEnumerator RespawnRoutine()
        {
            yield return 3f;
            
            // Check if player is in the way
            while (CollideCheck<Player>())
            {
                yield return 0.1f;
            }
            
            activated = false;
            Collidable = true;
            Visible = true;
            alpha = 1f;
            
            Audio.Play("guid://{735f3afc-00bc-46da-94f1-2cb51e3bfecc}", Position);
        }

        public override void Render()
        {
            Color color = Color.White * alpha;
            
            for (int x = 0; x < Width; x += texture.Width)
            {
                for (int y = 0; y < Height; y += texture.Height)
                {
                    texture.Draw(Position + new Vector2(x, y), Vector2.Zero, color);
                }
            }
        }
    }
}

