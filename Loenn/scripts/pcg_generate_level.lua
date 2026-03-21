-- PCG Generate Level Script for LoennScripts
-- Generates a complete multi-room level using PCG and adds the rooms to the map.
-- Uses prerun() since it modifies the map structure (adding rooms).

local state = require("loaded_state")
local utils = require("utils")
local snapshot = require("structs.snapshot")
local tilesStruct = require("structs.tiles")
local celesteRender = require("celeste_render")
local mapItemUtils = require("map_item_utils")
local roomStruct = require("structs.room")
local matrix = require("utils.matrix")
local mods = require("mods")

local pcg = mods.requireFromPlugin("libraries.pcg.init")
local seedUtils = mods.requireFromPlugin("libraries.pcg.seed_utils")

local script = {}

script.name = "pcgGenerateLevel"
script.displayName = "PCG: Generate Level"
script.tooltip = "Generates a complete multi-room PCG level and adds it to the current map.\nTrains on existing rooms, then generates new connected rooms."

script.parameters = {
    preset = "default",
    roomStyle = "normal",
    pattern = "(none)",
    numRooms = 6,
    areaLevels = 1,
    levelColumns = 1,
    autoSeed = true,
    seed = 0,
    entityDensity = 0.15,
    branchProb = 0.3,
    difficulty = 0.5,
    playabilityCheck = true,
    levelSpacingRoomsX = 5,
    levelSpacingRoomsY = 4,
    kirbyBossType = -1,
    bossTier = 3,
    namePrefix = "pcg",
}

script.fieldOrder = {
    "preset", "roomStyle", "pattern", "numRooms", "autoSeed", "seed", "entityDensity",
    "branchProb", "difficulty", "areaLevels", "levelColumns", "levelSpacingRoomsX",
    "levelSpacingRoomsY", "kirbyBossType", "bossTier", "playabilityCheck", "namePrefix",
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
            "asrielGodBoss", "asrielGodBossPhase2", "asrielGodBossFinal",
            "challenge", "puzzleRoom", "hubRoom", "serpentine", "dualBoss",
            -- BSP precision-platformer presets
            "bspPlatformer", "bspSummit", "bspTemple", "bspResort",
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
            "normalBossArena", "multiBossArena", "asrielGodBossArena",
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
}

script.tooltips = {
    preset = "MdMC preset controlling room size and generation parameters (includes pattern presets like kirbyBoss, challenge, etc.)",
    roomStyle = "Room style: normal, resort, temple, reflection, summit, core, wind, ice, cave, ruins, castle, darkStars, void, nightmare, farewell, dream, space, deepSpace",
    pattern = "Room pattern to apply: (none) for Markov-only, or pick a structured layout (overrides preset pattern)",
    numRooms = "Number of rooms to generate",
    areaLevels = "Number of generated level clusters inside the area",
    levelColumns = "How many level clusters to place per row",
    autoSeed = "Auto-generate a unique seed every run (ignores seed value)",
    seed = "Manual seed — only used when autoSeed is unchecked (0 = use current time)",
    entityDensity = "Probability of placing entities (0.0 - 1.0)",
    branchProb = "Probability of branching paths (0.0 - 1.0)",
    difficulty = "Difficulty 0.0 (easy) → 1.0 (hard). Affects pattern entity placement (boss HP, hazard density)",
    levelSpacingRoomsX = "Horizontal spacing between generated levels (in room widths)",
    levelSpacingRoomsY = "Vertical spacing between generated levels (in room heights)",
    kirbyBossType = "Kirby mid-boss type 0-9 (-1 = random). Only used by Kirby boss patterns.",
    bossTier = "Normal player boss tier 1-5. Only used by player boss patterns.",
    playabilityCheck = "Verify rooms are traversable using A* pathfinding",
    namePrefix = "Prefix for generated room names (e.g. pcg → lvl_pcg_01)",
}

function script.prerun(args, layer)
    if not state.map then
        error("[PCG] No map is currently open. Please open a map before running this script.")
        return
    end

    local ok, resultOrErr = pcall(function()
        return script._prerunImpl(args, layer)
    end)

    if not ok then
        -- Surface the real error so it appears in Lönn's log/console
        -- instead of a generic "failed to run" message.
        print("[PCG Generate Level] ERROR: " .. tostring(resultOrErr))
        error(resultOrErr)
        return
    end

    return resultOrErr
end

