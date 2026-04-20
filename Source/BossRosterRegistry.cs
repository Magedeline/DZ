using Celeste.Entities;
using Celeste.Entities.Bosses;
using static Celeste.Entities.Boss;

namespace Celeste;

/// <summary>
/// Centralized catalog of every boss in the mod, mapping each to its chapter,
/// tier, gimmick, and the copy ability it drops when defeated / inhaled.
/// </summary>
public static class BossRosterRegistry
{
    public class BossEntry
    {
        public string Id { get; init; }
        public string DisplayName { get; init; }
        public int ChapterNumber { get; init; }
        public BossTier Tier { get; init; }
        public GimmickAbility Gimmick { get; init; }
        public CopyAbilityType CopyAbility { get; init; }
        public bool IsMiniBoss { get; init; }
        public bool IsSecretBoss { get; init; }

        /// <summary>
        /// DX bosses are secret encounters or Final-tier bosses — the hardest
        /// encounters that unlock the Asriel mastery badge when all are defeated.
        /// </summary>
        public bool IsDXBoss => Tier == BossTier.Final || IsSecretBoss;
    }

    private static readonly List<BossEntry> _roster = new();
    private static readonly Dictionary<string, BossEntry> _byId = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<int, List<BossEntry>> _byChapter = new();

    public static IReadOnlyList<BossEntry> AllBosses => _roster;

