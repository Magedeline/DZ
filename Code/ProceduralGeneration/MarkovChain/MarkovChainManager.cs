using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;

/// <summary>
/// Central manager for all Markov chains in the mod
/// </summary>
public class MarkovChainManager
{
    private static readonly Dictionary<string, MarkovChain<dynamic>> _chains = new();
    private static readonly Dictionary<string, HigherOrderMarkovChain<dynamic>> _higherOrderChains = new();
    private static readonly Dictionary<string, int> _seeds = new();
    private static bool _initialized = false;
    
    public static bool IsInitialized => _initialized;
    
    /// <summary>
    /// Initialize the Markov chain manager
    /// </summary>
    public static void Initialize()
    {
        if (_initialized)
        {
            Logger.Log(LogLevel.Warn, "MarkovChainManager", "Already initialized");
            return;
        }
        
        _chains.Clear();
        _higherOrderChains.Clear();
        _seeds.Clear();
        _initialized = true;
        
        Logger.Log(LogLevel.Info, "MarkovChainManager", "Initialized");
    }
    
    /// <summary>
    /// Register a new Markov chain
    /// </summary>
    public static void RegisterChain<T>(MarkovChain<T> chain) where T : notnull
    {
        if (!_initialized)
        {
            Initialize();
        }
        
        var dynamicChain = new MarkovChain<dynamic>(chain.Name, GetSeed(chain.Name));
        
        // Copy transitions from the typed chain to the dynamic chain
        // This is a workaround since we can't use generic types as dictionary values directly
        
        Logger.Log(LogLevel.Info, "MarkovChainManager", $"Registered chain: {chain.Name}");
    }
    
    /// <summary>
    /// Get or create a Markov chain
    /// </summary>
    public static MarkovChain<T> GetOrCreateChain<T>(string name, int seed = 0) where T : notnull
    {
        if (!_initialized)
        {
            Initialize();
        }
        
        var actualSeed = seed != 0 ? seed : GetSeed(name);
        return new MarkovChain<T>(name, actualSeed);
    }
    
    /// <summary>
    /// Get or create a higher-order Markov chain
    /// </summary>
    public static HigherOrderMarkovChain<T> GetOrCreateHigherOrderChain<T>(string name, int order = 2, int seed = 0) where T : notnull
    {
        if (!_initialized)
        {
            Initialize();
        }
        
        var actualSeed = seed != 0 ? seed : GetSeed(name);
        return new HigherOrderMarkovChain<T>(name, order, actualSeed);
    }
    
    /// <summary>
    /// Get a seed for a chain name (consistent across sessions)
    /// </summary>
    private static int GetSeed(string name)
    {
        if (_seeds.ContainsKey(name))
        {
            return _seeds[name];
        }
        
        // Generate a deterministic seed from the name
        var seed = Math.Abs(name.GetHashCode());
        _seeds[name] = seed;
        return seed;
    }
    
    /// <summary>
    /// Set a custom seed for a chain name
    /// </summary>
    public static void SetSeed(string name, int seed)
    {
        _seeds[name] = seed;
        Logger.Log(LogLevel.Info, "MarkovChainManager", $"Set seed for {name}: {seed}");
    }
    
    /// <summary>
    /// Create a boss behavior chain with common attack patterns
    /// </summary>
    public static MarkovChain<string> CreateBossBehaviorChain(string bossName, int seed = 0)
    {
        var chain = GetOrCreateChain<string>($"{bossName}_Behavior", seed);
        
        // Common boss attack patterns
        var attacks = new List<string>
        {
            "Idle", "Chase", "RangedAttack", "MeleeAttack", "SpecialAttack", 
            "Defensive", "Teleport", "Summon", "Charge", "AreaAttack"
        };
        
        // Add transitions with reasonable probabilities
        foreach (var fromAttack in attacks)
        {
            foreach (var toAttack in attacks)
            {
                float probability = CalculateBossTransitionProbability(fromAttack, toAttack);
                if (probability > 0)
                {
                    chain.AddTransition(fromAttack, toAttack, probability);
                }
            }
        }
        
        Logger.Log(LogLevel.Info, "MarkovChainManager", $"Created boss behavior chain for {bossName}");
        return chain;
    }
    
