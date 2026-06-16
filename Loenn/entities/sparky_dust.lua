local sparkeyDust = {}

sparkeyDust.name = "DZ/SparkyDust"
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
            "guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}",
            "guid://{16b40879-0a79-4e42-8c91-fe419a8e186c}",
            "guid://{6160ad7b-16f6-49a5-aa9b-55d75da5a8e1}"
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
            soundEffect = "guid://{5a1e6a52-fa6a-44fb-b7ef-931a000b7c95}"
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
            soundEffect = "guid://{6160ad7b-16f6-49a5-aa9b-55d75da5a8e1}"
        }
    }
}

return sparkeyDust
