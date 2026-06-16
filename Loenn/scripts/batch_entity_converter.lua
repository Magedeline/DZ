-- Lönn Room Editor Script: Batch Entity Converter
-- Place this file in: Loenn/scripts/ and run from Lönn console
-- Advanced utility for batch converting entities in maps
-- Can convert between vanilla player, KirbyPlayerSpawner, and K_Player

local utils = require("utils")
local state = require("state")

local Converter = {}

-- Configuration presets
Converter.Presets = {
    -- Convert vanilla player to Kirby with specific ability
    toKirbyFire = {
        source = {"player", "spawnpoint"},
        target = "DZ/KirbyPlayerSpawner",
        data = {
            enableKirbyMode = true,
            spawnCompanion = false,
            startingAbility = "Fire"
        }
    },
    toKirbyIce = {
        source = {"player", "spawnpoint"},
        target = "DZ/KirbyPlayerSpawner",
        data = {
            enableKirbyMode = true,
            spawnCompanion = false,
            startingAbility = "Ice"
        }
    },
    toKirbySword = {
        source = {"player", "spawnpoint"},
        target = "DZ/KirbyPlayerSpawner",
        data = {
            enableKirbyMode = true,
            spawnCompanion = false,
            startingAbility = "Sword"
        }
    },
    toKirbyWithCompanion = {
        source = {"player", "spawnpoint"},
        target = "DZ/KirbyPlayerSpawner",
        data = {
            enableKirbyMode = true,
            spawnCompanion = true,
            startingAbility = "None"
        }
    },
    -- Convert vanilla player to K_Player with various configurations
    toKPlayerMadeline = {
        source = {"player", "spawnpoint"},
        target = "DZ/K_Player",
        data = {
            spriteMode = "Madeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = false,
            combatEnabled = false
        }
    },
    toKPlayerBadeline = {
        source = {"player", "spawnpoint"},
        target = "DZ/K_Player",
        data = {
            spriteMode = "Badeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = false,
            combatEnabled = false
        }
    },
    toKPlayerKirbyMode = {
        source = {"player", "spawnpoint"},
        target = "DZ/K_Player",
        data = {
            spriteMode = "Madeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = true,
            combatEnabled = false
        }
    },
    toKPlayerCombatMode = {
        source = {"player", "spawnpoint"},
        target = "DZ/K_Player",
        data = {
            spriteMode = "Madeline",
            introType = "Transition",
            maxHealth = 6,
            kirbyMode = false,
            combatEnabled = true
        }
    },
    toKPlayerFullFeatures = {
        source = {"player", "spawnpoint"},
        target = "DZ/K_Player",
        data = {
            spriteMode = "Madeline",
            introType = "Transition",
            maxHealth = 10,
            kirbyMode = true,
            combatEnabled = true
        }
    },
    -- Convert Kirby spawner back to vanilla (for quick testing)
    toVanillaPlayer = {
        source = {"DZ/KirbyPlayerSpawner", "DZ/K_Player"},
        target = "player",
        data = {}
    }
}

-- Count entities of specific types in the map
function Converter:countEntities()
    local map = state.map
    if not map or not map.rooms then
        print("[ERROR] No map loaded")
        return nil
    end
    
    local counts = {
        vanillaPlayer = 0,
        spawnpoint = 0,
        kirbyPlayerSpawner = 0,
        kPlayer = 0,
        kirbyPlayerCore = 0,
        other = 0,
        total = 0
    }
    
    for _, room in ipairs(map.rooms) do
        if room.entities then
            for _, entity in ipairs(room.entities) do
                counts.total = counts.total + 1
                
                if entity.name == "player" then
                    counts.vanillaPlayer = counts.vanillaPlayer + 1
                elseif entity.name == "spawnpoint" then
                    counts.spawnpoint = counts.spawnpoint + 1
                elseif entity.name == "DZ/KirbyPlayerSpawner" then
                    counts.kirbyPlayerSpawner = counts.kirbyPlayerSpawner + 1
                elseif entity.name == "DZ/K_Player" then
                    counts.kPlayer = counts.kPlayer + 1
                elseif entity.name == "DZ/KirbyPlayerCore" then
                    counts.kirbyPlayerCore = counts.kirbyPlayerCore + 1
                else
                    counts.other = counts.other + 1
                end
            end
        end
    end
    
    return counts
end

