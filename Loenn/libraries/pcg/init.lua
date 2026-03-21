-- PCG Init: Main Entry Point for the Procedural Content Generation Library
-- Ties together all PCG modules (markov, generator, skeleton, pathfinder, trainer)
-- and provides high-level API for level generation.
--
-- Usage from other Lönn plugins:
--   local pcg = require("mods").requireFromPlugin("libraries.pcg.init")
--   pcg.trainFromMap()
--   local room = pcg.generateRoom()
--   local level = pcg.generateLevel(8)

local pcg = {}

--------------------------------------------------------------------------------
-- Module Loading
--------------------------------------------------------------------------------

-- Use mods.requireFromPlugin when running inside Lönn, fall back to plain require
-- for standalone usage (e.g. Tools/generate_pcg_map.lua)
local function pluginRequire(path)
    local ok, mods = pcall(require, "mods")
    if ok and mods and mods.requireFromPlugin then
        local result = mods.requireFromPlugin(path)
        if result then return result end
    end
    -- Fallback for standalone Lua (e.g. test scripts)
    return require(path)
end

-- Lazy-load submodules to avoid circular dependencies
local _markov, _generator, _skeleton, _pathfinder, _trainer, _patterns, _bsp, _lua_pcg

local function getMarkov()
    if not _markov then _markov = pluginRequire("libraries.pcg.markov") end
    return _markov
end

local function getGenerator()
    if not _generator then _generator = pluginRequire("libraries.pcg.generator") end
    return _generator
end

local function getSkeleton()
    if not _skeleton then _skeleton = pluginRequire("libraries.pcg.skeleton") end
    return _skeleton
end

local function getPathfinder()
    if not _pathfinder then _pathfinder = pluginRequire("libraries.pcg.pathfinder") end
    return _pathfinder
end

local function getTrainer()
    if not _trainer then _trainer = pluginRequire("libraries.pcg.trainer") end
    return _trainer
end

local function getPatterns()
    if not _patterns then _patterns = pluginRequire("libraries.pcg.patterns") end
    return _patterns
end

local function getBsp()
    if not _bsp then _bsp = pluginRequire("libraries.pcg.bsp_generator") end
    return _bsp
end

local function getLuaPcg()
    if not _lua_pcg then _lua_pcg = pluginRequire("libraries.lua_pcg") end
    return _lua_pcg
end

-- Expose submodules as properties
pcg.markov     = setmetatable({}, { __index = function(_, k) return getMarkov()[k] end })
pcg.generator  = setmetatable({}, { __index = function(_, k) return getGenerator()[k] end })
pcg.skeleton   = setmetatable({}, { __index = function(_, k) return getSkeleton()[k] end })
pcg.pathfinder = setmetatable({}, { __index = function(_, k) return getPathfinder()[k] end })
pcg.trainer    = setmetatable({}, { __index = function(_, k) return getTrainer()[k] end })
pcg.patterns   = setmetatable({}, { __index = function(_, k) return getPatterns()[k] end })
pcg.bsp        = setmetatable({}, { __index = function(_, k) return getBsp()[k] end })
pcg.lua_pcg    = setmetatable({}, { __index = function(_, k) return getLuaPcg()[k] end })

--------------------------------------------------------------------------------
-- Configuration Presets
--------------------------------------------------------------------------------