    public static void Initialize()
    {
        _roster.Clear();
        _byId.Clear();
        _byChapter.Clear();

        // ═══════════════════════════════════════════════════════════
        //  ACT I — Chapters 1-9
        // ═══════════════════════════════════════════════════════════

        // Ch1 - Forbidden Metropolis
        Add(new BossEntry
        {
            Id = "DragoneerJrBoss", DisplayName = "Dragoneer Jr.",
            ChapterNumber = 1, Tier = BossTier.Lowest,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Fire
        });

        // Ch2 - Veil of Shadows
        Add(new BossEntry
        {
            Id = "CharaBoss", DisplayName = "Chara",
            ChapterNumber = 2, Tier = BossTier.Low,
            Gimmick = GimmickAbility.Teleport, CopyAbility = CopyAbilityType.Sword
        });

        // Ch3 - Arrival
        Add(new BossEntry
        {
            Id = "KrackoBoss", DisplayName = "Kracko",
            ChapterNumber = 3, Tier = BossTier.Low,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Spark
        });

        // Ch4 - Chronicles of Destiny
        Add(new BossEntry
        {
            Id = "WhispyWoodsBoss", DisplayName = "Whispy Woods",
            ChapterNumber = 4, Tier = BossTier.Lowest,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Needle
        });

        // Ch5 - Fractured Memories
        Add(new BossEntry
        {
            Id = "DededeBoss", DisplayName = "King Dedede",
            ChapterNumber = 5, Tier = BossTier.Mid,
            Gimmick = GimmickAbility.ShieldBreaker, CopyAbility = CopyAbilityType.Hammer
        });

        // Ch6 - Fortress of Solitude
        Add(new BossEntry
        {
            Id = "MetaKnightBoss", DisplayName = "Meta Knight",
            ChapterNumber = 6, Tier = BossTier.Mid,
            Gimmick = GimmickAbility.Teleport, CopyAbility = CopyAbilityType.Sword
        });
        Add(new BossEntry
        {
            Id = "BuzzoBoss", DisplayName = "Buzzo",
            ChapterNumber = 6, Tier = BossTier.Low,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Cutter,
            IsMiniBoss = true
        });

        // Ch7 - Infernal Reflections
        Add(new BossEntry
        {
            Id = "DarkMatterMidBoss", DisplayName = "Dark Matter",
            ChapterNumber = 7, Tier = BossTier.Mid,
            Gimmick = GimmickAbility.TimeFreeze, CopyAbility = CopyAbilityType.Beam
        });

        // Ch8 - Revelation's Edge
        Add(new BossEntry
        {
            Id = "DarkerDarkMatterBoss", DisplayName = "Darker Dark Matter",
            ChapterNumber = 8, Tier = BossTier.High,
            Gimmick = GimmickAbility.ElementalFusion, CopyAbility = CopyAbilityType.Mirror
        });

        // Ch9 - Apex of Reality
        Add(new BossEntry
        {
            Id = "SummitCorruptedMountainBoss", DisplayName = "Corrupted Mountain",
            ChapterNumber = 9, Tier = BossTier.High,
            Gimmick = GimmickAbility.GravityControl, CopyAbility = CopyAbilityType.Stone
        });

        // ═══════════════════════════════════════════════════════════
        //  ACT II — Chapters 10-15
        // ═══════════════════════════════════════════════════════════

        // Ch10 - Echoes of the Past
        Add(new BossEntry
        {
            Id = "KingTitanBoss", DisplayName = "King Titan",
            ChapterNumber = 10, Tier = BossTier.Mid,
            Gimmick = GimmickAbility.ShieldBreaker, CopyAbility = CopyAbilityType.Fighter
        });
        Add(new BossEntry
        {
            Id = "FrostyBoss", DisplayName = "Frosty",
            ChapterNumber = 10, Tier = BossTier.Low,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Ice,
            IsMiniBoss = true
        });

        // Ch11 - Frozen Sanctuary
        Add(new BossEntry
        {
            Id = "GigantEdgeBoss", DisplayName = "Gigant Edge",
            ChapterNumber = 11, Tier = BossTier.Mid,
            Gimmick = GimmickAbility.ShieldBreaker, CopyAbility = CopyAbilityType.Sword
        });
        Add(new BossEntry
        {
            Id = "MockbyBoss", DisplayName = "Mockby",
            ChapterNumber = 11, Tier = BossTier.Low,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Ice,
            IsMiniBoss = true
        });

        // Ch12 - Cascading Depths
        Add(new BossEntry
        {
            Id = "TitantisBoss", DisplayName = "Titantis",
            ChapterNumber = 12, Tier = BossTier.High,
            Gimmick = GimmickAbility.ElementalFusion, CopyAbility = CopyAbilityType.Parasol
        });
        Add(new BossEntry
        {
            Id = "EmbryoBoss", DisplayName = "Embryo",
            ChapterNumber = 12, Tier = BossTier.Low,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Beam,
            IsMiniBoss = true
        });

        // Ch13 - Blazing Territories
        Add(new BossEntry
        {
            Id = "AxisTerminatorBoss", DisplayName = "Axis Terminator",
            ChapterNumber = 13, Tier = BossTier.High,
            Gimmick = GimmickAbility.TimeFreeze, CopyAbility = CopyAbilityType.Bomb
        });
        Add(new BossEntry
        {
            Id = "SpamtonNeoDeluxeBoss", DisplayName = "Spamton NEO Deluxe",
            ChapterNumber = 13, Tier = BossTier.Mid,
            Gimmick = GimmickAbility.Teleport, CopyAbility = CopyAbilityType.Spark,
            IsMiniBoss = true
        });

        // Ch14 - Cyber Nexus
        Add(new BossEntry
        {
            Id = "GigaAxisBoss", DisplayName = "Giga Axis",
            ChapterNumber = 14, Tier = BossTier.Highest,
            Gimmick = GimmickAbility.GravityControl, CopyAbility = CopyAbilityType.UFO
        });
        Add(new BossEntry
        {
            Id = "GigaBoltUltra86000Boss", DisplayName = "Giga Bolt Ultra 86000",
            ChapterNumber = 14, Tier = BossTier.High,
            Gimmick = GimmickAbility.ElementalFusion, CopyAbility = CopyAbilityType.Spark,
            IsMiniBoss = true
        });
        Add(new BossEntry
        {
            Id = "RoboboGuardBoss", DisplayName = "Robobo Guard",
            ChapterNumber = 14, Tier = BossTier.Low,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Fighter,
            IsMiniBoss = true
        });

        // Ch15 - Ethereal Citadel
        Add(new BossEntry
        {
            Id = "GalactaKnightBoss", DisplayName = "Galacta Knight",
            ChapterNumber = 15, Tier = BossTier.Highest,
            Gimmick = GimmickAbility.GravityControl, CopyAbility = CopyAbilityType.Wing
        });
        Add(new BossEntry
        {
            Id = "MorphoKnightDeltaBoss", DisplayName = "Morpho Knight Delta",
            ChapterNumber = 15, Tier = BossTier.Highest,
            Gimmick = GimmickAbility.DimensionRift, CopyAbility = CopyAbilityType.Sword
        });
        Add(new BossEntry
        {
            Id = "SansBoss", DisplayName = "Sans",
            ChapterNumber = 15, Tier = BossTier.High,
            Gimmick = GimmickAbility.GravityControl, CopyAbility = CopyAbilityType.Bomb,
            IsSecretBoss = true
        });
        Add(new BossEntry
        {
            Id = "TumbleKevinBoss", DisplayName = "Tumble Kevin",
            ChapterNumber = 15, Tier = BossTier.Low,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Stone,
            IsMiniBoss = true
        });

        // ═══════════════════════════════════════════════════════════
        //  ACT III — Chapters 16-20
        // ═══════════════════════════════════════════════════════════

        // Ch16 - Organ Garden of Despair
        Add(new BossEntry
        {
            Id = "ApexPredatorBoss", DisplayName = "Apex Predator",
            ChapterNumber = 16, Tier = BossTier.Highest,
            Gimmick = GimmickAbility.TimeFreeze, CopyAbility = CopyAbilityType.Ninja
        });
        Add(new BossEntry
        {
            Id = "AlphaApexPredatorBoss", DisplayName = "Alpha Apex Predator",
            ChapterNumber = 16, Tier = BossTier.Highest,
            Gimmick = GimmickAbility.DimensionRift, CopyAbility = CopyAbilityType.Fighter
        });

        // Ch18 - Core of Existence
        Add(new BossEntry
        {
            Id = "AnotherVesselCorruptedBoss", DisplayName = "Corrupted Vessel",
            ChapterNumber = 18, Tier = BossTier.Highest,
            Gimmick = GimmickAbility.ElementalFusion, CopyAbility = CopyAbilityType.Mirror
        });
        Add(new BossEntry
        {
            Id = "AsrielGodBoss", DisplayName = "Asriel (God of Hyperdeath)",
            ChapterNumber = 18, Tier = BossTier.Final,
            Gimmick = GimmickAbility.DimensionRift, CopyAbility = CopyAbilityType.Fire
        });

        // Ch19 - Farewell to Stars
        Add(new BossEntry
        {
            Id = "AsrielAngelOfDeathBoss", DisplayName = "Asriel (Angel of Death)",
            ChapterNumber = 19, Tier = BossTier.Final,
            Gimmick = GimmickAbility.DimensionRift, CopyAbility = CopyAbilityType.Wing
        });
        Add(new BossEntry
        {
            Id = "ElsKnightCloneBoss", DisplayName = "El's Knight Clone",
            ChapterNumber = 19, Tier = BossTier.Highest,
            Gimmick = GimmickAbility.GravityControl, CopyAbility = CopyAbilityType.Sword
        });
        Add(new BossEntry
        {
            Id = "KirbyBoss", DisplayName = "Kirby (Mirror World)",
            ChapterNumber = 19, Tier = BossTier.High,
            Gimmick = GimmickAbility.Teleport, CopyAbility = CopyAbilityType.None,
            IsSecretBoss = true
        });

        // Ch20 - The Last Push
        Add(new BossEntry
        {
            Id = "TesseractBoss", DisplayName = "Tesseract",
            ChapterNumber = 20, Tier = BossTier.Highest,
            Gimmick = GimmickAbility.DimensionRift, CopyAbility = CopyAbilityType.UFO
        });
        Add(new BossEntry
        {
            Id = "SiamoZeroFinalBoss", DisplayName = "Siamo Zero",
            ChapterNumber = 20, Tier = BossTier.Final,
            Gimmick = GimmickAbility.DimensionRift, CopyAbility = CopyAbilityType.None
        });
        Add(new BossEntry
        {
            Id = "BlackholeAngelBoss", DisplayName = "Blackhole Angel",
            ChapterNumber = 20, Tier = BossTier.Final,
            Gimmick = GimmickAbility.DimensionRift, CopyAbility = CopyAbilityType.None
        });
        Add(new BossEntry
        {
            Id = "TennaTVBoss", DisplayName = "Tenna TV",
            ChapterNumber = 20, Tier = BossTier.Mid,
            Gimmick = GimmickAbility.None, CopyAbility = CopyAbilityType.Spark,
            IsMiniBoss = true
        });

        if (_byId.TryGetValue("SiamoZeroFinalBoss", out var siamoZeroFinalBoss))
        {
            _byId["ElsTrueFinalBoss"] = siamoZeroFinalBoss;
        }

        Logger.Log(LogLevel.Info, "MaggyHelper",
            $"BossRosterRegistry: {_roster.Count} bosses registered");
    }

