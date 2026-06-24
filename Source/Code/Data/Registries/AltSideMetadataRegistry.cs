namespace Celeste;

internal class AltSideMetadata
{
    public string Id { get; set; }
    public string FromSide { get; set; }
    public string ToSide { get; set; }
    public string DisplayName { get; set; }

    public AltSideMetadata() { }

    public AltSideMetadata(string id, string fromSide, string toSide, string displayName)
    {
        Id = id;
        FromSide = fromSide;
        ToSide = toSide;
        DisplayName = displayName;
    }
}

internal static class AltSideMetadataRegistry
{
    private static readonly List<AltSideMetadata> _list = new();
    private static string _dir;

    public static IReadOnlyList<AltSideMetadata> AltSides => _list;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "altside");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    private static void LoadAll()
    {
        _list.Clear();
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
                    var list = deserializer.Deserialize<List<AltSideMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(a => !string.IsNullOrWhiteSpace(a.Id)))
                        _list.Add(m);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "MaggyHelper",
                        $"Failed to load alt-side metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"AltSideMetadataRegistry: Loaded {_list.Count} alt-side metadata entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"AltSideMetadataRegistry: Failed to load alt-side metadata - {ex.Message}");
        }
    }
}
