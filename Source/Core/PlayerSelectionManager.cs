using System;
using Monocle;

namespace Celeste.Entities;

/// <summary>
/// Manages player selection for the mod (Kirby vs. Madeline).
/// Provides a singleton persistent across level loads, with support for mod-wide defaults
/// and per-level overrides via map metadata.
/// </summary>
[Tracked(true)]
public class PlayerSelectionManager : Entity
{
    /// <summary>Available player types in the system.</summary>
    public enum PlayerType
    {
        /// <summary>Kirby - Kirby character with special abilities (double-jump, alternate dash, combat, hover)</summary>
        Kirby = 0,
        
        /// <summary>Madeline - Standard Celeste player without special abilities</summary>
        Madeline = 1,
    }

    /// <summary>Global singleton instance, persistent across level transitions.</summary>
    public static PlayerSelectionManager Instance { get; private set; }

    /// <summary>Currently selected player type.</summary>
    private PlayerType currentPlayerType = PlayerType.Kirby;

    /// <summary>Default player type from mod settings.</summary>
    private PlayerType defaultPlayerType = PlayerType.Kirby;

    /// <summary>Per-level override (null = use default).</summary>
    private PlayerType? levelOverride;

    /// <summary>
    /// Map-supplied recommended player for the next level.
    /// Set via <see cref="SetRecommendedPlayer"/> before entering a map;
    /// automatically cleared after it is read once.
    /// </summary>
    private PlayerType? recommendedPlayer;

    /// <summary>
    /// When true the manager will automatically apply
    /// <see cref="recommendedPlayer"/> as the active selection before entering
    /// a level (instead of showing the manual picker).
    /// Persisted through <see cref="DZModule.Settings.PlayerAutoSelect"/>.
    /// </summary>
    public static bool AutoSelectEnabled
    {
        get => DZModule.Settings?.PlayerAutoSelect ?? false;
        set { if (DZModule.Settings != null) DZModule.Settings.PlayerAutoSelect = value; }
    }

    /// <summary>Event fired when player selection changes.</summary>
    public static event Action<PlayerType> OnPlayerSelectionChanged;

    public PlayerSelectionManager() : base(Vector2.Zero)
    {
        Tag = Tags.Persistent;
        Depth = -10000; // Render above most entities, non-interactive
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        
        // Register as singleton or capture existing instance data
        if (Instance != null && Instance != this)
        {
            // Preserve existing selection when transitioning levels
            currentPlayerType = Instance.currentPlayerType;
            defaultPlayerType = Instance.defaultPlayerType;
            levelOverride = Instance.levelOverride;
            RemoveSelf();
            return;
        }

        Instance = this;
        
        // Load default from mod settings
        LoadDefaultFromSettings();
    }

    public override void Removed(Scene scene)
    {
        // Don't clear Instance - keep it persistent for level transitions
        base.Removed(scene);
    }

    /// <summary>
    /// Get the currently active player type (respecting per-level overrides).
    /// </summary>
    public static PlayerType GetSelectedPlayer()
    {
        if (Instance == null)
            return PlayerType.Kirby; // Fallback default
        
        return Instance.levelOverride ?? Instance.currentPlayerType;
    }

    /// <summary>
    /// Set the global default player type (from mod settings).
    /// </summary>
    public static void SetDefaultPlayer(PlayerType playerType)
    {
        if (Instance == null)
            return;

        if (Instance.defaultPlayerType != playerType)
        {
            Instance.defaultPlayerType = playerType;
            Instance.currentPlayerType = playerType;
            Instance.levelOverride = null;
            OnPlayerSelectionChanged?.Invoke(playerType);
        }
    }

    /// <summary>
    /// Override the player type for the current level only.
    /// Pass null to clear the override and use the default.
    /// </summary>
    public static void SetLevelOverride(PlayerType? playerType)
    {
        if (Instance == null)
            return;

        if (Instance.levelOverride != playerType)
        {
            Instance.levelOverride = playerType;
            var selected = GetSelectedPlayer();
            OnPlayerSelectionChanged?.Invoke(selected);
        }
    }

    /// <summary>
    /// Check if a given player type is currently selected.
    /// </summary>
    public static bool IsPlayerSelected(PlayerType playerType)
    {
        return GetSelectedPlayer() == playerType;
    }

    /// <summary>
    /// Set the recommended player for the upcoming map (supplied by map metadata
    /// or a Lönn trigger).  Pass <c>null</c> to clear the recommendation.
    /// </summary>
    public static void SetRecommendedPlayer(PlayerType? playerType)
    {
        if (Instance == null)
            return;
        Instance.recommendedPlayer = playerType;
    }

    /// <summary>
    /// Get the recommended player for the current map, if any.
    /// Returns <c>null</c> when no recommendation has been set.
    /// </summary>
    public static PlayerType? GetRecommendedPlayer() =>
        Instance?.recommendedPlayer;

    /// <summary>
    /// Apply <see cref="GetRecommendedPlayer()"/> as the active selection and
    /// clear the stored recommendation.  Does nothing when no recommendation
    /// has been set.  Call this just before entering a level.
    /// </summary>
    public static void ApplyRecommendedPlayerIfAuto()
    {
        if (Instance == null) return;
        if (!AutoSelectEnabled) return;
        if (Instance.recommendedPlayer == null) return;

        SetDefaultPlayer(Instance.recommendedPlayer.Value);
        Instance.recommendedPlayer = null;
    }

    /// <summary>
    /// Reset to defaults and clear per-level overrides.
    /// Used for mod reset or debug purposes.
    /// </summary>
    public static void Reset()
    {
        if (Instance == null)
            return;

        Instance.LoadDefaultFromSettings();
        Instance.levelOverride = null;
        OnPlayerSelectionChanged?.Invoke(Instance.currentPlayerType);
    }

    /// <summary>
    /// Load the default player type from mod settings.
    /// CALL THIS: on mod init, after settings load, or to reset to default settings.
    /// </summary>
    private void LoadDefaultFromSettings()
    {
        // Derive default from whether Kirby player is enabled in settings.
        // KirbyPlayerEnabled == true  → Kirby is the default character.
        // KirbyPlayerEnabled == false → Fall back to Madeline.
        bool kirbyEnabled = DZModule.Settings?.KirbyPlayerEnabled ?? true;
        defaultPlayerType = kirbyEnabled ? PlayerType.Kirby : PlayerType.Madeline;
        currentPlayerType = defaultPlayerType;
        levelOverride = null;
    }

    /// <summary>
    /// Create or retrieve the singleton instance for the given level.
    /// Call this during level initialization to ensure manager exists.
    /// </summary>
    public static PlayerSelectionManager GetOrCreate(Level level)
    {
        if (Instance != null)
            return Instance;

        var manager = level?.Tracker?.GetEntity<PlayerSelectionManager>();
        if (manager == null)
        {
            manager = new PlayerSelectionManager();
            level?.Add(manager);
        }

        return manager;
    }

    /// <summary>
    /// Returns a human-readable name for the player type.
    /// </summary>
    public static string GetPlayerName(PlayerType playerType)
    {
        return playerType switch
        {
            PlayerType.Kirby => "Kirby",
            PlayerType.Madeline => "Madeline",
            _ => "Unknown"
        };
    }

    public override void Update()
    {
        base.Update();
        // Passive; no active behavior needed per frame
    }
}
