#nullable enable
namespace MaggyHelper.Entities
{
    internal class PinkPlatberryPoints : Entity
    {
        private readonly bool isGhostBerry;
        private readonly string text;
        private readonly Color color;
        private float alpha = 1.0f;

        public PinkPlatberryPoints(Vector2 position, bool isGhostBerry) : base(position)
        {
            this.isGhostBerry = isGhostBerry;
            text = this.isGhostBerry ? "+200 GHOST" : "+1000 PLATINUM";
            color = this.isGhostBerry ? Color.LightBlue : Calc.HexToColor("f1cff4");
            Depth = -10000;
            
            Add(new Coroutine(routine()));
        }

        private IEnumerator routine()
        {
            // Animate the points text upward and fading
            Vector2 startPos = Position;
            Vector2 endPos = startPos + Vector2.UnitY * -50f;
            
            float timer = 0f;
            float duration = 2f;
            
            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                float progress = timer / duration;
                
                Position = Vector2.Lerp(startPos, endPos, Ease.SineOut(progress));
                alpha = 1.0f - progress;
                
                yield return null;
            }
            
            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            
            if (alpha > 0)
            {
                var size = ActiveFont.Measure(text);
                var drawPos = Position - size * 0.5f;
                
                // Draw outline first (black border)
                ActiveFont.Draw(text, drawPos + Vector2.UnitX, new Vector2(0f, 0f), Vector2.One, Color.Black * alpha);
                ActiveFont.Draw(text, drawPos - Vector2.UnitX, new Vector2(0f, 0f), Vector2.One, Color.Black * alpha);
                ActiveFont.Draw(text, drawPos + Vector2.UnitY, new Vector2(0f, 0f), Vector2.One, Color.Black * alpha);
                ActiveFont.Draw(text, drawPos - Vector2.UnitY, new Vector2(0f, 0f), Vector2.One, Color.Black * alpha);
                
                // Draw main text
                ActiveFont.Draw(text, drawPos, new Vector2(0f, 0f), Vector2.One, color * alpha);
            }
        }
    }
}



