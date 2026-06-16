local instantFallingBlock = {}

instantFallingBlock.name = "DZ/InstantFallingBlock"
instantFallingBlock.depth = 0
instantFallingBlock.texture = "objects/IngesteHelper/instant_falling_block"
instantFallingBlock.justification = {0.0, 0.0}
instantFallingBlock.minimumSize = {8, 8}

instantFallingBlock.fieldInformation = {
    tiletype = {
        options = {"3", "4", "7", "9", "g", "m"},
        editable = false
    },
    climbFall = {
        fieldType = "boolean"
    },
    behind = {
        fieldType = "boolean"
    },
    finalBoss = {
        fieldType = "boolean"
    },
    fallOnTouch = {
        fieldType = "boolean"
    },
    fallOnShake = {
        fieldType = "boolean"
    },
    fallDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0
    },
    respawnTime = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 30.0
    },
    shakeIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    }
}

instantFallingBlock.placements = {
    {
        name = "normal",
        data = {
            width = 16,
            height = 16,
            tiletype = "3",
            climbFall = true,
            behind = false,
            finalBoss = false,
            fallOnTouch = true,
            fallOnShake = false,
            fallDelay = 0.0,
            respawnTime = 5.0,
            shakeIntensity = 0.0
        }
    },
    {
        name = "shake_activated",
        data = {
            width = 16,
            height = 16,
            tiletype = "4",
            climbFall = false,
            behind = false,
            finalBoss = false,
            fallOnTouch = false,
            fallOnShake = true,
            fallDelay = 0.5,
            respawnTime = 8.0,
            shakeIntensity = 2.0
        }
    },
    {
        name = "boss_block",
        data = {
            width = 32,
            height = 32,
            tiletype = "7",
            climbFall = false,
            behind = false,
            finalBoss = true,
            fallOnTouch = true,
            fallOnShake = true,
            fallDelay = 0.2,
            respawnTime = 0.0,
            shakeIntensity = 5.0
        }
    }
}

function instantFallingBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    
    return {
        texture = "objects/IngesteHelper/instant_falling_block",
        x = x,
        y = y,
        width = width,
        height = height
    }
end

return instantFallingBlock