using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.Difficulty;

/// <summary>
/// Dynamic difficulty balancer using Markov chains for adaptive difficulty scaling
/// </summary>
public class DynamicDifficultyBalancer
{
    private const string LogTag = "DynamicDifficultyBalancer";
    
    private readonly MarkovChain<DifficultyTier> _difficultyChain;
    private readonly Dictionary<string, float> _playerPerformanceMetrics;
    private readonly Dictionary<string, DifficultyAdjustment> _difficultyAdjustments;
    private readonly Random _random;
    private readonly int _seed;
    
    private DifficultyTier _currentDifficulty;
    private float _performanceScore;
    private int _consecutiveDeaths;
    private int _consecutiveSuccesses;
    
    public DifficultyTier CurrentDifficulty => _currentDifficulty;
    public float PerformanceScore => _performanceScore;
    
    public DynamicDifficultyBalancer(DifficultyTier startDifficulty, int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _difficultyChain = new MarkovChain<DifficultyTier>("DifficultyProgression", seed);
        _playerPerformanceMetrics = new Dictionary<string, float>();
        _difficultyAdjustments = new Dictionary<string, DifficultyAdjustment>();
        
        _currentDifficulty = startDifficulty;
        _performanceScore = 0.5f;
        _consecutiveDeaths = 0;
        _consecutiveSuccesses = 0;
        
        InitializeChains();
    }
    
    private void InitializeChains()
    {
        var difficulties = Enum.GetValues<DifficultyTier>();
        
        foreach (var from in difficulties)
            foreach (var to in difficulties)
                _difficultyChain.AddTransition(from, to, CalculateDifficultyTransition(from, to));
        
        _difficultyChain.Initialize(_currentDifficulty);
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized difficulty balancing chains");
    }
    
    private float CalculateDifficultyTransition(DifficultyTier from, DifficultyTier to)
    {
        // Small difficulty changes are more likely
        int diff = Math.Abs((int)to - (int)from);
        
        if (diff == 0) return 0.4f;
        if (diff == 1) return 0.3f;
        if (diff == 2) return 0.2f;
        if (diff == 3) return 0.1f;
        return 0f;
    }
    
    /// <summary>
    /// Record player performance event
    /// </summary>
    public void RecordPerformanceEvent(string eventType, float value)
    {
        _playerPerformanceMetrics[eventType] = value;
        
        switch (eventType)
        {
            case "Death":
                _consecutiveDeaths++;
                _consecutiveSuccesses = 0;
                _performanceScore -= 0.1f;
                break;
            case "Checkpoint":
                _consecutiveSuccesses++;
                _consecutiveDeaths = 0;
                _performanceScore += 0.05f;
                break;
            case "RoomComplete":
                _consecutiveSuccesses++;
                _performanceScore += 0.1f;
                break;
            case "Strawberry":
                _performanceScore += 0.02f;
                break;
            case "BossDefeat":
                _performanceScore += 0.15f;
                break;
        }
        
        // Clamp performance score
        _performanceScore = Math.Clamp(_performanceScore, 0f, 1f);
        
        // Check if difficulty adjustment is needed
        if (ShouldAdjustDifficulty())
        {
            AdjustDifficulty();
        }
    }
    
    /// <summary>
    /// Check if difficulty should be adjusted
    /// </summary>
    private bool ShouldAdjustDifficulty()
    {
        // Lower difficulty if player is struggling
        if (_consecutiveDeaths >= 3 && _performanceScore < 0.3f)
            return true;
        
        // Raise difficulty if player is excelling
        if (_consecutiveSuccesses >= 5 && _performanceScore > 0.7f)
            return true;
        
        return false;
    }
    
    /// <summary>
    /// Adjust difficulty based on player performance
    /// </summary>
    private void AdjustDifficulty()
    {
        if (_performanceScore < 0.3f)
        {
            // Lower difficulty
            var newDifficulty = _difficultyChain.GetNextState();
            if ((int)newDifficulty < (int)_currentDifficulty)
            {
                _currentDifficulty = newDifficulty;
                Logger.Log(LogLevel.Info, LogTag, $"Difficulty lowered to {_currentDifficulty}");
            }
        }
        else if (_performanceScore > 0.7f)
        {
            // Raise difficulty
            var newDifficulty = _difficultyChain.GetNextState();
            if ((int)newDifficulty > (int)_currentDifficulty)
            {
                _currentDifficulty = newDifficulty;
                Logger.Log(LogLevel.Info, LogTag, $"Difficulty raised to {_currentDifficulty}");
            }
        }
        
        // Reset counters
        _consecutiveDeaths = 0;
        _consecutiveSuccesses = 0;
    }
    
