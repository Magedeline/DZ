namespace Celeste;

internal class SubmapMetadata
{
    public string Id { get; set; }
    public string ParentArea { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Vector2Data EntryPoint { get; set; }
    public Vector2Data ExitPoint { get; set; }
    public string RequiredFlag { get; set; }
    public string CompletionFlag { get; set; }
    public string BackgroundMusic { get; set; }
    public string AmbientSound { get; set; }
    public string Difficulty { get; set; }
    public bool IsSecret { get; set; }
    public List<string> UnlockConditions { get; set; }

    public SubmapMetadata()
    {
        UnlockConditions = new List<string>();
        EntryPoint = new Vector2Data();
        ExitPoint = new Vector2Data();
    }
}

internal static class SubmapMetadataRegistry
{
    private static readonly Dictionary<string, SubmapMetadata> _submaps = new();
    private static string _dir;

    public static IReadOnlyDictionary<string, SubmapMetadata> Submaps => _submaps;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "submaps");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    public static SubmapMetadata Find(string id) => _submaps.TryGetValue(id, out var submap) ? submap : null;

    public static IEnumerable<SubmapMetadata> GetByParentArea(string parentArea) =>
        _submaps.Values.Where(s => s.ParentArea == parentArea);

    public static bool ValidateSubmap(SubmapMetadata submap, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(submap?.Id))
        {
            error = "Invalid or missing submap ID";
            return false;
        }
        if (string.IsNullOrWhiteSpace(submap.ParentArea))
        {
            error = "Invalid or missing parent area";
            return false;
        }
        return true;
    }

    private static void LoadAll()
    {
        _submaps.Clear();
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
                    var list = deserializer.Deserialize<List<SubmapMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(s => !string.IsNullOrWhiteSpace(s.Id)))
                    {
                        if (ValidateSubmap(m, out var error))
                            _submaps[m.Id] = m;
                        else
                            Logger.Log(LogLevel.Warn, "MaggyHelper",
                                $"SubmapMetadataRegistry: Invalid submap metadata '{m.Id}': {error}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "MaggyHelper",
                        $"Failed to load submap metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"SubmapMetadataRegistry: Loaded {_submaps.Count} submap metadata entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"SubmapMetadataRegistry: Failed to load submap metadata - {ex.Message}");
        }
    }
}
