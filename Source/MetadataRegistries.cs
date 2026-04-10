using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MaggyHelper;

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

// New metadata classes for extended functionality

internal class Vector3Data
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public Vector3Data() { }
    public Vector3Data(float x, float y, float z) { X = x; Y = y; Z = z; }
    
    public Vector3 ToVector3() => new Vector3(X, Y, Z);
    public static Vector3Data FromVector3(Vector3 v) => new Vector3Data(v.X, v.Y, v.Z);
}

internal class Vector2Data
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2Data() { }
    public Vector2Data(float x, float y) { X = x; Y = y; }
    
    public Vector2 ToVector2() => new Vector2(X, Y);
    public static Vector2Data FromVector2(Vector2 v) => new Vector2Data(v.X, v.Y);
}

internal class Vector4Data
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }

    public Vector4Data() { }
    public Vector4Data(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
    
    public Vector4 ToVector4() => new Vector4(X, Y, Z, W);
    public static Vector4Data FromVector4(Vector4 v) => new Vector4Data(v.X, v.Y, v.Z, v.W);
}

internal class ColorData
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public ColorData() { }
    public ColorData(float r, float g, float b, float a) { R = r; G = g; B = b; A = a; }
    
    public Color ToColor() => new Color(R, G, B, A);
    public static ColorData FromColor(Color c) => new ColorData(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
}

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

internal class PlayerMetadata
{
    public string Id { get; set; }
    public string CharacterName { get; set; }
    public string Description { get; set; }
    public float BaseHealth { get; set; }
    public float BaseStamina { get; set; }
    public float MovementSpeed { get; set; }
    public float JumpHeight { get; set; }
    public float DashDistance { get; set; }
    public int MaxDashes { get; set; }
    public bool CanWallJump { get; set; }
    public bool CanClimb { get; set; }
    public bool CanSwim { get; set; }
    public List<string> StartingAbilities { get; set; }
    public List<string> UnlockableAbilities { get; set; }
    public Dictionary<string, float> AbilityModifiers { get; set; }
    public string DefaultSkin { get; set; }
    public List<string> UnlockableSkins { get; set; }
    public Dictionary<string, object> CustomStats { get; set; }
    public List<string> SpecialMoves { get; set; }
    public Dictionary<string, string> VoiceLines { get; set; }
    public string CharacterClass { get; set; }
    public Dictionary<string, ColorData> ElementalAffinities { get; set; }

    public PlayerMetadata()
    {
        StartingAbilities = new List<string>();
        UnlockableAbilities = new List<string>();
        AbilityModifiers = new Dictionary<string, float>();
        UnlockableSkins = new List<string>();
        CustomStats = new Dictionary<string, object>();
        SpecialMoves = new List<string>();
        VoiceLines = new Dictionary<string, string>();
        ElementalAffinities = new Dictionary<string, ColorData>();
        BaseHealth = 100f;
        BaseStamina = 100f;
        MovementSpeed = 1f;
        MaxDashes = 1;
    }
}

internal class BossMetadata
{
    public string Id { get; set; }
    public string BossName { get; set; }
    public string Description { get; set; }
    public float BaseHealth { get; set; }
    public List<Dictionary<string, object>> Phases { get; set; }
    public Dictionary<string, object> AttackPatterns { get; set; }
    public List<string> SpecialAbilities { get; set; }
    public string ArenaId { get; set; }
    public Vector2Data SpawnPoint { get; set; }
    public string IntroductionCutscene { get; set; }
    public string DefeatCutscene { get; set; }
    public List<string> WeakPoints { get; set; }
    public Dictionary<string, float> ResistanceModifiers { get; set; }
    public List<string> PhaseTransitionTriggers { get; set; }
    public Dictionary<string, string> BossDialogue { get; set; }
    public string BossThemeMusic { get; set; }
    public List<string> SpecialEffects { get; set; }
    public Dictionary<string, string> RewardPool { get; set; }
    public string Difficulty { get; set; }
    public bool IsOptional { get; set; }
    public List<string> RequiredItems { get; set; }
    public Dictionary<string, object> AIBehavior { get; set; }

