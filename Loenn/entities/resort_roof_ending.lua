local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local resortRoofEnding = {}

resortRoofEnding.name = "MaggyHelper/RoofTopEnding"
resortRoofEnding.depth = 0
resortRoofEnding.minimumSize = {8, 8}
resortRoofEnding.canResize = {true, false}
resortRoofEnding.placements = {
    name = "resort_roof_ending",
    data = {
        width = 24
    }
}

local startTexture = "decals/3-resort/roofEdge_d"
local endTexture = "decals/3-resort/roofEdge"

local centerTextures = {
    "decals/3-resort/roofCenter",
    "decals/3-resort/roofCenter_b",
    "decals/3-resort/roofCenter_c",
    "decals/3-resort/roofCenter_d"
}

-- Manual offsets and justifications of the sprites
function resortRoofEnding.sprite(room, entity)
    utils.setSimpleCoordinateSeed(entity.x, entity.y)

    local sprites = {}

    local width = entity.width or 8
    local tileSize = 8
    local numTiles = math.max(1, math.floor(width / tileSize))

    -- Start sprite (leftmost)
    local startSprite = drawableSprite.fromTexture(startTexture, entity)
    startSprite:addPosition(0, 4)
    table.insert(sprites, startSprite)

    -- Center sprites (if width > 8)
    for i = 1, numTiles - 2 do
        local texture = centerTextures[math.random(1, #centerTextures)]
        local middleSprite = drawableSprite.fromTexture(texture, entity)
        middleSprite:addPosition(i * tileSize, 4)
        table.insert(sprites, middleSprite)
    end

    -- End sprite (rightmost) - only if width > 8
    if numTiles > 1 then
        local endSprite = drawableSprite.fromTexture(endTexture, entity)
        endSprite:addPosition((numTiles - 1) * tileSize, 4)
        table.insert(sprites, endSprite)
    end

    return sprites
end

function resortRoofEnding.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, math.max(entity.width or 0, 8), 8)
end

return resortRoofEnding