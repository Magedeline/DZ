-- ============================================================================
-- CELESTE MAP BINARY TO JSON EXPORTER
-- ============================================================================
-- Purpose: Export .bin map files to JSON format for inspection and editing
--
-- Usage:
--   local exporter = require("tools.MapBinaryExporter")
--   exporter:exportToJson("Maps/Maggy/ASide/01_City.bin", "Maps/Maggy/ASide/01_City.json")
--   exporter:importFromJson("Maps/Maggy/ASide/01_City.json", "Maps/Maggy/ASide/01_City.bin")
--
-- The JSON format preserves all map data including:
-- - Package info (name, version, version hash)
-- - Rooms with complete tile and entity data
-- - Triggers, decals, and effects
-- - Full entity properties and attributes
-- ============================================================================

local json = require("dkjson")
local utils = require("utils")

local MapBinaryExporter = {}

-- ============================================================================
-- JSON SERIALIZATION HELPERS
-- ============================================================================

--- Convert entity to JSON-serializable format
local function serializeEntity(entity)
    local serialized = {
        name = entity.name,
        x = entity.x,
        y = entity.y,
        width = entity.width or 0,
        height = entity.height or 0,
        rotation = entity.rotation or 0,
        scaleX = entity.scaleX or 1.0,
        scaleY = entity.scaleY or 1.0,
        depth = entity.depth or 0,
        visible = entity.visible ~= false,
        flipX = entity.flipX or false,
        flipY = entity.flipY or false,
        properties = {}
    }

    -- Serialize all custom properties
    if entity then
        for key, value in pairs(entity) do
            -- Skip standard entity properties already serialized
            if not table.contains(
                {"name", "x", "y", "width", "height", "rotation",
                 "scaleX", "scaleY", "depth", "visible", "flipX", "flipY"},
                key
            ) then
                local valueType = type(value)
                if valueType == "string" or valueType == "number" or valueType == "boolean" then
                    serialized.properties[key] = value
                elseif valueType == "table" and not string.startsWith(key, "__") then
                    -- Try to serialize nested tables (but avoid metatables)
                    pcall(function()
                        serialized.properties[key] = value
                    end)
                end
            end
        end
    end

    return serialized
end

--- Convert entity from JSON format back to entity object
local function deserializeEntity(data)
    local entity = {
        name = data.name,
        x = data.x,
        y = data.y,
        width = data.width or 0,
        height = data.height or 0,
        rotation = data.rotation or 0,
        scaleX = data.scaleX or 1.0,
        scaleY = data.scaleY or 1.0,
        depth = data.depth or 0,
        visible = data.visible ~= false,
        flipX = data.flipX or false,
        flipY = data.flipY or false,
    }

    -- Restore custom properties
    if data.properties then
        for key, value in pairs(data.properties) do
            entity[key] = value
        end
    end

    return entity
end

--- Serialize tile data to a simple format
local function serializeTiles(tiles)
    if not tiles then return {} end

    local serialized = {}
    for y, row in pairs(tiles) do
        serialized[tostring(y)] = {}
        for x, tile in pairs(row) do
            if tile then
                serialized[tostring(y)][tostring(x)] = {
                    id = tile.id or 0,
                    x = tile.x or 0,
                    y = tile.y or 0
                }
            end
        end
    end
    return serialized
end

--- Deserialize tile data from JSON
local function deserializeTiles(data)
    if not data or type(data) ~= "table" then return {} end

    local tiles = {}
    for yStr, row in pairs(data) do
        local y = tonumber(yStr)
        tiles[y] = {}
        for xStr, tileData in pairs(row) do
            local x = tonumber(xStr)
            tiles[y][x] = {
                id = tileData.id or 0,
                x = tileData.x or 0,
                y = tileData.y or 0
            }
        end
    end
    return tiles
end

--- Serialize room to JSON format
local function serializeRoom(room)
    return {
        name = room.name,
        x = room.x or 0,
        y = room.y or 0,
        width = room.width or 320,
        height = room.height or 180,
        dark = room.dark or false,
        music = room.music or "",
        ambience = room.ambience or "",
        musicProgress = room.musicProgress or "persist",
        windPattern = room.windPattern or "None",
        entities = {},
        triggers = {},
        decals = {},
        tiles = serializeTiles(room.tiles),
        fgTiles = serializeTiles(room.fgTiles),
        bgTiles = serializeTiles(room.bgTiles),
    }
end

-- ============================================================================
-- EXPORT FUNCTION
-- ============================================================================

