namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// CoreFragment - Central processing core fragment, boss-related collectible
    /// Key item for progressing through digital dimension
    /// Sprite path: objects/core_fragment/
    /// </summary>
    [CustomEntity("MaggyHelper/CoreFragment")]
    [Tracked]
    public class CoreFragment : Actor
    {
        #region Enums
        public enum FragmentState
        {
            Inactive,
            Protected,
            Active,
            Collecting,
            Collected
        }
        #endregion

        #region Properties
        public FragmentState State { get; private set; }
        public string FragmentId { get; private set; }
        public int FragmentIndex { get; private set; }
        public bool RequiresProtection { get; private set; }
        public bool IsCollected => State == FragmentState.Collected;
        
        private Sprite sprite;
        private VertexLight coreLight;
        private List<CoreShield> shields;
        private float floatTimer;
        private float rotateTimer;
        private float pulseTimer;
        private Level level;
        private List<CoreParticle> particles;
        private bool canCollect;
        private Color coreColor;
        private int requiredShields;
        #endregion

        #region Constructor
        public CoreFragment(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Attr("fragmentId", ""),
                data.Int("fragmentIndex", 1),
                data.Bool("requiresProtection", true),
                data.Int("requiredShields", 3)
            );
        }

        public CoreFragment(Vector2 position, string fragmentId = "", int fragmentIndex = 1,
            bool requiresProtection = true, int requiredShields = 3)
            : base(position)
        {
            Initialize(fragmentId, fragmentIndex, requiresProtection, requiredShields);
        }

        private void Initialize(string fragmentId, int fragmentIndex, bool requiresProtection, int requiredShields)
        {
            FragmentId = fragmentId;
            FragmentIndex = fragmentIndex;
            RequiresProtection = requiresProtection;
            this.requiredShields = requiredShields;
            
            State = requiresProtection ? FragmentState.Protected : FragmentState.Active;
            floatTimer = 0f;
            rotateTimer = 0f;
            pulseTimer = 0f;
            canCollect = !requiresProtection;
            shields = new List<CoreShield>();
            particles = new List<CoreParticle>();
            
            coreColor = GetCoreColor(fragmentIndex);
            
            Collider = new Hitbox(24f, 24f, -12f, -12f);
            
            Add(sprite = GFX.SpriteBank.Create("core_fragment"));
            sprite.Play(requiresProtection ? "protected" : "active");
            
            Add(coreLight = new VertexLight(coreColor, requiresProtection ? 0.3f : 0.8f, 12, 32));
        }
        #endregion

        #region Public Methods
        public void Activate()
        {
            if (State != FragmentState.Protected) return;
            
            State = FragmentState.Active;
            canCollect = true;
            sprite.Play("active");
            coreLight.Alpha = 0.8f;
            
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
            level?.Flash(coreColor * 0.3f);
            
            // Remove shields
            foreach (var shield in shields)
            {
                shield.Destroy();
            }
            shields.Clear();
        }

        public void Collect()
        {
            if (!canCollect || IsCollected) return;
            
            State = FragmentState.Collecting;
            sprite.Play("collecting");
            
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
            
            Add(new Coroutine(CollectRoutine()));
        }

        public void AddShield(CoreShield shield)
        {
            shields.Add(shield);
        }

        public void OnShieldDestroyed()
        {
            if (shields.Count == 0 && State == FragmentState.Protected)
            {
                Activate();
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator CollectRoutine()
        {
            // Collection effect
            for (int i = 0; i < 25; i++)
            {
                CreateCoreParticle();
                yield return 0.02f;
            }
            
            level?.Flash(coreColor * 0.5f);
            level?.Shake(0.3f);
            
            // Show collection dialogue
            yield return Textbox.Say("CORE_FRAGMENT_" + FragmentIndex);
            
            // Set collected flag
            level?.Session.SetFlag("core_fragment_" + FragmentId + "_collected", true);
            // Count tracking would need a different approach
            
            State = FragmentState.Collected;
            
            yield return 0.3f;
            RemoveSelf();
        }

        private void CreateCoreParticle()
        {
            var particle = new CoreParticle(
                Position,
                new Vector2(Calc.Random.NextFloat() * 150f - 75f, Calc.Random.NextFloat() * 150f - 75f),
                coreColor
            );
            particles.Add(particle);
            Scene.Add(particle);
        }

        private Color GetCoreColor(int index)
        {
            Color[] colors = {
                Color.Cyan,
                Color.Magenta,
                Color.Yellow,
                Color.Green,
                Color.Orange
            };
            return colors[(index - 1) % colors.Length];
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Track collected fragments
            bool hasCollected = level?.Session.GetFlag("core_fragment_" + FragmentId + "_collected") ?? false;
            if (hasCollected)
            {
                RemoveSelf();
                return;
            }
            
            // Create protective shields if needed
            if (RequiresProtection)
            {
                CreateShields();
            }
        }

        private void CreateShields()
        {
            for (int i = 0; i < requiredShields; i++)
            {
                float angle = (MathHelper.TwoPi / requiredShields) * i;
                Vector2 shieldPos = Position + Calc.AngleToVector(angle, 60f);
                
                var shield = new CoreShield(shieldPos, this, angle);
                shields.Add(shield);
                Scene.Add(shield);
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (State == FragmentState.Active || State == FragmentState.Protected)
            {
                floatTimer += Engine.DeltaTime * 2f;
                rotateTimer += Engine.DeltaTime * 1.5f;
                pulseTimer += Engine.DeltaTime * 3f;
                
                // Float effect
                sprite.Y = (float)Math.Sin(floatTimer) * 4f;
                
                // Rotate
                sprite.Rotation = (float)Math.Sin(rotateTimer) * 0.1f;
                
                // Pulse
                float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.1f;
                sprite.Scale = Vector2.One * pulse;
                
                // Create particles
                if (Scene.OnInterval(0.1f))
                {
                    CreateCoreParticle();
                }
                
                // Check collection
                if (canCollect)
                {
                    var player = Scene.Tracker.GetEntity<Player>();
                    if (player != null && Collide.Check(this, player))
                    {
                        Collect();
                    }
                }
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw core glow
            if (State == FragmentState.Active)
            {
                Draw.Circle(Position, 24f, coreColor * 0.2f, 12);
            }
            
            // Draw protection indicator
            if (State == FragmentState.Protected)
            {
                Draw.Circle(Position, 50f, Color.Red * 0.1f, 16);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// CoreShield - Protective shield around core fragment
    /// </summary>
    public class CoreShield : Actor
    {
        private CoreFragment core;
        private float baseAngle;
        private float orbitAngle;
        private float orbitSpeed;
        private float orbitRadius;
        private Sprite sprite;
        private int health;
        private VertexLight shieldLight;

        public CoreShield(Vector2 position, CoreFragment core, float angle)
            : base(position)
        {
            this.core = core;
            baseAngle = angle;
            orbitAngle = angle;
            orbitSpeed = 1f;
            orbitRadius = 60f;
            health = 1;
            
            Collider = new Hitbox(20f, 20f, -10f, -10f);
            
            Add(sprite = GFX.SpriteBank.Create("core_shield"));
            sprite.Play("active");
            
            Add(shieldLight = new VertexLight(Color.Red, 0.5f, 8, 16));
        }

        public void Destroy()
        {
            // Destruction effect
            for (int i = 0; i < 10; i++)
            {
                var particle = new CoreParticle(
                    Position,
                    new Vector2(Calc.Random.NextFloat() * 80f - 40f, Calc.Random.NextFloat() * 80f - 40f),
                    Color.Red
                );
                Scene.Add(particle);
            }
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            RemoveSelf();
        }

        public void TakeDamage(int damage)
        {
            health -= damage;
            
            if (health <= 0)
            {
                core.OnShieldDestroyed();
                Destroy();
            }
        }

        public override void Update()
        {
            base.Update();
            
            // Orbit around core
            orbitAngle += orbitSpeed * Engine.DeltaTime;
            Position = core.Position + Calc.AngleToVector(orbitAngle, orbitRadius);
        }

        public override void Render()
        {
            // Draw shield connection to core
            Draw.Line(Position, core.Position, Color.Red * 0.3f, 1f);
            
            base.Render();
        }
    }

    /// <summary>
    /// CoreParticle - Particle for core effects
    /// </summary>
    public class CoreParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;
        private float scale;

        public CoreParticle(Vector2 position, Vector2 velocity, Color color)
            : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1.5f - 0.5f) + 0.5f;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 60f * Engine.DeltaTime;
            velocity *= 0.97f;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 5f * scale, color * (alpha * 0.6f), 5);
        }
    }
}

