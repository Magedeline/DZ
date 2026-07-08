-- Lönn Script: PCG AI Generation Request Builder
-- Builds a generation request JSON file that can be sent to the gamelab PCG AI
-- via the gamelab-mcp MCP server through Windsurf/Cascade
-- Usage: Run from Lönn console to export current map context for AI generation

local state = require("loaded_state")
local utils = require("utils")

local PCGRequest = {}

PCGRequest.name = "pcgAiRequest"
PCGRequest.displayName = "PCG AI: Build Generation Request (v5)"
PCGRequest.tooltip = "Exports current map context as a generation request for the gamelab PCG AI (v5).\nSaves to Loenn/pcg/output/generation_request.json\nSupports decals, enhanced entity catalog, and AI-powered analysis."

PCGRequest.parameters = {
    outputFile = "Loenn/pcg/output/generation_request.json",
    preset = "platforming_room",
    roomCount = 3,
    roomWidth = 320,
    roomHeight = 184,
    theme = "summit",
    difficulty = "normal",
    includeExistingContext = true,
    customPrompt = "",
}

PCGRequest.fieldInformation = {
    outputFile = {
        fieldType = "string",
        description = "Output path for the generation request JSON"
    },
    preset = {
        fieldType = "string",
        description = "Generation preset: platforming_room, kirby_combat_room, puzzle_room, boss_arena, narrative_room, tower_climb, speedrun_room"
    },
    roomCount = {
        fieldType = "integer",
        description = "Number of rooms to generate"
    },
    roomWidth = {
        fieldType = "integer",
        description = "Width of each generated room (in pixels)"
    },
    roomHeight = {
        fieldType = "integer",
        description = "Height of each generated room (in pixels)"
    },
    theme = {
        fieldType = "string",
        description = "Visual theme: summit, ruins, tower, digital, void, inferno, resort, snow"
    },
    difficulty = {
        fieldType = "string",
        description = "Difficulty: easy, normal, hard, expert"
    },
    includeExistingContext = {
        fieldType = "boolean",
        description = "Include current map rooms as context for the AI"
    },
    customPrompt = {
        fieldType = "string",
        description = "Additional prompt instructions for the AI"
    },
}

PCGRequest.fieldOrder = {"outputFile", "preset", "roomCount", "roomWidth", "roomHeight", "theme", "difficulty", "includeExistingContext", "customPrompt"}

local function serializeRoom(room)
    local r = {
        name = room.name,
        x = room.x,
        y = room.y,
        width = room.width,
        height = room.height,
        entities = {},
        triggers = {},
        decalsFg = {},
        decalsBg = {},
        music = room.music or "",
        ambience = room.ambience or "",
        color = room.color or 0,
        windPattern = room.windPattern or "None",
        dark = room.dark or false,
        space = room.space or false,
    }

    for _, ent in ipairs(room.entities or {}) do
        local e = {
            name = ent._name or ent.name or "unknown",
            x = ent.x,
            y = ent.y,
            width = ent.width or 8,
            height = ent.height or 8,
        }
        for k, v in pairs(ent) do
            if k ~= "_name" and k ~= "name" and k ~= "x" and k ~= "y" and k ~= "width" and k ~= "height" then
                e[k] = v
            end
        end
        table.insert(r.entities, e)
    end

    for _, trig in ipairs(room.triggers or {}) do
        local t = {
            name = trig._name or trig.name or "unknown",
            x = trig.x,
            y = trig.y,
            width = trig.width or 16,
            height = trig.height or 16,
        }
        for k, v in pairs(trig) do
            if k ~= "_name" and k ~= "name" and k ~= "x" and k ~= "y" and k ~= "width" and k ~= "height" then
                t[k] = v
            end
        end
        table.insert(r.triggers, t)
    end

    for _, decal in ipairs(room.decalsFg or {}) do
        local d = {
            texture = decal.texture or "",
            x = decal.x or 0,
            y = decal.y or 0,
            scaleX = decal.scaleX or 1,
            scaleY = decal.scaleY or 1,
        }
        table.insert(r.decalsFg, d)
    end

    for _, decal in ipairs(room.decalsBg or {}) do
        local d = {
            texture = decal.texture or "",
            x = decal.x or 0,
            y = decal.y or 0,
            scaleX = decal.scaleX or 1,
            scaleY = decal.scaleY or 1,
        }
        table.insert(r.decalsBg, d)
    end

    return r
