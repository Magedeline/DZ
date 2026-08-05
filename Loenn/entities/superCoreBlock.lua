local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local superCoreBlock = {}

superCoreBlock.name = "DZ/SuperCoreBlock"
superCoreBlock.depth = 8990
superCoreBlock.warnBelowSize = {16, 16}
superCoreBlock.placements = {
    {
        name = "super_fire",
        alternativeName = "super_fire_bounce",
        data = {
            width = 16,
            height = 16,
            notCoreMode = false,
            speedMultiplier = 3,
            launchRange = 400,
            requiresCoreMode = false,
            hotColor = "FF4500",
            coldColor = "00BFFF",
            superColor = "FFD700"
        }
    },
    {
        name = "super_ice",
        data = {
            width = 16,
            height = 16,
            notCoreMode = true,
            speedMultiplier = 3,
            launchRange = 400,
            requiresCoreMode = false,
            hotColor = "FF4500",
            coldColor = "00BFFF",
            superColor = "FFD700"
        }
    },
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local fireBlockTexture = "objects/DZ/BumpBlockNew/fire00"
local fireCrystalTexture = "objects/DZ/BumpBlockNew/fire_center00"

local iceBlockTexture = "objects/DZ/BumpBlockNew/ice00"
local iceCrystalTexture = "objects/DZ/BumpBlockNew/ice_center00"

local function getBlockTexture(entity)
    if entity.notCoreMode then
        return iceBlockTexture, iceCrystalTexture

    else
        return fireBlockTexture, fireCrystalTexture
    end
end

function superCoreBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockTexture, crystalTexture = getBlockTexture(entity)

    local ninePatch = drawableNinePatch.fromTexture(blockTexture, ninePatchOptions, x, y, width, height)
    local crystalSprite = drawableSprite.fromTexture(crystalTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    crystalSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, crystalSprite)

    return sprites
end

return superCoreBlock