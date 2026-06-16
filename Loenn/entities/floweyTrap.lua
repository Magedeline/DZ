local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local floweyTrap = {}

floweyTrap.name = "DZ/FloweyTrap"
floweyTrap.depth = -100
floweyTrap.fieldInformation = {
    health = {
        minimumValue = 1
    },
    detectionRange = {
        minimumValue = 0
    },
    retractRange = {
        minimumValue = 0
    },
    pelletCount = {
        minimumValue = 1
    },
    pelletSpeed = {
        minimumValue = 0
    },
    attackInterval = {
        minimumValue = 0
    },
    attackPattern = {
        options = {"Circular", "Aimed", "Spread", "Spiral"},
        editable = false
    }
}
floweyTrap.placements = {
    {
        name = "normal",
        data = {
            health = 2,
            detectionRange = 120,
            retractRange = 180,
            pelletCount = 5,
            pelletSpeed = 150,
            attackInterval = 1.5,
            attackPattern = "Circular"
        }
    }
}

function floweyTrap.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/flowey_trap/hidden", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function floweyTrap.selection(room, entity)
    return utils.rectangle(entity.x - 10, entity.y - 20, 20, 20)
end

return floweyTrap
