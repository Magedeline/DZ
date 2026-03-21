-- PCG Cell Generate Level Script for LoennScripts
-- Generates a complete level using Spelunky-style cell assembly:
--   1. Harvests 8×8 (or custom-size) tile cells from existing rooms
--   2. Classifies each cell by edge openings (top/right/bottom/left)
--   3. Carves a guaranteed solution path through an NxM grid
--   4. Fills remaining grid slots with edge-compatible cells
--   5. Stitches everything into one Lönn room
--
-- No hand-building needed — just design rooms in Lönn and the cells
-- are extracted automatically, like Spelunky's room templates.
--
-- References:
--   https://tinysubversions.com/spelunkyGen/
--   https://tinysubversions.com/spelunkyGen2/

local state = require("loaded_state")
local utils = require("utils")
local snapshot = require("structs.snapshot")
local tilesStruct = require("structs.tiles")
local celesteRender = require("celeste_render")
local matrix = require("utils.matrix")
local mods = require("mods")

local cellCatalog   = mods.requireFromPlugin("libraries.pcg.cell_catalog")
local cellAssembler = mods.requireFromPlugin("libraries.pcg.cell_assembler")

local script = {}

script.name = "pcgCellGenerateLevel"
script.displayName = "PCG: Cell Generate Level (Spelunky)"
script.tooltip = "Generates a level using Spelunky-style cell assembly.\n" ..
    "Harvests cells from existing rooms, classifies edges,\n" ..
    "creates a solution path, and stitches a grid map.\n\n" ..
    "Design your rooms in Lönn — cells are extracted automatically!"

script.parameters = {
    gridCols = 4,
    gridRows = 4,
    cellSize = 8,
    areaFilter = "(all)",
    autoSeed = true,
    seed = 0,
    fillNonPath = true,
    fallbackToAny = false,
    emptyTile = "1",
    namePrefix = "pcg_cell",
    splitToRooms = false,
}

script.fieldOrder = {
    "gridCols", "gridRows", "cellSize", "areaFilter",
    "autoSeed", "seed", "fillNonPath", "fallbackToAny",
    "emptyTile", "namePrefix", "splitToRooms",
}

script.fieldInformation = {
    gridCols = { fieldType = "integer" },
    gridRows = { fieldType = "integer" },
    cellSize = {
        fieldType = "loennScripts.dropdown",
        options = { "8", "16", "32" },
    },
    areaFilter = {
        fieldType = "loennScripts.dropdown",
        options = {
            "(all)",
            "a", "b", "c",
            "A01", "A03", "A10",
            "default",
        },
    },
    seed = { fieldType = "integer" },
    emptyTile = {
        fieldType = "loennScripts.dropdown",
        options = { "0", "1", "3", "5", "7", "8", "9", "d", "g", "i", "k", "n",
                    "A", "B", "O", "X", "p", "N" },
    },
}

script.tooltips = {
    gridCols = "Number of columns in the cell grid (e.g. 4 = Spelunky default)",
    gridRows = "Number of rows in the cell grid (e.g. 4 = Spelunky default)",
    cellSize = "Tile size of each cell: 8 (64px), 16 (128px), or 32 (256px)",
    areaFilter = "Filter cells by area prefix from room names. '(all)' uses all rooms.",
    autoSeed = "Auto-generate a unique seed every run",
    seed = "Manual seed (only used when autoSeed is unchecked)",
    fillNonPath = "Fill non-path grid cells with compatible catalog cells",
    fallbackToAny = "If no edge-compatible cell exists, use any cell as fallback",
    emptyTile = "Tile material for empty/unfilled cells (1=dirt, 0=air)",
    namePrefix = "Prefix for generated room names",
    splitToRooms = "If true, each grid cell becomes a separate Lönn room (useful for large grids). If false, one big room.",
}

