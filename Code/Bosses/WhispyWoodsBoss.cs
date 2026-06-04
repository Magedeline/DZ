using Celeste.Helpers;

namespace Celeste.Entities
{
    [CustomEntity(ids: "MaggyHelper/WhispyWoodsBoss")]
    [Tracked(true)]
    public partial class WhispyWoodsBoss : BossActor
    {
        // Sprites and visuals
        // Using inherited Sprite from BossEntity
        private VertexLight light;
        private Wiggler scaleWiggler;
        private SoundSource windSfx;
        
        // Boss parameters
        private Level level;
        private Monocle.Circle circle;
        private Vector2[] nodes;
        private int patternIndex;
        private float CameraYPastMax;
        private bool dialog;
        private bool startHit;
        
        // Attack management
        private Coroutine attackCoroutine;
        private bool isAttacking;
        private bool useCustomSequence;
        private List<AttackStep> customAttackSteps;
        private string attackSequenceData;
        
        // Boss state
        private BossState currentState = BossState.Idle;
        private int currentHealth = 10; // Default health
        private int maxHealth = 10;
        private bool isEnraged = false; // Activated at halfway HP
        private float playerHitCooldown = 0f;
        private const float PLAYER_HIT_COOLDOWN_TIME = 1.0f;
        
        private struct AttackStep
        {
            public string Action;
            public float Delay;
            public float Arg;
            public AttackStep(string action, float delay, float arg = 0f)
            {
                Action = action;
                Delay = delay;
                Arg = arg;
            }
        }
        
        public enum BossState
        {
            Idle,
            Attacking,
            Hurt,
            Defeated
        }
        
        public enum AttackType
        {
            // Main 5 attacks
            AppleDrop,
            WindGust,
            RootSpike,
            LeafTornado,
            AirPuff,
            
            // Enraged attacks (halfway HP)
            PoisonAppleBarrage,
            MegaWindstorm
        }
        
        public WhispyWoodsBoss(
            Vector2 position,
            Vector2[] nodes,
            int patternIndex,
            float cameraYPastMax,
            bool dialog,
            bool startHit,
            bool cameraLockY,
            string attackSequence = "")
            : base(
                position,
                "whispy_woods_boss",
                Vector2.One,
                maxFall: 0f,
                collidable: true,
                solidCollidable: false,
                gravityMult: 0f,
                collider: new Hitbox(64f, 80f, -32f, -80f)
            )
        {
            this.patternIndex = patternIndex;
            this.CameraYPastMax = cameraYPastMax;
            this.dialog = dialog;
            this.startHit = startHit;
            this.attackSequenceData = attackSequence;
            
            this.Add((Component)(this.light = new VertexLight(new Color(0.4f, 0.8f, 0.3f), 1f, 48, 96)));
            this.Collider = (Collider)(this.circle = new Monocle.Circle(24f, y: -32f));
            this.Add((Component)new PlayerCollider(new Action<global::Celeste.Player>(this.OnPlayer)));
            
            this.nodes = new Vector2[nodes.Length + 1];
            this.nodes[0] = this.Position;
            for (int index = 0; index < nodes.Length; ++index)
                this.nodes[index + 1] = nodes[index];
                
            this.attackCoroutine = new Coroutine(false);
            this.Add((Component)this.attackCoroutine);
            
            this.Add((Component)new CameraLocker(cameraLockY ? Level.CameraLockModes.FinalBoss : Level.CameraLockModes.FinalBossNoY, 140f, cameraYPastMax));
            this.Add((Component)(this.scaleWiggler = Wiggler.Create(0.6f, 3f)));
            this.Add((Component)(this.windSfx = new SoundSource()));
            
            Depth = -8500;
        }
        
        public WhispyWoodsBoss(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.NodesOffset(offset), e.Int(nameof(patternIndex)),
                  e.Float("cameraPastY", 120f), e.Bool(nameof(dialog)), e.Bool(nameof(startHit)),
                  e.Bool("cameraLockY", true), e.Attr("attackSequence", ""))
        {
            // Parse custom attack sequence
            string seq = attackSequenceData.Trim();
            if (!string.IsNullOrEmpty(seq))
            {
                useCustomSequence = true;
                customAttackSteps = parseCustomAttackSequence(seq);
            }
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.level = this.SceneAs<Level>();
            this.createBossSprite();
            
            if (this.patternIndex == 0 && this.dialog)
            {
                // Wait for intro dialog
                this.currentState = BossState.Idle;
            }
            else if (this.startHit)
            {
                Alarm.Set((Entity)this, 0.5f, (Action)(() => this.OnPlayer((global::Celeste.Player)null)));
            }
            
            this.light.Position = this.Sprite.Position + new Vector2(0.0f, -32f);
        }
        
