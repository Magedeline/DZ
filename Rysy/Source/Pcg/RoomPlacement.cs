using Microsoft.Xna.Framework;
using Rysy;

namespace DZ.RysyPlugin.Pcg;

/// <summary>
/// Tunables for <see cref="RoomPlacement.PlaceEntities"/>/<see cref="RoomPlacement.PlaceAll"/>,
/// mirroring the `opts` table pcg_toolkit.lua's placement functions accept.
/// </summary>
public sealed record PlacementOptions {
    public bool IsStart { get; init; }
    public bool IsEnd { get; init; }

    // Strawberries: density budget (area * density, clamped), not a raw floor-count fraction,
    // so open rooms with lots of floor don't nominate dozens of berries.
    public double BerryDensity { get; init; } = 0.002;
    public int BerryMin { get; init; } = 1;
    public int BerryMax { get; init; } = 3;
    /// <summary>0 = auto (max(4, width/14)).</summary>
    public int BerrySpacing { get; init; }

    public int SpikeMax { get; init; } = 12;
    /// <summary>0 = density-driven (ceilings.Count * HazardDensity).</summary>
    public double HazardDensity { get; init; } = 0.05;

    public int SpringMax { get; init; } = 6;
    public double SpringDensity { get; init; }

    public string Style { get; init; } = "city";
    public string Era { get; init; } = "new";
    public double DecalDensity { get; init; } = 0.12;
    public IReadOnlyList<string>? BgDecalSet { get; init; }
    public IReadOnlyList<string>? FgDecalSet { get; init; }

    public string TriggerMode { get; init; } = "camera"; // camera | spawn | all | none
}

/// <summary>
/// Smart entity/decal/trigger placement, ported from pcg_toolkit.lua's placement functions.
/// This is the piece that actually talks to Rysy: everything upstream (generation, repair,
/// scoring) only ever touched <see cref="PcgGrid"/>, but placement has to create real
/// <see cref="Entity"/> instances via <see cref="EntityRegistry"/> and add them to the room.
///
/// SID correction versus the Lua: pcg_toolkit.lua places hazards via
/// <c>pcg.addEntity(room, "spikes", x, y, { direction = "down" })</c>. There is no "spikes"
/// entity in vanilla Celeste -- the real SIDs are "spikesUp"/"spikesDown"/"spikesLeft"/
/// "spikesRight", each a distinct class (Rysy/Rysy/Entities/Spikes.cs). Every "spikes"
/// placement in the Lua would silently render as an Unknown Entity placeholder in Lönn and
/// fail to load as a real hazard in-game. This port selects the direction-specific SID
/// directly instead.
/// </summary>
public static class RoomPlacement {
    private const int T = 8;

    private static Vector2 TilePos(int x, int y) => new(x * T, y * T);

    public static Entity AddEntity(Room room, string sid, Vector2 pos, Dictionary<string, object>? overrides = null) {
        var e = EntityRegistry.CreateFromMainPlacement(sid, pos, room, overrides ?? new());
        room.Entities.Add(e);
        return e;
    }

    public static Entity AddTrigger(Room room, string sid, Vector2 pos, Vector2 size,
        Dictionary<string, object>? overrides = null) {
        var attrs = overrides is null ? new Dictionary<string, object>() : new Dictionary<string, object>(overrides);
        attrs["width"] = (int) size.X;
        attrs["height"] = (int) size.Y;
        var e = EntityRegistry.CreateFromMainPlacement(sid, pos, room, attrs, isTrigger: true);
        room.Triggers.Add(e);
        return e;
    }

    public static Entity AddDecal(Room room, bool fg, string textureName, Vector2 pos,
        Dictionary<string, object>? overrides = null) {
        var texture = Style.ResolveDecalTexture(textureName);
        var attrs = overrides is null ? new Dictionary<string, object>() : new Dictionary<string, object>(overrides);
        attrs["texture"] = texture;
        attrs.TryAdd("scaleX", 1f);
        attrs.TryAdd("scaleY", 1f);
        attrs.TryAdd("rotation", 0f);

        var sid = fg ? EntityRegistry.FgDecalSid : EntityRegistry.BgDecalSid;
        var e = EntityRegistry.CreateFromMainPlacement(sid, pos, room, attrs);
        (fg ? room.FgDecals : room.BgDecals).Add(e);
        return e;
    }

