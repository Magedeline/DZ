namespace MaggyHelper.Entities
{
    [Tracked]
    public class MagicFountain : Entity
    {
        private Sprite sprite;
        private bool isActive = false;
        private float particleTimer = 0f;
        private Level level;
        
        public bool IsActive => isActive;
        
        public MagicFountain(Vector2 position) : base(position)
        {
            Depth = -1000; // Render above most things
            
            // Set up sprite
            if (GFX.SpriteBank.Has("fountain"))
            {
                sprite = GFX.SpriteBank.Create("fountain");
            }
            else
            {
                // Fallback - create a simple animated sprite
                sprite = new Sprite(GFX.Game, "objects/temple/");
                sprite.AddLoop("idle", "mirror", 0.1f);
                sprite.AddLoop("active", "mirror", 0.05f);
            }
            
            Add(sprite);
            sprite.Play("idle");
            
            // Set up collider
            Collider = new Hitbox(32f, 64f, -16f, -64f);
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }
        
        public void Activate()
        {
            isActive = true;
            sprite?.Play("active");
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }
        
        public void Emerge()
        {
            // Start the emergence animation/effect
            if (level != null)
            {
                level.Shake(0.2f);
                level.ParticlesFG.Emit(ParticleTypes.Dust, 10, Position, Vector2.One * 16f);
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            if (isActive)
            {
                particleTimer += Engine.DeltaTime;
                
                // Emit magical particles
                if (particleTimer >= 0.3f)
                {
                    particleTimer = 0f;
                    
                    if (level != null)
                    {
                        // Emit fountain particles
                        Vector2 particlePos = Position + new Vector2(0f, -32f);
                        level.ParticlesFG.Emit(ParticleTypes.Dust, 3, particlePos, Vector2.One * 8f, Color.LightBlue);
                        
                        // Emit sparkles
                        for (int i = 0; i < 2; i++)
                        {
                            Vector2 sparklePos = particlePos + new Vector2(
                                Calc.Random.Range(-16f, 16f),
                                Calc.Random.Range(-8f, 8f)
                            );
                            // With this line, using a valid particle type (e.g., SparkyDust, which is visually similar to sparkles):
                            level.ParticlesFG.Emit(ParticleTypes.SparkyDust, 1, sparklePos, Vector2.Zero, Color.Gold);
                        }
                    }
                }
            }
        }
        
        public override void Render()
        {
            base.Render();
            
            // Draw magical glow effect when active
            if (isActive)
            {
                float glowIntensity = (float)(Math.Sin(Scene.TimeActive * 3) * 0.2 + 0.8);
                Color glowColor = Color.LightBlue * glowIntensity * 0.5f;
                
                Draw.Circle(Position, 48f, glowColor, 16);
                Draw.Circle(Position, 24f, Color.White * glowIntensity * 0.3f, 8);
            }
            
            // Debug rendering
            if (Engine.Commands.Open)
            {
                Draw.HollowRect(Collider, Color.Magenta);
            }
        }
    }
}



