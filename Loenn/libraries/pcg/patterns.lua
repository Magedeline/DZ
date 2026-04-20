-- PCG Room Pattern Library
-- Defines reusable tile-path layouts ("patterns") that carve navigable paths
-- through generated rooms.  Each pattern is a function that stamps a path,
-- platforms, arenas, or boss stages onto a tile matrix *after* the Markov
-- pass, giving rooms unique structure instead of pure noise.
--
-- Patterns categories:
--   PATH      – winding traversal paths (like overworld maps)
--   ARENA     – open combat / boss arenas
--   CHALLENGE – precision platforming corridors
--   PUZZLE    – lock-and-key / switch gating
--   BOSS      – large boss fight rooms (Kirby mid-boss & final boss, normal player bosses)
--   HUB       – multi-exit hub rooms
--
-- Usage from other PCG modules:
--   local patterns = require("mods").requireFromPlugin("libraries.pcg.patterns")
--   patterns.apply(matrix, "serpentine", exits, opts)

local patterns = {}

--------------------------------------------------------------------------------
-- Internal helpers
--------------------------------------------------------------------------------

local TILE_AIR   = "0"
local TILE_SOLID = "1"  -- overridden by caller via opts.material

local function clamp(v, lo, hi) return math.max(lo, math.min(hi, v)) end

--- Carve a horizontal corridor of given height into the matrix.
local function carveHLine(matrix, x1, x2, y, halfH, w, h)
    local lo, hi = math.min(x1, x2), math.max(x1, x2)
    for x = clamp(lo, 2, w - 1), clamp(hi, 2, w - 1) do
        for dy = -halfH, halfH do
            local yy = clamp(y + dy, 2, h - 1)
            matrix:set(x, yy, TILE_AIR)
        end
    end
end

--- Carve a vertical corridor of given width into the matrix.
local function carveVLine(matrix, y1, y2, x, halfW, w, h)
    local lo, hi = math.min(y1, y2), math.max(y1, y2)
    for y = clamp(lo, 2, h - 1), clamp(hi, 2, h - 1) do
        for dx = -halfW, halfW do
            local xx = clamp(x + dx, 2, w - 1)
            matrix:set(xx, y, TILE_AIR)
        end
    end
end

--- Carve a rectangular area.
local function carveRect(matrix, cx, cy, rw, rh, w, h)
    local x1 = clamp(cx - math.floor(rw / 2), 2, w - 1)
    local x2 = clamp(cx + math.floor(rw / 2), 2, w - 1)
    local y1 = clamp(cy - math.floor(rh / 2), 2, h - 1)
    local y2 = clamp(cy + math.floor(rh / 2), 2, h - 1)
    for y = y1, y2 do
        for x = x1, x2 do
            matrix:set(x, y, TILE_AIR)
        end
    end
end

--- Place a platform (solid line) inside carved air.
local function placePlatform(matrix, x1, x2, y, material, w, h)
    material = material or TILE_SOLID
    for x = clamp(x1, 2, w - 1), clamp(x2, 2, w - 1) do
        if y >= 1 and y <= h then
            matrix:set(x, y, material)
        end
    end
end

--------------------------------------------------------------------------------
-- Organic Terrain Helpers — natural-looking shapes instead of rectangles
--------------------------------------------------------------------------------

--- Simple deterministic noise function (no external deps).
-- Returns a value roughly in [-1, 1] for continuous-ish terrain height variation.
local function noiseAt(x, seed)
    -- Hash-based pseudo-noise using prime mixing
    local n = x * 374761393 + (seed or 0) * 668265263
    n = (n * n * n) % 2147483647
    return (n / 1073741823.5) - 1.0
end

--- Smoothed noise: average of neighbouring noiseAt values for organic curves.
local function smoothNoise(x, seed, scale)
    scale = scale or 4
    local sx = x / scale
    local i = math.floor(sx)
    local f = sx - i
    -- Cubic smoothing
    f = f * f * (3 - 2 * f)
    local a = noiseAt(i, seed)
    local b = noiseAt(i + 1, seed)
    return a + (b - a) * f
end

--- Generate a natural ground-height profile across a room width.
-- Returns a table of Y-values (floor height in tiles from the room bottom).
-- @param w         Room width in tiles
-- @param baseY     Base floor Y position (tiles from top)
-- @param amplitude Max deviation in tiles (e.g., 3 means +/-3 from baseY)
-- @param seed      RNG seed for determinism
-- @param smoothing Noise scale (higher = smoother hills)
-- @return table    Array [1..w] of floor Y positions
local function generateGroundProfile(w, baseY, amplitude, seed, smoothing)
    amplitude = amplitude or 3
    seed = seed or 42
    smoothing = smoothing or 6
    local profile = {}
    for x = 1, w do
        -- Two octaves for natural variety
        local n1 = smoothNoise(x, seed, smoothing)
        local n2 = smoothNoise(x, seed + 100, smoothing * 0.5) * 0.4
        local offset = math.floor((n1 + n2) * amplitude + 0.5)
        profile[x] = clamp(baseY + offset, 4, baseY + amplitude + 2)
    end
    return profile
end

--- Generate a natural ceiling profile (inverse of ground).
local function generateCeilingProfile(w, baseY, amplitude, seed, smoothing)
    amplitude = amplitude or 2
    seed = seed or 99
    smoothing = smoothing or 5
    local profile = {}
    for x = 1, w do
        local n1 = smoothNoise(x, seed, smoothing)
        local n2 = smoothNoise(x, seed + 200, smoothing * 0.5) * 0.3
        local offset = math.floor((n1 + n2) * amplitude + 0.5)
        profile[x] = clamp(baseY + offset, 2, baseY + amplitude + 1)
    end
    return profile
end

--- Apply a natural ground profile to a room matrix — fills solid below the profile.
-- This replaces the flat-floor approach with rolling hills and valleys.
local function applyGroundProfile(matrix, profile, material, w, h)
    material = material or TILE_SOLID
    for x = 1, w do
        local groundY = profile[x]
        for y = groundY, h do
            matrix:set(clamp(x, 1, w), clamp(y, 1, h), material)
        end
    end
end

--- Apply a natural ceiling profile — fills solid above the profile.
local function applyCeilingProfile(matrix, profile, material, w, h)
    material = material or TILE_SOLID
    for x = 1, w do
        local ceilY = profile[x]
        for y = 1, ceilY do
            matrix:set(clamp(x, 1, w), clamp(y, 1, h), material)
        end
    end
end

--- Carve an organic (elliptical/blob) opening in the matrix.
-- Creates natural alcoves, cave bubbles, or rounded openings.
local function carveBlob(matrix, cx, cy, radiusX, radiusY, w, h, irregularity)
    irregularity = irregularity or 0.2
    local seed = cx * 31 + cy * 17
    for y = clamp(cy - radiusY - 1, 1, h), clamp(cy + radiusY + 1, 1, h) do
        for x = clamp(cx - radiusX - 1, 1, w), clamp(cx + radiusX + 1, 1, w) do
            local dx = (x - cx) / radiusX
            local dy = (y - cy) / radiusY
            -- Ellipse + noise for blobby edge
            local distSq = dx * dx + dy * dy
            local noiseMod = smoothNoise(x + y * 3, seed, 3) * irregularity
            if distSq + noiseMod < 1.0 then
                matrix:set(x, y, TILE_AIR)
            end
        end
    end
end

--- Place a curved (arch-shaped) platform between two points.
local function placeArchPlatform(matrix, x1, x2, peakY, baseY, material, w, h)
    material = material or TILE_SOLID
    local midX = math.floor((x1 + x2) / 2)
    local halfSpan = math.max((x2 - x1) / 2, 1)
    for x = clamp(x1, 2, w - 1), clamp(x2, 2, w - 1) do
        local t = (x - midX) / halfSpan  -- -1 to 1
        local archY = math.floor(peakY + (baseY - peakY) * t * t + 0.5)
        local py = clamp(archY, 2, h - 1)
        matrix:set(x, py, material)
    end
end

--- Add a natural-looking ledge/overhang.
local function placeLedge(matrix, x, y, length, direction, material, w, h)
    material = material or TILE_SOLID
    -- direction: 1 = right, -1 = left
    direction = direction or 1
    for i = 0, length - 1 do
        local lx = clamp(x + i * direction, 2, w - 1)
        -- Taper the ledge (gets thinner toward the end)
        local thickness = math.max(1, math.floor((length - i) / length * 2 + 0.5))
        for t = 0, thickness - 1 do
            local ly = clamp(y + t, 2, h - 1)
            matrix:set(lx, ly, material)
        end
    end
end

--- Add stalactites (hanging terrain from ceiling) or stalagmites (ground spikes).
local function placeFormation(matrix, x, fromY, toY, baseWidth, material, w, h)
    material = material or TILE_SOLID
    local height = math.abs(toY - fromY)
    local dir = toY > fromY and 1 or -1
    for i = 0, height do
        local y = clamp(fromY + i * dir, 2, h - 1)
        -- Taper: wider at base, narrower at tip
        local progress = i / math.max(height, 1)
        local halfW = math.max(0, math.floor(baseWidth * (1 - progress * 0.8) + 0.5))
        for dx = -halfW, halfW do
            matrix:set(clamp(x + dx, 2, w - 1), y, material)
        end
    end
end

--- Create a hill mound on the ground at a given position.
local function placeHill(matrix, cx, groundY, moundHeight, moundWidth, material, w, h)
    material = material or TILE_SOLID
    local halfW = math.floor(moundWidth / 2)
    for dx = -halfW, halfW do
        local x = clamp(cx + dx, 2, w - 1)
        local t = dx / halfW  -- -1 to 1
        local hillHeight = math.floor(moundHeight * (1 - t * t) + 0.5)
        for dy = 0, hillHeight - 1 do
            local y = clamp(groundY - dy, 2, h - 1)
            matrix:set(x, y, material)
        end
    end
end

--- Create stepped/terraced ground (Kirby Green Greens style).
local function placeSteppedTerrain(matrix, x1, x2, baseY, stepCount, stepHeight, material, w, h)
    material = material or TILE_SOLID
    local stepWidth = math.max(1, math.floor((x2 - x1) / stepCount))
    for s = 0, stepCount - 1 do
        local sx1 = x1 + s * stepWidth
        local sx2 = math.min(sx1 + stepWidth - 1, x2)
        local sy = baseY - s * stepHeight
        -- Fill the step and everything below
        for x = clamp(sx1, 2, w - 1), clamp(sx2, 2, w - 1) do
            for y = clamp(sy, 2, h - 1), clamp(baseY + 2, 2, h) do
                matrix:set(x, y, material)
            end
        end
    end
