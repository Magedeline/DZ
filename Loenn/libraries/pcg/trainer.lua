-- PCG Trainer: Training Data Extraction
-- Extracts tile matrices from existing rooms to train the Markov Chain.
-- Supports both Lönn's loaded_state (when running inside Lönn) and
-- standalone operation with raw matrix data.

local trainer = {}

--------------------------------------------------------------------------------
-- Constants
--------------------------------------------------------------------------------

local TILE_AIR = "0"

--------------------------------------------------------------------------------
-- Matrix Extraction
--------------------------------------------------------------------------------

--- Extract a usable tile matrix from a Lönn room's tileFg/tileBg data.
-- Lönn stores tiles as matrix objects with :get(x,y) and :size() methods.
-- @param room       Lönn room table
-- @param layer      "fg" or "bg" (default "fg")
-- @return matrix    The tile matrix, or nil if not found
function trainer.extractMatrix(room, layer)
    layer = layer or "fg"

    if layer == "fg" then
        return room.tilesFg and room.tilesFg.matrix or nil
    elseif layer == "bg" then
        return room.tilesBg and room.tilesBg.matrix or nil
    end

    return nil
end

--------------------------------------------------------------------------------
-- Room Filtering
--------------------------------------------------------------------------------

--- Filter rooms suitable for training.
-- Excludes rooms that are too empty, too full, or have unusual dimensions.
-- @param rooms       List of room tables
-- @param opts        Options table:
--   minAirRatio     Minimum ratio of air tiles (default 0.2)
--   maxAirRatio     Maximum ratio of air tiles (default 0.9)
--   minWidth        Minimum room width in tiles (default 20)
--   minHeight       Minimum room height in tiles (default 14)
--   excludeNames    Set of room names to exclude (default {})
-- @return table      Filtered list of rooms
function trainer.filterRooms(rooms, opts)
    opts = opts or {}
    local minAirRatio = opts.minAirRatio or 0.2
    local maxAirRatio = opts.maxAirRatio or 0.9
    local minWidth    = opts.minWidth or 20
    local minHeight   = opts.minHeight or 14
    local excludeNames = opts.excludeNames or {}

    local filtered = {}

    for _, room in ipairs(rooms) do
        local include = true

        -- Check excluded names
        if excludeNames[room.name] then
            include = false
        end

        -- Check dimensions (room.width/height are in pixels, tiles = /8)
        if include then
            local tw = math.floor((room.width or 320) / 8)
            local th = math.floor((room.height or 184) / 8)
            if tw < minWidth or th < minHeight then
                include = false
            end
        end

        -- Check air ratio
        if include then
            local matrix = trainer.extractMatrix(room, "fg")
            if matrix then
                local airCount = 0
                local w, h = matrix:size()
                local total = w * h

                for y = 1, h do
                    for x = 1, w do
                        local tile = matrix:get(x, y, TILE_AIR)
                        if tile == TILE_AIR or tile == " " then
                            airCount = airCount + 1
                        end
                    end
                end

                local ratio = airCount / math.max(total, 1)
                if ratio < minAirRatio or ratio > maxAirRatio then
                    include = false
                end
            else
                include = false
            end
        end

        if include then
            table.insert(filtered, room)
        end
    end

    return filtered
end

--------------------------------------------------------------------------------
-- Training Pipeline
--------------------------------------------------------------------------------

--- Train a Markov Chain model from a list of rooms.
-- @param rooms      List of room tables with tilesFg data
-- @param markov     The markov module (require result)
-- @param config     Configuration string for MdMC (default "000011012")
-- @param layer      "fg" or "bg" (default "fg")
-- @return table     DPT (Dictionary of Probability Transitions)
-- @return table     Stats about training
function trainer.train(rooms, markov, config, layer)
    config = config or "000011012"
    layer = layer or "fg"

    local offsets = markov.parseConfig(config)
    local matrices = {}

    for _, room in ipairs(rooms) do
        local matrix = trainer.extractMatrix(room, layer)
        if matrix then
            table.insert(matrices, matrix)
        end
    end

    if #matrices == 0 then
        return nil, { error = "No valid matrices found", roomCount = 0 }
    end

    local dpt = markov.train(matrices, offsets)

    -- Compute stats
    local ngramCount = 0
    local totalTransitions = 0
    for _, dist in pairs(dpt) do
        ngramCount = ngramCount + 1
        for _, _ in pairs(dist) do
            totalTransitions = totalTransitions + 1
        end
    end

    local stats = {
        roomCount = #matrices,
        ngramCount = ngramCount,
        totalTransitions = totalTransitions,
        config = config,
        layer = layer,
    }

    return dpt, stats
