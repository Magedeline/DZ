using StateMachine = Monocle.StateMachine;

namespace MaggyHelper.Entities
{
    [CustomEntity(ids: "MaggyHelper/Boss")]
    [Tracked]
    [HotReloadable]
    public class Boss : Actor
    {
        public enum BossPhase
        {
            Intro,
            Phase1,
            Phase2,
            Phase3,
            Transition,
            Defeated
        }

        public enum BossTier
        {
            Lowest = 1,
            Low = 2,
            Mid = 3,
            High = 4,
            Highest = 5,
            Final = 6 // For the blackhole angel final boss
        }

        public enum GimmickAbility
        {
            None,
            Teleport,
            TimeFreeze,
            ShieldBreaker,
            ElementalFusion,
            GravityControl,
            DimensionRift
        }
        
        public string BossType { get; protected set; }
        public BossPhase CurrentPhase { get; private set; }
        public BossTier Tier { get; protected set; }
        public GimmickAbility Gimmick { get; protected set; }
        public bool IsDefeated { get; private set; }
        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public float Speed { get; set; } = 30f;
        public bool IsInvulnerable { get; set; }
        public bool IsInGimmickMode { get; private set; }
        
    protected Sprite sprite;
    private StateMachine stateMachine;
    private global::Celeste.Player targetPlayer;
    private Vector2 arenaCenter;

    protected global::Celeste.Player TargetPlayer => targetPlayer;
    protected Vector2 ArenaCenter => arenaCenter;
        protected float arenaRadius = 200f;
        private bool facingRight = true;
        private float attackTimer = 0f;
        private float attackCooldown = 2f;
        private float gimmickCooldown = 0f;
        private Random random = new Random();
        private TimeRateModifier timeRateModifier;
        
        // Boss-specific properties
        private int phaseTransitionHealth;
        private bool hasStartedIntro = false;
        
        private int calculateHealthByTier(BossTier tier, int baseHealth)
        {
            if (baseHealth > 0) return baseHealth; // Use custom health if specified
            
            return tier switch
            {
                BossTier.Lowest => 100,
                BossTier.Low => 200,
                BossTier.Mid => 350,
                BossTier.High => 500,
                BossTier.Highest => 750,
                BossTier.Final => 1000,
                _ => 100
            };
        }
        
        private float calculateSpeedByTier(BossTier tier)
        {
            return tier switch
            {
                BossTier.Lowest => 20f,
                BossTier.Low => 30f,
                BossTier.Mid => 45f,
                BossTier.High => 60f,
                BossTier.Highest => 80f,
                BossTier.Final => 100f,
                _ => 30f
            };
        }
        
        private float getGimmickCooldownByTier()
        {
            return Tier switch
            {
                BossTier.Lowest => 8f,
                BossTier.Low => 6f,
                BossTier.Mid => 5f,
                BossTier.High => 4f,
                BossTier.Highest => 3f,
                BossTier.Final => 2f,
                _ => 6f
            };
        }
        
        public Boss(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            arenaCenter = Position;
            
            // Read properties from EntityData
            BossType = data.Attr("bossType", "Generic");
            Tier = (BossTier)data.Int("tier", 1);
            Gimmick = (GimmickAbility)data.Int("gimmick", 0);
            MaxHealth = calculateHealthByTier(Tier, data.Int("health", 0));
            Health = MaxHealth;
            Speed = data.Float("speed", calculateSpeedByTier(Tier));
            arenaRadius = data.Float(nameof(arenaRadius), 200f);
            attackCooldown = data.Float(nameof(attackCooldown), 2f);
            
            phaseTransitionHealth = MaxHealth / 3; // Transition every 1/3 health
            
            // Setup collider - bosses are typically larger
            Collider = new Hitbox(32f, 32f, -16f, -16f);
            
            // Setup sprite
            setupSprite();
            
            // Setup state machine
            setupStateMachine();

            Add(timeRateModifier = new TimeRateModifier(1f, false));
            
            Depth = 10; // Render above most entities
        }
        
