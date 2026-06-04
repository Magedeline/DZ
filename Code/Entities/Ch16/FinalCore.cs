namespace Celeste.Entities.Chapters.Ch16
{
    /// <summary>
    /// FinalCore - The final boss core entity
    /// Central entity for final chapter confrontation
    /// Sprite path: characters/final_core/
    /// </summary>
    [CustomEntity("MaggyHelper/FinalCore")]
    [Tracked]
    public class FinalCore : Actor
    {
        #region Enums
        public enum CoreState
        {
            Dormant,
            Awakening,
            Active,
            Phase1,
            Phase2,
            Phase3,
            Defeated
        }
        #endregion

        #region Properties
        public CoreState State { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float AttackInterval { get; private set; }
        
        private Sprite sprite;
        private StateMachine stateMachine;
        private float attackTimer;
        private Player targetPlayer;
        private Level level;
        private List<CoreAttack> attacks;
        private List<CoreParticle> particles;
        private VertexLight coreLight;
        private Color coreColor;
        private float pulseTimer;
        #endregion

        #region Constructor
        public FinalCore(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(data.Int("health", 10), data.Float("attackInterval", 2f));
        }

        public FinalCore(Vector2 position, int health = 10, float attackInterval = 2f)
            : base(position)
        {
            Initialize(health, attackInterval);
        }

        private void Initialize(int health, float attackInterval)
        {
            Health = health;
            MaxHealth = health;
            AttackInterval = attackInterval;
            
            State = CoreState.Dormant;
            attackTimer = 0f;
            pulseTimer = 0f;
            attacks = new List<CoreAttack>();
            particles = new List<CoreParticle>();
            coreColor = Color.Purple;
            
            Collider = new Hitbox(48f, 48f, -24f, -24f);
            
            Add(sprite = GFX.SpriteBank.Create("final_core"));
            sprite.Play("dormant");
            
            Add(coreLight = new VertexLight(coreColor, 0.4f, 16, 48));
            
            Add(stateMachine = new StateMachine());
        }
        #endregion

        #region State Begin Methods
        private void DormantBegin()
        {
            sprite.Play("dormant");
            State = CoreState.Dormant;
            coreLight.Alpha = 0.2f;
        }

        private void AwakeningBegin()
        {
            sprite.Play("awakening");
            State = CoreState.Awakening;
            coreLight.Alpha = 0.6f;
            Audio.Play("event:/game/gen_crumble_fall", Position);
            level?.Shake(0.5f);
        }

        private void ActiveBegin()
        {
            sprite.Play("active");
            State = CoreState.Active;
        }

        private void Phase1Begin()
        {
            sprite.Play("phase1");
            State = CoreState.Phase1;
            coreColor = Color.DarkRed;
            coreLight.Color = coreColor;
        }

        private void Phase2Begin()
        {
            sprite.Play("phase2");
            State = CoreState.Phase2;
            coreColor = Color.Purple;
            coreLight.Color = coreColor;
        }

        private void Phase3Begin()
        {
            sprite.Play("phase3");
            State = CoreState.Phase3;
            coreColor = Color.White;
            coreLight.Color = coreColor;
        }

        private void DefeatedBegin()
        {
            sprite.Play("defeat");
            State = CoreState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator DormantRoutine()
        {
            yield break;
        }

        private IEnumerator AwakeningRoutine()
        {
            for (int i = 0; i < 20; i++)
            {
                CreateCoreParticle();
                yield return 0.05f;
            }
            stateMachine.State = 2;
        }

        private IEnumerator ActiveRoutine()
        {
            while (true)
            {
                attackTimer += Engine.DeltaTime;
                
                if (attackTimer >= AttackInterval)
                {
                    attackTimer = 0f;
                    PerformAttack();
                }
                
                // Check health for phase transitions
                if (Health <= MaxHealth * 0.7f && State == CoreState.Active)
                {
                    stateMachine.State = 3;
                    yield break;
                }
                
                yield return null;
            }
        }

        private IEnumerator Phase1Routine()
        {
            while (true)
            {
                attackTimer += Engine.DeltaTime;
                
                if (attackTimer >= AttackInterval * 0.8f)
                {
                    attackTimer = 0f;
                    PerformPhase1Attack();
                }
                
                if (Health <= MaxHealth * 0.4f)
                {
                    stateMachine.State = 4;
                    yield break;
                }
                
                yield return null;
            }
        }

        private IEnumerator Phase2Routine()
        {
            while (true)
            {
                attackTimer += Engine.DeltaTime;
                
                if (attackTimer >= AttackInterval * 0.6f)
                {
                    attackTimer = 0f;
                    PerformPhase2Attack();
                }
                
                if (Health <= MaxHealth * 0.1f)
                {
                    stateMachine.State = 5;
                    yield break;
                }
                
                yield return null;
            }
        }

        private IEnumerator Phase3Routine()
        {
            while (true)
            {
                attackTimer += Engine.DeltaTime;
                
                if (attackTimer >= AttackInterval * 0.5f)
                {
                    attackTimer = 0f;
                    PerformPhase3Attack();
                }
                
                if (Health <= 0)
                {
                    stateMachine.State = 6;
                    yield break;
                }
                
                yield return null;
            }
        }

        private IEnumerator DefeatedRoutine()
        {
            for (int i = 0; i < 30; i++)
            {
                CreateCoreParticle();
            }
            
            level?.Flash(Color.White * 0.6f);
            level?.Shake(0.8f);
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
            
            level?.Session.SetFlag("final_core_defeated", true);
            
            yield return 1f;
            RemoveSelf();
        }
        #endregion

        #region Private Methods
        private void PerformAttack()
        {
            var attack = new CoreAttack(Position, Vector2.UnitX * 150f, coreColor);
            attacks.Add(attack);
            Scene.Add(attack);
            
            Audio.Play("event:/game/char_badeline/beam_launch", Position);
        }

        private void PerformPhase1Attack()
        {
            // Triple attack
            for (int i = -1; i <= 1; i++)
            {
                Vector2 dir = new Vector2(i, -0.5f).SafeNormalize();
                var attack = new CoreAttack(Position, dir * 180f, Color.DarkRed);
                attacks.Add(attack);
                Scene.Add(attack);
            }
            
            Audio.Play("event:/game/char_badeline/beam_launch", Position);
        }

        private void PerformPhase2Attack()
        {
            // Spiral attack
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8 * i;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                var attack = new CoreAttack(Position, dir * 200f, Color.Purple);
                attacks.Add(attack);
                Scene.Add(attack);
            }
            
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
        }

        private void PerformPhase3Attack()
        {
            // Omnidirectional burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16 * i;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                var attack = new CoreAttack(Position, dir * 250f, Color.White);
                attacks.Add(attack);
                Scene.Add(attack);
            }
            
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
            level?.Shake(0.3f);
        }

        private void CreateCoreParticle()
        {
            var particle = new CoreParticle(
                Position + new Vector2(Calc.Random.NextFloat() * 32f - 16f, Calc.Random.NextFloat() * 32f - 16f),
                new Vector2(Calc.Random.NextFloat() * 80f - 40f, -Calc.Random.NextFloat() * 60f),
                coreColor
            );
            particles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void Awaken()
        {
            if (State != CoreState.Dormant) return;
            stateMachine.State = 1;
        }

        public void TakeDamage(int damage)
        {
            if (State == CoreState.Defeated) return;
            
            Health -= damage;
            
            for (int i = 0; i < 5; i++)
            {
                CreateCoreParticle();
            }
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            level?.Shake(0.2f);
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
            
            if (State != CoreState.Dormant && State != CoreState.Defeated)
            {
                float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.1f;
                sprite.Scale = Vector2.One * pulse;
                
                if (Scene.OnInterval(0.1f))
                {
                    CreateCoreParticle();
                }
            }
            
            attacks.RemoveAll(a => a == null || a.Scene == null);
            particles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw core aura
            if (State != CoreState.Dormant)
            {
                Draw.Circle(Position, 32f, coreColor * 0.2f, 16);
            }
            
            // Draw health bar
            if (State != CoreState.Dormant && State != CoreState.Defeated)
            {
                float healthPercent = Health / (float)MaxHealth;
                Draw.Rect(Position.X - 30, Position.Y - 40, 60 * healthPercent, 6, coreColor * 0.6f);
            }
            
            base.Render();
        }
        #endregion
    }

    public class CoreAttack : Actor
    {
        private Vector2 velocity;
        private Color color;
        private float lifetime;

        public CoreAttack(Vector2 position, Vector2 velocity, Color color)
            : base(position)
        {
            this.velocity = velocity;
            this.color = color;
            lifetime = 3f;
            
            Collider = new Hitbox(12f, 12f, -6f, -6f);
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            lifetime -= Engine.DeltaTime;
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
                RemoveSelf();
                return;
            }
            
            if (lifetime <= 0f || CollideCheck<Solid>())
                RemoveSelf();
        }

        public override void Render()
        {
            Draw.Circle(Position, 6f, color * 0.7f, 6);
        }
    }

    public class CoreParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;

        public CoreParticle(Vector2 position, Vector2 velocity, Color color)
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
            velocity.Y -= 80f * Engine.DeltaTime;
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

