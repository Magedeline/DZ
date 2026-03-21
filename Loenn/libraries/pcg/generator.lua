-- PCG Room Generator for Celeste
-- Generates room content (tiles + entities) using trained Markov models
-- Handles post-processing: border walls, exit placement, spawn points

local generator = {}

local randomUtils = require("libraries.pcg.random_utils")

--------------------------------------------------------------------------------
-- Tile Constants (Celeste tileset characters)
--------------------------------------------------------------------------------
generator.TILE_AIR   = "0"
generator.TILE_DIRT  = "1"
generator.TILE_SNOW  = "3"
generator.TILE_CLIFF = "4"
generator.TILE_RESORT = "5"
generator.TILE_TEMPLE = "6"
generator.TILE_SUMMIT = "7"
generator.TILE_ROCK  = "8"
generator.TILE_CORE  = "9"
generator.TILE_WOOD_STONE = "a"
generator.TILE_CLIFFSIDE  = "b"
generator.TILE_POOL       = "c"
generator.TILE_TEMPLE_A   = "d"
generator.TILE_TEMPLE_B   = "e"
generator.TILE_CLIFFSIDE_ALT = "f"
generator.TILE_REFLECTION = "g"
generator.TILE_GRASS      = "h"
generator.TILE_SUMMIT_SNO = "i"
generator.TILE_SUMMIT_NO  = "j"
generator.TILE_CORE_ALT   = "k"
generator.TILE_DEADGRASS  = "l"
generator.TILE_LOSTLEVELS = "m"
generator.TILE_SCIFI      = "n"
-- MaggyHelper custom tilesets
generator.TILE_RUINS      = "A"
generator.TILE_CASTLE     = "B"
generator.TILE_VOID       = "O"
generator.TILE_TESSERACT  = "S"
generator.TILE_DETERMINED = "T"
generator.TILE_VOID_STONE = "X"
generator.TILE_PENUMBRA   = "p"
generator.TILE_HOPES_DREAMS = "N"
generator.TILE_FINAL      = "v"

-- All valid foreground tile IDs used in vanilla Celeste
generator.VALID_FG_TILES = {
    "1", "3", "4", "5", "6", "7", "8", "9",
    "a", "b", "c", "d", "e", "f", "g", "h",
    "i", "j", "k", "l", "m", "n"
}

-- Tile mapping per room style for thematic visuals
generator.STYLE_TILES = {
    normal      = "1",    -- dirt
    resort      = "5",    -- tower/resort
    temple      = "d",    -- temple A
    reflection  = "g",    -- reflection
    summit      = "i",    -- summit (snow)
    core        = "k",    -- core
    wind        = "3",    -- snow/golden ridge
    ice         = "3",    -- snow
    cave        = "8",    -- rock
    ruins       = "A",    -- ruins (custom)
    castle      = "B",    -- castle (custom)
    darkStars   = "O",    -- void (custom)
    void        = "X",    -- void stone (custom)
    nightmare   = "p",    -- penumbra (custom)
    farewell    = "n",    -- sci-fi
    dream       = "N",    -- hopes and dreams (custom)
    space       = "1",    -- dirt (eroded)
    deepSpace   = "1",    -- dirt (heavily eroded)
}

-- Entity types categorized for PCG metrics
generator.NLE_ENTITIES = {  -- Non-Lethal Entities
    "player", "refill", "booster", "spring", "jumpThru",
    "dreamBlock", "dashBlock", "moveBlock", "zipMover",
    "touchSwitch", "crumbleWallOnRumble", "fallingBlock",
    "swapBlock", "switchGate", "floatySpaceBlock",
    "bounceBlock", "coreBlock", "starJumpBlock",
    "cloud", "kevinsPC",
}

generator.LE_ENTITIES = {  -- Lethal Entities
    "spikesUp", "spikesDown", "spikesLeft", "spikesRight",
    "spinner", "lightning", "killbox",
    "rotateSpinner", "trackSpinner", "dustStaticSpinner",
    "finalBoss", "seekerBarrier", "seeker",
}

--------------------------------------------------------------------------------
-- Exit Handling
--------------------------------------------------------------------------------

--- Place exits (air gaps) on the specified sides of a room matrix.
-- @param matrix    Tile matrix to modify
-- @param exits     Table with boolean keys: {left=bool, right=bool, top=bool, bottom=bool}
-- @param exitSize  Width/height of exit in tiles (default 5)
function generator.placeExits(matrix, exits, exitSize)
    exitSize = exitSize or 5
    local w, h = matrix:size()

    if exits.left then
        local startY = math.floor(h / 2) - math.floor(exitSize / 2)
        for dy = 0, exitSize - 1 do
            local y = startY + dy
            if y >= 1 and y <= h then
                matrix:set(1, y, generator.TILE_AIR)
                matrix:set(2, y, generator.TILE_AIR)
            end
        end
    end

    if exits.right then
        local startY = math.floor(h / 2) - math.floor(exitSize / 2)
        for dy = 0, exitSize - 1 do
            local y = startY + dy
            if y >= 1 and y <= h then
                matrix:set(w, y, generator.TILE_AIR)
                matrix:set(w - 1, y, generator.TILE_AIR)
            end
        end
    end

    if exits.top then
        local startX = math.floor(w / 2) - math.floor(exitSize / 2)
        for dx = 0, exitSize - 1 do
            local x = startX + dx
            if x >= 1 and x <= w then
                matrix:set(x, 1, generator.TILE_AIR)
                matrix:set(x, 2, generator.TILE_AIR)
            end
        end
    end

    if exits.bottom then
        local startX = math.floor(w / 2) - math.floor(exitSize / 2)
        for dx = 0, exitSize - 1 do
            local x = startX + dx
            if x >= 1 and x <= w then
                matrix:set(x, h, generator.TILE_AIR)
                matrix:set(x, h - 1, generator.TILE_AIR)
            end
        end
    end
end

--------------------------------------------------------------------------------
-- Border Generation
--------------------------------------------------------------------------------

