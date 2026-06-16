using System.Globalization;
using Celeste.Helpers;
using Celeste.Cutscenes;
using Celeste.Entities.Effects;
using Celeste.Entities.Projectiles;
using Celeste.Extensions;
using FMOD.Studio;

namespace Celeste.Entities
{
    /// <summary>
    /// SiamoZeroFinalBoss - A ConqueredPeak/BadelineBoss style boss with pattern-based attacks.
    /// Uses node-based movement and phase transitions similar to Celeste's FinalBoss and AsrielGodBoss.
    /// Els' true form - Phase 1: Doppia Elillca, Phase 2: Penumbra Phastasm (true final phase)
    /// 
    /// Lore context (Desolo Zantas - "Fallen Path / Siamo Zero"):
    /// "Siamo Zero" (Italian: "We Are Zero") is the corrupted dark-path version of Kirby Ã¢â‚¬â€
    /// the nightmare scenario shown by Elmerninis where Kirby falls to darkness without
    /// love or friendship. A hero "no longer shaped like a friend," molded by sorrow,
    /// hate, jealousy, and all-consuming loneliness. The Fallen Path demonstrates that
    /// Kirby would naturally grow sinister over time even without Elmerninis' intervention.
    /// "All it takes is one hero to fall for the story to end."
    /// </summary>
    [CustomEntity(ids: "DZ/SiamoZeroFinalBoss,DZ/SiamoZeroDelta,DZ/CelestialZero,DZ/ElsTrueFinalBoss")]
    [Tracked(true)]
    [HotReloadable]
    public partial class SiamoZeroFinalBoss : BossActor
    {
        #region Constants and Audio Events
        
        // Audio Events
        private const string SFX_BOSS_HIT = "event:/new_content/char/pusheen/els/Els_Scream_Hit";
        private const string SFX_ELS_ACTIVATE = "event:/new_content/char/pusheen/els/Els_Activate";
        private const string SFX_ELS_BEAMSLASH = "event:/new_content/char/pusheen/els/Els_BeamSlash";
        private const string SFX_ELS_BIGHIT = "event:/new_content/char/pusheen/els/Els_BigHit";
        private const string SFX_ELS_BUBBLE = "event:/new_content/char/pusheen/els/Els_Bubble";
        private const string SFX_ELS_BUILD = "event:/new_content/char/pusheen/els/Els_Build";
        private const string SFX_ELS_CHARGE = "event:/new_content/char/pusheen/els/Els_Charge";
        private const string SFX_ELS_CREATE = "event:/new_content/char/pusheen/els/Els_Create";
        private const string SFX_ELS_DARKMATTER_SPAWN = "event:/new_content/char/pusheen/els/Els_Darkmatter_Spawn";
        private const string SFX_ELS_FINAL_CRY = "event:/new_content/char/pusheen/els/Els_Final_Cry";
        private const string SFX_ELS_IMPACT = "event:/new_content/char/pusheen/els/Els_Impact";
        private const string SFX_ELS_KNOCKOUT = "event:/new_content/char/pusheen/els/Els_Knockout";
        private const string SFX_ELS_PRECREATE = "event:/new_content/char/pusheen/els/Els_Precreate";
        private const string SFX_ELS_PREDEATH = "event:/new_content/char/pusheen/els/Els_Predeath";
        private const string SFX_ELS_PREIMPACT = "event:/new_content/char/pusheen/els/Els_PreImpact";
        private const string SFX_ELS_REVIVAL = "event:/new_content/char/pusheen/els/Els_Revival";
        private const string SFX_ELS_RIFT = "event:/new_content/char/pusheen/els/Els_Rift";
        private const string SFX_ELS_RIFT_BULLET = "event:/new_content/char/pusheen/els/Els_Rift_Bullet";
        private const string SFX_ELS_SCREAM_HIT = "event:/new_content/char/pusheen/els/Els_Scream_Hit";
        private const string SFX_ELS_SHELL_SCREAMER = "event:/new_content/char/pusheen/els/Els_Shell_Screamer";
        private const string SFX_ELS_SHELLCRACK = "event:/new_content/char/pusheen/els/Els_Shellcrack";
        private const string SFX_ELS_SLICE = "event:/new_content/char/pusheen/els/Els_Slice";
        private const string SFX_ELS_SPAWN = "event:/new_content/char/pusheen/els/Els_Spawn";
        private const string SFX_ELS_STARDEATH = "event:/new_content/char/pusheen/els/Els_StarDeath";
        private const string SFX_ELS_TELEPORT = "event:/new_content/char/pusheen/els/Els_Teleport";
        private const string SFX_ELS_TIME_MANIPULATOR_END = "event:/new_content/char/pusheen/els/Els_Time_Manipulator_End";
        private const string SFX_ELS_TIME_MANIPULATOR_START = "event:/new_content/char/pusheen/els/Els_Time_Manipulator_Start";
        private const string MUSIC_SIAMO_ZERO = "event:/new_content/music/pusheen/lvl20/siamo_zero_finale";
        private const string MUSIC_SIAMO_ZERO_DELTA = "event:/new_content/music/pusheen/lvl20/siamo_zero_delta";
        private const string MUSIC_CELESTIAL_ZERO = "event:/new_content/music/pusheen/lvl20/celestial_zero";
        private const string ENTITY_SIAMO_ZERO_FINAL_BOSS = "DZ/SiamoZeroFinalBoss";
        private const string ENTITY_SIAMO_ZERO_DELTA = "DZ/SiamoZeroDelta";
        private const string ENTITY_CELESTIAL_ZERO = "DZ/CelestialZero";
        private const string INTRO_FLAG_PREFIX = "siamo_zero_final_boss_intro_";
        private const string LEGACY_INTRO_FLAG_PREFIX = "els_true_final_boss_intro_";
        private const string INTRO_FLAG = "siamo_zero_final_boss_intro";
        private const string LEGACY_INTRO_FLAG = "els_true_final_boss_intro";
        private const string DEFEATED_FLAG = "siamo_zero_final_boss_defeated";
        private const string LEGACY_DEFEATED_FLAG = "els_true_final_boss_defeated";
        
        // ConqueredPeak/BadelineBoss constants
        private const float MOVE_SPEED = 600f;
        private const float HIT_INVULNERABILITY_TIME = 1.5f;

        private const string BossSpriteAtlasRoot = "characters/els_true_final_boss/";
        private const string LegacyBossAnimationPath = "boss";
        private const string Phase1AnimationPath = "boss";
        private const string Phase2AnimationPath = "boss";
        private const string Phase3AnimationPath = "boss";
        private const string Phase4AnimationPath = "boss";
        private const string FinalZeroAnimationPath = "boss";
        private const string SiamoZeroAnimationPath = "siamo_zero";
        private const string BasePartSuffix = "_base";
        private const string WingsPartSuffix = "_wings";
        private const string EyesPartSuffix = "_eyes";
        private const string PupilPartSuffix = "_pupil";
        private const string RevealSequenceAnimationId = "reveal_sequence";
        private const float RevealSequenceDuration = 0.56f;
        
        // HP/Phase System (following AsrielGodBoss pattern)
        private const int MAX_HITS_PER_PHASE = 3;  // Hits needed to progress to next phase
        private const int TOTAL_PHASES = 5;        // Total number of boss phases
        private const int MAX_HITS = MAX_HITS_PER_PHASE * TOTAL_PHASES; // Total hits to defeat boss (15)
        
        #endregion
        
        #region Particle Types
        
        public static ParticleType PBurst;
        public static ParticleType PShoot;
        