    public BossMetadata()
    {
        Phases = new List<Dictionary<string, object>>();
        AttackPatterns = new Dictionary<string, object>();
        SpecialAbilities = new List<string>();
        WeakPoints = new List<string>();
        ResistanceModifiers = new Dictionary<string, float>();
        PhaseTransitionTriggers = new List<string>();
        BossDialogue = new Dictionary<string, string>();
        SpecialEffects = new List<string>();
        RewardPool = new Dictionary<string, string>();
        RequiredItems = new List<string>();
        AIBehavior = new Dictionary<string, object>();
        BaseHealth = 1000f;
        IsOptional = false;
    }
}

internal class ShaderMetadata
{
    public string Id { get; set; }
    public string ShaderName { get; set; }
    public string Description { get; set; }
    public string ShaderPath { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public Dictionary<string, Vector4Data> DefaultValues { get; set; }
    public List<string> TextureSlots { get; set; }
    public string RenderQueue { get; set; }
    public bool IsTransparent { get; set; }
    public bool ReceiveShadows { get; set; }
    public bool CastShadows { get; set; }
    public string BlendMode { get; set; }
    public string CullMode { get; set; }
    public string ZWrite { get; set; }
    public List<string> Keywords { get; set; }
    public Dictionary<string, object> VariantSettings { get; set; }
    public List<string> RequiredFeatures { get; set; }
    public string FallbackShader { get; set; }

    public ShaderMetadata()
    {
        Parameters = new Dictionary<string, object>();
        DefaultValues = new Dictionary<string, Vector4Data>();
        TextureSlots = new List<string>();
        Keywords = new List<string>();
        VariantSettings = new Dictionary<string, object>();
        RequiredFeatures = new List<string>();
        RenderQueue = "Geometry";
        BlendMode = "Opaque";
        CullMode = "Back";
        ZWrite = "On";
        IsTransparent = false;
        ReceiveShadows = true;
        CastShadows = true;
    }
}

internal class InterludeMetadata
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ParentArea { get; set; }
    public Vector2Data EntryPoint { get; set; }
    public Vector2Data ExitPoint { get; set; }
    public float Duration { get; set; }
    public string BackgroundMusic { get; set; }
    public string AmbientSound { get; set; }
    public List<string> RequiredFlags { get; set; }
    public List<string> SetFlags { get; set; }
    public string TransitionIn { get; set; }
    public string TransitionOut { get; set; }
    public List<Dictionary<string, object>> Events { get; set; }
    public Dictionary<string, Vector2Data> Checkpoints { get; set; }
    public List<string> EnvironmentalEffects { get; set; }
    public bool Skippable { get; set; }
    public string WeatherEffect { get; set; }
    public ColorData AmbientLight { get; set; }
    public Dictionary<string, object> Collectibles { get; set; }

    public InterludeMetadata()
    {
        RequiredFlags = new List<string>();
        SetFlags = new List<string>();
        Events = new List<Dictionary<string, object>>();
        Checkpoints = new Dictionary<string, Vector2Data>();
        EnvironmentalEffects = new List<string>();
        Collectibles = new Dictionary<string, object>();
        Duration = 0f;
        Skippable = true;
    }
}

internal static class AreaMetadataRegistry
{
    private static readonly Dictionary<string, AreaMetadata> _areas = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<AreaMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(a => !string.IsNullOrWhiteSpace(a.Id)))
                        _areas[m.Id] = m;
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load area metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_areas.Count} area metadata entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load area metadata");
        }
    }
}

internal static class AltSideMetadataRegistry
{
    private static readonly List<AltSideMetadata> _list = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<AltSideMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(a => !string.IsNullOrWhiteSpace(a.Id)))
                        _list.Add(m);
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load alt-side metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_list.Count} alt-side metadata entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load alt-side metadata");
        }
    }
}

internal static class PluginMetadataRegistry
{
    private static readonly Dictionary<string, PluginMetadata> _plugins = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<PluginMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(a => !string.IsNullOrWhiteSpace(a.Id)))
                        _plugins[m.Id] = m;
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load plugin metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_plugins.Count} plugin metadata entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load plugin metadata");
        }
    }
}

// New registry classes for extended metadata functionality

internal static class SubmapMetadataRegistry
{
    private static readonly Dictionary<string, SubmapMetadata> _submaps = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

    public static IReadOnlyDictionary<string, SubmapMetadata> Submaps => _submaps;

    public static void Initialize(string modRoot)
    {
        _dir = Path.Combine(modRoot, "metadata", "submaps");
        LoadAll();
    }

