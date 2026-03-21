-- PCG Cell Catalog: Spelunky-Style Cell Library from Existing Rooms
--
-- Extracts fixed-size "cells" (8×8 tile chunks = 64×64 px) from hand-made
-- rooms in the currently loaded map. Each cell is classified by its four
-- edges (open/closed) so the assembler can pick compatible neighbours.
--
-- The idea mirrors Spelunky's level generator:
--   https://tinysubversions.com/spelunkyGen/
--   https://tinysubversions.com/spelunkyGen2/
--
-- Spelunky divides each level into a grid of fixed-size rooms (cells).
-- Each cell has openings on 0-4 sides. A "solution path" guarantees the
-- player can reach the exit, and the remaining cells are filled from a
-- compatible pool.
--
-- Here, we harvest cells directly from the Lönn map editor — every room
-- you design becomes a source of reusable cell templates without building
-- a complete map.
--
-- Cell size can be configured (default 8 tiles = 64 px per side, matching
-- the shortlist docs). Larger cells (16×16, 32×32) are also supported.

local cellCatalog = {}

--------------------------------------------------------------------------------
-- Constants
--------------------------------------------------------------------------------

cellCatalog.DEFAULT_CELL_SIZE = 8   -- tiles per cell side (8 tiles × 8 px = 64 px)
cellCatalog.TILE_AIR = "0"

-- Edge classification thresholds:
-- An edge is "open" when at least this fraction of its border tiles are air.
cellCatalog.OPEN_THRESHOLD = 0.35

--------------------------------------------------------------------------------
-- Edge Detection
--------------------------------------------------------------------------------

--- Classify one edge of a cell sub-matrix as open or closed.
-- @param matrix     Full room tile matrix (has :get(x,y) and :size())
-- @param startX     Top-left X of the cell in the matrix (1-based)
-- @param startY     Top-left Y of the cell in the matrix (1-based)
-- @param cellW      Cell width in tiles
-- @param cellH      Cell height in tiles
-- @param side       "top" | "bottom" | "left" | "right"
-- @return boolean   true = open
local function isEdgeOpen(matrix, startX, startY, cellW, cellH, side)
    local airCount = 0
    local total = 0

    if side == "top" then
        for dx = 0, cellW - 1 do
            local x = startX + dx
            local y = startY
            local t = matrix:get(x, y, cellCatalog.TILE_AIR)
            if t == cellCatalog.TILE_AIR then airCount = airCount + 1 end
            total = total + 1
        end
    elseif side == "bottom" then
        for dx = 0, cellW - 1 do
            local x = startX + dx
            local y = startY + cellH - 1
            local t = matrix:get(x, y, cellCatalog.TILE_AIR)
            if t == cellCatalog.TILE_AIR then airCount = airCount + 1 end
            total = total + 1
        end
    elseif side == "left" then
        for dy = 0, cellH - 1 do
            local x = startX
            local y = startY + dy
            local t = matrix:get(x, y, cellCatalog.TILE_AIR)
            if t == cellCatalog.TILE_AIR then airCount = airCount + 1 end
            total = total + 1
        end
    elseif side == "right" then
        for dy = 0, cellH - 1 do
            local x = startX + cellW - 1
            local y = startY + dy
            local t = matrix:get(x, y, cellCatalog.TILE_AIR)
            if t == cellCatalog.TILE_AIR then airCount = airCount + 1 end
            total = total + 1
        end
    end

    if total == 0 then return false end
    return (airCount / total) >= cellCatalog.OPEN_THRESHOLD
end

