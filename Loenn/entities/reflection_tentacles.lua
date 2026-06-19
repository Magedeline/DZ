local reflectionTentacles = {}

reflectionTentacles.name = "DZ/ReflectionTentacles"
reflectionTentacles.depth = 0
reflectionTentacles.texture = "objects/IngesteHelper/reflection_tentacles"
reflectionTentacles.justification = {0.5, 1.0}
reflectionTentacles.nodeLineRenderType = "line"
reflectionTentacles.nodeLimits = {1, -1}

reflectionTentacles.fieldInformation = {
    tentacleCount = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    speed = {
        fieldType = "number",
        minimumValue = 10.0,
        maximumValue = 200.0
    },
    color = {
        fieldType = "color"
    },
    followKglobal::global::Celeste.Player= {
        fieldType = "boolean"
    },
    aggressive = {
        fieldType = "boolean"
    },
    retreatDistance = {
        fieldType = "number",
        minimumValue = 32.0,
        maximumValue = 200.0
    },
    attackDistance = {
        fieldType = "number",
        minimumValue = 16.0,
        maximumValue = 128.0
    },
    flagName = {
        fieldType = "string"
    }
}

reflectionTentacles.placements = {
    {
        name = "main",
        data = {
            tentacleCount = 3,
            speed = 80.0,
            color = "8844ff",
            followKglobal::global::Celeste.Player= true,
            aggressive = false,
            retreatDistance = 64.0,
            attackDistance = 32.0,
            flagName = ""
        }
    },
    {
        name = "aggressive",
        data = {
            tentacleCount = 5,
            speed = 120.0,
            color = "ff4444",
            followKglobal::global::Celeste.Player= true,
            aggressive = true,
            retreatDistance = 32.0,
            attackDistance = 64.0,
            flagName = ""
        }
    },
    {
        name = "defensive",
        data = {
            tentacleCount = 2,
            speed = 40.0,
            color = "4488ff",
            followKglobal::global::Celeste.Player= false,
            aggressive = false,
            retreatDistance = 128.0,
            attackDistance = 16.0,
            flagName = ""
        }
    }
}

return reflectionTentacles