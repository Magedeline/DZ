using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE;

/// <summary>
/// Room template for hybrid PCG - defines base room structure with procedural placement zones
/// </summary>
public class RoomTemplate
{
    public string Name { get; set; }
    public string SourceMap { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DifficultyTier Difficulty { get; set; }
    public RoomType Type { get; set; }
    
    // Connection points for room linking
    public List<ConnectionPoint> Connections { get; set; }
    
    // Procedural placement zones
    public List<PlacementZone> HazardZones { get; set; }
    public List<PlacementZone> EntityZones { get; set; }
    public List<PlacementZone> PlatformZones { get; set; }
    
    // Required gameplay elements
    public List<RequiredElement> RequiredElements { get; set; }
    
    // Template metadata
    public Dictionary<string, string> Tags { get; set; }
    
    public RoomTemplate()
    {
        Connections = new List<ConnectionPoint>();
        HazardZones = new List<PlacementZone>();
        EntityZones = new List<PlacementZone>();
        PlatformZones = new List<PlacementZone>();
        RequiredElements = new List<RequiredElement>();
        Tags = new Dictionary<string, string>();
    }
}

/// <summary>
/// Connection point for linking rooms together
/// </summary>
public class ConnectionPoint
{
    public Direction Direction { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public ConnectionType Type { get; set; }
    public bool IsEntrance { get; set; }
    public bool IsExit { get; set; }
}

/// <summary>
/// Zone where procedural elements can be placed
/// </summary>
public class PlacementZone
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public ZoneType Type { get; set; }
    public float Density { get; set; }
    public List<string> AllowedEntities { get; set; }
    public List<string> ForbiddenEntities { get; set; }
    
    public PlacementZone()
    {
        AllowedEntities = new List<string>();
        ForbiddenEntities = new List<string>();
    }
}

/// <summary>
/// Required gameplay element that must be present
/// </summary>
public class RequiredElement
{
    public string EntityType { get; set; }
    public int MinCount { get; set; }
    public int MaxCount { get; set; }
    public PlacementPreference Preference { get; set; }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public enum ConnectionType
{
    Normal,
    Warp,
    Dash,
    Climb
}

public enum RoomType
{
    Standard,
    Puzzle,
    Challenge,
    Transition,
    Boss,
    Secret,
    Checkpoint
}

public enum DifficultyTier
{
    Tutorial,
    Easy,
    Normal,
    Hard,
    Expert,
    Master
}

public enum ZoneType
{
    Ground,
    Air,
    Wall,
    Ceiling,
    Water,
    Lava
}

public enum PlacementPreference
{
    Anywhere,
    GroundLevel,
    MidAir,
    NearWalls,
    Centered,
    Edges
}
