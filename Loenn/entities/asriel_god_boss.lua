-- Asriel God Boss
local asrielGodBoss = {}

asrielGodBoss.name = "DZ/AsrielGodBoss"
asrielGodBoss.depth = 0
asrielGodBoss.texture = "characters/asrielgodboss/boss00"
asrielGodBoss.justification = {0.5, 0.5}

asrielGodBoss.nodeLimits = {0, -1}
asrielGodBoss.nodeLineRenderType = "line"

asrielGodBoss.placements = {
    {
        name = "main",
        data = {
            patternIndex = 0,
            cameraPastY = 120.0,
            dialog = true,
            startHit = false,
            cameraLockY = true,
            attackSequence = ""
        }
    },
    {
        name = "intro_cutscene",
        data = {
            patternIndex = 0,
            cameraPastY = 120.0,
            dialog = true,
            startHit = false,
            cameraLockY = true,
            attackSequence = ""
        }
    },
    {
        name = "hard_mode",
        data = {
            patternIndex = 30,
            cameraPastY = 120.0,
            dialog = true,
            startHit = true,
            cameraLockY = true,
            attackSequence = ""
        }
    },
    {
        name = "hypergoner_finale",
        data = {
            patternIndex = 61,
            cameraPastY = 120.0,
            dialog = true,
            startHit = true,
            cameraLockY = true,
            attackSequence = "HyperGoner"
        }
    }
}

asrielGodBoss.fieldOrder = {
    "x", "y",
    "patternIndex", "cameraPastY", "cameraLockY",
    "dialog", "startHit",
    "attackSequence"
}

asrielGodBoss.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
        options = celesteEnums.asriel_god_boss_patterns,
        editable = true,
        description = "Attack pattern index (0-60). Different patterns have different attack combinations and dialog triggers."
    },
    cameraPastY = {
        fieldType = "number",
        description = "Camera Y offset for boss arena bounds"
    },
    cameraLockY = {
        fieldType = "boolean",
        description = "Whether to lock camera Y position during the fight"
    },
    dialog = {
        fieldType = "boolean",
        description = "Enable Chapter 20 story dialog sequences"
    },
    startHit = {
        fieldType = "boolean",
        description = "Start attacking immediately (skips waiting for player movement)"
    },
    attackSequence = {
        fieldType = "string",
        options = celesteEnums.asriel_god_boss_attacks,
        description = "Custom attack sequence (optional). Overrides patternIndex when set."
    }
}

local celesteEnums = require("consts.celeste_enums")

celesteEnums.asriel_god_boss_patterns = {
    0, 1, 2, 3, 4,
    5, 6, 7, 8, 9,
    10, 11, 12, 13, 14,
    15, 16, 17, 18, 19,
    20, 21, 22, 23, 24,
    25, 26, 27, 28, 29,
    30, 31, 32, 33, 34,
    35, 36, 37, 38, 39,
    40, 41, 42, 43, 44,
    45, 46, 47, 48, 49,
    50, 51, 52, 53, 54,
    55, 56, 57, 58, 59,
    60
}

celesteEnums.asriel_god_boss_attacks = {
    "Shoot",
    "Beam",
    "BiggerBeam",
    "BigBeamBall",
    "RainbowBlackhole",
    "BladeThrower",
    "FireShockwave",
    "StarsMeteorite",
    "ChaosBlaster",
    "HyperGoner",
    "GalacticSaber",
    "StarstormRain",
    "LightningStorm",
    "DimensionalRift",
    "RainbowInferno",
    "CelestialSpears",
    "TimewarpVortex",
    "PrismBurst",
    "SoulResonance",
    "EternalChaos",
    "SwordSlash"
}

asrielGodBoss.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
        options = celesteEnums.asriel_god_boss_patterns,
        editable = false
    },
    attackSequence = {
        fieldType = "string",
        options = celesteEnums.asriel_god_boss_attacks
    }
}

return asrielGodBoss
