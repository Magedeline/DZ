-- PCG Cell Assembler: Spelunky-Style Grid Map Assembly
--
-- Given a cell catalog (from cell_catalog.lua), this module places cells
-- onto an NxM grid to form a complete map, exactly like Spelunky:
--
--   1. Create a "solution path" from an entrance cell to an exit cell.
--   2. Fill remaining grid slots with compatible cells from the catalog.
--   3. Stitch tile data + entities into Lönn room structures.
--
-- Reference:
--   Spelunky generator: https://tinysubversions.com/spelunkyGen/
--   Spelunky 2 generator: https://tinysubversions.com/spelunkyGen2/
--
-- The key constraint is EDGE COMPATIBILITY:
--   If the solution path goes from cell (2,0) → (2,1) downward, then
--   cell (2,0) must have an open bottom and cell (2,1) must have an
--   open top.
--
-- Non-path cells can be anything, but their edges must match their
-- neighbours (open↔open, closed↔closed) to avoid visible seams.

local cellAssembler = {}

local cellCatalog  -- lazy-loaded

local function getCatalog()
    if not cellCatalog then
        local ok, mods = pcall(require, "mods")
        if ok and mods and mods.requireFromPlugin then
            cellCatalog = mods.requireFromPlugin("libraries.pcg.cell_catalog")
        end
        if not cellCatalog then
            cellCatalog = require("libraries.pcg.cell_catalog")
        end
    end
    return cellCatalog
end

--------------------------------------------------------------------------------
-- Random Helpers
--------------------------------------------------------------------------------

local function shuffleArray(arr)
    local n = #arr
    for i = n, 2, -1 do
        local j = math.random(1, i)
        arr[i], arr[j] = arr[j], arr[i]
    end
    return arr
end

