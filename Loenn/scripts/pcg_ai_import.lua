-- Lönn Script: PCG AI Map Importer
-- Imports AI-generated map data from the gamelab PCG AI into the current map
-- Usage: Open a map in Lönn, then run this script from the console
-- The AI generates a JSON file via the gamelab-mcp server, this script imports it

local state = require("loaded_state")
local history = require("history")
local utils = require("utils")

local PCGImport = {}

PCGImport.name = "pcgAiImport"
PCGImport.displayName = "PCG AI: Import Generated Map"
PCGImport.tooltip = "Imports AI-generated map data from the gamelab PCG AI server.\nExpects a JSON file at Loenn/pcg/output/generated_map.json"

PCGImport.parameters = {
    inputFile = "Loenn/pcg/output/generated_map.json",
    mode = "merge",          -- "merge" or "replace"
    targetRoom = "",         -- empty = import all rooms, or specify a room name
    offset_x = 0,
    offset_y = 0,
    dryRun = false,
}

PCGImport.fieldInformation = {
    inputFile = {
        fieldType = "string",
        description = "Path to the AI-generated map JSON file"
    },
    mode = {
        fieldType = "string",
        description = "merge = add to existing map, replace = clear and replace"
    },
    targetRoom = {
        fieldType = "string",
        description = "Import into a specific room (empty = create new rooms)"
    },
    offset_x = {
        fieldType = "integer",
        description = "X offset for imported entities/tiles"
    },
    offset_y = {
        fieldType = "integer",
        description = "Y offset for imported entities/tiles"
    },
    dryRun = {
        fieldType = "boolean",
        description = "Preview without applying changes"
    },
}

PCGImport.fieldOrder = {"inputFile", "mode", "targetRoom", "offset_x", "offset_y", "dryRun"}

local function deepCopy(tbl)
    if type(tbl) ~= "table" then return tbl end
    local copy = {}
    for k, v in pairs(tbl) do
        copy[k] = deepCopy(v)
    end
    return copy
end

local function loadJSON(filepath)
    local file = io.open(filepath, "r")
    if not file then
        print(string.format("[ERROR] Could not open file: %s", filepath))
        return nil
    end
    local content = file:read("*all")
    file:close()

    if not content or content == "" then
        print("[ERROR] File is empty")
        return nil
    end

    -- Lönn uses Lua-based JSON parsing; try multiple approaches
    local success, json = pcall(require, "json")
    if success and json then
        local ok, data = pcall(json.decode, content)
        if ok then return data end
    end

    -- Fallback: try dkjson
    local ok2, dkjson = pcall(require, "dkjson")
    if ok2 and dkjson then
        local data = dkjson.decode(content)
        if data then return data end
    end

    -- Fallback: manual parse for simple structures
    print("[WARN] No JSON library found, attempting manual parse...")
    return nil
end

local function applyOffset(entity, ox, oy)
    if entity.x then entity.x = entity.x + ox end
    if entity.y then entity.y = entity.y + oy end
    if entity.nodes then
        for _, node in ipairs(entity.nodes) do
            if node.x then node.x = node.x + ox end
            if node.y then node.y = node.y + oy end
        end
    end
end

local function importEntity(entityData, ox, oy)
    local entity = {
        _name = entityData.name or entityData._name or "unknown",
        name = entityData.name or entityData._name or "unknown",
        x = entityData.x or 0,
        y = entityData.y or 0,
        width = entityData.width or 8,
        height = entityData.height or 8,
        nodes = deepCopy(entityData.nodes or {}),
    }

    -- Copy all additional data fields
    for key, value in pairs(entityData) do
        if key ~= "name" and key ~= "_name" and key ~= "x" and key ~= "y"
           and key ~= "width" and key ~= "height" and key ~= "nodes" then
            entity[key] = deepCopy(value)
        end
    end

    applyOffset(entity, ox, oy)
    return entity
end

local function importTrigger(triggerData, ox, oy)
    local trigger = {
        _name = triggerData.name or triggerData._name or "unknown",
        name = triggerData.name or triggerData._name or "unknown",
        x = triggerData.x or 0,
        y = triggerData.y or 0,
        width = triggerData.width or 16,
        height = triggerData.height or 16,
        nodes = deepCopy(triggerData.nodes or {}),
    }

    for key, value in pairs(triggerData) do
        if key ~= "name" and key ~= "_name" and key ~= "x" and key ~= "y"
           and key ~= "width" and key ~= "height" and key ~= "nodes" then
            trigger[key] = deepCopy(value)
        end
    end

    applyOffset(trigger, ox, oy)
    return trigger
end

