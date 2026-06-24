using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using System;
using NezCore = Nez.Core;

namespace KirbyCelesteStandalone.Core;

/// <summary>
/// Manages scene transitions and state.
/// Replaces: Everest's area/level loading + OuiChapterSelect
/// </summary>
public class SceneManager
{
    private readonly NezCore _core;
    private Scene _currentScene;

    public SceneManager(NezCore core)
    {
        _core = core;
    }

    /// <summary>
    /// Load the main menu scene
    /// </summary>
    public void LoadMainMenu()
    {
        var scene = new MainMenuScene();
        SetScene(scene);
    }

    /// <summary>
    /// Load a gameplay level by ID
    /// </summary>
    public void LoadLevel(string levelId)
    {
        var scene = new GameplayScene(levelId);
        SetScene(scene);
    }

    /// <summary>
    /// Load a boss fight scene
    /// </summary>
    public void LoadBossFight(string bossId, string arenaId)
    {
        var scene = new BossFightScene(bossId, arenaId);
        SetScene(scene);
    }

    /// <summary>
    /// Transition to a new scene with optional fade effect
    /// </summary>
    private void SetScene(Scene scene, bool fadeTransition = true)
    {
        if (fadeTransition)
        {
            // Create fade transition
            NezCore.StartSceneTransition(new FadeTransition(() => scene));
        }
        else
        {
            NezCore.Scene = scene;
        }

        _currentScene = scene;
        Console.WriteLine($"[SceneManager] Loaded scene: {scene.GetType().Name}");
    }

    /// <summary>
    /// Get current scene
    /// </summary>
    public Scene GetCurrentScene() => _currentScene;
}

/// <summary>
/// Main menu scene - replaces OuiChapterSelect
/// </summary>
public class MainMenuScene : Scene
{
    public override void Initialize()
    {
        base.Initialize();

        // Add camera
        var camera = CreateEntity("camera");
        camera.AddComponent(new Camera());

        // Setup pixel-perfect rendering at 320x180 (Celeste's native resolution)
        SetDesignResolution(320, 180, Scene.SceneResolutionPolicy.NoBorderPixelPerfect);

        // TODO: Add menu UI entities
        // - Continue
        // - New Game
        // - Chapter Select (your 21 chapters!)
        // - Options
        // - Quit

        Console.WriteLine("[MainMenuScene] Initialized");
    }

    public override void Update()
    {
        base.Update();

        // Example: Press Enter to start level
        if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
        {
            // Load first level
            KirbyGame.Scenes.LoadLevel("ch1_intro");
        }
    }
}

/// <summary>
/// Main gameplay scene - replaces Level from Celeste
/// </summary>
public class GameplayScene : Scene
{
    private readonly string _levelId;

    public GameplayScene(string levelId)
    {
        _levelId = levelId;
    }

    public override void Initialize()
    {
        base.Initialize();

        // Setup pixel-perfect rendering
        SetDesignResolution(320, 180, Scene.SceneResolutionPolicy.NoBorderPixelPerfect);

        // Create camera that follows player
        var cameraEntity = CreateEntity("camera");
        var camera = cameraEntity.AddComponent(new Camera());

        // Setup physics world with Velcro
        // TODO: Initialize Velcro Physics world

        // Load map
        LoadMap(_levelId);

        // Spawn player
        SpawnPlayer();

        Console.WriteLine($"[GameplayScene] Level {_levelId} loaded");
    }

    private void LoadMap(string levelId)
    {
        // TODO: Implement Tiled map loading
        // 1. Load .tmx file from assets/maps/
        // 2. Parse layers (solids, background, entities)
        // 3. Create entities for each tile/object
        // 4. Spawn triggers, hazards, collectibles

        // For now, just create a placeholder ground
        var ground = CreateEntity("ground");
        ground.Position = new Vector2(160, 160);
        // ground.AddComponent(new BoxCollider(320, 20)); // Solid platform
    }

    private void SpawnPlayer()
    {
        // TODO: Create player entity
        // This will be your Kirby player controller with:
        // - Platformer physics (Velcro)
        // - Dash mechanics
        // - Copy ability system
        // - Health/damage system

        var player = CreateEntity("player");
        player.Position = new Vector2(160, 100);
        player.AddComponent(new PlayerController());

        // Camera follows player
        var camera = FindEntity("camera")?.GetComponent<Camera>();
        if (camera != null)
        {
            // camera.AddComponent(new FollowCamera(player));
        }
    }

    public override void Update()
    {
        base.Update();

        // Update audio
        KirbyGame.Audio.Update();

        // Example: Press M for main menu
        if (Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.M))
        {
            KirbyGame.Scenes.LoadMainMenu();
        }
    }
}

/// <summary>
/// Boss fight scene - dedicated arena for boss encounters
/// </summary>
public class BossFightScene : Scene
{
    private readonly string _bossId;
    private readonly string _arenaId;

    public BossFightScene(string bossId, string arenaId)
    {
        _bossId = bossId;
        _arenaId = arenaId;
    }

    public override void Initialize()
    {
        base.Initialize();

        SetDesignResolution(320, 180, Scene.SceneResolutionPolicy.NoBorderPixelPerfect);

        // Load arena
        LoadArena(_arenaId);

        // Spawn player
        SpawnPlayer();

        // Spawn boss
        SpawnBoss();

        Console.WriteLine($"[BossFightScene] Boss: {_bossId}, Arena: {_arenaId}");
    }

    private void LoadArena(string arenaId)
    {
        // Boss arenas are typically single-room with no scrolling
        // TODO: Load arena map
    }

    private void SpawnPlayer()
    {
        var player = CreateEntity("player");
        player.Position = new Vector2(40, 120); // Left side of arena
        player.AddComponent(new PlayerController());
    }

    private void SpawnBoss()
    {
        // TODO: Boss factory system
        // Based on _bossId, create appropriate boss entity:
        // - AsrielGodBoss
        // - AxisTerminatorBoss
        // - CharaBoss
        // - etc.

        var boss = CreateEntity($"boss_{_bossId}");
        boss.Position = new Vector2(280, 120); // Right side of arena
        // boss.AddComponent(BossFactory.Create(_bossId));

        Console.WriteLine($"[BossFightScene] Spawned boss: {_bossId}");
    }
}
