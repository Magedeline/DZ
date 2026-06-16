local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local towerLantern = {}

towerLantern.name = "DZ/TowerLantern"
towerLantern.depth = -50
towerLantern.fieldInformation = {
    lightRadius = {
        minimumValue = 0
    },
    flickerIntensity = {
        minimumValue = 0,
        maximumValue = 1
    },
    lightColor = {
        fieldType = "color"
    }
}
towerLantern.placements = {
    {
        name = "unlit",
        data = {
            lightRadius = 80,
            flickerIntensity = 0.1,
            lanternId = "",
            startLit = false,
            lightColor = "FFA500"
        }
    },
    {
        name = "lit",
        data = {
            lightRadius = 80,
            flickerIntensity = 0.1,
            lanternId = "",
            startLit = true,
            lightColor = "FFA500"
        }
    }
}

function towerLantern.sprite(room, entity)
    local state = entity.startLit and "lit" or "unlit"
    local sprite = drawableSprite.fromTexture("objects/tower_lantern/" .. state, entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    
    if entity.startLit then
        sprite.color = {1.0, 0.65, 0.0, 1.0}
    end
    
    return sprite
end

function towerLantern.selection(room, entity)
    return utils.rectangle(entity.x - 6, entity.y - 20, 12, 20)
end

return towerLantern
