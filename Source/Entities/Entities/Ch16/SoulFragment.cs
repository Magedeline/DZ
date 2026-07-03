namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// SoulFragment - Collectible soul piece for final boss
    /// Required to access final confrontation
    /// Sprite path: objects/DZ/DZ/soul_fragment/
    /// </summary>
    [CustomEntity("DZ/SoulFragment")]
    [Tracked]
    public class SoulFragment : Actor
    {
        #region Enums
        public enum FragmentState
        {
            Hidden,
            Emerge,
            Floating
        }
        #endregion

        #region Properties
        public FragmentState State { get; private set; }
        public string FragmentId { get; private set; }
        public int RequiredSouls { get; private set; }
        public int CollectedSouls { get; private set; }
        
        private Sprite sprite;
        private VertexLight fragmentLight;
        private float floatTimer;
        private Level level;
        private List<SoulParticle> particles;
        private Color soulColor;
        #endregion

        #region Constructor
        public SoulFragment(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Attr("fragmentId", ""), data.Int("requiredSouls", 3));
        }

        public SoulFragment(Vector2 position, string fragmentId = "", int requiredSouls = 3)
            : base(position)
        {
            Initialize(fragmentId, requiredSouls);
        }

        private void Initialize(string fragmentId, int requiredSouls)
        {
            FragmentId = fragmentId;
            RequiredSouls = requiredSouls;
            CollectedSouls = 0;
            
            State = FragmentState.Hidden;
            floatTimer = 0f;
            particles = new List<SoulParticle>();
            soulColor = Color.White;
            
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            
            Add(sprite = DZModule.SpriteBank.Create("soul_fragment"));
            sprite.Play("hidden");
            sprite.Visible = false;
            
            Add(fragmentLight = new VertexLight(soulColor, 0f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void Emerge()
        {
            if (State != FragmentState.Hidden) return;
            
            State = FragmentState.Emerge;
            sprite.Visible = true;
            sprite.Play("emerge");
            fragmentLight.Alpha = 0.6f;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            Add(new Coroutine(EmergeRoutine()));
        }

        public void CollectSoul()
        {
            CollectedSouls++;
            
            if (CollectedSouls >= RequiredSouls)
            {
                Complete();
            }
        }

        public void Complete()
        {
            if (State == FragmentState.Floating) return;
            
            State = FragmentState.Floating;
            sprite.Play("complete");
            
            level?.Flash(Color.White * 0.5f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            level?.Session.SetFlag("soul_fragment_" + FragmentId + "_complete", true);
            
            Add(new Coroutine(CompleteRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator EmergeRoutine()
        {
            for (int i = 0; i < 12; i++)
            {
                CreateSoulParticle();
                yield return 0.05f;
            }
            
            State = FragmentState.Floating;
            sprite.Play("floating");
        }

        private IEnumerator CompleteRoutine()
        {
            for (int i = 0; i < 20; i++)
            {
                CreateSoulParticle();
                yield return 0.02f;
            }
            
            yield return 0.5f;
            RemoveSelf();
        }

        private void CreateSoulParticle()
        {
            var particle = new SoulParticle(
                Position,
                new Vector2(Calc.Random.NextFloat() * 80f - 40f, Calc.Random.NextFloat() * 80f - 40f),
                soulColor
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
            
            if (level?.Session.GetFlag("soul_fragment_" + FragmentId + "_complete") == true)
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
                sprite.Y = (float)Math.Sin(floatTimer) * 4f;
                
                if (Scene.OnInterval(0.1f))
                {
                    CreateSoulParticle();
                }
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            if (State == FragmentState.Floating)
            {
                Draw.Circle(Position, 12f, soulColor * 0.3f, 8);
            }
            base.Render();
        }
        #endregion
    }

    public class SoulParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public SoulParticle(Vector2 position, Vector2 velocity, Color color)
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
            velocity.Y -= 50f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 3f, color * (alpha * 0.6f), 4);
        }
    }
}

