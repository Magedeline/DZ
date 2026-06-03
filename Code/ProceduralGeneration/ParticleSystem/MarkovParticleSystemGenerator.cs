using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.ParticleSystem;

/// <summary>
/// Markov-based particle system generator for procedural visual effects
/// </summary>
public class MarkovParticleSystemGenerator
{
    private const string LogTag = "MarkovParticleSystemGenerator";
    
    private readonly MarkovChain<string> _emissionPatternChain;
    private readonly MarkovChain<string> _behaviorChain;
    private readonly MarkovChain<string> _colorChain;
    private readonly Random _random;
    private readonly int _seed;
    
    private readonly Dictionary<string, GeneratedParticleSystem> _generatedSystems;
    private int _systemCounter;
    
    public MarkovParticleSystemGenerator(int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _emissionPatternChain = new MarkovChain<string>("EmissionPatterns", seed);
        _behaviorChain = new MarkovChain<string>("ParticleBehaviors", seed);
        _colorChain = new MarkovChain<string>("ColorSchemes", seed);
        
        _generatedSystems = new Dictionary<string, GeneratedParticleSystem>();
        _systemCounter = 0;
        
        InitializeChains();
    }
    
    /// <summary>
    /// Initialize Markov chains for particle generation
    /// </summary>
    private void InitializeChains()
    {
        var emissionPatterns = new[]
        {
            "Burst", "Continuous", "Pulse", "Wave", "Spiral",
            "Explosion", "Implosion", "Fountain", "Rain", "Swirl"
        };
        
        var behaviors = new[]
        {
            "Fade", "Shrink", "Grow", "Rotate", "Oscillate",
            "Gravity", "Float", "Chase", "Flee", "Orbit"
        };
        
        var colorSchemes = new[]
        {
            "Fire", "Ice", "Electric", "Nature", "Cosmic",
            "Toxic", "Holy", "Shadow", "Rainbow", "Custom"
        };
        
        // Initialize emission pattern chain
        foreach (var fromPattern in emissionPatterns)
        {
            foreach (var toPattern in emissionPatterns)
            {
                float probability = CalculateEmissionTransition(fromPattern, toPattern);
                _emissionPatternChain.AddTransition(fromPattern, toPattern, probability);
            }
        }
        
        // Initialize behavior chain
        foreach (var fromBehavior in behaviors)
        {
            foreach (var toBehavior in behaviors)
            {
                float probability = CalculateBehaviorTransition(fromBehavior, toBehavior);
                _behaviorChain.AddTransition(fromBehavior, toBehavior, probability);
            }
        }
        
        // Initialize color chain
        foreach (var fromColor in colorSchemes)
        {
            foreach (var toColor in colorSchemes)
            {
                float probability = 0.1f; // Balanced color transitions
                if (fromColor == toColor) probability = 0.05f;
                _colorChain.AddTransition(fromColor, toColor, probability);
            }
        }
        
        _emissionPatternChain.Initialize("Burst");
        _behaviorChain.Initialize("Fade");
        _colorChain.Initialize("Fire");
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized particle system chains");
    }
    
    /// <summary>
    /// Calculate emission pattern transition probabilities
    /// </summary>
    private float CalculateEmissionTransition(string from, string to)
    {
        // Explosive patterns often followed by calmer patterns
        if ((from == "Explosion" || from == "Burst") && to == "Continuous")
            return 0.3f;
        
        // Pattern progression
        if (from == "Continuous" && to == "Pulse")
            return 0.25f;
        
        // Complex patterns are rare
        if (to == "Spiral" || to == "Swirl")
            return 0.08f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        return 0.12f;
    }
    
    /// <summary>
    /// Calculate behavior transition probabilities
    /// </summary>
    private float CalculateBehaviorTransition(string from, string to)
    {
        // Fade is common and transitions well
        if (from == "Fade")
            return 0.15f;
        
        // Physics-based behaviors
        if ((from == "Gravity" || from == "Float") && (to == "Gravity" || to == "Float"))
            return 0.2f;
        
        // Complex behaviors are rare
        if (to == "Chase" || to == "Flee" || to == "Orbit")
            return 0.08f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        return 0.1f;
    }
    
