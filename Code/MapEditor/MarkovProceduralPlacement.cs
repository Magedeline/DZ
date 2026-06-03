using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE;

/// <summary>
/// Markov-enhanced procedural placement system for intelligent entity/hazard/platform placement
/// </summary>
public static class MarkovProceduralPlacement
{
    private const string LogTag = "MarkovProceduralPlacement";
    
    private static MarkovChain<string>? _entityTransitionChain;
    private static MarkovChain<string>? _hazardTransitionChain;
    private static MarkovChain<string>? _roomTransitionChain;
    private static bool _initialized = false;
    
    /// <summary>
    /// Initialize the Markov-enhanced placement system
    /// </summary>
    public static void Initialize(int seed = 0)
    {
        if (_initialized)
        {
            Logger.Log(LogLevel.Warn, LogTag, "Already initialized");
            return;
        }
        
        MarkovChainManager.Initialize();
        
        // Create specialized chains for placement
        _entityTransitionChain = MarkovChainManager.GetOrCreateChain<string>("EntityTransitions", seed);
        _hazardTransitionChain = MarkovChainManager.GetOrCreateChain<string>("HazardTransitions", seed);
        _roomTransitionChain = MarkovChainManager.GetOrCreateChain<string>("RoomTransitions", seed);
        
        // Initialize with common entity type transitions
        InitializeEntityTransitions();
        InitializeHazardTransitions();
        InitializeRoomTransitions();
        
        _initialized = true;
        Logger.Log(LogLevel.Info, LogTag, "Initialized Markov-enhanced placement system");
    }
    
    /// <summary>
    /// Initialize entity type transitions based on game design patterns
    /// </summary>
    private static void InitializeEntityTransitions()
    {
        if (_entityTransitionChain == null) return;
        
        // Common entity types
        var entityTypes = new[]
        {
            "Refill", "Strawberry", "Key", "Spring", "Spinner", 
            "MovingPlatform", "CassetteBlock", "DashBlock", "Trigger"
        };
        
        // Create intelligent transitions
        foreach (var fromType in entityTypes)
        {
            foreach (var toType in entityTypes)
            {
                float probability = CalculateEntityTransitionProbability(fromType, toType);
                _entityTransitionChain.AddTransition(fromType, toType, probability);
            }
        }
    }
    
    /// <summary>
    /// Calculate intelligent entity placement transitions
    /// </summary>
    private static float CalculateEntityTransitionProbability(string from, string to)
    {
        // Refills often follow hazards
        if ((from == "Spinner" || from == "DashBlock") && to == "Refill")
            return 0.25f;
        
        // Strawberries often come after platforms
        if (from == "MovingPlatform" && to == "Strawberry")
            return 0.2f;
        
        // Keys often near doors (triggers)
        if (from == "Key" && to == "Trigger")
            return 0.3f;
        
        // Spring rooms often have strawberries
        if (from == "Spring" && to == "Strawberry")
            return 0.15f;
        
        // Prevent same entity repetition
        if (from == to)
            return 0.05f;
        
        // Default balanced probability
        return 0.1f;
    }
    
    /// <summary>
    /// Initialize hazard type transitions
    /// </summary>
    private static void InitializeHazardTransitions()
    {
        if (_hazardTransitionChain == null) return;
        
        var hazardTypes = new[]
        {
            "Spikes", "Buzzsaw", "Ice", "Fire", "Water", 
            "Lightning", "Lasers", "CrushBlock", "FallingBlock"
        };
        
        foreach (var fromType in hazardTypes)
        {
            foreach (var toType in hazardTypes)
            {
                float probability = CalculateHazardTransitionProbability(fromType, toType);
                _hazardTransitionChain.AddTransition(fromType, toType, probability);
            }
        }
    }
    
    /// <summary>
    /// Calculate intelligent hazard placement transitions
    /// </summary>
    private static float CalculateHazardTransitionProbability(string from, string to)
    {
        // After ice hazards, place non-slippery hazards
        if (from == "Ice" && to != "Ice")
            return 0.3f;
        
        // Fire and ice contrasts are interesting
        if ((from == "Fire" && to == "Ice") || (from == "Ice" && to == "Fire"))
            return 0.25f;
        
        // Spikes are common and can transition to anything
        if (from == "Spikes")
            return 0.15f;
        
        // Prevent hazard overload (avoid consecutive intense hazards)
        if (IsIntenseHazard(from) && IsIntenseHazard(to))
            return 0.02f;
        
        // Default probability
        return 0.12f;
    }
    
    private static bool IsIntenseHazard(string hazard)
    {
        return hazard == "Lightning" || hazard == "Lasers" || hazard == "CrushBlock";
    }
    