--- Ensure the room has solid borders (walls) except at exits.
-- @param matrix    Tile matrix to modify
-- @param material  Border tile material (default "1")
-- @param exits     Exit configuration (same as placeExits)
-- @param exitSize  Size of exits in tiles
function generator.placeBorders(matrix, material, exits, exitSize)
    material = material or "1"
    exits = exits or {}
    exitSize = exitSize or 5
    local w, h = matrix:size()

    local leftExitStart, leftExitEnd = -1, -1
    local rightExitStart, rightExitEnd = -1, -1
    local topExitStart, topExitEnd = -1, -1
    local bottomExitStart, bottomExitEnd = -1, -1

    if exits.left then
        leftExitStart = math.floor(h / 2) - math.floor(exitSize / 2)
        leftExitEnd = leftExitStart + exitSize - 1
    end
    if exits.right then
        rightExitStart = math.floor(h / 2) - math.floor(exitSize / 2)
        rightExitEnd = rightExitStart + exitSize - 1
    end
    if exits.top then
        topExitStart = math.floor(w / 2) - math.floor(exitSize / 2)
        topExitEnd = topExitStart + exitSize - 1
    end
    if exits.bottom then
        bottomExitStart = math.floor(w / 2) - math.floor(exitSize / 2)
        bottomExitEnd = bottomExitStart + exitSize - 1
    end

    -- Top and bottom borders
    for x = 1, w do
        local isTopExit = (x >= topExitStart and x <= topExitEnd)
        local isBottomExit = (x >= bottomExitStart and x <= bottomExitEnd)

        if not isTopExit then
            matrix:set(x, 1, material)
        end
        if not isBottomExit then
            matrix:set(x, h, material)
        end
    end

    -- Left and right borders
    for y = 1, h do
        local isLeftExit = (y >= leftExitStart and y <= leftExitEnd)
        local isRightExit = (y >= rightExitStart and y <= rightExitEnd)

        if not isLeftExit then
            matrix:set(1, y, material)
        end
        if not isRightExit then
            matrix:set(w, y, material)
        end
    end
end

--- Place precision-platformer borders: solid floor, open top, stubbed side walls.
-- Unlike placeBorders (dungeon-style full enclosure), this creates the open
-- top-and-sides layout appropriate for a 2D precision platformer — rooms feel
-- like a segment of a larger world rather than an enclosed chamber.
--
-- Behaviour:
--   bottom row  – always solid (floor), except where a bottom exit is requested.
--   top row     – always open (air), regardless of exits, so the sky is visible.
--   side walls  – solid only in the bottom quarter of the room height; the upper
--                 portion is left open, trimmed back from the dungeon-style column.
--
-- @param matrix    Tile matrix to modify
-- @param material  Border tile material (default "1")
-- @param exits     Exit configuration { left, right, top, bottom }
-- @param exitSize  Width/height of exit opening in tiles (default 5)
function generator.placeTrimmedBorders(matrix, material, exits, exitSize)
    material = material or "1"
    exits = exits or {}
    exitSize = exitSize or 5
    local w, h = matrix:size()

    -- Bottom border: solid floor (good landing surface)
    local bottomExitStart, bottomExitEnd = -1, -1
    if exits.bottom then
        bottomExitStart = math.floor(w / 2) - math.floor(exitSize / 2)
        bottomExitEnd   = bottomExitStart + exitSize - 1
    end
    for x = 1, w do
        local isBottomExit = (x >= bottomExitStart and x <= bottomExitEnd)
        if not isBottomExit then
            matrix:set(x, h, material)
        end
    end

    -- Top row: always air — open sky for precision platformer feel
    for x = 1, w do
        matrix:set(x, 1, generator.TILE_AIR)
    end

    -- Side walls: only the bottom quarter, so rooms feel open at the top.
    -- Exits on left/right trim even the lower stub at the exit opening row.
    local stubHeight = math.max(2, math.floor(h / 4))
    local leftExitStart, leftExitEnd   = -1, -1
    local rightExitStart, rightExitEnd = -1, -1
    if exits.left then
        leftExitStart = math.floor(h / 2) - math.floor(exitSize / 2)
        leftExitEnd   = leftExitStart + exitSize - 1
    end
    if exits.right then
        rightExitStart = math.floor(h / 2) - math.floor(exitSize / 2)
        rightExitEnd   = rightExitStart + exitSize - 1
    end
    for y = h - stubHeight + 1, h do
        local isLeftExit  = (y >= leftExitStart  and y <= leftExitEnd)
        local isRightExit = (y >= rightExitStart and y <= rightExitEnd)
        if not isLeftExit then
            matrix:set(1, y, material)
        end
        if not isRightExit then
            matrix:set(w, y, material)
        end
    end
end

--------------------------------------------------------------------------------
-- Styleground / Decal Generation
--------------------------------------------------------------------------------

