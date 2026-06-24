namespace Celeste;

internal class PluginMetadata
{
    public string Id { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }

    public PluginMetadata() { }

    public PluginMetadata(string id, string version, string author, string description)
    {
        Id = id;
        Version = version;
        Author = author;
        Description = description;
    }
}

internal static class PluginMetadataRegistry
{
    private static readonly Dictionary<string, PluginMetadata> _plugins = new();
    private static string _dir;

    public static IReadOnlyDictionary<string, PluginMetadata> Plugins => _plugins;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "plugins");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    private static void LoadAll()
    {
        _plugins.Clear();
        try
        {
            if (!Directory.Exists(_dir))
                return;

            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var deserializer = RegistryDeserializer.GetDeserializer();
                    var list = deserializer.Deserialize<List<PluginMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(a => !string.IsNullOrWhiteSpace(a.Id)))
                        _plugins[m.Id] = m;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "MaggyHelper",
                        $"Failed to load plugin metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"PluginMetadataRegistry: Loaded {_plugins.Count} plugin metadata entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"PluginMetadataRegistry: Failed to load plugin metadata - {ex.Message}");
        }
    }
}
