local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local towerWindow = {}

towerWindow.name = "DZ/TowerWindow"
towerWindow.depth = -50
towerWindow.fieldInformation = {
    view = {
        options = {"Sky", "Stars", "Moon", "Sunrise", "Storm", "City"},
        editable = false
    },
    lightIntensity = {
        minimumValue = 0,
        maximumValue = 1
    }
}
towerWindow.placements = {
    {
        name = "sky",
        data = {
            view = "Sky",
            lightIntensity = 0.6,
            startLit = true
        }
    },
    {
        name = "stars",
        data = {
            view = "Stars",
            lightIntensity = 0.4,
            startLit = true
        }
    },
    {
        name = "dark",
        data = {
            view = "Sky",
            lightIntensity = 0.0,
            startLit = false
        }
    }
}

function towerWindow.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/tower_window_frame/default", entity)
    sprite:setPosition(entity.x, entity.y)
    sprite:setJustification(0.5, 1.0)
    return sprite
end

function towerWindow.selection(room, entity)
    return utils.rectangle(entity.x - 24, entity.y - 64, 48, 64)
end

return towerWindow
