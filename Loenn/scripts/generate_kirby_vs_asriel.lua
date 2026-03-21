--[[
    Kirby vs Asriel God Boss Map Generator — PCG Boss Level (60 Rooms)
    =========================================================
    Generates a multi-section boss battle map featuring:
    - Kirby player vs Asriel God Boss encounters
    - Approach corridor sections
    - Multi-phase boss arenas (60 rooms total)
    - Dynamic platform layouts
    
    Run from Lönn Scripts menu.
    
    Output:
        Maps/Maggy/PCG/kirby_vs_asriel.bin   (SID: Maggy/PCG/kirby_vs_asriel)
]]

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

local script = {}

script.name = "generateKirbyVsAsriel"
script.displayName = "PCG: Generate Kirby vs Asriel (60 Rooms)"
script.tooltip = "Generates a 60-room Kirby vs Asriel God Boss battle map"

--------------------------------------------------------------------------------
-- Matrix Library (uses Lönn's matrix)
--------------------------------------------------------------------------------
local matrixLib = {}
function matrixLib.filled(val, w, h)
    local m = matrix.filled(val, w, h)
    return m
end

--------------------------------------------------------------------------------
-- Constants
--------------------------------------------------------------------------------
local TILE_AIR   = "0"
local TILE_SOLID = "N"  -- Hopes and Dreams tileset for dramatic boss fight
local TILE_VOID  = "O"  -- Void tileset for phase transitions

-- Room dimensions
local APPROACH_W    = 50    -- tiles
local APPROACH_H    = 28    -- tiles
local ARENA_W       = 72    -- tiles (larger boss arena)
local ARENA_H       = 40    -- tiles
local PHASE2_W      = 80    -- tiles (even larger for phase 2)
local PHASE2_H      = 45    -- tiles
local FINAL_W       = 88    -- tiles (final phase)
local FINAL_H       = 50    -- tiles

local ROOM_SPACING  = 16    -- pixels between rooms

local nextEntityId = 1
local function eid()
    local id = nextEntityId
    nextEntityId = nextEntityId + 1
    return id
end

--------------------------------------------------------------------------------
-- Room Pattern Helpers
--------------------------------------------------------------------------------

local function clamp(v, lo, hi) return math.max(lo, math.min(hi, v)) end

--- Carve a horizontal corridor
local function carveHLine(matrix, x1, x2, y, halfH, w, h)
    local lo, hi = math.min(x1, x2), math.max(x1, x2)
    for x = clamp(lo, 2, w - 1), clamp(hi, 2, w - 1) do
        for dy = -halfH, halfH do
            local yy = clamp(y + dy, 2, h - 1)
            matrix:set(x, yy, TILE_AIR)
        end
    end
end

--- Carve a vertical corridor
local function carveVLine(matrix, y1, y2, x, halfW, w, h)
    local lo, hi = math.min(y1, y2), math.max(y1, y2)
    for y = clamp(lo, 2, h - 1), clamp(hi, 2, h - 1) do
        for dx = -halfW, halfW do
            local xx = clamp(x + dx, 2, w - 1)
            matrix:set(xx, y, TILE_AIR)
        end
    end
end

--- Carve a rectangular area
local function carveRect(matrix, cx, cy, rw, rh, w, h)
    local x1 = clamp(cx - math.floor(rw / 2), 2, w - 1)
    local x2 = clamp(cx + math.floor(rw / 2), 2, w - 1)
    local y1 = clamp(cy - math.floor(rh / 2), 2, h - 1)
    local y2 = clamp(cy + math.floor(rh / 2), 2, h - 1)
    for y = y1, y2 do
        for x = x1, x2 do
            matrix:set(x, y, TILE_AIR)
        end
    end
end

--- Place a platform (solid line)
local function placePlatform(matrix, x1, x2, y, material, w, h)
    material = material or TILE_SOLID
    for x = clamp(x1, 2, w - 1), clamp(x2, 2, w - 1) do
        if y >= 1 and y <= h then
            matrix:set(x, y, material)
        end
    end
end

--- Fill borders with solid tiles (except exits)
local function fillBorders(matrix, material, w, h, exitSide)
    material = material or TILE_SOLID
    for x = 1, w do
        matrix:set(x, 1, material)
        matrix:set(x, 2, material)
        if exitSide ~= "bottom" then
            matrix:set(x, h, material)
            matrix:set(x, h - 1, material)
        end
    end
    for y = 1, h do
        if exitSide ~= "left" or (y < h/2 - 3 or y > h/2 + 3) then
            matrix:set(1, y, material)
            matrix:set(2, y, material)
        end
        if exitSide ~= "right" or (y < h/2 - 3 or y > h/2 + 3) then
            matrix:set(w, y, material)
            matrix:set(w - 1, y, material)
        end
    end
end

--------------------------------------------------------------------------------
-- Room Generators
--------------------------------------------------------------------------------

-- Room dimension variations for variety
local ROOM_SIZES = {
    tiny      = { w = 30, h = 18 },
    small     = { w = 40, h = 24 },
    medium    = { w = 50, h = 30 },
    large     = { w = 64, h = 38 },
    huge      = { w = 80, h = 45 },
    massive   = { w = 96, h = 54 },
    corridor  = { w = 60, h = 20 },
    vertical  = { w = 30, h = 50 },
    tower     = { w = 35, h = 70 },
}

-- Attack sequence pools per phase
local ATTACK_SEQUENCES = {
    phase1 = {
        "Shoot,Beam",
        "Shoot,StarstormRain",
        "Beam,BladeThrower",
        "Shoot,Beam,StarstormRain",
        "BladeThrower,Beam,Shoot",
    },
    phase2 = {
        "BiggerBeam,RainbowBlackhole",
        "ChaosBlaster,LightningStorm",
        "HyperGoner,BiggerBeam",
        "RainbowBlackhole,ChaosBlaster,LightningStorm",
        "BiggerBeam,HyperGoner,ChaosBlaster",
    },
    phase3 = {
        "HyperGoner,EternalChaos",
        "GalacticSaber,DimensionalRift",
        "RainbowInferno,CelestialSpears",
        "TimewarpVortex,PrismBurst,SoulResonance",
        "EternalChaos,GalacticSaber,DimensionalRift,RainbowInferno",
        "HyperGoner,EternalChaos,GalacticSaber,DimensionalRift,RainbowInferno,CelestialSpears,TimewarpVortex,PrismBurst,SoulResonance",
    },
    miniboss = {
        "Shoot,Shoot,Beam",
        "StarstormRain",
        "BladeThrower,Shoot",
    },
}

--- Build the approach corridor room (pre-boss tension)
local function buildApproachRoom(index, globalX, globalY)
    local w, h = APPROACH_W, APPROACH_H
    local m = matrixLib.filled(TILE_SOLID, w, h)
    
    -- Clear main corridor
    local corridorH = 3
    local cy = math.floor(h / 2)
    carveHLine(m, 3, w - 2, cy, corridorH, w, h)
    
    -- Floor below corridor
    placePlatform(m, 3, w - 2, cy + corridorH + 1, TILE_SOLID, w, h)
    
    -- Add some dramatic pillars
    local pillarCount = 4
    for i = 1, pillarCount do
        local px = math.floor(5 + (w - 10) * i / (pillarCount + 1))
        local pillarHeight = 3 + (i % 2)
        for py = cy - corridorH - pillarHeight, cy - corridorH do
            m:set(px, clamp(py, 2, h - 1), TILE_SOLID)
            m:set(px + 1, clamp(py, 2, h - 1), TILE_SOLID)
        end
    end
    
    -- Clear left entry and right exit
    local exitSize = 5
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        local y = exitStart + dy
        m:set(1, y, TILE_AIR); m:set(2, y, TILE_AIR)
        m:set(w, y, TILE_AIR); m:set(w - 1, y, TILE_AIR)
    end
    
    -- Entities
    local entities = {}
    local px8 = globalX
    local py8 = globalY
    
    -- Player spawn
    table.insert(entities, {
        _name = "player", id = eid(),
        x = 32, y = cy * 8
    })
    
    -- Kirby spawn point
    table.insert(entities, {
        _name = "MaggyHelper/KirbySpawnPoint", id = eid(),
        x = 48, y = cy * 8,
        spawnAsKirby = true,
        startingAbility = "Sword"
    })
    
    -- Atmosphere refills
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = math.floor(w * 8 * 0.3), y = (cy - 1) * 8,
        oneUse = false, twoDash = false
    })
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = math.floor(w * 8 * 0.7), y = (cy - 1) * 8,
        oneUse = false, twoDash = true
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_approach", index),
        roomStyle = "dream"
    }