    /// <summary>
    /// Initialize room type transitions for level flow
    /// </summary>
    private static void InitializeRoomTransitions()
    {
        if (_roomTransitionChain == null) return;
        
        var roomTypes = new[]
        {
            "Platforming", "Combat", "Puzzle", "Boss", "Rest",
            "Secret", "Challenge", "Cinematic"
        };
        
        foreach (var fromType in roomTypes)
        {
            foreach (var toType in roomTypes)
            {
                float probability = CalculateRoomTransitionProbability(fromType, toType);
                _roomTransitionChain.AddTransition(fromType, toType, probability);
            }
        }
    }
    
    /// <summary>
    /// Calculate intelligent room flow transitions
    /// </summary>
    private static float CalculateRoomTransitionProbability(string from, string to)
    {
        // Rest rooms after combat
        if (from == "Combat" && to == "Rest")
            return 0.35f;
        
        // Boss rooms are rare and significant
        if (to == "Boss")
            return 0.03f;
        
        // No consecutive boss rooms
        if (from == "Boss" && to == "Boss")
            return 0f;
        
        // Cinematic after boss
        if (from == "Boss" && to == "Cinematic")
            return 0.4f;
        
        // Platforming is common and transitions well
        if (from == "Platforming")
            return 0.15f;
        
        // Secret rooms are rare
        if (to == "Secret")
            return 0.05f;
        
        // Default
        return 0.1f;
    }
    
    /// <summary>
    /// Place entities with Markov-guided transitions for coherent placement
    /// </summary>
    public static List<EntityPlacement> PlaceEntitiesWithMarkov(
        RoomTemplate template,
        int seed,
        DifficultyTier difficulty,
        PlacementStrategy strategy = PlacementStrategy.Balanced,
        string? previousEntityType = null)
    {
        if (!_initialized)
        {
            Initialize(seed);
        }
        
        var placements = new List<EntityPlacement>();
        var rng = new Random(seed);
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Placing entities with Markov in template: {template.Name}");
        
        // Set initial state if provided
        if (previousEntityType != null && _entityTransitionChain != null)
        {
            _entityTransitionChain.Initialize(previousEntityType);
        }
        
        // Place required elements first
        foreach (var required in template.RequiredElements)
        {
            var count = rng.Next(required.MinCount, required.MaxCount + 1);
            for (int i = 0; i < count; i++)
            {
                var placement = PlaceEntityInZone(template, required.EntityType, required.Preference, rng);
                if (placement != null)
                {
                    placements.Add(placement);
                }
                
                // Update Markov chain state
                if (_entityTransitionChain != null)
                {
                    _entityTransitionChain.GetNextState();
                }
            }
        }
        
        // Place optional entities using Markov guidance
        foreach (var zone in template.EntityZones)
        {
            int entityCount = CalculateEntityCount(zone, difficulty, strategy, rng);
            
            for (int i = 0; i < entityCount; i++)
            {
                string entityType;
                
                // Use Markov chain to guide entity selection
                if (_entityTransitionChain != null && _entityTransitionChain.IsInitialized)
                {
                    var possibleTypes = zone.AllowedEntities;
                    if (possibleTypes.Count > 0)
                    {
                        // Get next suggested type from Markov chain
                        var suggestedType = _entityTransitionChain.GetNextState();
                        
                        // If the suggested type is available, use it
                        if (possibleTypes.Contains(suggestedType))
                        {
                            entityType = suggestedType;
                        }
                        else
                        {
                            // Otherwise fall back to random selection from available types
                            entityType = possibleTypes[rng.Next(possibleTypes.Count)];
                        }
                    }
                    else
                    {
                        entityType = SelectEntityType(zone, rng);
                    }
                }
                else
                {
                    entityType = SelectEntityType(zone, rng);
                }
                
                if (!string.IsNullOrEmpty(entityType))
                {
                    var placement = PlaceEntityInZone(zone, entityType, rng);
                    if (placement != null)
                    {
                        placements.Add(placement);
                    }
                }
            }
        }
        
        Logger.Log(LogLevel.Info, LogTag, $"Placed {placements.Count} entities with Markov guidance");
        return placements;
    }
    
