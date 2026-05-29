using System.Threading;

namespace Celeste
{
    [HotReloadable]
    public class WaveFazePresentation : Entity
    {
        public Vector2 ScaleInPoint = new Vector2(1920f, 1080f) / 2f;
        public readonly int ScreenWidth = 1920;
        public readonly int ScreenHeight = 1080;
        private float ease;
        private bool loading;
        private float waitingForInputTime;
        private VirtualRenderTarget screenBuffer;
        private VirtualRenderTarget prevPageBuffer;
        private VirtualRenderTarget currPageBuffer;
        private int pageIndex;
        private List<WaveFazePage> pages = new List<WaveFazePage>();
        private float pageEase;
        private bool pageTurning;
        private bool pageUpdating;
        private bool waitingForPageTurn;
        private VertexPositionColorTexture[] verts = new VertexPositionColorTexture[6];
        private bool audioPlaying = false;

        public bool Viewing { get; private set; }

        public Atlas Gfx { get; private set; }

        public bool ShowInput
        {
            get
            {
                if (!waitingForPageTurn)
                {
                    if (CurrPage != null)
                    {
                        return CurrPage.WaitingForInput;
                    }
                    return false;
                }
                return true;
            }
        }

        private WaveFazePage PrevPage
        {
            get
            {
                if (pageIndex <= 0)
                {
                    return null;
                }
                return pages[pageIndex - 1];
            }
        }

        private WaveFazePage CurrPage
        {
            get
            {
                if (pageIndex >= pages.Count)
                {
                    return null;
                }
                return pages[pageIndex];
            }
        }

        public WaveFazePresentation(bool playAudio = false)
        {
            base.Tag = Tags.HUD;
            Viewing = true;
            loading = true;
            audioPlaying = playAudio;
            Add(new Coroutine(Routine()));
            RunThread.Start(LoadingThread, "Wave Faze Presentation Loading", highPriority: true);
        }

