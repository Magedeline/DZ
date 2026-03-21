local drawableSprite = require("structs.drawable_sprite")

local soulColor = {1.0, 0.0, 0.0, 1.0} -- Red
local soulIndex = 0

local determinationSoulBoost = {}

determinationSoulBoost.name = "MaggyHelper/DeterminationSoulBoost"
determinationSoulBoost.depth = -1000000
determinationSoulBoost.nodeLineRenderType = "line"
determinationSoulBoost.nodeLimits = {1, -1}

determinationSoulBoost.fieldInformation = {
    boostSpeed = {
        fieldType = "number",
        minimumValue = 50.0
    },
    abilityDuration = {
        fieldType = "number",
        minimumValue = 0.5
    },
    extraDashes = {
        fieldType = "integer",
        minimumValue = 1
    },
    dashPowerMultiplier = {
        fieldType = "number",
        minimumValue = 1.0
    }
}

determinationSoulBoost.fieldOrder = {
    "x", "y",
    "lockCamera", "canSkip", "oneUse",
    "boostSpeed", "abilityDuration",
    "extraDashes", "dashPowerMultiplier"
}

determinationSoulBoost.placements = {
    {
        name = "determination_soul_boost",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = false,
            boostSpeed = 320.0,
            abilityDuration = 3.0,
            extraDashes = 2,
            dashPowerMultiplier = 1.5
        }
    }
}

function determinationSoulBoost.sprite(room, entity)
    local sprites = {}
    
    local mainSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", entity)
    mainSprite:setJustification(0.5, 0.5)
    mainSprite:setColor(soulColor)
    table.insert(sprites, mainSprite)
    
    local soulSprite = drawableSprite.fromTexture("objects/sevensoulboost/soul/vessel_soul0" .. soulIndex, entity)
    soulSprite:setJustification(0.5, 0.5)
    soulSprite:setColor(soulColor)
    soulSprite:addPosition(0, -12)
    table.insert(sprites, soulSprite)
    
    return sprites
end

function determinationSoulBoost.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    
    local nodeSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", {x = node.x, y = node.y})
    nodeSprite:setJustification(0.5, 0.5)
    nodeSprite:setColor({soulColor[1], soulColor[2], soulColor[3], 0.4})
    table.insert(sprites, nodeSprite)
    
    return sprites
end

function determinationSoulBoost.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return determinationSoulBoost
