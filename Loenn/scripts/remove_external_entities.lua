-- External Entity Remover for Loenn
-- Removes all entities from external mods that are no longer dependencies
-- WARNING: Make a backup of your maps before running!

local state = require("loaded_state")
local history = require("history")

local script = {}

script.name = "removeExternalEntities"
script.displayName = "REMOVE: External Mod Entities (DANGER)"
script.tooltip = "⚠️ DELETES all entities from removed external mods!\nMake a backup first!\nRun Audit script first to see what will be removed."

-- External mod prefixes to REMOVE
local EXTERNAL_MOD_PREFIXES = {
    "AdventureHelper/",
    "Anonhelper/",
    "AxoHelper/",
    "BounceHelper/",
    "BrokemiaHelper/",
    "CherryHelper/",
    "ChroniaHelper/",
    "CollabUtils2/",
    "CommunalHelper/",
    "ContortHelper/",
    "CrystallineHelper/",
    "DJMapHelper/",
    "DoonvHelper/",  -- Keep if you haven't ported graphics yet
    "EeveeHelper/",
    "ExtendedVariantMode/",
    "FemtoHelper/",
    "FlaglinesAndSuch/",
    "FrostHelper/",
    "HonlyHelper/",
    "IsaGrabBag/",
    "JackalHelper/",
    "JungleHelper/",
    "LunaticHelper/",
    "MaxHelpingHand/",
    "ModderToolkit/",
    "MoreDasheline/",
    "NerdHelper/",
    "OutbackHelper/",
    "ReverseHelper/",
    "SorbetHelper/",
    "VivHelper/",
    }

script.parameters = {
    dryRun = true,  -- Default to safe mode (preview only)
    includeDoonvHelper = false,  -- Set to true only after porting graphics
    backupMaps = true,
    }

script.fieldInformation = {
    dryRun = {
        fieldType = "boolean",
        description = "When enabled, only shows what would be removed without actually deleting"
    },
    includeDoonvHelper = {
        fieldType = "boolean",
        description = "Also remove DoonvHelper entities (only enable after porting graphics!)"
    },
    backupMaps = {
        fieldType = "boolean",
        description = "Recommend keeping backups enabled"
    },
    }

script.fieldOrder = {"dryRun", "includeDoonvHelper", "backupMaps"}

function script.run(args)
    local map = state.map
    if not map then
        print("[ERROR] No map loaded!")
        return
    end
    
    if args.includeDoonvHelper then
        table.insert(EXTERNAL_MOD_PREFIXES, "DoonvHelper/")
    end
    
    print(string.format("\n=== %s MODE ===", args.dryRun and "DRY RUN (Preview)" or "LIVE DELETION"))
    
    if not args.dryRun then
        print("⚠️  WARNING: This will PERMANENTLY DELETE entities!")
        print("Creating history entry for undo...")
        history.addSnapshot()
    end
    
    local totalRemoved = 0
    local removedEntities = {}
    local removedTriggers = {}
    
    -- Scan all rooms
    for roomIdx, roomData in ipairs(map.rooms or {}) do
        local roomName = roomData.name or ("Room_" .. roomIdx)
        
        -- Process entities
        if roomData.entities then
            local toRemove = {}
            for i, entity in ipairs(roomData.entities) do
                local name = entity._name or entity.name
                if name and isExternalEntity(name) then
                    table.insert(toRemove, i)
                    addRemoval(removedEntities, name, roomName)
                end
            end
            
            -- Remove in reverse order to preserve indices
            for i = #toRemove, 1, -1 do
                local idx = toRemove[i]
                if not args.dryRun then
                    table.remove(roomData.entities, idx)
                end
                totalRemoved = totalRemoved + 1
            end
        end
        
        -- Process triggers
        if roomData.triggers then
            local toRemove = {}
            for i, trigger in ipairs(roomData.triggers) do
                local name = trigger._name or trigger.name
                if name and isExternalEntity(name) then
                    table.insert(toRemove, i)
                    addRemoval(removedTriggers, name, roomName)
                end
            end
            
            -- Remove in reverse order
            for i = #toRemove, 1, -1 do
                local idx = toRemove[i]
                if not args.dryRun then
                    table.remove(roomData.triggers, idx)
                end
                totalRemoved = totalRemoved + 1
            end
        end
    end
    
    -- Print report
    print("\n=== REMOVAL REPORT ===")
    
    print("\n[ENTITIES REMOVED]")
    printRemovalList(removedEntities)
    
    print("\n[TRIGGERS REMOVED]")
    printRemovalList(removedTriggers)
    
    print(string.format("\n=== SUMMARY ==="))
    print(string.format("Total items %s: %d", args.dryRun and "that would be removed" or "removed", totalRemoved))
    
    if args.dryRun then
        print("\n[DRY RUN COMPLETE]")
        print("Set 'dryRun' to false to actually remove these entities.")
        print("⚠️  Make sure you have backups before running live deletion!")
    else
        print("\n[LIVE DELETION COMPLETE]")
        print("Entities have been removed from the map.")
        print("Use Ctrl+Z in Loenn to undo if needed.")
        print("Remember to save the map (Ctrl+S) to persist changes.")
    end
end

function isExternalEntity(name)
    for _, prefix in ipairs(EXTERNAL_MOD_PREFIXES) do
        if name:find("^" .. prefix) then
            return true
        end
    end
    return false
end

function addRemoval(list, name, roomName)
    if not list[name] then
        list[name] = {}
    end
    table.insert(list[name], roomName)
end

function printRemovalList(list)
    if next(list) == nil then
        print("  (none)")
        return
    end
    
    for name, rooms in pairs(list) do
        print(string.format("  %s (rooms: %d)", name, #rooms))
        for _, roomName in ipairs(rooms) do
            print(string.format("    - %s", roomName))
        end
    end
end

return script