        static SiamoZeroFinalBoss()
        {
            PBurst = new ParticleType
            {
                Color = Color.Purple,
                Color2 = Color.DarkRed,
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
            
            PShoot = new ParticleType
            {
                Color = Color.DarkBlue,
                Color2 = Color.Purple,
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
        private SoundSource chargeSfx;
        private SoundSource laserSfx;
        
        // Movement and position
        private Vector2 avoidPos;
        public bool Moving;
        public bool Sitting;
        private int facing;
        private Vector2[] nodes;
        private int nodeIndex;
        
        // Pattern and attack system
        private int patternIndex;
        private Coroutine attackCoroutine;
        private Coroutine triggerBlocksCoroutine;
        private SineWave floatSine;
        private bool dialog;
        private bool startHit;
#pragma warning disable CS0414
        private bool isAttacking;
#pragma warning restore CS0414
        
        // Phase management (BadelineBoss style) - HP System
        private int currentPhase = 0;
        private int hitsInPhase = 0;
        private bool phaseTransitioning = false;
        private int totalHitsTaken = 0;
        
        // Custom attack sequence
        private bool useCustomSequence;
        private List<AttackStep> customAttackSteps;
        private string attackSequenceData;
        
        // Phase-specific properties
        private Sprite doppiaSprite;
        private Sprite doppiaWingSprite;
        private Sprite doppiaEyeSprite;
        private Sprite doppiaPupilSprite;
        private Sprite penumbraSprite;
        private Sprite penumbraWingSprite;
        private Sprite penumbraEyeSprite;
        private Sprite penumbraPupilSprite;
        private Sprite siamoSprite;
        private Sprite siamoWingSprite;
        private Sprite siamoEyeSprite;
        private Sprite siamoPupilSprite;
        private List<VertexLight> phantasmLights = new List<VertexLight>();
        private float dualityFactor = 0f;
        private float voidPower = 0f;
        private bool isInVoidMode = false;
        private bool hasDoppiaWingParts = false;
        private bool hasDoppiaEyeParts = false;
        private bool hasDoppiaPupilParts = false;
        private bool hasPenumbraWingParts = false;
        private bool hasPenumbraEyeParts = false;
        private bool hasPenumbraPupilParts = false;
        private bool hasSiamoWingParts = false;
        private bool hasSiamoEyeParts = false;
        private bool hasSiamoPupilParts = false;
        
        // Defense and healing
        private bool isDefending = false;
        private float defenseDuration = 0f;
        private float defenseReduction = 0.75f;
        private float healCooldown = 0f;
        private const float HEAL_COOLDOWN_TIME = 15f;
        
        // Gimmick power tracking
        private float dimensionRiftPower = 0f;
        private const float MAX_RIFT_POWER = 100f;
        private bool canUseUltimateRiftAttack = false;
        
        // Shared effects
        private VertexLight coreLight;
        private SineWave energyPulse;
        private Wiggler phaseWiggler;
        
        // Boss state management
        private BossState currentState = BossState.Waiting;
        private Vector2 knockbackVelocity = Vector2.Zero;
        private float knockbackTimer = 0f;
        private bool isHitSlowdownActive = false;
        private readonly TimeRateModifier timeRateModifier;
        private SiamoZeroIdentity siamoIdentity = SiamoZeroIdentity.Zero;
        private string currentBossMusicEvent;
        private bool bossMusicInitialized;
        
        // Phase tracking
        private ElsPhase currentElsPhase = ElsPhase.DoppiaElillca;
        private bool hasTransitionedToPhase2 = false;
        private bool introSequencePlaying = false;
        
        // Team mechanics
        private List<global::Celeste.Player> allies;
        private int teamAttackCounter = 0;
        
        // Arena bounds
        private float arenaRadius = 400f;
        private global::Celeste.Effects.ElsTrueFinalBackdrop bossBg;
        
        // Shockwave tracking for cleanup
        private Coroutine shockwaveCoroutine;
        private bool shockwaveEnabled = true;

        private static readonly Color PhoenixHaloGold = new Color(255, 236, 170);
        private static readonly Color PhoenixHaloOrange = new Color(255, 150, 92);
        private static readonly Color PhoenixHaloMagenta = new Color(255, 84, 164);
        private static readonly Color PhoenixHaloWhite = new Color(255, 248, 228);

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
        
        #region State and Type Enums
        
        public enum BossState
        {
            Waiting,
            Idle,
            Moving,
            Attacking,
            Hit,
            Hurt,
            Transitioning,
            Defeated
        }

        public enum AttackType
        {
            DoppiaCloneAssault,
            DualityWave,
            ShadowBlast,
            MirrorDimension,
            DimensionalDefense,
            DualityHeal,
            RiftStrikeCombo,
            PenumbraVoidStorm,
            PhantasmBarrage,
            VoidCollapse,
            DimensionalTear,
            UltimateAnnihilation
        }
        
        public enum ElsPhase
        {
            DoppiaElillca,
            PenumbraPhastasm,
            /// <summary>
            /// Siamo Zero ("We Are Zero") - The Fallen Path.
            /// Corrupted Kirby nightmare vision phase. Not a combat phase Ã¢â‚¬â€ used during
            /// the CH20_FALLEN_PATH_VISION cutscene where Els shows Kirby what he could
            /// become without love and friendship.
            /// </summary>
            SiamoZero
        }
        public enum SiamoZeroTier
        {
            Pink,
            SoulBlack,
            Stellarruss
        }

        public enum SiamoZeroIdentity
        {
            Zero,
            Delta,
            Celestial
        }

        private SiamoZeroTier siamoZeroTier = SiamoZeroTier.SoulBlack;
        
        #endregion
        
        #region Constructors

        private static bool IsPrimarySiamoFinalBossEntity(string entityName)
        {
            return string.Equals(entityName, ENTITY_SIAMO_ZERO_FINAL_BOSS, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityName, ENTITY_SIAMO_ZERO_DELTA, StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityName, ENTITY_CELESTIAL_ZERO, StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveSiamoVariant(EntityData data)
        {
            string explicitVariant = data.Attr("siamoVariant", "");
            if (!string.IsNullOrWhiteSpace(explicitVariant))
                return explicitVariant;

            return data.Name switch
            {
                ENTITY_SIAMO_ZERO_DELTA => "delta",
                ENTITY_CELESTIAL_ZERO => "celestial",
                _ => "zero"
            };
        }

        private static string ResolveSiamoTier(EntityData data)
        {
            string explicitTier = data.Attr("siamoTier", "");
            if (!string.IsNullOrWhiteSpace(explicitTier))
                return explicitTier;

            return data.Name switch
            {
                ENTITY_CELESTIAL_ZERO => "stellarruss",
                _ => "soulBlack"
            };
        }

        private static int ResolvePatternIndex(EntityData data)
        {
            int explicitPatternIndex = data.Int(nameof(patternIndex), -1);
            if (explicitPatternIndex >= 0)
                return explicitPatternIndex;

            return IsPrimarySiamoFinalBossEntity(data.Name) ? 4 : 0;
        }

        private static bool ResolveDialog(EntityData data)
        {
            return data.Bool(nameof(dialog), string.Equals(data.Name, ENTITY_SIAMO_ZERO_FINAL_BOSS, StringComparison.OrdinalIgnoreCase));
        }

        private static bool ResolveStartHit(EntityData data)
        {
            return data.Bool(nameof(startHit), !string.Equals(data.Name, ENTITY_SIAMO_ZERO_FINAL_BOSS, StringComparison.OrdinalIgnoreCase));
        }

        private static string ResolveAttackSequence(EntityData data)
        {
            string explicitAttackSequence = data.Attr("attackSequence", "");
            if (!string.IsNullOrWhiteSpace(explicitAttackSequence))
                return explicitAttackSequence;

            return data.Name switch
            {
                ENTITY_SIAMO_ZERO_FINAL_BOSS => "CrescentBeamShot,EnergySwordCombo,TornadoSlash,RevolutionSword,RisingSpine,DownThrust,DrillStab,EnergyShower,VortexStrike,DoubleSideSlash,MorphoEmerge,TimeborderCollapse",
                ENTITY_SIAMO_ZERO_DELTA => "CrescentBeamShot,EnergySwordCombo,ConqueredPeakCascade,TornadoSlash,EnergyShower,VortexStrike,DoubleSideSlash,MorphoEmerge,TimeborderCollapse",
                ENTITY_CELESTIAL_ZERO => "CrescentBeamShot,RevolutionSword,ConqueredPeakCascade,EnergyShower,VortexStrike,DoubleSideSlash,TimeborderCollapse,MorphoEmerge,ConqueredPeakCascade",
                _ => string.Empty
            };
        }

        private static string GetIntroFlagForRoom(string roomId)
        {
            return string.IsNullOrWhiteSpace(roomId)
                ? INTRO_FLAG
                : $"{INTRO_FLAG_PREFIX}{roomId}";
        }

        private static string GetLegacyIntroFlagForRoom(string roomId)
        {
            return string.IsNullOrWhiteSpace(roomId)
                ? LEGACY_INTRO_FLAG
                : $"{LEGACY_INTRO_FLAG_PREFIX}{roomId}";
        }

        private bool HasSeenEncounterIntro(string roomId)
        {
            return level.Session.GetFlag(GetIntroFlagForRoom(roomId))
                || level.Session.GetFlag(GetLegacyIntroFlagForRoom(roomId))
                || level.Session.GetFlag(INTRO_FLAG)
                || level.Session.GetFlag(LEGACY_INTRO_FLAG);
        }

        private void MarkEncounterIntroSeen(string roomId)
        {
            level.Session.SetFlag(GetIntroFlagForRoom(roomId), true);
            level.Session.SetFlag(GetLegacyIntroFlagForRoom(roomId), true);
            level.Session.SetFlag(INTRO_FLAG, true);
            level.Session.SetFlag(LEGACY_INTRO_FLAG, true);
        }

        private void MarkEncounterDefeated()
        {
            level.Session.SetFlag(DEFEATED_FLAG, true);
            level.Session.SetFlag(LEGACY_DEFEATED_FLAG, true);
        }
        
        public SiamoZeroFinalBoss(
            Vector2 position,
            Vector2[] nodes,
            int patternIndex,
            bool dialog,
            bool startHit,
            string attackSequence = "",
            string siamoTier = "soulBlack",
            string siamoVariant = "zero")
            : base(position,
                   spriteName: "els_true_final_boss",
                   spriteScale: Vector2.One,
                   maxFall: 160f,
                   collidable: true,
                   solidCollidable: false,
                   gravityMult: 0.0f,
                   collider: new Monocle.Circle(14f, y: -6f))
        {
            this.patternIndex = patternIndex;
            this.dialog = dialog;
            this.startHit = startHit;
            this.attackSequenceData = attackSequence;
            this.siamoZeroTier = ParseSiamoZeroTier(siamoTier);
            this.siamoIdentity = ParseSiamoZeroIdentity(siamoVariant);
            this.MaxHealth = MAX_HITS;
            this.Health = this.MaxHealth;
            this.Add((Component)(this.light = new VertexLight(Color.White, 1f, 32, 64)));
            this.circle = (Monocle.Circle)this.Collider;
            this.Add(new PlayerCollider(player => this.OnPlayer(player)));
            this.nodes = new Vector2[nodes.Length + 1];
            this.nodes[0] = this.Position;
            for (int index = 0; index < nodes.Length; ++index)
                this.nodes[index + 1] = nodes[index];
            this.attackCoroutine = new Coroutine(false);
            this.Add((Component)this.attackCoroutine);
            this.triggerBlocksCoroutine = new Coroutine(false);
            this.Add((Component)this.triggerBlocksCoroutine);
            this.Add((Component)(this.floatSine = new SineWave(0.6f)));
            this.Add((Component)(this.scaleWiggler = Wiggler.Create(0.6f, 3f)));
            this.Add((Component)(this.chargeSfx = new SoundSource()));
            this.Add((Component)(this.laserSfx = new SoundSource()));
            this.Add((Component)(this.timeRateModifier = new TimeRateModifier(1f, false)));
            
            Add(energyPulse = new SineWave(0.5f, 0f));
            energyPulse.Randomize();
            Add(phaseWiggler = Wiggler.Create(1.2f, 4f));
        }

        public SiamoZeroFinalBoss(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.NodesOffset(offset), ResolvePatternIndex(data),
                  ResolveDialog(data), ResolveStartHit(data),
                  ResolveAttackSequence(data), ResolveSiamoTier(data), ResolveSiamoVariant(data))
        {
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
            
            // Initialize the live map backdrop used by the encounter
            this.bossBg = this.level.Background.Get<global::Celeste.Effects.ElsTrueFinalBackdrop>();
            
            if (this.patternIndex == 0)
            {
                this.NormalSprite = new global::Celeste.PlayerSprite(global::Celeste.PlayerSpriteMode.Badeline);
                this.NormalSprite.Scale.X = -1f;
                if (this.NormalSprite.Has("idle"))
                    this.NormalSprite.Play("idle");
                this.Add((Component)this.NormalSprite);
            }
            else
                this.createBossSprite();
            
            setupPhase1();
            ApplyConfiguredStartingPhase();
            
            string currentRoomId = this.level.Session.Level;
            bool hasSeenIntro = HasSeenEncounterIntro(currentRoomId);
            bool shouldPlayIntro = dialog && currentElsPhase == ElsPhase.DoppiaElillca && !hasSeenIntro && ShouldShowIntroForRoom(currentRoomId);
            
            if (shouldPlayIntro)
            {
                Add(new Coroutine(finalBossEntrance()));
            }
            else
            {
                this.level.Session.Audio.Apply();
                EnsureBossMusicState(force: true);
            }
            
            if (this.startHit)
                Alarm.Set(this, 0.5f, (Action)(() => 
                {
                    var player = this.level?.Tracker.GetEntity<global::Celeste.Player>();
                    if (player != null)
                        this.OnPlayer(player);
                }));
        }
        
        private bool ShouldShowIntroForRoom(string roomId)
        {
            string[] introRoomIds = new string[] { "els-true-final-00" };
            foreach (string id in introRoomIds)
            {
                if (roomId.Equals(id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void ApplyConfiguredStartingPhase()
        {
            ElsPhase configuredPhase = DetermineConfiguredStartingPhase();

            switch (configuredPhase)
            {
                case ElsPhase.PenumbraPhastasm:
                    hasTransitionedToPhase2 = true;
                    currentElsPhase = ElsPhase.PenumbraPhastasm;
                    currentPhase = 1;
                    currentState = BossState.Idle;
                    setupPhase2();
                    break;

                case ElsPhase.SiamoZero:
                    hasTransitionedToPhase2 = true;
                    currentPhase = 2;
                    currentState = BossState.Idle;
                    ActivateSiamoZeroCombat();
                    break;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        #endregion
        
        #region Setup Methods
        
        private void setupPhase1()
        {
            // Doppia Elillca form
            if (doppiaSprite == null)
            {
                Add(doppiaSprite = CreateBossLayerSprite());
                AddBossAnimation(doppiaSprite, "idle", Phase1AnimationPath + BasePartSuffix, Phase1AnimationPath, 0.1f, loop: true);
                AddBossAnimation(doppiaSprite, "boss", Phase1AnimationPath + BasePartSuffix, Phase1AnimationPath, 0.1f, loop: true);
                AddBossAnimation(doppiaSprite, "attack", Phase3AnimationPath + BasePartSuffix, Phase3AnimationPath, 0.08f, loop: true);
                AddBossAnimation(doppiaSprite, "duality", Phase2AnimationPath + BasePartSuffix, Phase2AnimationPath, 0.12f, loop: true);
                AddBossAnimation(doppiaSprite, "charge", Phase2AnimationPath + BasePartSuffix, Phase2AnimationPath, 0.1f, loop: false);
                AddBossAnimation(doppiaSprite, "hit", Phase1AnimationPath + BasePartSuffix, Phase1AnimationPath, 0.08f, loop: false);
                AddBossAnimation(doppiaSprite, "getHit", Phase1AnimationPath + BasePartSuffix, Phase1AnimationPath, 0.08f, loop: false, gotoAnimation: "idle");
                AddBossAnimation(doppiaSprite, "transform", Phase2AnimationPath + BasePartSuffix, Phase2AnimationPath, 0.1f, loop: false);
                AddBossAnimation(doppiaSprite, "death", Phase4AnimationPath + BasePartSuffix, Phase4AnimationPath, 0.1f, loop: false);

                Add(doppiaWingSprite = CreateBossLayerSprite());
                hasDoppiaWingParts = false;
                hasDoppiaWingParts |= AddBossPartAnimation(doppiaWingSprite, "idle", Phase1AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.1f, loop: true);
                hasDoppiaWingParts |= AddBossPartAnimation(doppiaWingSprite, "boss", Phase1AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.1f, loop: true);
                hasDoppiaWingParts |= AddBossPartAnimation(doppiaWingSprite, "attack", Phase3AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.08f, loop: true);
                hasDoppiaWingParts |= AddBossPartAnimation(doppiaWingSprite, "duality", Phase2AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.12f, loop: true);
                hasDoppiaWingParts |= AddBossPartAnimation(doppiaWingSprite, "charge", Phase2AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.1f, loop: false);
                hasDoppiaWingParts |= AddBossPartAnimation(doppiaWingSprite, "hit", Phase1AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.08f, loop: false);
                hasDoppiaWingParts |= AddBossPartAnimation(doppiaWingSprite, "getHit", Phase1AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.08f, loop: false, gotoAnimation: "idle");
                hasDoppiaWingParts |= AddBossPartAnimation(doppiaWingSprite, "transform", Phase2AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.1f, loop: false);
                hasDoppiaWingParts |= AddBossPartAnimation(doppiaWingSprite, "death", Phase4AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.1f, loop: false);

                Add(doppiaEyeSprite = CreateBossLayerSprite());
                hasDoppiaEyeParts = false;
                hasDoppiaEyeParts |= AddBossPartAnimation(doppiaEyeSprite, "idle", Phase1AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.1f, loop: true);
                hasDoppiaEyeParts |= AddBossPartAnimation(doppiaEyeSprite, "boss", Phase1AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.1f, loop: true);
                hasDoppiaEyeParts |= AddBossPartAnimation(doppiaEyeSprite, "attack", Phase3AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.08f, loop: true);
                hasDoppiaEyeParts |= AddBossPartAnimation(doppiaEyeSprite, "duality", Phase2AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.12f, loop: true);
                hasDoppiaEyeParts |= AddBossPartAnimation(doppiaEyeSprite, "charge", Phase2AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.1f, loop: false);
                hasDoppiaEyeParts |= AddBossPartAnimation(doppiaEyeSprite, "hit", Phase1AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.08f, loop: false);
                hasDoppiaEyeParts |= AddBossPartAnimation(doppiaEyeSprite, "getHit", Phase1AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.08f, loop: false, gotoAnimation: "idle");
                hasDoppiaEyeParts |= AddBossPartAnimation(doppiaEyeSprite, "transform", Phase2AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.1f, loop: false);
                hasDoppiaEyeParts |= AddBossPartAnimation(doppiaEyeSprite, "death", Phase4AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.1f, loop: false);

                Add(doppiaPupilSprite = CreateBossLayerSprite());
                hasDoppiaPupilParts = false;
                hasDoppiaPupilParts |= AddBossPartAnimation(doppiaPupilSprite, "idle", Phase1AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.1f, loop: true);
                hasDoppiaPupilParts |= AddBossPartAnimation(doppiaPupilSprite, "boss", Phase1AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.1f, loop: true);
                hasDoppiaPupilParts |= AddBossPartAnimation(doppiaPupilSprite, "attack", Phase3AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.08f, loop: true);
                hasDoppiaPupilParts |= AddBossPartAnimation(doppiaPupilSprite, "duality", Phase2AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.12f, loop: true);
                hasDoppiaPupilParts |= AddBossPartAnimation(doppiaPupilSprite, "charge", Phase2AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.1f, loop: false);
                hasDoppiaPupilParts |= AddBossPartAnimation(doppiaPupilSprite, "hit", Phase1AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.08f, loop: false);
                hasDoppiaPupilParts |= AddBossPartAnimation(doppiaPupilSprite, "getHit", Phase1AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.08f, loop: false, gotoAnimation: "idle");
                hasDoppiaPupilParts |= AddBossPartAnimation(doppiaPupilSprite, "transform", Phase2AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.1f, loop: false);
                hasDoppiaPupilParts |= AddBossPartAnimation(doppiaPupilSprite, "death", Phase4AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.1f, loop: false);
            }

            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "idle", "boss");
            doppiaSprite.Visible = true;
            
            // Core light (changes color between phases)
            Add(coreLight = new VertexLight(PhoenixHaloOrange * 1.2f, 1f, 256, 384));
            coreLight.Position = Vector2.Zero;
            
            // Energy pulse
            Add(energyPulse = new SineWave(0.5f, 0f));
            energyPulse.Randomize();
            
            // Phase wiggler
            Add(phaseWiggler = Wiggler.Create(1.2f, 4f));
        }
        
        private void setupPhase2()
        {
            if (doppiaSprite != null)
                doppiaSprite.Visible = false;
            if (doppiaWingSprite != null)
                doppiaWingSprite.Visible = false;
            if (doppiaEyeSprite != null)
                doppiaEyeSprite.Visible = false;
            if (doppiaPupilSprite != null)
                doppiaPupilSprite.Visible = false;
            
            if (penumbraSprite == null)
            {
                Add(penumbraSprite = CreateBossLayerSprite());
                AddBossAnimation(penumbraSprite, "idle", Phase4AnimationPath + BasePartSuffix, Phase4AnimationPath, 0.08f, loop: true);
                AddBossAnimation(penumbraSprite, "boss", Phase4AnimationPath + BasePartSuffix, Phase4AnimationPath, 0.08f, loop: true);
                AddBossAnimation(penumbraSprite, "attack", Phase3AnimationPath + BasePartSuffix, Phase3AnimationPath, 0.06f, loop: true);
                AddBossAnimation(penumbraSprite, "void", Phase4AnimationPath + BasePartSuffix, Phase4AnimationPath, 0.1f, loop: true);
                AddBossAnimation(penumbraSprite, "ultimate", FinalZeroAnimationPath + BasePartSuffix, FinalZeroAnimationPath, 0.05f, loop: true);
                AddBossAnimation(penumbraSprite, "charge", Phase4AnimationPath + BasePartSuffix, Phase4AnimationPath, 0.1f, loop: false);
                AddBossAnimation(penumbraSprite, "hit", Phase4AnimationPath + BasePartSuffix, Phase4AnimationPath, 0.08f, loop: false);
                AddBossAnimation(penumbraSprite, "getHit", Phase4AnimationPath + BasePartSuffix, Phase4AnimationPath, 0.08f, loop: false, gotoAnimation: "idle");
                AddBossAnimation(penumbraSprite, "transform", Phase4AnimationPath + BasePartSuffix, Phase4AnimationPath, 0.1f, loop: false);
                AddBossAnimation(penumbraSprite, "death", FinalZeroAnimationPath + BasePartSuffix, FinalZeroAnimationPath, 0.1f, loop: false);

                Add(penumbraWingSprite = CreateBossLayerSprite());
                hasPenumbraWingParts = false;
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "idle", Phase4AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.08f, loop: true);
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "boss", Phase4AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.08f, loop: true);
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "attack", Phase3AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.06f, loop: true);
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "void", Phase4AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.1f, loop: true);
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "ultimate", FinalZeroAnimationPath, Phase4AnimationPath, WingsPartSuffix, 0.05f, loop: true);
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "charge", Phase4AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.1f, loop: false);
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "hit", Phase4AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.08f, loop: false);
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "getHit", Phase4AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.08f, loop: false, gotoAnimation: "idle");
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "transform", Phase4AnimationPath, LegacyBossAnimationPath, WingsPartSuffix, 0.1f, loop: false);
                hasPenumbraWingParts |= AddBossPartAnimation(penumbraWingSprite, "death", FinalZeroAnimationPath, Phase4AnimationPath, WingsPartSuffix, 0.1f, loop: false);

                Add(penumbraEyeSprite = CreateBossLayerSprite());
                hasPenumbraEyeParts = false;
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "idle", Phase4AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.08f, loop: true);
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "boss", Phase4AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.08f, loop: true);
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "attack", Phase3AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.06f, loop: true);
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "void", Phase4AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.1f, loop: true);
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "ultimate", FinalZeroAnimationPath, Phase4AnimationPath, EyesPartSuffix, 0.05f, loop: true);
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "charge", Phase4AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.1f, loop: false);
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "hit", Phase4AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.08f, loop: false);
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "getHit", Phase4AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.08f, loop: false, gotoAnimation: "idle");
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "transform", Phase4AnimationPath, LegacyBossAnimationPath, EyesPartSuffix, 0.1f, loop: false);
                hasPenumbraEyeParts |= AddBossPartAnimation(penumbraEyeSprite, "death", FinalZeroAnimationPath, Phase4AnimationPath, EyesPartSuffix, 0.1f, loop: false);

                Add(penumbraPupilSprite = CreateBossLayerSprite());
                hasPenumbraPupilParts = false;
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "idle", Phase4AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.08f, loop: true);
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "boss", Phase4AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.08f, loop: true);
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "attack", Phase3AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.06f, loop: true);
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "void", Phase4AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.1f, loop: true);
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "ultimate", FinalZeroAnimationPath, Phase4AnimationPath, PupilPartSuffix, 0.05f, loop: true);
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "charge", Phase4AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.1f, loop: false);
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "hit", Phase4AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.08f, loop: false);
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "getHit", Phase4AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.08f, loop: false, gotoAnimation: "idle");
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "transform", Phase4AnimationPath, LegacyBossAnimationPath, PupilPartSuffix, 0.1f, loop: false);
                hasPenumbraPupilParts |= AddBossPartAnimation(penumbraPupilSprite, "death", FinalZeroAnimationPath, Phase4AnimationPath, PupilPartSuffix, 0.1f, loop: false);
            }
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "idle", "boss");
            penumbraSprite.Visible = true;
            
            if (phantasmLights.Count == 0)
            {
                Color[] lightColors = {
                    PhoenixHaloGold,
                    PhoenixHaloWhite,
                    PhoenixHaloOrange,
                    PhoenixHaloMagenta,
                    Color.Lerp(PhoenixHaloMagenta, Color.Black, 0.35f)
                };
                
                foreach (var color in lightColors)
                {
                    var plight = new VertexLight(color * 1.5f, 1f, 192, 256);
                    Add(plight);
                    phantasmLights.Add(plight);
                }
            }
            
            if (coreLight != null)
            {
                coreLight.Color = Color.Lerp(PhoenixHaloMagenta, PhoenixHaloOrange, 0.4f) * 1.35f;
                coreLight.StartRadius = 384f;
            }
        }

        private void setupSiamoZero()
        {
            // Hide previous phase sprites
            if (penumbraSprite != null)
                penumbraSprite.Visible = false;
            if (penumbraWingSprite != null)
                penumbraWingSprite.Visible = false;
            if (penumbraEyeSprite != null)
                penumbraEyeSprite.Visible = false;
            if (penumbraPupilSprite != null)
                penumbraPupilSprite.Visible = false;
            if (doppiaSprite != null)
                doppiaSprite.Visible = false;
            if (doppiaWingSprite != null)
                doppiaWingSprite.Visible = false;
            if (doppiaEyeSprite != null)
                doppiaEyeSprite.Visible = false;
            if (doppiaPupilSprite != null)
                doppiaPupilSprite.Visible = false;

            if (siamoSprite == null)
            {
                string siamoBodyPath = ResolveSiamoBodyAnimationPath();
                string siamoWingPath = ResolveSiamoPartAnimationPath(WingsPartSuffix);
                string siamoEyePath = ResolveSiamoPartAnimationPath(EyesPartSuffix);
                string siamoPupilPath = ResolveSiamoPartAnimationPath(PupilPartSuffix);

                Add(siamoSprite = CreateBossLayerSprite());
                AddBossAnimation(siamoSprite, "idle", siamoBodyPath, siamoBodyPath, 0.1f, loop: true);
                AddBossAnimation(siamoSprite, "boss", siamoBodyPath, siamoBodyPath, 0.1f, loop: true);
                AddBossAnimation(siamoSprite, "void", siamoBodyPath, siamoBodyPath, 0.08f, loop: true);
                AddBossAnimation(siamoSprite, "hit", siamoBodyPath, siamoBodyPath, 0.08f, loop: false);
                AddBossAnimation(siamoSprite, "getHit", siamoBodyPath, siamoBodyPath, 0.08f, loop: false, gotoAnimation: "idle");
                AddBossAnimation(siamoSprite, "death", siamoBodyPath, siamoBodyPath, 0.1f, loop: false);

                Add(siamoWingSprite = CreateBossLayerSprite());
                hasSiamoWingParts = HasBossFrames(siamoWingPath);
                if (hasSiamoWingParts)
                {
                    AddBossAnimation(siamoWingSprite, "idle", siamoWingPath, siamoWingPath, 0.1f, loop: true);
                    AddBossAnimation(siamoWingSprite, "boss", siamoWingPath, siamoWingPath, 0.1f, loop: true);
                    AddBossAnimation(siamoWingSprite, "void", siamoWingPath, siamoWingPath, 0.08f, loop: true);
                    AddBossAnimation(siamoWingSprite, "hit", siamoWingPath, siamoWingPath, 0.08f, loop: false);
                    AddBossAnimation(siamoWingSprite, "getHit", siamoWingPath, siamoWingPath, 0.08f, loop: false, gotoAnimation: "idle");
                    AddBossAnimation(siamoWingSprite, "death", siamoWingPath, siamoWingPath, 0.1f, loop: false);
                }

                Add(siamoEyeSprite = CreateBossLayerSprite());
                hasSiamoEyeParts = HasBossFrames(siamoEyePath);
                if (hasSiamoEyeParts)
                {
                    AddBossAnimation(siamoEyeSprite, "idle", siamoEyePath, siamoEyePath, 0.1f, loop: true);
                    AddBossAnimation(siamoEyeSprite, "boss", siamoEyePath, siamoEyePath, 0.1f, loop: true);
                    AddBossAnimation(siamoEyeSprite, "void", siamoEyePath, siamoEyePath, 0.08f, loop: true);
                    AddBossAnimation(siamoEyeSprite, "hit", siamoEyePath, siamoEyePath, 0.08f, loop: false);
                    AddBossAnimation(siamoEyeSprite, "getHit", siamoEyePath, siamoEyePath, 0.08f, loop: false, gotoAnimation: "idle");
                    AddBossAnimation(siamoEyeSprite, "death", siamoEyePath, siamoEyePath, 0.1f, loop: false);
                }

                Add(siamoPupilSprite = CreateBossLayerSprite());
                hasSiamoPupilParts = HasBossFrames(siamoPupilPath);
                if (hasSiamoPupilParts)
                {
                    AddBossAnimation(siamoPupilSprite, "idle", siamoPupilPath, siamoPupilPath, 0.1f, loop: true);
                    AddBossAnimation(siamoPupilSprite, "boss", siamoPupilPath, siamoPupilPath, 0.1f, loop: true);
                    AddBossAnimation(siamoPupilSprite, "void", siamoPupilPath, siamoPupilPath, 0.08f, loop: true);
                    AddBossAnimation(siamoPupilSprite, "hit", siamoPupilPath, siamoPupilPath, 0.08f, loop: false);
                    AddBossAnimation(siamoPupilSprite, "getHit", siamoPupilPath, siamoPupilPath, 0.08f, loop: false, gotoAnimation: "idle");
                    AddBossAnimation(siamoPupilSprite, "death", siamoPupilPath, siamoPupilPath, 0.1f, loop: false);
                }
            }

            PlayBossAnimationSet(ElsPhase.SiamoZero, "idle", "boss");
            siamoSprite.Visible = true;
        }

        private static Color GetPhoenixHaloColor(float amount)
        {
            amount = Calc.Clamp(amount, 0f, 1f);
            if (amount < 0.33f)
                return Color.Lerp(PhoenixHaloGold, PhoenixHaloOrange, amount / 0.33f);
            if (amount < 0.66f)
                return Color.Lerp(PhoenixHaloOrange, PhoenixHaloMagenta, (amount - 0.33f) / 0.33f);
            return Color.Lerp(PhoenixHaloMagenta, PhoenixHaloWhite, (amount - 0.66f) / 0.34f);
        }

        private void UpdateCoreHaloLight(float amount, bool penumbraPhase)
        {
            if (coreLight == null)
                return;

            Color haloColor = GetPhoenixHaloColor(amount);
            if (penumbraPhase)
                haloColor = Color.Lerp(haloColor, PhoenixHaloMagenta, 0.22f);

            coreLight.Color = haloColor * (penumbraPhase ? 1.7f : 1.35f);
            coreLight.Alpha = (penumbraPhase ? 0.95f : 0.82f) + amount * (penumbraPhase ? 0.5f : 0.32f);
            coreLight.StartRadius = penumbraPhase ? 360f + amount * 72f : 256f + amount * 48f;
        }

        private void UpdatePhoenixCrownLights(float amount)
        {
            if (phantasmLights.Count == 0)
                return;

            int maxIndex = Math.Max(1, phantasmLights.Count - 1);
            for (int i = 0; i < phantasmLights.Count; i++)
            {
                float spread = i / (float) maxIndex;
                float arcAngle = MathHelper.Lerp(-1.2f, 1.2f, spread);
                float radius = 108f + amount * 28f;
                float flutter = (float) Math.Sin(Scene.TimeActive * 3.5f + i * 0.55f) * (8f + amount * 10f);

                phantasmLights[i].Position = new Vector2(
                    (float) Math.Cos(arcAngle) * radius,
                    -96f + (float) Math.Sin(arcAngle) * 18f + flutter
                );

                float colorSample = (spread + amount * 0.65f) % 1f;
                phantasmLights[i].Color = GetPhoenixHaloColor(colorSample) * 1.45f;
                phantasmLights[i].Alpha = 0.58f + amount * 0.34f;
            }
        }

        private static bool HasBossFrames(string relativePath)
        {
            return GFX.Game.HasAtlasSubtextures(BossSpriteAtlasRoot + relativePath);
        }

        private string GetSiamoVariantFolderSuffix()
        {
            switch (siamoZeroTier)
            {
                case SiamoZeroTier.Pink:
                    return "_pink";
                case SiamoZeroTier.SoulBlack:
                    return "_soul_black";
                case SiamoZeroTier.Stellarruss:
                    return "_stellarruss";
                default:
                    return string.Empty;
            }
        }

        private static string AppendVariantSuffix(string basePath, string variantSuffix)
        {
            if (string.IsNullOrEmpty(variantSuffix))
                return basePath;

            if (basePath.EndsWith("/", StringComparison.Ordinal))
                return basePath.Substring(0, basePath.Length - 1) + variantSuffix + "/";

            return basePath + variantSuffix;
        }

        private string ResolveSiamoAnimationPath(string basePath, string fallbackPath = null)
        {
            string variantSuffix = GetSiamoVariantFolderSuffix();
            if (!string.IsNullOrEmpty(variantSuffix))
            {
                string variantPath = AppendVariantSuffix(basePath, variantSuffix);
                if (HasBossFrames(variantPath))
                    return variantPath;
            }

            if (HasBossFrames(basePath))
                return basePath;

            return fallbackPath ?? basePath;
        }

        private string ResolveSiamoBodyAnimationPath()
        {
            return ResolveSiamoAnimationPath(SiamoZeroAnimationPath + BasePartSuffix, SiamoZeroAnimationPath);
        }

        private string ResolveSiamoPartAnimationPath(string partSuffix)
        {
            return ResolveSiamoAnimationPath(SiamoZeroAnimationPath + partSuffix, LegacyBossAnimationPath + partSuffix);
        }

        private static Sprite CreateBossLayerSprite()
        {
            Sprite sprite = new Sprite(GFX.Game, BossSpriteAtlasRoot);
            sprite.CenterOrigin();
            sprite.Visible = false;
            return sprite;
        }

        private static string ResolveBossAnimationPath(string preferredPath, string fallbackPath)
        {
            return HasBossFrames(preferredPath) ? preferredPath : fallbackPath;
        }

        private static bool AddBossPartAnimation(
            Sprite sprite,
            string id,
            string preferredPath,
            string fallbackPath,
            string suffix,
            float delay,
            bool loop,
            string gotoAnimation = null)
        {
            string preferredPartPath = preferredPath + suffix;
            string fallbackPartPath = fallbackPath + suffix;
            if (!HasBossFrames(preferredPartPath) && !HasBossFrames(fallbackPartPath))
                return false;

            AddBossAnimation(sprite, id, preferredPartPath, fallbackPartPath, delay, loop, gotoAnimation);
            return true;
        }

        private static void AddBossAnimation(
            Sprite sprite,
            string id,
            string preferredPath,
            string fallbackPath,
            float delay,
            bool loop,
            string gotoAnimation = null)
        {
            string resolvedPath = ResolveBossAnimationPath(preferredPath, fallbackPath);

            if (loop)
            {
                sprite.AddLoop(id, resolvedPath, delay);
            }
            else if (!string.IsNullOrEmpty(gotoAnimation))
            {
                sprite.Add(id, resolvedPath, delay, gotoAnimation);
            }
            else
            {
                sprite.Add(id, resolvedPath, delay);
            }
        }

        private Sprite GetActiveBossSprite()
        {
            Sprite phaseSprite = GetPhaseBossSprite(currentElsPhase);
            if (phaseSprite != null)
                return phaseSprite;

            return Sprite;
        }

        private Sprite GetActiveBossWingSprite()
        {
            return GetPhaseBossWingSprite(currentElsPhase);
        }

        private Sprite GetActiveBossEyeSprite()
        {
            return GetPhaseBossEyeSprite(currentElsPhase);
        }

        private Sprite GetActiveBossPupilSprite()
        {
            return GetPhaseBossPupilSprite(currentElsPhase);
        }

        private Sprite GetPhaseBossSprite(ElsPhase phase)
        {
            if (phase == ElsPhase.SiamoZero && siamoSprite != null)
                return siamoSprite;

            if (phase == ElsPhase.PenumbraPhastasm && penumbraSprite != null)
                return penumbraSprite;

            if (currentElsPhase == ElsPhase.PenumbraPhastasm && penumbraSprite != null)
                return penumbraSprite;

            if (doppiaSprite != null)
                return doppiaSprite;

            return null;
        }

        private Sprite GetPhaseBossWingSprite(ElsPhase phase)
        {
            if (phase == ElsPhase.SiamoZero && hasSiamoWingParts)
                return siamoWingSprite;

            if (phase == ElsPhase.PenumbraPhastasm && hasPenumbraWingParts)
                return penumbraWingSprite;

            if (phase == ElsPhase.DoppiaElillca && hasDoppiaWingParts)
                return doppiaWingSprite;

            return null;
        }

        private Sprite GetPhaseBossEyeSprite(ElsPhase phase)
        {
            if (phase == ElsPhase.SiamoZero && hasSiamoEyeParts)
                return siamoEyeSprite;

            if (phase == ElsPhase.PenumbraPhastasm && hasPenumbraEyeParts)
                return penumbraEyeSprite;

            if (phase == ElsPhase.DoppiaElillca && hasDoppiaEyeParts)
                return doppiaEyeSprite;

            return null;
        }

        private Sprite GetPhaseBossPupilSprite(ElsPhase phase)
        {
            if (phase == ElsPhase.SiamoZero && hasSiamoPupilParts)
                return siamoPupilSprite;

            if (phase == ElsPhase.PenumbraPhastasm && hasPenumbraPupilParts)
                return penumbraPupilSprite;

            if (phase == ElsPhase.DoppiaElillca && hasDoppiaPupilParts)
                return doppiaPupilSprite;

            return null;
        }

        private static void PlayBossSpriteAnimation(Sprite sprite, string animationId, string fallbackAnimation = null)
        {
            if (sprite == null)
                return;

            if (sprite.Has(animationId))
            {
                sprite.Play(animationId);
                return;
            }

            if (!string.IsNullOrEmpty(fallbackAnimation) && sprite.Has(fallbackAnimation))
                sprite.Play(fallbackAnimation);
        }

        private void PlayBossAnimationSet(ElsPhase phase, string animationId, string fallbackAnimation = null)
        {
            PlayBossSpriteAnimation(GetPhaseBossSprite(phase), animationId, fallbackAnimation);
            PlayBossSpriteAnimation(GetPhaseBossWingSprite(phase), animationId, fallbackAnimation);
            PlayBossSpriteAnimation(GetPhaseBossEyeSprite(phase), animationId, fallbackAnimation);
            PlayBossSpriteAnimation(GetPhaseBossPupilSprite(phase), animationId, fallbackAnimation);
        }

        private void PlayActiveBossAnimation(string animationId, string fallbackAnimation = null)
        {
            PlayBossAnimationSet(currentElsPhase, animationId, fallbackAnimation);
        }

        private void SetBossIntroVisualState(bool introActive)
        {
            introSequencePlaying = introActive;

            if (Sprite != null)
            {
                Sprite.Visible = introActive && Sprite.Has(RevealSequenceAnimationId);
                if (introActive && Sprite.Visible)
                {
                    Sprite.Play(RevealSequenceAnimationId);
                }
            }

            if (!introActive)
            {
                PlayActiveBossAnimation("idle", "boss");
            }

            UpdateBossSpriteVisibility();
        }
        
        private void createBossSprite()
        {
            if (this.Sprite == null)
            {
                try
                {
                    this.Sprite = GFX.SpriteBank.Create("els_true_final_boss");
                    if (this.Sprite != null)
                    {
                        this.Sprite.Visible = false;
                        Add((Component)this.Sprite);
                        if (this.Sprite.Has("idle"))
                            this.Sprite.Play("idle");
                        else if (this.Sprite.Has("boss"))
                            this.Sprite.Play("boss");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "DZ", $"SiamoZeroFinalBoss: Failed to create sprite: {ex.Message}");
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
        
        public Vector2 BeamOrigin => this.Center + (GetActiveBossSprite()?.Position ?? this.Sprite?.Position ?? Vector2.Zero) + new Vector2(0.0f, -14f);
        public Vector2 ShotOrigin => this.Center + (GetActiveBossSprite()?.Position ?? this.Sprite?.Position ?? Vector2.Zero) + new Vector2(6f * (GetActiveBossSprite()?.Scale.X ?? this.Sprite?.Scale.X ?? 1f), 2f);
        
        #endregion
        
        #region Player Collision
        
        public void OnPlayer(global::Celeste.Player player)
        {
            if (GetActiveBossSprite() == null && Sprite == null)
                return;
            PlayActiveBossAnimation("getHit", "idle");
            if (Sprite != null && Sprite != GetActiveBossSprite() && Sprite.Has("getHit"))
                Sprite.Play("getHit");
            Audio.Play("event:/new_content/char/pusheen/els/Els_Scream_Hit", Position);
            chargeSfx.Stop();
            if (laserSfx.EventName == "event:/char/badeline/boss_laser_charge" && laserSfx.Playing)
                laserSfx.Stop();
            
            // Disable shockwave effects to prevent player from getting stuck
            shockwaveEnabled = false;
            if (shockwaveCoroutine != null)
            {
                shockwaveCoroutine.Active = false;
                shockwaveCoroutine.Cancel();
                shockwaveCoroutine = null;
            }
            
            // Clear any active knockback to prevent boss movement during attract
            knockbackVelocity = Vector2.Zero;
            knockbackTimer = 0f;
            
            // Trigger starfield visual burst (visual only, no player pushback)
            if (bossBg != null)
            {
                bossBg.TriggerBurst();
            }
            
            Collidable = false;
            avoidPos = Vector2.Zero;
            ++nodeIndex;
            attackCoroutine.Active = false;
            Moving = true;
            bool lastHit = nodeIndex == nodes.Length - 1;
            
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                // Check if this is the last hit in a special room
                if (lastHit && level.Session.Level.Equals("x-12"))
                {
                    // Special handling for x-12 room
                    MarkEncounterDefeated();
                }
                else if (totalHitsTaken + 1 >= MAX_HITS)
                {
                    // Boss is defeated
                    currentState = BossState.Defeated;
                    MarkEncounterDefeated();
                }
                
                // Update hit tracking
                hitsInPhase++;
                totalHitsTaken++;
                
                // Check for phase transition
                if (totalHitsTaken % MAX_HITS_PER_PHASE == 0 && currentPhase < TOTAL_PHASES - 1)
                {
                    currentPhase++;
                    phaseTransitioning = true;
                }
            }
            
            Add(new Coroutine(MoveSequence(player, lastHit)));
        }

        
        private IEnumerator MoveSequence(global::Celeste.Player player, bool lastHit)
        {
            SiamoZeroFinalBoss finalBoss = this;
            if (lastHit)
            {
                Audio.SetMusicParam("boss_pitch", 1f);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.3f, start: true);
                tween.OnUpdate = t => Glitch.Value = 0.6f * t.Eased;
                finalBoss.Add(tween);
            }
            else
            {
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.3f, start: true);
                tween.OnUpdate = t => Glitch.Value = (float) (0.5 * (1.0 - t.Eased));
                finalBoss.Add(tween);
            }
            if (player != null && !player.Dead)
                player.StartAttract(finalBoss.Center + Vector2.UnitY * 4f);
            float timer = 0.15f;
            float maxWaitTime = 2f; // Maximum time to wait for attract to complete
            while (player != null && !player.Dead && !player.AtAttractTarget && maxWaitTime > 0f)
            {
                yield return null;
                timer -= Engine.DeltaTime;
                maxWaitTime -= Engine.DeltaTime;
            }
            // Always release player from attract state to prevent mid-air softlock
            // (previously only released on timeout, causing softlock on normal completion)
            if (player != null && !player.Dead && player.StateMachine.State == Player.StAttract)
            {
                player.StateMachine.State = Player.StNormal;
            }
            if (timer > 0.0)
                yield return timer;
            foreach (ReflectionTentacles entity in finalBoss.Scene.Tracker.GetEntities<ReflectionTentacles>())
                entity.Retreat();
            if (player != null)
            {
                global::Celeste.Celeste.Freeze(0.1f);
                timeRateModifier.SetTimeRateMultiplier(!lastHit ? 0.75f : 0.5f);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            yield return 0.05f;
            Audio.SetMusicParam("boss_pitch", 0.0f);
            float from1 = timeRateModifier.CurrentTimeRate();
            float assistAdjustedRate = Engine.TimeRate / Math.Max(from1, 0.0001f);
            Tween tween1 = Tween.Create(Tween.TweenMode.Oneshot, duration: (0.35f / assistAdjustedRate), start: true);
            tween1.UseRawDeltaTime = true;
            tween1.OnUpdate = t =>
            {
                if (bossBg != null && bossBg.Alpha < (double) t.Eased)
                    bossBg.Alpha = t.Eased;
                timeRateModifier.SetTimeRateMultiplier(MathHelper.Lerp(from1, 1f, t.Eased));
                if (!lastHit)
                    return;
                Glitch.Value = (float) (0.60000002384185791 * (1.0 - t.Eased));
            };
            finalBoss.Add(tween1);
            yield return 0.2f;
            Vector2 from2 = finalBoss.Position;
            Vector2 to = finalBoss.nodes[finalBoss.nodeIndex];
            float duration = Vector2.Distance(from2, to) / 600f;
            float dir = (to - from2).Angle();
            Tween tween2 = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, duration, true);
            tween2.OnUpdate = t =>
            {
                Position = Vector2.Lerp(from2, to, t.Eased);
                if (t.Eased < 0.10000000149011612 || t.Eased > 0.89999997615814209 || !Scene.OnInterval(0.02f))
                    return;
                TrailManager.Add(this, Player.NormalHairColor, 0.5f);
                level.Particles.Emit(Player.P_DashB, 2, Center, Vector2.One * 3f, dir);
            };
            tween2.OnComplete = t =>
            {
                PlayActiveBossAnimation("idle", "boss");
                if (Sprite != null && Sprite != GetActiveBossSprite())
                {
                    if (Sprite.Has("recoverHit"))
                        Sprite.Play("recoverHit");
                    else if (Sprite.Has("idle"))
                        Sprite.Play("idle");
                    else if (Sprite.Has("boss"))
                        Sprite.Play("boss");
                }
                Moving = false;
                Collidable = true;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    facing = Math.Sign(entity.X - X);
                    if (facing == 0)
                        facing = -1;
                }
                StartAttacking();
                floatSine.Reset();
            };
            finalBoss.Add(tween2);
        }
        
        #endregion
        
        #region HP/Phase Properties
        
        public int TotalHitsTaken => totalHitsTaken;
        public int MaxTotalHits => MAX_HITS;
        public int CurrentPhase => currentPhase;
        public int TotalPhases => TOTAL_PHASES;
        public int HitsInCurrentPhase => hitsInPhase;
        public int HitsPerPhase => MAX_HITS_PER_PHASE;
        public float PhaseProgress => (float)hitsInPhase / MAX_HITS_PER_PHASE;
        public float OverallProgress => (float)totalHitsTaken / MAX_HITS;
        public new bool IsDefeated => currentState == BossState.Defeated;
        public override string BossDisplayName => GetEncounterBossName();
        
        #endregion
        
        #region Entrance
        
        private IEnumerator finalBossEntrance()
        {
            currentState = BossState.Transitioning;
            Collidable = false;
            SetBossIntroVisualState(true);

            Audio.Play("event:/boss/els_true_form_reveal", Position);
            
            level?.Shake(3f);
            level?.Displacement.AddBurst(Position, 2f, 192f, 384f, 4f);
            level?.Flash(Color.DarkRed, true);
            
            if (Sprite == null || !Sprite.Has(RevealSequenceAnimationId))
            {
                PlayActiveBossAnimation("transform", "idle");
            }

            yield return RevealSequenceDuration + 0.2f;
            
            Audio.Play("event:/boss/els_doppia_roar", Position);
            phaseWiggler.Start();

            SetBossIntroVisualState(false);
            
            yield return 0.8f;
            
            string currentRoomId = level?.Session.Level ?? string.Empty;
            MarkEncounterIntroSeen(currentRoomId);
            currentState = BossState.Idle;
            Collidable = true;
            EnsureBossMusicState(force: true);
        }

        private string GetBossMusicEvent()
        {
            return siamoIdentity switch
            {
                SiamoZeroIdentity.Delta => MUSIC_SIAMO_ZERO_DELTA,
                SiamoZeroIdentity.Celestial => MUSIC_CELESTIAL_ZERO,
                _ => MUSIC_SIAMO_ZERO
            };
        }

        private float GetBossMusicPhaseParam()
        {
            return currentElsPhase switch
            {
                ElsPhase.PenumbraPhastasm => 1f,
                ElsPhase.SiamoZero => 2f,
                _ => 0f
            };
        }

        private float GetBossMusicVariantParam()
        {
            return siamoIdentity switch
            {
                SiamoZeroIdentity.Delta => 1f,
                SiamoZeroIdentity.Celestial => 2f,
                _ => 0f
            };
        }

        private float GetBossMusicProgress()
        {
            float maxHealth = Math.Max(1, MaxHealth);
            float healthProgress = 1f - Health / maxHealth;
            float hitProgress = Calc.ClampedMap(totalHitsTaken, 0f, MAX_HITS, 0f, 1f);
            float phaseFloor = currentElsPhase switch
            {
                ElsPhase.PenumbraPhastasm => 0.35f,
                ElsPhase.SiamoZero => 0.68f,
                _ => 0f
            };

            return Calc.Clamp(Math.Max(phaseFloor, Math.Max(healthProgress, hitProgress)), 0f, 1f);
        }

        private void EnsureBossMusicState(bool force = false)
        {
            if (level == null)
                return;

            string targetEvent = GetBossMusicEvent();
            if (string.IsNullOrWhiteSpace(targetEvent))
                return;

            if (force || !bossMusicInitialized || !string.Equals(currentBossMusicEvent, targetEvent, StringComparison.Ordinal))
            {
                Audio.SetMusic(targetEvent, true, true);
                currentBossMusicEvent = targetEvent;
                bossMusicInitialized = true;
            }

            float progress = GetBossMusicProgress();
            Audio.SetMusicParam("siamo_progress", progress);
            Audio.SetMusicParam("boss_progress", progress);
            Audio.SetMusicParam("piano", Calc.ClampedMap(progress, 0.12f, 0.48f));
            Audio.SetMusicParam("choir", Calc.ClampedMap(progress, 0.48f, 0.94f));
            Audio.SetMusicParam("choir_epic", Calc.ClampedMap(progress, 0.7f, 1f));
            Audio.SetMusicParam("siamo_phase", GetBossMusicPhaseParam());
            Audio.SetMusicParam("siamo_variant", GetBossMusicVariantParam());
            Audio.SetMusicParam("intensity", 0.25f + progress * 0.75f);
        }
        
        private List<AttackStep> parseCustomAttackSequence(string sequence)
        {
            List<AttackStep> steps = new List<AttackStep>();
            if (string.IsNullOrWhiteSpace(sequence))
                return steps;

            string[] tokens = sequence.Split(',');
            foreach (string token in tokens)
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                steps.Add(ParseAttackSequenceEntry(token));
            }

            return steps;
        }
        
        #endregion
        
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
            timeRateModifier.SetTimeRateMultiplier(slowdownScale);
            
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

                }
            }
            
            // Wait for slowdown duration
            yield return slowdownDuration;
            
            // Restore normal time rate
            timeRateModifier.ResetTimeRateMultiplier();
            
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
            // Null safety check
            if (player == null || level == null)
                return;
            
            // Don't apply shockwave pushback during hit/move sequence - causes mid-air softlock
            if (Moving || !Collidable)
                return;
            
            // Calculate direction from Asriel to player (push AWAY from boss)
            Vector2 pushDirection = (player.Center - Center).SafeNormalize();
            
            // If no valid direction, push away from player's facing direction
            if (pushDirection == Vector2.Zero)
            {
                pushDirection = new Vector2(player.Facing == Facings.Right ? 1 : -1, 0);
            }
            
            // Pushback strength - stronger for Kirby mode
            float pushStrength = isKirbyMode ? 400f : 300f;
            
            // Apply knockback velocity to the boss (gets pushed back from impact)
            knockbackVelocity = -pushDirection * (pushStrength * 0.5f); // Boss gets pushed back too
            knockbackTimer = 0.3f; // Duration of knockback
            
            // Create shockwave visual effect with expanding ring
            CreateShockwaveEffect(Center, pushDirection, isKirbyMode);
            
            // Play pushback sound
            Audio.Play("event:/new_content/char/pusheen/els/Els_Scream_Hit", Center);
            
            // Screen shake - more intense for Kirby mode
            level.Shake(isKirbyMode ? 0.5f : 0.3f);
            
            // Emit particles in a radial burst from boss center
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
                level.Flash(new Color(1.000f, 0.000f, 0.000f, 1.000f), true);
                
                // Additional particle ring
                for (int i = 0; i < 20; i++)
                {
                    float angle = (i / 20f) * MathHelper.TwoPi;
                    Vector2 particleDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    level.Particles.Emit(PBurst, 1, Center + particleDir * 16f, Vector2.One * 6f, angle);
                }
                
                // Extra displacement burst for Kirby
                level.Displacement.AddBurst(Center, 1.5f, 96f, 192f, 0.8f);
            }
        }

        private void CreateShockwaveEffect(Vector2 center, Vector2 pushDirection, bool isKirbyMode)
        {
            // Check if shockwave is disabled (to prevent player from getting stuck)
            if (!shockwaveEnabled)
                return;
            
            var level = Scene as Level;
            if (level == null)
                return;
            
            // Main displacement burst
            level.Displacement.AddBurst(center, 1.5f, 96f, 192f, 0.6f);
            level.Shake(0.5f);

            // Create expanding particle ring
            for (int i = 0; i < 16; i++)
            {
                float angle = (i / 16f) * MathHelper.TwoPi;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 32f;
                level.ParticlesFG.Emit(PBurst, 1, center + offset, Vector2.One * 4f);
            }
            
            // Start the expanding ring effect coroutine and track it
            shockwaveCoroutine = new Coroutine(ShockwaveRingEffect(isKirbyMode));
            Add(shockwaveCoroutine);
        }

        /// <summary>
        /// Create a visual shockwave ring effect
        /// </summary>
        private void StartAttacking()
        {
            isAttacking = true;
            currentState = BossState.Attacking;
            shockwaveEnabled = true;

            if (attackCoroutine != null && attackCoroutine.Active)
                return;

            if (useCustomSequence && customAttackSteps != null && customAttackSteps.Count > 0)
                attackCoroutine.Replace(RunCustomAttackSequence());
            else
                attackCoroutine.Replace(RunDefaultAttackSequence());

            if (attackCoroutine != null)
                attackCoroutine.Active = true;
        }

        private IEnumerator ShockwaveRingEffect(bool isKirbyMode)
        {
            float duration = 0.5f;
            float maxRadius = isKirbyMode ? 60f : 45f;
            int particleCount = isKirbyMode ? 32 : 24;
            
            for (float t = 0; t < duration; t += Engine.DeltaTime)
            {
                // Safety check - abort if level is no longer valid or shockwave disabled
                if (level == null || Scene == null || !shockwaveEnabled)
                    yield break;
                
                float progress = t / duration;
                float currentRadius = Ease.CubeOut(progress) * maxRadius;
                
                // Emit particles in a ring
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = (i / (float)particleCount) * MathHelper.TwoPi;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * currentRadius;
                    
                    // Emit particle at ring position
                    level?.ParticlesFG?.Emit(PBurst, 1, Center + offset, Vector2.One * 2f);
                }
                
                yield return null;
            }
        }

        #region Update and Behavior
        
        public override void Update()
        {
            base.Update();
            UpdateBossSpriteVisibility();

            if (introSequencePlaying)
            {
                if (coreLight != null)
                    coreLight.Alpha = 1f;
                return;
            }
            
            // Apply knockback velocity
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Engine.DeltaTime;
                
                // Apply the knockback movement
                Position += knockbackVelocity * Engine.DeltaTime;
                
                // Decay the knockback velocity over time
                knockbackVelocity *= 0.9f;
                
                // Stop knockback when timer expires
                if (knockbackTimer <= 0f)
                {
                    knockbackVelocity = Vector2.Zero;
                }
            }
            
            // Update defense duration
            if (isDefending)
            {
                defenseDuration -= Engine.DeltaTime;
                if (defenseDuration <= 0f)
                {
                    isDefending = false;
                    Audio.Play("event:/boss/els_defense_end", Position);
                }
            }
            
            // Update heal cooldown
            if (healCooldown > 0f)
            {
                healCooldown -= Engine.DeltaTime;
            }
            
            // Build dimension rift power over time
            if (dimensionRiftPower < MAX_RIFT_POWER)
            {
                dimensionRiftPower += Engine.DeltaTime * 2f;
                
                if (dimensionRiftPower >= MAX_RIFT_POWER && !canUseUltimateRiftAttack)
                {
                    canUseUltimateRiftAttack = true;
                    Audio.Play("event:/boss/els_rift_charged", Position);
                    phaseWiggler.Start();
                }
            }
            
            // Update based on current phase
            if (currentElsPhase == ElsPhase.DoppiaElillca)
            {
                updatePhase1();
            }
            else if (currentElsPhase == ElsPhase.PenumbraPhastasm)
            {
                updatePhase2();
            }
            else if (currentElsPhase == ElsPhase.SiamoZero)
            {
                updateSiamoZero();
            }

            UpdateAutomaticKnightCloneSummons();
            
            // Check for phase transition
            float healthPercent = (float)Health / MaxHealth;
            if (!hasTransitionedToPhase2 && healthPercent <= 0.5f)
            {
                transitionToPhase2();
            }
            
            // Update core light
            if (coreLight != null)
                coreLight.Alpha = 0.8f + phaseWiggler.Value * 0.4f;

            if (!introSequencePlaying)
                EnsureBossMusicState();
        }

        private void UpdateBossSpriteVisibility()
        {
            bool showIntroSprite = introSequencePlaying && Sprite != null && Sprite.Has(RevealSequenceAnimationId);

            if (Sprite != null)
                Sprite.Visible = showIntroSprite;

            bool showDoppia = !introSequencePlaying && currentElsPhase == ElsPhase.DoppiaElillca;
            bool showPenumbra = !introSequencePlaying && currentElsPhase == ElsPhase.PenumbraPhastasm;
            bool showSiamoZero = !introSequencePlaying && currentElsPhase == ElsPhase.SiamoZero;

            if (doppiaSprite != null)
                doppiaSprite.Visible = showDoppia;
            if (doppiaWingSprite != null)
                doppiaWingSprite.Visible = showDoppia && hasDoppiaWingParts;
            if (doppiaEyeSprite != null)
                doppiaEyeSprite.Visible = showDoppia && hasDoppiaEyeParts;
            if (doppiaPupilSprite != null)
                doppiaPupilSprite.Visible = showDoppia && hasDoppiaPupilParts;

            if (penumbraSprite != null)
                penumbraSprite.Visible = showPenumbra;
            if (penumbraWingSprite != null)
                penumbraWingSprite.Visible = showPenumbra && hasPenumbraWingParts;
            if (penumbraEyeSprite != null)
                penumbraEyeSprite.Visible = showPenumbra && hasPenumbraEyeParts;
            if (penumbraPupilSprite != null)
                penumbraPupilSprite.Visible = showPenumbra && hasPenumbraPupilParts;

            if (siamoSprite != null)
                siamoSprite.Visible = showSiamoZero;
            if (siamoWingSprite != null)
                siamoWingSprite.Visible = showSiamoZero && hasSiamoWingParts;
            if (siamoEyeSprite != null)
                siamoEyeSprite.Visible = showSiamoZero && hasSiamoEyeParts;
            if (siamoPupilSprite != null)
                siamoPupilSprite.Visible = showSiamoZero && hasSiamoPupilParts;
        }

        private static void ApplyBossLayerTransform(Sprite sprite, Vector2 position, Vector2 scale, float rotation = 0f)
        {
            if (sprite == null)
                return;

            sprite.Position = position;
            sprite.Scale = scale;
            sprite.Rotation = rotation;
        }

        private void SetPhaseLayerColor(ElsPhase phase, Color color)
        {
            if (GetPhaseBossSprite(phase) != null)
                GetPhaseBossSprite(phase).Color = color;
            if (GetPhaseBossWingSprite(phase) != null)
                GetPhaseBossWingSprite(phase).Color = color;
            if (GetPhaseBossEyeSprite(phase) != null)
                GetPhaseBossEyeSprite(phase).Color = color;
            if (GetPhaseBossPupilSprite(phase) != null)
                GetPhaseBossPupilSprite(phase).Color = color;
        }
        
        private void updatePhase1()
        {
            // Doppia Elillca behavior
            dualityFactor = energyPulse.Value;
            float haloAmount = (energyPulse.Value + 1f) * 0.5f;
            
            float wingPulse = (float)Math.Sin(Scene.TimeActive * 8f);
            float eyePulse = (float)Math.Sin(Scene.TimeActive * 5f);
            float pupilPulse = (float)Math.Sin(Scene.TimeActive * 6.5f);

            UpdateCoreHaloLight(haloAmount, penumbraPhase: false);

            ApplyBossLayerTransform(doppiaSprite, Vector2.Zero, Vector2.One * (1f + phaseWiggler.Value * 0.03f));
            ApplyBossLayerTransform(
                doppiaWingSprite,
                Vector2.Zero,
                new Vector2(1.1f + wingPulse * 0.17f, 0.88f + Math.Abs(wingPulse) * 0.18f),
                wingPulse * 0.12f
            );
            ApplyBossLayerTransform(
                doppiaEyeSprite,
                Vector2.Zero,
                new Vector2(1f + eyePulse * 0.08f, 0.9f + Math.Abs(eyePulse) * 0.11f),
                eyePulse * 0.045f
            );
            ApplyBossLayerTransform(
                doppiaPupilSprite,
                Vector2.Zero,
                new Vector2(1f + pupilPulse * 0.05f, 1f + Math.Abs(pupilPulse) * 0.06f),
                pupilPulse * 0.03f
            );
        }
        
        private void updatePhase2()
        {
            // Penumbra Phastasm behavior
            voidPower += Engine.DeltaTime * 0.1f;
            float haloAmount = (energyPulse.Value + 1f) * 0.5f;
            float wingPulse = (float)Math.Sin(Scene.TimeActive * 10f);
            float eyePulse = (float)Math.Sin(Scene.TimeActive * 7f);
            float pupilPulse = (float)Math.Sin(Scene.TimeActive * 8.5f);

            UpdateCoreHaloLight(haloAmount, penumbraPhase: true);
            UpdatePhoenixCrownLights(haloAmount);

            ApplyBossLayerTransform(penumbraSprite, Vector2.Zero, Vector2.One * (1.02f + phaseWiggler.Value * 0.04f), wingPulse * 0.02f);
            ApplyBossLayerTransform(
                penumbraWingSprite,
                Vector2.Zero,
                new Vector2(1.16f + wingPulse * 0.22f, 0.82f + Math.Abs(wingPulse) * 0.22f),
                wingPulse * 0.16f
            );
            ApplyBossLayerTransform(
                penumbraEyeSprite,
                Vector2.Zero,
                new Vector2(1f + eyePulse * 0.09f, 0.86f + Math.Abs(eyePulse) * 0.13f),
                eyePulse * 0.055f
            );
            ApplyBossLayerTransform(
                penumbraPupilSprite,
                Vector2.Zero,
                new Vector2(1f + pupilPulse * 0.06f, 1f + Math.Abs(pupilPulse) * 0.07f),
                pupilPulse * 0.035f
            );
            
            // Void mode activation at low health
            if (Health <= MaxHealth * 0.2f && !isInVoidMode)
            {
                enterVoidMode();
            }
        }
        
        private void transitionToPhase2()
        {
            hasTransitionedToPhase2 = true;
            currentState = BossState.Transitioning;
            ClearSummonedKnightClones();
            automaticCloneSummonCooldown = 1.4f;
            Add(new Coroutine(phase2TransitionSequence()));
        }
        
        private IEnumerator phase2TransitionSequence()
        {
            var level = Scene as Level;
            
            // Transition announcement
            Audio.Play("event:/boss/els_phase_transition", Position);
            level?.Shake(2f);
            
            // Visual transformation
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 0.3f)
            {
                float fade = Ease.CubeInOut(t);
                SetPhaseLayerColor(ElsPhase.DoppiaElillca, Color.White * (1f - fade));
                
                if (t >= 0.5f && penumbraSprite == null)
                {
                    setupPhase2();
                    SetPhaseLayerColor(ElsPhase.PenumbraPhastasm, Color.White * 0f);
                }
                
                if (penumbraSprite != null)
                {
                    SetPhaseLayerColor(ElsPhase.PenumbraPhastasm, Color.White * fade);
                }
                
                yield return null;
            }
            
            // Full heal for phase 2 (or partial)
            Health = MaxHealth / 2; // Half health for second phase
            
            // Massive displacement burst
            level?.Displacement.AddBurst(Position, 2.5f, 256f, 512f, 3f);
            
            // Change music to Penumbra phase
            Audio.Play("event:/boss/els_penumbra_reveal", Position);
            
            // Update phase
            currentElsPhase = ElsPhase.PenumbraPhastasm;
            
            phaseWiggler.Start();
            
            yield return 2f;

            currentState = BossState.Idle;
        }
        
        private void enterVoidMode()
        {
            isInVoidMode = true;
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "void", "boss");
            
            Audio.Play("event:/boss/els_void_mode", Position);
            
            // Change all lights to dark void
            foreach (var light in phantasmLights)
            {
                light.Color = Color.Lerp(PhoenixHaloMagenta, Color.Black, 0.45f) * 1.4f;
            }
            
            var level = Scene as Level;
            level?.Shake(1.5f);
        }
        
        // Phase 1 Attacks (Doppia Elillca)
        public void ExecuteDoppiaAttack(int attackId)
        {
            switch (attackId)
            {
                case 0:
                    doppiaCloneAssault();
                    break;
                case 1:
                    dualityWave();
                    break;
                case 2:
                    shadowBlast();
                    break;
                case 3:
                    mirrorDimension();
                    break;
                case 4:
                    dimensionalDefense();
                    break;
                case 5:
                    dualityHeal();
                    break;
                case 6:
                    riftStrikeCombo();
                    break;
                case 7:
                    quickDashAttack();
                    break;
                case 8:
                    energyOrbShot();
                    break;
                case 9:
                    burstHeal();
                    break;
            }
            
            phaseWiggler.Start();
        }
        
        private void doppiaCloneAssault()
        {
            Audio.Play(SFX_ELS_CREATE, Position);
            Audio.Play(SFX_ELS_SPAWN, Position);
            
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "attack", "boss");
            
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 1f, 128f, 256f, 1f);
            
            // Spawn shadow clones
            for (int i = 0; i < 4; i++)
            {
                float angle = (i / 4f) * MathHelper.TwoPi;
                Vector2 spawnPos = Position + new Vector2(
                    (float)Math.Cos(angle) * 150f,
                    (float)Math.Sin(angle) * 150f
                );
                
                // Clone spawn effect
                level?.ParticlesFG.Emit(PBurst, 15, spawnPos, Vector2.One * 8f);
                Audio.Play(SFX_ELS_ACTIVATE, spawnPos);
            }
            
            phaseWiggler.Start();
        }
        
        private void dualityWave()
        {
            Audio.Play(SFX_ELS_CHARGE, Position);
            
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "duality", "boss");
            
            // Create expanding wave of energy
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 1f, 128f, 256f, 1.5f);
            
            // Release wave after charge
            Alarm.Set(this, 0.5f, () =>
            {
                Audio.Play(SFX_ELS_IMPACT, Position);
                level?.Shake(1.5f);
                level?.Displacement.AddBurst(Position, 2f, 256f, 512f, 2f);
                
                // Wave particles
                for (int i = 0; i < 360; i += 10)
                {
                    float angle = MathHelper.ToRadians(i);
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    level?.ParticlesFG.Emit(PShoot, 3, Position + direction * 80f, Vector2.One * 8f);
                }
            });
            
            phaseWiggler.Start();
        }
        
        private void shadowBlast()
        {
            Audio.Play(SFX_ELS_BUILD, Position);
            
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "attack", "boss");
            var level = Scene as Level;
            
            // Build up effect
            level?.Displacement.AddBurst(Position, 0.5f, 96f, 192f, 0.8f);
            
            // Fire after buildup
            Alarm.Set(this, 0.6f, () =>
            {
                Audio.Play(SFX_ELS_SLICE, Position);
                level?.Shake(1f);
                
                // Fire shadow projectiles in all directions
                for (int i = 0; i < 12; i++)
                {
                    float angle = (i / 12f) * MathHelper.TwoPi;
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    
                    // Projectile effect
                    level?.ParticlesFG.Emit(PShoot, 8, Position, direction * 4f);
                    Audio.Play(SFX_ELS_RIFT_BULLET, Position);
                }
            });
            
            phaseWiggler.Start();
        }
        
        private void mirrorDimension()
        {
            Audio.Play(SFX_ELS_RIFT, Position);
            
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "duality", "boss");
            
            // Create mirror dimension effect
            var level = Scene as Level;
            level?.Flash(Color.Purple, false);
            level?.Displacement.AddBurst(Position, 1.8f, 192f, 384f, 1.5f);
            
            // Teleport effect
            Audio.Play(SFX_ELS_TELEPORT, Position);
            
            // Create dimensional rifts
            for (int i = 0; i < 6; i++)
            {
                float angle = (i / 6f) * MathHelper.TwoPi;
                Vector2 riftPos = Position + new Vector2(
                    (float)Math.Cos(angle) * 200f,
                    (float)Math.Sin(angle) * 200f
                );
                
                level?.ParticlesFG.Emit(PBurst, 20, riftPos, Vector2.One * 12f);
            }
            
            phaseWiggler.Start();
        }
        
        private void dimensionalDefense()
        {
            if (isDefending) return;
            
            Audio.Play(SFX_ELS_BUBBLE, Position);
            Audio.Play(SFX_ELS_ACTIVATE, Position);
            
            isDefending = true;
            defenseDuration = 5f; // Defend for 5 seconds
            
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "duality", "boss");
            
            // Visual shield effect
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.5f, 96f, 192f, 0.5f);
            level?.Flash(new Color(0.000f, 1.000f, 1.000f, 1.000f), false);
            
            // Create protective barrier particles
            for (int i = 0; i < 360; i += 15)
            {
                float angle = MathHelper.ToRadians(i);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 80f;
                level?.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 2, Position + offset, Vector2.One * 4f);
            }
            
            phaseWiggler.Start();
        }
        
        private void dualityHeal()
        {
            if (healCooldown > 0f) return;
            
            Audio.Play(SFX_ELS_REVIVAL, Position);
            Audio.Play(SFX_ELS_CHARGE, Position);
            
            int healAmount = (int)(MaxHealth * 0.15f); // Heal 15% of max health
            Health = Math.Min(Health + healAmount, MaxHealth);
            
            healCooldown = HEAL_COOLDOWN_TIME;
            
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "duality", "boss");
            
            // Healing visual effect
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 1f, 128f, 256f, 1f);
            level?.Flash(Color.LightGreen, false);
            
            // Green healing particles
            for (int i = 0; i < 30; i++)
            {
                Vector2 randomOffset = new Vector2(
                    Calc.Random.Range(-64f, 64f),
                    Calc.Random.Range(-64f, 64f)
                );
                level?.ParticlesFG.Emit(global::Celeste.Player.P_DashA, 3, Position + randomOffset, Vector2.One * 8f, Color.Green);
            }
            
            phaseWiggler.Start();
        }
        
        private void riftStrikeCombo()
        {
            if (dimensionRiftPower < 30f) return;
            
            Audio.Play(SFX_ELS_BEAMSLASH, Position);
            Audio.Play(SFX_ELS_RIFT, Position);
            
            dimensionRiftPower -= 30f; // Cost 30 rift power
            
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "attack", "boss");
            
            // Multi-hit combo using dimension rift
            Add(new Coroutine(riftStrikeSequence()));
        }
        
        private IEnumerator riftStrikeSequence()
        {
            var level = Scene as Level;
            
            for (int hit = 0; hit < 5; hit++)
            {
                // Teleport to random position near center
                Vector2 targetPos = Position + new Vector2(
                    Calc.Random.Range(-arenaRadius * 0.5f, arenaRadius * 0.5f),
                    Calc.Random.Range(-arenaRadius * 0.5f, arenaRadius * 0.5f)
                );
                
                // Rift visual
                level?.Displacement.AddBurst(Position, 0.5f, 64f, 128f, 0.3f);
                Audio.Play(SFX_ELS_TELEPORT, Position);
                
                Position = targetPos;
                
                // Strike effect
                level?.Displacement.AddBurst(Position, 0.8f, 96f, 192f, 0.5f);
                Audio.Play(SFX_ELS_IMPACT, Position);
                Audio.Play(SFX_ELS_SLICE, Position);
                
                // Create projectiles
                for (int i = 0; i < 8; i++)
                {
                    float angle = (i / 8f) * MathHelper.TwoPi;
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    // Create rift projectile
                }
                
                yield return 0.2f;
            }
        }

        private void quickDashAttack()
        {
            Audio.Play(SFX_ELS_TELEPORT, Position);
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "attack", "boss");

            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.3f, 64f, 128f, 0.5f);
            
            // Simple dash visual: move quickly to a new position (e.g., near the player)
            // For simplicity, let's just make a small, quick movement effect.
            // In a real game, this would involve more complex pathfinding or target acquisition.
            Vector2 dashTarget = Position + new Vector2(Calc.Random.Range(-100f, 100f), Calc.Random.Range(-50f, 50f));
            Position = Vector2.Lerp(Position, dashTarget, 0.8f); // Instant "dash"

            level?.Flash(Color.White * 0.5f, false);
            level?.ParticlesFG.Emit(PShoot, 10, Position, Vector2.One * 6f);
        }

        private void energyOrbShot()
        {
            Audio.Play(SFX_ELS_RIFT_BULLET, Position);
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "attack", "boss");

            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 0.2f, 32f, 64f, 0.3f);
            
            // Fire a single projectile towards player's general direction
            // For a simple attack, we'll just emit particles as a visual projectile.
            // A real projectile would be a new Entity.
            Vector2 playerPos = level?.Tracker.GetEntity<global::Celeste.Player>()?.Position ?? Position;
            Vector2 directionToPlayer = (playerPos - Position).SafeNormalize();

            level?.ParticlesFG.Emit(PShoot, 5, ShotOrigin, directionToPlayer * 5f);
            level?.Particles.Emit(PBurst, 3, ShotOrigin, directionToPlayer * 3f);
        }

        private void burstHeal()
        {
            if (healCooldown > 0f) return;

            Audio.Play(SFX_ELS_REVIVAL, Position);
            PlayBossAnimationSet(ElsPhase.DoppiaElillca, "duality", "boss");

            int healAmount = (int)(MaxHealth * 0.05f); // Small burst heal
            Health = Math.Min(Health + healAmount, MaxHealth);
            healCooldown = HEAL_COOLDOWN_TIME / 2f; // Shorter cooldown for small heal

            var level = Scene as Level;
            level?.Flash(Color.LightGreen * 0.5f, false);
            level?.Displacement.AddBurst(Position, 0.5f, 64f, 128f, 0.5f);

            // Emit healing particles
            for (int i = 0; i < 15; i++)
            {
                Vector2 randomOffset = new Vector2(
                    Calc.Random.Range(-32f, 32f),
                    Calc.Random.Range(-32f, 32f)
                );
                level?.ParticlesFG.Emit(global::Celeste.Player.P_DashA, 2, Position + randomOffset, Vector2.One * 4f, Color.LightGreen);
            }
        }
        
        // Phase 2 Attacks (Penumbra Phastasm)
        public void ExecutePenumbraAttack(int attackId)
        {
            switch (attackId)
            {
                case 0:
                    penumbraVoidStorm();
                    break;
                case 1:
                    phantasmBarrage();
                    break;
                case 2:
                    voidCollapseAttack();
                    break;
                case 3:
                    dimensionalTear();
                    break;
                case 4:
                    ultimateAnnihilation();
                    break;
                case 5:
                    voidShield();
                    break;
                case 6:
                    penumbraRegeneration();
                    break;
                case 7:
                    dimensionalCataclysm();
                    break;
                case 8:
                    riftMaelstrom();
                    break;
                case 9:
                    apocalypticRiftBlast();
                    break;
            }
            
            phaseWiggler.Start();
        }
        
        private void penumbraVoidStorm()
        {
            Audio.Play(SFX_ELS_DARKMATTER_SPAWN, Position);
            Audio.Play(SFX_ELS_BUILD, Position);
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "attack", "boss");
            
            var level = Scene as Level;
            level?.Shake(2f);
            level?.Flash(Color.Black * 0.8f, true);
            
            // Create void storm
            for (int i = 0; i < 20; i++)
            {
                Vector2 spawnPos = Position + new Vector2(
                    Calc.Random.Range(-250f, 250f),
                    Calc.Random.Range(-250f, 250f)
                );
                
                // Void projectile effect
                level?.ParticlesFG.Emit(PBurst, 10, spawnPos, Vector2.One * 10f);
                Audio.Play(SFX_ELS_SPAWN, spawnPos);
            }
            
            phaseWiggler.Start();
        }
        
        private void phantasmBarrage()
        {
            Audio.Play(SFX_ELS_SHELL_SCREAMER, Position);
            Audio.Play(SFX_ELS_PRECREATE, Position);
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "attack", "boss");
            
            // Rapid fire from all phantasm lights
            foreach (var light in phantasmLights)
            {
                // Create projectile from each light position
                var level = Scene as Level;
                level?.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 5, Position + light.Position, Vector2.One * 8f);
                Audio.Play(SFX_ELS_RIFT_BULLET, Position + light.Position);
            }
            
            phaseWiggler.Start();
        }
        
        private void voidCollapseAttack()
        {
            Audio.Play(SFX_ELS_PREIMPACT, Position);
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "void", "boss");
            
            var level = Scene as Level;
            
            // Implosion effect
            level?.Displacement.AddBurst(Position, -1.5f, 256f, 512f, 1.2f);
            
            // Then explode
            Alarm.Set(this, 1.2f, () =>
            {
                Audio.Play(SFX_ELS_BIGHIT, Position);
                Audio.Play(SFX_ELS_IMPACT, Position);
                
                level?.Shake(2f);
                level?.Displacement.AddBurst(Position, 2f, 192f, 384f, 2f);
                level?.Flash(Color.Purple, true);
                
                // Explosion particles
                for (int i = 0; i < 50; i++)
                {
                    Vector2 direction = Calc.AngleToVector(Calc.Random.NextFloat() * MathHelper.TwoPi, 1f);
                    level?.ParticlesFG.Emit(PBurst, 8, Position, direction * 12f);
                }
            });
            
            phaseWiggler.Start();
        }
        
        private void dimensionalTear()
        {
            Audio.Play(SFX_ELS_SHELLCRACK, Position);
            Audio.Play(SFX_ELS_RIFT, Position);
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "void", "boss");
            
            // Create tears in reality
            var level = Scene as Level;
            level?.Flash(Color.Black, true);
            level?.Shake(1.5f);
            
            // Create multiple tears
            for (int i = 0; i < 8; i++)
            {
                Vector2 tearPos = Position + new Vector2(
                    Calc.Random.Range(-300f, 300f),
                    Calc.Random.Range(-300f, 300f)
                );
                
                level?.Displacement.AddBurst(tearPos, 1.5f, 128f, 256f, 1.5f);
                level?.ParticlesFG.Emit(PBurst, 15, tearPos, Vector2.One * 16f);
            }
            
            phaseWiggler.Start();
        }
        
        private void ultimateAnnihilation()
        {
            if (!isInVoidMode) return;
            
            Audio.Play(SFX_ELS_FINAL_CRY, Position);
            Audio.Play(SFX_ELS_PREDEATH, Position);
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "ultimate", "void");
            
            // Final desperate attack
            var level = Scene as Level;
            level?.Shake(3f);
            level?.Displacement.AddBurst(Position, 3f, 384f, 768f, 4f);
            level?.Flash(Color.White, true);
            
            // Charge up
            Alarm.Set(this, 2f, () =>
            {
                Audio.Play(SFX_ELS_BIGHIT, Position);
                Audio.Play(SFX_ELS_STARDEATH, Position);
                
                level?.Shake(4f);
                level?.Flash(Color.Purple, true);
                
                // Massive screen effect
                for (int i = 0; i < 50; i++)
                {
                    Vector2 randomPos = Position + new Vector2(
                        Calc.Random.Range(-400f, 400f),
                        Calc.Random.Range(-400f, 400f)
                    );
                    
                    // Create explosion projectiles
                    level?.Displacement.AddBurst(randomPos, 2f, 128f, 256f, 1.5f);
                    level?.ParticlesFG.Emit(PBurst, 20, randomPos, Vector2.One * 20f);
                }
            });
            
            phaseWiggler.Start();
        }
        
        private void voidShield()
        {
            if (isDefending) return;
            
            Audio.Play(SFX_ELS_BUBBLE, Position);
            Audio.Play(SFX_ELS_DARKMATTER_SPAWN, Position);
            
            isDefending = true;
            defenseDuration = 6f; // Longer defense in phase 2
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "void", "boss");
            
            // Massive void shield
            var level = Scene as Level;
            level?.Shake(1f);
            level?.Displacement.AddBurst(Position, 1.5f, 192f, 384f, 1.5f);
            
            // Dark shield particles
            for (int i = 0; i < 360; i += 10)
            {
                float angle = MathHelper.ToRadians(i);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 120f;
                level?.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 3, Position + offset, Vector2.One * 6f, Color.Purple);
            }
            
            // All phantasm lights intensify
            foreach (var light in phantasmLights)
            {
                light.Alpha = 1.5f;
            }
            
            phaseWiggler.Start();
        }
        
        private void penumbraRegeneration()
        {
            if (healCooldown > 0f) return;
            
            Audio.Play(SFX_ELS_REVIVAL, Position);
            Audio.Play(SFX_ELS_CHARGE, Position);
            
            int healAmount = (int)(MaxHealth * 0.20f); // Heal 20% in phase 2
            Health = Math.Min(Health + healAmount, MaxHealth);
            
            healCooldown = HEAL_COOLDOWN_TIME;
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "void", "boss");
            
            // Void energy absorption effect
            var level = Scene as Level;
            level?.Displacement.AddBurst(Position, 1.5f, 192f, 384f, 2f);
            
            // Purple/dark healing energy
            for (int i = 0; i < 50; i++)
            {
                Vector2 randomOffset = new Vector2(
                    Calc.Random.Range(-128f, 128f),
                    Calc.Random.Range(-128f, 128f)
                );
                level?.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 4, Position + randomOffset, Vector2.One * 12f, Color.Purple);
            }
            
            // Also restore some rift power
            dimensionRiftPower = Math.Min(dimensionRiftPower + 20f, MAX_RIFT_POWER);
            
            phaseWiggler.Start();
        }
        
        private void dimensionalCataclysm()
        {
            if (dimensionRiftPower < 40f) return;
            
            Audio.Play(SFX_ELS_TIME_MANIPULATOR_START, Position);
            Audio.Play(SFX_ELS_PRECREATE, Position);
            
            dimensionRiftPower -= 40f;
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "ultimate", "void");
            
            // Create multiple dimension rifts across the arena
            Add(new Coroutine(dimensionalCataclysmSequence()));
        }
        
        private IEnumerator dimensionalCataclysmSequence()
        {
            var level = Scene as Level;
            
            // Create 8 rifts in a pattern
            for (int rift = 0; rift < 8; rift++)
            {
                float angle = (rift / 8f) * MathHelper.TwoPi;
                Vector2 riftPos = Position + new Vector2(
                    (float)Math.Cos(angle) * 250f,
                    (float)Math.Sin(angle) * 250f
                );
                
                // Rift spawn effect
                level?.Displacement.AddBurst(riftPos, 1.5f, 128f, 256f, 1f);
                level?.Flash(Color.Purple, false);
                Audio.Play(SFX_ELS_RIFT, riftPos);
                Audio.Play(SFX_ELS_SPAWN, riftPos);
                
                // Create projectiles from rift
                for (int i = 0; i < 12; i++)
                {
                    float projAngle = (i / 12f) * MathHelper.TwoPi;
                    Vector2 direction = new Vector2((float)Math.Cos(projAngle), (float)Math.Sin(projAngle));
                    // Create dimensional projectile
                }
                
                yield return 0.3f;
            }
            
            // Final explosion
            level?.Shake(2f);
            level?.Displacement.AddBurst(Position, 2f, 256f, 512f, 2f);
        }
        
        private void riftMaelstrom()
        {
            if (dimensionRiftPower < 50f) return;
            
            Audio.Play(SFX_ELS_TIME_MANIPULATOR_START, Position);
            Audio.Play(SFX_ELS_RIFT, Position);
            
            dimensionRiftPower -= 50f;
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "attack", "boss");
            
            var level = Scene as Level;
            level?.Shake(1.5f);
            level?.Flash(Color.Purple, false);
            
            // Spinning vortex of rifts
            Add(new Coroutine(riftMaelstromSequence()));
        }
        
        private IEnumerator riftMaelstromSequence()
        {
            var level = Scene as Level;
            
            float duration = 4f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                // Create spiral pattern of rifts
                float angle = elapsed * 4f;
                float radius = 100f + elapsed * 30f;
                
                Vector2 riftPos = Position + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );
                
                level?.Displacement.AddBurst(riftPos, 0.5f, 64f, 128f, 0.3f);
                level?.ParticlesFG.Emit(PBurst, 5, riftPos, Vector2.One * 6f);
                
                if (elapsed % 0.5f < 0.1f)
                {
                    Audio.Play(SFX_ELS_TELEPORT, riftPos);
                }
                
                // Create projectiles
                for (int i = 0; i < 6; i++)
                {
                    float projAngle = (i / 6f) * MathHelper.TwoPi + angle;
                    Vector2 direction = new Vector2((float)Math.Cos(projAngle), (float)Math.Sin(projAngle));
                    
                    level?.ParticlesFG.Emit(PShoot, 3, riftPos, direction * 8f);
                    
                    if (i == 0)
                        Audio.Play(SFX_ELS_RIFT_BULLET, riftPos);
                }
                
                elapsed += Engine.DeltaTime;
                yield return null;
            }
        }
        
        private void apocalypticRiftBlast()
        {
            if (!canUseUltimateRiftAttack || dimensionRiftPower < MAX_RIFT_POWER) return;
            
            Audio.Play(SFX_ELS_KNOCKOUT, Position);
            Audio.Play(SFX_ELS_FINAL_CRY, Position);
            
            dimensionRiftPower = 0f;
            canUseUltimateRiftAttack = false;
            
            PlayBossAnimationSet(ElsPhase.PenumbraPhastasm, "ultimate", "void");
            
            var level = Scene as Level;
            level?.Flash(Color.Red, true);
            level?.Shake(2f);
            
            // Ultimate dimension rift attack
            Add(new Coroutine(apocalypticRiftSequence()));
        }
        
        private IEnumerator apocalypticRiftSequence()
        {
            var level = Scene as Level;
            
            // Charge up
            Audio.Play(SFX_ELS_BUILD, Position);
            Audio.Play(SFX_ELS_CHARGE, Position);
            level?.Shake(1f);
            
            for (float t = 0; t < 2f; t += Engine.DeltaTime)
            {
                level?.Displacement.AddBurst(Position, 0.5f, 192f, 384f, 0.2f);
                
                // Pull in effect
                for (int i = 0; i < 5; i++)
                {
                    Vector2 randomPos = Position + new Vector2(
                        Calc.Random.Range(-300f, 300f),
                        Calc.Random.Range(-300f, 300f)
                    );
                    level?.ParticlesFG.Emit(global::Celeste.Player.P_DashB, 2, randomPos, Vector2.One * 8f);
                }
                
                yield return null;
            }
            
            // Release
            Audio.Play(SFX_ELS_BIGHIT, Position);
            Audio.Play(SFX_ELS_IMPACT, Position);
            Audio.Play(SFX_ELS_TIME_MANIPULATOR_END, Position);
            level?.Shake(4f);
            level?.Flash(Color.White, true);
            level?.Displacement.AddBurst(Position, 4f, 512f, 1024f, 5f);
            
            // Create massive wave of dimensional energy
            for (int wave = 0; wave < 3; wave++)
            {
                Audio.Play(SFX_ELS_BEAMSLASH, Position);
                Audio.Play(SFX_ELS_SLICE, Position);
                
                for (int i = 0; i < 24; i++)
                {
                    float angle = (i / 24f) * MathHelper.TwoPi;
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    
                    // Apocalyptic projectiles
                    level?.ParticlesFG.Emit(PShoot, 10, Position, direction * 15f);
                    
                    if (i % 6 == 0)
                        Audio.Play(SFX_ELS_RIFT_BULLET, Position);
                }
                
                yield return 0.4f;
                level?.Shake(2f);
            }
            
            // Create reality tears across arena
            for (int tear = 0; tear < 15; tear++)
            {
                Vector2 tearPos = Position + new Vector2(
                    Calc.Random.Range(-400f, 400f),
                    Calc.Random.Range(-400f, 400f)
                );
                
                level?.Displacement.AddBurst(tearPos, 1.5f, 128f, 256f, 1f);
                level?.ParticlesFG.Emit(PBurst, 12, tearPos, Vector2.One * 10f);
                
                Audio.Play(SFX_ELS_RIFT, tearPos);
                Audio.Play(SFX_ELS_SHELLCRACK, tearPos);
                
                yield return 0.1f;
            }
            
            // Final shockwave
            Audio.Play(SFX_ELS_STARDEATH, Position);
            level?.Shake(3f);
            level?.Displacement.AddBurst(Position, 3f, 384f, 768f, 3f);
        }
        
        public override void Render()
        {
            // Defense shield visual
            if (isDefending)
            {
                float shieldAlpha = 0.4f + energyPulse.Value * 0.3f;
                Color shieldColor = currentElsPhase == ElsPhase.DoppiaElillca ? Color.Blue : Color.Purple;
                
                Draw.Rect(Position.X - 96f, Position.Y - 96f, 192f, 192f, shieldColor * shieldAlpha);
                Draw.HollowRect(Position.X - 96f, Position.Y - 96f, 192f, 192f, shieldColor * (shieldAlpha * 1.5f));
            }
            
            // Dimension rift power indicator
            if (dimensionRiftPower > 0f)
            {
                float powerPercent = dimensionRiftPower / MAX_RIFT_POWER;
                Color riftColor = Color.Lerp(Color.Cyan, Color.Magenta, powerPercent);
                
                // Power bar
                float barWidth = 128f * powerPercent;
                Draw.Rect(Position.X - 64f, Position.Y - 100f, barWidth, 4f, riftColor * 0.8f);
                
                // Fully charged effect
                if (canUseUltimateRiftAttack)
                {
                    float pulse = (float)Math.Sin(Scene.TimeActive * 10f) * 0.5f + 0.5f;
                    Draw.Rect(Position.X - 128f, Position.Y - 128f, 256f, 256f, Color.White * (pulse * 0.2f));
                }
            }
            
            // Add phase-specific visual effects
            if (currentElsPhase == ElsPhase.DoppiaElillca)
            {
                // Duality glow effect
                Draw.Rect(Position.X - 64f, Position.Y - 64f, 128f, 128f,
                    Color.Red * (dualityFactor * 0.2f));
                Draw.Rect(Position.X - 64f, Position.Y - 64f, 128f, 128f,
                    Color.Blue * ((1f - dualityFactor) * 0.2f));
            }
            else if (currentElsPhase == ElsPhase.PenumbraPhastasm)
            {
                // Void aura
                if (isInVoidMode)
                {
                    Draw.Rect(Position.X - 192f, Position.Y - 192f, 384f, 384f,
                        Color.Black * (energyPulse.Value * 0.5f));
                }
                else
                {
                    Draw.Rect(Position.X - 128f, Position.Y - 128f, 256f, 256f,
                        Color.Purple * (energyPulse.Value * 0.3f));
                }
            }
            else if (currentElsPhase == ElsPhase.SiamoZero)
            {
                // Timeborder glow when active
                if (siamoTimeborderActive)
                {
                    float tbPulse = (float)Math.Sin(Scene.TimeActive * 4f) * 0.15f;
                    Draw.Rect(Position.X - 192f, Position.Y - 192f, 384f, 384f,
                        SiamoTimeborderRed * (0.2f + tbPulse));
                }
                // Base Siamo aura
                float siamoAuraAlpha = energyPulse.Value * 0.25f;
                Color siamoAuraColor = currentSiamoSubPhase == SiamoSubPhase.AeonHeroFake
                    ? SiamoAeonGold : SiamoMorphoPurple;
                Draw.Rect(Position.X - 128f, Position.Y - 128f, 256f, 256f,
                    siamoAuraColor * siamoAuraAlpha);
            }

            GetActiveBossWingSprite()?.Render();
            GetActiveBossSprite()?.Render();
            
            base.Render();

            GetActiveBossEyeSprite()?.Render();
            GetActiveBossPupilSprite()?.Render();
        }
        
        // Damage handling with defense reduction
        public override void TakeDamage(int damage)
        {
            if (Health <= 0)
            {
                return;
            }
            
            if (isDefending)
            {
                int reducedDamage = (int)(damage * (1f - defenseReduction));
                Health -= reducedDamage;
                
                Audio.Play("event:/boss/els_defense_block", Position);
                
                // Visual feedback for blocking
                var level = Scene as Level;
                level?.Displacement.AddBurst(Position, 0.3f, 48f, 96f, 0.2f);
            }
            else
            {
                Health -= damage;
                
                // Build rift power when taking damage
                dimensionRiftPower = Math.Min(dimensionRiftPower + damage * 0.5f, MAX_RIFT_POWER);
            }
        }
        
        public void SetAllies(List<global::Celeste.Player> teamMembers)
        {
            allies = teamMembers;
        }
        
        public void RegisterTeamAttack()
        {
            teamAttackCounter++;
            
            // Bonus damage when all team members attack together
            if (teamAttackCounter >= allies.Count)
            {
                Audio.Play("event:/boss/team_attack_bonus", Position);
                phaseWiggler.Start();
                teamAttackCounter = 0;
            }
        }
        
        #endregion
        
        public override void Removed(Scene scene)
        {
            ClearSummonedKnightClones();
            base.Removed(scene);
        }
    }
}

