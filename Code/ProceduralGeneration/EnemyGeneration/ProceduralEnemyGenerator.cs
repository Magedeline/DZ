using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.MaggyHelper.ProceduralGeneration.MarkovChain;
using Celeste.Mod.MaggyHelper.ProceduralGeneration.SpineAnimation;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaggyHelper.ProceduralGeneration.EnemyGeneration;

/// <summary>
/// Procedural enemy generator using Markov chains and component mixing
/// </summary>
public class ProceduralEnemyGenerator
{
    private const string LogTag = "ProceduralEnemyGenerator";
    
    private readonly MarkovChain<string> _behaviorChain;
    private readonly MarkovChain<string> _movementChain;
    private readonly MarkovChain<string> _attackChain;
    private readonly EnemyComponentLibrary _componentLibrary;
    private readonly Random _random;
    private readonly int _seed;
    
    private readonly Dictionary<string, EnemyVariant> _generatedVariants;
    private int _variantCounter;
    
    public ProceduralEnemyGenerator(int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _behaviorChain = new MarkovChain<string>("EnemyBehaviors", seed);
        _movementChain = new MarkovChain<string>("EnemyMovements", seed);
        _attackChain = new MarkovChain<string>("EnemyAttacks", seed);
        _componentLibrary = new EnemyComponentLibrary();
        
        _generatedVariants = new Dictionary<string, EnemyVariant>();
        _variantCounter = 0;
        
        InitializeChains();
    }
    
    /// <summary>
    /// Initialize Markov chains with enemy behavior patterns
    /// </summary>
    private void InitializeChains()
    {
        var behaviors = new[]
        {
            "Patrol", "Chase", "Flee", "Stationary", "Random", 
            "Circle", "Retreat", "Ambush", "Follow", "Lead"
        };
        
        var movements = new[]
        {
            "Walk", "Fly", "Swim", "Teleport", "Dash", 
            "Float", "Crawl", "Jump", "Glide", "Phase"
        };
        
        var attacks = new[]
        {
            "Contact", "Projectile", "Melee", "Beam", "Explosion",
            "Summon", "Debuff", "Charge", "Spin", "Spread"
        };
        
        // Initialize behavior chain
        foreach (var fromBehavior in behaviors)
        {
            foreach (var toBehavior in behaviors)
            {
                float probability = CalculateBehaviorTransition(fromBehavior, toBehavior);
                _behaviorChain.AddTransition(fromBehavior, toBehavior, probability);
            }
        }
        
        // Initialize movement chain
        foreach (var fromMovement in movements)
        {
            foreach (var toMovement in movements)
            {
                float probability = 0.1f; // Balanced transitions
                if (fromMovement == toMovement) probability = 0.05f; // Less self-loops
                _movementChain.AddTransition(fromMovement, toMovement, probability);
            }
        }
        
        // Initialize attack chain
        foreach (var fromAttack in attacks)
        {
            foreach (var toAttack in attacks)
            {
                float probability = CalculateAttackTransition(fromAttack, toAttack);
                _attackChain.AddTransition(fromAttack, toAttack, probability);
            }
        }
        
        _behaviorChain.Initialize("Patrol");
        _movementChain.Initialize("Walk");
        _attackChain.Initialize("Contact");
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized enemy generation chains");
    }
    
    /// <summary>
    /// Calculate behavior transition probabilities
    /// </summary>
    private float CalculateBehaviorTransition(string from, string to)
    {
        // Patrol often transitions to chase when player is near
        if (from == "Patrol" && to == "Chase")
            return 0.3f;
        
        // Chase often transitions to flee if enemy is damaged
        if (from == "Chase" && to == "Flee")
            return 0.2f;
        
        // Flee often transitions back to patrol when safe
        if (from == "Flee" && to == "Patrol")
            return 0.4f;
        
        // Ambush often transitions to chase
        if (from == "Ambush" && to == "Chase")
            return 0.35f;
        
        // Stationary rarely changes
        if (from == "Stationary")
            return 0.05f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        return 0.1f;
    }
    
    /// <summary>
    /// Calculate attack transition probabilities
    /// </summary>
    private float CalculateAttackTransition(string from, string to)
    {
        // Contact attacks often lead to other attacks
        if (from == "Contact")
            return 0.2f;
        
        // Projectile attacks often followed by melee
        if (from == "Projectile" && to == "Melee")
            return 0.25f;
        
        // Special attacks are rare
        if (to == "Summon" || to == "Debuff" || to == "Beam")
            return 0.08f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.03f;
        
        return 0.12f;
    }
    
