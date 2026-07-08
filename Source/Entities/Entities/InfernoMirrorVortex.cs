namespace Celeste.Entities
{
    /// <summary>
    /// A swirling vortex visual effect for Chapter 7 (Infernal Reflections).
    /// Can be placed inside a mirror to create a hellish portal vortex appearance.
    /// Renders an animated swirling pattern and applies displacement distortion.
    /// </summary>
    [CustomEntity(ids: "DZ/InfernoMirrorVortex")]
    [Tracked]
    public class InfernoMirrorVortex : Entity
    {
        private VirtualRenderTarget buffer;
        private float bufferAlpha;
        private float bufferTimer;
        private float distortionStrength;
        private Color colorFrom;
        private Color colorTo;
        private bool activated;
        private string activationFlag;

        public InfernoMirrorVortex(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = -1000000;
            Collider = new Hitbox(data.Width, data.Height, -data.Width / 2f, -data.Height / 2f);

            colorFrom = Calc.HexToColor(data.Attr("colorFrom", "ff3333"));
            colorTo = Calc.HexToColor(data.Attr("colorTo", "330000"));
            distortionStrength = data.Float("distortion", 0.25f);
            activationFlag = data.Attr("flag", "");

            Add(new BeforeRenderHook(BeforeRender));
            Add(new DisplacementRenderHook(RenderDisplacement));
        }

        public override void Update()
        {
            base.Update();

            Level level = SceneAs<Level>();
            bool shouldActivate = string.IsNullOrEmpty(activationFlag) || 
                                  level.Session.GetFlag(activationFlag);

            if (shouldActivate && !activated)
            {
                activated = true;
            }

            if (activated)
            {
                bufferAlpha = Calc.Approach(bufferAlpha, 1f, Engine.DeltaTime);
                bufferTimer += 4f * Engine.DeltaTime;
            }
            else
            {
                bufferAlpha = Calc.Approach(bufferAlpha, 0f, Engine.DeltaTime * 2f);
            }
        }

        private void BeforeRender()
        {
            if (bufferAlpha <= 0f)
                return;

            int w = (int)Collider.Width;
            int h = (int)Collider.Height;

            if (buffer == null)
                buffer = VirtualContent.CreateRenderTarget("inferno-vortex", w, h);

            Vector2 center = new Vector2(w, h) / 2f;
            MTexture portalTex = GFX.Game["objects/DZ/DZ/temple/portal/portal"];

            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Black);
            Draw.SpriteBatch.Begin();

            for (int i = 0; i < 10; i++)
            {
                float amount = (bufferTimer % 1f) * 0.1f + i / 10f;
                Color color = Color.Lerp(colorTo, colorFrom, amount);
                float scale = amount;
                float rotation = MathHelper.TwoPi * amount;
                portalTex.DrawCentered(center, color, scale, rotation);
            }

            Draw.SpriteBatch.End();
        }

        private void RenderDisplacement()
        {
            if (bufferAlpha <= 0f)
                return;

            float halfW = Collider.Width / 2f;
            float halfH = Collider.Height / 2f;
            Draw.Rect(X - halfW, Y - halfH, Collider.Width, Collider.Height,
                new Color(0.5f, 0.5f, distortionStrength * bufferAlpha, 1f));
        }

        public override void Render()
        {
            base.Render();

            if (buffer != null && bufferAlpha > 0f)
            {
                float halfW = Collider.Width / 2f;
                float halfH = Collider.Height / 2f;
                Draw.SpriteBatch.Draw(
                    (Texture2D)(RenderTarget2D)buffer,
                    Position + new Vector2(-halfW, -halfH),
                    Color.White * bufferAlpha);
            }
        }

        public override void Removed(Scene scene)
        {
            Dispose();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Dispose();
            base.SceneEnd(scene);
        }

        private void Dispose()
        {
            if (buffer != null)
                buffer.Dispose();
            buffer = null;
        }
    }
}