    /// <summary>
    /// Generate a particle system for a boss attack
    /// </summary>
    public GeneratedParticleSystem GenerateBossAttackSystem(string attackType)
    {
        var emissionPattern = _emissionPatternChain.GetNextState() ?? "Burst";
        var behavior = _behaviorChain.GetNextState() ?? "Fade";
        var colorScheme = _colorChain.GetNextState() ?? "Fire";
        
        var system = new GeneratedParticleSystem
        {
            Id = $"ParticleSystem_{_systemCounter++}",
            Name = $"{attackType}_Particles",
            EmissionPattern = emissionPattern,
            Behavior = behavior,
            ColorScheme = colorScheme,
            ParticleCount = 50 + _random.Next(50),
            Lifetime = 1f + (float)_random.NextDouble() * 2f,
            Size = new Vector2(4 + (float)_random.NextDouble() * 8, 4 + (float)_random.NextDouble() * 8),
            EmissionRate = 10 + _random.Next(20)
        };
        
        // Adjust parameters based on attack type
        if (attackType.Contains("Special", StringComparison.OrdinalIgnoreCase))
        {
            system.ParticleCount += 50;
            system.Lifetime += 1f;
            system.Size *= 1.5f;
        }
        
        if (attackType.Contains("Beam", StringComparison.OrdinalIgnoreCase))
        {
            emissionPattern = "Continuous";
            system.EmissionRate += 30;
        }
        
        if (attackType.Contains("Explosion", StringComparison.OrdinalIgnoreCase))
        {
            emissionPattern = "Explosion";
            system.ParticleCount += 100;
        }
        
        system.ParticleParameters = GenerateParticleParameters(emissionPattern, behavior, colorScheme);
        
        _generatedSystems[system.Id] = system;
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated particle system: {system.Name} ({emissionPattern}, {behavior})");
        
        return system;
    }
    
    /// <summary>
    /// Generate a particle system for environmental effects
    /// </summary>
    public GeneratedParticleSystem GenerateEnvironmentalSystem(string environmentType)
    {
        var emissionPattern = environmentType switch
        {
            "Waterfall" => "Continuous",
            "Fire" => "Burst",
            "Snow" => "Continuous",
            "Ash" => "Rain",
            "Magic" => "Swirl",
            _ => "Continuous"
        };
        
        var behavior = environmentType switch
        {
            "Waterfall" => "Gravity",
            "Fire" => "Fade",
            "Snow" => "Float",
            "Ash" => "Gravity",
            "Magic" => "Orbit",
            _ => "Fade"
        };
        
        var colorScheme = environmentType switch
        {
            "Waterfall" => "Ice",
            "Fire" => "Fire",
            "Snow" => "Ice",
            "Ash" => "Shadow",
            "Magic" => "Cosmic",
            _ => "Custom"
        };
        
        var system = new GeneratedParticleSystem
        {
            Id = $"ParticleSystem_{_systemCounter++}",
            Name = $"{environmentType}_Particles",
            EmissionPattern = emissionPattern,
            Behavior = behavior,
            ColorScheme = colorScheme,
            ParticleCount = 100 + _random.Next(100),
            Lifetime = 2f + (float)_random.NextDouble() * 3f,
            Size = new Vector2(2 + (float)_random.NextDouble() * 4, 2 + (float)_random.NextDouble() * 4),
            EmissionRate = 5 + _random.Next(10)
        };
        
        system.ParticleParameters = GenerateParticleParameters(emissionPattern, behavior, colorScheme);
        
        _generatedSystems[system.Id] = system;
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated environmental particle system: {system.Name}");
        
        return system;
    }
    
