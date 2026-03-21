local drawableSprite = require("structs.drawable_sprite")
--- IGNORE ---
local soulColor = {1.0, 0.0, 1.0, 1.0} -- Purple
local soulIndex = 6

local perseveranceSoulBoost = {}

perseveranceSoulBoost.name = "MaggyHelper/PerseveranceSoulBoost"
perseveranceSoulBoost.depth = -1000000
perseveranceSoulBoost.nodeLineRenderType = "line"
perseveranceSoulBoost.nodeLimits = {1, -1}

perseveranceSoulBoost.fieldInformation = {
    boostSpeed = {
        fieldType = "number",
        minimumValue = 50.0
    },
    abilityDuration = {
        fieldType = "number",
        minimumValue = 0.5
    },
    enduranceDuration = {
        fieldType = "number",
        minimumValue = 1.0
    },
    staminaRegen = {
        fieldType = "number",
        minimumValue = 1.0
    }
}

perseveranceSoulBoost.fieldOrder = {
    "x", "y",
    "lockCamera", "canSkip", "oneUse",
    "boostSpeed", "abilityDuration",
    "enduranceDuration", "staminaRegen", "autoClimb"
}

perseveranceSoulBoost.placements = {
    {
        name = "perseverance_soul_boost",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = false,
            boostSpeed = 300.0,
            abilityDuration = 4.0,
            enduranceDuration = 5.0,
            staminaRegen = 20.0,
            autoClimb = true
        }
    }
}

function perseveranceSoulBoost.sprite(room, entity)
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

function perseveranceSoulBoost.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    
    local nodeSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", {x = node.x, y = node.y})
    nodeSprite:setJustification(0.5, 0.5)
    nodeSprite:setColor({soulColor[1], soulColor[2], soulColor[3], 0.4})
    table.insert(sprites, nodeSprite)
    
    return sprites
end

function perseveranceSoulBoost.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return perseveranceSoulBoost
