local drawableSprite = require("structs.drawable_sprite")

local soulColors = {
    {1.0, 0.0, 0.0, 1.0},   -- Red - Determination
    {1.0, 0.5, 0.0, 1.0},   -- Orange - Bravery
    {1.0, 1.0, 0.0, 1.0},   -- Yellow - Justice
    {0.0, 1.0, 0.0, 1.0},   -- Green - Kindness
    {0.0, 1.0, 1.0, 1.0},   -- Cyan - Patience
    {0.0, 0.0, 1.0, 1.0},   -- Blue - Integrity
    {1.0, 0.0, 1.0, 1.0}    -- Purple - Perseverance
}

local sevenSoulBoost = {}

sevenSoulBoost.name = "MaggyHelper/SevenSoulBoost"
sevenSoulBoost.depth = -1000000
sevenSoulBoost.nodeLineRenderType = "line"
sevenSoulBoost.nodeLimits = {1, -1}

sevenSoulBoost.fieldInformation = {
    boostSpeed = {
        fieldType = "number",
        minimumValue = 50.0
    },
    dashCount = {
        fieldType = "integer",
        minimumValue = 1
    },
    finalCh20Dialog = {
        fieldType = "string"
    }
}

sevenSoulBoost.fieldOrder = {
    "x", "y",
    "lockCamera",
    "canSkip",
    "oneUse",
    "boostSpeed",
    "refillDashes",
    "refillStamina",
    "dashCount",
    "finalCh20Boost",
    "finalCh20GoldenBoost",
    "finalCh20Dialog"
}

sevenSoulBoost.placements = {
    {
        name = "seven_soul_boost",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = false,
            boostSpeed = 320.0,
            refillDashes = true,
            refillStamina = true,
            dashCount = 1,
            finalCh20Boost = false,
            finalCh20GoldenBoost = false,
            finalCh20Dialog = ""
        }
    },
    {
        name = "seven_soul_boost_one_use",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = true,
            boostSpeed = 320.0,
            refillDashes = true,
            refillStamina = true,
            dashCount = 1,
            finalCh20Boost = false,
            finalCh20GoldenBoost = false,
            finalCh20Dialog = ""
        }
    },
    {
        name = "seven_soul_boost_ch20_final",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = true,
            boostSpeed = 320.0,
            refillDashes = true,
            refillStamina = true,
            dashCount = 1,
            finalCh20Boost = true,
            finalCh20GoldenBoost = false,
            finalCh20Dialog = "CH20_SEVEN_SOULS_LAST_BOOST"
        }
    },
    {
        name = "seven_soul_boost_ch20_golden",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = true,
            boostSpeed = 320.0,
            refillDashes = true,
            refillStamina = true,
            dashCount = 1,
            finalCh20Boost = true,
            finalCh20GoldenBoost = true,
            finalCh20Dialog = "CH20_SEVEN_SOULS_LAST_BOOST"
        }
    }
}

-- Custom sprite function to render the seven souls orbiting
function sevenSoulBoost.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x, entity.y
    
    -- Main sprite in center
    local mainSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", entity)
    mainSprite:setJustification(0.5, 0.5)
    table.insert(sprites, mainSprite)
    
    -- Draw 7 souls orbiting around
    local radius = 20
    for i = 0, 6 do
        local angle = (i / 7) * math.pi * 2
        local offsetX = math.cos(angle) * radius
        local offsetY = math.sin(angle) * radius
        
        local soulSprite = drawableSprite.fromTexture("objects/sevensoulboost/soul/vessel_soul0" .. i, entity)
        soulSprite:setJustification(0.5, 0.5)
        soulSprite:setColor(soulColors[i + 1])
        soulSprite:addPosition(offsetX, offsetY)
        table.insert(sprites, soulSprite)
    end
    
    return sprites
end

-- Node sprite
function sevenSoulBoost.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    
    local mainSprite = drawableSprite.fromTexture("objects/sevensoulboost/idle00", node)
    mainSprite:setJustification(0.5, 0.5)
    mainSprite:setColor({1.0, 1.0, 1.0, 0.5})
    table.insert(sprites, mainSprite)
    
    return sprites
end

-- Selection for the entity
function sevenSoulBoost.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

-- Node selection
function sevenSoulBoost.nodeSelection(room, entity, node, nodeIndex)
    return utils.rectangle(node.x - 8, node.y - 8, 16, 16)
end

return sevenSoulBoost