--- Get a 4-bit edge signature for a cell.
-- Bit layout (matches Spelunky convention):
--   bit 0 (1)  = top open
--   bit 1 (2)  = right open
--   bit 2 (4)  = bottom open
--   bit 3 (8)  = left open
-- @return number  0..15
local function edgeSignature(matrix, startX, startY, cellW, cellH)
    local sig = 0
    if isEdgeOpen(matrix, startX, startY, cellW, cellH, "top")    then sig = sig + 1 end
    if isEdgeOpen(matrix, startX, startY, cellW, cellH, "right")  then sig = sig + 2 end
    if isEdgeOpen(matrix, startX, startY, cellW, cellH, "bottom") then sig = sig + 4 end
    if isEdgeOpen(matrix, startX, startY, cellW, cellH, "left")   then sig = sig + 8 end
    return sig
end

--------------------------------------------------------------------------------
-- Cell Extraction
--------------------------------------------------------------------------------

--- Extract all cells from a single room's tile matrix.
-- @param room       Lönn room table
-- @param cellSize   Cell size in tiles (default 8)
-- @param layer      "fg" or "bg" (default "fg")
-- @return table     List of cell descriptors:
--   { matrix=sub_matrix, edges={top,right,bottom,left}, sig=0..15,
--     roomName=string, gridCol=int, gridRow=int, area=string,
--     entities={}, airRatio=number }
function cellCatalog.extractCellsFromRoom(room, cellSize, layer)
    cellSize = cellSize or cellCatalog.DEFAULT_CELL_SIZE
    layer = layer or "fg"

    local tileMatrix
    if layer == "fg" then
        tileMatrix = room.tilesFg and room.tilesFg.matrix or nil
    else
        tileMatrix = room.tilesBg and room.tilesBg.matrix or nil
    end
    if not tileMatrix then return {} end

    local mw, mh = tileMatrix:size()
    local cellsX = math.floor(mw / cellSize)
    local cellsY = math.floor(mh / cellSize)

    local cells = {}
    local roomName = room.name or "unknown"

    for cy = 0, cellsY - 1 do
        for cx = 0, cellsX - 1 do
            local startX = cx * cellSize + 1  -- 1-based
            local startY = cy * cellSize + 1

            -- Extract sub-tile data as a plain 2D table
            local tileData = {}
            local airCount = 0
            local totalTiles = cellSize * cellSize

            for dy = 0, cellSize - 1 do
                local row = {}
                for dx = 0, cellSize - 1 do
                    local t = tileMatrix:get(startX + dx, startY + dy, cellCatalog.TILE_AIR)
                    row[dx + 1] = t
                    if t == cellCatalog.TILE_AIR then airCount = airCount + 1 end
                end
                tileData[dy + 1] = row
            end

            local airRatio = airCount / totalTiles

            -- Skip cells that are completely empty or completely solid
            if airRatio > 0.02 and airRatio < 0.98 then
                local sig = edgeSignature(tileMatrix, startX, startY, cellSize, cellSize)

                local edges = {
                    top    = (sig % 2) >= 1,
                    right  = (math.floor(sig / 2) % 2) >= 1,
                    bottom = (math.floor(sig / 4) % 2) >= 1,
                    left   = (math.floor(sig / 8) % 2) >= 1,
                }

                -- Collect entities that fall within this cell
                local cellEntities = {}
                local cellPixelX = (startX - 1) * 8
                local cellPixelY = (startY - 1) * 8
                local cellPixelW = cellSize * 8
                local cellPixelH = cellSize * 8

                if room.entities then
                    for _, entity in ipairs(room.entities) do
                        -- Entity x,y are absolute; room has an x,y offset
                        local ex = (entity.x or 0) - (room.x or 0)
                        local ey = (entity.y or 0) - (room.y or 0)

                        if ex >= cellPixelX and ex < cellPixelX + cellPixelW
                           and ey >= cellPixelY and ey < cellPixelY + cellPixelH then
                            -- Store entity with position relative to cell origin
                            local cellEntity = {}
                            for k, v in pairs(entity) do
                                cellEntity[k] = v
                            end
                            cellEntity.x = ex - cellPixelX
                            cellEntity.y = ey - cellPixelY
                            table.insert(cellEntities, cellEntity)
                        end
                    end
                end

                table.insert(cells, {
                    tileData   = tileData,
                    edges      = edges,
                    sig        = sig,
                    roomName   = roomName,
                    gridCol    = cx,
                    gridRow    = cy,
                    cellSize   = cellSize,
                    airRatio   = airRatio,
                    entities   = cellEntities,
                    sourceRoom = roomName,
                })
            end
        end
    end

    return cells
