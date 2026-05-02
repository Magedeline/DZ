-- Lönn Script: Add Warp Triggers to Connect Summit Rooms
-- This connects each room to the next with transition triggers
-- Run AFTER build_lastlevel_summit.lua

local state = require("loaded_state")
local history = require("history")

-- Configuration
local WARP_WIDTH = 32
local WARP_HEIGHT = 32

function addWarpTriggers()
    local map = state.map
    if not map then
        print("[ERROR] No map loaded!")
        return false
    end
    
    -- Collect and sort rooms by name (summit_CP_RR format)
    local rooms = {}
    for _, room in ipairs(map.rooms or {}) do
        if room.name and room.name:match("^summit_%d%d_%d%d$") then
            table.insert(rooms, room)
        end
    end
    
    table.sort(rooms, function(a, b) return a.name < b.name end)
    
    local roomCount = #rooms
    if roomCount == 0 then
        print("[ERROR] No summit rooms found! Run build_lastlevel_summit.lua first.")
        return false
    end
    
    print(string.format("[INFO] Adding warp triggers to %d rooms...", roomCount))
    
    -- Create history snapshot
    history.addSnapshot()
    
    for i, room in ipairs(rooms) do
        -- Initialize triggers table if needed
        if not room.triggers then
            room.triggers = {}
        end
        
        -- Add transition to next room (except for final room)
        if i < roomCount then
            local nextRoom = rooms[i + 1]
            
            -- Add exit trigger at top-right of current room (going up)
            table.insert(room.triggers, {
                _name = "transitionTrigger",
                name = "transitionTrigger",
                x = room.width - WARP_WIDTH,
                y = 0,
                width = WARP_WIDTH,
                height = WARP_HEIGHT,
                toRoom = nextRoom.name,
                toX = 40,
                toY = room.height - 48
            })
            
            print(string.format("[INFO] %s -> %s (top exit)", room.name, nextRoom.name))
        end
        
        -- Add safety return trigger at bottom for all rooms except first
        if i > 1 then
            local prevRoom = rooms[i - 1]
            
            -- Add return trigger at bottom-left
            table.insert(room.triggers, {
                _name = "transitionTrigger",
                name = "transitionTrigger",
                x = 0,
                y = room.height - WARP_HEIGHT,
                width = WARP_WIDTH,
                height = WARP_HEIGHT,
                toRoom = prevRoom.name,
                toX = prevRoom.width - 56,
                toY = 48
            })
        end
    end
    
    print("\n[SUCCESS] Warp triggers added!")
    print("  - Top-right exits advance to next room")
    print("  - Bottom-left exits return to previous room")
    print("\n[IMPORTANT] Save the map (Ctrl+S) to persist changes!")
    
    return true
end

-- Run the script
addWarpTriggers()
