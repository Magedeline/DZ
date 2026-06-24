namespace Celeste;

internal class AudioMetadata
{
    public string Id { get; set; }
    public string EventPath { get; set; }
    public string Category { get; set; }
    public float Volume { get; set; }
    public float Pitch { get; set; }
    public bool Looping { get; set; }
    public float FadeInTime { get; set; }
    public float FadeOutTime { get; set; }
    public int Priority { get; set; }
    public int MaxInstances { get; set; }
    public float MinDistance { get; set; }
    public float MaxDistance { get; set; }
    public string RolloffMode { get; set; }
    public float SpatialBlend { get; set; }
    public List<string> Tags { get; set; }
    public string Description { get; set; }

    public AudioMetadata()
    {
        Tags = new List<string>();
        Volume = 1f;
        Pitch = 1f;
        Priority = 50;
        MaxInstances = 1;
        MinDistance = 1f;
        MaxDistance = 100f;
        RolloffMode = "Linear";
        SpatialBlend = 1f;
        Category = "sfx";
    }
}

internal static class AudioMetadataRegistry
{
    private static readonly Dictionary<string, AudioMetadata> _audio = new();
    private static string _dir;

    public static IReadOnlyDictionary<string, AudioMetadata> Audio => _audio;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "audio");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    public static AudioMetadata Find(string id) => _audio.TryGetValue(id, out var audio) ? audio : null;

    public static IEnumerable<AudioMetadata> GetByCategory(string category) =>
        _audio.Values.Where(a => a.Category == category);

    public static IEnumerable<AudioMetadata> GetByTag(string tag) =>
        _audio.Values.Where(a => a.Tags.Contains(tag));

    private static void LoadAll()
    {
        _audio.Clear();
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
                    var list = deserializer.Deserialize<List<AudioMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(a => !string.IsNullOrWhiteSpace(a.Id)))
                        _audio[m.Id] = m;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "MaggyHelper",
                        $"Failed to load audio metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"AudioMetadataRegistry: Loaded {_audio.Count} audio metadata entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"AudioMetadataRegistry: Failed to load audio metadata - {ex.Message}");
        }
    }
}