pcg.PRESETS = {
    -- Based on the paper's findings: "000011012" gives best playability
    default = {
        config = "000011012",
        roomWidth = 40,           -- tiles
        roomHeight = 23,          -- tiles
        exitSize = 5,             -- tiles
        borderMaterial = "1",     -- dirt
        backtrackDepth = 3,       -- MdMC backtracking depth
        entityDensity = 0.15,     -- entity placement probability
        cleanupPasses = 2,        -- post-generation tile cleanup
        branchProb = 0.3,         -- skeleton branching probability
        playabilityCheck = true,  -- validate with A*
        maxRetries = 10,          -- max retries for playable room
        roomStyle = "normal",
    },

    -- For more open, platforming-heavy rooms
    open = {
        config = "000011012",
        roomWidth = 48,
        roomHeight = 27,
        exitSize = 6,
        borderMaterial = "1",
        backtrackDepth = 4,
        entityDensity = 0.2,
        cleanupPasses = 3,
        branchProb = 0.2,
        playabilityCheck = true,
        maxRetries = 15,
        roomStyle = "normal",
    },

    -- For tighter, more enclosed rooms
    tight = {
        config = "000011012",
        roomWidth = 32,
        roomHeight = 18,
        exitSize = 4,
        borderMaterial = "1",
        backtrackDepth = 2,
        entityDensity = 0.1,
        cleanupPasses = 1,
        branchProb = 0.4,
        playabilityCheck = true,
        maxRetries = 8,
        roomStyle = "normal",
    },

    -- Full row context: considers the entire row to the left
    fullRow = {
        config = "000111012",
        roomWidth = 40,
        roomHeight = 23,
        exitSize = 5,
        borderMaterial = "1",
        backtrackDepth = 3,
        entityDensity = 0.15,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "normal",
    },

    -- Minimal context: only the tile directly above
    minimal = {
        config = "000010012",
        roomWidth = 40,
        roomHeight = 23,
        exitSize = 5,
        borderMaterial = "1",
        backtrackDepth = 2,
        entityDensity = 0.15,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = false,
        maxRetries = 5,
        roomStyle = "normal",
    },

    -- Space: floating platforms, no gravity, sparse tiles
    space = {
        config = "000011012",
        roomWidth = 48,
        roomHeight = 27,
        exitSize = 6,
        borderMaterial = "1",
        backtrackDepth = 4,
        entityDensity = 0.25,
        cleanupPasses = 4,
        branchProb = 0.2,
        playabilityCheck = false,
        maxRetries = 8,
        roomStyle = "space",
        spaceErodePercent = 0.55,
        noBorders = true,
    },

    -- Deep Space: even sparser, larger rooms, dream blocks
    deepSpace = {
        config = "000011012",
        roomWidth = 56,
        roomHeight = 32,
        exitSize = 8,
        borderMaterial = "1",
        backtrackDepth = 4,
        entityDensity = 0.3,
        cleanupPasses = 5,
        branchProb = 0.15,
        playabilityCheck = false,
        maxRetries = 8,
        roomStyle = "deepSpace",
        spaceErodePercent = 0.70,
        noBorders = true,
    },

    ---------------------------------------------------------------------------
    -- Chapter-themed presets
    ---------------------------------------------------------------------------

    -- Resort (Ch3): move blocks, Kevin blocks, dash blocks
    resort = {
        config = "000011012",
        roomWidth = 40,
        roomHeight = 23,
        exitSize = 5,
        borderMaterial = "5",  -- tower / resort tileset
        backtrackDepth = 3,
        entityDensity = 0.2,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "resort",
    },

    -- Temple (Ch5): touch switches, torches, dash blocks, puzzle-like
    temple = {
        config = "000011012",
        roomWidth = 40,
        roomHeight = 23,
        exitSize = 5,
        borderMaterial = "d",  -- temple A tileset
        backtrackDepth = 3,
        entityDensity = 0.18,
        cleanupPasses = 2,
        branchProb = 0.35,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "temple",
    },

    -- Reflection (Ch6): seekers, feathers, dream blocks
    reflection = {
        config = "000011012",
        roomWidth = 44,
        roomHeight = 25,
        exitSize = 5,
        borderMaterial = "g",  -- reflection tileset
        backtrackDepth = 3,
        entityDensity = 0.2,
        cleanupPasses = 3,
        branchProb = 0.25,
        playabilityCheck = true,
        maxRetries = 12,
        roomStyle = "reflection",
    },

    -- Summit (Ch7): spinners, bumpers, clouds, boosters, zip movers
    summit = {
        config = "000011012",
        roomWidth = 40,
        roomHeight = 27,
        exitSize = 5,
        borderMaterial = "i",  -- summit tileset
        backtrackDepth = 3,
        entityDensity = 0.22,
        cleanupPasses = 2,
        branchProb = 0.25,
        playabilityCheck = true,
        maxRetries = 12,
        roomStyle = "summit",
    },

    -- Core (Ch8): core blocks, fire barriers, boosters, lightning
    core = {
        config = "000011012",
        roomWidth = 40,
        roomHeight = 23,
        exitSize = 5,
        borderMaterial = "k",  -- core tileset
        backtrackDepth = 3,
        entityDensity = 0.2,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "core",
    },

    -- Wind / Golden Ridge (Ch4): crumbling blocks, clouds, falling blocks, wind
    wind = {
        config = "000011012",
        roomWidth = 40,
        roomHeight = 30,       -- taller for vertical wind sections
        exitSize = 5,
        borderMaterial = "3",  -- snow tileset
        backtrackDepth = 3,
        entityDensity = 0.2,
        cleanupPasses = 2,
        branchProb = 0.2,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "wind",
        windPattern = "Left",  -- metadata for map_builder
    },

    -- Ice: slippery platforms, spinners, bouncy blocks
    ice = {
        config = "000011012",
        roomWidth = 40,
        roomHeight = 23,
        exitSize = 5,
        borderMaterial = "3",  -- snow tileset
        backtrackDepth = 3,
        entityDensity = 0.18,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "ice",
    },

    -- Cave: dark, torches, crystal spinners, falling blocks
    cave = {
        config = "000011012",
        roomWidth = 36,
        roomHeight = 20,       -- smaller, tighter
        exitSize = 4,
        borderMaterial = "8",  -- rock tileset
        backtrackDepth = 2,
        entityDensity = 0.18,
        cleanupPasses = 1,
        branchProb = 0.4,
        playabilityCheck = true,
        maxRetries = 8,
        roomStyle = "cave",
    },

    -- Ruins: ancient structures, touch switches, crumbling walls
    ruins = {
        config = "000011012",
        roomWidth = 44,
        roomHeight = 25,
        exitSize = 5,
        borderMaterial = "A",  -- ruins tileset (custom)
        backtrackDepth = 3,
        entityDensity = 0.18,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "ruins",
    },

    -- Castle: swap blocks, move blocks, Kevin blocks, medieval
    castle = {
        config = "000011012",
        roomWidth = 44,
        roomHeight = 25,
        exitSize = 5,
        borderMaterial = "B",  -- castle tileset (custom)
        backtrackDepth = 3,
        entityDensity = 0.2,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "castle",
    },

    -- Dark Stars (Ch20 "The Last Push"): void, dark, star jumps, spinners, dream blocks
    darkStars = {
        config = "000011012",
        roomWidth = 52,
        roomHeight = 30,
        exitSize = 6,
        borderMaterial = "O",  -- void tileset (custom)
        backtrackDepth = 4,
        entityDensity = 0.28,
        cleanupPasses = 4,
        branchProb = 0.15,
        playabilityCheck = false,
        maxRetries = 10,
        roomStyle = "darkStars",
        spaceErodePercent = 0.50,
        noBorders = true,
    },

    -- Void / Abyss: nightmare blocks, void tendrils, seekers, extreme dark
    void = {
        config = "000011012",
        roomWidth = 48,
        roomHeight = 27,
        exitSize = 6,
        borderMaterial = "X",  -- void stone tileset (custom)
        backtrackDepth = 4,
        entityDensity = 0.22,
        cleanupPasses = 4,
        branchProb = 0.2,
        playabilityCheck = false,
        maxRetries = 10,
        roomStyle = "void",
        spaceErodePercent = 0.45,
        noBorders = true,
    },

    -- Nightmare: penumbra tiles, lightning, seekers, nightmare blocks
    nightmare = {
        config = "000011012",
        roomWidth = 44,
        roomHeight = 25,
        exitSize = 5,
        borderMaterial = "p",  -- penumbra tileset (custom)
        backtrackDepth = 3,
        entityDensity = 0.24,
        cleanupPasses = 3,
        branchProb = 0.25,
        playabilityCheck = false,
        maxRetries = 10,
        roomStyle = "nightmare",
        spaceErodePercent = 0.30,
        noBorders = false,
    },

    -- Farewell (Ch9): floaty space blocks, jellyfish, feathers, dream blocks, sci-fi
    farewell = {
        config = "000011012",
        roomWidth = 52,
        roomHeight = 30,
        exitSize = 6,
        borderMaterial = "n",  -- sci-fi tileset
        backtrackDepth = 4,
        entityDensity = 0.25,
        cleanupPasses = 4,
        branchProb = 0.2,
        playabilityCheck = false,
        maxRetries = 10,
        roomStyle = "farewell",
        spaceErodePercent = 0.40,
        noBorders = true,
    },

    -- Dream / Hopes and Dreams: dreamlike, star jumps, dream orbs, soft
    dream = {
        config = "000011012",
        roomWidth = 48,
        roomHeight = 27,
        exitSize = 6,
        borderMaterial = "N",  -- hopes and dreams tileset (custom)
        backtrackDepth = 3,
        entityDensity = 0.22,
        cleanupPasses = 3,
        branchProb = 0.25,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "dream",
    },

    -- Large Area: broad exploration layouts with multiple sub-levels
    largeArea = {
        config = "000011012",
        roomWidth = 56,
        roomHeight = 32,
        exitSize = 7,
        borderMaterial = "1",
        backtrackDepth = 4,
        entityDensity = 0.2,
        cleanupPasses = 4,
        branchProb = 0.35,
        playabilityCheck = true,
        maxRetries = 14,
        roomStyle = "normal",
        levelCount = 2,
        levelColumns = 2,
        levelSpacingRoomsX = 5,
        levelSpacingRoomsY = 4,
    },

    -- Mega Area: very large, chapter-scale generation with stacked levels
    megaArea = {
        config = "000011012",
        roomWidth = 64,
        roomHeight = 36,
        exitSize = 8,
        borderMaterial = "1",
        backtrackDepth = 4,
        entityDensity = 0.22,
        cleanupPasses = 5,
        branchProb = 0.4,
        playabilityCheck = true,
        maxRetries = 16,
        roomStyle = "normal",
        levelCount = 3,
        levelColumns = 2,
        levelSpacingRoomsX = 6,
        levelSpacingRoomsY = 5,
    },

    ---------------------------------------------------------------------------
    -- Pattern-based presets (room patterns with structured layouts)
    ---------------------------------------------------------------------------

    -- Kirby mid-boss: flat arena with platforms, Kirby mid-boss entity
    kirbyBoss = {
        config = "000011012",
        roomWidth = 52,
        roomHeight = 30,
        exitSize = 6,
        borderMaterial = "B",  -- castle tileset
        backtrackDepth = 3,
        entityDensity = 0.15,
        cleanupPasses = 2,
        branchProb = 0.2,
        playabilityCheck = false,
        maxRetries = 8,
        roomStyle = "castle",
        pattern = "kirbyBossArena",
        patternOpts = {
            difficulty = 0.5,
            bossType = nil,  -- nil = random
            ceilingAlcove = true,
        },
    },

    -- Kirby final boss: multi-phase arena
    kirbyFinalBoss = {
        config = "000011012",
        roomWidth = 64,
        roomHeight = 36,
        exitSize = 7,
        borderMaterial = "B",  -- castle tileset
        backtrackDepth = 4,
        entityDensity = 0.1,
        cleanupPasses = 3,
        branchProb = 0.15,
        playabilityCheck = false,
        maxRetries = 10,
        roomStyle = "castle",
        pattern = "kirbyFinalBossArena",
        patternOpts = {
            difficulty = 0.8,
        },
    },

    -- Normal player boss: Celeste-style boss room
    playerBoss = {
        config = "000011012",
        roomWidth = 48,
        roomHeight = 27,
        exitSize = 5,
        borderMaterial = "1",
        backtrackDepth = 3,
        entityDensity = 0.1,
        cleanupPasses = 2,
        branchProb = 0.2,
        playabilityCheck = false,
        maxRetries = 8,
        roomStyle = "normal",
        pattern = "normalBossArena",
        patternOpts = {
            difficulty = 0.5,
            bossTier = 3,
            platforms = 5,
        },
    },

    -- Challenge gauntlet: precision platforming corridor
    challenge = {
        config = "000011012",
        roomWidth = 56,
        roomHeight = 20,
        exitSize = 5,
        borderMaterial = "7",  -- summit tileset
        backtrackDepth = 3,
        entityDensity = 0.2,
        cleanupPasses = 2,
        branchProb = 0.1,
        playabilityCheck = true,
        maxRetries = 12,
        roomStyle = "summit",
        pattern = "gauntlet",
        patternOpts = {
            difficulty = 0.6,
            corridorHeight = 3,
            obstacles = 6,
        },
    },

    -- Puzzle chambers: lock-and-key divided room
    puzzleRoom = {
        config = "000011012",
        roomWidth = 48,
        roomHeight = 23,
        exitSize = 5,
        borderMaterial = "d",  -- temple tileset
        backtrackDepth = 3,
        entityDensity = 0.15,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "temple",
        pattern = "dividedChambers",
        patternOpts = {
            difficulty = 0.4,
            chambers = 3,
        },
    },

    -- Hub room: central crossroads connecting many exits
    hubRoom = {
        config = "000011012",
        roomWidth = 48,
        roomHeight = 27,
        exitSize = 6,
        borderMaterial = "1",
        backtrackDepth = 3,
        entityDensity = 0.1,
        cleanupPasses = 2,
        branchProb = 0.5,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "normal",
        pattern = "crossroads",
        patternOpts = {
            corridorHeight = 3,
        },
    },

    -- Serpentine exploration: winding S-path through the room
    serpentine = {
        config = "000011012",
        roomWidth = 44,
        roomHeight = 30,
        exitSize = 5,
        borderMaterial = "A",  -- ruins tileset
        backtrackDepth = 3,
        entityDensity = 0.18,
        cleanupPasses = 2,
        branchProb = 0.25,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "ruins",
        pattern = "serpentine",
        patternOpts = {
            corridorHeight = 2,
            segments = 4,
        },
    },

    -- Dual boss arena: Kirby + normal player split fight
    dualBoss = {
        config = "000011012",
        roomWidth = 64,
        roomHeight = 30,
        exitSize = 6,
        borderMaterial = "B",  -- castle tileset
        backtrackDepth = 3,
        entityDensity = 0.1,
        cleanupPasses = 3,
        branchProb = 0.15,
        playabilityCheck = false,
        maxRetries = 10,
        roomStyle = "castle",
        pattern = "multiBossArena",
        patternOpts = {
            difficulty = 0.6,
            bossType = nil,   -- random Kirby boss
            bossTier = 3,
        },
    },

    -- Asriel God Boss arena: Epic Kirby vs Asriel God Boss fight
    asrielGodBoss = {
        config = "000011012",
        roomWidth = 72,        -- larger for epic boss fight
        roomHeight = 40,
        exitSize = 6,
        borderMaterial = "N",  -- Hopes and Dreams tileset
        backtrackDepth = 4,
        entityDensity = 0.08,  -- minimal extra entities, boss is focus
        cleanupPasses = 3,
        branchProb = 0.1,
        playabilityCheck = false,
        maxRetries = 8,
        roomStyle = "dream",
        pattern = "asrielGodBossArena",
        patternOpts = {
            difficulty = 0.7,
            phase = 1,
        },
    },

    -- Asriel God Boss Phase 2: Larger void arena
    asrielGodBossPhase2 = {
        config = "000011012",
        roomWidth = 80,
        roomHeight = 45,
        exitSize = 7,
        borderMaterial = "O",  -- Void tileset
        backtrackDepth = 4,
        entityDensity = 0.06,
        cleanupPasses = 4,
        branchProb = 0.08,
        playabilityCheck = false,
        maxRetries = 8,
        roomStyle = "void",
        spaceErodePercent = 0.15,
        noBorders = false,
        pattern = "asrielGodBossArena",
        patternOpts = {
            difficulty = 0.85,
            phase = 2,
        },
    },

    -- Asriel God Boss Final Phase: Massive climactic arena
    asrielGodBossFinal = {
        config = "000011012",
        roomWidth = 88,
        roomHeight = 50,
        exitSize = 8,
        borderMaterial = "O",  -- Void tileset
        backtrackDepth = 5,
        entityDensity = 0.05,
        cleanupPasses = 5,
        branchProb = 0.05,
        playabilityCheck = false,
        maxRetries = 10,
        roomStyle = "void",
        spaceErodePercent = 0.20,
        noBorders = false,
        pattern = "asrielGodBossArena",
        patternOpts = {
            difficulty = 1.0,
            phase = 3,
        },
    },

    ---------------------------------------------------------------------------
    -- BSP-based presets (Binary Space Partitioning — precision platformer style)
    -- These use the bsp_generator instead of Markov chains and produce open,
    -- non-dungeon layouts suited to 2D precision platforming sections.
    ---------------------------------------------------------------------------

    -- BSP platformer (default): staggered horizontal platforms, open top/sides
    bspPlatformer = {
        config = "000011012",
        roomWidth = 40,
        roomHeight = 23,
        exitSize = 5,
        borderMaterial = "1",
        backtrackDepth = 3,
        entityDensity = 0.2,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "normal",
        useBsp = true,           -- use BSP generator instead of Markov
        trimmedBorders = true,   -- open top / trimmed side walls
        bspOpts = { depth = 4, minLeafSize = 5, padding = 2 },
    },

    -- BSP summit: vertical climbing gauntlet on summit-style platforms
    bspSummit = {
        config = "000011012",
        roomWidth = 36,
        roomHeight = 30,
        exitSize = 5,
        borderMaterial = "i",   -- summit snow tileset
        backtrackDepth = 3,
        entityDensity = 0.22,
        cleanupPasses = 2,
        branchProb = 0.25,
        playabilityCheck = true,
        maxRetries = 12,
        roomStyle = "summit",
        useBsp = true,
        trimmedBorders = true,
        bspOpts = { depth = 5, minLeafSize = 4, padding = 2 },
    },

    -- BSP temple: puzzle-paced temple layout using BSP chambers
    bspTemple = {
        config = "000011012",
        roomWidth = 44,
        roomHeight = 25,
        exitSize = 5,
        borderMaterial = "d",   -- temple A tileset
        backtrackDepth = 3,
        entityDensity = 0.18,
        cleanupPasses = 2,
        branchProb = 0.35,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "temple",
        useBsp = true,
        trimmedBorders = true,
        bspOpts = { depth = 3, minLeafSize = 6, padding = 3 },
    },

    -- BSP resort: wide resort-style platform sections
    bspResort = {
        config = "000011012",
        roomWidth = 48,
        roomHeight = 23,
        exitSize = 5,
        borderMaterial = "5",   -- resort tileset
        backtrackDepth = 3,
        entityDensity = 0.2,
        cleanupPasses = 2,
        branchProb = 0.3,
        playabilityCheck = true,
        maxRetries = 10,
        roomStyle = "resort",
        useBsp = true,
        trimmedBorders = true,
        bspOpts = { depth = 4, minLeafSize = 5, padding = 2 },
    },
}

