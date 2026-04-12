namespace MaggyHelper;

/// <summary>
/// Groups chapters into narrative acts (I/II/III) and exposes per-chapter metadata
/// such as theme, boss list, and mechanics introduced.
/// </summary>
public static class ChapterActRegistry
{
    public enum Act
    {
        /// <summary>Chapters 0-9: Madeline's journey through the mountain</summary>
        ActI = 1,
        /// <summary>Chapters 10-15: Undertale crossover, hub worlds, SubMaps</summary>
        ActII = 2,
        /// <summary>Chapters 16-20: Corruption, Epilogue, Final DLC, The End</summary>
        ActIII = 3
    }

    public class ChapterInfo
    {
        public int Number { get; init; }
        public string Name { get; init; }
        public Act Act { get; init; }
        public string Theme { get; init; }
        public string[] Bosses { get; init; } = Array.Empty<string>();
        public string[] MechanicsIntroduced { get; init; } = Array.Empty<string>();
        public bool HasSubmaps { get; init; }
        public bool IsInterlude { get; init; }
    }

    private static readonly List<ChapterInfo> _chapters = new();
    private static readonly Dictionary<int, ChapterInfo> _byNumber = new();

    public static IReadOnlyList<ChapterInfo> AllChapters => _chapters;

    public static void Initialize()
    {
        _chapters.Clear();
        _byNumber.Clear();

        // ═══════════════════════════════════════════════════════════
        //  ACT I — Madeline's Ascent (Chapters 0-9)
        // ═══════════════════════════════════════════════════════════

        Add(new ChapterInfo
        {
            Number = 0, Name = "Prologue", Act = Act.ActI,
            Theme = "Introduction / Tutorial",
            IsInterlude = true,
            MechanicsIntroduced = new[] { "BasicMovement", "Dash", "Climb" }
        });

        Add(new ChapterInfo
        {
            Number = 1, Name = "Forbidden Metropolis", Act = Act.ActI,
            Theme = "Neon City",
            Bosses = new[] { "DragoneerJrBoss" },
            MechanicsIntroduced = new[] { "DreamBlocks", "Keys" }
        });

        Add(new ChapterInfo
        {
            Number = 2, Name = "Veil of Shadows", Act = Act.ActI,
            Theme = "Nightmare / Dark",
            Bosses = new[] { "CharaBoss" },
            MechanicsIntroduced = new[] { "DarkRooms", "Feathers" }
        });

        Add(new ChapterInfo
        {
            Number = 3, Name = "Arrival", Act = Act.ActI,
            Theme = "Starfields / Sky",
            Bosses = new[] { "KrackoBoss" },
            MechanicsIntroduced = new[] { "WindMechanic", "Clouds" }
        });

        Add(new ChapterInfo
        {
            Number = 4, Name = "Chronicles of Destiny", Act = Act.ActI,
            Theme = "Legend / Temple",
            Bosses = new[] { "WhispyWoodsBoss" },
            MechanicsIntroduced = new[] { "MovingBlocks", "CrumblePlatforms" }
        });

        Add(new ChapterInfo
        {
            Number = 5, Name = "Fractured Memories", Act = Act.ActI,
            Theme = "Resort / Memory",
            Bosses = new[] { "DededeBoss" },
            MechanicsIntroduced = new[] { "Seekers", "MirrorBlocks" }
        });

        Add(new ChapterInfo
        {
            Number = 6, Name = "Fortress of Solitude", Act = Act.ActI,
            Theme = "Stronghold / Fortress",
            Bosses = new[] { "MetaKnightBoss", "BuzzoBoss" },
            MechanicsIntroduced = new[] { "Bumpers", "Crushers" }
        });

        Add(new ChapterInfo
        {
            Number = 7, Name = "Infernal Reflections", Act = Act.ActI,
            Theme = "Hell / Lava",
            Bosses = new[] { "DarkMatterMidBoss" },
            MechanicsIntroduced = new[] { "LavaBlocks", "IceWalls" }
        });

        Add(new ChapterInfo
        {
            Number = 8, Name = "Revelation's Edge", Act = Act.ActI,
            Theme = "Truth / Revelation",
            Bosses = new[] { "CharaBoss", "DarkerDarkMatterBoss" },
            MechanicsIntroduced = new[] { "FlyingBlocks", "SoulBoosts" }
        });

        Add(new ChapterInfo
        {
            Number = 9, Name = "Apex of Reality", Act = Act.ActI,
            Theme = "Summit",
            Bosses = new[] { "SummitCorruptedMountainBoss" },
            MechanicsIntroduced = new[] { "FlagSystem", "MultiDash" }
        });

        // ═══════════════════════════════════════════════════════════
        //  ACT II — Undertale Crossover & Hub Worlds (Chapters 10-15)
        // ═══════════════════════════════════════════════════════════

        Add(new ChapterInfo
        {
            Number = 10, Name = "Echoes of the Past", Act = Act.ActII,
            Theme = "Ancient Ruins",
            HasSubmaps = true,
            Bosses = new[] { "KingTitanBoss", "FrostyBoss" },
            MechanicsIntroduced = new[] { "SubMaps", "KirbyMode", "CopyAbilities" }
        });

        Add(new ChapterInfo
        {
            Number = 11, Name = "Frozen Sanctuary", Act = Act.ActII,
            Theme = "Snow / Ice",
            HasSubmaps = true,
            Bosses = new[] { "GigantEdgeBoss", "MockbyBoss" },
            MechanicsIntroduced = new[] { "IcePhysics", "ElementalFusion" }
        });

        Add(new ChapterInfo
        {
            Number = 12, Name = "Cascading Depths", Act = Act.ActII,
            Theme = "Waterfall / Caverns",
            HasSubmaps = true,
            Bosses = new[] { "TitantisBoss", "EmbryoBoss" },
            MechanicsIntroduced = new[] { "WaterCurrents", "SoulTraits" }
        });

        Add(new ChapterInfo
        {
            Number = 13, Name = "Blazing Territories", Act = Act.ActII,
            Theme = "Fire / Hotland",
            HasSubmaps = true,
            Bosses = new[] { "AxisTerminatorBoss", "SpamtonNeoDeluxeBoss" },
            MechanicsIntroduced = new[] { "HeatMechanic", "WarperDash" }
        });

        Add(new ChapterInfo
        {
            Number = 14, Name = "Cyber Nexus", Act = Act.ActII,
            Theme = "Digital / Cyberspace",
            HasSubmaps = true,
            Bosses = new[] { "GigaAxisBoss", "GigaBoltUltra86000Boss" },
            MechanicsIntroduced = new[] { "DigitalGlitch", "RoboboGuards" }
        });

        Add(new ChapterInfo
        {
            Number = 15, Name = "Ethereal Citadel", Act = Act.ActII,
            Theme = "Castle / Throne",
            Bosses = new[] { "GalactaKnightBoss", "MorphoKnightDeltaBoss", "SansBoss" },
            MechanicsIntroduced = new[] { "KnightForm", "BossRush" }
        });

        // ═══════════════════════════════════════════════════════════
        //  ACT III — Corruption & The End (Chapters 16-20)
        // ═══════════════════════════════════════════════════════════

        Add(new ChapterInfo
        {
            Number = 16, Name = "Organ Garden of Despair", Act = Act.ActIII,
            Theme = "Corruption / Organ Garden",
            Bosses = new[] { "ApexPredatorBoss", "AlphaApexPredatorBoss" },
            MechanicsIntroduced = new[] { "CorruptionZones", "AstralTrail" }
        });

        Add(new ChapterInfo
        {
            Number = 17, Name = "Epilogue", Act = Act.ActIII,
            Theme = "Reflection / Calm",
            IsInterlude = true,
            MechanicsIntroduced = new[] { "NarrativeWalk" }
        });

        Add(new ChapterInfo
        {
            Number = 18, Name = "Core of Existence", Act = Act.ActIII,
            Theme = "Heart / Core",
            Bosses = new[] { "AnotherVesselCorruptedBoss", "AsrielGodBoss" },
            MechanicsIntroduced = new[] { "HeartMechanic", "GravityFlip" }
        });

        Add(new ChapterInfo
        {
            Number = 19, Name = "Farewell to Stars", Act = Act.ActIII,
            Theme = "Space / Farewell",
            Bosses = new[] { "AsrielAngelOfDeathBoss", "ElsKnightCloneBoss" },
            MechanicsIntroduced = new[] { "JellyBoost", "FinalDash" }
        });

        Add(new ChapterInfo
        {
            Number = 20, Name = "The Last Push", Act = Act.ActIII,
            Theme = "The End / Void",
            Bosses = new[] { "TesseractBoss", "SiamoZeroFinalBoss", "BlackholeAngelBoss" },
            MechanicsIntroduced = new[] { "VoidMoon", "DimensionRift" }
        });

        Logger.Log(LogLevel.Info, "MaggyHelper",
            $"ChapterActRegistry: {_chapters.Count} chapters across 3 acts");
    }

    // ── Queries ──────────────────────────────────────────────────────

    public static ChapterInfo GetChapter(int number) =>
        _byNumber.TryGetValue(number, out var info) ? info : null;

    public static Act GetAct(int chapterNumber) =>
        GetChapter(chapterNumber)?.Act ?? Act.ActI;

    public static IEnumerable<ChapterInfo> GetChaptersInAct(Act act) =>
        _chapters.Where(c => c.Act == act);

    public static (int first, int last) GetActRange(Act act) => act switch
    {
        Act.ActI => (0, 9),
        Act.ActII => (10, 15),
        Act.ActIII => (16, 20),
        _ => (0, 20)
    };

    public static bool HasSubmaps(int chapterNumber) =>
        GetChapter(chapterNumber)?.HasSubmaps ?? false;

    public static string[] GetBosses(int chapterNumber) =>
        GetChapter(chapterNumber)?.Bosses ?? Array.Empty<string>();

    // ── Internals ────────────────────────────────────────────────────

    private static void Add(ChapterInfo info)
    {
        _chapters.Add(info);
        _byNumber[info.Number] = info;
    }
}