-- PCG Export to .bin Script for LoennScripts
-- Exports PCG-generated rooms to a standalone Celeste map.bin file.
-- Can be loaded directly in Celeste via the Everest debug menu.

local state = require("loaded_state")
local matrix = require("utils.matrix")
local mods = require("mods")

local pcg = mods.requireFromPlugin("libraries.pcg.init")
local binEncoder = mods.requireFromPlugin("libraries.pcg.bin_encoder")
local mapBuilder = mods.requireFromPlugin("libraries.pcg.map_builder")
local seedUtils = mods.requireFromPlugin("libraries.pcg.seed_utils")

local script = {}

script.name = "pcgExportBin"
script.displayName = "PCG: Export .bin Map"
script.tooltip = "Generates a PCG level and saves it as a standalone .bin map file.\nThe file can be loaded in Celeste via the Everest debug map menu."

script.parameters = {
    preset = "default",
    roomStyle = "normal",
    pattern = "(none)",
    numRooms = 6,
    areaLevels = 1,
    levelColumns = 1,
    autoSeed = true,
    seed = 0,
    packageName = "Maggy/PCG/pcg_generated",
    outputPath = "",
    entityDensity = 0.15,
    branchProb = 0.3,
    difficulty = 0.5,
    levelSpacingRoomsX = 5,
    levelSpacingRoomsY = 4,
    kirbyBossType = -1,
    bossTier = 3,
}

script.fieldOrder = {
    "preset", "roomStyle", "pattern", "numRooms", "autoSeed", "seed", "packageName",
    "outputPath", "entityDensity", "branchProb", "difficulty", "areaLevels",
    "levelColumns", "levelSpacingRoomsX", "levelSpacingRoomsY",
    "kirbyBossType", "bossTier",
}

script.fieldInformation = {
    preset = {
        fieldType = "loennScripts.dropdown",
        options = {
            "default", "open", "tight", "fullRow", "minimal",
            "resort", "temple", "reflection", "summit", "core",
            "wind", "ice", "cave", "ruins", "castle",
            "darkStars", "void", "nightmare", "farewell", "dream",
            "space", "deepSpace", "largeArea", "megaArea",
            "kirbyBoss", "kirbyFinalBoss", "playerBoss",
            "challenge", "puzzleRoom", "hubRoom", "serpentine", "dualBoss",
        },
    },
    roomStyle = {
        fieldType = "loennScripts.dropdown",
        options = {
            "normal", "resort", "temple", "reflection", "summit", "core",
            "wind", "ice", "cave", "ruins", "castle",
            "darkStars", "void", "nightmare", "farewell", "dream",
            "space", "deepSpace",
        },
    },
    pattern = {
        fieldType = "loennScripts.dropdown",
        options = {
            "(none)",
            "serpentine", "zigzag", "loop", "branching", "spiral",
            "openArena", "tieredArena", "circularArena",
            "gauntlet", "staircase", "precisionGaps",
            "dividedChambers", "switchMaze",
            "kirbyBossArena", "kirbyFinalBossArena", "bossCorridorApproach",
            "normalBossArena", "multiBossArena",
            "crossroads", "starHub",
        },
    },
    numRooms = {
        fieldType = "integer",
    },
    seed = {
        fieldType = "integer",
    },
    difficulty = {
        fieldType = "number",
    },
    areaLevels = {
        fieldType = "integer",
    },
    levelColumns = {
        fieldType = "integer",
    },
    kirbyBossType = {
        fieldType = "integer",
    },
    bossTier = {
        fieldType = "integer",
    },
    levelSpacingRoomsX = {
        fieldType = "number",
    },
    levelSpacingRoomsY = {
        fieldType = "number",
    },
    outputPath = {
        fieldType = "loennScripts.directFilepath",
        extension = "bin",
    },
}

script.tooltips = {
    preset = "MdMC preset controlling room size and generation parameters (includes pattern presets like kirbyBoss, challenge, etc.)",
    roomStyle = "Room style: normal, resort, temple, reflection, summit, core, wind, ice, cave, ruins, castle, darkStars, void, nightmare, farewell, dream, space, deepSpace",
    pattern = "Room pattern to apply: (none) for Markov-only, or pick a structured layout (overrides preset pattern)",
    numRooms = "Number of rooms to generate",
    areaLevels = "Number of generated level clusters inside the exported area",
    levelColumns = "How many level clusters to place per row",
    autoSeed = "Auto-generate a unique seed every run (ignores seed value)",
    seed = "Manual seed — only used when autoSeed is unchecked (0 = use current time)",
    packageName = "Celeste map SID (e.g. Maggy/PCG/pcg_generated)",
    outputPath = "Output file path (leave empty for default location)",
    entityDensity = "Probability of placing entities (0.0 - 1.0)",
    branchProb = "Probability of branching paths (0.0 - 1.0)",
    difficulty = "Difficulty 0.0 (easy) → 1.0 (hard). Affects pattern entity placement (boss HP, hazard density)",
    levelSpacingRoomsX = "Horizontal spacing between generated levels (in room widths)",
    levelSpacingRoomsY = "Vertical spacing between generated levels (in room heights)",
    kirbyBossType = "Kirby mid-boss type 0-9 (-1 = random). Only used by Kirby boss patterns.",
    bossTier = "Normal player boss tier 1-5. Only used by player boss patterns.",
}