        private void LoadingThread()
        {
            Gfx = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "WaveFazing"), Atlas.AtlasDataFormat.Packer);
            loading = false;
        }

        private IEnumerator Routine()
        {
            while (loading)
            {
                yield return null;
            }
            
            pages.Add(new WaveFazePage00());
            pages.Add(new WaveFazePage01());
            pages.Add(new WaveFazePage02());
            pages.Add(new WaveFazePage03());
            pages.Add(new WaveFazePage04());
            pages.Add(new WaveFazePage05());
            pages.Add(new WaveFazePage06());
            
            foreach (WaveFazePage page in pages)
            {
                page.Added(this);
            }
            
            Add(new BeforeRenderHook(BeforeRender));
            
            while (ease < 1f)
            {
                ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 2f);
                yield return null;
            }
            
            if (audioPlaying)
            {
                Audio.Play("event:/pusheen/final_content/music/lvl19/part01");
            }
            
            while (pageIndex < pages.Count)
            {
                pageUpdating = true;
                yield return CurrPage.Routine();
                
                if (!CurrPage.AutoProgress)
                {
                    waitingForPageTurn = true;
                    while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
                    {
                        yield return null;
                    }
                    waitingForPageTurn = false;
                    
                    if (Input.MenuCancel.Pressed)
                    {
                        break;
                    }
                    
                    Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
                }
                
                pageUpdating = false;
                pageIndex++;
                
                if (pageIndex < pages.Count)
                {
                    float duration = 0.5f;
                    if (CurrPage.Transition == WaveFazePage.Transitions.Rotate3D)
                    {
                        duration = 1.5f;
                    }
                    else if (CurrPage.Transition == WaveFazePage.Transitions.Blocky)
                    {
                        duration = 1f;
                    }
                    
                    pageTurning = true;
                    pageEase = 0f;
                    Add(new Coroutine(TurnPage(duration)));
                    yield return duration * 0.8f;
                }
            }
            
            Audio.Play("event:/new_content/game/10_farewell/cafe_computer_off");
            
            while (ease > 0f)
            {
                ease = Calc.Approach(ease, 0f, Engine.DeltaTime * 2f);
                yield return null;
            }
            
            Viewing = false;
            RemoveSelf();
        }

        private IEnumerator TurnPage(float duration)
        {
            if (CurrPage.Transition != WaveFazePage.Transitions.ScaleIn && CurrPage.Transition != WaveFazePage.Transitions.FadeIn)
            {
                if (CurrPage.Transition == WaveFazePage.Transitions.Rotate3D)
                {
                    Audio.Play("event:/new_content/game/10_farewell/ppt_cube_transition");
                }
                else if (CurrPage.Transition == WaveFazePage.Transitions.Blocky)
                {
                    Audio.Play("event:/new_content/game/10_farewell/ppt_dissolve_transition");
                }
                else if (CurrPage.Transition == WaveFazePage.Transitions.Spiral)
                {
                    Audio.Play("event:/new_content/game/10_farewell/ppt_spinning_transition");
                }
            }
            
            while (pageEase < 1f)
            {
                pageEase += Engine.DeltaTime / duration;
                yield return null;
            }
            
            pageTurning = false;
        }

        private void BeforeRender()
        {
            if (loading)
            {
                return;
            }
            
            if (screenBuffer == null || screenBuffer.IsDisposed)
            {
                screenBuffer = VirtualContent.CreateRenderTarget("WaveFaze-Buffer", ScreenWidth, ScreenHeight, depth: true);
            }
            if (prevPageBuffer == null || prevPageBuffer.IsDisposed)
            {
                prevPageBuffer = VirtualContent.CreateRenderTarget("WaveFaze-Screen1", ScreenWidth, ScreenHeight);
            }
            if (currPageBuffer == null || currPageBuffer.IsDisposed)
            {
                currPageBuffer = VirtualContent.CreateRenderTarget("WaveFaze-Screen2", ScreenWidth, ScreenHeight);
            }
            
            if (pageTurning && PrevPage != null)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(prevPageBuffer);
                Engine.Graphics.GraphicsDevice.Clear(PrevPage.ClearColor);
                Draw.SpriteBatch.Begin();
                PrevPage.Render();
                Draw.SpriteBatch.End();
            }
            
            if (CurrPage != null)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(currPageBuffer);
                Engine.Graphics.GraphicsDevice.Clear(CurrPage.ClearColor);
                Draw.SpriteBatch.Begin();
                CurrPage.Render();
                Draw.SpriteBatch.End();
            }
            
            Engine.Graphics.GraphicsDevice.SetRenderTarget(screenBuffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Black);
            
            if (pageTurning)
            {
                if (CurrPage.Transition == WaveFazePage.Transitions.ScaleIn)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                    Vector2 scale = Vector2.One * pageEase;
                    Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, ScaleInPoint, currPageBuffer.Bounds, Color.White, 0f, ScaleInPoint, scale, SpriteEffects.None, 0f);
                    Draw.SpriteBatch.End();
                }
                else if (CurrPage.Transition == WaveFazePage.Transitions.FadeIn)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White * pageEase);
                    Draw.SpriteBatch.End();
                }
                else if (CurrPage.Transition == WaveFazePage.Transitions.Rotate3D)
                {
                    float rotation = -MathF.PI / 2f * pageEase;
                    RenderQuad((RenderTarget2D)prevPageBuffer, pageEase, rotation);
                    RenderQuad((RenderTarget2D)currPageBuffer, pageEase, MathF.PI / 2f + rotation);
                }
                else if (CurrPage.Transition == WaveFazePage.Transitions.Blocky)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                    uint seed = 1u;
                    int blockSize = ScreenWidth / 60;
                    for (int i = 0; i < ScreenWidth; i += blockSize)
                    {
                        for (int j = 0; j < ScreenHeight; j += blockSize)
                        {
                            if (PseudoRandRange(ref seed, 0f, 1f) <= pageEase)
                            {
                                Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, new Rectangle(i, j, blockSize, blockSize), new Rectangle(i, j, blockSize, blockSize), Color.White);
                            }
                        }
                    }
                    Draw.SpriteBatch.End();
                }
                else if (CurrPage.Transition == WaveFazePage.Transitions.Spiral)
                {
                    Draw.SpriteBatch.Begin();
                    Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                    Vector2 scale = Vector2.One * pageEase;
                    float rot = (1f - pageEase) * 12f;
                    Vector2 center = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);
                    Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, center, currPageBuffer.Bounds, Color.White, rot, center, scale, SpriteEffects.None, 0f);
                    Draw.SpriteBatch.End();
                }
            }
            else
            {
                Draw.SpriteBatch.Begin();
                Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
        }

        private void RenderQuad(Texture texture, float ease, float rotation)
        {
            float aspectRatio = (float)screenBuffer.Width / (float)screenBuffer.Height;
            float halfWidth = aspectRatio;
            float halfHeight = 1f;
            
            Vector3 topLeft = new Vector3(-halfWidth, halfHeight, 0f);
            Vector3 topRight = new Vector3(halfWidth, halfHeight, 0f);
            Vector3 bottomRight = new Vector3(halfWidth, -halfHeight, 0f);
            Vector3 bottomLeft = new Vector3(-halfWidth, -halfHeight, 0f);
            
            verts[0].Position = topLeft;
            verts[0].TextureCoordinate = new Vector2(0f, 0f);
            verts[0].Color = Color.White;
            verts[1].Position = topRight;
            verts[1].TextureCoordinate = new Vector2(1f, 0f);
            verts[1].Color = Color.White;
            verts[2].Position = bottomRight;
            verts[2].TextureCoordinate = new Vector2(1f, 1f);
            verts[2].Color = Color.White;
            verts[3].Position = topLeft;
            verts[3].TextureCoordinate = new Vector2(0f, 0f);
            verts[3].Color = Color.White;
            verts[4].Position = bottomRight;
            verts[4].TextureCoordinate = new Vector2(1f, 1f);
            verts[4].Color = Color.White;
            verts[5].Position = bottomLeft;
            verts[5].TextureCoordinate = new Vector2(0f, 1f);
            verts[5].Color = Color.White;
            
            float distance = 4.15f + Calc.YoYo(ease) * 1.7f;
            Matrix transform = Matrix.CreateTranslation(0f, 0f, aspectRatio) 
                * Matrix.CreateRotationY(rotation) 
                * Matrix.CreateTranslation(0f, 0f, -distance) 
                * Matrix.CreatePerspectiveFieldOfView(MathF.PI / 4f, aspectRatio, 1f, 10f);

            GraphicsDevice graphicsDevice = Engine.Instance.GraphicsDevice;
            RasterizerState previousRasterizerState = graphicsDevice.RasterizerState;
            BlendState previousBlendState = graphicsDevice.BlendState;
            DepthStencilState previousDepthStencilState = graphicsDevice.DepthStencilState;
            Texture previousTexture = graphicsDevice.Textures[0];

            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.Textures[0] = texture;

            GFX.FxTexture.Parameters["World"].SetValue(transform);
            foreach (EffectPass pass in GFX.FxTexture.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
            }

            graphicsDevice.Textures[0] = previousTexture;
            graphicsDevice.DepthStencilState = previousDepthStencilState;
            graphicsDevice.BlendState = previousBlendState;
            graphicsDevice.RasterizerState = previousRasterizerState;
        }

        public override void Update()
        {
            base.Update();
            
            if (ShowInput)
            {
                waitingForInputTime += Engine.DeltaTime;
            }
            else
            {
                waitingForInputTime = 0f;
            }
            
            if (!loading && CurrPage != null && pageUpdating)
            {
                CurrPage.Update();
            }
        }

        public override void Render()
        {
            if (!loading && screenBuffer != null && !screenBuffer.IsDisposed)
            {
                float width = (float)ScreenWidth * Ease.CubeOut(Calc.ClampedMap(ease, 0f, 0.5f));
                float height = (float)ScreenHeight * Ease.CubeInOut(Calc.ClampedMap(ease, 0.5f, 1f, 0.2f));
                Rectangle destRect = new Rectangle((int)((1920f - width) / 2f), (int)((1080f - height) / 2f), (int)width, (int)height);
                
                Draw.SpriteBatch.Draw((RenderTarget2D)screenBuffer, destRect, Color.White);
                
                if (ShowInput && waitingForInputTime > 0.2f)
                {
                    GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1856f, 1016 + ((base.Scene.TimeActive % 1f < 0.25f) ? 6 : 0)), Color.Black);
                }
                
                if ((base.Scene as Level).Paused)
                {
                    Draw.Rect(destRect, Color.Black * 0.7f);
                }
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose();
        }

        private void Dispose()
        {
            while (loading)
            {
                Thread.Sleep(1);
            }
            
            if (screenBuffer != null)
            {
                screenBuffer.Dispose();
            }
            screenBuffer = null;
            
            if (prevPageBuffer != null)
            {
                prevPageBuffer.Dispose();
            }
            prevPageBuffer = null;
            
            if (currPageBuffer != null)
            {
                currPageBuffer.Dispose();
            }
            currPageBuffer = null;
            
            if (Gfx != null)
            {
                Gfx.Dispose();
            }
            Gfx = null;
        }

        private static uint PseudoRand(ref uint seed)
        {
            uint num = seed;
            num ^= num << 13;
            num ^= num >> 17;
            return seed = num ^ (num << 5);
        }

        public static float PseudoRandRange(ref uint seed, float min, float max)
        {
            return min + (float)(PseudoRand(ref seed) % 1000) / 1000f * (max - min);
        }
    }
}
