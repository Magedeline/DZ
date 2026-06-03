using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE;

/// <summary>
/// Connects room templates together to form a complete map
/// </summary>
public static class RoomConnector
{
    private const string LogTag = "RoomConnector";
    
    /// <summary>
    /// Connect rooms together based on their connection points
    /// </summary>
    public static List<RoomConnection> ConnectRooms(
        List<RoomTemplate> templates, 
        int seed, 
        ConnectionStrategy strategy = ConnectionStrategy.Pathway)
    {
        var connections = new List<RoomConnection>();
        var rng = new Random(seed);
        
        Logger.Log(LogLevel.Info, LogTag, $"Connecting {templates.Count} rooms using {strategy} strategy");
        
        switch (strategy)
        {
            case ConnectionStrategy.Pathway:
                connections = ConnectPathway(templates, rng);
                break;
            case ConnectionStrategy.Labyrinth:
                connections = ConnectLabyrinth(templates, rng);
                break;
            case ConnectionStrategy.Branching:
                connections = ConnectBranching(templates, rng);
                break;
            case ConnectionStrategy.Linear:
                connections = ConnectLinear(templates, rng);
                break;
        }
        
        Logger.Log(LogLevel.Info, LogTag, $"Created {connections.Count} room connections");
        return connections;
    }
    
    /// <summary>
    /// Pathway strategy: Create a straight-line path with optional side routes
    /// </summary>
    private static List<RoomConnection> ConnectPathway(List<RoomTemplate> templates, Random rng)
    {
        var connections = new List<RoomConnection>();
        var unconnected = new List<RoomTemplate>(templates);
        
        if (unconnected.Count == 0)
            return connections;
        
        // Start with a checkpoint room
        var startRoom = unconnected.FirstOrDefault(r => r.Type == RoomType.Checkpoint) ?? unconnected[0];
        unconnected.Remove(startRoom);
        
        var currentRoom = startRoom;
        var visited = new HashSet<RoomTemplate> { startRoom };
        
        // Create main pathway
        while (unconnected.Count > 0)
        {
            var nextRoom = SelectNextRoom(currentRoom, unconnected, rng);
            if (nextRoom == null)
                break;
            
            var connection = CreateConnection(currentRoom, nextRoom, rng);
            if (connection != null)
            {
                connections.Add(connection);
                visited.Add(nextRoom);
                unconnected.Remove(nextRoom);
                currentRoom = nextRoom;
            }
            else
            {
                // Can't connect, try different room
                unconnected.Remove(nextRoom);
            }
        }
        
        // Add side routes for keys/collectibles
        var sideRooms = unconnected.Where(r => r.Type == RoomType.Secret || r.Type == RoomType.Puzzle).ToList();
        foreach (var sideRoom in sideRooms)
        {
            var hubRoom = visited.Where(r => r.Connections.Count > 2).FirstOrDefault();
            if (hubRoom != null)
            {
                var connection = CreateConnection(hubRoom, sideRoom, rng);
                if (connection != null)
                {
                    connections.Add(connection);
                }
            }
        }
        
        return connections;
    }
    
    /// <summary>
    /// Labyrinth strategy: Create a sprawling map with central hub
    /// </summary>
    private static List<RoomConnection> ConnectLabyrinth(List<RoomTemplate> templates, Random rng)
    {
        var connections = new List<RoomConnection>();
        var unconnected = new List<RoomTemplate>(templates);
        
        if (unconnected.Count == 0)
            return connections;
        
        // Create central hub
        var hubRoom = unconnected.FirstOrDefault(r => r.Type == RoomType.Standard) ?? unconnected[0];
        unconnected.Remove(hubRoom);
        
        // Connect rooms to hub
        foreach (var room in unconnected)
        {
            var connection = CreateConnection(hubRoom, room, rng);
            if (connection != null)
            {
                connections.Add(connection);
            }
        }
        
        // Add interconnections between non-hub rooms
        var nonHubRooms = unconnected.ToList();
        for (int i = 0; i < nonHubRooms.Count - 1; i++)
        {
            if (rng.NextDouble() < 0.3) // 30% chance of interconnection
            {
                var connection = CreateConnection(nonHubRooms[i], nonHubRooms[i + 1], rng);
                if (connection != null)
                {
                    connections.Add(connection);
                }
            }
        }
        
        return connections;
    }
    
    /// <summary>
    /// Branching strategy: Create a tree-like structure
    /// </summary>
    private static List<RoomConnection> ConnectBranching(List<RoomTemplate> templates, Random rng)
    {
        var connections = new List<RoomConnection>();
        var unconnected = new List<RoomTemplate>(templates);
        
        if (unconnected.Count == 0)
            return connections;
        
        // Start with root
        var root = unconnected[0];
        unconnected.Remove(root);
        
        var queue = new Queue<RoomTemplate>();
        queue.Enqueue(root);
        
        while (queue.Count > 0 && unconnected.Count > 0)
        {
            var current = queue.Dequeue();
            
            // Connect 1-3 children
            int childCount = rng.Next(1, Math.Min(4, unconnected.Count + 1));
            
            for (int i = 0; i < childCount && unconnected.Count > 0; i++)
            {
                var child = SelectNextRoom(current, unconnected, rng);
                if (child != null)
                {
                    var connection = CreateConnection(current, child, rng);
                    if (connection != null)
                    {
                        connections.Add(connection);
                        queue.Enqueue(child);
                        unconnected.Remove(child);
                    }
                }
            }
        }
        
        return connections;
    }
    
