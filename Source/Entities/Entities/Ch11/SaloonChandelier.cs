namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// SaloonChandelier - Swinging hazard that can be shot down
    /// Swings on a chain and can fall to create a hazard or platform
    /// Sprite path: objects/DZ/DZ/saloonDZ_CHandelier/
    /// </summary>
    [CustomEntity("DZ/SaloonChandelier")]
    [Tracked]
    public class SaloonChandelier : Actor
    {
        #region Enums
        public enum ChandelierState
        {
            Swinging,
            Falling,
            Crashed,
            Destroyed
        }
        #endregion

        #region Properties
        public ChandelierState State { get; private set; }
        public float SwingPeriod { get; private set; }
        public float SwingAngle { get; private set; }
        public float ChainLength { get; private set; }
        public bool CanFall { get; private set; }
        public bool IsHazard { get; private set; }
        
        private Sprite sprite;
        private Vector2 anchorPoint;
        private float currentAngle;
        private float angularVelocity;
        private float time;
        private Level level;
        private List<ChandelierShard> shards;
        private bool hasFallen;
        private VertexLight candleLight;
        private List<CandleFlameParticle> flameParticles;
        #endregion

        #region Constructor
        public SaloonChandelier(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("swingPeriod", 3f),
                data.Float("swingAngle", 0.4f),
                data.Float("chainLength", 80f),
                data.Bool("canFall", true),
                data.Bool("isHazard", true)
            );
        }

        public SaloonChandelier(Vector2 position, float swingPeriod = 3f, float swingAngle = 0.4f,
            float chainLength = 80f, bool canFall = true, bool isHazard = true)
            : base(position)
        {
            Initialize(swingPeriod, swingAngle, chainLength, canFall, isHazard);
        }

        private void Initialize(float swingPeriod, float swingAngle, float chainLength, bool canFall, bool isHazard)
        {
            SwingPeriod = swingPeriod;
            SwingAngle = swingAngle;
            ChainLength = chainLength;
            CanFall = canFall;
            IsHazard = isHazard;
            
            // Anchor is at initial position (ceiling)
            anchorPoint = Position;
            
            State = ChandelierState.Swinging;
            currentAngle = 0f;
            angularVelocity = 0f;
            time = 0f;
            hasFallen = false;
            shards = new List<ChandelierShard>();
            flameParticles = new List<CandleFlameParticle>();
            
            // Setup collider
            Collider = new Hitbox(48f, 32f, -24f, -16f);
            
            // Setup sprite
            Add(sprite = DZModule.SpriteBank.Create("saloonDZ_CHandelier"));
            sprite.Play("swinging");
            
            // Add candle light
            Add(candleLight = new VertexLight(Color.Orange, 0.6f, 16, 48));
        }
        #endregion

        #region Public Methods
        public void ShootDown()
        {
            if (!CanFall || State != ChandelierState.Swinging) return;
            
            State = ChandelierState.Falling;
            sprite.Play("falling");
            
            Audio.Play("event:/game/gen_crumble_fall", Position);
            
            Add(new Coroutine(FallRoutine()));
        }

        public void Destroy()
        {
            if (State == ChandelierState.Destroyed) return;
            
            State = ChandelierState.Destroyed;
            
            // Create shards
            for (int i = 0; i < 8; i++)
            {
                var shard = new ChandelierShard(
                    Position,
                    new Vector2(Calc.Random.NextFloat() * 100f - 50f, Calc.Random.NextFloat() * 100f - 50f)
                );
                shards.Add(shard);
                Scene.Add(shard);
            }
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.Shake(0.3f);
            
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private IEnumerator FallRoutine()
        {
            float fallSpeed = 0f;
            float gravity = 400f;
            
            while (!OnGround())
            {
                fallSpeed += gravity * Engine.DeltaTime;
                Position.Y += fallSpeed * Engine.DeltaTime;
                
                // Update light position
                candleLight.Position = Vector2.Zero;
                
                // Check player collision
                if (IsHazard)
                {
                    var player = Scene.Tracker.GetEntity<Player>();
                    if (player != null && Collide.Check(this, player))
                    {
                        player.Die(Vector2.Zero);
                    }
                }
                
                yield return null;
            }
            
            // Crash landing
            State = ChandelierState.Crashed;
            sprite.Play("crashed");
            
            level?.Shake(0.4f);
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 15, Position, Vector2.One * 8f);
            Audio.Play("event:/game/general/crystalheart_pulse", Position);
            
            hasFallen = true;
            
            // Extinguish candles
            candleLight.Alpha = 0.2f;
        }

        private void CreateFlameParticle()
        {
            var particle = new CandleFlameParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 24f - 12f, Calc.Random.NextFloat() * 24f - 12f),
                new Vector2(Calc.Random.NextFloat() * 20f - 10f, -Calc.Random.NextFloat() * 30f)
            );
            flameParticles.Add(particle);
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
            
            if (State == ChandelierState.Swinging)
            {
                // Pendulum physics
                time += Engine.DeltaTime;
                currentAngle = (float)Math.Sin(time * MathHelper.TwoPi / SwingPeriod) * SwingAngle;
                
                // Calculate position from angle
                Position.X = anchorPoint.X + (float)Math.Sin(currentAngle) * ChainLength;
                Position.Y = anchorPoint.Y + (float)Math.Cos(currentAngle) * ChainLength - ChainLength;
                
                // Rotate sprite
                sprite.Rotation = currentAngle;
                
                // Create flame particles
                if (Scene.OnInterval(0.1f))
                {
                    CreateFlameParticle();
                }
                
                // Check player collision
                if (IsHazard)
                {
                    var player = Scene.Tracker.GetEntity<Player>();
                    if (player != null && Collide.Check(this, player))
                    {
                        player.Die(Vector2.Zero);
                    }
                }
            }
            
            if (State == ChandelierState.Crashed)
            {
                // Now acts as platform
                // Check if player uses it as platform
            }
            
            flameParticles.RemoveAll(p => p == null || p.Scene == null);
            shards.RemoveAll(s => s == null || s.Scene == null);
        }

        public override void Render()
        {
            // Draw chain
            if (State == ChandelierState.Swinging)
            {
                Draw.Line(anchorPoint, Position, Color.Brown, 2f);
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// ChandelierShard - Shard from destroyed chandelier
    /// </summary>
    public class ChandelierShard : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float rotation;
        private float rotationSpeed;

        public ChandelierShard(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            lifetime = 2f;
            rotation = Calc.Random.NextFloat() * MathHelper.TwoPi;
            rotationSpeed = Calc.Random.NextFloat() * 10f - 5f;
            
            Collider = new Hitbox(8f, 8f, -4f, -4f);
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 300f * Engine.DeltaTime;
            rotation += rotationSpeed * Engine.DeltaTime;
            
            lifetime -= Engine.DeltaTime;
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
            }
            
            if (lifetime <= 0f || OnGround())
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            Draw.Rect(Position - new Vector2(4, 4), 8, 8, Color.Gold * 0.8f);
        }
    }

    /// <summary>
    /// CandleFlameParticle - Small flame particle from candles
    /// </summary>
    public class CandleFlameParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;

        public CandleFlameParticle(Vector2 position, Vector2 velocity)
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
            velocity.Y -= 60f * Engine.DeltaTime;
            
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 3f, Color.Orange * (alpha * 0.5f), 4);
        }
    }
}