    // Registry for PlayerMetadata
    internal static class PlayerMetadataRegistry
    {
        private static readonly Dictionary<string, PlayerMetadata> _players = new();
        private static string _dir;
        private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

        public static IReadOnlyDictionary<string, PlayerMetadata> Players => _players;

        public static void Load(string directory)
        {
            _dir = directory;
            _players.Clear();
            if (!Directory.Exists(_dir)) return;
            foreach (string file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                using StreamReader reader = new(file);
                var players = _deserializer.Deserialize<List<PlayerMetadata>>(reader);
                if (players == null) continue;
                foreach (var player in players)
                {
                    if (string.IsNullOrEmpty(player.Id)) continue;
                    _players[player.Id] = player;
                }
            }
        }
    }

    // Registry for BossMetadata
    internal static class BossMetadataRegistry
    {
        private static readonly Dictionary<string, BossMetadata> _bosses = new();
        private static string _dir;
        private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

        public static IReadOnlyDictionary<string, BossMetadata> Bosses => _bosses;

        public static void Load(string directory)
        {
            _dir = directory;
            _bosses.Clear();
            if (!Directory.Exists(_dir)) return;
            foreach (string file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                using StreamReader reader = new(file);
                var bosses = _deserializer.Deserialize<List<BossMetadata>>(reader);
                if (bosses == null) continue;
                foreach (var boss in bosses)
                {
                    if (string.IsNullOrEmpty(boss.Id)) continue;
                    _bosses[boss.Id] = boss;
                }
            }
        }
    }

    // Registry for ShaderMetadata
    internal static class ShaderMetadataRegistry
    {
        private static readonly Dictionary<string, ShaderMetadata> _shaders = new();
        private static string _dir;
        private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

        public static IReadOnlyDictionary<string, ShaderMetadata> Shaders => _shaders;

        public static void Load(string directory)
        {
            _dir = directory;
            _shaders.Clear();
            if (!Directory.Exists(_dir)) return;
            foreach (string file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                using StreamReader reader = new(file);
                var shaders = _deserializer.Deserialize<List<ShaderMetadata>>(reader);
                if (shaders == null) continue;
                foreach (var shader in shaders)
                {
                    if (string.IsNullOrEmpty(shader.Id)) continue;
                    _shaders[shader.Id] = shader;
                }
            }
        }
    }

    // Registry for InterludeMetadata
    internal static class InterludeMetadataRegistry
    {
        private static readonly Dictionary<string, InterludeMetadata> _interludes = new();
        private static string _dir;
        private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

        public static IReadOnlyDictionary<string, InterludeMetadata> Interludes => _interludes;

        public static void Load(string directory)
        {
            _dir = directory;
            _interludes.Clear();
            if (!Directory.Exists(_dir)) return;
            foreach (string file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                using StreamReader reader = new(file);
                var interludes = _deserializer.Deserialize<List<InterludeMetadata>>(reader);
                if (interludes == null) continue;
                foreach (var interlude in interludes)
                {
                    if (string.IsNullOrEmpty(interlude.Id)) continue;
                    _interludes[interlude.Id] = interlude;
                }
            }
        }
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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<SubmapMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(s => !string.IsNullOrWhiteSpace(s.Id)))
                    {
                        if (ValidateSubmap(m, out var error))
                            _submaps[m.Id] = m;
                        else
                            IngesteLogger.Warn($"Invalid submap metadata '{m.Id}': {error}");
                    }
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load submap metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_submaps.Count} submap metadata entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load submap metadata");
        }
    }
}

internal static class CutsceneMetadataRegistry
{
    private static readonly Dictionary<string, CutsceneMetadata> _cutscenes = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<CutsceneMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(c => !string.IsNullOrWhiteSpace(c.Id)))
                    {
                        if (ValidateCutscene(m, out var error))
                            _cutscenes[m.Id] = m;
                        else
                            IngesteLogger.Warn($"Invalid cutscene metadata '{m.Id}': {error}");
                    }
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load cutscene metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_cutscenes.Count} cutscene metadata entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load cutscene metadata");
        }
    }
}

internal static class ModelMetadataRegistry
{
    private static readonly Dictionary<string, ModelMetadata> _models = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<ModelMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(model => !string.IsNullOrWhiteSpace(model.Id)))
                    {
                        if (ValidateModel(m, out var error))
                            _models[m.Id] = m;
                        else
                            IngesteLogger.Warn($"Invalid model metadata '{m.Id}': {error}");
                    }
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load model metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_models.Count} model metadata entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load model metadata");
        }
    }
}

