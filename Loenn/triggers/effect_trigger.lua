local effectTrigger = {}

effectTrigger.name = "MaggyHelper/EffectTrigger"
effectTrigger.placements = {
    {
        name = "particle",
        data = {
            width = 16,
            height = 16,
            effectType = "sparkles",
            intensity = 1.0,
            duration = 3.0,
            triggerOnce = true
        }
    },
    {
        name = "screen_shake",
        data = {
            width = 24,
            height = 24,
            effectType = "screen_shake",
            intensity = 0.5,
            duration = 1.0,
            triggerOnce = false
        }
    },
    {
        name = "color_grade",
        data = {
            width = 32,
            height = 32,
            effectType = "color_grade",
            intensity = 0.8,
            duration = 2.0,
            triggerOnce = false
        }
    }
}

effectTrigger.fieldInformation = {
    effectType = {
        options = {"sparkles", "screen_shake", "color_grade", "wind", "lightning"},
        editable = false
    },
    intensity = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 2.0
    },
    duration = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 10.0
    },
    triggerOnce = {
        fieldType = "boolean"
    }
}

function effectTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    local colors = {
        sparkles = {1.0, 0.9, 0.3, 0.7},
        screen_shake = {0.9, 0.3, 0.3, 0.7},
        color_grade = {0.6, 0.3, 0.9, 0.7},
        wind = {0.3, 0.9, 0.9, 0.7},
        lightning = {0.9, 0.9, 0.9, 0.8}
    }
    
    local color = colors[entity.effectType] or colors.sparkles
    
    return {
        {
            texture = "ahorn/entityTrigger",
            x = entity.x,
            y = entity.y,
            scaleX = width / 8,
            scaleY = height / 8,
            color = color
        }
    }
end

function effectTrigger.selection(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    return {entity.x, entity.y, width, height}
end

return effectTrigger
