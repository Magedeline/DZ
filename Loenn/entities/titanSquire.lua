local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local titanSquire = {}

titanSquire.name = "MaggyHelper/TitanSquire"
titanSquire.depth = -100
titanSquire.fieldInformation = {
    health = {
        minimumValue = 1
    },
    detectionRange = {
        minimumValue = 0
    },
    attackRange = {
        minimumValue = 0
    },
    moveSpeed = {
        minimumValue = 0
    },
    patrolDistance = {
        minimumValue = 0
    }
}
titanSquire.placements = {
    {
        name = "normal",
        data = {
            health = 4,
            detectionRange = 180,
            attackRange = 80,
            moveSpeed = 70,
            patrolDistance = 120
        }
    }
}

function titanSquire.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/titan_squire/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function titanSquire.selection(room, entity)
    return utils.rectangle(entity.x - 14, entity.y - 36, 28, 36)
end

return titanSquire
