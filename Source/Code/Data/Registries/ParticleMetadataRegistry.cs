namespace Celeste;

internal class ParticleMetadata
{
    public string Id { get; set; }
    public string EffectName { get; set; }
    public string Texture { get; set; }
    public float Lifetime { get; set; }
    public float StartSize { get; set; }
    public float EndSize { get; set; }
    public ColorData StartColor { get; set; }
    public ColorData EndColor { get; set; }
    public Vector3Data Velocity { get; set; }
    public Vector3Data Acceleration { get; set; }
    public Vector3Data Gravity { get; set; }
    public float EmissionRate { get; set; }
    public int MaxParticles { get; set; }
    public string BlendMode { get; set; }
    public bool Billboard { get; set; }
    public bool WorldSpace { get; set; }
    public List<string> Tags { get; set; }
    public string Description { get; set; }

    public ParticleMetadata()
    {
        Tags = new List<string>();
        StartColor = new ColorData(1f, 1f, 1f, 1f);
        EndColor = new ColorData(1f, 1f, 1f, 0f);
        Velocity = new Vector3Data();
        Acceleration = new Vector3Data();
        Gravity = new Vector3Data(0f, -9.81f, 0f);
        Lifetime = 1f;
        StartSize = 1f;
        EmissionRate = 10f;
        MaxParticles = 100;
        BlendMode = "Alpha";
        Billboard = true;
    }
}

internal static class ParticleMetadataRegistry
{
    private static readonly Dictionary<string, ParticleMetadata> _particles = new();
    private static string _dir;

    public static IReadOnlyDictionary<string, ParticleMetadata> Particles => _particles;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "particles");
        LoadAll();
    }

    public static void Reload() => LoadAll();

    public static ParticleMetadata Find(string id) => _particles.TryGetValue(id, out var particle) ? particle : null;

    public static IEnumerable<ParticleMetadata> GetByTag(string tag) =>
        _particles.Values.Where(p => p.Tags.Contains(tag));

    private static void LoadAll()
    {
        _particles.Clear();
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
                    var list = deserializer.Deserialize<List<ParticleMetadata>>(txt);
                    if (list == null)
                        continue;

                    foreach (var m in list.Where(p => !string.IsNullOrWhiteSpace(p.Id)))
                        _particles[m.Id] = m;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "MaggyHelper",
                        $"Failed to load particle metadata from file: {file} - {ex.Message}");
                }
            }

            Logger.Log(LogLevel.Info, "MaggyHelper",
                $"ParticleMetadataRegistry: Loaded {_particles.Count} particle metadata entries");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, "MaggyHelper",
                $"ParticleMetadataRegistry: Failed to load particle metadata - {ex.Message}");
        }
    }
}
