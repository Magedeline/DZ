local launchCannon = {}

launchCannon.name = "MaggyHelper/LaunchCannon"
launchCannon.depth = -500
launchCannon.placements = {
    {
        name = "manual_aim",
        data = { launchSpeed = 400.0, autoFire = false, autoAngle = -90.0 }
    },
    {
        name = "auto_up",
        data = { launchSpeed = 400.0, autoFire = true, autoAngle = -90.0 }
    },
    {
        name = "auto_right",
        data = { launchSpeed = 400.0, autoFire = true, autoAngle = 0.0 }
    },
    {
        name = "auto_left",
        data = { launchSpeed = 400.0, autoFire = true, autoAngle = 180.0 }
    },
    {
        name = "auto_diagonal_up_right",
        data = { launchSpeed = 400.0, autoFire = true, autoAngle = -45.0 }
    }
}
launchCannon.fieldInformation = {
    launchSpeed = { fieldType = "number", minimumValue = 50.0 },
    autoFire = { fieldType = "boolean" },
    autoAngle = { fieldType = "number", minimumValue = -180.0, maximumValue = 180.0 }
}
launchCannon.fieldOrder = { "x", "y", "launchSpeed", "autoFire", "autoAngle" }

local function cannonSprite(room, entity)
    return "objects/spring/00", {0.5, 1.0}
end

function launchCannon.sprite(room, entity)
    return cannonSprite(room, entity)
end

return launchCannon