--------------------------------------------------------------------------------
-- State
--------------------------------------------------------------------------------

local _state = {
    trained = false,
    dpt = nil,
    offsets = nil,
    stats = nil,
    preset = nil,
    seed = nil,
}

--------------------------------------------------------------------------------
-- Training API
--------------------------------------------------------------------------------

--- Train the Markov Chain from the currently loaded map in Lönn.
-- @param presetName    Preset name or nil for "default"
-- @param filterOpts    Room filtering options (optional, see trainer.filterRooms)
-- @return boolean      Success
-- @return table        Training stats or error info
function pcg.trainFromMap(presetName, filterOpts)
    local preset = pcg.PRESETS[presetName or "default"]
    if not preset then
        return false, { error = "Unknown preset: " .. tostring(presetName) }
    end

    local markov = getMarkov()
    local trainer = getTrainer()

    local dpt, stats = trainer.trainFromLoadedMap(markov, preset.config, filterOpts)

    if not dpt then
        _state.trained = false
        return false, stats
    end

    _state.trained = true
    _state.dpt = dpt
    _state.offsets = markov.parseConfig(preset.config)
    _state.stats = stats
    _state.preset = preset

    return true, stats
end

--- Train from a list of room tables directly (for standalone usage).
-- @param rooms        List of rooms with tilesFg.matrix data
-- @param presetName   Preset name (default "default")
-- @return boolean     Success
-- @return table       Stats
function pcg.trainFromRooms(rooms, presetName)
    local preset = pcg.PRESETS[presetName or "default"]
    if not preset then
        return false, { error = "Unknown preset" }
    end

    local markov = getMarkov()
    local trainer = getTrainer()

    local filtered = trainer.filterRooms(rooms)
    local dpt, stats = trainer.train(filtered, markov, preset.config)

    if not dpt then
        _state.trained = false
        return false, stats
    end

    _state.trained = true
    _state.dpt = dpt
    _state.offsets = markov.parseConfig(preset.config)
    _state.stats = stats
    _state.preset = preset

    return true, stats
