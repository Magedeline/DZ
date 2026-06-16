local charaDummyAttacher = {}

charaDummyAttacher.name = "DZ/CharaDummyAttacher"
charaDummyAttacher.depth = 0
charaDummyAttacher.justification = {0.5, 1.0}
charaDummyAttacher.texture = "characters/chara/idle00"

charaDummyAttacher.fieldInformation = {
    hoverOffsetX = {
        fieldType = "number",
        minimumValue = -100,
        maximumValue = 100
    },
    hoverOffsetY = {
        fieldType = "number",
        minimumValue = -100,
        maximumValue = 100
    },
    hoverSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    hoverAmplitude = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 20.0
    }
}

charaDummyAttacher.placements = {
    {
        name = "default",
        data = {
            hoverOffsetX = 25,
            hoverOffsetY = -15,
            hoverSpeed = 2.5,
            hoverAmplitude = 5.0
        }
    }
}

function charaDummyAttacher.sprite(room, entity)
    local hoverOffsetX = entity.hoverOffsetX or 25
    local hoverOffsetY = entity.hoverOffsetY or -15
    
    return {
        texture = "characters/chara/idle00",
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = 1.0,
        scaleY = 1.0,
        color = {1.0, 1.0, 1.0, 1.0}
    }
end

return charaDummyAttacher