namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// VoidPortal - Portal to the void dimension
    /// Final chapter transportation and boss arena entrance
    /// Sprite path: objects/DZ/DZ/void_portal/
    /// </summary>
    [CustomEntity("DZ/VoidPortal")]
    [Tracked]
    public class VoidPortal : Actor
    {
        #region Enums
        public enum PortalState
        {
            Inactive,
            Opening,
            Active
        }
        #endregion

        #region Properties
        public PortalState State { get; private set; }
        public string DestinationId { get; private set; }
        public bool IsFinalPortal { get; private set; }
        
        private Sprite sprite;
        private VertexLight portalLight;
        private float rotation;
        private Level level;
        private List<VoidParticle> particles;
        private float pulseTimer;
        private Color voidColor;
        #endregion

        #region Constructor
        public VoidPortal(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Attr("destinationId", ""), data.Bool("isFinalPortal", false));
        }

        public VoidPortal(Vector2 position, string destinationId = "", bool isFinalPortal = false)
            : base(position)
        {
            Initialize(destinationId, isFinalPortal);
        }

        private void Initialize(string destinationId, bool isFinalPortal)
        {
            DestinationId = destinationId;
            IsFinalPortal = isFinalPortal;
            
            State = PortalState.Inactive;
            rotation = 0f;
            pulseTimer = 0f;
            particles = new List<VoidParticle>();
            voidColor = Color.Purple;
            
            Collider = new Hitbox(48f, 64f, -24f, -64f);
            
            Add(sprite = GFX.SpriteBank.Create("void_portal"));
            sprite.Play("inactive");
            
            Add(portalLight = new VertexLight(voidColor, 0.3f, 16, 48));
        }
        #endregion

        #region Public Methods
        public void Open()
        {
            if (State != PortalState.Inactive) return;
            
            State = PortalState.Opening;
            sprite.Play("opening");
            portalLight.Alpha = 0.6f;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            level?.Shake(0.4f);
            
            Add(new Coroutine(OpenRoutine()));
        }

        public void Transport(Player player)
        {
            if (State != PortalState.Active) return;
            
            Add(new Coroutine(TransportRoutine(player)));
        }
        #endregion

        #region Private Methods
        private IEnumerator OpenRoutine()
        {
            for (int i = 0; i < 20; i++)
            {
                CreateVoidParticle();
                yield return 0.05f;
            }
            
            State = PortalState.Active;
            sprite.Play("active");
            portalLight.Alpha = 0.8f;
        }

        private IEnumerator TransportRoutine(Player player)
        {
            player.StateMachine.State = Player.StDummy;
            player.StateMachine.Locked = true;
            player.Visible = false;
            
            // Transport effect
            for (int i = 0; i < 15; i++)
            {
                CreateVoidParticle();
                yield return 0.03f;
            }
            
            level?.Flash(voidColor * 0.5f);
            Audio.Play("event:/game/char_maddy/dash", Position);
            
            yield return 0.5f;
            
            // Set flag
            level?.Session.SetFlag("void_portal_entered", true);
            
            if (IsFinalPortal)
            {
                level?.Session.SetFlag("final_portal_entered", true);
            }
            
            RemoveSelf();
        }

        private void CreateVoidParticle()
        {
            float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
            var particle = new VoidParticle(
                Position + Calc.AngleToVector(angle, Calc.Random.NextFloat() * 50f + 50f),
                Calc.AngleToVector(angle + MathHelper.Pi, Calc.Random.NextFloat() * 50f + 50f),
                voidColor
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
        }

        public override void Update()
        {
            base.Update();
            
            if (State == PortalState.Active)
            {
                rotation += Engine.DeltaTime * 2f;
                pulseTimer += Engine.DeltaTime * 3f;
                
                float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.1f;
                sprite.Scale = Vector2.One * pulse;
                
                if (Scene.OnInterval(0.1f))
                {
                    CreateVoidParticle();
                }
                
                // Check for player entering
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    Transport(player);
                }
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            if (State == PortalState.Active)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = rotation + i * MathHelper.TwoPi / 3f;
                    Vector2 ringPos = Position + Calc.AngleToVector(angle, 24f);
                    Draw.Circle(ringPos, 12f, voidColor * 0.3f, 8);
                }
            }
            base.Render();
        }
        #endregion
    }

    public class VoidParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public VoidParticle(Vector2 position, Vector2 velocity, Color color)
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
            velocity *= 0.95f;
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

