namespace MaggyHelper.Entities {
    /// <summary>
    /// Background styleground for the 3D Tower with tiled and parallax elements
    /// </summary>
    [CustomEntity(ids: "MaggyHelper/TowerBackgroundStyleground")]
    public partial class TowerBackgroundStyleground : Entity {
        #region Fields
        private MTexture[] tileTextures;
        private MTexture[] backgroundLayers;
        private VirtualRenderTarget renderTarget;
        private List<TowerTile> tiles;
        private List<ParallaxLayer> parallaxLayers;
        private Vector2 cameraOffset;
        private float animationTimer;
        private float alpha = 1f;
        private bool isActive = true;
        private Color tintColor = Color.White;
        private TitanTower3D associatedTower;

        // Tileset configuration
        private const int tile_size = 8;
        private const int tiles_per_row = 40;
        private const int tiles_per_column = 30;
        private const float parallax_speed_base = 0.3f;
        #endregion

        #region Properties
        public new bool Active {
            get => isActive;
            set => isActive = value;
        }

        public float Alpha { get; set; }

        #endregion

        #region Structures
        private struct TowerTile {
            public Vector2 Position;
            public int TextureIndex;
            public float AnimationOffset;
            public Color Tint;
            public float Scale;
            public float Rotation;
        }

        private struct ParallaxLayer {
            public MTexture Texture;
            public Vector2 Position;
            public Vector2 ScrollSpeed;
            public float Depth;
            public Color Tint;
            public float Scale;
        }
        #endregion

        public TowerBackgroundStyleground(Vector2 position) : base(position) {
            Depth = 10000;
            tintColor = Color.White;
            alpha = 1.0f;

            initialize();
        }

        private void initialize() {
            // Load tileset textures
            loadTileTextures();

            // Load background layer textures
            loadBackgroundLayers();

            // Initialize tile grid
            initializeTileGrid();

            // Initialize parallax layers
            initializeParallaxLayers();

            // Create render target
            renderTarget = VirtualContent.CreateRenderTarget("tower_background", 320, 180);

            isActive = true;
        }

        private void loadTileTextures() {
            var textureList = new List<MTexture>();

            // Try to load custom tower background tiles
            if (GFX.Game.Has("objects/temple/bg_a"))
                textureList.Add(GFX.Game["objects/temple/bg_a"]);
            if (GFX.Game.Has("objects/temple/bg_b"))
                textureList.Add(GFX.Game["objects/temple/bg_b"]);
            if (GFX.Game.Has("objects/temple/bg_c"))
                textureList.Add(GFX.Game["objects/temple/bg_c"]);

            // Add some basic geometric patterns
            if (GFX.Game.Has("particles/rect"))
                textureList.Add(GFX.Game["particles/rect"]);

            // Ensure we have at least one texture
            if (textureList.Count == 0) {
                textureList.Add(GFX.Game["particles/rect"]);
            }

            tileTextures = textureList.ToArray();
        }

        private void loadBackgroundLayers() {
            var layerList = new List<MTexture>();

            // Fallback to existing background textures
            if (GFX.Game.Has("bgs/02/bg"))
                layerList.Add(GFX.Game["bgs/02/bg"]);
            if (GFX.Game.Has("bgs/03/bg"))
                layerList.Add(GFX.Game["bgs/03/bg"]);
            if (GFX.Game.Has("bgs/04/bg"))
                layerList.Add(GFX.Game["bgs/04/bg"]);

            backgroundLayers = layerList.ToArray();
        }

        private void initializeTileGrid() {
            tiles = new List<TowerTile>();

            for (int x = 0; x < tiles_per_row; x++) {
                for (int y = 0; y < tiles_per_column; y++) {
                    var tile = new TowerTile {
                        Position = new Vector2(x * tile_size, y * tile_size),
                        TextureIndex = Calc.Random.Range(0, tileTextures.Length),
                        AnimationOffset = Calc.Random.NextFloat() * MathHelper.TwoPi,
                        Tint = Color.Lerp(Color.DarkBlue, Color.Purple, Calc.Random.NextFloat()),
                        Scale = 0.8f + Calc.Random.NextFloat() * 0.4f,
                        Rotation = Calc.Random.NextFloat() * MathHelper.TwoPi
                    };

                    // Create pattern variations based on position
                    if ((x + y) % 3 == 0) {
                        tile.Tint = Color.Lerp(tile.Tint, Color.Gold, 0.3f);
                    }

                    tiles.Add(tile);
                }
            }
        }

