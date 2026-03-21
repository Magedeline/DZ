local kirbyDummy = {}

kirbyDummy.name = "MaggyHelper/KirbyDummy"
kirbyDummy.depth = 0
kirbyDummy.justification = {0.5, 1.0}
kirbyDummy.texture = "characters/kirby/idle00"

kirbyDummy.fieldInformation = {
    facing = {
        fieldType = "integer",
        options = {-1, 1},
        editable = false
    },
    animation = {
        fieldType = "string",
        options = {
            "idle",
            "walk",
            "fall",
            "happy",
            "normal"
        },
        editable = true
    },
    scale = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 3.0
    },
    alpha = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    isVisible = {
        fieldType = "boolean"
    }
}

kirbyDummy.placements = {
    {
        name = "normal",
        data = {
            facing = 1,
            animation = "idle",
            scale = 1.0,
            alpha = 1.0,
            isVisible = true
        }
    }
}

return kirbyDummy
