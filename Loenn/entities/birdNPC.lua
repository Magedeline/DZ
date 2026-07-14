local dzbirdNpc = {}

dzbirdNpc.name = "DZ/BirdNPC"
dzbirdNpc.depth = -1000000
dzbirdNpc.nodeLineRenderType = "line"
dzbirdNpc.justification = {0.5, 1.0}
dzbirdNpc.texture = "characters/DZ/bird/crow00"
dzbirdNpc.nodeLimits = {0, -1}
dzbirdNpc.fieldInformation = {
    mode = {
        options = {
            "None",
            "Sleeping",
            "HoverNGrab",
            "Grab",
            "ClimbingTutorial",
            "DashingTutorial",
            "DreamJumpTutorial",
            "SuperWallJumpTutorial",
            "HyperJumpTutorial",
            "FlyAway",
            "MoveToNodes",
            "WaitForLightningOff"
        },
        editable = false
    }
}
dzbirdNpc.placements = {
    name = "dz_bird_npc",
    data = {
        mode = "Sleeping",
        onlyOnce = false,
        onlyIfPlayerLeft = false
    }
}

local modeFacingScale = {
    climbingtutorial = -1,
    dashingtutorial = 1,
    dreamjumptutorial = 1,
    superwalljumptutorial = -1,
    hyperjumptutorial = -1,
    movetonodes = -1,
    waitforlightningoff = -1,
    flyaway = -1,
    sleeping = 1,
    none = -1
}

function dzbirdNpc.scale(room, entity)
    local mode = string.lower(entity.mode or "sleeping")

    return modeFacingScale[mode] or -1, 1
end

return dzbirdNpc