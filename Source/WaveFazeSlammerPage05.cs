namespace Celeste
{
    [HotReloadable]
    public class WaveFazeSlammerPage05 : WaveFazeSlammerPage
    {
        private AreaCompleteTitle title;
        private FancyText.Text messageText;
        private int messageIndex;
        private MTexture clipArt;
        private float clipArtScale;

        public WaveFazeSlammerPage05()
        {
            Transition = Transitions.Rotate3D;
            ClearColor = Calc.HexToColor("d9d2e9");
        }

        public override void Added(WaveFazeSlammerPresentation presentation)
        {
            base.Added(presentation);
            // Try to get dog clip art from the atlas
            if (presentation.Gfx != null && presentation.Gfx.Has("Dog Clip Art"))
            {
                clipArt = presentation.Gfx["Dog Clip Art"];
            }
        }

        public override IEnumerator Routine()
        {
            yield return 1f;
            Audio.Play("event:/new_content/game/10_farewell/ppt_happy_wavedashing");
            title = new AreaCompleteTitle(new Vector2(Width / 2f, 150f), Dialog.Clean("WAVEFAZESLAMMER_PAGE5_TITLE"), 2f, true);
            yield return 1.5f;
            
            // Show message
            messageText = FancyText.Parse(Dialog.Get("WAVEFAZESLAMMER_PAGE5_MESSAGE"), Width - 400, 32, defaultColor: Color.Black * 0.7f);
            float delay = 0.0f;
            for (; messageIndex < messageText.Nodes.Count; ++messageIndex)
            {
                if (messageText.Nodes[messageIndex] is FancyText.NewLine)
                {
                    yield return 0.25f;
                }
                else
                {
                    delay += 0.008f;
                    if (delay >= 0.016f)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
            }
            
            yield return 0.5f;
        }

        public override void Update()
        {
            if (title != null)
            {
                title.Update();
            }
            
            // Gentle pulsing animation for clip art
            clipArtScale = 1.5f + (float)Math.Sin(Engine.Scene.TimeActive * 2f) * 0.1f;
        }

        public override void Render()
        {
            // Render dog clip art if available
            if (clipArt != null)
            {
                clipArt.DrawCentered(new Vector2(Width / 2f, Height / 2f + 50f), Color.White, clipArtScale);
            }
            else
            {
                // Fallback: Draw a simple celebration graphic
                Vector2 center = new Vector2(Width / 2f, Height / 2f + 50f);
                float time = Engine.Scene.TimeActive;
                
                // Draw stars
                for (int i = 0; i < 5; i++)
                {
                    float angle = (MathF.PI * 2f / 5f) * i + time;
                    Vector2 pos = center + Calc.AngleToVector(angle, 200f);
                    float starScale = 1f + (float)Math.Sin(time * 3f + i) * 0.3f;
                    DrawStar(pos, Color.Yellow, starScale * 30f);
                }
            }
            
            // Render title
            if (title != null)
            {
                title.Render();
            }
            
            // Render message
            if (messageText != null)
            {
                messageText.Draw(new Vector2(Width / 2f, Height - 350f), new Vector2(0.5f, 0.0f), Vector2.One, 1f, end: messageIndex);
            }
        }
        
        private void DrawStar(Vector2 position, Color color, float size)
        {
            int points = 5;
            float outerRadius = size;
            float innerRadius = size * 0.4f;
            
            Vector2[] vertices = new Vector2[points * 2];
            for (int i = 0; i < points * 2; i++)
            {
                float angle = (MathF.PI * 2f / (points * 2)) * i - MathF.PI / 2f;
                float radius = (i % 2 == 0) ? outerRadius : innerRadius;
                vertices[i] = position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            }
            
            // Draw star outline
            for (int i = 0; i < vertices.Length; i++)
            {
                int next = (i + 1) % vertices.Length;
                Draw.Line(vertices[i], vertices[next], color, 3f);
            }
        }
    }
}
