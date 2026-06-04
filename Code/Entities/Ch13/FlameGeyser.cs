namespace Celeste.Entities.Chapters.Ch13
{
    /// <summary>
    /// FlameGeyser - Erupts flames in timed intervals
    /// Creates vertical fire hazards with warning periods
    /// Sprite path: objects/flame_geyser/
    /// </summary>
    [CustomEntity("MaggyHelper/FlameGeyser")]
    [Tracked]
    public class FlameGeyser : Actor
    {
        #region Enums
        public enum GeyserState
        {
            Idle,
            Warning,
            Erupting,
            Cooling,
            Overheated
        }
        #endregion

        #region Properties
        public GeyserState State { get; private set; }
        public float EruptInterval { get; private set; }
        public float EruptDuration { get; private set; }
        public float WarningTime { get; private set; }
        public float FlameHeight { get; private set; }
        public float DamageRadius { get; private set; }
        
        private Sprite sprite;
        private float stateTimer;
        private Level level;
        private List<FlameParticle> flameParticles;
        private List<WarningSteam> steamEffects;
        private float currentFlameHeight;
        private VertexLight geyserLight;
        private float pulseTimer;
        #endregion

        #region Constructor
        public FlameGeyser(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("eruptInterval", 4f),
                data.Float("eruptDuration", 1f),
                data.Float("warningTime", 1f),
                data.Float("flameHeight", 200f),
                data.Float("damageRadius", 30f)
            );
        }

        public FlameGeyser(Vector2 position, float eruptInterval = 4f, float eruptDuration = 1f,
            float warningTime = 1f, float flameHeight = 200f, float damageRadius = 30f)
            : base(position)
        {
            Initialize(eruptInterval, eruptDuration, warningTime, flameHeight, damageRadius);
        }

        private void Initialize(float eruptInterval, float eruptDuration, float warningTime, float flameHeight, float damageRadius)
        {
            EruptInterval = eruptInterval;
            EruptDuration = eruptDuration;
            WarningTime = warningTime;
            FlameHeight = flameHeight;
            DamageRadius = damageRadius;
            
            State = GeyserState.Idle;
            stateTimer = EruptInterval;
            currentFlameHeight = 0f;
            pulseTimer = 0f;
            flameParticles = new List<FlameParticle>();
            steamEffects = new List<WarningSteam>();
            
            Collider = new Hitbox(32f, 16f, -16f, -16f);
            
            Add(sprite = GFX.SpriteBank.Create("flame_geyser"));
            sprite.Play("idle");
            
            Add(geyserLight = new VertexLight(Color.Orange, 0.2f, 12, 32));
        }
        #endregion

        #region Public Methods
        public void ForceErupt()
        {
            if (State == GeyserState.Idle || State == GeyserState.Warning)
            {
                stateTimer = 0f;
            }
        }

        public void SetOverheated()
        {
            State = GeyserState.Overheated;
            geyserLight.Color = Color.Red;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.Shake(0.3f);
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
            
            pulseTimer += Engine.DeltaTime * 3f;
            
            var player = Scene.Tracker.GetEntity<Player>();
            
            switch (State)
            {
                case GeyserState.Idle:
                    stateTimer -= Engine.DeltaTime;
                    
                    if (stateTimer <= 0f)
                    {
                        State = GeyserState.Warning;
                        sprite.Play("warning");
                        stateTimer = WarningTime;
                        geyserLight.Alpha = 0.5f;
                        geyserLight.Color = Color.Yellow;
                    }
                    break;
                    
                case GeyserState.Warning:
                    stateTimer -= Engine.DeltaTime;
                    
                    // Create warning steam
                    if (Scene.OnInterval(0.1f))
                    {
                        CreateWarningSteam();
                    }
                    
                    // Pulse light
                    geyserLight.Alpha = 0.5f + (float)Math.Sin(pulseTimer) * 0.2f;
                    
                    if (stateTimer <= 0f)
                    {
                        State = GeyserState.Erupting;
                        sprite.Play("erupting");
                        stateTimer = EruptDuration;
                        geyserLight.Alpha = 0.8f;
                        geyserLight.Color = Color.Orange;
                        
                        Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
                        level?.Shake(0.3f);
                    }
                    break;
                    
                case GeyserState.Erupting:
                    stateTimer -= Engine.DeltaTime;
                    
                    // Rise flame
                    currentFlameHeight = Calc.Approach(currentFlameHeight, FlameHeight, 400f * Engine.DeltaTime);
                    
                    // Create flame particles
                    if (Scene.OnInterval(0.02f))
                    {
                        CreateFlameParticle();
                    }
                    
                    // Check player collision
                    if (player != null && IsPlayerInFlame(player))
                    {
                        player.Die(Vector2.Zero);
                    }
                    
                    if (stateTimer <= 0f)
                    {
                        State = GeyserState.Cooling;
                        sprite.Play("cooling");
                        stateTimer = 1f;
                    }
                    break;
                    
                case GeyserState.Cooling:
                    stateTimer -= Engine.DeltaTime;
                    
                    // Lower flame
                    currentFlameHeight = Calc.Approach(currentFlameHeight, 0f, 300f * Engine.DeltaTime);
                    
                    if (stateTimer <= 0f)
                    {
                        State = GeyserState.Idle;
                        sprite.Play("idle");
                        stateTimer = EruptInterval;
                        geyserLight.Alpha = 0.2f;
                        geyserLight.Color = Color.Orange;
                    }
                    break;
                    
                case GeyserState.Overheated:
                    // Constant eruption
                    currentFlameHeight = FlameHeight * 1.5f;
                    
                    if (Scene.OnInterval(0.01f))
                    {
                        CreateFlameParticle();
                    }
                    
                    if (player != null && IsPlayerInFlame(player))
                    {
                        player.Die(Vector2.Zero);
                    }
                    break;
            }
            
            flameParticles.RemoveAll(p => p == null || p.Scene == null);
            steamEffects.RemoveAll(s => s == null || s.Scene == null);
        }

        private bool IsPlayerInFlame(Player player)
        {
            float distance = Vector2.Distance(Position, player.Position);
            return distance < DamageRadius && player.Position.Y < Position.Y && player.Position.Y > Position.Y - currentFlameHeight;
        }

        private void CreateFlameParticle()
        {
            var particle = new FlameParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * currentFlameHeight - currentFlameHeight / 2),
                new Vector2(Calc.Random.NextFloat() * 40f - 20f, -Calc.Random.NextFloat() * 100f)
            );
            flameParticles.Add(particle);
            Scene.Add(particle);
        }

        private void CreateWarningSteam()
        {
            var steam = new WarningSteam(
                Position + new Vector2(Calc.Random.NextFloat() * 16f - 8f, 0f)
            );
            steamEffects.Add(steam);
            Scene.Add(steam);
        }

        public override void Render()
        {
            // Draw flame column
            if (currentFlameHeight > 0f)
            {
                float alpha = State == GeyserState.Overheated ? 0.6f : 0.4f;
                Draw.Rect(Position.X - DamageRadius, Position.Y - currentFlameHeight, DamageRadius * 2, currentFlameHeight, Color.Orange * alpha);
                
                // Flame tip
                Draw.Circle(Position - Vector2.UnitY * currentFlameHeight, DamageRadius * 0.8f, Color.Yellow * alpha * 0.5f, 8);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// FlameParticle - Particle for flame effects
    /// </summary>
    public class FlameParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public FlameParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.5f - 0.2f) + 0.2f;
            lifetime = maxLifetime;
            
            Color[] colors = { Color.Orange, Color.Red, Color.Yellow };
            color = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y -= 100f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 6f, color * (alpha * 0.6f), 5);
        }
    }

    /// <summary>
    /// WarningSteam - Steam effect before eruption
    /// </summary>
    public class WarningSteam : Actor
    {
        private float lifetime;
        private float maxLifetime;
        private float scale;

        public WarningSteam(Vector2 position)
            : base(position)
        {
            maxLifetime = 0.5f;
            lifetime = maxLifetime;
            scale = 0.5f;
        }

        public override void Update()
        {
            base.Update();
            Position.Y -= 30f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            scale += Engine.DeltaTime;
            
            if (lifetime <= 0f)
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 8f * scale, Color.White * (alpha * 0.3f), 6);
        }
    }
}

