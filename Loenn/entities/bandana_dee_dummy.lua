local bandanaDeeDummy = {}

bandanaDeeDummy.name = "DZ/BandanaDeeDummy"
bandanaDeeDummy.depth = 0
bandanaDeeDummy.texture = "characters/kirby/bandana_dee/idle00"
bandanaDeeDummy.justification = {0.5, 1.0}

bandanaDeeDummy.fieldInformation = {
    facing = {
        fieldType = "integer",
        options = {-1, 1},
        editable = false
    },
    animation = {
        fieldType = "string",
        options = {
            "idle",
            "walk",
            "talk",
            "happy"
        },
        editable = true
    },
    scale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 3.0
    },
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    isVisible = {
        fieldType = "boolean"
    },
    playAnimationOnSpawn = {
        fieldType = "boolean"
    },
    autoFollow = {
        fieldType = "boolean"
    },
    followDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0
    }
}

bandanaDeeDummy.placements = {
    {
        name = "main",
        data = {
            facing = 1,
            animation = "idle",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true,
            autoFollow = true,
            followDelay = 0.42
        }
    }
}

function bandanaDeeDummy.sprite(room, entity)
    local facing = entity.facing or 1
    local scale = entity.scale or 1.0
    local alpha = entity.alpha or 1.0

    return {
        texture = "characters/kirby/bandana_dee/idle00",
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = facing * scale,
        scaleY = scale,
        color = {1.0, 1.0, 1.0, alpha}
    }
end

return bandanaDeeDummy