        private void setupSprite()
        {
            string spriteId = BossType.ToLower() switch
            {
                "chara" => "boss_chara",
                "flowey" => "boss_flowey",
                "final" => "boss_final",
                _ => "boss_generic"
            };
            
            try
            {
                Add(sprite = GFX.SpriteBank.Create(spriteId));
                sprite.Play("idle");
            }
            catch
            {
                // Fallback if sprite doesn't exist
                Add(sprite = GFX.SpriteBank.Create("theo"));
                sprite.Play("idle");
                sprite.Scale = Vector2.One * 2f; // Make it bigger for boss
            }
        }
        
        private void setupStateMachine()
        {
            Add(stateMachine = new StateMachine());
            stateMachine.SetCallbacks(0, stateIntro, null, beginIntro, endIntro);
            stateMachine.SetCallbacks(1, statePhase1, null, beginPhase1, null);
            stateMachine.SetCallbacks(2, statePhase2, null, beginPhase2, null);
            stateMachine.SetCallbacks(3, statePhase3, null, beginPhase3, null);
            stateMachine.SetCallbacks(4, stateTransition, null, beginTransition, endTransition);
            stateMachine.SetCallbacks(5, stateDefeated, null, beginDefeated, null);
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            // Start with intro phase
            setPhase(BossPhase.Intro);
        }
        
        public override void Update()
        {
            base.Update();
            
            // Find target player
            if (targetPlayer == null || targetPlayer.Scene != Scene)
            {
                targetPlayer = Scene.Tracker.GetEntity<global::Celeste.Player>();
            }
            
            // Update attack timer
            if (attackTimer > 0f)
            {
                attackTimer -= Engine.DeltaTime;
            }
            
            // Update gimmick cooldown
            if (gimmickCooldown > 0f)
            {
                gimmickCooldown -= Engine.DeltaTime;
            }
            
            // Try to use gimmick abilities
            if (Gimmick != GimmickAbility.None && gimmickCooldown <= 0f && !IsInGimmickMode)
            {
                tryUseGimmickAbility();
            }
            
            // Check for phase transitions based on health
            checkPhaseTransitions();
        }
        
        private void checkPhaseTransitions()
        {
            if (IsDefeated || CurrentPhase == BossPhase.Transition) return;
            
            int healthPercentage = (Health * 100) / MaxHealth;
            
            switch (CurrentPhase)
            {
                case BossPhase.Phase1 when healthPercentage <= 66:
                    setPhase(BossPhase.Transition);
                    break;
                case BossPhase.Phase2 when healthPercentage <= 33:
                    setPhase(BossPhase.Transition);
                    break;
                case BossPhase.Phase3 when Health <= 0:
                    setPhase(BossPhase.Defeated);
                    break;
            }
        }
        
        private void setPhase(BossPhase newPhase)
        {
            CurrentPhase = newPhase;
            stateMachine.State = (int)newPhase;
        }
        
        #region State Methods
        
        private int stateIntro()
        {
            if (!hasStartedIntro)
            {
                hasStartedIntro = true;
                // Trigger intro cutscene or dialogue
                triggerIntroCutscene();
            }
            
            // Intro automatically transitions to Phase1 after completion
            return (int)BossPhase.Phase1;
        }
        
        private void beginIntro()
        {
            sprite.Play("intro");
            IsInvulnerable = true;
        }
        
        private void endIntro()
        {
            IsInvulnerable = false;
        }
        
        private int statePhase1()
        {
            executePhaseAi(1);
            return (int)CurrentPhase;
        }
        
        private void beginPhase1()
        {
            sprite.Play("phase1");
            attackCooldown = 2f;
        }
        
        private int statePhase2()
        {
            executePhaseAi(2);
            return (int)CurrentPhase;
        }
        
        private void beginPhase2()
        {
            sprite.Play("phase2");
            attackCooldown = 1.5f; // Faster attacks
        }
        
        private int statePhase3()
        {
            executePhaseAi(3);
            return (int)CurrentPhase;
        }
        
        private void beginPhase3()
        {
            sprite.Play("phase3");
            attackCooldown = 1f; // Even faster attacks
        }
        
