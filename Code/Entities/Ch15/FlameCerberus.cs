namespace Celeste.Entities.Chapters.Ch15
{
    /// <summary>
    /// FlameCerberus - Three-headed fire beast with different attacks per head
    /// Each head has unique attack patterns and can be damaged separately
    /// Sprite path: characters/flame_cerberus/
    /// </summary>
    [CustomEntity("MaggyHelper/FlameCerberus")]
    [Tracked]
    public class FlameCerberus : Actor
    {
        #region Enums
        public enum CerberusState
        {
            Idle,
            Roaming,
            Attacking,
            TripleAttack,
            Stunned,
            Defeated
        }

        public enum HeadType
        {
            Left,       // Fire breath - cone attack
            Center,     // Fireball - projectile attack
            Right       // Bite - melee attack
        }
        #endregion

        #region Properties
        public CerberusState State { get; private set; }
        public int TotalHealth { get; private set; }
        public int LeftHeadHealth { get; private set; }
        public int CenterHeadHealth { get; private set; }
        public int RightHeadHealth { get; private set; }
        public float DetectionRange { get; private set; }
        public float MoveSpeed { get; private set; }
        public bool IsAlive => TotalHealth > 0;
        public bool LeftHeadAlive => LeftHeadHealth > 0;
        public bool CenterHeadAlive => CenterHeadHealth > 0;
        public bool RightHeadAlive => RightHeadHealth > 0;
        
        private Sprite bodySprite;
        private Sprite leftHeadSprite;
        private Sprite centerHeadSprite;
        private Sprite rightHeadSprite;
        private StateMachine stateMachine;
        private Facings facing;
        private float attackCooldown;
        private float invincibilityTimer;
        private Player targetPlayer;
        private Level level;
        private VertexLight flameGlow;
        private List<CerberusFireball> fireballs;
        private List<FireBreathParticle> breathParticles;
        private int attackPattern;
        #endregion

        #region Constructor
        public FlameCerberus(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Initialize(
                data.Int("headHealth", 3),
                data.Float("detectionRange", 200f),
                data.Float("moveSpeed", 60f)
            );
        }

        public FlameCerberus(Vector2 position, int headHealth = 3, float detectionRange = 200f, float moveSpeed = 60f)
            : base(position)
        {
            Initialize(headHealth, detectionRange, moveSpeed);
        }

        private void Initialize(int headHealth, float detectionRange, float moveSpeed)
        {
            LeftHeadHealth = headHealth;
            CenterHeadHealth = headHealth;
            RightHeadHealth = headHealth;
            TotalHealth = headHealth * 3;
            DetectionRange = detectionRange;
            MoveSpeed = moveSpeed;
            
            State = CerberusState.Idle;
            facing = Facings.Right;
            attackCooldown = 0f;
            invincibilityTimer = 0f;
            attackPattern = 0;
            fireballs = new List<CerberusFireball>();
            breathParticles = new List<FireBreathParticle>();
            
            // Large collider for boss
            Collider = new Hitbox(80f, 60f, -40f, -60f);
            
            // Setup sprites
            Add(bodySprite = GFX.SpriteBank.Create("cerberus_body"));
            
            leftHeadSprite = GFX.SpriteBank.Create("cerberus_head_left");
            leftHeadSprite.Position = new Vector2(-30f, -40f);
            Add(leftHeadSprite);
            
            centerHeadSprite = GFX.SpriteBank.Create("cerberus_head_center");
            centerHeadSprite.Position = new Vector2(0f, -50f);
            Add(centerHeadSprite);
            
            rightHeadSprite = GFX.SpriteBank.Create("cerberus_head_right");
            rightHeadSprite.Position = new Vector2(30f, -40f);
            Add(rightHeadSprite);
            
            // Add flame glow
            Add(flameGlow = new VertexLight(Color.OrangeRed, 0.6f, 24, 64));
            
            // Setup state machine
            Add(stateMachine = new StateMachine());
        }
        #endregion

        #region State Begin Methods
        private void IdleBegin()
        {
            bodySprite.Play("idle");
            State = CerberusState.Idle;
        }

        private void RoamingBegin()
        {
            bodySprite.Play("walk");
            State = CerberusState.Roaming;
        }

        private void AttackingBegin()
        {
            bodySprite.Play("attack");
            State = CerberusState.Attacking;
        }

        private void TripleAttackBegin()
        {
            bodySprite.Play("triple_attack");
            State = CerberusState.TripleAttack;
            Audio.Play("guid://{78f2deb2-fccc-4d1d-b78a-9667d574bbf3}", Position);
        }

        private void StunnedBegin()
        {
            bodySprite.Play("stunned");
            State = CerberusState.Stunned;
        }

        private void DefeatedBegin()
        {
            bodySprite.Play("defeat");
            State = CerberusState.Defeated;
            Audio.Play("event:/game/char_badeline/disappear", Position);
        }
        #endregion

        #region State Routines
        private IEnumerator IdleRoutine()
        {
            targetPlayer = Scene.Tracker.GetEntity<Player>();
            
            if (targetPlayer != null && Vector2.Distance(Position, targetPlayer.Position) < DetectionRange)
            {
                stateMachine.State = 1; // Roaming
                yield break;
            }
            
            yield return 1f;
        }

        private IEnumerator RoamingRoutine()
        {
            while (true)
            {
                if (targetPlayer == null)
                    targetPlayer = Scene.Tracker.GetEntity<Player>();
                
                if (targetPlayer != null)
                {
                    // Move toward player
                    Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
                    MoveH(direction.X * MoveSpeed * Engine.DeltaTime);
                    
                    facing = targetPlayer.Position.X > Position.X ? Facings.Right : Facings.Left;
                    bodySprite.Scale.X = facing == Facings.Right ? 1 : -1;
                    
                    // Check attack range
                    float distance = Vector2.Distance(Position, targetPlayer.Position);
                    if (distance < 150f && attackCooldown <= 0f)
                    {
                        // Choose attack based on alive heads
                        if (LeftHeadAlive && CenterHeadAlive && RightHeadAlive)
                        {
                            if (attackPattern >= 3)
                            {
                                stateMachine.State = 3; // TripleAttack
                                attackPattern = 0;
                            }
                            else
                            {
                                stateMachine.State = 2; // Attacking
                                attackPattern++;
                            }
                        }
                        else
                        {
                            stateMachine.State = 2; // Attacking
                        }
                        yield break;
                    }
                }
                
                yield return null;
            }
        }

        private IEnumerator AttackingRoutine()
        {
            // Choose which head attacks
            HeadType attackingHead = ChooseAttackingHead();
            
            switch (attackingHead)
            {
                case HeadType.Left:
                    yield return LeftHeadAttack();
                    break;
                case HeadType.Center:
                    yield return CenterHeadAttack();
                    break;
                case HeadType.Right:
                    yield return RightHeadAttack();
                    break;
            }
            
            attackCooldown = 1.5f;
            stateMachine.State = 1; // Roaming
        }

        private IEnumerator TripleAttackRoutine()
        {
            // All three heads attack in sequence
            if (LeftHeadAlive)
            {
                yield return LeftHeadAttack();
                yield return 0.3f;
            }
            
            if (CenterHeadAlive)
            {
                yield return CenterHeadAttack();
                yield return 0.3f;
            }
            
            if (RightHeadAlive)
            {
                yield return RightHeadAttack();
            }
            
            attackCooldown = 3f;
            stateMachine.State = 1; // Roaming
        }

        private IEnumerator StunnedRoutine()
        {
            float stunDuration = 1.5f;
            while (stunDuration > 0f)
            {
                stunDuration -= Engine.DeltaTime;
                yield return null;
            }
            
            stateMachine.State = 1; // Roaming
        }

        private IEnumerator DefeatedRoutine()
        {
            // Death animation for each head
            if (LeftHeadAlive) leftHeadSprite.Play("death");
            if (CenterHeadAlive) centerHeadSprite.Play("death");
            if (RightHeadAlive) rightHeadSprite.Play("death");
            
            // Massive flame particles
            for (int i = 0; i < 30; i++)
            {
                CreateBreathParticle(Position + new Vector2(Calc.Random.NextFloat() * 80f - 40f, Calc.Random.NextFloat() * 80f - 40f));
                yield return 0.05f;
            }
            
            level?.Shake(0.5f);
            level?.Flash(Color.OrangeRed * 0.4f);
            
            yield return 2f;
            RemoveSelf();
        }
        #endregion

        #region Head Attacks
        private IEnumerator LeftHeadAttack()
        {
            leftHeadSprite.Play("breath");
            
            // Fire breath cone attack
            for (int i = 0; i < 15; i++)
            {
                float angle = facing == Facings.Right ? -0.4f : (float)Math.PI + 0.4f;
                angle += Calc.Random.NextFloat() * 0.8f - 0.4f;
                
                var particle = new FireBreathParticle(
                    Position + new Vector2(facing == Facings.Right ? -30f : 30f, 0f),
                    Calc.AngleToVector(angle, 150f + Calc.Random.NextFloat() * 50f)
                );
                breathParticles.Add(particle);
                Scene.Add(particle);
                
                Audio.Play("event:/game/char_badeline/beam_launch");
                yield return 0.08f;
            }
            
            leftHeadSprite.Play("idle");
        }

        private IEnumerator CenterHeadAttack()
        {
            centerHeadSprite.Play("fireball");
            
            yield return 0.3f;
            
            // Fire aimed projectile
            if (targetPlayer != null)
            {
                Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
                var fireball = new CerberusFireball(
                    Position + new Vector2(0f, -50f),
                    direction * 180f
                );
                fireballs.Add(fireball);
                Scene.Add(fireball);
                
                Audio.Play("event:/game/char_badeline/beam_launch", Position);
            }
            
            centerHeadSprite.Play("idle");
        }

        private IEnumerator RightHeadAttack()
        {
            rightHeadSprite.Play("bite");
            
            yield return 0.4f;
            
            // Melee bite attack
            Vector2 biteOffset = new Vector2(facing == Facings.Right ? 60f : -60f, -30f);
            var biteHitbox = new Hitbox(50f, 40f, biteOffset.X - 25f, biteOffset.Y - 20f);
            
            if (targetPlayer != null && biteHitbox.Bounds.Intersects(targetPlayer.Collider.Bounds))
            {
                targetPlayer.Die(Vector2.Zero);
            }
            
            rightHeadSprite.Play("idle");
        }
        #endregion

        #region Private Methods
        private HeadType ChooseAttackingHead()
        {
            List<HeadType> aliveHeads = new List<HeadType>();
            if (LeftHeadAlive) aliveHeads.Add(HeadType.Left);
            if (CenterHeadAlive) aliveHeads.Add(HeadType.Center);
            if (RightHeadAlive) aliveHeads.Add(HeadType.Right);
            
            return aliveHeads[Calc.Random.Next(aliveHeads.Count)];
        }

        private void CreateBreathParticle(Vector2 position)
        {
            var particle = new FireBreathParticle(position, new Vector2(Calc.Random.NextFloat() * 60f - 30f, Calc.Random.NextFloat() * 60f - 30f));
            breathParticles.Add(particle);
            Scene.Add(particle);
        }
        #endregion

        #region Public Methods
        public void DamageHead(HeadType head, int damage)
        {
            if (State == CerberusState.Defeated) return;
            
            switch (head)
            {
                case HeadType.Left:
                    LeftHeadHealth = Math.Max(0, LeftHeadHealth - damage);
                    leftHeadSprite.Play("hurt");
                    break;
                case HeadType.Center:
                    CenterHeadHealth = Math.Max(0, CenterHeadHealth - damage);
                    centerHeadSprite.Play("hurt");
                    break;
                case HeadType.Right:
                    RightHeadHealth = Math.Max(0, RightHeadHealth - damage);
                    rightHeadSprite.Play("hurt");
                    break;
            }
            
            TotalHealth = LeftHeadHealth + CenterHeadHealth + RightHeadHealth;
            
            Audio.Play("event:/game/char_badeline/disappear", Position);
            
            if (TotalHealth <= 0)
            {
                stateMachine.State = 5; // Defeated
            }
            else
            {
                stateMachine.State = 4; // Stunned
            }
        }

        public void TakeDamage(int damage)
        {
            // Damage random alive head
            DamageHead(ChooseAttackingHead(), damage);
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
            
            if (attackCooldown > 0f) attackCooldown -= Engine.DeltaTime;
            if (invincibilityTimer > 0f) invincibilityTimer -= Engine.DeltaTime;
            
            fireballs.RemoveAll(f => f == null || f.Scene == null);
            breathParticles.RemoveAll(p => p == null || p.Scene == null);
        }

        public override void Render()
        {
            // Draw massive flame aura
            Draw.Circle(Position - Vector2.UnitY * 30f, 50f, Color.OrangeRed * 0.2f, 16);
            base.Render();
        }
        #endregion
    }

    /// <summary>
    /// CerberusFireball - Projectile fired by center head
    /// </summary>
    public class CerberusFireball : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private Sprite sprite;

        public CerberusFireball(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            lifetime = 3f;
            
            Collider = new Hitbox(20f, 20f, -10f, -10f);
            Add(sprite = GFX.SpriteBank.Create("cerberus_fireball"));
            Add(new VertexLight(Color.Orange, 0.8f, 8, 20));
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity *= 0.98f;
            lifetime -= Engine.DeltaTime;
            
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Collide.Check(this, player))
            {
                player.Die(Vector2.Zero);
                RemoveSelf();
                return;
            }
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            Draw.Circle(Position, 14f, Color.Orange * 0.4f, 8);
            base.Render();
        }
    }

    /// <summary>
    /// FireBreathParticle - Particle for fire breath attack
    /// </summary>
    public class FireBreathParticle : Actor
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private Color color;
        private float scale;

        public FireBreathParticle(Vector2 position, Vector2 velocity)
            : base(position)
        {
            this.velocity = velocity;
            maxLifetime = Calc.Random.NextFloat() * (1f - 0.5f) + 0.5f;
            lifetime = maxLifetime;
            scale = Calc.Random.NextFloat() * (1.5f - 0.8f) + 0.8f;
            
            Color[] colors = { Color.Orange, Color.OrangeRed, Color.Red, Color.Yellow };
            color = colors[Calc.Random.Next(colors.Length)];
        }

        public override void Update()
        {
            base.Update();
            
            Position += velocity * Engine.DeltaTime;
            velocity.Y += 50f * Engine.DeltaTime;
            velocity *= 0.97f;
            
            lifetime -= Engine.DeltaTime;
            
            // Check player collision
            var player = Scene.Tracker.GetEntity<Player>();
            if (player != null && Vector2.Distance(Position, player.Position) < 12f)
            {
                player.Die(Vector2.Zero);
            }
            
            if (lifetime <= 0f)
            {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            float alpha = lifetime / maxLifetime;
            Draw.Circle(Position, 10f * scale, color * (alpha * 0.6f), 6);
        }
    }
}

