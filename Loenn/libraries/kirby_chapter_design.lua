-- Kirby Chapter Design Data
-- ============================================================================
-- Per-chapter mapping of signature abilities, enemies, bosses, and level
-- design patterns.  Follows Sakurai's design philosophy from his
-- "Game Concepts" video series:
--
--   1. Each chapter introduces ONE new signature ability
--   2. Enemies in the chapter teach the ability through gameplay
--   3. Practice rooms let the player experiment safely
--   4. Test rooms challenge mastery of the ability
--   5. Boss fights require the chapter's signature ability (or clever use of it)
--
-- Used by:
--   - Loenn/scripts/generate_kirby_chapter.lua   (PCG level generator)
--   - Loenn/entities/kirby_chapter_enemy.lua      (auto-configured enemies)
--   - Loenn/libraries/kirby_data.lua              (shared data reference)
-- ============================================================================

local chapterDesign = {}

-- ────────────────────────────────────────────────────────────────────────────
-- Chapter Definitions
-- ────────────────────────────────────────────────────────────────────────────
-- Each entry follows this schema:
--   number          : Chapter index (0-20)
--   name            : Display name
--   theme           : Visual/environmental theme
--   signatureAbility: The ONE ability this chapter teaches
--   secondaryAbility: Optional secondary ability available in the chapter
--   enemies         : Table of enemy types that appear, with their copy powers
--   midBoss         : Mid-boss entity data (optional)
--   boss            : End-of-chapter boss entity data
--   tileset         : Default solid tileset character for PCG rooms
--   bgStyle         : Background style for the chapter
--   designNotes     : Brief description of the Sakurai-style design flow
-- ────────────────────────────────────────────────────────────────────────────