    /// <summary>
    /// Generate particle parameters based on pattern, behavior, and color
    /// </summary>
    private ParticleParameters GenerateParticleParameters(string pattern, string behavior, string colorScheme)
    {
        var parameters = new ParticleParameters
        {
            Velocity = GenerateVelocity(pattern),
            Acceleration = GenerateAcceleration(behavior),
            StartColor = GetStartColor(colorScheme),
            EndColor = GetEndColor(colorScheme),
            RotationSpeed = (float)(_random.NextDouble() - 0.5) * 5f,
            StartSize = 1f + (float)_random.NextDouble() * 0.5f,
            EndSize = 0.1f + (float)_random.NextDouble() * 0.3f,
            DragCoefficient = 0.98f + (float)_random.NextDouble() * 0.02f
        };
        
        return parameters;
    }
    
    /// <summary>
    /// Generate initial velocity based on emission pattern
    /// </summary>
    private Vector2 GenerateVelocity(string pattern)
    {
        float speed = 50f + (float)_random.NextDouble() * 100f;
        float angle = (float)_random.NextDouble() * MathHelper.TwoPi;
        
        return pattern switch
        {
            "Burst" => new Vector2(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed),
            "Explosion" => new Vector2(
                (float)Math.Cos(angle) * speed * 2f,
                (float)Math.Sin(angle) * speed * 2f),
            "Implosion" => new Vector2(
                -(float)Math.Cos(angle) * speed,
                -(float)Math.Sin(angle) * speed),
            "Fountain" => new Vector2(
                (float)(_random.NextDouble() - 0.5) * speed * 0.5f,
                -speed),
            "Rain" => new Vector2(
                (float)(_random.NextDouble() - 0.5) * speed * 0.2f,
                speed * 0.5f),
            _ => new Vector2(
                (float)Math.Cos(angle) * speed,
                (float)Math.Sin(angle) * speed)
        };
    }
    
    /// <summary>
    /// Generate acceleration based on behavior
    /// </summary>
    private Vector2 GenerateAcceleration(string behavior)
    {
        return behavior switch
        {
            "Gravity" => new Vector2(0, 98f),
            "Float" => new Vector2(0, -20f),
            "Chase" => new Vector2(50f, 0), // Will be directed toward target
            "Flee" => new Vector2(-50f, 0), // Will be directed away from target
            "Orbit" => new Vector2(0, 0), // Handled in update
            _ => Vector2.Zero
        };
    }
    
    /// <summary>
    /// Get start color based on color scheme
    /// </summary>
    private Color GetStartColor(string colorScheme)
    {
        return colorScheme switch
        {
            "Fire" => new Color(255, 200, 100),
            "Ice" => new Color(150, 200, 255),
            "Electric" => new Color(255, 255, 100),
            "Nature" => new Color(100, 255, 150),
            "Cosmic" => new Color(200, 100, 255),
            "Toxic" => new Color(150, 255, 50),
            "Holy" => new Color(255, 255, 200),
            "Shadow" => new Color(100, 100, 150),
            "Rainbow" => new Color(
                (byte)_random.Next(256),
                (byte)_random.Next(256),
                (byte)_random.Next(256)),
            _ => Color.White
        };
    }
    
    /// <summary>
    /// Get end color based on color scheme
    /// </summary>
    private Color GetEndColor(string colorScheme)
    {
        var startColor = GetStartColor(colorScheme);
        
        return colorScheme switch
        {
            "Fire" => new Color(255, 100, 50, 0),
            "Ice" => new Color(200, 230, 255, 0),
            "Electric" => new Color(200, 200, 255, 0),
            "Nature" => new Color(150, 200, 100, 0),
            _ => new Color(startColor.R, startColor.G, startColor.B, 0)
        };
    }
    