end

--------------------------------------------------------------------------------
-- Area Catalog (per-chapter cell library)
--------------------------------------------------------------------------------

--- Build a catalog of cells from all rooms in the currently loaded map,
-- grouped by area prefix.
--
-- Area grouping uses the room name convention:
--   "lvl_a-00" → area = "a"
--   "lvl_b-05" → area = "b"
--   "A01_Intro_01" → area = "A01"
--
-- @param rooms     List of Lönn room tables
-- @param cellSize  Cell side in tiles (default 8)
-- @param opts      Options:
--   areaFromName   function(roomName) → areaKey (custom grouper)
--   minAirRatio    Minimum air ratio to include cell (default 0.02)
--   maxAirRatio    Maximum air ratio to include cell (default 0.98)
-- @return table    { [areaKey] = { cells = {cell, ...}, bySig = {[sig]={cell,...}} } }
function cellCatalog.buildAreaCatalog(rooms, cellSize, opts)
    cellSize = cellSize or cellCatalog.DEFAULT_CELL_SIZE
    opts = opts or {}

    local areaFromName = opts.areaFromName or function(name)
        -- Strip "lvl_" prefix
        local stripped = name:gsub("^lvl_", "")
        -- Get the letter/prefix before a dash or underscore
        local area = stripped:match("^([A-Za-z]+%d*)")
        return area or "default"
    end

    local catalog = {}

    for _, room in ipairs(rooms) do
        local areaKey = areaFromName(room.name or "")
        if not catalog[areaKey] then
            catalog[areaKey] = { cells = {}, bySig = {} }
        end

        local cells = cellCatalog.extractCellsFromRoom(room, cellSize)
        for _, cell in ipairs(cells) do
            cell.area = areaKey
            table.insert(catalog[areaKey].cells, cell)

            if not catalog[areaKey].bySig[cell.sig] then
                catalog[areaKey].bySig[cell.sig] = {}
            end
            table.insert(catalog[areaKey].bySig[cell.sig], cell)
        end
    end

    return catalog
end

--------------------------------------------------------------------------------
-- Catalog from Loaded Map (Lönn integration)
--------------------------------------------------------------------------------

--- Build cell catalog from the currently loaded map in Lönn.
-- @param cellSize  Tiles per cell side (default 8)
-- @param opts      Options (same as buildAreaCatalog)
-- @return table    Area catalog
-- @return table    Stats { totalCells, areaCount, cellsByArea }
function cellCatalog.buildFromLoadedMap(cellSize, opts)
    local ok, state = pcall(require, "loaded_state")
    if not ok or not state or not state.map then
        return nil, { error = "No map loaded" }
    end

    if not state.map.rooms or #state.map.rooms == 0 then
        return nil, { error = "No rooms in map" }
    end

    local catalog = cellCatalog.buildAreaCatalog(state.map.rooms, cellSize, opts)

    -- Compute stats
    local totalCells = 0
    local areaCount = 0
    local cellsByArea = {}

    for areaKey, areaCatalog in pairs(catalog) do
        areaCount = areaCount + 1
        local count = #areaCatalog.cells
        totalCells = totalCells + count
        cellsByArea[areaKey] = count
    end

    return catalog, {
        totalCells = totalCells,
        areaCount = areaCount,
        cellsByArea = cellsByArea,
        cellSize = cellSize,
    }
end

--------------------------------------------------------------------------------
-- Compatibility Checking
--------------------------------------------------------------------------------

