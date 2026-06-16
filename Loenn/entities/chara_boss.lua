local charaBoss = {}

charaBoss.name = "DZ/CharaBoss"
charaBoss.depth = -8500
charaBoss.texture = "characters/charaBoss/boss00"
charaBoss.justification = {0.5, 1.0}

charaBoss.nodeLimits = {1, 20}

charaBoss.placements = {
    {
        name = "chara_boss",
        data = {
            patternIndex = 0,
            cameraPastY = 120.0,
            dialog = false,
            startHit = false,
            cameraLockY = true,
    }
    }
}

local celesteEnums = require("consts.celeste_enums")

celesteEnums.chara_Boss_patterns = {
    0, 1, 2, 3, 4,
    5, 6, 7, 8, 9,
    10, 11, 12, 13, 14,
    15, 16, 17, 18, 19,
    20, 21
}

charaBoss.fieldOrder = {
    "x", "y",
    "patternIndex",
    "cameraPastY",
    "dialog",
    "startHit",
    "cameraLockY"
}

charaBoss.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
        options = celesteEnums.chara_Boss_patterns,
        editable = true
    },
    cameraPastY = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 500.0
    },
    dialog = {
        fieldType = "boolean"
    },
    startHit = {
        fieldType = "boolean"
    },
    cameraLockY = {
        fieldType = "boolean"
    }
}

return charaBoss