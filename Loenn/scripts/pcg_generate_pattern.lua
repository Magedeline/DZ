-- PCG Generate Pattern Room Script for LoennScripts
-- Generates rooms using structured room patterns (boss arenas, challenge
-- corridors, puzzle chambers, hub rooms, etc.) with pattern-specific entities.
-- Uses prerun() since it modifies the map structure (adding rooms).

local state = require("loaded_state")
local utils = require("utils")
local snapshot = require("structs.snapshot")
local tilesStruct = require("structs.tiles")
local celesteRender = require("celeste_render")
local matrix = require("utils.matrix")
local mods = require("mods")

local pcg = mods.requireFromPlugin("libraries.pcg.init")
local patterns = mods.requireFromPlugin("libraries.pcg.patterns")
local seedUtils = mods.requireFromPlugin("libraries.pcg.seed_utils")

local script = {}

script.name = "pcgGeneratePattern"
script.displayName = "PCG: Generate Pattern Room"
script.tooltip = "Generates rooms using structured patterns (boss arenas, challenge gauntlets, puzzle chambers, hub rooms, serpentine paths).\nTrains on existing rooms, then applies the selected pattern layout with matching entities."

script.parameters = {
    preset = "default",
    pattern = "kirbyBossArena",
    roomStyle = "normal",
    numRooms = 1,
    difficulty = 0.5,
    autoSeed = true,
    seed = 0,
    entityDensity = 0.15,
    kirbyBossType = -1,       -- -1 = random
    bossTier = 3,
    chambers = 3,
    autoSuggest = false,
    isKirby = false,
    namePrefix = "pcg_pat",
}

script.fieldOrder = {
    "preset", "pattern", "roomStyle", "numRooms", "difficulty",
    "autoSeed", "seed", "entityDensity",
    "kirbyBossType", "bossTier", "chambers",
    "autoSuggest", "isKirby", "namePrefix",
}

