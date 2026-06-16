-- ============================================================================
-- DZ MAP UPDATER LOENN PLUGIN
-- ============================================================================
-- Safe map batch update using Loenn's native binary serialization
--
-- Usage:
--   1. Open Loenn
--   2. This plugin auto-loads
--   3. Use: Debug → DZ Map Updater (from menu)
--   4. Or run console command: DZ_update_maps()
-- ============================================================================

local utils = require("utils")
local filesystem = require("utils.filesystem")
local state = require("loaded_state")

local MapUpdater = {}

-- Configuration
MapUpdater.mapsRoot = "Maps/Maggy"
MapUpdater.sides = {"ASide", "BSide", "CSide", "DSide"}
MapUpdater.chaptersASide = {
    "00_Prologue", "01_City", "02_Nightmare", "03_Stars", "04_Legend",
    "05_Restore", "06_Stronghold", "07_Hell", "08_Truth", "09_Summit",
    "10_Ruins", "11_Snow", "12_Water", "13_Fire", "14_Digital",
    "15_Castle", "16_Corruption", "17_Epilogue", "18_Heart", "19_Space",
    "20_TheEnd", "21_LastLevel"
}
MapUpdater.chaptersBSideDSide = {
    "01_City", "02_Nightmare", "03_Stars", "04_Legend",
    "05_Restore", "06_Stronghold", "07_Hell", "08_Truth", "09_Summit",
    "10_Ruins", "11_Snow", "12_Water", "13_Fire", "14_Digital",
    "15_Castle", "18_Heart"
}

-- Statistics
MapUpdater.stats = {
    totalMaps = 0,
    mapsProcessed = 0,
    mapsUpdated = 0,
    errors = {},
    warnings = {}
}

-- ============================================================================
-- CORE FUNCTIONS
-- ============================================================================

function MapUpdater.findMaps()
    print("[MapUpdater] Scanning for maps...")

    local maps = {}

    for _, side in ipairs(MapUpdater.sides) do
        local chapters = (side == "ASide") and MapUpdater.chaptersASide or MapUpdater.chaptersBSideDSide

        for _, chapter in ipairs(chapters) do
            local mapPath = MapUpdater.mapsRoot .. "/" .. side .. "/" .. chapter .. ".bin"

            if filesystem.exists(mapPath) then
                table.insert(maps, mapPath)
                MapUpdater.stats.totalMaps = MapUpdater.stats.totalMaps + 1
            else
                table.insert(MapUpdater.stats.warnings, "Map not found: " .. mapPath)
            end
        end
    end

    print(string.format("[MapUpdater] Found %d maps", MapUpdater.stats.totalMaps))
    return maps
end

function MapUpdater.updateMapFile(mapPath)
    print(string.format("[MapUpdater] Processing: %s", mapPath))

    try {
        function()
            -- Load the map using Loenn's native loader
            local map, err = require("entities.map").loadMap(mapPath)

            if not map or err then
                table.insert(MapUpdater.stats.errors,
                    string.format("Failed to load %s: %s", mapPath, err or "Unknown error"))
                return false
            end

            -- Validate and update entities
            local updated = false

            if map.entities then
                for _, entity in ipairs(map.entities) do
                    -- Update entity type names if they start with CH (old format)
                    if entity._name and string.match(entity._name, "^CH%d+_") then
                        entity._name = string.gsub(entity._name, "^CH(%d+_)", "DZ_CH%1")
                        updated = true
                    end

                    -- Update dialog key properties
                    if entity.dialogKey and string.match(entity.dialogKey, "^CH%d+_") then
                        entity.dialogKey = string.gsub(entity.dialogKey, "^CH(%d+_)", "DZ_CH%1")
                        updated = true
                    end
                end
            end

            -- Update triggers similarly
            if map.triggers then
                for _, trigger in ipairs(map.triggers) do
                    if trigger._name and string.match(trigger._name, "^CH%d+_") then
                        trigger._name = string.gsub(trigger._name, "^CH(%d+_)", "DZ_CH%1")
                        updated = true
                    end

                    if trigger.dialogKey and string.match(trigger.dialogKey, "^CH%d+_") then
                        trigger.dialogKey = string.gsub(trigger.dialogKey, "^CH(%d+_)", "DZ_CH%1")
                        updated = true
                    end
                end
            end

            -- Save the map if updates were made
            if updated then
                require("entities.map").saveMap(mapPath, map)
                MapUpdater.stats.mapsUpdated = MapUpdater.stats.mapsUpdated + 1
                print(string.format("[MapUpdater] ✓ Updated: %s", mapPath))
                return true
            else
                print(string.format("[MapUpdater] - No changes: %s", mapPath))
                return false
            end
        end,

        catch {
            function(err)
                table.insert(MapUpdater.stats.errors,
                    string.format("Error processing %s: %s", mapPath, tostring(err)))
                print(string.format("[MapUpdater] ✗ Error: %s", mapPath))
                return false
            end
        }
    }

    MapUpdater.stats.mapsProcessed = MapUpdater.stats.mapsProcessed + 1
end

function MapUpdater.processMaps(maps)
    print(string.format("[MapUpdater] Processing %d maps...", #maps))

    for idx, mapPath in ipairs(maps) do
        MapUpdater.updateMapFile(mapPath)

        -- Progress indicator
        if idx % 10 == 0 then
            print(string.format("[MapUpdater] Progress: %d/%d", idx, #maps))
        end
    end
end

function MapUpdater.printReport()
    print("")
    print("================================================================================")
    print("DZ MAP UPDATE REPORT")
    print("================================================================================")
    print("")
    print(string.format("Total Maps Found: %d", MapUpdater.stats.totalMaps))
    print(string.format("Maps Processed: %d", MapUpdater.stats.mapsProcessed))
    print(string.format("Maps Updated: %d", MapUpdater.stats.mapsUpdated))
    print(string.format("Warnings: %d", #MapUpdater.stats.warnings))
    print(string.format("Errors: %d", #MapUpdater.stats.errors))
    print("")

    if #MapUpdater.stats.warnings > 0 then
        print("⚠ Warnings:")
        for _, warning in ipairs(MapUpdater.stats.warnings) do
            print("  - " .. warning)
        end
        print("")
    end

    if #MapUpdater.stats.errors > 0 then
        print("✗ Errors:")
        for _, error in ipairs(MapUpdater.stats.errors) do
            print("  - " .. error)
        end
        print("")
    end

    print("================================================================================")
end

function MapUpdater.run()
    print("")
    print("================================================================================")
    print("DZ MAP UPDATER - LOENN EDITION")
    print("================================================================================")
    print("")

    local maps = MapUpdater.findMaps()

    if #maps == 0 then
        print("[ERROR] No maps found!")
        return false
    end

    MapUpdater.processMaps(maps)
    MapUpdater.printReport()

    return #MapUpdater.stats.errors == 0
end

-- ============================================================================
-- LOENN INTEGRATION
-- ============================================================================

-- Global function that can be called from Loenn console
function DZ_update_maps()
    return MapUpdater.run()
end

-- Register as a Loenn command/tool
if mods then
    mods.afterLoadedEntity = mods.afterLoadedEntity or {}
    table.insert(mods.afterLoadedEntity, function()
        print("[MapUpdater] DZ Map Updater loaded successfully!")
        print("[MapUpdater] Run 'DZ_update_maps()' in console to start")
    end)
end

return MapUpdater