end

--- Build the first Asriel God Boss arena (Phase 1)
local function buildPhase1Arena(index, globalX, globalY)
    local w, h = ARENA_W, ARENA_H
    local m = matrixLib.filled(TILE_SOLID, w, h)
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    
    -- Clear main arena space
    carveRect(m, cx, cy, w - 10, h - 10, w, h)
    
    -- Main floor
    local floorY = h - 5
    placePlatform(m, 5, w - 4, floorY, TILE_SOLID, w, h)
    
    -- Side platforms (for dodging)
    local platY1 = math.floor(h * 0.35)
    placePlatform(m, 6, math.floor(w * 0.25), platY1, TILE_SOLID, w, h)
    placePlatform(m, math.floor(w * 0.75), w - 5, platY1, TILE_SOLID, w, h)
    
    -- Mid platforms
    local platY2 = math.floor(h * 0.55)
    placePlatform(m, math.floor(w * 0.3), math.floor(w * 0.45), platY2, TILE_SOLID, w, h)
    placePlatform(m, math.floor(w * 0.55), math.floor(w * 0.7), platY2, TILE_SOLID, w, h)
    
    -- Center floating platform (boss perch)
    local centerPlatY = math.floor(h * 0.25)
    placePlatform(m, cx - 6, cx + 6, centerPlatY, TILE_SOLID, w, h)
    
    -- Left entry
    local exitSize = 5
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
    end
    
    fillBorders(m, TILE_SOLID, w, h, "left")
    
    -- Entities
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- No player spawn here (arrives from previous room)
    
    -- Asriel God Boss
    local bossX = cx * 8
    local bossY = (centerPlatY - 2) * 8
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBoss", id = eid(),
        x = bossX, y = bossY,
        patternIndex = 0,
        cameraPastY = 120.0,
        dialog = true,
        startHit = false,
        cameraLockY = true,
        health = 300,
        maxHealth = 300,
        attackSequence = "Shoot,Beam,StarstormRain"
    })
    
    -- Boss start trigger
    table.insert(entities, {
        _name = "MaggyHelper/AsrielStartHitTrigger", id = eid(),
        x = 0, y = 0,
        width = w8, height = h8,
        _type = "trigger"
    })
    
    -- Boss intro cutscene trigger
    table.insert(entities, {
        _name = "MaggyHelper/EventTrigger", id = eid(),
        x = 48, y = 0,
        width = 64, height = h8,
        eventName = "ch20_asriel_god_boss_identity_reveal",
        _type = "trigger"
    })
    
    -- Refills (strategic placement)
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = 48, y = (floorY - 1) * 8,
        oneUse = false, twoDash = true
    })
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = w8 - 56, y = (floorY - 1) * 8,
        oneUse = false, twoDash = true
    })
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = cx * 8, y = (platY2 - 1) * 8,
        oneUse = false, twoDash = true
    })
    
    -- Springs for mobility
    table.insert(entities, {
        _name = "spring", id = eid(),
        x = 64, y = (floorY - 1) * 8,
        orientation = 0
    })
    table.insert(entities, {
        _name = "spring", id = eid(),
        x = w8 - 72, y = (floorY - 1) * 8,
        orientation = 0
    })
    
    -- Camera trigger to lock during boss fight
    table.insert(entities, {
        _name = "cameraTargetTrigger", id = eid(),
        x = 0, y = 0,
        width = w8, height = h8,
        deleteFlag = "",
        yOnly = false,
        lerpStrength = 1.0,
        positionMode = "NoEffect",
        _type = "trigger"
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_phase1_arena", index),
        roomStyle = "dream"
    }
end

--- Build the Phase 2 arena (larger, more intense)
local function buildPhase2Arena(index, globalX, globalY)
    local w, h = PHASE2_W, PHASE2_H
    local m = matrixLib.filled(TILE_VOID, w, h) -- Void tileset for dramatic effect
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    
    -- Clear massive arena
    carveRect(m, cx, cy, w - 8, h - 8, w, h)
    
    -- Floating platform islands
    local floorY = h - 4
    
    -- Central main platform
    placePlatform(m, cx - 12, cx + 12, floorY, TILE_VOID, w, h)
    
    -- Left floating island
    placePlatform(m, 6, math.floor(w * 0.25), floorY - 3, TILE_VOID, w, h)
    placePlatform(m, 8, math.floor(w * 0.22), floorY - 10, TILE_VOID, w, h)
    
    -- Right floating island
    placePlatform(m, math.floor(w * 0.75), w - 5, floorY - 3, TILE_VOID, w, h)
    placePlatform(m, math.floor(w * 0.78), w - 7, floorY - 10, TILE_VOID, w, h)
    
    -- Vertical tower platforms
    for tier = 1, 4 do
        local tierY = floorY - (tier * 7)
        local tierW = 8 - tier
        if tierY > 5 then
            -- Left tower
            placePlatform(m, math.floor(w * 0.2) - tierW, math.floor(w * 0.2) + tierW, tierY, TILE_VOID, w, h)
            -- Center tower
            placePlatform(m, cx - tierW, cx + tierW, tierY, TILE_VOID, w, h)
            -- Right tower
            placePlatform(m, math.floor(w * 0.8) - tierW, math.floor(w * 0.8) + tierW, tierY, TILE_VOID, w, h)
        end
    end
    
    -- Top platform for boss hovering
    placePlatform(m, cx - 8, cx + 8, 6, TILE_VOID, w, h)
    
    fillBorders(m, TILE_VOID, w, h, "left")
    
    -- Left entry
    local exitSize = 6
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
    end
    
    -- Entities
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Asriel God Boss (Phase 2 - more powerful)
    local bossX = cx * 8
    local bossY = 4 * 8
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBoss", id = eid(),
        x = bossX, y = bossY,
        patternIndex = 10,
        cameraPastY = 150.0,
        dialog = true,
        startHit = true,
        cameraLockY = true,
        health = 500,
        maxHealth = 500,
        attackSequence = "BiggerBeam,RainbowBlackhole,ChaosBlaster,HyperGoner"
    })
    
    -- Movement nodes for boss teleportation
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBoss", id = eid(),
        x = bossX, y = bossY,
        nodes = {
            { x = math.floor(w * 0.2) * 8, y = 10 * 8 },
            { x = math.floor(w * 0.8) * 8, y = 10 * 8 },
            { x = cx * 8, y = math.floor(h * 0.4) * 8 },
        },
        patternIndex = 15,
        health = 500,
        maxHealth = 500
    })
    
    -- Many refills around the arena
    local refillPositions = {
        { 56, (floorY - 4) * 8 },
        { w8 - 64, (floorY - 4) * 8 },
        { cx * 8, (floorY - 1) * 8 },
        { math.floor(w * 0.2) * 8, (floorY - 11) * 8 },
        { math.floor(w * 0.8) * 8, (floorY - 11) * 8 },
    }
    for _, pos in ipairs(refillPositions) do
        table.insert(entities, {
            _name = "refill", id = eid(),
            x = pos[1], y = pos[2],
            oneUse = false, twoDash = true
        })
    end
    
    -- Boosters for traversal
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = math.floor(w * 0.15) * 8, y = (floorY - 6) * 8,
        red = true
    })
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = math.floor(w * 0.85) * 8, y = (floorY - 6) * 8,
        red = true
    })
    
    -- Side springs
    table.insert(entities, {
        _name = "spring", id = eid(),
        x = 48, y = (floorY - 4) * 8,
        orientation = 0
    })
    table.insert(entities, {
        _name = "spring", id = eid(),
        x = w8 - 56, y = (floorY - 4) * 8,
        orientation = 0
    })
    
    -- Starfield backdrop trigger
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBossStarfieldTrigger", id = eid(),
        x = 0, y = 0,
        width = w8, height = h8,
        enable = true,
        _type = "trigger"
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_phase2_arena", index),
        roomStyle = "void"
    }
