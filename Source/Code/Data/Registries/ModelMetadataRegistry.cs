namespace Celeste;

internal class ModelMetadata
{
    public string Id { get; set; }
    public string ModelPath { get; set; }
    public string TexturePath { get; set; }
    public string NormalMapPath { get; set; }
    public string MaterialPath { get; set; }
    public Vector3Data Scale { get; set; }
    public Vector3Data Rotation { get; set; }
    public Vector3Data Position { get; set; }
    public List<string> Animations { get; set; }
    public Vector3Data BoundingBox { get; set; }
    public string CollisionMesh { get; set; }
    public List<Dictionary<string, object>> LodLevels { get; set; }
    public float RenderDistance { get; set; }
    public bool CastShadows { get; set; }
    public bool ReceiveShadows { get; set; }
    public bool IsStatic { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }

    public ModelMetadata()
    {
        Scale = new Vector3Data(1f, 1f, 1f);
        Rotation = new Vector3Data();
        Position = new Vector3Data();
        Animations = new List<string>();
        LodLevels = new List<Dictionary<string, object>>();
        RenderDistance = 1000f;
        CastShadows = true;
        ReceiveShadows = true;
        IsStatic = true;
        Category = "Environment";
    }
}

internal static class ModelMetadataRegistry
{
    private static readonly Dictionary<string, ModelMetadata> _models = new();
    private static string _dir;

    public static IReadOnlyDictionary<string, ModelMetadata> Models => _models;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "models");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    public static ModelMetadata Find(string id) => _models.TryGetValue(id, out var model) ? model : null;

    public static IEnumerable<ModelMetadata> GetByCategory(string category) =>
        _models.Values.Where(m => m.Category == category);

    public static bool ValidateModel(ModelMetadata model, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(model?.Id))
        {
            error = "Invalid or missing model ID";
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.ModelPath))
        {
            error = "Invalid or missing model path";
            return false;
        }
        return true;
    }

    private static void LoadAll()
    {
        _models.Clear();
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
                    var list = deserializer.Deserialize<List<ModelMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(model => !string.IsNullOrWhiteSpace(model.Id)))
                    {
                        if (ValidateModel(m, out var error))
                            _models[m.Id] = m;
                        else
                            Logger.Log(LogLevel.Warn, "MaggyHelper",
                                $"ModelMetadataRegistry: Invalid model metadata '{m.Id}': {error}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "MaggyHelper",
                        $"Failed to load model metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"ModelMetadataRegistry: Loaded {_models.Count} model metadata entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"ModelMetadataRegistry: Failed to load model metadata - {ex.Message}");
        }
    }
}
