local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local fallenLeafPile = {}

fallenLeafPile.name = "DZ/FallenLeafPile"
fallenLeafPile.depth = -50
fallenLeafPile.fieldInformation = {
    hiddenContent = {
        options = {"Nothing", "Spikes", "Enemy", "Collectible", "SecretPath"},
        editable = false
    },
    detectionRange = {
        minimumValue = 0
    }
}
fallenLeafPile.placements = {
    {
        name = "normal",
        data = {
            hiddenContent = "Nothing",
            detectionRange = 40,
            enemyType = "DZ/RuinsSentinel",
            collectibleType = ""
        }
    },
    {
        name = "spikes",
        data = {
            hiddenContent = "Spikes",
            detectionRange = 40,
            enemyType = "",
            collectibleType = ""
        }
    },
    {
        name = "enemy",
        data = {
            hiddenContent = "Enemy",
            detectionRange = 40,
            enemyType = "DZ/RuinsSentinel",
            collectibleType = ""
        }
    },
    {
        name = "collectible",
        data = {
            hiddenContent = "Collectible",
            detectionRange = 40,
            enemyType = "",
            collectibleType = ""
        }
    }
}

function fallenLeafPile.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/fallen_leaf_pile/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function fallenLeafPile.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 16, 24, 16)
end

return fallenLeafPile