        private void createBossSprite()
        {
            // Sprite is already created in BossActor constructor
            // Just configure animations if they exist
            if (this.Sprite != null && !this.Sprite.Has("idle"))
            {
                // Add fallback animations
                // This.Sprite.Play("idle");
            }
            else if (this.Sprite != null)
            {
                this.Sprite.Play("idle");
            }
        }
        
        private void OnPlayer(global::Celeste.Player player)
        {
            if (currentState == BossState.Defeated) return;
            
            if (player != null && playerHitCooldown <= 0f)
            {
                player.Die((player.Position - Position).SafeNormalize());
                playerHitCooldown = PLAYER_HIT_COOLDOWN_TIME;
            }
        }
        
        public override void Update()
        {
            base.Update();
            
            if (playerHitCooldown > 0f)
                playerHitCooldown -= Engine.DeltaTime;
            
            // Update sprite position
            if (this.Sprite != null)
            {
                float wiggle = this.scaleWiggler != null ? this.scaleWiggler.Value : 0f;
                this.Sprite.Scale = Vector2.One * (1f + wiggle * 0.15f);
            }
            
            // Start attacking if not already
            if (currentState == BossState.Idle && patternIndex > 0)
            {
                startAttacking();
            }
        }
        
        public override void Render()
        {
            base.Render();
        }
        
        public override void Removed(Scene scene)
        {
            stopAttacking();
            base.Removed(scene);
        }
        
        private void SetState(BossState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            OnStateChanged(newState);
        }
        
        private void OnStateChanged(BossState newState)
        {
            switch (newState)
            {
                case BossState.Defeated:
                    stopAttacking();
                    Collidable = false;
                    break;
                case BossState.Hurt:
                    stopAttacking();
                    scaleWiggler.Start();
                    break;
                case BossState.Attacking:
                    startAttacking();
                    break;
            }
        }
        
        private void startAttacking()
        {
            if (attackCoroutine != null && attackCoroutine.Active) return;
            if (currentState == BossState.Defeated) return;
            
            isAttacking = true;
            SetState(BossState.Attacking);
            attackCoroutine = new Coroutine(attackSequence());
            Add(attackCoroutine);
        }
        
        private void stopAttacking()
        {
            isAttacking = false;
            if (attackCoroutine != null)
                attackCoroutine.Cancel();
        }
        
        private void takeDamage(int damage)
        {
            currentHealth = Math.Max(0, currentHealth - damage);
            
            // Check if boss should become enraged (halfway HP)
            if (!isEnraged && currentHealth <= maxHealth / 2)
            {
                isEnraged = true;
                OnEnraged();
            }
            
            if (currentHealth > 0)
            {
                SetState(BossState.Hurt);
                Add(new Coroutine(hurtStateTimer()));
            }
            else
            {
                SetState(BossState.Defeated);
                Add(new Coroutine(defeatSequence()));
            }
        }
        
        private void OnEnraged()
        {
            // Visual effect for enraged state
            if (Sprite != null && Sprite.Has("enrage"))
                Sprite.Play("enrage");
            
            scaleWiggler.Start();
            Audio.Play("guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}", Position);
        }
        
        private IEnumerator hurtStateTimer()
        {
            yield return 0.5f;
            if (currentState == BossState.Hurt)
                SetState(BossState.Attacking);
        }
        
        private IEnumerator defeatSequence()
        {
            if (Sprite != null && Sprite.Has("defeated"))
                Sprite.Play("defeated");
                
            Audio.Play("event:/game/general/fall_spike", Position);
            
            yield return 2.0f;
            
            // Set completion flag
            level.Session.SetFlag("whispy_woods_defeated", true);
            
            RemoveSelf();
        }
        
        // ====== ATTACK SEQUENCE ======
        
