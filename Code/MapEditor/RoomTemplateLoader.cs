using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Celeste;
using Monocle;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// Loads and analyzes existing rooms to create room templates
/// </summary>
public static class RoomTemplateLoader
{
    private const string LogTag = "RoomTemplateLoader";
    
    /// <summary>
    /// Extract templates from existing map files using BinaryPacker.
    /// </summary>
    public static List<RoomTemplate> ExtractTemplatesFromMap(string mapPath, string outputDir = "PCG/Templates")
    {
        var templates = new List<RoomTemplate>();

        try
        {
            Logger.Log(LogLevel.Info, LogTag, $"Extracting templates from: {mapPath}");

            if (!File.Exists(mapPath))
            {
                Logger.Log(LogLevel.Warn, LogTag, $"Map file not found: {mapPath}");
                return templates;
            }

            BinaryPacker.Element root;
            try
            {
                root = BinaryPacker.FromBinary(mapPath);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LogTag, $"BinaryPacker.FromBinary failed: {ex.Message}");
                return templates;
            }

            if (root == null)
            {
                Logger.Log(LogLevel.Error, LogTag, "Failed to load map binary data.");
                return templates;
            }

            var levelsNode = root.Children?.FirstOrDefault(c => c.Name == "levels");
            if (levelsNode?.Children == null || levelsNode.Children.Count == 0)
            {
                Logger.Log(LogLevel.Warn, LogTag, "No levels found in map.");
                return templates;
            }

            foreach (var level in levelsNode.Children.Where(c => c.Name == "level"))
            {
                var template = ExtractTemplateFromElement(level, mapPath);
                if (template != null)
                    templates.Add(template);
            }

            if (templates.Count > 0)
            {
                Directory.CreateDirectory(outputDir);
                string outputPath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(mapPath)}_templates.json");
                SaveTemplatesToJson(templates, outputPath);
                Logger.Log(LogLevel.Info, LogTag, $"Extracted {templates.Count} templates to: {outputPath}");
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Template extraction failed: {ex.Message}");
        }

