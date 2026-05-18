local dashCopyBerry = {}

dashCopyBerry.name = "MaggyHelper/DashCopyBerry"
dashCopyBerry.depth = -100
dashCopyBerry.texture = "collectables/strawberry/normal00"

dashCopyBerry.fieldInformation = {
    power = {
        fieldType = "string",
        options = {
            "None", "Fire", "Ice", "Spark", "Sword", "Cutter", "Beam",
            "Stone", "Needle", "Parasol", "Wheel", "Bomb", "Fighter",
            "Suplex", "Ninja", "Mirror", "Hammer", "Knight", "Wing",
            "UFO", "Sleep"
        },
        editable = false
    },
    dashRefill = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 5
    }
}

dashCopyBerry.placements = {
    {
        name = "dash_copy_berry",
        data = {
            power = "Sword",
            dashRefill = 1,
            refillOnlyWhenEmpty = false
        }
    },
    {
        name = "dash_copy_berry_fire",
        data = {
            power = "Fire",
            dashRefill = 1,
            refillOnlyWhenEmpty = false
        }
    },
    {
        name = "dash_copy_berry_wing",
        data = {
            power = "Wing",
            dashRefill = 2,
            refillOnlyWhenEmpty = false
        }
    }
}

return dashCopyBerry