end

--- Build the Final Phase arena (climactic battle)
local function buildFinalArena(index, globalX, globalY)
    local w, h = FINAL_W, FINAL_H
    local m = matrixLib.filled(TILE_VOID, w, h)
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    
    -- Massive open arena with minimal platforms
    carveRect(m, cx, cy, w - 6, h - 6, w, h)
    
    -- Central circular platform (symbolic)
    local radius = 15
    for y = cy - radius, cy + radius do
        for x = cx - radius, cx + radius do
            local dx = x - cx
            local dy = y - cy
            if dx*dx + dy*dy <= radius*radius and dx*dx + dy*dy > (radius-2)*(radius-2) then
                m:set(clamp(x, 2, w-1), clamp(y, 2, h-1), TILE_VOID)
            end
        end
    end
    
    -- Inner platform
    local innerRadius = 8
    placePlatform(m, cx - innerRadius, cx + innerRadius, cy + innerRadius - 2, TILE_VOID, w, h)
    
    -- Corner bastions
    local bastionSize = 6
    -- Top-left
    carveRect(m, bastionSize + 3, bastionSize + 3, bastionSize, bastionSize, w, h)
    placePlatform(m, 4, bastionSize * 2, bastionSize * 2, TILE_VOID, w, h)
    -- Top-right
    carveRect(m, w - bastionSize - 2, bastionSize + 3, bastionSize, bastionSize, w, h)
    placePlatform(m, w - bastionSize * 2, w - 3, bastionSize * 2, TILE_VOID, w, h)
    -- Bottom-left
    carveRect(m, bastionSize + 3, h - bastionSize - 2, bastionSize, bastionSize, w, h)
    placePlatform(m, 4, bastionSize * 2, h - bastionSize * 2 + 1, TILE_VOID, w, h)
    -- Bottom-right
    carveRect(m, w - bastionSize - 2, h - bastionSize - 2, bastionSize, bastionSize, w, h)
    placePlatform(m, w - bastionSize * 2, w - 3, h - bastionSize * 2 + 1, TILE_VOID, w, h)
    
    -- Floating pathway platforms
    local pathY = math.floor(h * 0.35)
    for i = 1, 5 do
        local px = math.floor(w * 0.15 + (w * 0.7) * (i - 1) / 4)
        local py = pathY + ((i % 2 == 0) and 3 or 0)
        placePlatform(m, px - 3, px + 3, py, TILE_VOID, w, h)
    end
    
    pathY = math.floor(h * 0.65)
    for i = 1, 5 do
        local px = math.floor(w * 0.15 + (w * 0.7) * (i - 1) / 4)
        local py = pathY + ((i % 2 == 1) and 3 or 0)
        placePlatform(m, px - 3, px + 3, py, TILE_VOID, w, h)
    end
    
    fillBorders(m, TILE_VOID, w, h, "left")
    
    -- Entry
    local exitSize = 7
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
    end
    
    -- Entities
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Final form Asriel God Boss
    local bossX = cx * 8
    local bossY = cy * 8 - 48
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBoss", id = eid(),
        x = bossX, y = bossY,
        patternIndex = 25,
        cameraPastY = 200.0,
        dialog = true,
        startHit = true,
        cameraLockY = false,
        health = 1000,
        maxHealth = 1000,
        attackSequence = "HyperGoner,EternalChaos,GalacticSaber,DimensionalRift,RainbowInferno,CelestialSpears,TimewarpVortex,PrismBurst,SoulResonance"
    })
    
    -- Kirby damage zone at bottom (void kill plane)
    table.insert(entities, {
        _name = "killbox", id = eid(),
        x = 0, y = h8 - 16,
        width = w8, height = 16
    })
    
    -- Many refills scattered
    local refillPositions = {
        { bastionSize * 8 + 16, (bastionSize * 2 - 1) * 8 },
        { w8 - bastionSize * 8 * 2 + 16, (bastionSize * 2 - 1) * 8 },
        { bastionSize * 8 + 16, h8 - (bastionSize * 2) * 8 },
        { w8 - bastionSize * 8 * 2 + 16, h8 - (bastionSize * 2) * 8 },
        { cx * 8, (cy + innerRadius - 3) * 8 },
    }
    for _, pos in ipairs(refillPositions) do
        table.insert(entities, {
            _name = "refill", id = eid(),
            x = pos[1], y = pos[2],
            oneUse = false, twoDash = true
        })
    end
    
    -- Red boosters in corners
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = bastionSize * 8 + 32, y = (bastionSize * 2 + 2) * 8,
        red = true
    })
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = w8 - bastionSize * 8 * 2, y = (bastionSize * 2 + 2) * 8,
        red = true
    })
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = bastionSize * 8 + 32, y = h8 - (bastionSize * 2 + 3) * 8,
        red = true
    })
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = w8 - bastionSize * 8 * 2, y = h8 - (bastionSize * 2 + 3) * 8,
        red = true
    })
    
    -- Mid-air dream blocks for enhanced mobility
    local dreamPositions = {
        { cx * 8 - 80, math.floor(h * 0.3) * 8 },
        { cx * 8 + 48, math.floor(h * 0.3) * 8 },
        { cx * 8, math.floor(h * 0.7) * 8 },
    }
    for _, pos in ipairs(dreamPositions) do
        table.insert(entities, {
            _name = "dreamBlock", id = eid(),
            x = pos[1], y = pos[2],
            width = 32, height = 24,
            featherMode = true
        })
    end
    
    -- Boss defeat trigger (completion)
    table.insert(entities, {
        _name = "MaggyHelper/BossDefeatTrigger", id = eid(),
        x = 0, y = 0,
        width = w8, height = h8,
        victoryDialog = "CH20_ASRIEL_BOSS_END",
        _type = "trigger"
    })
    
    -- Starfield backdrop
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBossStarfieldTrigger", id = eid(),
        x = 0, y = 0,
        width = w8, height = h8,
        enable = true,
        intensityMax = 1.5,
        _type = "trigger"
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_final_arena", index),
        roomStyle = "void"
    }
