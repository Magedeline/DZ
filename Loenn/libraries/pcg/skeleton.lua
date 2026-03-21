-- PCG Celeskeleton: Level Skeleton Generator
-- Generates the high-level room layout (which rooms connect to which)
-- Based on the Celeskeleton concept from the Celeste AI Framework paper.
--
-- A level skeleton defines:
--   - How many rooms exist
--   - Where each room is positioned in 2D space
--   - Which sides of each room have exits (connections to neighbors)
--   - A guaranteed path from start room to end room

local skeleton = {}

--------------------------------------------------------------------------------
-- Room Descriptor
--------------------------------------------------------------------------------

--- Create a room descriptor used by the skeleton.
-- @param id       Unique integer ID
-- @param gridX    Grid column (0-based)
-- @param gridY    Grid row (0-based)
-- @param roomW    Room width in tiles (default 40)
-- @param roomH    Room height in tiles (default 23)
-- @return table   Room descriptor
function skeleton.createRoom(id, gridX, gridY, roomW, roomH)
    roomW = roomW or 40
    roomH = roomH or 23

    return {
        id = id,
        gridX = gridX,
        gridY = gridY,
        width  = roomW * 8,   -- in pixels
        height = roomH * 8,   -- in pixels
        tilesW = roomW,
        tilesH = roomH,
        exits = { left = false, right = false, top = false, bottom = false },
        isStart = false,
        isEnd = false,
        connections = {},       -- list of connected room IDs
    }
end

--------------------------------------------------------------------------------
-- Direction Helpers
--------------------------------------------------------------------------------

local DIRECTIONS = {
    { name = "right",  dx =  1, dy =  0, opposite = "left"   },
    { name = "left",   dx = -1, dy =  0, opposite = "right"  },
    { name = "down",   dx =  0, dy =  1, opposite = "top"    },
    { name = "up",     dx =  0, dy = -1, opposite = "bottom" },
}

local function shuffleArray(arr)
    local n = #arr
    for i = n, 2, -1 do
        local j = math.random(1, i)
        arr[i], arr[j] = arr[j], arr[i]
    end
    return arr
end

--------------------------------------------------------------------------------
-- Skeleton Generation
--------------------------------------------------------------------------------

