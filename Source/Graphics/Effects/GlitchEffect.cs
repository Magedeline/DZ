namespace Celeste.Effects.ShaderEffects
{
    /// <summary>
    /// Applies glitch/corruption visual effects with randomized pixelation and RGB offset.
    /// Based on Glitch.fx shader from CelesteEffects.
    /// </summary>
    [HotReloadable]
    public class GlitchEffect : Backdrop
    {
        private Effect shader;
        private float glitchAmount = 0f;
        private float amplitude = 0.05f;
        private float timer = 0f;
        private Random random;

        public float GlitchAmount
        {
            get => glitchAmount;
            set => glitchAmount = MathHelper.Clamp(value, 0f, 1f);
        }

        public float Amplitude
        {
            get => amplitude;
            set => amplitude = value;
        }

        public GlitchEffect() : base()
        {
            UseSpritebatch = false;
            random = new Random();
            
            shader = EffectManager.LoadEffect("Glitch");
            
            if (shader == null)
            {
                IngesteLogger.Error("Failed to load Glitch shader effect");
            }

        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            timer += Engine.DeltaTime;
            
            if (glitchDuration > 0f)
            {
                glitchDuration -= Engine.DeltaTime;
                if (glitchDuration <= 0f)
                {
                    GlitchAmount = 0f;
                }
            }
        }

        public override void Render(Scene scene)
        {
            if (shader == null || glitchAmount <= 0f)
            {
                base.Render(scene);
                return;
            }

            try
            {
                // LOD check: background category
                var level = scene as Level;
                if (!LODManager.ShouldRender(LODManager.EffectCategory.Background, null, level))
                {
                    base.Render(scene);
                    return;
                }

                var buffer = RenderTargetPool.Get(Engine.Width, Engine.Height);
                // Set shader parameters
                shader.Parameters["glitch"]?.SetValue(glitchAmount);
                shader.Parameters["amplitude"]?.SetValue(amplitude);
                shader.Parameters["timer"]?.SetValue(timer);
                shader.Parameters["randomSeed"]?.SetValue((float)random.NextDouble());

                // Render with glitch effect
                Draw.SpriteBatch.End();

                Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

                Draw.SpriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    shader
                );

                base.Render(scene);

                Draw.SpriteBatch.End();

                Engine.Graphics.GraphicsDevice.SetRenderTarget(null);

                Draw.SpriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone
                );

                Draw.SpriteBatch.Draw(buffer, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();

                Draw.SpriteBatch.Begin();

                RenderTargetPool.Return(buffer);
            }
            catch (Exception ex)
            {
                IngesteLogger.Error(string.Format("Error rendering GlitchEffect: {0}", ex.Message));
            }
        }

        private float glitchDuration = 0f;
        
        /// <summary>
        /// Trigger a temporary glitch spike for dramatic effect.
        /// </summary>
        public void TriggerGlitch(float duration = 0.5f, float intensity = 0.8f)
        {
            GlitchAmount = intensity;
            glitchDuration = duration;
        }

        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            shader = null;
        }
    }
}




