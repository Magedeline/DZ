using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE;

/// <summary>
/// Builds a <see cref="BinaryPacker.Element"/> tree from PCG data that can be
/// written to a Celeste .bin file via <see cref="MapBinaryWriter"/>.
/// </summary>
public static class MapBuilder
{
    private const string LogTag = "MapBuilder";

    /// <summary>
    /// Build a map element tree from templates, connections, and placements.
    /// </summary>
    public static BinaryPacker.Element BuildMap(
        string packageName,
        List<RoomTemplate> rooms,
        List<RoomConnection> connections,
        List<EntityPlacement> placements,
        string defaultMusic = "event:/music/lvl1/main",
        string defaultAmbience = "event:/env/amb/00_prologue")
    {
        var root = new BinaryPacker.Element
        {
            Name = "Map",
            Package = packageName,
            Children = new List<BinaryPacker.Element>()
        };

        // Compute absolute positions for each room
        var roomPositions = ComputeRoomPositions(rooms, connections);

        var levelsElement = new BinaryPacker.Element
        {
            Name = "levels",
            Children = new List<BinaryPacker.Element>()
        };
        root.Children.Add(levelsElement);

        int strawberryOrder = 0;

        foreach (var room in rooms)
        {
            if (!roomPositions.TryGetValue(room.Name, out Vector2 pos))
                pos = Vector2.Zero;

            var level = BuildLevelElement(
                room,
                (int)pos.X,
                (int)pos.Y,
                placements.Where(p => p.RoomName == room.Name).ToList(),
                defaultMusic,
                defaultAmbience,
                ref strawberryOrder);

            levelsElement.Children.Add(level);
        }

        // Add empty Filler and Style nodes so MapData doesn't choke
        root.Children.Add(new BinaryPacker.Element { Name = "Filler" });
        root.Children.Add(new BinaryPacker.Element { Name = "Style" });

        // Ensure at least one player spawn exists somewhere in the map
        EnsurePlayerSpawn(levelsElement);

        Logger.Log(LogLevel.Info, LogTag, $"Built map with {rooms.Count} rooms, {placements.Count} placements");
        return root;
    }

    private static void EnsurePlayerSpawn(BinaryPacker.Element levelsElement)
    {
        if (levelsElement.Children == null || levelsElement.Children.Count == 0)
            return;

        bool hasPlayer = false;
        foreach (var level in levelsElement.Children)
        {
            var entities = level.Children?.FirstOrDefault(c => c.Name == "entities");
            if (entities?.Children != null && entities.Children.Any(e => e.Name == "player"))
            {
                hasPlayer = true;
                break;
            }
        }

        if (!hasPlayer)
        {
            var firstLevel = levelsElement.Children[0];
            var entities = firstLevel.Children?.FirstOrDefault(c => c.Name == "entities");
            if (entities != null)
            {
                int height = firstLevel.Attributes != null && firstLevel.Attributes.TryGetValue("height", out var h) ? Convert.ToInt32(h) : 180;
                entities.Children.Insert(0, new BinaryPacker.Element
                {
                    Name = "player",
                    Attributes = new Dictionary<string, object>
                    {
                        ["x"] = 16,
                        ["y"] = height - 16
                    }
                });
            }
        }
    }

    #region Room Layout

