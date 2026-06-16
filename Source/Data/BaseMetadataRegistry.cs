using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DZ;

/// <summary>
/// Abstract base class for all metadata registries to eliminate duplication.
/// Handles YAML deserialization, directory initialization, and loading/reloading logic.
/// </summary>
/// <typeparam name="TMetadata">The metadata type (e.g., AreaMetadata, SubmapMetadata)</typeparam>
/// <typeparam name="TRegistry">The registry type itself (for static access)</typeparam>
internal abstract class BaseMetadataRegistry<TMetadata, TRegistry>
    where TMetadata : class
    where TRegistry : BaseMetadataRegistry<TMetadata, TRegistry>, new()
{
    /// <summary>
    /// Singleton instance of the concrete registry, used to invoke the
    /// instance-level abstract members from the static API.
    /// </summary>
    protected static readonly TRegistry Instance = new();

    protected static readonly Dictionary<string, TMetadata> Items = new();
    protected static string RegistryDirectory;
    protected static readonly IDeserializer Deserializer =
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    /// <summary>Gets the human-readable name of this registry (for logging)</summary>
    protected abstract string RegistryName { get; }

    /// <summary>Initialize the registry with a directory path</summary>
    public static void Initialize(string modRoot)
    {
        RegistryDirectory = Instance.GetRegistryDirectory(modRoot);
        LoadAll();
    }

    /// <summary>Reload all metadata from disk</summary>
    public static void Reload() => LoadAll();

    /// <summary>Get the directory path for this registry's metadata files</summary>
    protected abstract string GetRegistryDirectory(string modRoot);

    /// <summary>Load all metadata from YAML files in the registry directory</summary>
    protected static void LoadAll()
    {
        Items.Clear();
        try
        {
            if (!Directory.Exists(RegistryDirectory))
                return;

            foreach (var file in Directory.GetFiles(RegistryDirectory, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = Deserializer.Deserialize<List<TMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var item in list)
                    {
                        Instance.OnItemLoaded(item);
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to load metadata from file: {file}", ex);
                }
            }

            LogInfo($"Loaded {Items.Count} entries");
        }
        catch (Exception ex)
        {
            LogError($"Failed to load metadata", ex);
        }
    }

    /// <summary>Called after an item is deserialized. Override to customize storage behavior.</summary>
    protected abstract void OnItemLoaded(TMetadata item);

    /// <summary>Log an info message</summary>
    protected static void LogInfo(string message) =>
        Logger.Log(LogLevel.Info, "DZ", $"{typeof(TRegistry).Name}: {message}");

    /// <summary>Log a warning message</summary>
    protected static void LogWarn(string message) =>
        Logger.Log(LogLevel.Warn, "DZ", $"{typeof(TRegistry).Name}: {message}");

    /// <summary>Log an error message</summary>
    protected static void LogError(string message, Exception ex = null)
    {
        if (ex != null)
            Logger.Log(LogLevel.Error, "DZ", $"{typeof(TRegistry).Name}: {message} - {ex.GetType().Name}: {ex.Message}");
        else
            Logger.Log(LogLevel.Error, "DZ", $"{typeof(TRegistry).Name}: {message}");
    }
}
