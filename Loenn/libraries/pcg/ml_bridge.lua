--[[
================================================================================
  MaggyHelper PCG — ML Bridge (ml_bridge.lua)
================================================================================

  Reads cluster metadata exported by the C# PCG pipeline and provides room
  classification + preset guidance for the Loenn PCG pipeline.

  Supported data sources (tried in priority order):
    1. MLModels/pcg_onnx_meta.json   — written by PCGOnnxExporter.cs or
                                        Tools/pcg_train_onnx.py (preferred)
    2. MLModels/pcg_centroids.json   — legacy K-Means centroid export

  The ONNX model itself (pcg_model.onnx) is used directly by the C# game
  runtime (PCGOnnxInference.cs).  Lönn cannot execute ONNX graphs directly,
  so this bridge continues to use the centroid data embedded in the meta
  JSON for nearest-centroid classification.

  Public API:
    ml_bridge.load(jsonPath)             -- load / reload centroids or ONNX meta
    ml_bridge.loadOnnxMeta(jsonPath)     -- explicitly load pcg_onnx_meta.json
    ml_bridge.isLoaded()                 -- true when cluster data is ready
    ml_bridge.modelSource()              -- "onnx_meta" | "centroids" | nil
    ml_bridge.classify(features)         -- nearest centroid → cluster table
    ml_bridge.guidePreset(features)      -- → { preset, entityDensity, difficulty, label }
    ml_bridge.extractFeatures(room)      -- Loenn room → feature table
    ml_bridge.classifyRoom(room)         -- shorthand: extract + classify
    ml_bridge.summary()                  -- human-readable string for logging
================================================================================
]]

local mlBridge = {}

--------------------------------------------------------------------------------
-- Internal state
--------------------------------------------------------------------------------
local _centroids   = nil   -- array of cluster tables from parsed JSON
local _featureNorms= nil   -- { enemyMax, platformMax, gapMax }
local _loadedPath  = nil   -- path we successfully loaded from
local _modelSource = nil   -- "onnx_meta" | "centroids"

--------------------------------------------------------------------------------
-- Default search paths — ONNX meta preferred over legacy centroids
--------------------------------------------------------------------------------
local ONNX_META_SEARCH_PATHS = {
    "Mods/MaggyHelper/MLModels/pcg_onnx_meta.json",
    "../MLModels/pcg_onnx_meta.json",
    "MLModels/pcg_onnx_meta.json",
}

local DEFAULT_SEARCH_PATHS = {
    -- When the mod is installed next to Celeste.exe
    "Mods/MaggyHelper/MLModels/pcg_centroids.json",
    -- Fallback: one level up (dev workspace layout)
    "../MLModels/pcg_centroids.json",
    "MLModels/pcg_centroids.json",
}

--------------------------------------------------------------------------------
-- Minimal JSON field extractor
-- We only need to parse the deterministic output of PCGGenerator.ExportCentroids.
-- Rather than bundling a full JSON library, we use Lua patterns on the known schema.
--------------------------------------------------------------------------------

--- Try to require a JSON library; returns nil if unavailable.
local function tryRequireJson()
    local ok, lib = pcall(require, "json")
    if ok and lib and lib.decode then return lib end
    ok, lib = pcall(require, "dkjson")
    if ok and lib and lib.decode then return lib end
    return nil
end

--- Parse a number string (handles integers and decimals).
local function parseNum(s)
    return tonumber(s)
end

--- Extract all occurrences of a key→number pattern from a string.
-- Matches:  "key": value  (integer or float)
local function extractNum(text, key)
    local pattern = '"' .. key .. '":%s*([%-]?%d+%.?%d*[eE]?[+-]?%d*)'
    local v = text:match(pattern)
    return v and parseNum(v) or nil
end

--- Extract a key→string value.
local function extractStr(text, key)
    local pattern = '"' .. key .. '":%s*"([^"]*)"'
    return text:match(pattern)
end

