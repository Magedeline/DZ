namespace Celeste.Entities
{
    /// <summary>
    /// A flying bird entity that can be used for ambient animations or scripted flight sequences.
    /// This is separate from BirdNPC and FlingBird entities.
    /// </summary>
    [CustomEntity(ids: "DZ/FlyingBird")]
    [Tracked(true)]
    [HotReloadable]
    public class FlyingBird : Entity
    {
        // Static particle type for feathers
        public static ParticleType P_Feather;

        // Sprite and visual components
        public Sprite Sprite;
        public VertexLight Light;
        
        // Movement and behavior
        private Vector2[] nodes;
        private int currentNodeIndex;
        private bool isFlying;
        private bool loopPath;
        private float speed = 60f;
        private bool emitFeathers;
        
        // Audio
        private bool disableFlapSfx;
        
        // State
        private Vector2 startPosition;
        private Level level;

        static FlyingBird()
        {
            // Initialize feather particle type
            P_Feather = new ParticleType
            {
                Source = GFX.Game["particles/feather"],
                Color = Color.White,
                Color2 = Color.Gray,
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.8f,
                LifeMax = 1.2f,
                Size = 1f,
                SizeRange = 0.5f,
                SpeedMin = 10f,
                SpeedMax = 20f,
                Direction = (float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 4f,
                Acceleration = new Vector2(0f, 10f),
                RotationMode = ParticleType.RotationModes.SameAsDirection
            };
        }

        public FlyingBird(Vector2 position, Vector2[] nodes, bool loopPath = false, float speed = 60f, bool emitFeathers = false, bool disableFlapSfx = false)
            : base(position)
        {
            this.nodes = nodes ?? new Vector2[0];
            this.loopPath = loopPath;
            this.speed = speed;
            this.emitFeathers = emitFeathers;
            this.disableFlapSfx = disableFlapSfx;
            this.startPosition = position;
            
            Depth = -1000000;
            
            // Create sprite
            Add(Sprite = GFX.SpriteBank.Create("bird"));
            Sprite.Play("fly");
            Sprite.UseRawDeltaTime = false;
            Sprite.OnFrameChange = OnSpriteFrameChange;
            
            // Add light
            Add(Light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 8, 32));
        }

        public FlyingBird(EntityData data, Vector2 offset)
            : this(
                data.Position + offset,
                data.NodesOffset(offset),
                data.Bool("loopPath", false),
                data.Float("speed", 60f),
                data.Bool("emitFeathers", false),
                data.Bool("disableFlapSfx", false))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Start flying if we have nodes
            if (nodes != null && nodes.Length > 0)
            {
                Add(new Coroutine(FlyToNodesRoutine()));
            }
        }

        private void OnSpriteFrameChange(string anim)
        {
            // Play flap sound effect
            if (!disableFlapSfx && level != null && IsOnScreen())
            {
                BirdNPC.FlapSfxCheck(Sprite);
            }
        }

        private bool IsOnScreen()
        {
            if (level == null || level.Camera == null)
                return false;

            var camera = level.Camera;
            return X > camera.Left - 64 && X < camera.Right + 64 &&
                   Y > camera.Top - 64 && Y < camera.Bottom + 64;
        }

        private IEnumerator FlyToNodesRoutine()
        {
            isFlying = true;
            Sprite.Play("fly");
            currentNodeIndex = 0;

            while (isFlying)
            {
                if (nodes == null || nodes.Length == 0)
                    yield break;

                Vector2 targetNode = nodes[currentNodeIndex];
                
                // Face the direction we're flying
                Vector2 direction = (targetNode - Position).SafeNormalize();
                if (direction.X != 0)
                {
                    Sprite.Scale.X = Math.Sign(direction.X);
                }

                // Fly toward the node
                while (Vector2.Distance(Position, targetNode) > 2f)
                {
                    Vector2 moveDirection = (targetNode - Position).SafeNormalize();
                    Position += moveDirection * speed * Engine.DeltaTime;
                    
                    // Emit feather particles if enabled
                    if (emitFeathers && level != null && Scene.OnInterval(0.15f))
                    {
                        level.Particles.Emit(P_Feather, 1, Position + new Vector2(0f, -6f), Vector2.One * 4f);
                    }
                    
                    yield return null;
                }

                // Move to next node
                currentNodeIndex++;
                
                // Handle looping or finishing
                if (currentNodeIndex >= nodes.Length)
                {
                    if (loopPath)
                    {
                        currentNodeIndex = 0;
                    }
                    else
                    {
                        // Finished flying through all nodes
                        isFlying = false;
                        Sprite.Play("idle");
                    }
                }

                yield return null;
            }
        }

        public void StartFlying()
        {
            if (!isFlying && nodes != null && nodes.Length > 0)
            {
                Add(new Coroutine(FlyToNodesRoutine()));
            }
        }

        public void StopFlying()
        {
            isFlying = false;
            Sprite.Play("idle");
        }

        public void FlyAway(bool flyUp = true)
        {
            Add(new Coroutine(FlyAwayRoutine(flyUp)));
        }

        private IEnumerator FlyAwayRoutine(bool flyUp)
        {
            Sprite.Play("fly");
            Light.Visible = false;

            if (!disableFlapSfx)
            {
                Audio.Play("event:/game/general/bird_startle", Position);
            }

            Vector2 flyDirection = flyUp ? new Vector2(0f, -1f) : new Vector2(Sprite.Scale.X, -0.5f);
            flyDirection.Normalize();

            // Emit feathers as we fly away
            if (level != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    level.Particles.Emit(P_Feather, 1, Position + new Vector2(0f, -6f), Vector2.One * 4f);
                    yield return 0.05f;
                }
            }

            // Fly away
            while (level != null)
            {
                Position += flyDirection * 80f * Engine.DeltaTime;
                
                // Remove when off screen
                if (Position.Y < level.Camera.Top - 32f || 
                    Position.X < level.Camera.Left - 32f ||
                    Position.X > level.Camera.Right + 32f)
                {
                    break;
                }

                yield return null;
            }

            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            
            // Add slight bobbing motion when not flying
            if (!isFlying && Sprite.CurrentAnimationID == "idle")
            {
                Sprite.Y = (float)Math.Sin(Scene.TimeActive * 2f) * 2f;
            }
        }

        public override void Render()
        {
            base.Render();
        }
    }
}





