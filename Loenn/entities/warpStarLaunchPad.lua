local warpStarLaunchPad = {}

warpStarLaunchPad.name = "DZ/WarpStarLaunchPad"
warpStarLaunchPad.depth = -100
warpStarLaunchPad.texture = "objects/DZ/warpstars/idle00"

warpStarLaunchPad.placements = {
    name = "warp_star_launch_pad",
    data = {
        finalBattleRoom = "ch21-final-battle",
        returnRoom = "",
        returnSpawnX = 0,
        returnSpawnY = 0,
        kirbyReadyFlag = "ch21_kirby_ready",
        madelineReadyFlag = "ch21_madeline_ready",
        autoReady = true,
        requireBothReady = true,
        onlyOnce = true,
        launchSfx = "event:/pusheen/new_content/game/21_desolo_zantas/warpstar_launch",
        rideSfx = "event:/pusheen/new_content/music/lvl21/warpstar_ride",
        bobAmplitude = 3.0,
        bobSpeed = 2.5,
        preLaunchDelay = 1.2,
        interactRadius = 24.0
    }
}

warpStarLaunchPad.fieldInformation = {
    returnSpawnX = {
        fieldType = "integer"
    },
    returnSpawnY = {
        fieldType = "integer"
    },
    bobAmplitude = {
        minimumValue = 0.0
    },
    bobSpeed = {
        minimumValue = 0.0
    },
    preLaunchDelay = {
        minimumValue = 0.0
    },
    interactRadius = {
        minimumValue = 0.0
    }
}

return warpStarLaunchPad
