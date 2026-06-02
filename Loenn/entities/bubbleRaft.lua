local bubbleRaft = {}

bubbleRaft.name = "MaggyHelper/BubbleRaft"
bubbleRaft.depth = -500
bubbleRaft.placements = {
    {
        name = "normal",
        data = { duration = 5.0, floatSpeed = 30.0 }
    },
    {
        name = "fast",
        data = { duration = 3.0, floatSpeed = 60.0 }
    },
    {
        name = "long_lasting",
        data = { duration = 10.0, floatSpeed = 20.0 }
    }
}
bubbleRaft.fieldInformation = {
    duration = { fieldType = "number", minimumValue = 1.0 },
    floatSpeed = { fieldType = "number", minimumValue = 5.0 }
}
bubbleRaft.fieldOrder = { "x", "y", "duration", "floatSpeed" }

function bubbleRaft.sprite(room, entity)
    return "objects/glider/idle0", {0.5, 0.5}
end

return bubbleRaft
