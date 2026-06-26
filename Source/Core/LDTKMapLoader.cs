using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using KirbyCelesteStandalone.Entities.Bosses;
using Scene = Nez.Scene;

namespace KirbyCelesteStandalone.Core;

/// <summary>
/// Loads LDTK maps and spawns entities.
/// Uses LDTK (Level Designer Toolkit) format - the same tool used by Dead Cells.
/// </summary>
public class LDTKMapLoader
{
    private readonly Scene _scene;
    private LDTKProject _project;

    public LDTKMapLoader(Scene scene)
    {
        _scene = scene;
    }

    /// <summary>
    /// Load an LDTK project file
    /// </summary>
    public void LoadProject(string ldtkPath)
    {
        string json = File.ReadAllText(ldtkPath);
        _project = System.Text.Json.JsonSerializer.Deserialize<LDTKProject>(json);

        Console.WriteLine($"[LDTK] Loaded project: {_project.Name}");
        Console.WriteLine($"[LDTK] Levels: {_project.Levels.Count}");
    }

    /// <summary>
    /// Load a specific level by name
    /// </summary>
    public void LoadLevel(string levelIdentifier)
    {
        var level = _project.Levels.FirstOrDefault(l => l.Identifier == levelIdentifier);
        if (level == null)
        {
            Console.WriteLine($"[LDTK] Level not found: {levelIdentifier}");
            return;
        }

        Console.WriteLine($"[LDTK] Loading level: {levelIdentifier} ({level.PxWid}x{level.PxHei})");

        // Load layers (bottom to top)
        foreach (var layer in level.LayerInstances.OrderBy(l => GetLayerOrder(l.Identifier)))
        {
            LoadLayer(layer);
        }
    }

    private int GetLayerOrder(string identifier) => identifier switch
    {
        "BG_Tiles" => 0,
        "BG_Entities" => 1,
        "Solids" => 2,
        "FG_Tiles" => 3,
        "Entities" => 4,
        "Triggers" => 5,
        _ => 99
    };

    private void LoadLayer(LDTKLayer layer)
    {
        switch (layer.Type)
        {
            case "IntGrid":
                LoadIntGrid(layer);
                break;
            case "Entities":
                LoadEntities(layer);
                break;
            case "Tiles":
                LoadTiles(layer);
                break;
        }
    }

