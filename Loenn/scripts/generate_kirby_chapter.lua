--[[
    Kirby Chapter Generator — PCG Level Generator
    ================================================
    Generates a full chapter map following Sakurai's design philosophy:
      1. Intro room         — safe arrival, NPC + AbilityStar
      2. Practice room      — signature enemy on flat ground
      3-5. Progression      — escalating enemy variety & density
      6. Mid-boss room      — optional mini-boss encounter
      7-8. Test rooms       — require ability mastery + hazards
      9. Boss approach      — corridor with last-chance AbilityStar
      10. Boss arena        — end-of-chapter boss fight

    Run from the Lönn Scripts menu.

    Output: Rooms appended to the current map.
]]

local state = require("loaded_state")
local utils = require("utils")
local snapshot = require("structs.snapshot")
local tilesStruct = require("structs.tiles")
local celesteRender = require("celeste_render")
local matrix = require("utils.matrix")
local mods = require("mods")

local chapterDesign = mods.requireFromPlugin("libraries.kirby_chapter_design")

local script = {}

script.name = "generateKirbyChapter"
script.displayName = "PCG: Generate Kirby Chapter Rooms"
script.tooltip = "Generates a Sakurai-style Kirby chapter (intro → practice → progression → midboss → test → boss)"
script.parameters = {
    chapterNumber = 1,
}

--------------------------------------------------------------------------------
-- Constants
--------------------------------------------------------------------------------
local TILE_AIR   = "0"
local TILE_SOLID = "1"

-- Room size presets (tiles)
local ROOM_SIZES = {
    intro      = { w = 50, h = 28 },
    practice   = { w = 55, h = 28 },
    progression = { w = 60, h = 34 },
    midboss    = { w = 60, h = 34 },
    test       = { w = 64, h = 38 },
    approach   = { w = 70, h = 24 },
    boss       = { w = 80, h = 45 },
}

local GRID_COLS = 3  -- rooms per row in the layout grid

--------------------------------------------------------------------------------
-- Matrix Helpers
--------------------------------------------------------------------------------

local function clamp(v, lo, hi) return math.max(lo, math.min(hi, v)) end

--- Fill a matrix with solid borders, floor, and open interior.
local function buildBaseRoom(w, h, tileset)
    tileset = tileset or TILE_SOLID
    local m = matrix.filled(tileset, w, h)

    -- Carve interior
    for y = 3, h - 4 do
        for x = 3, w - 2 do
            m:set(x, y, TILE_AIR)
        end
    end

    -- Floor at h - 3 (two tiles thick)
    for x = 3, w - 2 do
        m:set(x, h - 3, tileset)
        m:set(x, h - 2, tileset)
    end

    return m
end

--- Carve a doorway opening on the right edge for room transitions.
local function carveRightExit(m, w, h)
    local cy = math.floor(h / 2)
    for dy = -2, 2 do
        local y = clamp(cy + dy, 3, h - 4)
        m:set(w - 1, y, TILE_AIR)
        m:set(w, y, TILE_AIR)
    end
end

--- Carve a doorway opening on the left edge for room transitions.
local function carveLeftExit(m, w, h)
    local cy = math.floor(h / 2)
    for dy = -2, 2 do
        local y = clamp(cy + dy, 3, h - 4)
        m:set(1, y, TILE_AIR)
        m:set(2, y, TILE_AIR)
    end
end

--- Add floating platforms at specified positions.
local function placePlatform(m, x1, x2, y, tileset)
    tileset = tileset or TILE_SOLID
    for x = x1, x2 do
        m:set(x, y, tileset)
    end
end

--- Add a pit (remove floor tiles in a range) for hazard rooms.
local function carvePit(m, x1, x2, floorY)
    for x = x1, x2 do
        m:set(x, floorY, TILE_AIR)
        m:set(x, floorY + 1, TILE_AIR)
    end
end

--------------------------------------------------------------------------------
-- Entity ID Generator
--------------------------------------------------------------------------------
local nextEntityId = 1
local function eid()
    local id = nextEntityId
    nextEntityId = nextEntityId + 1
    return id
end

--------------------------------------------------------------------------------
-- Room Generators
--------------------------------------------------------------------------------

