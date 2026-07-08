namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// GlitchBlock - Unstable digital block that flickers in and out
    /// Appears and disappears randomly, creating dynamic platforming challenges
    /// Sprite path: objects/DZ/DZ/glitch_block/
    /// </summary>
    [CustomEntity("DZ/GlitchBlock")]
    [Tracked]
    public class GlitchBlock : Solid
    {
        #region Enums
        public enum GlitchState
        {
            Stable,
            Flickering,
            Invisible,
            Appearing,
            Disappearing,
            Corrupted
        }
        #endregion

        #region Properties
        public GlitchState State { get; private set; }
        public float Stability { get; private set; }
        public float GlitchInterval { get; private set; }
        public float VisibleTime { get; private set; }
        public float InvisibleTime { get; private set; }
        public bool IsVisible => State == GlitchState.Stable || State == GlitchState.Flickering;
        
        private Sprite sprite;
        private float stateTimer;
        private float glitchTimer;
        private int patternIndex;
        private Level level;
        private List<GlitchParticle> glitchParticles;
        private bool isPattern;
        private List<float> patternTimes;
        private Random random;
        private Color glitchColor;
        #endregion

        #region Constructor
        public GlitchBlock(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, false)
        {
            Initialize(
                data.Float("stability", 0.7f),
                data.Float("glitchInterval", 3f),
                data.Float("visibleTime", 2f),
                data.Float("invisibleTime", 1f),
                data.Bool("isPattern", false)
            );
        }

        public GlitchBlock(Vector2 position, int width, int height, float stability = 0.7f,
            float glitchInterval = 3f, float visibleTime = 2f, float invisibleTime = 1f, bool isPattern = false)
            : base(position, width, height, false)
        {
            Initialize(stability, glitchInterval, visibleTime, invisibleTime, isPattern);
        }

        private void Initialize(float stability, float glitchInterval, float visibleTime, float invisibleTime, bool isPattern)
        {
            Stability = stability;
            GlitchInterval = glitchInterval;
            VisibleTime = visibleTime;
            InvisibleTime = invisibleTime;
            this.isPattern = isPattern;
            
            State = GlitchState.Stable;
            stateTimer = 0f;
            glitchTimer = 0f;
            patternIndex = 0;
            random = new Random();
            glitchParticles = new List<GlitchParticle>();
            patternTimes = new List<float>();
            glitchColor = Color.Cyan;
            
            Add(sprite = GFX.SpriteBank.Create("glitch_block"));
            sprite.Play("stable");
        }
        #endregion

        #region Public Methods
        public void ForceGlitch()
        {
            if (State == GlitchState.Invisible)
            {
                StartAppearing();
            }
            else
            {
                StartDisappearing();
            }
        }

        public void SetVisible(bool visible)
        {
            if (visible && State == GlitchState.Invisible)
            {
                StartAppearing();
            }
            else if (!visible && IsVisible)
            {
                StartDisappearing();
            }
        }

        public void Corrupt()
        {
            State = GlitchState.Corrupted;
            sprite.Play("corrupted");
            glitchColor = Color.Red;
            
            level?.Shake(0.3f);
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region Private Methods
        private void StartAppearing()
        {
            State = GlitchState.Appearing;
            sprite.Play("appearing");
            
            Audio.Play("event:/game/general/diamond_get", Position);
            
            Add(new Coroutine(AppearRoutine()));
        }

        private void StartDisappearing()
        {
            State = GlitchState.Disappearing;
            sprite.Play("disappearing");
            
            // Create glitch particles
            for (int i = 0; i < 10; i++)
            {
                CreateGlitchParticle();
            }
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            Add(new Coroutine(DisappearRoutine()));
        }

        private IEnumerator AppearRoutine()
        {
            // Flicker effect while appearing
            for (int i = 0; i < 5; i++)
            {
                sprite.Visible = true;
                yield return 0.05f;
                sprite.Visible = false;
                yield return 0.05f;
            }
            
            sprite.Visible = true;
            State = GlitchState.Stable;
            sprite.Play("stable");
            stateTimer = VisibleTime;
        }

        private IEnumerator DisappearRoutine()
        {
            // Flicker effect while disappearing
            for (int i = 0; i < 5; i++)
            {
                sprite.Visible = !sprite.Visible;
                yield return 0.03f;
            }
            
            sprite.Visible = false;
            State = GlitchState.Invisible;
            stateTimer = InvisibleTime;
        }

        private void CreateGlitchParticle()
        {
            var particle = new GlitchParticle(
                Position + new Vector2(random.NextFloat(Width), random.NextFloat(Height)),
                new Vector2(random.NextFloat(100f) - 50f, random.NextFloat(100f) - 50f),
                glitchColor
            );
            glitchParticles.Add(particle);
            Scene.Add(particle);
        }

        private void ApplyGlitchEffect()
        {
            // Random visual distortion
            if (random.NextDouble() > Stability)
            {
                sprite.Color = Color.Red;
                sprite.Scale = new Vector2(1f + random.NextFloat() * 0.2f - 0.1f, 1f + random.NextFloat() * 0.2f - 0.1f);
            }
            else
            {
                sprite.Color = Color.White;
                sprite.Scale = Vector2.One;
            }
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
            
            switch (State)
            {
                case GlitchState.Stable:
                    stateTimer -= Engine.DeltaTime;
                    
                    // Random glitch effect
                    if (Scene.OnInterval(0.1f))
                    {
                        ApplyGlitchEffect();
                    }
                    
                    // Create occasional particles
                    if (Scene.OnInterval(0.3f))
                    {
                        CreateGlitchParticle();
                    }
                    
                    if (stateTimer <= 0f)
                    {
                        StartDisappearing();
                    }
                    break;
                    
                case GlitchState.Flickering:
                    sprite.Visible = !sprite.Visible;
                    break;
                    
                case GlitchState.Invisible:
                    stateTimer -= Engine.DeltaTime;
                    
                    if (stateTimer <= 0f)
                    {
                        StartAppearing();
                    }
                    break;
                    
                case GlitchState.Corrupted:
                    // Constant glitch effect
                    ApplyGlitchEffect();
                    if (Scene.OnInterval(0.05f))
                    {
                        CreateGlitchParticle();
                    }
                    break;
            }
            
            glitchParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            if (IsVisible || State == GlitchState.Appearing)
            {
                // Draw glitch effect overlay
                if (State == GlitchState.Flickering || State == GlitchState.Corrupted)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 offset = new Vector2(random.NextFloat() * 8f - 4f, random.NextFloat() * 8f - 4f);
                        var bounds = Collider.Bounds;
                        var offsetRect = new Rectangle(bounds.X + (int)offset.X, bounds.Y + (int)offset.Y, bounds.Width, bounds.Height);
                        Draw.Rect(offsetRect, glitchColor * 0.3f);
                    }
                }
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// GlitchParticle - Digital glitch particle effect
    /// </summary>
    public class GlitchParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;
        private float scale;

        public GlitchParticle(Vector2 position, Vector2 velocity, Color color)
            : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            maxLifetime = Calc.Random.NextFloat() * (0.5f - 0.2f) + 0.2f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1.5f - 0.5f) + 0.5f;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity *= 0.95f;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Rect(Position - new Vector2(4 * scale, 2 * scale), 8 * scale, 4 * scale, color * (alpha * 0.7f));
        }
    }

    /// <summary>
    /// GlitchBlockController - Synchronizes multiple glitch blocks
    /// </summary>
    [CustomEntity("DZ/GlitchBlockController")]
    public class GlitchBlockController : Entity
    {
        private List<GlitchBlock> blocks;
        private float syncInterval;
        private float timer;

        public GlitchBlockController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            blocks = new List<GlitchBlock>();
            syncInterval = data.Float("syncInterval", 2f);
            timer = 0f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            foreach (var block in scene.Tracker.GetEntities<GlitchBlock>())
            {
                blocks.Add((GlitchBlock)block);
            }
        }

        public override void Update()
        {
            base.Update();
            
            timer += Engine.DeltaTime;
            
            if (timer >= syncInterval)
            {
                timer = 0f;
                
                // Toggle all blocks
                foreach (var block in blocks)
                {
                    block.ForceGlitch();
                }
            }
        }
    }
}