function script.prerun(args)
    if not state.map then return end

    local seed = seedUtils.resolveSeed(args)
    pcg.setSeed(seed)

    -- Train
    local trainOk = pcg.trainFromMap(args.preset or "default")
    if not trainOk then
        print("[PCG] Training failed — not enough rooms.")
        return
    end

    -- Generate
    local presetData = pcg.getPreset(args.preset or "default")
    local roomStyle = args.roomStyle or (presetData and presetData.roomStyle) or "normal"

    local areaLevels = math.max(1, math.floor(tonumber(args.areaLevels or presetData and presetData.levelCount or 1) or 1))
    local levelColumns = math.max(1, math.floor(tonumber(args.levelColumns or presetData and presetData.levelColumns or areaLevels) or 1))

    -- Resolve pattern: explicit selection overrides preset, "(none)" means no pattern
    local patternName = nil
    if args.pattern and args.pattern ~= "(none)" then
        patternName = args.pattern
    elseif presetData and presetData.pattern then
        patternName = presetData.pattern
    end

    local bossType = args.kirbyBossType
    if bossType and bossType < 0 then bossType = nil end

    local patternOpts = nil
    if patternName then
        patternOpts = {
            difficulty = args.difficulty or 0.5,
            bossType = bossType,
            bossTier = args.bossTier or 3,
            material = presetData and presetData.borderMaterial or "1",
        }
        if presetData and presetData.patternOpts then
            for k, v in pairs(presetData.patternOpts) do
                if patternOpts[k] == nil then patternOpts[k] = v end
            end
        end
    end

    local levelData = pcg.generateArea(args.numRooms or 6, {
        entityDensity = args.entityDensity or 0.15,
        branchProb = args.branchProb or 0.3,
        playabilityCheck = true,
        roomStyle = roomStyle,
        levelCount = areaLevels,
        levelColumns = levelColumns,
        levelSpacingRoomsX = args.levelSpacingRoomsX,
        levelSpacingRoomsY = args.levelSpacingRoomsY,
        spaceErodePercent = presetData and presetData.spaceErodePercent,
        noBorders = presetData and presetData.noBorders,
        pattern = patternName,
        patternOpts = patternOpts,
    }, matrix)

    if not levelData or not levelData.rooms or #levelData.rooms == 0 then
        print("[PCG] Generation failed — no rooms produced.")
        return
    end

    -- Build map tree
    local packageName = args.packageName or "Maggy/PCG/pcg_generated"
    local mapData = mapBuilder.buildMap(levelData, packageName)

    -- Determine output path
    local outputPath = args.outputPath
    if not outputPath or outputPath == "" then
        -- Try to find the mod's Maps directory
        local modPath = mods.getFilePath and mods.getFilePath("Maps/" .. packageName .. ".bin")
        if modPath then
            outputPath = modPath
        else
            -- Fallback: save next to the currently loaded map
            outputPath = packageName:gsub("/", "_") .. ".bin"
        end
    end

    -- Encode and save
    local ok, err = binEncoder.encodeFile(outputPath, mapData)
    if ok then
        local generatedLevels = levelData.levels or {}
        print(string.format("[PCG] Map exported successfully!"))
        print(string.format("[PCG]   Rooms: %d", #levelData.rooms))
        print(string.format("[PCG]   Levels: %d", #generatedLevels))
        for _, info in ipairs(generatedLevels) do
            local levelIdx = info.levelIndex or 0
            local roomCount = info.roomCount or 0
            local failedCount = info.failedCount or 0
            print(string.format("[PCG]     Level %02d: %d rooms (%d failed)", levelIdx, roomCount, failedCount))
        end
        print(string.format("[PCG]   SID: %s", packageName))
        print(string.format("[PCG]   File: %s", outputPath))
        print(string.format("[PCG]   Seed: %d", seed))
    else
        print("[PCG] Export failed: " .. tostring(err))
    end
end

return script