--- Extract a JSON array of 4 numbers: [a,b,c,d]
local function extractArray4(text, key)
    local pattern = '"' .. key .. '":%s*%[([^%]]+)%]'
    local inner = text:match(pattern)
    if not inner then return nil end
    local nums = {}
    for n in inner:gmatch('[%-]?%d+%.?%d*[eE]?[+-]?%d*') do
        table.insert(nums, parseNum(n))
    end
    return #nums >= 4 and nums or nil
end

--- Parse the pcg_centroids.json text into Lua tables.
-- Returns: clusters (array), featureNorms (table),  or nil + errMsg.
local function parseJson(text)
    -- ---- Attempt proper JSON library first ----
    local jsonLib = tryRequireJson()
    if jsonLib then
        local ok, data = pcall(jsonLib.decode, text)
        if ok and data and data.clusters then
            return data.clusters, data.featureNorms
        end
    end

    -- ---- Manual pattern-based parser (fallback) ----
    -- Split file into per-cluster blocks by finding each "{...}" inside "clusters":[...]
    local clustersSection = text:match('"clusters"%s*:%s*%[(.-)%]%s*}%s*$')
    if not clustersSection then
        return nil, "Could not find 'clusters' array in JSON"
    end

    -- Split into individual cluster objects (each starts with '{', ends with '}')
    local clusters = {}
    -- We match each {...} block that contains at least an "id" field
    for block in clustersSection:gmatch('{([^}]+)}') do
        local c = {}
        c.id              = extractNum(block, "id")
        c.label           = extractStr(block, "label")
        c.suggestedPreset = extractStr(block, "suggestedPreset")
        c.entityDensity   = extractNum(block, "entityDensity")
        c.difficulty      = extractNum(block, "difficulty")
        c.centroid        = extractArray4(block, "centroid")

        if c.id and c.label and c.centroid then
            table.insert(clusters, c)
        end
    end

    if #clusters == 0 then
        return nil, "No valid cluster entries parsed from JSON"
    end

    -- Parse featureNorms
    local normsSection = text:match('"featureNorms"%s*:%s*({[^}]+})')
    local norms = {}
    if normsSection then
        norms.enemyMax    = extractNum(normsSection, "enemyMax")    or 1
        norms.platformMax = extractNum(normsSection, "platformMax") or 1
        norms.gapMax      = extractNum(normsSection, "gapMax")      or 1
    else
        norms.enemyMax    = 1
        norms.platformMax = 1
        norms.gapMax      = 1
    end

    return clusters, norms
end

--------------------------------------------------------------------------------
-- Public: load centroids from JSON file
--------------------------------------------------------------------------------

--- Load (or reload) cluster centroids from a pcg_centroids.json file.
-- @param jsonPath   Explicit path; when nil, tries ONNX meta paths first, then centroids.
-- @return boolean, string   success, errorMessage
function mlBridge.load(jsonPath)
    -- If an explicit path is given, determine its type and load accordingly
    if jsonPath then
        if jsonPath:find("onnx_meta") then
            return mlBridge.loadOnnxMeta(jsonPath)
        end
        -- Treat as legacy centroids JSON
        local f = io.open(jsonPath, "r")
        if not f then return false, "File not found: " .. jsonPath end
        local text = f:read("*a")
        f:close()
        local clusters, norms, err = parseJson(text)
        if not clusters then return false, "Parse error: " .. (err or "unknown") end
        _centroids    = clusters
        _featureNorms = norms
        _loadedPath   = jsonPath
        _modelSource  = "centroids"
        return true, nil
    end

    -- Auto-search: prefer ONNX meta JSON, fall back to legacy centroids JSON
    local ok, err = mlBridge.loadOnnxMeta()
    if ok then return true, nil end

    -- Fall back to legacy centroids
    for _, path in ipairs(DEFAULT_SEARCH_PATHS) do
        local f = io.open(path, "r")
        if f then
            local text = f:read("*a")
            f:close()
            local clusters, norms, parseErr = parseJson(text)
            if clusters then
                _centroids    = clusters
                _featureNorms = norms
                _loadedPath   = path
                _modelSource  = "centroids"
                return true, nil
            else
                return false, "Parse error in " .. path .. ": " .. (parseErr or "unknown")
            end
        end
    end

    return false, "Neither pcg_onnx_meta.json nor pcg_centroids.json found in search paths"
