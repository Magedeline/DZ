local charaDummy = {}

charaDummy.name = "DZ/CharaDummy"
charaDummy.depth = 0
charaDummy.justification = {0.5, 1.0}
charaDummy.texture = "characters/chara/idle00"

charaDummy.fieldInformation = {
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
            "fallSlow",
            "laugh",
            "angry",
            "spawn",
            "pretendDead"
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

charaDummy.placements = {
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

return charaDummy
