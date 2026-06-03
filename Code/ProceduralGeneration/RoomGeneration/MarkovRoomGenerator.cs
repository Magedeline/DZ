using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.MarkovChain;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.ProceduralGeneration.RoomGeneration;

/// <summary>
/// Markov-based room layout generator for coherent level design
/// </summary>
public class MarkovRoomGenerator
{
    private const string LogTag = "MarkovRoomGenerator";
    
    private readonly MarkovChain<string> _roomTypeChain;
    private readonly MarkovChain<string> _layoutChain;
    private readonly MarkovChain<string> _difficultyChain;
    private readonly RoomTemplateLibrary _templateLibrary;
    private readonly Random _random;
    private readonly int _seed;
    
    private readonly List<GeneratedRoom> _generatedRooms;
    private int _roomCounter;
    
    public MarkovRoomGenerator(int seed = 0)
    {
        _seed = seed;
        _random = seed == 0 ? new Random() : new Random(seed);
        
        _roomTypeChain = new MarkovChain<string>("RoomTypes", seed);
        _layoutChain = new MarkovChain<string>("RoomLayouts", seed);
        _difficultyChain = MarkovChainManager.CreateDifficultyChain("RoomDifficulty", seed);
        
        _templateLibrary = new RoomTemplateLibrary();
        _generatedRooms = new List<GeneratedRoom>();
        _roomCounter = 0;
        
        InitializeChains();
    }
    
    /// <summary>
    /// Initialize Markov chains for room generation
    /// </summary>
    private void InitializeChains()
    {
        var roomTypes = new[]
        {
            "Platforming", "Combat", "Puzzle", "Boss", "Rest",
            "Secret", "Challenge", "Cinematic", "Transition", "Treasure"
        };
        
        var layouts = new[]
        {
            "Linear", "Branching", "Vertical", "Horizontal", "Spiral",
            "Loop", "Arena", "Corridor", "Open", "Maze"
        };
        
        // Initialize room type chain
        foreach (var fromType in roomTypes)
        {
            foreach (var toType in roomTypes)
            {
                float probability = CalculateRoomTypeTransition(fromType, toType);
                _roomTypeChain.AddTransition(fromType, toType, probability);
            }
        }
        
        // Initialize layout chain
        foreach (var fromLayout in layouts)
        {
            foreach (var toLayout in layouts)
            {
                float probability = CalculateLayoutTransition(fromLayout, toLayout);
                _layoutChain.AddTransition(fromLayout, toLayout, probability);
            }
        }
        
        _roomTypeChain.Initialize("Platforming");
        _layoutChain.Initialize("Linear");
        
        Logger.Log(LogLevel.Info, LogTag, "Initialized room generation chains");
    }
    
    /// <summary>
    /// Calculate room type transition probabilities
    /// </summary>
    private float CalculateRoomTypeTransition(string from, string to)
    {
        // Rest rooms after combat
        if ((from == "Combat" || from == "Boss") && to == "Rest")
            return 0.35f;
        
        // Boss rooms are significant and rare
        if (to == "Boss")
            return 0.03f;
        
        // No consecutive boss rooms
        if (from == "Boss" && to == "Boss")
            return 0f;
        
        // Cinematic after boss
        if (from == "Boss" && to == "Cinematic")
            return 0.4f;
        
        // Secret rooms are rare
        if (to == "Secret")
            return 0.05f;
        
        // Platforming is common
        if (from == "Platforming" || to == "Platforming")
            return 0.15f;
        
        // Combat often followed by rest or platforming
        if (from == "Combat" && (to == "Rest" || to == "Platforming"))
            return 0.25f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.08f;
        
        return 0.1f;
    }
    
    /// <summary>
    /// Calculate layout transition probabilities
    /// </summary>
    private float CalculateLayoutTransition(string from, string to)
    {
        // Linear layouts often transition to branching
        if (from == "Linear" && to == "Branching")
            return 0.3f;
        
        // Arena layouts often followed by corridors
        if (from == "Arena" && to == "Corridor")
            return 0.25f;
        
        // Vertical often followed by horizontal for variety
        if (from == "Vertical" && to == "Horizontal")
            return 0.35f;
        
        // Spiral layouts are rare
        if (to == "Spiral" || to == "Loop")
            return 0.08f;
        
        // Prevent immediate repetition
        if (from == to)
            return 0.05f;
        
        return 0.12f;
    }
    
