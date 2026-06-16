namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// MemoryFragment - Collectible data fragment that reveals story
    /// Floating crystal containing memories/lore
    /// Sprite path: objects/memory_fragment/
    /// </summary>
    [CustomEntity("DZ/MemoryFragment")]
    [Tracked]
    public class MemoryFragment : Actor
    {
        #region Enums
        public enum FragmentState
        {
            Hidden,
            Appearing,
            Floating,
            Collecting,
            Collected
        }
        #endregion

        #region Properties
        public FragmentState State { get; private set; }
        public string FragmentId { get; private set; }
        public string DialogueKey { get; private set; }
        public int FragmentNumber { get; private set; }
        public bool IsCollected => State == FragmentState.Collected;
        
        private Sprite sprite;
        private VertexLight fragmentLight;
        private float floatTimer;
        private float rotateTimer;
        private Level level;
        private List<MemoryParticle> particles;
        private bool showDialogue;
        private Color fragmentColor;
        #endregion

        #region Constructor
        public MemoryFragment(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Attr("fragmentId", ""),
                data.Attr("dialogueKey", "MEMORY_FRAGMENT"),
                data.Int("fragmentNumber", 1),
                data.Bool("showDialogue", true)
            );
        }

        public MemoryFragment(Vector2 position, string fragmentId = "", string dialogueKey = "MEMORY_FRAGMENT",
            int fragmentNumber = 1, bool showDialogue = true)
            : base(position)
        {
            Initialize(fragmentId, dialogueKey, fragmentNumber, showDialogue);
        }

        private void Initialize(string fragmentId, string dialogueKey, int fragmentNumber, bool showDialogue)
        {
            FragmentId = fragmentId;
            DialogueKey = dialogueKey;
            FragmentNumber = fragmentNumber;
            this.showDialogue = showDialogue;
            
            State = FragmentState.Floating;
            floatTimer = 0f;
            rotateTimer = 0f;
            particles = new List<MemoryParticle>();
            
            // Color based on fragment number
            fragmentColor = GetFragmentColor(fragmentNumber);
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            
            Add(sprite = GFX.SpriteBank.Create("memory_fragment"));
            sprite.Play("floating");
            
            Add(fragmentLight = new VertexLight(fragmentColor, 0.7f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void Collect()
        {
            if (IsCollected) return;
            
            State = FragmentState.Collecting;
            sprite.Play("collecting");
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            Add(new Coroutine(CollectRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator CollectRoutine()
        {
            // Collection effect
            for (int i = 0; i < 15; i++)
            {
                CreateMemoryParticle();
                yield return 0.03f;
            }
            
            level?.Flash(fragmentColor * 0.3f);
            level?.Shake(0.1f);
            
            // Show dialogue if enabled
            if (showDialogue)
            {
                yield return Textbox.Say(DialogueKey + "_" + FragmentNumber);
            }
            
            // Set collected flag
            level?.Session.SetFlag("memory_fragment_" + FragmentId + "_collected", true);
            // Count tracking would need a different approach
            
            State = FragmentState.Collected;
            
            yield return 0.2f;
            RemoveSelf();
        }

        private void CreateMemoryParticle()
        {
            var particle = new MemoryParticle(
                Position,
                new Vector2(Calc.Random.NextFloat() * 100f - 50f, Calc.Random.NextFloat() * 100f - 50f),
                fragmentColor
            );
            particles.Add(particle);
            Scene.Add(particle);
        }

        private Color GetFragmentColor(int number)
        {
            Color[] colors = {
                Color.Cyan,
                Color.Magenta,
                Color.Yellow,
                Color.Green,
                Color.Orange,
                Color.Purple,
                Color.Blue,
                Color.Red
            };
            return colors[(number - 1) % colors.Length];
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Track collected memories
            bool hasCollected = level?.Session.GetFlag("memory_fragment_" + FragmentId + "_collected") ?? false;
            if (hasCollected)
            {
                RemoveSelf();
                return;
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (State == FragmentState.Floating)
            {
                floatTimer += Engine.DeltaTime * 2f;
                rotateTimer += Engine.DeltaTime * 1.5f;
                
                // Float up and down
                sprite.Y = (float)Math.Sin(floatTimer) * 4f;
                
                // Rotate
                sprite.Rotation = (float)Math.Sin(rotateTimer) * 0.15f;
                
                // Create ambient particles
                if (Scene.OnInterval(0.15f))
                {
                    CreateMemoryParticle();
                }
                
                // Check for collection
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    Collect();
                }
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw glow effect
            if (State == FragmentState.Floating)
            {
                Draw.Circle(Position, 16f, fragmentColor * 0.2f, 8);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// MemoryParticle - Particle for memory fragment effects
    /// </summary>
    public class MemoryParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;
        private float scale;

        public MemoryParticle(Vector2 position, Vector2 velocity, Color color)
            : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 40f * Engine.DeltaTime;
            velocity *= 0.97f;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 4f * scale, color * (alpha * 0.5f), 4);
        }
    }

    /// <summary>
    /// MemoryFragmentCounter - Tracks collected fragments
    /// </summary>
    [CustomEntity("DZ/MemoryFragmentCounter")]
    public class MemoryFragmentCounter : Entity
    {
        private int totalFragments;
        private int collectedFragments;
        private string collectionId;

        public MemoryFragmentCounter(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            totalFragments = data.Int("totalFragments", 8);
            collectionId = data.Attr("collectionId", "");
            collectedFragments = 0;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Count collected fragments
            var level = scene as Level;
            for (int i = 1; i <= totalFragments; i++)
            {
                if (level?.Session.GetFlag("memory_fragment_" + collectionId + "_" + i + "_collected") == true)
                {
                    collectedFragments++;
                }
            }
        }

        public void OnFragmentCollected()
        {
            collectedFragments++;
            
            if (collectedFragments >= totalFragments)
            {
                // All fragments collected
                var level = Scene as Level;
                level?.Session.SetFlag("memory_fragments_complete_" + collectionId, true);
                Audio.Play("event:/game/general/crystalheart_pulse", Position);
            }
        }

        public override void Render()
        {
            // Draw counter HUD
            // (Would need proper font rendering here)
        }
    }
}

