--[[
================================================================================
  MaggyHelper PCG — High-Level API Library
================================================================================

  Drop-in replacement for all Loenn PCG scripts.
  Use this library whenever you need PCG without wiring up Loenn script
  boilerplate (prerun / run hooks, fieldInformation, fieldOrder, etc.).

  Works both:
    • Inside Lönn:   local PCG = mods.requireFromPlugin("libraries.pcg.api")
    • Standalone:    local PCG = require("libraries.pcg.api")

  Quick start:
    local PCG = require("libraries.pcg.api")

    -- Train on hand-crafted rows (optional; Markov presets only)
    PCG.train({ preset = "resort" })

    -- Generate one room
    local room = PCG.generateRoom({ preset = "resort", entityDensity = 0.2 })
    -- room.matrix, room.entities, room.width, room.height

    -- Generate a full multi-room level
    local level = PCG.generateLevel({ preset = "default", numRooms = 8 })
    -- level.rooms  (array of room data tables)

    -- Generate a structured pattern room
    local room = PCG.generatePattern({ pattern = "kirbyBossArena", difficulty = 0.7 })

    -- Spelunky-style cell assembly
    local level = PCG.generateCellLevel({ gridCols = 4, gridRows = 4, cellSize = 8 })

    -- Export to a playable .bin
    PCG.exportBin({ preset = "default", numRooms = 6, outputPath = "out.bin" })

================================================================================
]]

local api = {}

--------------------------------------------------------------------------------
-- Module bootstrapping (Lönn-safe + standalone-safe)
--------------------------------------------------------------------------------

--- Generic require that works inside Lönn (mods.requireFromPlugin) and plain Lua.
local function pluginRequire(path)
    local ok, mods = pcall(require, "mods")
    if ok and mods and mods.requireFromPlugin then
        local r = mods.requireFromPlugin(path)
        if r then return r end
    end
    return require(path)
end

-- Lazy module handles
local _pcg, _seedUtils, _patterns, _cellCatalog, _cellAssembler, _binEncoder, _mapBuilder

local function getPCG()
    if not _pcg then _pcg = pluginRequire("libraries.pcg.init") end
    return _pcg
end
local function getSeedUtils()
    if not _seedUtils then _seedUtils = pluginRequire("libraries.pcg.seed_utils") end
    return _seedUtils
end
local function getPatterns()
    if not _patterns then _patterns = pluginRequire("libraries.pcg.patterns") end
    return _patterns
end
local function getCellCatalog()
    if not _cellCatalog then _cellCatalog = pluginRequire("libraries.pcg.cell_catalog") end
    return _cellCatalog
end
local function getCellAssembler()
    if not _cellAssembler then _cellAssembler = pluginRequire("libraries.pcg.cell_assembler") end
    return _cellAssembler
end
local function getBinEncoder()
    if not _binEncoder then _binEncoder = pluginRequire("libraries.pcg.bin_encoder") end
    return _binEncoder
end
local function getMapBuilder()
    if not _mapBuilder then _mapBuilder = pluginRequire("libraries.pcg.map_builder") end
    return _mapBuilder
end

-- Try to get the Lönn matrix utility; fall back to a minimal shim for standalone use.
local function getMatrix()
    local ok, mat = pcall(require, "utils.matrix")
    if ok and mat then return mat end
    -- Minimal shim so standalone callers don't crash
    local Matrix = {}
    function Matrix.create(w, h, default)
        local m = { _w = w, _h = h, _d = default, _data = {} }
        function m:size() return self._w, self._h end
        function m:get(x, y, def) return self._data[y] and self._data[y][x] or (def or self._d) end
        function m:set(x, y, v)
            if not self._data[y] then self._data[y] = {} end
            self._data[y][x] = v
        end
        return m
    end
    return Matrix
end

--------------------------------------------------------------------------------
-- Internal helpers
--------------------------------------------------------------------------------

--- Resolve a seed from an options table or generate a fresh one.
-- @param opts   table  Options with optional .seed (int) and .autoSeed (bool)
-- @return number
local function resolveSeed(opts)
    local su = getSeedUtils()
    return su.resolveSeed({
        seed     = opts.seed or 0,
        autoSeed = (opts.autoSeed == nil) and true or opts.autoSeed,
    })
end

