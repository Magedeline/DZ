using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.SpineAnimation;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.BossBehavior;

/// <summary>
/// Procedural boss behavior system using Markov chains and Spine animations
/// </summary>
public class ProceduralBossBehavior
{
    private const string LogTag = "ProceduralBossBehavior";
    
    private readonly MarkovChain<string> _behaviorChain;
    private readonly MarkovChain<string> _phaseChain;
    private readonly SpineAnimationGenerator _animationGenerator;
    private readonly BossProfile _bossProfile;
    private readonly Random _random;
    private readonly int _seed;
    
    private BossPhase _currentPhase;
    private string _currentBehavior;
    private float _difficultyMultiplier;
    private int _behaviorCount;
    
    public string BossName => _bossProfile.Name;
    public BossPhase CurrentPhase => _currentPhase;
    public string CurrentBehavior => _currentBehavior;
    public float DifficultyMultiplier => _difficultyMultiplier;
    
    public event Action<BossPhase, BossPhase>? OnPhaseTransition;
    public event Action<string, string>? OnBehaviorChange;
    public event Action<BossAttackPattern>? OnAttackPatternGenerated;
    
    public ProceduralBossBehavior(BossProfile profile, int seed = 0)
    {
        _bossProfile = profile;
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _behaviorChain = new MarkovChain<string>($"{profile.Name}_Behaviors", seed);
        _phaseChain = new MarkovChain<string>($"{profile.Name}_Phases", seed);
        _animationGenerator = new SpineAnimationGenerator(profile.SkeletonName, seed);
        
        _currentPhase = BossPhase.Intro;
        _currentBehavior = "Idle";
        _difficultyMultiplier = 1f;
        _behaviorCount = 0;
        
        InitializeBehaviors();
        InitializePhases();
    }
    
    /// <summary>
    /// Initialize behavior transitions based on boss profile
    /// </summary>
    private void InitializeBehaviors()
    {
        var behaviors = _bossProfile.AvailableBehaviors;
        
        foreach (var fromBehavior in behaviors)
        {
            foreach (var toBehavior in behaviors)
            {
                float probability = CalculateBehaviorTransition(fromBehavior, toBehavior);
                if (probability > 0)
                {
                    _behaviorChain.AddTransition(fromBehavior, toBehavior, probability);
                }
            }
        }
        
        _behaviorChain.Initialize("Idle");
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Initialized behaviors for {_bossProfile.Name}");
    }
    
    /// <summary>
    /// Initialize phase transitions
    /// </summary>
    private void InitializePhases()
    {
        var phases = Enum.GetValues(typeof(BossPhase)).Cast<BossPhase>();
        
        foreach (var fromPhase in phases)
        {
            foreach (var toPhase in phases)
            {
                float probability = CalculatePhaseTransition(fromPhase, toPhase);
                if (probability > 0)
                {
                    _phaseChain.AddTransition(fromPhase.ToString(), toPhase.ToString(), probability);
                }
            }
        }
        
        _phaseChain.Initialize(BossPhase.Intro.ToString());
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Initialized phases for {_bossProfile.Name}");
    }
    
    /// <summary>
    /// Calculate behavior transition probabilities
    /// </summary>
    private float CalculateBehaviorTransition(string from, string to)
    {
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        // Idle often transitions to attacks
        if (from == "Idle" && to.Contains("Attack"))
            return 0.25f;
        
        // Attacks often return to idle
        if (from.Contains("Attack") && to == "Idle")
            return 0.3f;
        
        // Special attacks are rare
        if (to.Contains("Special"))
            return 0.08f;
        
        // Defensive moves after attacks
        if (from.Contains("Attack") && to.Contains("Defense"))
            return 0.15f;
        
        // Movement behaviors
        if (to.Contains("Move"))
            return 0.12f;
        
        return 0.1f;
    }
    
    /// <summary>
    /// Calculate phase transition probabilities
    /// </summary>
    private float CalculatePhaseTransition(BossPhase from, BossPhase to)
    {
        // Normal phase progression
        if (from == BossPhase.Intro && to == BossPhase.Normal)
            return 0.8f;
        
        if (from == BossPhase.Normal && to == BossPhase.Enraged)
            return 0.15f;
        
        if (from == BossPhase.Enraged && to == BossPhase.Desperate)
            return 0.2f;
        
        if (from == BossPhase.Desperate && to == BossPhase.Defeated)
            return 0.5f;
        
        // Prevent backwards progression
        if ((int)to < (int)from && to != BossPhase.Normal)
            return 0f;
        
        return 0.05f;
    }
    
    /// <summary>
    /// Update the boss behavior state
    /// </summary>
    public void Update(float deltaTime, Player player, float bossHealthPercent)
    {
        _behaviorCount++;
        
        // Check for phase transition
        if (ShouldTransitionPhase(bossHealthPercent))
        {
            TransitionToNextPhase();
        }
        
        // Check for behavior change
        if (ShouldChangeBehavior(deltaTime))
        {
            TransitionToNextBehavior(player);
        }
        
        // Update difficulty based on phase
        UpdateDifficultyMultiplier();
    }
    