--- Check if two cells are compatible on a shared edge.
-- cellA is to the LEFT of cellB:   cellA.right must match cellB.left
-- cellA is ABOVE cellB:            cellA.bottom must match cellB.top
--
-- "Match" means both edges are the same state (both open or both closed).
--
-- @param cellA      Cell descriptor
-- @param cellB      Cell descriptor
-- @param relation   "horizontal" (A left of B) or "vertical" (A above B)
-- @return boolean
function cellCatalog.areCompatible(cellA, cellB, relation)
    if relation == "horizontal" then
        return cellA.edges.right == cellB.edges.left
    elseif relation == "vertical" then
        return cellA.edges.bottom == cellB.edges.top
    end
    return false
end

--- Find all cells in a catalog that are compatible with given edge constraints.
-- @param areaCatalog   The per-area catalog entry { cells={}, bySig={} }
-- @param constraints   { top=bool|nil, right=bool|nil, bottom=bool|nil, left=bool|nil }
--                      nil means "don't care" (any value accepted)
-- @return table        List of matching cells
function cellCatalog.findCompatible(areaCatalog, constraints)
    constraints = constraints or {}
    local results = {}

    for _, cell in ipairs(areaCatalog.cells) do
        local match = true

        if constraints.top ~= nil and cell.edges.top ~= constraints.top then
            match = false
        end
        if constraints.right ~= nil and cell.edges.right ~= constraints.right then
            match = false
        end
        if constraints.bottom ~= nil and cell.edges.bottom ~= constraints.bottom then
            match = false
        end
        if constraints.left ~= nil and cell.edges.left ~= constraints.left then
            match = false
        end

        if match then
            table.insert(results, cell)
        end
    end

    return results
end

--------------------------------------------------------------------------------
-- Signature Helpers
--------------------------------------------------------------------------------

--- Decode a signature number (0..15) into an edges table.
-- @param sig  Integer 0..15
-- @return table  { top=bool, right=bool, bottom=bool, left=bool }
function cellCatalog.decodeSig(sig)
    return {
        top    = (sig % 2) >= 1,
        right  = (math.floor(sig / 2) % 2) >= 1,
        bottom = (math.floor(sig / 4) % 2) >= 1,
        left   = (math.floor(sig / 8) % 2) >= 1,
    }
end

--- Encode an edges table into a signature number.
-- @param edges  { top=bool, right=bool, bottom=bool, left=bool }
-- @return number 0..15
function cellCatalog.encodeSig(edges)
    local sig = 0
    if edges.top    then sig = sig + 1 end
    if edges.right  then sig = sig + 2 end
    if edges.bottom then sig = sig + 4 end
    if edges.left   then sig = sig + 8 end
    return sig
end

--- Pretty-print a signature.
-- @param sig  Integer 0..15
-- @return string  e.g. "T_BL" (top, bottom, left open)
function cellCatalog.sigToString(sig)
    local edges = cellCatalog.decodeSig(sig)
    local parts = {}
    if edges.top    then table.insert(parts, "T") end
    if edges.right  then table.insert(parts, "R") end
    if edges.bottom then table.insert(parts, "B") end
    if edges.left   then table.insert(parts, "L") end
    if #parts == 0 then return "closed" end
    return table.concat(parts)
end

--------------------------------------------------------------------------------
-- Debug / Stats
--------------------------------------------------------------------------------

--- Print catalog statistics.
-- @param catalog  Area catalog from buildAreaCatalog
function cellCatalog.printStats(catalog)
    for areaKey, areaCat in pairs(catalog) do
        print(string.format("[CellCatalog] Area '%s': %d cells", areaKey, #areaCat.cells))
        for sig, cells in pairs(areaCat.bySig) do
            print(string.format("  sig %2d (%s): %d cells",
                sig, cellCatalog.sigToString(sig), #cells))
        end
    end
end

return cellCatalog