-- Print entity statistics
function Converter:printStats()
    local counts = self:countEntities()
    if not counts then return end
    
    print("========================================")
    print("Entity Statistics")
    print("========================================")
    print(string.format("Vanilla Player:     %d", counts.vanillaPlayer))
    print(string.format("Spawn Points:       %d", counts.spawnpoint))
    print(string.format("KirbyPlayerSpawner: %d", counts.kirbyPlayerSpawner))
    print(string.format("K_Player:           %d", counts.kPlayer))
    print(string.format("KirbyPlayerCore:    %d", counts.kirbyPlayerCore))
    print(string.format("Other Entities:     %d", counts.other))
    print(string.format("------------------------"))
    print(string.format("Total Entities:     %d", counts.total))
    print("========================================")
end

-- Convert entities using a preset
function Converter:convertWithPreset(presetName, roomFilter)
    local preset = self.Presets[presetName]
    if not preset then
        print(string.format("[ERROR] Unknown preset: %s", presetName))
        print("[INFO] Available presets:")
        for name, _ in pairs(self.Presets) do
            print(string.format("  - %s", name))
        end
        return 0
    end
    
    local map = state.map
    if not map or not map.rooms then
        print("[ERROR] No map loaded")
        return 0
    end
    
    local totalConverted = 0
    
    for _, room in ipairs(map.rooms) do
        -- Apply room filter if specified
        if roomFilter and room.name ~= roomFilter then
            goto continue
        end
        
        if room.entities then
            local entitiesToRemove = {}
            local entitiesToAdd = {}
            
            for i, entity in ipairs(room.entities) do
                -- Check if entity matches any source type
                for _, sourceType in ipairs(preset.source) do
                    if entity.name == sourceType then
                        table.insert(entitiesToRemove, i)
                        
                        -- Create new entity
                        local newEntity = {
                            name = preset.target,
                            x = entity.x,
                            y = entity.y,
                            width = entity.width or 0,
                            height = entity.height or 0,
                            nodes = entity.nodes or {},
                            data = {}
                        }
                        
                        -- Copy preset data
                        for key, value in pairs(preset.data) do
                            newEntity.data[key] = value
                        end
                        
                        table.insert(entitiesToAdd, newEntity)
                        totalConverted = totalConverted + 1
                        
                        print(string.format("[INFO] Room '%s': Converted %s -> %s at (%d, %d)",
                            room.name, entity.name, preset.target, entity.x, entity.y))
                        break
                    end
                end
            end
            
            -- Remove old entities
            for i = #entitiesToRemove, 1, -1 do
                table.remove(room.entities, entitiesToRemove[i])
            end
            
            -- Add new entities
            for _, entity in ipairs(entitiesToAdd) do
                table.insert(room.entities, entity)
            end
        end
        
        ::continue::
    end
    
    print(string.format("[SUCCESS] Converted %d entities using preset '%s'", totalConverted, presetName))
    return totalConverted
end

-- Quick conversion functions for common use cases
function Converter:toKirbyInCurrentRoom(ability)
    local currentRoom = state.room
    if not currentRoom then
        print("[ERROR] No room selected")
        return 0
    end
    
    -- Create custom preset for this ability
    local presetName = "toKirby" .. (ability or "None")
    self.Presets[presetName] = {
        source = {"player", "spawnpoint"},
        target = "DZ/KirbyPlayerSpawner",
        data = {
            enableKirbyMode = true,
            spawnCompanion = false,
            startingAbility = ability or "None"
        }
    }
    
    return self:convertWithPreset(presetName, currentRoom.name)
end

function Converter:toKPlayerInCurrentRoom(spriteMode, options)
    local currentRoom = state.room
    if not currentRoom then
        print("[ERROR] No room selected")
        return 0
    end
    
    options = options or {}
    
    -- Create custom preset for this configuration
    local presetName = "toKPlayer" .. (spriteMode or "Madeline")
    self.Presets[presetName] = {
        source = {"player", "spawnpoint"},
        target = "DZ/K_Player",
        data = {
            spriteMode = spriteMode or "Madeline",
            introType = options.introType or "Transition",
            maxHealth = options.maxHealth or 6,
            kirbyMode = options.kirbyMode or false,
            combatEnabled = options.combatEnabled or false
        }
    }
    
    return self:convertWithPreset(presetName, currentRoom.name)
end

-- Export module
return Converter