end

--- Explicitly load from a pcg_onnx_meta.json file.
-- The ONNX meta JSON includes the same centroid data as pcg_centroids.json
-- plus ONNX I/O metadata, so Lua can use it for nearest-centroid classification
-- without requiring an ONNX runtime on the Loenn side.
-- @param jsonPath   Explicit path; when nil, tries ONNX_META_SEARCH_PATHS.
-- @return boolean, string
function mlBridge.loadOnnxMeta(jsonPath)
    local paths = jsonPath and { jsonPath } or ONNX_META_SEARCH_PATHS

    for _, path in ipairs(paths) do
        local f = io.open(path, "r")
        if f then
            local text = f:read("*a")
            f:close()

            -- Try a proper JSON library first
            local jsonLib = tryRequireJson()
            local data
            if jsonLib then
                local ok, parsed = pcall(jsonLib.decode, text)
                if ok and parsed and parsed.clusters then
                    data = parsed
                end
            end

            if not data then
                -- Manual parse: the ONNX meta clusters block has the same format
                -- as pcg_centroids.json so parseJson handles it.
                local clusters, norms, err = parseJson(text)
                if not clusters then
                    return false, "Parse error in " .. path .. ": " .. (err or "unknown")
                end
                -- Build synthetic norms from centroid max values (ONNX meta may omit featureNorms)
                if not norms or (norms.enemyMax == 1 and norms.platformMax == 1) then
                    local eMax, pMax, gMax = 1, 1, 1
                    for _, c in ipairs(clusters) do
                        if c.centroid then
                            eMax = math.max(eMax, c.centroid[1] or 0)
                            pMax = math.max(pMax, c.centroid[2] or 0)
                            gMax = math.max(gMax, c.centroid[3] or 0)
                        end
                    end
                    norms = { enemyMax = eMax, platformMax = pMax, gapMax = gMax }
                end
                _centroids    = clusters
                _featureNorms = norms
                _loadedPath   = path
                _modelSource  = "onnx_meta"
                return true, nil
            end

            -- Proper JSON library succeeded
            local clusters = data.clusters or {}

            -- Derive feature norms from embedded featureNorms or from centroid ranges
            local norms = { enemyMax = 1, platformMax = 1, gapMax = 1 }
            if data.featureNorms then
                norms.enemyMax    = data.featureNorms.enemyMax    or 1
                norms.platformMax = data.featureNorms.platformMax or 1
                norms.gapMax      = data.featureNorms.gapMax      or 1
            else
                for _, c in ipairs(clusters) do
                    local ct = c.centroid
                    if ct then
                        norms.enemyMax    = math.max(norms.enemyMax,    ct[1] or 0)
                        norms.platformMax = math.max(norms.platformMax, ct[2] or 0)
                        norms.gapMax      = math.max(norms.gapMax,      ct[3] or 0)
                    end
                end
            end

            -- Normalise field names (ONNX meta uses camelCase same as centroids JSON)
            for _, c in ipairs(clusters) do
                c.suggestedPreset = c.suggestedPreset or c.suggested_preset or "default"
            end

            _centroids    = clusters
            _featureNorms = norms
            _loadedPath   = path
            _modelSource  = "onnx_meta"
            return true, nil
        end
    end

    return false, "pcg_onnx_meta.json not found in search paths"
end

--- Returns true when centroids have been successfully loaded.
function mlBridge.isLoaded()
    return _centroids ~= nil and #_centroids > 0
end

--- Returns the path the centroids were loaded from, or nil.
function mlBridge.loadedPath()
    return _loadedPath
end

--- Returns "onnx_meta" when loaded from pcg_onnx_meta.json, "centroids" for the
--- legacy pcg_centroids.json, or nil when nothing is loaded.
function mlBridge.modelSource()
    return _modelSource
end

--------------------------------------------------------------------------------
-- Feature normalisation + Euclidean distance
--------------------------------------------------------------------------------

