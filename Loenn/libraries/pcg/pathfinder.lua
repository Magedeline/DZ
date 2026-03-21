-- PCG Celeste-A* Pathfinder
-- Adapted A* algorithm with gravity weighting for evaluating room playability.
-- Based on the Celeste AI Framework paper's description of movement mechanics.
--
-- In Celeste, the player can:
--   - Walk left/right on ground
--   - Jump (variable height, ~3 tiles max)
--   - Dash (8 directions, ~5 tiles, recharges on ground)
--   - Wall jump (from walls)
--   - Climb walls (limited stamina)
--
-- The A* heuristic incorporates gravity: moving upward is costlier,
-- moving downward is cheaper. Solid tiles are impassable, air tiles
-- are traversable. Reaching from one exit to another proves playability.

local pathfinder = {}

--------------------------------------------------------------------------------
-- Constants
--------------------------------------------------------------------------------

local TILE_AIR = "0"

-- Movement costs (relative): gravity makes upward movement costly
local COST_HORIZONTAL   = 1.0      -- walking
local COST_DOWN          = 0.5      -- falling (gravity assists)
local COST_UP            = 2.5      -- jumping against gravity
local COST_DIAGONAL_DOWN = 0.8      -- diagonal with gravity
local COST_DIAGONAL_UP   = 2.0      -- diagonal against gravity
local COST_WALL_CLING    = 3.0      -- wall interaction cost

-- Max reachable distances (in tiles)
local MAX_JUMP_HEIGHT = 3           -- ~3 tiles vertical jump
local MAX_DASH_DIST   = 5           -- ~5 tiles per dash
local MAX_WALL_JUMP_HEIGHT = 2      -- vertical gain from wall jump
local MAX_CLIMB_HEIGHT = 6          -- stamina-limited wall climb

-- 8-directional neighbors with gravity-aware costs
local NEIGHBORS = {
    { dx =  1, dy =  0, cost = COST_HORIZONTAL },     -- right
    { dx = -1, dy =  0, cost = COST_HORIZONTAL },     -- left
    { dx =  0, dy =  1, cost = COST_DOWN },            -- down
    { dx =  0, dy = -1, cost = COST_UP },              -- up
    { dx =  1, dy =  1, cost = COST_DIAGONAL_DOWN },   -- down-right
    { dx = -1, dy =  1, cost = COST_DIAGONAL_DOWN },   -- down-left
    { dx =  1, dy = -1, cost = COST_DIAGONAL_UP },     -- up-right
    { dx = -1, dy = -1, cost = COST_DIAGONAL_UP },     -- up-left
}

--------------------------------------------------------------------------------
-- Priority Queue (min-heap)
--------------------------------------------------------------------------------

local function createPQ()
    local heap = {}
    local size = 0

    local function parent(i) return math.floor(i / 2) end
    local function left(i) return 2 * i end
    local function right(i) return 2 * i + 1 end

    local function swap(a, b)
        heap[a], heap[b] = heap[b], heap[a]
    end

    local function siftUp(i)
        while i > 1 and heap[parent(i)].f > heap[i].f do
            swap(i, parent(i))
            i = parent(i)
        end
    end

    local function siftDown(i)
        local smallest = i
        local l = left(i)
        local r = right(i)
        if l <= size and heap[l].f < heap[smallest].f then smallest = l end
        if r <= size and heap[r].f < heap[smallest].f then smallest = r end
        if smallest ~= i then
            swap(i, smallest)
            siftDown(smallest)
        end
    end

    return {
        push = function(node)
            size = size + 1
            heap[size] = node
            siftUp(size)
        end,
        pop = function()
            if size == 0 then return nil end
            local top = heap[1]
            heap[1] = heap[size]
            heap[size] = nil
            size = size - 1
            if size > 0 then siftDown(1) end
            return top
        end,
        isEmpty = function()
            return size == 0
        end,
    }
end

--------------------------------------------------------------------------------
-- Heuristic
--------------------------------------------------------------------------------

--- Gravity-weighted heuristic for A*.
-- Vertical distance costs more going up than going down.
-- @param x1, y1  Current position
-- @param x2, y2  Goal position
-- @return number  Estimated cost
function pathfinder.heuristic(x1, y1, x2, y2)
    local dx = math.abs(x2 - x1)
    local dy = y2 - y1  -- positive = below, negative = above

    local horizontalCost = dx * COST_HORIZONTAL
    local verticalCost

    if dy >= 0 then
        -- Goal is below: gravity helps
        verticalCost = dy * COST_DOWN
    else
        -- Goal is above: must jump/dash upward
        verticalCost = math.abs(dy) * COST_UP
    end

    return horizontalCost + verticalCost
