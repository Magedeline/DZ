namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// DigitalPortal - Teleportation portal between digital zones
    /// Creates visual distortion and transports player
    /// Sprite path: objects/DZ/DZ/digital_portal/
    /// </summary>
    [CustomEntity("DZ/DigitalPortal")]
    [Tracked]
    public class DigitalPortal : Actor
    {
        #region Enums
        public enum PortalState
        {
            Inactive,
            Active,
            Transporting,
            Arriving,
            Unstable
        }
        #endregion

        #region Properties
        public PortalState State { get; private set; }
        public string DestinationId { get; private set; }
        public float TransportDelay { get; private set; }
        public bool IsActive => State == PortalState.Active;
        
        private Sprite sprite;
        private DigitalPortal destination;
        private Player transportingPlayer;
        private Level level;
        private VertexLight portalLight;
        private List<PortalParticle> particles;
        private float rotation;
        private float pulseTimer;
        private bool isTwoWay;
        #endregion

        #region Constructor
        public DigitalPortal(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Attr("destinationId", ""),
                data.Float("transportDelay", 0.5f),
                data.Bool("isTwoWay", true)
            );
        }

        public DigitalPortal(Vector2 position, string destinationId = "", float transportDelay = 0.5f, bool isTwoWay = true)
            : base(position)
        {
            Initialize(destinationId, transportDelay, isTwoWay);
        }

        private void Initialize(string destinationId, float transportDelay, bool isTwoWay)
        {
            DestinationId = destinationId;
            TransportDelay = transportDelay;
            this.isTwoWay = isTwoWay;
            
            State = PortalState.Inactive;
            rotation = 0f;
            pulseTimer = 0f;
            particles = new List<PortalParticle>();
            
            Collider = new Hitbox(32f, 48f, -16f, -48f);
            
            Add(sprite = GFX.SpriteBank.Create("digital_portal"));
            sprite.Play("inactive");
            
            Add(portalLight = new VertexLight(Color.Cyan, 0.2f, 12, 32));
        }
        #endregion

        #region Public Methods
        public void Activate()
        {
            State = PortalState.Active;
            sprite.Play("active");
            portalLight.Alpha = 0.6f;
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
        }

        public void Deactivate()
        {
            State = PortalState.Inactive;
            sprite.Play("inactive");
            portalLight.Alpha = 0.2f;
        }

        public void SetUnstable()
        {
            State = PortalState.Unstable;
            sprite.Play("unstable");
            portalLight.Color = Color.Red;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }

        public void SetDestination(DigitalPortal dest)
        {
            destination = dest;
        }

        public void Transport(Player player)
        {
            if (State != PortalState.Active || destination == null) return;
            
            transportingPlayer = player;
            State = PortalState.Transporting;
            
            Add(new Coroutine(TransportRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator TransportRoutine()
        {
            // Lock player
            transportingPlayer.StateMachine.State = Player.StDummy;
            transportingPlayer.StateMachine.Locked = true;
            
            // Transport effect
            for (int i = 0; i < 15; i++)
            {
                CreatePortalParticle();
                yield return 0.03f;
            }
            
            // Screen flash
            level?.Flash(Color.Cyan * 0.5f);
            Audio.Play("event:/game/char_maddy/dash", Position);
            
            yield return TransportDelay;
            
            // Teleport
            if (destination != null)
            {
                destination.ReceivePlayer(transportingPlayer);
            }
            
            // Reset this portal
            State = PortalState.Active;
        }

        public void ReceivePlayer(Player player)
        {
            State = PortalState.Arriving;
            transportingPlayer = player;
            
            // Place player at portal
            player.Position = Position;
            player.Visible = false;
            
            Add(new Coroutine(ArriveRoutine()));
        }

        private IEnumerator ArriveRoutine()
        {
            // Arrival effect
            for (int i = 0; i < 15; i++)
            {
                CreatePortalParticle();
                yield return 0.03f;
            }
            
            level?.Flash(Color.Cyan * 0.3f);
            
            yield return 0.2f;
            
            // Show player
            transportingPlayer.Visible = true;
            transportingPlayer.StateMachine.Locked = false;
            transportingPlayer.StateMachine.State = Player.StNormal;
            
            State = PortalState.Active;
        }

        private void CreatePortalParticle()
        {
            float angle = Calc.Random.NextFloat() * MathHelper.TwoPi;
            var particle = new PortalParticle(
                Position + Calc.AngleToVector(angle, Calc.Random.NextFloat() * 50f + 50f),
                Calc.AngleToVector(angle + MathHelper.Pi, Calc.Random.NextFloat() * 50f + 50f)
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
            
            // Find destination portal
            if (!string.IsNullOrEmpty(DestinationId))
            {
                foreach (var portal in scene.Tracker.GetEntities<DigitalPortal>())
                {
                    if (portal != this)
                    {
                        destination = (DigitalPortal)portal;
                        if (isTwoWay)
                        {
                            ((DigitalPortal)portal).SetDestination(this);
                        }
                        break;
                    }
                }
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (State == PortalState.Active || State == PortalState.Unstable)
            {
                rotation += Engine.DeltaTime * 2f;
                pulseTimer += Engine.DeltaTime * 3f;
                
                float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.1f;
                sprite.Scale = Vector2.One * pulse;
                
                // Create ambient particles
                if (Scene.OnInterval(0.1f))
                {
                    CreatePortalParticle();
                }
                
                // Check for player entering
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                {
                    Transport(player);
                }
            }
            
            if (State == PortalState.Unstable)
            {
                // Random flicker
                if (Calc.Random.NextFloat() < 0.1f)
                {
                    sprite.Visible = !sprite.Visible;
                }
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw portal swirl
            if (State == PortalState.Active || State == PortalState.Unstable)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = rotation + i * MathHelper.TwoPi / 3f;
                    Vector2 ringPos = Position + Calc.AngleToVector(angle, 30f);
                    Draw.Circle(ringPos, 8f, portalLight.Color * 0.5f, 8);
                }
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// PortalParticle - Particle for portal effects
    /// </summary>
    public class PortalParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public PortalParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.6f - 0.3f) + 0.3f;
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
            Draw.Circle(Position, 4f, Color.Cyan * (alpha * 0.6f), 4);
        }
    }
}