    public static void ClearEntities(Room room) => room.Entities.Clear();

    public static void ClearDecals(Room room) {
        room.FgDecals.Clear();
        room.BgDecals.Clear();
    }

    public static void ClearTriggers(Room room) => room.Triggers.Clear();

    private static string CellKey(int x, int y) => $"{x},{y}";

    private static void Shuffle<T2>(IList<T2> list, Random rng) {
        for (var i = list.Count - 1; i >= 1; i--) {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Places player, golden berry, strawberries, ceiling spikes and springs with spatial
    /// awareness (reachability, spacing, avoiding the spawn area). Clears existing entities
    /// first. Returns the number of entities in the room afterward.
    /// </summary>
    public static int PlaceEntities(Room room, PcgGrid tiles, Random rng, PlacementOptions opts,
        RoomAnalysis? analysis = null) {
        var w = tiles.Width;
        var h = tiles.Height;
        var a = analysis ?? RoomAnalysis.Analyze(tiles);
        var floors = a.Floors;
        var spawnRegion = a.Spawn is { } sp0 ? a.RegionAt(sp0) : 0;
        var used = new HashSet<string>();

        ClearEntities(room);

        // Every room gets a spawn point: dying in a room with none errors out in-game, so
        // this is a hard requirement, not a start-room nicety.
        if (a.Spawn is { } spawn) {
            AddEntity(room, "player", TilePos(spawn.X, spawn.Y));
            used.Add(CellKey(spawn.X, spawn.Y));
        }

        if (opts.IsEnd && a.Goal is { } goal) {
            AddEntity(room, "goldenBerry", TilePos(goal.X, goal.Y - 1));
            used.Add(CellKey(goal.X, goal.Y));
        }

        // Strawberries: reachable floors in the spawn region, ranked toward the far side,
        // spaced apart, capped at a density budget so open rooms don't get dozens.
        var berryCount = Math.Max(opts.BerryMin, Math.Min(opts.BerryMax, (int) (w * h * opts.BerryDensity + 0.5)));
        var candidates = new List<(Poi P, double Score)>();
        foreach (var f in floors) {
            if (f.Y - 1 < 1 || a.RegionAt(f) != spawnRegion || used.Contains(CellKey(f.X, f.Y)))
                continue;
            var dx = f.X - (a.Spawn?.X ?? 2);
            var dy = f.Y - (a.Spawn?.Y ?? h / 2);
            candidates.Add((f, dx * 2 - Math.Abs(dy)));
        }
        candidates.Sort((x, y) => y.Score.CompareTo(x.Score));

        var minBerryDist = opts.BerrySpacing > 0 ? opts.BerrySpacing : Math.Max(4, w / 14);
        var berrySpots = new List<Poi>();

        void FillBerrySpots(int spacing) {
            foreach (var (c, _) in candidates) {
                if (berrySpots.Count >= berryCount)
                    break;
                var k = CellKey(c.X, c.Y);
                if (used.Contains(k))
                    continue;
                var tooClose = berrySpots.Any(b => Math.Abs(b.X - c.X) + Math.Abs(b.Y - c.Y) < spacing);
                if (tooClose)
                    continue;
                berrySpots.Add(c);
                used.Add(k);
            }
        }

        FillBerrySpots(minBerryDist);
        if (berrySpots.Count < berryCount && minBerryDist > 1)
            FillBerrySpots(Math.Max(1, minBerryDist / 2));

        foreach (var b in berrySpots)
            AddEntity(room, "strawberry", new Vector2(b.X * T + T / 2, (b.Y - 1) * T));

        // Ceiling spikes: reachable, spaced, kept clear of the spawn's immediate surroundings.
        var spikeCount = opts.HazardDensity > 0
            ? Math.Min(opts.SpikeMax, (int) (a.Ceilings.Count * opts.HazardDensity))
            : Math.Min(opts.SpikeMax, a.Ceilings.Count / 4);

        var dangerAroundSpawn = new HashSet<string>();
        if (a.Spawn is { } sp) {
            for (var dx = -3; dx <= 3; dx++)
            for (var dy = -3; dy <= 3; dy++)
                dangerAroundSpawn.Add(CellKey(sp.X + dx, sp.Y + dy));
        }

        bool IsAir(int x, int y) => x >= 0 && y >= 0 && x < w && y < h && tiles[x, y] == '0';

        var spikeCandidates = new List<(Poi P, int Score)>();
        foreach (var c in a.Ceilings) {
            if (c.X <= 2 || c.X >= w - 3 || c.Y <= 2 || c.Y >= h - 2)
                continue;
            if (dangerAroundSpawn.Contains(CellKey(c.X, c.Y)) || a.RegionId[c.X, c.Y - 1] != spawnRegion)
                continue;
            var nearWall = IsAir(c.X - 1, c.Y) && IsAir(c.X - 1, c.Y - 1);
            var nearWallR = IsAir(c.X + 1, c.Y) && IsAir(c.X + 1, c.Y - 1);
            spikeCandidates.Add((c, nearWall || nearWallR ? 2 : 1));
        }
        spikeCandidates.Sort((x, y) => y.Score.CompareTo(x.Score));
        Shuffle(spikeCandidates, rng);

        var spikeSpots = new List<Poi>();
        foreach (var (c, _) in spikeCandidates) {
            if (spikeSpots.Count >= spikeCount)
                break;
            var k = CellKey(c.X, c.Y);
            if (used.Contains(k))
                continue;
            if (spikeSpots.Any(s => Math.Abs(s.X - c.X) + Math.Abs(s.Y - c.Y) < 2))
                continue;
            spikeSpots.Add(c);
            used.Add(k);
        }

        // Ceiling spikes hang from the tile above and point down -- "spikesDown" is the
        // real SID; see the class-level note on why this can't be "spikes" + a direction.
        foreach (var s in spikeSpots)
            AddEntity(room, "spikesDown", TilePos(s.X, s.Y));

        // Springs: vertical shafts with headroom and a wall to brace against.
        if (opts.SpringDensity > 0) {
            var springCount = Math.Min(opts.SpringMax, (int) (a.AirCells.Count * opts.SpringDensity / 4));
            var springSpots = new List<Poi>();
            foreach (var c in a.AirCells) {
                if (c.Y < 2 || c.Y > h - 5 || c.X < 2 || c.X > w - 3)
                    continue;
                var hasFloor = tiles[c.X, c.Y + 1] != '0';
                var hasHeadroom = IsAir(c.X, c.Y - 1) && IsAir(c.X, c.Y - 2) && IsAir(c.X, c.Y - 3);
                var hasWall = tiles[c.X - 1, c.Y] != '0' || tiles[c.X + 1, c.Y] != '0';
                if (hasFloor && hasHeadroom && hasWall && a.RegionAt(c) == spawnRegion)
                    springSpots.Add(c);
            }
            Shuffle(springSpots, rng);
            foreach (var c in springSpots.Take(springCount))
                AddEntity(room, "spring", TilePos(c.X, c.Y));
        }

        return room.Entities.Count;
    }

    /// <summary>
    /// Simpler random-shuffle placement kept as a fallback: spawn near the left quarter,
    /// berries and spikes picked by raw shuffle rather than spatial scoring.
    /// </summary>
    public static void PlaceEntitiesLegacy(Room room, PcgGrid tiles, Random rng, bool isEnd) {
        var w = tiles.Width;
        var h = tiles.Height;
        var floors = RoomAnalysis.FindFloorSurfaces(tiles);
        var surfaces = RoomAnalysis.FindExposedTopSurfaces(tiles);

        var spawn = floors.Count > 0 ? floors[floors.Count / 4] : new Poi(w / 2, h / 2);
        AddEntity(room, "player", TilePos(spawn.X, spawn.Y));

        if (isEnd)
            AddEntity(room, "goldenBerry", new Vector2(w * T / 2f, h * T / 2f - T * 2));

        var berryCandidates = floors.Where(f => f.Y - 1 >= 1).ToList();
        Shuffle(berryCandidates, rng);
        var berryCount = Math.Max(1, floors.Count / 6);
        foreach (var s in berryCandidates.Take(berryCount))
            AddEntity(room, "strawberry", new Vector2(s.X * T + T / 2, (s.Y - 1) * T));

        Shuffle(surfaces, rng);
        var spikeCount = surfaces.Count / 4;
        foreach (var k in surfaces.Take(spikeCount))
            AddEntity(room, "spikesDown", TilePos(k.X, k.Y));
    }

    /// <summary>
    /// Guarantees the room has at least one player spawn on safe ground -- run this as a
    /// final safety net after any placement path, since dying in a spawn-less room errors
    /// out in-game. Returns whether a spawn was added (false if one already existed).
    /// </summary>
    public static bool EnsureSpawn(Room room, PcgGrid? tiles) {
        foreach (var e in room.Entities) {
            if (e.Name == "player")
                return false;
        }

        Poi? best = null;
        if (tiles is { } t) {
            var w = t.Width;
            var h = t.Height;
            var bestScore = double.NegativeInfinity;
            for (var x = 1; x < w - 1; x++)
            for (var y = 2; y < h - 1; y++) {
                if (t[x, y] != '0' || t[x, y + 1] == '0' || t[x, y - 1] != '0')
                    continue;
                // Prefer low, horizontally central floor cells with head clearance.
                var score = y - Math.Abs(x - w / 2);
                if (score > bestScore) {
                    bestScore = score;
                    best = new Poi(x, y);
                }
            }
        }

        var spot = best ?? new Poi((tiles?.Width ?? 40) / 2, (tiles?.Height ?? 23) / 2);
        AddEntity(room, "player", TilePos(spot.X, spot.Y));
        return true;
    }

    /// <summary>
    /// Scatters background particles through open air and foreground decals along walls,
    /// picked from the style's decal catalog.
    /// </summary>
    public static int PlaceDecals(Room room, PcgGrid tiles, Random rng, PlacementOptions opts,
        RoomAnalysis? analysis = null) {
        if (opts.DecalDensity <= 0)
            return 0;

        var a = analysis ?? RoomAnalysis.Analyze(tiles);
        var defaults = Style.DecalSetForStyle(opts.Style, opts.Era);
        var bgDecals = opts.BgDecalSet ?? defaults.Bg;
        var fgDecals = opts.FgDecalSet ?? defaults.Fg;
        var placed = 0;

        bool IsAir(int x, int y) => x >= 0 && y >= 0 && x < tiles.Width && y < tiles.Height && tiles[x, y] == '0';

        var bgCount = bgDecals.Count > 0 ? (int) (a.AirCells.Count * opts.DecalDensity / 8) : 0;
        for (var i = 0; i < bgCount && a.AirCells.Count > 0; i++) {
            var c = a.AirCells[rng.Next(a.AirCells.Count)];
            if (!IsAir(c.X, c.Y))
                continue;
            var name = bgDecals[rng.Next(bgDecals.Count)];
            var scale = 0.4f + (float) rng.NextDouble() * 0.6f;
            var rot = (float) rng.NextDouble() * 0.2f - 0.1f;
            var dx = Math.Clamp(c.X * T + rng.Next(T), 0, room.Width - 1);
            var dy = Math.Clamp(c.Y * T + rng.Next(T), 0, room.Height - 1);
            AddDecal(room, fg: false, name, new Vector2(dx, dy),
                new Dictionary<string, object> { ["scaleX"] = scale, ["scaleY"] = scale, ["rotation"] = rot });
            placed++;
        }

        var fgCount = fgDecals.Count > 0 ? (int) (a.Walls.Count * opts.DecalDensity / 2) : 0;
        for (var i = 0; i < fgCount && a.Walls.Count > 0; i++) {
            var c = a.Walls[rng.Next(a.Walls.Count)];
            var name = fgDecals[rng.Next(fgDecals.Count)];
            var ox = c.Side == WallSide.Left ? -(float) rng.NextDouble() * 4 : (float) rng.NextDouble() * 4;
            var oy = (float) rng.NextDouble() * 4;
            var scale = 0.8f + (float) rng.NextDouble() * 0.4f;
            var dx = Math.Clamp(c.X * T + ox, 0, room.Width - 1);
            var dy = Math.Clamp(c.Y * T + oy, 0, room.Height - 1);
            AddDecal(room, fg: true, name, new Vector2(dx, dy),
                new Dictionary<string, object> { ["scaleX"] = scale, ["scaleY"] = scale });
            placed++;
        }

        return placed;
    }

    /// <summary>
    /// Places camera-target triggers over vertical shafts/high areas and, optionally, a
    /// spawn-point trigger near safe reachable floor.
    ///
    /// "SpawnPointTrigger" is ported as-is from pcg_toolkit.lua; unlike "spikes" (a genuinely
    /// nonexistent SID -- see the class-level note) this one could not be independently
    /// confirmed against a vanilla Celeste entity list. An unregistered trigger SID is not
    /// harmful in Rysy -- it falls back to a plain colored rectangle (Rysy/Trigger.cs), same
    /// as every vanilla trigger without custom editor rendering -- so this is safe to leave
    /// even if the name turns out to be wrong; it would just be inert.
    /// </summary>
    public static int PlaceTriggers(Room room, PcgGrid tiles, Random rng, PlacementOptions opts,
        RoomAnalysis? analysis = null) {
        if (opts.TriggerMode == "none")
            return 0;

        var w = tiles.Width;
        var h = tiles.Height;
        var a = analysis ?? RoomAnalysis.Analyze(tiles);
        var placed = 0;

        if (opts.TriggerMode is "camera" or "all") {
            var camPoints = new List<Poi>(a.HighPoints);
            foreach (var f in a.Floors) {
                if (f.Y < h / 2)
                    camPoints.Add(new Poi(f.X, f.Y - 3));
            }
            Shuffle(camPoints, rng);
            var camCount = Math.Min(3, camPoints.Count / 6);

            for (var i = 0; i < camCount; i++) {
                var c = camPoints[i];
                var tw = Math.Min(w * T / 2f, 160);
                var th = Math.Min(h * T, 120);
                var tx = Math.Max(0, c.X * T - tw / 2);
                var ty = Math.Max(0, c.Y * T - th / 2);
                if (tx + tw > room.Width) tx = Math.Max(0, room.Width - tw);
                if (ty + th > room.Height) ty = Math.Max(0, room.Height - th);

                AddTrigger(room, "CameraTargetTrigger", new Vector2(tx, ty), new Vector2(tw, th),
                    new Dictionary<string, object> {
                        ["lerpStrength"] = 0.5f, ["positionMode"] = "NoEffect", ["xOnly"] = false,
                    });
                placed++;
            }
        }

        if (opts.TriggerMode is "spawn" or "all") {
            var spawnRegion = a.Spawn is { } sp ? a.RegionAt(sp) : 0;
            var spawnCandidates = a.Floors
                .Where(f => f.X >= 3 && f.X <= w - 4 && a.RegionAt(f) == spawnRegion)
                .ToList();
            if (spawnCandidates.Count > 0) {
                var c = spawnCandidates[rng.Next(spawnCandidates.Count)];
                AddTrigger(room, "SpawnPointTrigger", new Vector2(c.X * T, (c.Y - 1) * T), new Vector2(T * 2, T * 2));
                placed++;
            }
        }

        return placed;
    }

    public readonly record struct PlaceAllResult(int Entities, int Decals, int Triggers);

    /// <summary>One-pass smart placement of entities, decals and triggers.</summary>
    public static PlaceAllResult PlaceAll(Room room, PcgGrid tiles, Random rng, PlacementOptions opts,
        bool placeEntities = true, bool placeDecals = false, bool placeTriggers = false) {
        var a = RoomAnalysis.Analyze(tiles);

        var entities = placeEntities ? PlaceEntities(room, tiles, rng, opts, a) : room.Entities.Count;
        var decals = placeDecals ? PlaceDecals(room, tiles, rng, opts, a) : 0;
        var triggers = placeTriggers ? PlaceTriggers(room, tiles, rng, opts, a) : 0;

        return new PlaceAllResult(entities, decals, triggers);
    }
}
