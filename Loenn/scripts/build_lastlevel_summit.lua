-- Lönn Script: Build Summit Map Structure for lastlevel.bin
-- Creates a summit-style map with 12 checkpoints, 10 rooms each (120 total rooms)
-- Usage: Open lastlevel.bin in Loenn, then run this script from the console

local state = require("loaded_state")
local history = require("history")

-- Configuration
local CONFIG = {
    CHECKPOINTS = 12,
    ROOMS_PER_CHECKPOINT = 10,
    ROOM_WIDTH = 320,
    ROOM_HEIGHT = 184,
    ROOM_SPACING_X = 400,  -- Horizontal spacing between rooms
    ROOM_SPACING_Y = 240,  -- Vertical spacing (for summit vertical climb)
    CHECKPOINT_SPACING_Y = 2400,  -- Vertical spacing between checkpoints
    START_X = 0,
    START_Y = 0,
}

function buildSummitMap()
    local map = state.map
    if not map then
        print("[ERROR] No map loaded! Please open lastlevel.bin first.")
        return false
    end
    
    print("[INFO] Building summit map for lastlevel.bin...")
    print(string.format("[INFO] Creating %d checkpoints with %d rooms each = %d total rooms",
        CONFIG.CHECKPOINTS, CONFIG.ROOMS_PER_CHECKPOINT, 
        CONFIG.CHECKPOINTS * CONFIG.ROOMS_PER_CHECKPOINT))
    
    -- Create history snapshot for undo
    history.addSnapshot()
    
    -- Clear existing rooms (optional - comment out if you want to keep existing)
    local existingRoomCount = #map.rooms
    if existingRoomCount > 0 then
        print(string.format("[INFO] Clearing %d existing rooms...", existingRoomCount))
        map.rooms = {}
    end
    
    -- Initialize rooms table
    if not map.rooms then
        map.rooms = {}
    end
    
    local totalRooms = 0
    
    -- Create rooms for each checkpoint
    for cp = 1, CONFIG.CHECKPOINTS do
        local checkpointId = string.format("cp_%d", cp)
        local baseY = CONFIG.START_Y - ((cp - 1) * CONFIG.CHECKPOINT_SPACING_Y)
        
        print(string.format("[INFO] Creating checkpoint %s...", checkpointId))
        
        for roomNum = 1, CONFIG.ROOMS_PER_CHECKPOINT do
            local roomIndex = ((cp - 1) * CONFIG.ROOMS_PER_CHECKPOINT) + roomNum
            local roomName = string.format("summit_%02d_%02d", cp, roomNum)
            
            -- Calculate position (vertical summit climb with slight zigzag)
            local offsetX = ((roomNum - 1) % 2) * 80  -- Slight zigzag
            local roomX = CONFIG.START_X + offsetX
            local roomY = baseY - ((roomNum - 1) * CONFIG.ROOM_SPACING_Y)
            
            -- Create room
            local room = createRoom(roomName, roomX, roomY, 
                CONFIG.ROOM_WIDTH, CONFIG.ROOM_HEIGHT, checkpointId, cp, roomNum)
            
            table.insert(map.rooms, room)
            totalRooms = totalRooms + 1
        end
        
        print(string.format("[INFO] Checkpoint %s complete (%d rooms)", checkpointId, CONFIG.ROOMS_PER_CHECKPOINT))
    end
    
    print(string.format("\n[SUCCESS] Summit map built!"))
    print(string.format("  Total rooms created: %d", totalRooms))
    print(string.format("  Total checkpoints: %d", CONFIG.CHECKPOINTS))
    print(string.format("  Rooms per checkpoint: %d", CONFIG.ROOMS_PER_CHECKPOINT))
    print(string.format("\n[IMPORTANT] Remember to:"))
    print("  1. Save the map (Ctrl+S)")
    print("  2. Add tilemap data (fg/bg tiles) to rooms")
    print("  3. Add gameplay entities (spring, dream blocks, etc.)")
    print("  4. Set up level end trigger in final room")
    
    return true
end

function createRoom(name, x, y, width, height, checkpointId, cpNum, roomNum)
    local room = {
        name = name,
        x = x,
        y = y,
        width = width,
        height = height,
        entities = {},
        triggers = {},
        decalsFg = {},
        decalsBg = {},
        tilesFg = {},
        tilesBg = {},
        parallax = {},
        music = "",
        ambience = "",
        color = 0,
        disableDownTransition = false,
        windPattern = "None"
    }
    
    -- Initialize tilemap structures (empty - you'll fill these in Loenn)
    room.tilesFg = createEmptyTilemap(width, height)
    room.tilesBg = createEmptyTilemap(width, height)
    
    -- Add spawnpoint
    local spawnX = 40
    local spawnY = height - 32
    table.insert(room.entities, {
        _name = "spawnpoint",
        name = "spawnpoint",
        x = spawnX,
        y = spawnY,
        width = 8,
        height = 8
    })
    
    -- Add checkpoint trigger at start of first room in each checkpoint (except cp_1 which starts at level start)
    if roomNum == 1 and checkpointId ~= "cp_1" then
        table.insert(room.triggers, {
            _name = "MaggyHelper/CheckpointTrigger",
            name = "MaggyHelper/CheckpointTrigger",
            x = 0,
            y = 0,
            width = 16,
            height = height,
            checkpointId = checkpointId,
            showEffect = true
        })
        print(string.format("    [INFO] Added checkpoint trigger for %s in room %s", checkpointId, name))
    end
    
    -- Add level end trigger for final room
    if cpNum == CONFIG.CHECKPOINTS and roomNum == CONFIG.ROOMS_PER_CHECKPOINT then
        table.insert(room.triggers, {
            _name = "levelEndTrigger",
            name = "levelEndTrigger",
            x = width - 16,
            y = 0,
            width = 16,
            height = height,
            mode = "ChapterComplete"
        })
        print(string.format("    [INFO] Added chapter complete trigger in final room %s", name))
    end
    
    return room
end

function createEmptyTilemap(width, height)
    local tileWidth = math.floor(width / 8)
    local tileHeight = math.floor(height / 8)
    local tiles = {}
    
    for y = 1, tileHeight do
        local row = {}
        for x = 1, tileWidth do
            table.insert(row, "0")  -- Empty tile
        end
        table.insert(tiles, row)
    end
    
    return tiles
end

-- Run the builder
buildSummitMap()
