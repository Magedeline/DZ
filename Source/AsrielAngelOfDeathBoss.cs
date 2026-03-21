using MaggyHelper.Helpers;
using MaggyHelper.Entities;
using HeartGem = Celeste.HeartGem; // Vanilla HeartGem for particles (P_BlueShine)

namespace MaggyHelper
{
    /// <summary>
    /// Asriel Angel of Death Boss - Chapter 20: The End
    /// Multi-phase boss fight with emotional story beats, barrier mechanics,
    /// lost soul salvation, and FMOD audio integration.
    /// Sprite path: characters/asrielangelofdeathboss
    /// </summary>
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
        public AsrielAngelOfDeathBoss(Vector2 position) 
            : base(position, 
                   spriteName: "asriel_angelofdeath",
                   spriteScale: Vector2.One,
                   maxFall: 0f, // Boss hovers
                   collidable: true,
                   solidCollidable: false,
                   gravityMult: 0f,
                   collider: new Hitbox(128, 192, -64, -192))
        {
            Initialize();
        }

        public AsrielAngelOfDeathBoss(EntityData data, Vector2 offset) 
            : base(data.Position + offset, 
                   spriteName: "asriel_angelofdeath",
                   spriteScale: Vector2.One,
                   maxFall: 0f,
                   collidable: true,
                   solidCollidable: false,
                   gravityMult: 0f,
                   collider: new Hitbox(128, 192, -64, -192))
        {
            Health = data.Int("health", 2500);
            MaxHealth = data.Int("maxHealth", 2500);
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
                sprite.AddLoop("idle", "", 0.1f);
                sprite.CenterOrigin();
                sprite.Visible = true;
                Add(sprite);
            }
            catch
            {
                // Silently fail if sprite doesn't exist
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
        #endregion

        #region Phase 1: Rise Sequence - Boss rises behind player and creates barrier
        private IEnumerator DormantPhase()
        {
            // Boss is invisible/dormant until triggered
            Visible = false;
            Collidable = false;
            
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

        #region Update
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
        }

        private void UpdateSpriteLayers()
        {
            // Position all sprite layers relative to boss
            Vector2 offset = Vector2.Zero;
            
            if (bgSprite != null) bgSprite.Position = offset;
            if (cosmowingSprite != null) cosmowingSprite.Position = offset;
            if (shoulderSprite != null) shoulderSprite.Position = offset;
            if (stemSprite != null) stemSprite.Position = offset;
            if (orbSprite != null) orbSprite.Position = offset;
            if (orbwingSprite != null) orbwingSprite.Position = offset;
            if (faceSprite != null) faceSprite.Position = offset;
            if (crySprite != null) crySprite.Position = offset;
        }
        #endregion

        #region Render
        public override void Render()
        {
            // Render in specific order for layering
            bgSprite?.Render();
            cosmowingSprite?.Render();
            stemSprite?.Render();
            shoulderSprite?.Render();
            orbwingSprite?.Render();
            orbSprite?.Render();
            
            base.Render();
            
            faceSprite?.Render();
            crySprite?.Render();
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