    /// <summary>
    /// Generate a sequence of particle systems using Markov chains
    /// </summary>
    public List<GeneratedParticleSystem> GenerateSystemSequence(string startPattern, int count)
    {
        _emissionPatternChain.Initialize(startPattern);
        var sequence = new List<GeneratedParticleSystem>();
        
        for (int i = 0; i < count; i++)
        {
            var pattern = _emissionPatternChain.GetNextState() ?? "Burst";
            var behavior = _behaviorChain.GetNextState() ?? "Fade";
            var color = _colorChain.GetNextState() ?? "Fire";
            
            var system = new GeneratedParticleSystem
            {
                Id = $"ParticleSystem_{_systemCounter++}",
                Name = $"Sequence_Particle_{i}",
                EmissionPattern = pattern,
                Behavior = behavior,
                ColorScheme = color,
                ParticleCount = 50 + _random.Next(50),
                Lifetime = 1f + (float)_random.NextDouble(),
                Size = new Vector2(4, 4),
                EmissionRate = 10 + _random.Next(10)
            };
            
            system.ParticleParameters = GenerateParticleParameters(pattern, behavior, color);
            sequence.Add(system);
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated particle system sequence of {count} systems");
        
        return sequence;
    }
    
    /// <summary>
    /// Get a previously generated particle system
    /// </summary>
    public GeneratedParticleSystem? GetSystem(string systemId)
    {
        return _generatedSystems.TryGetValue(systemId, out var system) ? system : null;
    }
    
    /// <summary>
    /// Get all generated particle systems
    /// </summary>
    public List<GeneratedParticleSystem> GetAllSystems()
    {
        return _generatedSystems.Values.ToList();
    }
    
    /// <summary>
    /// Get statistics about generated particle systems
    /// </summary>
    public ParticleSystemStats GetStats()
    {
        return new ParticleSystemStats
        {
            TotalSystemsGenerated = _generatedSystems.Count,
            SystemsByPattern = _generatedSystems.GroupBy(s => s.Value.EmissionPattern)
                .ToDictionary(g => g.Key, g => g.Count()),
            SystemsByBehavior = _generatedSystems.GroupBy(s => s.Value.Behavior)
                .ToDictionary(g => g.Key, g => g.Count()),
            SystemsByColor = _generatedSystems.GroupBy(s => s.Value.ColorScheme)
                .ToDictionary(g => g.Key, g => g.Count()),
            EmissionPatternChainStats = _emissionPatternChain.GetStats(),
            BehaviorChainStats = _behaviorChain.GetStats(),
            ColorChainStats = _colorChain.GetStats()
        };
    }
}

/// <summary>
/// Generated particle system
/// </summary>
public class GeneratedParticleSystem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string EmissionPattern { get; set; } = string.Empty;
    public string Behavior { get; set; } = string.Empty;
    public string ColorScheme { get; set; } = string.Empty;
    public int ParticleCount { get; set; }
    public float Lifetime { get; set; }
    public Vector2 Size { get; set; }
    public int EmissionRate { get; set; }
    public ParticleParameters ParticleParameters { get; set; } = new();
}

/// <summary>
/// Particle parameters
/// </summary>
public class ParticleParameters
{
    public Vector2 Velocity { get; set; }
    public Vector2 Acceleration { get; set; }
    public Color StartColor { get; set; }
    public Color EndColor { get; set; }
    public float RotationSpeed { get; set; }
    public float StartSize { get; set; }
    public float EndSize { get; set; }
    public float DragCoefficient { get; set; }
}

/// <summary>
/// Statistics for particle system generation
/// </summary>
public class ParticleSystemStats
{
    public int TotalSystemsGenerated { get; set; }
    public Dictionary<string, int> SystemsByPattern { get; set; } = new();
    public Dictionary<string, int> SystemsByBehavior { get; set; } = new();
    public Dictionary<string, int> SystemsByColor { get; set; } = new();
    public MarkovChainStats EmissionPatternChainStats { get; set; } = new();
    public MarkovChainStats BehaviorChainStats { get; set; } = new();
    public MarkovChainStats ColorChainStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"Total Systems: {TotalSystemsGenerated}, " +
               $"Patterns: {string.Join(", ", SystemsByPattern.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
    }
}