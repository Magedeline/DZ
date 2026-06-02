local strawberryRemix = {}

strawberryRemix.name = "MaggyHelper/StrawberryRemix"
strawberryRemix.depth = -100

strawberryRemix.fieldInformation = {
    winged = {
        fieldType = "boolean"
    },
    golden = {
        fieldType = "boolean"
    },
    pink = {
        fieldType = "boolean"
    },
    moon = {
        fieldType = "boolean"
    },
    popstar = {
        fieldType = "boolean"
    },
    bobAmplitude = {
        fieldType = "number"
    },
    bobSpeed = {
        fieldType = "number"
    },
    collectDelay = {
        fieldType = "number"
    },
    glowInterval = {
        fieldType = "number"
    }
}

strawberryRemix.fieldOrder = {
    "x", "y",
    "winged", "golden", "pink", "moon", "popstar",
    "bobAmplitude", "bobSpeed", "collectDelay", "glowInterval"
}

local function getTexture(entity)
    if entity.popstar then
        return "collectables/popstarberry/spin/000"
    end

    if entity.moon then
        return "collectables/moonBerry/normal00"
    end

    if entity.pink then
        return "collectables/maggy/pinkplatberry/idle00"
    end

    if entity.golden then
        return "collectables/goldberry/idle00"
    end

    if entity.winged then
        return "collectables/strawberry/wings01"
    end

    return "collectables/strawberry/normal00"
end

function strawberryRemix.texture(room, entity)
    return getTexture(entity)
end

strawberryRemix.placements = {
    {
        name = "normal",
        data = {
            winged = false,
            golden = false,
            pink = false,
            moon = false,
            popstar = false,
            bobAmplitude = 2.0,
            bobSpeed = 4.0,
            collectDelay = 0.15,
            glowInterval = 0.08
        }
    },
    {
        name = "winged",
        data = {
            winged = true,
            golden = false,
            pink = false,
            moon = false,
            popstar = false,
            bobAmplitude = 2.0,
            bobSpeed = 4.0,
            collectDelay = 0.15,
            glowInterval = 0.08
        }
    },
    {
        name = "golden",
        data = {
            winged = false,
            golden = true,
            pink = false,
            moon = false,
            popstar = false,
            bobAmplitude = 2.0,
            bobSpeed = 4.0,
            collectDelay = 0.15,
            glowInterval = 0.08
        }
    },
    {
        name = "pink",
        data = {
            winged = false,
            golden = false,
            pink = true,
            moon = false,
            popstar = false,
            bobAmplitude = 2.0,
            bobSpeed = 4.0,
            collectDelay = 0.15,
            glowInterval = 0.08
        }
    },
    {
        name = "moon",
        data = {
            winged = false,
            golden = false,
            pink = false,
            moon = true,
            popstar = false,
            bobAmplitude = 2.0,
            bobSpeed = 4.0,
            collectDelay = 0.15,
            glowInterval = 0.08
        }
    },
    {
        name = "popstar",
        data = {
            winged = false,
            golden = false,
            pink = false,
            moon = false,
            popstar = true,
            bobAmplitude = 2.0,
            bobSpeed = 4.0,
            collectDelay = 0.15,
            glowInterval = 0.08
        }
    }
}

return strawberryRemix
