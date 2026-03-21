-- PCG Map Builder
-- Converts PCG output (tile matrices + entity lists + skeleton) into
-- the Celeste map element tree ready for binary encoding.
--
-- Usage:
--   local mapBuilder = require("libraries.pcg.map_builder")
--   local mapData = mapBuilder.buildMap(pcgLevel, "Maggy/PCG/Generated")
--   binEncoder.encodeFile("map.bin", mapData)

local mapBuilder = {}

--------------------------------------------------------------------------------
-- Tile Matrix → Tile String
--------------------------------------------------------------------------------

--- Convert a tile matrix to the newline-separated tile string format.
-- Trims trailing empty rows and columns for smaller output.
-- @param matrix   Tile matrix with :get(x,y) and :size()
-- @param empty    Empty tile character (default "0")
-- @return string  Tile data string
function mapBuilder.matrixToTileString(matrix, empty)
    empty = empty or "0"
    local w, h = matrix:size()

    -- Find last relevant row (any non-empty tile)
    local lastRow = 0
    for y = 1, h do
        for x = 1, w do
            if matrix:get(x, y, empty) ~= empty then
                lastRow = y
                break
            end
        end
    end
    if lastRow == 0 then lastRow = 1 end

    -- Find last relevant column per row
    local lines = {}
    for y = 1, lastRow do
        local lastCol = 0
        for x = w, 1, -1 do
            if matrix:get(x, y, empty) ~= empty then
                lastCol = x
                break
            end
        end
        if lastCol == 0 then lastCol = 1 end

        local row = {}
        for x = 1, lastCol do
            row[x] = matrix:get(x, y, empty)
        end
        lines[y] = table.concat(row)
    end

    return table.concat(lines, "\n")
end

--- Build empty object tiles string (comma-separated -1 values).
-- @param w   Width in tiles
-- @param h   Height in tiles
-- @return string
function mapBuilder.emptyObjectTiles(w, h)
    local row = {}
    for x = 1, w do
        row[x] = "-1"
    end
    local rowStr = table.concat(row, ",")
    local lines = {}
    for y = 1, h do
        lines[y] = rowStr
    end
    return table.concat(lines, "\n")
end

--------------------------------------------------------------------------------
-- Entity → Element
--------------------------------------------------------------------------------

--- Convert a PCG entity table to a binary-compatible element.
-- @param entity   Entity with _name, id, x, y, width, height, + custom attrs
-- @return table   Element tree node
function mapBuilder.entityToElement(entity)
    local elem = {
        __name = entity._name or "unknown",
        __children = {},
        id = entity.id or 0,
        x = entity.x or 0,
        y = entity.y or 0,
    }

    -- Copy standard fields
    if entity.width and entity.width ~= 0 then elem.width = entity.width end
    if entity.height and entity.height ~= 0 then elem.height = entity.height end

    -- Copy custom attributes
    for k, v in pairs(entity) do
        if k ~= "_name" and k ~= "_type" and k ~= "id" and k ~= "x" and k ~= "y"
           and k ~= "width" and k ~= "height" and k ~= "__name" and k ~= "__children" then
            elem[k] = v
        end
    end

    -- Handle nodes (if any)
    if entity.nodes then
        for _, node in ipairs(entity.nodes) do
            table.insert(elem.__children, {
                __name = "node",
                x = node.x or 0,
                y = node.y or 0,
            })
        end
    end

    return elem
end

--------------------------------------------------------------------------------
-- Room → Level Element
--------------------------------------------------------------------------------