    /// <summary>
    /// Generate a complete level with coherent room sequence
    /// </summary>
    public GeneratedLevel GenerateLevel(int roomCount, string levelName, DifficultyTier startingDifficulty)
    {
        var level = new GeneratedLevel
        {
            Name = levelName,
            StartingDifficulty = startingDifficulty
        };
        
        // Generate room sequence
        var roomSequence = GenerateRoomSequence(roomCount);
        
        // Generate each room
        foreach (var roomData in roomSequence)
        {
            var room = GenerateRoom(roomData);
            level.Rooms.Add(room);
            _generatedRooms.Add(room);
        }
        
        // Generate room connections
        GenerateRoomConnections(level);
        
        // Validate level solvability
        ValidateLevelSolvability(level);
        
        Logger.Log(LogLevel.Info, LogTag, 
            $"Generated level '{levelName}' with {level.Rooms.Count} rooms");
        
        return level;
    }
    
    /// <summary>
    /// Generate a sequence of room data
    /// </summary>
    private List<RoomData> GenerateRoomSequence(int count)
    {
        var sequence = new List<RoomData>();
        var currentDifficulty = DifficultyTier.Normal;

        for (int i = 0; i < count; i++)
        {
            // Get room type from Markov chain
            var roomType = _roomTypeChain.GetNextState();
            if (roomType == null) roomType = "Platforming";

            // Get layout from Markov chain
            var layout = _layoutChain.GetNextState();
            if (layout == null) layout = "Linear";

            // Get difficulty progression
            var difficultyStr = _difficultyChain.GetNextState();
            if (difficultyStr != null && Enum.TryParse<DifficultyTier>(difficultyStr, out var parsedDiff))
            {
                currentDifficulty = parsedDiff;
            }

            // Adjust difficulty based on room type
            var adjustedDifficulty = AdjustDifficultyForRoomType(currentDifficulty, roomType);
            
            sequence.Add(new RoomData
            {
                Index = i,
                Type = roomType,
                Layout = layout,
                Difficulty = adjustedDifficulty
            });
        }
        
        return sequence;
    }
    
    /// <summary>
    /// Adjust difficulty based on room type
    /// </summary>
    private DifficultyTier AdjustDifficultyForRoomType(DifficultyTier baseDifficulty, string roomType)
    {
        int difficulty = (int)baseDifficulty;
        
        // Boss rooms are harder
        if (roomType == "Boss")
            difficulty = Math.Min(10, difficulty + 2);
        
        // Rest rooms are easier
        if (roomType == "Rest")
            difficulty = Math.Max(1, difficulty - 2);
        
        // Challenge rooms are harder
        if (roomType == "Challenge")
            difficulty = Math.Min(10, difficulty + 1);
        
        return (DifficultyTier)Math.Clamp(difficulty, 1, 10);
    }
    
    /// <summary>
    /// Generate a single room from room data
    /// </summary>
    private GeneratedRoom GenerateRoom(RoomData roomData)
    {
        var template = _templateLibrary.GetTemplate(roomData.Type, roomData.Layout);
        
        var room = new GeneratedRoom
        {
            Id = $"Room_{_roomCounter++}",
            Name = $"{roomData.Type}_{roomData.Layout}_{roomData.Index}",
            Type = roomData.Type,
            Layout = roomData.Layout,
            Difficulty = roomData.Difficulty,
            Width = template?.Width ?? 640,
            Height = template?.Height ?? 360,
            Exits = GenerateRoomExits(roomData.Layout),
            Checkpoints = GenerateRoomCheckpoints(roomData.Type, roomData.Difficulty)
        };
        
        // Use Markov-enhanced procedural placement
        if (template != null)
        {
            room.Entities = MarkovProceduralPlacement.PlaceEntitiesWithMarkov(
                template, _seed + _roomCounter, roomData.Difficulty);
            
            room.Hazards = MarkovProceduralPlacement.PlaceHazardsWithMarkov(
                template, _seed + _roomCounter, roomData.Difficulty);
        }
        
        Logger.Log(LogLevel.Verbose, LogTag, 
            $"Generated room: {room.Name} ({roomData.Type}, {roomData.Layout})");
        
        return room;
    }
    
