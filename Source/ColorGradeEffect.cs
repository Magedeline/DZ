namespace MaggyHelper.Effects.ShaderEffects
{
    /// <summary>
    /// Applies color grading using LUT (Look-Up Table) textures.
    /// Based on ColorGrade.fx shader from CelesteEffects.
    /// </summary>
    [HotReloadable]
    public class ColorGradeEffect : Backdrop
    {
        private Effect shader;
        private Texture2D lutTexture;
        private string lutName;
        private float intensity = 1f;

        public float Intensity
        {
            get => intensity;
            set => intensity = MathHelper.Clamp(value, 0f, 1f);
        }

        public ColorGradeEffect(string lutTextureName = "default") : base()
        {
            UseSpritebatch = false;
            lutName = lutTextureName;
            
            shader = EffectManager.LoadEffect("ColorGrade");
            
            if (shader == null)
            {
                IngesteLogger.Error("Failed to load ColorGrade shader effect");
            }

            
            // Load LUT texture if available
            LoadLUTTexture(lutTextureName);
        }

        private void LoadLUTTexture(string lutTextureName)
        {
            try
            {
                // Try to load LUT from Graphics/ColorGrading directory
                string lutPath = string.Format("Graphics/ColorGrading/{0}", lutTextureName);
                
                if (Everest.Content.TryGet(lutPath, out ModAsset asset))
                {
                    using (var stream = asset.Stream)
                    {
                        lutTexture = Texture2D.FromStream(Engine.Graphics.GraphicsDevice, stream);
                        IngesteLogger.Info(string.Format("Loaded LUT texture: {0}", lutTextureName));
                    }
                }
                else
                {
                    IngesteLogger.Warn(string.Format("LUT texture not found: {0}", lutTextureName));
                }
            }
            catch (Exception ex)
            {
                IngesteLogger.Error(string.Format("Error loading LUT texture: {0}", ex.Message));
            }
        }

        public void SetLUT(string lutTextureName)
        {
            if (lutName != lutTextureName)
            {
                lutName = lutTextureName;
                LoadLUTTexture(lutTextureName);
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
        }

        public override void Render(Scene scene)
        {
            if (shader == null || lutTexture == null || intensity <= 0f)
            {
                base.Render(scene);
                return;
            }

            try
            {
                var level = scene as Level;
                if (!LODManager.ShouldRender(LODManager.EffectCategory.Background, null, level))
                {
                    base.Render(scene);
                    return;
                }

                var buffer = RenderTargetPool.Get(Engine.Width, Engine.Height);
                // Set shader parameters
                shader.Parameters["intensity"]?.SetValue(intensity);
                shader.Parameters["LutTexture"]?.SetValue(lutTexture);

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
                IngesteLogger.Error(string.Format("Error rendering ColorGradeEffect: {0}", ex.Message));
            }
        }
    }
}




