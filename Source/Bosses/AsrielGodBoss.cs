using System.Globalization;
using Celeste.Helpers;
using Celeste.Cutscenes;
using Celeste.Entities.Effects;
using Celeste.Entities.Projectiles;

namespace Celeste.Entities
{
    /// <summary>
    /// AsrielGodBoss - A ConqueredPeak/BadelineBoss style boss with pattern-based attacks.
    /// Uses node-based movement and phase transitions similar to Celeste's FinalBoss.
    /// </summary>
    [CustomEntity(ids: "DZ/AsrielGodBoss")]
    [Tracked(true)]
    [HotReloadable]
    public partial class AsrielGodBoss : BossActor
    {
        #region Constants and Audio Events
        
        // Audio Events
        private const string SFX_BARRIER_SHATTER = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_BarrierShatter";
        private const string SFX_BIG_BULLET_FIRE = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_Big_Bullet_Fire";
        private const string SFX_BIGGER_GUN_FIRE = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_Bigger_Gun_Fire";
        private const string SFX_BIGGER_LIGHTNING_HIT = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_Bigger_Lightninghit";
        private const string SFX_BIGGER_GUN_MECHANIZED = "event:/Mods/pusheen/new_content/char/bosses/asriel/biggergunmechanized";
        private const string SFX_CINEMATIC_CUT = "event:/Mods/pusheen/new_content/char/bosses/asriel/cinematiccut";
        private const string SFX_GRAB = "event:/Mods/pusheen/new_content/char/bosses/asriel/grab";
        private const string SFX_GUNSHOT = "event:/Mods/pusheen/new_content/char/bosses/asriel/gunshot";
        private const string SFX_HYPERGONER_CHARGE = "event:/Mods/pusheen/new_content/char/bosses/asriel/hypergoner_charge";
        private const string SFX_LIGHTNING_HIT = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_Lightninghit";
        private const string SFX_SEGA_POWER_01 = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_Segapower01";
        private const string SFX_SEGA_POWER_02 = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_Segapower02";
        private const string SFX_SPARKLES = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_Sparkles";
        private const string SFX_SPELLCAST_GLITCH = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_Spellcast_Glitch";
        private const string SFX_STAR = "event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_Star";
        private const string SFX_BOSS_HIT = "event:/char/badeline/boss_hug";
        private const string SFX_BOSS_LASER_CHARGE = "event:/char/badeline/boss_laser_charge";
        private const string SFX_BOSS_LASER_FIRE = "event:/char/badeline/boss_laser_fire";
        private const string SFX_BOSS_BULLET = "event:/char/badeline/boss_bullet";
        
        // Dialogue Events
        private const string DIALOGUE_ASRIEL = "event:/char/dialogue/asriel";
        private const string DIALOGUE_ASRIEL_GIGA = "event:/char/dialogue/asriel_giga";
        private const string DIALOGUE_ASRIEL_HG = "event:/char/dialogue/asriel_hg";
        private const string DIALOGUE_ASRIEL_ZERO = "event:/char/dialogue/asriel_zero";
        
        // ConqueredPeak/BadelineBoss constants
        public const float CAMERA_X_PAST_MAX = 140f;
        private const float MOVE_SPEED = 600f;
        private const float AVOID_RADIUS = 12f;
        private const float HIT_INVULNERABILITY_TIME = 1.5f;
        
        // HP/Phase System (from V2)
        private const int MAX_HITS_PER_PHASE = 3;  // Hits needed to progress to next phase
        private const int TOTAL_PHASES = 5;        // Total number of boss phases
        private const int MAX_HITS = MAX_HITS_PER_PHASE * TOTAL_PHASES; // Total hits to defeat boss (15)
        
        #endregion
        
        #region Particle Types
        
        public static ParticleType PBurst;
        public static ParticleType PShoot;
        
        static AsrielGodBoss()
        {
            // Initialize particle type for burst effect
            PBurst = new ParticleType
            {
                Color = Color.Cyan,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.4f,
                LifeMax = 0.8f,
                Size = 1f,
                SizeRange = 0.5f,
                DirectionRange = (float)Math.PI / 4f,
                SpeedMin = 40f,
                SpeedMax = 80f,
                SpeedMultiplier = 0.2f,
                Acceleration = new Vector2(0f, 60f)
            };
            
            // Shooting particle type (cyan/rainbow, no orange)
            PShoot = new ParticleType
            {
                Color = Color.Cyan,
                Color2 = Color.White,
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                Size = 1f,
                SpeedMin = 100f,
                SpeedMax = 200f
            };
        }
        
        #endregion
        
        #region Fields
        
        // Core components
        public global::Celeste.PlayerSprite NormalSprite;
        private PlayerHair normalHair;
        private Level level;
        private Monocle.Circle circle;
        private VertexLight light;
        private Wiggler scaleWiggler;
        private AsrielGodBossStarfield bossBg;
        private SoundSource chargeSfx;
        private SoundSource laserSfx;
        
        // Movement and position
        private Vector2 avoidPos;
        public float CameraYPastMax;
        public bool Moving;
        public bool Sitting;
        private int facing;
        private Vector2[] nodes;
        private int nodeIndex;
        
        // Pattern and attack system (ConqueredPeak style)
        private int patternIndex;
        private Coroutine attackCoroutine;
        private Coroutine triggerBlocksCoroutine;
        private bool playerHasMoved;
        private SineWave floatSine;
        private bool dialog;
        private bool startHit;
        private bool isAttacking;
        private bool canHit = true;
        
        // Phase management (BadelineBoss style) - HP System from V2
        private int currentPhase = 0;
        private int hitsInPhase = 0;              // Hits taken in current phase
#pragma warning disable CS0414
        private bool phaseTransitioning = false;
#pragma warning restore CS0414
        private int totalHitsTaken = 0;           // Total hits taken across all phases
#pragma warning disable CS0414
        private bool isPlayerInvulnerable = false;
#pragma warning restore CS0414
        
        // Blocks
        private List<Entity> fallingBlocks;
        private List<Entity> movingBlocks;
        
        // Custom attack sequence
        private bool useCustomSequence;
        private List<AttackStep> customAttackSteps;
        private string attackSequenceData;

        #endregion
        
        #region AttackStep Struct
        
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
        
        #endregion
        
        #region Attack Pattern System (ConqueredPeak Style)
        
        /// <summary>
        /// Attack pattern class for defining attack properties
        /// </summary>
        private class AttackPattern
        {
            public float ChargeTime { get; set; }
            public float Duration { get; set; }
            public float CooldownTime { get; set; }
            public Action<AsrielGodBoss> Execute { get; set; }
        }

        private Dictionary<AttackType, AttackPattern> attackPatterns;

        private void InitializeAttackPatterns()
        {
            attackPatterns = new Dictionary<AttackType, AttackPattern>
            {
                {
                    AttackType.ChaosBlaster,
                    new AttackPattern
                    {
                        ChargeTime = 0.5f,
                        Duration = 3.0f,
                        CooldownTime = 1.0f,
                        Execute = (boss) => ExecuteChaosBlaster()
                    }
                },
                {
                    AttackType.HyperGoner,
                    new AttackPattern
                    {
                        ChargeTime = 1.0f,
                        Duration = 4.0f,
                        CooldownTime = 2.0f,
                        Execute = (boss) => ExecuteHyperGoner()
                    }
                },
                {
                    AttackType.StarstormRain,
                    new AttackPattern
                    {
                        ChargeTime = 0.8f,
                        Duration = 5.0f,
                        CooldownTime = 1.5f,
                        Execute = (boss) => ExecuteStarstormRain()
                    }
                },
                {
                    AttackType.GalacticSaber,
                    new AttackPattern
                    {
                        ChargeTime = 0.3f,
                        Duration = 2.0f,
                        CooldownTime = 1.0f,
                        Execute = (boss) => ExecuteGalacticSaber()
                    }
                },
                {
                    AttackType.DimensionalRift,
                    new AttackPattern
                    {
                        ChargeTime = 1.2f,
                        Duration = 6.0f,
                        CooldownTime = 2.0f,
                        Execute = (boss) => ExecuteDimensionalRift()
                    }
                }
            };
        }

        // Attack execution methods
        private void ExecuteChaosBlaster()
        {
            const int PROJECTILE_COUNT = 8;
            const float SPREAD_ANGLE = 30f;
            
            for (int i = 0; i < PROJECTILE_COUNT; i++)
            {
                float angle = (i - (PROJECTILE_COUNT - 1) / 2f) * (SPREAD_ANGLE / (PROJECTILE_COUNT - 1));
                Vector2 direction = Calc.AngleToVector(angle * Calc.DegToRad, 1f);
                CreateStarProjectile(Position, direction * 300f, Color.Yellow);
            }
        }

        private void ExecuteHyperGoner()
        {
            const float PULL_STRENGTH = 150f;
            const float SAFE_DISTANCE = 40f;
            
            level.Add(new HyperGonerVortex(Position, PULL_STRENGTH, SAFE_DISTANCE));
            Audio.Play(SFX_HYPERGONER_CHARGE, Position);
        }
        
        #endregion

        private void ExecuteStarstormRain()
        {
            const int STARS_PER_WAVE = 5;
            
            for (int i = 0; i < STARS_PER_WAVE; i++)
            {
                float xOffset = (i - (STARS_PER_WAVE - 1) / 2f) * 32f;
                Vector2 spawnPos = new Vector2(Position.X + xOffset, Position.Y - 160f);
                CreateFallingStarProjectile(spawnPos);
            }
        }

        private void ExecuteGalacticSaber()
        {
            const float BLADE_SPEED = 400f;
            const int BLADE_COUNT = 3;
            
            for (int i = 0; i < BLADE_COUNT; i++)
            {
                float angle = Calc.Random.Range(0f, 360f);
                Vector2 direction = Calc.AngleToVector(angle * Calc.DegToRad, 1f);
                CreateEtherealBlade(Position, direction * BLADE_SPEED);
            }
        }

        private void ExecuteDimensionalRift()
        {
            const float RIFT_DURATION = 2f;
            const float RIFT_RADIUS = 32f;
            
            Vector2 targetPos = level.Tracker.GetEntity<global::Celeste.Player>()?.Position ?? Position;
            level.Add(new Effects.DimensionalRift(targetPos, RIFT_RADIUS, RIFT_DURATION));
        }

        // Helper methods for creating projectiles and effects
        private void CreateStarProjectile(Vector2 position, Vector2 velocity, Color color)
        {
            level.Add(new StarProjectile(position, velocity, color));
        }

        private void CreateFallingStarProjectile(Vector2 position)
        {
            level.Add(new FallingStarProjectile(position));
        }

        private void CreateEtherealBlade(Vector2 position, Vector2 velocity)
        {
            level.Add(new EtherealBlade(position, velocity));
        }
        
        #region State and Type Enums
        
        /// <summary>
        /// Boss state machine states
        /// </summary>
        public enum BossState
        {
            Waiting,        // Waiting for player to move
            Idle,           // Idle state between attacks
            Moving,         // Moving to next node
            Attacking,      // Executing attack pattern
            Hit,            // Just got hit by player
            Hurt,           // Hurt/recovery state
            Transitioning,  // Phase transition
            Defeated        // Boss is defeated
        }

        /// <summary>
        /// Attack types available to the boss
        /// </summary>
        public enum AttackType
        {
            Shoot,          // Basic projectile shot
            Beam,           // Laser beam attack
            BiggerBeam,     // Larger beam attack
            BigBeamBall,    // Large energy ball
            RainbowBlackhole,
            BladeThrower,
            FireShockwave,
            StarsMeteorite,
            ChaosBlaster,
            HyperGoner,
            GalacticSaber,
            StarstormRain,
            LightningStorm,
            DimensionalRift,
            RainbowInferno,
            CelestialSpears,
            TimewarpVortex,
            PrismBurst,
            SoulResonance,
            EternalChaos,
            SwordSlash
        }

        /// <summary>
        /// Attack phase for state management
        /// </summary>
        public enum AttackPhase
        {
            Charging,
            Executing,
            Recovery
        }
        
        // Boss state management
        private BossState currentState = BossState.Waiting;
        private AttackType currentAttackType = AttackType.Shoot;
#pragma warning disable CS0414
        private AttackPhase currentAttackPhase = AttackPhase.Charging;
#pragma warning restore CS0414
    private float playerHitCooldown = 0f;
    private float flashTimer = 0f;
        private Vector2 knockbackVelocity = Vector2.Zero;
        private float knockbackTimer = 0f;
    private float mercyTimer = 0f;
    private bool playerMercyActive = false;
        private readonly TimeRateModifier hitTimeRateModifier;
        
        // Sword attack fields
#pragma warning disable CS0414
        private float swordTweenDuration = 0.3f;
#pragma warning restore CS0414
#pragma warning disable CS0414
        private float swordTargetRotation = 0f;
#pragma warning restore CS0414
        // Sword animation fields

        // Music progression
        private string[] musicTracks = {
            "event:/Mods/pusheen/new_content/music/lvl20/kirby_vs_asriel_fight_1",
        };
        
        private const string ASRIEL_REMEMBER_PART_2 = "event:/Mods/pusheen/new_content/music/lvl20/kirby_vs_asriel_fight_1";
        private bool asrielRememberTriggered = false;
        private bool hyperGonerPhaseTransitionTriggered = false;
        
        #endregion
        
        #region Constructors
        
        public AsrielGodBoss(
            Vector2 position,
            Vector2[] nodes,
            int patternIndex,
            float cameraYPastMax,
            bool dialog,
            bool startHit,
            bool cameraLockY,
            string attackSequence = "")
            : base(position,
                   spriteName: "asrielgodboss",  // Must match sprite bank ID in Sprites.xml
                   spriteScale: Vector2.One,
                   maxFall: 160f,
                   collidable: true,
                   solidCollidable: false,
                   gravityMult: 0.0f,  // Asriel floats, no gravity
                   collider: new Monocle.Circle(14f, y: -6f))
        {
            this.patternIndex = patternIndex;
            this.CameraYPastMax = cameraYPastMax;
            this.dialog = dialog;
            this.startHit = startHit;
            this.attackSequenceData = attackSequence;
            this.Add((Component)(this.light = new VertexLight(Color.White, 1f, 32, 64)));
            // Collider already set in base constructor, just store reference
            this.circle = (Monocle.Circle)this.Collider;
            this.Add((Component)new PlayerCollider(new Action<global::Celeste.Player>(this.OnPlayer)));
            this.nodes = new Vector2[nodes.Length + 1];
            this.nodes[0] = this.Position;
            for (int index = 0; index < nodes.Length; ++index)
                this.nodes[index + 1] = nodes[index];
            this.attackCoroutine = new Coroutine(false);
            this.Add((Component)this.attackCoroutine);
            this.triggerBlocksCoroutine = new Coroutine(false);
            this.Add((Component)this.triggerBlocksCoroutine);
            this.Add((Component)new CameraLocker(cameraLockY ? Level.CameraLockModes.FinalBoss : Level.CameraLockModes.FinalBossNoY, 140f, cameraYPastMax));
            this.Add((Component)(this.floatSine = new SineWave(0.6f)));
            this.Add((Component)(this.scaleWiggler = Wiggler.Create(0.6f, 3f)));
            this.Add((Component)(this.chargeSfx = new SoundSource()));
            this.Add((Component)(this.laserSfx = new SoundSource()));
            this.Add((Component)(this.hitTimeRateModifier = new TimeRateModifier(1f, false)));
        }