        private IEnumerator attackSequence()
        {
            while (currentState == BossState.Attacking && currentHealth > 0)
            {
                // Use custom sequence if provided
                if (useCustomSequence && customAttackSteps != null)
                {
                    foreach (var step in customAttackSteps)
                    {
                        if (currentState != BossState.Attacking) yield break;
                        
                        switch (step.Action.ToLower())
                        {
                            case "apple":
                                yield return AppleDropAttack();
                                break;
                            case "wind":
                                yield return WindGustAttack();
                                break;
                            case "root":
                                yield return RootSpikeAttack();
                                break;
                            case "leaf":
                                yield return LeafTornadoAttack();
                                break;
                            case "puff":
                                yield return AirPuffAttack();
                                break;
                            case "poisonapple":
                                if (isEnraged) yield return PoisonAppleBarrageAttack();
                                break;
                            case "megawind":
                                if (isEnraged) yield return MegaWindstormAttack();
                                break;
                            default:
                                break;
                        }
                        
                        if (step.Delay > 0f)
                            yield return step.Delay;
                    }
                }
                else
                {
                    // Default attack pattern based on patternIndex
                    yield return DefaultAttackPattern();
                }
                
                yield return 0.5f; // Brief pause between cycles
            }
        }
        
        private IEnumerator DefaultAttackPattern()
        {
            // Normal phase: cycle through 5 main attacks
            if (!isEnraged)
            {
                yield return AppleDropAttack();
                yield return 1.0f;
                
                yield return WindGustAttack();
                yield return 1.0f;
                
                yield return RootSpikeAttack();
                yield return 1.0f;
                
                yield return LeafTornadoAttack();
                yield return 1.0f;
                
                yield return AirPuffAttack();
                yield return 1.5f;
            }
            else
            {
                // Enraged phase: add 2 powerful attacks
                yield return AppleDropAttack();
                yield return 0.8f;
                
                yield return PoisonAppleBarrageAttack();
                yield return 1.0f;
                
                yield return WindGustAttack();
                yield return 0.8f;
                
                yield return MegaWindstormAttack();
                yield return 1.0f;
                
                yield return RootSpikeAttack();
                yield return 0.8f;
                
                yield return LeafTornadoAttack();
                yield return 0.8f;
                
                yield return AirPuffAttack();
                yield return 1.0f;
            }
        }
        
        // ====== ATTACK IMPLEMENTATIONS ======
        
        // Attack 1: Apple Drop - Drops apples from above
        private IEnumerator AppleDropAttack()
        {
            yield return TelegraphIntent(BossTelegraphType.PositioningOrange);

            if (Sprite != null && Sprite.Has("attack_apple"))
                Sprite.Play("attack_apple");
            
            // Charge phase
            yield return 0.5f;
            
            // Drop 3-5 apples
            int appleCount = isEnraged ? 5 : 3;
            for (int i = 0; i < appleCount; i++)
            {
                Vector2 applePos = Position + new Vector2(
                    Calc.Random.Range(-80f, 80f),
                    -100f
                );
                
                // TODO: Implement WhispyApple projectile entity with gravity and collision
                // Future: Scene.Add(new WhispyApple(applePos));
                Audio.Play("guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}", Position);
                
                yield return 0.3f;
            }
            
            // Recovery
            yield return 0.5f;
        }
        
        // Attack 2: Wind Gust - Pushes player with wind
        private IEnumerator WindGustAttack()
        {
            yield return TelegraphIntent(BossTelegraphType.PositioningOrange);

            if (Sprite != null && Sprite.Has("attack_wind"))
                Sprite.Play("attack_wind");
            
            // Charge wind
            windSfx.Play("event:/env/local/02_old_site_steam");
            yield return 0.8f;
            
            // Execute wind push
            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null && player.X < Position.X + 200f)
            {
                Vector2 windForce = new Vector2(isEnraged ? -400f : -250f, 0f);
                player.Speed += windForce * Engine.DeltaTime * 60f;
            }
            
            yield return isEnraged ? 2.5f : 2.0f;
            windSfx.Stop();
            
            // Recovery
            yield return 0.5f;
        }
        
        // Attack 3: Root Spike - Ground spikes emerge
        private IEnumerator RootSpikeAttack()
        {
            yield return TelegraphIntent(BossTelegraphType.PositioningOrange);

            if (Sprite != null && Sprite.Has("attack_root"))
                Sprite.Play("attack_root");
            
            // Warning phase
            yield return 0.6f;
            
            // Spawn root spikes
            int spikeCount = isEnraged ? 6 : 4;
            for (int i = 0; i < spikeCount; i++)
            {
                Vector2 spikePos = Position + new Vector2(
                    Calc.Random.Range(-120f, 120f),
                    60f
                );
                
                // TODO: Implement WhispyRootSpike entity with ground emergence animation
                // Future: Scene.Add(new WhispyRootSpike(spikePos));
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_pop", Position);
                
                yield return 0.4f;
            }
            
            // Recovery
            yield return 0.7f;
        }
        