script.fieldInformation = {
    preset = {
        fieldType = "loennScripts.dropdown",
        options = {
            "default", "open", "tight", "fullRow", "minimal",
            "resort", "temple", "reflection", "summit", "core",
            "wind", "ice", "cave", "ruins", "castle",
            "darkStars", "void", "nightmare", "farewell", "dream",
            "space", "deepSpace",
            "kirbyBoss", "kirbyFinalBoss", "playerBoss",
            "challenge", "puzzleRoom", "hubRoom", "serpentine", "dualBoss",
        },
    },
    pattern = {
        fieldType = "loennScripts.dropdown",
        options = {
            -- Path patterns
            "serpentine", "zigzag", "loop", "branching", "spiral",
            -- Arena patterns
            "openArena", "tieredArena", "circularArena",
            -- Challenge patterns
            "gauntlet", "staircase", "precisionGaps",
            -- Puzzle patterns
            "dividedChambers", "switchMaze",
            -- Boss patterns
            "kirbyBossArena", "kirbyFinalBossArena", "bossCorridorApproach",
            "normalBossArena", "multiBossArena",
            -- Hub patterns
            "crossroads", "starHub",
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
    numRooms = {
        fieldType = "integer",
    },
    difficulty = {
        fieldType = "number",
    },
    seed = {
        fieldType = "integer",
    },
    kirbyBossType = {
        fieldType = "integer",
    },
    bossTier = {
        fieldType = "integer",
    },
    chambers = {
        fieldType = "integer",
    },
}

script.tooltips = {
    preset = "Base preset for room size and Markov parameters",
    pattern = "Room pattern to apply (overrides preset pattern if set)",
    roomStyle = "Visual style / entity theme",
    numRooms = "Number of pattern rooms to generate",
    difficulty = "Difficulty 0.0 (easy) → 1.0 (hard). Affects hazard density, boss HP, etc.",
    autoSeed = "Auto-generate a unique seed every run (ignores seed value)",
    seed = "Manual seed — only used when autoSeed is unchecked (0 = use current time)",
    entityDensity = "Entity density when using generic style entities (no pattern entities)",
    kirbyBossType = "Kirby mid-boss type 0-9 (-1 = random). See KirbyMidBoss.MidBossType enum.",
    bossTier = "Normal player boss tier 1-5",
    chambers = "Number of chambers for puzzle patterns (dividedChambers, switchMaze)",
    autoSuggest = "Auto-pick pattern based on difficulty (ignores 'pattern' field)",
    isKirby = "Treat as Kirby-mode for auto-suggest (affects boss type)",
    namePrefix = "Prefix for generated room names",
}

function script.prerun(args, layer)
    if not state.map then return end

    -- Train
    local seed = seedUtils.resolveSeed(args)
    pcg.setSeed(seed)

    local presetName = args.preset or "default"
    local trainOk = pcg.trainFromMap(presetName)
    if not trainOk then
        pcg.reset()
    end

    local preset = pcg.getPreset(presetName)
    if not preset then
        preset = pcg.getPreset("default")
    end

    local difficulty = args.difficulty or 0.5
    local patternName = args.pattern
    local isBoss = false

    -- Auto-suggest pattern from difficulty
    if args.autoSuggest and patterns and patterns.suggestPattern then
        isBoss = (patterns.categoryOf(patternName) == "boss")
        patternName = patterns.suggestPattern(difficulty, isBoss, args.isKirby)
    end

    -- Override pattern from preset if preset has one and user didn't explicitly pick
    if preset.pattern and (not args.pattern or args.pattern == "kirbyBossArena") then
        patternName = preset.pattern
    end

    -- Build pattern options
    local bossType = args.kirbyBossType
    if bossType and bossType < 0 then bossType = nil end

    local patternOpts = {
        difficulty = difficulty,
        bossType = bossType,
        bossTier = args.bossTier or 3,
        chambers = args.chambers or 3,
        corridorHeight = 3,
        material = preset.borderMaterial or "1",
    }

    local roomStyle = args.roomStyle or (preset and preset.roomStyle) or "normal"
    local numRooms = math.max(1, args.numRooms or 1)

    -- Find offset
    local maxX = 0
    for _, existingRoom in ipairs(state.map.rooms) do
        local rx = existingRoom.x + (existingRoom.width or 320)
        if rx > maxX then maxX = rx end
    end
    local offsetX = maxX + 320

    local prevRooms = utils.deepcopy(state.map.rooms)
    local newRoomNames = {}

    for i = 1, numRooms do
        -- Vary difficulty across rooms
        local roomDifficulty = difficulty
        if numRooms > 1 then
            roomDifficulty = difficulty * (0.5 + 0.5 * i / numRooms)
        end
        patternOpts.difficulty = roomDifficulty
        patternOpts.seed = seed + i * 37

        -- Generate room using the pattern
        local roomData, err = pcg.generateRoom(
            { left = true, right = true },
            {
                roomWidth = preset.roomWidth,
                roomHeight = preset.roomHeight,
                exitSize = preset.exitSize,
                borderMaterial = preset.borderMaterial,
                backtrackDepth = preset.backtrackDepth,
                entityDensity = args.entityDensity or preset.entityDensity,
                cleanupPasses = preset.cleanupPasses,
                playabilityCheck = preset.playabilityCheck,
                maxRetries = preset.maxRetries,
                roomStyle = roomStyle,
                spaceErodePercent = preset.spaceErodePercent,
                noBorders = preset.noBorders,
                pattern = patternName,
                patternOpts = patternOpts,
            },
            matrix
        )

        if roomData then
            local roomW = (roomData.width or preset.roomWidth) * 8
            local roomH = (roomData.height or preset.roomHeight) * 8

            -- Build tile string
            local tileString = ""
            if roomData.matrix then
                local mat = roomData.matrix
                local mw, mh = mat:size()
                for y = 1, mh do
                    local row = {}
                    for x = 1, mw do
                        row[x] = mat:get(x, y, "0")
                    end
                    if y > 1 then tileString = tileString .. "\n" end
                    tileString = tileString .. table.concat(row)
                end
            end

            -- Empty bg tiles
            local bgString = ""
            local tileW = math.floor(roomW / 8)
            local tileH = math.floor(roomH / 8)
            for y = 1, tileH do
                if y > 1 then bgString = bgString .. "\n" end
                bgString = bgString .. string.rep("0", tileW)
            end

            local roomName = string.format("%s_%02d", args.namePrefix or "pcg_pat", i)
            local roomX = offsetX + (i - 1) * (roomW + 160)
            local roomY = 0

            local newRoom = {
                name = "lvl_" .. roomName,
                x = roomX,
                y = roomY,
                width = roomW,
                height = roomH,
                tilesFg = tilesStruct.decode({ innerText = tileString }),
                tilesBg = tilesStruct.decode({ innerText = bgString }),
                entities = {},
                triggers = {},
                decalsFg = {},
                decalsBg = {},
                musicLayer1 = true,
                musicLayer2 = true,
                musicLayer3 = true,
                musicLayer4 = true,
                dark = ({deepSpace=true,darkStars=true,void=true,nightmare=true,cave=true})[roomStyle] or false,
                underwater = false,
                space = ({space=true,deepSpace=true,darkStars=true,void=true,farewell=true})[roomStyle] or false,
                windPattern = ({wind="Left"})[roomStyle] or "None",
                color = 0,
                cameraOffsetX = 0,
                cameraOffsetY = 0,
                music = "",
                alt_music = "",
                ambience = "",
            }

            -- Add entities
            if roomData.entities then
                for _, entity in ipairs(roomData.entities) do
                    local e = {
                        _name = entity.name or entity._name,
                        x = (entity.x or 0) + roomX,
                        y = (entity.y or 0) + roomY,
                        width = entity.width,
                        height = entity.height,
                        _id = entity._id or 0,
                    }
                    for k, v in pairs(entity) do
                        if k ~= "name" and k ~= "_name" and k ~= "x" and k ~= "y"
                           and k ~= "width" and k ~= "height" and k ~= "_id"
                           and k ~= "nodes" then
                            e[k] = v
                        end
                    end
                    if entity.nodes then
                        e.nodes = {}
                        for _, node in ipairs(entity.nodes) do
                            table.insert(e.nodes, {
                                x = node.x + roomX,
                                y = node.y + roomY,
                            })
                        end
                    end
                    -- Separate triggers from entities
                    if entity._type == "trigger" then
                        e._type = nil
                        table.insert(newRoom.triggers, e)
                    else
                        table.insert(newRoom.entities, e)
                    end
                end
            end

            table.insert(state.map.rooms, newRoom)
            table.insert(newRoomNames, newRoom.name)
        end
    end

    -- Invalidate render
    celesteRender.invalidateRoomCache()

    -- Undo snapshot
    local currentRooms = utils.deepcopy(state.map.rooms)

    local function backward()
        state.map.rooms = prevRooms
        celesteRender.invalidateRoomCache()
    end

    local function forward()
        state.map.rooms = currentRooms
        celesteRender.invalidateRoomCache()
    end

    return snapshot.create(script.name, {}, backward, forward)
end

return script