end

--------------------------------------------------------------------------------
-- From Loaded State (Lönn integration)
--------------------------------------------------------------------------------

--- Train from the currently loaded map in Lönn.
-- Uses loaded_state to access the map's rooms.
-- @param markov     The markov module
-- @param config     Configuration string (default "000011012")
-- @param filterOpts Room filtering options (optional)
-- @return table|nil DPT or nil
-- @return table     Stats
function trainer.trainFromLoadedMap(markov, config, filterOpts)
    -- Try to load Lönn's state module
    local ok, state = pcall(require, "loaded_state")
    if not ok or not state or not state.map then
        return nil, { error = "No map loaded in Loenn" }
    end

    local map = state.map
    if not map.rooms or #map.rooms == 0 then
        return nil, { error = "No rooms in loaded map" }
    end

    -- Filter rooms
    local rooms = trainer.filterRooms(map.rooms, filterOpts)
    if #rooms == 0 then
        return nil, { error = "No rooms passed filtering", totalRooms = #map.rooms }
    end

    return trainer.train(rooms, markov, config)
end

--------------------------------------------------------------------------------
-- Entity Statistics
--------------------------------------------------------------------------------

--- Collect entity type statistics across rooms (for NLE/LE density settings).
-- @param rooms   List of room tables
-- @return table  { [entityName] = count, ... }
-- @return number Total entity count
function trainer.entityStats(rooms)
    local stats = {}
    local total = 0

    for _, room in ipairs(rooms) do
        if room.entities then
            for _, entity in ipairs(room.entities) do
                local name = entity._name or entity.name or "unknown"
                stats[name] = (stats[name] or 0) + 1
                total = total + 1
            end
        end
    end

    return stats, total
end

--------------------------------------------------------------------------------
-- Tile Palette Extraction
--------------------------------------------------------------------------------

--- Extract the set of unique tile characters used across rooms.
-- Useful for understanding what tilesets are in use.
-- @param rooms   List of room tables
-- @param layer   "fg" or "bg" (default "fg")
-- @return table  Set of tile characters { ["0"] = count, ["1"] = count, ... }
function trainer.tilePalette(rooms, layer)
    layer = layer or "fg"
    local palette = {}

    for _, room in ipairs(rooms) do
        local matrix = trainer.extractMatrix(room, layer)
        if matrix then
            local w, h = matrix:size()
            for y = 1, h do
                for x = 1, w do
                    local tile = matrix:get(x, y, "0")
                    palette[tile] = (palette[tile] or 0) + 1
                end
            end
        end
    end

    return palette
end

--------------------------------------------------------------------------------
-- ML.NET Feature Vector Extraction
-- Extracts the PCGLevelData feature schema consumed by PCGGenerator.ExportCentroids
-- and the ml_bridge.lua K-Means classifier.
--------------------------------------------------------------------------------

-- Lethal entity types (match PCGGenerator + ml_bridge definitions)
local LE_ENTITIES_SET = {
    spikesUp=true, spikesDown=true, spikesLeft=true, spikesRight=true,
    spinner=true, lightning=true, killbox=true,
    rotateSpinner=true, trackSpinner=true, dustStaticSpinner=true,
    finalBoss=true, seekerBarrier=true, seeker=true,
}

-- Non-lethal platform / movement entities
local NLE_ENTITIES_SET = {
    refill=true, booster=true, spring=true, jumpThru=true,
    dreamBlock=true, dashBlock=true, moveBlock=true, zipMover=true,
    touchSwitch=true, crumbleWallOnRumble=true, fallingBlock=true,
    swapBlock=true, switchGate=true, floatySpaceBlock=true,
    bounceBlock=true, coreBlock=true, starJumpBlock=true,
    cloud=true, kevinsPC=true,
}

--- Estimate the mean horizontal air-gap length in a tile matrix.
local function estimateAvgGap(matrix)
    if not matrix then return 0 end
    local w, h = matrix:size()
    local totalRuns, runCount = 0, 0
    local currentRun = 0
    for y = 1, h do
        currentRun = 0
        for x = 1, w do
            local tile = matrix:get(x, y, TILE_AIR)
            if tile == TILE_AIR or tile == " " then
                currentRun = currentRun + 1
            else
                if currentRun > 0 then
                    totalRuns = totalRuns + currentRun
                    runCount  = runCount + 1
                    currentRun = 0
                end
            end
        end
        if currentRun > 0 then
            totalRuns = totalRuns + currentRun
            runCount  = runCount + 1
        end
    end
    return runCount > 0 and (totalRuns / runCount) or 0
end

--- Extract a PCGLevelData-compatible feature vector from a single Lönn room.
-- The returned table matches the four columns expected by C#'s PCGLevelData:
--   EnemyCount, PlatformCount, AverageGap, CompletionRate
-- completionRate is approximated as the foreground air-tile ratio.
-- @param room   Lönn room table
-- @return table  { enemyCount, platformCount, averageGap, completionRate }
function trainer.extractFeatureVector(room)
    local enemyCount    = 0
    local platformCount = 0

    if room.entities then
        for _, entity in ipairs(room.entities) do
            local name = entity._name or entity.name or ""
            if LE_ENTITIES_SET[name] then
                enemyCount = enemyCount + 1
            elseif NLE_ENTITIES_SET[name] then
                platformCount = platformCount + 1
            end
        end
    end

    local airRatio = 0.5
    local avgGap   = 2.0
    local matrix   = room.tilesFg and room.tilesFg.matrix or nil
    if matrix then
        local w, h   = matrix:size()
        local total  = w * h
        local airCnt = 0
        for y = 1, h do
            for x = 1, w do
                local tile = matrix:get(x, y, TILE_AIR)
                if tile == TILE_AIR or tile == " " then
                    airCnt = airCnt + 1
                end
            end
        end
        airRatio = airCnt / math.max(total, 1)
        avgGap   = estimateAvgGap(matrix)
    end

    return {
        enemyCount    = enemyCount,
        platformCount = platformCount,
        averageGap    = avgGap,
        completionRate= airRatio,
    }
end

--- Extract feature vectors for all rooms in a list.
-- Useful for bulk analysis before writing a CSV for C# K-Means training.
-- @param rooms   List of Lönn room tables
-- @return table  Array of feature vectors
function trainer.extractFeatureVectors(rooms)
    local vectors = {}
    for _, room in ipairs(rooms) do
        table.insert(vectors, trainer.extractFeatureVector(room))
    end
    return vectors
end

--- Write extracted feature vectors to a CSV file suitable for PCGGenerator.Train(dataPath).
-- @param vectors   Array of feature tables (from extractFeatureVectors)
-- @param csvPath   Output file path
-- @return boolean, string   success, errorMessage
function trainer.writeFeatureCSV(vectors, csvPath)
    local f, err = io.open(csvPath, "w")
    if not f then return false, err end
    f:write("EnemyCount,PlatformCount,AverageGap,CompletionRate\n")
    for _, v in ipairs(vectors) do
        f:write(string.format("%.4f,%.4f,%.4f,%.4f\n",
            v.enemyCount, v.platformCount, v.averageGap, v.completionRate))
    end
    f:close()
    return true, nil
end

--- Export the current map's room features to a CSV for ML.NET K-Means training.
-- Shorthand for trainFromLoadedMap flow when you only want the data, not the model.
-- @param csvPath   Output path for the CSV file
-- @return boolean, string, integer   success, errorMessage, roomCount
function trainer.exportCurrentMapToCSV(csvPath)
    local ok, state = pcall(require, "loaded_state")
    if not ok or not state or not state.map then
        return false, "No map loaded in Loenn", 0
    end
    local rooms = trainer.filterRooms(state.map.rooms)
    if #rooms == 0 then
        return false, "No rooms passed filtering", 0
    end
    local vectors = trainer.extractFeatureVectors(rooms)
    local writeOk, writeErr = trainer.writeFeatureCSV(vectors, csvPath)
    return writeOk, writeErr, #vectors
end

return trainer
