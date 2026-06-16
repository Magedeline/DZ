local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
local selectionBounds = require("libraries.selection_bounds")

local waterFlowPuzzle = {}

local defaultBounds = {
    width = 32,
    height = 32,
    currentVolume = 0,
    maxVolume = 100
}

waterFlowPuzzle.name = "DesoloZatnas/WaterFlowPuzzle"
waterFlowPuzzle.depth = -10000
waterFlowPuzzle.placements = {
    name = "water_flow_puzzle",
    data = {
        width = 32,
        height = 32,
        targetVolume = 50,
        currentVolume = 0,
        tolerance = 5,
        dialogCorrect = "DZ_CH11_GATEWAY_LIFTED",
        dialogIncorrect = "DZ_CH11_WRONG_VOLUME_WATER",
        leverCount = 3,
        flowRate = 10,
        maxVolume = 100,
        anvilWeight = 20,
        requiresAnvil = true,
        requiresBalance = true
    }
}

waterFlowPuzzle.fieldInformation = {
    targetVolume = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 100
    },
    currentVolume = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 100
    },
    tolerance = {
        fieldType = "integer",
        minimumValue = 0,
        maximumValue = 20
    },
    leverCount = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    flowRate = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 50
    },
    maxVolume = {
        fieldType = "integer",
        minimumValue = 10,
        maximumValue = 200
    },
    anvilWeight = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 100
    }
}

function waterFlowPuzzle.sprite(room, entity)
    local bounds = selectionBounds.resolve(entity, defaultBounds)
    local maxVolume = math.max(bounds.maxVolume, 1)
    local sprites = {}
    
    -- Main puzzle container
    local containerSprite = drawableSprite.fromTexture("objects/DesoloZatnas/water_puzzle/container", entity)
    containerSprite:setJustification(0, 0)
    containerSprite:setScale(bounds.width / 32, bounds.height / 32)
    table.insert(sprites, containerSprite)
    
    -- Water level indicator
    local waterLevel = bounds.currentVolume / maxVolume
    local waterSprite = drawableSprite.fromTexture("objects/DesoloZatnas/water_puzzle/water", entity)
    waterSprite:setJustification(0, 1)
    waterSprite:setPosition(4, bounds.height - 4)
    waterSprite:setScale((bounds.width - 8) / 32, waterLevel * (bounds.height - 8) / 32)
    waterSprite:setColor({0.3, 0.6, 1.0, 0.7})
    table.insert(sprites, waterSprite)
    
    -- Volume indicator text
    local textSprite = drawableSprite.fromTexture("objects/DesoloZatnas/water_puzzle/display", entity)
    textSprite:setPosition(bounds.width / 2, -8)
    table.insert(sprites, textSprite)
    
    return sprites
end

function waterFlowPuzzle.selection(room, entity)
    return selectionBounds.rectangle(entity, defaultBounds)
end

return waterFlowPuzzle
