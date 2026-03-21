-- Blackhole Riser - rising black hole column hazard
local selectionBounds = require("libraries.selection_bounds")

local blackholeRiser = {}

local defaultBounds = {
    width = 32,
    maxHeight = 200.0
}

blackholeRiser.name = "MaggyHelper/BlackholeRiser"
blackholeRiser.depth = -50
blackholeRiser.placements = {
    {
        name = "normal",
        data = {
            width = 32,
            speed = 120.0,
            maxHeight = 200.0,
            riseDelay = 1.0,
            glitchy = true,
            looping = true
        }
    },
    {
        name = "fast",
        data = {
            width = 32,
            speed = 250.0,
            maxHeight = 150.0,
            riseDelay = 0.5,
            glitchy = true,
            looping = true
        }
    },
    {
        name = "slow_tall",
        data = {
            width = 32,
            speed = 60.0,
            maxHeight = 300.0,
            riseDelay = 2.0,
            glitchy = true,
            looping = true
        }
    }
}

blackholeRiser.fieldInformation = {
    width = {
        fieldType = "integer",
        minimumValue = 8,
        maximumValue = 128
    },
    speed = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 500.0
    },
    maxHeight = {
        fieldType = "number",
        minimumValue = 32.0,
        maximumValue = 1000.0
    },
    riseDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    glitchy = {
        fieldType = "boolean"
    },
    looping = {
        fieldType = "boolean"
    }
}

function blackholeRiser.sprite(room, entity)
    local bounds = selectionBounds.resolve(entity, defaultBounds)

    return {
        {
            -- Base indicator
            texture = "objects/IngesteHelper/blackhole_riser_base",
            x = 0,
            y = 0,
            justificationX = 0.0,
            justificationY = 0.0,
            scaleX = bounds.width / 8,
            scaleY = 0.5,
            color = {0.5, 0.0, 0.5, 0.8}
        },
        {
            -- Max height indicator (preview)
            texture = "objects/IngesteHelper/blackhole_riser_top",
            x = 0,
            y = -bounds.maxHeight,
            justificationX = 0.0,
            justificationY = 0.0,
            scaleX = bounds.width / 8,
            scaleY = 0.5,
            color = {0.7, 0.0, 0.7, 0.4}
        }
    }
end

function blackholeRiser.rectangle(room, entity)
    -- Show both base position and max height area
    local bounds = selectionBounds.resolve(entity, defaultBounds)

    return selectionBounds.rectangle({
        x = bounds.x,
        y = bounds.y - bounds.maxHeight,
        width = bounds.width,
        height = bounds.maxHeight
    })
end

return blackholeRiser