internal static class InventoryMetadataRegistry
{
    private static readonly Dictionary<string, InventoryMetadata> _profiles = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<InventoryMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(p => !string.IsNullOrWhiteSpace(p.ProfileId)))
                    {
                        if (ValidateInventoryProfile(m, out var error))
                            _profiles[m.ProfileId] = m;
                        else
                            IngesteLogger.Warn($"Invalid inventory profile '{m.ProfileId}': {error}");
                    }
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load inventory metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_profiles.Count} inventory profile entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load inventory metadata");
        }
    }
}

internal static class AudioMetadataRegistry
{
    private static readonly Dictionary<string, AudioMetadata> _audio = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<AudioMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(a => !string.IsNullOrWhiteSpace(a.Id)))
                        _audio[m.Id] = m;
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load audio metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_audio.Count} audio metadata entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load audio metadata");
        }
    }
}

internal static class ParticleMetadataRegistry
{
    private static readonly Dictionary<string, ParticleMetadata> _particles = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<ParticleMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(p => !string.IsNullOrWhiteSpace(p.Id)))
                        _particles[m.Id] = m;
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load particle metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_particles.Count} particle metadata entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load particle metadata");
        }
    }
}

internal static class UIMetadataRegistry
{
    private static readonly Dictionary<string, UIMetadata> _themes = new();
    private static string _dir;
    private static IDeserializer _deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

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
            if (!Directory.Exists(_dir)) return;
            foreach (var file in Directory.GetFiles(_dir, "*.yaml", SearchOption.AllDirectories))
            {
                try
                {
                    var txt = File.ReadAllText(file);
                    var list = _deserializer.Deserialize<List<UIMetadata>>(txt);
                    if (list == null) continue;
                    foreach (var m in list.Where(t => !string.IsNullOrWhiteSpace(t.Id)))
                        _themes[m.Id] = m;
                }
                catch (Exception ex)
                {
                    IngesteLogger.Error(ex, $"Failed to load UI metadata from file: {file}");
                }
            }
            IngesteLogger.Info($"Loaded {_themes.Count} UI theme entries");
        }
        catch (Exception ex)
        {
            IngesteLogger.Error(ex, "Failed to load UI metadata");
        }
    }
}

// Central metadata manager for all registry types
internal static class MetadataManager
{
    private static string _modRoot;

    public static void Initialize(string modRoot)
    {
        _modRoot = modRoot;
        IngesteLogger.Info("Initializing metadata registries...");
        
        // Initialize all metadata registries
        AreaMetadataRegistry.Initialize(modRoot);
        AltSideMetadataRegistry.Initialize(modRoot);
        PluginMetadataRegistry.Initialize(modRoot);
        SubmapMetadataRegistry.Initialize(modRoot);
        CutsceneMetadataRegistry.Initialize(modRoot);
        ModelMetadataRegistry.Initialize(modRoot);
        InventoryMetadataRegistry.Initialize(modRoot);
        AudioMetadataRegistry.Initialize(modRoot);
        ParticleMetadataRegistry.Initialize(modRoot);
        UIMetadataRegistry.Initialize(modRoot);
        
        IngesteLogger.Info("All metadata registries initialized successfully");
        LogMetadataStatistics();
    }

    public static void ReloadAll()
    {
        IngesteLogger.Info("Reloading all metadata registries...");
        
        AreaMetadataRegistry.Reload();
        AltSideMetadataRegistry.Reload();
        PluginMetadataRegistry.Reload();
        SubmapMetadataRegistry.Reload();
        CutsceneMetadataRegistry.Reload();
        ModelMetadataRegistry.Reload();
        InventoryMetadataRegistry.Reload();
        AudioMetadataRegistry.Reload();
        ParticleMetadataRegistry.Reload();
        UIMetadataRegistry.Reload();
        
        IngesteLogger.Info("All metadata registries reloaded successfully");
        LogMetadataStatistics();
    }

