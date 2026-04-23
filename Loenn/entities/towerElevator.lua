local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local towerElevator = {}

towerElevator.name = "MaggyHelper/TowerElevator"
towerElevator.depth = -100
towerElevator.minimumSize = {24, 8}
towerElevator.fieldInformation = {
    moveSpeed = {
        minimumValue = 0
    },
    waitTime = {
        minimumValue = 0
    }
}
towerElevator.placements = {
    {
        name = "normal",
        data = {
            width = 32,
            height = 8,
            moveSpeed = 80,
            waitTime = 1.0,
            elevatorId = ""
        }
    }
}

function towerElevator.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 32
    local height = entity.height or 8
    
    for x = 0, width - 8, 8 do
        for y = 0, height - 8, 8 do
            local sprite = drawableSprite.fromTexture("objects/tower_elevator/idle", entity)
            sprite:setPosition(entity.x + x, entity.y + y)
            sprite:setJustification(0, 0)
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function towerElevator.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 32, entity.height or 8)
end

return towerElevator
