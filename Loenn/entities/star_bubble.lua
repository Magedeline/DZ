local star_bubble = {}

star_bubble.name = "DesoloZantas/StarBubble"
star_bubble.depth = -100
star_bubble.placements = {
    {
        name = "Star Bubble",
        data = {
            duration = 5.0,
            floatSpeed = 80.0,
            immuneToHazards = true,
            respawnTime = 3.0
        }
    },
    {
        name = "Star Bubble (Long Duration)",
        data = {
            duration = 10.0,
            floatSpeed = 80.0,
            immuneToHazards = true,
            respawnTime = 5.0
        }
    },
    {
        name = "Star Bubble (Fast)",
        data = {
            duration = 5.0,
            floatSpeed = 120.0,
            immuneToHazards = true,
            respawnTime = 3.0
        }
    }
}

star_bubble.fieldInformation = {
    duration = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 30.0
    },
    floatSpeed = {
        fieldType = "number",
        minimumValue = 40.0,
        maximumValue = 200.0
    },
    respawnTime = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 30.0
    }
}

function star_bubble.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x, entity.y
    local radius = 12
    
    -- Draw bubble circle
    local drawableRectangle = require("structs.drawable_rectangle")
    
    -- Outer glow
    local outerRect = drawableRectangle.fromRectangle("fill", x - radius - 2, y - radius - 2, (radius + 2) * 2, (radius + 2) * 2, {1.0, 0.8, 0.2, 0.3})
    for _, sprite in ipairs(outerRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    -- Inner bubble
    local innerRect = drawableRectangle.fromRectangle("bordered", x - radius, y - radius, radius * 2, radius * 2, {1.0, 0.9, 0.4, 0.5}, {1.0, 1.0, 0.6, 0.8})
    for _, sprite in ipairs(innerRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    return sprites
end

star_bubble.offset = {12, 12}

return star_bubble