    /// <summary>
    /// Compute absolute map positions for each room by walking the connection graph.
    /// </summary>
    private static Dictionary<string, Vector2> ComputeRoomPositions(
        List<RoomTemplate> rooms,
        List<RoomConnection> connections)
    {
        var positions = new Dictionary<string, Vector2>();
        var lookup = rooms.ToDictionary(r => r.Name);

        if (rooms.Count == 0)
            return positions;

        // Start with the first room (prefer checkpoint room)
        var startRoom = rooms.FirstOrDefault(r => r.Type == RoomType.Checkpoint) ?? rooms[0];
        positions[startRoom.Name] = Vector2.Zero;

        var visited = new HashSet<string> { startRoom.Name };
        var queue = new Queue<string>();
        queue.Enqueue(startRoom.Name);

        while (queue.Count > 0)
        {
            var currentName = queue.Dequeue();
            if (!lookup.TryGetValue(currentName, out var currentRoom))
                continue;
            if (!positions.TryGetValue(currentName, out var currentPos))
                continue;

            foreach (var conn in connections.Where(c => c.FromRoom == currentName || c.ToRoom == currentName))
            {
                bool fromCurrent = conn.FromRoom == currentName;
                var otherName = fromCurrent ? conn.ToRoom : conn.FromRoom;
                var fromRoom = fromCurrent ? currentRoom : lookup.GetValueOrDefault(conn.FromRoom);
                var toRoom = fromCurrent ? lookup.GetValueOrDefault(conn.ToRoom) : currentRoom;

                if (fromRoom == null || toRoom == null || positions.ContainsKey(otherName))
                    continue;

                var fromConn = fromCurrent ? conn.FromConnection : conn.ToConnection;
                var toConn = fromCurrent ? conn.ToConnection : conn.FromConnection;

                var offset = CalculatePlacementOffset(fromRoom, toRoom, fromConn, toConn);
                var fromPos = positions[fromRoom.Name];
                positions[otherName] = fromPos + offset;
                visited.Add(otherName);
                queue.Enqueue(otherName);
            }
        }

        // Any unconnected rooms get placed in a grid to the right
        int orphanIndex = 0;
        foreach (var room in rooms)
        {
            if (!positions.ContainsKey(room.Name))
            {
                positions[room.Name] = new Vector2(orphanIndex * (room.Width + 64), 0);
                orphanIndex++;
            }
        }

        return positions;
    }

    private static Vector2 CalculatePlacementOffset(
        RoomTemplate fromRoom,
        RoomTemplate toRoom,
        ConnectionPoint fromConn,
        ConnectionPoint toConn)
    {
        // Align the connection points so they meet at the same world position.
        // fromConn.X/Y are relative to fromRoom origin.
        // toConn.X/Y are relative to toRoom origin.
        // We want: fromPos + fromConn = toPos + toConn
        // Therefore: toPos = fromPos + fromConn - toConn
        return new Vector2(fromConn.X - toConn.X, fromConn.Y - toConn.Y);
    }

    #endregion

    #region Level Element

    private static BinaryPacker.Element BuildLevelElement(
        RoomTemplate room,
        int x,
        int y,
        List<EntityPlacement> roomPlacements,
        string music,
        string ambience,
        ref int strawberryOrder)
    {
        int width = room.Width;
        int height = room.Height;

        // Celeste auto-adjusts height 184 -> 180
        if (height == 184)
            height = 180;

        var level = new BinaryPacker.Element
        {
            Name = "level",
            Attributes = new Dictionary<string, object>
            {
                ["x"] = x,
                ["y"] = y,
                ["width"] = width,
                ["height"] = height,
                ["name"] = "lvl_" + room.Name,
                ["music"] = music,
                ["ambience"] = ambience,
                ["c"] = 0 // editor color
            },
            Children = new List<BinaryPacker.Element>()
        };

        // Tile layers
        int tileCols = (int)Math.Ceiling(width / 8.0);
        int tileRows = (int)Math.Ceiling(height / 8.0);
        string solidTiles = TileGenerator.GenerateSolidTiles(room.Type, tileCols, tileRows);
        string bgTiles = TileGenerator.GenerateBgTiles(room.Type, tileCols, tileRows);

        level.Children.Add(new BinaryPacker.Element
        {
            Name = "solids",
            Attributes = new Dictionary<string, object> { ["innerText"] = solidTiles }
        });
        level.Children.Add(new BinaryPacker.Element
        {
            Name = "bg",
            Attributes = new Dictionary<string, object> { ["innerText"] = bgTiles }
        });

        // Entities container
        var entities = new BinaryPacker.Element
        {
            Name = "entities",
            Children = new List<BinaryPacker.Element>()
        };
        level.Children.Add(entities);

        // Triggers container
        var triggers = new BinaryPacker.Element
        {
            Name = "triggers",
            Children = new List<BinaryPacker.Element>()
        };
        level.Children.Add(triggers);

        // Place entities
        foreach (var placement in roomPlacements)
        {
            var entity = BuildEntityElement(placement, ref strawberryOrder);
            entities.Children.Add(entity);
        }

        return level;
    }