end

local function simpleJSONEncode(tbl, indent)
    indent = indent or 0
    local pad = string.rep("  ", indent)
    local pad2 = string.rep("  ", indent + 1)

    if type(tbl) == "string" then
        return '"' .. tbl:gsub('"', '\\"'):gsub('\n', '\\n') .. '"'
    elseif type(tbl) == "number" then
        return tostring(tbl)
    elseif type(tbl) == "boolean" then
        return tbl and "true" or "false"
    elseif type(tbl) == "table" then
        -- Check if array
        local isArray = false
        local count = 0
        for k, _ in pairs(tbl) do
            count = count + 1
            if type(k) == "number" then isArray = true end
        end

        if isArray and count > 0 then
            local items = {}
            for _, v in ipairs(tbl) do
                table.insert(items, simpleJSONEncode(v, indent + 1))
            end
            if #items == 0 then return "[]" end
            return "[\n" .. pad2 .. table.concat(items, ",\n" .. pad2) .. "\n" .. pad .. "]"
        else
            local items = {}
            for k, v in pairs(tbl) do
                table.insert(items, '"' .. tostring(k) .. '": ' .. simpleJSONEncode(v, indent + 1))
            end
            if #items == 0 then return "{}" end
            table.sort(items)
            return "{\n" .. pad2 .. table.concat(items, ",\n" .. pad2) .. "\n" .. pad .. "}"
        end
    end
    return "null"
end

function PCGRequest.run(args)
    args = args or PCGRequest.parameters

    local map = state.map

    -- Build the request
    local request = {
        server = "gamelab-mcp",
        mod = "DZ",
        version = "4.0.0",
        generation = {
            preset = args.preset,
            roomCount = args.roomCount,
            roomWidth = args.roomWidth,
            roomHeight = args.roomHeight,
            theme = args.theme,
            difficulty = args.difficulty,
            customPrompt = args.customPrompt,
        },
        entityCatalog = "Loenn/pcg/entity_catalog.json",
        config = "Loenn/pcg/pcg_config.json",
        context = {
            existingRooms = {},
            mapName = "",
            totalRooms = 0,
        },
        output = {
            format = "json",
            path = "Loenn/pcg/output/generated_map.json",
        }
    }

    -- Include existing map context
    if args.includeExistingContext and map then
        request.context.mapName = map.name or "unknown"
        request.context.totalRooms = #(map.rooms or {})

        for _, room in ipairs(map.rooms or {}) do
            table.insert(request.context.existingRooms, serializeRoom(room))
        end

        print(string.format("[INFO] Included %d existing rooms as context", request.context.totalRooms))
    else
        print("[INFO] No existing context included")
    end

    -- Serialize to JSON
    local jsonStr = simpleJSONEncode(request)

    -- Write to file
    local file = io.open(args.outputFile, "w")
    if not file then
        print(string.format("[ERROR] Could not write to: %s", args.outputFile))
        print("[INFO] Make sure the output directory exists (Loenn/pcg/output/)")
        return
    end
    file:write(jsonStr)
    file:close()

    print(string.format("\n=== PCG GENERATION REQUEST BUILT ==="))
    print(string.format("Output: %s", args.outputFile))
    print(string.format("Preset: %s", args.preset))
    print(string.format("Rooms to generate: %d", args.roomCount))
    print(string.format("Room size: %dx%d", args.roomWidth, args.roomHeight))
    print(string.format("Theme: %s, Difficulty: %s", args.theme, args.difficulty))
    if args.customPrompt and args.customPrompt ~= "" then
        print(string.format("Custom prompt: %s", args.customPrompt))
    end
    print(string.format("Context rooms: %d", request.context.totalRooms))
    print(string.format("\n[NEXT STEPS]"))
    print("  1. In Windsurf/Cascade, ask the AI to generate a map using this request")
    print("  2. The AI will use the gamelab-mcp server to generate the map")
    print("  3. The generated map will be saved to Loenn/pcg/output/generated_map.json")
    print("  4. Run the 'PCG AI: Import Generated Map' script in Lönn to import it")
end

return PCGRequest
