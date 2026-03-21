local drawableSprite = require("structs.drawable_sprite")

local soulColor = {0.0, 1.0, 0.0, 1.0} -- Green
local soulIndex = 3

local kindnessSoulBoost = {}

kindnessSoulBoost.name = "MaggyHelper/KindnessSoulBoost"
kindnessSoulBoost.depth = -1000000
kindnessSoulBoost.nodeLineRenderType = "line"
kindnessSoulBoost.nodeLimits = {1, -1}

kindnessSoulBoost.fieldInformation = {
    boostSpeed = {
        fieldType = "number",
        minimumValue = 50.0
    },
    abilityDuration = {
        fieldType = "number",
        minimumValue = 0.5
    },
    shieldDuration = {
        fieldType = "number",
        minimumValue = 1.0
    }
}

kindnessSoulBoost.fieldOrder = {
    "x", "y",
    "lockCamera", "canSkip", "oneUse",
    "boostSpeed", "abilityDuration",
    "shieldDuration", "fullRestore"
}

kindnessSoulBoost.placements = {
    {
        name = "kindness_soul_boost",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = false,
            boostSpeed = 280.0,
            abilityDuration = 4.0,
            shieldDuration = 5.0,
            fullRestore = true
        }
    }
}

function kindnessSoulBoost.sprite(room, entity)
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

function kindnessSoulBoost.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    
    local nodeSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", {x = node.x, y = node.y})
    nodeSprite:setJustification(0.5, 0.5)
    nodeSprite:setColor({soulColor[1], soulColor[2], soulColor[3], 0.4})
    table.insert(sprites, nodeSprite)
    
    return sprites
end

function kindnessSoulBoost.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return kindnessSoulBoost
