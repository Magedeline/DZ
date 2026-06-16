local transportPlatform = {}

transportPlatform.name = "DZ/TransportPlatform"
transportPlatform.depth = 0

transportPlatform.placements = {
    {
        name = "TransportPlatform",
        data = {
            width = 32,
            height = 8,
            isActive = true,
            moveSpeed = 60.0,
            waitTime = 2.0,
            platformType = "stone"
        }
    },
    {
        name = "wooden",
        data = {
            width = 48,
            height = 8,
            isActive = true,
            moveSpeed = 40.0,
            waitTime = 3.0,
            platformType = "wood"
        }
    },
    {
        name = "magical",
        data = {
            width = 40,
            height = 12,
            isActive = true,
            moveSpeed = 100.0,
            waitTime = 1.0,
            platformType = "magical"
        }
    }
}

transportPlatform.fieldInformation = {
    width = {
        fieldType = "integer",
        minimumValue = 16,
        maximumValue = 128
    },
    height = {
        fieldType = "integer",
        minimumValue = 4,
        maximumValue = 32
    },
    isActive = {
        fieldType = "boolean"
    },
    moveSpeed = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 200.0
    },
    waitTime = {
        fieldType = "number",
        minimumValue = 0.5,
        maximumValue = 10.0
    },
    platformType = {
        options = {"stone", "wood", "magical", "ancient"},
        editable = false
    }
}

function transportPlatform.sprite(room, entity)
    local width = entity.width or 32
    local height = entity.height or 8
    local color = {0.8, 0.8, 0.9, 1.0}
    
    if entity.platformType == "wood" then
        color = {0.8, 0.6, 0.4, 1.0}
    elseif entity.platformType == "magical" then
        color = {0.7, 0.4, 1.0, 1.0}
    elseif entity.platformType == "ancient" then
        color = {0.6, 0.8, 0.3, 1.0}
    end
    
    return {
        {
            texture = "objects/moveBlock/base",
            x = entity.x,
            y = entity.y,
            scaleX = width / 16,
            scaleY = height / 8,
            color = color
        }
    }
end

function transportPlatform.selection(room, entity)
    local width = entity.width or 32
    local height = entity.height or 8
    return {entity.x, entity.y, width, height}
end

return transportPlatform