    /// <summary>
    /// Check if boss should transition to next phase
    /// </summary>
    private bool ShouldTransitionPhase(float healthPercent)
    {
        return _currentPhase switch
        {
            BossPhase.Intro => _behaviorCount > 60, // After ~1 second
            BossPhase.Normal => healthPercent < 0.7f,
            BossPhase.Enraged => healthPercent < 0.4f,
            BossPhase.Desperate => healthPercent < 0.1f,
            _ => false
        };
    }
    
    /// <summary>
    /// Check if boss should change behavior
    /// </summary>
    private bool ShouldChangeBehavior(float deltaTime)
    {
        // Behavior change frequency based on phase
        int changeInterval = _currentPhase switch
        {
            BossPhase.Intro => 120,
            BossPhase.Normal => 90,
            BossPhase.Enraged => 60,
            BossPhase.Desperate => 45,
            _ => 90
        };
        
        return _behaviorCount % changeInterval == 0;
    }
    
    /// <summary>
    /// Transition to the next phase
    /// </summary>
    private void TransitionToNextPhase()
    {
        var previousPhase = _currentPhase;
        var nextPhaseStr = _phaseChain.GetNextState();
        
        if (nextPhaseStr != null && Enum.TryParse<BossPhase>(nextPhaseStr, out var nextPhase))
        {
            _currentPhase = nextPhase;
            OnPhaseTransition?.Invoke(previousPhase, _currentPhase);
            
            Logger.Log(LogLevel.Info, LogTag, 
                $"{_bossProfile.Name} transitioned from {previousPhase} to {_currentPhase}");
            
            // Generate new behavior patterns for this phase
            GeneratePhaseBehaviorPatterns();
        }
    }
    
    /// <summary>
    /// Transition to the next behavior
    /// </summary>
    private void TransitionToNextBehavior(Player player)
    {
        var previousBehavior = _currentBehavior;
        var nextBehavior = _behaviorChain.GetNextState();
        
        if (nextBehavior != null)
        {
            _currentBehavior = nextBehavior;
            OnBehaviorChange?.Invoke(previousBehavior, _currentBehavior);
            
            Logger.Log(LogLevel.Verbose, LogTag, 
                $"{_bossProfile.Name} changed behavior from {previousBehavior} to {_currentBehavior}");
            
            // Generate attack pattern if it's an attack behavior
            if (_currentBehavior.Contains("Attack"))
            {
                GenerateAttackPattern(player);
            }
        }
    }
    
