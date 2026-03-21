local void_tendril = {}

void_tendril.name = "DesoloZantas/VoidTendril"
void_tendril.depth = 100
void_tendril.nodeLimits = {0, -1}
void_tendril.placements = {
    {
        name = "Void Tendril",
        data = {
            damage = 1,
            swaySpeed = 1.0,
            phaseableOnDash = true,
            phaseWindow = 0.3
        }
    },
    {
        name = "Void Tendril (Impassable)",
        data = {
            damage = 1,
            swaySpeed = 1.0,
            phaseableOnDash = false,
            phaseWindow = 0.0
        }
    }
}

void_tendril.fieldInformation = {
    damage = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 5
    },
    swaySpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0
    },
    phaseWindow = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 1.0
    }
}

function void_tendril.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x, entity.y
    local height = 48
    
    local drawableRectangle = require("structs.drawable_rectangle")
    
    -- Draw tendril body
    for i = 0, height - 4, 4 do
        local width = 8 - (i / height) * 4  -- Tapers toward top
        local alpha = 0.6 + (i / height) * 0.3
        local rect = drawableRectangle.fromRectangle("fill", x - width/2, y + i, width, 4, {0.4, 0.0, 0.4, alpha})
        for _, sprite in ipairs(rect:getDrawableSprite()) do
            table.insert(sprites, sprite)
        end
    end
    
    return sprites
end

function void_tendril.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    local drawableRectangle = require("structs.drawable_rectangle")
    local rect = drawableRectangle.fromRectangle("fill", node.x - 4, node.y, 8, 8, {0.6, 0.0, 0.6, 0.5})
    
    for _, sprite in ipairs(rect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    return sprites
end

return void_tendril