end

--------------------------------------------------------------------------------
-- Generation API
--------------------------------------------------------------------------------

--- Generate a single room with tiles and entities.
-- Must call trainFromMap() or trainFromRooms() first (unless using BSP preset).
-- @param exits        Exit specification { left=bool, right=bool, top=bool, bottom=bool }
-- @param opts         Override options (partial preset fields)
-- @param matrixLib    Matrix library reference (if running inside Lönn)
-- @return table|nil   { matrix=matrix, entities={}, width=N, height=N } or nil
function pcg.generateRoom(exits, opts, matrixLib)
    opts = opts or {}

    -- BSP / trimmed-border flags from opts or preset (resolve early so BSP can
    -- bypass the Markov training requirement)
    local preset = _state.preset or pcg.PRESETS.default
    local useBsp = opts.useBsp
    if useBsp == nil then useBsp = preset.useBsp end

    if not _state.trained and not useBsp then
        return nil, "Not trained yet. Call pcg.trainFromMap() first."
    end
    local roomW  = opts.roomWidth or preset.roomWidth
    local roomH  = opts.roomHeight or preset.roomHeight
    local exitSz = opts.exitSize or preset.exitSize
    local border = opts.borderMaterial or preset.borderMaterial
    local btDepth = opts.backtrackDepth or preset.backtrackDepth
    local density = opts.entityDensity or preset.entityDensity
    local cleanPasses = opts.cleanupPasses or preset.cleanupPasses
    local checkPlay = opts.playabilityCheck
    if checkPlay == nil then checkPlay = preset.playabilityCheck end

    -- Resolve remaining BSP / trimmed-border flags (useBsp already resolved above)
    local trimmedBorders = opts.trimmedBorders
    if trimmedBorders == nil then trimmedBorders = preset.trimmedBorders end
    local bspOpts = opts.bspOpts or preset.bspOpts or {}

    exits = exits or { left = true, right = true }

    local markov = getMarkov()
    local generator = getGenerator()
    local pathfinder_ = getPathfinder()

    -- Attempt generation with playability check
    local maxRetries = opts.maxRetries or preset.maxRetries
    local bestResult = nil
    local bestScore = -1

    for attempt = 1, maxRetries do
        local seed = (_state.seed or os.time()) + attempt

        -- Generate tile matrix: BSP or Markov Chain
        local matrix
        if useBsp then
            local bspGen = getBsp()
            local bspOptsForRun = {}
            for k, v in pairs(bspOpts) do bspOptsForRun[k] = v end
            bspOptsForRun.material = border
            bspOptsForRun.seed     = seed
            matrix = bspGen.generate(roomW, roomH, bspOptsForRun, matrixLib)
        else
            matrix = markov.generate(
                _state.dpt, _state.offsets,
                roomW, roomH,
                btDepth, seed, matrixLib
            )
        end

        if matrix then
            -- Space erosion: remove a percentage of solid tiles to create floating platforms
            local roomStyle = opts.roomStyle or (preset and preset.roomStyle) or "normal"
            local erodePercent = opts.spaceErodePercent or (preset and preset.spaceErodePercent) or 0
            local noBorders = opts.noBorders
            if noBorders == nil then noBorders = preset and preset.noBorders end

            if erodePercent > 0 then
                generator.erodeForSpace(matrix, erodePercent)
            end

            -- Post-processing borders
            if not noBorders then
                if trimmedBorders then
                    -- Precision-platformer: open top, stubbed side walls, solid floor
                    generator.placeTrimmedBorders(matrix, border, exits, exitSz)
                else
                    generator.placeBorders(matrix, border, exits, exitSz)
                end
            end
            generator.placeExits(matrix, exits, exitSz)

            -- Apply room pattern if requested (carves layout into Markov tiles)
            local patternName = opts.pattern or (preset and preset.pattern) or nil
            local patternApplied = false
            local patternEntities = nil
            if patternName then
                local patternOpts = opts.patternOpts or (preset and preset.patternOpts) or {}
                patternOpts.exits = exits
                patternOpts.material = border
                patternEntities, patternApplied = generator.applyPattern(
                    matrix, patternName, exits, 0, 0, patternOpts
                )
            end

            generator.cleanupTiles(matrix, cleanPasses)

            -- Generate entities: use pattern entities if available, else style-specific
            local entities
            if patternEntities then
                entities = patternEntities
            else
                entities = generator.generateEntitiesForStyle(
                    matrix, exits, 0, 0, density, roomStyle
                )
            end

            -- Playability check
            local playable = true
            local score = 1

            if checkPlay then
                playable = pathfinder_.evaluatePlayability(matrix, exits, exitSz)

                if playable and #entities > 0 then
                    -- Build NLE set
                    local nleSet = {}
                    for _, name in ipairs(generator.NLE_ENTITIES) do
                        nleSet[name] = true
                    end

                    -- Get path for NLE scoring
                    local w, h = matrix:size()
                    local startX, startY = math.floor(w / 2), math.floor(h / 2)
                    local endX, endY = w, math.floor(h / 2)
                    if exits.left then startX, startY = 1, math.floor(h / 2) end
                    if exits.right then endX, endY = w, math.floor(h / 2) end

                    local path = pathfinder_.findPath(matrix, startX, startY, endX, endY)
                    if path then
                        score = pathfinder_.nleProximityScore(path, entities, nleSet)
                    end
                end
            end

            if playable then
                local result = {
                    matrix = matrix,
                    entities = entities,
                    width = roomW,
                    height = roomH,
                    exits = exits,
                    playable = true,
                    score = score,
                    attempt = attempt,
                    roomStyle = roomStyle,
                    pattern = patternApplied and patternName or nil,
                }

                if not checkPlay then
                    return result
                end

                if score > bestScore then
                    bestScore = score
                    bestResult = result
                end
            end
        end
    end

    return bestResult, bestResult and nil or "Failed to generate playable room"
