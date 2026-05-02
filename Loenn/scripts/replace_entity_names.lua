-- Entity Name Replacer for Loenn
-- Replaces external mod entity names with your own custom equivalents
-- Useful for porting DoonvHelper/DashCodeGate -> MaggyHelper/DashCodeGate

local state = require("loaded_state")
local history = require("history")

local script = {}

script.name = "replaceEntityNames"
script.displayName = "REPLACE: Entity Names (Porting Tool)"
script.tooltip = "Replaces entity names from external mods with your own.\nUseful for porting DoonvHelper entities to MaggyHelper."

-- Default replacement rules (edit these for your needs)
local DEFAULT_REPLACEMENTS = {
    -- Format: ["OldPrefix/EntityName"] = "NewPrefix/EntityName"
    ["DoonvHelper/DashCodeGate"] = "MaggyHelper/DashCodeGate",
    -- Add more replacements as needed
    -- ["MaxHelpingHand/SomeEntity"] = "MaggyHelper/SomeEntity",
}

script.parameters = {
    dryRun = true,
    oldPrefix = "DoonvHelper",
    newPrefix = "MaggyHelper",
    specificEntity = "DashCodeGate",  -- Leave empty to replace all from oldPrefix
}

script.fieldInformation = {
    dryRun = {
        fieldType = "boolean",
        description = "Preview changes without applying"
    },
    oldPrefix = {
        fieldType = "string",
        description = "External mod prefix to replace (e.g., DoonvHelper)"
    },
    newPrefix = {
        fieldType = "string",
        description = "Your mod prefix (e.g., MaggyHelper)"
    },
    specificEntity = {
        fieldType = "string",
        description = "Specific entity name to replace (empty = all entities from oldPrefix)"
    },
}

script.fieldOrder = {"dryRun", "oldPrefix", "newPrefix", "specificEntity"}

function script.run(args)
    local map = state.map
    if not map then
        print("[ERROR] No map loaded!")
        return
    end
    
    -- Build the search pattern
    local searchPattern
    local replacementPattern
    
    if args.specificEntity and args.specificEntity ~= "" then
        searchPattern = args.oldPrefix .. "/" .. args.specificEntity
        replacementPattern = args.newPrefix .. "/" .. args.specificEntity
    else
        searchPattern = args.oldPrefix .. "/"
        replacementPattern = args.newPrefix .. "/"
    end
    
    print(string.format("\n=== %s ===", args.dryRun and "DRY RUN (Preview)" or "LIVE REPLACEMENT"))
    print(string.format("Replacing: '%s' -> '%s'", searchPattern, replacementPattern))
    
    if not args.dryRun then
        print("Creating history snapshot for undo...")
        history.addSnapshot()
    end
    
    local totalReplaced = 0
    local replacements = {}
    
    -- Scan all rooms
    for roomIdx, roomData in ipairs(map.rooms or {}) do
        local roomName = roomData.name or ("Room_" .. roomIdx)
        
        -- Process entities
        if roomData.entities then
            for _, entity in ipairs(roomData.entities) do
                local name = entity._name or entity.name
                if name then
                    local newName = name:gsub("^" .. searchPattern, replacementPattern)
                    if newName ~= name then
                        if not args.dryRun then
                            entity._name = newName
                            entity.name = newName
                        end
                        totalReplaced = totalReplaced + 1
                        addReplacement(replacements, name, newName, roomName)
                    end
                end
            end
        end
        
        -- Process triggers
        if roomData.triggers then
            for _, trigger in ipairs(roomData.triggers) do
                local name = trigger._name or trigger.name
                if name then
                    local newName = name:gsub("^" .. searchPattern, replacementPattern)
                    if newName ~= name then
                        if not args.dryRun then
                            trigger._name = newName
                            trigger.name = newName
                        end
                        totalReplaced = totalReplaced + 1
                        addReplacement(replacements, name, newName, roomName)
                    end
                end
            end
        end
    end
    
    -- Print report
    print("\n=== REPLACEMENT REPORT ===")
    
    if next(replacements) == nil then
        print("  (no matching entities found)")
    else
        for oldName, data in pairs(replacements) do
            print(string.format("  %s -> %s (count: %d)", oldName, data.newName, #data.rooms))
            for _, roomName in ipairs(data.rooms) do
                print(string.format("    - %s", roomName))
            end
        end
    end
    
    print(string.format("\n=== SUMMARY ==="))
    print(string.format("Total items %s: %d", args.dryRun and "that would be replaced" or "replaced", totalReplaced))
    
    if args.dryRun then
        print("\n[DRY RUN COMPLETE]")
        print("Set 'dryRun' to false to actually apply replacements.")
    else
        print("\n[LIVE REPLACEMENT COMPLETE]")
        print("Entity names have been updated.")
        print("Use Ctrl+Z in Loenn to undo if needed.")
        print("Remember to save the map (Ctrl+S) to persist changes.")
        print("\n⚠️  IMPORTANT: Make sure you have:")
        print("  1. Ported the entity class to your mod (C# code)")
        print("  2. Copied any required graphics to your mod's Graphics folder")
        print("  3. Tested the entity works correctly before committing changes")
    end
end

function addReplacement(list, oldName, newName, roomName)
    if not list[oldName] then
        list[oldName] = {newName = newName, rooms = {}}
    end
    table.insert(list[oldName].rooms, roomName)
end

return script
