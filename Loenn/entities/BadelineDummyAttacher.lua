local badelineDummyAttacher = {}

badelineDummyAttacher.name = "DZ/BadelineDummyAttacher"
badelineDummyAttacher.depth = 0
badelineDummyAttacher.justification = {0.5, 1.0}
badelineDummyAttacher.texture = "characters/badeline/idle00"

badelineDummyAttacher.fieldInformation = {
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

badelineDummyAttacher.placements = {
    {
        name = "default",
        data = {
            hoverOffsetX = -20,
            hoverOffsetY = -10,
            hoverSpeed = 2.0,
            hoverAmplitude = 4.0
        }
    }
}

function badelineDummyAttacher.sprite(room, entity)
    local hoverOffsetX = entity.hoverOffsetX or -20
    local hoverOffsetY = entity.hoverOffsetY or -10
    
    return {
        texture = "characters/badeline/idle00",
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = 1.0,
        scaleY = 1.0,
        color = {1.0, 1.0, 1.0, 1.0}
    }
end

return badelineDummyAttacher