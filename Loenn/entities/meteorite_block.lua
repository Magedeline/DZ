local meteoriteBlock = {}

meteoriteBlock.name = "DZ/metoriteblock"
meteoriteBlock.depth = 0
meteoriteBlock.texture = "objects/IngesteHelper/meteorite_block"
meteoriteBlock.justification = {0.0, 0.0}
meteoriteBlock.minimumSize = {8, 8}

meteoriteBlock.fieldInformation = {
    tiletype = {
        options = {"3", "4", "7", "9", "g", "m"},
        editable = false
    },
    fallSpeed = {
        fieldType = "number",
        minimumValue = 50.0,
        maximumValue = 500.0
    },
    impactDamage = {
        fieldType = "boolean"
    },
    explosive = {
        fieldType = "boolean"
    },
    explosionRadius = {
        fieldType = "number",
        minimumValue = 16.0,
        maximumValue = 128.0
    },
    fireTrail = {
        fieldType = "boolean"
    },
    shakeOnImpact = {
        fieldType = "boolean"
    },
    destroyOnImpact = {
        fieldType = "boolean"
    },
    respawnTime = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 60.0
    }
}

meteoriteBlock.placements = {
    {
        name = "normal",
        data = {
            width = 16,
            height = 16,
            tiletype = "7",
            fallSpeed = 200.0,
            impactDamage = false,
            explosive = false,
            explosionRadius = 32.0,
            fireTrail = true,
            shakeOnImpact = true,
            destroyOnImpact = true,
            respawnTime = 10.0
        }
    },
    {
        name = "explosive",
        data = {
            width = 24,
            height = 24,
            tiletype = "9",
            fallSpeed = 150.0,
            impactDamage = true,
            explosive = true,
            explosionRadius = 64.0,
            fireTrail = true,
            shakeOnImpact = true,
            destroyOnImpact = true,
            respawnTime = 20.0
        }
    },
    {
        name = "fast",
        data = {
            width = 12,
            height = 12,
            tiletype = "4",
            fallSpeed = 400.0,
            impactDamage = true,
            explosive = false,
            explosionRadius = 16.0,
            fireTrail = true,
            shakeOnImpact = false,
            destroyOnImpact = true,
            respawnTime = 5.0
        }
    }
}

function meteoriteBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    
    return {
        texture = "objects/IngesteHelper/meteorite_block",
        x = x,
        y = y,
        width = width,
        height = height
    }
end

return meteoriteBlock