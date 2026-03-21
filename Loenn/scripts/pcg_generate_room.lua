-- PCG Generate Room Script for LoennScripts
-- Replaces the current room's tiles and entities with PCG-generated content.
-- Trains on the current map's rooms, then generates new content.

local state = require("loaded_state")
local utils = require("utils")
local snapshot = require("structs.snapshot")
local tilesStruct = require("structs.tiles")
local celesteRender = require("celeste_render")
local matrix = require("utils.matrix")
local mods = require("mods")

local pcg = mods.requireFromPlugin("libraries.pcg.init")
local seedUtils = mods.requireFromPlugin("libraries.pcg.seed_utils")

local script = {}

script.name = "pcgGenerateRoom"
script.displayName = "PCG: Generate Room"
script.tooltip = "Replaces the current room with PCG-generated tiles and entities.\nTrains on the current map, then generates new content using Markov Chains."

script.parameters = {
    preset = "default",
    roomStyle = "normal",
    pattern = "(none)",
    autoSeed = true,
    seed = 0,
    entityDensity = 0.15,
    difficulty = 0.5,
    playabilityCheck = true,
    cleanupPasses = 2,
    maxRetries = 10,
    kirbyBossType = -1,
    bossTier = 3,
}

script.fieldOrder = {
    "preset", "roomStyle", "pattern", "autoSeed", "seed", "entityDensity",
    "difficulty", "playabilityCheck", "cleanupPasses", "maxRetries",
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
            "space", "deepSpace",
            "kirbyBoss", "kirbyFinalBoss", "playerBoss",
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
            "normalBossArena", "multiBossArena",
            "crossroads", "starHub",
        },
    },
    seed = {
        fieldType = "integer",
    },
    cleanupPasses = {
        fieldType = "integer",
    },
    maxRetries = {
        fieldType = "integer",
    },
    difficulty = {
        fieldType = "number",
    },
    kirbyBossType = {
        fieldType = "integer",
    },
    bossTier = {
        fieldType = "integer",
    },
}

script.tooltips = {
    preset = "MdMC preset controlling room size and generation parameters (includes pattern presets like kirbyBoss, challenge, etc.)",
    roomStyle = "Room style: normal, resort, temple, reflection, summit, core, wind, ice, cave, ruins, castle, darkStars, void, nightmare, farewell, dream, space, deepSpace",
    pattern = "Room pattern to apply: (none) for Markov-only, or pick a structured layout (overrides preset pattern)",
    autoSeed = "Auto-generate a unique seed every run (ignores seed value)",
    seed = "Manual seed — only used when autoSeed is unchecked (0 = use current time)",
    entityDensity = "Probability of placing entities (0.0 - 1.0)",
    difficulty = "Difficulty 0.0 (easy) → 1.0 (hard). Affects pattern entity placement (boss HP, hazard density)",
    playabilityCheck = "Verify the room is traversable using A* pathfinding",
    cleanupPasses = "Number of tile cleanup passes (remove floating tiles)",
    maxRetries = "Max attempts to generate a playable room",
    kirbyBossType = "Kirby mid-boss type 0-9 (-1 = random). Only used by Kirby boss patterns.",
    bossTier = "Normal player boss tier 1-5. Only used by player boss patterns.",
}

