local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local waterAnvil = {}

waterAnvil.name = "DesoloZatnas/WaterAnvil"
waterAnvil.depth = -25
waterAnvil.placements = {
    name = "water_anvil",
    data = {
        weight = 20,
        puzzleID = "puzzle_1",
        affectsBalance = true,
        canPickUp = true,
        requiresStrength = false
    }
}

waterAnvil.fieldInformation = {
    weight = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 100
    }
}

function waterAnvil.sprite(room, entity)
    local sprites = {}
    
    -- Anvil sprite
    local anvilSprite = drawableSprite.fromTexture("objects/DesoloZatnas/water_puzzle/anvil", entity)
    anvilSprite:setColor({0.3, 0.3, 0.35, 1.0})
    table.insert(sprites, anvilSprite)
    
    -- Weight indicator
    local weightSprite = drawableSprite.fromTexture("objects/DesoloZatnas/water_puzzle/weight_icon", entity)
    weightSprite:setPosition(0, -20)
    weightSprite:setScale(0.5, 0.5)
    table.insert(sprites, weightSprite)
    
    return sprites
end

function waterAnvil.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 16, 24, 24)
end

return waterAnvil