local function importTiles(tileData, ox, oy)
    if not tileData or type(tileData) ~= "table" then
        return {}
    end

    -- Tile data can be either:
    -- 1. Array of strings (rows of tile characters)
    -- 2. Array of arrays (grid of tile IDs)
    local tiles = {}
    for _, row in ipairs(tileData) do
        if type(row) == "string" then
            table.insert(tiles, row)
        elseif type(row) == "table" then
            local tileRow = {}
            for _, cell in ipairs(row) do
                table.insert(tileRow, tostring(cell))
            end
            table.insert(tiles, table.concat(tileRow))
        end
    end

    return tiles
end

local function importRoom(roomData, ox, oy)
    local room = {
        name = roomData.name or "pcg_room",
        x = (roomData.x or 0) + ox,
        y = (roomData.y or 0) + oy,
        width = roomData.width or 320,
        height = roomData.height or 184,
        entities = {},
        triggers = {},
        decalsFg = {},
        decalsBg = {},
        tilesFg = {},
        tilesBg = {},
        parallax = {},
        music = roomData.music or "",
        ambience = roomData.ambience or "",
        color = roomData.color or 0,
        disableDownTransition = roomData.disableDownTransition or false,
        windPattern = roomData.windPattern or "None",
    }

    -- Import entities
    if roomData.entities then
        for _, entData in ipairs(roomData.entities) do
            table.insert(room.entities, importEntity(entData, 0, 0))
        end
    end

    -- Import triggers
    if roomData.triggers then
        for _, trigData in ipairs(roomData.triggers) do
            table.insert(room.triggers, importTrigger(trigData, 0, 0))
        end
    end

    -- Import tiles
    room.tilesFg = importTiles(roomData.tilesFg)
    room.tilesBg = importTiles(roomData.tilesBg)

    -- Import decals
    if roomData.decalsFg then
        for _, decal in ipairs(roomData.decalsFg) do
            table.insert(room.decalsFg, deepCopy(decal))
        end
    end
    if roomData.decalsBg then
        for _, decal in ipairs(roomData.decalsBg) do
            table.insert(room.decalsBg, deepCopy(decal))
        end
    end

    -- Import parallax
    if roomData.parallax then
        for _, para in ipairs(roomData.parallax) do
            table.insert(room.parallax, deepCopy(para))
        end
    end

    return room
end

local function mergeIntoRoom(existingRoom, generatedRoom, ox, oy)
    local merged = 0

    -- Merge entities
    if generatedRoom.entities then
        for _, ent in ipairs(generatedRoom.entities) do
            local entity = importEntity(ent, ox, oy)
            table.insert(existingRoom.entities, entity)
            merged = merged + 1
        end
    end

    -- Merge triggers
    if generatedRoom.triggers then
        for _, trig in ipairs(generatedRoom.triggers) do
            local trigger = importTrigger(trig, ox, oy)
            table.insert(existingRoom.triggers, trigger)
            merged = merged + 1
        end
    end

    -- Merge tiles (append rows if room sizes match)
    if generatedRoom.tilesFg and #generatedRoom.tilesFg > 0 then
        if existingRoom.tilesFg and #existingRoom.tilesFg > 0 then
            -- Overwrite tiles where generated data exists
            for i, row in ipairs(generatedRoom.tilesFg) do
                if i <= #existingRoom.tilesFg then
                    existingRoom.tilesFg[i] = row
                else
                    table.insert(existingRoom.tilesFg, row)
                end
            end
        else
            existingRoom.tilesFg = importTiles(generatedRoom.tilesFg)
        end
        merged = merged + 1
    end

    if generatedRoom.tilesBg and #generatedRoom.tilesBg > 0 then
        if existingRoom.tilesBg and #existingRoom.tilesBg > 0 then
            for i, row in ipairs(generatedRoom.tilesBg) do
                if i <= #existingRoom.tilesBg then
                    existingRoom.tilesBg[i] = row
                else
                    table.insert(existingRoom.tilesBg, row)
                end
            end
        else
            existingRoom.tilesBg = importTiles(generatedRoom.tilesBg)
        end
        merged = merged + 1
    end

    -- Merge decals
    if generatedRoom.decalsFg then
        for _, decal in ipairs(generatedRoom.decalsFg) do
            local d = deepCopy(decal)
            if d.x then d.x = d.x + ox end
            if d.y then d.y = d.y + oy end
            table.insert(existingRoom.decalsFg, d)
            merged = merged + 1
        end
    end
    if generatedRoom.decalsBg then
        for _, decal in ipairs(generatedRoom.decalsBg) do
            local d = deepCopy(decal)
            if d.x then d.x = d.x + ox end
            if d.y then d.y = d.y + oy end
            table.insert(existingRoom.decalsBg, d)
            merged = merged + 1
        end
    end

    return merged
end