end

--- Build a transition corridor between phases
local function buildTransitionCorridor(index, globalX, globalY, fromPhase, toPhase)
    local w, h = 40, 20
    local m = matrixLib.filled(TILE_SOLID, w, h)
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    
    -- Simple corridor
    carveHLine(m, 3, w - 2, cy, 3, w, h)
    placePlatform(m, 3, w - 2, cy + 4, TILE_SOLID, w, h)
    
    -- Exits
    local exitSize = 5
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
        m:set(w, exitStart + dy, TILE_AIR)
        m:set(w - 1, exitStart + dy, TILE_AIR)
    end
    
    -- Entities
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Checkpoint
    table.insert(entities, {
        _name = "checkpoint", id = eid(),
        x = cx * 8, y = (cy - 1) * 8,
        allowOrigin = true
    })
    
    -- Refills
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = 64, y = (cy - 1) * 8,
        oneUse = false, twoDash = true
    })
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = w8 - 72, y = (cy - 1) * 8,
        oneUse = false, twoDash = true
    })
    
    -- Phase transition dialog
    table.insert(entities, {
        _name = "MaggyHelper/EventTrigger", id = eid(),
        x = w8 / 2 - 24, y = 0,
        width = 48, height = h8,
        eventName = string.format("ch20_asriel_phase%d_to_%d_transition", fromPhase, toPhase),
        _type = "trigger"
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_transition_%d_to_%d", index, fromPhase, toPhase),
        roomStyle = "dream"
    }
end

--------------------------------------------------------------------------------
-- Additional Room Builders for 60-Room Map
--------------------------------------------------------------------------------

--- Build a challenge gauntlet room with hazards
local function buildChallengeGauntlet(index, globalX, globalY, difficulty)
    difficulty = difficulty or 0.5
    local size = ROOM_SIZES.corridor
    local w, h = size.w, size.h
    local m = matrixLib.filled(TILE_SOLID, w, h)
    local cy = math.floor(h / 2)
    
    -- Narrow corridor
    carveHLine(m, 3, w - 2, cy, 2, w, h)
    placePlatform(m, 3, w - 2, cy + 3, TILE_SOLID, w, h)
    
    -- Add platforms at varying heights
    local platCount = math.floor(4 + difficulty * 4)
    for i = 1, platCount do
        local px = math.floor(5 + (w - 10) * i / (platCount + 1))
        local py = cy + ((i % 2 == 0) and -2 or 1)
        placePlatform(m, px - 2, px + 2, py, TILE_SOLID, w, h)
    end
    
    -- Exits
    local exitSize = 4
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
        m:set(w, exitStart + dy, TILE_AIR)
        m:set(w - 1, exitStart + dy, TILE_AIR)
    end
    
    fillBorders(m, TILE_SOLID, w, h)
    
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Spinners
    local spinnerCount = math.floor(3 + difficulty * 6)
    local spacing = math.floor((w8 - 80) / (spinnerCount + 1))
    for i = 1, spinnerCount do
        table.insert(entities, {
            _name = "spinner", id = eid(),
            x = 40 + i * spacing,
            y = cy * 8 + ((i % 3 == 0) and -24 or (i % 3 == 1) and 0 or 16),
            attachToSolid = false
        })
    end
    
    -- Refills
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = math.floor(w8 * 0.3), y = (cy - 1) * 8,
        oneUse = false, twoDash = difficulty >= 0.7
    })
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = math.floor(w8 * 0.7), y = (cy - 1) * 8,
        oneUse = false, twoDash = difficulty >= 0.7
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_challenge_gauntlet", index),
        roomStyle = "summit"
    }
end

--- Build a vertical shaft room
local function buildVerticalShaft(index, globalX, globalY, goingUp)
    local size = ROOM_SIZES.vertical
    local w, h = size.w, size.h
    local m = matrixLib.filled(TILE_SOLID, w, h)
    local cx = math.floor(w / 2)
    
    -- Carve vertical shaft
    carveVLine(m, 4, h - 3, cx, 4, w, h)
    
    -- Add alternating platforms
    local platCount = 8
    local spacing = math.floor((h - 10) / platCount)
    for i = 1, platCount do
        local py = 5 + (i - 1) * spacing
        local leftSide = (i % 2 == 1)
        if leftSide then
            placePlatform(m, 4, cx - 2, py, TILE_SOLID, w, h)
        else
            placePlatform(m, cx + 2, w - 3, py, TILE_SOLID, w, h)
        end
    end
    
    -- Exits (top and bottom)
    local exitSize = 5
    -- Bottom
    local exitStartX = math.floor(w / 2) - math.floor(exitSize / 2)
    for dx = 0, exitSize - 1 do
        m:set(exitStartX + dx, h, TILE_AIR)
        m:set(exitStartX + dx, h - 1, TILE_AIR)
    end
    -- Top
    for dx = 0, exitSize - 1 do
        m:set(exitStartX + dx, 1, TILE_AIR)
        m:set(exitStartX + dx, 2, TILE_AIR)
    end
    
    fillBorders(m, TILE_SOLID, w, h)
    
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Springs on platforms
    for i = 1, platCount, 2 do
        local py = 5 + (i - 1) * spacing
        local px = (i % 2 == 1) and (cx - 4) or (cx + 4)
        table.insert(entities, {
            _name = "spring", id = eid(),
            x = px * 8, y = (py - 1) * 8,
            orientation = 0
        })
    end
    
    -- Refills at midpoint
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = cx * 8, y = math.floor(h8 / 2),
        oneUse = false, twoDash = true
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_vertical_shaft_%s", index, goingUp and "up" or "down"),
        roomStyle = "void"
    }
end

--- Build a rest room with checkpoint
local function buildRestRoom(index, globalX, globalY, phase)
    phase = phase or 1
    local size = ROOM_SIZES.small
    local w, h = size.w, size.h
    local m = matrixLib.filled(TILE_SOLID, w, h)
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    
    -- Cozy interior
    carveRect(m, cx, cy, w - 8, h - 8, w, h)
    
    -- Floor
    local floorY = h - 5
    placePlatform(m, 5, w - 4, floorY, TILE_SOLID, w, h)
    
    -- Side exits
    local exitSize = 5
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
        m:set(w, exitStart + dy, TILE_AIR)
        m:set(w - 1, exitStart + dy, TILE_AIR)
    end
    
    fillBorders(m, TILE_SOLID, w, h)
    
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Checkpoint
    table.insert(entities, {
        _name = "checkpoint", id = eid(),
        x = cx * 8, y = (floorY - 1) * 8,
        allowOrigin = true
    })
    
    -- Multiple refills
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = (cx - 4) * 8, y = (floorY - 1) * 8,
        oneUse = false, twoDash = true
    })
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = (cx + 4) * 8, y = (floorY - 1) * 8,
        oneUse = false, twoDash = true
    })
    
    -- Healing crystal (custom entity if exists)
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = cx * 8, y = (cy - 2) * 8,
        oneUse = false, twoDash = true
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_rest_phase%d", index, phase),
        roomStyle = "dream"
    }
end

