local hole = {}

hole.name = "MaggyHelper/Hole"
hole.depth = 8999
hole.texture = "objects/IngesteHelper/hole"
hole.justification = {0.5, 0.5}

hole.fieldInformation = {
    holeType = {
        options = {"black", "white", "void", "portal"},
        editable = false
    },
    radius = {
        fieldType = "number",
        minimumValue = 8.0,
        maximumValue = 200.0
    },
    strength = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    affectsPlayer = {
        fieldType = "boolean"
    },
    affectsEntities = {
        fieldType = "boolean"
    },
    spawnParticles = {
        fieldType = "boolean"
    },
    teleportDestination = {
        fieldType = "string"
    },
    killPlayer = {
        fieldType = "boolean"
    },
    soundEffect = {
        fieldType = "string"
    }
}

hole.placements = {
    {
        name = "black_hole",
        data = {
            holeType = "black",
            radius = 64.0,
            strength = 5.0,
            affectsPlayer = true,
            affectsEntities = true,
            spawnParticles = true,
            teleportDestination = "",
            killPlayer = false,
            soundEffect = "event:/game/general/thing_booped"
        }
    },
    {
        name = "white_hole",
        data = {
            holeType = "white",
            radius = 48.0,
            strength = 3.0,
            affectsPlayer = true,
            affectsEntities = true,
            spawnParticles = true,
            teleportDestination = "",
            killPlayer = false,
            soundEffect = ""
        }
    },
    {
        name = "void_hole",
        data = {
            holeType = "void",
            radius = 80.0,
            strength = 8.0,
            affectsPlayer = true,
            affectsEntities = true,
            spawnParticles = false,
            teleportDestination = "",
            killPlayer = true,
            soundEffect = "event:/char/badeline/disappear"
        }
    },
    {
        name = "portal_hole",
        data = {
            holeType = "portal",
            radius = 32.0,
            strength = 1.0,
            affectsPlayer = true,
            affectsEntities = false,
            spawnParticles = true,
            teleportDestination = "spawn",
            killPlayer = false,
            soundEffect = "event:/game/general/seed_touch"
        }
    }
}

function hole.sprite(room, entity)
    local holeType = entity.holeType or "black"
    local texture = "objects/IngesteHelper/hole_" .. holeType
    
    return {
        texture = texture,
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return hole