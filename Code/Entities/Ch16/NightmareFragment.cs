namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// NightmareFragment - Collectible nightmare memory fragment
    /// Reveals story and unlocks final boss
    /// Sprite path: objects/nightmare_fragment/
    /// </summary>
    [CustomEntity("MaggyHelper/NightmareFragment")]
    [Tracked]
    public class NightmareFragment : Actor
    {
        #region Enums
        public enum FragmentState
        {
            Hidden,
            Appearing,
            Floating
        }
        #endregion

        #region Properties
        public FragmentState State { get; private set; }
        public string FragmentId { get; private set; }
        public int FragmentNumber { get; private set; }
        public bool IsCollected => State == FragmentState.Floating;
        
        private Sprite sprite;
        private VertexLight fragmentLight;
        private float floatTimer;
        private float rotateTimer;
        private Level level;
        private List<NightmareParticle> particles;
        private Color fragmentColor;
        #endregion

        #region Constructor
        public NightmareFragment(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Attr("fragmentId", ""), data.Int("fragmentNumber", 1));
        }

        public NightmareFragment(Vector2 position, string fragmentId = "", int fragmentNumber = 1)
            : base(position)
        {
            Initialize(fragmentId, fragmentNumber);
        }

        private void Initialize(string fragmentId, int fragmentNumber)
        {
            FragmentId = fragmentId;
            FragmentNumber = fragmentNumber;
            
            State = FragmentState.Hidden;
            floatTimer = 0f;
            rotateTimer = 0f;
            particles = new List<NightmareParticle>();
            fragmentColor = Color.Purple;
            
            Collider = new Hitbox(20f, 20f, -10f, -10f);
            
            Add(sprite = GFX.SpriteBank.Create("nightmare_fragment"));
            sprite.Play("hidden");
            sprite.Visible = false;
            
            Add(fragmentLight = new VertexLight(fragmentColor, 0f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void Reveal()
        {
            if (State != FragmentState.Hidden) return;
            
            State = FragmentState.Appearing;
            sprite.Visible = true;
            sprite.Play("appearing");
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            Add(new Coroutine(RevealRoutine()));
        }

        public void Collect()
        {
            if (IsCollected) return;
            
            State = FragmentState.Appearing;
            sprite.Play("collecting");
            
            Add(new Coroutine(CollectRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator RevealRoutine()
        {
            for (int i = 0; i < 15; i++)
            {
                CreateNightmareParticle();
                fragmentLight.Alpha = i / 15f * 0.8f;
                yield return 0.03f;
            }
            
            State = FragmentState.Floating;
            sprite.Play("floating");
        }

        private IEnumerator CollectRoutine()
        {
            for (int i = 0; i < 20; i++)
            {
                CreateNightmareParticle();
                yield return 0.02f;
            }
            
            level?.Flash(fragmentColor * 0.4f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            yield return Textbox.Say("NIGHTMARE_FRAGMENT_" + FragmentNumber);
            
            level?.Session.SetFlag("nightmare_fragment_" + FragmentId + "_collected", true);
            bool hasCount = level?.Session.GetFlag("nightmare_fragment_count") ?? false;
            level?.Session.SetFlag("nightmare_fragment_count", true);
            
            State = FragmentState.Floating;
            yield return 0.3f;
            RemoveSelf();
        }

        private void CreateNightmareParticle()
        {
            var particle = new NightmareParticle(
                Position,
                new Vector2(Calc.Random.NextFloat() * 100f - 50f, Calc.Random.NextFloat() * 100f - 50f),
                fragmentColor
            );
            particles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            if (level?.Session.GetFlag("nightmare_fragment_" + FragmentId + "_collected") == true)
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (State == FragmentState.Floating)
            {
                floatTimer += Engine.DeltaTime * 2f;
                rotateTimer += Engine.DeltaTime * 1.5f;
                
                sprite.Y = (float)Math.Sin(floatTimer) * 4f;
                sprite.Rotation = (float)Math.Sin(rotateTimer) * 0.15f;
                
                if (Scene.OnInterval(0.15f))
                {
                    CreateNightmareParticle();
                }
                
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
            if (State == FragmentState.Floating)
            {
                Draw.Circle(Position, 16f, fragmentColor * 0.2f, 8);
            }
            base.Render();
        }
        #endregion
    }

    public class NightmareParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public NightmareParticle(Vector2 position, Vector2 velocity, Color color)
            : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
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
            Draw.Circle(Position, 4f, color * (alpha * 0.5f), 4);
        }
    }
}