        // Attack 4: Leaf Tornado - Spinning leaf projectiles
        private IEnumerator LeafTornadoAttack()
        {
            yield return TelegraphIntent(BossTelegraphType.DangerRed);

            if (Sprite != null && Sprite.Has("attack_leaf"))
                Sprite.Play("attack_leaf");
            
            // Charge
            yield return 0.5f;
            
            // Shoot leaves in pattern
            int leafWaves = isEnraged ? 4 : 3;
            for (int wave = 0; wave < leafWaves; wave++)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = (float)(i * Math.PI * 2 / 5) + wave * 0.5f;
                    Vector2 direction = Calc.AngleToVector(angle, 1f);
                    
                    // TODO: Implement WhispyLeaf projectile entity with rotation
                    // For now, skip projectile spawning
                    // Future: Scene.Add(new WhispyLeaf(Position, direction));
                }
                
                Audio.Play("guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}", Position);
                yield return 0.6f;
            }
            
            // Recovery
            yield return 0.5f;
        }
        
        // Attack 5: Air Puff - Short range knockback
        private IEnumerator AirPuffAttack()
        {
            yield return TelegraphIntent(BossTelegraphType.DangerRed);

            if (Sprite != null && Sprite.Has("attack_puff"))
                Sprite.Play("attack_puff");
            
            // Charge
            yield return 0.4f;
            
            // Release puff
            global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                float distance = Vector2.Distance(player.Position, Position);
                if (distance < (isEnraged ? 150f : 100f))
                {
                    Vector2 knockback = (player.Position - Position).SafeNormalize() * (isEnraged ? 300f : 200f);
                    player.Speed = knockback;
                }
            }
            
            Audio.Play("guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}", Position);
            scaleWiggler.Start();
            
            // Recovery
            yield return 0.6f;
        }
        
        // ====== ENRAGED ATTACKS (Halfway HP) ======
        
        // Enraged Attack 1: Poison Apple Barrage - Many poison apples
        private IEnumerator PoisonAppleBarrageAttack()
        {
            yield return TelegraphIntent(BossTelegraphType.SpecialPurple, 0.6f);

            if (Sprite != null && Sprite.Has("attack_poison"))
                Sprite.Play("attack_poison");
            
            // Charge
            yield return 0.7f;
            
            // Massive apple barrage
            for (int wave = 0; wave < 3; wave++)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 applePos = Position + new Vector2(
                        Calc.Random.Range(-150f, 150f),
                        -120f - wave * 40f
                    );
                    
                    // TODO: Implement WhispyPoisonApple entity with poison/damage over time effect
                    // Future: Scene.Add(new WhispyPoisonApple(applePos));
                    Audio.Play("guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}", Position);
                }
                
                yield return 0.5f;
            }
            
            // Recovery
            yield return 1.0f;
        }
        
        // Enraged Attack 2: Mega Windstorm - Powerful sustained wind
        private IEnumerator MegaWindstormAttack()
        {
            yield return TelegraphIntent(BossTelegraphType.SpecialPurple, 0.6f);

            if (Sprite != null && Sprite.Has("attack_megawind"))
                Sprite.Play("attack_megawind");
            
            // Warning
            yield return 0.5f;
            
            // Sustained windstorm
            windSfx.Play("event:/env/local/09_core_rising");
            float duration = 4.0f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                global::Celeste.Player player = Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (player != null)
                {
                    Vector2 windForce = new Vector2(-600f, 0f);
                    player.Speed += windForce * Engine.DeltaTime;
                    
                    // Add turbulence
                    player.Speed.Y += Calc.Random.Range(-50f, 50f) * Engine.DeltaTime;
                }
                
                elapsed += Engine.DeltaTime;
                yield return null;
            }
            
            windSfx.Stop();
            
            // Recovery
            yield return 0.8f;
        }
        
        // ====== HELPER METHODS ======
        
        private List<AttackStep> parseCustomAttackSequence(string sequence)
        {
            List<AttackStep> steps = new List<AttackStep>();
            string[] parts = sequence.Split(';');
            
            foreach (string part in parts)
            {
                string[] tokens = part.Trim().Split(',');
                if (tokens.Length >= 2)
                {
                    string action = tokens[0].Trim();
                    float delay = float.Parse(tokens[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    float arg = tokens.Length > 2 ? float.Parse(tokens[2].Trim(), System.Globalization.CultureInfo.InvariantCulture) : 0f;
                    
                    steps.Add(new AttackStep(action, delay, arg));
                }
            }
            
            return steps;
        }
    }
}

