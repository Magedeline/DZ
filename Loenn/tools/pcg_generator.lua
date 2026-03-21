-- PCG Generator Tool for Lönn
-- Adds a "Procedural Content Generator" entry to the Lönn tools sidebar.
-- When selected, left-click generates a PCG room at the cursor position
-- using the lua_pcg (PCG-XSH-RR) random number generator.
--
-- Materials list = PCG presets; layer = entity/trigger target layer.

local mods = require("mods")

--------------------------------------------------------------------------------
-- Tool definition
--------------------------------------------------------------------------------
local tool = {}

tool._type      = "tool"
tool.name       = "pcg_generator"
tool.group      = "pcg_generator"
tool.image      = nil
tool.layer      = "entities"
tool.validLayers = {"entities", "triggers", "tilesFg", "tilesBg"}

-- The selected preset is stored as the "material"
tool.material   = "default"

-- RNG instance (created fresh each generation for reproducibility)
local rng = nil

--------------------------------------------------------------------------------
-- Materials = PCG presets
--------------------------------------------------------------------------------
local PRESETS = {
    "default", "open", "tight", "fullRow", "minimal",
    "resort", "temple", "reflection", "summit", "core",
    "wind", "ice", "cave", "ruins", "castle",
    "darkStars", "void", "nightmare", "farewell", "dream",
    "space", "deepSpace", "largeArea", "megaArea",
    "kirbyBoss", "kirbyFinalBoss", "playerBoss",
    "asrielGodBoss", "challenge", "puzzleRoom", "hubRoom",
    "serpentine", "dualBoss",
    "bspPlatformer", "bspSummit", "bspTemple", "bspResort",
}

function tool.getMaterials(layer)
    return PRESETS
end

function tool.setMaterial(material)
    tool.material = material
end

function tool.getMaterial()
    return tool.material
end

function tool.setLayer(layer)
    tool.layer = layer
end

function tool.getLayer()
    return tool.layer
end