--- Return a list of styleground effect tables appropriate for a given room style.
-- Each entry is a { _name=string, ...attrs } table matching Lönn's effect format.
-- @param style  Room style string (e.g. "normal", "summit", "space")
-- @return table Array of styleground element tables (may be empty)
function generator.generateStylegrounds(style)
    style = style or "normal"

    -- Maps each style to { bg=table[], fg=table[] } styleground definitions
    local styleMap = {
        normal     = {
            bg = {{ _name = "parallax", texture = "bgs/02/bg0", blendMode = "alphablend",
                    loopX = true, loopY = true, scrollX = 0.1, scrollY = 0.05,
                    x = 0, y = 0, color = "ffffff", alpha = 1.0, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        resort     = {
            bg = {{ _name = "parallax", texture = "bgs/03/bg", blendMode = "alphablend",
                    loopX = true, loopY = true, scrollX = 0.15, scrollY = 0.05,
                    x = 0, y = 0, color = "ffffff", alpha = 1.0, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        temple     = {
            bg = {{ _name = "parallax", texture = "bgs/05/bg0", blendMode = "alphablend",
                    loopX = true, loopY = true, scrollX = 0.1, scrollY = 0.05,
                    x = 0, y = 0, color = "ffffff", alpha = 1.0, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        reflection = {
            bg = {{ _name = "parallax", texture = "bgs/06/bg0", blendMode = "alphablend",
                    loopX = true, loopY = true, scrollX = 0.1, scrollY = 0.05,
                    x = 0, y = 0, color = "ffffff", alpha = 1.0, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        summit     = {
            bg = {{ _name = "parallax", texture = "bgs/07/bg0", blendMode = "alphablend",
                    loopX = true, loopY = true, scrollX = 0.2, scrollY = 0.1,
                    x = 0, y = 0, color = "ffffff", alpha = 1.0, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        core       = {
            bg = {{ _name = "HeatWave", bg = false }},
            fg = {},
        },
        wind       = {
            bg = {{ _name = "parallax", texture = "bgs/04/bg0", blendMode = "alphablend",
                    loopX = true, loopY = true, scrollX = 0.25, scrollY = 0.05,
                    x = 0, y = 0, color = "ffffff", alpha = 1.0, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {{ _name = "Wind" }},
        },
        space      = {
            bg = {{ _name = "StarField", starCount = 100, speed = -1.0, bgColor = "000000",
                    shootingStarColor = "7fffff", colors = "ffffff", seed = 0 }},
            fg = {},
        },
        deepSpace  = {
            bg = {{ _name = "StarField", starCount = 200, speed = -2.0, bgColor = "000000",
                    shootingStarColor = "7fffff", colors = "ffffff", seed = 0 },
                  { _name = "DreamStars" }},
            fg = {},
        },
        darkStars  = {
            bg = {{ _name = "StarField", starCount = 150, speed = -1.5, bgColor = "000000",
                    shootingStarColor = "ff4488", colors = "ccaaff", seed = 1 }},
            fg = {},
        },
        void       = {
            bg = {{ _name = "BlackholeBG", strength = "High" }},
            fg = {},
        },
        nightmare  = {
            bg = {{ _name = "parallax", texture = "bgs/10/sky", blendMode = "alphablend",
                    loopX = true, loopY = false, scrollX = 0.05, scrollY = 0.02,
                    x = 0, y = 0, color = "7a3040", alpha = 0.9, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        farewell   = {
            bg = {{ _name = "StarField", starCount = 80, speed = -0.5, bgColor = "000033",
                    shootingStarColor = "88ffff", colors = "aaffff", seed = 2 }},
            fg = {},
        },
        dream      = {
            bg = {{ _name = "DreamStars" },
                  { _name = "parallax", texture = "bgs/10/clouds", blendMode = "alphablend",
                    loopX = true, loopY = false, scrollX = 0.1, scrollY = 0.0,
                    x = 0, y = 0, color = "ffffff", alpha = 0.6, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        cave       = {
            bg = {{ _name = "parallax", texture = "bgs/08/bg0", blendMode = "alphablend",
                    loopX = true, loopY = true, scrollX = 0.05, scrollY = 0.05,
                    x = 0, y = 0, color = "ffffff", alpha = 1.0, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        ruins      = {
            bg = {{ _name = "parallax", texture = "bgs/02/bg0", blendMode = "alphablend",
                    loopX = true, loopY = true, scrollX = 0.08, scrollY = 0.04,
                    x = 0, y = 0, color = "aaaaaa", alpha = 0.8, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        castle     = {
            bg = {{ _name = "parallax", texture = "bgs/05/bg0", blendMode = "alphablend",
                    loopX = true, loopY = true, scrollX = 0.12, scrollY = 0.06,
                    x = 0, y = 0, color = "ffffff", alpha = 1.0, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
        ice        = {
            bg = {{ _name = "parallax", texture = "bgs/04/bg0", blendMode = "alphablend",
                    loopX = true, loopY = false, scrollX = 0.15, scrollY = 0.05,
                    x = 0, y = 0, color = "aaddff", alpha = 0.9, flipX = false, flipY = false,
                    instantIn = false, instantOut = false }},
            fg = {},
        },
    }

    local entry = styleMap[style]
    if entry then
        return { bg = entry.bg or {}, fg = entry.fg or {} }
    end
    return { bg = {}, fg = {} }
end

--- Generate background and foreground decal placements for a room.
-- Decals are small decorative sprites placed on solid tile surfaces.
-- @param matrix  Tile matrix
-- @param exits   Exit configuration
-- @param roomX   Room X offset in pixels
-- @param roomY   Room Y offset in pixels
-- @param style   Room style string
-- @param density Decal density (0.0 – 1.0, default 0.1)
-- @return table  { bgdecals={...}, fgdecals={...} }
function generator.generateDecals(matrix, exits, roomX, roomY, style, density)
    density = density or 0.1
    roomX = roomX or 0
    roomY = roomY or 0
    style = style or "normal"
    local w, h = matrix:size()

    -- Decal textures per style (foreground and background variants)
    local fgDecalSets = {
        normal    = { "scenery/rockBottom", "scenery/grass" },
        resort    = { "scenery/hotel/lobby", "scenery/resort" },
        temple    = { "scenery/temple/temple00", "scenery/temple/temple01" },
        reflection= { "scenery/fallA/bush00", "scenery/fallA/leaves" },
        summit    = { "scenery/summit/summitLog00", "scenery/summit/rock00" },
        cave      = { "scenery/darkness/stalactite", "scenery/darkness/mushroom" },
        ruins     = { "scenery/cliffside/rock00", "scenery/cliffside/rock01" },
        castle    = { "scenery/cliffside/rock02", "scenery/cliffside/rock03" },
        wind      = { "scenery/bridge/rail", "scenery/bridge/post" },
        ice       = { "scenery/snowberry", "scenery/coldDesert/rock00" },
        farewell  = { "scenery/rocket/rocket", "scenery/farewell/crystal" },
    }
    local bgDecalSets = {
        normal    = { "scenery/plant" },
        resort    = { "scenery/hotel/bg" },
        temple    = { "scenery/temple/bg" },
        cave      = { "scenery/darkness/bg" },
    }

    local fgSet = fgDecalSets[style] or fgDecalSets["normal"]
    local bgSet = bgDecalSets[style] or {}

    local fgdecals = {}
    local bgdecals = {}

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile  = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")

            -- Foreground decals: place on top of solid tiles with open air above
            if tile ~= "0" and above == "0" and #fgSet > 0 then
                if math.random() < density * 0.15 then
                    local tex = fgSet[math.random(#fgSet)]
                    table.insert(fgdecals, {
                        _name   = "decal",
                        texture = tex,
                        x       = roomX + (x - 1) * 8,
                        y       = roomY + (y - 1) * 8,
                        scaleX  = 1.0,
                        scaleY  = 1.0,
                        rotation = 0,
                    })
                end
            end

            -- Background decals: sparse placement in open air regions
            if tile == "0" and above == "0" and #bgSet > 0 then
                if math.random() < density * 0.03 then
                    local tex = bgSet[math.random(#bgSet)]
                    table.insert(bgdecals, {
                        _name   = "decal",
                        texture = tex,
                        x       = roomX + (x - 1) * 8,
                        y       = roomY + (y - 1) * 8,
                        scaleX  = 1.0,
                        scaleY  = 1.0,
                        rotation = 0,
                    })
                end
            end
        end
    end

    return { fgdecals = fgdecals, bgdecals = bgdecals }
end

--------------------------------------------------------------------------------
-- Entity Generation
--------------------------------------------------------------------------------

--- Generate entity placement data for a room based on the tile matrix.
-- Places spawn points, refills, springs etc. based on tile patterns.
-- @param matrix     Tile matrix of the room
-- @param exits      Exit configuration
-- @param roomX      Room X position in map coordinates (pixels)
-- @param roomY      Room Y position in map coordinates (pixels)
-- @param density    Entity density multiplier (0.0 - 1.0, default 0.3)
-- @return table     Array of entity data tables
function generator.generateEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.3
    roomX = roomX or 0
    roomY = roomY or 0
    local entities = {}
    local w, h = matrix:size()
    local nextId = 1

    local function addEntity(name, x, y, extra)
        local e = {
            _name = name,
            id = nextId,
            x = x,
            y = y,
            width = 0,
            height = 0,
            _type = "entity",
        }
        if extra then
            for k, v in pairs(extra) do
                e[k] = v
            end
        end
        table.insert(entities, e)
        nextId = nextId + 1
        return e
    end

    -- Place player spawn near the entrance exit
    local spawnX, spawnY

    if exits and exits.left then
        -- Spawn near left exit
        spawnX = 24
        spawnY = math.floor(h / 2) * 8
    elseif exits and exits.top then
        spawnX = math.floor(w / 2) * 8
        spawnY = 24
    else
        -- Default: mid-left area
        spawnX = 24
        spawnY = math.floor(h / 2) * 8
    end

    -- Find ground below spawn point
    local spawnTileX = math.floor(spawnX / 8) + 1
    local spawnTileY = math.floor(spawnY / 8) + 1
    for sy = spawnTileY, h do
        if matrix:get(spawnTileX, sy, "0") ~= "0" then
            spawnY = (sy - 1) * 8
            break
        end
    end

    addEntity("player", roomX + spawnX, roomY + spawnY)

    -- Scan for entity placement opportunities
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local tileAbove = matrix:get(x, y - 1, "0")
            local tileBelow = matrix:get(x, y + 1, "0")

            -- Platform top: solid with air above → candidate for entities on top
            if tile ~= "0" and tileAbove == "0" then
                local roll = math.random()

                if roll < density * 0.05 then
                    -- Place a refill (dash crystal)
                    addEntity("refill", roomX + (x - 1) * 8, roomY + (y - 2) * 8, {
                        oneUse = false,
                        twoDash = false,
                    })

                elseif roll < density * 0.10 then
                    -- Place a spring
                    addEntity("spring", roomX + (x - 1) * 8, roomY + (y - 1) * 8, {
                        orientation = 0
                    })
                end
            end

            -- Ceiling bottom: solid with air below → possible spike spot
            if tile ~= "0" and tileBelow == "0" then
                local roll = math.random()
                if roll < density * 0.08 then
                    addEntity("spikesDown", roomX + (x - 1) * 8, roomY + y * 8, {
                        width = 8,
                        ["type"] = "default",
                    })
                end
            end

            -- Large air gap check for springs/boosters
            if tile == "0" and tileAbove == "0" and tileBelow == "0" then
                local roll = math.random()
                if roll < density * 0.01 then
                    addEntity("booster", roomX + (x - 1) * 8, roomY + (y - 1) * 8, {
                        red = math.random() > 0.5,
                    })
                end
            end
        end
    end

    return entities
end

--------------------------------------------------------------------------------
-- Post-Processing
--------------------------------------------------------------------------------

--- Clean up generated room tiles: remove floating single tiles, fill tiny gaps.
-- @param matrix  Tile matrix to clean
-- @param passes  Number of passes (default 1)
function generator.cleanupTiles(matrix, passes)
    passes = passes or 1
    local w, h = matrix:size()

    for _ = 1, passes do
        for y = 2, h - 1 do
            for x = 2, w - 1 do
                local tile = matrix:get(x, y, "0")

                if tile ~= "0" then
                    -- Remove isolated solid tiles (no solid neighbors)
                    local neighbors = 0
                    if matrix:get(x - 1, y, "0") ~= "0" then neighbors = neighbors + 1 end
                    if matrix:get(x + 1, y, "0") ~= "0" then neighbors = neighbors + 1 end
                    if matrix:get(x, y - 1, "0") ~= "0" then neighbors = neighbors + 1 end
                    if matrix:get(x, y + 1, "0") ~= "0" then neighbors = neighbors + 1 end

                    if neighbors == 0 then
                        matrix:set(x, y, "0")
                    end

                else
                    -- Fill 1-tile air gaps surrounded by solids
                    local solidCount = 0
                    if matrix:get(x - 1, y, "0") ~= "0" then solidCount = solidCount + 1 end
                    if matrix:get(x + 1, y, "0") ~= "0" then solidCount = solidCount + 1 end
                    if matrix:get(x, y - 1, "0") ~= "0" then solidCount = solidCount + 1 end
                    if matrix:get(x, y + 1, "0") ~= "0" then solidCount = solidCount + 1 end

                    if solidCount >= 3 then
                        -- Get most common neighbor material
                        local materials = {}
                        for _, off in ipairs({{-1,0},{1,0},{0,-1},{0,1}}) do
                            local t = matrix:get(x + off[1], y + off[2], "0")
                            if t ~= "0" then
                                materials[t] = (materials[t] or 0) + 1
                            end
                        end
                        local best, bestCount = "1", 0
                        for mat, count in pairs(materials) do
                            if count > bestCount then
                                best = mat
                                bestCount = count
                            end
                        end
                        matrix:set(x, y, best)
                    end
                end
            end
        end
    end
end

--------------------------------------------------------------------------------
-- Space Erosion
--------------------------------------------------------------------------------

--- Erode solid tiles to create floating platform islands for space rooms.
-- Randomly removes a percentage of solid tiles, preferring tiles at edges
-- of solid clusters to create organic-looking floating platforms.
-- @param matrix        Tile matrix to erode
-- @param erodePercent  Fraction of solid tiles to remove (0.0 - 1.0)
function generator.erodeForSpace(matrix, erodePercent)
    erodePercent = erodePercent or 0.5
    local w, h = matrix:size()

    -- Collect solid tiles (not on border row/col)
    local solidTiles = {}
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            if matrix:get(x, y, "0") ~= "0" then
                table.insert(solidTiles, { x = x, y = y })
            end
        end
    end

    -- Shuffle (random_shuffle-style)
    randomUtils.random_shuffle(solidTiles)

    -- Score by how "edge-like" each tile is (fewer neighbors = erode first)
    for _, tile in ipairs(solidTiles) do
        local neighbors = 0
        if matrix:get(tile.x - 1, tile.y, "0") ~= "0" then neighbors = neighbors + 1 end
        if matrix:get(tile.x + 1, tile.y, "0") ~= "0" then neighbors = neighbors + 1 end
        if matrix:get(tile.x, tile.y - 1, "0") ~= "0" then neighbors = neighbors + 1 end
        if matrix:get(tile.x, tile.y + 1, "0") ~= "0" then neighbors = neighbors + 1 end
        tile.edgeScore = 4 - neighbors + math.random() * 0.5
    end

    table.sort(solidTiles, function(a, b) return a.edgeScore > b.edgeScore end)

    -- Remove tiles
    local toRemove = math.floor(#solidTiles * erodePercent)
    for i = 1, toRemove do
        local t = solidTiles[i]
        matrix:set(t.x, t.y, "0")
    end
end

--------------------------------------------------------------------------------
-- Space Entity Generation
--------------------------------------------------------------------------------

--- Generate entities suited for space/deep-space rooms.
-- Places dream blocks, floating refills, boosters, feathers, etc.
-- @param matrix     Tile matrix
-- @param exits      Exit configuration
-- @param roomX      Room X in pixels
-- @param roomY      Room Y in pixels
-- @param density    Entity density (0.0 - 1.0)
-- @param style      "space" or "deepSpace"
-- @return table     Array of entity tables
function generator.generateSpaceEntities(matrix, exits, roomX, roomY, density, style)
    density = density or 0.25
    roomX = roomX or 0
    roomY = roomY or 0
    style = style or "space"
    local entities = {}
    local w, h = matrix:size()
    local nextId = 1

    local function addEntity(name, x, y, extra)
        local e = {
            _name = name,
            id = nextId,
            x = x,
            y = y,
            width = 0,
            height = 0,
            _type = "entity",
        }
        if extra then
            for k, v in pairs(extra) do
                e[k] = v
            end
        end
        table.insert(entities, e)
        nextId = nextId + 1
        return e
    end

    -- Place player spawn
    local spawnX, spawnY
    if exits and exits.left then
        spawnX = 24
        spawnY = math.floor(h / 2) * 8
    elseif exits and exits.top then
        spawnX = math.floor(w / 2) * 8
        spawnY = 24
    else
        spawnX = 24
        spawnY = math.floor(h / 2) * 8
    end
    addEntity("player", roomX + spawnX, roomY + spawnY)

    -- Scan for placement opportunities
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local tileAbove = matrix:get(x, y - 1, "0")
            local tileBelow = matrix:get(x, y + 1, "0")
            local tileLeft  = matrix:get(x - 1, y, "0")
            local tileRight = matrix:get(x + 1, y, "0")

            -- On top of a floating platform
            if tile ~= "0" and tileAbove == "0" then
                local roll = math.random()

                if roll < density * 0.06 then
                    -- Refill (more common in space — need dashes)
                    addEntity("refill", roomX + (x - 1) * 8, roomY + (y - 2) * 8, {
                        oneUse = false,
                        twoDash = style == "deepSpace" and math.random() > 0.5,
                    })
                elseif roll < density * 0.10 then
                    -- Spring
                    addEntity("spring", roomX + (x - 1) * 8, roomY + (y - 1) * 8, {
                        orientation = 0,
                    })
                end
            end

            -- Large open void — space-specific entities
            if tile == "0" and tileAbove == "0" and tileBelow == "0"
               and tileLeft == "0" and tileRight == "0" then
                local roll = math.random()

                if roll < density * 0.015 then
                    -- Booster (red for deep space)
                    addEntity("booster", roomX + (x - 1) * 8, roomY + (y - 1) * 8, {
                        red = style == "deepSpace" or math.random() > 0.4,
                    })
                elseif roll < density * 0.025 then
                    -- Floating refill in the void
                    addEntity("refill", roomX + (x - 1) * 8, roomY + (y - 1) * 8, {
                        oneUse = false,
                        twoDash = style == "deepSpace",
                    })
                elseif roll < density * 0.04 and style == "deepSpace" then
                    -- Feather (fly through deep space)
                    addEntity("flyFeather", roomX + (x - 1) * 8, roomY + (y - 1) * 8, {
                        shielded = false,
                        singleUse = false,
                    })
                end
            end

            -- Dream blocks: check for 3x3+ solid clusters floating in air
            if tile ~= "0" and x <= w - 2 and y <= h - 2 then
                local is3x3 = true
                for dy = 0, 2 do
                    for dx = 0, 2 do
                        if matrix:get(x + dx, y + dy, "0") == "0" then
                            is3x3 = false
                        end
                    end
                end

                if is3x3 and math.random() < density * 0.03 then
                    local airAround = 0
                    for _, off in ipairs({{-1,0},{3,0},{0,-1},{0,3}}) do
                        if matrix:get(x + off[1], y + off[2], "0") == "0" then
                            airAround = airAround + 1
                        end
                    end
                    if airAround >= 3 then
                        local isFast = style == "deepSpace"
                        addEntity("dreamBlock", roomX + (x - 1) * 8, roomY + (y - 1) * 8, {
                            width = 24,
                            height = 24,
                            fastMoving = isFast,
                        })
                    end
                end
            end
        end
    end

    return entities
end

--------------------------------------------------------------------------------
-- Style-Specific Entity Generators
--------------------------------------------------------------------------------

-- Shared helper: create addEntity closure + spawn player
local function initEntityGen(matrix, exits, roomX, roomY)
    local entities = {}
    local w, h = matrix:size()
    local nextId = 1

    local function addEntity(name, x, y, extra)
        local e = {
            _name = name,
            id = nextId,
            x = x,
            y = y,
            width = 0,
            height = 0,
            _type = "entity",
        }
        if extra then for k, v in pairs(extra) do e[k] = v end end
        table.insert(entities, e)
        nextId = nextId + 1
        return e
    end

    -- Spawn player
    local spawnX, spawnY
    if exits and exits.left then
        spawnX = 24
        spawnY = math.floor(h / 2) * 8
    elseif exits and exits.top then
        spawnX = math.floor(w / 2) * 8
        spawnY = 24
    else
        spawnX = 24
        spawnY = math.floor(h / 2) * 8
    end
    for sy = math.floor(spawnY / 8) + 1, h do
        if matrix:get(math.floor(spawnX / 8) + 1, sy, "0") ~= "0" then
            spawnY = (sy - 1) * 8
            break
        end
    end
    addEntity("player", roomX + spawnX, roomY + spawnY)

    return entities, addEntity, w, h
end

--- Resort style: move blocks, dash blocks, Kevin blocks, springs, jump-thrus
function generator.generateResortEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.2
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.04 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.08 then
                    addEntity("spring", roomX + (x-1)*8, roomY + (y-1)*8, {orientation=0})
                elseif roll < density * 0.12 then
                    addEntity("jumpThru", roomX + (x-1)*8, roomY + (y-1)*8, {width=24, texture="default"})
                end
            end

            if tile == "0" and above == "0" and below == "0" then
                local roll = math.random()
                if roll < density * 0.015 then
                    addEntity("moveBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, direction="Right", canSteer=false, fast=false,
                    })
                elseif roll < density * 0.025 then
                    addEntity("dashBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, permanent=false, canDash=true,
                    })
                elseif roll < density * 0.03 then
                    addEntity("kevinsPC", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, axes="both",
                    })
                end
            end
        end
    end
    return entities
end

--- Temple style: touch switches, switch gates, torches, dash blocks
function generator.generateTempleEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.18
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)
    local switchCount = 0

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.03 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.06 and switchCount < 3 then
                    addEntity("touchSwitch", roomX + (x-1)*8, roomY + (y-2)*8)
                    switchCount = switchCount + 1
                elseif roll < density * 0.09 then
                    addEntity("torch", roomX + (x-1)*8, roomY + (y-2)*8, {startLit=false})
                end
            end

            if tile ~= "0" and below == "0" then
                if math.random() < density * 0.06 then
                    addEntity("spikesDown", roomX + (x-1)*8, roomY + y*8, {width=8, ["type"]="default"})
                end
            end

            if tile == "0" and above == "0" and below == "0" then
                if math.random() < density * 0.012 then
                    addEntity("dashBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=16, permanent=false, canDash=true,
                    })
                end
            end
        end
    end
    return entities
end

--- Reflection / Mirror style: seekers, feathers, dream blocks, seeker barriers
function generator.generateReflectionEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.2
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")
            local left  = matrix:get(x - 1, y, "0")
            local right = matrix:get(x + 1, y, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.05 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                end
            end

            if tile == "0" and above == "0" and below == "0" and left == "0" and right == "0" then
                local roll = math.random()
                if roll < density * 0.01 then
                    addEntity("seeker", roomX + (x-1)*8, roomY + (y-1)*8)
                elseif roll < density * 0.02 then
                    addEntity("flyFeather", roomX + (x-1)*8, roomY + (y-1)*8, {shielded=false, singleUse=false})
                elseif roll < density * 0.035 then
                    addEntity("dreamBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, fastMoving=false,
                    })
                end
            end

            -- Seeker barriers along walls
            if tile ~= "0" and (above == "0" or below == "0") then
                if math.random() < density * 0.008 then
                    addEntity("seekerBarrier", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=8, height=40,
                    })
                end
            end
        end
    end
    return entities
end

--- Summit style: spinners, bumpers, clouds, boosters, zip movers
function generator.generateSummitEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.22
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")
            local left  = matrix:get(x - 1, y, "0")
            local right = matrix:get(x + 1, y, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.04 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.07 then
                    addEntity("spring", roomX + (x-1)*8, roomY + (y-1)*8, {orientation=0})
                elseif roll < density * 0.10 then
                    addEntity("spinner", roomX + (x-1)*8, roomY + (y-2)*8, {attachToSolid=false})
                end
            end

            if tile == "0" and above == "0" and below == "0" and left == "0" and right == "0" then
                local roll = math.random()
                if roll < density * 0.012 then
                    addEntity("booster", roomX + (x-1)*8, roomY + (y-1)*8, {red=math.random()>0.4})
                elseif roll < density * 0.020 then
                    addEntity("cloud", roomX + (x-1)*8, roomY + (y-1)*8, {fragile=math.random()>0.6})
                elseif roll < density * 0.026 then
                    addEntity("bounceBlock", roomX + (x-1)*8, roomY + (y-1)*8, {width=24, height=24})
                end
            end

            -- Zip movers near walls
            if tile ~= "0" and above == "0" and x < w - 4 then
                if math.random() < density * 0.006 then
                    addEntity("zipMover", roomX + (x-1)*8, roomY + (y-2)*8, {
                        width=16, height=16,
                        nodes = {{x = roomX + (x+3)*8, y = roomY + (y-2)*8}},
                    })
                end
            end
        end
    end
    return entities
end

--- Core style: core blocks, fire barriers, hot/cold toggle, boosters
function generator.generateCoreEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.2
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")
            local left  = matrix:get(x - 1, y, "0")
            local right = matrix:get(x + 1, y, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.04 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.08 then
                    addEntity("spring", roomX + (x-1)*8, roomY + (y-1)*8, {orientation=0})
                end
            end

            if tile == "0" and above == "0" and below == "0" then
                local roll = math.random()
                if roll < density * 0.01 then
                    addEntity("booster", roomX + (x-1)*8, roomY + (y-1)*8, {red=true})
                elseif roll < density * 0.025 then
                    addEntity("coreBlock", roomX + (x-1)*8, roomY + (y-1)*8, {width=24, height=24})
                elseif roll < density * 0.035 then
                    addEntity("lightning", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=48, perLevel=false, moveTime=3.0,
                    })
                end
            end

            -- Fire spikes
            if tile ~= "0" and below == "0" then
                if math.random() < density * 0.06 then
                    addEntity("spikesDown", roomX + (x-1)*8, roomY + y*8, {width=8, ["type"]="default"})
                end
            end
        end
    end
    return entities
end

--- Wind style: crumbling blocks, clouds, springs, with windPattern metadata
function generator.generateWindEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.2
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")
            local left  = matrix:get(x - 1, y, "0")
            local right = matrix:get(x + 1, y, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.05 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.10 then
                    addEntity("spring", roomX + (x-1)*8, roomY + (y-1)*8, {orientation=0})
                end
            end

            if tile == "0" and above == "0" and below == "0" and left == "0" and right == "0" then
                local roll = math.random()
                if roll < density * 0.02 then
                    addEntity("cloud", roomX + (x-1)*8, roomY + (y-1)*8, {fragile=math.random()>0.5})
                elseif roll < density * 0.03 then
                    addEntity("crumbleWallOnRumble", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=16,
                    })
                end
            end

            -- Falling blocks
            if tile ~= "0" and above == "0" and below == "0" then
                if math.random() < density * 0.01 then
                    addEntity("fallingBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=16, climbFall=true, behind=false,
                    })
                end
            end
        end
    end
    return entities
end

--- Ice style: ice walls, springs, spinners, slippery surfaces
function generator.generateIceEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.18
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.04 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.08 then
                    addEntity("spring", roomX + (x-1)*8, roomY + (y-1)*8, {orientation=0})
                elseif roll < density * 0.12 then
                    addEntity("spinner", roomX + (x-1)*8, roomY + (y-2)*8, {attachToSolid=false})
                end
            end

            if tile == "0" and above == "0" and below == "0" then
                local roll = math.random()
                if roll < density * 0.01 then
                    addEntity("MaggyHelper/IcePlatform", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=32, height=8,
                    })
                elseif roll < density * 0.02 then
                    addEntity("bounceBlock", roomX + (x-1)*8, roomY + (y-1)*8, {width=24, height=24})
                end
            end
        end
    end
    return entities
end

--- Cave / underground style: crystal spinners, falling blocks, torches, darkness
function generator.generateCaveEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.18
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.04 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.07 then
                    addEntity("torch", roomX + (x-1)*8, roomY + (y-2)*8, {startLit=false})
                elseif roll < density * 0.10 then
                    addEntity("MaggyHelper/SafeCrystalSpinner", roomX + (x-1)*8, roomY + (y-2)*8)
                end
            end

            -- Stalactites (falling blocks from ceiling)
            if tile ~= "0" and below == "0" then
                if math.random() < density * 0.008 then
                    addEntity("fallingBlock", roomX + (x-1)*8, roomY + y*8, {
                        width=8, height=16, climbFall=true, behind=false,
                    })
                end
            end

            if tile == "0" and above == "0" and below == "0" then
                if math.random() < density * 0.008 then
                    addEntity("booster", roomX + (x-1)*8, roomY + (y-1)*8, {red=false})
                end
            end
        end
    end
    return entities
end

--- Ruins / ancient style: dash code gates, rune stones, dash blocks
function generator.generateRuinsEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.18
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)
    local switchCount = 0

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.04 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.07 and switchCount < 3 then
                    addEntity("touchSwitch", roomX + (x-1)*8, roomY + (y-2)*8)
                    switchCount = switchCount + 1
                elseif roll < density * 0.10 then
                    addEntity("dashBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=16, permanent=false, canDash=true,
                    })
                end
            end

            if tile ~= "0" and below == "0" then
                if math.random() < density * 0.05 then
                    addEntity("spikesDown", roomX + (x-1)*8, roomY + y*8, {width=8, ["type"]="default"})
                end
            end

            if tile == "0" and above == "0" and below == "0" then
                if math.random() < density * 0.008 then
                    addEntity("crumbleWallOnRumble", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=16,
                    })
                end
            end
        end
    end
    return entities
end

--- Castle style: swap blocks, move blocks, Kevin blocks, spikes
function generator.generateCastleEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.2
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.04 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.07 then
                    addEntity("spring", roomX + (x-1)*8, roomY + (y-1)*8, {orientation=0})
                elseif roll < density * 0.11 then
                    addEntity("spikesUp", roomX + (x-1)*8, roomY + (y-1)*8, {width=8, ["type"]="default"})
                end
            end

            if tile == "0" and above == "0" and below == "0" then
                local roll = math.random()
                if roll < density * 0.012 then
                    addEntity("swapBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=16,
                        nodes={{x=roomX+(x+3)*8, y=roomY+(y-1)*8}},
                    })
                elseif roll < density * 0.02 then
                    addEntity("moveBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, direction="Right", canSteer=false, fast=false,
                    })
                elseif roll < density * 0.025 then
                    addEntity("kevinsPC", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, axes="both",
                    })
                end
            end
        end
    end
    return entities
end

--- Dark Stars (Chapter 20 "The Last Push") style:
-- Void tiles, dark atmosphere, star jump blocks, determination orbs,
-- dream blocks, spinners, boosters. Aggressive hazards, sparse platforms.
function generator.generateDarkStarsEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.28
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")
            local left  = matrix:get(x - 1, y, "0")
            local right = matrix:get(x + 1, y, "0")

            -- On floating platforms
            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.05 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=true})
                elseif roll < density * 0.09 then
                    addEntity("spinner", roomX + (x-1)*8, roomY + (y-2)*8, {attachToSolid=false})
                elseif roll < density * 0.12 then
                    addEntity("starJumpBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=16, sinks=true,
                    })
                end
            end

            -- Open void areas
            if tile == "0" and above == "0" and below == "0" and left == "0" and right == "0" then
                local roll = math.random()
                if roll < density * 0.018 then
                    addEntity("booster", roomX + (x-1)*8, roomY + (y-1)*8, {red=true})
                elseif roll < density * 0.028 then
                    addEntity("dreamBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, fastMoving=true,
                    })
                elseif roll < density * 0.035 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-1)*8, {
                        oneUse=false, twoDash=true,
                    })
                elseif roll < density * 0.045 then
                    addEntity("flyFeather", roomX + (x-1)*8, roomY + (y-1)*8, {
                        shielded=false, singleUse=false,
                    })
                elseif roll < density * 0.05 then
                    addEntity("MaggyHelper/DeterminationOrb", roomX + (x-1)*8, roomY + (y-1)*8)
                end
            end

            -- Spinners attached to ceilings
            if tile ~= "0" and below == "0" then
                if math.random() < density * 0.04 then
                    addEntity("spinner", roomX + (x-1)*8, roomY + y*8, {attachToSolid=true})
                end
            end
        end
    end
    return entities
end

--- Void / abyss style: nightmare blocks, void tendrils, seekers, very dark
function generator.generateVoidEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.22
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")
            local left  = matrix:get(x - 1, y, "0")
            local right = matrix:get(x + 1, y, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.05 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=true})
                elseif roll < density * 0.08 then
                    addEntity("spring", roomX + (x-1)*8, roomY + (y-1)*8, {orientation=0})
                end
            end

            if tile == "0" and above == "0" and below == "0" and left == "0" and right == "0" then
                local roll = math.random()
                if roll < density * 0.01 then
                    addEntity("seeker", roomX + (x-1)*8, roomY + (y-1)*8)
                elseif roll < density * 0.02 then
                    addEntity("dreamBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, fastMoving=true,
                    })
                elseif roll < density * 0.028 then
                    addEntity("booster", roomX + (x-1)*8, roomY + (y-1)*8, {red=true})
                elseif roll < density * 0.035 then
                    addEntity("flyFeather", roomX + (x-1)*8, roomY + (y-1)*8, {shielded=false, singleUse=false})
                end
            end

            -- Void tendrils from walls
            if tile ~= "0" and (above == "0" or below == "0") then
                if math.random() < density * 0.006 then
                    addEntity("MaggyHelper/VoidTendril", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=8, height=32,
                    })
                end
            end
        end
    end
    return entities
end

--- Nightmare style: nightmare blocks, darkness, spinners, seekers, lightning
function generator.generateNightmareEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.24
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")
            local left  = matrix:get(x - 1, y, "0")
            local right = matrix:get(x + 1, y, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.04 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=true})
                elseif roll < density * 0.08 then
                    addEntity("spinner", roomX + (x-1)*8, roomY + (y-2)*8, {attachToSolid=false})
                end
            end

            if tile == "0" and above == "0" and below == "0" then
                local roll = math.random()
                if roll < density * 0.012 then
                    addEntity("lightning", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=48, perLevel=false, moveTime=2.5,
                    })
                elseif roll < density * 0.024 then
                    addEntity("nightmareBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24,
                    })
                elseif roll < density * 0.03 then
                    addEntity("seeker", roomX + (x-1)*8, roomY + (y-1)*8)
                end
            end

            if tile ~= "0" and below == "0" then
                if math.random() < density * 0.05 then
                    addEntity("spikesDown", roomX + (x-1)*8, roomY + y*8, {width=8, ["type"]="default"})
                end
            end
        end
    end
    return entities
end

--- Farewell (Ch9) style: floaty space blocks, jellyfish, feathers, boosters, dream blocks
function generator.generateFarewellEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.25
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")
            local left  = matrix:get(x - 1, y, "0")
            local right = matrix:get(x + 1, y, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.05 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=true})
                elseif roll < density * 0.08 then
                    addEntity("spring", roomX + (x-1)*8, roomY + (y-1)*8, {orientation=0})
                end
            end

            if tile == "0" and above == "0" and below == "0" and left == "0" and right == "0" then
                local roll = math.random()
                if roll < density * 0.015 then
                    addEntity("booster", roomX + (x-1)*8, roomY + (y-1)*8, {red=math.random()>0.3})
                elseif roll < density * 0.025 then
                    addEntity("floatySpaceBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, tiletype="3",
                    })
                elseif roll < density * 0.035 then
                    addEntity("flyFeather", roomX + (x-1)*8, roomY + (y-1)*8, {shielded=false, singleUse=false})
                elseif roll < density * 0.045 then
                    addEntity("dreamBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=24, height=24, fastMoving=math.random()>0.5,
                    })
                elseif roll < density * 0.05 then
                    addEntity("glider", roomX + (x-1)*8, roomY + (y-1)*8, {bubble=true, tutorial=false})
                end
            end
        end
    end
    return entities
end

--- Dream / Hopes and Dreams style: dream blocks, star jump blocks, boosters, orbs
function generator.generateDreamEntities(matrix, exits, roomX, roomY, density)
    density = density or 0.22
    roomX = roomX or 0; roomY = roomY or 0
    local entities, addEntity, w, h = initEntityGen(matrix, exits, roomX, roomY)

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local tile = matrix:get(x, y, "0")
            local above = matrix:get(x, y - 1, "0")
            local below = matrix:get(x, y + 1, "0")
            local left  = matrix:get(x - 1, y, "0")
            local right = matrix:get(x + 1, y, "0")

            if tile ~= "0" and above == "0" then
                local roll = math.random()
                if roll < density * 0.05 then
                    addEntity("refill", roomX + (x-1)*8, roomY + (y-2)*8, {oneUse=false, twoDash=false})
                elseif roll < density * 0.09 then
                    addEntity("starJumpBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=16, height=16, sinks=false,
                    })
                end
            end

            if tile == "0" and above == "0" and below == "0" and left == "0" and right == "0" then
                local roll = math.random()
                if roll < density * 0.02 then
                    addEntity("dreamBlock", roomX + (x-1)*8, roomY + (y-1)*8, {
                        width=32, height=32, fastMoving=false,
                    })
                elseif roll < density * 0.03 then
                    addEntity("booster", roomX + (x-1)*8, roomY + (y-1)*8, {red=false})
                elseif roll < density * 0.04 then
                    addEntity("MaggyHelper/DreamOrb", roomX + (x-1)*8, roomY + (y-1)*8)
                elseif roll < density * 0.05 then
                    addEntity("cloud", roomX + (x-1)*8, roomY + (y-1)*8, {fragile=false})
                end
            end
        end
    end
    return entities
end

--- Dispatch function: routes roomStyle to the appropriate entity generator.
-- @param style    Room style string
-- @param matrix   Tile matrix
-- @param exits    Exit configuration
-- @param roomX    Room X in pixels
-- @param roomY    Room Y in pixels
-- @param density  Entity density
-- @return table   Entity list
function generator.generateEntitiesForStyle(matrix, exits, roomX, roomY, density, style)
    style = style or "normal"

    local generators = {
        normal     = generator.generateEntities,
        resort     = generator.generateResortEntities,
        temple     = generator.generateTempleEntities,
        reflection = generator.generateReflectionEntities,
        summit    = generator.generateSummitEntities,
        core       = generator.generateCoreEntities,
        wind       = generator.generateWindEntities,
        ice        = generator.generateIceEntities,
        cave       = generator.generateCaveEntities,
        ruins      = generator.generateRuinsEntities,
        castle     = generator.generateCastleEntities,
        darkStars  = generator.generateDarkStarsEntities,
        void       = generator.generateVoidEntities,
        nightmare  = generator.generateNightmareEntities,
        farewell   = generator.generateFarewellEntities,
        dream      = generator.generateDreamEntities,
        space      = generator.generateSpaceEntities,
        deepSpace  = function(m, e, rx, ry, d)
            return generator.generateSpaceEntities(m, e, rx, ry, d, "deepSpace")
        end,
    }

    local fn = generators[style]
    if fn then
        return fn(matrix, exits, roomX, roomY, density)
    end
    -- Fallback to normal
    return generator.generateEntities(matrix, exits, roomX, roomY, density)
end

--------------------------------------------------------------------------------
-- Pattern Integration
--------------------------------------------------------------------------------

--- Apply a named pattern to a tile matrix and optionally generate pattern entities.
-- This is the entry point called by init.lua when a pattern is requested.
-- It carves the pattern layout into the matrix, then returns entity tables.
--
-- @param matrix       Tile matrix (already filled by Markov)
-- @param patternName  Pattern name string (e.g. "kirbyBossArena")
-- @param exits        Exit configuration { left=bool, right=bool, top=bool, bottom=bool }
-- @param roomX        Room X offset in pixels (for entity positioning)
-- @param roomY        Room Y offset in pixels (for entity positioning)
-- @param patternOpts  Options passed to pattern functions (difficulty, bossType, etc.)
-- @return table|nil   Entity list from pattern, or nil if generic entities should be used
-- @return boolean     true if pattern was successfully applied
function generator.applyPattern(matrix, patternName, exits, roomX, roomY, patternOpts)
    -- Lazy-load patterns module to avoid circular dependencies
    local ok, patterns = pcall(function()
        local mods_ok, mods = pcall(require, "mods")
        if mods_ok and mods and mods.requireFromPlugin then
            local result = mods.requireFromPlugin("libraries.pcg.patterns")
            if result then return result end
        end
        return require("libraries.pcg.patterns")
    end)

    if not ok or not patterns then
        return nil, false
    end

    -- Apply tile pattern
    local applied = patterns.apply(matrix, patternName, exits, patternOpts)
    if not applied then
        return nil, false
    end

    -- Get pattern-specific entities
    local w, h = matrix:size()
    local patternEntities = patterns.getEntities(patternName, w, h, patternOpts)

    if patternEntities then
        -- Offset entity positions to room world coordinates
        roomX = roomX or 0
        roomY = roomY or 0
        for _, entity in ipairs(patternEntities) do
            entity.x = entity.x + roomX
            entity.y = entity.y + roomY
        end
        return patternEntities, true
    end

    return nil, true  -- pattern applied to tiles, but no custom entities
end

--------------------------------------------------------------------------------
-- Spike Placement (wall-attached)
--------------------------------------------------------------------------------

--- Add spikes along walls where appropriate.
-- @param entities  Entity list to append to
-- @param matrix    Tile matrix
-- @param roomX     Room X offset
-- @param roomY     Room Y offset
-- @param density   Spike density (0.0 - 1.0, default 0.15)
function generator.placeWallSpikes(entities, matrix, roomX, roomY, density)
    density = density or 0.15
    local w, h = matrix:size()
    local nextId = #entities + 1

    for y = 2, h - 1 do
        for x = 2, w - 1 do
            if matrix:get(x, y, "0") == "0" then
                local roll = math.random()
                if roll >= density then goto continue end

                -- Check for adjacent walls to attach spikes
                if matrix:get(x, y + 1, "0") ~= "0" then
                    -- Floor spike (spikes up)
                    table.insert(entities, {
                        _name = "spikesUp",
                        id = nextId,
                        x = roomX + (x - 1) * 8,
                        y = roomY + y * 8,
                        width = 8,
                        height = 0,
                        _type = "entity",
                        ["type"] = "default",
                    })
                    nextId = nextId + 1

                elseif matrix:get(x - 1, y, "0") ~= "0" then
                    -- Left wall spike (spikes right)
                    table.insert(entities, {
                        _name = "spikesRight",
                        id = nextId,
                        x = roomX + (x - 1) * 8,
                        y = roomY + (y - 1) * 8,
                        width = 0,
                        height = 8,
                        _type = "entity",
                        ["type"] = "default",
                    })
                    nextId = nextId + 1
                end

                ::continue::
            end
        end
    end
end

return generator
