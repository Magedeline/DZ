using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.SpineAnimation;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.BossPhase;

/// <summary>
/// Procedural boss phase system with Markov-driven phase transitions
/// </summary>
public class ProceduralBossPhaseSystem
{
    private const string LogTag = "ProceduralBossPhaseSystem";
    
    private readonly MarkovChain<string> _phaseChain;
    private readonly SpineAnimationGenerator _animationGenerator;
    private readonly Random _random;
    private readonly int _seed;
    
    private readonly Dictionary<string, BossPhaseConfiguration> _phaseConfigurations;
    private int _phaseCounter;
    
    public ProceduralBossPhaseSystem(int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _phaseChain = new MarkovChain<string>("BossPhases", seed);
        _animationGenerator = new SpineAnimationGenerator("Boss_Skeleton", seed);
        
        _phaseConfigurations = new Dictionary<string, BossPhaseConfiguration>();
        _phaseCounter = 0;
        
        InitializeChains();
    }
    
    private void InitializeChains()
    {
        var phases = new[] { "Intro", "Normal", "Aggressive", "Desperate", "Final" };
        
        foreach (var from in phases)
            foreach (var to in phases)
                _phaseChain.AddTransition(from, to, CalculatePhaseTransition(from, to));
        
        _phaseChain.Initialize("Intro");
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized boss phase chains");
    }
    
    private float CalculatePhaseTransition(string from, string to)
    {
        // Linear progression preferred
        if (from == "Intro" && to == "Normal") return 0.7f;
        if (from == "Normal" && to == "Aggressive") return 0.4f;
        if (from == "Aggressive" && to == "Desperate") return 0.5f;
        if (from == "Desperate" && to == "Final") return 0.6f;
        
        // Prevent backwards progression
        if (from == "Final" && to != "Defeated") return 0f;
        
        // Prevent skipping phases
        if (from == "Intro" && (to == "Desperate" || to == "Final")) return 0f;
        
        return 0.1f;
    }
    
    public BossPhaseConfiguration GeneratePhaseConfiguration(string bossName, float healthPercent)
    {
        string phaseName;
        
        if (healthPercent > 0.7f)
            phaseName = "Normal";
        else if (healthPercent > 0.4f)
            phaseName = "Aggressive";
        else if (healthPercent > 0.1f)
            phaseName = "Desperate";
        else
            phaseName = "Final";
        
        return new BossPhaseConfiguration
        {
            Id = $"Phase_{_phaseCounter++}",
            BossName = bossName,
            PhaseName = phaseName,
            HealthThreshold = healthPercent,
            AttackPattern = GenerateAttackPattern(phaseName),
            MovementPattern = GenerateMovementPattern(phaseName),
            DefensePattern = GenerateDefensePattern(phaseName),
            VisualEffects = GenerateVisualEffects(phaseName),
            Duration = CalculatePhaseDuration(phaseName)
        };
    }
    
    private string GenerateAttackPattern(string phaseName)
    {
        var attacks = phaseName switch
        {
            "Normal" => new[] { "Single", "Double", "Triple" },
            "Aggressive" => new[] { "Double", "Triple", "Combo" },
            "Desperate" => new[] { "Triple", "Combo", "Ultimate" },
            "Final" => new[] { "Combo", "Ultimate", "Desperation" },
            _ => new[] { "Single" }
        };
        
        return attacks[_random.Next(attacks.Length)];
    }
    
    private string GenerateMovementPattern(string phaseName)
    {
        var movements = phaseName switch
        {
            "Normal" => new[] { "Patrol", "Chase" },
            "Aggressive" => new[] { "Chase", "Dash", "Teleport" },
            "Desperate" => new[] { "Dash", "Teleport", "Erratic" },
            "Final" => new[] { "Teleport", "Erratic", "PhaseShift" },
            _ => new[] { "Patrol" }
        };
        
        return movements[_random.Next(movements.Length)];
    }
    
    private string GenerateDefensePattern(string phaseName)
    {
        var defenses = phaseName switch
        {
            "Normal" => new[] { "None", "Block" },
            "Aggressive" => new[] { "Block", "Parry", "Dodge" },
            "Desperate" => new[] { "Parry", "Dodge", "Counter" },
            "Final" => new[] { "Dodge", "Counter", "Invincibility" },
            _ => new[] { "None" }
        };
        
        return defenses[_random.Next(defenses.Length)];
    }
    
    private List<string> GenerateVisualEffects(string phaseName)
    {
        var effects = phaseName switch
        {
            "Normal" => new List<string> { "Bloom" },
            "Aggressive" => new List<string> { "Bloom", "Shake" },
            "Desperate" => new List<string> { "Bloom", "Shake", "Glitch" },
            "Final" => new List<string> { "Bloom", "Shake", "Glitch", "Distortion" },
            _ => new List<string>()
        };
        
        return effects;
    }
    
    private float CalculatePhaseDuration(string phaseName)
    {
        return phaseName switch
        {
            "Normal" => 30f + (float)_random.NextDouble() * 20f,
            "Aggressive" => 20f + (float)_random.NextDouble() * 15f,
            "Desperate" => 15f + (float)_random.NextDouble() * 10f,
            "Final" => 10f + (float)_random.NextDouble() * 10f,
            _ => 30f
        };
    }
    
    public PhaseSystemStats GetStats()
    {
        return new PhaseSystemStats
        {
            TotalPhasesGenerated = _phaseCounter,
            PhaseChainStats = _phaseChain.GetStats()
        };
    }
}

public class BossPhaseConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string BossName { get; set; } = string.Empty;
    public string PhaseName { get; set; } = string.Empty;
    public float HealthThreshold { get; set; }
    public string AttackPattern { get; set; } = string.Empty;
    public string MovementPattern { get; set; } = string.Empty;
    public string DefensePattern { get; set; } = string.Empty;
    public List<string> VisualEffects { get; set; } = new();
    public float Duration { get; set; }
}

public class PhaseSystemStats
{
    public int TotalPhasesGenerated { get; set; }
    public MarkovChainStats PhaseChainStats { get; set; } = new();
}