    /// <summary>
    /// Generate room exits based on layout
    /// </summary>
    private List<RoomExit> GenerateRoomExits(string layout)
    {
        var exits = new List<RoomExit>();
        
        return layout switch
        {
            "Linear" => new List<RoomExit> { 
                new RoomExit { Direction = "Right", Position = new Vector2(620, 180) } 
            },
            "Branching" => new List<RoomExit> { 
                new RoomExit { Direction = "Right", Position = new Vector2(620, 180) },
                new RoomExit { Direction = "Up", Position = new Vector2(320, 20) }
            },
            "Arena" => new List<RoomExit> { 
                new RoomExit { Direction = "Right", Position = new Vector2(620, 180) },
                new RoomExit { Direction = "Left", Position = new Vector2(20, 180) }
            },
            "Vertical" => new List<RoomExit> { 
                new RoomExit { Direction = "Up", Position = new Vector2(320, 20) }
            },
            _ => new List<RoomExit> { 
                new RoomExit { Direction = "Right", Position = new Vector2(620, 180) }
            }
        };
    }
    
    /// <summary>
    /// Generate room checkpoints based on type and difficulty
    /// </summary>
    private List<Vector2> GenerateRoomCheckpoints(string roomType, DifficultyTier difficulty)
    {
        var checkpoints = new List<Vector2>();
        
        // More checkpoints in difficult rooms
        int checkpointCount = difficulty >= DifficultyTier.Hard ? 2 : 1;
        
        // Rest rooms have checkpoints
        if (roomType == "Rest")
        {
            checkpoints.Add(new Vector2(320, 300));
        }
        
        // Boss rooms have checkpoints before them
        if (roomType == "Boss")
        {
            checkpoints.Add(new Vector2(100, 180));
        }
        
        for (int i = 0; i < checkpointCount; i++)
        {
            float x = 150 + (i * 170);
            float y = 180 + (_random.Next(-50, 51));
            checkpoints.Add(new Vector2(x, y));
        }
        
        return checkpoints;
    }
    
    /// <summary>
    /// Generate connections between rooms
    /// </summary>
    private void GenerateRoomConnections(GeneratedLevel level)
    {
        for (int i = 0; i < level.Rooms.Count - 1; i++)
        {
            var currentRoom = level.Rooms[i];
            var nextRoom = level.Rooms[i + 1];
            
            // Find compatible exits
            var currentExit = currentRoom.Exits.FirstOrDefault(e => e.Direction == "Right");
            var nextExit = nextRoom.Exits.FirstOrDefault(e => e.Direction == "Left");
            
            if (currentExit != null)
            {
                level.Connections.Add(new RoomConnection
                {
                    FromRoom = currentRoom.Id,
                    ToRoom = nextRoom.Id,
                    FromExit = currentExit,
                    ToExit = nextExit,
                    Type = "Direct"
                });
            }
        }
    }
    
    /// <summary>
    /// Validate that the level is solvable
    /// </summary>
    private void ValidateLevelSolvability(GeneratedLevel level)
    {
        // Basic validation: ensure all rooms are reachable
        var reachableRooms = new HashSet<string> { level.Rooms[0].Id };
        var queue = new Queue<string>();
        queue.Enqueue(level.Rooms[0].Id);
        
        while (queue.Count > 0)
        {
            var currentRoomId = queue.Dequeue();
            var connections = level.Connections.Where(c => c.FromRoom == currentRoomId);
            
            foreach (var connection in connections)
            {
                if (!reachableRooms.Contains(connection.ToRoom))
                {
                    reachableRooms.Add(connection.ToRoom);
                    queue.Enqueue(connection.ToRoom);
                }
            }
        }
        
        if (reachableRooms.Count < level.Rooms.Count)
        {
            Logger.Log(LogLevel.Warn, LogTag, 
                $"Level '{level.Name}' has unreachable rooms. " +
                $"Reachable: {reachableRooms.Count}/{level.Rooms.Count}");
        }
        else
        {
            Logger.Log(LogLevel.Info, LogTag, 
                $"Level '{level.Name}' is fully reachable and solvable");
        }
    }
    
    /// <summary>
    /// Generate a single room for testing
    /// </summary>
    public GeneratedRoom GenerateSingleRoom(string roomType, string layout, DifficultyTier difficulty)
    {
        var roomData = new RoomData
        {
            Index = 0,
            Type = roomType,
            Layout = layout,
            Difficulty = difficulty
        };
        
        return GenerateRoom(roomData);
    }
    