--- Generate a level skeleton with connected rooms.
-- Uses a random walk + branching algorithm inspired by Spelunky's approach.
--
-- @param numRooms     Number of rooms to generate (default 10)
-- @param roomW        Room width in tiles (default 40)
-- @param roomH        Room height in tiles (default 23)
-- @param branchProb   Probability of branching [0..1] (default 0.3)
-- @param seed         Optional random seed
-- @return table       { rooms = {room_desc, ...}, startRoom = id, endRoom = id }
function skeleton.generate(numRooms, roomW, roomH, branchProb, seed)
    numRooms = numRooms or 10
    roomW = roomW or 40
    roomH = roomH or 23
    branchProb = branchProb or 0.3

    if seed then
        math.randomseed(seed)
    end

    -- Grid to track occupied cells
    local grid = {}
    local function gridKey(gx, gy) return gx .. "," .. gy end
    local function isOccupied(gx, gy) return grid[gridKey(gx, gy)] ~= nil end
    local function occupyCell(gx, gy, roomId) grid[gridKey(gx, gy)] = roomId end
    local function getRoomAt(gx, gy) return grid[gridKey(gx, gy)] end

    local rooms = {}
    local roomsById = {}
    local nextId = 1

    local function addRoom(gx, gy)
        local room = skeleton.createRoom(nextId, gx, gy, roomW, roomH)
        occupyCell(gx, gy, nextId)
        rooms[nextId] = room
        roomsById[nextId] = room
        nextId = nextId + 1
        return room
    end

    local function connectRooms(roomA, roomB, dirName, oppositeName)
        roomA.exits[dirName] = true
        roomB.exits[oppositeName] = true
        table.insert(roomA.connections, roomB.id)
        table.insert(roomB.connections, roomA.id)
    end

    -- Start with room at origin
    local startRoom = addRoom(0, 0)
    startRoom.isStart = true

    -- Random walk to create main path
    local current = startRoom
    local mainPath = { startRoom }
    local attempts = 0
    local maxAttempts = numRooms * 10

    while #rooms < numRooms and attempts < maxAttempts do
        attempts = attempts + 1

        local dirs = shuffleArray({1, 2, 3, 4})
        local placed = false

        for _, di in ipairs(dirs) do
            local dir = DIRECTIONS[di]
            local nx = current.gridX + dir.dx
            local ny = current.gridY + dir.dy

            if not isOccupied(nx, ny) then
                local newRoom = addRoom(nx, ny)
                connectRooms(current, newRoom, dir.name, dir.opposite)
                table.insert(mainPath, newRoom)
                current = newRoom
                placed = true
                break
            end
        end

        -- If stuck, backtrack along main path
        if not placed then
            if #mainPath > 1 then
                table.remove(mainPath)
                current = mainPath[#mainPath]
            else
                break  -- truly stuck
            end
        end

        -- Branching: occasionally start a side path from a random existing room
        if placed and math.random() < branchProb and #rooms < numRooms then
            local branchFrom = rooms[math.random(1, #rooms)]
            local branchDirs = shuffleArray({1, 2, 3, 4})

            for _, di in ipairs(branchDirs) do
                local dir = DIRECTIONS[di]
                local bx = branchFrom.gridX + dir.dx
                local by = branchFrom.gridY + dir.dy

                if not isOccupied(bx, by) and #rooms < numRooms then
                    local branchRoom = addRoom(bx, by)
                    connectRooms(branchFrom, branchRoom, dir.name, dir.opposite)
                    break
                end
            end
        end
    end

    -- Mark the last room on the main path as end
    current.isEnd = true

    -- Convert grid positions to pixel positions for the map
    for _, room in pairs(rooms) do
        room.x = room.gridX * room.width
        room.y = room.gridY * room.height
    end

    -- Build ordered list
    local orderedRooms = {}
    for i = 1, #rooms do
        table.insert(orderedRooms, rooms[i])
    end

    return {
        rooms = orderedRooms,
        startRoom = startRoom.id,
        endRoom = current.id,
        grid = grid,
    }
end

--------------------------------------------------------------------------------
-- Skeleton Visualization (for debug/preview)
--------------------------------------------------------------------------------

--- Generate a simple text visualization of the skeleton.
-- @param skel  Skeleton data from generate()
-- @return string  ASCII art representation
function skeleton.visualize(skel)
    -- Find grid bounds
    local minGX, maxGX, minGY, maxGY = math.huge, -math.huge, math.huge, -math.huge
    for _, room in ipairs(skel.rooms) do
        minGX = math.min(minGX, room.gridX)
        maxGX = math.max(maxGX, room.gridX)
        minGY = math.min(minGY, room.gridY)
        maxGY = math.max(maxGY, room.gridY)
    end

    -- Build lookup
    local lookup = {}
    for _, room in ipairs(skel.rooms) do
        lookup[room.gridX .. "," .. room.gridY] = room
    end

    local lines = {}
    for gy = minGY, maxGY do
        local row = {}
        for gx = minGX, maxGX do
            local room = lookup[gx .. "," .. gy]
            if room then
                if room.isStart then
                    table.insert(row, "[S]")
                elseif room.isEnd then
                    table.insert(row, "[E]")
                else
                    table.insert(row, string.format("[%d]", room.id))
                end
            else
                table.insert(row, "   ")
            end

            -- Horizontal connection
            if gx < maxGX then
                local roomRight = lookup[(gx + 1) .. "," .. gy]
                if room and roomRight and room.exits.right then
                    table.insert(row, "-")
                else
                    table.insert(row, " ")
                end
            end
        end
        table.insert(lines, table.concat(row))

        -- Vertical connections
        if gy < maxGY then
            local connRow = {}
            for gx = minGX, maxGX do
                local room = lookup[gx .. "," .. gy]
                local roomBelow = lookup[gx .. "," .. (gy + 1)]
                if room and roomBelow and room.exits.bottom then
                    table.insert(connRow, " | ")
                else
                    table.insert(connRow, "   ")
                end
                if gx < maxGX then
                    table.insert(connRow, " ")
                end
            end
            table.insert(lines, table.concat(connRow))
        end
    end

    return table.concat(lines, "\n")
end

return skeleton
