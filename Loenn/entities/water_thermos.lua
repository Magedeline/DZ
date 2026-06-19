local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local waterThermos = {}

waterThermos.name = "DesoloZatnas/WaterThermos"
waterThermos.depth = -50
waterThermos.placements = {
    {
        name = "main",
        data = {
        waterAmount = 50,
        isWarm = true,
        puzzleID = "puzzle_1",
        refillable = false,
        pourRate = 5
        }
    }
}

waterThermos.fieldInformation = {
    waterAmount = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 100
    },
    pourRate = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 20
    }
}

function waterThermos.sprite(room, entity)
    local sprites = {}
    
    -- Thermos body
    local thermosSprite = drawableSprite.fromTexture("objects/DesoloZatnas/water_puzzle/thermos", entity)
    
    -- Color based on temperature
    if entity.isWarm then
        thermosSprite:setColor({1.0, 0.7, 0.4, 1.0})
    else
        thermosSprite:setColor({0.6, 0.8, 1.0, 1.0})
    end
    
    table.insert(sprites, thermosSprite)
    
    -- Water level indicator
    local fillLevel = entity.waterAmount / 100
    local fillSprite = drawableSprite.fromTexture("objects/DesoloZatnas/water_puzzle/thermos_fill", entity)
    fillSprite:setScale(1.0, fillLevel)
    fillSprite:setColor({0.4, 0.7, 1.0, 0.6})
    table.insert(sprites, fillSprite)
    
    return sprites
end

function waterThermos.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 24)
end

return waterThermos