        public AsrielGodBoss(EntityData e, Vector2 offset)
            : this(e.Position + offset, e.NodesOffset(offset), e.Int(nameof(patternIndex)),
                  e.Float("cameraPastY", 120f), e.Bool(nameof(dialog)), e.Bool(nameof(startHit)),
                  e.Bool("cameraLockY", true), e.Attr("attackSequence", ""))
        {
            // Parse custom attack sequence (if any)
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
            this.level = scene as Level;
            // Restore persistent HP from session flags so health doesn't reset on room change
            RestoreHPFromSession();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.level = scene as Level;
            this.fallingBlocks = this.Scene.Tracker.GetEntitiesCopy<FallingBlock>();
            this.fallingBlocks.Sort((a, b) => (int)(a.X - b.X));
            this.movingBlocks = this.Scene.Tracker.GetEntitiesCopy<FinalBossMovingBlock>();
            this.movingBlocks.Sort((a, b) => (int)(a.X - b.X));

            // Initialize boss sprite (plays idle animation, sets up callbacks)
            createBossSprite();
            InitializeAttackPatterns();
        }

        private void RestoreHPFromSession()
        {
            if (this.level == null) return;
            int savedTotal = this.level.Session.GetCounter("asriel_god_total_hits");
            int savedPhase = this.level.Session.GetCounter("asriel_god_phase");
            int savedInPhase = this.level.Session.GetCounter("asriel_god_hits_in_phase");
            if (savedTotal > 0)
            {
                totalHitsTaken = savedTotal;
                currentPhase = Math.Min(savedPhase, TOTAL_PHASES - 1);
                hitsInPhase = Math.Min(savedInPhase, MAX_HITS_PER_PHASE - 1);
            }
        }

        private void SaveHPToSession()
        {
            if (this.level == null) return;
            this.level.Session.SetCounter("asriel_god_total_hits", totalHitsTaken);
            this.level.Session.SetCounter("asriel_god_phase", currentPhase);
            this.level.Session.SetCounter("asriel_god_hits_in_phase", hitsInPhase);
        }

        private void createBossSprite()
        {
            // Create the boss sprite from SpriteBank
            // The spriteName "asrielgodboss" must match the ID in Sprites.xml            
            if (this.Sprite == null)
            {
                try
                {
                    this.Sprite = GFX.SpriteBank.Create("asrielgodboss");
                    if (this.Sprite != null)
                    {
                        this.Sprite.Visible = true;
                        Add((Component)this.Sprite);                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("AsrielGodBoss", $"Error creating boss sprite: {ex}");
                }
            }
            
            // Configure sprite callbacks
            if (this.Sprite != null)
            {
                this.Sprite.OnFrameChange = (anim =>
                {
                    if (anim == "idle" && this.Sprite.CurrentAnimationFrame == 18)
                        Audio.Play(SFX_SPARKLES, this.Position);
                });
                
                // Play initial animation
                if (this.Sprite.Has("idle"))
                {
                    this.Sprite.Play("idle");
                }
                else if (this.Sprite.Has("boss"))
                {
                    this.Sprite.Play("boss");
                }
            }
            
            this.facing = -1;
            if (this.NormalSprite != null)
            {
                if (this.Sprite != null)
                    this.Sprite.Position = this.NormalSprite.Position;
                this.Remove((Component)this.NormalSprite);
            }
            if (this.normalHair != null)
                this.Remove((Component)this.normalHair);
            this.NormalSprite = null;
            this.normalHair = null;
        }

        public Vector2 BeamOrigin => this.Center + (this.Sprite?.Position ?? Vector2.Zero) + new Vector2(0.0f, -14f);
        public Vector2 ShotOrigin => this.Center + (this.Sprite?.Position ?? Vector2.Zero) + new Vector2(6f * (this.Sprite?.Scale.X ?? 1f), 2f);
        
        #region HP/Phase Properties (from V2)
        
        /// <summary>
        /// Total hits taken across all phases
        /// </summary>
        public int TotalHitsTaken => totalHitsTaken;
        
        /// <summary>
        /// Maximum hits needed to defeat the boss
        /// </summary>
        public int MaxTotalHits => MAX_HITS;
        
        /// <summary>
        /// Current phase (0 to TOTAL_PHASES-1)
        /// </summary>
        public int CurrentPhase => currentPhase;
        
        /// <summary>
        /// Total number of phases
        /// </summary>
        public int TotalPhases => TOTAL_PHASES;
        
        /// <summary>
        /// Hits taken in current phase
        /// </summary>
        public int HitsInCurrentPhase => hitsInPhase;
        
        /// <summary>
        /// Hits needed to complete current phase
        /// </summary>
        public int HitsPerPhase => MAX_HITS_PER_PHASE;
        
        /// <summary>
        /// Progress through current phase (0.0 to 1.0)
        /// </summary>
        public float PhaseProgress => (float)hitsInPhase / MAX_HITS_PER_PHASE;
        
        /// <summary>
        /// Overall boss HP progress (0.0 = full health, 1.0 = defeated)
        /// </summary>
        public float OverallProgress => (float)totalHitsTaken / MAX_HITS;
        
        /// <summary>
        /// Whether boss is defeated - combines state-based and HP-based checks
        /// </summary>
        public bool IsBossDefeated => currentState == BossState.Defeated || base.IsDefeated;
        
        #endregion

        public override void Update()
        {
            base.Update();

            // Update cooldowns
            if (playerHitCooldown > 0f)
                playerHitCooldown -= Engine.DeltaTime;

            if (flashTimer > 0f)
                flashTimer -= Engine.DeltaTime;

            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Engine.DeltaTime;
                Position += knockbackVelocity * Engine.DeltaTime;
                knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.Zero, Engine.DeltaTime * 5f);
                
                // When knockback ends, start moving to next node
                if (knockbackTimer <= 0f && nodes != null && nodes.Length > 0)
                {
                    Moving = true;
                    Add(new Coroutine(MoveToNextNode()));
                }
            }

            if (mercyTimer > 0f)
            {
                mercyTimer -= Engine.DeltaTime;
                if (mercyTimer <= 0f)
                    playerMercyActive = false;
            }

            // Check contact with K_Player / SoulPlayer for the refusal/mercy system
            CheckKirbyAndSoulPlayerCollision();

            Sprite sprite = this.Sprite != null ? this.Sprite : (Sprite)this.NormalSprite;
            if (!this.Sitting)
            {
                var entity = this.Scene.Tracker.GetEntity<global::Celeste.Player>();
                if (!this.Moving && entity != null)
                {
                    if (this.facing == -1 && entity.X > this.X + 20.0f)
                    {
                        this.facing = 1;
                        this.scaleWiggler.Start();
                    }
                    else if (this.facing == 1 && entity.X < this.X - 20.0f)
                    {
                        this.facing = -1;
                        this.scaleWiggler.Start();
                    }
                }
                if (!this.playerHasMoved && entity != null && entity.Speed != Vector2.Zero)
                {
                    this.playerHasMoved = true;
                    if (this.patternIndex != 0)
                        this.startAttacking();
                    // this.triggerMovingBlocks(0); // Commented out as method doesn't exist
                }
                if (!this.Moving)
                    sprite.Position = this.avoidPos + new Vector2(this.floatSine.Value * 3f, this.floatSine.ValueOverTwo * 4f);
                else
                    sprite.Position = Calc.Approach(sprite.Position, Vector2.Zero, 12f * Engine.DeltaTime);
                float radius = this.circle.Radius;
                this.circle.Radius = 6f;
                var dashBlock = this.CollideFirst<DashBlock>();
                if (dashBlock != null)
                    dashBlock.Break(Center, -Vector2.UnitY, true, false);
                this.circle.Radius = radius;
                if (!this.level.IsInBounds(this.Position, 24f))
                {
                    this.Visible = false;
                    this.Collidable = false;
                    return;
                }
                else if (!this.Visible)
                {
                    // Recover visibility when back in bounds
                    this.Visible = true;
                    this.Collidable = true;
                }
                Vector2 target;
                if (!this.Moving && entity != null)
                {
                    float length = Calc.ClampedMap((this.Center - entity.Center).Length(), 32f, 88f, 12f, 0.0f);
                    target = length > 0.0f ? (this.Center - entity.Center).SafeNormalize(length) : Vector2.Zero;
                }
                else
                    target = Vector2.Zero;
                this.avoidPos = Calc.Approach(this.avoidPos, target, 40f * Engine.DeltaTime);
            }
            this.light.Position = sprite.Position + new Vector2(0.0f, -10f);

        }

        private static Color GetAsrielHueShift(float time)
        {
            // Hue offset oscillates between -20 and -60 degrees (mapped to 0-1 hue space)
            float baseDeg = -20f;
            float rangeDeg = -40f; // -20 + -40 = -60 at peak
            float hueDeg = baseDeg + (float)Math.Sin(time * 1.2f) * rangeDeg * 0.5f - rangeDeg * 0.5f;
            float hueShift = hueDeg / 360f;
            // Apply hue rotation to a rainbow base driven by time
            float hue = (time * 0.15f + hueShift + 1f) % 1f;
            int i = (int)Math.Floor(hue * 6);
            float f = hue * 6 - i;
            float q = 1f - f;
            float t2 = f;
            return (i % 6) switch
            {
                0 => new Color(1f, t2, 0f),
                1 => new Color(q, 1f, 0f),
                2 => new Color(0f, 1f, t2),
                3 => new Color(0f, q, 1f),
                4 => new Color(t2, 0f, 1f),
                _ => new Color(1f, 0f, q)
            };
        }

        public override void Render()
        {
            if (this.Sprite != null)
            {
                this.Sprite.Scale.X = (float)this.facing;
                this.Sprite.Scale.Y = 1f;
                this.Sprite.Scale *= (float)(1.0 + this.scaleWiggler.Value * 0.2);

                // Apply flash effect when hit, otherwise rainbow hue-20 to hue-60 shift
                if (flashTimer > 0f)
                {
                    this.Sprite.Color = Color.Lerp(Color.White, Color.Red, flashTimer / 0.2f);
                }
                else
                {
                    float hueBlend = 0.28f; // subtle tint, keep recognizable
                    Color hueColor = GetAsrielHueShift(Scene?.TimeActive ?? 0f);
                    this.Sprite.Color = Color.Lerp(Color.White, hueColor, hueBlend);
                }
            }
            if (this.NormalSprite != null)
            {
                Vector2 position = this.NormalSprite.Position;
                this.NormalSprite.Position = this.NormalSprite.Position.Floor();
                base.Render();
                this.NormalSprite.Position = position;
            }
            else
                base.Render();
        }

        public void OnPlayer(global::Celeste.Player player)
        {
            if (player == null || currentState == BossState.Defeated)
                return;
            
            try
            {
                // Check if player is dash attacking (hits boss) or just colliding (boss hits player)
                if (player.DashAttacking || player.StateMachine.State == global::Celeste.Player.StBoost)
                {
                    // Player is attacking - apply HP damage to boss
                    OnHitByPlayer(player);
                }
                else
                {
                    // Boss collision with player - mercy system (player doesn't die)
                    OnBossHitsPlayer(player);
                }
            }           
            catch (Exception ex)
            {
                Logger.Log("AsrielGodBoss", $"Error in OnPlayer collision: {ex}");
            }
        }
        
        /// <summary>
        /// Called when player dash-attacks the boss - HP/Phase system from V2
        /// </summary>
        private void OnHitByPlayer(global::Celeste.Player player)
        {
            if (!canHit) return;
            
            // Disable hits during invulnerability
            canHit = false;
            hitsInPhase++;
            totalHitsTaken++;
            
            bool isKirbyMode = player.IsKirbyMode();
            
            // Visual and audio feedback
            flashTimer = 0.5f;
            Audio.Play(SFX_BOSS_HIT, Position);
            level.Flash(Color.White, true);
            level.Shake(0.3f);
            
            // Emit hit particles
            EmitHitParticles();
            
            // Bounce player away
            player.PointBounce(Center);
            
            // Stop current attack
            stopAttacking();
            SetState(BossState.Hit);
            
            // Apply hit slowdown effect
            ApplyHitEffect(player, isKirbyMode);
            
            // Apply shockwave pushback
            ApplyShockwavePushback(player, isKirbyMode);
            
            // Check if phase should transition
            if (hitsInPhase >= MAX_HITS_PER_PHASE)
            {
                // Always save HP before phase change so it persists
                SaveHPToSession();
                if (currentPhase >= TOTAL_PHASES - 1)
                {
                    // Boss defeated!
                    OnDefeated();
                }
                else
                {
                    // Transition to next phase
                    StartPhaseTransition();
                }
            }
            else
            {
                // Save HP to session so it persists across room transitions
                SaveHPToSession();
                // Continue fighting after invulnerability period
                Alarm.Set(this, HIT_INVULNERABILITY_TIME, () =>
                {
                    canHit = true;
                    if (currentState != BossState.Defeated)
                    {
                        GotoNextNode();
                    }
                });
            }
        }
        
        /// <summary>
        /// Check contact with K_Player and SoulPlayer for the refusal/mercy system.
        /// </summary>
        private void CheckKirbyAndSoulPlayerCollision()
        {
            if (currentState == BossState.Defeated || Scene == null)
                return;

            foreach (K_Player kirby in Scene.Tracker.GetEntities<K_Player>())
            {
                if (kirby != null && kirby.Collidable && !kirby.Dead && CollideCheck(kirby))
                {
                    OnKirbyPlayerHit(kirby);
                }
            }

            foreach (SoulPlayer soul in Scene.Tracker.GetEntities<SoulPlayer>())
            {
                if (soul != null && soul.Collidable && !soul.IsDead && CollideCheck(soul))
                {
                    soul.TakeDamage(1);
                }
            }
        }

        /// <summary>
        /// Mercy handling for K_Player contact.
        /// </summary>
        private void OnKirbyPlayerHit(K_Player kirby)
        {
            if (playerMercyActive)
                return;

            playerMercyActive = true;
            mercyTimer = 2.0f;
            Audio.Play(SFX_SPELLCAST_GLITCH, kirby.Position);
            level.Particles.Emit(PBurst, 10, kirby.Center, Vector2.One * 4f);

            if (!dialogTriggered_StruckDown && dialog && patternIndex >= 0 && patternIndex <= 30)
            {
                dialogTriggered_StruckDown = true;
                Add(new Coroutine(PlayStruckDownDialog()));
            }
        }

        /// <summary>
        /// Called when boss collides with non-attacking player - mercy system
        /// </summary>
        private void OnBossHitsPlayer(global::Celeste.Player player)
        {
            bool isKirbyMode = player.IsKirbyMode();
            
            // Player refuses to die - mercy system
            if (!playerMercyActive)
            {
                playerMercyActive = true;
                mercyTimer = 2.0f; // 2 seconds of mercy
                Audio.Play(SFX_SPELLCAST_GLITCH, player.Position); // Mercy sound
                // Visual effect for mercy
                level.Particles.Emit(PBurst, 10, player.Center, Vector2.One * 4f);
                
                // CH20_KIRBY_STRUCK_DOWN dialog in rooms azzyboss-00 through azzyboss-30
                if (!dialogTriggered_StruckDown && dialog && patternIndex >= 0 && patternIndex <= 30)
                {
                    dialogTriggered_StruckDown = true;
                    Add(new Coroutine(PlayStruckDownDialog()));
                }
            }
            // Don't damage the player during mercy
        }
        
        private IEnumerator PlayStruckDownDialog()
        {
            stopAttacking();
            yield return Textbox.Say("DZ_CH20_KIRBY_STRUCK_DOWN");
            yield return 0.5f;
            startAttacking();
        }
        
