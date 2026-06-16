local time_echo_platform = {}

time_echo_platform.name = "DesoloZantas/TimeEchoPlatform"
time_echo_platform.depth = 0
time_echo_platform.canResize = {true, false}
time_echo_platform.placements = {
    {
        name = "Time Echo Platform",
        data = {
            width = 16,
            phaseTime = 2.0,
            offset = 0.0,
            syncOnDash = true
        }
    },
    {
        name = "Time Echo Platform (Fast)",
        data = {
            width = 16,
            phaseTime = 1.0,
            offset = 0.0,
            syncOnDash = true
        }
    },
    {
        name = "Time Echo Platform (Slow)",
        data = {
            width = 16,
            phaseTime = 4.0,
            offset = 0.0,
            syncOnDash = true
        }
    }
}

time_echo_platform.fieldInformation = {
    phaseTime = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 10.0
    },
    offset = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    }
}

function time_echo_platform.sprite(room, entity)
    local sprites = {}
    local width = entity.width or 16
    
    -- Draw main platform
    local drawableRectangle = require("structs.drawable_rectangle")
    local mainRect = drawableRectangle.fromRectangle("bordered", entity.x, entity.y, width, 8, {0.2, 0.8, 1.0, 0.6}, {0.4, 0.9, 1.0, 0.8})
    
    for _, sprite in ipairs(mainRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    -- Draw ghost platform (phase indicator)
    local ghostRect = drawableRectangle.fromRectangle("fill", entity.x, entity.y - 16, width, 8, {0.2, 0.8, 1.0, 0.2})
    
    for _, sprite in ipairs(ghostRect:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    
    return sprites
end

function time_echo_platform.rectangle(room, entity)
    return {
        x = entity.x,
        y = entity.y,
        width = entity.width or 16,
        height = 8
    }
end

return time_echo_platform
