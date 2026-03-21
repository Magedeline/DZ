local determination_orb = {}

determination_orb.name = "DesoloZantas/DeterminationOrb"
determination_orb.depth = -100
determination_orb.placements = {
    {
        name = "Determination Orb",
        data = {
            dashBoost = 1,
            speedMultiplier = 1.2,
            duration = 10.0,
            oneUse = true
        }
    },
    {
        name = "Determination Orb (Strong)",
        data = {
            dashBoost = 2,
            speedMultiplier = 1.5,
            duration = 15.0,
            oneUse = true
        }
    },
    {
        name = "Determination Orb (Reusable)",
        data = {
            dashBoost = 1,
            speedMultiplier = 1.2,
            duration = 8.0,
            oneUse = false
        }
    }
}

determination_orb.fieldInformation = {
    dashBoost = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 3
    },
    speedMultiplier = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 2.0
    },
    duration = {
        fieldType = "number",
        minimumValue = 3.0,
        maximumValue = 60.0
    }
}

function determination_orb.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x, entity.y
    
    local drawableRectangle = require("structs.drawable_rectangle")
    
    -- Red glow
    local glowRect = drawableRectangle.fromRectangle("fill", x - 8, y - 8, 16, 16, {1.0, 0.0, 0.0, 0.4})
    for _, sprite in ipairs(glowRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    -- Core
    local coreRect = drawableRectangle.fromRectangle("bordered", x - 6, y - 6, 12, 12, {1.0, 0.2, 0.2, 0.8}, {1.0, 0.4, 0.4, 1.0})
    for _, sprite in ipairs(coreRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    return sprites
end

determination_orb.offset = {6, 6}

return determination_orb