    // ── Queries ──────────────────────────────────────────────────────

    public static BossEntry GetBoss(string id) =>
        _byId.TryGetValue(id, out var entry) ? entry : null;

    public static IReadOnlyList<BossEntry> GetBossesForChapter(int chapter) =>
        _byChapter.TryGetValue(chapter, out var list)
            ? list
            : (IReadOnlyList<BossEntry>)Array.Empty<BossEntry>();

    public static IEnumerable<BossEntry> GetBossesByTier(BossTier tier) =>
        _roster.Where(b => b.Tier == tier);

    public static IEnumerable<BossEntry> GetMainBosses() =>
        _roster.Where(b => !b.IsMiniBoss && !b.IsSecretBoss);

    public static IEnumerable<BossEntry> GetSecretBosses() =>
        _roster.Where(b => b.IsSecretBoss);

    public static int GetBossCount() => _roster.Count;

    // ── Internals ────────────────────────────────────────────────────

    private static void Add(BossEntry entry)
    {
        _roster.Add(entry);
        _byId[entry.Id] = entry;

        if (!_byChapter.TryGetValue(entry.ChapterNumber, out var list))
        {
            list = new List<BossEntry>();
            _byChapter[entry.ChapterNumber] = list;
        }
        list.Add(entry);
    }

}