-- DZ to DZ Entity Converter for Loenn
-- Converts all DZ/ entities and triggers to DZ/ across all rooms in the map
-- Run from Loenn: Map -> Run Script -> convert_DZ_to_dz

local state = require("loaded_state")
local history = require("history")

local script = {}

script.name = "convertDZToDZ"
script.displayName = "CONVERT: DZ -> DZ (All Rooms)"
script.tooltip = "Converts all DZ/ entities and triggers to DZ/ prefix.\nApplies to all rooms in the currently loaded map."

script.parameters = {
    dryRun = true,
}

script.fieldInformation = {
    dryRun = {
        fieldType = "boolean",
        description = "Preview changes without applying (set to false to apply)"
    },
}

script.fieldOrder = {"dryRun"}

function script.run(args)
    local map = state.map
    if not map then
        print("[ERROR] No map loaded!")
        return
    end

    local oldPrefix = "DZ/"
    local newPrefix = "DZ/"

    print(string.format("\n=== %s ===", args.dryRun and "DRY RUN (Preview)" or "LIVE CONVERSION"))
    print(string.format("Converting: '%s*' -> '%s*'", oldPrefix, newPrefix))
    print(string.format("Map: %s", map._type or "unknown"))

    if not args.dryRun then
        print("Creating history snapshot for undo...")
        history.addSnapshot()
    end

    local totalConverted = 0
    local conversions = {}

    for roomIdx, roomData in ipairs(map.rooms or {}) do
        local roomName = roomData.name or ("Room_" .. roomIdx)

        -- Process entities
        if roomData.entities then
            for _, entity in ipairs(roomData.entities) do
                local name = entity._name or entity.name
                if name and name:find("^" .. oldPrefix) then
                    local newName = name:gsub("^" .. oldPrefix, newPrefix)
                    if not args.dryRun then
                        entity._name = newName
                        if entity.name then
                            entity.name = newName
                        end
                    end
                    totalConverted = totalConverted + 1
                    addConversion(conversions, name, newName, roomName, "entity")
                end
            end
        end

        -- Process triggers
        if roomData.triggers then
            for _, trigger in ipairs(roomData.triggers) do
                local name = trigger._name or trigger.name
                if name and name:find("^" .. oldPrefix) then
                    local newName = name:gsub("^" .. oldPrefix, newPrefix)
                    if not args.dryRun then
                        trigger._name = newName
                        if trigger.name then
                            trigger.name = newName
                        end
                    end
                    totalConverted = totalConverted + 1
                    addConversion(conversions, name, newName, roomName, "trigger")
                end
            end
        end
    end

    -- Print report
    print("\n=== CONVERSION REPORT ===")

    if next(conversions) == nil then
        print("  (no DZ entities found in this map)")
    else
        for oldName, data in pairs(conversions) do
            print(string.format("  %s -> %s (%s, count: %d)", oldName, data.newName, data.kind, #data.rooms))
            for _, roomName in ipairs(data.rooms) do
                print(string.format("    - %s", roomName))
            end
        end
    end

    print(string.format("\n=== SUMMARY ==="))
    print(string.format("Total rooms scanned: %d", #(map.rooms or {})))
    print(string.format("Total items %s: %d", args.dryRun and "that would be converted" or "converted", totalConverted))

    if args.dryRun then
        print("\n[DRY RUN COMPLETE]")
        print("Set 'dryRun' to false to actually apply conversions.")
    else
        print("\n[LIVE CONVERSION COMPLETE]")
        print("All DZ entities/triggers have been renamed to DZ.")
        print("Use Ctrl+Z in Loenn to undo if needed.")
        print("Remember to save the map (Ctrl+S) to persist changes.")
        print("\nReminder: Make sure your DZ mod has the corresponding")
        print("entity/trigger classes registered for each converted name.")
    end
end

function addConversion(list, oldName, newName, roomName, kind)
    if not list[oldName] then
        list[oldName] = {newName = newName, kind = kind, rooms = {}}
    end
    table.insert(list[oldName].rooms, roomName)
end

return script