local function pickRandom(arr)
    if #arr == 0 then return nil end
    return arr[math.random(1, #arr)]
end

--------------------------------------------------------------------------------
-- Solution Path Generation (Spelunky Algorithm)
--------------------------------------------------------------------------------

--- Generate a solution path through an NxM grid.
--
-- Spelunky convention:
--   - Start at a random column in row 0 (top).
--   - Walk left/right/down. Never go up. Never revisit a column after
--     going down and changing direction.
--   - End when reaching row (rows-1).
--
-- Each path cell records which directions it must be open on:
--   { col=int, row=int, openTop=bool, openBottom=bool, openLeft=bool, openRight=bool }
--
-- @param cols   Grid width (number of columns, e.g. 4)
-- @param rows   Grid height (number of rows, e.g. 4)
-- @param seed   Optional random seed
-- @return table  { path = {cell,...}, startCol=int, endCol=int }
function cellAssembler.generateSolutionPath(cols, rows, seed)
    if seed then math.randomseed(seed) end

    cols = math.max(1, cols or 4)
    rows = math.max(1, rows or 4)

    local startCol = math.random(0, cols - 1)
    local path = {}
    local grid = {}  -- [row][col] = path entry

    for r = 0, rows - 1 do
        grid[r] = {}
    end

    local col = startCol
    local row = 0

    -- Place first cell
    local entry = {
        col = col, row = row,
        openTop = false, openBottom = false,
        openLeft = false, openRight = false,
        isPath = true,
    }
    table.insert(path, entry)
    grid[row][col] = entry

    while row < rows - 1 do
        -- Decide direction: left (-1), right (+1), or down (0)
        local choices = {}

        -- Can go left?
        if col > 0 and not grid[row][col - 1] then
            table.insert(choices, "left")
        end
        -- Can go right?
        if col < cols - 1 and not grid[row][col + 1] then
            table.insert(choices, "right")
        end
        -- Can always go down
        table.insert(choices, "down")

        local dir = pickRandom(choices)

        if dir == "left" then
            -- Current cell needs open left
            entry.openLeft = true
            col = col - 1
            entry = {
                col = col, row = row,
                openTop = false, openBottom = false,
                openLeft = false, openRight = true,  -- connects back to previous
                isPath = true,
            }
            table.insert(path, entry)
            grid[row][col] = entry

        elseif dir == "right" then
            entry.openRight = true
            col = col + 1
            entry = {
                col = col, row = row,
                openTop = false, openBottom = false,
                openLeft = true,  -- connects back to previous
                openRight = false,
                isPath = true,
            }
            table.insert(path, entry)
            grid[row][col] = entry

        else  -- "down"
            entry.openBottom = true
            row = row + 1
            entry = {
                col = col, row = row,
                openTop = true,   -- connects to cell above
                openBottom = false,
                openLeft = false, openRight = false,
                isPath = true,
            }
            table.insert(path, entry)
            grid[row][col] = entry
        end
    end

    return {
        path = path,
        grid = grid,
        startCol = startCol,
        endCol = col,
        cols = cols,
        rows = rows,
    }
end

--------------------------------------------------------------------------------
-- Grid Assembly
--------------------------------------------------------------------------------

--- Assemble a full grid map by:
--   1. Generating a solution path.
--   2. Setting edge constraints for path cells.
--   3. Filling non-path cells with compatible catalog entries.
--
-- @param areaCatalog  Per-area catalog entry from cell_catalog: { cells={}, bySig={} }
-- @param cols         Grid columns (default 4)
-- @param rows         Grid rows (default 4)
-- @param opts         Options:
--   seed              Random seed
--   fillNonPath       true to fill non-path cells (default true)
--   allowEmpty        true to allow empty cells where no match is found (default true)
--   emptyTile         Tile char for unfilled cells (default "0" = air, or "1" = solid)
--   fallbackToAny     If no exact-match cell, pick any cell (default false)
-- @return table       Grid result:
--   { grid = cell[row][col], rows=int, cols=int, path=path_info,
--     cellSize=int, stats={placed,empty,pathLen} }
function cellAssembler.assembleGrid(areaCatalog, cols, rows, opts)
    opts = opts or {}
    cols = cols or 4
    rows = rows or 4
    local catalog = getCatalog()

    local pathInfo = cellAssembler.generateSolutionPath(cols, rows, opts.seed)
    local fillNonPath = opts.fillNonPath ~= false
    local allowEmpty = opts.allowEmpty ~= false
    local fallbackToAny = opts.fallbackToAny or false
    local emptyTile = opts.emptyTile or "1"  -- solid wall for non-path empty

    -- Build the final grid: grid[row][col] = { cell=cellData|nil, constraints={} }
    local finalGrid = {}
    for r = 0, rows - 1 do
        finalGrid[r] = {}
        for c = 0, cols - 1 do
            finalGrid[r][c] = { cell = nil, constraints = {}, isPath = false }
        end
    end

    -- Mark path cells with their required edge openings
    for _, pathEntry in ipairs(pathInfo.path) do
        local slot = finalGrid[pathEntry.row][pathEntry.col]
        slot.isPath = true
        slot.constraints = {
            top    = pathEntry.openTop or nil,
            right  = pathEntry.openRight or nil,
            bottom = pathEntry.openBottom or nil,
            left   = pathEntry.openLeft or nil,
        }
        -- For path cells, openings must be true; but unopened sides can be anything
        -- Only enforce "must be open" constraints (true); nil = don't care
        -- However, if a side is NOT open on the path, we don't force it closed
        -- because a bonus opening is fine (extra exploration).
    end

    -- Phase 1: Place path cells
    local placed = 0
    local empty = 0

    for _, pathEntry in ipairs(pathInfo.path) do
        local slot = finalGrid[pathEntry.row][pathEntry.col]

        -- Build constraints: path openings must be at least open
        local constraints = {}
        if pathEntry.openTop    then constraints.top = true end
        if pathEntry.openRight  then constraints.right = true end
        if pathEntry.openBottom then constraints.bottom = true end
        if pathEntry.openLeft   then constraints.left = true end

        -- Also consider what neighbours already require
        -- (left neighbour's right must match our left, etc.)
        local leftNeighbour = pathEntry.col > 0 and finalGrid[pathEntry.row][pathEntry.col - 1] or nil
        local rightNeighbour = pathEntry.col < cols - 1 and finalGrid[pathEntry.row][pathEntry.col + 1] or nil
        local topNeighbour = pathEntry.row > 0 and finalGrid[pathEntry.row - 1][pathEntry.col] or nil
        local bottomNeighbour = pathEntry.row < rows - 1 and finalGrid[pathEntry.row + 1][pathEntry.col] or nil

        if leftNeighbour and leftNeighbour.cell then
            constraints.left = leftNeighbour.cell.edges.right
        end
        if rightNeighbour and rightNeighbour.cell then
            constraints.right = rightNeighbour.cell.edges.left
        end
        if topNeighbour and topNeighbour.cell then
            constraints.top = topNeighbour.cell.edges.bottom
        end
        if bottomNeighbour and bottomNeighbour.cell then
            constraints.bottom = bottomNeighbour.cell.edges.top
        end

        local candidates = catalog.findCompatible(areaCatalog, constraints)

        if #candidates > 0 then
            slot.cell = pickRandom(candidates)
            placed = placed + 1
        elseif fallbackToAny and #areaCatalog.cells > 0 then
            slot.cell = pickRandom(areaCatalog.cells)
            placed = placed + 1
        else
            empty = empty + 1
        end
    end

    -- Phase 2: Fill non-path cells
    if fillNonPath then
        for r = 0, rows - 1 do
            for c = 0, cols - 1 do
                local slot = finalGrid[r][c]
                if not slot.isPath and not slot.cell then
                    -- Build constraints from placed neighbours
                    local constraints = {}

                    local leftN  = c > 0 and finalGrid[r][c - 1] or nil
                    local rightN = c < cols - 1 and finalGrid[r][c + 1] or nil
                    local topN   = r > 0 and finalGrid[r - 1][c] or nil
                    local bottomN = r < rows - 1 and finalGrid[r + 1][c] or nil

                    if leftN and leftN.cell then
                        constraints.left = leftN.cell.edges.right
                    end
                    if rightN and rightN.cell then
                        constraints.right = rightN.cell.edges.left
                    end
                    if topN and topN.cell then
                        constraints.top = topN.cell.edges.bottom
                    end
                    if bottomN and bottomN.cell then
                        constraints.bottom = bottomN.cell.edges.top
                    end

                    local candidates = catalog.findCompatible(areaCatalog, constraints)

                    if #candidates > 0 then
                        slot.cell = pickRandom(candidates)
                        placed = placed + 1
                    elseif fallbackToAny and #areaCatalog.cells > 0 then
                        slot.cell = pickRandom(areaCatalog.cells)
                        placed = placed + 1
                    elseif not allowEmpty then
                        -- Force a solid/empty cell (no catalog match)
                        empty = empty + 1
                    else
                        empty = empty + 1
                    end
                end
            end
        end
    end

    return {
        grid = finalGrid,
        rows = rows,
        cols = cols,
        path = pathInfo,
        cellSize = areaCatalog.cells[1] and areaCatalog.cells[1].cellSize or 8,
        stats = {
            placed = placed,
            empty = empty,
            pathLen = #pathInfo.path,
            totalSlots = rows * cols,
        },
    }
end

--------------------------------------------------------------------------------
-- Stitch Grid into a Tile Matrix + Entity List
--------------------------------------------------------------------------------

--- Convert an assembled grid into a single tile matrix and entity list.
-- This produces data ready to be injected into a Lönn room.
--
-- @param gridResult   Output from assembleGrid()
-- @param matrixLib    Lönn matrix library (require("utils.matrix"))
-- @param emptyTile    Tile for empty cells (default "1" = solid)
-- @return table       { matrix=matrix, entities={}, width=int, height=int,
--                        pixelW=int, pixelH=int }
function cellAssembler.stitchGrid(gridResult, matrixLib, emptyTile)
    emptyTile = emptyTile or "1"
    local cellSize = gridResult.cellSize
    local totalW = gridResult.cols * cellSize   -- tiles
    local totalH = gridResult.rows * cellSize   -- tiles

    -- Create full tile matrix
    local matrix = matrixLib.filled(emptyTile, totalW, totalH)

    local allEntities = {}
    local nextEntityId = 1

    for r = 0, gridResult.rows - 1 do
        for c = 0, gridResult.cols - 1 do
            local slot = gridResult.grid[r][c]
            local baseX = c * cellSize  -- 0-based tile offset
            local baseY = r * cellSize

            if slot.cell then
                -- Copy tile data into the matrix
                for dy = 1, cellSize do
                    for dx = 1, cellSize do
                        local tile = slot.cell.tileData[dy] and slot.cell.tileData[dy][dx]
                        if tile then
                            matrix:set(baseX + dx, baseY + dy, tile)
                        end
                    end
                end

                -- Copy and offset entities
                if slot.cell.entities then
                    for _, entity in ipairs(slot.cell.entities) do
                        local e = {}
                        for k, v in pairs(entity) do
                            e[k] = v
                        end
                        -- Offset to grid position (in pixels)
                        e.x = (e.x or 0) + baseX * 8
                        e.y = (e.y or 0) + baseY * 8
                        e._id = nextEntityId
                        nextEntityId = nextEntityId + 1
                        table.insert(allEntities, e)
                    end
                end
            end
            -- else: emptyTile is already in the matrix from filled()
        end
    end

    -- Place player spawn at the start of the solution path
    local startCol = gridResult.path.startCol
    local startRow = 0
    local spawnTileX = startCol * cellSize + math.floor(cellSize / 2)
    local spawnTileY = math.floor(cellSize / 2)

    -- Find ground below spawn
    for y = spawnTileY, totalH do
        local t = matrix:get(spawnTileX, y, "0")
        if t ~= "0" and t ~= " " then
            spawnTileY = y - 1
            break
        end
    end

    table.insert(allEntities, 1, {
        _name = "player",
        x = (spawnTileX - 1) * 8,
        y = (spawnTileY - 1) * 8,
        _id = 0,
        _type = "entity",
    })

    return {
        matrix = matrix,
        entities = allEntities,
        width = totalW,
        height = totalH,
        pixelW = totalW * 8,
        pixelH = totalH * 8,
        cellSize = cellSize,
        gridCols = gridResult.cols,
        gridRows = gridResult.rows,
        path = gridResult.path,
        stats = gridResult.stats,
    }
end

--------------------------------------------------------------------------------
-- High-Level API: Generate Map from Area
--------------------------------------------------------------------------------

--- One-shot: extract cells from loaded map → assemble grid → stitch into room.
--
-- @param areaKey     Area key to filter cells (e.g. "a", "b", "A01") or nil for all
-- @param cols        Grid columns (default 4)
-- @param rows        Grid rows (default 4)
-- @param cellSize    Tiles per cell (default 8)
-- @param matrixLib   Matrix library
-- @param opts        Options for assembleGrid (seed, fallbackToAny, etc.)
-- @return table|nil  Stitched result or nil
-- @return table      Stats
function cellAssembler.generateFromMap(areaKey, cols, rows, cellSize, matrixLib, opts)
    local catalog = getCatalog()
    local areaCatalogMap, catalogStats = catalog.buildFromLoadedMap(cellSize)

    if not areaCatalogMap then
        return nil, catalogStats
    end

    -- Select the area catalog
    local areaCat
    if areaKey and areaCatalogMap[areaKey] then
        areaCat = areaCatalogMap[areaKey]
    else
        -- Merge all areas into a single catalog
        areaCat = { cells = {}, bySig = {} }
        for _, ac in pairs(areaCatalogMap) do
            for _, cell in ipairs(ac.cells) do
                table.insert(areaCat.cells, cell)
                if not areaCat.bySig[cell.sig] then
                    areaCat.bySig[cell.sig] = {}
                end
                table.insert(areaCat.bySig[cell.sig], cell)
            end
        end
    end

    if #areaCat.cells == 0 then
        return nil, { error = "No cells found for area: " .. tostring(areaKey) }
    end

    -- Assemble grid
    local gridResult = cellAssembler.assembleGrid(areaCat, cols, rows, opts)

    -- Stitch
    local stitched = cellAssembler.stitchGrid(gridResult, matrixLib)

    return stitched, {
        catalogStats = catalogStats,
        gridStats = gridResult.stats,
        areaKey = areaKey or "(all)",
        cellsAvailable = #areaCat.cells,
    }
end

--------------------------------------------------------------------------------
-- ASCII Visualization (Spelunky-style debug view)
--------------------------------------------------------------------------------

--- Generate an ASCII visualization of the assembled grid.
-- Shows path cells as [P], empty as [ ], filled as [·], start as [S], end as [E].
-- @param gridResult  Output from assembleGrid()
-- @return string     Multi-line ASCII art
function cellAssembler.visualize(gridResult)
    local lines = {}
    local path = gridResult.path

    -- Header
    table.insert(lines, string.format("Grid: %dx%d  Path length: %d  Placed: %d  Empty: %d",
        gridResult.cols, gridResult.rows,
        gridResult.stats.pathLen, gridResult.stats.placed, gridResult.stats.empty))
    table.insert(lines, "")

    -- Column headers
    local header = "   "
    for c = 0, gridResult.cols - 1 do
        header = header .. string.format(" %2d ", c)
    end
    table.insert(lines, header)

    -- Build path lookup
    local pathLookup = {}
    for i, p in ipairs(path.path) do
        pathLookup[p.row .. "," .. p.col] = i
    end

    for r = 0, gridResult.rows - 1 do
        local row = string.format("%2d ", r)
        for c = 0, gridResult.cols - 1 do
            local slot = gridResult.grid[r][c]
            local pathIdx = pathLookup[r .. "," .. c]

            if pathIdx then
                -- Path cell
                if r == 0 and c == path.startCol then
                    row = row .. "[S] "
                elseif r == gridResult.rows - 1 and c == path.endCol then
                    row = row .. "[E] "
                else
                    row = row .. string.format("[%d] ", pathIdx)
                end
            elseif slot.cell then
                -- Filled non-path
                local sig = slot.cell.sig
                row = row .. string.format("{%s}", cellCatalog and cellCatalog.sigToString(sig) or tostring(sig))
                -- Pad to 4 chars
                local label = cellCatalog and cellCatalog.sigToString(sig) or tostring(sig)
                if #label < 2 then row = row .. " " end
            else
                -- Empty
                row = row .. "[ ] "
            end
        end
        table.insert(lines, row)

        -- Show vertical connections between path cells
        if r < gridResult.rows - 1 then
            local connRow = "   "
            for c = 0, gridResult.cols - 1 do
                local slot = gridResult.grid[r][c]
                local slotBelow = gridResult.grid[r + 1][c]
                if slot.cell and slotBelow and slot.cell.edges.bottom then
                    connRow = connRow .. " |  "
                else
                    connRow = connRow .. "    "
                end
            end
            table.insert(lines, connRow)
        end
    end

    return table.concat(lines, "\n")
end

return cellAssembler
