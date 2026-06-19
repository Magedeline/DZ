local drawableSprite = require("structs.drawable_sprite")

local soulColors = {
    {1.0, 0.0, 0.0, 1.0},     -- Red - Determination (ff0000)
    {1.0, 0.502, 0.0, 1.0},   -- Orange - Bravery (ff8000)
    {1.0, 1.0, 0.0, 1.0},     -- Yellow - Justice (ffff00)
    {0.0, 1.0, 0.0, 1.0},     -- Green - Kindness (00ff00)
    {0.0, 1.0, 1.0, 1.0},     -- Cyan - Patience (00ffff)
    {0.0, 0.0, 1.0, 1.0},     -- Blue - Integrity (0000ff)
    {1.0, 0.0, 1.0, 1.0}      -- Purple - Perseverance (ff00ff)
}

local sevenSoulBoost = {}

sevenSoulBoost.name = "DZ/SevenSoulBoost"
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
    finalCh21Dialog = {
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
    "finalCh21Boost",
    "finalCh21GoldenBoost",
    "finalCh21Dialog"
}

sevenSoulBoost.placements = {
    {
        name = "main",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = false,
            boostSpeed = 320.0,
            refillDashes = true,
            refillStamina = true,
            dashCount = 10,
            finalCh21Boost = false,
            finalCh21GoldenBoost = false,
            finalCh21Dialog = ""
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
            dashCount = 10,
            finalCh21Boost = false,
            finalCh21GoldenBoost = false,
            finalCh21Dialog = ""
        }
    },
    {
        name = "seven_soul_boost_ch21_final",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = true,
            boostSpeed = 320.0,
            refillDashes = true,
            refillStamina = true,
            dashCount = 10,
            finalCh21Boost = true,
            finalCh21GoldenBoost = false,
            finalCh21Dialog = "DZ_CH21_MADELINE_LAST_BOOST"
        }
    },
    {
        name = "seven_soul_boost_ch21_golden",
        data = {
            lockCamera = true,
            canSkip = false,
            oneUse = true,
            boostSpeed = 320.0,
            refillDashes = true,
            refillStamina = true,
            dashCount = 1,
            finalCh21Boost = true,
            finalCh21GoldenBoost = true,
            finalCh21Dialog = "DZ_CH21_MADELINE_LAST_BOOST"
        }
    }
}

-- Custom sprite function to render the seven souls orbiting
function sevenSoulBoost.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x, entity.y
    
    -- Main sprite in center (matches C#: characters/soul/soul/vessel_soulA)
    local mainSprite = drawableSprite.fromTexture("characters/soul/soul/vessel_soulA00", entity)
    mainSprite:setJustification(0.5, 0.5)
    table.insert(sprites, mainSprite)
    
    -- Draw 7 souls orbiting around (matches C#: vessel_soulA through vessel_soulG)
    local radius = 20
    local soulSuffixes = {"A", "B", "C", "D", "E", "F", "G"}
    for i = 1, 7 do
        local angle = ((i - 1) / 7) * math.pi * 2
        local offsetX = math.cos(angle) * radius
        local offsetY = math.sin(angle) * radius
        
        local soulSprite = drawableSprite.fromTexture("characters/soul/soul/vessel_soul" .. soulSuffixes[i] .. "00", entity)
        soulSprite:setJustification(0.5, 0.5)
        soulSprite:setColor(soulColors[i])
        soulSprite:addPosition(offsetX, offsetY)
        table.insert(sprites, soulSprite)
    end
    
    return sprites
end

-- Node sprite
function sevenSoulBoost.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    
    local mainSprite = drawableSprite.fromTexture("characters/soul/soul/vessel_soulA00", node)
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