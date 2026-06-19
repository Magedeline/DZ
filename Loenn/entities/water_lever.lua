local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local waterLever = {}

waterLever.name = "DesoloZatnas/WaterLever"
waterLever.depth = -100
waterLever.placements = {
    {
        name = "main",
        data = {
        flowDirection = "increase",
        flowAmount = 10,
        puzzleID = "puzzle_1",
        oneUse = false,
        requiresHold = false,
        leverType = "standard"
        }
    }
}

waterLever.fieldInformation = {
    flowDirection = {
        options = {"increase", "decrease", "drain", "fill"},
        editable = false
    },
    leverType = {
        options = {"standard", "pressure", "timed", "toggle"},
        editable = false
    },
    flowAmount = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 100
    }
}

function waterLever.sprite(room, entity)
    local sprites = {}
    
    -- Lever base
    local baseSprite = drawableSprite.fromTexture("objects/DesoloZatnas/water_puzzle/lever_base", entity)
    table.insert(sprites, baseSprite)
    
    -- Lever handle
    local handleSprite = drawableSprite.fromTexture("objects/DesoloZatnas/water_puzzle/lever_handle", entity)
    handleSprite:setPosition(0, -8)
    
    -- Color based on flow direction
    if entity.flowDirection == "increase" then
        handleSprite:setColor({0.3, 0.8, 0.3, 1.0})
    elseif entity.flowDirection == "decrease" then
        handleSprite:setColor({0.8, 0.3, 0.3, 1.0})
    elseif entity.flowDirection == "drain" then
        handleSprite:setColor({0.3, 0.3, 0.8, 1.0})
    else
        handleSprite:setColor({0.8, 0.8, 0.3, 1.0})
    end
    
    table.insert(sprites, handleSprite)
    
    return sprites
end

function waterLever.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 16, 16, 24)
end

return waterLever