function PCGImport.run(args)
    args = args or PCGImport.parameters

    local map = state.map
    if not map then
        print("[ERROR] No map loaded! Open a map in Lönn first.")
        return
    end

    -- Load the generated JSON
    local data = loadJSON(args.inputFile)
    if not data then
        print(string.format("[ERROR] Failed to load PCG data from: %s", args.inputFile))
        print("[INFO] Make sure the gamelab PCG AI has generated a map file.")
        print("[INFO] Expected format: { rooms: [ { name, x, y, width, height, entities, triggers, tilesFg, tilesBg } ] }")
        return
    end

    local rooms = data.rooms or data
    if type(rooms) ~= "table" then
        print("[ERROR] Invalid PCG data format: expected 'rooms' array")
        return
    end

    -- Count what we'll import
    local totalEntities = 0
    local totalTriggers = 0
    local totalRooms = 0
    for _, roomData in ipairs(rooms) do
        totalRooms = totalRooms + 1
        totalEntities = totalEntities + #(roomData.entities or {})
        totalTriggers = totalTriggers + #(roomData.triggers or {})
    end

    print(string.format("\n=== PCG AI MAP IMPORT ==="))
    print(string.format("Source: %s", args.inputFile))
    print(string.format("Mode: %s", args.mode))
    print(string.format("Rooms to import: %d", totalRooms))
    print(string.format("Entities: %d, Triggers: %d", totalEntities, totalTriggers))
    if args.dryRun then
        print("[DRY RUN] No changes will be applied")
    end

    if args.dryRun then
        -- Just print what would happen
        for _, roomData in ipairs(rooms) do
            local roomName = roomData.name or "unnamed"
            local entCount = #(roomData.entities or {})
            local trigCount = #(roomData.triggers or {})
            print(string.format("  [Room] %s (%dx%d) - %d entities, %d triggers",
                roomName, roomData.width or 320, roomData.height or 184, entCount, trigCount))

            -- List entity types
            local entityTypes = {}
            for _, ent in ipairs(roomData.entities or {}) do
                local name = ent.name or ent._name or "unknown"
                entityTypes[name] = (entityTypes[name] or 0) + 1
            end
            for name, count in pairs(entityTypes) do
                print(string.format("    - %s x%d", name, count))
            end
        end
        print("\n[DRY RUN COMPLETE] Set dryRun=false to apply changes.")
        return
    end

    -- Create history snapshot for undo
    history.addSnapshot()

    -- Apply changes
    if args.mode == "replace" then
        print("[INFO] Replacing all rooms...")
        map.rooms = {}
    end

    if not map.rooms then
        map.rooms = {}
    end

    local importedRooms = 0
    local importedEntities = 0
    local importedTriggers = 0

    for _, roomData in ipairs(rooms) do
        if args.targetRoom and args.targetRoom ~= "" then
            -- Merge into specific existing room
            local targetFound = false
            for _, existingRoom in ipairs(map.rooms) do
                if existingRoom.name == args.targetRoom then
                    local merged = mergeIntoRoom(existingRoom, roomData, args.offset_x, args.offset_y)
                    importedEntities = importedEntities + merged
                    targetFound = true
                    print(string.format("[INFO] Merged %d items into room '%s'", merged, args.targetRoom))
                    break
                end
            end
            if not targetFound then
                print(string.format("[WARN] Target room '%s' not found, creating new room", args.targetRoom))
                local room = importRoom(roomData, args.offset_x, args.offset_y)
                room.name = args.targetRoom
                table.insert(map.rooms, room)
                importedRooms = importedRooms + 1
                importedEntities = importedEntities + #(room.entities or {})
                importedTriggers = importedTriggers + #(room.triggers or {})
            end
        else
            -- Import as new room (or update if name exists)
            local existingIdx = nil
            for i, existingRoom in ipairs(map.rooms) do
                if existingRoom.name == (roomData.name or "") then
                    existingIdx = i
                    break
                end
            end

            if existingIdx and args.mode == "merge" then
                -- Update existing room
                local merged = mergeIntoRoom(map.rooms[existingIdx], roomData, args.offset_x, args.offset_y)
                importedEntities = importedEntities + merged
                print(string.format("[INFO] Updated room '%s' (+%d items)", roomData.name or "unnamed", merged))
            else
                -- Create new room
                local room = importRoom(roomData, args.offset_x, args.offset_y)
                table.insert(map.rooms, room)
                importedRooms = importedRooms + 1
                importedEntities = importedEntities + #(room.entities or {})
                importedTriggers = importedTriggers + #(room.triggers or {})
                print(string.format("[INFO] Created room '%s' (%d entities, %d triggers)",
                    room.name, #(room.entities or {}), #(room.triggers or {})))
            end
        end
    end

    print(string.format("\n=== IMPORT COMPLETE ==="))
    print(string.format("  Rooms created: %d", importedRooms))
    print(string.format("  Entities imported: %d", importedEntities))
    print(string.format("  Triggers imported: %d", importedTriggers))
    print("\n[IMPORTANT] Remember to:")
    print("  1. Save the map (Ctrl+S)")
    print("  2. Review imported content in the editor")
    print("  3. Test in-game before committing")
    print("  4. Use Ctrl+Z to undo if needed")
end

return PCGImport