    private void LoadIntGrid(LDTKLayer layer)
    {
        // IntGrid layer = collision/solid tiles
        // layer.IntGridCsv contains tile values

        Console.WriteLine($"[LDTK] Loading IntGrid: {layer.Identifier} ({layer.PxWid}x{layer.PxHei})");

        int width = layer.PxWid / 8;  // 8px tiles
        int height = layer.PxHei / 8;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index < layer.IntGridCsv.Count && layer.IntGridCsv[index] > 0)
                {
                    // Value > 0 = solid
                    Vector2 position = new Vector2(x * 8 + 4, y * 8 + 4);
                    SpawnSolidTile(position, layer.IntGridCsv[index]);
                }
            }
        }
    }

    private void SpawnSolidTile(Vector2 position, int tileType)
    {
        var tile = _scene.CreateEntity($"solid_tile_{position.X}_{position.Y}");
        tile.Position = position;

        // Add collider
        var collider = tile.AddComponent(new BoxCollider(8, 8));
        collider.PhysicsLayer = 3; // Solid layer

        // TODO: Add visual based on tile type
    }

    private void LoadEntities(LDTKLayer layer)
    {
        Console.WriteLine($"[LDTK] Loading Entities: {layer.EntityInstances.Count} entities");

        foreach (var entity in layer.EntityInstances)
        {
            SpawnEntity(entity);
        }
    }

    private void LoadTiles(LDTKLayer layer)
    {
        // TODO: Load tile graphics
        Console.WriteLine($"[LDTK] Loading Tiles: {layer.Identifier}");
    }

    private void SpawnEntity(LDTKEntityInstance entity)
    {
        var position = new Vector2(entity.Px[0], entity.Px[1]);

        // Get properties from field instances
        var properties = new Dictionary<string, object>();
        foreach (var field in entity.FieldInstances)
        {
            properties[field.Identifier] = field.Value;
        }

        // Spawn based on entity type
        switch (entity.Identifier)
        {
            case "Player":
                SpawnPlayer(position);
                break;

            case "CharaBoss":
                int patternIndex = properties.GetInt("patternIndex", 0);
                SpawnCharaBoss(position, patternIndex);
                break;

            case "AsrielGodBoss":
                SpawnAsrielBoss(position);
                break;

            case "AsrielAngelBoss":
                SpawnAsrielAngelBoss(position);
                break;

            case "Spring":
                string springDir = properties.GetString("direction", "up");
                SpawnSpring(position, springDir);
                break;

            case "FallingBlock":
                SpawnFallingBlock(position);
                break;

            case "MovingBlock":
                SpawnMovingBlock(position, entity.DefPx);
                break;

            case "Strawberry":
                bool isGolden = properties.GetBool("isGolden", false);
                SpawnStrawberry(position, isGolden);
                break;

            case "Refill":
                bool twoDashes = properties.GetBool("twoDashes", false);
                SpawnRefill(position, twoDashes);
                break;

            case "Spikes":
                string spikeDir = properties.GetString("direction", "up");
                SpawnSpikes(position, spikeDir);
                break;

            case "RoomTransition":
                string targetRoom = properties.GetString("targetRoom", "");
                string targetSpawn = properties.GetString("targetSpawn", "");
                SpawnRoomTransition(position, targetRoom, targetSpawn);
                break;

            case "CutsceneTrigger":
                string cutsceneId = properties.GetString("cutsceneId", "");
                SpawnCutsceneTrigger(position, cutsceneId);
                break;

            default:
                Console.WriteLine($"[LDTK] Unknown entity type: {entity.Identifier}");
                break;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ENTITY SPAWNERS
    // ═══════════════════════════════════════════════════════════════════════

    private void SpawnPlayer(Vector2 position)
    {
        var player = _scene.CreateEntity("player");
        player.Position = position;
        player.AddComponent(new PlayerController());

        Console.WriteLine($"[LDTK] Spawned Player at {position}");
    }

    private void SpawnCharaBoss(Vector2 position, int patternIndex)
    {
        // Get nodes (waypoints) from entity
        // In LDTK, nodes are stored as defPx array

        var nodes = new Vector2[] { position }; // Simplified

        var boss = CharaBoss.Spawn(_scene, nodes, patternIndex);

        Console.WriteLine($"[LDTK] Spawned CharaBoss (pattern {patternIndex}) at {position}");
    }

    private void SpawnAsrielBoss(Vector2 position)
    {
        var boss = AsrielSpawner.Spawn(_scene, position);
        Console.WriteLine($"[LDTK] Spawned AsrielGodBoss at {position}");
    }

    private void SpawnAsrielAngelBoss(Vector2 position)
    {
        var boss = _scene.CreateEntity("asriel_angel_boss");
        boss.Position = position;
        // boss.AddComponent(new AsrielAngelBoss());

        Console.WriteLine($"[LDTK] Spawned AsrielAngelBoss at {position}");
    }

    private void SpawnSpring(Vector2 position, string direction)
    {
        var spring = _scene.CreateEntity("spring");
        spring.Position = position;
        // spring.AddComponent(new Spring(direction));

        Console.WriteLine($"[LDTK] Spawned Spring ({direction}) at {position}");
    }

    private void SpawnFallingBlock(Vector2 position)
    {
        var block = _scene.CreateEntity("falling_block");
        block.Position = position;
        // block.AddComponent(new FallingBlock());

        Console.WriteLine($"[LDTK] Spawned FallingBlock at {position}");
    }

    private void SpawnMovingBlock(Vector2 position, List<int> nodes)
    {
        var block = _scene.CreateEntity("moving_block");
        block.Position = position;
        // block.AddComponent(new MovingBlock(nodes));

        Console.WriteLine($"[LDTK] Spawned MovingBlock at {position}");
    }

    private void SpawnStrawberry(Vector2 position, bool isGolden)
    {
        var berry = _scene.CreateEntity("strawberry");
        berry.Position = position;
        // berry.AddComponent(new Strawberry(isGolden));

        Console.WriteLine($"[LDTK] Spawned {(isGolden ? "Golden" : "")}Strawberry at {position}");
    }

    private void SpawnRefill(Vector2 position, bool twoDashes)
    {
        var refill = _scene.CreateEntity("refill");
        refill.Position = position;
        // refill.AddComponent(new Refill(twoDashes));

        Console.WriteLine($"[LDTK] Spawned Refill ({(twoDashes ? "2" : "1")} dash) at {position}");
    }

    private void SpawnSpikes(Vector2 position, string direction)
    {
        var spikes = _scene.CreateEntity("spikes");
        spikes.Position = position;
        // spikes.AddComponent(new Spikes(direction));

        Console.WriteLine($"[LDTK] Spawned Spikes ({direction}) at {position}");
    }

    private void SpawnRoomTransition(Vector2 position, string targetRoom, string targetSpawn)
    {
        var trigger = _scene.CreateEntity("room_transition");
        trigger.Position = position;
        // trigger.AddComponent(new RoomTransitionTrigger(targetRoom, targetSpawn));

        Console.WriteLine($"[LDTK] Spawned RoomTransition -> {targetRoom} at {position}");
    }

    private void SpawnCutsceneTrigger(Vector2 position, string cutsceneId)
    {
        var trigger = _scene.CreateEntity("cutscene_trigger");
        trigger.Position = position;
        // trigger.AddComponent(new CutsceneTrigger(cutsceneId));

        Console.WriteLine($"[LDTK] Spawned CutsceneTrigger ({cutsceneId}) at {position}");
    }
}

// ═══════════════════════════════════════════════════════════════════════
// LDTK JSON DATA CLASSES
// ═══════════════════════════════════════════════════════════════════════

public class LDTKProject
{
    public string Name { get; set; } = "";
    public List<LDTKLevel> Levels { get; set; } = new();
}

public class LDTKLevel
{
    public string Identifier { get; set; } = "";
    public string Iid { get; set; } = "";
    public int Uid { get; set; }
    public int WorldX { get; set; }
    public int WorldY { get; set; }
    public int PxWid { get; set; }
    public int PxHei { get; set; }
    public List<LDTKLayer> LayerInstances { get; set; } = new();
}

public class LDTKLayer
{
    public string Identifier { get; set; } = "";
    public string Iid { get; set; } = "";
    public int Uid { get; set; }
    public string Type { get; set; } = ""; // "IntGrid", "Entities", "Tiles"
    public List<int> IntGridCsv { get; set; } = new();
    public List<LDTKEntityInstance> EntityInstances { get; set; } = new();
    public int PxWid { get; set; }
    public int PxHei { get; set; }
}

public class LDTKEntityInstance
{
    public string Identifier { get; set; } = "";
    public string Iid { get; set; } = "";
    public int Uid { get; set; }
    public int LayerUid { get; set; }
    public int Cx { get; set; }
    public int Cy { get; set; }
    public List<int> Px { get; set; } = new(); // [x, y]
    public List<LDTKFieldInstance> FieldInstances { get; set; } = new();
    public List<int> DefPx { get; set; } = new(); // Node positions
    public List<string> Tags { get; set; } = new();
}

public class LDTKFieldInstance
{
    public string Identifier { get; set; } = "";
    public int DefUid { get; set; }
    public string Type { get; set; } = "";
    public object Value { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// EXTENSIONS
// ═══════════════════════════════════════════════════════════════════════

public static class LDTKExtensions
{
    public static int GetInt(this Dictionary<string, object> dict, string key, int defaultValue = 0)
    {
        if (dict.TryGetValue(key, out var value) && value is JsonElement json)
            return json.GetInt32();
        return defaultValue;
    }

    public static string GetString(this Dictionary<string, object> dict, string key, string defaultValue = "")
    {
        if (dict.TryGetValue(key, out var value) && value is JsonElement json)
            return json.GetString() ?? defaultValue;
        return defaultValue;
    }

    public static bool GetBool(this Dictionary<string, object> dict, string key, bool defaultValue = false)
    {
        if (dict.TryGetValue(key, out var value) && value is JsonElement json)
            return json.GetBoolean();
        return defaultValue;
    }
}