--- Merge preset patternOpts as defaults into explicit patternOpts.
local function mergePatternOpts(base, presetOpts)
    if not presetOpts then return base end
    for k, v in pairs(presetOpts) do
        if base[k] == nil then base[k] = v end
    end
    return base
end

--- Build a patternOpts table from opts + preset defaults.
-- Returns nil when no pattern is selected.
-- @param patternName string|nil
-- @param opts        table       caller options
-- @param presetData  table       preset definition from pcg.PRESETS
local function buildPatternOpts(patternName, opts, presetData)
    if not patternName then return nil end
    local bossType = opts.kirbyBossType
    if bossType and bossType < 0 then bossType = nil end
    local po = {
        difficulty = opts.difficulty or 0.5,
        bossType   = bossType,
        bossTier   = opts.bossTier or 3,
        chambers   = opts.chambers or 3,
        material   = (presetData and presetData.borderMaterial) or "1",
    }
    return mergePatternOpts(po, presetData and presetData.patternOpts)
end

--- Resolve the effective pattern name from opts + preset fallback.
-- "(none)" is treated as no pattern.
-- @param opts       table
-- @param presetData table|nil
-- @return string|nil
local function resolvePattern(opts, presetData)
    if opts.pattern and opts.pattern ~= "(none)" then
        return opts.pattern
    end
    if presetData and presetData.pattern then
        return presetData.pattern
    end
    return nil
end

--------------------------------------------------------------------------------
-- Public API
--------------------------------------------------------------------------------

--- All available preset names that can be passed as `preset =` option.
-- Use PCG.PRESETS to introspect the full preset table.
api.PRESET_NAMES = {
    -- Generic
    "default", "open", "tight", "fullRow", "minimal",
    -- Space
    "space", "deepSpace",
    -- Chapter-themed
    "resort", "temple", "reflection", "summit", "core",
    "wind", "ice", "cave", "ruins", "castle",
    "darkStars", "void", "nightmare", "farewell", "dream",
    -- Multi-level
    "largeArea", "megaArea",
    -- Pattern-based
    "kirbyBoss", "kirbyFinalBoss", "playerBoss",
    "asrielGodBoss", "asrielGodBossPhase2", "asrielGodBossFinal",
    "challenge", "puzzleRoom", "hubRoom", "serpentine", "dualBoss",
    -- BSP
    "bspPlatformer", "bspSummit", "bspTemple", "bspResort",
}

--- All available pattern names that can be passed as `pattern =` option.
api.PATTERN_NAMES = {
    -- Path
    "serpentine", "zigzag", "loop", "branching", "spiral",
    -- Arena
    "openArena", "tieredArena", "circularArena",
    -- Challenge
    "gauntlet", "staircase", "precisionGaps",
    -- Puzzle
    "dividedChambers", "switchMaze",
    -- Boss
    "kirbyBossArena", "kirbyFinalBossArena", "bossCorridorApproach",
    "normalBossArena", "multiBossArena", "asrielGodBossArena",
    -- Hub
    "crossroads", "starHub",
}

--- All available room style names that can be passed as `roomStyle =` option.
api.ROOM_STYLE_NAMES = {
    "normal", "resort", "temple", "reflection", "summit", "core",
    "wind", "ice", "cave", "ruins", "castle",
    "darkStars", "void", "nightmare", "farewell", "dream",
    "space", "deepSpace",
}

--- Lazy accessor for the full PRESETS table from libraries.pcg.init.
-- Usage: PCG.PRESETS.resort.roomWidth
function api.getPreset(name)
    return getPCG().getPreset(name)
end

--- Get the raw PRESETS table.
api.PRESETS = setmetatable({}, { __index = function(_, k) return getPCG().PRESETS[k] end })

--------------------------------------------------------------------------------
-- Training
--------------------------------------------------------------------------------

