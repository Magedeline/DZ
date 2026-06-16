namespace DZ;

internal class AreaMetadata
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Side { get; set; }
    public string Description { get; set; }

    public AreaMetadata() { }

    public AreaMetadata(string id, string name, string side, string description)
    {
        Id = id;
        Name = name;
        Side = side;
        Description = description;
    }
}

internal static class AreaMetadataRegistry
{
    private static readonly Dictionary<string, AreaMetadata> _areas = new();
    private static string _dir;

    public static IReadOnlyDictionary<string, AreaMetadata> Areas => _areas;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "areas");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    private static void LoadAll()
    {
        _areas.Clear();
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
                    var list = deserializer.Deserialize<List<AreaMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(a => !string.IsNullOrWhiteSpace(a.Id)))
                        _areas[m.Id] = m;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "DZ",
                        $"Failed to load area metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "DZ",
                $"AreaMetadataRegistry: Loaded {_areas.Count} area metadata entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ",
                $"AreaMetadataRegistry: Failed to load area metadata - {ex.Message}");
        }
    }
}