    public static void LogMetadataStatistics()
    {
        var stats = GetMetadataStatistics();
        IngesteLogger.Info($"Metadata Statistics:");
        IngesteLogger.Info($"  Areas: {stats.Areas}");
        IngesteLogger.Info($"  Alt Sides: {stats.AltSides}");
        IngesteLogger.Info($"  Plugins: {stats.Plugins}");
        IngesteLogger.Info($"  Submaps: {stats.Submaps}");
        IngesteLogger.Info($"  Cutscenes: {stats.Cutscenes}");
        IngesteLogger.Info($"  Models: {stats.Models}");
        IngesteLogger.Info($"  Inventory Profiles: {stats.InventoryProfiles}");
        IngesteLogger.Info($"  Audio Events: {stats.AudioEvents}");
        IngesteLogger.Info($"  Particle Effects: {stats.ParticleEffects}");
        IngesteLogger.Info($"  UI Themes: {stats.UIThemes}");
        IngesteLogger.Info($"  Total: {stats.Total}");
    }

    public static MetadataStatistics GetMetadataStatistics()
    {
        return new MetadataStatistics
        {
            Areas = AreaMetadataRegistry.Areas.Count,
            AltSides = AltSideMetadataRegistry.AltSides.Count,
            Plugins = PluginMetadataRegistry.Plugins.Count,
            Submaps = SubmapMetadataRegistry.Submaps.Count,
            Cutscenes = CutsceneMetadataRegistry.Cutscenes.Count,
            Models = ModelMetadataRegistry.Models.Count,
            InventoryProfiles = InventoryMetadataRegistry.Profiles.Count,
            AudioEvents = AudioMetadataRegistry.Audio.Count, // <-- FIXED LINE
            ParticleEffects = ParticleMetadataRegistry.Particles.Count,
            UIThemes = UIMetadataRegistry.Themes.Count
        };
    }
}

internal class MetadataStatistics
{
    public int Areas { get; set; }
    public int AltSides { get; set; }
    public int Plugins { get; set; }
    public int Submaps { get; set; }
    public int Cutscenes { get; set; }
    public int Models { get; set; }
    public int InventoryProfiles { get; set; }
    public int AudioEvents { get; set; }
    public int ParticleEffects { get; set; }
    public int UIThemes { get; set; }
    public int Players { get; set; }
    public int Bosses { get; set; }
    public int Shaders { get; set; }
    public int Interludes { get; set; }
    
    public int Total => Areas + AltSides + Plugins + Submaps + Cutscenes + Models + 
                       InventoryProfiles + AudioEvents + ParticleEffects + UIThemes +
                       Players + Bosses + Shaders + Interludes;
}

/// <summary>
/// Facade class for initializing all metadata registries
/// </summary>
public static class MetadataRegistries
{
    private static bool _initialized = false;
    
    /// <summary>
    /// Initialize all metadata registries from the mod's root path
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        
        try
        {
            // Get the mod's content path - use the mod's directory from Everest
            var mod = Everest.Content.Mods
                .FirstOrDefault(m => m.Name == "MaggyHelper");
            
            // Try to get the path from the mod - ModContent doesn't have PathDirectory,
            // so we use an alternative approach to get the mod root
            string modRoot = "";
            if (mod != null)
            {
                // Get path from the mod's first map if available
                var mapPath = mod.Map?.FirstOrDefault().Key;
                if (!string.IsNullOrEmpty(mapPath))
                {
                    modRoot = Path.GetDirectoryName(mapPath) ?? "";
                }
            }
                
            if (string.IsNullOrEmpty(modRoot))
            {
                Logger.Log(LogLevel.Warn, "MaggyHelper", "Could not find mod root path for metadata initialization");
                return;
            }
            
            // Initialize all registries
            AreaMetadataRegistry.Initialize(modRoot);
            AltSideMetadataRegistry.Initialize(modRoot);
            PluginMetadataRegistry.Initialize(modRoot);
            SubmapMetadataRegistry.Initialize(modRoot);
            CutsceneMetadataRegistry.Initialize(modRoot);
            ModelMetadataRegistry.Initialize(modRoot);
            InventoryMetadataRegistry.Initialize(modRoot);
            AudioMetadataRegistry.Initialize(modRoot);
            ParticleMetadataRegistry.Initialize(modRoot);
            UIMetadataRegistry.Initialize(modRoot);
            // Note: PlayerMetadataRegistry is nested in SubmapMetadataRegistry
            // and doesn't have a public Initialize method
            
            _initialized = true;
            Logger.Log(LogLevel.Info, "MaggyHelper", "All metadata registries initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Warn, "MaggyHelper", $"Error initializing metadata registries: {ex.Message}");
        }
    }
}