end

--- Generate a complete level with multiple connected rooms.
-- @param numRooms    Number of rooms (default 10)
-- @param opts        Override options
-- @param matrixLib   Matrix library reference
-- @return table|nil  Level data { rooms = {roomData...}, skeleton = skel }
function pcg.generateLevel(numRooms, opts, matrixLib)
    if not _state.trained then
        return nil, "Not trained yet"
    end

    numRooms = numRooms or 10
    opts = opts or {}
    local preset = _state.preset or pcg.PRESETS.default
    local roomW = opts.roomWidth or preset.roomWidth
    local roomH = opts.roomHeight or preset.roomHeight
    local branchProb = opts.branchProb or preset.branchProb

    local skel = getSkeleton().generate(numRooms, roomW, roomH, branchProb, _state.seed)

    local levelRooms = {}
    local failedRooms = {}

    for _, skelRoom in ipairs(skel.rooms) do
        -- Generate room content
        local roomData, err = pcg.generateRoom(skelRoom.exits, opts, matrixLib)

        if roomData then
            roomData.name = string.format("pcg_%02d", skelRoom.id)
            roomData.x = skelRoom.x
            roomData.y = skelRoom.y
            roomData.skelRoom = skelRoom

            -- Offset entity positions to room's world position
            for _, entity in ipairs(roomData.entities) do
                entity.x = entity.x + skelRoom.x
                entity.y = entity.y + skelRoom.y
            end

            table.insert(levelRooms, roomData)
        else
            table.insert(failedRooms, { id = skelRoom.id, error = err })
        end
    end

    return {
        rooms = levelRooms,
        skeleton = skel,
        failedRooms = failedRooms,
        visualization = getSkeleton().visualize(skel),
    }