--- Build the INTRO room: safe arrival with NPC and AbilityStar.
local function buildIntroRoom(flowEntry, tileset)
    local size = ROOM_SIZES.intro
    local w, h = size.w, size.h
    local m = buildBaseRoom(w, h, tileset)
    carveRightExit(m, w, h)

    local floorY = h - 4  -- Y position ON the floor (entity placement)
    local entities = {}

    -- Player spawn (left side)
    table.insert(entities, {
        _name = "player",
        id = eid(),
        x = 40,
        y = (floorY) * 8,
    })

    -- Kirby NPC (tutorial hint)
    table.insert(entities, {
        _name = "MaggyHelper/KirbyNPC",
        id = eid(),
        x = 120,
        y = (floorY) * 8,
        dialogId = "KIRBY_INTRO_ABILITY",
    })

    -- AbilityStar for the chapter's signature ability
    local ability = flowEntry.abilityStars and flowEntry.abilityStars[1]
    if ability and ability ~= "None" then
        table.insert(entities, {
            _name = "MaggyHelper/AbilityStar",
            id = eid(),
            x = 220,
            y = (floorY - 3) * 8,
            ability = ability,
        })
    end

    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        name = flowEntry.name,
        roomStyle = "normal",
    }
end

--- Build the PRACTICE room: signature enemy on safe, flat ground.
local function buildPracticeRoom(flowEntry, tileset)
    local size = ROOM_SIZES.practice
    local w, h = size.w, size.h
    local m = buildBaseRoom(w, h, tileset)
    carveLeftExit(m, w, h)
    carveRightExit(m, w, h)

    local floorY = h - 4
    local entities = {}

    -- Place signature enemies (spread across the room)
    local count = flowEntry.enemyCount or 2
    for i = 1, count do
        local enemy = flowEntry.enemies[((i - 1) % #flowEntry.enemies) + 1]
        if enemy then
            local xPos = 80 + (i - 1) * math.floor((w * 8 - 160) / math.max(count, 1))
            table.insert(entities, {
                _name = "MaggyHelper/" .. enemy.type,
                id = eid(),
                x = xPos,
                y = (floorY) * 8,
                health = enemy.health,
                moveSpeed = enemy.speed,
                canBeInhaled = true,
            })
        end
    end

    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        name = flowEntry.name,
        roomStyle = "normal",
    }
end

--- Build a PROGRESSION room: mixed enemies, some platforms, increasing challenge.
local function buildProgressionRoom(flowEntry, tileset, progressionIndex)
    local size = ROOM_SIZES.progression
    local w, h = size.w, size.h
    local m = buildBaseRoom(w, h, tileset)
    carveLeftExit(m, w, h)
    carveRightExit(m, w, h)

    local floorY = h - 4
    local entities = {}

    -- Add floating platforms based on progression index (more platforms in later rooms)
    local platCount = progressionIndex or 1
    for p = 1, platCount do
        local px = math.floor(w * (p / (platCount + 1)))
        local py = floorY - 4 - (p % 2) * 3
        placePlatform(m, clamp(px - 3, 3, w - 2), clamp(px + 3, 3, w - 2), py, tileset)
    end

    -- Place enemies
    local count = flowEntry.enemyCount or 3
    for i = 1, math.min(count, #flowEntry.enemies * 2) do
        local enemy = flowEntry.enemies[((i - 1) % #flowEntry.enemies) + 1]
        if enemy then
            local xPos = 60 + (i - 1) * math.floor((w * 8 - 120) / math.max(count, 1))
            local yInTiles = floorY
            -- Some enemies placed on platforms for variety
            if i > count / 2 and platCount > 0 then
                yInTiles = floorY - 4 - (i % 2) * 3
            end
            table.insert(entities, {
                _name = "MaggyHelper/" .. enemy.type,
                id = eid(),
                x = xPos,
                y = yInTiles * 8,
                health = enemy.health,
                moveSpeed = enemy.speed,
                canBeInhaled = enemy.power ~= "None",
            })
        end
    end

    -- AbilityStar for secondary ability in last progression room
    if flowEntry.abilityStars and #flowEntry.abilityStars > 0 then
        local ability = flowEntry.abilityStars[1]
        if ability and ability ~= "None" then
            table.insert(entities, {
                _name = "MaggyHelper/AbilityStar",
                id = eid(),
                x = math.floor(w / 2) * 8,
                y = (floorY - 6) * 8,
                ability = ability,
            })
        end
    end

    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        name = flowEntry.name,
        roomStyle = "normal",
    }
end

--- Build the MID-BOSS room: locked arena with a mini-boss.
local function buildMidBossRoom(flowEntry, tileset)
    local size = ROOM_SIZES.midboss
    local w, h = size.w, size.h
    local m = buildBaseRoom(w, h, tileset)
    carveLeftExit(m, w, h)
    carveRightExit(m, w, h)

    -- Add side platforms for combat variety
    placePlatform(m, 8, 15, h - 10, tileset)
    placePlatform(m, w - 15, w - 8, h - 10, tileset)

    local floorY = h - 4
    local entities = {}

    -- Mid-boss entity
    local mb = flowEntry.midBoss
    if mb then
        table.insert(entities, {
            _name = mb.type,
            id = eid(),
            x = math.floor(w / 2) * 8,
            y = floorY * 8,
            health = mb.health,
        })
    end

    -- Boss arena trigger to lock the room
    table.insert(entities, {
        _name = "MaggyHelper/BossArenaTrigger",
        _type = "trigger",
        id = eid(),
        x = 0,
        y = 0,
        width = w * 8,
        height = h * 8,
        bossName = mb and mb.variant or "MidBoss",
        showHealthBar = true,
        createHealthUI = true,
        bossEntityType = mb and mb.type or "",
    })

    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        name = flowEntry.name,
        roomStyle = "normal",
    }
end

--- Build a TEST room: full enemy variety + hazards, requires ability mastery.
local function buildTestRoom(flowEntry, tileset, testIndex)
    local size = ROOM_SIZES.test
    local w, h = size.w, size.h
    local m = buildBaseRoom(w, h, tileset)
    carveLeftExit(m, w, h)
    carveRightExit(m, w, h)

    local floorY = h - 4
    local entities = {}

    -- Complex platform layout
    local platPositions = {
        { x1 = 10, x2 = 18, y = floorY - 5 },
        { x1 = 22, x2 = 30, y = floorY - 9 },
        { x1 = 34, x2 = 42, y = floorY - 5 },
        { x1 = 46, x2 = 54, y = floorY - 9 },
        { x1 = w - 18, x2 = w - 10, y = floorY - 5 },
    }
    for _, plat in ipairs(platPositions) do
        placePlatform(m, plat.x1, plat.x2, plat.y, tileset)
    end

    -- Add pits in test room 2 for extra danger
    if testIndex and testIndex >= 2 then
        carvePit(m, 20, 26, floorY)
        carvePit(m, 40, 46, floorY)
    end

    -- Place all enemies
    local count = flowEntry.enemyCount or 5
    for i = 1, math.min(count, 8) do
        local enemy = flowEntry.enemies[((i - 1) % #flowEntry.enemies) + 1]
        if enemy then
            local xPos = 64 + (i - 1) * math.floor((w * 8 - 128) / math.max(count, 1))
            table.insert(entities, {
                _name = "MaggyHelper/" .. enemy.type,
                id = eid(),
                x = xPos,
                y = floorY * 8,
                health = enemy.health,
                moveSpeed = enemy.speed,
                canBeInhaled = enemy.power ~= "None",
            })
        end
    end

    -- Hazard entities (Gordos, damage triggers)
    if flowEntry.hazards then
        for i, hazard in ipairs(flowEntry.hazards) do
            if hazard.type == "Gordo" then
                local platIdx = ((i - 1) % #platPositions) + 1
                local plat = platPositions[platIdx]
                table.insert(entities, {
                    _name = "MaggyHelper/Gordo",
                    id = eid(),
                    x = math.floor((plat.x1 + plat.x2) / 2) * 8,
                    y = (plat.y - 3) * 8,
                })
            end
        end
    end

    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        name = flowEntry.name,
        roomStyle = "normal",
    }
end

--- Build the BOSS APPROACH corridor: tension-building walk with last-chance ability star.
local function buildApproachRoom(flowEntry, tileset)
    local size = ROOM_SIZES.approach
    local w, h = size.w, size.h
    local m = buildBaseRoom(w, h, tileset)
    carveLeftExit(m, w, h)
    carveRightExit(m, w, h)

    -- Long flat corridor — no platforms, few enemies
    local floorY = h - 4
    local entities = {}

    -- Sparse fodder enemies
    local count = flowEntry.enemyCount or 2
    for i = 1, count do
        local enemy = flowEntry.enemies[((i - 1) % math.max(#flowEntry.enemies, 1)) + 1]
        if enemy then
            local xPos = 80 + (i - 1) * math.floor((w * 8 - 160) / math.max(count, 1))
            table.insert(entities, {
                _name = "MaggyHelper/" .. enemy.type,
                id = eid(),
                x = xPos,
                y = floorY * 8,
                health = enemy.health,
                moveSpeed = enemy.speed,
                canBeInhaled = true,
            })
        end
    end

    -- Last-chance AbilityStar before the boss
    if flowEntry.abilityStars and #flowEntry.abilityStars > 0 then
        local ability = flowEntry.abilityStars[1]
        if ability and ability ~= "None" then
            table.insert(entities, {
                _name = "MaggyHelper/AbilityStar",
                id = eid(),
                x = math.floor(w * 0.75) * 8,
                y = (floorY - 3) * 8,
                ability = ability,
            })
        end
    end

    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        name = flowEntry.name,
        roomStyle = "normal",
    }
end

--- Build the BOSS ARENA: large room with boss entity and arena trigger.
local function buildBossRoom(flowEntry, chapterData, tileset)
    local size = ROOM_SIZES.boss
    local w, h = size.w, size.h
    local m = buildBaseRoom(w, h, tileset)
    carveLeftExit(m, w, h)

    -- Boss arena has wide open interior + elevated side platforms
    placePlatform(m, 6, 16, h - 12, tileset)
    placePlatform(m, w - 16, w - 6, h - 12, tileset)
    placePlatform(m, math.floor(w/2) - 5, math.floor(w/2) + 5, h - 18, tileset)

    local floorY = h - 4
    local entities = {}

    -- Boss entity
    local boss = flowEntry.boss or chapterData.boss
    if boss then
        table.insert(entities, {
            _name = boss.type,
            id = eid(),
            x = math.floor(w * 0.65) * 8,
            y = floorY * 8,
            health = boss.health,
            maxHealth = boss.health,
        })
    end

    -- Boss arena trigger (covers entire room)
    table.insert(entities, {
        _name = "MaggyHelper/BossArenaTrigger",
        _type = "trigger",
        id = eid(),
        x = 0,
        y = 0,
        width = w * 8,
        height = h * 8,
        bossName = chapterData.name .. " Boss",
        showHealthBar = true,
        createHealthUI = true,
        bossEntityType = boss and boss.type or "",
    })

    -- Boss fight trigger
    table.insert(entities, {
        _name = "MaggyHelper/BossFightTrigger",
        _type = "trigger",
        id = eid(),
        x = 24,
        y = 0,
        width = (w - 6) * 8,
        height = h * 8,
        bossType = "KirbyBoss",
        lockRoom = true,
        playMusic = true,
        bossMusic = "event:/music/lvl9/main",
    })

    -- Kirby mode trigger (ensure player is in Kirby mode for the fight)
    table.insert(entities, {
        _name = "MaggyHelper/Kirby_Mode_Toggle_Trigger",
        _type = "trigger",
        id = eid(),
        x = 0,
        y = 0,
        width = 24,
        height = h * 8,
        activationMode = "OnEnter",
        transformEffect = "Instant",
        triggerState = "Enable",
        oneUse = true,
        respectSettings = true,
        flagRequired = "",
        flagToSet = "",
        silentMode = false,
        initialPower = chapterData.signatureAbility or "None",
        effectDuration = 1.0,
        particleColor = "FF69B4",
        particleCount = 15,
        screenShake = false,
        shakeIntensity = 0.0,
        transformSound = "",
        playSound = true,
    })

    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        name = flowEntry.name,
        roomStyle = "normal",
    }
end

--------------------------------------------------------------------------------
-- Room Dispatcher
--------------------------------------------------------------------------------

--- Route a room-flow entry to the correct builder.
local function buildRoomFromFlow(flowEntry, chapterData, tileset, extraIndex)
    local t = flowEntry.type
    if t == "intro" then
        return buildIntroRoom(flowEntry, tileset)
    elseif t == "practice" then
        return buildPracticeRoom(flowEntry, tileset)
    elseif t == "progression" then
        return buildProgressionRoom(flowEntry, tileset, extraIndex)
    elseif t == "midboss" then
        return buildMidBossRoom(flowEntry, tileset)
    elseif t == "test" then
        return buildTestRoom(flowEntry, tileset, extraIndex)
    elseif t == "approach" then
        return buildApproachRoom(flowEntry, tileset)
    elseif t == "boss" then
        return buildBossRoom(flowEntry, chapterData, tileset)
    else
        -- Fallback: plain progression-type room
        return buildProgressionRoom(flowEntry, tileset, 1)
    end
end

--------------------------------------------------------------------------------
-- Main Generation
--------------------------------------------------------------------------------

local function generateChapter(chapterNum)
    local ch = chapterDesign.getChapter(chapterNum)
    if not ch then
        return nil, "Unknown chapter number: " .. tostring(chapterNum)
    end

    local flow = chapterDesign.buildRoomFlow(chapterNum)
    if not flow or #flow == 0 then
        return nil, "Empty room flow for chapter " .. chapterNum
    end

    local tileset = ch.tileset or TILE_SOLID
    local rooms = {}

    -- Track progression/test indices for the builders
    local progressionIdx = 0
    local testIdx = 0

    for i, entry in ipairs(flow) do
        local extraIdx = nil
        if entry.type == "progression" then
            progressionIdx = progressionIdx + 1
            extraIdx = progressionIdx
        elseif entry.type == "test" then
            testIdx = testIdx + 1
            extraIdx = testIdx
        end

        local roomData = buildRoomFromFlow(entry, ch, tileset, extraIdx)
        if roomData then
            -- Layout rooms in a grid: GRID_COLS per row
            local col = (i - 1) % GRID_COLS
            local row = math.floor((i - 1) / GRID_COLS)
            roomData.x = col * (roomData.width * 8 + 32)
            roomData.y = row * (roomData.height * 8 + 32)
            table.insert(rooms, roomData)
        end
    end

    return rooms
end

--------------------------------------------------------------------------------
-- Lönn Script Interface
--------------------------------------------------------------------------------

function script.prerun(room, args)
    if not state.map then
        return
    end

    local chapterNum = (args and args.chapterNumber) or 1

    local rooms, err = generateChapter(chapterNum)
    if not rooms or #rooms == 0 then
        print("[KirbyChapterGen] " .. (err or "No rooms generated"))
        return
    end

    -- Offset so new rooms don't overlap existing ones
    local maxX = 0
    for _, existingRoom in ipairs(state.map.rooms) do
        local rx = existingRoom.x + (existingRoom.width or 320)
        if rx > maxX then maxX = rx end
    end
    local offsetX = maxX + 320  -- 320px gap

    -- Save state for undo
    local prevRooms = utils.deepcopy(state.map.rooms)

    -- Add generated rooms
    local newRoomNames = {}
    for i, roomData in ipairs(rooms) do
        local roomW = roomData.width * 8
        local roomH = roomData.height * 8

        -- Build tile string from matrix
        local tileString = ""
        if roomData.matrix then
            local mat = roomData.matrix
            local mw, mh = mat:size()
            for y = 1, mh do
                local rowTiles = {}
                for x = 1, mw do
                    rowTiles[x] = mat:get(x, y, "0")
                end
                if y > 1 then tileString = tileString .. "\n" end
                tileString = tileString .. table.concat(rowTiles)
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

        -- Room name
        local roomName = roomData.name or string.format("ch%02d_%02d", chapterNum, i)
        local roomX = offsetX + (roomData.x or 0)
        local roomY = roomData.y or 0

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
            dark = false,
            underwater = false,
            space = false,
            windPattern = "None",
            color = 0,
            cameraOffsetX = 0,
            cameraOffsetY = 0,
            music = "",
            alt_music = "",
            ambience = "",
        }

        -- Add entities to room
        if roomData.entities then
            for _, entity in ipairs(roomData.entities) do
                local e = {
                    _name = entity._name,
                    x = (entity.x or 0) + roomX,
                    y = (entity.y or 0) + roomY,
                    width = entity.width,
                    height = entity.height,
                    _id = entity.id or 0,
                }
                -- Copy custom attributes
                for k, v in pairs(entity) do
                    if k ~= "_name" and k ~= "x" and k ~= "y"
                       and k ~= "width" and k ~= "height"
                       and k ~= "id" and k ~= "_id"
                       and k ~= "nodes" and k ~= "_type" then
                        e[k] = v
                    end
                end
                -- Copy nodes
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

    local ch = chapterDesign.getChapter(chapterNum)
    local chapterLabel = ch and ch.name or ("Chapter " .. chapterNum)
    print(string.format("[KirbyChapterGen] Generated %d rooms for %s (ability: %s)",
        #rooms, chapterLabel, ch and ch.signatureAbility or "None"))

    return snapshot.create(script.name, {}, backward, forward)
end

return script
