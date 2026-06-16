-- External Entity Auditor for Loenn
-- Scans maps for entities from removed external mods
-- Usage: Run from Loenn Scripts menu

local state = require("loaded_state")
local room = require("room")
local mods = require("mods")

local script = {}

script.name = "auditExternalEntities"
script.displayName = "Audit: Find External Mod Entities"
script.tooltip = "Scans current map for entities from external mods (removed dependencies).\nGenerates a report of entities to remove or port."

-- External mod prefixes that should NOT be in your maps anymore
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
    "DoonvHelper/",
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
    -- Add more as needed
}

-- Your mod's prefix (these are OK)
local YOUR_MOD_PREFIXES = {
    "DZ/",
    "Celeste/",  -- Vanilla entities
}

function script.run()
    local map = state.map
    if not map then
        print("[ERROR] No map loaded!")
        return
    end
    
    local externalEntities = {}
    local yourEntities = {}
    local unknownEntities = {}
    
    -- Scan all rooms
    for _, roomData in ipairs(map.rooms or {}) do
        -- Scan entities
        for _, entity in ipairs(roomData.entities or {}) do
            local name = entity._name or entity.name or "unknown"
            categorizeEntity(name, externalEntities, yourEntities, unknownEntities, roomData.name)
        end
        
        -- Scan triggers
        for _, trigger in ipairs(roomData.triggers or {}) do
            local name = trigger._name or trigger.name or "unknown"
            categorizeEntity(name, externalEntities, yourEntities, unknownEntities, roomData.name)
        end
    end
    
    -- Print report
    print("\n=== EXTERNAL ENTITY AUDIT REPORT ===\n")
    
    print("[EXTERNAL MOD ENTITIES - NEED REMOVAL/PORTING]")
    printEntityList(externalEntities)
    
    print("\n[YOUR MOD ENTITIES - OK]")
    printEntityList(yourEntities)
    
    print("\n[UNKNOWN/VANILLA ENTITIES - VERIFY MANUALLY]")
    printEntityList(unknownEntities)
    
    -- Summary
    local totalExternal = countEntities(externalEntities)
    print(string.format("\n=== SUMMARY ==="))
    print(string.format("External mod entities found: %d", totalExternal))
    print(string.format("Your mod entities: %d", countEntities(yourEntities)))
    print(string.format("Unknown/vanilla entities: %d", countEntities(unknownEntities)))
    
    if totalExternal > 0 then
        print("\n[WARNING] Found " .. totalExternal .. " external mod entities!")
        print("Run 'Remove External Entities' script to delete them, or port them to DZ.")
    else
        print("\n[OK] No external mod entities found. Maps are clean!")
    end
end

function categorizeEntity(name, external, yours, unknown, roomName)
    -- Check if it's from an external mod
    for _, prefix in ipairs(EXTERNAL_MOD_PREFIXES) do
        if name:find("^" .. prefix) then
            addToList(external, name, roomName)
            return
        end
    end
    
    -- Check if it's from your mod
    for _, prefix in ipairs(YOUR_MOD_PREFIXES) do
        if name:find("^" .. prefix) then
            addToList(yours, name, roomName)
            return
        end
    end
    
    -- Unknown - could be vanilla or other
    addToList(unknown, name, roomName)
end

function addToList(list, name, roomName)
    if not list[name] then
        list[name] = {}
    end
    table.insert(list[name], roomName)
end

function printEntityList(list)
    if next(list) == nil then
        print("  (none)")
        return
    end
    
    for name, rooms in pairs(list) do
        local roomCount = #rooms
        local roomList = table.concat(rooms, ", ")
        if #roomList > 50 then
            roomList = roomList:sub(1, 50) .. "..."
        end
        print(string.format("  %s (count: %d, rooms: %s)", name, roomCount, roomList))
    end
end

function countEntities(list)
    local count = 0
    for _, rooms in pairs(list) do
        count = count + #rooms
    end
    return count
end

return script