    /// <summary>
    /// Generate a completely new enemy variant
    /// </summary>
    public EnemyVariant GenerateVariant(EnemyArchetype archetype, DifficultyTier difficulty)
    {
        var variantName = $"{archetype}_Variant_{_variantCounter++}";
        
        // Select components based on archetype and difficulty
        var behavior = SelectBehavior(archetype, difficulty);
        var movement = SelectMovement(archetype);
        var attack = SelectAttack(archetype, difficulty);
        
        // Select visual components
        var skeleton = SelectSkeleton(archetype);
        var colorScheme = SelectColorScheme(archetype, difficulty);
        var sizeMultiplier = CalculateSizeMultiplier(difficulty);
        
        // Select stats based on components
        var stats = CalculateStats(behavior, movement, attack, difficulty);
        
        // Generate Spine animation variants
        var animationGenerator = new SpineAnimationGenerator(skeleton, _seed + _variantCounter);
        var idleVariants = animationGenerator.GenerateIdleVariants("Idle", 3);
        var attackPattern = animationGenerator.GenerateAttackPattern(
            new List<string> { "Attack", "SpecialAttack" });
        
        var variant = new EnemyVariant
        {
            Name = variantName,
            Archetype = archetype,
            Behavior = behavior,
            Movement = movement,
            Attack = attack,
            Skeleton = skeleton,
            ColorScheme = colorScheme,
            SizeMultiplier = sizeMultiplier,
            Stats = stats,
            IdleVariants = idleVariants,
            AttackPattern = attackPattern,
            DifficultyTier = difficulty
        };
        
        _generatedVariants[variantName] = variant;
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated enemy variant: {variantName} ({archetype})");
        
        return variant;
    }
    