    /// <summary>
    /// Generate behavior patterns for the current phase
    /// </summary>
    private void GeneratePhaseBehaviorPatterns()
    {
        // Adjust behavior transition probabilities based on phase
        var behaviors = _bossProfile.AvailableBehaviors;
        
        foreach (var fromBehavior in behaviors)
        {
            foreach (var toBehavior in behaviors)
            {
                float adjustedProbability = CalculatePhaseAdjustedProbability(
                    fromBehavior, toBehavior, _currentPhase);
                
                _behaviorChain.AddTransition(fromBehavior, toBehavior, adjustedProbability);
            }
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated behavior patterns for {_bossProfile.Name} phase {_currentPhase}");
    }
    
    /// <summary>
    /// Calculate phase-adjusted behavior probability
    /// </summary>
    private float CalculatePhaseAdjustedProbability(string from, string to, BossPhase phase)
    {
        float baseProbability = CalculateBehaviorTransition(from, to);
        
        // Enraged phase: more attacks, less idle
        if (phase == BossPhase.Enraged)
        {
            if (to.Contains("Attack"))
                baseProbability *= 1.5f;
            if (to == "Idle")
                baseProbability *= 0.5f;
        }
        
        // Desperate phase: more special attacks, no idle
        if (phase == BossPhase.Desperate)
        {
            if (to.Contains("Special"))
                baseProbability *= 2.0f;
            if (to == "Idle")
                baseProbability *= 0.1f;
        }
        
        return baseProbability;
    }
    
    /// <summary>
    /// Generate an attack pattern based on current behavior
    /// </summary>
    private void GenerateAttackPattern(Player player)
    {
        var attackAnimations = _bossProfile.GetAttackAnimationsForBehavior(_currentBehavior);
        
        if (attackAnimations.Count > 0)
        {
            var pattern = _animationGenerator.GenerateAttackPattern(attackAnimations);
            pattern.IntensityMultiplier = _difficultyMultiplier;
            
            OnAttackPatternGenerated?.Invoke(pattern);
            
            Logger.Log(LogLevel.Verbose, LogTag, 
                $"{_bossProfile.Name} generated attack pattern for {_currentBehavior}");
        }
    }
    
    /// <summary>
    /// Update difficulty multiplier based on phase
    /// </summary>
    private void UpdateDifficultyMultiplier()
    {
        _difficultyMultiplier = _currentPhase switch
        {
            BossPhase.Intro => 0.8f,
            BossPhase.Normal => 1.0f,
            BossPhase.Enraged => 1.3f,
            BossPhase.Desperate => 1.6f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Get the current attack pattern
    /// </summary>
    public BossAttackPattern GetCurrentAttackPattern()
    {
        var attackAnimations = _bossProfile.GetAttackAnimationsForBehavior(_currentBehavior);
        return _animationGenerator.GenerateAttackPattern(attackAnimations);
    }
    
    /// <summary>
    /// Get animation variants for the current behavior
    /// </summary>
    public List<AnimationVariant> GetCurrentAnimationVariants(int count = 3)
    {
        var baseAnimation = _bossProfile.GetAnimationForBehavior(_currentBehavior);
        if (string.IsNullOrEmpty(baseAnimation))
            return new List<AnimationVariant>();
        
        var variants = new List<AnimationVariant>();
        for (int i = 0; i < count; i++)
        {
            float mutationAmount = _currentPhase switch
            {
                BossPhase.Enraged => 0.3f,
                BossPhase.Desperate => 0.4f,
                _ => 0.15f
            };
            
            variants.Add(_animationGenerator.GenerateVariant(baseAnimation, mutationAmount));
        }
        
        return variants;
    }
    
    /// <summary>
    /// Force a specific behavior (useful for cutscenes or scripted events)
    /// </summary>
    public void ForceBehavior(string behavior)
    {
        var previousBehavior = _currentBehavior;
        _currentBehavior = behavior;
        _behaviorChain.Initialize(behavior);
        
        OnBehaviorChange?.Invoke(previousBehavior, _currentBehavior);
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"{_bossProfile.Name} forced behavior to {behavior}");
    }
    
    /// <summary>
    /// Force a specific phase
    /// </summary>
    public void ForcePhase(BossPhase phase)
    {
        var previousPhase = _currentPhase;
        _currentPhase = phase;
        _phaseChain.Initialize(phase.ToString());
        
        OnPhaseTransition?.Invoke(previousPhase, _currentPhase);
        GeneratePhaseBehaviorPatterns();
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"{_bossProfile.Name} forced phase to {phase}");
    }
    
    /// <summary>
    /// Reset the boss behavior to initial state
    /// </summary>
    public void Reset()
    {
        _currentPhase = BossPhase.Intro;
        _currentBehavior = "Idle";
        _difficultyMultiplier = 1f;
        _behaviorCount = 0;
        
        _behaviorChain.Initialize("Idle");
        _phaseChain.Initialize(BossPhase.Intro.ToString());
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"{_bossProfile.Name} behavior reset");
    }
    
    /// <summary>
    /// Get statistics about the boss behavior
    /// </summary>
    public BossBehaviorStats GetStats()
    {
        return new BossBehaviorStats
        {
            BossName = _bossProfile.Name,
            CurrentPhase = _currentPhase,
            CurrentBehavior = _currentBehavior,
            DifficultyMultiplier = _difficultyMultiplier,
            BehaviorCount = _behaviorCount,
            BehaviorChainStats = _behaviorChain.GetStats(),
            PhaseChainStats = _phaseChain.GetStats()
        };
    }
}

/// <summary>
/// Boss phases
/// </summary>
public enum BossPhase
{
    Intro,
    Normal,
    Enraged,
    Desperate,
    Defeated
}

/// <summary>
/// Boss profile data
/// </summary>
public class BossProfile
{
    public string Name { get; set; } = string.Empty;
    public string SkeletonName { get; set; } = string.Empty;
    public List<string> AvailableBehaviors { get; set; } = new();
    public Dictionary<string, List<string>> BehaviorToAnimations { get; set; } = new();
    public Dictionary<string, List<string>> BehaviorToAttacks { get; set; } = new();
    
    public string GetAnimationForBehavior(string behavior)
    {
        return BehaviorToAnimations.TryGetValue(behavior, out var animations) && animations.Count > 0
            ? animations[0]
            : "Idle";
    }
    
    public List<string> GetAttackAnimationsForBehavior(string behavior)
    {
        return BehaviorToAttacks.TryGetValue(behavior, out var attacks) ? attacks : new List<string>();
    }
}

/// <summary>
/// Boss behavior statistics
/// </summary>
public class BossBehaviorStats
{
    public string BossName { get; set; } = string.Empty;
    public BossPhase CurrentPhase { get; set; }
    public string CurrentBehavior { get; set; } = string.Empty;
    public float DifficultyMultiplier { get; set; }
    public int BehaviorCount { get; set; }
    public MarkovChainStats BehaviorChainStats { get; set; } = new();
    public MarkovChainStats PhaseChainStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"{BossName}: Phase={CurrentPhase}, Behavior={CurrentBehavior}, " +
               $"Difficulty={DifficultyMultiplier:F2}, Count={BehaviorCount}";
    }
}