end

--- Create a pit with irregular/jagged edges (not a clean rectangle).
local function carveIrregularPit(matrix, cx, groundY, pitWidth, pitDepth, seed, w, h)
    seed = seed or cx * 7
    local halfW = math.floor(pitWidth / 2)
    for dx = -halfW, halfW do
        local x = clamp(cx + dx, 2, w - 1)
        -- Edge variation
        local edgeDist = math.min(math.abs(dx + halfW), math.abs(dx - halfW))
        local depthHere = pitDepth
        if edgeDist <= 2 then
            depthHere = math.floor(pitDepth * edgeDist / 3 + 0.5)
        end
        local noiseOff = math.floor(smoothNoise(x, seed, 3) * 1.5)
        depthHere = math.max(1, depthHere + noiseOff)
        for dy = 0, depthHere - 1 do
            local y = clamp(groundY + dy, 2, h)
            matrix:set(x, y, TILE_AIR)
        end
    end
end

--------------------------------------------------------------------------------
-- PATH Patterns – winding traversal paths
--------------------------------------------------------------------------------

--- "serpentine" – S-shaped path snaking left→right→left across the room
function patterns.serpentine(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local corridorH = opts.corridorHeight or 2
    local segments = opts.segments or 4

    local segH = math.floor((h - 4) / segments)
    local curY = 3
    local goingRight = true

    for seg = 1, segments do
        local x1 = goingRight and 3 or (w - 2)
        local x2 = goingRight and (w - 2) or 3
        carveHLine(matrix, x1, x2, curY, corridorH, w, h)

        if seg < segments then
            local nextY = curY + segH
            local turnX = goingRight and (w - 2) or 3
            carveVLine(matrix, curY, nextY, turnX, corridorH, w, h)
            curY = nextY
        end
        goingRight = not goingRight
    end
end

--- "zigzag" – sharp diagonal path bouncing wall to wall
function patterns.zigzag(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local turns = opts.turns or 5
    local corridorH = opts.corridorHeight or 2
    local segH = math.floor((h - 4) / turns)

    local curX = 4
    local curY = 3
    local goingRight = true

    for t = 1, turns do
        local targetX = goingRight and (w - 3) or 4
        -- Diagonal: step x and y simultaneously
        local steps = math.abs(targetX - curX)
        local yStep = segH / math.max(steps, 1)
        local xDir = targetX > curX and 1 or -1
        for s = 0, steps do
            local x = clamp(curX + s * xDir, 2, w - 1)
            local y = clamp(math.floor(curY + s * yStep), 2, h - 1)
            for dy = -corridorH, corridorH do
                local yy = clamp(y + dy, 2, h - 1)
                matrix:set(x, yy, TILE_AIR)
            end
        end
        curX = targetX
        curY = curY + segH
        goingRight = not goingRight
    end
end

--- "loop" – circular loop path around the room perimeter with shortcuts
function patterns.loop(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local margin = opts.margin or 4
    local corridorH = opts.corridorHeight or 2

    -- Outer perimeter loop
    carveHLine(matrix, margin, w - margin, margin, corridorH, w, h)           -- top
    carveHLine(matrix, margin, w - margin, h - margin, corridorH, w, h)       -- bottom
    carveVLine(matrix, margin, h - margin, margin, corridorH, w, h)           -- left
    carveVLine(matrix, margin, h - margin, w - margin, corridorH, w, h)       -- right

    -- Cross shortcut through center
    local cx, cy = math.floor(w / 2), math.floor(h / 2)
    carveHLine(matrix, margin, w - margin, cy, 1, w, h)
end

--- "branching" – main corridor with side branch pockets
function patterns.branching(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local branches = opts.branches or 4
    local corridorH = opts.corridorHeight or 2
    local cy = math.floor(h / 2)

    -- Main horizontal corridor
    carveHLine(matrix, 3, w - 2, cy, corridorH, w, h)

    -- Side branches going up or down alternately
    for i = 1, branches do
        local bx = math.floor(3 + (w - 6) * i / (branches + 1))
        local goUp = (i % 2 == 1)
        local by1 = cy
        local by2 = goUp and 3 or (h - 2)
        carveVLine(matrix, by1, by2, bx, 1, w, h)
        -- Small pocket at end
        carveRect(matrix, bx, by2, 5, 3, w, h)
    end
end

--- "spiral" – inward-spiraling path
function patterns.spiral(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local corridorH = opts.corridorHeight or 1
    local layers = opts.layers or 3

    local x1, y1, x2, y2 = 3, 3, w - 2, h - 2
    for layer = 1, layers do
        carveHLine(matrix, x1, x2, y1, corridorH, w, h)       -- top
        carveVLine(matrix, y1, y2, x2, corridorH, w, h)       -- right
        carveHLine(matrix, x1, x2, y2, corridorH, w, h)       -- bottom
        carveVLine(matrix, y1, y2, x1, corridorH, w, h)       -- left (partial, leave gap for entry)
        x1 = x1 + 3
        y1 = y1 + 3
        x2 = x2 - 3
        y2 = y2 - 3
        if x1 >= x2 or y1 >= y2 then break end
    end
end

--------------------------------------------------------------------------------
-- ARENA Patterns – open combat spaces
--------------------------------------------------------------------------------

--- "openArena" – large rectangular clearing with pillar obstacles
function patterns.openArena(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local margin = opts.margin or 4
    local pillars = opts.pillars or 3
    local material = opts.material or TILE_SOLID

    -- Clear central arena
    carveRect(matrix, math.floor(w / 2), math.floor(h / 2),
              w - margin * 2, h - margin * 2, w, h)

    -- Add pillars
    for i = 1, pillars do
        local px = math.floor(margin + (w - margin * 2) * i / (pillars + 1))
        local py = math.floor(h / 2) + ((i % 2 == 0) and -2 or 2)
        for dy = -1, 1 do
            for dx = -1, 1 do
                local xx = clamp(px + dx, 2, w - 1)
                local yy = clamp(py + dy, 2, h - 1)
                matrix:set(xx, yy, material)
            end
        end
    end
end

--- "tieredArena" – arena with raised platform tiers for verticality
function patterns.tieredArena(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local tiers = opts.tiers or 3
    local margin = opts.margin or 4
    local material = opts.material or TILE_SOLID

    -- Clear arena
    carveRect(matrix, math.floor(w / 2), math.floor(h / 2),
              w - margin * 2, h - margin * 2, w, h)

    -- Place tier platforms
    local tierSpacing = math.floor((h - margin * 2) / (tiers + 1))
    for t = 1, tiers do
        local py = margin + t * tierSpacing
        local platW = math.floor((w - margin * 2) * (0.4 + math.random() * 0.3))
        local px = math.floor(w / 2) + ((t % 2 == 0) and -3 or 3)
        placePlatform(matrix, px - math.floor(platW / 2), px + math.floor(platW / 2),
                      py, material, w, h)
    end
end

--- "circularArena" – round clearing with optional ring platforms
function patterns.circularArena(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    local radius = opts.radius or math.min(cx, cy) - 4
    local material = opts.material or TILE_SOLID

    -- Carve circle
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            local dx = x - cx
            local dy = y - cy
            if dx * dx + dy * dy < radius * radius then
                matrix:set(x, y, TILE_AIR)
            end
        end
    end

    -- Optional inner ring platform
    if opts.innerRing then
        local ir = math.floor(radius * 0.5)
        for y = 2, h - 1 do
            for x = 2, w - 1 do
                local dx = x - cx
                local dy = y - cy
                local dist2 = dx * dx + dy * dy
                if dist2 >= (ir - 1) * (ir - 1) and dist2 <= ir * ir then
                    matrix:set(x, y, material)
                end
            end
        end
    end
end

--------------------------------------------------------------------------------
-- CHALLENGE Patterns – precision platforming
--------------------------------------------------------------------------------

--- "gauntlet" – narrow corridor with alternating obstacles
function patterns.gauntlet(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local corridorH = opts.corridorHeight or 3
    local obstacles = opts.obstacles or 6
    local material = opts.material or TILE_SOLID
    local cy = math.floor(h / 2)

    -- Main narrow corridor
    carveHLine(matrix, 3, w - 2, cy, corridorH, w, h)

    -- Alternating floor/ceiling obstacles
    for i = 1, obstacles do
        local ox = math.floor(3 + (w - 6) * i / (obstacles + 1))
        local fromTop = (i % 2 == 1)
        if fromTop then
            -- Pillar from ceiling
            for y = cy - corridorH, cy - 1 do
                local yy = clamp(y, 2, h - 1)
                matrix:set(clamp(ox, 2, w - 1), yy, material)
            end
        else
            -- Pillar from floor
            for y = cy + 1, cy + corridorH do
                local yy = clamp(y, 2, h - 1)
                matrix:set(clamp(ox, 2, w - 1), yy, material)
            end
        end
    end
end

--- "staircase" – ascending / descending platforms
function patterns.staircase(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local steps = opts.steps or 8
    local ascending = opts.ascending
    if ascending == nil then ascending = true end
    local material = opts.material or TILE_SOLID

    -- Clear interior first
    carveRect(matrix, math.floor(w / 2), math.floor(h / 2),
              w - 6, h - 6, w, h)

    -- Place stair platforms
    local stepW = math.floor((w - 6) / steps)
    for i = 1, steps do
        local sx = 3 + (i - 1) * stepW
        local sy
        if ascending then
            sy = h - 3 - math.floor((h - 6) * (i - 1) / steps)
        else
            sy = 3 + math.floor((h - 6) * (i - 1) / steps)
        end
        placePlatform(matrix, sx, sx + stepW - 1, sy, material, w, h)
    end
end

--- "precisionGaps" – series of gaps the player must jump across
function patterns.precisionGaps(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local gaps = opts.gaps or 5
    local floorY = h - 3
    local material = opts.material or TILE_SOLID

    -- Clear interior
    carveRect(matrix, math.floor(w / 2), math.floor(h / 2),
              w - 6, h - 6, w, h)

    -- Place floor segments with gaps
    local segW = math.floor((w - 6) / (gaps * 2 + 1))
    for i = 0, gaps * 2 do
        if i % 2 == 0 then
            local sx = 3 + i * segW
            placePlatform(matrix, sx, sx + segW - 1, floorY, material, w, h)
        end
    end
end

--------------------------------------------------------------------------------
-- PUZZLE Patterns – lock-and-key / switch rooms
--------------------------------------------------------------------------------

--- "dividedChambers" – room split into chambers connected by narrow passages
function patterns.dividedChambers(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local chambers = opts.chambers or 3
    local material = opts.material or TILE_SOLID

    local chamberW = math.floor((w - 4) / chambers)

    for c = 0, chambers - 1 do
        local cx = 3 + c * chamberW + math.floor(chamberW / 2)
        local cy = math.floor(h / 2)
        carveRect(matrix, cx, cy, chamberW - 3, h - 6, w, h)

        -- Divider wall with door
        if c < chambers - 1 then
            local wallX = 3 + (c + 1) * chamberW
            for y = 2, h - 1 do
                matrix:set(clamp(wallX, 2, w - 1), y, material)
            end
            -- Door
            local doorY = math.floor(h / 2)
            for dy = -1, 1 do
                matrix:set(clamp(wallX, 2, w - 1), clamp(doorY + dy, 2, h - 1), TILE_AIR)
            end
        end
    end
end

--- "switchMaze" – small maze-like structure with dead ends for switches
function patterns.switchMaze(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local corridorH = opts.corridorHeight or 1

    -- Grid of corridors
    local cellW = math.floor((w - 4) / 4)
    local cellH = math.floor((h - 4) / 3)
    math.randomseed(opts.seed or os.time())

    for row = 0, 2 do
        for col = 0, 3 do
            local cx = 3 + col * cellW + math.floor(cellW / 2)
            local cy = 3 + row * cellH + math.floor(cellH / 2)
            carveRect(matrix, cx, cy, cellW - 2, cellH - 2, w, h)

            -- Random connections right and down
            if col < 3 and math.random() > 0.3 then
                carveHLine(matrix, cx, cx + cellW, cy, corridorH, w, h)
            end
            if row < 2 and math.random() > 0.3 then
                carveVLine(matrix, cy, cy + cellH, cx, corridorH, w, h)
            end
        end
    end
end

--------------------------------------------------------------------------------
-- BOSS Patterns – large boss fight rooms
--------------------------------------------------------------------------------

--- "kirbyBossArena" – wide flat arena with platforms, designed for Kirby mid-bosses
function patterns.kirbyBossArena(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID

    -- Clear large arena (leave thick border)
    carveRect(matrix, math.floor(w / 2), math.floor(h / 2),
              w - 8, h - 8, w, h)

    -- Ground: solid floor at bottom quarter
    local floorY = h - 5
    placePlatform(matrix, 5, w - 4, floorY, material, w, h)

    -- Side platforms (Kirby bosses often have elevated positions)
    local platY1 = math.floor(h * 0.4)
    placePlatform(matrix, 5, math.floor(w * 0.3), platY1, material, w, h)
    placePlatform(matrix, math.floor(w * 0.7), w - 4, platY1, material, w, h)

    -- Center floating platform
    local cx = math.floor(w / 2)
    local platY2 = math.floor(h * 0.55)
    placePlatform(matrix, cx - 4, cx + 4, platY2, material, w, h)

    -- Ceiling alcove for Kracko-type bosses
    if opts.ceilingAlcove then
        carveRect(matrix, cx, 4, 10, 3, w, h)
    end
end

--- "kirbyFinalBossArena" – multi-phase boss room, larger with phase-transition pillars
function patterns.kirbyFinalBossArena(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID

    -- Full room clear except thick border
    carveRect(matrix, math.floor(w / 2), math.floor(h / 2),
              w - 6, h - 6, w, h)

    -- Main floor
    local floorY = h - 4
    placePlatform(matrix, 4, w - 3, floorY, material, w, h)

    -- Three-tier platform layout for phase transitions
    local cx = math.floor(w / 2)
    -- Phase 1 platforms – low side platforms
    placePlatform(matrix, 5, math.floor(w * 0.25), floorY - 4, material, w, h)
    placePlatform(matrix, math.floor(w * 0.75), w - 4, floorY - 4, material, w, h)

    -- Phase 2 platforms – mid-height
    placePlatform(matrix, math.floor(w * 0.2), math.floor(w * 0.45), floorY - 9, material, w, h)
    placePlatform(matrix, math.floor(w * 0.55), math.floor(w * 0.8), floorY - 9, material, w, h)

    -- Phase 3 center pinnacle
    placePlatform(matrix, cx - 3, cx + 3, floorY - 14, material, w, h)

    -- Destructible pillars (visual markers, entities placed by generator)
    local pillarPositions = {
        math.floor(w * 0.33), math.floor(w * 0.67)
    }
    for _, px in ipairs(pillarPositions) do
        for y = floorY - 3, floorY - 1 do
            matrix:set(clamp(px, 2, w - 1), clamp(y, 2, h - 1), material)
        end
    end
end

--- "bossCorridorApproach" – long horizontal corridor leading into an arena
function patterns.bossCorridorApproach(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local corridorH = opts.corridorHeight or 3
    local arenaFraction = opts.arenaFraction or 0.45

    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    local arenaStartX = math.floor(w * (1 - arenaFraction))

    -- Corridor section (left portion)
    carveHLine(matrix, 3, arenaStartX, cy, corridorH, w, h)
    -- Floor under corridor
    placePlatform(matrix, 3, arenaStartX, cy + corridorH + 1, material, w, h)

    -- Arena section (right portion)
    carveRect(matrix, math.floor((arenaStartX + w) / 2), cy,
              w - arenaStartX - 4, h - 8, w, h)
    -- Arena floor
    placePlatform(matrix, arenaStartX, w - 3, h - 5, material, w, h)
    -- Arena side platform
    placePlatform(matrix, arenaStartX + 2, arenaStartX + 8, cy - 2, material, w, h)
end

--- "normalBossArena" – standard Celeste-style boss room (Badeline-like)
function patterns.normalBossArena(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID

    -- Clear room leaving border
    carveRect(matrix, math.floor(w / 2), math.floor(h / 2),
              w - 6, h - 6, w, h)

    -- Bottom floor
    placePlatform(matrix, 4, w - 3, h - 4, material, w, h)

    -- Scattered platforms at different heights for dodging
    local numPlats = opts.platforms or 5
    math.randomseed(opts.seed or os.time())
    for i = 1, numPlats do
        local px = math.floor(4 + (w - 8) * i / (numPlats + 1))
        local py = math.floor(h * 0.3 + math.random() * (h * 0.35))
        local pw = 4 + math.random(3)
        placePlatform(matrix, px - math.floor(pw / 2), px + math.floor(pw / 2),
                      py, material, w, h)
    end
end

--- "multiBossArena" – arena split into Kirby-side and normal-player-side
function patterns.multiBossArena(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID

    local split = math.floor(w / 2)

    -- Left side: Kirby-style (flat platforms, wide open)
    carveRect(matrix, math.floor(split / 2), math.floor(h / 2),
              split - 6, h - 8, w, h)
    placePlatform(matrix, 5, split - 3, h - 5, material, w, h)
    placePlatform(matrix, 5, math.floor(split * 0.5), math.floor(h * 0.45), material, w, h)

    -- Right side: Celeste-style (multi-height platforms)
    carveRect(matrix, split + math.floor((w - split) / 2), math.floor(h / 2),
              w - split - 6, h - 8, w, h)
    placePlatform(matrix, split + 3, w - 4, h - 5, material, w, h)
    local numPlats = 4
    for i = 1, numPlats do
        local px = split + math.floor((w - split - 6) * i / (numPlats + 1))
        local py = math.floor(h * 0.3 + (i % 2 == 0 and 0 or h * 0.15))
        placePlatform(matrix, px - 2, px + 2, py, material, w, h)
    end

    -- Connecting passage at mid height
    carveHLine(matrix, split - 2, split + 2, math.floor(h / 2), 1, w, h)
end

--------------------------------------------------------------------------------
-- HUB Patterns – multi-exit hub rooms
--------------------------------------------------------------------------------

--- "crossroads" – plus-shaped hub connecting 4 directions
function patterns.crossroads(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local corridorH = opts.corridorHeight or 3

    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)

    -- Horizontal corridor
    carveHLine(matrix, 3, w - 2, cy, corridorH, w, h)
    -- Vertical corridor
    carveVLine(matrix, 3, h - 2, cx, corridorH, w, h)
    -- Center clearing
    carveRect(matrix, cx, cy, corridorH * 3, corridorH * 3, w, h)
end

--- "starHub" – diamond-shaped center with radiating paths
function patterns.starHub(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    local armLen = opts.armLength or math.floor(math.min(w, h) / 3)
    local corridorH = opts.corridorHeight or 2

    -- Center diamond
    carveRect(matrix, cx, cy, 7, 7, w, h)

    -- Arms to exits
    if exits and exits.left then
        carveHLine(matrix, 3, cx, cy, corridorH, w, h)
    end
    if exits and exits.right then
        carveHLine(matrix, cx, w - 2, cy, corridorH, w, h)
    end
    if exits and exits.top then
        carveVLine(matrix, 3, cy, cx, corridorH, w, h)
    end
    if exits and exits.bottom then
        carveVLine(matrix, cy, h - 2, cx, corridorH, w, h)
    end

    -- Diagonal arms
    for angle = 0, 3 do
        local dx = (angle == 0 or angle == 3) and 1 or -1
        local dy = (angle < 2) and -1 or 1
        for step = 1, math.floor(armLen * 0.6) do
            local x = clamp(cx + dx * step, 2, w - 1)
            local y = clamp(cy + dy * step, 2, h - 1)
            matrix:set(x, y, TILE_AIR)
            if x + 1 <= w then matrix:set(x + 1, y, TILE_AIR) end
            if y + 1 <= h then matrix:set(x, y + 1, TILE_AIR) end
        end
    end
end

--- "asrielGodBossArena" – Epic boss arena for Kirby vs Asriel God Boss fights
-- Features multi-tier platforms, dramatic central layout, and strategic positions
function patterns.asrielGodBossArena(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local phase = opts.phase or 1  -- 1, 2, or 3 for increasing intensity

    -- Clear massive arena (larger than normal boss rooms)
    carveRect(matrix, math.floor(w / 2), math.floor(h / 2),
              w - 8, h - 8, w, h)

    local cx = math.floor(w / 2)
    local cy = math.floor(h / 2)
    local floorY = h - 5

    -- Main floor platform
    placePlatform(matrix, 6, w - 5, floorY, material, w, h)

    -- Side platforms for dodging (higher for phase 2+)
    local sidePlatY = math.floor(h * 0.35) - (phase - 1) * 2
    placePlatform(matrix, 6, math.floor(w * 0.25), sidePlatY, material, w, h)
    placePlatform(matrix, math.floor(w * 0.75), w - 5, sidePlatY, material, w, h)

    -- Mid-height platforms (more for higher phases)
    local midPlatY = math.floor(h * 0.55)
    placePlatform(matrix, math.floor(w * 0.28), math.floor(w * 0.42), midPlatY, material, w, h)
    placePlatform(matrix, math.floor(w * 0.58), math.floor(w * 0.72), midPlatY, material, w, h)

    if phase >= 2 then
        -- Additional floating platforms for phase 2
        local upperY = math.floor(h * 0.25)
        placePlatform(matrix, math.floor(w * 0.15), math.floor(w * 0.3), upperY, material, w, h)
        placePlatform(matrix, math.floor(w * 0.7), math.floor(w * 0.85), upperY, material, w, h)
    end

    -- Center boss perch platform
    local bossPerchY = math.floor(h * 0.22) - (phase - 1) * 2
    local perchWidth = 6 + phase * 2
    placePlatform(matrix, cx - perchWidth, cx + perchWidth, bossPerchY, material, w, h)

    if phase >= 3 then
        -- Phase 3: Create floating ring structure
        local radius = math.min(math.floor(w * 0.35), math.floor(h * 0.35))
        for angle = 0, 7 do
            local rad = angle * math.pi / 4
            local px = cx + math.floor(math.cos(rad) * radius * 0.7)
            local py = cy + math.floor(math.sin(rad) * radius * 0.5)
            if py > 4 and py < h - 4 then
                placePlatform(matrix, clamp(px - 3, 3, w - 2), clamp(px + 3, 3, w - 2), py, material, w, h)
            end
        end
    end

    -- Tower pillars at higher phases
    if phase >= 2 then
        local pillarPositions = { math.floor(w * 0.15), math.floor(w * 0.85) }
        for _, px in ipairs(pillarPositions) do
            for y = floorY - 8, floorY - 1 do
                matrix:set(clamp(px, 2, w - 1), clamp(y, 2, h - 1), material)
                matrix:set(clamp(px + 1, 2, w - 1), clamp(y, 2, h - 1), material)
            end
        end
    end
end

--------------------------------------------------------------------------------
-- KIRBY TERRAIN Patterns — nature-inspired, non-boxy room layouts
-- Based on design patterns from the Kirby series level design document.
--------------------------------------------------------------------------------

--- "greenGreens" – Kirby's iconic first level: rolling hills, tree trunk alcove,
--- stepped terrain, scattered platforms. Feels like a grassy field, not a dungeon.
function patterns.greenGreens(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 42

    -- Start with all air
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            matrix:set(x, y, TILE_AIR)
        end
    end

    -- Natural rolling ground with hills
    local baseGroundY = h - 5
    local groundProfile = generateGroundProfile(w, baseGroundY, 3, seed, 7)
    applyGroundProfile(matrix, groundProfile, material, w, h)

    -- Large tree trunk alcove (left-center area) — inside a tree trunk pattern
    local treeX = math.floor(w * 0.3)
    local trunkWidth = 6
    local trunkHeight = 10
    local trunkTop = groundProfile[treeX] - trunkHeight
    -- Trunk walls
    for y = clamp(trunkTop, 3, h), clamp(groundProfile[treeX], 3, h) do
        matrix:set(clamp(treeX - trunkWidth, 2, w - 1), y, material)
        matrix:set(clamp(treeX + trunkWidth, 2, w - 1), y, material)
    end
    -- Trunk top (canopy)
    placePlatform(matrix, treeX - trunkWidth, treeX + trunkWidth, clamp(trunkTop, 3, h - 1), material, w, h)
    -- Hollow inside
    for y = clamp(trunkTop + 1, 3, h), clamp(groundProfile[treeX] - 1, 3, h) do
        for x = clamp(treeX - trunkWidth + 1, 3, w - 1), clamp(treeX + trunkWidth - 1, 3, w - 1) do
            matrix:set(x, y, TILE_AIR)
        end
    end
    -- Trunk opening on one side
    local openSide = treeX + trunkWidth
    for dy = 0, 3 do
        matrix:set(clamp(openSide, 2, w - 1), clamp(groundProfile[treeX] - 1 - dy, 3, h), TILE_AIR)
    end

    -- Elevated grassy platforms at varying heights
    local platY1 = groundProfile[math.floor(w * 0.55)] - 5
    placePlatform(matrix, math.floor(w * 0.5), math.floor(w * 0.65), clamp(platY1, 4, h - 2), material, w, h)

    local platY2 = groundProfile[math.floor(w * 0.75)] - 7
    placePlatform(matrix, math.floor(w * 0.7), math.floor(w * 0.82), clamp(platY2, 4, h - 2), material, w, h)

    -- Small hill mound near the right side
    placeHill(matrix, math.floor(w * 0.85), baseGroundY, 3, 8, material, w, h)

    -- Stepped terrain going up on the left
    placeSteppedTerrain(matrix, 3, math.floor(w * 0.15), baseGroundY, 3, 2, material, w, h)
end

--- "castleDedede" – Castle interior: columns, arched ceilings, platforms, royal feel.
function patterns.castleDedede(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 55

    -- Start with all air inside
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            matrix:set(x, y, TILE_AIR)
        end
    end

    -- Arched ceiling
    local ceilProfile = generateCeilingProfile(w, 3, 2, seed, 8)
    applyCeilingProfile(matrix, ceilProfile, material, w, h)

    -- Flat stone floor
    local floorY = h - 4
    placePlatform(matrix, 2, w - 1, floorY, material, w, h)
    placePlatform(matrix, 2, w - 1, floorY + 1, material, w, h)

    -- Decorative columns
    local numCols = math.floor(w / 14)
    for i = 1, numCols do
        local cx = math.floor(w * i / (numCols + 1))
        for y = ceilProfile[clamp(cx, 1, w)] + 1, floorY - 1 do
            matrix:set(clamp(cx, 2, w - 1), y, material)
            matrix:set(clamp(cx + 1, 2, w - 1), y, material)
        end
    end

    -- Elevated balcony platforms between columns
    for i = 1, numCols - 1 do
        local lx = math.floor(w * i / (numCols + 1)) + 3
        local rx = math.floor(w * (i + 1) / (numCols + 1)) - 3
        if rx > lx + 2 then
            local balconyY = floorY - 6 - (i % 2) * 2
            placePlatform(matrix, lx, rx, clamp(balconyY, 4, floorY - 3), material, w, h)
        end
    end

    -- Royal carpet area (wider central floor section) — slightly raised center
    local centerL = math.floor(w * 0.35)
    local centerR = math.floor(w * 0.65)
    placePlatform(matrix, centerL, centerR, floorY - 1, material, w, h)
end

--- "iceCave" – Natural ice cavern: stalactites, stalagmites, rounded walls,
--- slippery-looking terrain with irregular floor/ceiling.
function patterns.iceCave(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 77

    -- Fill everything solid first, then carve the cave
    for y = 1, h do
        for x = 1, w do
            matrix:set(x, y, material)
        end
    end

    -- Carve main cave opening using a large irregular blob
    carveBlob(matrix, math.floor(w / 2), math.floor(h / 2),
              math.floor(w * 0.4), math.floor(h * 0.35), w, h, 0.25)

    -- Carve secondary connected chambers
    carveBlob(matrix, math.floor(w * 0.2), math.floor(h * 0.55),
              math.floor(w * 0.15), math.floor(h * 0.2), w, h, 0.3)
    carveBlob(matrix, math.floor(w * 0.8), math.floor(h * 0.45),
              math.floor(w * 0.15), math.floor(h * 0.2), w, h, 0.3)

    -- Stalactites hanging from ceiling
    local caveTopY = 4
    for i = 1, math.floor(w / 10) do
        local sx = math.floor(w * i / (math.floor(w / 10) + 1))
        local stalH = 3 + math.floor(smoothNoise(sx, seed + 300, 4) * 2)
        placeFormation(matrix, sx, caveTopY, caveTopY + stalH, 2, material, w, h)
    end

    -- Stalagmites rising from floor
    local caveFloorY = h - 5
    for i = 1, math.floor(w / 12) do
        local sx = math.floor(w * i / (math.floor(w / 12) + 1)) + 3
        local stagH = 2 + math.floor(smoothNoise(sx, seed + 400, 3) * 2)
        placeFormation(matrix, sx, caveFloorY, caveFloorY - stagH, 1, material, w, h)
    end

    -- Natural floor with rolling terrain
    local floorProfile = generateGroundProfile(w, h - 5, 2, seed + 500, 5)
    for x = 2, w - 1 do
        local fy = floorProfile[x]
        for y = fy, h - 1 do
            -- Only set solid where it was previously air (inside the cave)
            if matrix:get(x, y, material) == TILE_AIR then
                -- Skip — keep the carved cave shape
            else
                matrix:set(x, y, material)
            end
        end
    end

    -- Internal ledges for platforming
    local ledgeY1 = math.floor(h * 0.4)
    placeLedge(matrix, math.floor(w * 0.2), ledgeY1, 6, 1, material, w, h)
    placeLedge(matrix, math.floor(w * 0.75), ledgeY1 - 2, 5, -1, material, w, h)
end

--- "rollingHills" – Open outdoor Kirby level: gentle rolling terrain with
--- scattered platforms, small pits, and elevation changes. Like Pop Star stages.
function patterns.rollingHills(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 33

    -- Start all air
    for y = 1, h do
        for x = 1, w do
            matrix:set(x, y, TILE_AIR)
        end
    end

    -- Rolling ground profile — the main feature
    local baseY = h - 6
    local groundProfile = generateGroundProfile(w, baseY, 4, seed, 8)
    applyGroundProfile(matrix, groundProfile, material, w, h)

    -- Small pits with irregular edges
    carveIrregularPit(matrix, math.floor(w * 0.3), groundProfile[math.floor(w * 0.3)], 6, 4, seed + 10, w, h)
    carveIrregularPit(matrix, math.floor(w * 0.65), groundProfile[math.floor(w * 0.65)], 5, 3, seed + 20, w, h)

    -- Floating platforms at natural-feeling positions (follow the terrain rhythm)
    for i = 1, 4 do
        local px = math.floor(w * (0.15 + 0.2 * (i - 1)))
        local groundHere = groundProfile[clamp(px, 1, w)]
        local platY = groundHere - 5 - math.floor(smoothNoise(px, seed + 600, 5) * 2)
        local platLen = 4 + math.floor(math.abs(smoothNoise(px, seed + 700, 4)) * 3)
        placePlatform(matrix, clamp(px - math.floor(platLen / 2), 3, w - 2),
                      clamp(px + math.floor(platLen / 2), 3, w - 2),
                      clamp(platY, 4, groundHere - 3), material, w, h)
    end

    -- Hill mounds for visual interest
    placeHill(matrix, math.floor(w * 0.2), baseY, 2, 10, material, w, h)
    placeHill(matrix, math.floor(w * 0.7), baseY, 3, 12, material, w, h)
end

--- "bomberCorridor" – A straight path with platforms overhead holding hazards.
--- Based on the Kirby Bomber corridor design pattern.
function patterns.bomberCorridor(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 88

    -- All air
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            matrix:set(x, y, TILE_AIR)
        end
    end

    -- Natural ground
    local baseY = h - 4
    local groundProfile = generateGroundProfile(w, baseY, 1, seed, 10)
    applyGroundProfile(matrix, groundProfile, material, w, h)

    -- Ceiling with arched profile
    local ceilProfile = generateCeilingProfile(w, 3, 1, seed + 100, 8)
    applyCeilingProfile(matrix, ceilProfile, material, w, h)

    -- Overhead bomber platforms (evenly spaced, slightly varied heights)
    local numPlatforms = math.floor(w / 10)
    for i = 1, numPlatforms do
        local px = math.floor(w * i / (numPlatforms + 1))
        local platY = math.floor(h * 0.35) + math.floor(smoothNoise(px, seed + 200, 4) * 2)
        placePlatform(matrix, px - 2, px + 2, clamp(platY, 5, h - 8), material, w, h)
    end
end

--- "boulderRun" – Chase corridor: narrowing passage with slopes.
--- Based on the Kirby boulder run design pattern.
function patterns.boulderRun(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 66

    -- All air
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            matrix:set(x, y, TILE_AIR)
        end
    end

    -- Sloping downward ground (left to right descent)
    local startY = h - 8
    local endY = h - 3
    for x = 2, w - 1 do
        local t = (x - 2) / math.max(w - 3, 1)
        local baseFloor = math.floor(startY + (endY - startY) * t + 0.5)
        -- Add slight noise for organic feel
        local noiseOff = math.floor(smoothNoise(x, seed, 5) * 1.5)
        local floorY = clamp(baseFloor + noiseOff, 5, h - 2)
        for y = floorY, h do
            matrix:set(x, y, material)
        end
    end

    -- Narrowing ceiling (follows the descent but stays closer on the right)
    for x = 2, w - 1 do
        local t = (x - 2) / math.max(w - 3, 1)
        local baseCeil = math.floor(3 + t * 4 + 0.5)
        local noiseOff = math.floor(smoothNoise(x, seed + 50, 4))
        local ceilY = clamp(baseCeil + noiseOff, 2, h - 6)
        for y = 1, ceilY do
            matrix:set(x, y, material)
        end
    end

    -- Obstacle ledges jutting from walls (things to dodge while running)
    for i = 1, 4 do
        local lx = math.floor(w * i / 5)
        local fromTop = (i % 2 == 1)
        if fromTop then
            local cy = 3 + math.floor(i * 1.5)
            placeLedge(matrix, lx, clamp(cy, 3, h - 6), 4, 1, material, w, h)
        else
            local t = (lx - 2) / math.max(w - 3, 1)
            local floorHere = math.floor((h - 8) + ((h - 3) - (h - 8)) * t + 0.5)
            placeLedge(matrix, lx, clamp(floorHere - 3, 4, h - 4), 3, -1, material, w, h)
        end
    end
end

--- "crumblingCliffs" – Kirby crumbling terrain: tall room with ledge-hopping,
--- layered platforms that suggest fragile/breakable terrain.
function patterns.crumblingCliffs(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 44

    -- All air
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            matrix:set(x, y, TILE_AIR)
        end
    end

    -- Ground at bottom
    local baseY = h - 3
    local groundProfile = generateGroundProfile(w, baseY, 1, seed, 8)
    applyGroundProfile(matrix, groundProfile, material, w, h)

    -- Staggered crumbling platforms going upward (alternating sides)
    local layers = 5
    for layer = 1, layers do
        local t = layer / (layers + 1)
        local platY = math.floor(baseY - t * (h - 8))
        local goRight = (layer % 2 == 1)
        local startX, endX
        if goRight then
            startX = 4 + (layer - 1) * 2
            endX = startX + math.floor(w * 0.35)
        else
            endX = w - 4 - (layer - 1) * 2
            startX = endX - math.floor(w * 0.35)
        end
        -- Slightly irregular platform
        for x = clamp(startX, 3, w - 2), clamp(endX, 3, w - 2) do
            local nOff = math.floor(smoothNoise(x, seed + layer * 50, 3) * 0.8)
            matrix:set(x, clamp(platY + nOff, 3, h - 2), material)
        end
    end

    -- Small connecting ledges between layers
    for layer = 1, layers - 1 do
        local t1 = layer / (layers + 1)
        local t2 = (layer + 1) / (layers + 1)
        local y1 = math.floor(baseY - t1 * (h - 8))
        local y2 = math.floor(baseY - t2 * (h - 8))
        local midY = math.floor((y1 + y2) / 2)
        local midX = math.floor(w / 2) + ((layer % 2 == 0) and 4 or -4)
        placePlatform(matrix, clamp(midX - 2, 3, w - 2), clamp(midX + 2, 3, w - 2),
                      clamp(midY, 4, h - 3), material, w, h)
    end
end

--- "switchAndRun" – Kirby "hit the switch and run" puzzle:
--- A room with a switch area, a timed path, and a goal behind barriers.
function patterns.switchAndRun(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 99

    -- All air
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            matrix:set(x, y, TILE_AIR)
        end
    end

    -- Floor
    local floorY = h - 4
    local groundProfile = generateGroundProfile(w, floorY, 1, seed, 12)
    applyGroundProfile(matrix, groundProfile, material, w, h)

    -- Ceiling
    local ceilProfile = generateCeilingProfile(w, 3, 1, seed + 100, 10)
    applyCeilingProfile(matrix, ceilProfile, material, w, h)

    -- Switch alcove (left section) — recessed area
    carveBlob(matrix, 8, math.floor(h * 0.5), 5, 4, w, h, 0.15)

    -- Run corridor (middle) — clear path with slight obstacles
    carveRect(matrix, math.floor(w / 2), math.floor(h * 0.6),
              math.floor(w * 0.5), 6, w, h)

    -- Barrier wall with gap (right section) — the timed gate
    local barrierX = math.floor(w * 0.75)
    for y = ceilProfile[clamp(barrierX, 1, w)] + 1, floorY - 1 do
        matrix:set(clamp(barrierX, 2, w - 1), y, material)
    end
    -- Gap in barrier (the timed opening)
    local gapY = math.floor(h * 0.55)
    for dy = -2, 2 do
        matrix:set(clamp(barrierX, 2, w - 1), clamp(gapY + dy, 3, h - 2), TILE_AIR)
    end

    -- Reward chamber behind barrier
    carveBlob(matrix, math.floor(w * 0.88), math.floor(h * 0.5), 5, 4, w, h, 0.1)
end

--- "doorLabyrinth" – Multiple interconnected chambers with narrow passages.
--- Based on the Kirby door labyrinth design pattern.
function patterns.doorLabyrinth(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 111

    -- Fill solid initially
    for y = 1, h do
        for x = 1, w do
            matrix:set(x, y, material)
        end
    end

    -- Carve 4-6 interconnected chambers as organic blobs
    local chambers = opts.chambers or 5
    local chamberPositions = {}
    for i = 1, chambers do
        local cx = math.floor(w * (0.15 + 0.7 * ((i - 1) / math.max(chambers - 1, 1))))
        local cy = math.floor(h * (0.3 + 0.4 * smoothNoise(i * 7, seed, 2)))
        local rx = 4 + math.floor(math.abs(smoothNoise(i * 11, seed + 100, 3)) * 3)
        local ry = 3 + math.floor(math.abs(smoothNoise(i * 13, seed + 200, 3)) * 2)
        carveBlob(matrix, cx, cy, rx, ry, w, h, 0.2)
        table.insert(chamberPositions, {x = cx, y = cy})
        -- Floor in each chamber
        placePlatform(matrix, cx - rx + 1, cx + rx - 1, clamp(cy + ry - 1, 3, h - 2), material, w, h)
    end

    -- Connect adjacent chambers with narrow passages
    for i = 1, #chamberPositions - 1 do
        local a = chamberPositions[i]
        local b = chamberPositions[i + 1]
        -- Horizontal passage
        carveHLine(matrix, math.min(a.x, b.x), math.max(a.x, b.x),
                   math.floor((a.y + b.y) / 2), 1, w, h)
        -- Vertical adjustment if needed
        if math.abs(a.y - b.y) > 3 then
            local midX = math.floor((a.x + b.x) / 2)
            carveVLine(matrix, math.min(a.y, b.y), math.max(a.y, b.y), midX, 1, w, h)
        end
    end
end

--- "elevatorShaft" – Tall vertical room with platforms on alternating sides,
--- designed for vertical traversal. Kirby elevator pattern.
function patterns.elevatorShaft(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 123

    -- All air
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            matrix:set(x, y, TILE_AIR)
        end
    end

    -- Irregular side walls (not straight vertical lines)
    for y = 2, h - 1 do
        local leftWallW = 2 + math.floor(math.abs(smoothNoise(y, seed, 4)) * 3)
        local rightWallW = 2 + math.floor(math.abs(smoothNoise(y, seed + 50, 4)) * 3)
        for x = 1, clamp(leftWallW, 1, math.floor(w * 0.3)) do
            matrix:set(x, y, material)
        end
        for x = clamp(w - rightWallW + 1, math.floor(w * 0.7), w), w do
            matrix:set(x, y, material)
        end
    end

    -- Floor
    placePlatform(matrix, 2, w - 1, h - 1, material, w, h)
    placePlatform(matrix, 2, w - 1, h, material, w, h)

    -- Alternating platforms going up
    local numPlats = math.floor(h / 5)
    for i = 1, numPlats do
        local py = h - 3 - i * 4
        if py < 4 then break end
        local goLeft = (i % 2 == 1)
        local px1, px2
        if goLeft then
            px1 = 4
            px2 = math.floor(w * 0.55)
        else
            px1 = math.floor(w * 0.45)
            px2 = w - 4
        end
        -- Slightly curved platform
        placeArchPlatform(matrix, px1, px2, py - 1, py, material, w, h)
    end
end

--- "pitOfPits" – Kirby "pit of pits": large gap with narrow cliff islands.
function patterns.pitOfPits(matrix, exits, opts)
    local w, h = matrix:size()
    opts = opts or {}
    local material = opts.material or TILE_SOLID
    local seed = opts.seed or 55

    -- All air
    for y = 2, h - 1 do
        for x = 2, w - 1 do
            matrix:set(x, y, TILE_AIR)
        end
    end

    -- Left and right ground platforms (safe zones)
    local floorY = h - 5
    local leftEnd = math.floor(w * 0.15)
    local rightStart = math.floor(w * 0.85)
    for x = 2, leftEnd do
        for y = floorY, h do
            matrix:set(x, y, material)
        end
    end
    for x = rightStart, w - 1 do
        for y = floorY, h do
            matrix:set(x, y, material)
        end
    end

    -- Narrow cliff islands in the pit
    local numIslands = 4 + math.floor(math.abs(smoothNoise(1, seed, 3)) * 2)
    for i = 1, numIslands do
        local ix = math.floor(leftEnd + (rightStart - leftEnd) * i / (numIslands + 1))
        local islandW = 2 + math.floor(math.abs(smoothNoise(ix, seed + 100, 3)) * 2)
        local islandY = floorY - math.floor(smoothNoise(ix, seed + 200, 4) * 3)
        -- Small column island
        for x = clamp(ix - islandW, 3, w - 2), clamp(ix + islandW, 3, w - 2) do
            for y = clamp(islandY, 4, h - 2), clamp(islandY + 2, 4, h) do
                matrix:set(x, y, material)
            end
        end
    end
end

--------------------------------------------------------------------------------
-- Pattern Registry
--------------------------------------------------------------------------------

--- All registered pattern names grouped by category.
patterns.CATEGORIES = {
    path      = { "serpentine", "zigzag", "loop", "branching", "spiral" },
    arena     = { "openArena", "tieredArena", "circularArena" },
    challenge = { "gauntlet", "staircase", "precisionGaps" },
    puzzle    = { "dividedChambers", "switchMaze", "switchAndRun", "doorLabyrinth" },
    boss      = { "kirbyBossArena", "kirbyFinalBossArena", "bossCorridorApproach",
                  "normalBossArena", "multiBossArena", "asrielGodBossArena" },
    hub       = { "crossroads", "starHub" },
    kirbyTerrain = { "greenGreens", "castleDedede", "iceCave", "rollingHills",
                     "bomberCorridor", "boulderRun", "crumblingCliffs",
                     "elevatorShaft", "pitOfPits" },
}

--- Flat list of all pattern names.
patterns.ALL_NAMES = {}
for _, names in pairs(patterns.CATEGORIES) do
    for _, n in ipairs(names) do
        table.insert(patterns.ALL_NAMES, n)
    end
end
table.sort(patterns.ALL_NAMES)

--- Get pattern names for a given category.
function patterns.namesForCategory(cat)
    return patterns.CATEGORIES[cat] or {}
end

--- Get category of a pattern name.
function patterns.categoryOf(name)
    for cat, names in pairs(patterns.CATEGORIES) do
        for _, n in ipairs(names) do
            if n == name then return cat end
        end
    end
    return nil
end

--------------------------------------------------------------------------------
-- Application API
--------------------------------------------------------------------------------

--- Apply a named pattern to a tile matrix.
-- @param matrix      Tile matrix (already filled by Markov or filled solid)
-- @param patternName String name of the pattern (e.g. "serpentine")
-- @param exits       Exit config table { left=bool, right=bool, top=bool, bottom=bool }
-- @param opts        Optional settings for the pattern
-- @return boolean    true if pattern was found and applied
function patterns.apply(matrix, patternName, exits, opts)
    local fn = patterns[patternName]
    if type(fn) ~= "function" then
        return false
    end
    fn(matrix, exits, opts)
    return true
end

--- Pick a random pattern from a category (or any category if nil).
-- @param category   Category name or nil for random
-- @return string    Pattern name
function patterns.randomFrom(category)
    local pool
    if category and patterns.CATEGORIES[category] then
        pool = patterns.CATEGORIES[category]
    else
        pool = patterns.ALL_NAMES
    end
    return pool[math.random(1, #pool)]
end

--- Suggest a difficulty-appropriate pattern.
-- @param difficulty  Float 0.0 (easy) to 1.0 (hardest)
-- @param isBoss      Boolean — is this a boss room?
-- @param isKirby     Boolean — Kirby mode?
-- @return string     Pattern name
function patterns.suggestPattern(difficulty, isBoss, isKirby)
    if isBoss then
        if isKirby then
            if difficulty >= 0.7 then
                return "kirbyFinalBossArena"
            else
                return "kirbyBossArena"
            end
        else
            if difficulty >= 0.7 then
                return "normalBossArena"
            elseif difficulty >= 0.4 then
                return "bossCorridorApproach"
            else
                return "normalBossArena"
            end
        end
    end

    -- Kirby non-boss rooms: prefer organic terrain patterns
    if isKirby then
        if difficulty >= 0.7 then
            local pick = math.random()
            if pick < 0.4 then return patterns.randomFrom("kirbyTerrain")
            elseif pick < 0.7 then return patterns.randomFrom("challenge")
            else return "bomberCorridor"
            end
        elseif difficulty >= 0.4 then
            local pick = math.random()
            if pick < 0.6 then return patterns.randomFrom("kirbyTerrain")
            elseif pick < 0.85 then return patterns.randomFrom("puzzle")
            else return patterns.randomFrom("path")
            end
        else
            local pick = math.random()
            if pick < 0.7 then return patterns.randomFrom("kirbyTerrain")
            else return patterns.randomFrom("path")
            end
        end
    end

    if difficulty >= 0.8 then
        return patterns.randomFrom("challenge")
    elseif difficulty >= 0.5 then
        local pick = math.random()
        if pick < 0.4 then return patterns.randomFrom("path")
        elseif pick < 0.7 then return patterns.randomFrom("challenge")
        else return patterns.randomFrom("puzzle")
        end
    else
        local pick = math.random()
        if pick < 0.5 then return patterns.randomFrom("path")
        elseif pick < 0.8 then return patterns.randomFrom("hub")
        else return patterns.randomFrom("arena")
        end
    end
end

--------------------------------------------------------------------------------
-- Entity Template System
-- Generates entity placement tables for each pattern type.
-- Entities are positioned relative to room origin (0,0);
-- callers offset to world coordinates.
--------------------------------------------------------------------------------

--- Kirby mid-boss type enum mirrors C# KirbyMidBoss.MidBossType
patterns.KIRBY_BOSS_TYPES = {
    WhispyWoods = 0,
    Kracko      = 1,
    MrFrosty    = 2,
    Bonkers     = 3,
    Bugzzy      = 4,
    FireLion    = 5,
    IronMam     = 6,
    GrandWheely = 7,
    BoxBoxer    = 8,
    MasterHand  = 9,
    Custom      = 10,
}

--- Boss tier mapping for normal player bosses
patterns.BOSS_TIERS = {
    "MaggyHelper/BossTier1",
    "MaggyHelper/BossTier2",
    "MaggyHelper/BossTier3",
    "MaggyHelper/BossTier4",
    "MaggyHelper/BossTier5",
}

--- Helper: create a single entity table
local function makeEntity(id, name, x, y, extra)
    local e = {
        _name = name,
        id = id,
        x = x,
        y = y,
        width = 0,
        height = 0,
        _type = "entity",
    }
    if extra then
        for k, v in pairs(extra) do e[k] = v end
    end
    return e
end

--- Generate entities for a Kirby mid-boss arena pattern.
-- @param roomW     Room width in tiles
-- @param roomH     Room height in tiles
-- @param bossType  MidBossType integer (0-10), nil for random
-- @param difficulty 0.0-1.0
-- @return table    Array of entity tables
function patterns.kirbyBossEntities(roomW, roomH, bossType, difficulty)
    difficulty = difficulty or 0.5
    bossType = bossType or math.random(0, 9)

    local entities = {}
    local id = 1
    local w8 = roomW * 8
    local h8 = roomH * 8

    -- Player spawn (left side)
    table.insert(entities, makeEntity(id, "player", 32, h8 - 48)); id = id + 1

    -- Kirby mid-boss (center-right)
    local bossX = math.floor(w8 * 0.65)
    local bossY = h8 - 56  -- above floor
    local arenaW = math.floor(w8 * 0.7)
    local arenaH = math.floor(h8 * 0.6)
    table.insert(entities, makeEntity(id, "MaggyHelper/KirbyMidBoss", bossX, bossY, {
        bossType = bossType,
        arenaWidth = arenaW,
        arenaHeight = arenaH,
        health = math.floor(3 + difficulty * 7),        -- 3-10 HP
        attackCooldown = math.max(0.5, 2.0 - difficulty), -- faster at high difficulty
    })); id = id + 1

    -- Boss intro trigger (full room)
    table.insert(entities, makeEntity(id, "MaggyHelper/BossIntroTrigger", 0, 0, {
        width = w8,
        height = h8,
        _type = "trigger",
    })); id = id + 1

    -- Refill crystal near player
    table.insert(entities, makeEntity(id, "refill", 64, h8 - 72, {
        oneUse = false, twoDash = difficulty >= 0.7,
    })); id = id + 1

    -- Hazard spikes on ceiling at higher difficulty
    if difficulty >= 0.5 then
        local spikeCount = math.floor(3 + difficulty * 5)
        local spacing = math.floor((w8 - 64) / (spikeCount + 1))
        for i = 1, spikeCount do
            table.insert(entities, makeEntity(id, "spikesDown", 32 + i * spacing, 32, {
                width = 8, ["type"] = "default",
            })); id = id + 1
        end
    end

    -- Side wall spikes at very high difficulty
    if difficulty >= 0.8 then
        for y = 48, h8 - 64, 24 do
            table.insert(entities, makeEntity(id, "spikesRight", 28, y, {
                height = 8, ["type"] = "default",
            })); id = id + 1
        end
    end

    return entities
end

--- Generate entities for a Kirby final boss arena.
-- @param roomW     Room width in tiles
-- @param roomH     Room height in tiles
-- @param difficulty 0.0-1.0
-- @return table    Array of entity tables
function patterns.kirbyFinalBossEntities(roomW, roomH, difficulty)
    difficulty = difficulty or 0.8

    local entities = {}
    local id = 1
    local w8 = roomW * 8
    local h8 = roomH * 8

    -- Player spawn
    table.insert(entities, makeEntity(id, "player", 40, h8 - 48)); id = id + 1

    -- Full boss center
    local bossX = math.floor(w8 / 2)
    local bossY = h8 - 80
    table.insert(entities, makeEntity(id, "MaggyHelper/FullBoss", bossX, bossY, {
        width = 32, height = 48,
        health = math.floor(10 + difficulty * 15),
        phases = 3,
    })); id = id + 1

    -- Boss intro trigger
    table.insert(entities, makeEntity(id, "MaggyHelper/BossIntroTrigger", 0, 0, {
        width = w8, height = h8, _type = "trigger",
    })); id = id + 1

    -- Multiple refills
    local refillPositions = {
        { 48, h8 - 72 },
        { w8 - 56, h8 - 72 },
        { math.floor(w8 / 2), math.floor(h8 * 0.35) },
    }
    for _, pos in ipairs(refillPositions) do
        table.insert(entities, makeEntity(id, "refill", pos[1], pos[2], {
            oneUse = false, twoDash = true,
        })); id = id + 1
    end

    -- Springs on side platforms
    table.insert(entities, makeEntity(id, "spring", 48, h8 - math.floor(h8 * 0.6) - 8, {
        orientation = 0,
    })); id = id + 1
    table.insert(entities, makeEntity(id, "spring", w8 - 56, h8 - math.floor(h8 * 0.6) - 8, {
        orientation = 0,
    })); id = id + 1

    -- Ceiling spikes
    local spikeCount = math.floor(4 + difficulty * 8)
    local spacing = math.floor((w8 - 48) / (spikeCount + 1))
    for i = 1, spikeCount do
        table.insert(entities, makeEntity(id, "spikesDown", 24 + i * spacing, 24, {
            width = 8, ["type"] = "default",
        })); id = id + 1
    end

    -- Kirby damage trigger zones at edges
    table.insert(entities, makeEntity(id, "MaggyHelper/KirbyDamageTrigger", 0, h8 - 8, {
        width = w8, height = 8, damage = 1, _type = "trigger",
    })); id = id + 1

    return entities
end

--- Generate entities for a normal player boss room.
-- @param roomW     Room width in tiles
-- @param roomH     Room height in tiles
-- @param bossTier  1-5 boss tier
-- @param difficulty 0.0-1.0
-- @return table    Array of entity tables
function patterns.normalBossEntities(roomW, roomH, bossTier, difficulty)
    difficulty = difficulty or 0.5
    bossTier = clamp(bossTier or math.ceil(difficulty * 5), 1, 5)

    local entities = {}
    local id = 1
    local w8 = roomW * 8
    local h8 = roomH * 8

    -- Player spawn
    table.insert(entities, makeEntity(id, "player", 32, h8 - 48)); id = id + 1

    -- Boss entity (tier-based)
    local bossName = patterns.BOSS_TIERS[bossTier]
    local bossX = math.floor(w8 * 0.7)
    local bossY = h8 - 64
    table.insert(entities, makeEntity(id, bossName, bossX, bossY, {
        width = 24, height = 32,
    })); id = id + 1

    -- Refill
    table.insert(entities, makeEntity(id, "refill", 64, h8 - 72, {
        oneUse = false, twoDash = bossTier >= 4,
    })); id = id + 1

    -- Boosters at higher tiers
    if bossTier >= 3 then
        table.insert(entities, makeEntity(id, "booster", math.floor(w8 * 0.3), math.floor(h8 * 0.4), {
            red = bossTier >= 4,
        })); id = id + 1
    end

    -- Spinners as hazards at higher tiers
    if bossTier >= 2 then
        local spinnerCount = bossTier
        for i = 1, spinnerCount do
            local sx = math.floor(w8 * (0.2 + 0.6 * i / (spinnerCount + 1)))
            local sy = math.floor(h8 * 0.25) + ((i % 2 == 0) and 16 or 0)
            table.insert(entities, makeEntity(id, "spinner", sx, sy, {
                attachToSolid = false,
            })); id = id + 1
        end
    end

    return entities
end

--- Generate entities for the multi-boss arena (Kirby side + normal side).
-- @param roomW      Room width in tiles
-- @param roomH      Room height in tiles
-- @param kirbyBossType  Kirby mid-boss type (0-10)
-- @param bossTier   Normal player boss tier (1-5)
-- @param difficulty 0.0-1.0
-- @return table     Array of entity tables
function patterns.multiBossEntities(roomW, roomH, kirbyBossType, bossTier, difficulty)
    difficulty = difficulty or 0.6
    kirbyBossType = kirbyBossType or math.random(0, 9)
    bossTier = clamp(bossTier or 3, 1, 5)

    local entities = {}
    local id = 1
    local w8 = roomW * 8
    local h8 = roomH * 8
    local split = math.floor(w8 / 2)

    -- Player spawn (center, can go either way)
    table.insert(entities, makeEntity(id, "player", split, h8 - 48)); id = id + 1

    -- Kirby mid-boss (left half)
    table.insert(entities, makeEntity(id, "MaggyHelper/KirbyMidBoss",
        math.floor(split * 0.5), h8 - 56, {
        bossType = kirbyBossType,
        arenaWidth = split - 40,
        arenaHeight = math.floor(h8 * 0.6),
        health = math.floor(3 + difficulty * 5),
    })); id = id + 1

    -- Normal boss (right half)
    local bossName = patterns.BOSS_TIERS[clamp(bossTier, 1, 5)]
    table.insert(entities, makeEntity(id, bossName,
        split + math.floor((w8 - split) * 0.6), h8 - 64, {
        width = 24, height = 32,
    })); id = id + 1

    -- Refills on both sides
    table.insert(entities, makeEntity(id, "refill", 48, h8 - 72, {
        oneUse = false, twoDash = false,
    })); id = id + 1
    table.insert(entities, makeEntity(id, "refill", w8 - 56, h8 - 72, {
        oneUse = false, twoDash = difficulty >= 0.7,
    })); id = id + 1

    -- Boss intro trigger
    table.insert(entities, makeEntity(id, "MaggyHelper/BossIntroTrigger", 0, 0, {
        width = w8, height = h8, _type = "trigger",
    })); id = id + 1

    return entities
end

--- Generate entities for the Asriel God Boss arena.
-- @param roomW       Room width in tiles
-- @param roomH       Room height in tiles
-- @param phase       Boss phase (1, 2, or 3)
-- @param difficulty  0.0-1.0
-- @return table      Array of entity tables
function patterns.asrielGodBossEntities(roomW, roomH, phase, difficulty)
    difficulty = difficulty or 0.7
    phase = phase or 1

    local entities = {}
    local id = 1
    local w8 = roomW * 8
    local h8 = roomH * 8
    local cx = math.floor(w8 / 2)
    local floorY = h8 - 40

    -- Player spawn (left side)
    table.insert(entities, makeEntity(id, "player", 40, floorY)); id = id + 1

    -- Kirby spawn point
    table.insert(entities, makeEntity(id, "MaggyHelper/KirbySpawnPoint", 56, floorY, {
        spawnAsKirby = true,
        startingAbility = "Sword",
    })); id = id + 1

    -- Asriel God Boss (position and stats vary by phase)
    local bossY = math.floor(h8 * 0.2) - (phase - 1) * 16
    local bossHealth = 300 + (phase - 1) * 350  -- 300, 650, 1000
    local patternIndex = (phase - 1) * 10  -- Different attack patterns per phase

    local attackSequences = {
        [1] = "Shoot,Beam,StarstormRain,BladeThrower",
        [2] = "BiggerBeam,RainbowBlackhole,ChaosBlaster,LightningStorm,HyperGoner",
        [3] = "HyperGoner,EternalChaos,GalacticSaber,DimensionalRift,RainbowInferno,CelestialSpears,TimewarpVortex,PrismBurst,SoulResonance",
    }

    table.insert(entities, makeEntity(id, "MaggyHelper/AsrielGodBoss", cx, bossY, {
        patternIndex = patternIndex,
        cameraPastY = 120 + phase * 30,
        dialog = phase == 1,  -- Only dialog on first phase
        startHit = phase > 1,
        cameraLockY = true,
        health = bossHealth,
        maxHealth = bossHealth,
        attackSequence = attackSequences[phase] or attackSequences[1],
    })); id = id + 1

    -- Boss movement nodes (more for higher phases)
    if phase >= 2 then
        local nodeCount = phase + 1
        for i = 1, nodeCount do
            local nodeX = math.floor(w8 * (0.15 + 0.7 * (i - 1) / (nodeCount - 1)))
            local nodeY = math.floor(h8 * (0.15 + (i % 2) * 0.15))
            -- Nodes would be added as children of the boss entity in real usage
        end
    end

    -- Start hit trigger (activates boss)
    if phase == 1 then
        table.insert(entities, makeEntity(id, "MaggyHelper/AsrielStartHitTrigger", 0, 0, {
            width = w8, height = h8, _type = "trigger",
        })); id = id + 1

        -- Identity reveal cutscene trigger
        table.insert(entities, makeEntity(id, "MaggyHelper/EventTrigger", 56, 0, {
            width = 80, height = h8,
            eventName = "ch20_asriel_god_boss_identity_reveal",
            _type = "trigger",
        })); id = id + 1
    end

    -- Refills (more and better positioned for higher phases)
    local refillCount = 2 + phase
    local refillPositions = {}
    table.insert(refillPositions, { 56, floorY - 24 })
    table.insert(refillPositions, { w8 - 64, floorY - 24 })
    if phase >= 2 then
        table.insert(refillPositions, { cx, math.floor(h8 * 0.55) - 16 })
    end
    if phase >= 3 then
        table.insert(refillPositions, { math.floor(w8 * 0.25), math.floor(h8 * 0.35) - 8 })
        table.insert(refillPositions, { math.floor(w8 * 0.75), math.floor(h8 * 0.35) - 8 })
    end
    for _, pos in ipairs(refillPositions) do
        table.insert(entities, makeEntity(id, "refill", pos[1], pos[2], {
            oneUse = false, twoDash = true,
        })); id = id + 1
    end

    -- Springs for verticality
    table.insert(entities, makeEntity(id, "spring", 72, floorY - 8, {
        orientation = 0,
    })); id = id + 1
    table.insert(entities, makeEntity(id, "spring", w8 - 80, floorY - 8, {
        orientation = 0,
    })); id = id + 1

    -- Boosters at higher phases
    if phase >= 2 then
        table.insert(entities, makeEntity(id, "booster", math.floor(w8 * 0.2), math.floor(h8 * 0.5), {
            red = true,
        })); id = id + 1
        table.insert(entities, makeEntity(id, "booster", math.floor(w8 * 0.8), math.floor(h8 * 0.5), {
            red = true,
        })); id = id + 1
    end

    -- Dream blocks for phase 3 mobility
    if phase >= 3 then
        local dreamPositions = {
            { cx - 96, math.floor(h8 * 0.3) },
            { cx + 64, math.floor(h8 * 0.3) },
            { cx, math.floor(h8 * 0.65) },
        }
        for _, pos in ipairs(dreamPositions) do
            table.insert(entities, makeEntity(id, "dreamBlock", pos[1], pos[2], {
                width = 32, height = 24,
                featherMode = true,
            })); id = id + 1
        end
    end

    -- Starfield backdrop trigger
    if phase >= 2 then
        table.insert(entities, makeEntity(id, "MaggyHelper/AsrielGodBossStarfieldTrigger", 0, 0, {
            width = w8, height = h8,
            enable = true,
            intensityMax = 0.5 + phase * 0.4,
            _type = "trigger",
        })); id = id + 1
    end

    -- Hazards scale with difficulty and phase
    if difficulty >= 0.5 or phase >= 2 then
        local spikeCount = math.floor(2 + difficulty * 4 + phase)
        local spacing = math.floor((w8 - 64) / (spikeCount + 1))
        for i = 1, spikeCount do
            table.insert(entities, makeEntity(id, "spikesDown", 32 + i * spacing, 24, {
                width = 8, ["type"] = "default",
            })); id = id + 1
        end
    end

    -- Kill zone at very bottom for phase 3
    if phase >= 3 then
        table.insert(entities, makeEntity(id, "killbox", 0, h8 - 8, {
            width = w8, height = 8,
        })); id = id + 1
    end

    return entities
end

--- Generate entities for a challenge gauntlet room.
-- @param roomW     Room width in tiles
-- @param roomH     Room height in tiles
-- @param difficulty 0.0-1.0
-- @return table    Array of entity tables
function patterns.challengeEntities(roomW, roomH, difficulty)
    difficulty = difficulty or 0.5
    local entities = {}
    local id = 1
    local w8 = roomW * 8
    local h8 = roomH * 8

    -- Player spawn
    table.insert(entities, makeEntity(id, "player", 24, math.floor(h8 / 2))); id = id + 1

    -- Refills along the path
    local refillCount = math.floor(2 + difficulty * 3)
    local spacing = math.floor((w8 - 48) / (refillCount + 1))
    for i = 1, refillCount do
        table.insert(entities, makeEntity(id, "refill", 24 + i * spacing, math.floor(h8 / 2) - 16, {
            oneUse = difficulty >= 0.7,
            twoDash = difficulty >= 0.8,
        })); id = id + 1
    end

    -- Spinners scattered along path
    local spinnerCount = math.floor(3 + difficulty * 8)
    local spinSpacing = math.floor((w8 - 64) / (spinnerCount + 1))
    for i = 1, spinnerCount do
        local sx = 32 + i * spinSpacing
        local sy = math.floor(h8 / 2) + ((i % 3 == 0) and -20 or (i % 3 == 1) and 0 or 20)
        table.insert(entities, makeEntity(id, "spinner", sx, sy, {
            attachToSolid = false,
        })); id = id + 1
    end

    -- Springs to help navigate
    if difficulty < 0.6 then
        for i = 1, 2 do
            local sx = math.floor(w8 * i / 3)
            table.insert(entities, makeEntity(id, "spring", sx, math.floor(h8 / 2) + 16, {
                orientation = 0,
            })); id = id + 1
        end
    end

    return entities
end

--- Generate entities for a puzzle (divided chambers) room.
-- @param roomW     Room width in tiles
-- @param roomH     Room height in tiles
-- @param chambers  Number of chambers
-- @param difficulty 0.0-1.0
-- @return table    Array of entity tables
function patterns.puzzleEntities(roomW, roomH, chambers, difficulty)
    difficulty = difficulty or 0.4
    chambers = chambers or 3
    local entities = {}
    local id = 1
    local w8 = roomW * 8
    local h8 = roomH * 8

    -- Player spawn in first chamber
    table.insert(entities, makeEntity(id, "player", 32, math.floor(h8 / 2))); id = id + 1

    local chamberW = math.floor(w8 / chambers)

    for c = 1, chambers do
        local cx = (c - 1) * chamberW + math.floor(chamberW / 2)

        -- Touch switch in each chamber (except last = gate target)
        if c < chambers then
            table.insert(entities, makeEntity(id, "touchSwitch", cx, math.floor(h8 * 0.3)))
            id = id + 1
        end

        -- Refill in each chamber
        table.insert(entities, makeEntity(id, "refill", cx + 16, math.floor(h8 * 0.6), {
            oneUse = false, twoDash = false,
        })); id = id + 1

        -- Hazards at higher difficulty
        if difficulty >= 0.5 and c > 1 then
            table.insert(entities, makeEntity(id, "spinner", cx - 16, math.floor(h8 * 0.45), {
                attachToSolid = false,
            })); id = id + 1
        end
    end

    -- Switch gate in the last chamber
    local lastCX = (chambers - 1) * chamberW + math.floor(chamberW / 2)
    table.insert(entities, makeEntity(id, "switchGate", lastCX, math.floor(h8 / 2), {
        width = 16, height = 16,
    })); id = id + 1

    return entities
end

--- Generate entities for a hub/crossroads room.
-- @param roomW     Room width in tiles
-- @param roomH     Room height in tiles
-- @param exits     Exit config table
-- @return table    Array of entity tables
function patterns.hubEntities(roomW, roomH, exits)
    local entities = {}
    local id = 1
    local w8 = roomW * 8
    local h8 = roomH * 8

    -- Player spawn at center
    local cx = math.floor(w8 / 2)
    local cy = math.floor(h8 / 2)
    table.insert(entities, makeEntity(id, "player", cx, cy)); id = id + 1

    -- Central refill
    table.insert(entities, makeEntity(id, "refill", cx, cy - 16, {
        oneUse = false, twoDash = true,
    })); id = id + 1

    -- Springs pointing toward each exit
    if exits and exits.left then
        table.insert(entities, makeEntity(id, "spring", 48, cy, {
            orientation = 2, -- left-facing
        })); id = id + 1
    end
    if exits and exits.right then
        table.insert(entities, makeEntity(id, "spring", w8 - 56, cy, {
            orientation = 3, -- right-facing
        })); id = id + 1
    end
    if exits and exits.top then
        table.insert(entities, makeEntity(id, "spring", cx, 48, {
            orientation = 0,
        })); id = id + 1
    end
    if exits and exits.bottom then
        table.insert(entities, makeEntity(id, "spring", cx, h8 - 48, {
            orientation = 0,
        })); id = id + 1
    end

    return entities
end

--------------------------------------------------------------------------------
-- Unified entity template dispatch
--------------------------------------------------------------------------------

--- Get entity templates for a pattern, matching its layout.
-- @param patternName  Pattern name string
-- @param roomW        Room width in tiles
-- @param roomH        Room height in tiles
-- @param opts         Options: difficulty, bossType, bossTier, exits, chambers, isKirby, phase
-- @return table       Array of entity tables (positions relative to 0,0)
function patterns.getEntities(patternName, roomW, roomH, opts)
    opts = opts or {}
    local difficulty = opts.difficulty or 0.5
    local cat = patterns.categoryOf(patternName)

    if patternName == "kirbyBossArena" then
        return patterns.kirbyBossEntities(roomW, roomH, opts.bossType, difficulty)
    elseif patternName == "kirbyFinalBossArena" then
        return patterns.kirbyFinalBossEntities(roomW, roomH, difficulty)
    elseif patternName == "asrielGodBossArena" then
        return patterns.asrielGodBossEntities(roomW, roomH, opts.phase or 1, difficulty)
    elseif patternName == "normalBossArena" or patternName == "bossCorridorApproach" then
        return patterns.normalBossEntities(roomW, roomH, opts.bossTier, difficulty)
    elseif patternName == "multiBossArena" then
        return patterns.multiBossEntities(roomW, roomH, opts.bossType, opts.bossTier, difficulty)
    elseif cat == "challenge" then
        return patterns.challengeEntities(roomW, roomH, difficulty)
    elseif cat == "puzzle" then
        return patterns.puzzleEntities(roomW, roomH, opts.chambers or 3, difficulty)
    elseif cat == "hub" then
        return patterns.hubEntities(roomW, roomH, opts.exits)
    end

    -- Default: no special entities (generic generator handles it)
    return nil
end

return patterns
