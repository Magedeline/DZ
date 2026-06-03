using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.PlatformGeneration;

/// <summary>
/// Dynamic platform generator using Verlet physics and Markov chains
/// </summary>
public class DynamicPlatformGenerator
{
    private const string LogTag = "DynamicPlatformGenerator";
    
    private readonly MarkovChain<string> _movementChain;
    private readonly MarkovChain<string> _behaviorChain;
    private readonly Random _random;
    private readonly int _seed;
    
    private readonly List<DynamicPlatform> _generatedPlatforms;
    private int _platformCounter;
    
    public DynamicPlatformGenerator(int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _movementChain = new MarkovChain<string>("PlatformMovements", seed);
        _behaviorChain = new MarkovChain<string>("PlatformBehaviors", seed);
        
        _generatedPlatforms = new List<DynamicPlatform>();
        _platformCounter = 0;
        
        InitializeChains();
    }
    
    /// <summary>
    /// Initialize Markov chains for platform behavior
    /// </summary>
    private void InitializeChains()
    {
        var movements = new[]
        {
            "Horizontal", "Vertical", "Circular", "Linear", "SineWave",
            "Figure8", "Spiral", "Random", "Pendulum", "Elastic"
        };
        
        var behaviors = new[]
        {
            "Stationary", "Moving", "Bouncing", "Breaking", "Appearing",
            "Disappearing", "Stretching", "Rotating", "Scaling", "Pulsing"
        };
        
        // Initialize movement chain
        foreach (var fromMovement in movements)
        {
            foreach (var toMovement in movements)
            {
                float probability = CalculateMovementTransition(fromMovement, toMovement);
                _movementChain.AddTransition(fromMovement, toMovement, probability);
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
        
        _movementChain.Initialize("Horizontal");
        _behaviorChain.Initialize("Moving");
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized dynamic platform chains");
    }
    
    /// <summary>
    /// Calculate movement transition probabilities
    /// </summary>
    private float CalculateMovementTransition(string from, string to)
    {
        // Similar movements often transition
        if ((from.Contains("Linear") || from.Contains("Horizontal")) && 
            (to.Contains("Linear") || to.Contains("Horizontal")))
            return 0.25f;
        
        // Complex movements are rare
        if (to == "Spiral" || to == "Figure8")
            return 0.08f;
        
        // Random movement can transition to anything
        if (from == "Random")
            return 0.15f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        return 0.1f;
    }
    
    /// <summary>
    /// Calculate behavior transition probabilities
    /// </summary>
    private float CalculateBehaviorTransition(string from, string to)
    {
        // Stationary often transitions to moving
        if (from == "Stationary" && to == "Moving")
            return 0.4f;
        
        // Breaking platforms don't transition
        if (from == "Breaking")
            return 0.01f;
        
        // Disappearing often followed by appearing
        if (from == "Disappearing" && to == "Appearing")
            return 0.5f;
        
        // Complex behaviors are rare
        if (to == "Stretching" || to == "Rotating")
            return 0.08f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        return 0.12f;
    }
    
    /// <summary>
    /// Generate a single dynamic platform
    /// </summary>
    public DynamicPlatform GeneratePlatform(
        PlatformType type, 
        Vector2 position, 
        Vector2 size,
        DifficultyTier difficulty)
    {
        // Get movement pattern from Markov chain
        var movement = _movementChain.GetNextState() ?? "Horizontal";
        
        // Get behavior from Markov chain
        var behavior = _behaviorChain.GetNextState() ?? "Moving";
        
        // Calculate physics parameters based on difficulty
        var physicsParams = CalculatePhysicsParameters(movement, behavior, difficulty);
        
        // Generate Verlet physics points
        var verletPoints = GenerateVerletPoints(size, movement, physicsParams);
        
        var platform = new DynamicPlatform
        {
            Id = $"Platform_{_platformCounter++}",
            Type = type,
            Position = position,
            Size = size,
            MovementPattern = movement,
            Behavior = behavior,
            PhysicsParameters = physicsParams,
            VerletPoints = verletPoints,
            Difficulty = difficulty,
            IsOneWay = type == PlatformType.OneWay,
            CanDashThrough = type == PlatformType.DashThrough
        };
        
        _generatedPlatforms.Add(platform);
        
        Logger.Log(LogLevel.Verbose, LogTag, 
            $"Generated platform: {platform.Id} ({movement}, {behavior})");
        
        return platform;
    }
    
    /// <summary>
    /// Generate multiple platforms in a sequence
    /// </summary>
    public List<DynamicPlatform> GeneratePlatformSequence(
        PlatformType type,
        Vector2 startPosition,
        int count,
        Vector2 spacing,
        DifficultyTier difficulty)
    {
        var platforms = new List<DynamicPlatform>();
        
        for (int i = 0; i < count; i++)
        {
            var position = startPosition + (spacing * i);
            
            // Add some variation to position
            position.X += (float)_random.NextDouble() * 20 - 10;
            position.Y += (float)_random.NextDouble() * 20 - 10;
            
            var size = new Vector2(64 + (float)_random.NextDouble() * 32, 16);
            
            var platform = GeneratePlatform(type, position, size, difficulty);
            platforms.Add(platform);
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated platform sequence of {count} platforms");
        
        return platforms;
    }
    
    /// <summary>
    /// Calculate physics parameters based on movement and behavior
    /// </summary>
    private PlatformPhysicsParameters CalculatePhysicsParameters(
        string movement, string behavior, DifficultyTier difficulty)
    {
        float speedMultiplier = difficulty switch
        {
            DifficultyTier.Easy => 0.7f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.3f,
            DifficultyTier.Expert => 1.6f,
            DifficultyTier.Master => 2.0f,
            _ => 1.0f
        };
        
        var parameters = new PlatformPhysicsParameters
        {
            MovementSpeed = 50f * speedMultiplier,
            MovementRange = 100f,
            OscillationFrequency = 1f,
            Damping = 0.98f,
            Stiffness = 0.1f,
            Gravity = Vector2.Zero
        };
        
        // Adjust parameters based on movement type
        switch (movement)
        {
            case "Horizontal":
                parameters.MovementDirection = Vector2.UnitX;
                parameters.MovementRange = 150f;
                break;
            case "Vertical":
                parameters.MovementDirection = Vector2.UnitY;
                parameters.MovementRange = 100f;
                break;
            case "Circular":
                parameters.OscillationFrequency = 2f;
                parameters.MovementRange = 80f;
                break;
            case "SineWave":
                parameters.OscillationFrequency = 3f;
                parameters.MovementRange = 60f;
                break;
            case "Figure8":
                parameters.OscillationFrequency = 1.5f;
                parameters.MovementRange = 70f;
                break;
            case "Pendulum":
                parameters.OscillationFrequency = 2f;
                parameters.MovementRange = 120f;
                parameters.Stiffness = 0.15f;
                break;
            case "Elastic":
                parameters.Stiffness = 0.2f;
                parameters.Damping = 0.95f;
                break;
        }
        
        // Adjust parameters based on behavior
        switch (behavior)
        {
            case "Bouncing":
                parameters.MovementSpeed *= 1.5f;
                parameters.Damping = 0.9f;
                break;
            case "Breaking":
                parameters.Integrity = 3; // Number of hits before breaking
                break;
            case "Appearing":
            case "Disappearing":
                parameters.CycleDuration = 2f + (float)_random.NextDouble();
                break;
            case "Stretching":
                parameters.StretchAmount = 0.3f;
                parameters.StretchSpeed = 1f;
                break;
            case "Rotating":
                parameters.RotationSpeed = (float)Math.PI * 2f; // 360 degrees per second
                break;
            case "Scaling":
                parameters.ScaleRange = new Vector2(0.8f, 1.2f);
                parameters.ScaleSpeed = 0.5f;
                break;
            case "Pulsing":
                parameters.PulseIntensity = 0.2f;
                parameters.PulseSpeed = 2f;
                break;
        }
        
        return parameters;
    }
    
    /// <summary>
    /// Generate Verlet physics points for the platform
    /// </summary>
    private List<VerletPoint> GenerateVerletPoints(
        Vector2 size, string movement, PlatformPhysicsParameters parameters)
    {
        var points = new List<VerletPoint>();
        
        // Create points along the platform's width
        int pointCount = 4;
        float spacing = size.X / (pointCount - 1);
        
        for (int i = 0; i < pointCount; i++)
        {
            var point = new VerletPoint
            {
                Index = i,
                LocalPosition = new Vector2(i * spacing - size.X / 2, 0),
                Velocity = Vector2.Zero,
                Mass = 1f,
                IsPinned = i == 0 || i == pointCount - 1 // Pin endpoints
            };
            
            points.Add(point);
        }
        
        // Create constraints between adjacent points
        for (int i = 0; i < points.Count - 1; i++)
        {
            points[i].ConnectedPoints.Add(points[i + 1]);
            points[i + 1].ConnectedPoints.Add(points[i]);
        }
        
        return points;
    }
    
    /// <summary>
    /// Update a platform's physics using Verlet integration
    /// </summary>
    public void UpdatePlatformPhysics(DynamicPlatform platform, float deltaTime)
    {
        if (platform.VerletPoints == null || platform.VerletPoints.Count == 0)
            return;
        
        var parameters = platform.PhysicsParameters;
        
        foreach (var point in platform.VerletPoints)
        {
            if (point.IsPinned)
                continue;
            
            // Verlet integration
            var acceleration = parameters.Gravity;
            
            // Apply movement pattern forces
            var movementForce = CalculateMovementForce(
                platform.Position + point.LocalPosition,
                platform.MovementPattern,
                parameters);
            
            acceleration += movementForce;
            
            // Update velocity
            point.Velocity += acceleration * deltaTime;
            point.Velocity *= parameters.Damping;
            
            // Update position
            point.LocalPosition += point.Velocity * deltaTime;
            
            // Apply stiffness (spring forces)
            foreach (var connectedPoint in point.ConnectedPoints)
            {
                var direction = connectedPoint.LocalPosition - point.LocalPosition;
                float distance = direction.Length();
                direction.Normalize();
                
                float springForce = distance * parameters.Stiffness;
                point.Velocity += direction * springForce * deltaTime;
            }
        }
    }
    
    /// <summary>
    /// Calculate movement force based on pattern
    /// </summary>
    private Vector2 CalculateMovementForce(
        Vector2 position, string pattern, PlatformPhysicsParameters parameters)
    {
        float time = Engine.RawDeltaTime;
        
        return pattern switch
        {
            "Horizontal" => new Vector2(
                (float)Math.Cos(time * parameters.MovementSpeed * 0.01f) * parameters.MovementRange * 0.1f,
                0),
            "Vertical" => new Vector2(0,
                (float)Math.Cos(time * parameters.MovementSpeed * 0.01f) * parameters.MovementRange * 0.1f),
            "Circular" => new Vector2(
                (float)Math.Cos(time * parameters.OscillationFrequency) * parameters.MovementRange * 0.1f,
                (float)Math.Sin(time * parameters.OscillationFrequency) * parameters.MovementRange * 0.1f),
            "SineWave" => new Vector2(
                parameters.MovementSpeed * 0.1f,
                (float)Math.Sin(position.X * 0.05f + time * parameters.OscillationFrequency) * parameters.MovementRange * 0.1f),
            "Figure8" => new Vector2(
                (float)Math.Sin(time * parameters.OscillationFrequency) * parameters.MovementRange * 0.1f,
                (float)Math.Sin(time * parameters.OscillationFrequency * 2) * parameters.MovementRange * 0.1f),
            "Spiral" => new Vector2(
                (float)Math.Cos(time * parameters.OscillationFrequency) * parameters.MovementRange * 0.1f * (time % 5),
                (float)Math.Sin(time * parameters.OscillationFrequency) * parameters.MovementRange * 0.1f * (time % 5)),
            "Random" => new Vector2(
                (float)(_random.NextDouble() - 0.5) * parameters.MovementRange * 0.05f,
                (float)(_random.NextDouble() - 0.5) * parameters.MovementRange * 0.05f),
            "Pendulum" => new Vector2(
                (float)Math.Sin(time * parameters.OscillationFrequency) * parameters.MovementRange * 0.1f,
                0),
            _ => Vector2.Zero
        };
    }
    
    /// <summary>
    /// Generate a chain of connected platforms
    /// </summary>
    public List<DynamicPlatform> GeneratePlatformChain(
        PlatformType type,
        Vector2 startPosition,
        int platformCount,
        Vector2 platformSize,
        DifficultyTier difficulty)
    {
        var platforms = new List<DynamicPlatform>();
        
        for (int i = 0; i < platformCount; i++)
        {
            var position = startPosition + new Vector2(i * platformSize.X * 0.8f, 0);
            
            var platform = GeneratePlatform(type, position, platformSize, difficulty);
            
            // Create chain constraints
            if (i > 0)
            {
                platform.ChainParent = platforms[i - 1];
                platforms[i - 1].ChainChild = platform;
            }
            
            platforms.Add(platform);
        }
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated platform chain of {platformCount} platforms");
        
        return platforms;
    }
    
    /// <summary>
    /// Get all generated platforms
    /// </summary>
    public List<DynamicPlatform> GetAllPlatforms()
    {
        return _generatedPlatforms;
    }
    
    /// <summary>
    /// Get platforms by type
    /// </summary>
    public List<DynamicPlatform> GetPlatformsByType(PlatformType type)
    {
        return _generatedPlatforms.Where(p => p.Type == type).ToList();
    }
    
    /// <summary>
    /// Get statistics about generated platforms
    /// </summary>
    public PlatformGenerationStats GetStats()
    {
        return new PlatformGenerationStats
        {
            TotalPlatformsGenerated = _generatedPlatforms.Count,
            PlatformsByType = new Dictionary<PlatformType, int>
            {
                { PlatformType.Normal, _generatedPlatforms.Count(p => p.Type == PlatformType.Normal) },
                { PlatformType.OneWay, _generatedPlatforms.Count(p => p.Type == PlatformType.OneWay) },
                { PlatformType.DashThrough, _generatedPlatforms.Count(p => p.Type == PlatformType.DashThrough) },
                { PlatformType.Crumbling, _generatedPlatforms.Count(p => p.Type == PlatformType.Crumbling) }
            },
            PlatformsByMovement = _generatedPlatforms.GroupBy(p => p.MovementPattern)
                .ToDictionary(g => g.Key, g => g.Count()),
            PlatformsByBehavior = _generatedPlatforms.GroupBy(p => p.Behavior)
                .ToDictionary(g => g.Key, g => g.Count()),
            MovementChainStats = _movementChain.GetStats(),
            BehaviorChainStats = _behaviorChain.GetStats()
        };
    }
}

/// <summary>
/// Platform types
/// </summary>
public enum PlatformType
{
    Normal,
    OneWay,
    DashThrough,
    Crumbling
}

/// <summary>
/// Dynamic platform with physics
/// </summary>
public class DynamicPlatform
{
    public string Id { get; set; } = string.Empty;
    public PlatformType Type { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public string MovementPattern { get; set; } = string.Empty;
    public string Behavior { get; set; } = string.Empty;
    public PlatformPhysicsParameters PhysicsParameters { get; set; } = new();
    public List<VerletPoint>? VerletPoints { get; set; }
    public DifficultyTier Difficulty { get; set; }
    public bool IsOneWay { get; set; }
    public bool CanDashThrough { get; set; }
    
    // Chain properties for connected platforms
    public DynamicPlatform? ChainParent { get; set; }
    public DynamicPlatform? ChainChild { get; set; }
}

/// <summary>
/// Verlet physics point
/// </summary>
public class VerletPoint
{
    public int Index { get; set; }
    public Vector2 LocalPosition { get; set; }
    public Vector2 Velocity { get; set; }
    public float Mass { get; set; } = 1f;
    public bool IsPinned { get; set; }
    public List<VerletPoint> ConnectedPoints { get; set; } = new();
}

/// <summary>
/// Platform physics parameters
/// </summary>
public class PlatformPhysicsParameters
{
    public float MovementSpeed { get; set; } = 50f;
    public Vector2 MovementDirection { get; set; } = Vector2.UnitX;
    public float MovementRange { get; set; } = 100f;
    public float OscillationFrequency { get; set; } = 1f;
    public float Damping { get; set; } = 0.98f;
    public float Stiffness { get; set; } = 0.1f;
    public Vector2 Gravity { get; set; } = Vector2.Zero;
    
    // Behavior-specific parameters
    public int Integrity { get; set; } = -1; // -1 = unbreakable
    public float CycleDuration { get; set; } = 2f;
    public float StretchAmount { get; set; } = 0.2f;
    public float StretchSpeed { get; set; } = 1f;
    public float RotationSpeed { get; set; } = 0f;
    public Vector2 ScaleRange { get; set; } = new(1f, 1f);
    public float ScaleSpeed { get; set; } = 1f;
    public float PulseIntensity { get; set; } = 0.1f;
    public float PulseSpeed { get; set; } = 1f;
}

/// <summary>
/// Statistics for platform generation
/// </summary>
public class PlatformGenerationStats
{
    public int TotalPlatformsGenerated { get; set; }
    public Dictionary<PlatformType, int> PlatformsByType { get; set; } = new();
    public Dictionary<string, int> PlatformsByMovement { get; set; } = new();
    public Dictionary<string, int> PlatformsByBehavior { get; set; } = new();
    public MarkovChainStats MovementChainStats { get; set; } = new();
    public MarkovChainStats BehaviorChainStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"Total Platforms: {TotalPlatformsGenerated}, " +
               $"Types: {string.Join(", ", PlatformsByType.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
    }
}