        private int stateTransition()
        {
            // Handle phase transition animations/effects
            return (int)getNextPhase();
        }
        
        private void beginTransition()
        {
            sprite.Play("transition");
            IsInvulnerable = true;
        }
        
        private void endTransition()
        {
            IsInvulnerable = false;
        }
        
        private int stateDefeated()
        {
            return (int)CurrentPhase;
        }
        
        protected virtual void beginDefeated()
        {
            sprite.Play("defeated");
            IsDefeated = true;
            IsInvulnerable = true;
            
            // Trigger defeat cutscene or effects
            triggerDefeatCutscene();
        }
        
        #endregion
        
        private void executePhaseAi(int phase)
        {
            if (targetPlayer == null || attackTimer > 0f) return;
            
            // Face the player
            facingRight = targetPlayer.Position.X > Position.X;
            
            // Different AI patterns per phase
            switch (phase)
            {
                case 1:
                    executeBasicAttackPattern();
                    break;
                case 2:
                    executeAdvancedAttackPattern();
                    break;
                case 3:
                    executeDespiteAttackPattern();
                    break;
            }
        }
        
        private void executeBasicAttackPattern()
        {
            int attackType = random.Next(0, 3);
            
            switch (attackType)
            {
                case 0:
                    performChargeAttack();
                    break;
                case 1:
                    performProjectileAttack();
                    break;
                case 2:
                    performAreaAttack();
                    break;
            }
            
            attackTimer = attackCooldown;
        }
        
        private void executeAdvancedAttackPattern()
        {
            // More complex patterns for phase 2
            executeBasicAttackPattern();
            
            // Add additional effects
            if (random.NextFloat() < 0.3f)
            {
                performSpecialAttack();
            }
        }
        
        private void executeDespiteAttackPattern()
        {
            // Most aggressive patterns for phase 3
            executeAdvancedAttackPattern();
            
            // Even more frequent special attacks
            if (random.NextFloat() < 0.5f)
            {
                performUltimateAttack();
            }
        }
        
        private BossPhase getNextPhase()
        {
            return CurrentPhase switch
            {
                BossPhase.Intro => BossPhase.Phase1,
                BossPhase.Phase1 => BossPhase.Phase2,
                BossPhase.Phase2 => BossPhase.Phase3,
                BossPhase.Transition => getNextPhaseAfterTransition(),
                _ => CurrentPhase
            };
        }
        
        private BossPhase getNextPhaseAfterTransition()
        {
            int healthPercentage = (Health * 100) / MaxHealth;
            
            if (healthPercentage > 66) return BossPhase.Phase1;
            if (healthPercentage > 33) return BossPhase.Phase2;
            if (healthPercentage > 0) return BossPhase.Phase3;
            return BossPhase.Defeated;
        }
        
        #region Attack Methods
        
        private void performChargeAttack()
        {
            if (targetPlayer == null) return;
            
            Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
            MoveH(direction.X * Speed * 2f * Engine.DeltaTime);
            
            sprite.Play("charge");
        }
        
        private void performProjectileAttack()
        {
            if (targetPlayer == null) return;
            
            // Create projectile entity
            Vector2 direction = (targetPlayer.Position - Position).SafeNormalize();
            // Scene.Add(new BossProjectile(Position, direction));
            
            sprite.Play("ranged_attack");
        }
        
        private void performAreaAttack()
        {
            // Create area of effect around boss
            sprite.Play("area_attack");
        }
        
        private void performSpecialAttack()
        {
            // Phase 2+ special attack
            sprite.Play("special_attack");
        }
        
        private void performUltimateAttack()
        {
            // Phase 3 ultimate attack
            sprite.Play("ultimate_attack");
        }
        
        #endregion
        
        #region Gimmick Abilities
        
