namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// RealityGlitch - Area where reality breaks down
    /// Visual distortion and random teleportation
    /// Sprite path: effects/reality_glitch/
    /// </summary>
    [CustomEntity("DZ/RealityGlitch")]
    [Tracked]
    public class RealityGlitch : Actor
    {
        #region Enums
        public enum GlitchState
        {
            Stable,
            Minor,
            Major,
            Critical
        }
        #endregion

        #region Properties
        public GlitchState State { get; private set; }
        public float GlitchIntensity { get; private set; }
        public float TeleportChance { get; private set; }
        public Rectangle GlitchArea { get; private set; }
        
        private Sprite sprite;
        private float glitchTimer;
        private Level level;
        private List<GlitchArtifact> artifacts;
        private Random random;
        private Color glitchColor;
        #endregion

        #region Constructor
        public RealityGlitch(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Float("glitchIntensity", 0.5f), data.Float("teleportChance", 0.1f),
                data.Width, data.Height);
        }

        public RealityGlitch(Vector2 position, int width, int height, float glitchIntensity = 0.5f, float teleportChance = 0.1f)
            : base(position)
        {
            Initialize(glitchIntensity, teleportChance, width, height);
        }

        private void Initialize(float glitchIntensity, float teleportChance, int width, int height)
        {
            GlitchIntensity = glitchIntensity;
            TeleportChance = teleportChance;
            
            GlitchArea = new Rectangle((int)Position.X, (int)Position.Y, width, height);
            State = GlitchState.Minor;
            glitchTimer = 0f;
            artifacts = new List<GlitchArtifact>();
            random = new Random();
            glitchColor = Color.Purple;
            
            Collider = new Hitbox(width, height);
            
            Add(sprite = GFX.SpriteBank.Create("reality_glitch"));
            sprite.Play("minor");
        }
        #endregion

        #region Public Methods
        public void SetIntensity(GlitchState newState)
        {
            State = newState;
            sprite.Play(newState.ToString().ToLower());
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            
            glitchTimer += Engine.DeltaTime;
            
            // Create glitch artifacts
            if (Scene.OnInterval(0.05f))
            {
                CreateGlitchArtifact();
            }
            
            // Check player in area
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && GlitchArea.Contains(new Point((int)player.Position.X, (int)player.Position.Y)))
            {
                // Apply glitch effects based on state
                switch (State)
                {
                    case GlitchState.Minor:
                        // Visual distortion only
                        if (random.NextDouble() < GlitchIntensity * 0.5f)
                        {
                            player.Position += new Vector2(random.NextFloat(4f) - 2f, random.NextFloat(4f) - 2f);
                        }
                        break;
                        
                    case GlitchState.Major:
                        // More distortion + occasional teleport
                        if (random.NextDouble() < GlitchIntensity)
                        {
                            player.Position += new Vector2(random.NextFloat(10f) - 5f, random.NextFloat(10f) - 5f);
                        }
                        
                        if (random.NextDouble() < TeleportChance)
                        {
                            TeleportPlayer(player);
                        }
                        break;
                        
                    case GlitchState.Critical:
                        // Severe distortion + frequent teleport
                        if (random.NextDouble() < GlitchIntensity * 1.5f)
                        {
                            player.Position += new Vector2(random.NextFloat(20f) - 10f, random.NextFloat(20f) - 10f);
                        }
                        
                        if (random.NextDouble() < TeleportChance * 2f)
                        {
                            TeleportPlayer(player);
                        }
                        break;
                }
            }
            
            artifacts.RemoveAll(a => a == null || a.Scene == null);
        }

        private void TeleportPlayer(Player player)
        {
            Vector2 newPos = Position + new Vector2(
                random.NextFloat(GlitchArea.Width),
                random.NextFloat(GlitchArea.Height)
            );
            
            player.Position = newPos;
            
            Audio.Play("event:/game/char_maddy/dash", Position);
            level?.Flash(glitchColor * 0.3f);
            
            // Create teleport effect
            for (int i = 0; i < 10; i++)
            {
                CreateGlitchArtifact();
            }
        }

        private void CreateGlitchArtifact()
        {
            var artifact = new GlitchArtifact(
                Position + new Vector2(random.NextFloat(GlitchArea.Width), random.NextFloat(GlitchArea.Height)),
                glitchColor
            );
            artifacts.Add(artifact);
            Scene.Add(artifact);
        }

        public override void Render()
        {
            // Draw glitch area
            float alpha = State switch
            {
                GlitchState.Minor => 0.1f,
                GlitchState.Major => 0.2f,
                GlitchState.Critical => 0.3f,
                _ => 0f
            };
            
            Draw.Rect(GlitchArea, glitchColor * alpha);
            
            // Draw glitch lines
            if (State != GlitchState.Stable)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 start = Position + new Vector2(random.NextFloat(GlitchArea.Width), random.NextFloat(GlitchArea.Height));
                    Vector2 end = start + new Vector2(random.NextFloat(20f) - 10f, random.NextFloat(20f) - 10f);
                    Draw.Line(start, end, glitchColor * 0.5f, 1f);
                }
            }
            
            base.Render();
        }
        #endregion
    }

    public class GlitchArtifact : Actor
    {
        private float lifetime;
        private float maxLifetime;
        private Color color;
        private int size;

        public GlitchArtifact(Vector2 position, Color color)
            : base(position)
        {
            this.color = color;
            maxLifetime = Calc.Random.NextFloat() * (0.3f - 0.1f) + 0.1f;
            lifetime = maxLifetime;
            size = Calc.Random.Next(4, 12);
        }

        public override void Update()
        {
            base.Update();
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Rect(Position, size, size, color * (alpha * 0.5f));
        }
    }
}
