using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Monocle;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// Procedural placement system for entities and hazards within room templates
/// </summary>
public static class ProceduralPlacement
{
    private const string LogTag = "ProceduralPlacement";
    
    /// <summary>
    /// Place entities procedurally in a room template
    /// </summary>
    public static List<EntityPlacement> PlaceEntities(
        RoomTemplate template, 
        int seed, 
        DifficultyTier difficulty,
        PlacementStrategy strategy = PlacementStrategy.Balanced)
    {
        var placements = new List<EntityPlacement>();
        var rng = new Random(seed);
        
        Logger.Log(LogLevel.Info, LogTag, $"Placing entities in template: {template.Name}");
        
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
            }
        }
        
        // Place optional entities based on zones
        foreach (var zone in template.EntityZones)
        {
            int entityCount = CalculateEntityCount(zone, difficulty, strategy, rng);
            
            for (int i = 0; i < entityCount; i++)
            {
                var entityType = SelectEntityType(zone, rng);
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
        
        Logger.Log(LogLevel.Info, LogTag, $"Placed {placements.Count} entities");
        return placements;
    }
    
    /// <summary>
    /// Place hazards procedurally in a room template
    /// </summary>
    public static List<EntityPlacement> PlaceHazards(
        RoomTemplate template, 
        int seed, 
        DifficultyTier difficulty,
        PlacementStrategy strategy = PlacementStrategy.Balanced)
    {
        var placements = new List<EntityPlacement>();
        var rng = new Random(seed);
        
        Logger.Log(LogLevel.Info, LogTag, $"Placing hazards in template: {template.Name}");
        
        foreach (var zone in template.HazardZones)
        {
            int hazardCount = CalculateHazardCount(zone, difficulty, strategy, rng);
            
            for (int i = 0; i < hazardCount; i++)
            {
                var hazardType = SelectHazardType(zone, difficulty, rng);
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
        
        Logger.Log(LogLevel.Info, LogTag, $"Placed {placements.Count} hazards");
        return placements;
    }
    
    /// <summary>
    /// Place platforms procedurally in a room template
    /// </summary>
    public static List<EntityPlacement> PlacePlatforms(
        RoomTemplate template, 
        int seed, 
        DifficultyTier difficulty,
        PlacementStrategy strategy = PlacementStrategy.Balanced)
    {
        var placements = new List<EntityPlacement>();
        var rng = new Random(seed);
        
        Logger.Log(LogLevel.Info, LogTag, $"Placing platforms in template: {template.Name}");
        
        foreach (var zone in template.PlatformZones)
        {
            int platformCount = CalculatePlatformCount(zone, difficulty, strategy, rng);
            
            for (int i = 0; i < platformCount; i++)
            {
                var platformType = SelectPlatformType(zone, rng);
                if (!string.IsNullOrEmpty(platformType))
                {
                    var placement = PlaceEntityInZone(zone, platformType, rng);
                    if (placement != null)
                    {
                        placements.Add(placement);
                    }
                }
            }
        }
        
        Logger.Log(LogLevel.Info, LogTag, $"Placed {placements.Count} platforms");
        return placements;
    }
    
    /// <summary>
    /// Place a single entity in a specific zone
    /// </summary>
    private static EntityPlacement PlaceEntityInZone(
        PlacementZone zone, 
        string entityType, 
        Random rng)
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
    
    /// <summary>
    /// Place a single entity based on placement preference
    /// </summary>
    private static EntityPlacement PlaceEntityInZone(
        RoomTemplate template,
        string entityType,
        PlacementPreference preference,
        Random rng)
    {
        // Find appropriate zone based on preference
        var zone = SelectZoneByPreference(template, preference);
        if (zone == null)
        {
            // Fallback to any entity zone
            zone = template.EntityZones.FirstOrDefault();
        }
        
        if (zone != null)
        {
            return PlaceEntityInZone(zone, entityType, rng);
        }
        
        // Fallback to center of room
        return new EntityPlacement
        {
            EntityType = entityType,
            X = template.Width / 2,
            Y = template.Height / 2,
            Width = GetDefaultWidth(entityType),
            Height = GetDefaultHeight(entityType)
        };
    }
    
    /// <summary>
    /// Select a zone based on placement preference
    /// </summary>
    private static PlacementZone SelectZoneByPreference(RoomTemplate template, PlacementPreference preference)
    {
        // Simplified selection logic
        switch (preference)
        {
            case PlacementPreference.GroundLevel:
                return template.EntityZones.FirstOrDefault(z => z.Type == ZoneType.Ground);
            case PlacementPreference.MidAir:
                return template.EntityZones.FirstOrDefault(z => z.Type == ZoneType.Air);
            case PlacementPreference.NearWalls:
                return template.EntityZones.FirstOrDefault(z => z.Type == ZoneType.Wall);
            case PlacementPreference.Centered:
                return template.EntityZones.OrderByDescending(z => z.Width * z.Height).FirstOrDefault();
            case PlacementPreference.Edges:
                return template.EntityZones.OrderBy(z => z.Width * z.Height).FirstOrDefault();
            default:
                return template.EntityZones.FirstOrDefault();
        }
    }
    
    /// <summary>
    /// Calculate number of entities to place in a zone
    /// </summary>
    private static int CalculateEntityCount(PlacementZone zone, DifficultyTier difficulty, PlacementStrategy strategy, Random rng)
    {
        float baseCount = zone.Density * (zone.Width * zone.Height) / 1000f;
        
        // Adjust based on difficulty
        float difficultyMultiplier = GetDifficultyMultiplier(difficulty);
        
        // Adjust based on strategy
        float strategyMultiplier = GetStrategyMultiplier(strategy);
        
        int count = (int)(baseCount * difficultyMultiplier * strategyMultiplier);
        return Math.Max(1, count);
    }
    
    /// <summary>
    /// Calculate number of hazards to place in a zone
    /// </summary>
    private static int CalculateHazardCount(PlacementZone zone, DifficultyTier difficulty, PlacementStrategy strategy, Random rng)
    {
        float baseCount = zone.Density * (zone.Width * zone.Height) / 800f;
        
        float difficultyMultiplier = GetDifficultyMultiplier(difficulty);
        float strategyMultiplier = GetStrategyMultiplier(strategy);
        
        int count = (int)(baseCount * difficultyMultiplier * strategyMultiplier);
        return Math.Max(0, count);
    }
    
    /// <summary>
    /// Calculate number of platforms to place in a zone
    /// </summary>
    private static int CalculatePlatformCount(PlacementZone zone, DifficultyTier difficulty, PlacementStrategy strategy, Random rng)
    {
        float baseCount = zone.Density * (zone.Width * zone.Height) / 2000f;
        
        float difficultyMultiplier = GetDifficultyMultiplier(difficulty);
        float strategyMultiplier = GetStrategyMultiplier(strategy);
        
        int count = (int)(baseCount * difficultyMultiplier * strategyMultiplier);
        return Math.Max(0, count);
    }
    
    /// <summary>
    /// Select an entity type from zone's allowed entities
    /// </summary>
    private static string SelectEntityType(PlacementZone zone, Random rng)
    {
        if (zone.AllowedEntities.Count == 0)
        {
            return "strawberry"; // Default
        }
        
        int index = rng.Next(zone.AllowedEntities.Count);
        return zone.AllowedEntities[index];
    }
    
    /// <summary>
    /// Select a hazard type based on difficulty
    /// </summary>
    private static string SelectHazardType(PlacementZone zone, DifficultyTier difficulty, Random rng)
    {
        var availableHazards = GetAvailableHazards(difficulty);
        
        // Filter by zone's allowed entities
        var filtered = availableHazards.Where(h => 
            zone.AllowedEntities.Count == 0 || zone.AllowedEntities.Contains(h)).ToList();
        
        if (filtered.Count == 0)
        {
            return "spike"; // Default
        }
        
        int index = rng.Next(filtered.Count);
        return filtered[index];
    }
    
    /// <summary>
    /// Select a platform type
    /// </summary>
    private static string SelectPlatformType(PlacementZone zone, Random rng)
    {
        var availablePlatforms = new List<string> { "movingPlatform", "crumbleBlock", "bounceBlock" };
        
        var filtered = availablePlatforms.Where(p => 
            zone.AllowedEntities.Count == 0 || zone.AllowedEntities.Contains(p)).ToList();
        
        if (filtered.Count == 0)
        {
            return "movingPlatform"; // Default
        }
        
        int index = rng.Next(filtered.Count);
        return filtered[index];
    }
    
    /// <summary>
    /// Get available hazards based on difficulty
    /// </summary>
    private static List<string> GetAvailableHazards(DifficultyTier difficulty)
    {
        var hazards = new List<string> { "spike" };
        
        switch (difficulty)
        {
            case DifficultyTier.Easy:
                hazards.AddRange(new[] { "spike" });
                break;
            case DifficultyTier.Normal:
                hazards.AddRange(new[] { "spike", "spinner" });
                break;
            case DifficultyTier.Hard:
                hazards.AddRange(new[] { "spike", "spinner", "seeker" });
                break;
            case DifficultyTier.Expert:
                hazards.AddRange(new[] { "spike", "spinner", "seeker", "blade" });
                break;
            case DifficultyTier.Master:
                hazards.AddRange(new[] { "spike", "spinner", "seeker", "blade", "fireball" });
                break;
        }
        
        return hazards;
    }
    
    /// <summary>
    /// Get difficulty multiplier for entity/hazard density
    /// </summary>
    private static float GetDifficultyMultiplier(DifficultyTier difficulty)
    {
        return difficulty switch
        {
            DifficultyTier.Tutorial => 0.5f,
            DifficultyTier.Easy => 0.75f,
            DifficultyTier.Normal => 1.0f,
            DifficultyTier.Hard => 1.25f,
            DifficultyTier.Expert => 1.5f,
            DifficultyTier.Master => 2.0f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Get strategy multiplier for entity/hazard density
    /// </summary>
    private static float GetStrategyMultiplier(PlacementStrategy strategy)
    {
        return strategy switch
        {
            PlacementStrategy.Minimal => 0.5f,
            PlacementStrategy.Balanced => 1.0f,
            PlacementStrategy.Dense => 1.5f,
            PlacementStrategy.Chaotic => 2.0f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Get default width for an entity type
    /// </summary>
    private static int GetDefaultWidth(string entityType)
    {
        return entityType switch
        {
            "spike" => 8,
            "spinner" => 16,
            "strawberry" => 16,
            "refill" => 16,
            "key" => 16,
            "movingPlatform" => 40,
            "crumbleBlock" => 8,
            _ => 16
        };
    }
    
    /// <summary>
    /// Get default height for an entity type
    /// </summary>
    private static int GetDefaultHeight(string entityType)
    {
        return entityType switch
        {
            "spike" => 8,
            "spinner" => 16,
            "strawberry" => 16,
            "refill" => 16,
            "key" => 16,
            "movingPlatform" => 8,
            "crumbleBlock" => 8,
            _ => 16
        };
    }
}

/// <summary>
/// Represents a placed entity in the room
/// </summary>
public class EntityPlacement
{
    public string EntityType { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Dictionary<string, object> Properties { get; set; }
    public string RoomName { get; set; }

    public EntityPlacement()
    {
        Properties = new Dictionary<string, object>();
    }
}

public enum PlacementStrategy
{
    Minimal,
    Balanced,
    Dense,
    Chaotic
}