function script._prerunImpl(args, layer)
    -- Train on the current map (Markov-based presets need this; BSP presets do not)
    local seed = seedUtils.resolveSeed(args)
    pcg.setSeed(seed)

    local preset = pcg.getPreset(args.preset or "default")
    if not preset then return end

    -- Only try Markov training when the chosen preset doesn't use BSP
    if not preset.useBsp then
        local trainOk, trainStats = pcg.trainFromMap(args.preset or "default")
        if not trainOk then
            -- Not enough training data — still try with whatever we have
            pcg.reset()
        end
    end

    local areaLevels = math.max(1, math.floor(tonumber(args.areaLevels or preset.levelCount or 1) or 1))
    local levelColumns = math.max(1, math.floor(tonumber(args.levelColumns or preset.levelColumns or areaLevels) or 1))

    -- Generate level / area
    local presetData = pcg.getPreset(args.preset or "default")
    local roomStyle = args.roomStyle or (presetData and presetData.roomStyle) or "normal"

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
        -- Merge preset patternOpts as defaults
        if presetData and presetData.patternOpts then
            for k, v in pairs(presetData.patternOpts) do
                if patternOpts[k] == nil then patternOpts[k] = v end
            end
        end
    end

    local levelData = pcg.generateArea(args.numRooms or 6, {
        entityDensity = args.entityDensity or preset.entityDensity,
        branchProb = args.branchProb or preset.branchProb,
        playabilityCheck = args.playabilityCheck,
        roomStyle = roomStyle,
        levelCount = areaLevels,
        levelColumns = levelColumns,
        levelSpacingRoomsX = args.levelSpacingRoomsX,
        levelSpacingRoomsY = args.levelSpacingRoomsY,
        spaceErodePercent = presetData and presetData.spaceErodePercent,
        noBorders = presetData and presetData.noBorders,
        trimmedBorders = presetData and presetData.trimmedBorders,
        useBsp = presetData and presetData.useBsp,
        bspOpts = presetData and presetData.bspOpts,
        pattern = patternName,
        patternOpts = patternOpts,
    }, matrix)

    if not levelData or not levelData.rooms or #levelData.rooms == 0 then
        return
    end

    -- Find an offset so new rooms don't overlap existing ones
    local maxX = 0
    for _, existingRoom in ipairs(state.map.rooms) do
        local rx = existingRoom.x + (existingRoom.width or 320)
        if rx > maxX then maxX = rx end
    end
    local offsetX = maxX + 320  -- 320px gap

    -- Compute starting entity ID so generated entities don't collide with
    -- existing map entity IDs (Lönn crashes on click when IDs are duplicate).
    local nextEntityId = seedUtils.nextEntityId(state.map.rooms)

    -- Save state for undo
    local prevRooms = utils.deepcopy(state.map.rooms)

    -- Grab the generator library to produce stylegrounds and decals
    local mods = require("mods")
    local pcgLib = mods.requireFromPlugin("libraries.pcg.init")
    local gen = pcgLib and pcgLib.generator

    -- Add generated rooms to the map
    local newRoomNames = {}
    for i, roomData in ipairs(levelData.rooms) do
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

        -- Build empty bg tile string (all air)
        local bgString = ""
        local tileW = math.floor(roomW / 8)
        local tileH = math.floor(roomH / 8)
        for y = 1, tileH do
            if y > 1 then bgString = bgString .. "\n" end
            bgString = bgString .. string.rep("0", tileW)
        end

        -- Create room structure
        local roomName = string.format("%s_%02d", args.namePrefix or "pcg", i)
        local roomX = offsetX + (roomData.x or 0)
        local roomY = roomData.y or 0

        -- Generate stylegrounds for this room style
        local styleBg = {}
        local styleFg = {}
        if gen and gen.generateStylegrounds then
            local sg = gen.generateStylegrounds(roomStyle)
            styleBg = sg.bg or {}
            styleFg = sg.fg or {}
        end

        -- Generate decals (fgdecals / bgdecals)
        local fgDecals = {}
        local bgDecals = {}
        if gen and gen.generateDecals and roomData.matrix then
            local decals = gen.generateDecals(roomData.matrix, nil, roomX, roomY, roomStyle, 0.1)
            fgDecals = decals.fgdecals or {}
            bgDecals = decals.bgdecals or {}
        end

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
            decalsFg = fgDecals,
            decalsBg = bgDecals,
            foregrounds = styleFg,
            backgrounds = styleBg,
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

        -- Add entities with room-relative positions and unique _id values
        if roomData.entities then
            for _, entity in ipairs(roomData.entities) do
                local e = {
                    _name = entity.name or entity._name,
                    x = (entity.x or 0) + roomX,
                    y = (entity.y or 0) + roomY,
                    width = entity.width,
                    height = entity.height,
                    _id = nextEntityId,   -- unique ID avoids Lönn crash on room click
                }
                nextEntityId = nextEntityId + 1
                for k, v in pairs(entity) do
                    if k ~= "name" and k ~= "_name" and k ~= "x" and k ~= "y"
                       and k ~= "width" and k ~= "height" and k ~= "_id"
                       and k ~= "id"   -- skip PCG-internal 'id', use _id above
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

    -- Invalidate render cache
    celesteRender.invalidateRoomCache()

    -- Create undo snapshot
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
end  -- _prerunImpl

return script
