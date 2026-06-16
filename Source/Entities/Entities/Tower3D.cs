namespace Celeste.Entities
{
    [CustomEntity(ids: "DZ/Tower3D")]
    [Tracked]
    public class TitanTower3D : Entity
    {
        private global::Celeste.Player player;
        private float playerVerticalPosition = 0f;
        private List<TowerSegment> segments = new List<TowerSegment>();
        private VirtualRenderTarget towerRenderTarget;

        // Model and texture paths
        private const string model_path = "Mountain/Titan_Tower";
        private const string texture_path = "Mountain/Titan_Tower";

        // Tower parameters
        private const float tower_radius = 120f;
        private const float segment_height = 32f;
        private const int segments_count = 30; // Increased for taller tower

        private float rotation = 0f;
        private float rotationSpeed = 1f;
        private Vector2 basePosition;

        // Climbing mechanics
        private float climbingSpeed = 100f;
        private Vector2 lastPlayerPosition;

        // Visual effects
        private List<Vector2> climbingPoints = new List<Vector2>();

        // Obstacle management - commented out due to compilation issues
        // private List<TowerObstacle> ActiveObstacles = new List<TowerObstacle>();
        private float obstacleSpawnTimer = 0f;
        private const float obstacle_spawn_interval = 3f;

        // Tower state
        private Level level;

        // 3D Model and texture resources
        private MTexture towerTexture;
        private bool modelResourcesLoaded = false;

        // Auto properties (fixing IDE0032 warnings)
        public float TowerHeight { get; private set; } = 0f;
        public bool ClimbingEnabled { get; private set; } = false;
        public float GrowthProgress { get; private set; } = 1f; // 0 to 1, for cutscene growth animation
        public bool IsGrowing { get; private set; } = false;
        public bool IsActive { get; private set; } = false;
        public TowerBackgroundStyleground BackgroundStyleground { get; private set; }
        public List<TowerSegment> Obstacles { get; private set; } = new List<TowerSegment>();

        public new float Height => TowerHeight;
        public bool IsClimbingEnabled => ClimbingEnabled;
        public TowerBackgroundStyleground Background => BackgroundStyleground;
        public string ModelPath => model_path;
        public string TexturePath => texture_path;

        public TitanTower3D(Vector2 position) : base(position)
        {
            basePosition = position;
            Depth = -10000; // Render in background

            // Create render target for the tower
            towerRenderTarget = VirtualContent.CreateRenderTarget("tower3d", 320, 180);

            // Load 3D model resources
            loadModelResources();

            // Generate tower segments
            for (int i = 0; i < segments_count; i++)
            {
                segments.Add(new TowerSegment
                {
                    Height = i * segment_height,
                    Texture = towerTexture ?? (GFX.Game.HasAtlasSubtextures("objects/tower/segment") ?
                             GFX.Game["objects/tower/segment"] :
                             GFX.Game["particles/rect"]), // Use loaded texture or fallback
                    Rotation = 0f,
                    ObstacleSlots = new List<Vector2>()
                });
            }

            // Generate climbing points around the tower
            generateClimbingPoints();

            // Generate obstacle attachment points
            generateObstacleSlots();

            TowerHeight = segments_count * segment_height;
        }

        private void loadModelResources()
        {
            try
            {
                // Attempt to load the tower texture from the specified path
                // Note: In Celeste modding, textures are typically loaded through the atlas system
                if (GFX.Game.HasAtlasSubtextures("Mountain/Titan_Tower"))
                {
                    towerTexture = GFX.Game["Mountain/Titan_Tower"];
                    modelResourcesLoaded = true;
                }
                else
                {
                    modelResourcesLoaded = false;
                }
            }
            catch (Exception)
            {
                modelResourcesLoaded = false;
            }
        }

        private void generateClimbingPoints()
        {
            climbingPoints.Clear();

            // Create climbing points in a spiral pattern around the tower
            for (int i = 0; i < segments_count * 2; i++)
            {
                float angle = (i * 0.3f) % MathHelper.TwoPi;
                float height = i * (segment_height * 0.5f);

                Vector2 point = new Vector2(
                    basePosition.X + (float)Math.Cos(angle) * (tower_radius * 0.8f),
                    basePosition.Y - height
                );

                climbingPoints.Add(point);
            }
        }

        private void generateObstacleSlots()
        {
            // Generate attachment points for ActiveObstacles on each segment
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                // Create 4 attachment points around each segment (N, E, S, W)
                for (int j = 0; j < 4; j++)
                {
                    float angle = (MathHelper.TwoPi / 4f) * j;
                    Vector2 attachPoint = new Vector2(
                        (float)Math.Cos(angle) * tower_radius,
                        -segment.Height
                    );

                    segment.ObstacleSlots.Add(attachPoint);
                }
            }
        }

        public void SetGrowthProgress(float progress)
        {
            GrowthProgress = Calc.Clamp(progress, 0f, 1f);
            IsGrowing = progress < 1f;

            // Update background if it exists
            if (BackgroundStyleground != null)
            {
                float bgAlpha = 0.3f + (progress * 0.7f); // Background becomes more visible as tower grows
                BackgroundStyleground.SetAlpha(bgAlpha);
            }

            if (progress >= 1f && !IsActive)
            {
                IsActive = true;
                onTowerCompleted();
            }
        }

        private void onTowerCompleted()
        {
            // Tower is fully grown, start spawning ActiveObstacles
            spawnInitialObstacles();

            // Fully activate background
            if (BackgroundStyleground != null)
            {
                BackgroundStyleground.SetActive(true);
                BackgroundStyleground.SetAlpha(1.0f);
            }
        }

        /// <summary>
        /// Associates an existing background styleground with this tower
        /// </summary>
        public void SetBackground(TowerBackgroundStyleground background)
        {
            BackgroundStyleground = background;
        }

        private void spawnInitialObstacles()
        {
            // Spawn some initial ActiveObstacles
            int obstacleCount = Calc.Random.Range(5, 10);

            for (int i = 0; i < obstacleCount; i++)
            {
            }
        }

        private List<TowerSegment> GetObstacles()
        {
            return Obstacles;
        }

        private void spawnRandomObstacle(List<TowerSegment> ActiveObstacles, List<TowerSegment> activeObstacles)
        {
            spawnRandomObstacle(GetObstacles(), ActiveObstacles);
        }

        private void spawnRandomObstacle(TowerObstacle obstacle)
        {
            if (level == null) return;

            // Choose random segment (but not the first few or last few)
            int segmentIndex = Calc.Random.Range(3, segments.Count - 3);
            var segment = segments[segmentIndex];

            if (segment.ObstacleSlots.Count == 0) return;

            // Choose random attachment point
            int slotIndex = Calc.Random.Range(0, segment.ObstacleSlots.Count);
            Vector2 obstaclePosition = basePosition + segment.ObstacleSlots[slotIndex];

            // Create obstacle with proper parameters based on the StubEntities pattern
            var Obstacles = new TowerObstacle(obstaclePosition, TowerObstacle.ObstacleType.Spikes, TowerObstacle.MovementPattern.Static);
            level.Add((IEnumerable<Entity>)Obstacles);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            player = scene.Tracker.GetEntity<global::Celeste.Player>();
            lastPlayerPosition = player?.Position ?? Vector2.Zero;

            // Find existing background styleground if not already set
            BackgroundStyleground ??= scene.Tracker.GetEntity<TowerBackgroundStyleground>();
        }

        public override void Update()
        {
            base.Update();

            if (player != null && player.Scene != null)
            {
                // Update vertical position based on player Y
                float targetHeight = (Position.Y - player.Y) * 0.5f;
                playerVerticalPosition = Calc.Approach(playerVerticalPosition, targetHeight, 200f * Engine.DeltaTime);

                // Handle climbing mechanics
                if (ClimbingEnabled)
                {
                    handleClimbingMechanics();
                }

                // Rotate tower segments slightly
                foreach (var segment in segments)
                {
                    segment.Rotation += 0.2f * Engine.DeltaTime;
                }
            }

            // Rotate the tower
            rotation += rotationSpeed * Engine.DeltaTime;

            // Make tower sway slightly when growing or during climbing
            float swayAmount = IsGrowing ? 8f : 4f;
            Position.X = basePosition.X + (float)Math.Sin(rotation) * swayAmount;

            // Update obstacle spawning
            if (IsActive && ClimbingEnabled)
            {
                updateObstacleSpawning();
            }

            // Update background integration
            updateBackgroundIntegration();
        }

        private void updateBackgroundIntegration()
        {
            if (BackgroundStyleground != null)
            {
                // Sync background state with tower
                if (IsActive && !BackgroundStyleground.GetActive())
                {
                    BackgroundStyleground.SetActive(true);
                }

                // Adjust background color based on tower activity
                Color towerTint = Color.White;
                if (IsGrowing)
                {
                    towerTint = Color.Lerp(Color.Purple, Color.Gold, GrowthProgress);
                }
                else if (ClimbingEnabled)
                {
                    float intensity = (float)(Math.Sin(Scene.TimeActive * 2) * 0.1 + 0.9);
                    towerTint = Color.Lerp(Color.LightBlue, Color.Cyan, intensity);
                }

                BackgroundStyleground.SetTintColor(towerTint);
            }
        }

        private void updateObstacleSpawning()
        {
            obstacleSpawnTimer += Engine.DeltaTime;

            if (obstacleSpawnTimer >= obstacle_spawn_interval)
            {
                obstacleSpawnTimer = 0f;

                // Chance to spawn new obstacle
                if (Obstacles.Count < 15 && Calc.Random.Chance(0.3f))
                {
                }
            }

            // Clean up ActiveObstacles that are too far from player
            for (int i = Obstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = Obstacles[i];
                if (obstacle.Scene == true || Vector2.Distance(Position, player.Position) > 500f)
                {
                    obstacle.ToString();
                }
            }
        }

        private void handleClimbingMechanics()
        {
            float distanceToTower = Vector2.Distance(player.Position, basePosition);

            if (distanceToTower < tower_radius * 1.2f)
            {
                // Player is near the tower, enable climbing physics
                if (player.StateMachine.State == global::Celeste.Player.StClimb)
                {
                    // Find nearest climbing point
                    Vector2 nearestPoint = getNearestClimbingPoint(player.Position);

                    // Gently pull player toward climbing path
                    Vector2 direction = (nearestPoint - player.Position).SafeNormalize();
                    player.Position += direction * climbingSpeed * 0.3f * Engine.DeltaTime;

                    // Update camera to follow climbing
                    if (level != null)
                    {
                        Vector2 cameraTarget = player.Position + new Vector2(0f, -60f);
                        level.Camera.Position = Vector2.Lerp(level.Camera.Position, cameraTarget, 2f * Engine.DeltaTime);
                    }

                    // Add climbing particles
                    if (Scene.OnInterval(0.2f))
                    {
                        level?.ParticlesFG.Emit(ParticleTypes.Dust, 1, player.Position, Vector2.One * 4f, Color.White);
                    }
                }
            }

            lastPlayerPosition = player.Position;
        }

        private Vector2 getNearestClimbingPoint(Vector2 playerPos)
        {
            Vector2 nearest = climbingPoints[0];
            float minDistance = Vector2.Distance(playerPos, nearest);

            foreach (var point in climbingPoints)
            {
                float distance = Vector2.Distance(playerPos, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = point;
                }
            }

            return nearest;
        }

        public Vector2 GetNearestObstacleSlot(float height)
        {
            // Find the segment closest to the given height
            int segmentIndex = (int)(height / segment_height);
            segmentIndex = Calc.Clamp(segmentIndex, 0, segments.Count - 1);

            var segment = segments[segmentIndex];
            if (segment.ObstacleSlots.Count > 0)
            {
                // Return a random slot from this segment
                int slotIndex = Calc.Random.Range(0, segment.ObstacleSlots.Count);
                return basePosition + segment.ObstacleSlots[slotIndex];
            }

            return basePosition;
        }

        public override void Render()
        {
            if (player == null || player.Scene == null)
                return;

            // Don't render if tower hasn't grown yet
            if (GrowthProgress <= 0f)
                return;

            Camera camera = SceneAs<Level>().Camera;
            Vector2 cameraPos = camera.Position;

            // Draw tower segments from back to front
            for (int layer = 0; layer < 8; layer++)
            {
                float angle = (MathHelper.TwoPi / 8f) * layer;
                float depth = (float)Math.Cos(angle);

                if (depth < 0) continue; // Skip back-facing segments

                int visibleSegments = (int)(segments_count * GrowthProgress);

                for (int i = 0; i < visibleSegments; i++)
                {
                    var segment = segments[i];
                    float segmentY = segment.Height - playerVerticalPosition;

                    // Skip segments too far up or down
                    if (segmentY < -200 || segmentY > 300) continue;

                    float x = Position.X + (float)Math.Sin(angle + segment.Rotation) * tower_radius;
                    float scale = 1f - (depth * 0.3f); // Perspective scaling

                    Color color = Color.Lerp(Color.Gray, Color.White, depth);

                    // Add some color variation based on height
                    float heightFactor = (float)i / segments_count;
                    color = Color.Lerp(color, Color.LightBlue, heightFactor * 0.3f);

                    // Enhanced rendering with custom texture if loaded
                    if (segment.Texture != null)
                    {
                        // Apply additional scaling and effects if using custom model texture
                        if (modelResourcesLoaded && segment.Texture == towerTexture)
                        {
                            // Enhanced rendering for 3D model texture
                            float enhancedScale = scale * (1.0f + (depth * 0.2f));
                            segment.Texture.DrawCentered(
                                new Vector2(x - cameraPos.X, Position.Y + segmentY - cameraPos.Y),
                                color,
                                enhancedScale
                            );
                        }
                        else
                        {
                            // Standard rendering
                            segment.Texture.DrawCentered(
                                new Vector2(x - cameraPos.X, Position.Y + segmentY - cameraPos.Y),
                                color,
                                scale
                            );
                        }
                    }
                    else
                    {
                        // Fallback rectangle drawing
                        Draw.Rect(
                            x - cameraPos.X - 16f * scale,
                            Position.Y + segmentY - cameraPos.Y,
                            32f * scale,
                            segment_height,
                            color
                        );
                    }

                    // Draw obstacle attachment points (debug)
                    if (Engine.Commands.Open)
                    {
                        foreach (var slot in segment.ObstacleSlots)
                        {
                            Vector2 slotWorldPos = basePosition + slot;
                            if (Vector2.Distance(slotWorldPos, player.Position) < 200f)
                            {
                                Draw.Point(slotWorldPos - cameraPos, Color.Orange);
                            }
                        }
                    }
                }
            }

            // Draw climbing points when player is climbing (debug/helper)
            if (ClimbingEnabled && player.StateMachine.State == global::Celeste.Player.StClimb && Engine.Commands.Open)
            {
                foreach (var point in climbingPoints)
                {
                    if (Vector2.Distance(point, player.Position) < 100f)
                    {
                        Draw.Point(point - cameraPos, Color.Yellow);
                    }
                }
            }

            // Draw tower base
            Draw.Circle(basePosition - cameraPos, tower_radius * 0.3f, Color.DarkGray, 8);
        }

        public override void Removed(Scene scene)
        {
            // Clean up background when tower is removed
            if (BackgroundStyleground != null)
            {
                BackgroundStyleground.SetActive(false);
            }

            base.Removed(scene);
        }

        public class TowerSegment
        {
            public float Height;
            public MTexture Texture;
            public float Rotation;
            public List<Vector2> ObstacleSlots;
            public bool Scene;
        }

        public void EnableClimbing(bool b)
        {
            ClimbingEnabled = b;

            if (ClimbingEnabled)
            {
                generateClimbingPoints();
                handleClimbingMechanics();
            }
        }
    }
}