--------------------------------------------------------------------------------
-- Generation logic using lua_pcg's PCG-XSH-RR RNG (accessed via pcg.lua_pcg)
--------------------------------------------------------------------------------
local function generateAtCursor(mapX, mapY)
    -- Lazy-load all heavy dependencies at call time, not at file load time
    local state         = require("loaded_state")
    local utils         = require("utils")
    local snapshot      = require("structs.snapshot")
    local tilesStruct   = require("structs.tiles")
    local celesteRender = require("celeste_render")
    local matrix        = require("utils.matrix")

    local pcg       = mods.requireFromPlugin("libraries.pcg.init")
    local seedUtils = mods.requireFromPlugin("libraries.pcg.seed_utils")

    if not state.map then return end

    local presetName = tool.material or "default"
    local preset = pcg.getPreset(presetName)
    if not preset then
        preset = pcg.getPreset("default")
    end

    -- Create a fresh PCG32 RNG for this generation (via lazy-loaded pcg.lua_pcg)
    rng = pcg.lua_pcg.pcg32.new()
    local seed = rng:next()

    -- Seed the global PCG system too (Markov trainer / generator use math.random)
    pcg.setSeed(seed)
    print(string.format("[PCG Tool] Generating preset '%s' with PCG-XSH-RR seed %u", presetName, seed))

    -- Train on existing rooms (Markov presets need training data)
    if not preset.useBsp then
        pcg.trainFromMap(presetName)
    end

    -- Pick room style from preset
    local roomStyle = preset.roomStyle or "normal"

    -- Pattern from preset
    local patternName = preset.pattern
    local patternOpts = nil
    if patternName then
        patternOpts = {
            difficulty = 0.5,
            material   = preset.borderMaterial or "1",
        }
        if preset.patternOpts then
            for k, v in pairs(preset.patternOpts) do
                if patternOpts[k] == nil then patternOpts[k] = v end
            end
        end
    end

    -- Generate rooms
    local levelData = pcg.generateArea(6, {
        entityDensity    = preset.entityDensity or 0.15,
        branchProb       = preset.branchProb or 0.3,
        playabilityCheck = preset.playabilityCheck ~= false,
        roomStyle        = roomStyle,
        levelCount       = 1,
        useBsp           = preset.useBsp,
        bspOpts          = preset.bspOpts,
        pattern          = patternName,
        patternOpts      = patternOpts,
    }, matrix)

    if not levelData or not levelData.rooms or #levelData.rooms == 0 then
        print("[PCG Tool] Generation produced no rooms")
        if rng then rng:close() end
        return
    end

    -- Snap placement to 8px grid near the cursor
    local baseX = math.floor(mapX / 8) * 8
    local baseY = math.floor(mapY / 8) * 8

    local nextEntityId = seedUtils.nextEntityId(state.map.rooms)
    local prevRooms = utils.deepcopy(state.map.rooms)

    local gen = pcg.generator

    for i, roomData in ipairs(levelData.rooms) do
        local roomW = (roomData.width  or preset.roomWidth  or 40) * 8
        local roomH = (roomData.height or preset.roomHeight or 23) * 8

        -- Build fg tile string
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

        -- Empty bg
        local bgString = ""
        local tileW = math.floor(roomW / 8)
        local tileH = math.floor(roomH / 8)
        for y = 1, tileH do
            if y > 1 then bgString = bgString .. "\n" end
            bgString = bgString .. string.rep("0", tileW)
        end

        local roomName = string.format("pcg_%02d", i)
        local roomX = baseX + (roomData.x or 0)
        local roomY = baseY + (roomData.y or 0)

        -- Stylegrounds
        local styleBg, styleFg = {}, {}
        if gen and gen.generateStylegrounds then
            local sg = gen.generateStylegrounds(roomStyle)
            styleBg = sg.bg or {}
            styleFg = sg.fg or {}
        end

        -- Decals
        local fgDecals, bgDecals = {}, {}
        if gen and gen.generateDecals and roomData.matrix then
            local decals = gen.generateDecals(roomData.matrix, nil, roomX, roomY, roomStyle, 0.1)
            fgDecals = decals.fgdecals or {}
            bgDecals = decals.bgdecals or {}
        end

        local newRoom = {
            name       = "lvl_" .. roomName,
            x          = roomX,
            y          = roomY,
            width      = roomW,
            height     = roomH,
            tilesFg    = tilesStruct.decode({ innerText = tileString }),
            tilesBg    = tilesStruct.decode({ innerText = bgString }),
            entities   = {},
            triggers   = {},
            decalsFg   = fgDecals,
            decalsBg   = bgDecals,
            foregrounds = styleFg,
            backgrounds = styleBg,
            musicLayer1 = true,
            musicLayer2 = true,
            musicLayer3 = true,
            musicLayer4 = true,
            dark        = ({deepSpace=true,darkStars=true,void=true,nightmare=true,cave=true})[roomStyle] or false,
            underwater  = false,
            space       = ({space=true,deepSpace=true,darkStars=true,void=true,farewell=true})[roomStyle] or false,
            windPattern = ({wind="Left"})[roomStyle] or "None",
            color       = 0,
            cameraOffsetX = 0,
            cameraOffsetY = 0,
            music       = "",
            alt_music   = "",
            ambience    = "",
        }

        -- Place entities with unique _id values
        if roomData.entities then
            for _, entity in ipairs(roomData.entities) do
                local e = {
                    _name  = entity.name or entity._name,
                    x      = (entity.x or 0) + roomX,
                    y      = (entity.y or 0) + roomY,
                    width  = entity.width,
                    height = entity.height,
                    _id    = nextEntityId,
                }
                nextEntityId = nextEntityId + 1
                for k, v in pairs(entity) do
                    if k ~= "name" and k ~= "_name" and k ~= "x" and k ~= "y"
                       and k ~= "width" and k ~= "height" and k ~= "_id"
                       and k ~= "id" and k ~= "nodes" then
                        e[k] = v
                    end
                end
                if entity.nodes then
                    e.nodes = {}
                    for _, node in ipairs(entity.nodes) do
                        table.insert(e.nodes, { x = node.x + roomX, y = node.y + roomY })
                    end
                end
                if entity._type == "trigger" then
                    e._type = nil
                    table.insert(newRoom.triggers, e)
                else
                    table.insert(newRoom.entities, e)
                end
            end
        end

        table.insert(state.map.rooms, newRoom)
    end

    celesteRender.invalidateRoomCache()

    -- Undo/redo snapshot
    local currentRooms = utils.deepcopy(state.map.rooms)

    local function backward()
        state.map.rooms = prevRooms
        celesteRender.invalidateRoomCache()
    end

    local function forward()
        state.map.rooms = currentRooms
        celesteRender.invalidateRoomCache()
    end

    if rng then rng:close() end
    return snapshot.create("pcg_generator", {}, backward, forward)
end

--------------------------------------------------------------------------------
-- Tool callbacks
--------------------------------------------------------------------------------
function tool.mouseclicked(x, y, button, istouch, presses)
    if button ~= 1 then return end

    local state = require("loaded_state")
    if not state.map then return end

    -- Convert screen coordinates to map coordinates
    local viewportHandler = require("viewport_handler")
    local snapshot = require("structs.snapshot")
    local mapX, mapY = viewportHandler.getMapCoordinates(x, y)
    local snap = generateAtCursor(mapX, mapY)
    if snap then
        snapshot.addSnapshot(snap)
    end
end

return tool

return tool
