-- Asriel God Boss V2
-- A ConqueredPeak/BadelineBoss style boss with pattern-based attacks
-- Clean rewrite with proper phase management, node-based movement, and hit detection

local asrielGodBossV2 = {}

asrielGodBossV2.name = "MaggyHelper/AsrielGodBossV2"
asrielGodBossV2.depth = 0
asrielGodBossV2.texture = "characters/Maggy/DesoloZantas/asrielgodboss/boss00"
asrielGodBossV2.justification = {0.5, 0.5}
asrielGodBossV2.nodeVisibility = "always"

-- Supports unlimited nodes for movement patterns (BadelineBoss style)
asrielGodBossV2.nodeLimits = {0, -1}
asrielGodBossV2.nodeLineRenderType = "line"

asrielGodBossV2.placements = {
    {
        name = "default",
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
        name = "phase_1_basic",
        data = {
            patternIndex = 0,
            cameraPastY = 120.0,
            dialog = false,
            startHit = false,
            cameraLockY = true,
            attackSequence = ""
        }
    },
    {
        name = "phase_2_spread",
        data = {
            patternIndex = 1,
            cameraPastY = 120.0,
            dialog = false,
            startHit = false,
            cameraLockY = true,
            attackSequence = ""
        }
    },
    {
        name = "phase_3_blades",
        data = {
            patternIndex = 2,
            cameraPastY = 120.0,
            dialog = false,
            startHit = false,
            cameraLockY = true,
            attackSequence = ""
        }
    },
    {
        name = "phase_4_blackhole",
        data = {
            patternIndex = 3,
            cameraPastY = 120.0,
            dialog = false,
            startHit = false,
            cameraLockY = true,
            attackSequence = ""
        }
    },
    {
        name = "phase_5_hypergoner",
        data = {
            patternIndex = 4,
            cameraPastY = 120.0,
            dialog = false,
            startHit = true,
            cameraLockY = true,
            attackSequence = ""
        }
    }
}

-- Attack pattern options for V2 boss
local attackPatterns = {
    "Shoot",
    "ShootSpread",
    "ShootTargeted",
    "Beam",
    "BiggerBeam",
    "SweepBeam",
    "StarRain",
    "BladeThrower",
    "FireShockwave",
    "RainbowBlackhole",
    "HyperGoner",
    "ChaosStorm"
}

-- Pattern index corresponds to phases (0-4)
local patternOptions = {0, 1, 2, 3, 4}

asrielGodBossV2.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
        options = patternOptions,
        editable = true,
        minimumValue = 0,
        maximumValue = 4
    },
    cameraPastY = {
        fieldType = "number",
        minimumValue = 0.0
    },
    attackSequence = {
        fieldType = "string",
        options = attackPatterns
    }
}

asrielGodBossV2.fieldOrder = {
    "x", "y",
    "patternIndex",
    "cameraPastY",
    "cameraLockY",
    "dialog",
    "startHit",
    "attackSequence"
}

-- Tooltip descriptions for each field
function asrielGodBossV2.ignoredFields(entity)
    return {}
end

return asrielGodBossV2