function script.prerun(args, layer)
    if not state.map then return end

    -- Resolve seed
    local seed
    if args.autoSeed then
        seed = os.time() + math.random(0, 99999)
    else
        seed = args.seed or 0
        if seed == 0 then seed = os.time() end
    end
    math.randomseed(seed)

    local cellSize = tonumber(args.cellSize) or 8
    local gridCols = math.max(1, args.gridCols or 4)
    local gridRows = math.max(1, args.gridRows or 4)
    local areaKey = args.areaFilter
    if areaKey == "(all)" then areaKey = nil end

    -- Build catalog
    local catalog, catalogStats = cellCatalog.buildFromLoadedMap(cellSize)
    if not catalog then
        print("[PCG Cell] Failed to build catalog: " .. tostring(catalogStats and catalogStats.error))
        return
    end

    -- Print catalog stats
    print(string.format("[PCG Cell] Catalog built: %d cells across %d areas (cell size: %d tiles)",
        catalogStats.totalCells, catalogStats.areaCount, cellSize))
    for area, count in pairs(catalogStats.cellsByArea) do
        print(string.format("[PCG Cell]   Area '%s': %d cells", area, count))
    end

    -- Select area
    local areaCat
    if areaKey and catalog[areaKey] then
        areaCat = catalog[areaKey]
        print(string.format("[PCG Cell] Using area '%s' (%d cells)", areaKey, #areaCat.cells))
    else
        -- Merge all
        areaCat = { cells = {}, bySig = {} }
        for _, ac in pairs(catalog) do
            for _, cell in ipairs(ac.cells) do
                table.insert(areaCat.cells, cell)
                if not areaCat.bySig[cell.sig] then
                    areaCat.bySig[cell.sig] = {}
                end
                table.insert(areaCat.bySig[cell.sig], cell)
            end
        end
        print(string.format("[PCG Cell] Using all areas (%d cells)", #areaCat.cells))
    end

    if #areaCat.cells == 0 then
        print("[PCG Cell] No cells found! Rooms may be too small for the selected cell size.")
        return
    end

    -- Print signature distribution
    print("[PCG Cell] Edge signature distribution:")
    for sig, cells in pairs(areaCat.bySig) do
        print(string.format("  sig %2d (%s): %d cells",
            sig, cellCatalog.sigToString(sig), #cells))
    end

    -- Assemble grid
    local gridResult = cellAssembler.assembleGrid(areaCat, gridCols, gridRows, {
        seed = seed,
        fillNonPath = args.fillNonPath,
        fallbackToAny = args.fallbackToAny,
        emptyTile = args.emptyTile or "1",
    })

    -- Print visualization
    local viz = cellAssembler.visualize(gridResult)
    print("[PCG Cell] Grid layout:")
    for line in viz:gmatch("[^\n]+") do
        print("  " .. line)
    end

    -- Stitch into tiles + entities
    local stitched = cellAssembler.stitchGrid(gridResult, matrix, args.emptyTile or "1")

    if not stitched or not stitched.matrix then
        print("[PCG Cell] Stitching failed!")
        return
    end

    print(string.format("[PCG Cell] Stitched: %dx%d tiles (%dx%d px), %d entities",
        stitched.width, stitched.height,
        stitched.pixelW, stitched.pixelH,
        #stitched.entities))

    -- Find placement position (to the right of existing rooms)
    local maxX = 0
    for _, existingRoom in ipairs(state.map.rooms) do
        local rx = existingRoom.x + (existingRoom.width or 320)
        if rx > maxX then maxX = rx end
    end
    local offsetX = maxX + 320

    -- Save for undo
    local prevRooms = utils.deepcopy(state.map.rooms)

    if args.splitToRooms then
        -- Create one room per grid cell
        local roomIndex = 0
        for r = 0, gridResult.rows - 1 do
            for c = 0, gridResult.cols - 1 do
                local slot = gridResult.grid[r][c]
                if slot.cell then
                    roomIndex = roomIndex + 1
                    local roomW = cellSize * 8
                    local roomH = cellSize * 8

                    -- Extract tile string for this cell
                    local tileString = ""
                    for dy = 1, cellSize do
                        if dy > 1 then tileString = tileString .. "\n" end
                        local row = {}
                        for dx = 1, cellSize do
                            row[dx] = slot.cell.tileData[dy] and slot.cell.tileData[dy][dx] or (args.emptyTile or "1")
                        end
                        tileString = tileString .. table.concat(row)
                    end

                    -- BG tiles
                    local bgString = ""
                    for dy = 1, cellSize do
                        if dy > 1 then bgString = bgString .. "\n" end
                        bgString = bgString .. string.rep("0", cellSize)
                    end

                    local roomName = string.format("%s_r%dc%d", args.namePrefix or "pcg_cell", r, c)
                    local roomX = offsetX + c * roomW
                    local roomY = r * roomH

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

                    -- Copy entities with room offset
                    if slot.cell.entities then
                        for _, entity in ipairs(slot.cell.entities) do
                            local e = {}
                            for k, v in pairs(entity) do e[k] = v end
                            e.x = (e.x or 0) + roomX
                            e.y = (e.y or 0) + roomY
                            table.insert(newRoom.entities, e)
                        end
                    end

                    table.insert(state.map.rooms, newRoom)
                end
            end
        end
        print(string.format("[PCG Cell] Created %d rooms (split mode)", roomIndex))
    else
        -- Single big room
        local mat = stitched.matrix
        local mw, mh = mat:size()

        local tileString = ""
        for y = 1, mh do
            if y > 1 then tileString = tileString .. "\n" end
            local row = {}
            for x = 1, mw do
                row[x] = mat:get(x, y, "0")
            end
            tileString = tileString .. table.concat(row)
        end

        local bgString = ""
        for y = 1, mh do
            if y > 1 then bgString = bgString .. "\n" end
            bgString = bgString .. string.rep("0", mw)
        end

        local roomName = string.format("%s_%02d", args.namePrefix or "pcg_cell", 1)

        local newRoom = {
            name = "lvl_" .. roomName,
            x = offsetX,
            y = 0,
            width = stitched.pixelW,
            height = stitched.pixelH,
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

        -- Add entities with room offset
        for _, entity in ipairs(stitched.entities) do
            local e = {}
            for k, v in pairs(entity) do e[k] = v end
            e.x = (e.x or 0) + offsetX
            e.y = (e.y or 0)
            table.insert(newRoom.entities, e)
        end

        table.insert(state.map.rooms, newRoom)
        print(string.format("[PCG Cell] Created 1 room: %s (%dx%d px)",
            newRoom.name, newRoom.width, newRoom.height))
    end

    celesteRender.invalidateRoomCache()

    print(string.format("[PCG Cell] Done! Seed: %d", seed))

    -- Undo/redo support
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
