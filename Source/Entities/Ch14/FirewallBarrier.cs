namespace Celeste.Entities.Chapters.Ch14
{
    /// <summary>
    /// FirewallBarrier - Protective barrier that blocks enemies and damage
    /// Can be toggled on/off and provides safe zones
    /// Sprite path: objects/firewall_barrier/
    /// </summary>
    [CustomEntity("MaggyHelper/FirewallBarrier")]
    [Tracked]
    public class FirewallBarrier : Solid
    {
        #region Enums
        public enum BarrierState
        {
            Inactive,
            Activating,
            Active,
            Flickering,
            Deactivating,
            Overloaded
        }
        #endregion

        #region Properties
        public BarrierState State { get; private set; }
        public float Energy { get; private set; }
        public float MaxEnergy { get; private set; }
        public float EnergyDrain { get; private set; }
        public bool IsActive => State == BarrierState.Active;
        
        private Sprite sprite;
        private VertexLight barrierLight;
        private float stateTimer;
        private Level level;
        private List<BarrierParticle> particles;
        private List<BarrierPulse> pulses;
        private bool isRecharging;
        private float pulseTimer;
        private Color barrierColor;
        #endregion

        #region Constructor
        public FirewallBarrier(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, false)
        {
            Initialize(
                data.Float("maxEnergy", 100f),
                data.Float("energyDrain", 5f),
                data.Bool("startActive", true)
            );
        }

        public FirewallBarrier(Vector2 position, int width, int height, float maxEnergy = 100f,
            float energyDrain = 5f, bool startActive = true)
            : base(position, width, height, false)
        {
            Initialize(maxEnergy, energyDrain, startActive);
        }

        private void Initialize(float maxEnergy, float energyDrain, bool startActive)
        {
            MaxEnergy = maxEnergy;
            Energy = startActive ? maxEnergy : 0f;
            EnergyDrain = energyDrain;
            
            State = startActive ? BarrierState.Active : BarrierState.Inactive;
            stateTimer = 0f;
            pulseTimer = 0f;
            isRecharging = false;
            particles = new List<BarrierParticle>();
            pulses = new List<BarrierPulse>();
            barrierColor = Color.Orange;
            
            Add(sprite = GFX.SpriteBank.Create("firewall_barrier"));
            sprite.Play(startActive ? "active" : "inactive");
            
            Add(barrierLight = new VertexLight(barrierColor, startActive ? 0.6f : 0f, 8, 24));
        }
        #endregion

        #region Public Methods
        public void Activate()
        {
            if (State == BarrierState.Active) return;
            
            State = BarrierState.Activating;
            sprite.Play("activating");
            
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            Add(new Coroutine(ActivateRoutine()));
        }

        public void Deactivate()
        {
            if (State == BarrierState.Inactive) return;
            
            State = BarrierState.Deactivating;
            sprite.Play("deactivating");
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            Add(new Coroutine(DeactivateRoutine()));
        }

        public void Toggle()
        {
            if (IsActive)
                Deactivate();
            else
                Activate();
        }

        public void Recharge(float amount)
        {
            Energy = Math.Min(MaxEnergy, Energy + amount);
            isRecharging = true;
            
            if (Energy >= MaxEnergy * 0.3f && State == BarrierState.Inactive)
            {
                Activate();
            }
        }

        public void Overload()
        {
            State = BarrierState.Overloaded;
            barrierColor = Color.Red;
            barrierLight.Color = Color.Red;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.Shake(0.3f);
            
            Add(new Coroutine(OverloadRoutine()));
        }
        #endregion

        #region Private Methods
        private IEnumerator ActivateRoutine()
        {
            // Activation animation
            for (int i = 0; i < 10; i++)
            {
                CreateBarrierParticle();
                barrierLight.Alpha = i / 10f * 0.6f;
                yield return 0.05f;
            }
            
            State = BarrierState.Active;
            sprite.Play("active");
            barrierLight.Alpha = 0.6f;
        }

        private IEnumerator DeactivateRoutine()
        {
            // Deactivation animation
            for (int i = 10; i >= 0; i--)
            {
                CreateBarrierParticle();
                barrierLight.Alpha = i / 10f * 0.6f;
                yield return 0.03f;
            }
            
            State = BarrierState.Inactive;
            sprite.Play("inactive");
            barrierLight.Alpha = 0f;
        }

        private IEnumerator OverloadRoutine()
        {
            // Flicker and fail
            for (int i = 0; i < 20; i++)
            {
                sprite.Visible = !sprite.Visible;
                CreateBarrierParticle();
                yield return 0.05f;
            }
            
            Energy = 0f;
            State = BarrierState.Inactive;
            sprite.Play("inactive");
            barrierLight.Alpha = 0f;
            barrierColor = Color.Orange;
            barrierLight.Color = Color.Orange;
        }

        private void CreateBarrierParticle()
        {
            var particle = new BarrierParticle(
                Position + new Vector2(Calc.Random.NextFloat() * Width - Width / 2, Calc.Random.NextFloat() * Height - Height / 2),
                new Vector2(Calc.Random.NextFloat() * 30f - 15f, -Calc.Random.NextFloat() * 40f),
                barrierColor
            );
            particles.Add(particle);
            Scene.Add(particle);
        }

        private void CreateBarrierPulse()
        {
            var pulse = new BarrierPulse(Position, Width, Height, barrierColor);
            pulses.Add(pulse);
            Scene.Add(pulse);
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
            
            if (State == BarrierState.Active)
            {
                // Drain energy
                Energy -= EnergyDrain * Engine.DeltaTime;
                
                // Create particles
                if (Scene.OnInterval(0.1f))
                {
                    CreateBarrierParticle();
                }
                
                // Create pulses
                pulseTimer += Engine.DeltaTime;
                if (pulseTimer >= 0.5f)
                {
                    pulseTimer = 0f;
                    CreateBarrierPulse();
                }
                
                // Check for low energy
                if (Energy <= 0f)
                {
                    Overload();
                }
                else if (Energy < MaxEnergy * 0.2f)
                {
                    State = BarrierState.Flickering;
                    sprite.Play("flickering");
                }
            }
            
            if (State == BarrierState.Flickering)
            {
                sprite.Visible = Calc.Random.NextFloat() > 0.3f;
                barrierLight.Alpha = Calc.Random.NextFloat() * 0.3f;
                
                if (isRecharging && Energy >= MaxEnergy * 0.3f)
                {
                    State = BarrierState.Active;
                    sprite.Play("active");
                    sprite.Visible = true;
                    isRecharging = false;
                }
            }
            
            particles.RemoveAll(p => p == null || p.Scene == null);
            pulses.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            if (IsActive || State == BarrierState.Activating)
            {
                // Draw barrier field
                Draw.Rect(Collider.Bounds, barrierColor * 0.3f);
                
                // Draw energy level
                float energyPercent = Energy / MaxEnergy;
                Draw.Rect(Left, Bottom + 2, Width * energyPercent, 4, barrierColor * 0.6f);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// BarrierParticle - Particle for barrier effects
    /// </summary>
    public class BarrierParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public BarrierParticle(Vector2 position, Vector2 velocity, Color color)
            : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            maxLifetime = Calc.Random.NextFloat() * (0.6f - 0.3f) + 0.3f;
            lifetime = maxLifetime;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 30f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 3f, color * (alpha * 0.6f), 3);
        }
    }

    /// <summary>
    /// BarrierPulse - Expanding pulse effect
    /// </summary>
    public class BarrierPulse : Actor
    {
        private float width;
        private float height;
        private Color color;
        private float lifetime;
        private float maxLifetime;

        public BarrierPulse(Vector2 position, float width, float height, Color color)
            : base(position)
        {
            this.width = width;
            this.height = height;
            this.color = color;
            maxLifetime = 0.3f;
            lifetime = maxLifetime;
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
            float scale = 1f + (1f - alpha) * 0.5f;
            Draw.Rect(Position, width * scale, height * scale, color * (alpha * 0.2f));
        }
    }

    /// <summary>
    /// FirewallSwitch - Switch to control firewall barriers
    /// </summary>
    [CustomEntity("MaggyHelper/FirewallSwitch")]
    public class FirewallSwitch : Actor
    {
        private Sprite sprite;
        private FirewallBarrier barrier;
        private bool isPressed;

        public FirewallSwitch(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            isPressed = false;
            
            Collider = new Hitbox(16f, 16f, -8f, -16f);
            Add(sprite = GFX.SpriteBank.Create("firewall_switch"));
            sprite.Play("unpressed");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Find nearby barrier
            foreach (var b in scene.Tracker.GetEntities<FirewallBarrier>())
            {
                if (Vector2.Distance(Position, b.Position) < 100f)
                {
                    barrier = (FirewallBarrier)b;
                    break;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                if (!isPressed && Input.Grab.Pressed)
                {
                    Press();
                }
            }
            else if (isPressed)
            {
                isPressed = false;
                sprite.Play("unpressed");
            }
        }

        private void Press()
        {
            isPressed = true;
            sprite.Play("pressed");
            Audio.Play("event:/game/general/diamond_get", Position);
            
            barrier?.Toggle();
        }
    }
}