    /// <summary>
    /// Place hazards with Markov-guided transitions
    /// </summary>
    public static List<EntityPlacement> PlaceHazardsWithMarkov(
        RoomTemplate template,
        int seed,
        DifficultyTier difficulty,
        PlacementStrategy strategy = PlacementStrategy.Balanced,
        string? previousHazardType = null)
    {
        if (!_initialized)
        {
            Initialize(seed);
        }
        
        var placements = new List<EntityPlacement>();
        var rng = new Random(seed);
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Placing hazards with Markov in template: {template.Name}");
        
        // Set initial state if provided
        if (previousHazardType != null && _hazardTransitionChain != null)
        {
            _hazardTransitionChain.Initialize(previousHazardType);
        }
        
        foreach (var zone in template.HazardZones)
        {
            int hazardCount = CalculateHazardCount(zone, difficulty, strategy, rng);
            
            for (int i = 0; i < hazardCount; i++)
            {
                string hazardType;
                
                // Use Markov chain to guide hazard selection
                if (_hazardTransitionChain != null && _hazardTransitionChain.IsInitialized)
                {
                    var possibleTypes = zone.AllowedEntities;
                    if (possibleTypes.Count > 0)
                    {
                        var suggestedType = _hazardTransitionChain.GetNextState();
                        
                        if (possibleTypes.Contains(suggestedType))
                        {
                            hazardType = suggestedType;
                        }
                        else
                        {
                            hazardType = possibleTypes[rng.Next(possibleTypes.Count)];
                        }
                    }
                    else
                    {
                        hazardType = SelectHazardType(zone, difficulty, rng);
                    }
                }
                else
                {
                    hazardType = SelectHazardType(zone, difficulty, rng);
                }
                
                if (!string.IsNullOrEmpty(hazardType))
                {
                    var placement = PlaceEntityInZone(zone, hazardType, rng);
                    if (placement != null)
                    {
                        placements.Add(placement);
                    }
                }
            }
        }
        
        Logger.Log(LogLevel.Info, LogTag, $"Placed {placements.Count} hazards with Markov guidance");
        return placements;
    }
    
    /// <summary>
    /// Generate a sequence of room types using Markov chain
    /// </summary>
    public static List<string> GenerateRoomSequence(int length, string? startRoom = null, int seed = 0)
    {
        if (!_initialized)
        {
            Initialize(seed);
        }
        
        if (_roomTransitionChain == null)
        {
            return new List<string>();
        }
        
        if (startRoom != null)
        {
            _roomTransitionChain.Initialize(startRoom);
        }
        
        var sequence = _roomTransitionChain.GenerateSequence(length);
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated room sequence of length {sequence.Count}");
        
        return sequence;
    }
    
    /// <summary>
    /// Train the Markov chains from existing level data
    /// </summary>
    public static void TrainFromExistingData(List<string> entitySequence, List<string> hazardSequence)
    {
        if (!_initialized)
        {
            Initialize();
        }
        
        if (_entityTransitionChain != null && entitySequence.Count > 0)
        {
            _entityTransitionChain.Train(entitySequence);
            Logger.Log(LogLevel.Info, LogTag, 
                $"Trained entity chain from {entitySequence.Count} samples");
        }
        
        if (_hazardTransitionChain != null && hazardSequence.Count > 0)
        {
            _hazardTransitionChain.Train(hazardSequence);
            Logger.Log(LogLevel.Info, LogTag, 
                $"Trained hazard chain from {hazardSequence.Count} samples");
        }
    }
    
    /// <summary>
    /// Get the current state of the entity transition chain
    /// </summary>
    public static string? GetCurrentEntityState()
    {
        return _entityTransitionChain?.CurrentState?.ToString();
    }
    
    /// <summary>
    /// Get the current state of the hazard transition chain
    /// </summary>
    public static string? GetCurrentHazardState()
    {
        return _hazardTransitionChain?.CurrentState?.ToString();
    }
    
    /// <summary>
    /// Reset the Markov chains
    /// </summary>
    public static void Reset()
    {
        if (_entityTransitionChain != null)
        {
            _entityTransitionChain.Reset("Platforming");
        }
        
        if (_hazardTransitionChain != null)
        {
            _hazardTransitionChain.Reset("Spikes");
        }
        
        if (_roomTransitionChain != null)
        {
            _roomTransitionChain.Reset("Platforming");
        }
        
        Logger.Log(LogLevel.Info, LogTag, "Reset Markov chains");
    }
    
    /// <summary>
    /// Cleanup the Markov-enhanced placement system
    /// </summary>
    public static void Cleanup()
    {
        _entityTransitionChain?.Clear();
        _hazardTransitionChain?.Clear();
        _roomTransitionChain?.Clear();
        _initialized = false;
        
        MarkovChainManager.Cleanup();
        
        Logger.Log(LogLevel.Info, LogTag, "Cleaned up Markov-enhanced placement system");
    }
    
    // Helper methods that mirror the original ProceduralPlacement methods
    private static int CalculateEntityCount(PlacementZone zone, DifficultyTier difficulty, PlacementStrategy strategy, Random rng)
    {
        float baseCount = zone.Density * (zone.Width * zone.Height) / 1000f;
        float difficultyMultiplier = GetDifficultyMultiplier(difficulty);
        float strategyMultiplier = GetStrategyMultiplier(strategy);
        
        int count = (int)(baseCount * difficultyMultiplier * strategyMultiplier);
        return Math.Max(1, count);
    }
    
