local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local titanStatue = {}

titanStatue.name = "DZ/TitanStatue"
titanStatue.depth = -100
titanStatue.fieldInformation = {
    health = {
        minimumValue = 1
    }
}
titanStatue.placements = {
    {
        name = "inactive",
        data = {
            health = 5,
            canAwaken = false,
            isAnimated = false
        }
    },
    {
        name = "can_awaken",
        data = {
            health = 5,
            canAwaken = true,
            isAnimated = false
        }
    },
    {
        name = "animated",
        data = {
            health = 5,
            canAwaken = true,
            isAnimated = true
        }
    }
}

function titanStatue.sprite(room, entity)
    local state = entity.isAnimated and "active" or "inactive"
    local sprite = drawableSprite.fromTexture("objects/titan_statue/" .. state, entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function titanStatue.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 80, 48, 80)
end

return titanStatue
