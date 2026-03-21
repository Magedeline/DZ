local dimensional_rift = {}

dimensional_rift.name = "DesoloZantas/DimensionalRift"
dimensional_rift.depth = 2000
dimensional_rift.nodeLimits = {1, 1}
dimensional_rift.nodeLineRenderType = "line"
dimensional_rift.placements = {
    {
        name = "Dimensional Rift",
        data = {
            targetRoom = "",
            targetRiftId = "",
            riftId = "",
            bidirectional = true,
            transitionColor = "800080"
        }
    }
}

dimensional_rift.fieldInformation = {
    transitionColor = {
        fieldType = "color"
    }
}

function dimensional_rift.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x, entity.y
    local width, height = 24, 40
    
    local drawableRectangle = require("structs.drawable_rectangle")
    
    -- Outer glow
    local outerRect = drawableRectangle.fromRectangle("fill", x - 2, y - 2, width + 4, height + 4, {0.6, 0.0, 0.6, 0.3})
    for _, sprite in ipairs(outerRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    -- Inner portal
    local innerRect = drawableRectangle.fromRectangle("bordered", x, y, width, height, {0.4, 0.0, 0.5, 0.7}, {0.8, 0.2, 0.8, 0.9})
    for _, sprite in ipairs(innerRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    return sprites
end

function dimensional_rift.nodeSprite(room, entity, node, nodeIndex)
    local sprites = {}
    local drawableRectangle = require("structs.drawable_rectangle")
    
    local rect = drawableRectangle.fromRectangle("bordered", node.x, node.y, 16, 16, {0.4, 0.0, 0.5, 0.5}, {0.8, 0.2, 0.8, 0.7})
    for _, sprite in ipairs(rect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    return sprites
end

function dimensional_rift.rectangle(room, entity)
    return {
        x = entity.x,
        y = entity.y,
        width = 24,
        height = 40
    }
end

return dimensional_rift