        return templates;
    }

    /// <summary>
    /// Extract a single template from a BinaryPacker level element.
    /// </summary>
    private static RoomTemplate ExtractTemplateFromElement(BinaryPacker.Element level, string sourceMap)
    {
        if (level?.Attributes == null)
            return null;

        string name = GetAttr(level, "name", $"room_{Guid.NewGuid():N}");
        int width = GetAttrInt(level, "width", 320);
        int height = GetAttrInt(level, "height", 184);

        var entities = level.Children?.FirstOrDefault(c => c.Name == "entities")?.Children ?? new List<BinaryPacker.Element>();
        var triggers = level.Children?.FirstOrDefault(c => c.Name == "triggers")?.Children ?? new List<BinaryPacker.Element>();
        var solids = level.Children?.FirstOrDefault(c => c.Name == "solids");
        string solidTiles = solids?.Attributes?.GetValueOrDefault("innerText")?.ToString() ?? "";

        int tileCols = width / 8;
        int tileRows = height / 8;
        if (solidTiles.Length < tileCols * tileRows)
            solidTiles = solidTiles.PadRight(tileCols * tileRows, '0');

        var template = new RoomTemplate
        {
            Name = name,
            SourceMap = sourceMap,
            Width = width,
            Height = height,
            Difficulty = InferDifficulty(entities, solidTiles, width, height),
            Type = InferRoomType(entities, width, height)
        };

        template.Connections = AnalyzeConnections(level, width, height, solidTiles, tileCols);
        template.HazardZones = AnalyzeHazardZones(width, height, solidTiles, tileCols, tileRows);
        template.EntityZones = AnalyzeEntityZones(width, height, solidTiles, tileCols, tileRows);
        template.PlatformZones = AnalyzePlatformZones(width, height, solidTiles, tileCols, tileRows);
        template.RequiredElements = IdentifyRequiredElements(entities, triggers);

        template.Tags["theme"] = "unknown";
        template.Tags["verticality"] = CalculateVerticality(solidTiles, tileCols, tileRows).ToString("F2");
        template.Tags["openness"] = CalculateOpenness(solidTiles, tileCols, tileRows).ToString("F2");

        return template;
    }

    // --- Attribute helpers ---
    private static string GetAttr(BinaryPacker.Element el, string key, string fallback = "")
    {
        if (el?.Attributes != null && el.Attributes.TryGetValue(key, out object val))
            return val?.ToString() ?? fallback;
        return fallback;
    }

    private static int GetAttrInt(BinaryPacker.Element el, string key, int fallback = 0)
    {
        if (el?.Attributes != null && el.Attributes.TryGetValue(key, out object val))
        {
            if (int.TryParse(val?.ToString(), out int result))
                return result;
        }
        return fallback;
    }

    // --- Connection analysis ---
    private static List<ConnectionPoint> AnalyzeConnections(BinaryPacker.Element level, int width, int height, string solidTiles, int cols)
    {
        var connections = new List<ConnectionPoint>();

        // Scan edges for gaps (empty tiles) that indicate exits
        bool HasGap(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left:
                    for (int r = 0; r < height / 8; r++)
                        if (solidTiles[r * cols] == '0') return true;
                    break;
                case Direction.Right:
                    for (int r = 0; r < height / 8; r++)
                        if (solidTiles[r * cols + cols - 1] == '0') return true;
                    break;
                case Direction.Up:
                    for (int c = 0; c < cols; c++)
                        if (solidTiles[c] == '0') return true;
                    break;
                case Direction.Down:
                    int lastRow = (height / 8 - 1) * cols;
                    for (int c = 0; c < cols; c++)
                        if (solidTiles[lastRow + c] == '0') return true;
                    break;
            }
            return false;
        }

        if (HasGap(Direction.Left))
            connections.Add(new ConnectionPoint { Direction = Direction.Left, X = 0, Y = height / 2, Width = 40, Type = ConnectionType.Normal, IsEntrance = true });
        if (HasGap(Direction.Right))
            connections.Add(new ConnectionPoint { Direction = Direction.Right, X = width, Y = height / 2, Width = 40, Type = ConnectionType.Normal, IsExit = true });
        if (HasGap(Direction.Up))
            connections.Add(new ConnectionPoint { Direction = Direction.Up, X = width / 2, Y = 0, Width = 40, Type = ConnectionType.Normal, IsExit = true });
        if (HasGap(Direction.Down))
            connections.Add(new ConnectionPoint { Direction = Direction.Down, X = width / 2, Y = height, Width = 40, Type = ConnectionType.Normal, IsExit = true });

        // If no gaps found, add left/right defaults so rooms can still connect
        if (connections.Count == 0)
        {
            connections.Add(new ConnectionPoint { Direction = Direction.Left, X = 0, Y = height / 2, Width = 40, Type = ConnectionType.Normal, IsEntrance = true });
            connections.Add(new ConnectionPoint { Direction = Direction.Right, X = width, Y = height / 2, Width = 40, Type = ConnectionType.Normal, IsExit = true });
        }

        return connections;
    }

    // --- Placement zones ---
    private static List<PlacementZone> AnalyzeHazardZones(int width, int height, string solidTiles, int cols, int rows)
    {
        var zones = new List<PlacementZone>();
        // Find open air zones away from floor
        int groundRow = rows - 1;
        while (groundRow > 0 && CountSolidInRow(solidTiles, cols, groundRow) == 0) groundRow--;

        if (groundRow > 2)
        {
            zones.Add(new PlacementZone
            {
                X = 16,
                Y = 16,
                Width = width - 32,
                Height = groundRow * 8 - 16,
                Type = ZoneType.Air,
                Density = 0.25f,
                AllowedEntities = new List<string> { "spike", "spinner", "seeker" }
            });
        }
        return zones;
    }

    private static List<PlacementZone> AnalyzeEntityZones(int width, int height, string solidTiles, int cols, int rows)
    {
        var zones = new List<PlacementZone>();
        zones.Add(new PlacementZone
        {
            X = width / 4,
            Y = height / 4,
            Width = width / 2,
            Height = height / 2,
            Type = ZoneType.Air,
            Density = 0.3f,
            AllowedEntities = new List<string> { "strawberry", "refill", "key", "spring" }
        });
        return zones;
    }

    private static List<PlacementZone> AnalyzePlatformZones(int width, int height, string solidTiles, int cols, int rows)
    {
        var zones = new List<PlacementZone>();
        // Mid-air zones where platforms make sense
        zones.Add(new PlacementZone
        {
            X = width / 4,
            Y = height / 3,
            Width = width / 2,
            Height = height / 3,
            Type = ZoneType.Air,
            Density = 0.15f,
            AllowedEntities = new List<string> { "movingPlatform", "crumbleBlock", "bounceBlock" }
        });
        return zones;
    }

    private static int CountSolidInRow(string tiles, int cols, int row)
    {
        int count = 0;
        int start = row * cols;
        for (int i = 0; i < cols && start + i < tiles.Length; i++)
            if (tiles[start + i] != '0') count++;
        return count;
    }

    // --- Required elements ---
    private static List<RequiredElement> IdentifyRequiredElements(List<BinaryPacker.Element> entities, List<BinaryPacker.Element> triggers)
    {
        var elements = new List<RequiredElement>();
        var all = entities.Concat(triggers).ToList();

        bool hasCheckpoint = all.Any(e => e.Name == "checkpoint");
        bool hasPlayer = all.Any(e => e.Name == "player");
        bool hasStrawberry = all.Any(e => e.Name == "strawberry");
        bool hasRefill = all.Any(e => e.Name == "refill");

        if (hasCheckpoint)
            elements.Add(new RequiredElement { EntityType = "checkpoint", MinCount = 1, MaxCount = 1, Preference = PlacementPreference.GroundLevel });
        if (hasPlayer)
            elements.Add(new RequiredElement { EntityType = "player", MinCount = 1, MaxCount = 1, Preference = PlacementPreference.GroundLevel });
        if (hasStrawberry)
            elements.Add(new RequiredElement { EntityType = "strawberry", MinCount = 1, MaxCount = 3, Preference = PlacementPreference.MidAir });
        if (hasRefill)
            elements.Add(new RequiredElement { EntityType = "refill", MinCount = 0, MaxCount = 2, Preference = PlacementPreference.Anywhere });

        return elements;
    }

    // --- Inference ---
    private static DifficultyTier InferDifficulty(List<BinaryPacker.Element> entities, string solidTiles, int width, int height)
    {
        int hazards = entities.Count(e => e.Name is "spike" or "spinner" or "seeker" or "blade" or "fireball" or "fallingBlock");
        int strawberries = entities.Count(e => e.Name == "strawberry");

        if (hazards > 20) return DifficultyTier.Master;
        if (hazards > 12) return DifficultyTier.Expert;
        if (hazards > 6) return DifficultyTier.Hard;
        if (hazards > 2) return DifficultyTier.Normal;
        if (strawberries > 0 || hazards > 0) return DifficultyTier.Easy;
        return DifficultyTier.Tutorial;
    }

    private static RoomType InferRoomType(List<BinaryPacker.Element> entities, int width, int height)
    {
        bool hasCheckpoint = entities.Any(e => e.Name == "checkpoint");
        bool hasStrawberry = entities.Any(e => e.Name == "strawberry");
        bool hasBoss = entities.Any(e => e.Name.Contains("Boss") || e.Name == "finalBoss" || e.Name == "badelineBoss");
        bool hasSpring = entities.Any(e => e.Name == "spring");
        int spikes = entities.Count(e => e.Name is "spike" or "spinner");

        if (hasCheckpoint) return RoomType.Checkpoint;
        if (hasBoss) return RoomType.Boss;
        if (hasStrawberry && spikes > 5) return RoomType.Challenge;
        if (hasSpring && spikes == 0) return RoomType.Puzzle;
        if (width < 200 && height < 200 && !hasStrawberry) return RoomType.Secret;
        if (width > 400 && height < 200 && spikes == 0) return RoomType.Transition;
        return RoomType.Standard;
    }

    private static float CalculateVerticality(string solidTiles, int cols, int rows)
    {
        if (solidTiles.Length == 0) return 0.5f;
        int solidCount = solidTiles.Count(c => c != '0');
        float ratio = solidCount / (float)(cols * rows);
        return 1f - ratio; // more open = more vertical potential
    }

    private static float CalculateOpenness(string solidTiles, int cols, int rows)
    {
        if (solidTiles.Length == 0) return 0.5f;
        int solidCount = solidTiles.Count(c => c != '0');
        return 1f - (solidCount / (float)(cols * rows));
    }
    
    /// <summary>
    /// Save templates to JSON file
    /// </summary>
    public static void SaveTemplatesToJson(List<RoomTemplate> templates, string outputPath)
    {
        // Simple JSON serialization
        // In production, use a proper JSON library
        var json = System.Text.Json.JsonSerializer.Serialize(templates, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        
        File.WriteAllText(outputPath, json);
    }
    
    /// <summary>
    /// Load templates from JSON file
    /// </summary>
    public static List<RoomTemplate> LoadTemplatesFromJson(string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            Logger.Log(LogLevel.Error, LogTag, $"Template file not found: {jsonPath}");
            return new List<RoomTemplate>();
        }
        
        try
        {
            var json = File.ReadAllText(jsonPath);
            var templates = System.Text.Json.JsonSerializer.Deserialize<List<RoomTemplate>>(json);
            Logger.Log(LogLevel.Info, LogTag, $"Loaded {templates.Count} templates from: {jsonPath}");
            return templates;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"Failed to load templates: {ex.Message}");
            return new List<RoomTemplate>();
        }
    }
}
