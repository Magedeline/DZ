using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.Achievements;

/// <summary>
/// Procedural achievement system using Markov chains for unique challenges
/// </summary>
public class ProceduralAchievementSystem
{
    private const string LogTag = "ProceduralAchievementSystem";
    
    private readonly MarkovChain<string> _achievementTypeChain;
    private readonly MarkovChain<string> _difficultyChain;
    private readonly Random _random;
    private readonly int _seed;
    
    private readonly Dictionary<string, GeneratedAchievement> _generatedAchievements;
    private int _achievementCounter;
    
    public ProceduralAchievementSystem(int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _achievementTypeChain = new MarkovChain<string>("AchievementTypes", seed);
        _difficultyChain = MarkovChainManager.CreateDifficultyChain("AchievementDifficulty", seed);
        
        _generatedAchievements = new Dictionary<string, GeneratedAchievement>();
        _achievementCounter = 0;
        
        InitializeChains();
    }
    
    private void InitializeChains()
    {
        var types = new[] { "Combat", "Exploration", "Collection", "Speed", "Stealth", "Survival" };
        
        foreach (var from in types)
            foreach (var to in types)
                _achievementTypeChain.AddTransition(from, to, 0.12f);
        
        _achievementTypeChain.Initialize("Exploration");
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized achievement generation chains");
    }
    
    public GeneratedAchievement GenerateAchievement(string playerPlayStyle)
    {
        var type = _achievementTypeChain.GetNextState() ?? "Exploration";
        var difficultyStr = _difficultyChain.GetNextState() ?? "Normal";
        var difficulty = Enum.TryParse<DifficultyTier>(difficultyStr, out var parsed) ? parsed : DifficultyTier.Normal;

        return new GeneratedAchievement
        {
            Id = $"Achievement_{_achievementCounter++}",
            Name = GenerateAchievementName(type, difficulty),
            Description = GenerateAchievementDescription(type, difficulty),
            Type = type,
            Difficulty = difficulty,
            Requirements = GenerateRequirements(type, difficulty),
            Reward = GenerateReward(difficulty),
            Icon = GenerateIcon(type)
        };
    }
    
    private string GenerateAchievementName(string type, DifficultyTier difficulty)
    {
        var prefixes = new[] { "The", "Master", "Expert", "Legendary", "Ultimate" };
        var suffixes = new[] { "Hunter", "Explorer", "Collector", "Warrior", "Survivor" };
        
        string prefix = difficulty >= DifficultyTier.Hard ? prefixes[(int)difficulty - 1] : "";
        string suffix = type switch
        {
            "Combat" => "Warrior",
            "Exploration" => "Explorer",
            "Collection" => "Collector",
            "Speed" => "Speedster",
            "Stealth" => "Ghost",
            "Survival" => "Survivor",
            _ => "Achiever"
        };
        
        return string.IsNullOrEmpty(prefix) ? suffix : $"{prefix} {suffix}";
    }
    
    private string GenerateAchievementDescription(string type, DifficultyTier difficulty)
    {
        var descriptions = type switch
        {
            "Combat" => new[] { "Defeat enemies without taking damage", "Complete a fight in under 30 seconds", "Defeat 10 enemies in a row" },
            "Exploration" => new[] { "Find all hidden areas", "Explore every room", "Discover 5 secrets" },
            "Collection" => new[] { "Collect all items in a level", "Gather 100 strawberries", "Complete collection without missing any" },
            "Speed" => new[] { "Complete level in under 2 minutes", "Beat a boss in under 1 minute", "Reach the end in record time" },
            "Stealth" => new[] { "Avoid detection entirely", "Complete without triggering alarms", "Sneak past 5 enemies" },
            "Survival" => new[] { "Survive for 5 minutes without healing", "Complete with 1 HP remaining", "Survive 10 boss attacks" },
            _ => new[] { "Complete the challenge", "Achieve the goal", "Master the task" }
        };
        
        return descriptions[_random.Next(descriptions.Length)];
    }
    
    private List<string> GenerateRequirements(string type, DifficultyTier difficulty)
    {
        var requirements = new List<string>();
        
        int baseCount = (int)difficulty * 10;
        
        switch (type)
        {
            case "Combat":
                requirements.Add($"Defeat {baseCount} enemies");
                if (difficulty >= DifficultyTier.Hard)
                    requirements.Add("Without taking damage");
                break;
            case "Exploration":
                requirements.Add($"Discover {baseCount} areas");
                if (difficulty >= DifficultyTier.Hard)
                    requirements.Add("Without using checkpoints");
                break;
            case "Collection":
                requirements.Add($"Collect {baseCount} items");
                if (difficulty >= DifficultyTier.Expert)
                    requirements.Add("Within time limit");
                break;
            case "Speed":
                requirements.Add($"Complete in {300 - (int)difficulty * 20} seconds");
                break;
            case "Stealth":
                requirements.Add($"Avoid detection {baseCount} times");
                break;
            case "Survival":
                requirements.Add($"Survive {baseCount} seconds");
                if (difficulty >= DifficultyTier.Hard)
                    requirements.Add("Without healing");
                break;
        }
        
        return requirements;
    }
    
    private string GenerateReward(DifficultyTier difficulty)
    {
        return difficulty switch
        {
            DifficultyTier.Easy => "100 points",
            DifficultyTier.Normal => "250 points",
            DifficultyTier.Hard => "500 points",
            DifficultyTier.Expert => "1000 points",
            DifficultyTier.Master => "2500 points",
            _ => "100 points"
        };
    }
    
    private string GenerateIcon(string type)
    {
        return type switch
        {
            "Combat" => "sword_icon",
            "Exploration" => "compass_icon",
            "Collection" => "gem_icon",
            "Speed" => "clock_icon",
            "Stealth" => "shadow_icon",
            "Survival" => "heart_icon",
            _ => "star_icon"
        };
    }
    
    public AchievementStats GetStats()
    {
        return new AchievementStats
        {
            TotalAchievementsGenerated = _generatedAchievements.Count,
            AchievementsByType = _generatedAchievements.GroupBy(a => a.Value.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            AchievementTypeChainStats = _achievementTypeChain.GetStats(),
            DifficultyChainStats = _difficultyChain.GetStats()
        };
    }
}

public class GeneratedAchievement
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DifficultyTier Difficulty { get; set; }
    public List<string> Requirements { get; set; } = new();
    public string Reward { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public class AchievementStats
{
    public int TotalAchievementsGenerated { get; set; }
    public Dictionary<string, int> AchievementsByType { get; set; } = new();
    public MarkovChainStats AchievementTypeChainStats { get; set; } = new();
    public MarkovChainStats DifficultyChainStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"Total Achievements: {TotalAchievementsGenerated}, " +
               $"Types: {string.Join(", ", AchievementsByType.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
    }
}