    /// <summary>
    /// Get statistics about generated rooms
    /// </summary>
    public RoomGenerationStats GetStats()
    {
        return new RoomGenerationStats
        {
            TotalRoomsGenerated = _generatedRooms.Count,
            RoomsByType = _generatedRooms.GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            RoomsByLayout = _generatedRooms.GroupBy(r => r.Layout)
                .ToDictionary(g => g.Key, g => g.Count()),
            RoomTypeChainStats = _roomTypeChain.GetStats(),
            LayoutChainStats = _layoutChain.GetStats(),
            DifficultyChainStats = _difficultyChain.GetStats()
        };
    }
}

/// <summary>
/// Room data for generation
/// </summary>
public class RoomData
{
    public int Index { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Layout { get; set; } = string.Empty;
    public DifficultyTier Difficulty { get; set; }
}

/// <summary>
/// Generated room
/// </summary>
public class GeneratedRoom
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Layout { get; set; } = string.Empty;
    public DifficultyTier Difficulty { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<RoomExit> Exits { get; set; } = new();
    public List<Vector2> Checkpoints { get; set; } = new();
    public List<EntityPlacement> Entities { get; set; } = new();
    public List<EntityPlacement> Hazards { get; set; } = new();
    public List<EnemyGeneration.EnemyVariant> GeneratedEnemies { get; set; } = new();
    public List<PlatformGeneration.DynamicPlatform> GeneratedPlatforms { get; set; } = new();
    public BackgroundGeneration.GeneratedBackground GeneratedBackground { get; set; }
}

/// <summary>
/// Room exit
/// </summary>
public class RoomExit
{
    public string Direction { get; set; } = string.Empty;
    public Vector2 Position { get; set; }
}

/// <summary>
/// Room connection
/// </summary>
public class RoomConnection
{
    public string FromRoom { get; set; } = string.Empty;
    public string ToRoom { get; set; } = string.Empty;
    public RoomExit? FromExit { get; set; }
    public RoomExit? ToExit { get; set; }
    public string Type { get; set; } = string.Empty; // Direct, Warp, Secret
}

/// <summary>
/// Generated level
/// </summary>
public class GeneratedLevel
{
    public string Name { get; set; } = string.Empty;
    public DifficultyTier StartingDifficulty { get; set; }
    public List<GeneratedRoom> Rooms { get; set; } = new();
    public List<RoomConnection> Connections { get; set; } = new();
}

/// <summary>
/// Room template library
/// </summary>
public class RoomTemplateLibrary
{
    private readonly Dictionary<string, RoomTemplate> _templates;
    
    public RoomTemplateLibrary()
    {
        _templates = new Dictionary<string, RoomTemplate>();
        InitializeTemplates();
    }
    
    private void InitializeTemplates()
    {
        // Create basic templates
        _templates["Platforming_Linear"] = new RoomTemplate
        {
            Name = "Platforming_Linear",
            Type = RoomType.Standard,
            Width = 640,
            Height = 360,
            RequiredElements = new List<RequiredElement>(),
            EntityZones = new List<PlacementZone>(),
            HazardZones = new List<PlacementZone>(),
            PlatformZones = new List<PlacementZone>()
        };

        _templates["Combat_Arena"] = new RoomTemplate
        {
            Name = "Combat_Arena",
            Type = RoomType.Challenge,
            Width = 800,
            Height = 480,
            RequiredElements = new List<RequiredElement>(),
            EntityZones = new List<PlacementZone>(),
            HazardZones = new List<PlacementZone>(),
            PlatformZones = new List<PlacementZone>()
        };
        
        // Add more templates as needed...
    }
    
    public RoomTemplate? GetTemplate(string roomType, string layout)
    {
        string key = $"{roomType}_{layout}";
        return _templates.TryGetValue(key, out var template) ? template : null;
    }
}

/// <summary>
/// Statistics for room generation
/// </summary>
public class RoomGenerationStats
{
    public int TotalRoomsGenerated { get; set; }
    public Dictionary<string, int> RoomsByType { get; set; } = new();
    public Dictionary<string, int> RoomsByLayout { get; set; } = new();
    public MarkovChainStats RoomTypeChainStats { get; set; } = new();
    public MarkovChainStats LayoutChainStats { get; set; } = new();
    public MarkovChainStats DifficultyChainStats { get; set; } = new();
    
    public override string ToString()
    {
        return $"Total Rooms: {TotalRoomsGenerated}, " +
               $"Types: {string.Join(", ", RoomsByType.Select(kvp => $"{kvp.Key}={kvp.Value}"))}, " +
               $"Layouts: {string.Join(", ", RoomsByLayout.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
    }
}