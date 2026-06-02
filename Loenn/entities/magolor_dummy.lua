local magolorDummy = {}

magolorDummy.name = "MaggyHelper/MagolorDummy"
magolorDummy.depth = 0
magolorDummy.justification = {0.5, 1.0}
magolorDummy.texture = "characters/kirby/magolor/idle00"

magolorDummy.fieldInformation = {
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
            "happy",
            "talk"
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

magolorDummy.placements = {
    {
        name = "normal",
        data = {
            facing = 1,
            animation = "idle",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true,
            autoFollow = true,
            followDelay = 0.55
        }
    }
}

function magolorDummy.sprite(room, entity)
    local facing = entity.facing or 1
    local scale = entity.scale or 1.0
    local alpha = entity.alpha or 1.0
    
    return {
        texture = "characters/kirby/magolor/idle00",
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = facing * scale,
        scaleY = scale,
        color = {1.0, 1.0, 1.0, alpha}
    }
end

return magolorDummy