    /// <summary>
    /// Get difficulty adjustment for a specific aspect
    /// </summary>
    public DifficultyAdjustment GetDifficultyAdjustment(string aspect)
    {
        if (!_difficultyAdjustments.ContainsKey(aspect))
        {
            _difficultyAdjustments[aspect] = CalculateAdjustment(aspect);
        }
        
        return _difficultyAdjustments[aspect];
    }
    
    /// <summary>
    /// Calculate difficulty adjustment for an aspect
    /// </summary>
    private DifficultyAdjustment CalculateAdjustment(string aspect)
    {
        float difficultyMultiplier = _currentDifficulty switch
        {
            DifficultyTier.Easy => 0.7f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.3f,
            DifficultyTier.Expert => 1.6f,
            DifficultyTier.Master => 2.0f,
            _ => 1.0f
        };
        
        // Adjust based on performance
        float performanceModifier = Microsoft.Xna.Framework.MathHelper.Lerp(0.8f, 1.2f, _performanceScore);
        float finalMultiplier = difficultyMultiplier * performanceModifier;
        
        return aspect switch
        {
            "EnemyHealth" => new DifficultyAdjustment
            {
                Multiplier = finalMultiplier,
                Description = "Enemy health adjusted"
            },
            "EnemySpeed" => new DifficultyAdjustment
            {
                Multiplier = finalMultiplier * 0.8f, // Speed doesn't scale as much
                Description = "Enemy speed adjusted"
            },
            "EnemyDamage" => new DifficultyAdjustment
            {
                Multiplier = finalMultiplier,
                Description = "Enemy damage adjusted"
            },
            "PlatformSpeed" => new DifficultyAdjustment
            {
                Multiplier = finalMultiplier * 0.7f,
                Description = "Platform speed adjusted"
            },
            "HazardDamage" => new DifficultyAdjustment
            {
                Multiplier = finalMultiplier,
                Description = "Hazard damage adjusted"
            },
            _ => new DifficultyAdjustment
            {
                Multiplier = finalMultiplier,
                Description = "General adjustment"
            }
        };
    }
    
    /// <summary>
    /// Force a specific difficulty level
    /// </summary>
    public void SetDifficulty(DifficultyTier difficulty)
    {
        _currentDifficulty = difficulty;
        _difficultyChain.Reset(difficulty);
        _difficultyAdjustments.Clear();
        
        Logger.Log(LogLevel.Info, LogTag, $"Difficulty set to {_currentDifficulty}");
    }
    
    /// <summary>
    /// Reset the difficulty balancer
    /// </summary>
    public void Reset(DifficultyTier startDifficulty)
    {
        _currentDifficulty = startDifficulty;
        _performanceScore = 0.5f;
        _consecutiveDeaths = 0;
        _consecutiveSuccesses = 0;
        _playerPerformanceMetrics.Clear();
        _difficultyAdjustments.Clear();
        _difficultyChain.Reset(startDifficulty);
        
        Logger.Log(LogLevel.Info, LogTag, "Difficulty balancer reset");
    }
    
    /// <summary>
    /// Get statistics about difficulty balancing
    /// </summary>
    public DifficultyBalancingStats GetStats()
    {
        return new DifficultyBalancingStats
        {
            CurrentDifficulty = _currentDifficulty,
            PerformanceScore = _performanceScore,
            ConsecutiveDeaths = _consecutiveDeaths,
            ConsecutiveSuccesses = _consecutiveSuccesses,
            TotalPerformanceEvents = _playerPerformanceMetrics.Count,
            DifficultyChainStats = _difficultyChain.GetStats()
        };
    }
}

/// <summary>
/// Difficulty adjustment for specific game aspects
/// </summary>
public class DifficultyAdjustment
{
    public float Multiplier { get; set; } = 1f;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Statistics for difficulty balancing
/// </summary>
public class DifficultyBalancingStats
{
    public DifficultyTier CurrentDifficulty { get; set; }
    public float PerformanceScore { get; set; }
    public int ConsecutiveDeaths { get; set; }
    public int ConsecutiveSuccesses { get; set; }
    public int TotalPerformanceEvents { get; set; }
    public MarkovChainStats DifficultyChainStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"Difficulty: {CurrentDifficulty}, Performance: {PerformanceScore:F2}, " +
               $"Deaths: {ConsecutiveDeaths}, Successes: {ConsecutiveSuccesses}";
    }
}
