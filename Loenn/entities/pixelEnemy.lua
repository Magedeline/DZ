local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local pixelEnemy = {}

pixelEnemy.name = "DZ/PixelEnemy"
pixelEnemy.depth = -100
pixelEnemy.fieldInformation = {
    enemyType = {
        options = {"Walker", "Shooter", "Dasher", "Glitcher"},
        editable = false
    },
    health = {
        minimumValue = 1
    },
    moveSpeed = {
        minimumValue = 0
    },
    detectionRange = {
        minimumValue = 0
    },
    gridSize = {
        minimumValue = 1
    }
}
pixelEnemy.placements = {
    {
        name = "main",
        data = {
            enemyType = "Walker",
            health = 2,
            moveSpeed = 60,
            detectionRange = 150,
            gridSize = 8
        }
    },
    {
        name = "shooter",
        data = {
            enemyType = "Shooter",
            health = 2,
            moveSpeed = 60,
            detectionRange = 150,
            gridSize = 8
        }
    },
    {
        name = "dasher",
        data = {
            enemyType = "Dasher",
            health = 2,
            moveSpeed = 60,
            detectionRange = 150,
            gridSize = 8
        }
    }
}

function pixelEnemy.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/pixel_enemy/idle", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    sprite.color = {0.0, 0.8, 0.2, 1.0}
    return sprite
end

function pixelEnemy.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return pixelEnemy
