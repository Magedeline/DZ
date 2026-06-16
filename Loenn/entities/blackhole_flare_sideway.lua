-- Blackhole Flare Sideway - horizontal moving black hole hazard with rainbow glitchy effects
local selectionBounds = require("libraries.selection_bounds")

local blackholeFlareSideway = {}

local defaultBounds = {
    width = 32,
    height = 32
}

blackholeFlareSideway.name = "DZ/BlackholeFlareSideway"
blackholeFlareSideway.depth = -50
blackholeFlareSideway.placements = {
    {
        name = "right",
        data = {
            width = 32,
            height = 32,
            direction = "Right",
            speed = 100.0,
            glitchy = true
        }
    },
    {
        name = "left",
        data = {
            width = 32,
            height = 32,
            direction = "Left",
            speed = 100.0,
            glitchy = true
        }
    }
}

blackholeFlareSideway.fieldInformation = {
    direction = {
        options = { "Left", "Right" },
        editable = false
    },
    speed = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 500.0
    },
    glitchy = {
        fieldType = "boolean"
    }
}

function blackholeFlareSideway.sprite(room, entity)
    local bounds = selectionBounds.resolve(entity, defaultBounds)

    return {
        {
            texture = "objects/IngesteHelper/blackhole_flare",
            x = 0,
            y = 0,
            justificationX = 0.0,
            justificationY = 0.0,
            scaleX = bounds.width / 8,
            scaleY = bounds.height / 8,
            color = {0.5, 0.0, 0.5, 0.8}
        }
    }
end

function blackholeFlareSideway.rectangle(room, entity)
    return selectionBounds.rectangle(entity, defaultBounds)
end

return blackholeFlareSideway