end

--- Generate a large area split across multiple level clusters.
-- Each level cluster is internally connected; clusters are spatially separated.
-- @param numRooms    Total number of rooms to generate
-- @param opts        Override options (supports levelCount / levelColumns / level spacing)
-- @param matrixLib   Matrix library reference
-- @return table|nil  Area data { rooms = {...}, levels = {...}, failedRooms = {...} }
function pcg.generateArea(numRooms, opts, matrixLib)
    opts = opts or {}
    -- BSP presets bypass the Markov training requirement
    local useBsp = opts.useBsp
    if useBsp == nil then
        local p = _state.preset or pcg.PRESETS.default
        useBsp = p and p.useBsp
    end
    if not _state.trained and not useBsp then
        return nil, "Not trained yet"
    end

    numRooms = math.max(1, tonumber(numRooms) or 10)
    opts = opts or {}

    local preset = _state.preset or pcg.PRESETS.default
    local roomW = opts.roomWidth or preset.roomWidth
    local roomH = opts.roomHeight or preset.roomHeight
    local branchProb = opts.branchProb or preset.branchProb

    local levelCount = math.max(1, math.floor(tonumber(opts.levelCount or preset.levelCount or 1) or 1))
    local levelColumns = math.max(1, math.floor(tonumber(opts.levelColumns or preset.levelColumns or 1) or 1))

    local spacingRoomsX = tonumber(opts.levelSpacingRoomsX or preset.levelSpacingRoomsX or 5) or 5
    local spacingRoomsY = tonumber(opts.levelSpacingRoomsY or preset.levelSpacingRoomsY or 4) or 4
    local levelStrideX = math.floor((roomW * 8) * spacingRoomsX)
    local levelStrideY = math.floor((roomH * 8) * spacingRoomsY)

    local roomsPerLevel = math.floor(numRooms / levelCount)
    local remainder = numRooms % levelCount

    local allRooms = {}
    local allFailed = {}
    local levelSummaries = {}
    local roomIdOffset = 0

    for levelIndex = 1, levelCount do
        local levelRoomCount = roomsPerLevel
        if levelIndex <= remainder then
            levelRoomCount = levelRoomCount + 1
        end

        if levelRoomCount > 0 then
            local levelSeed = (_state.seed or os.time()) + (levelIndex * 997)
            local skel = getSkeleton().generate(levelRoomCount, roomW, roomH, branchProb, levelSeed)

            local minX, minY = math.huge, math.huge
            local maxX, maxY = -math.huge, -math.huge
            for _, skelRoom in ipairs(skel.rooms) do
                if skelRoom.x < minX then minX = skelRoom.x end
                if skelRoom.y < minY then minY = skelRoom.y end
                if skelRoom.x > maxX then maxX = skelRoom.x end
                if skelRoom.y > maxY then maxY = skelRoom.y end
            end

            local col = (levelIndex - 1) % levelColumns
            local row = math.floor((levelIndex - 1) / levelColumns)
            local baseX = col * levelStrideX
            local baseY = row * levelStrideY

            local generatedCount = 0
            local failedCount = 0

            for _, skelRoom in ipairs(skel.rooms) do
                local roomData, err = pcg.generateRoom(skelRoom.exits, opts, matrixLib)

                if roomData then
                    roomIdOffset = roomIdOffset + 1
                    roomData.name = string.format("pcg_l%02d_%02d", levelIndex, skelRoom.id)
                    roomData.x = (skelRoom.x - minX) + baseX
                    roomData.y = (skelRoom.y - minY) + baseY
                    roomData.skelRoom = skelRoom
                    roomData.levelIndex = levelIndex

                    for _, entity in ipairs(roomData.entities) do
                        entity.x = entity.x + roomData.x
                        entity.y = entity.y + roomData.y
                    end

                    table.insert(allRooms, roomData)
                    generatedCount = generatedCount + 1
                else
                    table.insert(allFailed, {
                        id = roomIdOffset + skelRoom.id,
                        levelIndex = levelIndex,
                        error = err,
                    })
                    failedCount = failedCount + 1
                end
            end

            table.insert(levelSummaries, {
                levelIndex = levelIndex,
                roomCount = generatedCount,
                failedCount = failedCount,
                bounds = {
                    minX = baseX,
                    minY = baseY,
                    maxX = baseX + (maxX - minX),
                    maxY = baseY + (maxY - minY),
                },
                startRoom = skel.startRoom,
                endRoom = skel.endRoom,
                skeleton = skel,
            })
        end
    end

    return {
        rooms = allRooms,
        levels = levelSummaries,
        failedRooms = allFailed,
        roomCount = #allRooms,
        levelCount = levelCount,
    }
