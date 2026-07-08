namespace DZ;

internal class InventoryMetadata
{
    public string ProfileId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<string> Items { get; set; }
    public List<string> Abilities { get; set; }
    public List<string> Unlocks { get; set; }
    public List<string> PowerUps { get; set; }
    public List<Dictionary<string, object>> Collectibles { get; set; }
    public int MaxHealth { get; set; }
    public int MaxStamina { get; set; }
    public int StartingHealth { get; set; }
    public int StartingStamina { get; set; }
    public int StartingDashes { get; set; }
    public bool HasWallJump { get; set; }
    public bool HasClimbing { get; set; }
    public bool HasDashing { get; set; }
    public bool HasDoubleJump { get; set; }
    public List<string> CustomFlags { get; set; }

    public InventoryMetadata()
    {
        Items = new List<string>();
        Abilities = new List<string>();
        Unlocks = new List<string>();
        PowerUps = new List<string>();
        Collectibles = new List<Dictionary<string, object>>();
        CustomFlags = new List<string>();
        MaxHealth = 100;
        MaxStamina = 100;
        StartingHealth = 100;
        StartingStamina = 100;
        StartingDashes = 1;
        HasClimbing = true;
        HasDashing = true;
    }
}

internal static class InventoryMetadataRegistry
{
    private static readonly Dictionary<string, InventoryMetadata> _profiles = new();
    private static string _dir;

    public static IReadOnlyDictionary<string, InventoryMetadata> Profiles => _profiles;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "inventory");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    public static InventoryMetadata Find(string profileId) => _profiles.TryGetValue(profileId, out var profile) ? profile : null;

    public static bool ValidateInventoryProfile(InventoryMetadata profile, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(profile?.ProfileId))
        {
            error = "Invalid or missing profile ID";
            return false;
        }
        if (profile.MaxHealth <= 0)
        {
            error = "Invalid max health (must be positive)";
            return false;
        }
        if (profile.MaxStamina <= 0)
        {
            error = "Invalid max stamina (must be positive)";
            return false;
        }
        if (profile.StartingDashes < 0)
        {
            error = "Invalid starting dashes (must be non-negative)";
            return false;
        }
        return true;
    }

    private static void LoadAll()
    {
        _profiles.Clear();
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
                    var list = deserializer.Deserialize<List<InventoryMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(p => !string.IsNullOrWhiteSpace(p.ProfileId)))
                    {
                        if (ValidateInventoryProfile(m, out var error))
                            _profiles[m.ProfileId] = m;
                        else
                            Logger.Log(LogLevel.Warn, "DZ",
                                $"InventoryMetadataRegistry: Invalid inventory profile '{m.ProfileId}': {error}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "DZ",
                        $"Failed to load inventory metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "DZ",
                $"InventoryMetadataRegistry: Loaded {_profiles.Count} inventory profile entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "DZ",
                $"InventoryMetadataRegistry: Failed to load inventory metadata - {ex.Message}");
        }
    }
}