        private void tryUseGimmickAbility()
        {
            if (targetPlayer == null || random.NextFloat() > 0.3f) return;
            
            IsInGimmickMode = true;
            gimmickCooldown = getGimmickCooldownByTier();
            
            switch (Gimmick)
            {
                case GimmickAbility.Teleport:
                    performTeleportAbility();
                    break;
                case GimmickAbility.TimeFreeze:
                    performTimeFreezeAbility();
                    break;
                case GimmickAbility.ShieldBreaker:
                    performShieldBreakerAbility();
                    break;
                case GimmickAbility.ElementalFusion:
                    performElementalFusionAbility();
                    break;
                case GimmickAbility.GravityControl:
                    performGravityControlAbility();
                    break;
                case GimmickAbility.DimensionRift:
                    performDimensionRiftAbility();
                    break;
            }
            
            // Schedule end of gimmick mode
            Add(new Coroutine(endGimmickModeAfterDelay()));
        }
        
        private void performTeleportAbility()
        {
            // Teleport behind player
            Vector2 newPos = targetPlayer.Position + Vector2.UnitX * (facingRight ? -80f : 80f);
            Position = newPos;
            sprite.Play("teleport");
            Audio.Play("event:/teleport_ability", Position);
        }
        
        private void performTimeFreezeAbility()
        {
            // Temporarily slow down time for player
            timeRateModifier.SetTimeRateMultiplier(0.3f);
            sprite.Play("time_freeze");
            Audio.Play("event:/time_freeze_ability", Position);
        }
        
        private void performShieldBreakerAbility()
        {
            // Ignore player's defensive abilities for a short time
            sprite.Play("shield_breaker");
            Audio.Play("event:/shield_breaker_ability", Position);
        }
        
        private void performElementalFusionAbility()
        {
            // Combine multiple element attacks
            sprite.Play("elemental_fusion");
            Audio.Play("event:/elemental_fusion_ability", Position);
        }
        
        private void performGravityControlAbility()
        {
            // Manipulate gravity in the arena
            sprite.Play("gravity_control");
            Audio.Play("event:/gravity_control_ability", Position);
        }
        
        private void performDimensionRiftAbility()
        {
            // Create portals for attacks or movement
            sprite.Play("dimension_rift");
            Audio.Play("event:/dimension_rift_ability", Position);
        }
        
        private IEnumerator endGimmickModeAfterDelay()
        {
            yield return 3f; // Gimmick lasts 3 seconds
            IsInGimmickMode = false;
            
            // Reset time rate if it was modified
            if (Gimmick == GimmickAbility.TimeFreeze)
            {
                timeRateModifier.ResetTimeRateMultiplier();
            }
        }
        
        #endregion
        
        public virtual void TakeDamage(int damage)
        {
            if (IsInvulnerable || IsDefeated) return;
            
            Health -= damage;
            
            if (Health <= 0)
            {
                Health = 0;
                setPhase(BossPhase.Defeated);
            }
            
            // Visual feedback for taking damage
            sprite.Color = Color.Red;
            Tween.Set(this, Tween.TweenMode.Oneshot, 0.1f, Ease.Linear, 
                t => sprite.Color = Color.Lerp(Color.Red, Color.White, t.Percent));
        }
        
        private void triggerIntroCutscene()
        {
            // Trigger boss intro cutscene based on boss type
            string cutsceneId = $"boss_intro_{BossType.ToLower()}";
            // Scene.Add(new CutsceneTrigger(cutsceneId, targetPlayer));
        }
        
        private void triggerDefeatCutscene()
        {
            // Trigger boss defeat cutscene
            string cutsceneId = $"boss_defeat_{BossType.ToLower()}";
            // Scene.Add(new CutsceneTrigger(cutsceneId, targetPlayer));
            
            // Mark boss as defeated in the manager
            if (Scene is Level level)
            {
                var bossData = new EnemyBossManager.BossData
                {
                    BossType = BossType,
                    Position = Position,
                    Defeated = true
                };
                EnemyBossManager.RegisterRoomBoss(level.Session.Level, bossData);
            }
        }
        
        public override void Render()
        {
            if (sprite != null)
            {
                sprite.Scale.X = Math.Abs(sprite.Scale.X) * (facingRight ? 1f : -1f);
            }
            base.Render();
        }
    }
}



