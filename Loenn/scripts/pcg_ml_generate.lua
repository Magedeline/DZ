-- PCG ML-Guided Generate Room Script for LoennScripts
-- Uses the ML.NET K-Means cluster model (exported by the C# PCGGenerator)
-- to automatically pick a preset and difficulty that matches the map's
-- existing room distribution — then generates new room content using Markov Chains.
--
-- Prerequisites:
--   1. Run the Celeste game at least once with gameplay data → game writes
--      MLModels/pcg_centroids.json next to Celeste.exe.
--   2. Open the target map in Lönn.
--   3. Run this script on the room you want to regenerate.
--
-- Falls back to the manual preset/difficulty when centroids are unavailable.

local state       = require("loaded_state")
local utils       = require("utils")
local snapshot    = require("structs.snapshot")
local tilesStruct = require("structs.tiles")
local celesteRender = require("celeste_render")
local matrix      = require("utils.matrix")
local mods        = require("mods")

local pcg         = mods.requireFromPlugin("libraries.pcg.init")
local seedUtils   = mods.requireFromPlugin("libraries.pcg.seed_utils")

local script = {}

script.name        = "pcgMlGenerateRoom"
script.displayName = "PCG: ML-Guided Generate Room"
script.tooltip     = table.concat({
    "Generates room content guided by the ML.NET K-Means difficulty model.",
    "The model reads MLModels/pcg_centroids.json (written by the game).",
    "Falls back to the chosen manual preset when centroids are unavailable.",
    "Train first: run 'PCG: Train Model', then this script.",
}, "\n")

script.parameters = {
    -- ML guidance
    useMLModel       = true,
    centroidsPath    = "",        -- leave blank for auto-detection
    -- Fallback preset (used when ML model not loaded)
    fallbackPreset   = "default",
    roomStyle        = "normal",
    -- Seed
    autoSeed         = true,
    seed             = 0,
    -- Overrides (applied on top of ML guidance; 0 = use ML value)
    entityDensityOverride = 0,
    difficultyOverride    = 0,
    -- Generation options
    playabilityCheck = true,
    cleanupPasses    = 2,
    maxRetries       = 10,
}

script.fieldOrder = {
    "useMLModel", "centroidsPath",
    "fallbackPreset", "roomStyle",
    "autoSeed", "seed",
    "entityDensityOverride", "difficultyOverride",
    "playabilityCheck", "cleanupPasses", "maxRetries",
}

script.fieldInformation = {
    fallbackPreset = {
        fieldType = "loennScripts.dropdown",
        options = {
            "default", "open", "tight", "fullRow",
            "resort", "temple", "reflection", "summit", "core",
            "wind", "ice", "cave", "ruins", "castle",
            "darkStars", "void", "nightmare", "farewell", "dream",
            "bspPlatformer", "bspSummit", "bspTemple", "bspResort",
        },
    },
    roomStyle = {
        fieldType = "loennScripts.dropdown",
        options = {
            "normal", "resort", "temple", "reflection", "summit", "core",
            "wind", "ice", "cave", "ruins", "castle",
            "darkStars", "void", "nightmare", "farewell", "dream",
        },
    },
    seed             = { fieldType = "integer" },
    cleanupPasses    = { fieldType = "integer" },
    maxRetries       = { fieldType = "integer" },
    entityDensityOverride = { fieldType = "number" },
    difficultyOverride    = { fieldType = "number" },
}

script.tooltips = {
    useMLModel            = "Use the ML.NET K-Means centroids to auto-select preset and difficulty.",
    centroidsPath         = "Explicit path to pcg_centroids.json. Leave blank to search default locations.",
    fallbackPreset        = "Preset used when the ML model is not loaded.",
    entityDensityOverride = "Override ML entity density (0 = use ML value).",
    difficultyOverride    = "Override ML difficulty [0,1] (0 = use ML value).",
}

--------------------------------------------------------------------------------
-- Helpers
--------------------------------------------------------------------------------

local function resolveExits(room)
    -- Simple heuristic: add exits where there are adjacent rooms in the map
    local exits = { left = true, right = true }
    if state and state.map and state.map.rooms then
        for _, other in ipairs(state.map.rooms) do
            if other ~= room then
                local dx = other.x - room.x
                local dy = other.y - room.y
                if math.abs(dy) < (room.height or 184) and math.abs(dx) < (other.width or 320) + (room.width or 320) then
                    if dx < 0 then exits.left  = true end
                    if dx > 0 then exits.right = true end
                    if dy < 0 then exits.top   = true end
                    if dy > 0 then exits.bottom = true end
                end
            end
        end
    end
    return exits
end

--------------------------------------------------------------------------------
-- Script entry point
--------------------------------------------------------------------------------

function script.prerun(args)
    -- Try to reload centroids if useMLModel is set
    if args.useMLModel then
        local path = (args.centroidsPath and args.centroidsPath ~= "") and args.centroidsPath or nil
        local ok, err = pcg.mlReloadCentroids(path)
        if ok then
            print("[PCG ML] Centroids loaded: " .. (pcg.mlBridge.loadedPath() or "?"))
            print(pcg.mlStatus())
        else
            print("[PCG ML] Centroids not available — " .. (err or "unknown error"))
            print("[PCG ML] Falling back to preset: " .. (args.fallbackPreset or "default"))
        end
    end
end

function script.run(args)
    -- ------------------------------------------------------------------ --
    -- Validate map + current room
    -- ------------------------------------------------------------------ --
    if not state.map then
        error("No map loaded in Lönn.")
    end

    local map = state.map
    local currentRoom = state.selectedRoom or (map.rooms and map.rooms[1])
    if not currentRoom then
        error("No room selected. Please select a room first.")
    end

    -- ------------------------------------------------------------------ --
    -- Train Markov model on map rooms
    -- ------------------------------------------------------------------ --
    local trainOk, trainStats = pcg.trainFromMap(args.fallbackPreset or "default")
    if not trainOk then
        print("[PCG ML] Markov training warning: " .. (trainStats and trainStats.error or "unknown"))
        -- Don't abort — BSP presets don't need training
    else
        print(string.format("[PCG ML] Markov trained: %d rooms, %d n-gram states",
            trainStats.roomCount or 0, trainStats.ngramCount or 0))
    end

    -- ------------------------------------------------------------------ --
    -- Discover ML guidance for *this* room
    -- ------------------------------------------------------------------ --
    local mlGuide = nil
    if args.useMLModel and pcg.mlBridge.isLoaded() then
        -- Extract feature vector from the current (source) room
        local features = pcg.mlBridge.extractFeatures(currentRoom)
        mlGuide = pcg.mlGuidePreset(features)

        if mlGuide then
            print(string.format(
                "[PCG ML] Room classified as '%s' (cluster %d) — preset=%s, density=%.2f, diff=%.2f, dist=%.4f",
                mlGuide.label, mlGuide.clusterId,
                mlGuide.preset, mlGuide.entityDensity, mlGuide.difficulty,
                mlGuide.distance or 0))
        end
    end

    -- ------------------------------------------------------------------ --
    -- Resolve final generation parameters
    -- ML values take priority; manual overrides trump both
    -- ------------------------------------------------------------------ --
    local resolvedPreset   = (mlGuide and mlGuide.preset)       or args.fallbackPreset or "default"
    local resolvedDensity  = (mlGuide and mlGuide.entityDensity) or 0.15
    local resolvedDifficulty = (mlGuide and mlGuide.difficulty)  or 0.5
    local resolvedLabel    = (mlGuide and mlGuide.label)         or "N/A (no ML model)"

    if args.entityDensityOverride and args.entityDensityOverride > 0 then
        resolvedDensity = args.entityDensityOverride
    end
    if args.difficultyOverride and args.difficultyOverride > 0 then
        resolvedDifficulty = args.difficultyOverride
    end

    print(string.format(
        "[PCG ML] Generating with preset=%s, density=%.2f, difficulty=%.2f (label: %s)",
        resolvedPreset, resolvedDensity, resolvedDifficulty, resolvedLabel))

    -- ------------------------------------------------------------------ --
    -- Seed
    -- ------------------------------------------------------------------ --
    local seed
    if args.autoSeed then
        seed = seedUtils and seedUtils.generate() or os.time()
    else
        seed = args.seed or os.time()
    end
    math.randomseed(seed)
    print("[PCG ML] Seed: " .. tostring(seed))

    -- ------------------------------------------------------------------ --
    -- Determine exits
    -- ------------------------------------------------------------------ --
    local exits = resolveExits(currentRoom)

    -- ------------------------------------------------------------------ --
    -- Build override option table for pcg.generateRoom
    -- ------------------------------------------------------------------ --
    local roomW  = math.floor((currentRoom.width  or 320) / 8)
    local roomH  = math.floor((currentRoom.height or 184) / 8)

    local genOpts = {
        preset           = resolvedPreset,
        entityDensity    = resolvedDensity,
        difficulty       = resolvedDifficulty,
        roomWidth        = roomW,
        roomHeight       = roomH,
        roomStyle        = args.roomStyle or "normal",
        playabilityCheck = args.playabilityCheck,
        cleanupPasses    = args.cleanupPasses,
        maxRetries       = args.maxRetries,
    }

    -- Inherit BSP flags from the resolved preset
    local presetCfg = pcg.getPreset(resolvedPreset) or {}
    if presetCfg.useBsp then
        genOpts.useBsp        = true
        genOpts.trimmedBorders = presetCfg.trimmedBorders
        genOpts.bspOpts       = presetCfg.bspOpts
    end

    -- ------------------------------------------------------------------ --
    -- Generate room content
    -- ------------------------------------------------------------------ --
    local result, genErr = pcg.generateRoom(exits, genOpts, matrix)
    if not result then
        error("[PCG ML] Generation failed: " .. (genErr or "unknown error"))
    end

    -- ------------------------------------------------------------------ --
    -- Apply to current room (snapshot for undo)
    -- ------------------------------------------------------------------ --
    local roomSnap = snapshot.roomSnapshot(map, currentRoom, "PCG ML Generate Room")

    -- Write foreground tiles
    local fgMatrix = result.matrix
    local fgW, fgH = fgMatrix:size()

    currentRoom.tilesFg = tilesStruct.decode(
        { data = fgMatrix, width = fgW, height = fgH },
        currentRoom.tilesFg and currentRoom.tilesFg.default or "0"
    )

    -- Clear existing entities and write PCG entities
    local kept = {}
    if currentRoom.entities then
        for _, entity in ipairs(currentRoom.entities) do
            local name = entity._name or entity.name or ""
            -- Preserve player spawn
            if name == "player" then
                table.insert(kept, entity)
            end
        end
    end

    for _, entity in ipairs(result.entities or {}) do
        table.insert(kept, entity)
    end
    currentRoom.entities = kept

    snapshot.sendRoomSnapshot(roomSnap)
    celesteRender.invalidateRoomCache(currentRoom)

    print(string.format(
        "[PCG ML] Done! %dx%d tiles, %d entities, attempt %d/%d, score %.3f",
        fgW, fgH,
        #(result.entities or {}),
        result.attempt or 1, args.maxRetries,
        result.score   or 0))

    if mlGuide then
        print(string.format("[PCG ML] ML cluster: %s (id=%d)", mlGuide.label, mlGuide.clusterId))
    else
        print("[PCG ML] ML model not used (centroids unavailable).")
    end
end

return script