end

--------------------------------------------------------------------------------
-- Tile Access Helpers
--------------------------------------------------------------------------------

local function isWalkable(matrix, x, y, w, h)
    if x < 1 or y < 1 or x > w or y > h then
        return false  -- out of bounds = impassable
    end
    local tile = matrix:get(x, y, "0")
    return tile == TILE_AIR or tile == " "
end

local function isSolid(matrix, x, y, w, h)
    if x < 1 or y < 1 or x > w or y > h then
        return true  -- out of bounds = solid
    end
    local tile = matrix:get(x, y, "0")
    return tile ~= TILE_AIR and tile ~= " "
end

--- Check if a position has ground below it.
local function hasGround(matrix, x, y, w, h)
    return y >= h or isSolid(matrix, x, y + 1, w, h)
end

--- Check if a position has a wall adjacent.
local function hasWall(matrix, x, y, w, h)
    return isSolid(matrix, x - 1, y, w, h) or isSolid(matrix, x + 1, y, w, h)
end

--------------------------------------------------------------------------------
-- Movement Context Modifier
--------------------------------------------------------------------------------

--- Adjust neighbor costs based on surroundings (ground contact, wall proximity).
-- This models Celeste's movement system more accurately.
local function getMovementCost(matrix, x, y, nx, ny, baseCost, w, h)
    local cost = baseCost

    -- On ground: normal movement
    if hasGround(matrix, x, y, w, h) then
        -- Can jump from ground
        if ny < y then
            local jumpDist = y - ny
            if jumpDist <= MAX_JUMP_HEIGHT then
                cost = cost * 0.6  -- jumps from ground are easier
            elseif jumpDist <= MAX_JUMP_HEIGHT + MAX_DASH_DIST then
                cost = cost * 1.0  -- needs dash to reach
            else
                cost = cost * 5.0  -- very difficult without special mechanics
            end
        end
    end

    -- Near wall: wall jump / wall climb possible
    if hasWall(matrix, x, y, w, h) then
        if ny < y then
            local climbDist = y - ny
            if climbDist <= MAX_WALL_JUMP_HEIGHT then
                cost = cost * 0.7  -- wall jump is efficient
            elseif climbDist <= MAX_CLIMB_HEIGHT then
                cost = cost * 1.2  -- climbing is feasible but costly
            end
        end
    end

    -- Falling into air (no ground below destination)
    if not hasGround(matrix, nx, ny, w, h) and ny >= y then
        -- Falling is easy
        cost = cost * 0.8
    end

    return cost
end

--------------------------------------------------------------------------------
-- A* Pathfinding
--------------------------------------------------------------------------------

--- Run Celeste-A* between two points on a tile matrix.
-- @param matrix    Tile matrix (matrix:get(x,y), matrix:size())
-- @param startX    Start X (tile coords, 1-based)
-- @param startY    Start Y (tile coords, 1-based)
-- @param goalX     Goal X (tile coords, 1-based)
-- @param goalY     Goal Y (tile coords, 1-based)
-- @param maxNodes  Max nodes to explore before giving up (default 5000)
-- @return table|nil  Path as list of {x,y} or nil if unreachable
-- @return number     Cost of path or math.huge
function pathfinder.findPath(matrix, startX, startY, goalX, goalY, maxNodes)
    maxNodes = maxNodes or 5000

    local w, h = matrix:size()

    -- Validate start/end
    if not isWalkable(matrix, startX, startY, w, h) then
        return nil, math.huge
    end
    if not isWalkable(matrix, goalX, goalY, w, h) then
        return nil, math.huge
    end

    local pq = createPQ()
    local visited = {}
    local gScore = {}
    local cameFrom = {}
    local nodesExplored = 0

    local function key(x, y) return x .. "," .. y end

    local startKey = key(startX, startY)
    gScore[startKey] = 0
    pq.push({
        x = startX, y = startY,
        f = pathfinder.heuristic(startX, startY, goalX, goalY),
    })

    while not pq.isEmpty() do
        local current = pq.pop()
        local cx, cy = current.x, current.y
        local ck = key(cx, cy)

        if visited[ck] then
            goto continue
        end
        visited[ck] = true
        nodesExplored = nodesExplored + 1

        -- Reached goal
        if cx == goalX and cy == goalY then
            -- Reconstruct path
            local path = {}
            local k = ck
            while k do
                local parts = {}
                for part in k:gmatch("[^,]+") do
                    table.insert(parts, tonumber(part))
                end
                table.insert(path, 1, { x = parts[1], y = parts[2] })
                k = cameFrom[k]
            end
            return path, gScore[ck]
        end

        -- Node limit
        if nodesExplored >= maxNodes then
            return nil, math.huge
        end

        -- Explore neighbors
        for _, neighbor in ipairs(NEIGHBORS) do
            local nx = cx + neighbor.dx
            local ny = cy + neighbor.dy
            local nk = key(nx, ny)

            if not visited[nk] and isWalkable(matrix, nx, ny, w, h) then
                local moveCost = getMovementCost(matrix, cx, cy, nx, ny, neighbor.cost, w, h)
                local tentativeG = gScore[ck] + moveCost

                if not gScore[nk] or tentativeG < gScore[nk] then
                    gScore[nk] = tentativeG
                    cameFrom[nk] = ck
                    local h = pathfinder.heuristic(nx, ny, goalX, goalY)
                    pq.push({
                        x = nx, y = ny,
                        f = tentativeG + h,
                    })
                end
            end
        end

        ::continue::
    end

    return nil, math.huge  -- no path found