        private void initializeParallaxLayers() {
            parallaxLayers = new List<ParallaxLayer>();

            if (backgroundLayers.Length > 0) {
                // Create multiple parallax layers with different speeds
                for (int i = 0; i < Math.Min(backgroundLayers.Length, 4); i++) {
                    var layer = new ParallaxLayer {
                        Texture = backgroundLayers[i % backgroundLayers.Length],
                        Position = Vector2.Zero,
                        ScrollSpeed = new Vector2(
                            parallax_speed_base * (0.1f + i * 0.1f),
                            parallax_speed_base * (0.05f + i * 0.05f)
                        ),
                        Depth = 1.0f - i * 0.2f,
                        Tint = Color.Lerp(Color.DarkBlue, Color.Purple, i * 0.25f),
                        Scale = 1.0f + i * 0.1f
                    };

                    parallaxLayers.Add(layer);
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // Find associated tower
            if (scene.Tracker.GetEntity<TitanTower3D>() is TitanTower3D tower) {
                associatedTower = tower;
            }
        }

        public override void Update() {
            base.Update();

            if (!isActive) return;

            animationTimer += Engine.DeltaTime;

            // Update camera offset for parallax
            if (Scene is Level level) {
                cameraOffset = level.Camera.Position;
            }

            // Update parallax layers
            updateParallaxLayers();

            // Update tile animations
            updateTileAnimations();

            // Sync with tower if available
            if (associatedTower != null) {
                syncWithTower();
            }
        }

        private void updateParallaxLayers() {
            for (int i = 0; i < parallaxLayers.Count; i++) {
                var layer = parallaxLayers[i];

                // Update position based on camera and scroll speed
                layer.Position = -cameraOffset * layer.ScrollSpeed +
                               new Vector2(
                                   (float)Math.Sin(animationTimer * 0.5f + i) * 10f,
                                   (float)Math.Cos(animationTimer * 0.3f + i) * 5f
                               );

                parallaxLayers[i] = layer;
            }
        }

        private void updateTileAnimations() {
            for (int i = 0; i < tiles.Count; i++) {
                var tile = tiles[i];

                // Gentle pulsing animation
                float pulse = (float)Math.Sin(animationTimer * 2f + tile.AnimationOffset) * 0.1f + 1f;
                tile.Scale = (0.8f + Calc.Random.NextFloat() * 0.4f) * pulse;

                // Slow rotation
                tile.Rotation += Engine.DeltaTime * 0.1f;

                tiles[i] = tile;
            }
        }

        private void syncWithTower() {
            // Adjust alpha based on tower activity
            if (associatedTower.IsActive) {
                alpha = Calc.Approach(alpha, 1.0f, Engine.DeltaTime);
            } else {
                alpha = Calc.Approach(alpha, 0.3f, Engine.DeltaTime);
            }

            // Adjust tint based on tower height or state
            float towerInfluence = Math.Min(((Entity)associatedTower).Height / 1000f, 1f);
            tintColor = Color.Lerp(Color.DarkBlue, Color.Purple, towerInfluence);
        }

        public override void Render() {
            if (!isActive || alpha <= 0) return;

            var level = Scene as Level;
            if (level == null) return;

            // Render to target for post-processing
            Engine.Graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.Identity);

            // Render parallax layers first
            renderParallaxLayers();

            // Render tile grid
            renderTileGrid();

            Draw.SpriteBatch.End();

            // Render the result to screen
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, level.Camera.Matrix);

            Draw.SpriteBatch.Draw(renderTarget, Vector2.Zero, tintColor * alpha);

            Draw.SpriteBatch.End();
        }

        private void renderParallaxLayers() {
            foreach (var layer in parallaxLayers) {
                if (layer.Texture != null) {
                    // Tile the texture across the screen
                    Vector2 textureSize = new Vector2(layer.Texture.Width, layer.Texture.Height) * layer.Scale;
                    Vector2 startPos = layer.Position;

                    // Calculate how many tiles we need
                    int tilesX = (int)Math.Ceiling(320f / textureSize.X) + 2;
                    int tilesY = (int)Math.Ceiling(180f / textureSize.Y) + 2;

                    // Wrap the starting position
                    startPos.X = ((startPos.X % textureSize.X) + textureSize.X) % textureSize.X - textureSize.X;
                    startPos.Y = ((startPos.Y % textureSize.Y) + textureSize.Y) % textureSize.Y - textureSize.Y;

                    for (int x = 0; x < tilesX; x++) {
                        for (int y = 0; y < tilesY; y++) {
                            Vector2 pos = startPos + new Vector2(x * textureSize.X, y * textureSize.Y);
                            layer.Texture.Draw(pos, Vector2.Zero, layer.Tint * layer.Depth, layer.Scale);
                        }
                    }
                }
            }
        }

        private void renderTileGrid() {
            foreach (var tile in tiles) {
                if (tile.TextureIndex < tileTextures.Length) {
                    var texture = tileTextures[tile.TextureIndex];
                    Vector2 renderPos = tile.Position - cameraOffset * 0.8f; // Slight parallax for tiles

                    texture.DrawCentered(
                        renderPos + texture.Center,
                        tile.Tint * alpha,
                        tile.Scale,
                        tile.Rotation
                    );
                }
            }
        }

        public void SetAlpha(float value) {
            alpha = MathHelper.Clamp(value, 0f, 1f);
        }

        public void SetActive(bool active) {
            isActive = active;
            Visible = active;
        }

        public void SetTintColor(Color color) {
            tintColor = color;
        }

        public float GetAlpha() => alpha;
        public bool GetActive() => isActive;
        public Color GetTintColor() => tintColor;

        public override void Removed(Scene scene) {
            renderTarget?.Dispose();
            base.Removed(scene);
        }
    }
}



