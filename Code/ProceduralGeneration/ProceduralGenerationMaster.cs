using System;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.AI;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.Achievements;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.AudioGeneration;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.BackgroundGeneration;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.BossBehavior;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.BossPhase;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.Dialogue;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.Difficulty;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.EnemyGeneration;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.ParticleSystem;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.PlatformGeneration;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.RoomGeneration;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.SpineAnimation;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration;

/// <summary>
/// Master integration system for all procedural generation components
/// </summary>
public class ProceduralGenerationMaster
{
    private const string LogTag = "ProceduralGenerationMaster";
    
    private static ProceduralGenerationMaster? _instance;
    private static bool _initialized = false;
    
    // Subsystems
    private readonly MarkovChainManager _markovChainManager;
    private readonly SpineAnimationGenerator _spineAnimationGenerator;
    private readonly ProceduralBossBehavior _bossBehaviorSystem;
    private readonly ProceduralEnemyGenerator _enemyGenerator;
    private readonly MarkovRoomGenerator _roomGenerator;
    private readonly DynamicPlatformGenerator _platformGenerator;
    private readonly MarkovParticleSystemGenerator _particleSystemGenerator;
    private readonly DynamicBackgroundGenerator _backgroundGenerator;
    private readonly MarkovAudioSystem _audioSystem;
    private readonly MarkovPlayerPredictionAI _aiSystem;
    private readonly ProceduralBossPhaseSystem _bossPhaseSystem;
    private readonly MarkovDialogueGenerator _dialogueGenerator;
    private readonly ProceduralAchievementSystem _achievementSystem;
    private readonly DynamicDifficultyBalancer _difficultyBalancer;
    
    public static ProceduralGenerationMaster Instance => _instance ?? throw new InvalidOperationException("ProceduralGenerationMaster not initialized");
    
    public bool IsInitialized => _initialized;
    
    private ProceduralGenerationMaster(int seed)
    {
        _markovChainManager = new MarkovChainManager();
        MarkovChainManager.Initialize();
        
        // Initialize all subsystems with consistent seed
        _spineAnimationGenerator = new SpineAnimationGenerator("Default_Skeleton", seed);
        _bossBehaviorSystem = new ProceduralBossBehavior(new BossProfile { Name = "DefaultBoss" }, seed);
        _enemyGenerator = new ProceduralEnemyGenerator(seed);
        _roomGenerator = new MarkovRoomGenerator(seed);
        _platformGenerator = new DynamicPlatformGenerator(seed);
        _particleSystemGenerator = new MarkovParticleSystemGenerator(seed);
        _backgroundGenerator = new DynamicBackgroundGenerator(seed);
        _audioSystem = new MarkovAudioSystem(seed);
        _aiSystem = new MarkovPlayerPredictionAI(2, seed);
        _bossPhaseSystem = new ProceduralBossPhaseSystem(seed);
        _dialogueGenerator = new MarkovDialogueGenerator(seed);
        _achievementSystem = new ProceduralAchievementSystem(seed);
        _difficultyBalancer = new DynamicDifficultyBalancer(DifficultyTier.Normal, seed);
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized all procedural generation subsystems");
    }
    
    /// <summary>
    /// Initialize the procedural generation master system
    /// </summary>
    public static void Initialize(int seed = 0)
    {
        if (_initialized)
        {
            Logger.Log(LogLevel.Warn, LogTag, "Already initialized");
            return;
        }
        
        _instance = new ProceduralGenerationMaster(seed);
        _initialized = true;
        
        Logger.Log(LogLevel.Info, LogTag, "Procedural generation master system initialized");
    }
    
