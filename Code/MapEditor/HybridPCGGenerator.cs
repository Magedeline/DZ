using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Celeste;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE;

/// <summary>
/// Main hybrid PCG generator - combines room templates with procedural elements
/// </summary>
public static class HybridPCGGenerator
{
    private const string LogTag = "HybridPCGGenerator";
    
    /// <summary>
    /// Generate a complete map using hybrid PCG
    /// </summary>
    public static async Task<bool> GenerateHybridMapAsync(
        string templateLibraryPath,
        string outputPath,
        int seed = -1,
        int roomCount = 10,
        DifficultyTier difficulty = DifficultyTier.Normal,
        ConnectionStrategy connectionStrategy = ConnectionStrategy.Pathway,
        PlacementStrategy placementStrategy = PlacementStrategy.Balanced)
    {
        try
        {
            Logger.Log(LogLevel.Info, LogTag, 
                $"Starting hybrid PCG generation: seed={seed}, rooms={roomCount}, difficulty={difficulty}");
            
            // Load template library
            var templates = RoomTemplateLoader.LoadTemplatesFromJson(templateLibraryPath);
            if (templates.Count == 0)
            {
                Logger.Log(LogLevel.Error, LogTag, "No templates loaded");
                return false;
            }
            
            // Select rooms for the map
            var selectedRooms = SelectRooms(templates, roomCount, difficulty, seed);
            Logger.Log(LogLevel.Info, LogTag, $"Selected {selectedRooms.Count} rooms");
            
            // Connect rooms
            var connections = RoomConnector.ConnectRooms(selectedRooms, seed, connectionStrategy);
            Logger.Log(LogLevel.Info, LogTag, $"Created {connections.Count} connections");
            
            // Place procedural elements
            var allPlacements = new List<EntityPlacement>();
            var rng = new Random(seed);
            
            foreach (var room in selectedRooms)
            {
                int roomSeed = rng.Next();

                // Place entities
                var entities = ProceduralPlacement.PlaceEntities(room, roomSeed, difficulty, placementStrategy);
                foreach (var p in entities) p.RoomName = room.Name;
                allPlacements.AddRange(entities);

                // Place hazards
                var hazards = ProceduralPlacement.PlaceHazards(room, roomSeed + 1, difficulty, placementStrategy);
                foreach (var p in hazards) p.RoomName = room.Name;
                allPlacements.AddRange(hazards);

                // Place platforms
                var platforms = ProceduralPlacement.PlacePlatforms(room, roomSeed + 2, difficulty, placementStrategy);
                foreach (var p in platforms) p.RoomName = room.Name;
                allPlacements.AddRange(platforms);
            }
            
            Logger.Log(LogLevel.Info, LogTag, $"Placed {allPlacements.Count} procedural elements");
            
            // Validate playability
            var validation = ValidateMap(selectedRooms, connections, allPlacements);
            if (!validation.IsValid)
            {
                Logger.Log(LogLevel.Warn, LogTag, $"Map validation failed: {validation.Reason}");
                // Continue anyway for now, but log the issue
            }
            
            // Generate the actual map file
            bool success = await GenerateMapFile(selectedRooms, connections, allPlacements, outputPath, seed);
            
            if (success)
            {
                Logger.Log(LogLevel.Info, LogTag, $"Hybrid PCG generation complete: {outputPath}");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Hybrid PCG generation failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Select rooms from template library based on criteria
    /// </summary>
    private static List<RoomTemplate> SelectRooms(
        List<RoomTemplate> templates, 
        int count, 
        DifficultyTier difficulty,
        int seed)
    {
        var rng = new Random(seed);
        var selected = new List<RoomTemplate>();
        
        // Filter by difficulty (allow one tier difference)
        var filtered = templates.Where(t => 
            Math.Abs((int)t.Difficulty - (int)difficulty) <= 1).ToList();
        
        if (filtered.Count == 0)
        {
            // Fallback to all templates
            filtered = templates;
        }
        
        // Ensure we have a checkpoint room
        var checkpointRoom = filtered.FirstOrDefault(t => t.Type == RoomType.Checkpoint);
        if (checkpointRoom != null)
        {
            selected.Add(checkpointRoom);
            filtered.Remove(checkpointRoom);
        }
        
        // Select remaining rooms
        while (selected.Count < count && filtered.Count > 0)
        {
            int index = rng.Next(filtered.Count);
            selected.Add(filtered[index]);
            filtered.RemoveAt(index);
        }
        
        // If we don't have enough rooms, add more from original list
        var remaining = templates.Except(selected).ToList();
        while (selected.Count < count && remaining.Count > 0)
        {
            int index = rng.Next(remaining.Count);
            selected.Add(remaining[index]);
            remaining.RemoveAt(index);
        }
        
        return selected;
    }
    
    /// <summary>
    /// Validate the generated map for playability
    /// </summary>
    private static MapValidationResult ValidateMap(
        List<RoomTemplate> rooms,
        List<RoomConnection> connections,
        List<EntityPlacement> placements)
    {
        var result = new MapValidationResult { IsValid = true };
        
        // Check if map has at least one checkpoint
        if (!rooms.Any(r => r.Type == RoomType.Checkpoint))
        {
            result.IsValid = false;
            result.Reason = "No checkpoint room found";
            return result;
        }
        
        // Check if all rooms are connected
        var connectedRooms = new HashSet<string>();
        var queue = new Queue<string>();
        
        // Start from first room
        if (rooms.Count > 0)
        {
            queue.Enqueue(rooms[0].Name);
            connectedRooms.Add(rooms[0].Name);
        }
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            
            foreach (var conn in connections.Where(c => c.FromRoom == current))
            {
                if (!connectedRooms.Contains(conn.ToRoom))
                {
                    connectedRooms.Add(conn.ToRoom);
                    queue.Enqueue(conn.ToRoom);
                }
            }
            
            foreach (var conn in connections.Where(c => c.ToRoom == current))
            {
                if (!connectedRooms.Contains(conn.FromRoom))
                {
                    connectedRooms.Add(conn.FromRoom);
                    queue.Enqueue(conn.FromRoom);
                }
            }
        }
        
        if (connectedRooms.Count < rooms.Count)
        {
            result.IsValid = false;
            result.Reason = $"Not all rooms are connected: {connectedRooms.Count}/{rooms.Count}";
            return result;
        }
        
        // Check for required keys
        var keyConnections = connections.Where(c => c.RequiresKey).ToList();
        foreach (var keyConn in keyConnections)
        {
            var keyPlaced = placements.Any(p => p.EntityType == "key" && 
                p.Properties.ContainsKey("id") && 
                p.Properties["id"].ToString() == keyConn.KeyId);
            
            if (!keyPlaced)
            {
                result.IsValid = false;
                result.Reason = $"Key required for connection but not placed: {keyConn.FromRoom} -> {keyConn.ToRoom}";
                return result;
            }
        }
        
        // Check for goal/strawberry placement
        var hasGoal = placements.Any(p => p.EntityType == "strawberry" || p.EntityType == "goal");
        if (!hasGoal)
        {
            result.Warnings.Add("No goal or strawberry placed");
        }
        
        return result;
    }
    
    /// <summary>
    /// Generate the actual Celeste .bin map file from the hybrid generation.
    /// </summary>
    private static async Task<bool> GenerateMapFile(
        List<RoomTemplate> rooms,
        List<RoomConnection> connections,
        List<EntityPlacement> placements,
        string outputPath,
        int seed)
    {
        try
        {
            string packageName = Path.GetFileNameWithoutExtension(outputPath);

            await Task.Run(() =>
            {
                var mapElement = MapBuilder.BuildMap(packageName, rooms, connections, placements);
                MapBinaryWriter.Write(outputPath, packageName, mapElement);
            });

            Logger.Log(LogLevel.Info, LogTag, $"Wrote .bin map: {outputPath}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Map file generation failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Build template library from existing maps
    /// </summary>
    public static async Task<bool> BuildTemplateLibraryAsync(
        string[] mapPaths,
        string outputPath = "PCG/Templates/library.json")
    {
        try
        {
            Logger.Log(LogLevel.Info, LogTag, $"Building template library from {mapPaths.Length} maps");
            
            var allTemplates = new List<RoomTemplate>();
            
            foreach (var mapPath in mapPaths)
            {
                if (File.Exists(mapPath))
                {
                    var templates = RoomTemplateLoader.ExtractTemplatesFromMap(mapPath);
                    allTemplates.AddRange(templates);
                }
            }
            
            if (allTemplates.Count > 0)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                RoomTemplateLoader.SaveTemplatesToJson(allTemplates, outputPath);
                Logger.Log(LogLevel.Info, LogTag, $"Template library built: {allTemplates.Count} templates -> {outputPath}");
                return true;
            }
            
            Logger.Log(LogLevel.Warn, LogTag, "No templates extracted from maps");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Template library build failed: {ex.Message}");
            return false;
        }
    }
}

/// <summary>
/// Result of map validation
/// </summary>
public class MapValidationResult
{
    public bool IsValid { get; set; }
    public string Reason { get; set; }
    public List<string> Warnings { get; set; }
    
    public MapValidationResult()
    {
        Warnings = new List<string>();
    }
}
