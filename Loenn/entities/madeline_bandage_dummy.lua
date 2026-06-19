local madelineBandageDummy = {}

madelineBandageDummy.name = "DZ/MadelineBandageDummy"
madelineBandageDummy.depth = 0
madelineBandageDummy.justification = {0.5, 1.0}
madelineBandageDummy.texture = "characters/madeline_bandage/idle00"

madelineBandageDummy.fieldInformation = {
    facing = {
        fieldType = "integer",
        options = {-1, 1},
        editable = false
    },
    animation = {
        fieldType = "string",
        options = {
            "idle",
            "walk"
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

madelineBandageDummy.placements = {
    {
        name = "main",
        data = {
            facing = 1,
            animation = "idle",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true
        }
    }
}

function madelineBandageDummy.sprite(room, entity)
    local facing = entity.facing or 1
    local scale = entity.scale or 1.0
    local alpha = entity.alpha or 1.0

    return {
        texture = "characters/madeline_bandage/idle00",
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = facing * scale,
        scaleY = scale,
        color = {1.0, 1.0, 1.0, alpha}
    }
end

return madelineBandageDummy