chapterDesign.chapters = {
    -- ================================================================
    -- Chapter 0: Prologue — No abilities, pure Celeste platforming
    -- ================================================================
    [0] = {
        number = 0,
        name = "Prologue",
        theme = "tutorial",
        signatureAbility = "None",
        secondaryAbility = nil,
        enemies = {
            { type = "WaddleDee", power = "None", health = 1, speed = 20, role = "tutorial" },
        },
        midBoss = nil,
        boss = nil,
        tileset = "1",   -- basic dirt/grass
        bgStyle = "Normal",
        designNotes = "Pure introduction. Teach movement, dash, and Kirby float. "
            .. "Single WaddleDee appears as a friendly NPC demonstration, not a threat."
    },

    -- ================================================================
    -- Chapter 1: Forbidden Metropolis — FIRE
    -- Sakurai pattern: Hot Head enemies teach Fire copy ability
    -- ================================================================
    [1] = {
        number = 1,
        name = "Forbidden Metropolis",
        theme = "city",
        signatureAbility = "Fire",
        secondaryAbility = "Beam",
        enemies = {
            { type = "WaddleDee",   power = "None",  health = 1, speed = 25, role = "fodder" },
            { type = "HotHead",     power = "Fire",  health = 1, speed = 25, role = "signature" },
            { type = "WaddleDoo",   power = "Beam",  health = 2, speed = 20, role = "secondary" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Bonkers", health = 30, power = "Hammer" },
        boss = { type = "MaggyHelper/BossTier1", bossType = "BasicEnemy", health = 100, speed = 20 },
        tileset = "3",   -- stone/city
        bgStyle = "Normal",
        designNotes = "PRACTICE: First room has a HotHead on flat ground — safe to inhale. "
            .. "TEST: Later rooms require Fire to melt ice blocks or light torches to open doors. "
            .. "BOSS: Tier 1 boss weak to Fire projectiles."
    },

    -- ================================================================
    -- Chapter 2: Veil of Shadows — ICE
    -- ================================================================
    [2] = {
        number = 2,
        name = "Veil of Shadows",
        theme = "nightmare",
        signatureAbility = "Ice",
        secondaryAbility = "Spark",
        enemies = {
            { type = "WaddleDee",   power = "None",  health = 1, speed = 25, role = "fodder" },
            { type = "Chilly",      power = "Ice",   health = 1, speed = 15, role = "signature" },
            { type = "Sparky",      power = "Spark", health = 1, speed = 20, role = "secondary" },
            { type = "Gordo",       power = "None",  health = 99, speed = 0, role = "hazard" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "MrFrosty", health = 40, power = "Ice" },
        boss = { type = "MaggyHelper/BossTier1", bossType = "BasicEnemy", health = 120, speed = 25 },
        tileset = "5",   -- dark stone
        bgStyle = "Dreamy",
        designNotes = "PRACTICE: Chilly walks slowly — easy first inhale for Ice. "
            .. "TEST: Use Ice to freeze lava pits and create temporary platforms. "
            .. "Gordos teach dodge-only hazards (can't inhale). "
            .. "BOSS: Weak to Ice — freeze + shatter pattern."
    },

    -- ================================================================
    -- Chapter 3: Arrival — SWORD
    -- ================================================================
    [3] = {
        number = 3,
        name = "Arrival",
        theme = "star",
        signatureAbility = "Sword",
        secondaryAbility = "Cutter",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 25, role = "fodder" },
            { type = "BladeKnight", power = "Sword",   health = 2, speed = 35, role = "signature" },
            { type = "SirKibble",   power = "Cutter",  health = 2, speed = 25, role = "secondary" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "BugzzyCrab", health = 50, power = "Sword" },
        boss = { type = "MaggyHelper/BossTier2", bossType = "ElementalGuardian", health = 200, speed = 30 },
        tileset = "6",
        bgStyle = "Normal",
        designNotes = "PRACTICE: Blade Knight patrols — visible attack pattern before you engage. "
            .. "TEST: Sword can cut ropes holding platforms, slash through vines. "
            .. "BOSS: Tier 2 — requires close range combat mastery."
    },

    -- ================================================================
    -- Chapter 4: Chronicles of Destiny — BEAM
    -- ================================================================
    [4] = {
        number = 4,
        name = "Chronicles of Destiny",
        theme = "legend",
        signatureAbility = "Beam",
        secondaryAbility = "Esp",
        enemies = {
            { type = "WaddleDee",   power = "None",  health = 1, speed = 25, role = "fodder" },
            { type = "WaddleDoo",   power = "Beam",  health = 2, speed = 20, role = "signature" },
            { type = "Scarfy",      power = "None",  health = 2, speed = 15, role = "hazard" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "MasterHand", health = 50, power = "Beam" },
        boss = { type = "MaggyHelper/BossTier2", bossType = "ElementalGuardian", health = 220, speed = 35 },
        tileset = "4",
        bgStyle = "Normal",
        designNotes = "PRACTICE: Waddle Doo's beam has visible telegraph. "
            .. "TEST: Beam can hit switches through walls — puzzle rooms. "
            .. "Scarfys teach 'not everything is inhalable'. "
            .. "BOSS: Attacks from distance — Beam is the ideal counter."
    },

    -- ================================================================
    -- Chapter 5: Fractured Memories — STONE
    -- ================================================================
    [5] = {
        number = 5,
        name = "Fractured Memories",
        theme = "resort",
        signatureAbility = "Stone",
        secondaryAbility = "Hammer",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 25, role = "fodder" },
            { type = "Rocky",       power = "Stone",   health = 2, speed = 15, role = "signature" },
            { type = "BombWaddleDee", power = "Bomb",  health = 1, speed = 25, role = "secondary" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Bonkers", health = 60, power = "Hammer" },
        boss = { type = "MaggyHelper/BossTier2", bossType = "ElementalGuardian", health = 240, speed = 30 },
        tileset = "9",
        bgStyle = "Normal",
        designNotes = "PRACTICE: Rocky drops from above — teaches you to look up. "
            .. "TEST: Stone form blocks wind currents and crushes weight switches. "
            .. "BOSS: Uses ground pound attacks — Stone makes you invincible during stomps."
    },

    -- ================================================================
    -- Chapter 6: Fortress of Solitude — HAMMER
    -- ================================================================
    [6] = {
        number = 6,
        name = "Fortress of Solitude",
        theme = "stronghold",
        signatureAbility = "Hammer",
        secondaryAbility = "Stone",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 30, role = "fodder" },
            { type = "Bonkers",     power = "Hammer",  health = 3, speed = 20, role = "signature" },
            { type = "Rocky",       power = "Stone",   health = 2, speed = 15, role = "secondary" },
            { type = "ShieldEnemy", power = "None",    health = 2, speed = 30, role = "hazard" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Bonkers", health = 80, power = "Hammer" },
        boss = { type = "MaggyHelper/BossTier3", bossType = "ShadowWarrior", health = 350, speed = 45 },
        tileset = "7",
        bgStyle = "Normal",
        designNotes = "PRACTICE: Bonkers is a mini-boss enemy — beating him gives Hammer. "
            .. "TEST: Hammer smashes stakes, breaks reinforced blocks. "
            .. "Shield enemies teach 'attack from behind' — Hammer's wide arc helps. "
            .. "BOSS: Tier 3 Shadow Warrior — needs heavy damage."
    },

    -- ================================================================
    -- Chapter 7: Infernal Reflections — MIRROR
    -- ================================================================
    [7] = {
        number = 7,
        name = "Infernal Reflections",
        theme = "hell",
        signatureAbility = "Mirror",
        secondaryAbility = "Fire",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 30, role = "fodder" },
            { type = "MirrorEnemy", power = "Mirror",  health = 2, speed = 25, role = "signature" },
            { type = "HotHead",     power = "Fire",    health = 1, speed = 25, role = "secondary" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "DarkMirror", health = 80, power = "Mirror" },
        boss = { type = "MaggyHelper/BossTier3", bossType = "ShadowWarrior", health = 380, speed = 50 },
        tileset = "8",
        bgStyle = "Normal",
        designNotes = "PRACTICE: Mirror enemy reflects projectiles — teaches defensive play. "
            .. "TEST: Mirror reflects boss projectiles back. Puzzle rooms with beam redirects. "
            .. "BOSS: Has projectile attacks — Mirror is the counter."
    },

    -- ================================================================
    -- Chapter 8: Revelation's Edge — ARCHER
    -- ================================================================
    [8] = {
        number = 8,
        name = "Revelation's Edge",
        theme = "truth",
        signatureAbility = "Archer",
        secondaryAbility = "Ranger",
        enemies = {
            { type = "WaddleDee",   power = "None",    health = 1, speed = 30, role = "fodder" },
            { type = "Scarfy",      power = "None",    health = 2, speed = 15, role = "hazard" },
            { type = "WaddleDoo",   power = "Beam",    health = 2, speed = 20, role = "secondary" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Bonkers", health = 80, power = "Archer" },
        boss = { type = "MaggyHelper/BossTier3", bossType = "ShadowWarrior", health = 400, speed = 50 },
        tileset = "3",
        bgStyle = "Normal",
        designNotes = "PRACTICE: AbilityStar grants Archer in safe room. "
            .. "TEST: Charged arrows hit distant switches, snipe flying enemies. "
            .. "Long vertical rooms reward precision aim. "
            .. "BOSS: Stays at distance — Archer's charge shot is key."
    },

    -- ================================================================
    -- Chapter 9: Apex of Reality (Summit) — WHEEL
    -- ================================================================
    [9] = {
        number = 9,
        name = "Apex of Reality",
        theme = "summit",
        signatureAbility = "Wheel",
        secondaryAbility = "Wing",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 35, role = "fodder" },
            { type = "Gordo",       power = "None",    health = 99, speed = 0, role = "hazard" },
            { type = "Sparky",      power = "Spark",   health = 1, speed = 20, role = "secondary" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Wheelie", health = 80, power = "Wheel" },
        boss = { type = "MaggyHelper/BossTier4", bossType = "CrystalLord", health = 500, speed = 60 },
        tileset = "N",   -- hopes and dreams
        bgStyle = "Rainbow",
        designNotes = "PRACTICE: Long flat corridor — perfect for Wheel speed runs. "
            .. "TEST: Timed races to reach platforms before they vanish. "
            .. "Summit-style climbing interspersed with speed sections. "
            .. "BOSS: Tier 4 — multiple phases test all learned abilities."
    },

    -- ================================================================
    -- Chapter 10: Echoes of the Past — LEAF
    -- ================================================================
    [10] = {
        number = 10,
        name = "Echoes of the Past",
        theme = "ruins",
        signatureAbility = "Leaf",
        secondaryAbility = "Water",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 25, role = "fodder" },
            { type = "Scarfy",      power = "None",    health = 2, speed = 15, role = "hazard" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Bonkers", health = 90, power = "Leaf" },
        boss = { type = "MaggyHelper/BossTier3", bossType = "ShadowWarrior", health = 400, speed = 45 },
        tileset = "4",
        bgStyle = "Normal",
        designNotes = "PRACTICE: Leaf hides Kirby — demonstrates stealth. "
            .. "TEST: Sneak past Scarfys (who chase if spotted). "
            .. "Ruins exploration rewards patient, hidden-path gameplay."
    },

    -- ================================================================
    -- Chapter 11: Frozen Sanctuary — WATER
    -- ================================================================
    [11] = {
        number = 11,
        name = "Frozen Sanctuary",
        theme = "snow",
        signatureAbility = "Water",
        secondaryAbility = "Ice",
        enemies = {
            { type = "WaddleDee",   power = "None",  health = 1, speed = 20, role = "fodder" },
            { type = "Chilly",      power = "Ice",   health = 1, speed = 15, role = "secondary" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "MrFrosty", health = 100, power = "Ice" },
        boss = { type = "MaggyHelper/BossTier4", bossType = "CrystalLord", health = 550, speed = 55 },
        tileset = "b",
        bgStyle = "Normal",
        designNotes = "PRACTICE: AbilityStar grants Water — stream pushes objects. "
            .. "TEST: Water extinguishes fire hazards, moves floating platforms. "
            .. "Snow environment + Water create unique freeze/flow puzzles. "
            .. "BOSS: Fire-themed boss — Water is the counter."
    },

    -- ================================================================
    -- Chapter 12: Cascading Depths — BOMB
    -- ================================================================
    [12] = {
        number = 12,
        name = "Cascading Depths",
        theme = "water",
        signatureAbility = "Bomb",
        secondaryAbility = "Drill",
        enemies = {
            { type = "WaddleDee",     power = "None",  health = 1, speed = 25, role = "fodder" },
            { type = "BombWaddleDee", power = "Bomb",  health = 1, speed = 25, role = "signature" },
            { type = "Gordo",         power = "None",  health = 99, speed = 0, role = "hazard" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Poppy", health = 100, power = "Bomb" },
        boss = { type = "MaggyHelper/BossTier4", bossType = "CrystalLord", health = 600, speed = 55 },
        tileset = "b",
        bgStyle = "Normal",
        designNotes = "PRACTICE: Bomb Waddle Dee tosses bombs — easy to inhale mid-throw. "
            .. "TEST: Bomb breaks cracked underwater walls, opens secret paths. "
            .. "Water sections + Bomb = depth-charge mechanics. "
            .. "BOSS: Armored boss — Bomb pierces armor."
    },

    -- ================================================================
    -- Chapter 13: Blazing Territories — SPARK
    -- ================================================================
    [13] = {
        number = 13,
        name = "Blazing Territories",
        theme = "fire",
        signatureAbility = "Spark",
        secondaryAbility = "Fire",
        enemies = {
            { type = "WaddleDee",   power = "None",  health = 1, speed = 30, role = "fodder" },
            { type = "Sparky",      power = "Spark", health = 1, speed = 20, role = "signature" },
            { type = "HotHead",     power = "Fire",  health = 1, speed = 25, role = "secondary" },
            { type = "ElectricEnemy", power = "Spark", health = 2, speed = 30, role = "elite" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Sparky", health = 100, power = "Spark" },
        boss = { type = "MaggyHelper/BossTier4", bossType = "CrystalLord", health = 650, speed = 65 },
        tileset = "8",
        bgStyle = "Normal",
        designNotes = "PRACTICE: Sparky's barrier is visible — teaches dodge-then-inhale. "
            .. "TEST: Spark powers conductors that open gates and activate platforms. "
            .. "Fire environment + Spark creates electric-lava puzzles. "
            .. "BOSS: Water-type boss — Spark is super effective."
    },

    -- ================================================================
    -- Chapter 14: Cyber Nexus — RANGER
    -- ================================================================
    [14] = {
        number = 14,
        name = "Cyber Nexus",
        theme = "digital",
        signatureAbility = "Ranger",
        secondaryAbility = "Beam",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 30, role = "fodder" },
            { type = "Sparky",      power = "Spark",   health = 1, speed = 20, role = "secondary" },
            { type = "Gordo",       power = "None",    health = 99, speed = 0, role = "hazard" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Mecha", health = 120, power = "Ranger" },
        boss = { type = "MaggyHelper/BossTier5", bossType = "VoidKnight", health = 750, speed = 80 },
        tileset = "3",
        bgStyle = "Dreamy",
        designNotes = "PRACTICE: AbilityStar + target range room (shoot targets). "
            .. "TEST: Ranger rapid-fire destroys digital barriers on timers. "
            .. "Cyber theme = neon platforms, hacking puzzles. "
            .. "BOSS: Tier 5 Void Knight — long-range duel."
    },

    -- ================================================================
    -- Chapter 15: Ethereal Citadel — PAINTER
    -- ================================================================
    [15] = {
        number = 15,
        name = "Ethereal Citadel",
        theme = "castle",
        signatureAbility = "Painter",
        secondaryAbility = "Mirror",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 30, role = "fodder" },
            { type = "ShieldEnemy", power = "None",    health = 2, speed = 30, role = "hazard" },
            { type = "MirrorEnemy", power = "Mirror",  health = 2, speed = 25, role = "secondary" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "Paintra", health = 120, power = "Painter" },
        boss = { type = "MaggyHelper/BossTier5", bossType = "VoidKnight", health = 800, speed = 75 },
        tileset = "7",
        bgStyle = "Normal",
        designNotes = "PRACTICE: Painter creates platforms from thin air. "
            .. "TEST: Paint specific colors on canvases to unlock doors. "
            .. "Castle setting with art gallery puzzle rooms. "
            .. "BOSS: Shield-heavy — Painter's area attack bypasses shields."
    },

    -- ================================================================
    -- Chapter 16: Organ Garden of Despair — All abilities available
    -- ================================================================
    [16] = {
        number = 16,
        name = "Organ Garden of Despair",
        theme = "corruption",
        signatureAbility = "Bomb",
        secondaryAbility = "Sword",
        enemies = {
            { type = "WaddleDee",     power = "None",   health = 1, speed = 35, role = "fodder" },
            { type = "BombWaddleDee", power = "Bomb",   health = 1, speed = 30, role = "signature" },
            { type = "BladeKnight",   power = "Sword",  health = 2, speed = 35, role = "secondary" },
            { type = "Gordo",         power = "None",   health = 99, speed = 0, role = "hazard" },
        },
        midBoss = nil,
        boss = { type = "MaggyHelper/ElsFloweyBoss", health = 1000 },
        tileset = "O",
        bgStyle = "Night",
        designNotes = "CULMINATION: All previously learned abilities available via stars. "
            .. "Organ Garden requires switching between abilities. "
            .. "BOSS: Els Flowey — 6-phase boss tests everything learned."
    },

    -- ================================================================
    -- Chapter 17: Epilogue — Interlude, no combat
    -- ================================================================
    [17] = {
        number = 17,
        name = "Epilogue",
        theme = "post_epilogue",
        signatureAbility = "None",
        secondaryAbility = nil,
        enemies = {},
        midBoss = nil,
        boss = nil,
        tileset = "1",
        bgStyle = "Sunset",
        designNotes = "Story interlude — no enemies, no abilities. Peaceful exploration."
    },

    -- ================================================================
    -- Chapter 18: Core of Existence — ESP
    -- ================================================================
    [18] = {
        number = 18,
        name = "Core of Existence",
        theme = "heart",
        signatureAbility = "Esp",
        secondaryAbility = "Phase",
        enemies = {
            { type = "WaddleDee",   power = "None",  health = 1, speed = 35, role = "fodder" },
            { type = "GhostEnemy",  power = "Esp",   health = 2, speed = 20, role = "signature" },
            { type = "Gordo",       power = "None",  health = 99, speed = 0, role = "hazard" },
        },
        midBoss = { type = "MaggyHelper/KirbyMidBoss", variant = "PhantomKnight", health = 150, power = "Esp" },
        boss = { type = "MaggyHelper/BossTier5", bossType = "VoidKnight", health = 900, speed = 80 },
        tileset = "O",
        bgStyle = "Rainbow",
        designNotes = "PRACTICE: ESP lets Kirby move objects with mind power. "
            .. "TEST: Telekinesis puzzles — move blocks through walls. "
            .. "Phase ability lets Kirby pass through dream blocks. "
            .. "BOSS: Void Knight with psychic attacks — ESP counters."
    },

    -- ================================================================
    -- Chapter 19: Farewell to Stars — KNIGHT (Ultimate)
    -- ================================================================
    --
    -- NARRATIVE BEATS (mapped to Dialog/English.txt keys):
    --
    --  1. CH19_GRAVESTONE — Kirby finds Madeline & Badeline's gravestones.
    --     Collapses in grief. Chara arrives, then all allies (Undyne, Toriel,
    --     Theo, Asgore, Starsi, Ralsei, Sans, Papyrus, Magolor, Alphys,
    --     Noelle, Suzy, Berdly). Ends: seven birds land on the grave.
    --
    --  2. CH19_GRAVESTONE_SEVEN_BIRDS — Extended version with seven birds
    --     scene. Birds carry divine essence, glow, then fly upward forming
    --     a portal of light. Seven souls = seven pure souls prophecy.
    --
    --  3. CH19_SEVEN_BIRDS_REACTION — Everyone reacts: "a second chance!"
    --     Chara joins Kirby. "WE'RE COMING TO SAVE YOU!"
    --
    --  4. CH19_BEYOND_THE_VOID — Traveling into the void. Sans, Papyrus,
    --     Theo, Toriel, Undyne, Asgore express fear. Kirby: "just focus
    --     and keep moving!"
    --
    --  5. CH19_SPACE_INTRO — Deep space. Allies want to turn back. Chara
    --     doubts. Kirby: "Then go! I'll push forward alone!" Chara
    --     vanishes. Kirby goes solo.
    --
    --  6. CS19_HUB_SECOND_INTRO — Massive chamber with sealed passages.
    --     Dozens of keys to collect. Gameplay hub.
    --
    --  7. CH19_WRONG_HEART — Fake heart gem discovered. "Something's off...
    --     This isn't where it ends."
    --
    --  8. CH19_KEEP_GOING_KIRBY — "They're waiting for me out there."
    --     Determination pushes Kirby forward.
    --
    --  9. CH19_MISS_THE_BIRD — Bird won't stop. Kirby questions reality.
    --     Discovers the bird is working WITH Els! Els breaks free.
    --     Grand Sunset / Black Hole Zero 3 flickers. Els: "universal
    --     reset is imminent. Savor the spectacle of annihilation."
    --
    -- 10. CH19_HELPING_HAND — Chara returns, reveals truth: "This is a
    --     modified game, Kirby." Kirby furious: "You're completely
    --     useless! Then go!" Chara leaves.
    --
    -- 11. CH19_KILL_THE_BIRD — Kirby catches the bird, almost kills it.
    --     Els taunts: "You're transforming into one of us. Like Zero
    --     himself." Bird squawks, flies away. Chara returns. Emotional
    --     scene — Chara shares Asriel story. "They're gone forever..."
    --     Kirby accepts loss, becomes the Broken Star Warrior.
    --     But refuses to give up — chooses to help the bird.
    --     "Meta Knight's teachings echo in my mind."
    --
    -- 12. CH19_TRAP_IN_LOOP — Temporal loop trap! Magolor arrives with
    --     power to break the cycle. Spots the Els Power Generator.
    --     "Destroying everything in sight should break you free."
    --     Dream Friends + Magolor reinforce.
    --
    -- 13. CH19_LAST_ROOM — "We've reached the threshold! Almost there."
    --     CH19_LAST_ROOM_ALT — Retry encouragement from Chara.
    --     CH19_LAST_ROOM_ALT_2 — Retry encouragement from Sans.
    --
    -- 14. CH19_CHARA_LAST_BOOST — "Bring them home Kirby!!!" → Ch20.
    --
    [19] = {
        number = 19,
        name = "Farewell to Stars",
        theme = "space",
        signatureAbility = "Knight",
        secondaryAbility = "UltraSword",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 40, role = "fodder" },
            { type = "BladeKnight", power = "Sword",   health = 3, speed = 40, role = "secondary" },
            { type = "ShieldEnemy", power = "None",    health = 3, speed = 35, role = "hazard" },
            { type = "Gordo",       power = "None",    health = 99, speed = 0, role = "hazard" },
        },
        midBoss = nil,
        boss = { type = "MaggyHelper/AsrielGodBoss", health = 1000 },
        tileset = "N",
        bgStyle = "Rainbow",
        narrativeBeats = {
            -- Dialog Key                     -- Scene
            "CH19_GRAVESTONE",                -- Grief at the gravestones; all allies arrive
            "CH19_GRAVESTONE_SEVEN_BIRDS",    -- Seven birds land, glow, fly up → portal of light
            "CH19_SEVEN_BIRDS_REACTION",      -- Hope rekindled; 'WE'RE COMING TO SAVE YOU!'
            "CH19_BEYOND_THE_VOID",           -- Traveling into the void; allies afraid
            "CH19_SPACE_INTRO",               -- Deep space; allies turn back; Kirby goes alone
            "CS19_HUB_SECOND_INTRO",          -- Massive hub chamber; collect keys
            "CH19_WRONG_HEART",               -- Fake heart gem — 'This isn't where it ends'
            "CH19_KEEP_GOING_KIRBY",          -- Determination; 'They're waiting for me'
            "CH19_MISS_THE_BIRD",             -- Bird works with Els! Els breaks free; Zero 3 flickers
            "CH19_HELPING_HAND",              -- 'This is a modified game'; Kirby furious; Chara leaves
            "CH19_KILL_THE_BIRD",             -- Almost kills bird; Broken Star Warrior; chooses to help
            "CH19_TRAP_IN_LOOP",              -- Temporal loop; Magolor arrives; Els Power Generators
            "CH19_LAST_ROOM",                 -- Final chambers (+ ALT, ALT_2 retry variants)
            "CH19_CHARA_LAST_BOOST",          -- 'Bring them home Kirby!!!' → Ch20
        },
        designNotes = "CLIMAX: Knight mode unlocks — 2x damage, special attacks. "
            .. "Narrative-heavy chapter with 14 dialog scenes alternating grief, rage, "
            .. "revelation, and defiance. Kirby becomes the Broken Star Warrior "
            .. "(CH19_KILL_THE_BIRD). Loop-breaking mechanic via Els Power Generators "
            .. "(CH19_TRAP_IN_LOOP). Dream Friends + Magolor reinforce for final push. "
            .. "BOSS: Asriel God Boss — epic multi-phase final battle. "
            .. "Ends with Chara's last boost into Ch20 (CH19_CHARA_LAST_BOOST)."
    },

    -- ================================================================
    -- Chapter 20: The Last Push — KNIGHT + all abilities
    -- ================================================================
    --
    -- Follows directly from Ch19's Chara boost. Els stands revealed in
    -- the darkness ahead. Everything learned across 20 chapters converges.
    -- The Broken Star Warrior faces Asriel Angel of Death — 11 phases.
    --
    [20] = {
        number = 20,
        name = "The Last Push",
        theme = "the_end",
        signatureAbility = "Knight",
        secondaryAbility = "InfernoLight",
        enemies = {
            { type = "WaddleDee",   power = "None",   health = 1, speed = 40, role = "fodder" },
            { type = "BladeKnight", power = "Sword",   health = 3, speed = 40, role = "secondary" },
            { type = "Gordo",       power = "None",    health = 99, speed = 0, role = "hazard" },
        },
        midBoss = nil,
        boss = { type = "MaggyHelper/AsrielAngelOfDeathBoss", health = 2500 },
        tileset = "O",
        bgStyle = "Night",
        designNotes = "FINALE: Continues from Chara's last boost at Ch19 end. "
            .. "Els revealed in darkness — the Broken Star Warrior faces the end. "
            .. "Knight mode throughout. All enemy types + super abilities. "
            .. "BOSS: Asriel Angel of Death — 11-phase ultimate boss. "
            .. "Tests mastery of every ability learned across all 20 chapters."
    },
}

-- ────────────────────────────────────────────────────────────────────────────
-- Helper Functions
-- ────────────────────────────────────────────────────────────────────────────

--- Get chapter data by number.
-- @param chapterNum  Chapter number (0-20)
-- @return table or nil
function chapterDesign.getChapter(chapterNum)
    return chapterDesign.chapters[chapterNum]
end

--- Get the signature ability for a chapter.
-- @param chapterNum  Chapter number
-- @return string  Ability name (e.g. "Fire", "Ice")
function chapterDesign.getSignatureAbility(chapterNum)
    local ch = chapterDesign.chapters[chapterNum]
    return ch and ch.signatureAbility or "None"
end

--- Get all enemies for a chapter, optionally filtered by role.
-- @param chapterNum  Chapter number
-- @param role        Optional role filter ("signature", "fodder", "secondary", "hazard", "elite")
-- @return table  Array of enemy definitions
function chapterDesign.getEnemies(chapterNum, role)
    local ch = chapterDesign.chapters[chapterNum]
    if not ch then return {} end
    if not role then return ch.enemies end

    local filtered = {}
    for _, enemy in ipairs(ch.enemies) do
        if enemy.role == role then
            table.insert(filtered, enemy)
        end
    end
    return filtered
end

--- Get the boss data for a chapter.
-- @param chapterNum  Chapter number
-- @return table or nil
function chapterDesign.getBoss(chapterNum)
    local ch = chapterDesign.chapters[chapterNum]
    return ch and ch.boss
end

--- Get the mid-boss data for a chapter.
-- @param chapterNum  Chapter number
-- @return table or nil
function chapterDesign.getMidBoss(chapterNum)
    local ch = chapterDesign.chapters[chapterNum]
    return ch and ch.midBoss
end

--- Get the chapter number where a specific ability is first introduced.
-- @param ability  Ability name (e.g. "Fire")
-- @return number or nil
function chapterDesign.getAbilityIntroChapter(ability)
    for num = 0, 20 do
        local ch = chapterDesign.chapters[num]
        if ch and ch.signatureAbility == ability then
            return num
        end
    end
    return nil
end

--- Get a summary table of all chapters for UI display.
-- @return table  Array of {number, name, ability, bossType}
function chapterDesign.getSummary()
    local summary = {}
    for num = 0, 20 do
        local ch = chapterDesign.chapters[num]
        if ch then
            table.insert(summary, {
                number = num,
                name = ch.name,
                ability = ch.signatureAbility,
                bossType = ch.boss and ch.boss.type or "none",
            })
        end
    end
    return summary
end

--- Build the Sakurai-style room flow for a chapter:
---   1. Intro room (safe, NPC explains ability)
---   2. Practice room (signature enemy + safe terrain)
---   3-5. Progression rooms (increasing enemy variety & density)
---   6. Mid-boss room (optional)
---   7-8. Test rooms (require ability mastery)
---   9. Boss approach corridor
---   10. Boss arena
-- @param chapterNum  Chapter number
-- @return table  Array of room descriptors
function chapterDesign.buildRoomFlow(chapterNum)
    local ch = chapterDesign.chapters[chapterNum]
    if not ch then return {} end

    local flow = {}

    -- 1. Intro
    table.insert(flow, {
        type = "intro",
        name = string.format("%02d_intro", ch.number),
        enemies = {},
        abilityStars = { ch.signatureAbility },
        description = "Kirby arrives. AbilityStar grants " .. ch.signatureAbility .. "."
    })

    -- 2. Practice
    local sigEnemies = chapterDesign.getEnemies(chapterNum, "signature")
    table.insert(flow, {
        type = "practice",
        name = string.format("%02d_practice", ch.number),
        enemies = sigEnemies,
        enemyCount = 2,
        abilityStars = {},
        description = "Safe room with " .. (sigEnemies[1] and sigEnemies[1].type or "enemies") .. " to inhale."
    })

    -- 3-5. Progression
    for i = 1, 3 do
        local roomEnemies = {}
        -- Add fodder
        for _, e in ipairs(chapterDesign.getEnemies(chapterNum, "fodder")) do
            table.insert(roomEnemies, e)
        end
        -- Add signature from room 2 onward
        if i >= 2 then
            for _, e in ipairs(sigEnemies) do
                table.insert(roomEnemies, e)
            end
        end
        -- Add secondary from room 3
        if i >= 3 then
            for _, e in ipairs(chapterDesign.getEnemies(chapterNum, "secondary")) do
                table.insert(roomEnemies, e)
            end
        end
        table.insert(flow, {
            type = "progression",
            name = string.format("%02d_prog_%d", ch.number, i),
            enemies = roomEnemies,
            enemyCount = 2 + i,
            abilityStars = (i == 3) and { ch.secondaryAbility } or {},
            description = "Progression room " .. i
        })
    end

    -- 6. Mid-boss (if exists)
    if ch.midBoss then
        table.insert(flow, {
            type = "midboss",
            name = string.format("%02d_midboss", ch.number),
            midBoss = ch.midBoss,
            enemies = {},
            abilityStars = {},
            description = "Mid-boss: " .. (ch.midBoss.variant or "Unknown")
        })
    end

    -- 7-8. Test rooms
    for i = 1, 2 do
        local allEnemies = ch.enemies
        local hazards = chapterDesign.getEnemies(chapterNum, "hazard")
        table.insert(flow, {
            type = "test",
            name = string.format("%02d_test_%d", ch.number, i),
            enemies = allEnemies,
            hazards = hazards,
            enemyCount = 4 + i,
            abilityStars = {},
            description = "Test room " .. i .. " — ability mastery required."
        })
    end

    -- 9. Boss approach
    table.insert(flow, {
        type = "approach",
        name = string.format("%02d_approach", ch.number),
        enemies = chapterDesign.getEnemies(chapterNum, "fodder"),
        enemyCount = 2,
        abilityStars = { ch.signatureAbility },
        description = "Boss approach — last chance to get " .. ch.signatureAbility .. "."
    })

    -- 10. Boss arena
    if ch.boss then
        table.insert(flow, {
            type = "boss",
            name = string.format("%02d_boss", ch.number),
            boss = ch.boss,
            enemies = {},
            abilityStars = {},
            description = "Boss fight!"
        })
    end

    return flow
end

return chapterDesign
