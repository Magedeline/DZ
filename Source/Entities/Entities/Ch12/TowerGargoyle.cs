namespace Celeste.Entities.Chapters.Ch12
{
    /// <summary>
    /// TowerGargoyle - Stone guardian that awakens and swoops at player
    /// Perches on tower walls, activates when player approaches
    /// Sprite path: characters/DZ/tower_gargoyle/
    /// </summary>
    [CustomEntity("DZ/TowerGargoyle")]
    [Tracked]
    public class TowerGargoyle : Actor
    {
        #region Enums
        public enum GargoyleState
        {
            Dormant,
            Awakening,
            Perching,
            Swooping,
            Gliding,
            Attacking,
            Stunned,
            Defeated
        }
        #endregion

        #region Properties
        public GargoyleState State { get; private set; }
        public int Health { get; private set; }
        public float DetectionRange { get; private set; }
        public float SwoopSpeed { get; private set; }
        public float GlideSpeed { get; private set; }
        public bool IsAlive => Health > 0;
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private Vector2 perchPosition;
        private Vector2 swoopTarget;
        private Vector2 velocity;
        private float stateTimer;
        private Player targetPlayer;
        private Level level;
        private List<StoneParticle> stoneParticles;
        private bool hasAwakened;
        private VertexLight eyeGlow;
        #endregion

        #region Constructor
        public TowerGargoyle(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("health", 2),
                data.Float("detectionRange", 150f),
                data.Float("swoopSpeed", 250f),
                data.Float("glideSpeed", 100f)
            );
        }

        public TowerGargoyle(Vector2 position, int health = 2, float detectionRange = 150f,
            float swoopSpeed = 250f, float glideSpeed = 100f)
            : base(position)
        {
            Initialize(health, detectionRange, swoopSpeed, glideSpeed);
        }

        private void Initialize(int health, float detectionRange, float swoopSpeed, float glideSpeed)
        {
            Health = health;
            DetectionRange = detectionRange;
            SwoopSpeed = swoopSpeed;
            GlideSpeed = glideSpeed;
            
            perchPosition = Position;
            State = GargoyleState.Dormant;
            velocity = Vector2.Zero;
            stateTimer = 0f;
            hasAwakened = false;
            stoneParticles = new List<StoneParticle>();
            
            Collider = new Hitbox(28f, 24f, -14f, -12f);
            Add(sprite = DZModule.SpriteBank.Create("tower_gargoyle"));
            sprite.Play("dormant");
            
            Add(eyeGlow = new VertexLight(Color.Red, 0f, 4, 12));
            eyeGlow.Position = new Vector2(0f, -8f);
            
            Add(stateMachine = new StateMachine());
        }
        #endregion

        #region State Begin Methods
        private void DormantBegin()
        {
            sprite.Play("dormant");
            State = GargoyleState.Dormant;
            eyeGlow.Alpha = 0f;
        }

        private void AwakeningBegin()
        {
            sprite.Play("awakening");
            State = GargoyleState.Awakening;
            Audio.Play("event:/game/gen_crumble_fall", Position);
            level?.Shake(0.2f);
        }

        private void PerchingBegin()
        {
            sprite.Play("perch");
            State = GargoyleState.Perching;
            eyeGlow.Alpha = 0.6f;
        }

        private void SwoopingBegin()
        {
            sprite.Play("swoop");
            State = GargoyleState.Swooping;
            Audio.Play("event:/game/char_maddy/jump", Position);
        }

        private void GlidingBegin()
        {
            sprite.Play("glide");
            State = GargoyleState.Gliding;
        }

        private void AttackingBegin()
        {
            sprite.Play("attack");
            State = GargoyleState.Attacking;
        }

        private void StunnedBegin()
        {
            sprite.Play("stunned");
            State = GargoyleState.Stunned;
            velocity = Vector2.Zero;
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = GargoyleState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator DormantRoutine()
        {
            while (true)
            {
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
                {
                    stateMachine.State = 1;
                    yield break;
                }
                yield return null;
            }
        }

        private IEnumerator AwakeningRoutine()
        {
            for (int i = 0; i < 8; i++)
            {
                CreateStoneParticle();
                yield return 0.1f;
            }
            
            hasAwakened = true;
            stateMachine.State = 2;
        }

        private IEnumerator PerchingRoutine()
        {
            float waitTimer = 0f;
            
            while (true)
            {
                targetPlayer = Scene.Tracker.GetEntity<Player>();
                
                if (targetPlayer != null)
                {
                    sprite.Scale.X = targetPlayer.Position.X > Position.X ? 1 : -1;
                    
                    float distance = Vector2.Distance(Position, targetPlayer.Position);
                    if (distance < DetectionRange * 1.2f && waitTimer > 1f)
                    {
                        swoopTarget = targetPlayer.Position;
                        stateMachine.State = 3;
                        yield break;
                    }
                }
                
                waitTimer += Engine.DeltaTime;
                yield return null;
            }
        }

        private IEnumerator SwoopingRoutine()
        {
            Vector2 swoopDirection = (swoopTarget - Position).SafeNormalize();
            velocity = swoopDirection * SwoopSpeed;
            
            float swoopTime = 0.8f;
            stateTimer = swoopTime;
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                Position += velocity * Engine.DeltaTime;
                
                if (Scene.OnInterval(0.05f))
                    CreateStoneParticle();
                
                if (targetPlayer != null && Collide.Check(this, targetPlayer))
                    targetPlayer.Die(Vector2.Zero);
                
                yield return null;
            }
            
            stateMachine.State = 4;
        }

        private IEnumerator GlidingRoutine()
        {
            float glideTime = 2f;
            stateTimer = glideTime;
            Vector2 glideDirection = velocity.SafeNormalize();
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                velocity = glideDirection * GlideSpeed;
                velocity.Y += 50f * Engine.DeltaTime;
                Position += velocity * Engine.DeltaTime;
                
                var player = Scene.Tracker.GetEntity<Player>();
                if (player != null && Collide.Check(this, player))
                    player.Die(Vector2.Zero);
                
                if (CollideCheck<Solid>(Position + velocity * Engine.DeltaTime))
                {
                    stateMachine.State = 2;
                    Position = FindPerchPosition();
                    yield break;
                }
                
                yield return null;
            }
            
            stateMachine.State = 2;
            Position = FindPerchPosition();
        }

        private IEnumerator AttackingRoutine()
        {
            yield return 0.3f;
            stateMachine.State = 2;
        }

        private IEnumerator StunnedRoutine()
        {
            float stunDuration = 1f;
            stateTimer = stunDuration;
            
            while (stateTimer > 0f)
            {
                stateTimer -= Engine.DeltaTime;
                yield return null;
            }
            
            stateMachine.State = 2;
        }

        private IEnumerator DefeatedRoutine()
        {
            for (int i = 0; i < 15; i++)
            {
                CreateStoneParticle();
                yield return 0.05f;
            }
            
            level?.ParticlesFG.Emit(ParticleTypes.Dust, 10, Position, Vector2.One * 8f);
            yield return 0.5f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private Vector2 FindPerchPosition()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 checkPos = Position + new Vector2(i * 20, 0);
                if (Scene.CollideCheck<Solid>(checkPos))
                    return checkPos + Vector2.UnitY * 12f;
            }
            return perchPosition;
        }

        private void CreateStoneParticle()
        {
            var particle = new StoneParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 20f - 10f, Calc.Random.NextFloat() * 20f - 10f),
                new Vector2(Calc.Random.NextFloat() * 60f - 30f, Calc.Random.NextFloat() * 80f - 30f)
            );
            stoneParticles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void TakeDamage(int damage)
        {
            if (State == GargoyleState.Defeated) return;
            
            Health -= damage;
            Audio.Play("event:/game/char_badeline/disappear", Position);
            CreateStoneParticle();
            
            if (Health <= 0)
                stateMachine.State = 7;
            else
                stateMachine.State = 6;
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
            stoneParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            if (hasAwakened && State != GargoyleState.Defeated)
            {
                Vector2 eyePos = Position + new Vector2(sprite.Scale.X * 6f, -8f);
                Draw.Circle(eyePos, 4f, Color.Red * 0.4f, 4);
            }
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// StoneParticle - Particle for stone effects
    /// </summary>
    public class StoneParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float scale;

        public StoneParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (0.8f - 0.4f) + 0.4f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (0.8f - 0.3f) + 0.3f;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 200f * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            if (lifetime <= 0f || OnGround())
                RemoveSelf();
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Rect(Position - new Vector2(4 * scale, 4 * scale), 8 * scale, 8 * scale, Color.Gray * (alpha * 0.6f));
        }
    }
}