---Train the Markov model.
-- When called inside Lönn with a live map loaded it reads rooms from
-- `state.map`.  When called standalone you can pass `opts.rooms` — an array
-- of raw room tables (each with a `tilesFg.matrix` or plain `.matrix`).
--
-- @param opts table  Optional configuration:
--   .preset   string   Preset name (default "default")
--   .rooms    table    Array of room tables to train on (standalone only)
-- @return boolean, table   success, stats
function api.train(opts)
    opts = opts or {}
    local pcg = getPCG()
    pcg.reset()
    local presetName = opts.preset or "default"
    if opts.rooms then
        -- Standalone path: inject rooms directly into the trainer
        local trainer = pcg.trainer
        if trainer and trainer.trainFromRooms then
            return trainer.trainFromRooms(opts.rooms, presetName)
        end
        return false, { error = "trainer.trainFromRooms not available" }
    end
    -- Lönn path: uses loaded_state internally
    return pcg.trainFromMap(presetName)
end

---Check whether the model has been trained (delegates to pcg.isTrained).
function api.isTrained()
    return getPCG().isTrained()
end

---Reset the trained model.
function api.reset()
    getPCG().reset()
end

---Set the global RNG seed.
-- @param seed number  Integer seed value.
function api.setSeed(seed)
    getPCG().setSeed(seed)
end

--------------------------------------------------------------------------------
-- Single room generation
--------------------------------------------------------------------------------

---Generate a single room's tile matrix + entity list.
--
-- @param opts table  Generation options:
--   .preset          string   Preset name (default "default")
--   .roomStyle       string   Visual style (default = preset default)
--   .pattern         string   Structured pattern (default = preset pattern or nil)
--   .exits           table    {left=bool, right=bool, top=bool, bottom=bool}
--                             Defaults to {left=true, right=true}
--   .roomWidth       number   Width in tiles (default = preset)
--   .roomHeight      number   Height in tiles (default = preset)
--   .entityDensity   number   Entity probability 0-1 (default = preset)
--   .cleanupPasses   number   Tile cleanup passes (default = preset)
--   .playabilityCheck bool    Run A* check (default = preset)
--   .maxRetries      number   Retry limit (default = preset)
--   .difficulty      number   0.0-1.0 (default 0.5)
--   .kirbyBossType   number   Mid-boss type -1..9 (default -1 = random)
--   .bossTier        number   Player boss tier 1-5 (default 3)
--   .seed            number   Fixed seed (default auto)
--   .autoSeed        bool     True = auto seed each call (default true)
--   .spaceErodePercent number Override for space erosion
--   .noBorders       bool     Disable border walls
--   .useBsp          bool     Use BSP generator instead of Markov
-- @return table|nil  Room data: { matrix, entities, width, height }
--                    Returns nil on generation failure.
function api.generateRoom(opts)
    opts = opts or {}
    local pcg = getPCG()
    local matrix = getMatrix()

    local seed = resolveSeed(opts)
    pcg.setSeed(seed)

    local presetName = opts.preset or "default"
    local presetData = pcg.getPreset(presetName)
    if not presetData then
        presetData = pcg.getPreset("default")
    end

    -- Train first if not already trained (Markov presets only)
    if not pcg.isTrained() and not (opts.useBsp or (presetData and presetData.useBsp)) then
        pcg.trainFromMap(presetName)
    end

    local exits = opts.exits or { left = true, right = true }
    local roomStyle = opts.roomStyle or (presetData and presetData.roomStyle) or "normal"

    local patternName = resolvePattern(opts, presetData)
    local patternOpts = buildPatternOpts(patternName, opts, presetData)

    return pcg.generateRoom(exits, {
        roomWidth        = opts.roomWidth  or presetData.roomWidth,
        roomHeight       = opts.roomHeight or presetData.roomHeight,
        entityDensity    = opts.entityDensity  or presetData.entityDensity,
        cleanupPasses    = opts.cleanupPasses  or presetData.cleanupPasses,
        playabilityCheck = (opts.playabilityCheck ~= nil) and opts.playabilityCheck or presetData.playabilityCheck,
        maxRetries       = opts.maxRetries or presetData.maxRetries,
        roomStyle        = roomStyle,
        spaceErodePercent = opts.spaceErodePercent or presetData.spaceErodePercent,
        noBorders        = opts.noBorders ~= nil and opts.noBorders or presetData.noBorders,
        trimmedBorders   = opts.trimmedBorders ~= nil and opts.trimmedBorders or presetData.trimmedBorders,
        useBsp           = opts.useBsp ~= nil and opts.useBsp or presetData.useBsp,
        bspOpts          = opts.bspOpts or presetData.bspOpts,
        pattern          = patternName,
        patternOpts      = patternOpts,
        exitSize         = opts.exitSize or presetData.exitSize,
        borderMaterial   = opts.borderMaterial or presetData.borderMaterial,
        backtrackDepth   = opts.backtrackDepth or presetData.backtrackDepth,
    }, matrix)