    /// <summary>
    /// Linear strategy: Simple linear progression
    /// </summary>
    private static List<RoomConnection> ConnectLinear(List<RoomTemplate> templates, Random rng)
    {
        var connections = new List<RoomConnection>();
        
        for (int i = 0; i < templates.Count - 1; i++)
        {
            var connection = CreateConnection(templates[i], templates[i + 1], rng);
            if (connection != null)
            {
                connections.Add(connection);
            }
        }
        
        return connections;
    }
    
    /// <summary>
    /// Select the next room to connect based on compatibility
    /// </summary>
    private static RoomTemplate SelectNextRoom(RoomTemplate current, List<RoomTemplate> candidates, Random rng)
    {
        if (candidates.Count == 0)
            return null;
        
        // Score candidates based on compatibility
        var scored = candidates.Select(c => new
        {
            Room = c,
            Score = CalculateCompatibilityScore(current, c)
        }).OrderByDescending(x => x.Score).ToList();
        
        // Select from top 3 candidates randomly
        int topCount = Math.Min(3, scored.Count);
        var topCandidates = scored.Take(topCount).ToList();
        int selectedIndex = rng.Next(topCandidates.Count);
        
        return topCandidates[selectedIndex].Room;
    }
    
    /// <summary>
    /// Calculate compatibility score between two rooms
    /// </summary>
    private static float CalculateCompatibilityScore(RoomTemplate roomA, RoomTemplate roomB)
    {
        float score = 0f;
        
        // Check if rooms have compatible connection points
        var compatibleConnections = 0;
        foreach (var connA in roomA.Connections.Where(c => c.IsExit))
        {
            foreach (var connB in roomB.Connections.Where(c => c.IsEntrance))
            {
                if (AreConnectionsCompatible(connA, connB))
                {
                    compatibleConnections++;
                    score += 10f;
                }
            }
        }
        
        // Prefer similar difficulty
        int difficultyDiff = Math.Abs((int)roomA.Difficulty - (int)roomB.Difficulty);
        score -= difficultyDiff * 2f;
        
        // Prefer room type progression
        if (roomA.Type == RoomType.Checkpoint && roomB.Type == RoomType.Standard)
            score += 5f;
        if (roomA.Type == RoomType.Standard && roomB.Type == RoomType.Challenge)
            score += 3f;
        
        return score;
    }
    
    /// <summary>
    /// Check if two connection points are compatible
    /// </summary>
    private static bool AreConnectionsCompatible(ConnectionPoint a, ConnectionPoint b)
    {
        // Opposite directions can connect
        if (a.Direction == Direction.Left && b.Direction == Direction.Right)
            return true;
        if (a.Direction == Direction.Right && b.Direction == Direction.Left)
            return true;
        if (a.Direction == Direction.Up && b.Direction == Direction.Down)
            return true;
        if (a.Direction == Direction.Down && b.Direction == Direction.Up)
            return true;
        
        return false;
    }
    
    /// <summary>
    /// Create a connection between two rooms
    /// </summary>
    private static RoomConnection CreateConnection(RoomTemplate from, RoomTemplate to, Random rng)
    {
        // Find compatible connection points
        var fromExit = from.Connections.FirstOrDefault(c => c.IsExit);
        var toEntrance = to.Connections.FirstOrDefault(c => c.IsEntrance);
        
        if (fromExit == null || toEntrance == null)
        {
            // Create default connection
            fromExit = new ConnectionPoint
            {
                Direction = Direction.Right,
                X = from.Width,
                Y = from.Height / 2,
                Width = 40,
                Type = ConnectionType.Normal,
                IsExit = true
            };
            
            toEntrance = new ConnectionPoint
            {
                Direction = Direction.Left,
                X = 0,
                Y = to.Height / 2,
                Width = 40,
                Type = ConnectionType.Normal,
                IsEntrance = true
            };
        }
        
        return new RoomConnection
        {
            FromRoom = from.Name,
            ToRoom = to.Name,
            FromConnection = fromExit,
            ToConnection = toEntrance,
            ConnectionType = fromExit.Type,
            RequiresKey = rng.NextDouble() < 0.1 // 10% chance of requiring key
        };
    }
}

/// <summary>
/// Represents a connection between two rooms
/// </summary>
public class RoomConnection
{
    public string FromRoom { get; set; }
    public string ToRoom { get; set; }
    public ConnectionPoint FromConnection { get; set; }
    public ConnectionPoint ToConnection { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public bool RequiresKey { get; set; }
    public string KeyId { get; set; }
}

public enum ConnectionStrategy
{
    Pathway,
    Labyrinth,
    Branching,
    Linear
}