end

--------------------------------------------------------------------------------
-- Room Playability Evaluation
--------------------------------------------------------------------------------

--- Evaluate whether a room is playable by checking if exits are reachable.
-- @param matrix    Tile matrix
-- @param exits     Table of exit directions { left=bool, right=bool, top=bool, bottom=bool }
-- @param exitSize  Size of exit openings in tiles (default 4)
-- @return boolean  Whether a path exists between at least one pair of exits
-- @return table    Detailed results per exit pair
function pathfinder.evaluatePlayability(matrix, exits, exitSize)
    exitSize = exitSize or 4

    local w, h = matrix:size()
    local results = {}
    local anyReachable = false

    -- Determine exit positions (center of exit opening)
    local exitPositions = {}
    if exits.left then
        local ey = math.floor(h / 2)
        exitPositions.left = { x = 1, y = ey }
    end
    if exits.right then
        local ey = math.floor(h / 2)
        exitPositions.right = { x = w, y = ey }
    end
    if exits.top then
        local ex = math.floor(w / 2)
        exitPositions.top = { x = ex, y = 1 }
    end
    if exits.bottom then
        local ex = math.floor(w / 2)
        exitPositions.bottom = { x = ex, y = h }
    end

    -- Try to find paths between all exit pairs
    local exitNames = {}
    for name, _ in pairs(exitPositions) do
        table.insert(exitNames, name)
    end

    for i = 1, #exitNames do
        for j = i + 1, #exitNames do
            local nameA = exitNames[i]
            local nameB = exitNames[j]
            local posA = exitPositions[nameA]
            local posB = exitPositions[nameB]

            local path, cost = pathfinder.findPath(
                matrix, posA.x, posA.y, posB.x, posB.y
            )

            local result = {
                from = nameA,
                to = nameB,
                reachable = path ~= nil,
                cost = cost,
                pathLength = path and #path or 0,
            }
            table.insert(results, result)

            if path then
                anyReachable = true
            end
        end
    end

    return anyReachable, results
end

--------------------------------------------------------------------------------
-- NLE Proximity Score
--------------------------------------------------------------------------------

--- Calculate NLE (Non-Lethal Entity) proximity score for a path.
-- Rooms with more NLEs near the critical path are considered more playable.
-- @param path       Path from findPath()
-- @param entities   List of entity descriptors with .x, .y fields
-- @param nleTypes   Set of NLE type strings (e.g., {"Refill" = true, "Spring" = true})
-- @param radius     Search radius in tiles (default 3)
-- @return number    NLE proximity score (higher = more accessible NLEs)
function pathfinder.nleProximityScore(path, entities, nleTypes, radius)
    radius = radius or 3
    if not path or #path == 0 then return 0 end

    local score = 0
    local countedEntities = {}  -- avoid double-counting

    for _, pathNode in ipairs(path) do
        for _, entity in ipairs(entities) do
            if not countedEntities[entity] then
                local isNLE = nleTypes[entity._name] or nleTypes[entity.name] or false
                if isNLE then
                    -- Entity positions are in pixels; convert to tile coords
                    local ex = math.floor(entity.x / 8) + 1
                    local ey = math.floor(entity.y / 8) + 1
                    local dist = math.abs(pathNode.x - ex) + math.abs(pathNode.y - ey)
                    if dist <= radius then
                        score = score + (radius - dist + 1)
                        countedEntities[entity] = true
                    end
                end
            end
        end
    end

    return score
end

return pathfinder
