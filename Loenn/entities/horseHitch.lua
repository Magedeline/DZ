local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local horseHitch = {}

horseHitch.name = "DZ/HorseHitch"
horseHitch.depth = -100
horseHitch.fieldInformation = {}
horseHitch.placements = {
    {
        name = "locked",
        data = {
            hitchId = "",
            destinationId = "",
            isUnlocked = false
        }
    },
    {
        name = "unlocked",
        data = {
            hitchId = "",
            destinationId = "",
            isUnlocked = true
        }
    }
}

function horseHitch.sprite(room, entity)
    local state = entity.isUnlocked and "active" or "inactive"
    local sprite = drawableSprite.fromTexture("objects/horse_hitch/" .. state, entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function horseHitch.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 40, 24, 40)
end

return horseHitch
