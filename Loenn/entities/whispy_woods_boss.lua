local whispyWoodsBoss = {}

whispyWoodsBoss.name = "MaggyHelper/WhispyWoodsBoss"
whispyWoodsBoss.depth = -8500
whispyWoodsBoss.texture = "objects/DesoloZantas/whispy_woods/idle00"
whispyWoodsBoss.justification = {0.5, 1.0}

whispyWoodsBoss.nodeLimits = {1, 20}

whispyWoodsBoss.placements = {
    {
        name = "whispy_woods_boss",
        data = {
            patternIndex = 1,
            cameraPastY = 120.0,
            dialog = false,
            startHit = false,
            cameraLockY = true,
            attackSequence = ""
        }
    }
}

whispyWoodsBoss.fieldOrder = {
    "x", "y",
    "patternIndex",
    "attackSequence",
    "cameraPastY",
    "dialog",
    "startHit",
    "cameraLockY"
}

whispyWoodsBoss.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
        options = {0, 1, 2},
        editable = true
    },
    cameraPastY = {
        fieldType = "number",
        minimumValue = 0.0
    },
    dialog = {
        fieldType = "boolean"
    },
    startHit = {
        fieldType = "boolean"
    },
    cameraLockY = {
        fieldType = "boolean"
    },
    attackSequence = {
        fieldType = "string"
    }
}

return whispyWoodsBoss