--- Build a mini-boss room with weaker Asriel attacks
local function buildMiniBossRoom(index, globalX, globalY, variant)
    variant = variant or 1
    local size = ROOM_SIZES.medium
    local w, h = size.w, size.h
    local m = matrixLib.filled(TILE_SOLID, w, h)
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    
    -- Arena
    carveRect(m, cx, cy, w - 8, h - 8, w, h)
    
    -- Floor
    local floorY = h - 5
    placePlatform(m, 5, w - 4, floorY, TILE_SOLID, w, h)
    
    -- Side platforms
    local platY = math.floor(h * 0.45)
    placePlatform(m, 6, math.floor(w * 0.3), platY, TILE_SOLID, w, h)
    placePlatform(m, math.floor(w * 0.7), w - 5, platY, TILE_SOLID, w, h)
    
    -- Exits
    local exitSize = 5
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
        m:set(w, exitStart + dy, TILE_AIR)
        m:set(w - 1, exitStart + dy, TILE_AIR)
    end
    
    fillBorders(m, TILE_SOLID, w, h)
    
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Mini Asriel God Boss (weaker version)
    local attackSeq = ATTACK_SEQUENCES.miniboss[((variant - 1) % #ATTACK_SEQUENCES.miniboss) + 1]
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBoss", id = eid(),
        x = cx * 8, y = (platY - 4) * 8,
        patternIndex = variant,
        cameraPastY = 80.0,
        dialog = false,
        startHit = true,
        cameraLockY = true,
        health = 100 + variant * 50,
        maxHealth = 100 + variant * 50,
        attackSequence = attackSeq
    })
    
    -- Refills
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = 48, y = (floorY - 1) * 8,
        oneUse = false, twoDash = false
    })
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = w8 - 56, y = (floorY - 1) * 8,
        oneUse = false, twoDash = false
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_miniboss_%d", index, variant),
        roomStyle = "castle"
    }
end

--- Build a platform puzzle room
local function buildPlatformPuzzle(index, globalX, globalY, complexity)
    complexity = complexity or 1
    local size = ROOM_SIZES.large
    local w, h = size.w, size.h
    local m = matrixLib.filled(TILE_SOLID, w, h)
    local cx = math.floor(w / 2)
    
    -- Clear most of room
    carveRect(m, cx, math.floor(h / 2), w - 6, h - 6, w, h)
    
    -- Scattered platforms
    local platConfigs = {
        -- {x_fraction, y_fraction, width}
        { 0.15, 0.8, 8 },
        { 0.35, 0.6, 6 },
        { 0.55, 0.75, 7 },
        { 0.75, 0.5, 6 },
        { 0.25, 0.4, 5 },
        { 0.5, 0.3, 8 },
        { 0.8, 0.35, 6 },
        { 0.6, 0.55, 5 },
    }
    
    local usedPlats = math.min(3 + complexity * 2, #platConfigs)
    for i = 1, usedPlats do
        local cfg = platConfigs[i]
        local px = math.floor(w * cfg[1])
        local py = math.floor(h * cfg[2])
        placePlatform(m, px - cfg[3], px + cfg[3], py, TILE_SOLID, w, h)
    end
    
    -- Exits
    local exitSize = 5
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
        m:set(w, exitStart + dy, TILE_AIR)
        m:set(w - 1, exitStart + dy, TILE_AIR)
    end
    
    fillBorders(m, TILE_SOLID, w, h)
    
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Boosters to help traverse
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = math.floor(w8 * 0.25), y = math.floor(h8 * 0.65),
        red = complexity >= 2
    })
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = math.floor(w8 * 0.75), y = math.floor(h8 * 0.45),
        red = complexity >= 2
    })
    
    -- Refills
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = cx * 8, y = math.floor(h8 * 0.28),
        oneUse = false, twoDash = true
    })
    
    -- Dream blocks for complex rooms
    if complexity >= 3 then
        table.insert(entities, {
            _name = "dreamBlock", id = eid(),
            x = math.floor(w8 * 0.4), y = math.floor(h8 * 0.5),
            width = 32, height = 24,
            featherMode = true
        })
    end
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_platform_puzzle_%d", index, complexity),
        roomStyle = "reflection"
    }
end

--- Build a void realm floating islands room
local function buildVoidRealm(index, globalX, globalY, variant)
    variant = variant or 1
    local size = ROOM_SIZES.huge
    local w, h = size.w, size.h
    local m = matrixLib.filled(TILE_VOID, w, h)
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    
    -- Clear everything
    carveRect(m, cx, cy, w - 4, h - 4, w, h)
    
    -- Floating islands pattern based on variant
    math.randomseed(12345 + variant * 1000)  -- Deterministic randomness
    local islandCount = 6 + variant * 2
    for i = 1, islandCount do
        local ix = math.floor(6 + math.random() * (w - 12))
        local iy = math.floor(6 + math.random() * (h - 12))
        local iw = 4 + math.floor(math.random() * 6)
        placePlatform(m, ix - iw, ix + iw, iy, TILE_VOID, w, h)
        -- Some islands get a second layer
        if math.random() > 0.6 then
            placePlatform(m, ix - math.floor(iw * 0.6), ix + math.floor(iw * 0.6), iy - 1, TILE_VOID, w, h)
        end
    end
    
    -- Guaranteed main platforms
    placePlatform(m, 5, 15, cy, TILE_VOID, w, h)  -- Entry
    placePlatform(m, w - 15, w - 5, cy, TILE_VOID, w, h)  -- Exit
    placePlatform(m, cx - 6, cx + 6, cy - 5, TILE_VOID, w, h)  -- Center
    
    -- Exits
    local exitSize = 6
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
        m:set(w, exitStart + dy, TILE_AIR)
        m:set(w - 1, exitStart + dy, TILE_AIR)
    end
    
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Kill zone at bottom
    table.insert(entities, {
        _name = "killbox", id = eid(),
        x = 0, y = h8 - 8,
        width = w8, height = 8
    })
    
    -- Boosters scattered
    for i = 1, 3 do
        table.insert(entities, {
            _name = "booster", id = eid(),
            x = math.floor(w8 * (0.2 + 0.3 * (i - 1))),
            y = math.floor(h8 * (0.3 + math.random() * 0.4)),
            red = true
        })
    end
    
    -- Refills
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = cx * 8, y = (cy - 6) * 8,
        oneUse = false, twoDash = true
    })
    
    -- Starfield
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBossStarfieldTrigger", id = eid(),
        x = 0, y = 0,
        width = w8, height = h8,
        enable = true,
        _type = "trigger"
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_void_realm_%d", index, variant),
        roomStyle = "void"
    }
end

