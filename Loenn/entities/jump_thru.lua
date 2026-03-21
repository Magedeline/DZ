local jumpThru = {}

jumpThru.name = "MaggyHelper/JumpThru"
jumpThru.depth = -60
jumpThru.texture = "objects/IngesteHelper/jump_thru"
jumpThru.justification = {0.0, 0.0}
jumpThru.minimumSize = {8, 8}

jumpThru.fieldInformation = {
    texture = {
        fieldType = "string",
        options = {
            "wood",
            "stone",
            "metal",
            "crystal",
            "ice",
            "cloud",
            "magic",
            "bone"
        },
        editable = true
    },
    surfaceIndex = {
        fieldType = "integer",
        minimumValue = -1,
        maximumValue = 32
    },
    overrideTexture = {
        fieldType = "string"
    },
    pushPlayer = {
        fieldType = "boolean"
    },
    moveSpeed = {
        fieldType = "number",
        minimumValue = -200.0,
        maximumValue = 200.0
    },
    attachToSolids = {
        fieldType = "boolean"
    },
    sinkAmount = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 8.0
    }
}

jumpThru.placements = {
    {
        name = "wood",
        data = {
            width = 24,
            height = 8,
            texture = "wood",
            surfaceIndex = -1,
            overrideTexture = "",
            pushPlayer = false,
            moveSpeed = 0.0,
            attachToSolids = false,
            sinkAmount = 0.0
        }
    },
    {
        name = "stone",
        data = {
            width = 24,
            height = 8,
            texture = "stone",
            surfaceIndex = -1,
            overrideTexture = "",
            pushPlayer = false,
            moveSpeed = 0.0,
            attachToSolids = false,
            sinkAmount = 0.0
        }
    },
    {
        name = "moving",
        data = {
            width = 32,
            height = 8,
            texture = "metal",
            surfaceIndex = -1,
            overrideTexture = "",
            pushPlayer = true,
            moveSpeed = 50.0,
            attachToSolids = true,
            sinkAmount = 2.0
        }
    },
    {
        name = "cloud",
        data = {
            width = 40,
            height = 12,
            texture = "cloud",
            surfaceIndex = -1,
            overrideTexture = "",
            pushPlayer = false,
            moveSpeed = 0.0,
            attachToSolids = false,
            sinkAmount = 4.0
        }
    }
}

function jumpThru.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 8
    local texture = entity.texture or "wood"
    
    return {
        texture = "objects/IngesteHelper/jump_thru_" .. texture,
        x = x,
        y = y,
        width = width,
        height = height
    }
end

return jumpThru