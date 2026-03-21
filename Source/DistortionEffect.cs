namespace MaggyHelper.Effects.ShaderEffects
{
    /// <summary>
    /// Applies distortion, displacement, and chromatic aberration effects.
    /// Based on Distort.fx shader from CelesteEffects.
    /// </summary>
    [HotReloadable]
    public class DistortionEffect : Backdrop
    {
        private Effect shader;
        private float anxiety = 0f;
        private float waterAlpha = 1f;
        private float gamerate = 1f;
        private float timer = 0f;

        public float Anxiety
        {
            get => anxiety;
            set => anxiety = MathHelper.Clamp(value, 0f, 1f);
        }

        public float WaterAlpha
        {
            get => waterAlpha;
            set => waterAlpha = MathHelper.Clamp(value, 0f, 1f);
        }

        public float GameRate
        {
            get => gamerate;
            set => gamerate = value;
        }

        public DistortionEffect() : base()
        {
            UseSpritebatch = false;
            
            // Load the distortion shader
            shader = EffectManager.LoadEffect("Distort");
            
            if (shader == null)
            {
                IngesteLogger.Error("Failed to load Distort shader effect");
            }

        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            timer += Engine.DeltaTime * gamerate;
        }

        public override void Render(Scene scene)
        {
            if (shader == null)
            {
                return;
            }

            try
            {
                // LOD check: background effect budget
                var level = scene as Level;
                if (!LODManager.ShouldRender(LODManager.EffectCategory.Background, null, level))
                {
                    return;
                }

                // Acquire pooled render target
                var buffer = RenderTargetPool.Get(Engine.Width, Engine.Height);

                // Set shader parameters
                shader.Parameters["anxiety"]?.SetValue(anxiety);
                shader.Parameters["waterAlpha"]?.SetValue(waterAlpha);
                shader.Parameters["gamerate"]?.SetValue(gamerate);
                shader.Parameters["timer"]?.SetValue(timer);

                // Apply the shader effect
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

                // Render the scene content
                base.Render(scene);

                Draw.SpriteBatch.End();

                // Restore default render target
                Engine.Graphics.GraphicsDevice.SetRenderTarget(null);

                // Draw the processed buffer to screen
                Draw.SpriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone
                );

                Draw.SpriteBatch.Draw(buffer, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();

                // Restart spritebatch for next render operations
                Draw.SpriteBatch.Begin();

                // Return pooled target
                RenderTargetPool.Return(buffer);
            }
            catch (Exception ex)
            {
                IngesteLogger.Error(string.Format("Error rendering DistortionEffect: {0}", ex.Message));
            }
        }
    }
}




