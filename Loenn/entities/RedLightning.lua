-- Loenn integration for Red Lightning entity
-- Creates dramatic red lightning visual effects

local selectionBounds = require("libraries.selection_bounds")

local redLightning = {}

local defaultBounds = {
    width = 16,
    height = 32
}

redLightning.name = "MaggyHelper/RedLightning"
redLightning.depth = -10
redLightning.texture = "effects/lightning_red"

redLightning.fieldInformation = {
    duration = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    intensity = {
        fieldType = "number", 
        minimumValue = 0.1,
        maximumValue = 2.0
    },
    color = {
        fieldType = "color",
        allowAlpha = true
    },
    followKglobal::global::Celeste.Player= {
        fieldType = "boolean"
    },
    soundEffect = {
        fieldType = "string",
        options = {
            "event:/game/general/lightning",
            "event:/char/badeline/disappear",
            "event:/game/general/thing_booped"
        },
        editable = true
    }
}

redLightning.placements = {
    {
        name = "dramatic_effect",
        data = {
            width = 32,
            height = 64,
            duration = 2.0,
            intensity = 1.0,
            color = "FF0000FF",
            followKglobal::global::Celeste.Player= false,
            soundEffect = "event:/game/general/lightning"
        }
    },
    {
        name = "chase_effect",
        data = {
            width = 24,
            height = 48,
            duration = 1.5,
            intensity = 1.5,
            color = "FF4444AA",
            followKglobal::global::Celeste.Player= true,
            soundEffect = "event:/char/badeline/disappear"
        }
    },
    {
        name = "ambient_effect",
        data = {
            width = 16,
            height = 32,
            duration = 0.5,
            intensity = 0.7,
            color = "DD0000BB",
            followKglobal::global::Celeste.Player= false,
            soundEffect = ""
        }
    }
}

function redLightning.sprite(room, entity)
    local bounds = selectionBounds.resolve(entity, defaultBounds)
    local sprite = {}
    
    sprite.texture = redLightning.texture
    sprite.x = bounds.x
    sprite.y = bounds.y
    sprite.scaleX = bounds.width / 16
    sprite.scaleY = bounds.height / 16
    
    return sprite
end

return redLightning