    private static BinaryPacker.Element BuildEntityElement(EntityPlacement placement, ref int strawberryOrder)
    {
        var entity = new BinaryPacker.Element
        {
            Name = placement.EntityType,
            Attributes = new Dictionary<string, object>
            {
                ["x"] = placement.X,
                ["y"] = placement.Y,
                ["width"] = placement.Width,
                ["height"] = placement.Height
            }
        };

        // Copy custom properties
        if (placement.Properties != null)
        {
            foreach (var prop in placement.Properties)
            {
                entity.Attributes[prop.Key] = prop.Value;
            }
        }

        // Auto-assign strawberry order for tracking
        if (placement.EntityType == "strawberry" || placement.EntityType == "snowberry")
        {
            if (!entity.Attributes.ContainsKey("order"))
            {
                entity.Attributes["order"] = strawberryOrder;
                strawberryOrder++;
            }
            if (!entity.Attributes.ContainsKey("checkpointID"))
            {
                entity.Attributes["checkpointID"] = 0;
            }
        }

        return entity;
    }

    #endregion
}

/// <summary>
/// Generates basic tile data for procedurally built rooms.
/// Uses vanilla tile IDs where '0' = empty and '1' = default tile.
/// </summary>
public static class TileGenerator
{
    private const char Empty = '0';
    private const char Solid = '1';
    private const char Platform = '2';

    public static string GenerateSolidTiles(RoomType type, int cols, int rows)
    {
        var sb = new StringBuilder(cols * rows);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                sb.Append(GetSolidTile(type, c, r, cols, rows));
            }
        }
        return sb.ToString();
    }

    public static string GenerateBgTiles(RoomType type, int cols, int rows)
    {
        // Background tiles are usually sparse decoration
        var sb = new StringBuilder(cols * rows);
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                sb.Append(GetBgTile(type, c, r, cols, rows));
            }
        }
        return sb.ToString();
    }

    private static char GetSolidTile(RoomType type, int c, int r, int cols, int rows)
    {
        bool bottom = r == rows - 1;
        bool top = r == 0;
        bool left = c == 0;
        bool right = c == cols - 1;

        switch (type)
        {
            case RoomType.Checkpoint:
                // Solid floor, side walls for safety
                if (bottom) return Solid;
                if (left || right) return Solid;
                return Empty;

            case RoomType.Transition:
                // Corridor: floor and ceiling
                if (bottom || top) return Solid;
                return Empty;

            case RoomType.Boss:
                // Large arena with floor and wall boundaries
                if (bottom) return Solid;
                if (left || right) return Solid;
                if (r == rows / 2 && c > 2 && c < cols - 2) return Platform;
                return Empty;

            case RoomType.Secret:
                // Small enclosed room
                if (bottom || top || left || right) return Solid;
                return Empty;

            case RoomType.Challenge:
                // Narrower platforms, gaps
                if (bottom && (c < cols / 3 || c > cols * 2 / 3)) return Solid;
                if (r == rows * 2 / 3 && c > 1 && c < cols - 1) return Platform;
                if (r == rows / 2 && c > 3 && c < cols - 3) return Platform;
                return Empty;

            case RoomType.Puzzle:
                // Open with a few obstacles
                if (bottom) return Solid;
                if (r == rows / 2 && c == cols / 2) return Solid;
                return Empty;

            case RoomType.Standard:
            default:
                // Standard platforming room: floor with some mid platforms
                if (bottom) return Solid;
                if (r == rows * 2 / 3 && c > 2 && c < cols - 2) return Platform;
                if (r == rows / 2 && c > 4 && c < cols - 4) return Platform;
                return Empty;
        }
    }

    private static char GetBgTile(RoomType type, int c, int r, int cols, int rows)
    {
        // Background is mostly empty with occasional backing behind solids
        bool bottom = r == rows - 1;
        bool top = r == 0;
        bool left = c == 0;
        bool right = c == cols - 1;

        switch (type)
        {
            case RoomType.Secret:
                if (bottom || top || left || right) return Solid;
                return Empty;

            case RoomType.Checkpoint:
            case RoomType.Transition:
                if (bottom || top || left || right) return Solid;
                return Empty;

            default:
                if (bottom || left || right) return Solid;
                return Empty;
        }
    }
}
