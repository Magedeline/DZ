namespace Celeste;

/// <summary>
/// Facade for initializing all metadata registries.
/// Each registry is now in its own file for better maintainability.
/// </summary>
public static class MetadataRegistries
{
    private static bool _initialized = false;

    /// <summary>Initialize all metadata registries from the mod's root path</summary>
    public static void Initialize()
    {
        if (_initialized)
            return;

        try
        {
            var mod = Everest.Content.Mods
                .FirstOrDefault(m => m.Name == "MaggyHelper");

            string modRoot = "";
            if (mod != null)
            {
                var mapPath = mod.Map?.FirstOrDefault().Key;
                if (!string.IsNullOrEmpty(mapPath))
                {
                    modRoot = Path.GetDirectoryName(mapPath) ?? "";
                }
            }

            if (string.IsNullOrEmpty(modRoot))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper",
                    "Could not find mod root path for metadata initialization");
                return;
            }

            AreaMetadataRegistry.Initialize(modRoot);
            AltSideMetadataRegistry.Initialize(modRoot);
            PluginMetadataRegistry.Initialize(modRoot);
            SubmapMetadataRegistry.Initialize(modRoot);
            CutsceneMetadataRegistry.Initialize(modRoot);
            ModelMetadataRegistry.Initialize(modRoot);
            InventoryMetadataRegistry.Initialize(modRoot);
            AudioMetadataRegistry.Initialize(modRoot);
            ParticleMetadataRegistry.Initialize(modRoot);
            UIMetadataRegistry.Initialize(modRoot);

            _initialized = true;
            Logger.Log(LogLevel.Info, "MaggyHelper",
                "All metadata registries initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper",
                $"Error initializing metadata registries: {ex.Message}");
        }
    }

    /// <summary>Reload all metadata registries</summary>
    public static void ReloadAll()
    {
        Logger.Log(LogLevel.Info, "MaggyHelper", "Reloading all metadata registries...");

        AreaMetadataRegistry.Reload();
        AltSideMetadataRegistry.Reload();
        PluginMetadataRegistry.Reload();
        SubmapMetadataRegistry.Reload();
        CutsceneMetadataRegistry.Reload();
        ModelMetadataRegistry.Reload();
        InventoryMetadataRegistry.Reload();
        AudioMetadataRegistry.Reload();
        ParticleMetadataRegistry.Reload();
        UIMetadataRegistry.Reload();

        Logger.Log(LogLevel.Info, "MaggyHelper",
            "All metadata registries reloaded successfully");
    }

    /// <summary>Get metadata statistics</summary>
    public static MetadataStatistics GetMetadataStatistics()
    {
        return new MetadataStatistics
        {
            Areas = AreaMetadataRegistry.Areas.Count,
            AltSides = AltSideMetadataRegistry.AltSides.Count,
            Plugins = PluginMetadataRegistry.Plugins.Count,
            Submaps = SubmapMetadataRegistry.Submaps.Count,
            Cutscenes = CutsceneMetadataRegistry.Cutscenes.Count,
            Models = ModelMetadataRegistry.Models.Count,
            InventoryProfiles = InventoryMetadataRegistry.Profiles.Count,
            AudioEvents = AudioMetadataRegistry.Audio.Count,
            ParticleEffects = ParticleMetadataRegistry.Particles.Count,
            UIThemes = UIMetadataRegistry.Themes.Count
        };
    }
}

/// <summary>Metadata statistics snapshot</summary>
public class MetadataStatistics
{
    public int Areas { get; set; }
    public int AltSides { get; set; }
    public int Plugins { get; set; }
    public int Submaps { get; set; }
    public int Cutscenes { get; set; }
    public int Models { get; set; }
    public int InventoryProfiles { get; set; }
    public int AudioEvents { get; set; }
    public int ParticleEffects { get; set; }
    public int UIThemes { get; set; }

    public int Total => Areas + AltSides + Plugins + Submaps + Cutscenes + Models +
                        InventoryProfiles + AudioEvents + ParticleEffects + UIThemes;
}
