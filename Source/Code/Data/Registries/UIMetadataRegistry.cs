namespace Celeste;

internal class UIMetadata
{
    public string Id { get; set; }
    public string ThemeName { get; set; }
    public Dictionary<string, string> ColorScheme { get; set; }
    public string Layout { get; set; }
    public string FontFamily { get; set; }
    public int FontSize { get; set; }
    public string ButtonStyle { get; set; }
    public string PanelStyle { get; set; }
    public string IconSet { get; set; }
    public List<string> Animations { get; set; }
    public Dictionary<string, string> SoundEffects { get; set; }
    public Dictionary<string, int> ResponsiveBreakpoints { get; set; }
    public Dictionary<string, bool> Accessibility { get; set; }
    public Dictionary<string, object> CustomProperties { get; set; }
    public string Description { get; set; }

    public UIMetadata()
    {
        ColorScheme = new Dictionary<string, string>();
        Animations = new List<string>();
        SoundEffects = new Dictionary<string, string>();
        ResponsiveBreakpoints = new Dictionary<string, int>();
        Accessibility = new Dictionary<string, bool>();
        CustomProperties = new Dictionary<string, object>();
        FontFamily = "default";
        FontSize = 12;
        Layout = "default";
    }
}

internal static class UIMetadataRegistry
{
    private static readonly Dictionary<string, UIMetadata> _themes = new();
    private static string _dir;

    public static IReadOnlyDictionary<string, UIMetadata> Themes => _themes;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "ui");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    public static UIMetadata Find(string id) => _themes.TryGetValue(id, out var theme) ? theme : null;

    private static void LoadAll()
    {
        _themes.Clear();
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
                    var list = deserializer.Deserialize<List<UIMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(t => !string.IsNullOrWhiteSpace(t.Id)))
                        _themes[m.Id] = m;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "MaggyHelper",
                        $"Failed to load UI metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"UIMetadataRegistry: Loaded {_themes.Count} UI theme entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"UIMetadataRegistry: Failed to load UI metadata - {ex.Message}");
        }
    }
}