    private static int CalculateHazardCount(PlacementZone zone, DifficultyTier difficulty, PlacementStrategy strategy, Random rng)
    {
        float baseCount = zone.Density * (zone.Width * zone.Height) / 1500f;
        float difficultyMultiplier = GetDifficultyMultiplier(difficulty);
        float strategyMultiplier = GetStrategyMultiplier(strategy);
        
        int count = (int)(baseCount * difficultyMultiplier * strategyMultiplier);
        return Math.Max(1, count);
    }
    
    private static float GetDifficultyMultiplier(DifficultyTier difficulty)
    {
        return difficulty switch
        {
            DifficultyTier.Easy => 0.7f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.3f,
            DifficultyTier.Expert => 1.6f,
            DifficultyTier.Master => 2.0f,
            _ => 1.0f
        };
    }
    
    private static float GetStrategyMultiplier(PlacementStrategy strategy)
    {
        return strategy switch
        {
            PlacementStrategy.Minimal => 0.6f,
            PlacementStrategy.Balanced => 1.0f,
            PlacementStrategy.Dense => 1.4f,
            PlacementStrategy.Chaotic => 1.8f,
            _ => 1.0f
        };
    }
    
    private static string SelectEntityType(PlacementZone zone, Random rng)
    {
        if (zone.AllowedEntities.Count == 0)
            return string.Empty;
            
        return zone.AllowedEntities[rng.Next(zone.AllowedEntities.Count)];
    }
    
    private static string SelectHazardType(PlacementZone zone, DifficultyTier difficulty, Random rng)
    {
        if (zone.AllowedEntities.Count == 0)
            return string.Empty;
            
        return zone.AllowedEntities[rng.Next(zone.AllowedEntities.Count)];
    }
    
    private static EntityPlacement PlaceEntityInZone(PlacementZone zone, string entityType, Random rng)
    {
        var x = rng.Next(zone.X, zone.X + zone.Width);
        var y = rng.Next(zone.Y, zone.Y + zone.Height);
        
        return new EntityPlacement
        {
            EntityType = entityType,
            X = x,
            Y = y,
            Width = GetDefaultWidth(entityType),
            Height = GetDefaultHeight(entityType)
        };
    }
    
    private static EntityPlacement PlaceEntityInZone(RoomTemplate template, string entityType, PlacementPreference preference, Random rng)
    {
        var zone = SelectZoneByPreference(template, preference);
        if (zone == null)
        {
            zone = template.EntityZones.FirstOrDefault();
        }
        
        if (zone != null)
        {
            return PlaceEntityInZone(zone, entityType, rng);
        }
        
        return new EntityPlacement
        {
            EntityType = entityType,
            X = template.Width / 2,
            Y = template.Height / 2,
            Width = GetDefaultWidth(entityType),
            Height = GetDefaultHeight(entityType)
        };
    }
    
    private static PlacementZone? SelectZoneByPreference(RoomTemplate template, PlacementPreference preference)
    {
        return preference switch
        {
            PlacementPreference.GroundLevel => template.EntityZones.FirstOrDefault(z => z.Type == ZoneType.Ground),
            PlacementPreference.MidAir => template.EntityZones.FirstOrDefault(z => z.Type == ZoneType.Air),
            PlacementPreference.NearWalls => template.EntityZones.FirstOrDefault(z => z.Type == ZoneType.Wall),
            PlacementPreference.Centered => template.EntityZones.OrderByDescending(z => z.Width * z.Height).FirstOrDefault(),
            PlacementPreference.Edges => template.EntityZones.OrderBy(z => z.Width * z.Height).FirstOrDefault(),
            _ => template.EntityZones.FirstOrDefault()
        };
    }
    
    private static int GetDefaultWidth(string entityType)
    {
        return entityType switch
        {
            "Refill" => 16,
            "Strawberry" => 24,
            "Key" => 20,
            "Spring" => 32,
            "Spinner" => 16,
            "MovingPlatform" => 64,
            "CassetteBlock" => 32,
            "DashBlock" => 32,
            _ => 16
        };
    }
    
    private static int GetDefaultHeight(string entityType)
    {
        return entityType switch
        {
            "Refill" => 16,
            "Strawberry" => 24,
            "Key" => 20,
            "Spring" => 16,
            "Spinner" => 16,
            "MovingPlatform" => 16,
            "CassetteBlock" => 32,
            "DashBlock" => 32,
            _ => 16
        };
    }
}