end

--------------------------------------------------------------------------------
-- Full level generation
--------------------------------------------------------------------------------

---Generate a complete multi-room level (like the pcg_generate_level script).
--
-- @param opts table  Generation options:
--   .preset          string   Preset name (default "default")
--   .roomStyle       string   Visual style override
--   .pattern         string   Structured pattern override
--   .numRooms        number   Rooms per level (default 6)
--   .areaLevels      number   Number of level clusters (default 1)
--   .levelColumns    number   Clusters per row (default = areaLevels)
--   .entityDensity   number   Entity probability 0-1
--   .branchProb      number   Branching probability 0-1
--   .playabilityCheck bool    A* check
--   .difficulty      number   0.0-1.0
--   .kirbyBossType   number   -1..9 (default -1 = random)
--   .bossTier        number   1-5
--   .levelSpacingRoomsX number  X spacing between clusters (room widths)
--   .levelSpacingRoomsY number  Y spacing between clusters (room heights)
--   .seed / .autoSeed         As for generateRoom
-- @return table|nil  Level data: { rooms = { {matrix, entities, x, y, width, height}, … } }
function api.generateLevel(opts)
    opts = opts or {}
    local pcg = getPCG()
    local matrix = getMatrix()

    local seed = resolveSeed(opts)
    pcg.setSeed(seed)

    local presetName = opts.preset or "default"
    local presetData = pcg.getPreset(presetName)
    if not presetData then
        presetData = pcg.getPreset("default")
    end

    -- Train (BSP presets skip this)
    if not presetData.useBsp then
        local ok = pcg.trainFromMap(presetName)
        if not ok then pcg.reset() end
    end

    local roomStyle = opts.roomStyle or presetData.roomStyle or "normal"
    local patternName = resolvePattern(opts, presetData)
    local patternOpts = buildPatternOpts(patternName, opts, presetData)

    local areaLevels  = math.max(1, math.floor(tonumber(opts.areaLevels  or presetData.levelCount  or 1) or 1))
    local levelColumns = math.max(1, math.floor(tonumber(opts.levelColumns or presetData.levelColumns or areaLevels) or 1))

    return pcg.generateArea(opts.numRooms or 6, {
        entityDensity        = opts.entityDensity  or presetData.entityDensity,
        branchProb           = opts.branchProb     or presetData.branchProb,
        playabilityCheck     = (opts.playabilityCheck ~= nil) and opts.playabilityCheck or presetData.playabilityCheck,
        roomStyle            = roomStyle,
        levelCount           = areaLevels,
        levelColumns         = levelColumns,
        levelSpacingRoomsX   = opts.levelSpacingRoomsX or presetData.levelSpacingRoomsX,
        levelSpacingRoomsY   = opts.levelSpacingRoomsY or presetData.levelSpacingRoomsY,
        spaceErodePercent    = opts.spaceErodePercent or presetData.spaceErodePercent,
        noBorders            = opts.noBorders ~= nil and opts.noBorders or presetData.noBorders,
        trimmedBorders       = opts.trimmedBorders ~= nil and opts.trimmedBorders or presetData.trimmedBorders,
        useBsp               = opts.useBsp ~= nil and opts.useBsp or presetData.useBsp,
        bspOpts              = opts.bspOpts or presetData.bspOpts,
        pattern              = patternName,
        patternOpts          = patternOpts,
        roomWidth            = opts.roomWidth  or presetData.roomWidth,
        roomHeight           = opts.roomHeight or presetData.roomHeight,
        borderMaterial       = opts.borderMaterial or presetData.borderMaterial,
        exitSize             = opts.exitSize or presetData.exitSize,
        backtrackDepth       = opts.backtrackDepth or presetData.backtrackDepth,
    }, matrix)
end

--------------------------------------------------------------------------------
-- Pattern room generation
--------------------------------------------------------------------------------