    /// <summary>
    /// Generate multiple variants from a base enemy type
    /// </summary>
    public List<EnemyVariant> GenerateVariantsFromBase(
        string baseEnemyType, int count, DifficultyTier difficulty)
    {
        var variants = new List<EnemyVariant>();
        var archetype = ParseArchetype(baseEnemyType);
        
        for (int i = 0; i < count; i++)
        {
            var variant = GenerateVariant(archetype, difficulty);
            variant.BaseEnemyType = baseEnemyType;
            variants.Add(variant);
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated {count} variants from base enemy: {baseEnemyType}");
        
        return variants;
    }
    
    /// <summary>
    /// Select behavior based on archetype and difficulty
    /// </summary>
    private string SelectBehavior(EnemyArchetype archetype, DifficultyTier difficulty)
    {
        var possibleBehaviors = _componentLibrary.GetBehaviorsForArchetype(archetype);
        
        // Higher difficulty = more aggressive behaviors
        if (difficulty >= DifficultyTier.Hard)
        {
            var aggressive = possibleBehaviors.Where(b => 
                b == "Chase" || b == "Ambush" || b == "Circle").ToList();
            if (aggressive.Count > 0 && _random.NextDouble() < 0.7f)
            {
                return aggressive[_random.Next(aggressive.Count)];
            }
        }
        
        return possibleBehaviors[_random.Next(possibleBehaviors.Count)];
    }
    
    /// <summary>
    /// Select movement type based on archetype
    /// </summary>
    private string SelectMovement(EnemyArchetype archetype)
    {
        var possibleMovements = _componentLibrary.GetMovementsForArchetype(archetype);
        return possibleMovements[_random.Next(possibleMovements.Count)];
    }
    
    /// <summary>
    /// Select attack type based on archetype and difficulty
    /// </summary>
    private string SelectAttack(EnemyArchetype archetype, DifficultyTier difficulty)
    {
        var possibleAttacks = _componentLibrary.GetAttacksForArchetype(archetype);
        
        // Higher difficulty = more complex attacks
        if (difficulty >= DifficultyTier.Expert)
        {
            var complexAttacks = possibleAttacks.Where(a => 
                a == "Beam" || a == "Summon" || a == "Spread").ToList();
            if (complexAttacks.Count > 0 && _random.NextDouble() < 0.5f)
            {
                return complexAttacks[_random.Next(complexAttacks.Count)];
            }
        }
        
        return possibleAttacks[_random.Next(possibleAttacks.Count)];
    }
    
    /// <summary>
    /// Select skeleton based on archetype
    /// </summary>
    private string SelectSkeleton(EnemyArchetype archetype)
    {
        return archetype switch
        {
            EnemyArchetype.Grounded => "GroundedEnemy_Skeleton",
            EnemyArchetype.Flying => "FlyingEnemy_Skeleton",
            EnemyArchetype.Aquatic => "AquaticEnemy_Skeleton",
            EnemyArchetype.Boss => "BossEnemy_Skeleton",
            _ => "BaseEnemy_Skeleton"
        };
    }
    
    /// <summary>
    /// Select color scheme based on archetype and difficulty
    /// </summary>
    private ColorScheme SelectColorScheme(EnemyArchetype archetype, DifficultyTier difficulty)
    {
        var baseScheme = archetype switch
        {
            EnemyArchetype.Grounded => new ColorScheme(Color.OrangeRed, Color.Red),
            EnemyArchetype.Flying => new ColorScheme(Color.SkyBlue, Color.Blue),
            EnemyArchetype.Aquatic => new ColorScheme(Color.Teal, Color.Cyan),
            EnemyArchetype.Boss => new ColorScheme(Color.Purple, Color.Magenta),
            _ => new ColorScheme(Color.White, Color.LightGray)
        };
        
        // Modify colors based on difficulty
        if (difficulty >= DifficultyTier.Hard)
        {
            baseScheme.PrimaryColor = AdjustColorForDifficulty(baseScheme.PrimaryColor, difficulty);
            baseScheme.SecondaryColor = AdjustColorForDifficulty(baseScheme.SecondaryColor, difficulty);
        }
        
        return baseScheme;
    }
    
    /// <summary>
    /// Adjust color intensity based on difficulty
    /// </summary>
    private Color AdjustColorForDifficulty(Color baseColor, DifficultyTier difficulty)
    {
        float intensity = difficulty switch
        {
            DifficultyTier.Easy => 0.8f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.2f,
            DifficultyTier.Expert => 1.4f,
            DifficultyTier.Master => 1.6f,
            _ => 1.0f
        };
        
        return new Color(
            (byte)Math.Min(255, baseColor.R * intensity),
            (byte)Math.Min(255, baseColor.G * intensity),
            (byte)Math.Min(255, baseColor.B * intensity)
        );
    }
    
    /// <summary>
    /// Calculate size multiplier based on difficulty
    /// </summary>
    private float CalculateSizeMultiplier(DifficultyTier difficulty)
    {
        return difficulty switch
        {
            DifficultyTier.Easy => 0.8f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.1f,
            DifficultyTier.Expert => 1.2f,
            DifficultyTier.Master => 1.3f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Calculate enemy stats based on components and difficulty
    /// </summary>
    private EnemyStats CalculateStats(string behavior, string movement, string attack, DifficultyTier difficulty)
    {
        float baseHealth = 100f;
        float baseSpeed = 1f;
        float baseDamage = 10f;
        
        // Adjust based on behavior
        if (behavior == "Chase") baseSpeed *= 1.5f;
        if (behavior == "Stationary") baseHealth *= 1.5f;
        if (behavior == "Flee") baseSpeed *= 2f; baseHealth *= 0.8f;
        
        // Adjust based on movement
        if (movement == "Fly") baseSpeed *= 1.3f;
        if (movement == "Teleport") baseSpeed *= 0.5f; baseDamage *= 1.2f;
        if (movement == "Dash") baseSpeed *= 2f; baseHealth *= 0.9f;
        
        // Adjust based on attack
        if (attack == "Beam") baseDamage *= 2f; baseHealth *= 0.8f;
        if (attack == "Explosion") baseDamage *= 3f; baseHealth *= 0.7f;
        if (attack == "Summon") baseDamage *= 0.5f; baseHealth *= 1.5f;
        
        // Adjust based on difficulty
        float difficultyMultiplier = difficulty switch
        {
            DifficultyTier.Easy => 0.7f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.3f,
            DifficultyTier.Expert => 1.6f,
            DifficultyTier.Master => 2.0f,
            _ => 1.0f
        };
        
        return new EnemyStats
        {
            MaxHealth = baseHealth * difficultyMultiplier,
            MoveSpeed = baseSpeed,
            AttackDamage = baseDamage * difficultyMultiplier,
            AttackRange = attack == "Beam" ? 300f : 100f,
            DetectionRange = behavior == "Ambush" ? 400f : 200f
        };
    }
    
    /// <summary>
    /// Parse enemy archetype from string
    /// </summary>
    private EnemyArchetype ParseArchetype(string enemyType)
    {
        if (enemyType.Contains("fly", StringComparison.OrdinalIgnoreCase))
            return EnemyArchetype.Flying;
        if (enemyType.Contains("swim", StringComparison.OrdinalIgnoreCase) || 
            enemyType.Contains("water", StringComparison.OrdinalIgnoreCase))
            return EnemyArchetype.Aquatic;
        if (enemyType.Contains("boss", StringComparison.OrdinalIgnoreCase))
            return EnemyArchetype.Boss;
        
        return EnemyArchetype.Grounded;
    }
    
    /// <summary>
    /// Get a previously generated variant
    /// </summary>
    public EnemyVariant? GetVariant(string variantName)
    {
        return _generatedVariants.TryGetValue(variantName, out var variant) ? variant : null;
    }
    
    /// <summary>
    /// Get all generated variants
    /// </summary>
    public List<EnemyVariant> GetAllVariants()
    {
        return _generatedVariants.Values.ToList();
    }
    
    /// <summary>
    /// Get variants by archetype
    /// </summary>
    public List<EnemyVariant> GetVariantsByArchetype(EnemyArchetype archetype)
    {
        return _generatedVariants.Values
            .Where(v => v.Archetype == archetype)
            .ToList();
    }
    
    /// <summary>
    /// Generate a behavior sequence for an enemy
    /// </summary>
    public List<string> GenerateBehaviorSequence(string startBehavior, int length)
    {
        _behaviorChain.Initialize(startBehavior);
        return _behaviorChain.GenerateSequence(length);
    }
    
    /// <summary>
    /// Get statistics about generated enemies
    /// </summary>
    public EnemyGenerationStats GetStats()
    {
        return new EnemyGenerationStats
        {
            TotalVariantsGenerated = _generatedVariants.Count,
            VariantsByArchetype = new Dictionary<EnemyArchetype, int>
            {
                { EnemyArchetype.Grounded, GetVariantsByArchetype(EnemyArchetype.Grounded).Count },
                { EnemyArchetype.Flying, GetVariantsByArchetype(EnemyArchetype.Flying).Count },
                { EnemyArchetype.Aquatic, GetVariantsByArchetype(EnemyArchetype.Aquatic).Count },
                { EnemyArchetype.Boss, GetVariantsByArchetype(EnemyArchetype.Boss).Count }
            },
            BehaviorChainStats = _behaviorChain.GetStats(),
            MovementChainStats = _movementChain.GetStats(),
            AttackChainStats = _attackChain.GetStats()
        };
    }
}

/// <summary>
/// Enemy archetypes
/// </summary>
public enum EnemyArchetype
{
    Grounded,
    Flying,
    Aquatic,
    Boss
}

/// <summary>
/// Procedurally generated enemy variant
/// </summary>
public class EnemyVariant
{
    public string Name { get; set; } = string.Empty;
    public string BaseEnemyType { get; set; } = string.Empty;
    public EnemyArchetype Archetype { get; set; }
    public string Behavior { get; set; } = string.Empty;
    public string Movement { get; set; } = string.Empty;
    public string Attack { get; set; } = string.Empty;
    public string Skeleton { get; set; } = string.Empty;
    public ColorScheme ColorScheme { get; set; } = new();
    public float SizeMultiplier { get; set; } = 1f;
    public EnemyStats Stats { get; set; } = new();
    public List<AnimationVariant> IdleVariants { get; set; } = new();
    public BossAttackPattern AttackPattern { get; set; } = new();
    public DifficultyTier DifficultyTier { get; set; }
}

/// <summary>
/// Enemy color scheme
/// </summary>
public class ColorScheme
{
    public Color PrimaryColor { get; set; }
    public Color SecondaryColor { get; set; }
    
    public ColorScheme()
    {
        PrimaryColor = Color.White;
        SecondaryColor = Color.Gray;
    }
    
    public ColorScheme(Color primary, Color secondary)
    {
        PrimaryColor = primary;
        SecondaryColor = secondary;
    }
}

/// <summary>
/// Enemy stats
/// </summary>
public class EnemyStats
{
    public float MaxHealth { get; set; } = 100f;
    public float MoveSpeed { get; set; } = 1f;
    public float AttackDamage { get; set; } = 10f;
    public float AttackRange { get; set; } = 100f;
    public float DetectionRange { get; set; } = 200f;
}

/// <summary>
/// Library of enemy components organized by archetype
/// </summary>
public class EnemyComponentLibrary
{
    private readonly Dictionary<EnemyArchetype, List<string>> _behaviors;
    private readonly Dictionary<EnemyArchetype, List<string>> _movements;
    private readonly Dictionary<EnemyArchetype, List<string>> _attacks;
    
    public EnemyComponentLibrary()
    {
        _behaviors = new Dictionary<EnemyArchetype, List<string>>();
        _movements = new Dictionary<EnemyArchetype, List<string>>();
        _attacks = new Dictionary<EnemyArchetype, List<string>>();
        
        InitializeLibrary();
    }
    
    private void InitializeLibrary()
    {
        // Grounded enemies
        _behaviors[EnemyArchetype.Grounded] = new List<string> { "Patrol", "Chase", "Stationary", "Ambush" };
        _movements[EnemyArchetype.Grounded] = new List<string> { "Walk", "Dash", "Jump", "Crawl" };
        _attacks[EnemyArchetype.Grounded] = new List<string> { "Contact", "Melee", "Charge", "Spin" };
        
        // Flying enemies
        _behaviors[EnemyArchetype.Flying] = new List<string> { "Patrol", "Chase", "Circle", "Ambush", "Follow" };
        _movements[EnemyArchetype.Flying] = new List<string> { "Fly", "Float", "Dash", "Glide" };
        _attacks[EnemyArchetype.Flying] = new List<string> { "Contact", "Projectile", "Beam", "Spread" };
        
        // Aquatic enemies
        _behaviors[EnemyArchetype.Aquatic] = new List<string> { "Patrol", "Chase", "Circle", "Retreat" };
        _movements[EnemyArchetype.Aquatic] = new List<string> { "Swim", "Float" };
        _attacks[EnemyArchetype.Aquatic] = new List<string> { "Contact", "Projectile", "Debuff" };
        
        // Boss enemies
        _behaviors[EnemyArchetype.Boss] = new List<string> { "Chase", "Circle", "Retreat", "Ambush", "Lead" };
        _movements[EnemyArchetype.Boss] = new List<string> { "Fly", "Teleport", "Dash", "Phase", "Float" };
        _attacks[EnemyArchetype.Boss] = new List<string> { "Projectile", "Beam", "Summon", "Explosion", "Spread", "Debuff" };
    }
    
    public List<string> GetBehaviorsForArchetype(EnemyArchetype archetype)
    {
        return _behaviors.TryGetValue(archetype, out var behaviors) ? behaviors : new List<string>();
    }
    
    public List<string> GetMovementsForArchetype(EnemyArchetype archetype)
    {
        return _movements.TryGetValue(archetype, out var movements) ? movements : new List<string>();
    }
    
    public List<string> GetAttacksForArchetype(EnemyArchetype archetype)
    {
        return _attacks.TryGetValue(archetype, out var attacks) ? attacks : new List<string>();
    }
}

/// <summary>
/// Statistics for enemy generation
/// </summary>
public class EnemyGenerationStats
{
    public int TotalVariantsGenerated { get; set; }
    public Dictionary<EnemyArchetype, int> VariantsByArchetype { get; set; } = new();
    public MarkovChainStats BehaviorChainStats { get; set; } = new();
    public MarkovChainStats MovementChainStats { get; set; } = new();
    public MarkovChainStats AttackChainStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"Total Variants: {TotalVariantsGenerated}, " +
               $"Grounded: {VariantsByArchetype[EnemyArchetype.Grounded]}, " +
               $"Flying: {VariantsByArchetype[EnemyArchetype.Flying]}, " +
               $"Aquatic: {VariantsByArchetype[EnemyArchetype.Aquatic]}, " +
               $"Boss: {VariantsByArchetype[EnemyArchetype.Boss]}";
    }
}
