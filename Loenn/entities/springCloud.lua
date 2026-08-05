local drawableSpriteStruct = require("structs.drawable_sprite")

local springCloud = {}

springCloud.name = "DZ/SpringCloud"
springCloud.depth = 0
springCloud.placements = {
    {
        name = "red_cloud",
        data = {
            fragile = false,
            small = false
        }
    },
    {
        name = "red_cloud_fragile",
        data = {
            fragile = true,
            small = false
        }
    }
}

local normalScale = 1.0
local smallScale = 29 / 35

local function getTexture(entity)
    local fragile = entity.fragile

    if fragile then
        return "objects/DZ/clouds/fragile00"

    else
        return "objects/DZ/clouds/cloud00"
    end
end

function springCloud.sprite(room, entity)
    local texture = getTexture(entity)
    local sprite = drawableSpriteStruct.fromTexture(texture, entity)
    local small = entity.small
    local scale = small and smallScale or normalScale

    sprite:setScale(scale, 1.0)

    return sprite
end

return springCloud