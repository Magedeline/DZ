-- Blackhole Sandwich - dual hazards that move up/down with hot/cold toggle
local selectionBounds = require("libraries.selection_bounds")

local blackholeSandwich = {}

local defaultBounds = {
    width = 64,
    height = 128
}

blackholeSandwich.name = "DZ/BlackholeSandwich"
blackholeSandwich.depth = -50
blackholeSandwich.placements = {
    {
        name = "hot",
        data = {
            width = 64,
            height = 128,
            mode = "Hot",
            speed = 80.0,
            glitchy = true,
            canSwitch = true,
            switchFlag = ""
        }
    },
    {
        name = "cold",
        data = {
            width = 64,
            height = 128,
            mode = "Cold",
            speed = 80.0,
            glitchy = true,
            canSwitch = true,
            switchFlag = ""
        }
    }
}

blackholeSandwich.fieldInformation = {
    mode = {
        options = { "Hot", "Cold" },
        editable = false
    },
    speed = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 300.0
    },
    glitchy = {
        fieldType = "boolean"
    },
    canSwitch = {
        fieldType = "boolean"
    },
    switchFlag = {
        fieldType = "string"
    }
}

function blackholeSandwich.sprite(room, entity)
    local bounds = selectionBounds.resolve(entity, defaultBounds)
    local sprites = {}
    
    -- Draw the space between (safe zone)
    table.insert(sprites, {
        texture = "objects/IngesteHelper/blackhole_sandwich_space",
        x = 0,
        y = 0,
        justificationX = 0.0,
        justificationY = 0.0,
        scaleX = bounds.width / 8,
        scaleY = bounds.height / 8,
        color = {0.1, 0.1, 0.1, 0.3}
    })
    
    -- Draw top hazard
    table.insert(sprites, {
        texture = "objects/IngesteHelper/blackhole_sandwich_top",
        x = 0,
        y = 0,
        justificationX = 0.0,
        justificationY = 0.0,
        scaleX = bounds.width / 8,
        scaleY = 2,
        color = entity.mode == "Hot" and {1.0, 0.3, 0.0, 0.9} or {0.0, 0.5, 1.0, 0.9}
    })
    
    -- Draw bottom hazard
    table.insert(sprites, {
        texture = "objects/IngesteHelper/blackhole_sandwich_bottom",
        x = 0,
        y = bounds.height - 16,
        justificationX = 0.0,
        justificationY = 0.0,
        scaleX = bounds.width / 8,
        scaleY = 2,
        color = entity.mode == "Hot" and {1.0, 0.3, 0.0, 0.9} or {0.0, 0.5, 1.0, 0.9}
    })
    
    return sprites
end

function blackholeSandwich.rectangle(room, entity)
    return selectionBounds.rectangle(entity, defaultBounds)
end

return blackholeSandwich