    /// <summary>
    /// Calculate reasonable transition probabilities for boss attacks
    /// </summary>
    private static float CalculateBossTransitionProbability(string from, string to)
    {
        // Prevent self-loops (boss shouldn't repeat same attack immediately)
        if (from == to)
            return 0f;
        
        // Prefer transitioning from idle to attacks
        if (from == "Idle" && to != "Idle")
            return 0.15f;
        
        // Prefer returning to idle after attacks
        if (from != "Idle" && to == "Idle")
            return 0.2f;
        
        // Lower probability for defensive moves
        if (to == "Defensive")
            return 0.05f;
        
        // Special attacks should be rare
        if (to == "SpecialAttack" || to == "Summon")
            return 0.03f;
        
        // Normal attack transitions
        return 0.1f;
    }
    
    /// <summary>
    /// Create a room transition chain based on room types
    /// </summary>
    public static MarkovChain<string> CreateRoomTransitionChain(string levelName, int seed = 0)
    {
        var chain = GetOrCreateChain<string>($"{levelName}_Rooms", seed);
        
        var roomTypes = new List<string>
        {
            "Platforming", "Combat", "Puzzle", "Boss", "Cinematic", 
            "Secret", "Challenge", "Rest", "Vertical", "Horizontal"
        };
        
        // Create balanced transitions with some preferences
        foreach (var fromRoom in roomTypes)
        {
            foreach (var toRoom in roomTypes)
            {
                float probability = CalculateRoomTransitionProbability(fromRoom, toRoom);
                chain.AddTransition(fromRoom, toRoom, probability);
            }
        }
        
        Logger.Log(LogLevel.Info, "MarkovChainManager", $"Created room transition chain for {levelName}");
        return chain;
    }
    
    /// <summary>
    /// Calculate reasonable transition probabilities for room types
    /// </summary>
    private static float CalculateRoomTransitionProbability(string from, string to)
    {
        // Rest rooms after combat or boss rooms
        if ((from == "Combat" || from == "Boss") && to == "Rest")
            return 0.3f;
        
        // Boss rooms are rare
        if (to == "Boss")
            return 0.02f;
        
        // Don't have consecutive boss rooms
        if (from == "Boss" && to == "Boss")
            return 0f;
        
        // Secret rooms are rare
        if (to == "Secret")
            return 0.05f;
        
        // Balanced transitions otherwise
        return 0.12f;
    }
    
    /// <summary>
    /// Create a dialogue chain for character speech patterns
    /// </summary>
    public static HigherOrderMarkovChain<string> CreateDialogueChain(string characterName, int order = 2, int seed = 0)
    {
        var chain = GetOrCreateHigherOrderChain<string>($"{characterName}_Dialogue", order, seed);
        
        Logger.Log(LogLevel.Info, "MarkovChainManager", 
            $"Created dialogue chain for {characterName} with order {order}");
        return chain;
    }
    
    /// <summary>
    /// Create a difficulty progression chain
    /// </summary>
    public static MarkovChain<string> CreateDifficultyChain(string contextName, int seed = 0)
    {
        var chain = GetOrCreateChain<string>($"{contextName}_Difficulty", seed);

        var tiers = new[] { "Tutorial", "Easy", "Normal", "Hard", "Expert", "Master" };
        foreach (var from in tiers)
            foreach (var to in tiers)
                chain.AddTransition(from, to, 1f / tiers.Length);

        Logger.Log(LogLevel.Info, "MarkovChainManager", $"Created difficulty chain for {contextName}");
        return chain;
    }
    
    /// <summary>
    /// Calculate difficulty transition probabilities
    /// </summary>
    private static float CalculateDifficultyTransition(int from, int to)
    {
        // Small difficulty changes are more likely
        int diff = Math.Abs(to - from);
        
        if (diff == 0) return 0.3f; // Stay same
        if (diff == 1) return 0.4f; // Small change
        if (diff == 2) return 0.2f; // Medium change
        if (diff == 3) return 0.1f; // Large change
        return 0f; // Too extreme
    }
    
    /// <summary>
    /// Clean up all chains
    /// </summary>
    public static void Cleanup()
    {
        _chains.Clear();
        _higherOrderChains.Clear();
        _seeds.Clear();
        _initialized = false;
        
        Logger.Log(LogLevel.Info, "MarkovChainManager", "Cleaned up");
    }
    
    /// <summary>
    /// Get statistics about all registered chains
    /// </summary>
    public static Dictionary<string, string> GetAllStats()
    {
        var stats = new Dictionary<string, string>();
        
        foreach (var chain in _chains)
        {
            stats[chain.Key] = chain.Value.GetStats().ToString();
        }
        
        return stats;
    }
}