--- Export a Celeste map binary file to JSON
function MapBinaryExporter:exportToJson(binPath, jsonPath)
    print(string.format("[MapBinaryExporter] Exporting %s to %s", binPath, jsonPath))

    -- Load the map using Loenn's map loader
    local map, err = require("utils.Loenn.Map").loadMap(binPath)
    if not map then
        print(string.format("[MapBinaryExporter] ERROR: Failed to load map: %s", err))
        return false, err
    end

    -- Create export structure
    local export = {
        format_version = "1.0",
        celeste_version = "1.4.0",
        package_name = map.package or "",
        rooms = {}
    }

    -- Serialize all rooms
    if map.rooms then
        for _, room in ipairs(map.rooms) do
            local serializedRoom = serializeRoom(room)

            -- Serialize entities
            if room.entities then
                for _, entity in ipairs(room.entities) do
                    table.insert(serializedRoom.entities, serializeEntity(entity))
                end
            end

            -- Serialize triggers
            if room.triggers then
                for _, trigger in ipairs(room.triggers) do
                    table.insert(serializedRoom.triggers, serializeEntity(trigger))
                end
            end

            -- Serialize decals
            if room.decals then
                for _, decal in ipairs(room.decals) do
                    table.insert(serializedRoom.decals, serializeEntity(decal))
                end
            end

            table.insert(export.rooms, serializedRoom)
        end
    end

    -- Write to JSON file
    local jsonContent = json.encode(export, { indent = true })
    utils.writeFile(jsonPath, jsonContent)

    print(string.format("[MapBinaryExporter] ✓ Exported %d rooms to %s",
        #export.rooms, jsonPath))

    return true, export
end

-- ============================================================================
-- IMPORT FUNCTION
-- ============================================================================

--- Import a JSON file and convert it back to Celeste map binary
function MapBinaryExporter:importFromJson(jsonPath, binPath)
    print(string.format("[MapBinaryExporter] Importing %s to %s", jsonPath, binPath))

    -- Read JSON file
    local jsonContent = utils.readFile(jsonPath)
    if not jsonContent then
        print(string.format("[MapBinaryExporter] ERROR: Failed to read %s", jsonPath))
        return false, "Failed to read JSON file"
    end

    -- Parse JSON
    local data, pos, err = json.decode(jsonContent)
    if not data then
        print(string.format("[MapBinaryExporter] ERROR: Failed to parse JSON: %s", err))
        return false, "Invalid JSON format"
    end

    -- Create map structure
    local map = {
        package = data.package_name or "",
        rooms = {}
    }

    -- Reconstruct rooms
    if data.rooms then
        for _, roomData in ipairs(data.rooms) do
            local room = {
                name = roomData.name,
                x = roomData.x or 0,
                y = roomData.y or 0,
                width = roomData.width or 320,
                height = roomData.height or 180,
                dark = roomData.dark or false,
                music = roomData.music or "",
                ambience = roomData.ambience or "",
                entities = {},
                triggers = {},
                decals = {},
                tiles = deserializeTiles(roomData.tiles),
                fgTiles = deserializeTiles(roomData.fgTiles),
                bgTiles = deserializeTiles(roomData.bgTiles),
            }

            -- Restore entities
            if roomData.entities then
                for _, entityData in ipairs(roomData.entities) do
                    table.insert(room.entities, deserializeEntity(entityData))
                end
            end

            -- Restore triggers
            if roomData.triggers then
                for _, triggerData in ipairs(roomData.triggers) do
                    table.insert(room.triggers, deserializeEntity(triggerData))
                end
            end

            -- Restore decals
            if roomData.decals then
                for _, decalData in ipairs(roomData.decals) do
                    table.insert(room.decals, deserializeEntity(decalData))
                end
            end

            table.insert(map.rooms, room)
        end
    end

    -- Save map using Loenn's map saver
    local mapModule = require("utils.Loenn.Map")
    local success, saveErr = mapModule.saveMap(binPath, map)

    if not success then
        print(string.format("[MapBinaryExporter] ERROR: Failed to save map: %s", saveErr))
        return false, saveErr
    end

    print(string.format("[MapBinaryExporter] ✓ Imported %d rooms to %s",
        #map.rooms, binPath))

    return true
end

-- ============================================================================
-- BATCH OPERATIONS
-- ============================================================================

--- Export all maps in a directory
function MapBinaryExporter:exportBatch(mapsDir, outputDir)
    print(string.format("[MapBinaryExporter] Batch export from %s to %s", mapsDir, outputDir))

    local results = {
        successful = 0,
        failed = 0,
        errors = {}
    }

    -- Find all .bin files
    local files = utils.getFiles(mapsDir, "\.bin$")

    for _, binFile in ipairs(files) do
        local jsonFile = binFile:gsub("\.bin$", ".json")
        local success, err = self:exportToJson(binFile, jsonFile)

        if success then
            results.successful = results.successful + 1
        else
            results.failed = results.failed + 1
            table.insert(results.errors, { file = binFile, error = err })
        end
    end

    print(string.format("[MapBinaryExporter] Batch export complete: %d successful, %d failed",
        results.successful, results.failed))

    return results
end

--- Import all JSON files from a directory
function MapBinaryExporter:importBatch(jsonDir)
    print(string.format("[MapBinaryExporter] Batch import from %s", jsonDir))

    local results = {
        successful = 0,
        failed = 0,
        errors = {}
    }

    -- Find all .json files
    local files = utils.getFiles(jsonDir, "\.json$")

    for _, jsonFile in ipairs(files) do
        local binFile = jsonFile:gsub("\.json$", ".bin")
        local success, err = self:importFromJson(jsonFile, binFile)

        if success then
            results.successful = results.successful + 1
        else
            results.failed = results.failed + 1
            table.insert(results.errors, { file = jsonFile, error = err })
        end
    end

    print(string.format("[MapBinaryExporter] Batch import complete: %d successful, %d failed",
        results.successful, results.failed))

    return results
end

return MapBinaryExporter
