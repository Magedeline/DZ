namespace Celeste.Entities.Chapters.Ch11
{
    /// <summary>
    /// TumbleweedCluster - Fast-moving tumbleweeds that push player randomly
    /// Bounces off walls and creates wind effects
    /// Sprite path: objects/DZ/DZ/tumbleweed/
    /// </summary>
    [CustomEntity("DZ/TumbleweedCluster")]
    [Tracked]
    public class TumbleweedCluster : Actor
    {
        #region Enums
        public enum TumbleweedState
        {
            Idle,
            Rolling,
            Bouncing,
            Scattering
        }
        #endregion

        #region Properties
        public TumbleweedState State { get; private set; }
        public float RollSpeed { get; private set; }
        public float PushForce { get; private set; }
        public int TumbleweedCount { get; private set; }
        public float BounceChance { get; private set; }
        
        private List<Tumbleweed> tumbleweeds;
        public Vector2 clusterVelocity;
        private float rotation;
        private Level level;
        private List<DustParticle> dustParticles;
        private bool isActive;
        #endregion

        #region Constructor
        public TumbleweedCluster(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Float("rollSpeed", 180f),
                data.Float("pushForce", 100f),
                data.Int("tumbleweedCount", 3),
                data.Float("bounceChance", 0.3f)
            );
        }

        public TumbleweedCluster(Vector2 position, float rollSpeed = 180f, float pushForce = 100f,
            int tumbleweedCount = 3, float bounceChance = 0.3f)
            : base(position)
        {
            Initialize(rollSpeed, pushForce, tumbleweedCount, bounceChance);
        }

        private void Initialize(float rollSpeed, float pushForce, int tumbleweedCount, float bounceChance)
        {
            RollSpeed = rollSpeed;
            PushForce = pushForce;
            TumbleweedCount = tumbleweedCount;
            BounceChance = bounceChance;
            
            State = TumbleweedState.Idle;
            clusterVelocity = new Vector2(Calc.Random.Choose(-1, 1) * rollSpeed, 0f);
            rotation = 0f;
            isActive = true;
            tumbleweeds = new List<Tumbleweed>();
            dustParticles = new List<DustParticle>();
            
            // Large collision area for cluster
            Collider = new Hitbox(60f, 40f, -30f, -20f);
            
            // Create tumbleweeds
            for (int i = 0; i < TumbleweedCount; i++)
            {
                var tumbleweed = new Tumbleweed(
                    Position + new Vector2(Calc.Random.NextFloat() * 40f - 20f, Calc.Random.NextFloat() * 40f - 20f),
                    clusterVelocity
                );
                tumbleweeds.Add(tumbleweed);
            }
        }
        #endregion

        #region Public Methods
        public void Scatter()
        {
            State = TumbleweedState.Scattering;
            
            // Scatter tumbleweeds in random directions
            foreach (var tumbleweed in tumbleweeds)
            {
                tumbleweed.SetVelocity(new Vector2(
                    Calc.Random.NextFloat() * RollSpeed * 2f - RollSpeed,
                    Calc.Random.NextFloat() * RollSpeed * 2f - RollSpeed
                ));
            }
            
            Audio.Play("event:/game/char_maddy/jump", Position);
        }

        public void SetDirection(Vector2 direction)
        {
            clusterVelocity = direction.SafeNormalize() * RollSpeed;
            
            foreach (var tumbleweed in tumbleweeds)
            {
                tumbleweed.SetVelocity(clusterVelocity);
            }
        }
        #endregion

        #region Entity Overrides
        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            
            // Add tumbleweeds to scene
            foreach (var tumbleweed in tumbleweeds)
            {
                scene.Add(tumbleweed);
            }
        }

        public override void Update()
        {
            base.Update();
            
            if (!isActive) return;
            
            // Update rotation
            rotation += clusterVelocity.X * 0.01f * Engine.DeltaTime;
            
            // Move cluster
            State = TumbleweedState.Rolling;
            
            // Check for wall collision
            if (MoveH(clusterVelocity.X * Engine.DeltaTime))
            {
                // Bounce off wall
                clusterVelocity.X *= -1;
                State = TumbleweedState.Bouncing;
                
                // Random chance to scatter
                if (Calc.Random.NextFloat() < BounceChance)
                {
                    Scatter();
                }
                
                Audio.Play("event:/game/char_maddy/land");
            }
            
            // Create dust particles
            if (Scene.OnInterval(0.1f))
            {
                CreateDustParticle();
            }
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                // Push player
                Vector2 pushDir = clusterVelocity.SafeNormalize();
                player.Speed += pushDir * PushForce * Engine.DeltaTime;
                
                // Create impact dust
                for (int i = 0; i < 5; i++)
                {
                    CreateDustParticle();
                }
            }
            
            // Update tumbleweeds
            foreach (var tumbleweed in tumbleweeds)
            {
                tumbleweed.UpdatePosition(clusterVelocity);
            }
            
            dustParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        private void CreateDustParticle()
        {
            var particle = new DustParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 40f - 20f, Calc.Random.NextFloat() * 40f - 20f),
                new Vector2(Calc.Random.NextFloat() * 40f - 20f, -Calc.Random.NextFloat() * 40f)
            );
            dustParticles.Add(particle);
            Scene.Add(particle);
        }

        public override void Render()
        {
            // Draw wind lines behind cluster
            if (State == TumbleweedState.Rolling)
            {
                Vector2 windDir = -clusterVelocity.SafeNormalize();
                for (int i = 0; i < 3; i++)
                {
                    Vector2 lineStart = Position + new Vector2(Calc.Random.NextFloat() * 30f - 15f, Calc.Random.NextFloat() * 30f - 15f);
                    Draw.Line(lineStart, lineStart + windDir * 20f, Color.Beige * 0.3f, 1f);
                }
            }
            
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// Tumbleweed - Individual tumbleweed in cluster
    /// </summary>
    public class Tumbleweed : Actor
    {
        private Sprite sprite;
        private Vector2 velocity;
        private float rotation;
        private float rotationSpeed;

        public Tumbleweed(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            rotation = Calc.Random.NextFloat() * MathHelper.TwoPi;
            rotationSpeed = Calc.Random.NextFloat() * 5f - 2.5f;
            
            Collider = new Hitbox(20f, 20f, -10f, -10f);
            Add(sprite = GFX.SpriteBank.Create("tumbleweed"));
            sprite.Rotation = rotation;
        }

        public void SetVelocity(Vector2 newVelocity)
        {
            velocity = newVelocity;
        }

        public void UpdatePosition(Vector2 clusterVelocity)
        {
            // Follow cluster with some wobble
            Position += clusterVelocity * Engine.DeltaTime;
            Position += new Vector2(
                (float)Math.Sin(Calc.Random.NextFloat() * MathHelper.TwoPi) * 2f * Engine.DeltaTime,
                (float)Math.Cos(Calc.Random.NextFloat() * MathHelper.TwoPi) * 1f * Engine.DeltaTime
            );
            
            // Rotate
            rotation += rotationSpeed * Engine.DeltaTime * (velocity.X > 0 ? -1 : 1);
            sprite.Rotation = rotation;
        }

        public override void Update()
        {
            base.Update();
            
            // Individual physics when scattered
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 100f * Engine.DeltaTime; // Light gravity
            velocity *= 0.99f;
            
            rotation += rotationSpeed * Engine.DeltaTime;
            sprite.Rotation = rotation;
            
            // Bounce off ground
            if (OnGround())
            {
                velocity.Y = -Math.Abs(velocity.Y) * 0.5f;
                velocity.X *= 0.8f;
            }
            
            // Remove if stopped
            if (velocity.Length() < 10f && OnGround())
            {
                RemoveSelf();
            }
        }
    }

    /// <summary>
    /// DustParticle - Desert dust particle
    /// </summary>
    public class DustParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float scale;
        private float rotation;
        private float rotationSpeed;

        public DustParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.5f - 0.2f) + 0.2f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (0.8f - 0.3f) + 0.3f;
            rotation = 0f;
            rotationSpeed = Calc.Random.NextFloat() * 5f - 2.5f;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 100f * Engine.DeltaTime; // Light gravity
            velocity *= 0.99f;
            
            rotation += rotationSpeed * Engine.DeltaTime;
            
            // Bounce off ground
            if (OnGround())
            {
                velocity.Y = -Math.Abs(velocity.Y) * 0.5f;
                velocity.X *= 0.8f;
            }
            
            // Remove if stopped
            if (velocity.Length() < 10f && OnGround())
            {
                RemoveSelf();
            }
        }
    }

    /// <summary>
    /// WindZone - Area that affects tumbleweed direction
    /// </summary>
    [CustomEntity("DZ/WindZone")]
    public class WindZone : Trigger
    {
        private Vector2 windDirection;
        private float windStrength;

        public WindZone(EntityData data, Vector2 offset)
            : base(data, offset)
        {
            windDirection = Calc.AngleToVector(data.Float("windAngle", 0f) * (float)(Math.PI / 180f), 1f);
            windStrength = data.Float("windStrength", 100f);
        }

        public override void OnEnter(Player player)
        {
            // Apply wind to player
            player.Speed += windDirection * windStrength * Engine.DeltaTime;
            
            // Affect tumbleweeds
            foreach (var cluster in Scene.Tracker.GetEntities<TumbleweedCluster>())
            {
                ((TumbleweedCluster)cluster).clusterVelocity += windDirection * windStrength * Engine.DeltaTime;
            }
        }
    }
}