function script.run(room, args)
    -- Train on the current map if not already trained
    if not pcg.isTrained() then
        local seed = seedUtils.resolveSeed(args)
        pcg.setSeed(seed)

        local ok, stats = pcg.trainFromMap(args.preset or "default")
        if not ok then
            -- Couldn't train — not enough rooms
            return
        end
    end

    local preset = pcg.getPreset(args.preset or "default")
    if not preset then return end

    -- Determine exits based on room connections
    -- Simple heuristic: check all 4 sides
    local w = room.width or (preset.roomWidth * 8)
    local h = room.height or (preset.roomHeight * 8)
    local tileW = math.floor(w / 8)
    local tileH = math.floor(h / 8)

    local exits = { left = false, right = false, top = false, bottom = false }

    -- Check existing room edges for openings in current tile data
    if room.tilesFg and room.tilesFg.matrix then
        local mat = room.tilesFg.matrix
        local mw, mh = mat:size()
        local mid = math.floor(mh / 2)

        -- Left edge
        local leftOpen = 0
        for y = mid - 2, mid + 2 do
            if y >= 1 and y <= mh then
                local t = mat:get(1, y, "0")
                if t == "0" then leftOpen = leftOpen + 1 end
            end
        end
        if leftOpen >= 3 then exits.left = true end

        -- Right edge
        local rightOpen = 0
        for y = mid - 2, mid + 2 do
            if y >= 1 and y <= mh then
                local t = mat:get(mw, y, "0")
                if t == "0" then rightOpen = rightOpen + 1 end
            end
        end
        if rightOpen >= 3 then exits.right = true end

        -- Top edge
        local midX = math.floor(mw / 2)
        local topOpen = 0
        for x = midX - 2, midX + 2 do
            if x >= 1 and x <= mw then
                local t = mat:get(x, 1, "0")
                if t == "0" then topOpen = topOpen + 1 end
            end
        end
        if topOpen >= 3 then exits.top = true end

        -- Bottom edge
        local botOpen = 0
        for x = midX - 2, midX + 2 do
            if x >= 1 and x <= mw then
                local t = mat:get(x, mh, "0")
                if t == "0" then botOpen = botOpen + 1 end
            end
        end
        if botOpen >= 3 then exits.bottom = true end
    end

    -- Default: at least left+right exits
    if not exits.left and not exits.right and not exits.top and not exits.bottom then
        exits.left = true
        exits.right = true
    end

    -- Generate room content
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
        if presetData and presetData.patternOpts then
            for k, v in pairs(presetData.patternOpts) do
                if patternOpts[k] == nil then patternOpts[k] = v end
            end
        end
    end

    local roomData = pcg.generateRoom(exits, {
        roomWidth = tileW,
        roomHeight = tileH,
        entityDensity = args.entityDensity or preset.entityDensity,
        cleanupPasses = args.cleanupPasses or preset.cleanupPasses,
        playabilityCheck = args.playabilityCheck,
        maxRetries = args.maxRetries or preset.maxRetries,
        roomStyle = roomStyle,
        spaceErodePercent = presetData and presetData.spaceErodePercent,
        noBorders = presetData and presetData.noBorders,
        trimmedBorders = presetData and presetData.trimmedBorders,
        useBsp = presetData and presetData.useBsp,
        bspOpts = presetData and presetData.bspOpts,
        pattern = patternName,
        patternOpts = patternOpts,
    }, matrix)

    if not roomData then return end

    -- Apply generated tiles to the room
    local tileString = ""
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

    room.tilesFg = tilesStruct.decode({ innerText = tileString })

    -- Set room flags based on style
    local darkStyles = {deepSpace=true, darkStars=true, void=true, nightmare=true, cave=true}
    local spaceStyles = {space=true, deepSpace=true, darkStars=true, void=true, farewell=true}
    local windPatterns = {wind="Left"}
    if spaceStyles[roomStyle] then room.space = true end
    if darkStyles[roomStyle] then room.dark = true end
    if windPatterns[roomStyle] then room.windPattern = windPatterns[roomStyle] end

    -- Clear and replace entities
    room.entities = {}
    room.triggers = room.triggers or {}
    -- Clear existing triggers added by patterns
    local oldTriggers = {}
    for _, t in ipairs(room.triggers) do table.insert(oldTriggers, t) end

    -- Compute a starting entity ID that won't collide with other rooms
    local nextEntityId = seedUtils.nextEntityId(state.map.rooms or {})

    for _, entity in ipairs(roomData.entities) do
        local e = {
            _name = entity.name or entity._name,
            x = (entity.x or 0) + room.x,
            y = (entity.y or 0) + room.y,
            width = entity.width,
            height = entity.height,
            _id = nextEntityId,   -- unique ID prevents Lönn crash on click
        }
        nextEntityId = nextEntityId + 1
        -- Copy extra attributes
        for k, v in pairs(entity) do
            if k ~= "name" and k ~= "_name" and k ~= "x" and k ~= "y"
               and k ~= "width" and k ~= "height" and k ~= "_id"
               and k ~= "id"   -- skip PCG-internal 'id'
               and k ~= "nodes" then
                e[k] = v
            end
        end
        if entity.nodes then
            e.nodes = {}
            for _, node in ipairs(entity.nodes) do
                table.insert(e.nodes, {
                    x = node.x + room.x,
                    y = node.y + room.y,
                })
            end
        end
        -- Separate triggers from entities
        if entity._type == "trigger" then
            e._type = nil
            table.insert(room.triggers, e)
        else
            table.insert(room.entities, e)
        end
    end

    -- Apply stylegrounds for the selected room style
    local gen = pcg and pcg.generator
    if gen and gen.generateStylegrounds then
        local sg = gen.generateStylegrounds(presetData and presetData.roomStyle or roomStyle)
        if sg and (sg.bg and #sg.bg > 0 or sg.fg and #sg.fg > 0) then
            room.backgrounds = sg.bg or {}
            room.foregrounds = sg.fg or {}
        end
    end

    -- Apply decals for the selected room style
    if gen and gen.generateDecals and roomData.matrix then
        local decals = gen.generateDecals(roomData.matrix, exits, room.x, room.y, roomStyle, 0.1)
        if decals then
            room.decalsFg = decals.fgdecals or room.decalsFg or {}
            room.decalsBg = decals.bgdecals or room.decalsBg or {}
        end
    end

    -- Invalidate render cache so Lönn redraws the room
    celesteRender.invalidateRoomCache(room)
end

return script
