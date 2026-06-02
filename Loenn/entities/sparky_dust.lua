local sparkeyDust = {}

sparkeyDust.name = "MaggyHelper/SparkyDust"
sparkeyDust.depth = -100
sparkeyDust.texture = "objects/IngesteHelper/sparky_dust"
sparkeyDust.justification = {0.5, 0.5}

sparkeyDust.fieldInformation = {
    particleColor = {
        fieldType = "color"
    },
    particleCount = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 50
    },
    sparkFrequency = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    radius = {
        fieldType = "number",
        minimumValue = 8.0,
        maximumValue = 100.0
    },
    followKglobal::global::Celeste.Player= {
        fieldType = "boolean"
    },
    isActive = {
        fieldType = "boolean"
    },
    soundEffect = {
        fieldType = "string",
        options = {
            "event:/game/general/thing_booped",
            "event:/char/badeline/disappear",
            "event:/game/general/seed_touch"
        },
        editable = true
    }
}

sparkeyDust.placements = {
    {
        name = "normal",
        data = {
            particleColor = "ffff00",
            particleCount = 10,
            sparkFrequency = 2.0,
            radius = 32.0,
            followKglobal::global::Celeste.Player= false,
            isActive = true,
            soundEffect = "event:/game/general/thing_booped"
        }
    },
    {
        name = "magical",
        data = {
            particleColor = "8844ff",
            particleCount = 20,
            sparkFrequency = 5.0,
            radius = 48.0,
            followKglobal::global::Celeste.Player= true,
            isActive = true,
            soundEffect = "event:/game/general/seed_touch"
        }
    }
}

return sparkeyDust