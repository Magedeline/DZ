-- Lönn Room Editor Script: Swap Vanilla Player to K_Player (Custom Player)
-- Place this file in: Loenn/scripts/ and run from Lönn console
-- Usage: Run this script to replace vanilla player spawn with DZ/Player
-- This uses the mod's custom K_Player class with enhanced features

local utils = require("utils")
local state = require("state")

-- Configuration
local SPRITE_MODE = "Madeline"  -- Options: "Madeline", "MadelineNoBackpack", "Badeline", "MadelineAsBadeline", "BadelineAsMadeline"
local INTRO_TYPE = "Transition" -- Options: "Transition", "Respawn", "WalkInRight", "WalkInLeft", "Jump", "WakeUp", "Fall", "TempleMirrorVoid", "None"
local MAX_HEALTH = 6            -- Range: 1-20 (default: 6)
local KIRBY_MODE = false        -- Enable Kirby features (inhale, float, hat+scarf)
local COMBAT_ENABLED = false    -- Enable combat abilities (dash attack, combos)

-- Sprite modes available:
-- "Madeline"           - Standard Madeline
-- "MadelineNoBackpack" - Madeline without backpack
-- "Badeline"           - Badeline
-- "MadelineAsBadeline" - Madeline with Badeline appearance
-- "BadelineAsMadeline" - Badeline with Madeline appearance

-- Intro types:
-- "Transition"         - Standard room transition
-- "Respawn"            - Spawn from respawn point
-- "WalkInRight"        - Walk in from right side
-- "WalkInLeft"         - Walk in from left side
-- "Jump"               - Jump into room
-- "WakeUp"             - Wake up animation (campfire style)
-- "Fall"               - Fall into room
-- "TempleMirrorVoid"   - Temple mirror void sequence
-- "None"               - Instant spawn, no animation

function swapPlayerToKPlayerInRoom(room)
    if not room or not room.entities then
        return 0
    end
    
    local replacedCount = 0
    local entitiesToRemove = {}
    local entitiesToAdd = {}
    
    -- Find all vanilla player spawns
    for i, entity in ipairs(room.entities) do
        -- Check for vanilla player or spawnpoint
        if entity.name == "player" or entity.name == "spawnpoint" then
            table.insert(entitiesToRemove, i)
            
            -- Create K_Player at same position
            local kPlayer = {
                name = "DZ/K_Player",
                x = entity.x,
                y = entity.y,
                width = 0,
                height = 0,
                nodes = {},
                data = {
                    spriteMode = SPRITE_MODE,
                    introType = INTRO_TYPE,
                    maxHealth = MAX_HEALTH,
                    kirbyMode = KIRBY_MODE,
                    combatEnabled = COMBAT_ENABLED
                }
            }

            table.insert(entitiesToAdd, kPlayer)
            replacedCount = replacedCount + 1

            print(string.format("[INFO] Room '%s': Replaced %s at (%d, %d) with K_Player (sprite: %s, intro: %s, health: %d, kirby: %s, combat: %s)",
                room.name, entity.name, entity.x, entity.y, SPRITE_MODE, INTRO_TYPE, MAX_HEALTH,
                tostring(KIRBY_MODE), tostring(COMBAT_ENABLED)))
        end
    end
    
    -- Remove old entities (in reverse order to maintain indices)
    for i = #entitiesToRemove, 1, -1 do
        table.remove(room.entities, entitiesToRemove[i])
    end
    
    -- Add new entities
    for _, entity in ipairs(entitiesToAdd) do
        table.insert(room.entities, entity)
    end
    
    return replacedCount
end

function swapPlayerToKPlayerCurrentRoom()
    local map = state.map
    if not map then
        print("[ERROR] No map loaded")
        return false
    end
    
    local currentRoom = state.room
    if not currentRoom then
        print("[ERROR] No room selected")
        return false
    end
    
    print(string.format("[INFO] Processing room: %s", currentRoom.name))
    local count = swapPlayerToKPlayerInRoom(currentRoom)
    
    if count > 0 then
        print(string.format("[SUCCESS] Replaced %d player spawn(s) with K_Player in room '%s'", count, currentRoom.name))
    else
        print(string.format("[INFO] No vanilla player spawns found in room '%s'", currentRoom.name))
    end
    
    print("[INFO] Don't forget to save the map!")
    return true
end

function swapPlayerToKPlayerAllRooms()
    local map = state.map
    if not map then
        print("[ERROR] No map loaded")
        return false
    end
    
    if not map.rooms then
        print("[ERROR] No rooms in map")
        return false
    end
    
    local totalReplaced = 0
    
    print(string.format("[INFO] Processing %d rooms...", #map.rooms))
    
    for _, room in ipairs(map.rooms) do
        local count = swapPlayerToKPlayerInRoom(room)
        totalReplaced = totalReplaced + count
    end
    
    if totalReplaced > 0 then
        print(string.format("[SUCCESS] Replaced %d player spawn(s) with K_Player across all rooms", totalReplaced))
    else
        print("[INFO] No vanilla player spawns found in any room")
    end
    
    print("[INFO] Don't forget to save the map!")
    return true
end

-- Main execution
print("========================================")
print("K_Player (Custom Player) Swap Script")
print("========================================")
print(string.format("Configuration:"))
print(string.format("  Sprite Mode:    %s", SPRITE_MODE))
print(string.format("  Intro Type:     %s", INTRO_TYPE))
print(string.format("  Max Health:     %d", MAX_HEALTH))
print(string.format("  Kirby Mode:     %s", tostring(KIRBY_MODE)))
print(string.format("  Combat Enabled: %s", tostring(COMBAT_ENABLED)))
print("")
print("Available Sprite Modes:")
print("  Madeline           - Standard Madeline")
print("  MadelineNoBackpack - Madeline without backpack")
print("  Badeline           - Badeline (ghost form)")
print("  MadelineAsBadeline - Madeline using Badeline sprites")
print("  BadelineAsMadeline - Badeline using Madeline sprites")
print("")
print("Available Intro Types:")
print("  Transition, Respawn, WalkInRight, WalkInLeft")
print("  Jump, WakeUp, Fall, TempleMirrorVoid, None")
print("")
print("Usage:")
print("  swapPlayerToKPlayerCurrentRoom() - Replace in current room only")
print("  swapPlayerToKPlayerAllRooms()    - Replace in all rooms")
print("")
print("Edit the Configuration section at the top of this file to customize.")
print("========================================")

-- Uncomment the one you want to run by default:
swapPlayerToKPlayerCurrentRoom()
-- swapPlayerToKPlayerAllRooms()
