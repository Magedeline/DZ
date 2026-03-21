-- PCG BSP Generator — Binary Space Partitioning layout for Celeste rooms
--
-- Generates precision-platformer-friendly layouts by recursively splitting a
-- room into sub-regions and placing horizontal platforms rather than enclosing
-- dungeon walls.  The result feels like a slice of a larger world rather than
-- a sealed chamber.
--
-- Usage (from pcg/init.lua or scripts):
--   local bsp = require("libraries.pcg.bsp_generator")
--   local matrix = bsp.generate(roomW, roomH, opts, matrixLib)

local bsp = {}

local randomUtils

local function getRandomUtils()
    if not randomUtils then
        local ok, mods = pcall(require, "mods")
        if ok and mods and mods.requireFromPlugin then
            randomUtils = mods.requireFromPlugin("libraries.pcg.random_utils")
        end
        if not randomUtils then
            randomUtils = require("libraries.pcg.random_utils")
        end
    end
    return randomUtils
end

--------------------------------------------------------------------------------
-- BSP Node
--------------------------------------------------------------------------------

local function newNode(x, y, w, h)
    return { x = x, y = y, w = w, h = h, left = nil, right = nil }
end

--- Split a node horizontally or vertically.
-- Returns true if split was performed.
local function splitNode(node, minSize, seed)
    if node.left or node.right then return false end  -- already split

    local splitH = math.random() > 0.5  -- horizontal split

    -- Enforce minimum size
    if splitH then
        if node.h < minSize * 2 then splitH = false end
    end
    if not splitH then
        if node.w < minSize * 2 then
            if node.h >= minSize * 2 then
                splitH = true
            else
                return false  -- too small to split either way
            end
        end
    end

    local maxSize
    if splitH then
        maxSize = node.h - minSize
        if maxSize <= minSize then return false end
        local range = maxSize - minSize
        local splitAt = minSize + (range > 0 and math.random(range) or 0)
        node.left  = newNode(node.x, node.y,          node.w, splitAt)
        node.right = newNode(node.x, node.y + splitAt, node.w, node.h - splitAt)
    else
        maxSize = node.w - minSize
        if maxSize <= minSize then return false end
        local range = maxSize - minSize
        local splitAt = minSize + (range > 0 and math.random(range) or 0)
        node.left  = newNode(node.x,            node.y, splitAt,          node.h)
        node.right = newNode(node.x + splitAt,  node.y, node.w - splitAt, node.h)
    end

    return true
end

--- Recursively split a tree of nodes up to `depth` levels.
local function buildTree(node, depth, minSize)
    if depth <= 0 then return end
    if splitNode(node, minSize) then
        buildTree(node.left,  depth - 1, minSize)
        buildTree(node.right, depth - 1, minSize)
    end
end

--- Collect all leaf nodes of the BSP tree.
local function getLeaves(node, leaves)
    leaves = leaves or {}
    if not node.left and not node.right then
        table.insert(leaves, node)
    else
        if node.left  then getLeaves(node.left,  leaves) end
        if node.right then getLeaves(node.right, leaves) end
    end
    return leaves
end

--------------------------------------------------------------------------------
-- Platform Placement
--------------------------------------------------------------------------------

--- Place a horizontal platform (1–3 tiles thick) in a leaf node.
-- The platform is placed at a random Y within the node, spanning most of its width.
local function placePlatform(matrix, node, material, padding)
    padding = padding or 2
    local nx, ny, nw, nh = node.x, node.y, node.w, node.h

    if nw < padding * 2 + 4 or nh < 3 then return end

    local platformY = ny + math.floor(nh * 0.55) + math.random(0, math.max(0, math.floor(nh * 0.3) - 1))
    local startX    = nx + padding + math.random(0, padding)
    local endX      = nx + nw - padding - math.random(0, padding)
    local thickness = math.random(1, 2)

    if startX >= endX then return end

    local mw, mh = matrix:size()
    for t = 0, thickness - 1 do
        local py = platformY + t
        if py >= 1 and py <= mh then
            for px = startX, endX do
                if px >= 1 and px <= mw then
                    matrix:set(px, py, material)
                end
            end
        end
    end
end

--------------------------------------------------------------------------------
-- Ground Floor
--------------------------------------------------------------------------------

--- Place a solid ground floor in the bottom 2 rows (for a landing surface).
local function placeFloor(matrix, material)
    local w, h = matrix:size()
    for x = 1, w do
        matrix:set(x, h,     material)
        matrix:set(x, h - 1, material)
    end
end

--------------------------------------------------------------------------------
-- BSP Generate
--------------------------------------------------------------------------------

--- Generate a tile matrix using BSP for a precision-platformer layout.
-- The resulting matrix has:
--   - A solid two-tile floor at the bottom
--   - No enclosing top or side walls (open-world feel)
--   - Staggered horizontal platforms generated from BSP leaf nodes
--   - Air everywhere else
--
-- @param roomW      Room width in tiles
-- @param roomH      Room height in tiles
-- @param opts       Options: { material, seed, depth, minLeafSize, padding }
-- @param matrixLib  Lönn matrix library (or nil for standalone)
-- @return matrix    Tile matrix
function bsp.generate(roomW, roomH, opts, matrixLib)
    opts = opts or {}
    local material    = opts.material    or "1"
    local depth       = opts.depth       or 4
    local minLeafSize = opts.minLeafSize or 5
    local padding     = opts.padding     or 2
    local seed        = opts.seed        or os.time()

    math.randomseed(seed)

    -- Build matrix (all air)
    local matrix
    if matrixLib then
        matrix = matrixLib.filled(roomW, roomH, "0")
    else
        -- Standalone fallback: simple 2D array with :get/:set/:size interface
        local data = {}
        for y = 1, roomH do
            data[y] = {}
            for x = 1, roomW do
                data[y][x] = "0"
            end
        end
        matrix = {
            get  = function(_, x, y, def) return (data[y] and data[y][x]) or def or "0" end,
            set  = function(_, x, y, v)
                if data[y] then data[y][x] = v end
            end,
            size = function(_) return roomW, roomH end,
        }
        -- Also support direct call style used by Lönn
        setmetatable(matrix, {
            __index = function(t, k)
                if k == "get"  then return function(x, y, d) return (data[y] and data[y][x]) or d or "0" end end
                if k == "set"  then return function(x, y, v) if data[y] then data[y][x] = v end end end
                if k == "size" then return function() return roomW, roomH end end
            end
        })
    end

    -- BSP tree over the interior (leave 1-tile margin on all sides for exits)
    local root = newNode(2, 2, roomW - 2, roomH - 4)  -- leave 2 rows at bottom for floor
    buildTree(root, depth, minLeafSize)

    -- Place platforms in leaves
    local leaves = getLeaves(root)
    for _, leaf in ipairs(leaves) do
        placePlatform(matrix, leaf, material, padding)
    end

    -- Always place a solid floor
    placeFloor(matrix, material)

    return matrix
end

return bsp