--- Build a boss encounter variant
local function buildBossEncounter(index, globalX, globalY, phase, variant)
    phase = phase or 1
    variant = variant or 1
    
    local sizes = {
        [1] = ROOM_SIZES.large,
        [2] = ROOM_SIZES.huge,
        [3] = ROOM_SIZES.massive,
    }
    local size = sizes[phase] or ROOM_SIZES.large
    local w, h = size.w, size.h
    local material = (phase >= 2) and TILE_VOID or TILE_SOLID
    local m = matrixLib.filled(material, w, h)
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    
    -- Clear arena
    carveRect(m, cx, cy, w - 8, h - 8, w, h)
    
    -- Floor
    local floorY = h - 5
    placePlatform(m, 5, w - 4, floorY, material, w, h)
    
    -- Phase-specific platform layouts
    if phase == 1 then
        -- Simple side platforms
        local platY = math.floor(h * 0.4)
        placePlatform(m, 6, math.floor(w * 0.25), platY, material, w, h)
        placePlatform(m, math.floor(w * 0.75), w - 5, platY, material, w, h)
        -- Center platform
        placePlatform(m, cx - 5, cx + 5, math.floor(h * 0.5), material, w, h)
    elseif phase == 2 then
        -- Multiple tiers
        for tier = 1, 3 do
            local tierY = floorY - tier * 6
            local tierW = 8 - tier
            placePlatform(m, math.floor(w * 0.2) - tierW, math.floor(w * 0.2) + tierW, tierY, material, w, h)
            placePlatform(m, math.floor(w * 0.5) - tierW, math.floor(w * 0.5) + tierW, tierY, material, w, h)
            placePlatform(m, math.floor(w * 0.8) - tierW, math.floor(w * 0.8) + tierW, tierY, material, w, h)
        end
    else
        -- Phase 3: Circular floating platforms
        local radius = math.min(w, h) * 0.35
        for angle = 0, 7 do
            local rad = angle * math.pi / 4
            local px = cx + math.floor(math.cos(rad) * radius)
            local py = cy + math.floor(math.sin(rad) * radius * 0.6)
            if py > 4 and py < h - 4 then
                placePlatform(m, clamp(px - 4, 3, w - 2), clamp(px + 4, 3, w - 2), py, material, w, h)
            end
        end
        -- Center pinnacle
        placePlatform(m, cx - 6, cx + 6, cy - 4, material, w, h)
    end
    
    -- Exits
    local exitSize = 5 + phase
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
        m:set(w, exitStart + dy, TILE_AIR)
        m:set(w - 1, exitStart + dy, TILE_AIR)
    end
    
    fillBorders(m, material, w, h)
    
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Boss
    local attackPool = ATTACK_SEQUENCES["phase" .. phase] or ATTACK_SEQUENCES.phase1
    local attackSeq = attackPool[((variant - 1) % #attackPool) + 1]
    local bossHealth = 200 + (phase - 1) * 200 + variant * 50
    
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBoss", id = eid(),
        x = cx * 8,
        y = math.floor(h8 * 0.2),
        patternIndex = (phase - 1) * 10 + variant,
        cameraPastY = 100 + phase * 20,
        dialog = variant == 1,
        startHit = variant > 1,
        cameraLockY = true,
        health = bossHealth,
        maxHealth = bossHealth,
        attackSequence = attackSeq
    })
    
    -- Start trigger for first variant
    if variant == 1 then
        table.insert(entities, {
            _name = "MaggyHelper/AsrielStartHitTrigger", id = eid(),
            x = 0, y = 0,
            width = w8, height = h8,
            _type = "trigger"
        })
    end
    
    -- Refills
    local refillCount = 2 + phase
    for i = 1, refillCount do
        local rx = math.floor(w8 * (i / (refillCount + 1)))
        table.insert(entities, {
            _name = "refill", id = eid(),
            x = rx, y = (floorY - 1) * 8,
            oneUse = false, twoDash = true
        })
    end
    
    -- Springs
    table.insert(entities, {
        _name = "spring", id = eid(),
        x = 64, y = (floorY - 1) * 8,
        orientation = 0
    })
    table.insert(entities, {
        _name = "spring", id = eid(),
        x = w8 - 72, y = (floorY - 1) * 8,
        orientation = 0
    })
    
    -- Boosters for phase 2+
    if phase >= 2 then
        table.insert(entities, {
            _name = "booster", id = eid(),
            x = math.floor(w8 * 0.2), y = math.floor(h8 * 0.5),
            red = true
        })
        table.insert(entities, {
            _name = "booster", id = eid(),
            x = math.floor(w8 * 0.8), y = math.floor(h8 * 0.5),
            red = true
        })
    end
    
    -- Dream blocks for phase 3
    if phase >= 3 then
        table.insert(entities, {
            _name = "dreamBlock", id = eid(),
            x = cx * 8 - 48, y = math.floor(h8 * 0.35),
            width = 32, height = 24,
            featherMode = true
        })
        table.insert(entities, {
            _name = "dreamBlock", id = eid(),
            x = cx * 8 + 16, y = math.floor(h8 * 0.35),
            width = 32, height = 24,
            featherMode = true
        })
    end
    
    -- Starfield for phase 2+
    if phase >= 2 then
        table.insert(entities, {
            _name = "MaggyHelper/AsrielGodBossStarfieldTrigger", id = eid(),
            x = 0, y = 0,
            width = w8, height = h8,
            enable = true,
            intensityMax = 0.5 + phase * 0.3,
            _type = "trigger"
        })
    end
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_boss_p%d_v%d", index, phase, variant),
        roomStyle = phase >= 2 and "void" or "dream"
    }
end

