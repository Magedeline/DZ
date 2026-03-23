-- Lönn Room Editor Script: Add SpawnPoint to Kirby Test Room
-- Place this file in: Loenn/scripts/ and run from Lönn console
-- Usage: Run this script in Lönn to automatically add SpawnPoint to kirb room

local utils = require("utils")
local state = require("state")

-- Configuration
local ROOM_NAME = "kirb"
local SPAWN_X = 48
local SPAWN_Y = 160

function addSpawnPointToRoom()
    -- Get current map and room
    local map = state.map
    if not map then
        print("[ERROR] No map loaded")
        return false
    end
    
    -- Find the room
    local room = nil
    for _, r in ipairs(map.rooms) do
        if r.name == ROOM_NAME then
            room = r
            break
        end
    end
    
    if not room then
        print("[ERROR] Room '" .. ROOM_NAME .. "' not found in map")
        return false
    end
    
    print("[INFO] Found room: " .. room.name)
    
    -- Check if SpawnPoint already exists
    if room.entities then
        for _, entity in ipairs(room.entities) do
            if entity.name == "spawnpoint" then
                print("[WARN] SpawnPoint already exists at (" .. entity.x .. ", " .. entity.y .. ")")
                return true
            end
        end
    end
    
    -- Initialize entities list if needed
    if not room.entities then
        room.entities = {}
    end
    
    -- Create SpawnPoint entity
    local spawnpoint = {
        name = "spawnpoint",
        x = SPAWN_X,
        y = SPAWN_Y,
        width = 8,
        height = 8,
        nodes = {},
        data = {}
    }
    
    -- Add entity to room
    table.insert(room.entities, spawnpoint)
    
    print("[SUCCESS] Added SpawnPoint to room '" .. ROOM_NAME .. "' at (" .. SPAWN_X .. ", " .. SPAWN_Y .. ")")
    print("[INFO] Don't forget to save the map!")
    
    return true
end

-- Run the function
addSpawnPointToRoom()
