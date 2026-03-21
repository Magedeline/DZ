-- PCG Markov Chain Engine for Celeste Level Generation
-- Based on "Towards a Celeste AI Framework" (Robinet, Gómez-Maureira, Preuss 2025)
-- Implements Multi-dimensional Markov Chains (MdMC) for tile-based room generation

local markov = {}

local randomUtils = require("libraries.pcg.random_utils")

--------------------------------------------------------------------------------
-- Configuration Matrix
--------------------------------------------------------------------------------
-- A 3x3 matrix encoding which neighbors influence the current tile.
-- Values: 0 = ignore, 1 = use as context, 2 = tile being predicted (center)
-- Stored as a flat 9-element array read row-by-row.
--
-- Example: "000011012" means:
--   [0 0 0]
--   [0 1 1]   → consider left tile and top-left tile
--   [0 1 2]   → the "2" marks the target tile
--
-- The paper found "000011012" to give best playability results.

--- Parse a 9-character config string into a 3x3 lookup table.
-- Returns a list of {dx, dy} offsets for the "1" entries (relative to the "2" entry).
function markov.parseConfig(configStr)
    assert(#configStr == 9, "Config string must be 9 characters (3x3 matrix)")

    -- First, find where "2" (the target tile) is located
    local targetRow, targetCol
    for i = 1, 9 do
        local val = tonumber(configStr:sub(i, i))
        if val == 2 then
            targetRow = math.floor((i - 1) / 3)  -- 0..2
            targetCol = (i - 1) % 3               -- 0..2
            break
        end
    end
    assert(targetRow and targetCol, "Config string must contain exactly one '2' (target tile)")

    -- Collect offsets of "1" entries relative to the "2" position
    local offsets = {}
    for i = 1, 9 do
        local val = tonumber(configStr:sub(i, i))
        if val == 1 then
            local row = math.floor((i - 1) / 3)  -- 0..2
            local col = (i - 1) % 3               -- 0..2
            -- Offset relative to the target ("2") position
            table.insert(offsets, {col - targetCol, row - targetRow})
        end
    end

    return offsets
end

--------------------------------------------------------------------------------
-- N-gram extraction
--------------------------------------------------------------------------------

--- Extract the n-gram of adjacent tiles at position (x,y) using the given offsets.
-- @param matrix  A 2D matrix (1-indexed, uses matrix:get(x, y, default))
-- @param x       Column of the target tile
-- @param y       Row of the target tile
-- @param offsets List of {dx, dy} from parseConfig
-- @return string  The concatenated n-gram, e.g. "01" for two neighbors
function markov.getNgram(matrix, x, y, offsets)
    local parts = {}
    for _, off in ipairs(offsets) do
        local nx = x + off[1]
        local ny = y + off[2]
        local tile = matrix:get(nx, ny, "0")
        table.insert(parts, tostring(tile))
    end
    return table.concat(parts, ",")
end

--------------------------------------------------------------------------------
-- Training: build Dictionary of Probability Transitions (DPT)
--------------------------------------------------------------------------------

--- Train a Markov model from a set of room matrices.
-- @param rooms    Array of tile matrices (each has :get(x,y,default) and :size())
-- @param offsets  Neighbor offsets from parseConfig
-- @return table   DPT: { [ngram] = { [tile] = probability, ... }, ... }
-- @return table   Counts table for diagnostics
function markov.train(rooms, offsets)
    local counts = {}   -- counts[ngram][tile] = count
    local totals = {}   -- totals[ngram] = total count

    for _, mat in ipairs(rooms) do
        local w, h = mat:size()

        -- Determine safe scanning boundaries based on max offset magnitudes
        local minDx, maxDx, minDy, maxDy = 0, 0, 0, 0
        for _, off in ipairs(offsets) do
            minDx = math.min(minDx, off[1])
            maxDx = math.max(maxDx, off[1])
            minDy = math.min(minDy, off[2])
            maxDy = math.max(maxDy, off[2])
        end

        local startX = 1 - math.min(0, minDx)
        local startY = 1 - math.min(0, minDy)
        local endX   = w - math.max(0, maxDx)
        local endY   = h - math.max(0, maxDy)

        for y = startY, endY do
            for x = startX, endX do
                local ngram = markov.getNgram(mat, x, y, offsets)
                local tile  = mat:get(x, y, "0")

                if not counts[ngram] then
                    counts[ngram] = {}
                    totals[ngram] = 0
                end
                counts[ngram][tile] = (counts[ngram][tile] or 0) + 1
                totals[ngram] = totals[ngram] + 1
            end
        end
    end

    -- Convert counts to probabilities
    local dpt = {}
    for ngram, tileCounts in pairs(counts) do
        dpt[ngram] = {}
        local total = totals[ngram]
        for tile, count in pairs(tileCounts) do
            dpt[ngram][tile] = count / total
        end
    end

    return dpt, counts
end

--------------------------------------------------------------------------------
-- Tile sampling from probability distribution
--------------------------------------------------------------------------------

--- Sample a tile from a probability distribution table.
-- @param dist  Table { [tile] = probability, ... }
-- @return string  The sampled tile
function markov.sampleTile(dist)
    local r = math.random()
    local cumulative = 0

    for tile, prob in pairs(dist) do
        cumulative = cumulative + prob
        if r <= cumulative then
            return tile
        end
    end

    -- Fallback: return last tile seen (floating point edge case)
    local lastTile = "0"
    for tile, _ in pairs(dist) do
        lastTile = tile
    end
    return lastTile
end

--- Get all known tile types from the DPT.
function markov.getKnownTiles(dpt)
    local tiles = {}
    local seen = {}
    for _, dist in pairs(dpt) do
        for tile, _ in pairs(dist) do
            if not seen[tile] then
                seen[tile] = true
                table.insert(tiles, tile)
            end
        end
    end
    return tiles
end

--------------------------------------------------------------------------------
-- Room Generation with Backtracking
--------------------------------------------------------------------------------

--- Generate a room matrix using the trained MdMC model.
-- @param dpt         Dictionary of Probability Transitions from train()
-- @param offsets     Neighbor offsets from parseConfig
-- @param width       Room width in tiles
-- @param height      Room height in tiles
-- @param btDepth     Max backtracking depth (default 2)
-- @param seed        Optional random seed for reproducibility
-- @return matrix     Generated tile matrix
function markov.generate(dpt, offsets, width, height, btDepth, seed, matrixLib)
    btDepth = btDepth or 2

    if seed then
        math.randomseed(seed)
    end

    -- Create output matrix filled with air
    local result = matrixLib.filled("0", width, height)

    -- Determine generation start based on config offsets
    local minDx, maxDx, minDy, maxDy = 0, 0, 0, 0
    for _, off in ipairs(offsets) do
        minDx = math.min(minDx, off[1])
        maxDx = math.max(maxDx, off[1])
        minDy = math.min(minDy, off[2])
        maxDy = math.max(maxDy, off[2])
    end

    local startX = 1 - math.min(0, minDx)
    local startY = 1 - math.min(0, minDy)
    local endX   = width  - math.max(0, maxDx)
    local endY   = height - math.max(0, maxDy)

    local knownTiles = markov.getKnownTiles(dpt)

    -- Scan left-to-right, top-to-bottom
    for y = startY, endY do
        for x = startX, endX do
            local placed = false

            -- Try normal generation
            local ngram = markov.getNgram(result, x, y, offsets)
            if dpt[ngram] then
                local tile = markov.sampleTile(dpt[ngram])
                result:set(x, y, tile)
                placed = true
            end

            -- Backtracking on unseen state
            if not placed and btDepth > 0 then
                placed = markov._backtrack(result, dpt, offsets, x, y, btDepth, knownTiles)
            end

            -- Final fallback: random tile from known set
            if not placed then
                local idx = math.random(1, #knownTiles)
                result:set(x, y, knownTiles[idx])
            end
        end
    end

    return result
end

--- Internal backtracking helper.
-- Tries replacing previous tiles to resolve unseen n-gram states.
function markov._backtrack(result, dpt, offsets, x, y, depth, knownTiles)
    if depth <= 0 then
        return false
    end

    -- Shuffle known tiles to try
    local shuffled = {}
    for i, t in ipairs(knownTiles) do shuffled[i] = t end
    randomUtils.random_shuffle(shuffled)

    -- Try each tile at the previous position
    local prevX = x - 1
    local prevY = y
    if prevX < 1 then
        prevX = result:size()  -- wrap to end of previous row
        prevY = y - 1
    end

    if prevY < 1 then
        return false
    end

    local originalTile = result:get(prevX, prevY, "0")

    for _, tryTile in ipairs(shuffled) do
        result:set(prevX, prevY, tryTile)

        local ngram = markov.getNgram(result, x, y, offsets)
        if dpt[ngram] then
            local tile = markov.sampleTile(dpt[ngram])
            result:set(x, y, tile)
            return true
        end
    end

    -- Deeper backtracking
    local deepResult = markov._backtrack(result, dpt, offsets, prevX, prevY, depth - 1, knownTiles)
    if deepResult then
        local ngram = markov.getNgram(result, x, y, offsets)
        if dpt[ngram] then
            local tile = markov.sampleTile(dpt[ngram])
            result:set(x, y, tile)
            return true
        end
    end

    -- Restore original
    result:set(prevX, prevY, originalTile)
    return false
end

return markov
