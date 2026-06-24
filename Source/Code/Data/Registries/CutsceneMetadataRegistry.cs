namespace Celeste;

internal class CutsceneMetadata
{
    public string Id { get; set; }
    public string Type { get; set; }
    public float Duration { get; set; }
    public string Description { get; set; }
    public List<string> Characters { get; set; }
    public List<string> Dialog { get; set; }
    public Dictionary<string, string> Animations { get; set; }
    public List<Dictionary<string, object>> CameraMovements { get; set; }
    public string MusicTrack { get; set; }
    public List<string> SoundEffects { get; set; }
    public string ScreenEffects { get; set; }
    public string TransitionIn { get; set; }
    public string TransitionOut { get; set; }
    public bool Skippable { get; set; }
    public bool AutoStart { get; set; }
    public bool TriggerOnce { get; set; }
    public string RequiredFlag { get; set; }
    public string FlagToSet { get; set; }

    public CutsceneMetadata()
    {
        Characters = new List<string>();
        Dialog = new List<string>();
        Animations = new Dictionary<string, string>();
        CameraMovements = new List<Dictionary<string, object>>();
        SoundEffects = new List<string>();
        Skippable = true;
        TriggerOnce = true;
    }
}

internal static class CutsceneMetadataRegistry
{
    private static readonly Dictionary<string, CutsceneMetadata> _cutscenes = new();
    private static string _dir;

    public static IReadOnlyDictionary<string, CutsceneMetadata> Cutscenes => _cutscenes;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "cutscenes");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    public static CutsceneMetadata Find(string id) => _cutscenes.TryGetValue(id, out var cutscene) ? cutscene : null;

    public static IEnumerable<CutsceneMetadata> GetByType(string type) =>
        _cutscenes.Values.Where(c => c.Type == type);

    public static bool ValidateCutscene(CutsceneMetadata cutscene, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(cutscene?.Id))
        {
            error = "Invalid or missing cutscene ID";
            return false;
        }
        if (!string.IsNullOrEmpty(cutscene.Type) &&
            cutscene.Type != "dialog" && cutscene.Type != "animation" && cutscene.Type != "scripted")
        {
            error = "Invalid cutscene type (must be 'dialog', 'animation', or 'scripted')";
            return false;
        }
        if (cutscene.Duration < 0)
        {
            error = "Invalid duration (must be non-negative)";
            return false;
        }
        return true;
    }

    private static void LoadAll()
    {
        _cutscenes.Clear();
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
                    var list = deserializer.Deserialize<List<CutsceneMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(c => !string.IsNullOrWhiteSpace(c.Id)))
                    {
                        if (ValidateCutscene(m, out var error))
                            _cutscenes[m.Id] = m;
                        else
                            Logger.Log(LogLevel.Warn, "MaggyHelper",
                                $"CutsceneMetadataRegistry: Invalid cutscene metadata '{m.Id}': {error}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "MaggyHelper",
                        $"Failed to load cutscene metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"CutsceneMetadataRegistry: Loaded {_cutscenes.Count} cutscene metadata entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"CutsceneMetadataRegistry: Failed to load cutscene metadata - {ex.Message}");
        }
    }
}
