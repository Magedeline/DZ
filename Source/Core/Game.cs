using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System;
using System.IO;
using NezCore = Nez.Core;

namespace KirbyCelesteStandalone.Core;

/// <summary>
/// Main Game class - replaces MaggyHelperModule and Everest integration.
/// This is the entry point for your standalone fangame.
/// </summary>
public class KirbyGame : NezCore
{
    public static KirbyGame Instance { get; private set; }

    // Core managers
    public static AudioManager Audio { get; private set; }
    public static SceneManager Scenes { get; private set; }

    // Configuration
    public static string ContentPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
    public static string AudioBankPath => Path.Combine(ContentPath, "audio", "banks");

    public KirbyGame() : base(width: 1920, height: 1080, isFullScreen: false)
    {
        Instance = this;

        // Set window properties
        Window.Title = "Kirby Celeste - Standalone Fangame";

        // Target 60 FPS like Celeste
        TargetElapsedTime = TimeSpan.FromTicks(166667); // 60 FPS
        IsFixedTimeStep = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Initialize core systems
        InitializeAudio();
        InitializeScenes();

        // Set initial scene
        Scenes.LoadMainMenu();

        Console.WriteLine("[KirbyGame] Initialization complete!");
    }

    private void InitializeAudio()
    {
        try
        {
            Audio = new AudioManager();
            Audio.Initialize();

            // Load your custom banks
            // These are your existing pusheen_audio*.bank files
            Audio.LoadBank("pusheen_audio");
            Audio.LoadBank("pusheen_audio_A");
            Audio.LoadBank("pusheen_audio_B");
            Audio.LoadBank("pusheen_audio_C");
            Audio.LoadBank("pusheen_audio_D");

            Console.WriteLine("[KirbyGame] Audio initialized successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KirbyGame] Audio initialization failed: {ex.Message}");
            // Continue without audio - game can still run
        }
    }

    private void InitializeScenes()
    {
        Scenes = new SceneManager(this);
    }

    // NOTE: Update(GameTime) and Draw(GameTime) overrides are omitted because
    // Nez.Core was compiled against MonoGame's GameTime type while this project
    // references FNA. The parameter types are binary-compatible at runtime but
    // the C# compiler sees them as different types, preventing override.
    // Nez.Core handles the game loop internally; hook into Nez's scene/component
    // update system for custom per-frame logic instead.

    protected override void OnExiting(object sender, EventArgs args)
    {
        base.OnExiting(sender, args);

        // Cleanup
        Audio?.Shutdown();
    }
}

/// <summary>
/// Program entry point
/// </summary>
public static class Program
{
    [STAThread]
    public static void Main()
    {
        using var game = new KirbyGame();
        game.Run();
    }
}