--- Convert a PCG room to a "level" element.
-- @param roomData  PCG room data { matrix, entities, width, height, exits, x, y, name }
-- @param index     Room index (for default naming)
-- @return table    Level element
function mapBuilder.roomToLevel(roomData, index)
    local tilesW = roomData.width or 40
    local tilesH = roomData.height or 23
    local pixW = tilesW * 8
    local pixH = tilesH * 8
    local roomX = roomData.x or 0
    local roomY = roomData.y or 0

    -- Room name
    local name = roomData.name or string.format("pcg-%02d", index or 0)
    if name:sub(1, 4) ~= "lvl_" then
        name = "lvl_" .. name
    end

    -- Tile strings
    local fgTiles = mapBuilder.matrixToTileString(roomData.matrix)
    local bgTiles = string.rep("0", tilesW)  -- empty background by default
    local objTiles = mapBuilder.emptyObjectTiles(tilesW, tilesH)

    -- Build entity children
    local entityChildren = {}
    for _, entity in ipairs(roomData.entities or {}) do
        table.insert(entityChildren, mapBuilder.entityToElement(entity))
    end

    -- Determine room flags based on style
    local style = roomData.roomStyle or "normal"
    local darkStyles = {
        deepSpace=true, darkStars=true, void=true, nightmare=true, cave=true,
    }
    local spaceStyles = {
        space=true, deepSpace=true, darkStars=true, void=true, farewell=true,
    }
    local windPatterns = {
        wind = "Left",
    }

    -- Level element
    local level = {
        __name = "level",
        name = name,
        x = roomX,
        y = roomY,
        width = pixW,
        height = pixH,
        -- Music/atmosphere defaults
        music = "",
        alt_music = "",
        ambience = "",
        dark = darkStyles[style] or false,
        space = spaceStyles[style] or false,
        underwater = false,
        whisper = style == "nightmare" or style == "void",
        disableDownTransition = false,
        windPattern = windPatterns[style] or "None",
        musicLayer1 = true,
        musicLayer2 = true,
        musicLayer3 = true,
        musicLayer4 = true,
        musicProgress = "",
        ambienceProgress = "",
        cameraOffsetX = 0,
        cameraOffsetY = 0,
        delayAltMusicFade = false,
        c = 0,
        __children = {
            -- Foreground tiles
            {
                __name = "solids",
                innerText = fgTiles,
                offsetX = 0,
                offsetY = 0,
            },
            -- Background tiles
            {
                __name = "bg",
                innerText = bgTiles,
                offsetX = 0,
                offsetY = 0,
            },
            -- Object tiles
            {
                __name = "objtiles",
                innerText = objTiles,
                offsetX = 0,
                offsetY = 0,
                tileset = "scenery",
            },
            {
                __name = "fgtiles",
                innerText = objTiles,
                offsetX = 0,
                offsetY = 0,
                tileset = "scenery",
            },
            {
                __name = "bgtiles",
                innerText = objTiles,
                offsetX = 0,
                offsetY = 0,
                tileset = "scenery",
            },
            -- Entities
            {
                __name = "entities",
                __children = entityChildren,
            },
            -- Triggers (empty)
            {
                __name = "triggers",
                __children = {},
            },
            -- Decals (empty)
            {
                __name = "fgdecals",
                __children = {},
            },
            {
                __name = "bgdecals",
                __children = {},
            },
        },
    }

    return level
end

--------------------------------------------------------------------------------
-- Full Map Build
--------------------------------------------------------------------------------

--- Build a complete Celeste map element tree from PCG level output.
-- @param levelData  PCG level data from pcg.generateLevel() or manual assembly
--                   { rooms = {roomData...}, skeleton = skel }
-- @param packageName  Map package name (e.g. "Maggy/PCG/Generated_01")
-- @return table       Root map element tree (ready for binEncoder.encodeFile)
function mapBuilder.buildMap(levelData, packageName)
    packageName = packageName or "Maggy/PCG/Generated"

    local levelChildren = {}
    for i, roomData in ipairs(levelData.rooms) do
        table.insert(levelChildren, mapBuilder.roomToLevel(roomData, i))
    end

    -- Filler rectangles (empty for PCG — Celeste uses these for map borders)
    local fillerChildren = {}

    -- Style (minimal defaults)
    local style = {
        __name = "Style",
        __children = {
            {
                __name = "Foregrounds",
                __children = {},
            },
            {
                __name = "Backgrounds",
                __children = {},
            },
        },
    }

    -- Root map element
    local mapData = {
        __name = "Map",
        _package = packageName,
        __children = {
            {
                __name = "Filler",
                __children = fillerChildren,
            },
            {
                __name = "levels",
                __children = levelChildren,
            },
            style,
        },
    }

    return mapData
end

--- Build a map from a single room (convenience function).
-- @param roomData     Single PCG room data
-- @param packageName  Map package name
-- @return table       Root map element tree
function mapBuilder.buildSingleRoomMap(roomData, packageName)
    return mapBuilder.buildMap({ rooms = { roomData } }, packageName)
end

return mapBuilder