--- Build an escape sequence room
local function buildEscapeSequence(index, globalX, globalY, intensity)
    intensity = intensity or 1
    local size = ROOM_SIZES.corridor
    local w, h = size.w + intensity * 10, size.h
    local m = matrixLib.filled(TILE_VOID, w, h)
    local cy = math.floor(h / 2)
    
    -- Main corridor
    carveHLine(m, 3, w - 2, cy, 3, w, h)
    
    -- Collapsing platforms (visual only - triggers add falling blocks)
    local platCount = 4 + intensity * 2
    local spacing = math.floor((w - 10) / platCount)
    for i = 1, platCount do
        local px = 5 + (i - 1) * spacing
        placePlatform(m, px, px + 4, cy + 4, TILE_VOID, w, h)
    end
    
    -- Exits
    local exitSize = 5
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
        m:set(w, exitStart + dy, TILE_AIR)
        m:set(w - 1, exitStart + dy, TILE_AIR)
    end
    
    fillBorders(m, TILE_VOID, w, h)
    
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- Falling blocks (crumble platforms)
    for i = 1, platCount do
        local px = 5 + (i - 1) * spacing
        table.insert(entities, {
            _name = "crumbleBlock", id = eid(),
            x = px * 8, y = (cy + 3) * 8,
            width = 32, height = 8
        })
    end
    
    -- Chase hazard (projectiles from behind)
    table.insert(entities, {
        _name = "MaggyHelper/EventTrigger", id = eid(),
        x = 32, y = 0,
        width = 48, height = h8,
        eventName = "ch20_asriel_escape_sequence",
        _type = "trigger"
    })
    
    -- Refills sparsely
    table.insert(entities, {
        _name = "refill", id = eid(),
        x = math.floor(w8 * 0.5), y = (cy - 1) * 8,
        oneUse = false, twoDash = true
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_escape_%d", index, intensity),
        roomStyle = "void"
    }
end

--- Build the ultimate final boss room
local function buildUltimateFinalBoss(index, globalX, globalY)
    local w, h = 100, 60
    local m = matrixLib.filled(TILE_VOID, w, h)
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    
    -- Massive clear arena
    carveRect(m, cx, cy, w - 6, h - 6, w, h)
    
    -- Epic platform structure
    -- Central pillar
    for y = cy + 10, h - 6 do
        m:set(clamp(cx - 2, 2, w - 1), y, TILE_VOID)
        m:set(clamp(cx - 1, 2, w - 1), y, TILE_VOID)
        m:set(clamp(cx, 2, w - 1), y, TILE_VOID)
        m:set(clamp(cx + 1, 2, w - 1), y, TILE_VOID)
        m:set(clamp(cx + 2, 2, w - 1), y, TILE_VOID)
    end
    
    -- Main floor
    local floorY = h - 5
    placePlatform(m, 5, w - 4, floorY, TILE_VOID, w, h)
    
    -- Ascending platforms
    for tier = 1, 6 do
        local tierY = floorY - tier * 7
        local tierSpread = 12 + (6 - tier) * 4
        if tierY > 8 then
            placePlatform(m, cx - tierSpread, cx - tierSpread + 8, tierY, TILE_VOID, w, h)
            placePlatform(m, cx + tierSpread - 8, cx + tierSpread, tierY, TILE_VOID, w, h)
        end
    end
    
    -- Top pinnacle platform
    placePlatform(m, cx - 8, cx + 8, 8, TILE_VOID, w, h)
    
    -- Corner bastions
    local bastionSize = 10
    placePlatform(m, 5, bastionSize + 5, bastionSize, TILE_VOID, w, h)
    placePlatform(m, w - bastionSize - 4, w - 4, bastionSize, TILE_VOID, w, h)
    placePlatform(m, 5, bastionSize + 5, h - bastionSize - 3, TILE_VOID, w, h)
    placePlatform(m, w - bastionSize - 4, w - 4, h - bastionSize - 3, TILE_VOID, w, h)
    
    -- Entry
    local exitSize = 8
    local exitStart = math.floor(h / 2) - math.floor(exitSize / 2)
    for dy = 0, exitSize - 1 do
        m:set(1, exitStart + dy, TILE_AIR)
        m:set(2, exitStart + dy, TILE_AIR)
    end
    
    fillBorders(m, TILE_VOID, w, h)
    
    local entities = {}
    local w8 = w * 8
    local h8 = h * 8
    
    -- THE FINAL BOSS
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBoss", id = eid(),
        x = cx * 8, y = 5 * 8,
        patternIndex = 50,
        cameraPastY = 250.0,
        dialog = true,
        startHit = false,
        cameraLockY = false,
        health = 2000,
        maxHealth = 2000,
        attackSequence = "HyperGoner,EternalChaos,GalacticSaber,DimensionalRift,RainbowInferno,CelestialSpears,TimewarpVortex,PrismBurst,SoulResonance,HyperGoner,EternalChaos"
    })
    
    -- Start trigger
    table.insert(entities, {
        _name = "MaggyHelper/AsrielStartHitTrigger", id = eid(),
        x = 0, y = 0,
        width = w8, height = h8,
        _type = "trigger"
    })
    
    -- Kill zones
    table.insert(entities, {
        _name = "killbox", id = eid(),
        x = 0, y = h8 - 8,
        width = w8, height = 8
    })
    
    -- Many refills everywhere
    local refillPositions = {
        { bastionSize * 8 / 2 + 40, (bastionSize - 1) * 8 },
        { w8 - bastionSize * 8 / 2 - 40, (bastionSize - 1) * 8 },
        { bastionSize * 8 / 2 + 40, h8 - (bastionSize + 4) * 8 },
        { w8 - bastionSize * 8 / 2 - 40, h8 - (bastionSize + 4) * 8 },
        { cx * 8, (floorY - 1) * 8 },
        { cx * 8, (cy) * 8 },
        { cx * 8, 9 * 8 },
    }
    for _, pos in ipairs(refillPositions) do
        table.insert(entities, {
            _name = "refill", id = eid(),
            x = pos[1], y = pos[2],
            oneUse = false, twoDash = true
        })
    end
    
    -- Red boosters at corners
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = bastionSize * 8 + 16, y = (bastionSize + 3) * 8,
        red = true
    })
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = w8 - bastionSize * 8 - 48, y = (bastionSize + 3) * 8,
        red = true
    })
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = bastionSize * 8 + 16, y = h8 - (bastionSize + 6) * 8,
        red = true
    })
    table.insert(entities, {
        _name = "booster", id = eid(),
        x = w8 - bastionSize * 8 - 48, y = h8 - (bastionSize + 6) * 8,
        red = true
    })
    
    -- Dream blocks for ultimate mobility
    local dreamPositions = {
        { cx * 8 - 100, cy * 8 - 60 },
        { cx * 8 + 68, cy * 8 - 60 },
        { cx * 8 - 100, cy * 8 + 40 },
        { cx * 8 + 68, cy * 8 + 40 },
    }
    for _, pos in ipairs(dreamPositions) do
        table.insert(entities, {
            _name = "dreamBlock", id = eid(),
            x = pos[1], y = pos[2],
            width = 32, height = 32,
            featherMode = true
        })
    end
    
    -- Epic starfield
    table.insert(entities, {
        _name = "MaggyHelper/AsrielGodBossStarfieldTrigger", id = eid(),
        x = 0, y = 0,
        width = w8, height = h8,
        enable = true,
        intensityMax = 2.0,
        _type = "trigger"
    })
    
    -- Victory trigger
    table.insert(entities, {
        _name = "MaggyHelper/BossDefeatTrigger", id = eid(),
        x = 0, y = 0,
        width = w8, height = h8,
        victoryDialog = "CH20_ASRIEL_BOSS_END",
        _type = "trigger"
    })
    
    return {
        matrix = m,
        entities = entities,
        width = w,
        height = h,
        x = globalX,
        y = globalY,
        name = string.format("%02d_ultimate_final", index),
        roomStyle = "void"
    }
end

--------------------------------------------------------------------------------
-- Main Map Generation (60 Rooms)
--------------------------------------------------------------------------------

--- Helper to add a room and advance position
local function addRoom(rooms, room, direction)
    table.insert(rooms, room)
    local w8, h8 = room.width * 8, room.height * 8
    if direction == "right" then
        return room.x + w8 + ROOM_SPACING, room.y
    elseif direction == "up" then
        return room.x, room.y - h8 - ROOM_SPACING
    elseif direction == "down" then
        return room.x, room.y + h8 + ROOM_SPACING
    else
        return room.x + w8 + ROOM_SPACING, room.y
    end
end

