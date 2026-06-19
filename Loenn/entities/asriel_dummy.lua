local asrielDummy = {}

asrielDummy.name = "DZ/AsrielDummy"
asrielDummy.depth = 0
asrielDummy.texture = "characters/asriel/back00"
asrielDummy.justification = {0.5, 1.0}

asrielDummy.fieldInformation = {
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
            "sad",
            "happy",
            "magic",
            "transform"
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
    }
}

asrielDummy.placements = {
    {
        name = "main",
        data = {
            facing = 1,
            animation = "idle",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = false
        }
    },
    {
        name = "sad",
        data = {
            facing = 1,
            animation = "sad",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true
        }
    },
    {
        name = "happy",
        data = {
            facing = 1,
            animation = "happy",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true
        }
    }
}

function asrielDummy.sprite(room, entity)
    local facing = entity.facing or 1
    local scale = entity.scale or 1.0
    local alpha = entity.alpha or 1.0
    
    return {
        texture = "characters/asriel/idle00",
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = facing * scale,
        scaleY = scale,
        color = {1.0, 1.0, 1.0, alpha}
    }
end

return asrielDummy
