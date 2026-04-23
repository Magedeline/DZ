local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local towerGargoyle = {}

towerGargoyle.name = "MaggyHelper/TowerGargoyle"
towerGargoyle.depth = -100
towerGargoyle.fieldInformation = {
    health = {
        minimumValue = 1
    },
    detectionRange = {
        minimumValue = 0
    },
    swoopSpeed = {
        minimumValue = 0
    },
    glideSpeed = {
        minimumValue = 0
    }
}
towerGargoyle.placements = {
    {
        name = "normal",
        data = {
            health = 2,
            detectionRange = 150,
            swoopSpeed = 250,
            glideSpeed = 100
        }
    }
}

function towerGargoyle.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("characters/tower_gargoyle/dormant", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function towerGargoyle.selection(room, entity)
    return utils.rectangle(entity.x - 14, entity.y - 24, 28, 24)
end

return towerGargoyle
