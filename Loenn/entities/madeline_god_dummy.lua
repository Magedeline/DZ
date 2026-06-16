local madelineGodDummy = {}

madelineGodDummy.name = "DZ/MadelineGodDummy"
madelineGodDummy.depth = -100
madelineGodDummy.justification = {0.5, 1.0}
madelineGodDummy.texture = "characters/madeline/maddy00"

madelineGodDummy.fieldInformation = {
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
            "run",
            "wakeup",
            "sleep",
            "alert",
            "boost"
        },
        editable = true
    },
    combatActive = {
        fieldType = "boolean"
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

madelineGodDummy.placements = {
    {
        name = "normal",
        data = {
            facing = 1,
            animation = "boost",
            combatActive = true,
            scale = 1.0,
            alpha = 1.0,
            isVisible = true,
            playAnimationOnSpawn = true
        }
    }
}

function madelineGodDummy.sprite(room, entity)
    local facing = entity.facing or 1
    local scale = entity.scale or 1.0
    local alpha = entity.alpha or 1.0

    return {
        texture = "characters/madeline/maddy00",
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = facing * scale,
        scaleY = scale,
        color = {1.0, 1.0, 1.0, alpha}
    }
end

return madelineGodDummy
