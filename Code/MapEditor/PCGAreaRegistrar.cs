using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.MaggyHelper;

/// <summary>
/// Inspired by CelesteRandomizer: dynamically registers a generated PCG map
/// as an in-game Area so it can be played immediately without manual file copying
/// or everest.yaml edits.
/// </summary>
public static class PCGAreaRegistrar
{
    private const string LogTag = "PCGAreaRegistrar";
    private const string AreaPrefix = "maggy_pcg";

    /// <summary>
    /// Queue of areas waiting to be registered. Consumed by the OnLoadLevel hook.
    /// </summary>
    private static readonly Queue<AreaData> AreaHandoff = new();

    private static bool _hooked;

    /// <summary>
    /// Register the handoff hook with Everest. Call once during mod Load().
    /// </summary>
    public static void Load()
    {
        if (_hooked) return;
        _hooked = true;
        Everest.Events.Level.OnLoadLevel += OnLoadLevel_Handoff;
    }

    /// <summary>
    /// Unregister the handoff hook. Call during mod Unload().
    /// </summary>
    public static void Unload()
    {
        if (!_hooked) return;
        _hooked = false;
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel_Handoff;
    }

    private static void OnLoadLevel_Handoff(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
    {
        while (AreaHandoff.Count > 0)
        {
            var area = AreaHandoff.Dequeue();
            try
            {
                if (!AreaData.Areas.Any(a => a.SID == area.SID))
                {
                    AreaData.Areas.Add(area);
                    Logger.Log(LogLevel.Info, LogTag, $"Registered dynamic area: {area.SID} (ID={area.ID})");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, LogTag, $"Failed to register area {area.SID}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Registers a generated .bin map as a playable Celeste area/chapter.
    /// The map will appear in the chapter select screen (at the end) and can be entered immediately.
    /// </summary>
    /// <param name="mapPath">Path to the generated .bin file</param>
    /// <param name="displayName">Name shown in menus</param>
    /// <returns>AreaKey for the newly registered area, or None on failure</returns>
    public static AreaKey RegisterGeneratedMap(string mapPath, string displayName = "")
    {
        if (!File.Exists(mapPath))
        {
            Logger.Log(LogLevel.Warn, LogTag, $"Map file not found: {mapPath}");
            return AreaKey.None;
        }

        try
        {
            // Resolve to a SID-friendly path relative to mod root
            string fullPath = Path.GetFullPath(mapPath);
            string fileName = Path.GetFileNameWithoutExtension(mapPath);
            string sid = $"{AreaPrefix}/{fileName}";
            string safeName = string.IsNullOrEmpty(displayName) ? fileName : displayName;

            int newId = AreaData.Areas.Count;
            // Re-use slot if last area is also a PCG area
            if (AreaData.Areas.Count > 0 && AreaData.Areas.Last().SID.StartsWith($"{AreaPrefix}/"))
            {
                newId--;
            }

            var area = new AreaData
            {
                ID = newId,
                Name = safeName,
                SID = sid,
                IntroType = Player.IntroTypes.WakeUp,
                Interlude = false,
                Dreaming = false,
                Icon = AreaData.Areas[0].Icon,
                MountainIdle = AreaData.Areas[0].MountainIdle,
                MountainZoom = AreaData.Areas[0].MountainZoom,
                MountainState = AreaData.Areas[0].MountainState,
                MountainCursor = new Vector3(0, 100000, 0),
                MountainSelect = AreaData.Areas[0].MountainSelect,
                MountainCursorScale = AreaData.Areas[0].MountainCursorScale,
                Wipe = (scene, wipeIn, onComplete) => new CurtainWipe(scene, wipeIn, onComplete),
                CompleteScreenName = AreaData.Areas[1].CompleteScreenName,
                CassetteSong = null,
                ColorGrade = "none",
                BloomBase = 0f,
                DarknessAlpha = 0f,
                Mode = new ModeProperties[1]
            };

            // Use AudioController to determine appropriate music/ambience for PCG maps
            // Default to lvl1 explore music and prologue ambience, but allow AudioController override
            string defaultMusic = "event:/pusheen/music/lvl1/explore";
            string defaultAmbience = "event:/pusheen/ambience/ruins";
            
            var mode = new ModeProperties
            {
                Inventory = PlayerInventory.Default,
                AudioState = new AudioState(defaultMusic, defaultAmbience),
                Checkpoints = null
            };
            
            // Store PCG metadata for AudioController to pick up on level load
            var audioDyn = new DynData<ModeProperties>(mode);
            audioDyn["PCGDynamicAudio"] = true;
            audioDyn["PCGSourcePath"] = fullPath;

            // Attempt to attach MapData from the generated .bin
            try
            {
                mode.MapData = new MapData(new AreaKey(newId));
                // Force reload from our file by replacing the internal filename
                var mapDyn = new DynData<MapData>(mode.MapData);
                mapDyn["filename"] = fullPath;
                mode.MapData.Reload();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Warn, LogTag, $"MapData load failed, will retry on level load: {ex.Message}");
                // Queue a lazy load
                mode.MapData = null;
            }

            area.Mode[0] = mode;

            // Meta for Everest
            area.Meta = new global::Celeste.Mod.Meta.MapMeta
            {
                Modes = new global::Celeste.Mod.Meta.MapMetaModeProperties[]
                {
                    new global::Celeste.Mod.Meta.MapMetaModeProperties
                    {
                        HeartIsEnd = true,
                        SeekerSlowdown = false
                    }
                }
            };

            // Attach settings for retrieval later
            var dyn = new DynData<AreaData>(area);
            dyn["PCGSourcePath"] = fullPath;
            dyn["PCGGeneratedAt"] = DateTime.Now;

            // Queue for registration
            AreaHandoff.Enqueue(area);

            Logger.Log(LogLevel.Info, LogTag, $"Queued area registration: {sid} -> {fullPath}");
            return new AreaKey(newId);
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, LogTag, $"RegisterGeneratedMap failed: {ex.Message}");
            return AreaKey.None;
        }
    }

    /// <summary>
    /// Warps the player directly into a generated PCG area.
    /// Call this after RegisterGeneratedMap to start playing immediately.
    /// </summary>
    public static void WarpToGeneratedMap(AreaKey key)
    {
        if (key == AreaKey.None)
        {
            Logger.Log(LogLevel.Warn, LogTag, "Cannot warp to AreaKey.None");
            return;
        }

        if (Engine.Scene is not Level currentLevel)
        {
            Logger.Log(LogLevel.Warn, LogTag, "Must be in a level to warp.");
            return;
        }

        // Ensure the area is actually registered
        if (key.ID >= AreaData.Areas.Count)
        {
            Logger.Log(LogLevel.Warn, LogTag, $"Area ID {key.ID} not yet registered. Waiting for level transition...");
            return;
        }

        var session = new Session(key);
        session.StartedFromBeginning = true;
        session.FirstLevel = true;

        currentLevel.PauseLock = true;
        currentLevel.Paused = true;

        var warpEntity = new Entity();
        warpEntity.Add(new Coroutine(WarpRoutine(currentLevel, session)));
        currentLevel.Add(warpEntity);
    }

    private static IEnumerator WarpRoutine(Level fromLevel, Session session)
    {
        // Fade out (duration configurable in mod settings)
        var fade = new FadeWipe(fromLevel, false, () => { });
        fade.Duration = MaggyHelperModule.Settings?.PCGWarpFadeDuration ?? 0.5f;
        yield return fade.Wait();

        // Transfer to new level
        Engine.Scene = new LevelLoader(session);
    }

    /// <summary>
    /// Async helper: generate a map, write it to disk, register it, and optionally warp to it.
    /// </summary>
    public static async Task<AreaKey> GenerateAndPlay(
        string templateLibrary,
        int seed = -1,
        int roomCount = 8,
        int difficulty = 2,
        bool autoWarp = true)
    {
        string outputPath = $"PCG/Generated/pcg_play_{DateTime.Now:yyyyMMdd_HHmmss}.bin";

        bool ok = await PCGService.GenerateHybridMapAsync(
            templateLibrary,
            outputPath,
            seed,
            roomCount,
            difficulty,
            "pathway",
            "balanced");

        if (!ok)
        {
            Logger.Log(LogLevel.Error, LogTag, "Map generation failed.");
            return AreaKey.None;
        }

        var key = RegisterGeneratedMap(outputPath, $"PCG Map {DateTime.Now:MM-dd HH:mm}");

        if (autoWarp && key != AreaKey.None)
        {
            // Wait a moment for area registration to propagate
            await Task.Delay(100);
            WarpToGeneratedMap(key);
        }

        return key;
    }
}