local function generateKirbyVsAsrielMap()
    local rooms = {}
    local x, y = 0, 0
    local idx = 1
    
    -----------------------------
    -- ACT 1: THE APPROACH (Rooms 1-10)
    -- Journey to face Asriel, building anticipation
    -----------------------------
    
    -- Room 1: Opening approach
    local r = buildApproachRoom(idx, x, y)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 2: Challenge gauntlet (warm-up)
    r = buildChallengeGauntlet(idx, x, y, 0.3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 3: Platform puzzle 1
    r = buildPlatformPuzzle(idx, x, y, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 4: Vertical shaft up
    r = buildVerticalShaft(idx, x, y - 200, true)
    x, y = addRoom(rooms, r, "up")
    idx = idx + 1
    
    -- Room 5: Rest room before phase 1
    r = buildRestRoom(idx, x, y, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 6: Challenge gauntlet (medium)
    r = buildChallengeGauntlet(idx, x, y, 0.5)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 7: Platform puzzle 2
    r = buildPlatformPuzzle(idx, x, y, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 8: Void realm intro (mild)
    r = buildVoidRealm(idx, x, y - 100, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 9: Challenge corridor
    r = buildChallengeGauntlet(idx, x, y, 0.6)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 10: Rest before Phase 1 boss
    r = buildRestRoom(idx, x, y, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -----------------------------
    -- ACT 2: PHASE 1 - HOPES AND DREAMS (Rooms 11-20)
    -- First encounters with Asriel's power
    -----------------------------
    
    -- Room 11: Phase 1 Arena - Opening fight
    r = buildPhase1Arena(idx, x, y - 80)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 12: Mini-boss encounter 1
    r = buildMiniBossRoom(idx, x, y, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 13: Transition corridor
    r = buildTransitionCorridor(idx, x, y, 1, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 14: Boss encounter variant
    r = buildBossEncounter(idx, x, y - 50, 1, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 15: Challenge room
    r = buildChallengeGauntlet(idx, x, y, 0.7)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 16: Mini-boss encounter 2
    r = buildMiniBossRoom(idx, x, y, 2)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 17: Platform puzzle (harder)
    r = buildPlatformPuzzle(idx, x, y, 2)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 18: Boss encounter variant 2
    r = buildBossEncounter(idx, x, y - 60, 1, 2)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 19: Escape sequence 1
    r = buildEscapeSequence(idx, x, y, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 20: Phase 1 finale + transition
    r = buildTransitionCorridor(idx, x, y, 1, 2)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -----------------------------
    -- ACT 3: PHASE 2 - SAVE THE WORLD (Rooms 21-35)
    -- Asriel's power escalates
    -----------------------------
    
    -- Room 21: Phase 2 Arena - Opening
    r = buildPhase2Arena(idx, x, y - 120)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 22: Void realm 2
    r = buildVoidRealm(idx, x, y - 80, 2)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 23: Mini-boss 3
    r = buildMiniBossRoom(idx, x, y, 3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 24: Boss encounter p2v1
    r = buildBossEncounter(idx, x, y - 100, 2, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 25: Rest room mid-phase
    r = buildRestRoom(idx, x, y, 2)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 26: Challenge gauntlet (hard)
    r = buildChallengeGauntlet(idx, x, y, 0.8)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 27: Vertical shaft down
    r = buildVerticalShaft(idx, x, y + 200, false)
    x, y = addRoom(rooms, r, "down")
    idx = idx + 1
    
    -- Room 28: Void realm 3
    r = buildVoidRealm(idx, x, y, 3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 29: Mini-boss 4
    r = buildMiniBossRoom(idx, x, y, 4)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 30: Boss encounter p2v2
    r = buildBossEncounter(idx, x, y - 80, 2, 2)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 31: Platform puzzle (complex)
    r = buildPlatformPuzzle(idx, x, y, 3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 32: Challenge gauntlet extreme
    r = buildChallengeGauntlet(idx, x, y, 0.9)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 33: Mini-boss 5
    r = buildMiniBossRoom(idx, x, y, 5)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 34: Escape sequence 2
    r = buildEscapeSequence(idx, x, y, 2)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 35: Phase 2 → 3 transition
    r = buildTransitionCorridor(idx, x, y, 2, 3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -----------------------------
    -- ACT 4: PHASE 3 - BURN IN DESPAIR (Rooms 36-50)
    -- The void realm opens, maximum intensity
    -----------------------------
    
    -- Room 36: Final Arena introduction
    r = buildFinalArena(idx, x, y - 150)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 37: Void realm 4
    r = buildVoidRealm(idx, x, y, 4)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 38: Boss encounter p3v1
    r = buildBossEncounter(idx, x, y - 100, 3, 1)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 39: Mini-boss 6
    r = buildMiniBossRoom(idx, x, y, 6)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 40: Rest room (sanctuary in void)
    r = buildRestRoom(idx, x, y, 3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 41: Challenge gauntlet (maximum)
    r = buildChallengeGauntlet(idx, x, y, 1.0)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 42: Vertical shaft up (ascent)
    r = buildVerticalShaft(idx, x, y - 300, true)
    x, y = addRoom(rooms, r, "up")
    idx = idx + 1
    
    -- Room 43: Void realm 5
    r = buildVoidRealm(idx, x, y, 5)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 44: Boss encounter p3v2
    r = buildBossEncounter(idx, x, y - 120, 3, 2)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 45: Platform puzzle (4)
    r = buildPlatformPuzzle(idx, x, y, 4)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 46: Mini-boss 7
    r = buildMiniBossRoom(idx, x, y, 7)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 47: Boss encounter p3v3
    r = buildBossEncounter(idx, x, y - 100, 3, 3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 48: Escape sequence 3
    r = buildEscapeSequence(idx, x, y, 3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 49: Mini-boss 8 (penultimate)
    r = buildMiniBossRoom(idx, x, y, 8)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 50: Rest before finale
    r = buildRestRoom(idx, x, y, 3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -----------------------------
    -- ACT 5: THE FINAL CONFRONTATION (Rooms 51-60)
    -- Ultimate showdown with Asriel God of Hyperdeath
    -----------------------------
    
    -- Room 51: Void realm 6 (ominous approach)
    r = buildVoidRealm(idx, x, y, 6)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 52: Boss encounter (warm-up)
    r = buildBossEncounter(idx, x, y - 80, 3, 4)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 53: Mini-boss 9
    r = buildMiniBossRoom(idx, x, y, 9)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 54: Challenge gauntlet final
    r = buildChallengeGauntlet(idx, x, y, 1.0)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 55: Void realm 7
    r = buildVoidRealm(idx, x, y, 7)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 56: Boss encounter (intense)
    r = buildBossEncounter(idx, x, y - 100, 3, 5)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 57: Escape sequence final
    r = buildEscapeSequence(idx, x, y, 4)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 58: Mini-boss 10 (last)
    r = buildMiniBossRoom(idx, x, y, 10)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 59: Pre-finale rest
    r = buildRestRoom(idx, x, y, 3)
    x, y = addRoom(rooms, r, "right")
    idx = idx + 1
    
    -- Room 60: THE ULTIMATE FINAL BOSS
    r = buildUltimateFinalBoss(idx, x, y - 200)
    table.insert(rooms, r)
    
    return rooms
end

--------------------------------------------------------------------------------
-- Lönn Script Interface
--------------------------------------------------------------------------------

function script.prerun(room, args)
    if not state.map then
        return
    end
    
    local rooms = generateKirbyVsAsrielMap()
    
    if not rooms or #rooms == 0 then
        return
    end
    
    -- Find an offset so new rooms don't overlap existing ones
    local maxX = 0
    for _, existingRoom in ipairs(state.map.rooms) do
        local rx = existingRoom.x + (existingRoom.width or 320)
        if rx > maxX then maxX = rx end
    end
    local offsetX = maxX + 320  -- 320px gap
    
    -- Save state for undo
    local prevRooms = utils.deepcopy(state.map.rooms)
    
    -- Add generated rooms to the map
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
        local roomName = roomData.name or string.format("asriel_%02d", i)
        local roomX = offsetX + (roomData.x or 0)
        local roomY = roomData.y or 0
        local roomStyle = roomData.roomStyle or "dream"
        
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
            dark = ({void=true})[roomStyle] or false,
            underwater = false,
            space = ({void=true, dream=true})[roomStyle] or false,
            windPattern = "None",
            color = 0,
            cameraOffsetX = 0,
            cameraOffsetY = 0,
            music = "",
            alt_music = "",
            ambience = "",
        }
        
        -- Add entities with room-relative positions
        if roomData.entities then
            for _, entity in ipairs(roomData.entities) do
                local e = {
                    _name = entity.name or entity._name,
                    x = (entity.x or 0) + roomX,
                    y = (entity.y or 0) + roomY,
                    width = entity.width,
                    height = entity.height,
                    _id = entity.id or entity._id or 0,
                }
                for k, v in pairs(entity) do
                    if k ~= "name" and k ~= "_name" and k ~= "x" and k ~= "y"
                       and k ~= "width" and k ~= "height" and k ~= "_id" and k ~= "id"
                       and k ~= "nodes" and k ~= "_type" then
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
end

return script
