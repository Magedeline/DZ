local vanishingWall = {}

vanishingWall.name = "DZ/VanishingWall"
vanishingWall.depth = 0
vanishingWall.texture = "objects/vanishing_wall"
vanishingWall.justification = {0.0, 0.0}
vanishingWall.minimumSize = {8, 8}

vanishingWall.fieldInformation = {
    tiletype = {
        options = {"3", "4", "7", "9", "g", "m"},
        editable = false
    },
    persistent = {
        fieldType = "boolean"
    },
    activationTime = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    duration = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 20.0
    },
    respawnTime = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 30.0
    },
    activateOnTouch = {
        fieldType = "boolean"
    },
    activateOnDash = {
        fieldType = "boolean"
    },
    flagName = {
        fieldType = "string"
    }
}

vanishingWall.placements = {
    {
        name = "VanishingWall",
        data = {
            width = 16,
            height = 16,
            tiletype = "3",
            persistent = false,
            activationTime = 0.5,
            duration = 3.0,
            respawnTime = 5.0,
            activateOnTouch = true,
            activateOnDash = false,
            flagName = ""
        }
    },
    {
        name = "dash_activated",
        data = {
            width = 16,
            height = 16,
            tiletype = "4",
            persistent = false,
            activationTime = 0.2,
            duration = 2.0,
            respawnTime = 3.0,
            activateOnTouch = false,
            activateOnDash = true,
            flagName = ""
        }
    }
}

function vanishingWall.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    
    return {
        texture = "objects/IngesteHelper/vanishing_wall",
        x = x,
        y = y,
        width = width,
        height = height
    }
end

return vanishingWall