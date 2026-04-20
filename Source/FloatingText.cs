namespace Celeste.Entities
{
    /// <summary>
    /// Simple floating text that appears and fades out
    /// Used for refuse-to-die messages during boss fights
    /// </summary>
    [HotReloadable]
    public class FloatingText : Entity
    {
        private string text;
        private Color color;
        private float alpha;
        private float duration;
        private float timer;
        private float scale;
        private Vector2 velocity;

        public FloatingText(string text, Vector2 position, Color color, float duration = 2.5f)
            : base(position)
        {
            this.text = text;
            this.color = color;
            this.duration = duration;
            this.alpha = 1f;
            this.scale = 1f;
            this.timer = 0f;
            this.velocity = new Vector2(0f, -20f); // Float upward
            
            Depth = -1000000; // Render on top of everything
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            
            // Move upward
            Position += velocity * Engine.DeltaTime;
            
            // Fade out over time
            float progress = timer / duration;
            if (progress > 0.7f)
            {
                alpha = 1f - ((progress - 0.7f) / 0.3f);
            }
            
            // Scale pulse effect
            scale = 1f + (float)Math.Sin(timer * 5f) * 0.05f;
            
            // Remove when done
            if (timer >= duration)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();
            
            Vector2 screenPosition = Position - SceneAs<Level>().Camera.Position;
            
            // Draw text with outline for visibility
            Vector2 textSize = ActiveFont.Measure(text);
            Vector2 origin = textSize / 2f;
            
            // Draw black outline
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 0 || y != 0)
                    {
                        ActiveFont.Draw(
                            text,
                            screenPosition + new Vector2(x * 2, y * 2),
                            origin,
                            new Vector2(scale, scale),
                            Color.Black * alpha * 0.8f
                        );
                    }
                }
            }
            
            // Draw main text
            ActiveFont.Draw(
                text,
                screenPosition,
                origin,
                new Vector2(scale, scale),
                color * alpha
            );
        }
    }
}
