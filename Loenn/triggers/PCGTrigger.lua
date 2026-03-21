local trigger = {}

trigger.name = "MaggyHelper/PCGTrigger"

trigger.placements = {
    {
        name = "default",
        data = {
            preset = "default",
            roomCount = 8,
            seed = -1,
            trainFromMap = true,
            targetRoom = "",
            width = 16,
            height = 16,
        },
    },
}

trigger.fieldInformation = {
    preset = {
        options = {
            "default", "open", "tight", "space", "deepSpace",
            "resort", "temple", "summit", "core", "wind", "farewell",
        },
        editable = true,
    },
    roomCount = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 50,
    },
    seed = {
        fieldType = "integer",
        minimumValue = -1,
    },
}

trigger.fieldOrder = {
    "x", "y", "width", "height",
    "preset", "roomCount", "seed", "trainFromMap", "targetRoom",
}

return trigger