        /// <summary>
        /// Emit radial burst of particles when hit
        /// </summary>
        private void EmitHitParticles()
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = (i / 16f) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                level.ParticlesFG.Emit(PBurst, 1, Center + dir * 8f, Vector2.One * 4f, angle);
            }
        }
        
        /// <summary>
        /// Move to the next node after being hit
        /// </summary>
        private void GotoNextNode()
        {
            if (nodes == null || nodes.Length == 0)
            {
                Moving = false;
                startAttacking();
                return;
            }
            
            nodeIndex = (nodeIndex + 1) % nodes.Length;
            Moving = true;
            Add(new Coroutine(MoveToNextNode()));
        }
        
        #region Phase Transition System (from V2)
        
        /// <summary>
        /// Start transition to next phase
        /// </summary>
        private void StartPhaseTransition()
        {
            phaseTransitioning = true;
            SetState(BossState.Transitioning);
            currentPhase++;
            hitsInPhase = 0;
            
            Add(new Coroutine(PhaseTransitionRoutine()));
        }
        
        /// <summary>
        /// Phase transition animation and effects
        /// </summary>
        private IEnumerator PhaseTransitionRoutine()
        {
            // Dramatic effects
            level.Shake(0.5f);
            level.Flash(Color.White * 0.5f, false);
            Audio.Play(SFX_BARRIER_SHATTER, Position);
            
            // Emit burst of particles
            for (int i = 0; i < 24; i++)
            {
                float angle = (i / 24f) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                level.ParticlesFG.Emit(PBurst, 2, Center + dir * 12f, Vector2.One * 6f, angle);
            }
            
            // Brief pause
            yield return 0.5f;
            
            // Update music for new phase
            UpdatePhaseMusic();
            
            // Invulnerability during transition
            yield return 1f;
            
            // Move to a central node if available
            if (nodes != null && nodes.Length > 1)
            {
                int centerNode = nodes.Length / 2;
                nodeIndex = centerNode;
                yield return MoveToNodePosition(nodes[centerNode]);
            }
            
            // Resume fighting
            phaseTransitioning = false;
            canHit = true;
            SetState(BossState.Idle);
            startAttacking();
        }
        
        /// <summary>
        /// Move to a specific position
        /// </summary>
        private IEnumerator MoveToNodePosition(Vector2 target)
        {
            Moving = true;
            float speed = MOVE_SPEED;
            
            while (Vector2.Distance(Position, target) > 4f)
            {
                Vector2 direction = (target - Position).SafeNormalize();
                Position += direction * speed * Engine.DeltaTime;
                yield return null;
            }
            
            Position = target;
            Moving = false;
        }
        
        /// <summary>
        /// Update music based on current phase
        /// </summary>
        private void UpdatePhaseMusic()
        {
            // Music progression per phase - can be customized
            // Phase 0-1: Standard boss music
            // Phase 2-3: Intensified
            // Phase 4: Final phase / climax
            if (currentPhase >= 2 && !asrielRememberTriggered)
            {
                // Could trigger more intense music phase here
                level.Session.Audio.Music.Event = ASRIEL_REMEMBER_PART_2;
                level.Session.Audio.Apply();
            }
        }
        
        /// <summary>
        /// Called when boss is defeated (all phases complete)
        /// </summary>
        private void OnDefeated()
        {
            SetState(BossState.Defeated);
            Collidable = false;
            stopAttacking();
            
            // Call base implementation if available
            // base.OnDefeated(); // Uncomment if base has logic we need
            
            // Trigger defeat effects
            Audio.Play(SFX_BARRIER_SHATTER, Position);
            level.Flash(Color.White, false);
            level.Shake(1f);
            
            // Large particle explosion
            for (int i = 0; i < 40; i++)
            {
                float angle = (i / 40f) * MathHelper.TwoPi;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                level.ParticlesFG.Emit(PBurst, 3, Center + dir * 16f, Vector2.One * 8f, angle);
            }
            
            // Trigger any post-defeat cutscene or flag
            level.Session.SetFlag("asriel_boss_defeated", true);
            
            // Add defeat coroutine for fadeout/cleanup
            Add(new Coroutine(DefeatRoutine()));
        }
        
        /// <summary>
        /// Defeat animation and cleanup
        /// </summary>
        private IEnumerator DefeatRoutine()
        {
            // Play defeat animation if available
            if (Sprite != null && Sprite.Has("fadeout"))
            {
                Sprite.Play("fadeout");
            }
            
            // Fade out effect
            float fadeTime = 2f;
            for (float t = 0; t < fadeTime; t += Engine.DeltaTime)
            {
                if (Sprite != null)
                {
                    Sprite.Color = Color.White * (1f - (t / fadeTime));
                }
                yield return null;
            }
            
            // Check if we should trigger the Angel of Death phase transition
            if (!hyperGonerPhaseTransitionTriggered && !level.Session.GetFlag("asriel_boss_end_hypergoner"))
            {
                // Optionally trigger the next boss phase (Angel form)
                TriggerAngelPhaseTransition();
            }
            
            RemoveSelf();
        }
        
        #endregion
        
        private bool isHitSlowdownActive = false;
        
        /// <summary>
        /// Apply hit effect with time slowdown (pitch slowdown) when player hits Asriel
        /// </summary>
        private void ApplyHitEffect(global::Celeste.Player player, bool isKirbyMode)
        {
            // Don't apply multiple hit effects at once
            if (isHitSlowdownActive) return;
            
            // Start hit slowdown coroutine
            Add(new Coroutine(HitSlowdownEffect(player, isKirbyMode)));
        }
        
        private IEnumerator HitSlowdownEffect(global::Celeste.Player player, bool isKirbyMode)
        {
            isHitSlowdownActive = true;
            
            // Slowdown parameters - shorter and more impactful for hit feedback
            float slowdownScale = 0.3f; // Slow to 30% speed
            float slowdownDuration = 0.15f; // Very brief slowdown
            float pitchSlowdown = 0.5f; // Music pitch during slowdown (lower = deeper)
            
            // Apply time slowdown
            hitTimeRateModifier.SetTimeRateMultiplier(slowdownScale);
            
            // Apply music pitch slowdown for dramatic effect
            if (level != null)
            {
                Audio.SetMusicParam("pitch", pitchSlowdown);
            }
            
            // Hit sound effect
            string hitSound = isKirbyMode ? "event:/game/general/thing_booped" : "event:/game/general/landing";
            if (player != null)
            {
                Audio.Play(hitSound, player.Position);
            }
            
            // Visual feedback - flash and particles (null check for level)
            if (level != null)
            {
                level.Flash(Color.White, true);
                level.Shake(0.2f);
                
                // Emit hit particles in all directions
                if (player != null)
                {
                    level.ParticlesFG.Emit(PBurst, 12, player.Center, Vector2.One * 12f);
                }
                
                // Kirby-specific effects
                if (isKirbyMode && player != null)
                {
                    // Extra sparkle for Kirby hits
                    level.Particles.Emit(PBurst, 15, player.Center, Vector2.One * 8f);
                    Audio.Play(SFX_SPARKLES, player.Position);
                }
            }
            
            // Wait for slowdown duration
            yield return slowdownDuration;
            
            // Restore normal time rate
            hitTimeRateModifier.ResetTimeRateMultiplier();
            
            // Restore normal music pitch
            if (level != null)
            {
                Audio.SetMusicParam("pitch", 1f);
            }
            
            isHitSlowdownActive = false;
        }

        /// <summary>
        /// Apply shockwave pushback effect when player hits Asriel
        /// </summary>
        private void ApplyShockwavePushback(global::Celeste.Player player, bool isKirbyMode)
        {
            // Calculate direction from player to Asriel
            Vector2 pushDirection = (Center - player.Center).SafeNormalize();
            
            // If no valid direction, push away from player's facing direction
            if (pushDirection == Vector2.Zero)
            {
                pushDirection = new Vector2(player.Facing == Facings.Right ? 1 : -1, 0);
            }
            
            // Pushback strength - stronger for Kirby mode
            float pushStrength = isKirbyMode ? 400f : 300f;
            
            // Apply knockback velocity
            knockbackVelocity = pushDirection * pushStrength;
            knockbackTimer = 0.3f; // Duration of knockback
            
            // Create shockwave visual effect
            CreateShockwaveEffect(isKirbyMode);
            
            // Play pushback sound
            Audio.Play("event:/Mods/pusheen/new_content/char/bosses/asriel/Asriel_scream_hit", Center);
            
            // Screen shake
            level.Shake(0.3f);
            
            // Emit particles in a radial burst from Asriel
            for (int i = 0; i < 16; i++)
            {
                float angle = (i / 16f) * MathHelper.TwoPi;
                Vector2 particleDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                level.ParticlesFG.Emit(PBurst, 1, Center + particleDir * 8f, Vector2.One * 4f, angle);
            }
            
            // Extra effects for Kirby mode
            if (isKirbyMode)
            {
                // More intense flash
                level.Flash(Color.Cyan, true);
                // Additional particle ring
                for (int i = 0; i < 20; i++)
                {
                    float angle = (i / 20f) * MathHelper.TwoPi;
                    Vector2 particleDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    level.Particles.Emit(PBurst, 1, Center + particleDir * 16f, Vector2.One * 6f, angle);
                }
            }
        }

        /// <summary>
        /// Create a visual shockwave ring effect
        /// </summary>
        private void CreateShockwaveEffect(bool isKirbyMode)
        {
            // Create expanding ring of particles
            Add(new Coroutine(ShockwaveRingEffect(isKirbyMode)));
        }

        private IEnumerator ShockwaveRingEffect(bool isKirbyMode)
        {
            float duration = 0.5f;
            float maxRadius = isKirbyMode ? 60f : 45f;
            int particleCount = isKirbyMode ? 32 : 24;
            
            for (float t = 0; t < duration; t += Engine.DeltaTime)
            {
                float progress = t / duration;
                float currentRadius = Ease.CubeOut(progress) * maxRadius;
                
                // Emit particles in a ring
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = (i / (float)particleCount) * MathHelper.TwoPi;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * currentRadius;
                    
                    // Emit particle at ring position
                    level.ParticlesFG.Emit(PBurst, 1, Center + offset, Vector2.One * 2f);
                }
                
                yield return null;
            }
        }

        private void SetState(BossState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                OnStateChanged(newState);
            }
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
                    break;
                case BossState.Attacking:
                    startAttacking();
                    break;
            }
        }

        /// <summary>
        /// Public method to trigger the startHit behavior from external triggers.
        /// This starts the boss attack sequence.
        /// </summary>
        public void TriggerStartHit()
        {
            if (currentState == BossState.Defeated) return;
            
            // Play a sound effect to indicate the boss is triggered
            Audio.Play(SFX_SEGA_POWER_01, Position);
            
            // Start the attack sequence
            startAttacking();
        }
        
        /// <summary>
        /// Public method to manually trigger the Angel of Death phase transition.
        /// Call this after Kirby survives the HyperGoner attack.
        /// </summary>
        public void TriggerAngelPhaseTransition()
        {
            if (hyperGonerPhaseTransitionTriggered) return;
            if (level?.Session?.GetFlag("asriel_boss_end_hypergoner") == true) return;
            
            hyperGonerPhaseTransitionTriggered = true;
            level?.Add(new CS20_BossEnd());
        }

        /// <summary>
        /// Gets or sets the boss backdrop starfield.
        /// Note: Backdrops are typically added via the map editor (LÃƒÂ¶nn) as stylegrounds.
        /// This property allows external access for cutscenes or dynamic effects.
        /// </summary>
        public AsrielGodBossStarfield BossBackdrop
        {
            get => bossBg;
            set => bossBg = value;
        }

        /// <summary>
        /// Public method to move Asriel to a target position.
        /// </summary>
        /// <param name="target">The target position to move to.</param>
        /// <param name="speed">The movement speed.</param>
        public void MoveToTarget(Vector2 target, float speed = 300f)
        {
            if (currentState == BossState.Defeated) return;
            
            Moving = true;
            Add(new Coroutine(MoveToTargetRoutine(target, speed)));
        }

        private IEnumerator MoveToTargetRoutine(Vector2 target, float speed)
        {
            while (Vector2.Distance(Position, target) > 4f)
            {
                Vector2 direction = (target - Position).SafeNormalize();
                Position += direction * speed * Engine.DeltaTime;
                yield return null;
            }
            Position = target;
            Moving = false;
        }

        /// <summary>
        /// Move to the next node in the node sequence after knockback
        /// </summary>
        private IEnumerator MoveToNextNode()
        {
            if (nodes == null || nodes.Length == 0)
            {
                Moving = false;
                yield break;
            }

            // Move to next node in sequence
            nodeIndex = (nodeIndex + 1) % nodes.Length;
            Vector2 targetNode = nodes[nodeIndex];
            
            float speed = MOVE_SPEED;
            
            while (Vector2.Distance(Position, targetNode) > 4f)
            {
                Vector2 direction = (targetNode - Position).SafeNormalize();
                Position += direction * speed * Engine.DeltaTime;
                yield return null;
            }
            
            Position = targetNode;
            Moving = false;
        }

        private void startAttacking()
        {
            if (attackCoroutine != null && attackCoroutine.Active) return;
            if (currentState == BossState.Defeated) return;

            isAttacking = true;
            SetState(BossState.Attacking);

            if (useCustomSequence && customAttackSteps != null && customAttackSteps.Count > 0)
            {
                attackCoroutine.Replace(attackSequence());
            }
            else
            {
                StartAttacking();
            }
        }

        private void StartAttacking()
        {
            switch (patternIndex)
            {
                case 0:
                case 1:
                    attackCoroutine.Replace(Attack01Sequence());
                    break;
                case 2:
                    attackCoroutine.Replace(Attack02Sequence());
                    break;
                case 3:
                    attackCoroutine.Replace(Attack03Sequence());
                    break;
                case 4:
                    attackCoroutine.Replace(Attack04Sequence());
                    break;
                case 5:
                    attackCoroutine.Replace(Attack05Sequence());
                    break;
                case 6:
                    attackCoroutine.Replace(Attack06Sequence());
                    break;
                case 7:
                    attackCoroutine.Replace(Attack07Sequence());
                    break;
                case 8:
                    attackCoroutine.Replace(Attack08Sequence());
                    break;
                case 9:
                    attackCoroutine.Replace(Attack09Sequence());
                    break;
                case 10:
                    attackCoroutine.Replace(Attack10Sequence());
                    break;
                case 11:
                    attackCoroutine.Replace(Attack11Sequence());
                    break;
                case 12:
                    attackCoroutine.Replace(Attack12Sequence());
                    break;
                case 13:
                    attackCoroutine.Replace(Attack13Sequence());
                    break;
                case 14:
                    attackCoroutine.Replace(Attack14Sequence());
                    break;
                case 15:
                    attackCoroutine.Replace(Attack15Sequence());
                    break;
                case 16:
                    attackCoroutine.Replace(Attack16Sequence());
                    break;
                case 17:
                    attackCoroutine.Replace(Attack17Sequence());
                    break;
                case 18:
                    attackCoroutine.Replace(Attack18Sequence());
                    break;
                case 19:
                    attackCoroutine.Replace(Attack19Sequence());
                    break;
                case 20:
                    attackCoroutine.Replace(Attack20Sequence());
                    break;
                case 21:
                    attackCoroutine.Replace(Attack21Sequence());
                    break;
                case 22:
                    attackCoroutine.Replace(Attack22Sequence());
                    break;
                case 23:
                    attackCoroutine.Replace(Attack23Sequence());
                    break;
                case 24:
                    attackCoroutine.Replace(Attack24Sequence());
                    break;
                case 25:
                    attackCoroutine.Replace(Attack25Sequence());
                    break;
                case 26:
                    attackCoroutine.Replace(Attack26Sequence());
                    break;
                case 27:
                    attackCoroutine.Replace(Attack27Sequence());
                    break;
                case 28:
                    attackCoroutine.Replace(Attack28Sequence());
                    break;
                case 29:
                    attackCoroutine.Replace(Attack29Sequence());
                    break;
                case 30:
                    attackCoroutine.Replace(Attack30Sequence());
                    break;
                case 31:
                    attackCoroutine.Replace(Attack31Sequence());
                    break;
                case 32:
                    attackCoroutine.Replace(Attack32Sequence());
                    break;
                case 33:
                    attackCoroutine.Replace(Attack33Sequence());
                    break;
                case 34:
                    attackCoroutine.Replace(Attack34Sequence());
                    break;
                case 35:
                    attackCoroutine.Replace(Attack35Sequence());
                    break;
                case 36:
                    attackCoroutine.Replace(Attack36Sequence());
                    break;
                case 37:
                    attackCoroutine.Replace(Attack37Sequence());
                    break;
                case 38:
                    attackCoroutine.Replace(Attack38Sequence());
                    break;
                case 39:
                    attackCoroutine.Replace(Attack39Sequence());
                    break;
                case 40:
                    attackCoroutine.Replace(Attack40Sequence());
                    break;
                case 41:
                    attackCoroutine.Replace(Attack41Sequence());
                    break;
                case 42:
                    attackCoroutine.Replace(Attack42Sequence());
                    break;
                case 43:
                    attackCoroutine.Replace(Attack43Sequence());
                    break;
                case 44:
                    attackCoroutine.Replace(Attack44Sequence());
                    break;
                case 45:
                    attackCoroutine.Replace(Attack45Sequence());
                    break;
                case 46:
                    attackCoroutine.Replace(Attack46Sequence());
                    break;
                case 47:
                    attackCoroutine.Replace(Attack47Sequence());
                    break;
                case 48:
                    attackCoroutine.Replace(Attack48Sequence());
                    break;
                case 49:
                    attackCoroutine.Replace(Attack49Sequence());
                    break;
                case 50:
                    attackCoroutine.Replace(Attack50Sequence());
                    break;
                case 51:
                    attackCoroutine.Replace(Attack51Sequence());
                    break;
                case 52:
                    attackCoroutine.Replace(Attack52Sequence());
                    break;
                case 53:
                    attackCoroutine.Replace(Attack53Sequence());
                    break;
                case 54:
                    attackCoroutine.Replace(Attack54Sequence());
                    break;
                case 55:
                    attackCoroutine.Replace(Attack55Sequence());
                    break;
                case 56:
                    attackCoroutine.Replace(Attack56Sequence());
                    break;
                case 57:
                    attackCoroutine.Replace(Attack57Sequence());
                    break;
                case 58:
                    attackCoroutine.Replace(Attack58Sequence());
                    break;
                case 59:
                    attackCoroutine.Replace(Attack59Sequence());
                    break;
                case 60:
                    attackCoroutine.Replace(Attack60Sequence());
                    break;
            }
        }

        private void stopAttacking()
        {
            isAttacking = false;
            if (attackCoroutine != null)
                attackCoroutine.Active = false;
        }

        private IEnumerator attackSequence()
        {
            if (useCustomSequence && customAttackSteps != null && customAttackSteps.Count > 0)
            {
                while (isAttacking && currentState == BossState.Attacking)
                {
                    foreach (var step in customAttackSteps)
                    {
                        if (!isAttacking || currentState != BossState.Attacking) yield break;
                        switch (step.Action)
                        {
                            case nameof(shoot):
                                currentAttackType = AttackType.Shoot;
                                startShootCharge();
                                yield return 0.15f;
                                shoot(MathHelper.ToRadians(step.Arg));
                                break;
                            case nameof(beam):
                                currentAttackType = AttackType.Beam;
                                yield return beam();
                                break;
                            case "biggerbeam":
                                currentAttackType = AttackType.BiggerBeam;
                                yield return biggerBeam();
                                break;
                            case "bigbeamball":
                                currentAttackType = AttackType.BigBeamBall;
                                yield return bigBeamBall();
                                break;
                            case "rainbowblackhole":
                                currentAttackType = AttackType.RainbowBlackhole;
                                yield return rainbowBlackhole();
                                break;
                            case "bladethrower":
                                currentAttackType = AttackType.BladeThrower;
                                yield return bladeThrower();
                                break;
                            case "fireshockwave":
                                currentAttackType = AttackType.FireShockwave;
                                yield return fireShockwave();
                                break;
                            case "starsmeteorite":
                                currentAttackType = AttackType.StarsMeteorite;
                                yield return starsMeteorite();
                                break;
                            default:
                                break;
                        }
                        if (step.Delay > 0f)
                            yield return step.Delay;
                    }
                }
                yield break;
            }
        }

        private void startShootCharge()
        {
            currentAttackPhase = AttackPhase.Charging;
            if (Sprite != null && Sprite.CurrentAnimationID != "castsp")
                Sprite.Play("castsp");
            if (!chargeSfx.Playing)
                chargeSfx.Play("event:/char/badeline/boss_bullet");
        }

        // Pattern sequences following the reference format
        private IEnumerator Attack01Sequence()
        {
            startShootCharge();
            while (true)
            {
                yield return 0.5f;
                shoot();
                yield return 1f;
                startShootCharge();
                yield return 0.15f;
                yield return 0.3f;
            }
        }

        private IEnumerator Attack02Sequence()
        {
            while (true)
            {
                yield return 0.5f;
                yield return beam();
                yield return 0.4f;
                startShootCharge();
                yield return 0.3f;
                shoot();
                yield return 0.5f;
                yield return 0.3f;
            }
        }

        private IEnumerator Attack03Sequence()
        {
            startShootCharge();
            yield return 0.1f;
            while (true)
            {
                for (int i = 0; i < 5; i++)
                {
                    var entity = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; j++)
                        {
                            shootAt(at);
                            yield return 0.15f;
                        }
                    }
                    if (i < 4)
                    {
                        startShootCharge();
                        yield return 0.5f;
                    }
                }
                yield return 2f;
                startShootCharge();
                yield return 0.7f;
            }
        }

        private IEnumerator Attack04Sequence()
        {
            startShootCharge();
            yield return 0.1f;
            while (true)
            {
                for (int i = 0; i < 5; i++)
                {
                    var entity = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; j++)
                        {
                            shootAt(at);
                            yield return 0.15f;
                        }
                    }
                    if (i < 4)
                    {
                        startShootCharge();
                        yield return 0.5f;
                    }
                }
                yield return 1.5f;
                yield return beam();
                yield return 1.5f;
                startShootCharge();
            }
        }

        private IEnumerator Attack05Sequence()
        {
            // CH20_KIRBY_ASK_WHY dialog in azzyboss-05
            if (!dialogTriggered_AskWhy && dialog)
            {
                dialogTriggered_AskWhy = true;
                stopAttacking();
                yield return Textbox.Say("DZ_CH20_KIRBY_ASK_WHY");
                yield return 0.5f;
            }
            yield return 0.2f;
            while (true)
            {
                yield return beam();
                yield return 0.6f;
                startShootCharge();
                yield return 0.3f;
                for (int i = 0; i < 3; i++)
                {
                    var entity = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        Vector2 at = entity.Center;
                        for (int j = 0; j < 2; j++)
                        {
                            shootAt(at);
                            yield return 0.15f;
                        }
                    }
                    if (i < 2)
                    {
                        startShootCharge();
                        yield return 0.5f;
                    }
                }
                yield return 0.8f;
            }
        }

        private IEnumerator Attack06Sequence()
        {
            while (true)
            {
                yield return beam();
                yield return 0.7f;
            }
        }

        private IEnumerator Attack07Sequence()
        {
            while (true)
            {
                shoot();
                yield return 0.8f;
                startShootCharge();
                yield return 0.8f;
            }
        }

        private IEnumerator Attack08Sequence()
        {
            while (true)
            {
                yield return 0.1f;
                yield return beam();
                yield return 0.8f;
            }
        }

        private IEnumerator Attack09Sequence()
        {
            startShootCharge();
            while (true)
            {
                yield return 0.5f;
                shoot();
                yield return 0.15f;
                startShootCharge();
                shoot();
                yield return 0.4f;
                startShootCharge();
                yield return 0.1f;
            }
        }

        private IEnumerator Attack10Sequence()
        {
            // CH20_KIRBY_ASK_WHAT dialog in azzyboss-10
            if (!dialogTriggered_AskWhat && dialog)
            {
                dialogTriggered_AskWhat = true;
                stopAttacking();
                yield return Textbox.Say("DZ_CH20_KIRBY_ASK_WHAT");
                yield return 0.5f;
            }
            yield break;
        }

        private IEnumerator Attack11Sequence()
        {
            if (nodeIndex == 0)
            {
                startShootCharge();
                yield return 0.6f;
            }
            while (true)
            {
                shoot();
                yield return 1.9f;
                startShootCharge();
                yield return 0.6f;
            }
        }

        private IEnumerator Attack12Sequence()
        {
            while (true)
            {
                yield return rainbowBlackhole();
                yield return 2.5f;
            }
        }

        private IEnumerator Attack13Sequence()
        {
            if (nodeIndex != 0)
            {
                yield return Attack01Sequence();
            }
        }

        private IEnumerator Attack14Sequence()
        {
            while (true)
            {
                yield return 0.2f;
                yield return beam();
                yield return 0.3f;
            }
        }

        private IEnumerator Attack15Sequence()
        {
            // CH20_KIRBY_DO_NOT_WANT_TO_FIGHT dialog in azzyboss-15
            if (!dialogTriggered_DoNotWantToFight && dialog)
            {
                dialogTriggered_DoNotWantToFight = true;
                stopAttacking();
                yield return Textbox.Say("DZ_CH20_KIRBY_DO_NOT_WANT_TO_FIGHT");
                yield return 0.5f;
            }
            while (true)
            {
                yield return 0.2f;
                yield return beam();
                yield return 1.2f;
            }
        }

        private IEnumerator Attack16Sequence()
        {
            while (true)
            {
                yield return bladeThrower();
                yield return 1.5f;
                startShootCharge();
                yield return 0.5f;
                shoot();
                yield return 0.8f;
            }
        }

        private IEnumerator Attack17Sequence()
        {
            while (true)
            {
                yield return bigBeamBall();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack18Sequence()
        {
            while (true)
            {
                yield return fireShockwave();
                yield return 1.8f;
                startShootCharge();
                yield return 0.5f;
                shoot();
                yield return 0.5f;
            }
        }

        private IEnumerator Attack19Sequence()
        {
            while (true)
            {
                yield return starsMeteorite();
                yield return 2.5f;
            }
        }

        private IEnumerator Attack20Sequence()
        {
            while (true)
            {
                yield return biggerBeam();
                yield return 1.5f;
                startShootCharge();
                yield return 0.3f;
                shoot();
                yield return 0.8f;
            }
        }

        private IEnumerator Attack21Sequence()
        {
            startShootCharge();
            while (true)
            {
                for (int i = 0; i < 3; i++)
                {
                    yield return 0.3f;
                    shoot();
                    yield return 0.2f;
                    startShootCharge();
                }
                yield return 1.0f;
                yield return beam();
                yield return 1.2f;
                startShootCharge();
            }
        }

        private IEnumerator Attack22Sequence()
        {
            while (true)
            {
                yield return bladeThrower();
                yield return 0.8f;
                yield return bladeThrower();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack23Sequence()
        {
            while (true)
            {
                yield return rainbowBlackhole();
                yield return 1.5f;
                yield return fireShockwave();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack24Sequence()
        {
            while (true)
            {
                yield return starsMeteorite();
                yield return 1.0f;
                startShootCharge();
                yield return 0.5f;
                shoot();
                yield return 0.5f;
                shoot();
                yield return 1.5f;
                startShootCharge();
            }
        }

        private IEnumerator Attack25Sequence()
        {
            while (true)
            {
                yield return bigBeamBall();
                yield return 1.2f;
                yield return bladeThrower();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack26Sequence()
        {
            startShootCharge();
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    yield return 0.4f;
                    shoot();
                    yield return 0.15f;
                    startShootCharge();
                }
                yield return 0.8f;
                yield return biggerBeam();
                yield return 1.5f;
                startShootCharge();
            }
        }

        private IEnumerator Attack27Sequence()
        {
            while (true)
            {
                yield return fireShockwave();
                yield return 0.8f;
                yield return starsMeteorite();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack28Sequence()
        {
            while (true)
            {
                yield return beam();
                yield return 0.5f;
                yield return bladeThrower();
                yield return 1.2f;
            }
        }

        private IEnumerator Attack29Sequence()
        {
            while (true)
            {
                yield return rainbowBlackhole();
                yield return 1.0f;
                startShootCharge();
                yield return 0.3f;
                for (int i = 0; i < 3; i++)
                {
                    shoot();
                    yield return 0.2f;
                    startShootCharge();
                    yield return 0.3f;
                }
                yield return 1.5f;
            }
        }

        private IEnumerator Attack30Sequence()
        {
            // CH20_KIRBY_REFUSED_TO_DIE Ã¢â€ â€™ CH20_ASRIEL_FIRST_SPECIAL_ATTACK Ã¢â€ â€™ CH20_MADELINE_AND_BADELINE_SAVE_KIRBY_FROM_ASRIEL_FIRST_SPECIAL_ATTACK
            if (!dialogTriggered_RefusedToDie && dialog)
            {
                dialogTriggered_RefusedToDie = true;
                stopAttacking();
                yield return Textbox.Say("DZ_CH20_KIRBY_REFUSED_TO_DIE");
                yield return 0.3f;

                if (!dialogTriggered_FirstSpecialAttack)
                {
                    dialogTriggered_FirstSpecialAttack = true;
                    yield return Textbox.Say("DZ_CH20_ASRIEL_FIRST_SPECIAL_ATTACK");
                    yield return 0.3f;
                    yield return Textbox.Say("DZ_CH20_MADELINE_AND_BADELINE_SAVE_KIRBY_FROM_ASRIEL_FIRST_SPECIAL_ATTACK");
                    yield return 0.5f;
                }
            }
            while (true)
            {
                yield return bigBeamBall();
                yield return 0.8f;
                yield return fireShockwave();
                yield return 0.8f;
                yield return starsMeteorite();
                yield return 2.5f;
            }
        }

        private IEnumerator Attack31Sequence()
        {
            startShootCharge();
            while (true)
            {
                yield return 0.3f;
                shoot();
                yield return 0.3f;
                yield return beam();
                yield return 0.5f;
                startShootCharge();
                yield return 0.3f;
                shoot();
                yield return 1.0f;
                startShootCharge();
            }
        }

        private IEnumerator Attack32Sequence()
        {
            while (true)
            {
                yield return bladeThrower();
                yield return 0.5f;
                yield return biggerBeam();
                yield return 1.5f;
            }
        }

        private IEnumerator Attack33Sequence()
        {
            while (true)
            {
                for (int i = 0; i < 2; i++)
                {
                    yield return rainbowBlackhole();
                    yield return 1.2f;
                }
                yield return 2.0f;
            }
        }

        private IEnumerator Attack34Sequence()
        {
            while (true)
            {
                yield return starsMeteorite();
                yield return 0.8f;
                yield return bladeThrower();
                yield return 0.8f;
                yield return fireShockwave();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack35Sequence()
        {
            startShootCharge();
            while (true)
            {
                for (int i = 0; i < 6; i++)
                {
                    yield return 0.25f;
                    shoot();
                    yield return 0.15f;
                    startShootCharge();
                }
                yield return 1.5f;
                startShootCharge();
            }
        }

        private IEnumerator Attack36Sequence()
        {
            while (true)
            {
                yield return bigBeamBall();
                yield return 0.6f;
                yield return biggerBeam();
                yield return 1.8f;
            }
        }

        private IEnumerator Attack37Sequence()
        {
            while (true)
            {
                yield return beam();
                yield return 0.3f;
                yield return bladeThrower();
                yield return 0.5f;
                yield return fireShockwave();
                yield return 1.5f;
            }
        }

        private IEnumerator Attack38Sequence()
        {
            while (true)
            {
                yield return rainbowBlackhole();
                yield return 0.8f;
                yield return starsMeteorite();
                yield return 0.8f;
                yield return bigBeamBall();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack39Sequence()
        {
            startShootCharge();
            while (true)
            {
                for (int i = 0; i < 5; i++)
                {
                    var entity = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        shootAt(entity.Center);
                    }
                    yield return 0.3f;
                    startShootCharge();
                    yield return 0.3f;
                }
                yield return 1.2f;
                yield return bladeThrower();
                yield return 1.5f;
                startShootCharge();
            }
        }

        private IEnumerator Attack40Sequence()
        {
            while (true)
            {
                yield return fireShockwave();
                yield return 0.5f;
                yield return beam();
                yield return 0.5f;
                yield return starsMeteorite();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack41Sequence()
        {
            while (true)
            {
                yield return biggerBeam();
                yield return 0.8f;
                yield return rainbowBlackhole();
                yield return 1.8f;
            }
        }

        private IEnumerator Attack42Sequence()
        {
            startShootCharge();
            while (true)
            {
                yield return 0.2f;
                shoot();
                yield return 0.2f;
                yield return bladeThrower();
                yield return 0.5f;
                startShootCharge();
                yield return 0.3f;
                shoot();
                yield return 1.0f;
                startShootCharge();
            }
        }

        private IEnumerator Attack43Sequence()
        {
            while (true)
            {
                yield return bigBeamBall();
                yield return 0.6f;
                yield return fireShockwave();
                yield return 0.6f;
                yield return bladeThrower();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack44Sequence()
        {
            while (true)
            {
                for (int i = 0; i < 3; i++)
                {
                    yield return beam();
                    yield return 0.4f;
                }
                yield return 1.5f;
            }
        }

        private IEnumerator Attack45Sequence()
        {
            while (true)
            {
                yield return starsMeteorite();
                yield return 0.5f;
                yield return rainbowBlackhole();
                yield return 0.8f;
                yield return biggerBeam();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack46Sequence()
        {
            startShootCharge();
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    yield return 0.3f;
                    shoot();
                    yield return 0.1f;
                    startShootCharge();
                }
                yield return 0.8f;
                yield return bladeThrower();
                yield return 0.8f;
                yield return fireShockwave();
                yield return 1.8f;
                startShootCharge();
            }
        }

        private IEnumerator Attack47Sequence()
        {
            while (true)
            {
                yield return bigBeamBall();
                yield return 0.5f;
                yield return beam();
                yield return 0.5f;
                yield return starsMeteorite();
                yield return 1.8f;
            }
        }

        private IEnumerator Attack48Sequence()
        {
            while (true)
            {
                yield return rainbowBlackhole();
                yield return 0.6f;
                yield return bladeThrower();
                yield return 0.6f;
                yield return biggerBeam();
                yield return 1.5f;
            }
        }

        private IEnumerator Attack49Sequence()
        {
            while (true)
            {
                yield return fireShockwave();
                yield return 0.4f;
                yield return starsMeteorite();
                yield return 0.8f;
                yield return bigBeamBall();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack50Sequence()
        {
            startShootCharge();
            while (true)
            {
                for (int i = 0; i < 8; i++)
                {
                    yield return 0.2f;
                    shoot();
                    yield return 0.15f;
                    startShootCharge();
                }
                yield return 1.0f;
                yield return beam();
                yield return 0.8f;
                yield return bladeThrower();
                yield return 2.0f;
                startShootCharge();
            }
        }

        private IEnumerator Attack51Sequence()
        {
            while (true)
            {
                yield return biggerBeam();
                yield return 0.5f;
                yield return fireShockwave();
                yield return 0.5f;
                yield return rainbowBlackhole();
                yield return 1.8f;
            }
        }

        private IEnumerator Attack52Sequence()
        {
            while (true)
            {
                yield return bladeThrower();
                yield return 0.4f;
                yield return starsMeteorite();
                yield return 0.6f;
                yield return bigBeamBall();
                yield return 1.8f;
            }
        }

        private IEnumerator Attack53Sequence()
        {
            startShootCharge();
            while (true)
            {
                for (int i = 0; i < 3; i++)
                {
                    var entity = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            shootAt(entity.Center);
                            yield return 0.12f;
                        }
                    }
                    yield return 0.4f;
                    startShootCharge();
                }
                yield return 1.0f;
                yield return rainbowBlackhole();
                yield return 1.8f;
                startShootCharge();
            }
        }

        private IEnumerator Attack54Sequence()
        {
            while (true)
            {
                yield return beam();
                yield return 0.3f;
                yield return fireShockwave();
                yield return 0.5f;
                yield return biggerBeam();
                yield return 1.5f;
            }
        }

        private IEnumerator Attack55Sequence()
        {
            while (true)
            {
                yield return bigBeamBall();
                yield return 0.4f;
                yield return bladeThrower();
                yield return 0.6f;
                yield return starsMeteorite();
                yield return 1.8f;
            }
        }

        private IEnumerator Attack56Sequence()
        {
            while (true)
            {
                for (int i = 0; i < 2; i++)
                {
                    yield return rainbowBlackhole();
                    yield return 0.8f;
                    yield return fireShockwave();
                    yield return 0.8f;
                }
                yield return 2.0f;
            }
        }

        private IEnumerator Attack57Sequence()
        {
            startShootCharge();
            while (true)
            {
                for (int i = 0; i < 5; i++)
                {
                    yield return 0.25f;
                    shoot();
                    yield return 0.1f;
                    startShootCharge();
                }
                yield return 0.6f;
                yield return biggerBeam();
                yield return 0.6f;
                yield return bladeThrower();
                yield return 1.5f;
                startShootCharge();
            }
        }

        private IEnumerator Attack58Sequence()
        {
            while (true)
            {
                yield return starsMeteorite();
                yield return 0.5f;
                yield return bigBeamBall();
                yield return 0.5f;
                yield return fireShockwave();
                yield return 0.5f;
                yield return rainbowBlackhole();
                yield return 2.0f;
            }
        }

        private IEnumerator Attack59Sequence()
        {
            while (true)
            {
                yield return beam();
                yield return 0.3f;
                yield return bladeThrower();
                yield return 0.3f;
                yield return biggerBeam();
                yield return 0.5f;
                yield return fireShockwave();
                yield return 1.5f;
            }
        }

        private IEnumerator Attack60Sequence()
        {
            // CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK Ã¢â€ â€™ CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK_END in azzyboss-60
            if (!dialogTriggered_SecondSpecialAttack && dialog)
            {
                dialogTriggered_SecondSpecialAttack = true;
                stopAttacking();
                yield return Textbox.Say("DZ_CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK");
                yield return 0.3f;
                yield return Textbox.Say("DZ_CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK_END");
                yield return 0.5f;
            }
            startShootCharge();
            while (true)
            {
                // Ultimate chaos pattern
                for (int i = 0; i < 10; i++)
                {
                    var entity = level.Tracker.GetEntity<global::Celeste.Player>();
                    if (entity != null)
                    {
                        shootAt(entity.Center);
                    }
                    yield return 0.15f;
                    startShootCharge();
                    yield return 0.15f;
                }
                yield return 0.5f;
                yield return bigBeamBall();
                yield return 0.5f;
                yield return bladeThrower();
                yield return 0.5f;
                yield return rainbowBlackhole();
                yield return 0.5f;
                yield return fireShockwave();
                yield return 0.5f;
                yield return starsMeteorite();
                yield return 0.8f;
                yield return biggerBeam();
                yield return 2.5f;
                startShootCharge();
            }
        }

        private IEnumerator Attack61Sequence()
        {
            while (true)
            {
                // Hyper Goner ultimate attack
                yield return HyperGonerSequence();
                yield return 2.0f;
                
                // Follow up with chaos attacks
                startShootCharge();
                for (int i = 0; i < 3; i++)
                {
                    yield return 0.3f;
                    shoot();
                    yield return 0.2f;
                    startShootCharge();
                }
                yield return 1.5f;
            }
        }

        private void shootAt(Vector2 target)
        {
            var entity = level?.Tracker.GetEntity<global::Celeste.Player>();
            if (entity == null || Sprite == null) return;

            currentAttackPhase = AttackPhase.Executing;
            var projectile = new AsrielGodBossShot().Init(this, entity, 0f);
            Scene.Add(projectile);
            Audio.Play(SFX_GUNSHOT, Position);

            Add(new Coroutine(SetRecoveryPhase()));
        }

        private void shoot(float angleOffset = 0.0f)
        {
            var entity = level?.Tracker.GetEntity<global::Celeste.Player>();
            if (entity == null || Sprite == null) return;

            currentAttackPhase = AttackPhase.Executing;
            var projectile = new AsrielGodBossShot().Init(this, entity, angleOffset);
            Scene.Add(projectile);
            Audio.Play(SFX_BIG_BULLET_FIRE, Position);

            Add(new Coroutine(SetRecoveryPhase()));
        }

        private IEnumerator SetRecoveryPhase()
        {
            yield return 0.2f;
            currentAttackPhase = AttackPhase.Recovery;
        }

        private IEnumerator beam()
        {
            var asrielboss = this;
            currentAttackPhase = AttackPhase.Charging;
            asrielboss.laserSfx.Play(SFX_BOSS_LASER_CHARGE);
            asrielboss.Sprite.Play("beamStart", true);
            yield return 0.1f;
            var entity = asrielboss.level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                currentAttackPhase = AttackPhase.Executing;
                asrielboss.level.Add(new AsrielGodBossBeam().Init(asrielboss, entity));
            }

            yield return 0.9f;
            asrielboss.Sprite.Play("beam", true);
            yield return 0.5f;
            asrielboss.laserSfx.Stop();
            Audio.Play(SFX_BOSS_LASER_FIRE, asrielboss.Position);
            asrielboss.Sprite.Play("idle");
            currentAttackPhase = AttackPhase.Recovery;
        }

        private IEnumerator biggerBeam()
        {
            var asrielboss = this;
            currentAttackPhase = AttackPhase.Charging;
            asrielboss.laserSfx.Play(SFX_BOSS_LASER_CHARGE);
            asrielboss.Sprite.Play("beamStart", true);
            yield return 0.2f;
            var entity = asrielboss.level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                currentAttackPhase = AttackPhase.Executing;
                asrielboss.level.Add(new AsrielGodBossBiggerBeam().Init(asrielboss, entity));
            }

            yield return 1.4f;
            asrielboss.Sprite.Play("beam", true);
            yield return 0.6f;
            asrielboss.laserSfx.Stop();
            Audio.Play(SFX_BIGGER_GUN_MECHANIZED, asrielboss.Position);
            asrielboss.Sprite.Play("idle");
            currentAttackPhase = AttackPhase.Recovery;
        }

        private IEnumerator bigBeamBall()
        {
            var asrielboss = this;
            currentAttackPhase = AttackPhase.Charging;
            asrielboss.laserSfx.Play(SFX_BOSS_LASER_CHARGE);
            asrielboss.Sprite.Play("beamStart", true);
            yield return 0.3f;
            var entity = asrielboss.level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                currentAttackPhase = AttackPhase.Executing;
                asrielboss.level.Add(new AsrielGodBossBigBeamBall().Init(asrielboss, entity));
            }

            yield return 1.0f;
            asrielboss.laserSfx.Stop();
            Audio.Play(SFX_CINEMATIC_CUT, asrielboss.Position);
            asrielboss.Sprite.Play("idle");
            currentAttackPhase = AttackPhase.Recovery;
        }

        private IEnumerator rainbowBlackhole()
        {
            var asrielboss = this;
            currentAttackPhase = AttackPhase.Charging;
            Audio.Play(SFX_LIGHTNING_HIT, asrielboss.Position);
            yield return 0.5f;
            var entity = asrielboss.level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                currentAttackPhase = AttackPhase.Executing;
                asrielboss.level.Add(new AsrielGodBossRainbowBlackhole(entity.Position, AsrielGodBossRainbowBlackhole.LIFETIME));
            }

            yield return AsrielGodBossRainbowBlackhole.LIFETIME;
            currentAttackPhase = AttackPhase.Recovery;
        }

        private IEnumerator bladeThrower()
        {
            var asrielboss = this;
            currentAttackPhase = AttackPhase.Charging;
            Audio.Play(SFX_GRAB, asrielboss.Position);
            yield return 0.3f;
            var entity = asrielboss.level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                currentAttackPhase = AttackPhase.Executing;
                for (int i = 0; i < 5; i++)
                {
                    asrielboss.level.Add(new AsrielGodBossBlade().Init(asrielboss, entity, i * 72f)); // 5 blades in circle
                    yield return 0.1f;
                }
            }

            yield return 1.0f;
            currentAttackPhase = AttackPhase.Recovery;
        }

        private IEnumerator fireShockwave()
        {
            var asrielboss = this;
            currentAttackPhase = AttackPhase.Charging;
            Audio.Play(SFX_SPELLCAST_GLITCH, asrielboss.Position);
            yield return 0.4f;
            var entity = asrielboss.level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                currentAttackPhase = AttackPhase.Executing;
                asrielboss.level.Add(new AsrielGodBossFireShockwave(entity.Position));
            }

            yield return 2.0f;
            currentAttackPhase = AttackPhase.Recovery;
        }

        private IEnumerator starsMeteorite()
        {
            var asrielboss = this;
            currentAttackPhase = AttackPhase.Charging;
            Audio.Play(SFX_STAR, asrielboss.Position);
            yield return 0.5f;
            var entity = asrielboss.level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                currentAttackPhase = AttackPhase.Executing;
                for (int i = 0; i < 8; i++)
                {
                    Vector2 spawnPos = new Vector2(Calc.Random.Range(level.Bounds.Left, level.Bounds.Right), level.Bounds.Top - 20);
                    asrielboss.level.Add(new AsrielGodBossMeteorite(spawnPos, entity.Position));
                    yield return 0.2f;
                }
            }

            yield return 2.0f;
            currentAttackPhase = AttackPhase.Recovery;
        }

        private IEnumerator StarDeathBlackhole()
        {
            var asrielboss = this;
            currentAttackPhase = AttackPhase.Charging;
            Audio.Play("event:/Mods/pusheen/new_content/char/els/Els_StarDeath", asrielboss.Position);
            yield return 0.5f;
            var entity = asrielboss.level.Tracker.GetEntity<global::Celeste.Player>();
            if (entity != null)
            {
                currentAttackPhase = AttackPhase.Executing;
                for (int i = 0; i < 8; i++)
                {
                    Vector2 spawnPos = new Vector2(Calc.Random.Range(level.Bounds.Left, level.Bounds.Right), level.Bounds.Top - 20);
                    asrielboss.level.Add(new SupernovaStarDeathBlackhole(spawnPos, entity.Position));
                    yield return 0.2f;
                }
            }

            yield return 2.0f;
            currentAttackPhase = AttackPhase.Recovery;
        }

        private const float HYPERGONER_DURATION = 6.0f;

        private IEnumerator hyperGoner()
        {
            var asrielboss = this;
            currentAttackPhase = AttackPhase.Charging;
            
            // Play the hypergoner sprite animation
            asrielboss.Sprite.Play("hypergoner", true);
            Audio.Play(SFX_HYPERGONER_CHARGE, asrielboss.Position);
            yield return 0.5f;
            
            // Start the inhale animation
            asrielboss.Sprite.Play("hypergoner_inhale", true);
            yield return 0.8f;
            
            var entity = asrielboss.level.Tracker.GetEntity<global::Celeste.Player>();
            AsrielGodBossCountdownOverlay countdownHg = null;
            if (entity != null)
            {
                currentAttackPhase = AttackPhase.Executing;
                
                // Create the vortex effect with countdown timer
                const float PULL_STRENGTH = 150f;
                const float SAFE_DISTANCE = 40f;
                asrielboss.level.Add(new HyperGonerVortex(asrielboss.Position, PULL_STRENGTH, SAFE_DISTANCE));
                countdownHg = new AsrielGodBossCountdownOverlay(asrielboss, HYPERGONER_DURATION, "HyperGoner");
                asrielboss.level.Add(countdownHg);
                
                // Play laughing animation
                asrielboss.Sprite.Play("hypergoner_laughing", true);
            }

            // Duration of the vortex effect
            yield return HYPERGONER_DURATION;
            
            // Return to idle
            asrielboss.Sprite?.Play("idle", true);
            currentAttackPhase = AttackPhase.Recovery;
            
            // Check if we should trigger the phase transition cutscene
            // This happens after the HyperGoner attack when player survives
            if (!hyperGonerPhaseTransitionTriggered && ShouldTriggerAngelPhaseTransition())
            {
                hyperGonerPhaseTransitionTriggered = true;
                
                // Check if we should teleport to hypergoner room or play cutscene
                string currentRoom = level.Session.Level;
                if (currentRoom.Equals("azzyboss-hypergoner", StringComparison.OrdinalIgnoreCase))
                {
                    // Already in hypergoner room, play the cutscene
                    asrielboss.level?.Add(new CS20_BossEnd());
                }
                else
                {
                    // Teleport player to azzyboss-hypergoner room
                    yield return TeleportToHyperGonerRoom();
                }
            }
        }

        /// <summary>
        /// Teleports the player to the azzyboss-hypergoner room for the Angel of Death phase.
        /// </summary>
        private IEnumerator TeleportToHyperGonerRoom()
        {
            var player = level.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                // Store strawberries
                Leader.StoreStrawberries(player.Leader);
                level.Remove(player);
            }
            
            // Unload current level and prepare for room transition
            level.UnloadLevel();
            
            // Set the target room
            level.Session.Level = "azzyboss-hypergoner";
            level.Session.RespawnPoint = level.GetSpawnPoint(new Vector2(level.Bounds.Left, level.Bounds.Top));
            level.Session.FirstLevel = false;
            
            // Load the new room
            level.LoadLevel(global::Celeste.Player.IntroTypes.None);
            
            Logger.Log(LogLevel.Info, "DZ", "[AsrielGodBoss] Teleported player to azzyboss-hypergoner room");
            
            yield return null;
        }

        /// <summary>
        /// Alias for hyperGoner() to follow PascalCase naming convention.
        /// This is the method called by Attack61Sequence.
        /// </summary>
        private IEnumerator HyperGonerSequence() => hyperGoner();
        
        /// <summary>
        /// Determines if the Angel of Death phase transition should be triggered.
        /// Override this logic based on your specific conditions (health, room ID, etc.)
        /// </summary>
        private bool ShouldTriggerAngelPhaseTransition()
        {
            // Check if the flag hasn't been set already
            if (level.Session.GetFlag("asriel_boss_end_hypergoner"))
                return false;
            
            // Check if we're in a room where the phase transition should happen
            string currentRoomId = level.Session.Level;
            string[] transitionRoomIds = new string[]
            {
                "azzyboss-00",          // Intro room - CH20_ASRIEL_REVEAL_IDENTITY
                "azzyboss-60",          // CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK Ã¢â€ â€™ CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK_END
                "azzyboss-hypergoner",  // HyperGoner specific room
            };
            
            foreach (string id in transitionRoomIds)
            {
                if (currentRoomId.Equals(id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            // Alternatively, trigger based on health threshold
            // Uncomment the following to enable health-based transition:
            // if (currentHealth <= maxHealth * 0.1f) // When health drops below 10%
            //     return true;
            
            return false;
        }

        private List<AttackStep> parseCustomAttackSequence(string seq)
        {
            List<AttackStep> list = new List<AttackStep>();

            try
            {
                char[] splitters = new[] { ',', ';', '\n' };
                foreach (string raw in seq.Split(splitters, StringSplitOptions.RemoveEmptyEntries))
                {
                    string token = raw.Trim();
                    if (token.Length == 0) continue;

                    try
                    {
                        string[] parts = token.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 0) continue;

                        string action = parts[0].ToLowerInvariant();
                        float arg = 0f;
                        float delay = 0.3f;

                        if (parts.Length == 2)
                        {
                            if (action == nameof(shoot))
                            {
                                if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out delay))
                                    delay = 0.3f;
                            }
                            else
                            {
                                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out delay);
                            }
                        }
                        else if (parts.Length >= 3)
                        {
                            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out arg);
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out delay);
                        }

                        list.Add(new AttackStep(action, Math.Max(0f, delay), arg));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "DZ", $"Error parsing attack step: {token} - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "DZ", $"Error parsing custom attack sequence: {ex.Message}");
                return new List<AttackStep>();
            }

            return list;
        }

        public override void Removed(Scene scene)
        {
            stopAttacking();
            // Cleanup boss backdrop if it was created
            if (bossBg != null)
            {
                bossBg.Alpha = 1f;
            }
            base.Removed(scene);
        }

        private void playHitEffect()
        {
            Vector2 hitPos = Center + new Vector2(0, -Height / 2);

            for (int i = 0; i < 10; i++)
            {
                Vector2 particleVel = Calc.AngleToVector((float)(Calc.Random.NextFloat() * Math.PI * 2f),
                                 Calc.Random.Range(50f, 100f));
                Scene.Add(new HitParticle(hitPos, particleVel, Color.Red));
            }

            flashTimer = 0.2f;
        }

        private void createHitParticles(Vector2 position)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 velocity = Calc.AngleToVector((float)(Calc.Random.NextFloat() * Math.PI * 2f),
                              Calc.Random.Range(30f, 60f));
                Scene.Add(new HitParticle(position, velocity, Color.Yellow));
            }
        }

        private void addKnockback(Vector2 knockback)
        {
            knockbackVelocity = knockback;
            knockbackTimer = 0.3f;
        }

        private IEnumerator playerInvulnerabilityTimer()
        {
            float timer = 1.0f;
            while (timer > 0f)
            {
                timer -= Engine.DeltaTime;
                yield return null;
            }
            isPlayerInvulnerable = false;
        }

        private IEnumerator hurtStateTimer()
        {
            float hurtTime = 0.5f;
            while (hurtTime > 0f && currentState == BossState.Hurt)
            {
                hurtTime -= Engine.DeltaTime;
                yield return null;
            }

            if (currentState == BossState.Hurt)
            {
                SetState(BossState.Idle);
            }
        }

        private IEnumerator defeatSequence()
        {
            yield return 2.0f;
            // Trigger defeat cutscene or dialog
            // Implementation depends on cutscene system
            yield return null;
        }

        // Dialog sequence triggers for Chapter 20 Asriel boss fight
        public void TriggerDialog(string dialogKey)
        {
            // Don't start a new dialog if one is already active
            if (isDialogActive) return;
            
            isDialogActive = true;
            Add(new Coroutine(PlayDialogSequence(dialogKey)));
        }

        private IEnumerator PlayDialogSequence(string dialogKey)
        {
            switch (dialogKey)
            {
                case "DZ_CH20_ASRIEL_REVEAL_IDENTITY":
                    yield return PlayDialog_RevealIdentity();
                    break;
                case "DZ_CH20_HEART_REFUSAL_PHASE4":
                    yield return PlayDialog_HeartRefusalPhase4();
                    break;
                case "DZ_CH20_MADELINE_BADELINE_RETURN":
                    yield return PlayDialog_MadelineBadelineReturn();
                    break;
                case "DZ_CH20_ASRIEL_ZERO_TRANSFORMATION":
                    yield return PlayDialog_AsrielZeroTransformation();
                    break;
                case "DZ_CH20_ASRIEL_HEART_LOST_SOUL":
                    yield return PlayDialog_AsrielHeartLostSoul();
                    break;
                case "DZ_CH20_ASRIEL_REMEMBER_A":
                    yield return PlayDialog_AsrielRememberA();
                    break;
                case "DZ_CH20_ASRIEL_REMEMBER_B":
                    yield return PlayDialog_AsrielRememberB();
                    break;
                case "DZ_CH20_ASRIEL_REMEMBER_C":
                    yield return PlayDialog_AsrielRememberC();
                    break;
                case "DZ_CH20_ASRIEL_REMEMBER_D":
                    yield return PlayDialog_AsrielRememberD();
                    break;
                case "DZ_CH20_ASRIEL_REMEMBER_E":
                    yield return PlayDialog_AsrielRememberE();
                    break;
                case "DZ_CH20_ASRIEL_REMEMBER_FINAL":
                    yield return PlayDialog_AsrielRememberFinal();
                    break;
                case "DZ_CH20_ASRIEL_REMEMBER_F":
                    yield return PlayDialog_AsrielRememberF();
                    break;
                case "DZ_CH20_ASRIEL_BOSS_END":
                    yield return PlayDialog_AsrielBossEnd();
                    break;
                case "DZ_CH20_KIRBY_ASK_WHY":
                    yield return PlayDialog_KirbyAskWhy();
                    break;
                case "DZ_CH20_KIRBY_ASK_WHAT":
                    yield return PlayDialog_KirbyAskWhat();
                    break;
                case "DZ_CH20_KIRBY_DO_NOT_WANT_TO_FIGHT":
                    yield return PlayDialog_KirbyDoNotWantToFight();
                    break;
                case "DZ_CH20_KIRBY_STRUCK_DOWN":
                    yield return PlayDialog_KirbyStruckDown();
                    break;
                case "DZ_CH20_KIRBY_REFUSED_TO_DIE":
                    yield return PlayDialog_KirbyRefusedToDie();
                    break;
                case "DZ_CH20_ASRIEL_FIRST_SPECIAL_ATTACK":
                    yield return PlayDialog_AsrielFirstSpecialAttack();
                    break;
                case "DZ_CH20_MADELINE_AND_BADELINE_SAVE_KIRBY_FROM_ASRIEL_FIRST_SPECIAL_ATTACK":
                    yield return PlayDialog_MadelineSaveKirbyFromFirstSpecialAttack();
                    break;
                case "DZ_CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK":
                    yield return PlayDialog_AsrielGodSecondSpecialAttack();
                    break;
                case "DZ_CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK_END":
                    yield return PlayDialog_AsrielGodSecondSpecialAttackEnd();
                    break;
            }
            
            // Mark dialog as finished
            isDialogActive = false;
        }

        private IEnumerator PlayDialog_RevealIdentity()
        {
            // CH20_ASRIEL_REVEAL_IDENTITY dialog sequence with triggers
            yield return Textbox.Say("ch20_asriel_reveal_identity", new Func<IEnumerator>[] {
                RevealIdentity_KirbyWalksCloser,
                RevealIdentity_AsrielTurnaround,
                RevealIdentity_AsrielTurnIntoGodAndFinalWriter,
            });
        }

        private IEnumerator PlayDialog_HeartRefusalPhase4()
        {
            // CH20_HEART_REFUSAL_PHASE4 dialog sequence
            yield return Textbox.Say("DZ_CH20_HEART_REFUSAL_PHASE4");
        }

        private IEnumerator PlayDialog_MadelineBadelineReturn()
        {
            // CH20_MADELINE_BADELINE_RETURN dialog sequence
            yield return Textbox.Say("DZ_CH20_MADELINE_BADELINE_RETURN");
        }

        private IEnumerator PlayDialog_AsrielZeroTransformation()
        {
            // CH20_ASRIEL_ZERO_TRANSFORMATION dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_ZERO_TRANSFORMATION");
        }

        private IEnumerator PlayDialog_AsrielHeartLostSoul()
        {
            // CH20_ASRIEL_HEART_LOST_SOUL dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_HEART_LOST_SOUL");
        }

        private IEnumerator PlayDialog_AsrielRememberA()
        {
            // CH20_ASRIEL_REMEMBER_A dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_REMEMBER_A");
        }

        private IEnumerator PlayDialog_AsrielRememberB()
        {
            // CH20_ASRIEL_REMEMBER_B dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_REMEMBER_B");
        }

        private IEnumerator PlayDialog_AsrielRememberC()
        {
            // CH20_ASRIEL_REMEMBER_C dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_REMEMBER_C");
        }

        private IEnumerator PlayDialog_AsrielRememberD()
        {
            // CH20_ASRIEL_REMEMBER_D dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_REMEMBER_D");
        }

        private IEnumerator PlayDialog_AsrielRememberE()
        {
            // CH20_ASRIEL_REMEMBER_E dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_REMEMBER_E");
        }

        private IEnumerator PlayDialog_AsrielRememberFinal()
        {
            // CH20_ASRIEL_REMEMBER_FINAL dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_REMEMBER_FINAL");
        }

        private IEnumerator PlayDialog_AsrielRememberF()
        {
            // CH20_ASRIEL_REMEMBER_F dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_REMEMBER_F");
        }

        private IEnumerator PlayDialog_AsrielBossEnd()
        {
            // CH20_ASRIEL_BOSS_END dialog sequence
            yield return Textbox.Say("DZ_CH20_ASRIEL_BOSS_END");
        }

        private IEnumerator PlayDialog_KirbyAskWhy()
        {
            yield return Textbox.Say("DZ_CH20_KIRBY_ASK_WHY");
        }

        private IEnumerator PlayDialog_KirbyAskWhat()
        {
            yield return Textbox.Say("DZ_CH20_KIRBY_ASK_WHAT");
        }

        private IEnumerator PlayDialog_KirbyDoNotWantToFight()
        {
            yield return Textbox.Say("DZ_CH20_KIRBY_DO_NOT_WANT_TO_FIGHT");
        }

        private IEnumerator PlayDialog_KirbyStruckDown()
        {
            yield return Textbox.Say("DZ_CH20_KIRBY_STRUCK_DOWN");
        }

        private IEnumerator PlayDialog_KirbyRefusedToDie()
        {
            yield return Textbox.Say("DZ_CH20_KIRBY_REFUSED_TO_DIE");
        }

        private IEnumerator PlayDialog_AsrielFirstSpecialAttack()
        {
            yield return Textbox.Say("DZ_CH20_ASRIEL_FIRST_SPECIAL_ATTACK");
        }

        private IEnumerator PlayDialog_MadelineSaveKirbyFromFirstSpecialAttack()
        {
            yield return Textbox.Say("DZ_CH20_MADELINE_AND_BADELINE_SAVE_KIRBY_FROM_ASRIEL_FIRST_SPECIAL_ATTACK");
        }

        private IEnumerator PlayDialog_AsrielGodSecondSpecialAttack()
        {
            yield return Textbox.Say("DZ_CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK");
        }

        private IEnumerator PlayDialog_AsrielGodSecondSpecialAttackEnd()
        {
            yield return Textbox.Say("DZ_CH20_ASRIEL_GOD_SECOND_SPECIAL_ATTACK_END");
        }

        // Dialog trigger flags
#pragma warning disable CS0414
        private bool dialogTriggered_Phase3 = false;
#pragma warning restore CS0414
        private bool isDialogActive = false;
        private bool dialogTriggered_AskWhy = false;
        private bool dialogTriggered_AskWhat = false;
        private bool dialogTriggered_DoNotWantToFight = false;
        private bool dialogTriggered_StruckDown = false;
        private bool dialogTriggered_RefusedToDie = false;
        private bool dialogTriggered_FirstSpecialAttack = false;
        private bool dialogTriggered_SecondSpecialAttack = false;
        
        #endregion

        #region Asriel Reveal Identity Cutscene (merged from CS20_AsrielRevealIdentity)

        private const string RevealIdentityFlag = "asriel_reveal_identity";
        private global::Celeste.Player revealPlayer;
        private Entity revealAsrielDummy;
        private Sprite revealAsrielSprite;

        /// <summary>
        /// Main cutscene for Asriel's identity reveal.
        /// Shows Asriel facing away (back sprite), then flipping to face Kirby (idle sprite)
        /// when the dramatic revelation happens.
        /// </summary>
        private IEnumerator RevealIdentityCutscene(string roomId)
        {
            Logger.Log(LogLevel.Info, "DZ", $"AsrielGodBoss: Starting dialog intro cutscene (RevealIdentityCutscene) for room '{roomId}'");
            
            // Wait for player to be available
            while (revealPlayer == null)
            {
                revealPlayer = Scene.Tracker.GetEntity<global::Celeste.Player>();
                yield return null;
            }

            // Lock player in dummy state
            revealPlayer.StateMachine.State = Player.StDummy; // Player.StDummy
            revealPlayer.StateMachine.Locked = true;

            // Wait for player to land if in air
            while (!revealPlayer.OnGround())
            {
                yield return null;
            }

            // Use the boss itself as the Asriel entity, or find an AsrielDummy
            revealAsrielDummy = level.Entities.FindFirst<AsrielDummy>();
            if (revealAsrielDummy == null)
            {
                // Use the boss entity itself
                revealAsrielDummy = this;
                revealAsrielSprite = this.NormalSprite ?? (this.Sprite as Sprite);
            }
            else
            {
                revealAsrielSprite = revealAsrielDummy.Get<Sprite>();
            }

            // Start with Asriel facing away (back animation)
            if (revealAsrielSprite != null && revealAsrielSprite.Has("back"))
            {
                revealAsrielSprite.Play("back");
            }

            // Brief pause for tension
            yield return 0.5f;

            // Zoom in on the confrontation
            yield return level.ZoomTo(new Vector2(160f, 90f), 2f, 0.5f);

            // Small pause
            yield return 0.3f;

            // Main dialogue with triggers
            Logger.Log(LogLevel.Info, "DZ", "AsrielGodBoss: Starting dialog 'ch20_asriel_reveal_identity'");
            yield return Textbox.Say("ch20_asriel_reveal_identity", new Func<IEnumerator>[] {
                RevealIdentity_KirbyWalksCloser,
                RevealIdentity_AsrielTurnaround,
                RevealIdentity_AsrielTurnIntoGodAndFinalWriter,
            });
            Logger.Log(LogLevel.Info, "DZ", "AsrielGodBoss: Dialog 'ch20_asriel_reveal_identity' completed");

            // Small pause after dialogue
            yield return 0.3f;

            // Zoom back out
            yield return level.ZoomBack(0.5f);

            // Screen shake for dramatic effect
            level.Shake(0.3f);

            yield return 0.5f;

            // Start boss music now that the god boss has appeared
            level.Session.Audio.Music.Event = "event:/Mods/pusheen/new_content/music/lvl20/kirby_vs_asriel_fight_1";
            level.Session.Audio.Apply();

            // Restore player control
            if (revealPlayer != null)
            {
                revealPlayer.StateMachine.Locked = false;
                revealPlayer.StateMachine.State = Player.StNormal; // Player.StNormal
            }

            // Set flag to prevent re-triggering (use room-specific flag if roomId provided)
            string flagToSet = !string.IsNullOrEmpty(roomId) ? $"{RevealIdentityFlag}_{roomId}" : RevealIdentityFlag;
            level.Session.SetFlag(flagToSet);
            Logger.Log(LogLevel.Info, "DZ", $"AsrielGodBoss: Dialog intro cutscene completed, flag '{flagToSet}' set");
        }

        /// <summary>
        /// Kirby walks closer to Asriel for the confrontation
        /// </summary>
        private IEnumerator RevealIdentity_KirbyWalksCloser()
        {
            Logger.Log(LogLevel.Info, "DZ", "AsrielGodBoss: Trigger 0 - RevealIdentity_KirbyWalksCloser started");
            if (revealPlayer == null || revealAsrielDummy == null)
            {
                Logger.Log(LogLevel.Warn, "DZ", $"AsrielGodBoss: Trigger 0 - Aborting (revealPlayer={revealPlayer != null}, revealAsrielDummy={revealAsrielDummy != null})");
                yield break;
            }

            // Calculate target position (closer to Asriel)
            float targetX = revealAsrielDummy.X - 50f;
            Logger.Log(LogLevel.Info, "DZ", $"AsrielGodBoss: Trigger 0 - Player at X={revealPlayer.X}, target X={targetX}");
            
            // Make Kirby walk towards Asriel
            revealPlayer.Facing = (Facings)Math.Sign(revealAsrielDummy.X - revealPlayer.X);
            
            float timeout = 5f; // 5 second timeout to prevent infinite loop
            while (Math.Abs(revealPlayer.X - targetX) > 4f && timeout > 0f)
            {
                float moveDir = Math.Sign(targetX - revealPlayer.X);
                revealPlayer.Speed.X = moveDir * 64f;
                timeout -= Engine.DeltaTime;
                yield return null;
            }
            
            if (timeout <= 0f)
                Logger.Log(LogLevel.Warn, "DZ", "AsrielGodBoss: Trigger 0 - Walk timeout reached");
            
            revealPlayer.Speed.X = 0f;
            Logger.Log(LogLevel.Info, "DZ", "AsrielGodBoss: Trigger 0 - RevealIdentity_KirbyWalksCloser completed");
            yield return 0.2f;
        }

        /// <summary>
        /// Asriel turns around to face Kirby
        /// </summary>
        private IEnumerator RevealIdentity_AsrielTurnaround()
        {
            Logger.Log(LogLevel.Info, "DZ", "AsrielGodBoss: Trigger 1 - RevealIdentity_AsrielTurnaround started");
            if (revealAsrielSprite == null)
            {
                Logger.Log(LogLevel.Warn, "DZ", "AsrielGodBoss: Trigger 1 - Aborting (revealAsrielSprite is null)");
                yield break;
            }

            // Small pause before turn
            yield return 0.3f;

            // Play turn animation if available
            if (revealAsrielSprite.Has("turn"))
            {
                revealAsrielSprite.Play("turn");
                yield return 0.4f;
            }

            // Flip to idle animation (facing Kirby)
            if (revealAsrielSprite.Has("idle"))
            {
                revealAsrielSprite.Play("idle");
            }

            // Ensure Asriel is facing Kirby
            if (revealPlayer != null)
            {
                float direction = Math.Sign(revealPlayer.X - revealAsrielDummy.X);
                revealAsrielSprite.Scale.X = direction != 0 ? direction : -1f;
            }

            // Screen shake for dramatic effect
            level?.Shake(0.3f);

            Logger.Log(LogLevel.Info, "DZ", "AsrielGodBoss: Trigger 1 - RevealIdentity_AsrielTurnaround completed");
            yield return 0.2f;
        }

        /// <summary>
        /// Asriel transforms into god form with dramatic effects and distorted name reveal
        /// </summary>
        private IEnumerator RevealIdentity_AsrielTurnIntoGodAndFinalWriter()
        {
            Logger.Log(LogLevel.Info, "DZ", "AsrielGodBoss: Trigger 2 - RevealIdentity_AsrielTurnIntoGodAndFinalWriter started");
            // Play the Asriel create sound effect
            Audio.Play("event:/Mods/pusheen/new_content/char/bosses/asriel/create");

            // Screen flash for dramatic effect
            if (level != null)
            {
                level.Flash(Color.White * 0.8f, false);
                level.Shake(0.5f);
                
                // Displacement burst
                if (revealAsrielDummy != null)
                {
                    level.Displacement.AddBurst(
                        revealAsrielDummy.Position,
                        1.0f,
                        64f,
                        128f,
                        1.5f
                    );
                }
            }

            yield return 0.3f;

            // Create FancyText with distortion font
            FancyText.Text text = FancyText.Parse(
                Dialog.Get("ch20_asriel_name_reveal"),
                (int)(1920f * 0.7f),
                -1,
                1f,
                Color.White,
                Dialog.Language
            );

            // Display the distorted text
            float textAlpha = 0f;
            float textScale = 2f;
            Vector2 textPosition = new Vector2(960f, 540f);

            // Fade in with distortion effect
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
            {
                textAlpha = Ease.SineOut(t);
                textScale = MathHelper.Lerp(2.5f, 1.5f, Ease.SineOut(t));
                yield return null;
            }

            yield return 1.0f;

            // Fade out
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
            {
                textAlpha = 1f - Ease.SineIn(t);
                yield return null;
            }

            Logger.Log(LogLevel.Info, "DZ", "AsrielGodBoss: Trigger 2 - RevealIdentity_AsrielTurnIntoGodAndFinalWriter completed");
            yield return 0.2f;
        }

        #endregion
    }

    internal class SupernovaStarDeathBlackhole : Entity
    {
        private Vector2 spawnPos;

        public SupernovaStarDeathBlackhole(Vector2 spawnPos, Vector2 position)
        {
            this.spawnPos = spawnPos;
            Position = position;
        }
    }

    /// <summary>
    /// Projectile shot by AsrielGodBoss using spr_firebullet sprite with rainbow hue cycling.
    /// </summary>
    [Tracked]
    public class AsrielGodBossShot : Entity
    {
        private const float SPEED = 200f;
        private const float LIFETIME = 6f;
        private Vector2 velocity;
        private float timer;
        private Sprite sprite;
        private float hueTime;

        public AsrielGodBossShot Init(AsrielGodBoss boss, global::Celeste.Player target, float angleOffset)
        {
            Position = boss.ShotOrigin;
            Collider = new Monocle.Circle(4f);
            Add(new PlayerCollider(OnPlayer));
            Depth = -1000000;

            Vector2 dir = (target.Center - Position).SafeNormalize();
            if (angleOffset != 0f)
                dir = Calc.AngleToVector(dir.Angle() + MathHelper.ToRadians(angleOffset), 1f);
            velocity = dir * SPEED;
            timer = LIFETIME;

            // Use spr_firebullet sprite from asrielgodboss sprite bank
            try
            {
                sprite = GFX.SpriteBank.Create("asrielgodboss");
                sprite.Play("firebullet");
                sprite.Scale = Vector2.One * 1.2f;
                Add(sprite);
            }
            catch
            {
                // fallback: drawn in Render()
            }

            Add(new VertexLight(Color.Cyan, 0.8f, 16, 32));
            return this;
        }

        private static Color RainbowFromHue(float hue)
        {
            hue = ((hue % 1f) + 1f) % 1f;
            int i = (int)(hue * 6);
            float f = hue * 6 - i;
            return (i % 6) switch
            {
                0 => new Color(1f, f, 0f),
                1 => new Color(1f - f, 1f, 0f),
                2 => new Color(0f, 1f, f),
                3 => new Color(0f, 1f - f, 1f),
                4 => new Color(f, 0f, 1f),
                _ => new Color(1f, 0f, 1f - f)
            };
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            timer -= Engine.DeltaTime;
            hueTime += Engine.DeltaTime * 3f;

            // Cycle hue on the sprite each frame
            if (sprite != null)
                sprite.Color = RainbowFromHue(hueTime);

            if (timer <= 0f || !SceneAs<Level>().IsInBounds(Position, 16f))
            {
                RemoveSelf();
                return;
            }
        }

        public override void Render()
        {
            base.Render();
            if (sprite == null)
            {
                Color c = RainbowFromHue(hueTime);
                Draw.Circle(Position, 5f, c, 10);
                Draw.Circle(Position, 2f, Color.White, 6);
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            try { player.Die((player.Center - Position).SafeNormalize()); }
            catch (Exception) { /* third-party mod hook crash */ }
            RemoveSelf();
        }
    }

    /// <summary>
    /// Laser beam fired by AsrielGodBoss toward the player.
    /// </summary>
    [Tracked]
    public class AsrielGodBossBeam : Entity
    {
        private const float CHARGE_TIME = 1.4f;
        private const float FOLLOW_TIME = 0.9f;
        private const float ACTIVE_TIME = 0.12f;
        private AsrielGodBoss boss;
        private global::Celeste.Player target;
        private float chargeTimer;
        private float activeTimer;
        private float angle;
        private float beamAlpha;
        private float sideDist = 1f;

        public AsrielGodBossBeam Init(AsrielGodBoss boss, global::Celeste.Player target)
        {
            this.boss = boss;
            this.target = target;
            chargeTimer = CHARGE_TIME;
            activeTimer = ACTIVE_TIME;
            angle = (target.Center - boss.BeamOrigin).Angle();
            Depth = -1000000;
            return this;
        }

        public override void Update()
        {
            base.Update();
            if (boss == null || boss.Scene == null) { RemoveSelf(); return; }

            if (chargeTimer > 0f)
            {
                // Track player during charge/follow phase
                chargeTimer -= Engine.DeltaTime;
                if (chargeTimer > CHARGE_TIME - FOLLOW_TIME && target != null)
                    angle = Calc.Approach(angle, (target.Center - boss.BeamOrigin).Angle(), 6f * Engine.DeltaTime);
                beamAlpha = Calc.Approach(beamAlpha, 1f, Engine.DeltaTime * 2f);
            }
            else if (activeTimer > 0f)
            {
                // Fire phase - check collision
                activeTimer -= Engine.DeltaTime;
                sideDist = 12f;
                Vector2 origin = boss.BeamOrigin;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                for (float d = 0f; d < 320f; d += 4f)
                {
                    Vector2 point = origin + dir * d;
                    if (target != null && Vector2.Distance(point, target.Center) < 8f)
                    {
                        try { target.Die((target.Center - origin).SafeNormalize()); }
                        catch (Exception) { /* third-party mod hook crash */ }
                        break;
                    }
                }
            }
            else
            {
                RemoveSelf();
            }
        }

        private static Color RainbowBeamColor(float timeActive, float offset)
        {
            float hue = (timeActive * 1.5f + offset) % 1f;
            int i = (int)(hue * 6);
            float f = hue * 6 - i;
            return (i % 6) switch
            {
                0 => new Color(1f, f, 0f),
                1 => new Color(1f - f, 1f, 0f),
                2 => new Color(0f, 1f, f),
                3 => new Color(0f, 1f - f, 1f),
                4 => new Color(f, 0f, 1f),
                _ => new Color(1f, 0f, 1f - f)
            };
        }

        public override void Render()
        {
            Vector2 origin = boss?.BeamOrigin ?? Position;
            Vector2 dir = Calc.AngleToVector(angle, 1f);
            float t = boss?.Scene?.TimeActive ?? 0f;

            if (chargeTimer > 0f)
            {
                // Rainbow charge line
                Color chargeCol = RainbowBeamColor(t, 0f) * beamAlpha * 0.5f;
                Draw.Line(origin, origin + dir * 320f, chargeCol);
                Draw.Line(origin, origin + dir * 320f, Color.White * beamAlpha * 0.15f);
            }
            else
            {
                // Rainbow active beam (Badeline-style multi-line)
                for (int i = -2; i <= 2; i++)
                {
                    Color lineCol = RainbowBeamColor(t, i * 0.08f);
                    Vector2 perp = dir.Perpendicular() * i * sideDist * 0.4f;
                    Draw.Line(origin + perp, origin + perp + dir * 320f, lineCol * (i == 0 ? 1f : 0.6f));
                }
            }
        }
    }

    /// <summary>
    /// Larger beam variant with wider hitbox.
    /// </summary>
    [Tracked]
    public class AsrielGodBossBiggerBeam : Entity
    {
        private const float CHARGE_TIME = 1.8f;
        private const float FOLLOW_TIME = 1.2f;
        private const float ACTIVE_TIME = 0.2f;
        private AsrielGodBoss boss;
        private global::Celeste.Player target;
        private float chargeTimer;
        private float activeTimer;
        private float angle;
        private float beamAlpha;
        private float beamWidth = 2f;

        public AsrielGodBossBiggerBeam Init(AsrielGodBoss boss, global::Celeste.Player target)
        {
            this.boss = boss;
            this.target = target;
            chargeTimer = CHARGE_TIME;
            activeTimer = ACTIVE_TIME;
            angle = (target.Center - boss.BeamOrigin).Angle();
            Depth = -1000000;
            return this;
        }

        public override void Update()
        {
            base.Update();
            if (boss == null || boss.Scene == null) { RemoveSelf(); return; }

            if (chargeTimer > 0f)
            {
                chargeTimer -= Engine.DeltaTime;
                if (chargeTimer > CHARGE_TIME - FOLLOW_TIME && target != null)
                    angle = Calc.Approach(angle, (target.Center - boss.BeamOrigin).Angle(), 4f * Engine.DeltaTime);
                beamAlpha = Calc.Approach(beamAlpha, 1f, Engine.DeltaTime * 2f);
            }
            else if (activeTimer > 0f)
            {
                activeTimer -= Engine.DeltaTime;
                beamWidth = 24f;
                Vector2 origin = boss.BeamOrigin;
                Vector2 dir = Calc.AngleToVector(angle, 1f);
                for (float d = 0f; d < 400f; d += 4f)
                {
                    Vector2 point = origin + dir * d;
                    if (target != null && Vector2.Distance(point, target.Center) < 14f)
                    {
                        try { target.Die((target.Center - origin).SafeNormalize()); }
                        catch (Exception) { /* third-party mod hook crash */ }
                        break;
                    }
                }
            }
            else
            {
                RemoveSelf();
            }
        }

        private static Color RainbowBeamColor(float timeActive, float offset)
        {
            float hue = (timeActive * 1.5f + offset) % 1f;
            int i = (int)(hue * 6);
            float f = hue * 6 - i;
            return (i % 6) switch
            {
                0 => new Color(1f, f, 0f),
                1 => new Color(1f - f, 1f, 0f),
                2 => new Color(0f, 1f, f),
                3 => new Color(0f, 1f - f, 1f),
                4 => new Color(f, 0f, 1f),
                _ => new Color(1f, 0f, 1f - f)
            };
        }

        public override void Render()
        {
            Vector2 origin = boss?.BeamOrigin ?? Position;
            Vector2 dir = Calc.AngleToVector(angle, 1f);
            float t = boss?.Scene?.TimeActive ?? 0f;

            if (chargeTimer > 0f)
            {
                Color chargeCol = RainbowBeamColor(t, 0f) * beamAlpha * 0.5f;
                Draw.Line(origin, origin + dir * 400f, chargeCol);
                Draw.Line(origin + dir.Perpendicular() * 2f, origin + dir.Perpendicular() * 2f + dir * 400f, Color.White * beamAlpha * 0.15f);
                Draw.Line(origin - dir.Perpendicular() * 2f, origin - dir.Perpendicular() * 2f + dir * 400f, Color.White * beamAlpha * 0.15f);
            }
            else
            {
                Vector2 perp = dir.Perpendicular();
                for (float w = -beamWidth / 2f; w <= beamWidth / 2f; w += 2f)
                {
                    float offsetFrac = w / beamWidth;
                    Color lineCol = RainbowBeamColor(t, offsetFrac * 0.2f);
                    Draw.Line(origin + perp * w, origin + perp * w + dir * 400f, lineCol);
                }
            }
        }
    }

    /// <summary>
    /// Big energy ball that tracks toward the player.
    /// </summary>
    [Tracked]
    public class AsrielGodBossBigBeamBall : Entity
    {
        private const float SPEED = 120f;
        private const float LIFETIME = 5f;
        private Vector2 velocity;
        private float timer;
        private global::Celeste.Player target;

        public AsrielGodBossBigBeamBall Init(AsrielGodBoss boss, global::Celeste.Player target)
        {
            Position = boss.ShotOrigin;
            this.target = target;
            Collider = new Monocle.Circle(10f);
            Add(new PlayerCollider(OnPlayer));
            Depth = -1000000;
            velocity = (target.Center - Position).SafeNormalize() * SPEED;
            timer = LIFETIME;
            Add(new VertexLight(Color.Cyan, 0.8f, 24, 48));
            return this;
        }

        public override void Update()
        {
            base.Update();
            // Gently home toward player
            if (target != null && target.Scene != null)
            {
                Vector2 desired = (target.Center - Position).SafeNormalize() * SPEED;
                velocity = Vector2.Lerp(velocity, desired, Engine.DeltaTime * 1.5f);
            }
            Position += velocity * Engine.DeltaTime;
            timer -= Engine.DeltaTime;
            if (timer <= 0f) RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            Draw.Circle(Position, 10f, Color.Cyan, 12);
            Draw.Circle(Position, 6f, Color.White, 10);
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            try { player.Die((player.Center - Position).SafeNormalize()); }
            catch (Exception) { /* third-party mod hook crash */ }
            RemoveSelf();
        }
    }

    /// <summary>
    /// Countdown timer HUD overlay for boss attacks (RainbowBlackhole, HyperGoner).
    /// </summary>
    public class AsrielGodBossCountdownOverlay : Entity
    {
        private float total;
        private float remaining;
        private AsrielGodBoss boss;
        private string label;

        public AsrielGodBossCountdownOverlay(AsrielGodBoss boss, float duration, string label)
        {
            this.boss = boss;
            this.total = duration;
            this.remaining = duration;
            this.label = label;
            Depth = -9999999;
        }

        public override void Update()
        {
            base.Update();
            remaining -= Engine.DeltaTime;
            if (remaining <= 0f) RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            if (boss == null || boss.Scene == null) return;
            Level level = SceneAs<Level>();
            if (level == null) return;

            float pct = Math.Max(0f, remaining / total);
            Vector2 screenPos = level.WorldToScreen(boss.Position) + new Vector2(0f, -40f);

            float barW = 60f;
            float barH = 6f;
            Draw.Rect(screenPos.X - barW * 0.5f - 1f, screenPos.Y - 1f, barW + 2f, barH + 2f, Color.Black * 0.7f);

            // Rainbow bar color
            float hue = (1f - pct);
            int hi = (int)(hue * 6);
            float f = hue * 6 - hi;
            Color barColor = (hi % 6) switch
            {
                0 => new Color(1f, f, 0f),
                1 => new Color(1f - f, 1f, 0f),
                2 => new Color(0f, 1f, f),
                3 => new Color(0f, 1f - f, 1f),
                4 => new Color(f, 0f, 1f),
                _ => new Color(1f, 0f, 1f - f)
            };

            Draw.Rect(screenPos.X - barW * 0.5f, screenPos.Y, barW * pct, barH, barColor);

            // Seconds remaining text
            ActiveFont.DrawOutline(
                $"{label}: {remaining:F1}s",
                screenPos + new Vector2(0f, barH + 4f),
                new Vector2(0.5f, 0f),
                Vector2.One * 0.4f,
                barColor,
                1f,
                Color.Black
            );
        }
    }

    /// <summary>
    /// Rainbow blackhole that pulls the player inward.
    /// </summary>
    [Tracked]
    public class AsrielGodBossRainbowBlackhole : Entity
    {
        private const float PULL_STRENGTH = 75f;
        private const float KILL_RADIUS = 8f;
        private const float PULL_RADIUS = 60f;
        public const float LIFETIME = 3f;
        private float timer;

        public AsrielGodBossRainbowBlackhole(Vector2 position, float lifetime = LIFETIME) : base(position)
        {
            Collider = new Monocle.Circle(KILL_RADIUS);
            Add(new PlayerCollider(OnPlayer));
            Depth = -1000000;
            timer = lifetime;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            // Add position-anchored countdown overlay next to the blackhole
            scene.Add(new AsrielGodBossBlackholeCountdown(this, timer));
        }

        public override void Update()
        {
            base.Update();
            timer -= Engine.DeltaTime;
            if (timer <= 0f) { RemoveSelf(); return; }

            // Pull player toward center
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                float dist = Vector2.Distance(player.Center, Position);
                if (dist < PULL_RADIUS && dist > 1f)
                {
                    Vector2 pullDir = (Position - player.Center).SafeNormalize();
                    float strength = PULL_STRENGTH * (1f - dist / PULL_RADIUS);
                    player.Speed += pullDir * strength * Engine.DeltaTime;
                }
            }
        }

        public override void Render()
        {
            base.Render();
            float pulse = 0.8f + (float)Math.Sin(Scene.TimeActive * 6f) * 0.2f;
            float hue = (Scene.TimeActive * 0.5f) % 1f;
            int hi = (int)(hue * 6);
            float f = hue * 6 - hi;
            Color rainbowCol = (hi % 6) switch
            {
                0 => new Color(1f, f, 0f),
                1 => new Color(1f - f, 1f, 0f),
                2 => new Color(0f, 1f, f),
                3 => new Color(0f, 1f - f, 1f),
                4 => new Color(f, 0f, 1f),
                _ => new Color(1f, 0f, 1f - f)
            };
            Draw.Circle(Position, KILL_RADIUS * pulse, rainbowCol, 12);
            Draw.Circle(Position, PULL_RADIUS * 0.5f * pulse, rainbowCol * 0.4f, 16);
            Draw.Circle(Position, PULL_RADIUS * pulse, rainbowCol * 0.2f, 20);
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            try { player.Die((player.Center - Position).SafeNormalize()); }
            catch (Exception) { /* third-party mod hook crash */ }
        }
    }

    /// <summary>
    /// Countdown overlay anchored to an entity's world position (for Blackhole).
    /// </summary>
    internal class AsrielGodBossBlackholeCountdown : Entity
    {
        private AsrielGodBossRainbowBlackhole anchor;
        private float total;
        private float remaining;

        public AsrielGodBossBlackholeCountdown(AsrielGodBossRainbowBlackhole anchor, float duration)
        {
            this.anchor = anchor;
            this.total = duration;
            this.remaining = duration;
            Depth = -9999999;
        }

        public override void Update()
        {
            base.Update();
            remaining -= Engine.DeltaTime;
            if (remaining <= 0f || anchor == null || anchor.Scene == null)
                RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            if (anchor == null || anchor.Scene == null) return;
            Level level = SceneAs<Level>();
            if (level == null) return;

            float pct = Math.Max(0f, remaining / total);
            Vector2 screenPos = level.WorldToScreen(anchor.Position) + new Vector2(0f, -28f);

            float barW = 48f;
            float barH = 5f;
            Draw.Rect(screenPos.X - barW * 0.5f - 1f, screenPos.Y - 1f, barW + 2f, barH + 2f, Color.Black * 0.7f);

            float hue = 1f - pct;
            int hi = (int)(hue * 6);
            float f = hue * 6 - hi;
            Color barColor = (hi % 6) switch
            {
                0 => new Color(1f, f, 0f),
                1 => new Color(1f - f, 1f, 0f),
                2 => new Color(0f, 1f, f),
                3 => new Color(0f, 1f - f, 1f),
                4 => new Color(f, 0f, 1f),
                _ => new Color(1f, 0f, 1f - f)
            };

            Draw.Rect(screenPos.X - barW * 0.5f, screenPos.Y, barW * pct, barH, barColor);
            ActiveFont.DrawOutline(
                $"Blackhole: {remaining:F1}s",
                screenPos + new Vector2(0f, barH + 3f),
                new Vector2(0.5f, 0f),
                Vector2.One * 0.38f,
                barColor,
                1f,
                Color.Black
            );
        }
    }

    /// <summary>
    /// Spinning blade projectile.
    /// </summary>
    [Tracked]
    public class AsrielGodBossBlade : Entity
    {
        private const float SPEED = 250f;
        private const float LIFETIME = 4f;
        private Vector2 velocity;
        private float timer;
        private float rotation;

        public AsrielGodBossBlade Init(AsrielGodBoss boss, global::Celeste.Player target, float angleOffset)
        {
            Position = boss.ShotOrigin;
            Collider = new Monocle.Circle(6f);
            Add(new PlayerCollider(OnPlayer));
            Depth = -1000000;
            Vector2 dir = (target.Center - Position).SafeNormalize();
            dir = Calc.AngleToVector(dir.Angle() + MathHelper.ToRadians(angleOffset), 1f);
            velocity = dir * SPEED;
            timer = LIFETIME;
            return this;
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            rotation += Engine.DeltaTime * 12f;
            timer -= Engine.DeltaTime;
            if (timer <= 0f) RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            // Draw a spinning cross/blade shape
            for (int i = 0; i < 4; i++)
            {
                float a = rotation + i * MathHelper.PiOver2;
                Vector2 tip = Position + Calc.AngleToVector(a, 6f);
                Draw.Line(Position, tip, Color.Orange, 2f);
            }
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            try { player.Die((player.Center - Position).SafeNormalize()); }
            catch (Exception) { /* third-party mod hook crash */ }
            RemoveSelf();
        }
    }

    /// <summary>
    /// Expanding fire shockwave that damages the player.
    /// </summary>
    [Tracked]
    public class AsrielGodBossFireShockwave : Entity
    {
        private const float EXPAND_SPEED = 150f;
        private const float MAX_RADIUS = 100f;
        private const float RING_WIDTH = 8f;
        private float radius;

        public AsrielGodBossFireShockwave(Vector2 position) : base(position)
        {
            Depth = -1000000;
            radius = 0f;
        }

        public override void Update()
        {
            base.Update();
            radius += EXPAND_SPEED * Engine.DeltaTime;
            if (radius >= MAX_RADIUS) { RemoveSelf(); return; }

            // Check player collision with the ring
            var player = Scene?.Tracker.GetEntity<global::Celeste.Player>();
            if (player != null)
            {
                float dist = Vector2.Distance(player.Center, Position);
                if (dist >= radius - RING_WIDTH / 2f && dist <= radius + RING_WIDTH / 2f)
                {
                    try { player.Die((player.Center - Position).SafeNormalize()); }
                    catch (Exception) { /* third-party mod hook crash */ }
                }
            }
        }

        public override void Render()
        {
            base.Render();
            float alpha = 1f - radius / MAX_RADIUS;
            Draw.Circle(Position, radius, Color.OrangeRed * alpha, 24);
            if (radius > 2f)
                Draw.Circle(Position, radius - 2f, Color.Yellow * alpha * 0.5f, 24);
        }
    }

    /// <summary>
    /// Meteorite projectile that falls from above.
    /// </summary>
    [Tracked]
    public class AsrielGodBossMeteorite : Entity
    {
        private const float SPEED = 300f;
        private const float LIFETIME = 4f;
        private Vector2 velocity;
        private float timer;

        public AsrielGodBossMeteorite(Vector2 position, Vector2 target) : base(position)
        {
            Collider = new Monocle.Circle(5f);
            Add(new PlayerCollider(OnPlayer));
            Depth = -1000000;
            velocity = (target - position).SafeNormalize() * SPEED;
            timer = LIFETIME;
            Add(new VertexLight(Color.OrangeRed, 0.6f, 16, 32));
        }

        public override void Update()
        {
            base.Update();
            Position += velocity * Engine.DeltaTime;
            timer -= Engine.DeltaTime;
            if (timer <= 0f) RemoveSelf();
        }

        public override void Render()
        {
            base.Render();
            Draw.Circle(Position, 5f, Color.OrangeRed, 8);
            Draw.Circle(Position, 3f, Color.Yellow, 6);
        }

        private void OnPlayer(global::Celeste.Player player)
        {
            try { player.Die((player.Center - Position).SafeNormalize()); }
            catch (Exception) { /* third-party mod hook crash */ }
            RemoveSelf();
        }
    }
}