--- Normalise a feature table against the feature norms from the JSON.
-- Features: { enemyCount, platformCount, averageGap, completionRate }
local function normalise(f)
    local norms = _featureNorms or { enemyMax=1, platformMax=1, gapMax=1 }
    return {
        f.enemyCount    / math.max(norms.enemyMax,    1),
        f.platformCount / math.max(norms.platformMax, 1),
        f.averageGap    / math.max(norms.gapMax,      1),
        f.completionRate,   -- already in [0,1]
    }
end

--- Squared Euclidean distance between a normalised feature vector and a centroid array.
local function sqDist(normVec, centroidVec, norms)
    -- centroidVec values are raw; re-normalise centroids on the fly
    local cn = {
        centroidVec[1] / math.max(norms.enemyMax,    1),
        centroidVec[2] / math.max(norms.platformMax, 1),
        centroidVec[3] / math.max(norms.gapMax,      1),
        centroidVec[4],   -- completionRate — already [0,1]
    }
    local sum = 0
    for i = 1, 4 do
        local d = normVec[i] - cn[i]
        sum = sum + d * d
    end
    return sum
end

--------------------------------------------------------------------------------
-- Public: classify features against loaded centroids
--------------------------------------------------------------------------------

--- Find the nearest K-Means centroid for the given feature table.
-- @param features   table: { enemyCount=N, platformCount=N, averageGap=N, completionRate=N }
-- @return cluster   The matching cluster table (id, label, suggestedPreset, entityDensity,
--                   difficulty, centroid), or nil if not loaded.
-- @return number    Squared Euclidean distance to the nearest centroid.
function mlBridge.classify(features)
    if not mlBridge.isLoaded() then return nil, nil end

    local norms  = _featureNorms or { enemyMax=1, platformMax=1, gapMax=1 }
    local normVec = normalise(features)

    local bestCluster, bestDist
    for _, cluster in ipairs(_centroids) do
        local d = sqDist(normVec, cluster.centroid, norms)
        if not bestDist or d < bestDist then
            bestDist    = d
            bestCluster = cluster
        end
    end

    return bestCluster, bestDist
end

--- Convenience: classify and return generation-ready guidance.
-- @param features   table: { enemyCount, platformCount, averageGap, completionRate }
-- @return table     { preset, entityDensity, difficulty, label, clusterId }
--                   or nil when ml_bridge is not loaded (caller should use defaults).
function mlBridge.guidePreset(features)
    local cluster, dist = mlBridge.classify(features)
    if not cluster then return nil end

    return {
        preset        = cluster.suggestedPreset or "default",
        entityDensity = cluster.entityDensity or 0.15,
        difficulty    = cluster.difficulty or 0.5,
        label         = cluster.label or "Unknown",
        clusterId     = cluster.id,
        distance      = dist,
    }
end

--------------------------------------------------------------------------------
-- Feature extraction from a Lönn room
--------------------------------------------------------------------------------

-- Lethal entity types (match generator.LE_ENTITIES)
local LE_ENTITIES = {
    spikesUp=true, spikesDown=true, spikesLeft=true, spikesRight=true,
    spinner=true, lightning=true, killbox=true,
    rotateSpinner=true, trackSpinner=true, dustStaticSpinner=true,
    finalBoss=true, seekerBarrier=true, seeker=true,
}

-- Non-lethal platform/movement entities (match generator.NLE_ENTITIES)
local NLE_ENTITIES = {
    refill=true, booster=true, spring=true, jumpThru=true,
    dreamBlock=true, dashBlock=true, moveBlock=true, zipMover=true,
    touchSwitch=true, crumbleWallOnRumble=true, fallingBlock=true,
    swapBlock=true, switchGate=true, floatySpaceBlock=true,
    bounceBlock=true, coreBlock=true, starJumpBlock=true,
    cloud=true, kevinsPC=true,
}

local TILE_AIR = "0"

