local flyingBird = {}

flyingBird.name = "MaggyHelper/FlyingBird"
flyingBird.depth = -1000000
flyingBird.nodeLineRenderType = "line"
flyingBird.texture = "characters/Maggy/DesoloZantas/bird/Hover04"
flyingBird.nodeLimits = {0, -1}
flyingBird.justification = {0.5, 1.0}

flyingBird.fieldInformation = {
    loopPath = {
        fieldType = "boolean"
    },
    speed = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 200.0
    },
    emitFeathers = {
        fieldType = "boolean"
    },
    disableFlapSfx = {
        fieldType = "boolean"
    }
}

flyingBird.placements = {
    {
        name = "flying_bird",
        data = {
            loopPath = false,
            speed = 60.0,
            emitFeathers = false,
            disableFlapSfx = false
        }
    },
    {
        name = "flying_bird_looping",
        data = {
            loopPath = true,
            speed = 60.0,
            emitFeathers = true,
            disableFlapSfx = false
        }
    }
}

function flyingBird.scale(room, entity)
    return -1, 1
end

return flyingBird