---Generate one or more structured pattern rooms (like pcg_generate_pattern).
--
-- @param opts table  Generation options:
--   .pattern         string   Pattern name (required, see api.PATTERN_NAMES)
--   .preset          string   Base preset (default "default")
--   .roomStyle       string   Visual style override
--   .numRooms        number   How many pattern rooms to generate (default 1)
--   .difficulty      number   0.0-1.0 (default 0.5)
--   .kirbyBossType   number   -1..9 (default random)
--   .bossTier        number   1-5 (default 3)
--   .chambers        number   Chambers for puzzle patterns (default 3)
--   .entityDensity   number   Entity density for generic entities
--   .autoSuggest     bool     Auto-pick pattern from difficulty (overrides .pattern)
--   .isKirby         bool     Kirby-mode flag for autoSuggest
--   .seed / .autoSeed         As for generateRoom
-- @return table[]  Array of room data tables (each: {matrix, entities, width, height})
function api.generatePattern(opts)
    opts = opts or {}
    local pcg = getPCG()
    local pats = getPatterns()
    local matrix = getMatrix()

    local seed = resolveSeed(opts)
    pcg.setSeed(seed)

    local presetName = opts.preset or "default"
    local presetData = pcg.getPreset(presetName) or pcg.getPreset("default")

    -- Train (may fail — that's OK for pattern-only rooms)
    local ok = pcg.trainFromMap(presetName)
    if not ok then pcg.reset() end

    local difficulty  = opts.difficulty or 0.5
    local patternName = opts.pattern or (presetData and presetData.pattern) or "kirbyBossArena"

    -- Auto-suggest pattern from difficulty
    if opts.autoSuggest and pats and pats.suggestPattern then
        local isBoss = pats.categoryOf and (pats.categoryOf(patternName) == "boss")
        patternName = pats.suggestPattern(difficulty, isBoss, opts.isKirby)
    end

    local bossType = opts.kirbyBossType
    if bossType and bossType < 0 then bossType = nil end

    local patternOpts = mergePatternOpts({
        difficulty    = difficulty,
        bossType      = bossType,
        bossTier      = opts.bossTier or 3,
        chambers      = opts.chambers or 3,
        corridorHeight = opts.corridorHeight or 3,
        material      = (presetData and presetData.borderMaterial) or "1",
    }, presetData and presetData.patternOpts)

    local roomStyle = opts.roomStyle or (presetData and presetData.roomStyle) or "normal"
    local numRooms  = math.max(1, opts.numRooms or 1)
    local results   = {}

    for i = 1, numRooms do
        local roomDifficulty = difficulty
        if numRooms > 1 then
            roomDifficulty = difficulty * (0.5 + 0.5 * i / numRooms)
        end
        patternOpts.difficulty = roomDifficulty
        patternOpts.seed       = seed + i * 37

        local roomData = pcg.generateRoom(
            { left = true, right = true },
            {
                roomWidth     = opts.roomWidth  or presetData.roomWidth,
                roomHeight    = opts.roomHeight or presetData.roomHeight,
                exitSize      = opts.exitSize or presetData.exitSize,
                borderMaterial = opts.borderMaterial or presetData.borderMaterial,
                entityDensity = opts.entityDensity or presetData.entityDensity,
                cleanupPasses = opts.cleanupPasses or presetData.cleanupPasses,
                maxRetries    = opts.maxRetries or presetData.maxRetries,
                playabilityCheck = false,
                roomStyle     = roomStyle,
                pattern       = patternName,
                patternOpts   = patternOpts,
            },
            matrix
        )

        if roomData then
            table.insert(results, roomData)
        else
            print(string.format("[PCG API] Pattern room %d/%d failed to generate.", i, numRooms))
        end
    end

    return results
end

--------------------------------------------------------------------------------
-- Spelunky-style cell generation
--------------------------------------------------------------------------------

---Generate a Spelunky-style cell-assembled level.
-- Harvests tile cells from the loaded map (inside Lönn) or from `opts.rooms`
-- (standalone), then assembles a grid of edge-compatible cells.
--
-- @param opts table  Generation options:
--   .gridCols      number   Grid columns (default 4)
--   .gridRows      number   Grid rows (default 4)
--   .cellSize      number   Tile size per cell: 8, 16 or 32 (default 8)
--   .areaFilter    string   Area key to filter catalog cells (default nil = all)
--   .fillNonPath   bool     Fill non-path cells (default true)
--   .fallbackToAny bool     Allow any cell as fallback (default false)
--   .emptyTile     string   Material for unfilled cells (default "1")
--   .seed / .autoSeed
--   .rooms         table    Array of room tables for standalone use
-- @return table|nil  { matrix, entities, width, height, gridResult }
function api.generateCellLevel(opts)
    opts = opts or {}
    local cellCatalog   = getCellCatalog()
    local cellAssembler = getCellAssembler()
    local matrix        = getMatrix()

    -- Resolve seed
    local seed
    if opts.autoSeed or not opts.seed or opts.seed == 0 then
        seed = os.time() + math.random(0, 99999)
    else
        seed = opts.seed
    end
    math.randomseed(seed)

    local cellSize = tonumber(opts.cellSize) or 8
    local gridCols = math.max(1, opts.gridCols or 4)
    local gridRows = math.max(1, opts.gridRows or 4)
    local areaKey  = opts.areaFilter

    -- Build catalog
    local catalog, catalogStats
    if opts.rooms then
        catalog, catalogStats = cellCatalog.buildFromRooms(opts.rooms, cellSize)
    else
        catalog, catalogStats = cellCatalog.buildFromLoadedMap(cellSize)
    end

    if not catalog then
        print("[PCG API] Failed to build cell catalog: " ..
              tostring(catalogStats and catalogStats.error))
        return nil
    end

    -- Select area
    local areaCat
    if areaKey and catalog[areaKey] then
        areaCat = catalog[areaKey]
    else
        areaCat = { cells = {}, bySig = {} }
        for _, ac in pairs(catalog) do
            for _, cell in ipairs(ac.cells) do
                table.insert(areaCat.cells, cell)
                if not areaCat.bySig[cell.sig] then areaCat.bySig[cell.sig] = {} end
                table.insert(areaCat.bySig[cell.sig], cell)
            end
        end
    end

    if #areaCat.cells == 0 then
        print("[PCG API] No cells found! Rooms may be too small for cellSize=" .. cellSize)
        return nil
    end

    -- Assemble grid
    local gridResult = cellAssembler.assembleGrid(areaCat, gridCols, gridRows, {
        seed          = seed,
        fillNonPath   = (opts.fillNonPath == nil) and true or opts.fillNonPath,
        fallbackToAny = opts.fallbackToAny or false,
        emptyTile     = opts.emptyTile or "1",
    })

    -- Stitch into tiles + entities
    local stitched = cellAssembler.stitchGrid(gridResult, matrix, opts.emptyTile or "1")
    if not stitched or not stitched.matrix then
        print("[PCG API] Cell grid stitching failed!")
        return nil
    end

    stitched.gridResult = gridResult
    return stitched
end

--------------------------------------------------------------------------------
-- Export to .bin
--------------------------------------------------------------------------------

---Generate a PCG level and write it to a Celeste-compatible .bin map file.
-- Requires the `libraries.pcg.bin_encoder` and `libraries.pcg.map_builder` modules.
--
-- @param opts table  Generation options (same as generateLevel) plus:
--   .packageName  string  Celeste map SID (default "Maggy/PCG/pcg_generated")
--   .outputPath   string  Output .bin path. Defaults to the mod's Maps folder.
--   .namePrefix   string  Room name prefix (default "pcg")
-- @return boolean, string  success, outputPath|errorMessage
function api.exportBin(opts)
    opts = opts or {}
    local pcg        = getPCG()
    local binEncoder = getBinEncoder()
    local mapBuilder = getMapBuilder()
    local matrix     = getMatrix()

    local seed = resolveSeed(opts)
    pcg.setSeed(seed)

    local presetName  = opts.preset or "default"
    local presetData  = pcg.getPreset(presetName) or pcg.getPreset("default")

    -- Train
    local trainOk = pcg.trainFromMap(presetName)
    if not trainOk then
        print("[PCG API] Training failed — not enough rooms. Continuing anyway.")
        pcg.reset()
    end

    local roomStyle   = opts.roomStyle or presetData.roomStyle or "normal"
    local patternName = resolvePattern(opts, presetData)
    local patternOpts = buildPatternOpts(patternName, opts, presetData)

    local areaLevels   = math.max(1, math.floor(tonumber(opts.areaLevels  or presetData.levelCount  or 1) or 1))
    local levelColumns  = math.max(1, math.floor(tonumber(opts.levelColumns or presetData.levelColumns or areaLevels) or 1))

    local levelData = pcg.generateArea(opts.numRooms or 6, {
        entityDensity      = opts.entityDensity  or presetData.entityDensity,
        branchProb         = opts.branchProb     or presetData.branchProb,
        playabilityCheck   = true,
        roomStyle          = roomStyle,
        levelCount         = areaLevels,
        levelColumns       = levelColumns,
        levelSpacingRoomsX = opts.levelSpacingRoomsX or presetData.levelSpacingRoomsX,
        levelSpacingRoomsY = opts.levelSpacingRoomsY or presetData.levelSpacingRoomsY,
        spaceErodePercent  = opts.spaceErodePercent or presetData.spaceErodePercent,
        noBorders          = opts.noBorders ~= nil and opts.noBorders or presetData.noBorders,
        trimmedBorders     = opts.trimmedBorders ~= nil and opts.trimmedBorders or presetData.trimmedBorders,
        useBsp             = opts.useBsp ~= nil and opts.useBsp or presetData.useBsp,
        bspOpts            = opts.bspOpts or presetData.bspOpts,
        pattern            = patternName,
        patternOpts        = patternOpts,
        roomWidth          = opts.roomWidth  or presetData.roomWidth,
        roomHeight         = opts.roomHeight or presetData.roomHeight,
        borderMaterial     = opts.borderMaterial or presetData.borderMaterial,
        exitSize           = opts.exitSize or presetData.exitSize,
        backtrackDepth     = opts.backtrackDepth or presetData.backtrackDepth,
    }, matrix)

    if not levelData or not levelData.rooms or #levelData.rooms == 0 then
        return false, "PCG generation produced no rooms"
    end

    -- Determine output path
    local packageName = opts.packageName or "Maggy/PCG/pcg_generated"
    local outPath = opts.outputPath
    if not outPath or outPath == "" then
        -- Default: Maps/<packageName>.bin relative to the mod root
        local scriptDir = (arg and arg[0]) and arg[0]:match("(.-)[^/\\]*$") or "./"
        outPath = scriptDir .. "../Maps/" .. packageName .. ".bin"
    end

    -- Build the map structure and encode to .bin
    local mapData = mapBuilder.buildMap(levelData, {
        packageName = packageName,
        namePrefix  = opts.namePrefix or "pcg",
        roomStyle   = roomStyle,
        seed        = seed,
    })

    if not mapData then
        return false, "mapBuilder.buildMap returned nil"
    end

    local ok, err = binEncoder.encode(mapData, outPath)
    if ok then
        print(string.format("[PCG API] Exported %d rooms to: %s", #levelData.rooms, outPath))
        return true, outPath
    else
        return false, tostring(err)
    end
end

--------------------------------------------------------------------------------
-- Utility helpers re-exported for convenience
--------------------------------------------------------------------------------

---Generate a unique random seed (wraps seedUtils.generateSeed).
function api.generateSeed()
    return getSeedUtils().generateSeed()
end

---Compute the next safe entity _id from a list of Lönn room tables.
-- Useful when inserting generated entities into a live map to avoid ID
-- collisions that crash Lönn when clicking rooms.
-- @param rooms table  Array of Lönn room tables (state.map.rooms)
-- @return number
function api.nextEntityId(rooms)
    return getSeedUtils().nextEntityId(rooms)
end

---Return the pattern category ("path", "arena", "challenge", "puzzle", "boss", "hub").
-- @param patternName string
-- @return string|nil
function api.patternCategory(patternName)
    local pats = getPatterns()
    return pats and pats.categoryOf and pats.categoryOf(patternName)
end

---Auto-suggest a pattern name from difficulty and context flags.
-- @param difficulty number  0.0-1.0
-- @param isBoss     bool    True for boss-context rooms
-- @param isKirby    bool    True for Kirby-mode
-- @return string  Pattern name
function api.suggestPattern(difficulty, isBoss, isKirby)
    local pats = getPatterns()
    if pats and pats.suggestPattern then
        return pats.suggestPattern(difficulty, isBoss, isKirby)
    end
    return "openArena"
end

return api
