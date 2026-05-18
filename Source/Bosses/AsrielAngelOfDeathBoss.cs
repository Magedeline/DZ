using Celeste.Helpers;
using Celeste.Entities;
using HeartGem = Celeste.HeartGem; // Vanilla HeartGem for particles (P_BlueShine)
using FMOD.Studio;
using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    /// <summary>
    /// Asriel Angel of Death Boss - Chapter 20: The End
    /// Multi-phase boss fight with emotional story beats, barrier mechanics,
    /// lost soul salvation, and FMOD audio integration.
    /// Sprite path: characters/asrielangelofdeathboss
    /// </summary>
    [CustomEntity("MaggyHelper/AsrielAngelOfDeathBoss")]
    [Tracked]
    [HotReloadable]
    public class AsrielAngelOfDeathBoss : BossActor
    {
        #region Boss States and Phases
        public enum BossPhase
        {
            Dormant,           // Pre-fight state
            RiseSequence,      // Boss rises behind player, creates barrier
            Phase1,            // Angel of Death form
            Struggle,          // Player is trapped, calling for help
            VoidAnswer,        // Astral Birth Void answers the call
            LostSouls,         // Saving lost souls to weaken Asriel
            FlashbackTrigger,  // Player calls out "Azzy" 
            MemoryRecovery,    // Asriel remembers who he was
            FinalBeam,         // Els still possesses Asriel - final attack
            Redemption,        // Asriel breaks free
            Defeated
        }

        public enum AttackType
        {
            // Phase 1 Attacks
            UltimaBullet,
            CrossShocker,
            StarStormUltra,
            CosmicSweep,
            DivineLightning,
            
            // Phase 2 / Transcendent Attacks
            ShockerBreaker3,
            GalacticNova,
            HyperGoner,
            RainbowDelta,
            FinalBeam
        }
        #endregion

        #region Boss Properties
        public BossPhase CurrentPhase { get; private set; }
        public bool IsVulnerable { get; private set; }
        public int SoulsRescued { get; private set; }
        public bool PlayerIsTrapped { get; private set; }
        
        private const int TOTAL_LOST_SOULS = 12; // Magolor, Chara, Theo, Oshiro, Toriel, Asgore, Alphys, Papyrus, Sans, Undyne, Ralsei, Starsei

        private global::Celeste.Player player;
        private global::Celeste.Level level;

        // Visual components
        private string currentAnimation;
        private Vector2 basePosition;
        private Vector2 riseStartPosition;
        
        // Multi-part sprite components
        private Sprite faceSprite;
        private Sprite orbSprite;
        private Sprite orbwingSprite;
        private Sprite shoulderSprite;
        private Sprite stemSprite;
        private Sprite bgSprite;
        private Sprite cosmowingSprite;
        private Sprite crySprite;
        
        // GML-converted animation state (from obj_asrielfinal/obj_mypart1)
        private float anim;           // Animation frame counter
        private float siner;          // Sine wave counter for oscillations
        private float side;           // Background scroll offset
        private float yoff;           // Vertical offset from sin(siner / 4)
        private float yoff2;          // Secondary vertical offset from sin(siner / 16)
        private int cry;              // Cry state: 0=normal, 1=cry1, 2=cry2
        private int ucon;             // Ultimate attack controller state
        private int bcon;             // Beam attack controller state
        private float ar_shake;       // Arm shake intensity
        private float armrot;         // Arm rotation angle
        private float bodyfader;      // Body fade alpha (0-1)
        private bool darker;          // Screen darkening flag
        private float darker_x;       // Darkening amount
        private float arf;            // Arm rotation force
        private int turns;            // Battle turn counter
        private int hits;             // Hit counter for final attack
        private float radi;           // Beam charge radius
        private float r_siner;        // Beam sine counter
        private float r_al;           // Beam alpha
        private bool r_break;         // Beam break flag
        private float armx, army;     // Arm vector positions
        private int psfx;             // Sound effect handle
        private bool attacked;        // Attack completed flag
        private float mycommand;        // Random command value
        private float vol;            // Music volume for fade
        private int songcon;          // Song controller state
        private int savecon_a;        // Save controller state
        private int savecon_a_x;      // Save controller timer
        private int endcon;           // End controller state
        private int gocon;            // Go controller state
        private int gotimer;          // Go timer
        private int whatiheard;       // Player choice heard
        private int nextbattle;       // Next battle group
        private int gg;               // Random value
        private int gen_type;         // Generator type for ultimate
        private bool talked;          // Talk state flag
        private int blconwd;          // Dialog writer instance reference
        private int blcon;            // Dialog box reference
        private int iii;              // Instance reference
        private bool rickyImmune;     // Ricky's immunity flag
        
        // FMOD event instances for audio
        private EventInstance psfxEvent;
        private EventInstance batmusic;

        // Attack patterns
        private List<AttackType> currentAttackPattern;
        private int attackIndex;
        private float attackCooldown;
        
        // Barrier mechanics
        private UndertaleBarrier activeBarrier;
        private bool barrierActive;
        
        // Audio - FMOD Events
        private const string MUSIC_BURN_IN_DESPAIR = "event:/desolozantas/final_content/music/lvl20/burn_in_despair";
        private const string MUSIC_HIS_THEME_01 = "event:/desolozantas/final_content/music/lvl20/his_theme01";
        private const string MUSIC_HIS_THEME_02 = "event:/desolozantas/final_content/music/lvl20/his_theme02";
        private const string MUSIC_KIRBY_VS_ASRIEL = "event:/desolozantas/final_content/music/lvl20/kirby_vs_asriel_fight_2";
        
        // Lost soul tracking
        private Dictionary<string, bool> soulsSaved;
        private List<LostSoulEntity> activeSouls;
        #endregion

        #region Constructor
        // Scale factor for Celeste-style pixel art downsampling (50% of original)
        public static readonly Vector2 SpriteScale = new Vector2(0.5f, 0.5f);

        public AsrielAngelOfDeathBoss(Vector2 position) 
            : base(position, 
                   spriteName: "asriel_angelofdeath",
                   spriteScale: SpriteScale,
                   maxFall: 0f, // Boss hovers
                   collidable: true,
                   solidCollidable: false,
                   gravityMult: 0f,
                   collider: new Hitbox(64, 96, -32, -96))
        {
            Initialize();
        }

        private bool autoStart = false;

        public AsrielAngelOfDeathBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, 
                   spriteName: "asriel_angelofdeath",
                   spriteScale: SpriteScale,
                   maxFall: 0f,
                   collidable: true,
                   solidCollidable: false,
                   gravityMult: 0f,
                   collider: new Hitbox(64, 96, -32, -96))
        {
            Health = data.Int("health", 2500);
            MaxHealth = data.Int("maxHealth", 2500);
            autoStart = data.Bool("autoStart", false);
            Initialize();
        }

        private void Initialize()
        {
            // Set up basic properties
            if (Health <= 0) Health = MaxHealth = 2500;
            CurrentPhase = BossPhase.Dormant;
            IsVulnerable = false;
            SoulsRescued = 0;
            PlayerIsTrapped = false;
            barrierActive = false;
            
            // Store base position
            basePosition = Position;
            riseStartPosition = Position + new Vector2(0, 400); // Start below screen
            
            // Initialize sprite components
            SetupSpriteComponents();
            
            // Initialize soul tracking
            InitializeLostSouls();
            
            // Add collision handling
            Add(new PlayerCollider(OnPlayerCollision));
            
            // Start main coroutine
            Add(new Coroutine(BossRoutine()));
            
            // If autoStart is enabled, start the boss fight immediately when added to scene
            if (autoStart)
            {
                Add(new Coroutine(AutoStartRoutine()));
            }
        }

        private IEnumerator AutoStartRoutine()
        {
            // Wait for level to be ready
            while (level == null)
            {
                level = Scene as global::Celeste.Level;
                yield return null;
            }
            
            yield return null;
            
            // Make visible and start fight immediately
            Visible = true;
            CurrentPhase = BossPhase.RiseSequence;
        }
        #endregion

        #region Sprite Setup
        private void SetupSpriteComponents()
        {
            // Load individual sprite parts for the multi-component boss
            if (GFX.SpriteBank.Has("asriel_angelofdeath"))
            {
                // Main sprite already created by BossEntity
                if (Sprite != null)
                {
                    Sprite.Play("idle");
                    Sprite.CenterOrigin();
                }
            }
            
            // Create additional sprite layers for effects
            CreateSpriteLayer(ref bgSprite, "bg", "00");
            CreateSpriteLayer(ref cosmowingSprite, "cosmoswing", "00");
            CreateSpriteLayer(ref faceSprite, "face", "00");
            CreateSpriteLayer(ref orbSprite, "orb", "00");
            CreateSpriteLayer(ref orbwingSprite, "orbwing", "00");
            CreateSpriteLayer(ref shoulderSprite, "shoulder", "00");
            CreateSpriteLayer(ref stemSprite, "stem", "00");
            CreateSpriteLayer(ref crySprite, "cry", "00");
            
            currentAnimation = "idle";
        }

        private void CreateSpriteLayer(ref Sprite sprite, string folder, string defaultFrame)
        {
            string path = $"characters/asrielangelofdeathboss/{folder}/";
            
            try
            {
                sprite = new Sprite(GFX.Game, path);
                
                // Add animation loops based on folder type
                if (folder == "face")
                {
                    // Face has 8 frames (00-07) - create face0-face7 animations
                    for (int i = 0; i < 8; i++)
                        sprite.AddLoop($"face{i}", $"{i:D2}", 0.1f);
                    sprite.AddLoop("idle", "00", 0.1f);
                }
                else if (folder == "cry")
                {
                    // Cry has 3 frames (00-02)
                    sprite.AddLoop("cry0", "00", 0.1f);
                    sprite.AddLoop("cry1", "01", 0.1f);
                    sprite.AddLoop("cry2", "02", 0.1f);
                    // Create cry animations for each frame
                    for (int i = 0; i < 4; i++)
                    {
                        sprite.AddLoop($"cry{i}", $"{(i % 3):D2}", 0.1f);
                        sprite.AddLoop($"cry2_{i}", $"{(i % 3):D2}", 0.1f);
                    }
                    sprite.AddLoop("idle", "00", 0.1f);
                }
                else if (folder == "cosmoswing" || folder == "orb" || folder == "orbwing" || folder == "stem" || folder == "shoulder")
                {
                    // These have animation frames - add numbered loops
                    int frameCount = folder == "stem" ? 5 : (folder == "shoulder" ? 4 : 3);
                    // Add individual frame loops (00, 01, 02, etc.)
                    for (int i = 0; i < frameCount; i++)
                    {
                        sprite.AddLoop($"wing{i}", $"{i:D2}", 0.1f);
                        sprite.AddLoop($"{i:D2}", $"{i:D2}", 0.1f);
                    }
                    sprite.AddLoop("idle", "00", 0.1f);
                }
                else
                {
                    // Static sprites (bg, blackbg) - just use the default frame
                    sprite.AddLoop("idle", defaultFrame, 0.1f);
                }
                
                sprite.CenterOrigin();
                sprite.Visible = true;
                sprite.Play("idle");
                Add(sprite);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "MaggyHelper", $"Failed to load sprite for {folder}: {ex.Message}");
                sprite = null;
            }
        }

        private void InitializeLostSouls()
        {
            soulsSaved = new Dictionary<string, bool>
            {
                { "MAGOLOR", false },
                { "CHARA", false },
                { "THEO", false },
                { "OSHIRO", false },
                { "TORIEL", false },
                { "ASGORE", false },
                { "ALPHYS", false },
                { "PAPYRUS", false },
                { "SANS", false },
                { "UNDYNE", false },
                { "RALSEI", false },
                { "STARSEI", false }
            };
            
            activeSouls = new List<LostSoulEntity>();
        }
        #endregion

        #region Main Boss Routine
        private IEnumerator BossRoutine()
        {
            // Wait for level to be ready
            while (level == null)
            {
                level = Scene as global::Celeste.Level;
                yield return null;
            }
            
            // Find player
            player = level.Tracker.GetEntity<global::Celeste.Player>();

            while (CurrentPhase != BossPhase.Defeated)
            {
                switch (CurrentPhase)
                {
                    case BossPhase.Dormant:
                        yield return DormantPhase();
                        break;
                    case BossPhase.RiseSequence:
                        yield return RiseSequencePhase();
                        break;
                    case BossPhase.Phase1:
                        yield return Phase1Combat();
                        break;
                    case BossPhase.Struggle:
                        yield return StrugglePhase();
                        break;
                    case BossPhase.VoidAnswer:
                        yield return VoidAnswerPhase();
                        break;
                    case BossPhase.LostSouls:
                        yield return LostSoulsPhase();
                        break;
                    case BossPhase.FlashbackTrigger:
                        yield return FlashbackTriggerPhase();
                        break;
                    case BossPhase.MemoryRecovery:
                        yield return MemoryRecoveryPhase();
                        break;
                    case BossPhase.FinalBeam:
                        yield return FinalBeamPhase();
                        break;
                    case BossPhase.Redemption:
                        yield return RedemptionPhase();
                        break;
                }
                
                yield return 0.1f;
            }
        }

        /// <summary>
        /// Start the boss fight - called by trigger or cutscene
        /// </summary>
        public override void StartBossFight()
        {
            if (CurrentPhase == BossPhase.Dormant)
            {
                CurrentPhase = BossPhase.RiseSequence;
            }
            base.StartBossFight();
        }

        /// <summary>
        /// Determines if the Angel of Death phase should be active based on current room.
        /// </summary>
        private bool IsAngelPhaseRoom()
        {
            string currentRoomId = level.Session.Level;
            string[] transitionRoomIds = new string[]
            {
                "azzyboss-hypergoner",  // HyperGoner specific room
            };

            foreach (string id in transitionRoomIds)
            {
                if (currentRoomId.Equals(id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
        #endregion

        #region Phase 1: Rise Sequence - Boss rises behind player and creates barrier
        private IEnumerator DormantPhase()
        {
            // Boss is invisible/dormant until triggered (unless autoStart is enabled)
            if (!autoStart)
            {
                Visible = false;
                Collidable = false;
            }
            else
            {
                Visible = true;
                Collidable = true;
            }
            
            while (CurrentPhase == BossPhase.Dormant)
            {
                yield return null;
            }
        }

        private IEnumerator RiseSequencePhase()
        {
            // Make boss visible
            Visible = true;
            
            // Start with ominous music
            Audio.SetMusic(MUSIC_BURN_IN_DESPAIR);
            
            // Position behind where player will see
            Position = riseStartPosition;
            
            // Dramatic pause
            yield return 1f;
            
            // Rise up behind the player
            float riseTime = 3f;
            float timer = 0f;
            
            while (timer < riseTime)
            {
                timer += Engine.DeltaTime;
                float progress = Ease.CubeOut(timer / riseTime);
                Position = Vector2.Lerp(riseStartPosition, basePosition, progress);
                
                // Camera shake increases as boss rises
                if (timer > riseTime * 0.5f)
                {
                    level.DirectionalShake(Vector2.UnitY, 0.1f);
                }
                
                yield return null;
            }
            
            Position = basePosition;
            
            // Play dramatic sfx
            Audio.Play("event:/desolozantas/sfx/boss/asriel_rise", Position);
            level.DirectionalShake(Vector2.One, 0.5f);
            
            // AFTER REFUSAL - Kill player with overwhelming power
            yield return Textbox.Say("CH20_ASRIEL_ZERO_RISE_KILL");
            
            // Create Undertale-style barrier - player cannot escape
            CreateBarrier();
            
            // Force player into struggle state
            PlayerIsTrapped = true;
            
            // Transition to struggle phase
            CurrentPhase = BossPhase.Struggle;
        }

        /// <summary>
        /// Phase 1 combat - before player gets trapped
        /// </summary>
        private IEnumerator Phase1Combat()
        {
            // Set attack pattern for Phase 1
            SetPhase1AttackPattern();
            
            // Execute attacks until phase changes
            float phaseTime = 30f;
            float timer = 0f;
            
            while (timer < phaseTime && CurrentPhase == BossPhase.Phase1)
            {
                yield return ExecuteCurrentAttack();
                timer += attackCooldown;
                yield return attackCooldown;
            }
            
            // If player survives, transition to rise sequence for the kill
            if (CurrentPhase == BossPhase.Phase1)
            {
                CurrentPhase = BossPhase.RiseSequence;
            }
        }

        private void SetPhase1AttackPattern()
        {
            currentAttackPattern = new List<AttackType>
            {
                AttackType.UltimaBullet,
                AttackType.CrossShocker,
                AttackType.StarStormUltra
            };
            attackIndex = 0;
            attackCooldown = 2f;
        }

        private IEnumerator ExecuteCurrentAttack()
        {
            if (currentAttackPattern == null || currentAttackPattern.Count == 0)
                yield break;

            AttackType attack = currentAttackPattern[attackIndex % currentAttackPattern.Count];
            attackIndex++;

            switch (attack)
            {
                case AttackType.UltimaBullet:
                    yield return UltimaBulletAttack();
                    break;
                case AttackType.CrossShocker:
                    yield return CrossShockerAttack();
                    break;
                case AttackType.StarStormUltra:
                    yield return StarStormUltraAttack();
                    break;
            }
        }

        private IEnumerator StarStormUltraAttack()
        {
            if (Sprite != null)
            {
                Sprite.Play("attack_starstormultra_start");
            }
            
            yield return 0.5f;
            
            // Rain projectiles from above
            if (player != null && level != null)
            {
                for (int wave = 0; wave < 3; wave++)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float x = player.Position.X + Calc.Random.Range(-100f, 100f);
                        Audio.Play("event:/desolozantas/sfx/boss/star_fall", new Vector2(x, Position.Y));
                        yield return 0.15f;
                    }
                    yield return 0.5f;
                }
            }
            
            if (Sprite != null)
            {
                Sprite.Play("idle");
            }
            
            yield return 0.5f;
        }

        private void CreateBarrier()
        {
            if (level == null) return;
            
            // Create barrier around the arena
            activeBarrier = new UndertaleBarrier(
                Position - new Vector2(200, 150),
                400f, 300f,
                Color.White * 0.8f
            );
            
            level.Add(activeBarrier);
            barrierActive = true;
            
            Audio.Play("event:/desolozantas/sfx/boss/barrier_create", Position);
        }
        #endregion

        #region Phase 2: Struggle - Player is trapped, calling for help
        private IEnumerator StrugglePhase()
        {
            // Player struggles but nothing happens
            yield return Textbox.Say("CH20_ASRIEL_ZERO_STRUGGLE_START");
            
            // Player tries to move but can't escape
            if (player != null)
            {
                // Disable player movement temporarily
                player.StateMachine.State = global::Celeste.Player.StDummy;
            }
            
            yield return 1f;
            
            // Player calls out for help - no answer
            yield return Textbox.Say("CH20_ASRIEL_ZERO_CALL_FOR_HELP");
            
            // Attempt to call Madeline
            yield return Textbox.Say("CH20_ASRIEL_ZERO_CALL_MADELINE");
            yield return 0.5f;
            
            // Attempt to call Badeline
            yield return Textbox.Say("CH20_ASRIEL_ZERO_CALL_BADELINE");
            yield return 0.5f;
            
            // Attempt to call anyone
            yield return Textbox.Say("CH20_ASRIEL_ZERO_CALL_ANYONE");
            yield return 1f;
            
            // Final desperate call into the void
            yield return Textbox.Say("CH20_ASRIEL_ZERO_CALL_VOID");
            
            // Transition to void answer phase
            CurrentPhase = BossPhase.VoidAnswer;
        }
        #endregion

        #region Phase 3: Void Answer - Astral Birth Void answers the call
        private IEnumerator VoidAnswerPhase()
        {
            // Dramatic pause
            yield return 2f;
            
            // Visual effect - void energy appears
            SpawnVoidEnergyEffects();
            
            // The void responds
            yield return Textbox.Say("CH20_ASRIEL_ZERO_VOID_ANSWERS");
            
            // Switch music to His Theme (hopeful version)
            Audio.SetMusic(MUSIC_HIS_THEME_01);
            
            // Re-enable player with new determination
            if (player != null)
            {
                player.StateMachine.State = global::Celeste.Player.StNormal;
            }
            
            // Player receives guidance from Astral Birth Void
            yield return Textbox.Say("CH20_ASRIEL_ZERO_VOID_GUIDANCE");
            
            // Spawn lost souls within Asriel's heart
            SpawnLostSouls();
            
            // Transition to lost souls phase
            CurrentPhase = BossPhase.LostSouls;
        }

        private void SpawnVoidEnergyEffects()
        {
            if (level == null) return;
            
            // Create void particles around the player
            for (int i = 0; i < 50; i++)
            {
                Vector2 pos = (player?.Position ?? Position) + Calc.Random.Range(new Vector2(-100, -100), new Vector2(100, 100));
                level.ParticlesFG.Emit(FlyFeather.P_Boost, pos);
            }
        }
        #endregion

        #region Phase 4: Lost Souls - Save souls to remind them who they were
        private IEnumerator LostSoulsPhase()
        {
            // Explain the mechanic
            yield return Textbox.Say("CH20_ASRIEL_ZERO_LOST_SOULS_INTRO");
            
            // Process each lost soul salvation
            while (SoulsRescued < TOTAL_LOST_SOULS && CurrentPhase == BossPhase.LostSouls)
            {
                // Check if player has interacted with any souls
                CheckSoulSalvation();
                
                // Asriel attacks between soul saves (but weaker each time)
                float desperationLevel = 1f - ((float)SoulsRescued / TOTAL_LOST_SOULS);
                yield return ExecuteDesperateAttack(desperationLevel);
                
                yield return 0.5f;
            }
            
            // All souls saved - transition to flashback
            if (SoulsRescued >= TOTAL_LOST_SOULS)
            {
                CurrentPhase = BossPhase.FlashbackTrigger;
            }
        }

        private void SpawnLostSouls()
        {
            if (level == null) return;
            
            // Spawn lost soul entities around the arena
            string[] soulNames = { "MAGOLOR", "CHARA", "THEO", "OSHIRO", "TORIEL", "ASGORE", 
                                   "ALPHYS", "PAPYRUS", "SANS", "UNDYNE", "RALSEI", "STARSEI" };
            
            float angleStep = MathHelper.TwoPi / soulNames.Length;
            float radius = 150f;
            
            for (int i = 0; i < soulNames.Length; i++)
            {
                float angle = i * angleStep;
                Vector2 soulPos = Position + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );
                
                var soul = new LostSoulEntity(soulPos, soulNames[i], this);
                level.Add(soul);
                activeSouls.Add(soul);
            }
        }

        private void CheckSoulSalvation()
        {
            foreach (var soul in activeSouls)
            {
                if (soul.IsSaved && !soulsSaved[soul.SoulName])
                {
                    soulsSaved[soul.SoulName] = true;
                    SoulsRescued++;
                    OnSoulSaved(soul.SoulName);
                }
            }
        }

        /// <summary>
        /// Called when a soul is saved - triggers appropriate dialog
        /// </summary>
        public void OnSoulSaved(string soulName)
        {
            // Trigger soul-specific dialog
            string dialogKey = $"CH20_ASRIEL_ZERO_SOUL_{soulName}";
            Add(new Coroutine(PlaySoulSavedDialog(dialogKey, soulName)));
        }

        private IEnumerator PlaySoulSavedDialog(string dialogKey, string soulName)
        {
            yield return Textbox.Say(dialogKey);
            
            // Visual effect
            if (level != null)
            {
                for (int i = 0; i < 20; i++)
                {
                    level.ParticlesFG.Emit(HeartGem.P_BlueShine, Position + Calc.Random.Range(new Vector2(-50, -50), new Vector2(50, 50)));
                }
            }
        }

        private IEnumerator ExecuteDesperateAttack(float desperationLevel)
        {
            // Weaker attacks as more souls are saved
            if (desperationLevel > 0.7f)
            {
                yield return UltimaBulletAttack();
            }
            else if (desperationLevel > 0.4f)
            {
                yield return CrossShockerAttack();
            }
            else
            {
                // Very weak, almost symbolic attacks
                yield return WeakCosmicBurst();
            }
        }
        #endregion

        #region Phase 5: Flashback Trigger - Call out "Azzy"
        private IEnumerator FlashbackTriggerPhase()
        {
            // Switch to emotional His Theme version
            Audio.SetMusic(MUSIC_HIS_THEME_02);
            
            // Player realizes they can call out to Asriel directly
            yield return Textbox.Say("CH20_ASRIEL_ZERO_CALL_AZZY");
            
            // Asriel reacts - Els loses control momentarily
            yield return Textbox.Say("CH20_ASRIEL_REMEMBER_A");
            
            // Visual glitch effect - Asriel fighting back
            yield return FlashbackVisualEffect();
            
            yield return Textbox.Say("CH20_ASRIEL_REMEMBER_B");
            
            // Transition to memory recovery
            CurrentPhase = BossPhase.MemoryRecovery;
        }

        private IEnumerator FlashbackVisualEffect()
        {
            // Screen distortion, color shifts
            if (level != null)
            {
                // Flash between boss sprite and crying sprite
                for (int i = 0; i < 10; i++)
                {
                    if (crySprite != null)
                    {
                        crySprite.Visible = (i % 2 == 0);
                    }
                    if (Sprite != null)
                    {
                        Sprite.Color = (i % 2 == 0) ? Color.White * 0.5f : Color.White;
                    }
                    level.DirectionalShake(Calc.Random.Range(Vector2.One * -0.3f, Vector2.One * 0.3f), 0.2f);
                    yield return 0.15f;
                }
                
                // Restore normal visuals
                if (crySprite != null) crySprite.Visible = false;
                if (Sprite != null) Sprite.Color = Color.White;
            }
        }
        #endregion

        #region Phase 6: Memory Recovery - Asriel remembers
        private IEnumerator MemoryRecoveryPhase()
        {
            // Asriel's memories flood back
            yield return Textbox.Say("CH20_ASRIEL_REMEMBER_C");
            
            yield return 0.5f;
            
            yield return Textbox.Say("CH20_ASRIEL_REMEMBER_D");
            
            yield return 0.5f;
            
            yield return Textbox.Say("CH20_ASRIEL_REMEMBER_E");
            
            // Els fights to maintain control
            yield return Textbox.Say("CH20_ASRIEL_ZERO_ELS_CONTROL");
            
            // Asriel won't hold much longer
            yield return Textbox.Say("CH20_ASRIEL_ZERO_LOSING_CONTROL");
            
            // Transition to final beam
            CurrentPhase = BossPhase.FinalBeam;
        }
        #endregion

        #region Phase 7: Final Beam - Els still possessing Asriel
        private IEnumerator FinalBeamPhase()
        {
            // Switch to intense battle music
            Audio.SetMusic(MUSIC_KIRBY_VS_ASRIEL);
            
            // Els forces one final attack
            yield return Textbox.Say("CH20_ASRIEL_REMEMBER_FINAL");
            
            // Charge final beam
            yield return FinalBeamChargeSequence();
            
            // Execute devastating attack
            yield return FinalBeamAttack();
            
            // After the beam, Asriel breaks free
            yield return Textbox.Say("CH20_ASRIEL_REMEMBER_F");
            
            // Transition to redemption
            CurrentPhase = BossPhase.Redemption;
        }

        private IEnumerator FinalBeamChargeSequence()
        {
            // Play charging animation
            if (Sprite != null)
            {
                Sprite.Play("attack_finalbeam_charge");
            }
            
            // Screen darkens, energy gathers
            float chargeTime = 3f;
            float timer = 0f;
            
            while (timer < chargeTime)
            {
                timer += Engine.DeltaTime;
                
                // Increasing screen shake
                float intensity = timer / chargeTime;
                level?.DirectionalShake(Vector2.One * intensity * 0.5f, 0.1f);
                
                // Energy particles converging
                if (level != null && timer % 0.1f < Engine.DeltaTime)
                {
                    Vector2 particlePos = Position + Calc.Random.Range(new Vector2(-200, -200), new Vector2(200, 200));
                    level.ParticlesFG.Emit(FlyFeather.P_Boost, particlePos);
                }
                
                yield return null;
            }
        }

        private IEnumerator FinalBeamAttack()
        {
            // Play beam animation
            if (Sprite != null)
            {
                Sprite.Play("attack_finalbeam_fire");
            }
            
            // Massive screen shake
            level?.DirectionalShake(Vector2.One, 2f);
            
            // Audio
            Audio.Play("event:/desolozantas/sfx/boss/asriel_final_beam", Position);
            
            // Create beam hitbox (player should dodge this)
            // In actual implementation, this would spawn a beam entity
            
            yield return 3f; // Beam duration
            
            // Beam ends
            if (Sprite != null)
            {
                Sprite.Play("attack_finalbeam_end");
            }
            
            yield return 1f;
        }
        #endregion

        #region Phase 8: Redemption - Asriel breaks free
        private IEnumerator RedemptionPhase()
        {
            // Asriel breaks free from Els
            yield return Textbox.Say("CH20_ASRIEL_BOSS_END");
            
            // Remove barrier
            if (activeBarrier != null)
            {
                activeBarrier.Dissolve();
                barrierActive = false;
            }
            
            // Become non-hostile
            Collidable = false;
            IsVulnerable = false;
            
            // Play defeat animation
            if (Sprite != null)
            {
                Sprite.Play("defeat");
            }
            
            // Transition to Els reveal
            yield return Textbox.Say("CH20_DOPPIA_ELICA_BOSS_START");
            
            // Boss fight ends here - Els Doppia Elica takes over
            CurrentPhase = BossPhase.Defeated;
        }
        #endregion

        #region Attack Implementations
        private IEnumerator UltimaBulletAttack()
        {
            if (Sprite != null)
            {
                Sprite.Play("attack_ultimabullet_start");
            }
            
            yield return 0.5f;
            
            // Fire projectiles at player
            if (player != null && level != null)
            {
                Vector2 targetPos = player.Position;
                for (int i = 0; i < 8; i++)
                {
                    float angle = (i / 8f) * MathHelper.TwoPi;
                    Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    
                    // Spawn projectile (would be actual entity in full implementation)
                    Audio.Play("event:/desolozantas/sfx/boss/bullet_fire", Position);
                    yield return 0.1f;
                }
            }
            
            if (Sprite != null)
            {
                Sprite.Play("idle");
            }
            
            yield return 1f;
        }

        private IEnumerator CrossShockerAttack()
        {
            if (Sprite != null)
            {
                Sprite.Play("attack_crossshocker_start");
            }
            
            yield return 0.3f;
            
            // Create cross-shaped lightning pattern
            level?.DirectionalShake(Vector2.UnitY, 0.5f);
            Audio.Play("event:/desolozantas/sfx/boss/lightning", Position);
            
            yield return 1f;
            
            if (Sprite != null)
            {
                Sprite.Play("attack_crossshocker_end");
            }
            
            yield return 0.5f;
        }

        private IEnumerator WeakCosmicBurst()
        {
            // Very weak attack when nearly defeated
            level?.DirectionalShake(Vector2.One * 0.1f, 0.2f);
            
            // Small particle burst
            if (level != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    level.ParticlesFG.Emit(ParticleTypes.Dust, Position + Calc.Random.Range(new Vector2(-30, -30), new Vector2(30, 30)));
                }
            }
            
            yield return 0.5f;
        }
        #endregion

        #region Player Collision
        private void OnPlayerCollision(global::Celeste.Player player)
        {
            if (!IsVulnerable && CurrentPhase != BossPhase.Redemption && CurrentPhase != BossPhase.Defeated)
            {
                // Player takes damage from contact
                player.Die(Vector2.Zero);
            }
        }
        #endregion

        #region Update - GML Converted Step Code
        public override void Update()
        {
            base.Update();
            
            // Update level reference
            if (level == null)
            {
                level = Scene as global::Celeste.Level;
            }
            
            // Update player reference
            if (player == null && level != null)
            {
                player = level.Tracker.GetEntity<global::Celeste.Player>();
            }
            
            // Update barrier position to follow arena
            if (activeBarrier != null && barrierActive)
            {
                activeBarrier.UpdatePosition(Position - new Vector2(200, 150));
            }
            
            // Update sprite layers to follow boss position
            UpdateSpriteLayers();
            
            // GML Step Code - Ultimate Attack Controller (ucon)
            UpdateUconState();
            
            // GML Step Code - Beam Attack Controller (bcon)
            UpdateBconState();
            
            // GML Step Code - Song Controller
            UpdateSongController();
            
            // GML Step Code - Save Controller
            UpdateSaveController();
            
            // GML Step Code - End/Go Controllers
            UpdateEndGoControllers();
        }

        private void UpdateUconState()
        {
            if (ucon > 0)
            {
                if (ucon == 1)
                {
                    // GML: caster_play(psfx, 0.7, 1.2); arf = 30; ucon = 2;
                    Audio.Play("event:/desolozantas/sfx/boss/asriel_ultimate_charge");
                    arf = 30f;
                    ucon = 2;
                }
                
                if (ucon == 2)
                {
                    // GML: armrot += arf; arf -= 2;
                    armrot += arf * Engine.DeltaTime * 60f;
                    arf -= 2f * Engine.DeltaTime * 60f;
                    
                    if (arf <= 0f)
                    {
                        ucon = 3;
                        Add(Alarm.Set(this, 5f / 60f, () => { ucon = 4; }, Alarm.AlarmMode.Oneshot)); // alarm[10] = 5
                    }
                }
                
                if (ucon == 4)
                {
                    // GML: gen = instance_create(x, y, obj_ultimagen); gen.type = u_gen;
                    // Create ultimate bullet generator
                    SpawnUltimaGenerator();
                    
                    ucon = 5;
                    float alarmTime = (gen_type == 2) ? 130f : 140f;
                    Add(Alarm.Set(this, alarmTime / 60f, () => { ucon = 6; }, Alarm.AlarmMode.Oneshot));
                    arf = -30f;
                }
                
                if (ucon == 6)
                {
                    // GML: with (gen) instance_destroy();
                    DestroyUltimaGenerator();
                    
                    armrot += arf * Engine.DeltaTime * 60f;
                    arf += 2f * Engine.DeltaTime * 60f;
                    
                    if (arf >= 0f)
                    {
                        ucon = 0;
                        attacked = true;
                    }
                }
            }
        }

        private void UpdateBconState()
        {
            if (bcon > 0)
            {
                if (bcon == 1)
                {
                    // Initialize beam charge
                    arf = 30f;
                    bcon = 2;
                    Add(Alarm.Set(this, 7f / 60f, () => { 
                        ar_shake += 0.2f;
                        if (radi < 60f) radi += 1.5f;
                        r_siner += 1f;
                    }, Alarm.AlarmMode.Oneshot));
                    r_break = false;
                    r_al = 1f;
                    radi = 0f;
                    r_siner = 0f;
                }
                
                if (bcon == 2)
                {
                    // GML: armrot -= arf; arf -= 5;
                    armrot -= arf * Engine.DeltaTime * 60f;
                    arf -= 5f * Engine.DeltaTime * 60f;
                    
                    if (arf <= 0f)
                    {
                        bcon = 3;
                        Add(Alarm.Set(this, 35f / 60f, () => { bcon = 4; }, Alarm.AlarmMode.Oneshot));
                    }
                }
                
                if (bcon == 4)
                {
                    bcon = 41; // 4.1 equivalent
                    Add(Alarm.Set(this, 2f / 60f, () => { bcon = 51; }, Alarm.AlarmMode.Oneshot));
                }
                
                if (bcon == 41)
                {
                    // GML: armrot -= 5;
                    armrot -= 5f * Engine.DeltaTime * 60f;
                }
                
                if (bcon == 51)
                {
                    bcon = 5;
                    Add(Alarm.Set(this, 5f / 60f, () => { bcon = 6; }, Alarm.AlarmMode.Oneshot));
                }
                
                if (bcon == 5)
                {
                    ar_shake = 0f;
                    armrot += 26f * Engine.DeltaTime * 60f;
                }
                
                if (bcon == 6)
                {
                    // Fire the beam
                    cry = 2;
                    ar_shake = 5f;
                    float angleRad = (-armrot - 90f) * MathHelper.Pi / 180f;
                    armx = (float)Math.Cos(angleRad) * 150f;
                    army = (float)Math.Sin(angleRad) * 150f;
                    SpawnFinalBeam();
                    bcon = 7;
                    Add(Alarm.Set(this, 400f / 60f, () => { bcon = 8; }, Alarm.AlarmMode.Oneshot));
                }
                
                if (bcon == 8)
                {
                    cry = 0;
                    if (ar_shake > 0f) ar_shake -= 1f * Engine.DeltaTime * 60f;
                    if (armrot > 0f) armrot -= 2f * Engine.DeltaTime * 60f;
                    else armrot = 0f;
                    
                    if (ar_shake <= 0f)
                    {
                        ar_shake = 0f;
                        bcon = 0;
                        attacked = true;
                    }
                }
                
                // Beam charging effect updates
                if (bcon < 7 && r_al > 0f && !r_break)
                {
                    ar_shake += 0.2f * Engine.DeltaTime * 60f;
                    if (radi < 60f) radi += 1.5f * Engine.DeltaTime * 60f;
                    r_siner += 1f * Engine.DeltaTime * 60f;
                    
                    // Calculate arm position for beam
                    float angleRad = (-armrot - 90f) * MathHelper.Pi / 180f;
                    armx = (float)Math.Cos(angleRad) * 150f;
                    army = (float)Math.Sin(angleRad) * 150f;
                }
            }
        }

        private void UpdateSongController()
        {
            if (songcon == 1)
            {
                // Fade out music
                vol -= 0.04f * Engine.DeltaTime * 60f;
                if (batmusic != null)
                {
                    batmusic.setVolume(Math.Max(vol, 0f));
                }
                
                if (vol <= 0.04f)
                {
                    vol = 0f;
                    batmusic?.stop(STOP_MODE.ALLOWFADEOUT);
                    songcon = 2;
                }
            }
        }

        private void UpdateSaveController()
        {
            if (savecon_a > 0)
            {
                vol -= 0.04f * Engine.DeltaTime * 60f;
                if (batmusic != null)
                {
                    batmusic.setVolume(Math.Max(vol, 0f));
                }
                
                if (savecon_a == 1 && !IsTextboxActive())
                {
                    savecon_a = 2;
                    savecon_a_x = 0;
                }
                
                if (savecon_a == 2)
                {
                    savecon_a_x++;
                    
                    if (savecon_a_x == 70)
                    {
                        // Screen white flash
                        level?.Flash(Color.White);
                    }
                    
                    if (savecon_a_x == 138)
                    {
                        // Transition to memory room
                        // (Would trigger room transition here)
                    }
                }
            }
        }

        private void UpdateEndGoControllers()
        {
            if (endcon == 1 && !IsTextboxActive())
            {
                level?.Flash(Color.White);
                endcon = 3;
                Add(Alarm.Set(this, 136f / 60f, () => { 
                    // End sequence complete
                }, Alarm.AlarmMode.Oneshot));
            }
            
            if (gocon == 1 && !IsTextboxActive())
            {
                if (gotimer == 0)
                {
                    level?.Flash(Color.White);
                }
                gotimer++;
                
                if (gotimer == 34)
                {
                    // Room restart with new battle
                    gocon = 0;
                }
            }
        }

        private bool IsTextboxActive()
        {
            return level != null && level.Entities.FindFirst<Textbox>() != null;
        }

        private void SpawnUltimaGenerator()
        {
            // Spawn ultimate bullet generator entity
            // This would create the attack pattern based on gen_type
        }

        private void DestroyUltimaGenerator()
        {
            // Destroy the ultimate generator
        }

        private void SpawnFinalBeam()
        {
            // Spawn the final beam attack entity
            if (level != null)
            {
                // Create beam at arm position
                Vector2 beamPos = Position + new Vector2(56 + armx, 56 + army) - new Vector2(0, 20);
                // level.Add(new AsrielFinalBeam(beamPos));
                Audio.Play("event:/desolozantas/sfx/boss/asriel_final_beam", beamPos);
            }
        }

        /// <summary>
        /// Final attack during Asriel remember scene - GML converted from obj_asrielfinal
        /// Called when Asriel hits the player during the emotional climax
        /// </summary>
        public void FinalAttackDuringRememberScene()
        {
            // GML: snd_play(snd_hurt1);
            Audio.Play("event:/game/10/pit_hurt");
            
            // GML: if (instance_exists(obj_shaker) == 0) instance_create(0, 0, obj_shaker);
            // Shake the screen
            level?.Shake();
            
            // Apply damage based on hit count - for Celeste this would be pseudo-health system
            // or visual feedback since Celeste uses one-hit deaths
            // GML: if (hits == 0) global.hp = 1; etc.
            switch (hits)
            {
                case 0:
                case 1:
                    // Player at critical health (1 HP)
                    ApplyFakeHealth(1.0f);
                    break;
                case 2:
                    ApplyFakeHealth(0.9f);
                    break;
                case 3:
                    ApplyFakeHealth(0.5f);
                    break;
                case 4:
                    ApplyFakeHealth(0.1f);
                    break;
                case 5:
                    ApplyFakeHealth(0.01f);
                    break;
                case 6:
                    // GML: global.flag[509] = 1; - progression flags
                    SetRememberFlag(1);
                    break;
                case 7:
                    SetRememberFlag(2);
                    break;
                case 8:
                    SetRememberFlag(3);
                    break;
                case 9:
                    SetRememberFlag(4);
                    break;
            }
            
            hits++;
            
            // GML: alarm[5] = 40;
            Add(Alarm.Set(this, 40f / 60f, () => {
                // Next attack ready
            }, Alarm.AlarmMode.Oneshot));
        }

        private void ApplyFakeHealth(float healthValue)
        {
            // In Celeste, this would be a visual representation of the boss fight "health"
            // rather than actual player health since Celeste is one-hit-kill
            // This tracks Asriel's memory recovery progress
        }

        private void SetRememberFlag(int value)
        {
            // Store progression of Asriel's memory recovery
            // This could trigger different dialog or visual states
            if (level?.Session != null)
            {
                level.Session.SetCounter("asriel_remember_flag", value);
            }
        }

        private void UpdateSpriteLayers()
        {
            // Position all sprite layers relative to boss
            Vector2 offset = Vector2.Zero;
            
            if (bgSprite != null) bgSprite.Position = offset;
            if (cosmowingSprite != null) cosmowingSprite.Position = new Vector2(0, -140) + offset;
            if (orbwingSprite != null) orbwingSprite.Position = new Vector2(0, -140) + offset;
            if (shoulderSprite != null) shoulderSprite.Position = offset;
            if (stemSprite != null) stemSprite.Position = offset;
            if (orbSprite != null) orbSprite.Position = new Vector2(0, -60) + offset;
            if (faceSprite != null) faceSprite.Position = new Vector2(0, -60) + offset;
            if (crySprite != null) crySprite.Position = new Vector2(0, -60) + offset;
        }
        #endregion

        #region Render - GML Converted Draw Code
        public override void Render()
        {
            // Update animation counters (equivalent to GML: anim += 1; siner += 1; side += 2;)
            anim += Engine.DeltaTime * 60f; // Scale to ~60fps GML speed
            siner += Engine.DeltaTime * 60f;
            side += Engine.DeltaTime * 120f; // side += 2 per frame at 60fps

            // Wrap side at 800 (GML: if (side > 800) side -= 800;)
            if (side > 800f)
                side -= 800f;

            // Calculate vertical offsets (GML: yoff = sin(siner / 4); yoff2 = sin(siner / 16);)
            yoff = (float)Math.Sin(siner / 4f) - 60f;
            yoff2 = (float)Math.Sin(siner / 16f) - 60f;

            // Draw black background overlay (GML: draw_set_color(c_black); ossafe_fill_rectangle(-10, 240, 999, -10);)
            Draw.Rect(Position.X - 500, Position.Y - 500, 2000, 2000, Color.Black);

            // Calculate HSV color for background (GML: thiscolor = make_color_hsv(siner * 6, 200, 200);)
            Color thiscolor = ColorFromHSV((siner * 6f) % 360f, 200f / 255f, 200f / 255f);

            // Draw background sprite parts with extended parameters
            // GML: draw_sprite_part_ext(spr_asrielbg, 0, side, 0, 276, 216, 640, 0, -1, 1, thiscolor, 0.5);
            DrawBackgroundSpritePart(side, 640, 0, -1, 1, thiscolor, 0.5f);
            DrawBackgroundSpritePart(side + 60, 640, 0, -1, 1, thiscolor, 0.5f);
            DrawBackgroundSpritePart(side + 120, 640, 0, -1, 1, thiscolor, 0.5f);

            // Draw at x=0 (GML: draw_sprite_part_ext(..., 0, 0, 1, 1, ...))
            DrawBackgroundSpritePart(side, 0, 0, 1, 1, thiscolor, 0.5f);
            DrawBackgroundSpritePart(side + 60, 0, 0, 1, 1, thiscolor, 0.5f);
            DrawBackgroundSpritePart(side + 120, 0, 0, 1, 1, thiscolor, 0.5f);

            // Draw wings (GML: draw_sprite_ext(spr_afinal_cosmoswing, floor(anim / 6), x + 42, (y - 52) + (yoff2 * 4), 2, 2, 0, image_blend, image_alpha);)
            // Raised by 60 pixels for visibility
            string wingAnim = "wing" + ((int)(anim / 6f) % 4);
            Vector2 wingOffset = new Vector2(0, yoff2 * 4f - 60f);
            
            if (cosmowingSprite != null)
            {
                string anim = cosmowingSprite.Has(wingAnim) ? wingAnim : (cosmowingSprite.Has("idle") ? "idle" : null);
                // Right wing
                cosmowingSprite.Position = new Vector2(42, -112) + wingOffset;
                cosmowingSprite.Scale = new Vector2(-2, 2); // -1 xscale = flipped
                if (anim != null) cosmowingSprite.Play(anim);
                cosmowingSprite.Render();
                
                // Left wing
                cosmowingSprite.Position = new Vector2(-44, -112) + wingOffset;
                cosmowingSprite.Scale = new Vector2(2, 2);
                if (anim != null) cosmowingSprite.Play(anim);
                cosmowingSprite.Render();
            }

            // Draw orb wings (GML: draw_sprite_ext(spr_afinal_orbwing, floor(anim / 6), x - 110, y - 52, 2, 2, 0, image_blend, image_alpha);)
            // Raised by 60 pixels for visibility
            if (orbwingSprite != null)
            {
                string anim = orbwingSprite.Has(wingAnim) ? wingAnim : (orbwingSprite.Has("idle") ? "idle" : null);
                orbwingSprite.Position = new Vector2(-110, -112);
                orbwingSprite.Scale = new Vector2(2, 2);
                if (anim != null) orbwingSprite.Play(anim);
                orbwingSprite.Render();
                
                orbwingSprite.Position = new Vector2(108, -112);
                orbwingSprite.Scale = new Vector2(-2, 2);
                if (anim != null) orbwingSprite.Play(anim);
                orbwingSprite.Render();
            }

            // Draw stem (GML: draw_sprite_ext(spr_afinal_stem, floor(anim / 6), x - 2, y + 146, 2, 2, 0, image_blend, image_alpha);)
            // Raised by 60 pixels for visibility
            if (stemSprite != null)
            {
                stemSprite.Position = new Vector2(-2, 86);
                stemSprite.Scale = new Vector2(2, 2);
                if (stemSprite.Has(wingAnim))
                    stemSprite.Play(wingAnim);
                else if (stemSprite.Has("idle"))
                    stemSprite.Play("idle");
                stemSprite.Render();
            }

            // Draw orb (GML: draw_sprite_ext(spr_afinal_orb, floor(anim / 6), x - 2, y + 68, 2, 2, 0, image_blend, image_alpha);)
            // Raised by 60 pixels for visibility
            if (orbSprite != null)
            {
                orbSprite.Position = new Vector2(-2, 8);
                orbSprite.Scale = new Vector2(2, 2);
                if (orbSprite.Has(wingAnim))
                    orbSprite.Play(wingAnim);
                else if (orbSprite.Has("idle"))
                    orbSprite.Play("idle");
                orbSprite.Render();
            }

            // Calculate shake (GML: rx = random(ar_shake) - random(ar_shake); ry = random(ar_shake) - random(ar_shake);)
            float rx = Calc.Random.NextFloat(ar_shake) - Calc.Random.NextFloat(ar_shake);
            float ry = Calc.Random.NextFloat(ar_shake) - Calc.Random.NextFloat(ar_shake);
            ry *= 1.5f;
            rx *= 0.7f;

            // Draw body fade overlay (GML: draw_set_alpha(bodyfader); draw_set_color(c_black); ...)
            if (bodyfader > 0f)
            {
                Draw.Rect(Position.X - 500, Position.Y - 500, 2000, 2000, Color.Black * bodyfader);
            }

            // Draw face based on cry state
            DrawFace(0, rx, ry);

            // Draw arms (GML: draw_sprite_ext(spr_afinal_arm, floor(anim / 6), (x - 58) + rx, y + 56 + (yoff * 2) + ry, 2, 2, armrot, image_blend, image_alpha - bodyfader);)
            // Arms raised by 60 pixels for visibility
            if (shoulderSprite != null)
            {
                string anim = shoulderSprite.Has(wingAnim) ? wingAnim : (shoulderSprite.Has("idle") ? "idle" : null);
                // Left arm
                shoulderSprite.Position = new Vector2(-58 + rx, -4 + (yoff * 2) + ry);
                shoulderSprite.Scale = new Vector2(2, 2);
                shoulderSprite.Rotation = armrot * MathHelper.Pi / 180f; // Convert to radians
                shoulderSprite.Color = Color.White * (1f - bodyfader);
                if (anim != null) shoulderSprite.Play(anim);
                shoulderSprite.Render();
                
                // Right arm (GML: x + 56 + rx, ... -armrot)
                shoulderSprite.Position = new Vector2(56 + rx, -4 + (yoff * 2) + ry);
                shoulderSprite.Scale = new Vector2(-2, 2);
                shoulderSprite.Rotation = -armrot * MathHelper.Pi / 180f;
                shoulderSprite.Color = Color.White * (1f - bodyfader);
                if (anim != null) shoulderSprite.Play(anim);
                shoulderSprite.Render();
            }

            // Draw shoulders (GML: draw_sprite_ext(spr_afinal_shoulder, ...))
            // Shoulders raised by 60 pixels for visibility
            if (Sprite != null)
            {
                string anim = Sprite.Has(wingAnim) ? wingAnim : (Sprite.Has("idle") ? "idle" : null);
                // Left shoulder
                Sprite.Position = new Vector2(-84, -28);
                Sprite.Scale = new Vector2(2, 2);
                if (anim != null) Sprite.Play(anim);
                Sprite.Color = Color.White * (1f - bodyfader);
                Sprite.Render();
                
                // Right shoulder
                Sprite.Position = new Vector2(82, -28);
                Sprite.Scale = new Vector2(-2, 2);
                if (anim != null) Sprite.Play(anim);
                Sprite.Color = Color.White * (1f - bodyfader);
                Sprite.Render();
            }

            // Render beam charge effects (from bcon logic in GML draw)
            RenderBeamCharge(rx, ry);

            // Darker screen effect (GML: if (darker == 1) ...)
            if (darker)
            {
                if (darker_x < 1f)
                    darker_x += Engine.DeltaTime * 2.4f; // 0.04 * 60fps

                Draw.Rect(Position.X - 500, Position.Y - 500, 2000, 2000, Color.Black * Math.Min(darker_x, 1f));
            }

            // Render existing sprite layers that weren't handled above
            faceSprite?.Render();
            crySprite?.Render();
        }

        private void DrawBackgroundSpritePart(float sideOffset, float x, float y, float xscale, float yscale, Color color, float alpha)
        {
            if (bgSprite == null) return;
            
            // Calculate source rectangle for sprite part
            float srcX = sideOffset % 276f;
            
            bgSprite.Position = new Vector2(x, y);
            bgSprite.Scale = new Vector2(xscale, yscale);
            bgSprite.Color = color * alpha;
            bgSprite.Render();
        }

        private void DrawFace(int animFrame, float rx, float ry)
        {
            // Face raised by 60 pixels for visibility
            // GML: if (cry == 0) draw_sprite_ext(spr_afinal_face, global.faceemotion, x, y, 2, 2, 0, image_blend, image_alpha);
            if (cry == 0 && faceSprite != null)
            {
                faceSprite.Position = new Vector2(0, -60);
                faceSprite.Scale = new Vector2(2, 2);
                faceSprite.Play("face0"); // global.faceemotion would be mapped here
                faceSprite.Color = Color.White;
                faceSprite.Render();
            }
            // GML: if (cry == 1) draw_sprite_ext(spr_afinal_face_cry, floor(siner / 8), x + (rx / 3), y + (ry / 3), 2, 2, 0, image_blend, image_alpha);
            else if (cry == 1 && crySprite != null)
            {
                string cryAnim = "cry" + ((int)(siner / 8f) % 4);
                crySprite.Position = new Vector2(rx / 3f, ry / 3f - 60f);
                crySprite.Scale = new Vector2(2, 2);
                crySprite.Play(cryAnim);
                crySprite.Color = Color.White;
                crySprite.Render();
            }
            // GML: if (cry == 2) draw_sprite_ext(spr_afinal_face_cry2, floor(siner / 2), x + (rx / 3), y + (ry / 3), 2, 2, 0, image_blend, image_alpha);
            else if (cry == 2 && crySprite != null)
            {
                string cryAnim = "cry2_" + ((int)(siner / 2f) % 4);
                crySprite.Position = new Vector2(rx / 3f, ry / 3f - 60f);
                crySprite.Scale = new Vector2(2, 2);
                crySprite.Play(cryAnim);
                crySprite.Color = Color.White;
                crySprite.Render();
            }
        }

        private void RenderBeamCharge(float rx, float ry)
        {
            // GML beam charging effect from bcon < 7 && r_al > 0
            if (bcon < 7 && bcon > 0 && r_al > 0f)
            {
                // Calculate arm vector (GML: armx = lengthdir_x(150, -armrot - 90); army = lengthdir_y(150, -armrot - 90);)
                float angleRad = (-armrot - 90f) * MathHelper.Pi / 180f;
                armx = (float)Math.Cos(angleRad) * 150f;
                army = (float)Math.Sin(angleRad) * 150f;

                // Draw charging circles
                Draw.Circle(Position + new Vector2(56 + armx, 56 + army), radi, Color.White * r_al, 32);
                Draw.Circle(Position + new Vector2(-58 - armx, 56 + army), radi, Color.White * r_al, 32);
            }
        }

        private Color ColorFromHSV(float hue, float saturation, float value)
        {
            // Convert HSV to RGB
            int hi = (int)(hue / 60f) % 6;
            float f = (hue / 60f) - hi;

            float v = value;
            float p = value * (1f - saturation);
            float q = value * (1f - f * saturation);
            float t = value * (1f - (1f - f) * saturation);

            return hi switch
            {
                0 => new Color(v, t, p),
                1 => new Color(q, v, p),
                2 => new Color(p, v, t),
                3 => new Color(p, q, v),
                4 => new Color(t, p, v),
                _ => new Color(v, p, q)
            };
        }
        #endregion
    }

    #region Supporting Entities
    /// <summary>
    /// Undertale-style barrier that traps the player in the boss arena
    /// </summary>
    [Tracked]
    public class UndertaleBarrier : Entity
    {
        private float width;
        private float height;
        private Color barrierColor;
        private float alpha;
#pragma warning disable CS0414
        private bool dissolving;
#pragma warning restore CS0414

        public UndertaleBarrier(Vector2 position, float width, float height, Color color)
            : base(position)
        {
            this.width = width;
            this.height = height;
            this.barrierColor = color;
            this.alpha = 1f;
            this.dissolving = false;

            // Create collision barriers on all sides
            Collider = new ColliderList(
                new Hitbox(width, 8, 0, 0),           // Top
                new Hitbox(width, 8, 0, height - 8),  // Bottom
                new Hitbox(8, height, 0, 0),          // Left
                new Hitbox(8, height, width - 8, 0)   // Right
            );

            Collidable = true;
            Depth = -100000;
        }

        public void UpdatePosition(Vector2 newPosition)
        {
            Position = newPosition;
        }

        public void Dissolve()
        {
            dissolving = true;
            Add(new Coroutine(DissolveRoutine()));
        }

        private IEnumerator DissolveRoutine()
        {
            float duration = 2f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Engine.DeltaTime;
                alpha = 1f - Ease.CubeIn(timer / duration);
                yield return null;
            }

            RemoveSelf();
        }

        public override void Render()
        {
            base.Render();

            Color drawColor = barrierColor * alpha;

            // Draw glowing barrier borders
            Draw.Rect(Position.X, Position.Y, width, 4, drawColor);           // Top
            Draw.Rect(Position.X, Position.Y + height - 4, width, 4, drawColor); // Bottom
            Draw.Rect(Position.X, Position.Y, 4, height, drawColor);           // Left
            Draw.Rect(Position.X + width - 4, Position.Y, 4, height, drawColor); // Right
        }
    }

    /// <summary>
    /// Lost Soul entity that can be saved by the player
    /// </summary>
    [Tracked]
    public class LostSoulEntity : Entity
    {
        public string SoulName { get; private set; }
        public bool IsSaved { get; private set; }
        
        private AsrielAngelOfDeathBoss parentBoss;
        private float floatOffset;
        private float floatSpeed;
        private Color soulColor;
        private bool interacting;

        public LostSoulEntity(Vector2 position, string soulName, AsrielAngelOfDeathBoss boss)
            : base(position)
        {
            SoulName = soulName;
            parentBoss = boss;
            IsSaved = false;
            interacting = false;
            floatOffset = Calc.Random.NextFloat() * MathHelper.TwoPi;
            floatSpeed = 1f + Calc.Random.NextFloat() * 0.5f;

            // Set soul color based on character
            soulColor = GetSoulColor(soulName);

            Collider = new Hitbox(24, 24, -12, -12);
            Add(new PlayerCollider(OnPlayerTouch));

            Depth = -10000;
        }

        private Color GetSoulColor(string name)
        {
            return name switch
            {
                "MAGOLOR" => new Color(214, 120, 219),  // Purple-pink
                "CHARA" => new Color(255, 0, 0),        // Red
                "THEO" => new Color(255, 165, 0),       // Orange
                "OSHIRO" => new Color(128, 0, 128),     // Purple
                "TORIEL" => new Color(138, 43, 226),    // Blue-violet
                "ASGORE" => new Color(255, 215, 0),     // Gold
                "ALPHYS" => new Color(255, 255, 0),     // Yellow
                "PAPYRUS" => new Color(255, 140, 0),    // Dark orange
                "SANS" => new Color(0, 191, 255),       // Deep sky blue
                "UNDYNE" => new Color(0, 0, 255),       // Blue
                "RALSEI" => new Color(0, 255, 127),     // Spring green
                "STARSEI" => new Color(255, 255, 255),  // White
                _ => Color.White
            };
        }

        private void OnPlayerTouch(global::Celeste.Player player)
        {
            if (!IsSaved && !interacting)
            {
                interacting = true;
                Add(new Coroutine(SaveSoulSequence()));
            }
        }

        private IEnumerator SaveSoulSequence()
        {
            // Play soul-specific dialog
            yield return Textbox.Say($"CH20_ASRIEL_ZERO_SOUL_{SoulName}_LOST");
            
            // Pause for effect
            yield return 0.5f;
            
            // Play redemption dialog
            yield return Textbox.Say($"CH20_ASRIEL_ZERO_SOUL_{SoulName}_SAVED");
            
            // Mark as saved
            IsSaved = true;
            
            // Visual effect
            Level level = Scene as Level;
            if (level != null)
            {
                for (int i = 0; i < 30; i++)
                {
                    level.ParticlesFG.Emit(HeartGem.P_BlueShine, Position);
                }
            }
            
            Audio.Play("event:/desolozantas/sfx/soul_saved", Position);
            
            // Fade out
            float fadeTime = 1f;
            float timer = 0f;
            while (timer < fadeTime)
            {
                timer += Engine.DeltaTime;
                // Would update alpha here
                yield return null;
            }
            
            // Notify parent boss
            parentBoss?.OnSoulSaved(SoulName);
            
            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();

            if (!IsSaved)
            {
                // Float animation
                floatOffset += Engine.DeltaTime * floatSpeed;
                Position.Y += (float)Math.Sin(floatOffset) * 0.5f;
            }
        }

        public override void Render()
        {
            base.Render();

            if (!IsSaved)
            {
                // Draw glowing soul orb
                Draw.Circle(Position, 12, soulColor, 16);
                Draw.Circle(Position, 8, Color.White * 0.8f, 12);
            }
        }
    }
    #endregion
}