end

--------------------------------------------------------------------------------
-- Utility API
--------------------------------------------------------------------------------

--- Set the random seed for generation.
-- @param seed   Integer seed value
function pcg.setSeed(seed)
    _state.seed = seed
    math.randomseed(seed)
end

--- Check if the model is trained.
-- @return boolean
function pcg.isTrained()
    return _state.trained
end

--- Get training stats.
-- @return table|nil
function pcg.getStats()
    return _state.stats
end

--- Get tile palette from current map (useful for previewing available tiles).
-- @return table|nil  Palette of tile chars → counts
function pcg.getTilePalette()
    local ok, state = pcall(require, "loaded_state")
    if not ok or not state or not state.map then
        return nil
    end
    return getTrainer().tilePalette(state.map.rooms)
end

--- Get entity statistics from current map.
-- @return table|nil  Entity names → counts
function pcg.getEntityStats()
    local ok, state = pcall(require, "loaded_state")
    if not ok or not state or not state.map then
        return nil
    end
    return getTrainer().entityStats(state.map.rooms)
end

--- Reset trained state.
function pcg.reset()
    _state.trained = false
    _state.dpt = nil
    _state.offsets = nil
    _state.stats = nil
end

--- Get available preset names.
-- @return table  List of preset name strings
function pcg.getPresetNames()
    local names = {}
    for name, _ in pairs(pcg.PRESETS) do
        table.insert(names, name)
    end
    table.sort(names)
    return names