--- Estimate average horizontal gap (run of air tiles) in a tile matrix.
-- Scan each row and compute the mean length of consecutive air runs.
local function estimateAvgGap(matrix)
    if not matrix then return 0 end
    local w, h = matrix:size()
    local totalRuns, runCount = 0, 0
    local currentRun = 0

    for y = 1, h do
        currentRun = 0
        for x = 1, w do
            local tile = matrix:get(x, y, TILE_AIR)
            if tile == TILE_AIR or tile == " " then
                currentRun = currentRun + 1
            else
                if currentRun > 0 then
                    totalRuns = totalRuns + currentRun
                    runCount  = runCount + 1
                    currentRun = 0
                end
            end
        end
        if currentRun > 0 then
            totalRuns = totalRuns + currentRun
            runCount  = runCount + 1
        end
    end

    return runCount > 0 and (totalRuns / runCount) or 0
end

--- Extract the PCGLevelData feature vector from a Lönn room table.
-- Returns: { enemyCount, platformCount, averageGap, completionRate }
-- completionRate is estimated as the air tile ratio (proxy for reachability).
function mlBridge.extractFeatures(room)
    local enemyCount    = 0
    local platformCount = 0

    if room.entities then
        for _, entity in ipairs(room.entities) do
            local name = entity._name or entity.name or ""
            if LE_ENTITIES[name] then
                enemyCount = enemyCount + 1
            elseif NLE_ENTITIES[name] then
                platformCount = platformCount + 1
            end
        end
    end

    -- Air ratio from foreground tiles → proxy for completion rate / openness
    local airRatio   = 0.5   -- default if no matrix
    local avgGap     = 2.0
    local matrix     = room.tilesFg and room.tilesFg.matrix or nil
    if matrix then
        local w, h   = matrix:size()
        local total  = w * h
        local airCnt = 0
        for y = 1, h do
            for x = 1, w do
                local tile = matrix:get(x, y, TILE_AIR)
                if tile == TILE_AIR or tile == " " then
                    airCnt = airCnt + 1
                end
            end
        end
        airRatio = airCnt / math.max(total, 1)
        avgGap   = estimateAvgGap(matrix)
    end

    return {
        enemyCount    = enemyCount,
        platformCount = platformCount,
        averageGap    = avgGap,
        completionRate= airRatio,
    }
end

--- Classify a Lönn room directly: extract features then classify.
-- @param room   Lönn room table
-- @return cluster, features, dist
function mlBridge.classifyRoom(room)
    local features = mlBridge.extractFeatures(room)
    local cluster, dist = mlBridge.classify(features)
    return cluster, features, dist
end

--------------------------------------------------------------------------------
-- Diagnostics
--------------------------------------------------------------------------------

--- Return a human-readable summary string for logging / Loenn status display.
function mlBridge.summary()
    if not mlBridge.isLoaded() then
        return "[PCG ML Bridge] No model data loaded.\n"
            .. "  Run the game once (for pcg_centroids.json) OR run:\n"
            .. "    python Tools/pcg_train_onnx.py --csv MLModels/gameplay_log.csv\n"
            .. "  to generate pcg_model.onnx + pcg_onnx_meta.json."
    end
    local src   = _modelSource or "unknown"
    local lines = { string.format("[PCG ML Bridge] Source=%-12s  Path=%s", src, _loadedPath or "?") }
    for _, c in ipairs(_centroids) do
        local ct = c.centroid or {0, 0, 0, 0}
        table.insert(lines, string.format(
            "  Cluster %d %-7s preset=%-8s  density=%.2f  diff=%.2f | centroid=[%.2f %.2f %.2f %.2f]",
            c.id or 0, "(" .. (c.label or "?") .. ")",
            c.suggestedPreset or "default",
            c.entityDensity or 0,
            c.difficulty or 0,
            ct[1] or 0, ct[2] or 0, ct[3] or 0, ct[4] or 0))
    end
    return table.concat(lines, "\n")
end

--- Try to auto-load on first require. Prefers pcg_onnx_meta.json, falls back to
--- pcg_centroids.json. Silent on failure so maps without exported models still open.
do
    local ok, _ = mlBridge.load()
    if not ok then
        -- Not an error — the model files haven't been generated yet.
        -- Call mlBridge.load() manually after running the game or the Python trainer.
    end
end

return mlBridge