    /// <summary>
    /// Generate a complete procedurally generated level
    /// </summary>
    public GeneratedLevel GenerateCompleteLevel(
        string levelName, 
        int roomCount, 
        DifficultyTier startingDifficulty,
        string bossName = "GeneratedBoss")
    {
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generating complete level: {levelName} with {roomCount} rooms");
        
        // Generate level layout
        var level = _roomGenerator.GenerateLevel(roomCount, levelName, startingDifficulty);
        
        // Generate boss behavior
        var bossProfile = new BossProfile
        {
            Name = bossName,
            SkeletonName = $"{bossName}_Skeleton",
            AvailableBehaviors = new List<string> { "Chase", "Attack", "Defend", "Special" },
            BehaviorToAnimations = new Dictionary<string, List<string>>
            {
                { "Chase", new List<string> { "Chase_Idle", "Chase_Run" } },
                { "Attack", new List<string> { "Attack_Light", "Attack_Heavy" } }
            },
            BehaviorToAttacks = new Dictionary<string, List<string>>
            {
                { "Attack", new List<string> { "Light_Attack", "Heavy_Attack" } },
                { "Special", new List<string> { "Special_Attack" } }
            }
        };
        
        var bossBehavior = new ProceduralBossBehavior(bossProfile, _difficultyBalancer.GetHashCode());
        
        // Generate enemies for each room
        foreach (var room in level.Rooms)
        {
            if (room.Type == "Combat" || room.Type == "Challenge")
            {
                var enemyVariant = _enemyGenerator.GenerateVariant(
                    EnemyArchetype.Grounded, room.Difficulty);
                room.GeneratedEnemies = new List<EnemyVariant> { enemyVariant };
            }
        }
        
        // Generate dynamic platforms
        foreach (var room in level.Rooms)
        {
            if (room.Type == "Platforming")
            {
                var platforms = _platformGenerator.GeneratePlatformSequence(
                    PlatformType.Normal, new Microsoft.Xna.Framework.Vector2(100, 200), 
                    5, new Microsoft.Xna.Framework.Vector2(80, 0), room.Difficulty);
                room.GeneratedPlatforms = platforms;
            }
        }
        
        // Generate backgrounds
        foreach (var room in level.Rooms)
        {
            room.GeneratedBackground = _backgroundGenerator.GenerateBackground(room.Type, room.Difficulty);
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Complete level generated: {levelName} with {level.Rooms.Count} rooms");
        
        return level;
    }
    
    /// <summary>
    /// Update all procedural generation systems
    /// </summary>
    public void Update(float deltaTime, Player player)
    {
        // Update AI system with player data
        if (player != null)
        {
            var playerAction = new PlayerAction
            {
                Type = DeterminePlayerActionType(player),
                Position = player.Position,
                Direction = player.Facing == Facings.Right ? Microsoft.Xna.Framework.Vector2.UnitX : -Microsoft.Xna.Framework.Vector2.UnitX,
                Timestamp = Engine.RawDeltaTime
            };
            
            _aiSystem.RecordPlayerAction(playerAction);
        }
        
        // Update difficulty balancer
        _difficultyBalancer.RecordPerformanceEvent("Time", deltaTime);
    }
    
    private AI.ActionType DeterminePlayerActionType(Player player)
    {
        if (player.DashAttacking) return AI.ActionType.Dash;
        if (player.Speed.Y < 0) return AI.ActionType.Jump;
        if (player.Speed.Length() > 50f) return AI.ActionType.Move;
        return AI.ActionType.Idle;
    }
    
    /// <summary>
    /// Get comprehensive statistics from all systems
    /// </summary>
    public MasterGenerationStats GetMasterStats()
    {
        return new MasterGenerationStats
        {
            MarkovChainStats = new Dictionary<string, string>(),
            RoomGenerationStats = _roomGenerator.GetStats(),
            EnemyGenerationStats = _enemyGenerator.GetStats(),
            PlatformGenerationStats = _platformGenerator.GetStats(),
            ParticleSystemStats = _particleSystemGenerator.GetStats(),
            BackgroundGenerationStats = _backgroundGenerator.GetStats(),
            AudioStats = _audioSystem.GetStats(),
            AIStats = _aiSystem.GetStats(),
            BossPhaseSystemStats = _bossPhaseSystem.GetStats(),
            DialogueGenerationStats = _dialogueGenerator.GetStats(),
            AchievementStats = _achievementSystem.GetStats(),
            DifficultyBalancingStats = _difficultyBalancer.GetStats()
        };
    }
    
    /// <summary>
    /// Cleanup all procedural generation systems
    /// </summary>
    public static void Cleanup()
    {
        if (_instance != null)
        {
            MarkovChainManager.Cleanup();
            _instance = null;
        }
        
        _initialized = false;
        
        Logger.Log(LogLevel.Info, LogTag, "Procedural generation master system cleaned up");
    }
    
    // Accessors for subsystems
    public MarkovChainManager MarkovChainManager => _markovChainManager;
    public SpineAnimationGenerator SpineAnimationGenerator => _spineAnimationGenerator;
    public ProceduralBossBehavior BossBehaviorSystem => _bossBehaviorSystem;
    public ProceduralEnemyGenerator EnemyGenerator => _enemyGenerator;
    public MarkovRoomGenerator RoomGenerator => _roomGenerator;
    public DynamicPlatformGenerator PlatformGenerator => _platformGenerator;
    public MarkovParticleSystemGenerator ParticleSystemGenerator => _particleSystemGenerator;
    public DynamicBackgroundGenerator BackgroundGenerator => _backgroundGenerator;
    public MarkovAudioSystem AudioSystem => _audioSystem;
    public MarkovPlayerPredictionAI AISystem => _aiSystem;
    public ProceduralBossPhaseSystem BossPhaseSystem => _bossPhaseSystem;
    public MarkovDialogueGenerator DialogueGenerator => _dialogueGenerator;
    public ProceduralAchievementSystem AchievementSystem => _achievementSystem;
    public DynamicDifficultyBalancer DifficultyBalancer => _difficultyBalancer;
}

/// <summary>
/// Comprehensive statistics for all procedural generation systems
/// </summary>
public class MasterGenerationStats
{
    public Dictionary<string, string> MarkovChainStats { get; set; } = new();
    public RoomGenerationStats RoomGenerationStats { get; set; } = new();
    public EnemyGenerationStats EnemyGenerationStats { get; set; } = new();
    public PlatformGenerationStats PlatformGenerationStats { get; set; } = new();
    public ParticleSystemStats ParticleSystemStats { get; set; } = new();
    public BackgroundGenerationStats BackgroundGenerationStats { get; set; } = new();
    public AudioStats AudioStats { get; set; } = new();
    public AIStats AIStats { get; set; } = new();
    public PhaseSystemStats BossPhaseSystemStats { get; set; } = new();
    public DialogueGenerationStats DialogueGenerationStats { get; set; } = new();
    public AchievementStats AchievementStats { get; set; } = new();
    public DifficultyBalancingStats DifficultyBalancingStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"=== PROCEDURAL GENERATION STATS ===\n" +
               $"Rooms: {RoomGenerationStats}\n" +
               $"Enemies: {EnemyGenerationStats}\n" +
               $"Platforms: {PlatformGenerationStats}\n" +
               $"Particles: {ParticleSystemStats}\n" +
               $"Backgrounds: {BackgroundGenerationStats}\n" +
               $"Audio: {AudioStats.TotalSegmentsGenerated}\n" +
               $"AI: {AIStats}\n" +
               $"Boss Phases: {BossPhaseSystemStats.TotalPhasesGenerated}\n" +
               $"Dialogue: {DialogueGenerationStats}\n" +
               $"Achievements: {AchievementStats}\n" +
               $"Difficulty: {DifficultyBalancingStats}";
    }
}