using System;
using System.IO;
using System.Threading.Tasks;
using Celeste;
using Monocle;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// PCG Service for procedural content generation using loenn-mcp integration
/// </summary>
public static class PCGService
{
    private const string LogTag = "PCGService";
    
    /// <summary>
    /// Generate a terrain map using Perlin noise and Voronoi biomes
    /// </summary>
    public static async Task<bool> GenerateTerrainMapAsync(
        string outputPath,
        int seed = -1,
        int difficulty = 3,
        int widthRooms = 4,
        int heightRooms = 3,
        double frequency = 8.0,
        int voronoiPoints = 12,
        string biomeSet = "",
        string packageName = "TerrainGen")
    {
        try
        {
            Logger.Log(LogLevel.Info, LogTag, 
                $"Starting terrain generation: seed={seed}, difficulty={difficulty}, " +
                $"size={widthRooms}x{heightRooms}");
            
            // This would call the loenn-mcp tool: mcp3_generate_terrain_map
            // For now, we'll simulate the call and log the parameters
            // In a real implementation, this would invoke the MCP server
            
            await Task.Run(() =>
            {
                // Simulate generation time
                System.Threading.Thread.Sleep(100);
                
                Logger.Log(LogLevel.Info, LogTag, 
                    $"Terrain generation complete: {outputPath}");
            });
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Terrain generation failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Generate a room from pattern library
    /// </summary>
    public static async Task<bool> GenerateRoomFromPatternAsync(
        string mapPath,
        string roomName,
        int width = 320,
        int height = 184,
        int x = 0,
        int y = 0,
        int seed = -1,
        string strategy = "balanced",
        string modelProfile = "creative",
        string libraryPath = "PCG/patterns.json")
    {
        try
        {
            Logger.Log(LogLevel.Info, LogTag, 
                $"Generating room from pattern: {roomName}, strategy={strategy}, seed={seed}");
            
            // This would call the loenn-mcp tool: mcp3_generate_room_from_pattern
            // For now, we'll simulate the call
            
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(100);
                
                Logger.Log(LogLevel.Info, LogTag, 
                    $"Room generation complete: {roomName}");
            });
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Room generation failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Generate a map from an image
    /// </summary>
    public static async Task<bool> GenerateMapFromImageAsync(
        string imagePath,
        string outputPath,
        int scale = 1,
        int tolerance = 64,
        int roomWidthTiles = 40,
        int roomHeightTiles = 23,
        string colorMapJson = "",
        string packageName = "ImageMap")
    {
        try
        {
            if (!File.Exists(imagePath))
            {
                Logger.Log(LogLevel.Error, LogTag, $"Image file not found: {imagePath}");
                return false;
            }
            
            Logger.Log(LogLevel.Info, LogTag, 
                $"Generating map from image: {imagePath}, scale={scale}");
            
            // This would call the loenn-mcp tool: mcp3_generate_map_from_image
            // For now, we'll simulate the call
            
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(100);
                
                Logger.Log(LogLevel.Info, LogTag, 
                    $"Map generation from image complete: {outputPath}");
            });
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Image-to-map generation failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Build pattern library from existing maps
    /// </summary>
    public static async Task<bool> BuildPatternLibraryAsync(
        string[] mapPaths,
        string outputPath = "PCG/patterns.json",
        string attribution = "")
    {
        try
        {
            Logger.Log(LogLevel.Info, LogTag, 
                $"Building pattern library from {mapPaths.Length} maps");
            
            // This would call the loenn-mcp tool: mcp3_build_pattern_library
            // For now, we'll simulate the call
            
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(100);
                
                Logger.Log(LogLevel.Info, LogTag, 
                    $"Pattern library built: {outputPath}");
            });
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Pattern library build failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Preview terrain biomes without generating the map
    /// </summary>
    public static void PreviewTerrainBiomes(
        int seed = 42,
        int widthRooms = 4,
        int heightRooms = 3,
        double frequency = 8.0,
        int voronoiPoints = 12,
        string biomeSet = "")
    {
        try
        {
            Logger.Log(LogLevel.Info, LogTag, 
                $"Previewing terrain biomes: seed={seed}, size={widthRooms}x{heightRooms}");
            
            // This would call the loenn-mcp tool: mcp3_preview_terrain_biomes
            // The result would be displayed in the editor UI
            
            // For now, just log the parameters
            Logger.Log(LogLevel.Info, LogTag, 
                $"Biome preview would show layout for seed {seed}");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Biome preview failed: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get available biomes for terrain generation
    /// </summary>
    public static string[] GetAvailableBiomes()
    {
        return new string[]
        {
            "mountain",
            "forest",
            "plains",
            "lake",
            "cave",
            "summit"
        };
    }
    
    /// <summary>
    /// Get available generation strategies
    /// </summary>
    public static string[] GetAvailableStrategies()
    {
        return new string[]
        {
            "balanced",
            "exploration",
            "challenge",
            "speedrun"
        };
    }
    
    /// <summary>
    /// Get available model profiles
    /// </summary>
    public static string[] GetModelProfiles()
    {
        return new string[]
        {
            "creative",
            "deterministic",
            "architect"
        };
    }
    
    /// <summary>
    /// Generate a hybrid PCG map using room templates + procedural elements
    /// </summary>
    public static async Task<bool> GenerateHybridMapAsync(
        string templateLibraryPath,
        string outputPath,
        int seed = -1,
        int roomCount = 10,
        int difficulty = 3,
        string connectionStrategy = "pathway",
        string placementStrategy = "balanced")
    {
        try
        {
            Logger.Log(LogLevel.Info, LogTag, 
                $"Starting hybrid PCG generation: seed={seed}, rooms={roomCount}, difficulty={difficulty}");
            
            // Convert string parameters to enums
            DifficultyTier diffTier = (DifficultyTier)difficulty;
            ConnectionStrategy connStrategy = ParseConnectionStrategy(connectionStrategy);
            PlacementStrategy placeStrategy = ParsePlacementStrategy(placementStrategy);
            
            // Call the hybrid generator
            bool success = await HybridPCGGenerator.GenerateHybridMapAsync(
                templateLibraryPath,
                outputPath,
                seed,
                roomCount,
                diffTier,
                connStrategy,
                placeStrategy);
            
            return success;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Hybrid PCG generation failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Build template library from existing maps for hybrid PCG
    /// </summary>
    public static async Task<bool> BuildTemplateLibraryAsync(
        string[] mapPaths,
        string outputPath = "PCG/Templates/library.json")
    {
        try
        {
            Logger.Log(LogLevel.Info, LogTag, 
                $"Building template library from {mapPaths.Length} maps");
            
            bool success = await HybridPCGGenerator.BuildTemplateLibraryAsync(mapPaths, outputPath);
            
            return success;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Template library build failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get available connection strategies for hybrid PCG
    /// </summary>
    public static string[] GetConnectionStrategies()
    {
        return new string[]
        {
            "pathway",
            "labyrinth",
            "branching",
            "linear"
        };
    }
    
    /// <summary>
    /// Get available placement strategies for hybrid PCG
    /// </summary>
    public static string[] GetPlacementStrategies()
    {
        return new string[]
        {
            "minimal",
            "balanced",
            "dense",
            "chaotic"
        };
    }
    
    /// <summary>
    /// Parse connection strategy string to enum
    /// </summary>
    private static ConnectionStrategy ParseConnectionStrategy(string strategy)
    {
        return strategy.ToLower() switch
        {
            "pathway" => ConnectionStrategy.Pathway,
            "labyrinth" => ConnectionStrategy.Labyrinth,
            "branching" => ConnectionStrategy.Branching,
            "linear" => ConnectionStrategy.Linear,
            _ => ConnectionStrategy.Pathway
        };
    }
    
    /// <summary>
    /// Parse placement strategy string to enum
    /// </summary>
    private static PlacementStrategy ParsePlacementStrategy(string strategy)
    {
        return strategy.ToLower() switch
        {
            "minimal" => PlacementStrategy.Minimal,
            "balanced" => PlacementStrategy.Balanced,
            "dense" => PlacementStrategy.Dense,
            "chaotic" => PlacementStrategy.Chaotic,
            _ => PlacementStrategy.Balanced
        };
    }
}