end

--- Get a preset by name.
-- @param name  Preset name
-- @return table|nil  Preset configuration
function pcg.getPreset(name)
    return pcg.PRESETS[name]
end

--------------------------------------------------------------------------------
-- ML.NET Bridge (ml_bridge.lua)
-- Reads cluster centroids exported by the C# PCGGenerator and provides
-- K-Means room classification for ML-guided preset / difficulty selection.
--------------------------------------------------------------------------------

local _mlBridge
local function getMlBridge()
    if not _mlBridge then
        _mlBridge = pluginRequire("libraries.pcg.ml_bridge")
    end
    return _mlBridge
end

-- Expose as a proxy property (same pattern as other submodules)
pcg.mlBridge = setmetatable({}, { __index = function(_, k) return getMlBridge()[k] end })

--- Classify a feature table using the ML.NET K-Means centroids.
-- Returns the nearest cluster (id, label, suggestedPreset, entityDensity, difficulty)
-- or nil when the bridge is not loaded (centroids file not found).
-- @param features   table: { enemyCount, platformCount, averageGap, completionRate }
-- @return cluster|nil, distance|nil
function pcg.mlClassify(features)
    local bridge = getMlBridge()
    if not bridge or not bridge.isLoaded() then return nil, nil end
    return bridge.classify(features)
end

--- Get ML-guided generation parameters for a given feature vector.
-- Returns a table: { preset, entityDensity, difficulty, label, clusterId }
-- or nil when centroids are unavailable (falls back to manual parameters).
-- @param features   table: { enemyCount, platformCount, averageGap, completionRate }
-- @return table|nil
function pcg.mlGuidePreset(features)
    local bridge = getMlBridge()
    if not bridge or not bridge.isLoaded() then return nil end
    return bridge.guidePreset(features)
end

--- Extract PCGLevelData features from a Lönn room and classify it.
-- Combines mlBridge.extractFeatures + mlBridge.classify in one call.
-- @param room   Lönn room table
-- @return cluster|nil, features, distance
function pcg.mlClassifyRoom(room)
    local bridge = getMlBridge()
    if not bridge or not bridge.isLoaded() then return nil, nil, nil end
    return bridge.classifyRoom(room)
end

--- Reload the centroids file (e.g. after the game re-exports the model).
-- @param jsonPath   Optional explicit path; uses default search paths when nil.
-- @return boolean, string   success, errorMessage
function pcg.mlReloadCentroids(jsonPath)
    local bridge = getMlBridge()
    if not bridge then return false, "ml_bridge module unavailable" end
    return bridge.load(jsonPath)
end

--- Return a human-readable ML bridge status string for logging / Loenn UI.
function pcg.mlStatus()
    local bridge = getMlBridge()
    if not bridge then return "[PCG ML Bridge] Module unavailable" end
    return bridge.summary()
end

return pcg
